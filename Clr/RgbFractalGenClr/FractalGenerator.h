// Allow debug code, comment this when releasing for slightly better performance
#define CUSTOMDEBUG


#pragma once
#pragma region Includes
#include <vector>
#include <mutex>
#include <random>
#include <queue>
#include <array>
#include <map>
#include <chrono>
#include "Vector.h"//#include <functional> // Required for std::function
#include "Fractal.h"
#pragma endregion

#pragma region Defines

// COMPILE
#define CLR // Will use CLR ref class instead of pure C++ one

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

// UNIVERSAL
#define PALETTE std::pair<STRING, Vector*>

#pragma endregion

namespace RgbFractalGenCpp {

#ifdef CLR
    using namespace System;
    using namespace System::Collections::Generic;
    using namespace System::Collections::Concurrent;
    using namespace System::Threading;
    using namespace System::Threading::Tasks;
    //using namespace System::Numerics;
#endif

#pragma region Enums
    enum BitmapState : uint8_t {
        /*
        Queued = 0,				// Hasn't even started generating the dots yet (and it might stay this way if OnlyImage)
        Dots = 1,				// Started Generating Dots
        Void = 2,				// Started Dijksring the Void
        Drawing = 3,			// Started drawing
        DrawingFinished = 4,	// Finished drawing
        UnlockedRAM = 5,		// Unlocked bitmap without encoding
        Encoding = 6,			// Started Encoding
        EncodingFinished = 7,	// Finished encoding
        FinishedBitmap = 8,		// Finished bitmap (finished drawing if not encodeGIF, or finished encoding if encodeGIF)
        Unlocked = 9,			// Unlocked bitmap
        Error = 10				// Unused, unless error obviously
        */
        Queued = 0,             // Hasn't even started generating the dots yet (and it might stay this way if OnlyImage)
        Dots = 1,               // Started Generating Dots
        Void = 2,               // Started Dijkstra of the Void
        Drawing = 3,            // Started drawing
        DrawingFinished = 4,    // Finished drawing
        Unlocked = 5,           // Unlocked bitmap without encoding
        Error = 6               // Unused, unless error obviously
    };
    private enum TaskState : uint8_t {
        Free = 0,		// The task has not been started yet, or already finished and joined and ready to be started again
        Done = 1,	    // The task if finished and ready to join without waiting
        Running = 2     // The task is running
    };
    enum ParallelType : uint8_t {
        OfAnimation = 0,// Parallel batching of each animation image into its own single thread. No pixel conflicts, and more likely to use all the threads consistently. Recommended for animations.
        OfDepth = 1     // Parallelism of each single image, generates the single next image faster, but there might be rare conflicts of accessing pixels, and it's a bit slower overall if generating an animation. Used automatically for OnlyImage.
    };
    enum GenerationType : uint8_t {
        OnlyImage = 0,  // Will only render one still image. Cannot export PNG animation, or GIF animation, only a single image, the first one from a zoom animation.
        Animation = 1,  // Will render the animation as set up by your settings.
        AllSeeds = 2,   // Will not zoom/spin/shift at all, but instead cycle through all the available CutFunction seeds.
        HashSeeds = 3   // Same as All Seeds, but will also export a file with all detected unique seeds. (Only really useful for the developer, to hide the repeating seeds)
    };
    enum PngType : uint8_t {
        No = 0,			// Will not encode PNGs durign the generation, but will encode them just as fast if not faster when you do an export
        Yes = 1			// Will export animation as PNG series, that will enable you to export the PNGS or MP4 quicker after it's finished, but not really quicker overall.
    };
    enum GifType : uint8_t {
        No = 0,			// Will not encode a gif, you will not be able to export a gif afterwards, but you can switch to a Local/Global later and it will not fully reset the generator, just catches up with missing frames.
        Local = 1,		// Will encode a GIF during the generation, so you could save it when finished. With a local colormap (one for each frame). Higher color precision. Slow encoding, larger GIF file size. 
        Global = 2		// Also encodes a GIF. But with a global color map learned from a first frame. Not recommended for changing colors. Much faster encoding, smaller GIF file size
    };
    enum ExportType : uint8_t {
        None = 0,		// Nothing
        PngsToMp4 = 1,  // Convert PNG series into MP4
        GifToMp4 = 2,   // Convert GIF into MP4
        Png = 3,		// Export preview image as PNG
        Pngs = 4,		// Export PNG series (lossless animation quality)
        Gif = 5,		// Export GIF
    };
#pragma endregion

#pragma region Structs
    public struct FractalTask {
#ifndef CLR
    private:
        std::thread task;               // Parallel Animation Tasks
#endif
    public:
        TaskState State;                // States of Animation Tasks
        Vector** Buffer;                // Buffer for points to print into bmp
        int16_t** VoidDepth;            // Depths of Void
        Vector** VoidNoise;	            // Randomized noise samples for the void noise
        std::queue<std::pair<int16_t, int16_t>> 
            VoidQueue;                  // Void depth calculating Dijkstra queue
        std::map<int64_t, std::array<Vector, 3>>
            ColorBlends;                // ???
        std::map<int64_t, std::array<Vector, 3>>
            FinalColors;		        // Mixed children color
        int32_t BitmapIndex;            // The bitmapIndex of which the task is working on
        uint16_t TaskIndex;             // The task index
        int16_t ApplyWidth, ApplyHeight,// can be smaller for previews
            WidthBorder, HeightBorder;  // slightly below the width and height
        double Bloom0, Bloom1,          // = selectBloom (0,+1);
            RightEnd, DownEnd,          // slightly beyond width and depth, to ensure even bloomed pixels don't cutoff too early
            ApplyDetail,                // allocated detail
            UpLeftStart;                // = -selectBloom;
        float LightNormalizer,	        // maximum brightness found in the buffer, for normalizing the final image brightness
            VoidDepthMax;	            // maximum reached void depth during the dijkstra search, for normalizing the void intensity
        std::tuple<float, float, std::pair<float, float>*>*
            PreIterate;

