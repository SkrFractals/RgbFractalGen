// Allow debug code, comment this when releasing for slightly better performance
//#define CUSTOMDEBUG

#pragma once
#include <vector>
#include <mutex>
#include <random>
#include <queue>
#include "Vector.h"
#include "Fractal.h"

#ifdef CUSTOMDEBUG
#include <chrono>
#endif

namespace RgbFractalGenClr {

    using namespace System;
    using namespace System::Drawing;
    using namespace System::Collections::Generic;
    using namespace System::Collections::Concurrent;
    using namespace System::Threading;
    using namespace System::Threading::Tasks;
    //using namespace System::Numerics;

    public struct FractalTask {
        uint8_t state;      // States of Animation Tasks
        Vector** buffer;    // Buffer for points to print into bmp
        int16_t** voidDepth;// Depths of Void
        std::queue<std::pair<int16_t, int16_t>> voidQueue;
        Vector H;           // mixed children color
        Vector I;           // pure parent color
        bool taskStarted;   // Additional safety, could remove if it never gets triggered for a while
        uint16_t index;
        std::tuple<float, float, std::pair<float, float>*>*
            preIterate;
        FractalTask() {
            taskStarted = false;
            state = 2;
            buffer = nullptr;
            voidDepth = nullptr;
        }
    };

