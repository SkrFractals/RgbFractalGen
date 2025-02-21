// Allow debug code, comment this when releasing for slightly better performance
#define CUSTOMDEBUG



#define MINTASKS 1

#pragma once
#include <vector>
#include <mutex>
#include <random>
#include <queue>
#include <array>
#include <map>
#include "Vector.h"//#include <functional> // Required for std::function
#include "Fractal.h"

#ifdef CUSTOMDEBUG
#include <chrono>
#endif

#define CLR
// CLR VS CPP
#ifdef CLR
#define STRING System::String^
#define VOIDTYPE System::Void
#define TUPLEPACK double, double
#else
#include <functional>
#define STRING std::string
#define VOIDTYPE void
#define TUPLEPACK std::pair<float,float>, std::pair<float,float>
#endif

namespace RgbFractalGenClr {

#ifdef CLR
    using namespace System;
    using namespace System::Collections::Generic;
    using namespace System::Collections::Concurrent;
    using namespace System::Threading;
    using namespace System::Threading::Tasks;
    //using namespace System::Numerics;
#endif

    enum BitmapState : uint8_t {
        Queued = 0,				// Hasn't even started generating the dots yet (and it might stay this way if OnlyImage)
        Dots = 1,				// Started Generating Dots
        Void = 2,				// Started Dijksring the Void
        Drawing = 3,			// Started drawing
        DrawingFinished = 4,	// Finished drawing
        UnlockedRAM = 5,		// Unlocked bitmap without encoding
        Encoding = 6,			// Started Encoding
        EncodingFinished = 7,	// Finished encoding
        FinishedBitmap = 8,		// Finished bitmap (finished drawing if not encodeGIF, or fnished encoding if encodeGIF)
        Unlocked = 9,			// Unlocked bitmap
        Error = 10				// Unused, unless error obviously
    };
    private enum TaskState : uint8_t {
        Free = 0,		// The task has not been started yet, or already finished and joined and ready to be started again
        Done = 1,	    // The task if finished and ready to join without waiting
        Running = 2     // The task is running
    };
    enum ParallelType : uint8_t {
        OfAnimation = 0,// Parallel batching of each animation image into it's own single thread. No pixel conflicts, and more likely to use all the threads consistently. Recommended for animations.
        OfDepth = 1,    // Parallelism of each single image, generates the single next image faster, but there might be rare conflicts of accessing pixels, and it's a bit slower overall if generating an animation. Used automatically for OnlyImage.
        OfRecursion = 2 // Deprecated older image parallelism, use OfDepth instead
    };
    enum GenerationType : uint8_t {
        OnlyImage = 0,      // Only generates the first single image, and then halts. Or stops generating animation, if selected during an animation generation and at least one frame has already been finished.
        AnimationRAM = 1,   // Will only generate the animation for preview, faster than encoding GIF, but cannot save the GIF when finished.
        EncodeGIF = 2,      // Will encode a GIF while generating the animation, will be slower than generating the animation without GIF.
        GlobalGIF = 3,      // Will encode a gif but only analyze the first frame for a color map, much faster, but not recommended for shifting hues or if you want the highest possible quality
        Mp4 = 4,				// Will encode an mp4
        AllParam = 5,
        HashParam = 6
    };
    public struct FractalTask {
#ifndef CLR
        std::thread task;       // Parallel Animation Tasks
#endif

        TaskState state;        // States of Animation Tasks
        Vector** buffer;        // Buffer for points to print into bmp
        int16_t** voidDepth;    // Depths of Void
        Vector** voidNoise;	    // Randomized noise samples for the void noise
        std::queue<std::pair<int16_t, int16_t>> 
            voidQueue;          // Dijkstra queue for void depth
        std::map<int64_t, std::array<Vector, 3>>
            F;                  // ???
        std::map<int64_t, std::array<Vector, 3>>
            H;		            // Mixed children color
        Vector I[3];            // Pure parent color
        uint8_t huemod;
        bool taskStarted;		// Additional safety, could remove if it never gets triggered for a while
        uint16_t taskIndex;     // The task index
        int32_t bitmapIndex;   // The bitmapIndex of which the task is working on
        int16_t applyWidth,
            applyHeight;        // can be smaller for previews
        short widthBorder,
            heightBorder;       // slightly below the width and height
        float rightEnd,
            downEnd;            // slightly beyond width and depth, to ensure even bloomed pixels don't cutoff too early
        float bloom0;
        float bloom1;           // = selectBloom + 1;
        float upleftStart;      // = -selectBloom;
        float lightNormalizer;	// maximum brightness found in the buffer, for normalizing the final image brightness
        float voidDepthMax;	    // maximum reached void depth during the dijkstra search, for normalizing the void intensity
        std::tuple<float, float, std::pair<float, float>*>*
            preIterate;

