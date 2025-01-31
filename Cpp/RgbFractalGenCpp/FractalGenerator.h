// Allow debug code, comment this when releasing for slightly better performance
//#define CUSTOMDEBUG


#pragma once
#include <vector>
#include <mutex>
#include <random>
#include <queue>
#include <atomic>
#include "Vector.h"
#include "Fractal.h"

#ifdef CUSTOMDEBUG
#include <chrono>
#endif

namespace RgbFractalGenCpp {

    using namespace System;

    public struct FractalTask {
        std::thread task;   // Parallel Animation Tasks
        uint8_t state;      // States of Animation Tasks
        Vector** buffer;    // Buffer for points to print into bmp
        int16_t** voidDepth;// Depths of Void
        std::queue<std::pair<int16_t, int16_t>> voidQueue;
        Vector H;           // mixed children color
        Vector I;           // pure parent color
        bool taskStarted;   // Additional safety, could remove if it never gets triggered for a while
        uint16_t index;
        std::tuple<float, float, std::pair<float, float>*>*
            preIterate;         // (childSize, childDetail, childSpread, (childX,childY)[])
        FractalTask() {
            taskStarted = false;
            state = 2;
            buffer = nullptr;
            voidDepth = nullptr;
        }
        bool FinishTask() {
            if (state == 1) {
                Join();
                state = 2;
            }
            return state >= 2;
        }
        void Join() {
            if (taskStarted) {
                if (task.joinable()) {
//#ifdef PARALLELDEBUG
//                    Debug::WriteLine("join" + index);
//#endif
                    task.join();
                } else {
//#ifdef PARALLELDEBUG
//                    Debug::WriteLine("ERROR not joinable " + index);
//#endif
                }
            } else {
//#ifdef PARALLELDEBUG
//                Debug::WriteLine("ERROR join: task not running " + index);
//#endif
            }
            taskStarted = false;
        }

        void Start() {
            if (taskStarted) {
//#ifdef PARALLELDEBUG
//                Debug::WriteLine("ERROR start: task already running " + index);
//#endif
                if (task.joinable()) {
                    task.join();
                } else {
//#ifdef PARALLELDEBUG
//                    Debug::WriteLine("ERROR not joinable " + index);
//#endif
                }
            } else {
//#ifdef PARALLELDEBUG
//                Debug::WriteLine("start" + index);
//#endif
            }
            taskStarted = true;
        }
    };

    class FractalGenerator {

#ifdef CUSTOMDEBUG
    private:
        // Debug variables
        std::string logString;                      // Debug Log
        std::chrono::steady_clock::time_point* startTime; // Start Stopwatch
        long long initTimes, iterTimes, voidTimes, drawTimes, gifTimes;
#endif

    private:
        // Definitions
        Fractal** fractals;                         // Fractal definitions
        Fractal* f;                                 // Selected fractal definition
        uint16_t maxChildren;
        float* childAngle;						    // A copy of selected childAngle
        float periodAngle;      // Angle symmetry of current fractal
        uint8_t* childColor;						// A copy of childCoolor for allowing BGR
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
        uint16_t frames;                            // how many animation frames am i generating?
        uint8_t** bitmap;                           // BitmapData pointers
        std::vector<uint8_t> bitmapState;           // Flag if the bitmap was finished generating (ready to encode GIF and UnlockBits)
        //array<System::Drawing::Imaging::BitmapData^>^
        //    bitmapData;	                // Locked Bits for bitmaps
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
        float ambnoise;         // Normalizer for maximum void depth - Precomputed amb * noise
        float bloom1;           // Precomputed bloom+1
        //Random random;          // Random generator

        // Threading
        FractalTask* tasks;
        uint8_t applyParallelType;			// Safely copy parallelType in here so it doesn't change in the middle of generation
        int16_t applyMaxTasks;				// Safely copy maxTasks in here so it doesn't change in the middle of generation
        int16_t applyMaxGenerationTasks;	// Safely copy maxGenerationTasks in here so it doesn't change in the middle of generation
        int16_t maxDepth;					// Maximum depth for Recusrion parallelism
        int16_t allocatedTasks;				// How many buffer tasks are currently allocated?
        std::mutex taskLock;                        // Monitor lock
        std::atomic<bool> cancelRequested;          // Cancellation Token Source
        std::thread mainTask;                       // Main Generation Task
        //ConcurrentBag<Task^>^ imageTasks;   // Parallel Image Tasks
        //List<Task^>^ taskSnapshot;          // Snapshot for safely checking imageTasks Wait
        std::tuple<uint16_t, std::pair<float, float>, std::pair<float, float>, uint8_t, int, uint8_t>*
            tuples; // Queue struct for GenerateDots_OfDepth

