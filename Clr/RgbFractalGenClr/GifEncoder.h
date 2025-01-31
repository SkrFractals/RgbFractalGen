//
// Created by xiaozhuai on 2020/12/20.
//

#ifndef GIF_GIFENCODER_H
#define GIF_GIFENCODER_H

#include <string>
#include <vector>
#include "algorithm/NeuQuant.h"
#include <mutex>
#include "giflib/gif_lib.h"

public enum GifEncoderPixelFormat : uint8_t {
    PIXEL_FORMAT_UNKNOWN = 0,
    PIXEL_FORMAT_BGR = 1,
    PIXEL_FORMAT_RGB = 2,
    PIXEL_FORMAT_BGRA = 3,
    PIXEL_FORMAT_RGBA = 4,
};

public enum GifEncoderTryWriteResult : uint8_t {
    Failed = 0,				// Error has occured, and the file was aborted.
    Waiting = 1,			// Next frame was not written because it was either not supplied yet, or it's task is still running.
    FinishedFrame = 2,		// Next frame was successfully written into the stream
    FinishedAnimation = 3	// The stream was sucessfully finished
};

public struct NeuQuantTask {

    // Paralellism variables:
    ColorMapObject* colorMap;
    GifByteType* rasterBits;
    int frameIndex;
    int m_delay;
    bool finished;
    bool failed;
    int m_allocSize;
    int m_frameWidth;
    int m_frameHeight;
    //int factor;
    NeuQuant neuquant = NeuQuant();
    NeuQuantTask* nextTask;
    std::thread* thisTask;
    uint8_t* thepicture;
    int lengthcount;        /* lengthcount = H*W*3 */
    //int m_quality;          /* sampling factor 1..30 */
    int samplepixels;
    System::Threading::CancellationToken* token;

    NeuQuantTask() {
        token = nullptr;
        nextTask = nullptr;
        finished = failed = false;
        thisTask = nullptr;
        thepicture = nullptr;
        m_allocSize = -1;
        colorMap = nullptr;
        rasterBits = nullptr;
        frameIndex = m_frameWidth = m_frameHeight = -1;
    }
    void setSize(int w, int h) {
        lengthcount = 3 * (m_frameWidth = w) * (m_frameHeight = h);
    }
    bool alloc(int needSize) {
        if (m_allocSize >= (needSize *= lengthcount)) // already allocated enough
            return false;
        void* tryalloc = m_allocSize <= 0 ? malloc(m_allocSize = needSize) : realloc(thepicture, m_allocSize = needSize); // try allocate or reallocate
        if (tryalloc == nullptr)
            return true; // failed to allocate memory
        thepicture = (uint8_t*)tryalloc;
        return false;
    }
    bool convertToBGR(GifEncoderPixelFormat format, uint8_t* dst, const uint8_t* src, const int len) {
        switch (format) {
        case PIXEL_FORMAT_BGR:
            memcpy(dst, src, len);
            break;
        case PIXEL_FORMAT_RGB:
            for (const uint8_t* dstEnd = dst + len; dst < dstEnd; src += 3) {
                *(dst++) = *(src + 2);
                *(dst++) = *(src + 1);
                *(dst++) = *(src);
            }
            break;
        case PIXEL_FORMAT_BGRA:
            for (const uint8_t* dstEnd = dst + len; dst < dstEnd; src += 4) {
                *(dst++) = *(src);
                *(dst++) = *(src + 1);
                *(dst++) = *(src + 2);
            }
            break;
        case PIXEL_FORMAT_RGBA:
            for (const uint8_t* dstEnd = dst + len; dst < dstEnd; src += 4) {
                *(dst++) = *(src + 2);
                *(dst++) = *(src + 1);
                *(dst++) = *(src);
            }
            break;
        default:
            return false;
        }
        return true;
    }
    bool getColorMap(const int quality, System::Threading::CancellationToken* canceltoken) {
        colorMap = GifMakeMapObject(256, nullptr);
        return learn(canceltoken == nullptr ? token : canceltoken, quality);
    }
    bool getRasterBits(uint8_t* pixels, System::Threading::CancellationToken* canceltoken) {
        const int pix = m_frameWidth * m_frameHeight;
        rasterBits = (GifByteType*)malloc(pix);

#define GRB_BODY(i) const auto i3 = i * 3; rasterBits[i++] = neuquant.inxsearch( pixels[i3], pixels[i3 + 1], pixels[i3 + 2]);

        if ((canceltoken = canceltoken == nullptr ? token : canceltoken) == nullptr) {
            // without cancellation token
            for (int i = 0, x = pix; x > 0; --x) {
                GRB_BODY(i)
            }
            return false;
        }
        // with cancellation token
        auto& tokenR = *canceltoken;
        for (int i = 0, y = m_frameHeight; y > 0; --y) {
            if (tokenR.IsCancellationRequested)
                return true;
            for (int x = m_frameWidth; x > 0; --x) {
                GRB_BODY(i)
            }
        }
        return false;
    }
private:
    bool learn(System::Threading::CancellationToken* canceltoken, const int quality) {
        int s = samplepixels = lengthcount / (3 * quality);
        neuquant.factor = 1;

        canceltoken = canceltoken == nullptr ? token : canceltoken;

        if (canceltoken != nullptr) // find a balanced factor of samplepixels
            for (int t = 2, ft = neuquant.factor * t, st = s / t; ft - st < s - neuquant.factor; ft = neuquant.factor * t, st = s / t) {
                if ((s % t) == 0) {
                    neuquant.factor = ft;
                    s = st;
                    continue;
                }
                ++t;
            }
        neuquant.initnet(/*thepicture,*/ lengthcount, quality, samplepixels);
        if (neuquant.learn(thepicture, canceltoken))
            return true;
        neuquant.unbiasnet();
        neuquant.inxbuild();
        neuquant.getcolourmap((uint8_t*)colorMap->Colors);
        return false;
    }

};