        FractalTask() {
            taskStarted = false;
            state = TaskState::Free;
            buffer = nullptr;
            voidDepth = nullptr;
        }
    };
#ifdef CLR
    public ref class FractalGenerator {
#else
    public class FractalGenerator {
#endif

#ifdef CUSTOMDEBUG
    private:
        // Debug variables
        STRING logString;       // Debug Log
        std::chrono::steady_clock::time_point* 
            startTime;          // Start Stopwatch
        long long initTimes, iterTimes, voidTimes, drawTimes, gifsTimes;
#endif
#pragma region Variables
    private:
        // Definitions
        std::vector<Fractal*>* fractals;     // Fractal definitions
        Fractal* f;             // Selected fractal definition
        float* childAngle;		// A copy of selected childAngle
        float selectPeriodAngle;// Angle symmetry of current fractal
        float applyPeriodAngle; // Angle symmetry corrected for periodMultiplier
       
        //Vector* colorBlend;     // Color blend of a selected fractal for more seamless loop (between single color to the sum of children)
        Fractal::CutFunction* 
            cutFunction;        // Selected CutFunction pointer
        int64_t applyCutparam;	// Applied cutparam (selected or random)
        uint16_t selectMaxIterations;  // maximum depth of iteration (dependent on fractal/detail/resolution)
        float logBase;          // Size Log Base
        int64_t startCutParam = 0;
        std::map<std::string, uint32_t>* 
            hash;
        std::map<int64_t, std::array<Vector, 3>>* 
            colorBlends;
        float* emptyFloat;      // Preallocated empty angle array when it doesn't exist
        uint16_t maxChildren;   // The maximum number of children any fractal definition has (for presizing the copy arrays)

        // Resolution
        int16_t allocatedWidth;	// How much buffer width is currently allocated?
        int16_t allocatedHeight;// How much buffer height is currently allocated?

        // Frames
        uint16_t frames;                // Number of frames (bitmap.Length)
        BitmapState* bitmapState;       // Flag if the bitmap was finished generating (ready to encode GIF and UnlockBits)
        uint16_t finalPeriodMultiplier; // How much will the period get finally stretched? (calculated for seamless + user multiplier)
        uint16_t debug;				    // Debug frame count override
        uint32_t bitmapsFinished;		// How many bitmaps are completely finished generating? (ready to display, encoded if possible)
        uint8_t previewFrames;           // how many preview frames are we making for this resolution?
        uint32_t nextBitmap;			// How many bitmaps have started generating? (next task should begin with this one)
        int32_t allocatedFrames;        // How many bitmap frames are currently allocated?
        int16_t applyZoom;		        // Applied zoom (selected or random)
        uint16_t applyMaxIterations;
        float applyDetail;
        GenerationType 
            applyGenerationType;        // Applied generation type (for example a gif error can temporarily change it to AnimationRAM)

        // Color
        int16_t applyBlur;          // Applied blur level (1 + selectBlur, because 0 would not render any image)
        int16_t applyColorPalette;  // RGB or BGR? (0/1)
        int16_t applyHueCycle;      // Hue Cycling Direction (-1,0,1)
        uint8_t hueCycleMultiplier;	// How fast should the hue shift to loop seamlessly?
        uint16_t applyVoid;
        bool applyPreviewMode;

        // Void
        float ambnoise;     // Normalizer for maximum void depth - Precomputed amb * noise
        float bloom1;       // Precomputed bloom + 1
        std::mt19937* randomGenerator;
        std::uniform_real_distribution<float>* randomDist;
        //Random random;    // Random generator

        // Threading
        FractalTask* tasks;     // All available tasks
        ParallelType 
            applyParallelType;  // Safely copy parallelType in here so it doesn't change in the middle of generation
        int16_t applyMaxTasks;	// Safely copy maxTasks in here so it doesn't change in the middle of generation
        int16_t maxDepth;		// Maximum depth for Recusrion parallelism
        int16_t allocatedTasks;	// How many buffer tasks are currently allocated?
#ifdef CLR
        Object^ 
            taskLock = gcnew Object();  // Monitor lock
        CancellationTokenSource^ 
            cancel;                     // Cancellation Token Source
        CancellationToken token;        // Cancellation token
        CancellationTokenSource^
            gifCancel;                  // GIF Cancellation Token Source
        CancellationToken gifToken;     // GIF Cancellation token
        Task^ mainTask;                 // Main Generation Task
        array<Task^>^ parallelTasks;    // Parallel Animation Tasks
#else
        std::mutex taskLock;        // Monitor lock
        std::atomic<bool>
            cancelRequested;        // Cancellation Token Source
        std::atomic<bool>
            gifCancelRequested;     // Cancellation Token Source for the GifEncoder
        std::thread mainTask;       // Main Generation Task
#endif
        std::tuple<uint16_t, TUPLEPACK, uint8_t, int64_t, uint8_t>*
            tuples;                     // Queue struct for GenerateDots_OfDepth
        uint8_t isWritingBitmaps = 8; // counter and lock to try writing bitmap toa file once every 8 threads

        // Export
        void* gifEncoder;   // Export GIF encoder
        void* mp4Encoder;
        int32_t gifSuccess;	// Temp GIF file "gif.tmp" successfuly created
        bool gifThread = false;
        STRING gifTempPath;         // Temporary GIF file name
#ifdef CLR
        System::Drawing::Rectangle 
            rect;                   // Bitmap rectangle TODO implement
#endif

    public:

        // Selected Settings
        int16_t selectFractal,      // Fractal definition (0-fractals.Length)
            selectChildAngle,       // Child angle definition (0-childAngle.Length)
            selectChildColor,       // Child color definition (0-childColor.Length)
            selectCut;              // Selected CutFunction index (0-cutFunction.Length)
        int32_t selectCutparam;     // Cutparam seed (0-maxCutparam)
        uint16_t selectWidth,       // Resolution width (1-X)
            selectHeight;           // Resolution height (1-X)
        int16_t selectPeriod,       // Parent to child frames period (1-X)
            selectPeriodMultiplier, // Multiplier of frames period (1-X)
            selectZoom,             // Zoom direction (-1 out, 0 random, 1 in)
            selectDefaultZoom,      // Default skipped frames of zoom (0-frames)
            selectExtraSpin,        // Extra spin speed (0-X)
            selectDefaultAngle,     // Default spin angle (0-360)
            selectSpin,             // Spin direction (-2 random, -1 anticlockwise, 0 none, 1 clockwise, 2 antispin)
            selectHue,              // -1 = random, 0 = RGB, 1 = BGR, 2 = RGB->GBR, 3 = BGR->RBG, 4 =RGB->BRG, 5 = BGR->GRB
            selectExtraHue,         // Extra hue angle speed (0-X)
            selectDefaultHue,       // Default hue angle (0-360)
            selectAmbient,          // Void ambient strength (0-120)
            selectVoid;				// Void Noise Size
        float selectNoise,		    // Void noise strength (0-3)
            selectSaturate,			// Saturation boost level (0-1)
            selectDetail,			// MinSize multiplier (1-10)
            selectBloom;            // Dot bloom level (pixels)
        int16_t selectBlur,         // Dot blur level
            selectBrightness;       // Light normalizer brightness (0-300)
        ParallelType 
            selectParallelType;     // 0 = Animation, 1 = Depth, 2 = Recursion
        int16_t selectMaxTasks,     // Maximum allowed total tasks
            selectDelay,			// Animation frame delay
            selectFps = 1;
        GenerationType 
            selectGenerationType;   // 0 = Only Image, 1 = Animation, 2 = Animation + GIF
        int32_t cutparamMaximum;    // Maximum seed for the selected CutFunction
        bool
            restartGif = false; // Makes me restart the gif encoder (called when delay is changed, which should restart the encoder, but not toss the finished bitmaps)
        bool selectPreviewMode = false;

        uint8_t* childColor;	// A copy of childCoolor for allowing BGR
        bool debugmode = false;
        System::String^ debugString;
        int16_t* counter = new int16_t[11];

        uint8_t** bitmap;
        uint16_t* strides;

        static const int depthdiv = 4;

#ifdef CLR
        Action^ UpdatePreview;
        Action<const uint16_t, const uint16_t, const uint16_t>^ NewBitmap;
        Action^ UnlockBitmaps;
        Action^ ReencodeBitmaps;
        Action^ SetRect;
        Action^ AllocateBitmaps;
        Action<const uint16_t>^ ReLockBits;
        Action<const uint16_t>^ UnlockBits;

        /*gcroot<Action<const uint16_t bitmapIndex, const uint16_t w, const uint16_t h>^> NewBitmap;
        gcroot<Action<>^> UnlockBitmaps;
        gcroot<Action<>^> ReencodeBitmaps;
        gcroot<Action<>^> SetRect;
        gcroot<Action<>^> AllocateBitmaps;
        gcroot<Action<const uint16_t bitmapIndex>^> ReLockBits;*/
#else
        std::function<void(const uint16_t bitmapIndex, const uint16_t w, const uint16_t h)> NewBitmap;
        std::function<void()> UnlockBitmaps;
        std::function<void()> ReencodeBitmaps;
        std::function<void()> SetRect;
        std::function<void()> AllocateBitmaps;
        std::function<void(const uint16_t bitmapIndex)> ReLockBits;
#endif

#pragma endregion

#pragma region Init
    public:
        FractalGenerator();
    private:
        VOIDTYPE DeleteEncoder();
        VOIDTYPE DeleteBuffer(const FractalTask& task, const uint16_t vh);
        VOIDTYPE NewBuffer(FractalTask& task, const uint16_t vw, const uint16_t vh);
#pragma endregion

#pragma region Generate_Tasks
    private:
        inline uint32_t GetGenerateLength() { return applyGenerationType > GenerationType::OnlyImage && !applyPreviewMode ? frames : previewFrames + 1; }
        // TODO CPP
        VOIDTYPE PreviewResolution(FractalTask& task) {
            if (task.bitmapIndex < previewFrames) {
                int div = 1 << (applyParallelType == ParallelType::OfDepth ? previewFrames - task.bitmapIndex : previewFrames - task.bitmapIndex - 1);
                task.applyWidth = (short)(selectWidth / div);
                task.applyHeight = (short)(selectHeight / div);
                task.bloom0 = selectBloom / div;
            } else {
                task.bloom0 = selectBloom;
                task.applyWidth = selectWidth;
                task.applyHeight = selectHeight;
            }
            task.widthBorder = (short)(task.applyWidth - 2);
            task.heightBorder = (short)(task.applyHeight - 2);
            task.bloom1 = task.bloom0 + 1;
            task.upleftStart = -task.bloom1;
            task.rightEnd = task.widthBorder + task.bloom1;
            task.downEnd = task.heightBorder + task.bloom1;
        }
        // TODO CPP
        VOIDTYPE PregenerateParam(const int32_t bitmapIndex, std::map<int64_t, std::array<Vector, 3>>* blends, int64_t& startParam) {
            std::vector<int32_t>& m = f->cutFunction[selectCut].second;
            switch(applyGenerationType) {
            case GenerationType::AllParam: startParam = (m = f->cutFunction[selectCut].second).size() > 0 && m[0] >= 0
                    ? -m[bitmapIndex] : -bitmapIndex; 
                break;
            case GenerationType::HashParam : startParam = -bitmapIndex;
                    break;
            default: 
                startParam = (m = f->cutFunction[selectCut].second).size() > 0 && m[0] >= 0 ? -m[applyCutparam] : -applyCutparam;
                break;
            };
            if (applyPreviewMode)
                return;
            uint8_t max = 3;
            blends->clear();
            int32_t prevcount;
            do {
                prevcount = static_cast<int32_t>(blends->size());
                blends->clear();
                PregenerateColor(blends, 0, startParam, 0, max);
                ++max;
            } while (prevcount != blends->size());
        }
        // TODO CPP
        VOIDTYPE PregenerateColor(std::map<int64_t, std::array<Vector, 3>>* blends, uint8_t index, int64_t inFlags, uint8_t inDepth, int8_t max/* = 2 */ ) {
            int16_t i = f->childCount;
            int64_t newFlags;
            if (++inDepth < max) {
                while (0 <= --i)
                    if ((newFlags = CalculateFlags(i, inFlags)) >= 0)
                        PregenerateColor(blends, i, newFlags, inDepth, max);
            } else {
                uint8_t v[3];
                v[0] = v[1] = v[2] = 0;
                int n = 0;
                while (0 <= --i)
                    if ((newFlags = CalculateFlags(i, inFlags)) >= 0) {
                        ++v[childColor[i]];
                        ++n;
                    }
                std::array<Vector, 3> cb;
                if (n <= 0) cb[0] = cb[1] = cb[3] = zero;
                else {
                    cb[0] = Vector(v[0], v[1], v[2]);
                    cb[1] = Y(cb[0]);
                    cb[2] = Z(cb[0]);
                }
                (*blends)[index + f->childCount * (inFlags & ((static_cast<int64_t>(1) << f->childCount) - 1))] = cb;
            }
        }
        VOIDTYPE GenerateAnimation();
        bool ApplyGenerationType();
        VOIDTYPE FinishAnimation();
        VOIDTYPE StartGif();
        VOIDTYPE FinishGif();
        VOIDTYPE StopGif(const int16_t taskIndex);
        VOIDTYPE GenerateDots(const uint32_t& bitmapIndex, const int16_t& stateIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color);
        VOIDTYPE GenerateImage(const uint16_t taskIndex);
        VOIDTYPE GenerateGif(const uint16_t taskIndex);
        VOIDTYPE GenerateDots_SingleTask(const FractalTask& task, double inXY, double AA, const uint8_t inColor, const int64_t inFlags, uint8_t inDepth);
        VOIDTYPE GenerateDots_OfDepth(const uint32_t bitmapIndex);
        // OfRecusrion is replaced with OdDepth, which is already better
        //VOIDTYPE GenerateDots_OfRecursion(const uint16_t taskIndex, double inXY, double AA, const uint8_t inColor, const int inFlags, uint8_t inDepth);
        //FinishTasks had to be unpacked directly into the code because lambdas are not supported here in a managed class
        /*VOIDTYPE FractalGenerator::FinishTasks(bool includingGif, Func<int, bool>^ lambda) {
	        for (auto tasksRemaining = true; tasksRemaining; MakeDebugString()) {
		        tasksRemaining = false;
		        for (uint16_t t = applyMaxTasks; 0 <= --t; ) {
			        FractalTask& task = tasks[t];
			        if (IsStillRunning(task)) tasksRemaining |= includingGif || task.type == 0; // Task not finished yet
			        else if (!cancel->Token.IsCancellationRequested && selectMaxTasks == applyMaxTasks) {
				        if (TryGif(task)) tasksRemaining |= lambda(t);
				        else if (includingGif) tasksRemaining = true;
			        }
		        }
	        }
        }
        VOIDTYPE FractalGenerator::FinishTasks(bool includingGif, Func<int, bool>^ lambda);*/
        bool IsTaskStillRunning(FractalTask& task);
        bool FinishTasks(bool mainLoop, uint8_t& i, bool& tasksRemaining, uint16_t& taskIndex ) {
            // bool tasksRemaining = true; uint8_t i = 3; uint16_t taskIndex = 0; while(FinishTasks(mainLoop, i, tasksRemaining, taskIndex)) tasksRemaining = lambda(taskIndex)
            while (true) {
                MakeDebugString();
                if (i <= 0)
                    return false;
                if (taskIndex == 0) {
                    if (!tasksRemaining)
                        return false;
                    else i = 3;
                    taskIndex = applyMaxTasks;
                    tasksRemaining = false;
                }
                while (0 <= --taskIndex) {
                    FractalTask& task = tasks[taskIndex];
                    if (IsTaskStillRunning(task)) {
                        tasksRemaining |= mainLoop || task.bitmapIndex >= 0 && bitmapState[task.bitmapIndex] <= BitmapState::Dots;
                    } else {
                        if (!token.IsCancellationRequested && ( // Cancel Request forbids any new threads to start
                            !mainLoop || selectMaxTasks == applyMaxTasks && applyParallelType == selectParallelType && selectGenerationType == applyGenerationType // chaning these settings yout exit, then they get updated and restart the main loop with them updated (except onDepth which must finish first)
                            )) {
                            bool r = (mainLoop && (TryWriteBitmaps(task) || TryFinishBitmap(task)));
                            if (r) tasksRemaining = true;
                            else return true; // Let the outer caller call the lamba
                        }
                    }
                }
                --i;
            }
        }
        bool TryWriteBitmaps(FractalTask& task);
        VOIDTYPE TryWriteBitmap(const uint16_t taskIndex);
        bool TryFinishBitmap(FractalTask& task);
        VOIDTYPE TryFinishBitmaps(bool gif);
#pragma endregion

#pragma region Generate_Inline
    private:
        VOIDTYPE InitPreiterate(std::tuple<float, float, std::pair<float, float>*>*& preIterateTask) {
            preIterateTask = new std::tuple<float, float, std::pair<float, float>*>[applyMaxIterations];
            for (int i = 0; i < applyMaxIterations; preIterateTask[i++] = { 0.0f, 0.0f, nullptr });
        }
        VOIDTYPE ApplyDot(const FractalTask& task, const int64_t key, const float& inX, const float& inY, const float& inDetail, const uint8_t& inColor);
        inline int64_t CalculateFlags(const int& index, const int64_t& inFlags) { return cutFunction == nullptr ? inFlags : (*cutFunction)(index, inFlags, *f); }
        inline bool TestSize(const FractalTask& task, const float& newX, const float& newY, const float& inSize);
        static const Vector Normalize(const Vector& pixel, const float lightNormalizer);
        static const Vector ApplyAmbientNoise(const Vector& rgb, const float Amb, const float Noise, const Vector& rand);
        const Vector ApplySaturate(const Vector& rgb);
        static VOIDTYPE ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p);
        inline VOIDTYPE NoiseSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p);
        inline VOIDTYPE NoiseNoSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p);
        inline VOIDTYPE NoNoiseSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p);
        inline VOIDTYPE NoNoiseNoSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p);
