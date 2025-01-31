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
        return abort(true);
    m_quality = quality;
    m_useGlobalColorMap = useGlobalColorMap; // global color map is available in old synchronous mode
    m_parallel = false; // flag that we are NOT running a parallel mode. whis will make any parallel calls refuse
    auto& encode = *data_encode;
    encode.token = cancelToken = nullptr;
    if (preAllocSize > 0)
        encode.thepicture = (uint8_t*)malloc(encode.m_allocSize = preAllocSize);
    encode.setSize(width, height);
    return endOpen(encode, loop);
}
bool GifEncoder::push(GifEncoderPixelFormat format, const uint8_t* frame, int width, int height, int delay) {
    // this should work the samw way as the original, but it doesn't work either
    
    if (m_gifFile == nullptr || frame == nullptr || m_parallel)
        return abort(true); // no file, no image, opened parallel
    if (m_useGlobalColorMap) {
        auto& encode = *data_encode;
        if (encode.m_frameWidth != width || encode.m_frameHeight != height)
            return abort(true); //throw std::runtime_error("Frame size must be same when use global color map!");
        encode.setSize(width, height);
        // allocate framecount+1 worth of memory and try to convert into BGR
        if (encode.alloc(m_frameCount + 1) || !encode.convertToBGR(format, encode.thepicture + encode.lengthcount * m_frameCount, frame, encode.lengthcount)) 
            return abort(true); // failed to allocate memory or convert to BGR
        m_allFrameDelays.push_back(delay);
    } else {
        auto encodePtr = nextEncode(-1);
        if (encodePtr == nullptr)
            return abort(true);
        auto& encode = *encodePtr;
        encode.m_delay = delay;
        encode.setSize(width, height);
        // allocate 1 frame worth of memory and try to convert into BGR
        if (encode.alloc(1) || !encode.convertToBGR(format, encode.thepicture, frame, encode.lengthcount)) 
            return abort(true); // failed to allocate memory
        encode.getColorMap(m_quality, nullptr);
        encode.getRasterBits(encode.thepicture, nullptr);
        encodeFrame(encode, delay);
    }
    ++m_frameCount;
    return true;
}
bool GifEncoder::close() {
    if (m_gifFile == nullptr)
        return abort(true); // file not opened - fail
    auto& write = *data_write;
    if (m_parallel) // if parallel mode, just mark the last task as finished wihtout starting it, that will let the tryWrite know it's the end
        return write.finished = true; 
    if (m_useGlobalColorMap) {
        // encode the whole gif here if global map (this is why I can't parallelize this)
        write.getColorMap(m_quality, nullptr);
        m_gifFile->SColorMap = write.colorMap;
        // write all the global map frames into the file
        for (int i = 0; i < m_frameCount; ++i) {
            write.getRasterBits(write.thepicture + write.lengthcount * i, nullptr);
            encodeFrame(write, m_allFrameDelays[i]);
        }
    }
    // finish the file
    int error, ok = true;
    if (EGifSpew(m_gifFile) == GIF_ERROR) {
        EGifCloseFile(m_gifFile, &error);
        m_gifFileHandler = nullptr;
        reset();
        return false;
    }
    freeEverything();
    return ok && (finishedAnimation = true);
}

bool GifEncoder::abort(bool fail) {
    if (!fail)
        return true;
    int error;
    if(m_gifFile != nullptr)
        EGifCloseFile(m_gifFile, &error);
    //freeEverything();
    m_gifFileHandler = nullptr;
    reset();
    return false;
}