class GifEncoder {

    // ORIGINALS:
    //bool open(const std::string &file, int width, int height, int quality, bool useGlobalColorMap, int16_t loop, int preAllocSize = 0);
    //bool push(PixelFormat format, const uint8_t *frame, int width, int height, int delay);
    //bool close();

public:
    GifEncoder() = default;

#pragma region ORIGINAL
    /**
     * create gif file - original synchronous version for compatibily
     *
     * @param file file path
     * @param width gif width
     * @param height gif height
     * @param quality 1..30, 1 is best
     * @param useGlobalColorMap if true each frame wil lhave it's own color map, and gets encoded during a push, if false, all framnes get encoded at close
     * @param loop loop count, 0 is endless
     * @param preAllocSize For better performance, it's suggested to set preAllocSize. If you can't determine it, set to 0.
     *        If use global color map (you won't, it's disabled in this threading version), all frames size must be same, and preAllocSize = width * height * 3 * nFrame
     *        If use local color map, preAllocSize = MAX(width * height) * 3
     * @return success
     */
    bool open(const std::string& file, int width, int height, int quality, bool useGlobalColorMap, int16_t loop, int preAllocSize = 0);

    /**
     * add frame - original compatibility, doesn't support cancellation or out of order frame pushing
     *
     * @param format pixel format
     * @param frame frame data
     * @param width frame width
     * @param height frame height
     * @param delay delay time 0.01s
     * @param task - starts its own push task and puts it into the reference
     * @return
     */
    bool push(GifEncoderPixelFormat format, const uint8_t* frame, int width, int height, int delay);

    /**
     * close gif file
     *
     * @return
     */
    bool close();

#pragma endregion

#pragma region PARALLEL
    /**
    * create gif file
    *
    * @param file file path
    * @param width gif width
    * @param height gif height
    * @param quality 1..30, 1 is best
    * @param loop loop count, 0 is endless
    * @token cancellation token pointer, send nullptr if you don't have one or don't want it to be cancellable
    * @return success
    */
    bool open_parallel(const std::string& file, const int width, const int height,
                       const int quality, int16_t loop, System::Threading::CancellationToken* token);

    /**
    * add frame - not cancellable (or cancellable from open's token)
    *
    * @param format pixel format
    * @param frame frame data
    * @param width frame width
    * @param height frame height
    * @param delay delay time 0.01s
    * @param frameIndex -1 for original-like in order pushing, otherwise supply the frame index (and don't mix or mess that up or else you get a failed return when closing)
    * @return
    */
    inline bool push_parallel(GifEncoderPixelFormat format, uint8_t* frame, const int width, const int height, const int delay, const int frameIndex) {
        return push_parallel(format, frame, width, height, delay, frameIndex, nullptr);
    }

    /**
     * add frame - only sequential
     *
     * @param format pixel format
     * @param frame frame data
     * @param width frame width
     * @param height frame height
     * @param delay delay time 0.01s
     * @token cancellation token pointer, send nullptr if you don't have one or don't want it to be cancellable, or if you want to use the one supplied from open()
     * @return
     */
    inline bool push_parallel(GifEncoderPixelFormat format, uint8_t* frame, const int width, const int height, const int delay, System::Threading::CancellationToken* token) {
        return push_parallel(format, frame, width, height, delay, -1, token);
    }

    /**
    * add frame - only sequential, not cancellable (or cancellable from open's token)
    *
    * @param format pixel format
    * @param frame frame data
    * @param width frame width
    * @param height frame height
    * @param delay delay time 0.01s
    * @token cancellation token pointer, send nullptr if you don't have one or don't want it to be cancellable, or if you want to use the one supplied from open()
    * @return
    */
    inline bool push_parallel(GifEncoderPixelFormat format, uint8_t* frame, const int width, const int height, const int delay) {
        push_parallel(format, frame, width, height, delay, -1, nullptr);
    }