#pragma endregion

#pragma region TaskWrappers
    private:
        VOIDTYPE Task_Dots(System::Object^ obj);
        VOIDTYPE Task_Image(System::Object^ obj);
        VOIDTYPE Task_GenerateGif(System::Object^ obj);
        VOIDTYPE Task_TryWriteBitmap(System::Object^ obj);
        VOIDTYPE Task_OfDepth(System::Object^ obj);
        VOIDTYPE TaskOfDepth(const uint16_t taskIndex, const uint16_t tupleIndex);
        //VOIDTYPE Task_OfRecursion(System::Object^ obj);
        inline bool Join(FractalTask& task);
        inline VOIDTYPE Start(const uint16_t taskIndex, int16_t bitmap, Action<Object^>^ action, Object^ state);
        // Pack two floats into a double
        static double pack(float f1, float f2) {
            uint64_t combined_bits = (static_cast<uint64_t>(*reinterpret_cast<uint32_t*>(&f1)) << 32) | static_cast<uint64_t>(*reinterpret_cast<uint32_t*>(&f2));   // Combine into a 64-bit value
            return *reinterpret_cast<double*>(&combined_bits);    // Reinterpret as a double
        }
        // Unpack a double into two floats
        static VOIDTYPE unpack(double packed, float& f1, float& f2) {
            uint64_t combined_bits = *reinterpret_cast<uint64_t*>(&packed); // Get bits of double
            uint32_t f1_bits = combined_bits >> 32;         // Extract upper 32 bits
            uint32_t f2_bits = combined_bits & 0xFFFFFFFF;  // Extract lower 32 bits
            f1 = *reinterpret_cast<float*>(&f1_bits);       // Reinterpret as float
            f2 = *reinterpret_cast<float*>(&f2_bits);       // Reinterpret as float
        }