    public ref class FractalGenerator {

    private:

#ifdef CUSTOMDEBUG
        // Debug variables
        System::String^ logString;                  // Debug Log
        std::chrono::steady_clock::time_point* startTime; // Start Stopwatch
        long long initTimes, iterTimes, voidTimes, drawTimes, gifTimes;
#endif
    private:
        // Definitions
        Fractal** fractals;     // Fractal definitions
        Fractal* f;             // Selected fractal definition
        uint16_t maxChildren;
        float* childAngle;		// A copy of selected childAngle
        float periodAngle;      // Angle symmetry of current fractal
        uint8_t* childColor;	// A copy of childCoolor for allowing BGR
        Vector* colorBlend;     // Color blend of a selected fractal for more seamless loop (between single color to the sum of children)
        Fractal::CutFunction* 
            cutFunction;        // Selected CutFunction pointer
        int16_t applyCutparam;	// Applied cutparam (selected or random)
        int16_t maxIterations;  // maximum depth of iteration (dependent on fractal/detail/resolution)
        //std::tuple<float, float, std::pair<float, float>*>** 
        //    preIterate;         // (childSize, childDetail, childSpread, (childX,childY)[])
        float logBase;          // Size Log Base
        float* emptyFloat;      // Preallocated empty angle array when it doesn't exist

        // Resolution
        int16_t allocatedWidth;	// How much buffer width is currently allocated?
        int16_t allocatedHeight;// How much buffer height is currently allocated?
        int16_t widthBorder;	// Precomputed maximum x coord fot pixel input
        int16_t heightBorder;	// Precomputed maximum y coord for pixel input
        float upleftStart;		// Precomputed minimum x/y for dot iteration/application
        float rightEnd;			// Precomputed maximum -x- for dot iteration/application
        float downEnd;			// Precomputed maximum -y- for dot iteration/application

        // Frames
        //Vector*** buffer;               // Buffer for points to print into bmp
        array<Bitmap^>^ bitmap;		    // Prerender as an array of bitmaps
        uint8_t* bitmapState;           // Flag if the bitmap was finished generating (ready to encode GIF and UnlockBits)
        array<System::Drawing::Imaging::BitmapData^>^ 
            bitmapData;	                // Locked Bits for bitmaps
        uint16_t finalPeriodMultiplier; // How much will the period get finally stretched? (calculated for seamless + user multiplier)
        uint16_t debug;				    // Debug frame count override
        uint16_t bitmapsFinished;		// How many bitmaps are completely finished generating? (ready to display, encoded if possible)
        uint16_t nextBitmap;			// How many bitmaps have started generating? (next task should begin with this one)
        int16_t allocatedFrames;        // How many bitmap frames are currently allocated?
        int16_t applyZoom;		        // Applied zoom (selected or random)

        // Color
        int16_t applyColorPalette;  // RGB or BGR? (0/1)
        int16_t applyHueCycle;      // Hue Cycling Direction (-1,0,1)
        uint8_t hueCycleMultiplier;	// How fast should the hue shift to loop seamlessly?

        // Void
        //int16_t*** voidDepth;   // Depths of Void
        //std::queue<std::pair<int16_t, int16_t>>** 
        //    voidQueue;          // Queue for Void Dijkra
        float ambnoise;         // Normalizer for maximum void depth - Precomputed amb * noise
        float bloom1;           // Precomputed bloom+1
        Random random;          // Random generator

        // Threading
        FractalTask* tasks;
        uint8_t applyParallelType;			// Safely copy parallelType in here so it doesn't change in the middle of generation
        int16_t applyMaxTasks;				// Safely copy maxTasks in here so it doesn't change in the middle of generation
        int16_t applyMaxGenerationTasks;	// Safely copy maxGenerationTasks in here so it doesn't change in the middle of generation
        int16_t maxDepth;					// Maximum depth for Recusrion parallelism
        int16_t allocatedTasks;				// How many buffer tasks are currently allocated?
        Object^ taskLock = gcnew Object();  // Monitor lock
        CancellationTokenSource^ cancel;    // Cancellation Token Source
        Task^ mainTask;                     // Main Generation Task
        ConcurrentBag<Task^>^ imageTasks;   // Parallel Image Tasks
        array<Task^>^ parallelTasks;        // Parallel Animation Tasks
        //uint8_t* parallelTaskFinished;		// States of Animation Tasks
        List<Task^>^ taskSnapshot;          // Snapshot for safely checking imageTasks Wait
        std::tuple<uint16_t, double, double, uint8_t, int, uint8_t>* 
            tuples;                         // Queue struct for GenerateDots_OfDepth
        //bool* taskStarted;                  // Additional safety, could remove if it never gets triggered for a while

        // Export
        void* gifEncoder;               // Export GIF encoder
        bool gifSuccess, exportingGif;	// Temp GIF file "gif.tmp" successfuly created | flag only allowing one GIF Encoding thread
        System::String^ gifTempPath;    // Temporary GIF file name
        System::Drawing::Rectangle rect;// Bitmap rectangle TODO implement

    public:
        // Selected Settings
        int16_t selectFractal,          // Fractal definition (0-fractals.Length)
            selectChildAngle,           // Child angle definition (0-childAngle.Length)
            selectChildColor,           // Child color definition (0-childColor.Length)
            selectCut,                  // Selected CutFunction index (0-cutFunction.Length)
            selectCutparam;             // Cutparam seed (0-maxCutparam)
        uint16_t selectWidth,           // Resolution width (1-X)
            selectHeight;               // Resolution height (1-X)
        int16_t selectPeriod,           // Parent to child frames period (1-X)
            selectPeriodMultiplier,     // Multiplier of frames period (1-X)
            selectZoom,                 // Zoom direction (-1 out, 0 random, 1 in)
            selectDefaultZoom,          // Default skipped frames of zoom (0-frames)
            selectExtraSpin,            // Extra spin speed (0-X)
            selectDefaultAngle,         // Default spin angle (0-360)
            selectSpin,                 // Spin direction (-2 random, -1 anticlockwise, 0 none, 1 clockwise, 2 antispin)
            selectHue,                  // -1 = random, 0 = RGB, 1 = BGR, 2 = RGB->GBR, 3 = BGR->RBG, 4 =RGB->BRG, 5 = BGR->GRB
            selectExtraHue,             // Extra hue angle speed (0-X)
            selectDefaultHue,           // Default hue angle (0-360)
            selectAmbient;              // Void ambient strength (0-120)
        float selectNoise,		        // Void noise strength (0-3)
            selectSaturate,				// Saturation boost level (0-1)
            selectDetail,				// MinSize multiplier (1-10)
            selectBloom;                // Dot bloom level (pixels)
        int16_t selectBlur,             // Dot blur level
            selectBrightness;           // Light normalizer brightness (0-300)
        uint8_t selectParallelType;           // 0 = Animation, 1 = Depth, 2 = Recursion
        int16_t selectMaxTasks,         // Maximum allowed total tasks
            selectMaxGenerationTasks,   // Maximum allowed tasks for GenerateDot and GenerateBitmap
            selectDelay;				// Animation frame delay
        uint8_t selectEncode;           // 0 = Only Image, 1 = Animation, 2 = Animation + GIF
        int16_t cutparamMaximum;        // Maximum seed for the selected CutFunction

#pragma region Init
    public:
        FractalGenerator();
    private:
        System::Void DeleteEncoder();
        System::Void DeleteBuffer(const FractalTask& task);
        System::Void NewBuffer(FractalTask& task);
#pragma endregion

#pragma region Generate_Tasks
    private:
        System::Void GenerateAnimation();
        System::Void GenerateImage(const uint16_t& bitmapIndex, const int16_t& stateIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color);
        System::Void GenerateDots_SingleTask(const uint16_t taskIndex, double inXY, double AA, const uint8_t inColor, const int inFlags, uint8_t inDepth);
        System::Void GenerateDots_OfDepth();
        System::Void GenerateDots_OfRecursion(const uint16_t taskIndex, double inXY, double AA, const uint8_t inColor, const int inFlags, uint8_t inDepth);
        bool FinishTask(FractalTask& task);
        System::Void TryGif(FractalTask& task);
#pragma endregion

#pragma region Generate_Inline
    private:
        bool ApplyDot(const bool apply, const FractalTask& task, const float& inX, const float& inY, const float& inDetail, const uint8_t& inColor);
        inline int CalculateFlags(const int& index, const int& inFlags) { return cutFunction == nullptr ? inFlags : (*cutFunction)(index, inFlags); }
        inline bool TestSize(const float& newX, const float& newY, const float& inSize) {
            const auto testSize = inSize * f->cutSize;
            return ((Math::Min(newX, newY) + testSize > upleftStart) && (newX - testSize < rightEnd) && (newY - testSize < downEnd) );
        }
        static Vector Normalize(const Vector& pixel, const float lightNormalizer);
        static Vector& ApplyAmbientNoise(Vector& rgb, const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen);
        Vector ApplySaturate(const Vector& rgb);
        static System::Void ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p);
        inline System::Void NoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline System::Void NoNoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline System::Void NoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline System::Void NoNoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
#pragma endregion

#pragma region TaskWrappers
    private:
        System::Void Task_Animation(System::Object^ obj);
        System::Void Task_OfDepth(System::Object^ obj);
        System::Void Task_OfRecursion(System::Object^ obj);
        System::Void Task_Gif(System::Object^ obj);
        inline void Join(FractalTask& task) {
            if (task.taskStarted) {
                if (parallelTasks[task.index] != nullptr) {
#ifdef PARALLELDEBUG
                    Debug::WriteLine("join" + task.index);
#endif
                    parallelTasks[task.index]->Wait();
                    parallelTasks[task.index] = nullptr;
                } else {
#ifdef PARALLELDEBUG
                    Debug::WriteLine("ERROR not joinable " + task.index);
#endif
                }
            } else {
#ifdef PARALLELDEBUG
                Debug::WriteLine("ERROR join: task not running " + task.index);
#endif
            }
            task.taskStarted = false;
        }

