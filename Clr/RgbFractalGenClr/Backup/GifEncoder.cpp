//
// Created by xiaozhuai on 2020/12/20.
//

#include <string>
#include <vector>
#include <exception>
#include <stdexcept>
#include <cstdlib>
#include <cstring>
#include <thread>
#include "GifEncoder.h"

#define m_gifFile ((GifFileType *) m_gifFileHandler)
#define GifAddExtensionBlockFor(a, func, len, data) GifAddExtensionBlock(       \
        &((a)->ExtensionBlockCount),                                            \
        &((a)->ExtensionBlocks),                                                \
        func, len, data)



#pragma region ORIGINAL
bool GifEncoder::open(const std::string& file, int width, int height, 
                      int quality, bool useGlobalColorMap, int16_t loop, int preAllocSize) {
    if (initOpen(file, width, height))
        return false;
    m_useGlobalColorMap = useGlobalColorMap;
    cancelToken = nullptr;
    m_parallel = false;
    auto& encode = *data_encode;
    if (preAllocSize > 0)
        encode.thepicture = (uint8_t*)malloc(encode.m_allocSize = preAllocSize);
    return endOpen(encode, loop);
}
bool GifEncoder::push(NeuQuantTask::PixelFormat format, const uint8_t* frame, int width, int height, int delay) {
    auto& encode = *data_encode;
    if (m_gifFile == nullptr || frame == nullptr || m_parallel)
        return false; // no file, no image, opened parallel
    if (m_useGlobalColorMap) {
        if (encode.m_frameWidth != width || encode.m_frameHeight != height)
            return false; //throw std::runtime_error("Frame size must be same when use global color map!");
        encode.setSize(width, height);
        if (encode.alloc(m_frameCount + 1) || !encode.convertToBGR(format, encode.thepicture + encode.lengthcount * m_frameCount, frame, encode.lengthcount))
            return false; // failed to allocate memory or convert to BGR
        m_allFrameDelays.push_back(delay);
    } else {
        encode.m_delay = delay;
        encode.setSize(width, height);
        if (encode.alloc(1))
            return false; // failed to allocate memory
        auto* pixels = encode.thepicture;
        encode.convertToBGR(format, pixels, frame, encode.lengthcount);
        encode.getColorMap(m_quality, nullptr);
        encode.getRasterBits(encode.thepicture, nullptr);
        encodeFrame(encode, delay);
    }
    ++m_frameCount;
    return true;
}
bool GifEncoder::close() {
    if (m_gifFile == nullptr) 
        return false;
    auto& write = *data_write;
    if (m_parallel) 
        return write.finished = true;
    
    //ColorMapObject* globalColorMap = nullptr;

    if (m_useGlobalColorMap) {

        write.getColorMap(m_quality, nullptr);
        m_gifFile->SColorMap = write.colorMap;

        for (int i = 0; i < m_frameCount; ++i) {
            write.getRasterBits(write.thepicture + write.lengthcount * i, nullptr);
            encodeFrame(write, m_allFrameDelays[i]);
        }
    }

    int extCount = m_gifFile->ExtensionBlockCount;
    auto* extBlocks = m_gifFile->ExtensionBlocks;

    int savedImageCount = m_gifFile->ImageCount;
    auto* savedImages = m_gifFile->SavedImages;

    int error;
    if (EGifSpew(m_gifFile) == GIF_ERROR) {
        EGifCloseFile(m_gifFile, &error);
        m_gifFileHandler = nullptr;
        return false;
    }

    GifFreeExtensions(&extCount, &extBlocks);
    for (auto* sp = savedImages; sp < savedImages + savedImageCount; sp++) {
        if (sp->ImageDesc.ColorMap != nullptr) {
            GifFreeMapObject(sp->ImageDesc.ColorMap);
            sp->ImageDesc.ColorMap = nullptr;
        }

        if (sp->RasterBits != nullptr) {
            free((char*)sp->RasterBits);
            sp->RasterBits = nullptr;
        }

        GifFreeExtensions(&sp->ExtensionBlockCount, &sp->ExtensionBlocks);
    }
    free(savedImages);

    m_gifFileHandler = nullptr;

    reset();

    return finishedAnimation = true;
}
void GifEncoder::encodeFrame(NeuQuantTask& encode, int delay) {
    auto* gifImage = GifMakeSavedImage(m_gifFile, nullptr);

    gifImage->ImageDesc.Left = 0;
    gifImage->ImageDesc.Top = 0;
    gifImage->ImageDesc.Width = encode.m_frameWidth;
    gifImage->ImageDesc.Height = encode.m_frameHeight;
    gifImage->ImageDesc.Interlace = false;
    gifImage->ImageDesc.ColorMap = (ColorMapObject*)encode.colorMap;
    gifImage->RasterBits = (GifByteType*)encode.rasterBits;
    gifImage->ExtensionBlockCount = 0;
    gifImage->ExtensionBlocks = nullptr;

    GraphicsControlBlock gcb;
    gcb.DisposalMode = DISPOSE_DO_NOT;
    gcb.UserInputFlag = false;
    gcb.DelayTime = delay;
    gcb.TransparentColor = NO_TRANSPARENT_COLOR;
    uint8_t gcbBytes[4];
    EGifGCBToExtension(&gcb, gcbBytes);
    GifAddExtensionBlockFor(gifImage, GRAPHICS_EXT_FUNC_CODE, sizeof(gcbBytes), gcbBytes);
}
#pragma endregion