#pragma endregion

#pragma region AnimationParameters
    private:
        VOIDTYPE SwitchParentChild(float& angle, int8_t& spin, uint8_t& color);
        VOIDTYPE ModFrameParameters(const uint16_t width, const uint16_t height, float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color);
        VOIDTYPE IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const uint16_t blur);
        inline VOIDTYPE IncFrameSize(float& size, const uint16_t period) {
            size *= powf(f->childSize, selectZoom * 1.0f / period);
        }
#pragma endregion

#pragma region Interface_Calls
    public:
        VOIDTYPE StartGenerate();
        VOIDTYPE ResetGenerator();
        VOIDTYPE RequestCancel();
        bool SaveGif();
#ifdef CUSTOMDEBUG
        VOIDTYPE Log(STRING log, STRING line);
#endif
        VOIDTYPE DebugStart();
        static inline STRING GetBitmapState(BitmapState state) {
            switch (state) {
            case BitmapState::Queued: return "QUEUED";
            case BitmapState::Dots: return "GENERATING FRACTAL DOTS";
            case BitmapState::Void: return "GENERATING DIJKSTRA VOIDTYPE";
            case BitmapState::Drawing: return "DRAWING BITMAP (LOCKED)";
            case BitmapState::DrawingFinished: return "DRAWING FINISHED (LOCKED)";
            case BitmapState::Encoding:return "ENCODING (LOCKED)";
            case BitmapState::EncodingFinished: return "ENCODING FINISHED (LOCKED)";
            case BitmapState::FinishedBitmap: return "BITMAP FINISHED (LOCKED)";
            case BitmapState::UnlockedRAM: return "UNLOCKED_RAM";
            case BitmapState::Unlocked: return "UNLOCKED";
            default: return "ERROR! (SHOULDN'T HAPPEN)";
            }
        }
        VOIDTYPE MakeDebugString();