        inline void Start(FractalTask& task) {
            if (task.taskStarted) {
#ifdef PARALLELDEBUG
                Debug::WriteLine("ERROR start: task already running " + task.index);
#endif

                if (parallelTasks[task.index] != nullptr) {
                    parallelTasks[task.index]->Wait();
                    parallelTasks[task.index] = nullptr;
                } else {
#ifdef PARALLELDEBUG
                    Debug::WriteLine("ERROR not joinable " + task.index);
#endif
                }
            } else {
#ifdef PARALLELDEBUG
                Debug::WriteLine("start" + task.index);
#endif
            }
            task.taskStarted = true;
        }
#pragma endregion

#pragma region AnimationParameters
    private:
        System::Void SwitchParentChild(float& angle, int8_t& spin, uint8_t& color);
        System::Void ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color);
        System::Void IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const uint16_t blur);
        inline System::Void IncFrameSize(float& size, const uint16_t period) {
            size *= powf(f->childSize, selectZoom * 1.0f / period);
        }
        //System::Void SetupFrameColorBlend(const float hueAngle, VecRefWrapper^ R);
#pragma endregion

#pragma region Interface_Calls
    public:
        System::Void StartGenerate();
        System::Void ResetGenerator();
        System::Void RequestCancel();
        bool SaveGif(System::String^ gifPath);