#pragma region PARALLEL
bool GifEncoder::open_parallel(const std::string& file, const int width, const int height,
                      const int quality, int16_t loop, System::Threading::CancellationToken* token) {
    if (initOpen(file, width, height))
        return false;
    m_quality = quality;
    m_useGlobalColorMap = false;
    m_parallel = true;
    auto& encode = *data_encode;
    encode.token = cancelToken = token;
    return endOpen(encode, loop);
}
bool GifEncoder::push_parallel(NeuQuantTask::PixelFormat format, uint8_t* frame, const int width, const int height, const int delay, const int frameIndex, System::Threading::CancellationToken* token) {
    if (m_gifFile == nullptr || frame == nullptr) 
        return false;
    auto encodePtr = nextEncode(frameIndex);
    if (encodePtr == nullptr)
        return false;
    auto& encode = *encodePtr;
    encode.setSize(width, height);
    if (format == NeuQuantTask::PIXEL_FORMAT_BGR) 
        encode.thepicture = frame;
    else if (encode.alloc(1) || !encode.convertToBGR(format, encode.thepicture, frame, encode.lengthcount))
        return false; // failed to allocate memory or convert to BGR
    return pushTask(encode, token == nullptr ? encode.token : token);
}
bool GifEncoder::pushTask(NeuQuantTask& encode, System::Threading::CancellationToken* token) const {
    const auto nPix = encode.m_frameWidth * encode.m_frameHeight;
    encode.getColorMap(m_quality, token); // slowest
    if (token->IsCancellationRequested)
        return !(encode.failed = true);
    encode.getRasterBits(encode.thepicture, token); // slow
    return encode.finished = !(encode.failed = token->IsCancellationRequested);
}
TryWrite GifEncoder::tryWrite(System::Threading::CancellationToken* token) {
    if(!m_parallel)
        return TryWrite::Failed; // This should only be called to attepmt to write frames in parallel mode!
    if (finishedAnimation)
        return TryWrite::FinishedAnimation;
    if (m_gifFile == nullptr || data_write->failed)
        return TryWrite::Failed;
    auto write = data_write;
    NeuQuantTask* prev = nullptr;
    while (write->frameIndex > finishedFrame) {
        if (write->nextTask == nullptr) {
            return write->finished
                ? TryWrite::Failed	// You must have called finish while having gaps in supplied out of order frames!
                : TryWrite::Waiting;	// You have not supplied the next out of order frame yet, so the Task has not even started running yet.
        }
        write = (prev = write)->nextTask;
    }
    if (write == nullptr)
        return TryWrite::Failed;
    auto& w = *write;
    if (!w.finished)
        return TryWrite::Waiting; // the task for the next sequential frame is not finished yet, try again later
    if (w.nextTask == nullptr) {
        // i always assign to next right before i start its task, so if i set the finished flag without starting a task, that means i have called close() and there's no next frame
        // The finish normally closed the filestream, but not I will do it here
        // and the next close() just sets the encodeTaskData->finished = true to trigger this code when all frames wating to be written have been written
        m_parallel = false; // makes sure it actually writed the file end instead of setting the finish flag again
        return close() ? TryWrite::FinishedAnimation : TryWrite::Failed; // Has writing the full file finished or failed?
    }
    // this nextTask is not null, so this task is actually a frame and not an end yet, so write that frame into the file
    if (w.frameIndex < finishedFrame) {
        // You must have messed up supplying the frameIndexes! The should be a queued frame with an index less than what has alraedy been written!
        // This has probablty happened because you either mixed the automatic ordered, and out of order AddFrame.
        // Or you messed out the out of order frame counting and supplied the same index twice.
        return TryWrite::Failed;
    }
    if(w.thisTask != nullptr && w.thisTask->joinable())
        w.thisTask->join(); // join the task if if was created here
    encodeFrame(w, w.m_delay);
    ++finishedFrame;
    cleartask(w);
    if (prev == nullptr) {
        // Now that I wrote the frame, I can forget the task data, and move on to the next one.
        data_write = data_write->nextTask;
    } else {
        // The first queued writeTaskData was not the next sequential frame to be written and we were not writing that one...
        // ...so we have to skip and forget the one later in the list that we actually wrote
        prev->nextTask = w.nextTask;
    }
    return TryWrite::FinishedFrame; // Has writing this next frame finished or failed?
}