    /**
    * add frame - BGR, not cancellable (or cancellable from open's token)
    *
    * @param frame frame data
    * @param width frame width
    * @param height frame height
    * @param delay delay time 0.01s
    * @param frameIndex -1 for original-like in order pushing, otherwise supply the frame index (and don't mix or mess that up or else you get a failed return when closing)
    * @return
    */
    inline bool push_parallel(uint8_t* frame, const int width, const int height, const int delay, const int frameIndex) {
        return  push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame, width, height, delay, frameIndex, nullptr);
    }

    /**
     * add frame - BGR, only sequential
     *
     * @param frame frame data
     * @param width frame width
     * @param height frame height
     * @param delay delay time 0.01s
     * @token cancellation token pointer, send nullptr if you don't have one or don't want it to be cancellable, or if you want to use the one supplied from open()
     * @return
     */
    inline bool push_parallel(uint8_t* frame, const int width, const int height, const int delay, System::Threading::CancellationToken* token) {
        return  push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame, width, height, delay, -1, token);
    }

    /**
    * add frame - BGR, only sequential, not cancellable (or cancellable from open's token)
    *
    * @param frame frame data
    * @param width frame width
    * @param height frame height
    * @param delay delay time 0.01s
    * @token cancellation token pointer, send nullptr if you don't have one or don't want it to be cancellable, or if you want to use the one supplied from open()
    * @return
    */
    inline bool push_parallel(uint8_t* frame, const int width, const int height, const int delay) {
        return push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame, width, height, delay, -1, nullptr);
    }

    /**
     * add frame - BGR
     *
     * @param frame frame data
     * @param width frame width
     * @param height frame height
     * @param delay delay time 0.01s
     * @param frameIndex -1 for original-like in order pushing, otherwise supply the frame index (and don't mix or mess that up or else you get a failed return when closing)
     * @token cancellation token pointer, send nullptr if you don't have one or don't want it to be cancellable, or if you want to use the one supplied from open()
     * @return
     */
    inline bool push_parallel(uint8_t* frame, const int width, const int height, const int delay, const int frameIndex, System::Threading::CancellationToken* token) {
        return push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame, width, height, delay, frameIndex, token);
    }

    /**
     * add frame
     *
     * @param format pixel format
     * @param frame frame data
     * @param width frame width
     * @param height frame height
     * @param delay delay time 0.01s
     * @param frameIndex -1 for original-like in order pushing, otherwise supply the frame index (and don't mix or mess that up or else you get a failed return when closing)
     * @token cancellation token pointer, send nullptr if you don't have one or don't want it to be cancellable, or if you want to use the one supplied from open()
     * @return
     */
    bool push_parallel(GifEncoderPixelFormat format, uint8_t* frame, const int width, const int height, const int delay, const int frameIndex, System::Threading::CancellationToken* token);

    GifEncoderTryWriteResult tryWrite(System::Threading::CancellationToken* token);
#pragma endregion

    inline bool isFinishedAnimation() const { return finishedAnimation; }
    inline int getFinishedFrame() const { return finishedFrame; }

    bool m_parallel = false;

private:
    GifEncoderTryWriteResult tryWriteInternal(System::Threading::CancellationToken* token);
    /** Attempts to open a file, returns true if failed (unlike most other function returns here) */
    bool initOpen(const std::string& file, int width, int height);
    bool endOpen(NeuQuantTask& encode, int16_t loop);
    bool pushTask(NeuQuantTask& task, System::Threading::CancellationToken* token);
    void cleartask(NeuQuantTask& task);

    /*inline bool isFirstFrame() const {
        return m_frameCount == 0;
    }*/

    inline void reset() {
        // Free the task memory
        while (data_write != nullptr) {
            cleartask(*data_write);
            data_write = data_write->nextTask;
        }
        data_encode = nullptr;
        m_allFrameDelays.clear();
        m_frameCount = 0;
    }

    void encodeFrame(NeuQuantTask& encode, int delay);

    NeuQuantTask* GifEncoder::nextEncode(int index);

    bool abort(bool fail);

    void freeEverything();

   

private:

    // Original variables
    void* m_gifFileHandler = nullptr;

    int m_quality = 10;
    bool m_useGlobalColorMap = false;

    //uint8_t* m_framePixels = nullptr;

    std::vector<int> m_allFrameDelays{};
    int m_frameCount = 0;
    //int m_frameWidth = -1;
    //int m_frameHeight = -1;

    // SkrFractals additions:
    NeuQuantTask* data_encode = nullptr, * data_write = nullptr;
    bool finishedAnimation = false;
   
    int finishedFrame = 0;
    int addedFrames = 0;
    std::mutex mtx;
    System::Threading::CancellationToken* cancelToken;
};


#endif //GIF_GIFENCODER_H