        //Vector I[3];                  // Pure parent color
        //uint8_t huemod;
        bool taskStarted;		        // Additional safety, could remove if it never gets triggered for a while
        
        FractalTask() {
            taskStarted = false;
            State = TaskState::Free;
            Buffer = nullptr;
            VoidDepth = nullptr;
        }
    };
#pragma endregion

#ifdef CLR
    public ref class FractalGenerator {
#else
    public class FractalGenerator {
#endif

#pragma region Constants
#define MINTASKS 1
#define MAXPNGFAILS 200
#define DEPTHDIV 6               // multiples of maxThreads to queue for OfDepth parallelism
#pragma endregion

#pragma region Variables_Unsorted
    public:
        //int16_t applyColorPalette;  // RGB or BGR? (0/1)
        //int16_t applyHueCycle;      // Hue Cycling Direction (-1,0,1)
       //double selectPeriodAngle;     // Angle symmetry of current fractal

        uint16_t* strides;
        bool
            restartGif = false; // Makes me restart the gif encoder (called when delay is changed, which should restart the encoder, but not toss the finished bitmaps)
        bool debugmode = false;
       
        
        //double* emptyDouble;      // Preallocated empty angle array when it doesn't exist
        uint16_t maxChildren;   // The maximum number of children any fractal definition has (for presizing the copy arrays)
        uint16_t debug;				    // Debug frame count override
        double bloom1;       // Precomputed bloom + 1
       
        bool gifThread = false;
        STRING gifTempPath;         // Temporary GIF file name

#pragma endregion

#pragma region Variables_Definitions
    public:
        PALETTE** Colors = new PALETTE * [20] {
            new PALETTE("RGB", new Vector[3]{ Vector(255, 0, 0), Vector(0, 255, 0), Vector(0, 0, 255) }),
                new PALETTE("BGR", new Vector[3]{Vector(0, 0, 255), Vector(0, 255, 0), Vector(255, 0, 0)}),
                new PALETTE("->", new Vector[6]{Vector(255, 0, 0), Vector(255, 255, 0), Vector(0, 255, 0), Vector(0, 255, 255), Vector(0, 0, 255), Vector(255, 0, 255)}),
                new PALETTE("<-", new Vector[6]{Vector(255, 0, 255), Vector(0, 0, 255), Vector(0, 255, 255), Vector(0, 255, 0), Vector(255, 255, 0), Vector(255, 0, 0)}),
                new PALETTE("Colorblind ", new Vector[4]{Vector(255, 0, 0), Vector(0, 0, 255), Vector(0, 255, 0), Vector(0, 0, 255)}),
                new PALETTE("ColorblindLike", new Vector[3]{Vector(255, 255, 0), Vector(0, 0, 255)}),
                new PALETTE("WhiteTransp", new Vector[3]{Vector(255, 255, 255), Vector(0, 0, 0)}),
                new PALETTE("WhiteBlack", new Vector[3]{Vector(255, 255, 255), Vector(1, 1, 1)}),
                new PALETTE("RedTransp", new Vector[3]{Vector(255, 0, 0), Vector(0, 0, 0)}),
                new PALETTE("GreenTransp", new Vector[3]{Vector(0, 255, 0), Vector(0, 0, 0)}),
                new PALETTE("BlueTransp", new Vector[3]{Vector(0, 0, 255), Vector(0, 0, 0)}),
                new PALETTE("RedBlack", new Vector[3]{Vector(255, 0, 0), Vector(1, 0, 0)}),
                new PALETTE("GreenBlack", new Vector[3]{Vector(0, 255, 0), Vector(0, 1, 0)}),
                new PALETTE("BlueBlack", new Vector[3]{Vector(0, 0, 255), Vector(0, 0, 1)}),
                new PALETTE("GreyUp", new Vector[3]{Vector(1, 1, 1), Vector(128, 128, 128), Vector(255, 255, 255)}),
                new PALETTE("GreyDown", new Vector[3]{Vector(255, 255, 255), Vector(128, 128, 128), Vector(1, 1, 1)}),
                new PALETTE("WhiteWave", new Vector[4]{Vector(1, 1, 1), Vector(128, 128, 128), Vector(255, 255, 255), Vector(128, 128, 128)}),
                new PALETTE("RedWave", new Vector[4]{Vector(1, 0, 0), Vector(128, 0, 0), Vector(255, 0, 0), Vector(128, 0, 0)}),
                new PALETTE("GreenWave", new Vector[4]{Vector(0, 1, 0), Vector(0, 128, 0), Vector(0, 255, 0), Vector(0, 128, 0)}),
                new PALETTE("BlueWave", new Vector[4]{Vector(0, 0, 1), Vector(0, 0, 128), Vector(0, 0, 255), Vector(0, 0, 128)})
            };
        double* ChildAngle;		        // A copy of selected childAngle combinations
        uint8_t* ChildColor;            // A copy of childColor for allowing BGR and combinations
        std::map<std::string, uint32_t>*
            Hash;                       // Seed hashes

    private:
        std::vector<Fractal*>*
            fractals;                   // Fractal definitions
        Fractal* f;                     // Selected fractal definition
        std::map<int64_t, std::array<Vector, 3>>*
            colorBlends;                // What mix of colors will a parent split into?
#pragma endregion

#pragma region Variables_Selected
    public:
        double
            SelectedDefaultHue,         // Default hue angle (-1 - Colors.Length-1)
            SelectedNoise,			    // Void noise strength (0-3)
            SelectedDetail,             // MinSize multiplier (1-10)
            SelectedSaturate,		    // Saturation boost level (0-1)
            SelectedBloom;              // Dot bloom level (pixels)
        uint64_t
            SelectedChildAngles,	    // Child angle bitmask
            SelectedChildColors;        // Child color bitmask
        int32_t
            SelectedCutSeed,            // Cutparam seed (0-CutSeed_Max)
            CutSeed_Max;				// Maximum seed for the selected CutFunction
        int16_t
            SelectedFractal,            // Fractal definition (0-fractals.Length)
            SelectedChildAngle,         // Child angle definition (0-childAngle.Length)
            SelectedChildColor,         // Child color definition (0-childColor.Length)
            SelectedCut,                // Selected CutFunction index (0-cutFunction.Length)
            SelectedPaletteType,        // Color Palette (-1 - Colors.Length-1)
            SelectedZoom,               // Zoom direction (-1 out, 0 random, 1 in)
            SelectedHue,                // -1 = random, 0 = RGB, 1 = BGR, 2 = RGB->GBR, 3 = BGR->RBG, 4 = RGB->BRG, 5 = BGR->GRB
            SelectedSpin,               // Spin direction (-2 Random, -1 AntiClockwise, 0 None, 1 Clockwise, 2 AntiSpin, 3 PeriAntiSpin)
            SelectedAmbient,            // Void ambient strength (-1 - MaxAmbient)
            SelectedMaxTasks;           // Maximum allowed total tasks
        uint16_t
            SelectedWidth,              // Resolution width (1-X)
            SelectedHeight,             // Resolution height (1-X)
            SelectedPeriod,             // Parent to child frames period (1-X)
            SelectedPeriodMultiplier,   // Multiplier of frames period (1-X)
            SelectedDefaultZoom,        // Default skipped frames of zoom (0-frames)
            SelectedExtraHue,           // Extra hue angle speed (0-X)
            SelectedDefaultAngle,       // Default spin angle (0-360)
            SelectedExtraSpin,          // Extra spin speed (0-X)
            SelectedVoid,			    // Void Noise Size
            SelectedBrightness,         // Light normalizer brightness (0-MaxBrightness)
            SelectedBlur,               // Dot blur level
            MaxZoomChild,              // Maximum allowed zoom child
            SelectedDelay = 1,			// Animation frame delay
            SelectedFps = 1;
        ParallelType
            SelectedParallelType;       // 0 = Animation, 1 = Depth
        GenerationType
            SelectedGenerationType;     // 0 = Only Image, 1 = Animation, 2 = Animation + GIF
        PngType
            SelectedPngType;            // 0 = No, 1 = Yes
        GifType
            SelectedGifType;            // 0 = No, 1 = Local, 2 = Global
        bool
            SelectedPreviewMode = false,// Preview mode only renders a single smaller fractal with only one color shift at the highest level - for definition editor
            SelectedEditorMode = false; // In editor mode, we will only show the selected childColors and childAngles

    private:
        Fractal::CutFunction*
            selectedCutFunction;        // Selected CutFunction pointer
        uint16_t 
            selectedZoomChild;          // Which child to zoom into
        int16_t 
            maxIterations = -1;        // maximum depth of iteration (dependent on fractal/detail/resolution)
#pragma endregion

#pragma region Variables_Allocated
    private:
        Vector***
            buffer;                 // Buffer for points to print into bmp - separated for OfDepth
        FractalTask* 
            tasks;                  // All available tasks
        uint8_t** 
            bitmap;                 // Prerender as an array of bitmaps
        BitmapState* 
            bitmapState;            // What stage is the bitmap in? Created? Drawing? Exporting?
        // bitmapdata
        // lockedbmp
        Vector*
            allocPalette;
        std::mt19937* 
            random;
        std::uniform_real_distribution<double>*
            nextDouble;             // C++ only (in C# it's NextDouble())
        double
            logBase,                // Size Log Base
            applyPeriodAngle,       // Angle symmetry corrected for periodMultiplier
            allocDetail,
            allocNoise;             // Normalizer for maximum void depth - Precomputed amb * noise
        int64_t 
            allocCutSeed,	        // Allocated cutparam (selected or random)
            startCutParam = 0;      // What cut seed will the generator start with?
        int32_t 
            allocFrames,            // How many bitmap frames are currently allocated?
            nextBitmap;			    // How many bitmaps have started generating? (next task should begin with this one)
        uint32_t
            allocPalette2,
            bitmapsFinished;		// How many bitmaps are completely finished generating? (ready to display, encoded if possible)
        int16_t 
            allocWidth,	            // How much buffer width is currently allocated?
            allocHeight,            // How much buffer height is currently allocated?
            allocZoom,		        // Allocated zoom (selected or random)
            allocHue,               // Allocated hue setting
            allocTasks,	            // How many buffer tasks are currently allocated?
            allocMaxIterations,
            allocMaxTasks,	        // Safely copy maxTasks in here so it doesn't change in the middle of generation
            isWritingBitmaps = 2,   // counter and lock to try writing bitmap toa file once every 8 threads
            isFinishingBitmaps = 2; // counter and lock to try finishing bitmaps once every 2 threads
        uint16_t
            allocPeriodMultiplier,  // How much will the period get finally stretched? (calculated for seamless + user multiplier)
            allocExtraSpin,         // Allocated extra spin setting
            allocVoid,              // Allocated void scale setting
            allocBlur,              // Allocated blur level (1 + selectBlur, because 0 would not render any image)
            allocDelay,
            debugPeriodOverride,    // Debug frame count override
            bitmapLength,           // C++ only (in C# it's just bitmap.Length)
            bufferLength0,          // C++ only (in C# it's buffer.Length(0))
            bufferLength1,          // C++ only (in C# it's buffer.Length(1))
            bufferLength2;          // C++ only (in C# it's buffer.Length(2))
        uint8_t 
            previewFrames,          // how many preview frames are we making for this resolution?
            hueCycleMultiplier;	    // How fast should the hue shift to loop seamlessly?
        bool
            allocPreviewMode;       // Will render a simpliefied fractal colored 1 layer deep definition showing it's structure
        GenerationType
            allocGenerationType;    // Allocated generation type (for example a gif error can temporarily change it to AnimationRAM)
        ParallelType
            allocParallelType;      // Safely copy parallelType in here so it doesn't change in the middle of generation
        PngType
            allocPngType;
        GifType
            allocGifType;
        std::tuple<uint16_t, TUPLEPACK, uint8_t, int64_t, uint8_t>*
            tuples;                     // Queue struct for GenerateDots_OfDepth
#pragma endregion

#pragma region Variables_Export
    private:
        void* gifEncoder;                   // Export GIF encoder
        //private MemoryStream[]
        //    msPngs;						// MemoryStreams for Png Exporting
        int16_t 
            gifSuccess,                 // Temp GIF file "gif.tmp" successfuly created
            tryPng;	                    // how many sequential Pngs have been exported?
        //internal string
        //    GifTempPath;                // Temporary GIF file name
        //private byte[]
        //    encodedGif,             // 0 - not encoded, 1 - encoding, 2 - encoded, 3 - written
        //    encodedPng;             // 0 - not encoded, 1 - encoding, 2 - encoded
        //private int  
        //    tryGif;						// how many sequential gif frames have been exported
        //private ushort
        //    encodedMp4,					// report of how many mp4 frames have been encoded
        //    pngFailed;					// how many attempts to write a png have failed?
        //private Rectangle
        //    rect;						// Bitmap rectangle TODO implement
        STRING
            filePrefix;                 // Specific file identifier
        //private ExportType
        //    exportType = ExportType.None;
#pragma endregion

#pragma region Variables_Debug
    public:
        bool 
            DebugTasks = false,         // export task states to realtime debug string
            DebugAnim = false,          // export bitmap states to realtime debug string
            DebugPng = false,           // export png states to realtime debug string
            DebugGif = false;           // export gif states to realtime debug string
        STRING DebugString;

    private:
        System::String^ logString;
        int16_t* 
            counter = new int16_t[11];  // counter of bitmap states
        int16_t* 
            counterE = new int16_t[11]; // counter of encode states

#ifdef CUSTOMDEBUG
        // Debug variables
        STRING logString;       // Debug Log
        std::chrono::steady_clock::time_point*
            startTime;          // Start Stopwatch
        //uint64_t initTimes, iterTimes, voidTimes, drawTimes, gifsTimes;
#endif

#pragma endregion
        
#pragma region Variables_Calls

#ifdef CLR
    public:
        Action^ UpdatePreview;
        Action<const uint16_t, const uint16_t, const uint16_t>^ NewBitmap;
        Action^ UnlockBitmaps;
        Action^ ReencodeBitmaps;
        Action^ SetRect;
        Action^ AllocateBitmaps;
        Action<const uint16_t>^ ReLockBits;
        Action<const uint16_t>^ UnlockBits;

    private:
        Object^
            taskLock = gcnew Object();  // Monitor lock
        CancellationTokenSource^
            cancel;                     // Cancellation Token Source
        CancellationToken 
            token;                      // Cancellation token
        CancellationTokenSource^
            gifCancel;                  // GIF Cancellation Token Source
        CancellationToken 
            gifToken;                   // GIF Cancellation token
        Task^ 
            mainTask;                   // Main Generation Task

        array<Task^>^ parallelTasks;    // Parallel Animation Tasks
        System::Drawing::Rectangle
            rect;                       // Bitmap rectangle TODO implement

        typedef Vector(*RowFunc)(const FractalTask&, const uint16_t, uint8_t*&);
        RowFunc rowFunc;
        typedef Vector(*SaturateFunc)(const Vector&, const double);
        SaturateFunc saturateFunc;
        typedef VOIDTYPE(*DitherFunc)(const Vector&, uint8_t*&);
        DitherFunc ditherFunc;
#else
    public:
        std::function<void(const uint16_t bitmapIndex, const uint16_t w, const uint16_t h)> NewBitmap;
        std::function<void()> UnlockBitmaps;
        std::function<void()> ReencodeBitmaps;
        std::function<void()> SetRect;
        std::function<void()> AllocateBitmaps;
        std::function<void(const uint16_t bitmapIndex)> ReLockBits;

    private:
        std::mutex 
            taskLock;                   // Monitor lock
        std::atomic<bool>
            cancelRequested;            // Cancellation Token Source
        std::atomic<bool>
            gifCancelRequested;         // GIF Cancellation Token Source
        std::thread 
            mainTask;                   // Main Generation Task

        using RowProcessor = const Vector(*)(const FractalTask&, const uint16_t, uint8_t*&);
        RowProcessor rowFunc = nullptr;
        using SaturateFunc = const Vector(*)(const Vector&, const double);
        SaturateFunc saturateFunc = nullptr;
        using Dither = void(*)(const Vector&, uint8_t*&);
        Dither ditherFunc = nullptr;
#endif

#pragma endregion

#pragma region FractalTask
        bool IsTaskStillRunning(FractalTask& task);
        inline bool Join(FractalTask& task);
        inline FractalTask& Start(const uint16_t taskIndex, int16_t bitmap);
#pragma endregion

#pragma region Delegates
    private:
        // Row:
        VOIDTYPE Noise(const FractalTask& drawTask, const uint16_t y, uint8_t*& p);
        VOIDTYPE NoNoise(const FractalTask& drawTask, const uint16_t y, uint8_t*& p);
        VOIDTYPE NoAmbient(const FractalTask& drawTask, const uint16_t y, uint8_t*& p);
        // Saturate:
        const static Vector Identity(const Vector& v, const double) { return v; }
        const static Vector ApplySaturate(const Vector& rgb, const double selectedSaturate);
        // Dither:
        VOIDTYPE Dithering(const Vector& rgb, uint8_t*& p);
        VOIDTYPE NoDithering(const Vector& rgb, uint8_t*& p);
#pragma endregion

#pragma region Unsorted
        inline uint32_t GetGenerateLength();
#pragma endregion

#pragma region Init
    public:
        FractalGenerator();
    private:
        static VOIDTYPE MakeTemp();
#pragma endregion

#pragma region Tasks
    private:
        static std::chrono::steady_clock::time_point* BeginTask();
        VOIDTYPE FinishTask(FractalTask& task, std::chrono::steady_clock::time_point* stop, STRING log);
        bool FinishTasks(bool mainLoop, uint8_t& i, bool& tasksRemaining, uint16_t& taskIndex);
#pragma endregion


#pragma region InitData
    private:
        VOIDTYPE DeleteEncoder();
        VOIDTYPE DeleteBuffer(const FractalTask& task, const uint16_t vh);
        VOIDTYPE NewBuffer(FractalTask& task, const uint16_t vw, const uint16_t vh);
        VOIDTYPE NewOfDepthBuffer();
#pragma endregion



#pragma region Generate_Tasks
    private:
        
        // TODO CPP
        VOIDTYPE PreviewResolution(FractalTask& task) {
            if (task.BitmapIndex < previewFrames) {
                int div = 1 << (allocParallelType == ParallelType::OfDepth ? previewFrames - task.BitmapIndex : previewFrames - task.BitmapIndex - 1);
                task.ApplyWidth = (short)(SelectedWidth / div);
                task.ApplyHeight = (short)(SelectedHeight / div);
                task.Bloom0 = SelectedBloom / div;
            } else {
                task.Bloom0 = SelectedBloom;
                task.ApplyWidth = SelectedWidth;
                task.ApplyHeight = SelectedHeight;
            }
            task.WidthBorder = (short)(task.ApplyWidth - 2);
            task.HeightBorder = (short)(task.ApplyHeight - 2);
            task.Bloom1 = task.Bloom0 + 1;
            task.UpLeftStart = -task.Bloom1;
            task.RightEnd = task.WidthBorder + task.Bloom1;
            task.DownEnd = task.HeightBorder + task.Bloom1;
        }
        // TODO CPP
        VOIDTYPE PregenerateParam(const int32_t bitmapIndex, std::map<int64_t, std::array<Vector, 3>>* blends, int64_t& startParam) {
            std::vector<int32_t>& m = f->cutFunction[SelectedCut].second;
            switch(allocGenerationType) {
            case GenerationType::AllSeeds: startParam = (m = f->cutFunction[SelectedCut].second).size() > 0 && m[0] >= 0
                    ? -m[bitmapIndex] : -bitmapIndex; 
                break;
            case GenerationType::HashSeeds : startParam = -bitmapIndex;
                    break;
            default: 
                startParam = (m = f->cutFunction[SelectedCut].second).size() > 0 && m[0] >= 0 ? -m[allocCutSeed] : -allocCutSeed;
                break;
            };
            if (allocPreviewMode)
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
                        ++v[ChildColor[i]];
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

        bool TryWriteBitmaps(FractalTask& task);
        VOIDTYPE TryWriteBitmap(const uint16_t taskIndex);
        bool TryFinishBitmap(FractalTask& task);
        VOIDTYPE TryFinishBitmaps(bool gif);
#pragma endregion

#pragma region Generate_Inline
    private:
        VOIDTYPE InitPreiterate(std::tuple<float, float, std::pair<float, float>*>*& preIterateTask) {
            preIterateTask = new std::tuple<float, float, std::pair<float, float>*>[allocMaxIterations];
            for (int i = 0; i < allocMaxIterations; preIterateTask[i++] = { 0.0f, 0.0f, nullptr });
        }
        VOIDTYPE ApplyDot(const FractalTask& task, const int64_t key, const float& inX, const float& inY, const float& inDetail, const uint8_t& inColor);
        inline int64_t CalculateFlags(const int& index, const int64_t& inFlags) { return selectedCutFunction == nullptr ? inFlags : (*selectedCutFunction)(index, inFlags, *f); }
        inline bool TestSize(const FractalTask& task, const float& newX, const float& newY, const float& inSize);
        static const Vector Normalize(const Vector& pixel, const float lightNormalizer);
        static const Vector ApplyAmbientNoise(const Vector& rgb, const float Amb, const float Noise, const Vector& rand);
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
        // Pack two floats into a double
        /*static double pack(float f1, float f2) {
            uint64_t combined_bits = (static_cast<uint64_t>(*reinterpret_cast<uint32_t*>(&f1)) << 32) | static_cast<uint64_t>(*reinterpret_cast<uint32_t*>(&f2));   // Combine into a 64-bit value
            return *reinterpret_cast<double*>(&combined_bits);    // Reinterpret as a double
        }*/
        // Unpack a double into two floats
        /*static VOIDTYPE unpack(double packed, float& f1, float& f2) {
            uint64_t combined_bits = *reinterpret_cast<uint64_t*>(&packed); // Get bits of double
            uint32_t f1_bits = combined_bits >> 32;         // Extract upper 32 bits
            uint32_t f2_bits = combined_bits & 0xFFFFFFFF;  // Extract lower 32 bits
            f1 = *reinterpret_cast<float*>(&f1_bits);       // Reinterpret as float
            f2 = *reinterpret_cast<float*>(&f2_bits);       // Reinterpret as float
        }*/
#pragma endregion

#pragma region AnimationParameters
    private:
        VOIDTYPE SwitchParentChild(float& angle, int8_t& spin, uint8_t& color);
        VOIDTYPE ModFrameParameters(const uint16_t width, const uint16_t height, float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color);
        VOIDTYPE IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const uint16_t blur);
        inline VOIDTYPE IncFrameSize(float& size, const uint16_t period) {
            size *= powf(f->childSize, SelectedZoom * 1.0f / period);
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
            selectedCutFunction = f->cutFunction.size() <= 0 || f->cutFunction[SelectedCut].first < 0 ? nullptr : &Fractal::cutFunctions[f->cutFunction[SelectedCut].first].second;
        }
        inline VOIDTYPE SelectThreadingDepth() {
            //preIterate = new std::tuple<float, float, std::pair<float, float>*>*[MAX(static_cast<int16_t>(1), selectMaxTasks)];
            //SetMaxIterations(true);
            maxDepth = 0;
            if (f->childCount <= 0)
                return;
            for (auto n = 1, threadCount = 0; (threadCount += n) < SelectedMaxTasks - 1; n *= f->childCount)
                ++maxDepth;
        }
#pragma endregion

#pragma region Interface_Getters
    public:
        inline STRING GetTempGif() { return gifTempPath; }
        inline uint32_t GetMaxCutparam() {
            if (SelectedCut >= 0) {
                auto& c = GetFractal()->cutFunction[SelectedCut].second;
                if (c.size() > 0)
                    return static_cast<int32_t>(c[0] < 0 ? -c[0] : c.size() - 1);
            }
            return CutSeed_Max;
        }
        inline std::vector<Fractal*>& GetFractals() { return *fractals; }
        inline Fractal* GetFractal() { return (*fractals)[SelectedFractal]; }
        // TODO CLR VS CPP
        //inline Bitmap^ GetBitmap(const int index) { 
        //    return bitmap == nullptr || frames <= index ? nullptr : bitmap[index + previewFrames];
        //}
        // TODO CLR VS CPP
        //inline Bitmap^ GetPreviewBitmap() { return bitmapsFinished < 1 ? nullptr : bitmap[bitmapsFinished - 1]; }
        inline int GetFrames() { return bitmapLength - previewFrames; }
        inline int GetPreviewFrames() { return previewFrames; }
        inline int GetTotalFrames() { return bitmapLength; }
        inline int GetBitmapsFinished() { return bitmapsFinished - previewFrames; }
        inline int GetTotalFinished() { return bitmapsFinished; }
        inline ParallelType GetApplyParallelType() { return allocParallelType; }
        inline BitmapState& GetBitmapState(int index) { return bitmapState[index]; }
        inline int32_t IsGifReady() { return gifSuccess; }
        inline Fractal::CutFunction* GetCutFunction() { 
            auto ff = GetFractal();
            if (ff == nullptr)
                return nullptr;
            if (ff->cutFunction.size() <= 0)
                return nullptr;
            auto& c = ff->cutFunction[SelectedCut];
            return c.first < 0 ? nullptr : &Fractal::cutFunctions[c.first].second;
        }
#pragma endregion

    };

}