        // Export
        void* gifEncoder;               // Export GIF encoder
        bool gifSuccess, exportingGif;	// Temp GIF file "gif.tmp" successfuly created | flag only allowing one GIF Encoding thread
        std::string gifTempPath;                    // Temporary GIF file name

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
        void DeleteEncoder();
        void DeleteBuffer(const FractalTask& task) const;
        void NewBuffer(FractalTask& task) const;
#pragma endregion

#pragma region Generate_Tasks
    private:
        void GenerateAnimation();
        void GenerateImage(const uint16_t& bitmapIndex, const int16_t& stateIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color);
        void GenerateDots_SingleTask(const uint16_t taskIndex, const std::pair<float, float> inXY, const std::pair<float, float> inAngle, const uint8_t inColor, const int inFlags, uint8_t inDepth);
        void GenerateDots_OfDepth();
        void GenerateDots_OfRecursion(const uint16_t taskIndex, const std::pair<float, float> inXY, const std::pair<float, float> inAngle, const uint8_t inColor, const int inFlags, uint8_t inDepth);
        void GenerateGif(const int16_t taskIndex);
        //bool FinishTask(FractalTask& task);
        void TryGif(FractalTask& task);
#pragma endregion

#pragma region Generate_Inline
    private:
        bool ApplyDot(const bool apply, const FractalTask& taskIndex, const std::pair<float, float>& inXY, const float& inDetail, const uint8_t& inColor) const;
        inline int CalculateFlags(const int& index, const int& inFlags) { return cutFunction == nullptr ? inFlags : (*cutFunction)(index, inFlags); }
        inline bool TestSize(const std::pair<float, float>& newXY, const float& inSize) {
            const auto testSize = inSize * f->cutSize;
            return ((std::min(newXY.first, newXY.second) + testSize > upleftStart) && (newXY.first - testSize < rightEnd) && (newXY.second - testSize < downEnd));
        }
        static Vector Normalize(const Vector& pixel, const float lightNormalizer);
        Vector& ApplyAmbientNoise(Vector& rgb, const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen);
        Vector ApplySaturate(const Vector& rgb);
        void ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p);
        inline void NoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline void NoNoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline void NoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline void NoNoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
#pragma endregion

#pragma region TaskWrappers
    private:
        void Task_OfDepth(const int16_t taskIndex, const uint16_t tupleIndex);
#pragma endregion

#pragma region AnimationParameters
    private:
        void SwitchParentChild(float& angle, int8_t& spin, uint8_t& color);
        void ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color);
        void IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const int16_t blur);
        inline void IncFrameSize(float& size, const uint16_t period) {
            size *= powf(f->childSize, selectZoom * 1.0f / period);
        }
#pragma endregion

#pragma region Interface_Calls
    public:
        void StartGenerate();
        void ResetGenerator();
        void RequestCancel();
        bool SaveGif();
        inline void SetPixelsPointer(const uint16_t i, uint8_t* pointer) { bitmap[i] = pointer; }
        inline void UnlockBitmapState(uint16_t bitmapIndex) { bitmapState[bitmapIndex] = 5; }
#ifdef CUSTOMDEBUG
        void Log(std::string& log, const std::string& line);
#endif
        void DebugStart();
#pragma endregion

#pragma region Interface_Settings
    public:
        bool SelectFractal(const uint16_t select);
        void SetupFractal();
        void SetMaxIterations(bool forcedReset);
        void SetupAngle();
        void SetupColor();
        inline void SetupCutFunction() {
            cutFunction = f->cutFunction == nullptr
                || f->cutFunction->first == ""
                ? nullptr
                : &f->cutFunction[selectCut].second;
        }
        inline void SelectThreadingDepth() {
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
        //GetBitmap()
        uint16_t GetFinalPeriod();
        inline uint16_t GetFrames() const { return frames; }
        inline uint16_t GetBitmapsFinished() const { return bitmapsFinished; }
        inline bool IsGifReady() const { return gifSuccess; }
        std::string GetTempGif() { return gifTempPath; }
        inline Fractal::CutFunction* GetCutFunction() { return fractals[selectFractal]->cutFunction == nullptr ? nullptr : &fractals[selectFractal]->cutFunction[selectCut].second; }
        inline uint8_t GetBitmapState(uint16_t bitmapIndex) { return bitmapState.size() > bitmapIndex ? bitmapState[bitmapIndex] : 0; }
        inline uint8_t* GetPixelsPointer(const uint16_t i) { return bitmap[i]; }
#pragma endregion

    };


    


}