//private:
    //VOIDTYPE TestEncoder(array<Bitmap^>^ bitmap);
#pragma endregion

#pragma region Interface_Settings
    public:
        bool SelectFractal(const uint16_t select);
        VOIDTYPE SetupFractal();
        VOIDTYPE SetMaxIterations();
        VOIDTYPE SetupAngle();
        VOIDTYPE SetupColor();
        inline VOIDTYPE SetupCutFunction() {
            cutFunction = f->cutFunction.size() <= 0 || f->cutFunction[selectCut].first < 0 ? nullptr : &Fractal::cutFunctions[f->cutFunction[selectCut].first].second;
        }
        inline VOIDTYPE SelectThreadingDepth() {
            //preIterate = new std::tuple<float, float, std::pair<float, float>*>*[MAX(static_cast<int16_t>(1), selectMaxTasks)];
            //SetMaxIterations(true);
            maxDepth = 0;
            if (f->childCount <= 0)
                return;
            for (auto n = 1, threadCount = 0; (threadCount += n) < selectMaxTasks - 1; n *= f->childCount)
                ++maxDepth;
        }
#pragma endregion

#pragma region Interface_Getters
    public:
        inline STRING GetTempGif() { return gifTempPath; }
        inline uint32_t GetMaxCutparam() {
            if (selectCut >= 0) {
                auto& c = GetFractal()->cutFunction[selectCut].second;
                if (c.size() > 0)
                    return static_cast<int32_t>(c[0] < 0 ? -c[0] : c.size() - 1);
            }
            return cutparamMaximum;
        }
        inline std::vector<Fractal*>& GetFractals() { return *fractals; }
        inline Fractal* GetFractal() { return (*fractals)[selectFractal]; }
        // TODO CLR VS CPP
        //inline Bitmap^ GetBitmap(const int index) { 
        //    return bitmap == nullptr || frames <= index ? nullptr : bitmap[index + previewFrames];
        //}
        // TODO CLR VS CPP
        //inline Bitmap^ GetPreviewBitmap() { return bitmapsFinished < 1 ? nullptr : bitmap[bitmapsFinished - 1]; }
        inline int GetFrames() { return frames - previewFrames; }
        inline int GetPreviewFrames() { return previewFrames; }
        inline int GetTotalFrames() { return frames; }
        inline int GetBitmapsFinished() { return bitmapsFinished - previewFrames; }
        inline int GetTotalFinished() { return bitmapsFinished; }
        inline ParallelType GetApplyParallelType() { return applyParallelType; }
        inline BitmapState& GetBitmapState(int index) { return bitmapState[index]; }
        inline int32_t IsGifReady() { return gifSuccess; }
        inline Fractal::CutFunction* GetCutFunction() { 
            auto ff = GetFractal();
            if (ff == nullptr)
                return nullptr;
            if (ff->cutFunction.size() <= 0)
                return nullptr;
            auto& c = ff->cutFunction[selectCut];
            return c.first < 0 ? nullptr : &Fractal::cutFunctions[c.first].second;
        }
#pragma endregion

    };

}