#ifdef CUSTOMDEBUG
        System::Void Log(System::String^ log, System::String^ line);
#endif
        void DebugStart();
#pragma endregion

#pragma region Interface_Settings
    public:
        bool SelectFractal(const uint16_t select);
        System::Void SetupFractal();
        System::Void SetMaxIterations(bool forcedReset);
        System::Void SetupAngle();
        System::Void SetupColor();
        inline System::Void SetupCutFunction() {
            cutFunction = f->cutFunction == nullptr
                || f->cutFunction->first == ""
                ? nullptr
                : &f->cutFunction[selectCut].second;
        }
        /*inline bool SelectDetail(const float detail) {
            if (this->detail == detail * f->minSize)
                return true;
            this->detail = detail * f->minSize;
            return false;
        }*/
        inline System::Void SelectThreadingDepth() {
            //preIterate = new std::tuple<float, float, std::pair<float, float>*>*[Math::Max(static_cast<int16_t>(1), selectMaxTasks)];
            //SetMaxIterations(true);
            maxDepth = 0;
            if (f->childCount <= 0)
                return;
            for (auto n = 1, threadCount = 0; (threadCount += n) < selectMaxGenerationTasks; n *= f->childCount)
                ++maxDepth;
        }
#pragma endregion

#pragma region Interface_Getters
    public:
        inline Fractal** GetFractals() { return fractals; }
        inline Fractal* GetFractal() { return fractals[selectFractal]; }
        inline Bitmap^ GetBitmap(const int index) { return bitmap == nullptr || bitmap->Length <= index ? nullptr : bitmap[index]; }
        inline uint16_t GetFinalPeriod();
        inline int GetFrames() { return bitmap == nullptr ? 0 : (int)(bitmap->Length); }
        inline int GetBitmapsFinished() { return bitmapsFinished; }
        inline bool IsGifReady() { return gifSuccess; }
        //GetTempGif();
        inline Fractal::CutFunction* GetCutFunction() { return fractals[selectFractal]->cutFunction == nullptr ? nullptr : &fractals[selectFractal]->cutFunction[selectCut].second; }
        std::string ConvertToStdString(System::String^ managedString);
#pragma endregion

        // Pack two floats into a double
        double pack(float f1, float f2) {
            uint64_t combined_bits = (static_cast<uint64_t>(*reinterpret_cast<uint32_t*>(&f1)) << 32) | static_cast<uint64_t>(*reinterpret_cast<uint32_t*>(&f2));   // Combine into a 64-bit value
            return *reinterpret_cast<double*>(&combined_bits);    // Reinterpret as a double
        }
        // Unpack a double into two floats
        void unpack(double packed, float& f1, float& f2) {
            uint64_t combined_bits = *reinterpret_cast<uint64_t*>(&packed); // Get bits of double
            uint32_t f1_bits = combined_bits >> 32;         // Extract upper 32 bits
            uint32_t f2_bits = combined_bits & 0xFFFFFFFF;  // Extract lower 32 bits
            f1 = *reinterpret_cast<float*>(&f1_bits);       // Reinterpret as float
            f2 = *reinterpret_cast<float*>(&f2_bits);       // Reinterpret as float
        }

    };
}