NeuQuantTask* GifEncoder::nextEncode(int index) {
    NeuQuantTask* encode = nullptr;
    mtx.lock();
    if (m_gifFile == nullptr || (encode = data_encode)->failed)
        return nullptr;
    data_encode = encode->nextTask = new NeuQuantTask(); // make a next one for the next call of Addframe to work on
    ++m_frameCount;
    mtx.unlock();
    encode->frameIndex = index < 0 ? addedFrames++ : index;
    return encode;
}

bool GifEncoder::initOpen(const std::string& file, int width, int height) {
    if (m_gifFile != nullptr)
        return true; // file already opened
    int error;
    m_gifFileHandler = EGifOpenFileName(file.c_str(), false, &error);
    if (!m_gifFile)
        return true; // file failed to open
    finishedAnimation = false;
    finishedFrame = addedFrames = 0;
    // encode should always be th same as write or further, so start clearing out from write
    reset();
    data_encode = data_write = new NeuQuantTask();
    return false; // file successfuly opened
}


bool GifEncoder::endOpen(NeuQuantTask& encode, int16_t loop) {
    encode.setSize(m_gifFile->SWidth = encode.m_frameWidth, m_gifFile->SHeight = encode.m_frameHeight);
    m_gifFile->SColorResolution = 8;
    m_gifFile->SBackGroundColor = 0;
    m_gifFile->SColorMap = nullptr;
    uint8_t appExt[11] = {
            'N', 'E', 'T', 'S', 'C', 'A', 'P', 'E',
            '2', '.', '0'
    };
    uint8_t appExtSubBlock[3] = {
            0x01,       // hex 0x01
            0x00, 0x00  // little-endian short. The number of times the loop should be executed.
    };
    memcpy(appExtSubBlock + 1, &loop, sizeof(loop));
    GifAddExtensionBlockFor(m_gifFile, APPLICATION_EXT_FUNC_CODE, sizeof(appExt), appExt);
    GifAddExtensionBlockFor(m_gifFile, CONTINUE_EXT_FUNC_CODE, sizeof(appExtSubBlock), appExtSubBlock);
    return true;
}

void GifEncoder::cleartask(NeuQuantTask& task) {
    //if (task.colorMap != nullptr)
    //    GifFreeMapObject(task.colorMap);
    if (task.m_allocSize >= 0) {
        free(task.thepicture);
        task.thepicture = nullptr;
    }
    task.m_allocSize = -1;
    //if(task.rasterBits != nullptr)
    //delete[] task.rasterBits;
}