void GifEncoder::freeEverything() {
    if (m_gifFile == nullptr)
        return;
    int extCount = m_gifFile->ExtensionBlockCount;
    auto* extBlocks = m_gifFile->ExtensionBlocks;
    int savedImageCount = m_gifFile->ImageCount;
    auto* savedImages = m_gifFile->SavedImages;
    if(extCount > 0)
        GifFreeExtensions(&extCount, &extBlocks);
    if (savedImageCount > 0) {
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
    }
    m_gifFileHandler = nullptr;
    reset();
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
    if (initOpen(file, width, height)) // open the file
        return abort(true); // open failed
    m_quality = quality;
    m_useGlobalColorMap = false; // globa color map dones't work it parallel mode, because i am literally parallel batching the separate local map learnings
    m_parallel = true; // flag that we are running a parallel mode. whis will make any synchronous calls refuse
    auto& encode = *data_encode;
    encode.token = cancelToken = token; // give the task a cancellation token
    encode.setSize(width, height);
    return endOpen(encode, loop); // write the file header
}
bool GifEncoder::push_parallel(GifEncoderPixelFormat format, uint8_t* frame, const int width, const int height, const int delay, const int frameIndex, System::Threading::CancellationToken* token) {
    if (m_gifFile == nullptr || frame == nullptr || !m_parallel)
        return abort(true);
    auto encodePtr = nextEncode(frameIndex); // fetch the task data struct to run the task from, and make another empty one after that
    if (encodePtr == nullptr)
        return abort(true); // fail if that failed
    auto& encode = *encodePtr;
    encode.m_delay = delay;
    encode.setSize(width, height); // set the frame size
    if (format == GifEncoderPixelFormat::PIXEL_FORMAT_BGR)
        encode.thepicture = frame; // use the pixels directly without copying if they are alraedy BGR
    else 
        if (encode.alloc(1) || !encode.convertToBGR(format, encode.thepicture, frame, encode.lengthcount)) // otherwise, do the copy allocation coversion
        return abort(true); // failed to allocate memory or convert to BGR
    if(encode.getColorMap(m_quality, token)) // run the neuquant learning
        return abort(encode.failed = true); // fail if cancelled
    if(encode.getRasterBits(encode.thepicture, token)) // make raster bits
        return abort(encode.failed = true);
    return encode.finished = true;
}
GifEncoderTryWriteResult GifEncoder::tryWrite(System::Threading::CancellationToken* token) {
    GifEncoderTryWriteResult r;
    mtx.lock();
    r = tryWriteInternal(token);
    if (r == GifEncoderTryWriteResult::Failed)
        abort(true);
    mtx.unlock();
    return r;
}
GifEncoderTryWriteResult GifEncoder::tryWriteInternal(System::Threading::CancellationToken* token) {
    if (!m_parallel)
        return GifEncoderTryWriteResult::Failed; // This should only be called to attempt to write frames in parallel mode! With original synchronous mode, just call the close!
    if (finishedAnimation)
        return GifEncoderTryWriteResult::FinishedAnimation; // Already finished (you seem to have called the tryWrite even after it resturned finished once already)
    if (m_gifFile == nullptr || data_write->failed)
        return GifEncoderTryWriteResult::Failed;    // No file opened, or something failed in the frame pushing
    auto write = data_write;
    // Find the task containing the next frame to write into the file
    NeuQuantTask* prev = nullptr;
    while (write->frameIndex > finishedFrame) {
        if (write->nextTask == nullptr) {
            return write->finished
                ? GifEncoderTryWriteResult::Failed	    // You must have called finish while having gaps in supplied out of order frames!
                : GifEncoderTryWriteResult::Waiting;	// You have not supplied the next out of order frame yet, so the Task has not even started running yet.
        }
        write = (prev = write)->nextTask;
    }
    if (write == nullptr)
        return GifEncoderTryWriteResult::Failed;    // This should never happen, but I'm gonna check it anyway. (the last taskdata should always have frameIndex -1, so the while should not go to nullptr from there)
    auto& w = *write;
    if (!w.finished)
        return GifEncoderTryWriteResult::Waiting; // the task for the next sequential frame is not finished yet, try again later
    if (w.nextTask == nullptr) {
        // i always assign to next right before i start its task, so if i set the finished flag without starting a task, that means i have called close() and there's no next frame
        // The finish normally closed the filestream, but not I will do it here
        // and the next close() just sets the encodeTaskData->finished = true to trigger this code when all frames wating to be written have been written
        m_parallel = false; // makes sure it actually writed the file end instead of setting the finish flag again
        return close() ? GifEncoderTryWriteResult::FinishedAnimation : GifEncoderTryWriteResult::Failed; // Has writing the full file finished or failed?
    }
    // this nextTask is not null, so this task is actually a frame and not an end yet, so write that frame into the file
    if (w.frameIndex < finishedFrame) {
        // You must have messed up supplying the frameIndexes! The should be a queued frame with an index less than what has alraedy been written!
        // This has probablty happened because you either mixed the automatic ordered, and out of order AddFrame.
        // Or you messed out the out of order frame counting and supplied the same index twice.
        return GifEncoderTryWriteResult::Failed;
    }
    if (w.thisTask != nullptr && w.thisTask->joinable())
        w.thisTask->join(); // join the task if if was created here
    // write the frame into the file
    //w.getRasterBits(w.thepicture, token);
    encodeFrame(w, w.m_delay);
    // increment the counter of frames written into the file
    ++finishedFrame;
    // clear any copy allocated pixel data in the task that wouln't be freed elsewhere
    cleartask(w);
    //remove the written task from the list:
    if (prev == nullptr) {
        // Now that I wrote the frame, I can forget the task data, and move on to the next one.
        data_write = data_write->nextTask;
    } else {
        // The first queued writeTaskData was not the next sequential frame to be written and we were not writing that one...
        // ...so we have to skip and forget the one later in the list that we actually wrote
        prev->nextTask = w.nextTask;
    }
    return GifEncoderTryWriteResult::FinishedFrame; // Has writing this next frame finished or failed?
}

NeuQuantTask* GifEncoder::nextEncode(int index) {
    NeuQuantTask* encode = nullptr;
    // make sure to monitor lock this part to preserve the list integrity while multithreading
    mtx.lock();
    if (m_gifFile == nullptr || (encode = data_encode)->failed)
        return nullptr;
    //create the new empty task on the end of the list, we will return the previous one (the one before the new last one)
    data_encode = encode->nextTask = new NeuQuantTask(); // make a next one for the next call of Addframe to work on
    //increment number of pushed frames
    ++m_frameCount;
    mtx.unlock();
    // add automatic sequential, or manual our of order frameIndex
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
    m_gifFile->SWidth = encode.m_frameWidth; m_gifFile->SHeight = encode.m_frameHeight;
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
    if (task.m_allocSize >= 0) {
        // only clear the pixeldata if they were copy allocated, not when using a byte pointer from the push directly
        free(task.thepicture);
        task.thepicture = nullptr;
    }
    task.m_allocSize = -1;
}
