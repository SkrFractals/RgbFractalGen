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

    public ref class FractalGenerator {

    private:

#ifdef CUSTOMDEBUG
        // Debug variables
        System::String^ logString;                  // Debug Log
        std::chrono::steady_clock::time_point* startTime; // Start Stopwatch
        long long initTimes, iterTimes, voidTimes, drawTimes, gifTimes;
#endif

        // Fractal definitions
        Fractal** fractals;                         // Fractal definitions
        Fractal* f;                                 // Selected fractal definition

        // Generated data
        Vector*** buffer;                           // Buffer for points to print into bmp
        int16_t*** voidDepth;                       // Depths of Void
        std::queue<std::pair<int16_t, int16_t>>** voidQueue;
        uint8_t* childColor;						// A copy of childCoolor for allowing BGR
        float* childAngle;						    // A copy of selected childAngle
        array<Bitmap^>^ bitmap;		                // Prerender as an array of bitmaps
        uint8_t* bitmapState;                       // Flag if the bitmap was finished generating (ready to encode GIF and UnlockBits)
        array<System::Drawing::Imaging::BitmapData^>^ bitmapData;	// Locked Bits for bitmaps
        std::tuple<float, float, float, float, float, uint8_t, int>* tuples; // Queue struct for GenerateDots_OfDepth

        // Threading
        Object^ taskLock = gcnew Object();          // Monitor lock
        CancellationTokenSource^ cancel;            // Cancellation Token Source
        Task^ mainTask;                             // Main Generation Task
        ConcurrentBag<Task^>^ imageTasks;           // Parallel Image Tasks
        array<Task^>^ parallelTasks;                // Parallel Animation Tasks
        uint8_t* parallelTaskFinished;			    // States of Animation Tasks
        List<Task^>^ taskSnapshot;                  // Snapshot for safely checking imageTasks Wait

        bool* taskStarted;                          // Additional safety, could remove if it never gets triggered for a while

        // Generation variables
        uint8_t hueCycleMultiplier;                 // How fast should the hue shift to loop seamlessly?
        uint8_t applyParallelType;                  // Safely copy parallelType in here so it doesn't change in the middle of generation

        int8_t selectColorPalette;                  // RGB or BGR?

        uint16_t bitmapsFinished;					// How many bitmaps are completely finished generating? (ready to display, encoded if possible)
        uint16_t nextBitmap;						// How many bitmaps have started generating? (next task should begin with this one)
        uint16_t finalPeriodMultiplier;			    // How much will the period get finally stretched? (calculated for seamless + user multiplier)

        int16_t selectColor;						// Selected childColor definition
        int16_t selectAngle;						// Selected childAngle definition
        int16_t selectCut;						    // Selected CutFunction
        int16_t allocatedWidth;					    // How much buffer width is currently allocated?
        int16_t allocatedHeight;					// How much buffer height is currently allocated?
        int16_t allocatedTasks;					    // How many buffer tasks are currently allocated?
        int16_t allocatedFrames;					// How many bitmap frames are currently allocated?
        int16_t applyMaxTasks;                      // Safely copy maxTasks in here so it doesn't change in the middle of generation
        int16_t applyMaxGenerationTasks;            // Safely copy maxGenerationTasks in here so it doesn't change in the middle of generation

        int16_t widthBorder;						// Precomputed maximum x coord fot pixel input
        int16_t heightBorder;						// Precomputed maximum y coord for pixel input
        float upleftStart;						    // Precomputed minimum x/y for dot iteration/application
        float rightEnd;                             // Precomputed maximum -x- for dot iteration/application
        float downEnd;                              // Precomputed maximum -y- for dot iteration/application

        Fractal::CutFunction* cutFunction;          // Selected CutFunction pointer
        bool gifSuccess, exportingGif;              // Temp GIF file "gif.tmp" successfuly created | flag only allowing one GIF Encoding thread
        void* gifEncoder;                           // Export GIF encoder
        float logBase, periodAngle, bloom1;         // Size Log Base | Angle symmetry of current fractal | Precomputed bloom+1
        Vector* colorBlend;                         // Color blend of a selected fractal for more seamless loop (between single color to the sum of children)
        System::Drawing::Rectangle rect;            // Bitmap rectangle TODO implement
        System::String^ gifTempPath;                // Temporary GIF file name
        
        float* emptyFloat;                          // Preallocated empty angle array when it doesn't exist
        float ambnoise;                             // Normalizer for maximum void depth - Precomputed amb * noise
       
    public:
        // Settings
        float detail, noise, saturate, bloom;
        uint8_t parallelType, encode;
        int8_t zoom, hueCycle;
        uint16_t debug, width, height, period, delay, defaultZoom, defaultAngle, defaultHue, amb, selectBlur, extraSpin, extraHue;
        int16_t select, maxTasks, maxGenerationTasks, cutparam, defaultSpin, maxDepth, periodMultiplier;

#pragma region Init
    public:
        FractalGenerator();
    private:
        System::Void DeleteEncoder();
        System::Void DeleteBuffer(const uint16_t taskIndex);
        System::Void NewBuffer(const uint16_t taskIndex);
#pragma endregion

#pragma region Generate_Tasks
    private:
        System::Void GenerateAnimation();
        System::Void GenerateImage(const uint16_t& bitmapIndex, const int16_t& taskIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color);
        System::Void GenerateDots_SingleTask(VecRefWrapper^ R,
                                             const float inX, const float inY, const float inAngle, const float inAntiAngle, const float inSize,
                                             const uint8_t inColor, const int inFlags);
        System::Void GenerateDots_OfDepth(VecRefWrapper^ R);
        System::Void GenerateDots_OfRecursion(VecRefWrapper^ R,
                                         const float inX, const float inY, const float inAngle, const float inAntiAngle, const float inSize,
                                         const uint8_t inColor, const int inFlags, const uint16_t inDepth);
        //System::Void GenerateVoid(std::queue<std::pair<int16_t, int16_t>>& queueT, const Vector**& buffT, int16_t**& voidT, float& lightNormalizer, float& voidDepthMax);
        //System::Void GenerateBitmap(uint16_t bitmapIndex, const Vector**& buffT, const int16_t**& voidT, const float lightNormalizer, const float voidDepthMax);
        //System::Void GenerateGif(const int16_t taskIndex);
        bool FinishTask(const int16_t taskIndex);
        System::Void TryGif(const int16_t taskIndex);
        //System::Void WaitForRecursiveTasks();
#pragma endregion

#pragma region Generate_Inline
    private:
        inline void ApplyDot(const VecRefWrapper^ R, const float& inX, const float& inY, const float& inSize, const uint8_t& inColor) {
            //Vector<float> dotColor = (1 - lerp) * colorBlendH + lerp * colorBlendI;
            Vector dotColor = Vector::Lerp(R->H->ToNative(), R->I->ToNative(), logf(detail / inSize) / logBase);
            switch (inColor) {
            case 1: dotColor = Y(dotColor); break;
            case 2: dotColor = Z(dotColor); break;
            }
            // Iterated deep into a single point - Interpolate inbetween 4 pixels and Stop
            const auto startY = Math::Max(static_cast<int16_t>(1), static_cast<int16_t>(floor(inY - bloom))),
                endX = Math::Min(widthBorder, static_cast<int16_t>(ceil(inX + bloom))),
                endY = Math::Min(heightBorder, static_cast<int16_t>(ceil(inY + bloom)));
            for (int16_t y, x = Math::Max(static_cast<int16_t>(1), static_cast<int16_t>(floor(inX - bloom))); x <= endX; x++) {
                const auto xd = bloom1 - abs(x - inX);
                for (y = startY; y <= endY; y++)
                    R->T[y][x] += xd * (bloom1 - abs(y - inY)) * dotColor; //buffT[y][x] += Vector<float>((1.0f - abs(x - inX)) * (1.0f - abs(y - inY))) * dotColor;
            }
        }
        inline int CalculateFlags(const int& index, const int& inFlags) { return cutFunction == nullptr ? inFlags : (*cutFunction)(index, inFlags); }
        inline bool TestSize(const float& newX, const float& newY, const float& inSize) {
            const auto testSize = inSize * f->cutSize;
            return ((Math::Min(newX, newY) + testSize <= upleftStart) || (newX - testSize >= rightEnd) || (newY - testSize >= downEnd) );
        }
        Vector& ApplyAmbientNoise(Vector& rgb, const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen);
        Vector ApplySaturate(const Vector& rgb);
        System::Void ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p);
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
        inline void Join(int i) {
            if (taskStarted[i]) {
                if (parallelTasks[i] != nullptr) {
#ifdef PARALLELDEBUG
                    Debug::WriteLine("join" + i);
#endif
                    parallelTasks[i]->Wait();
                    parallelTasks[i] = nullptr;
                } else {
#ifdef PARALLELDEBUG
                    Debug::WriteLine("ERROR not joinable " + i);
#endif
                }
            } else {
#ifdef PARALLELDEBUG
                Debug::WriteLine("ERROR join: task not running " + i);
#endif
            }
            taskStarted[i] = false;
        }

        inline void Start(int i) {
            if (taskStarted[i]) {
#ifdef PARALLELDEBUG
                Debug::WriteLine("ERROR start: task already running " + i);
#endif

                if (parallelTasks[i] != nullptr) {
                    parallelTasks[i]->Wait();
                    parallelTasks[i] = nullptr;
                } else {
#ifdef PARALLELDEBUG
                    Debug::WriteLine("ERROR not joinable " + i);
#endif
                }
            } else {
#ifdef PARALLELDEBUG
                Debug::WriteLine("start" + i);
#endif
            }
            taskStarted[i] = true;
        }
#pragma endregion

#pragma region AnimationParameters
    private:
        System::Void SwitchParentChild(float& angle, int8_t& spin, uint8_t& color);
        System::Void ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color);
        System::Void IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const uint16_t blur);
        inline System::Void IncFrameSize(float& size, const uint16_t period) {
            size *= powf(f->childSize, zoom * 1.0f / period);
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
        bool SelectAngle(const uint16_t selectAngle);
        System::Void SetupAngle();
        bool SelectColor(const uint16_t selectColor);
        bool SelectColorPalette(const uint8_t selectColorPalette);
        System::Void SetupColor();
        bool SelectCutFunction(const uint16_t selectCut);
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
            maxDepth = 0;
            if (f->childCount <= 0)
                return;
            for (auto n = 1, threadCount = 0; (threadCount += n) < maxGenerationTasks; n *= f->childCount)
                ++maxDepth;
        }
#pragma endregion

#pragma region Interface_Getters
    public:
        inline Fractal** GetFractals() { return fractals; }
        inline Fractal* GetFractal() { return fractals[select]; }
        inline Bitmap^ GetBitmap(const int index) { return bitmap == nullptr || bitmap->Length <= index ? nullptr : bitmap[index]; }
        inline uint16_t GetFinalPeriod();
        inline int GetFrames() { return bitmap == nullptr ? 0 : (int)(bitmap->Length); }
        inline int GetBitmapsFinished() { return bitmapsFinished; }
        inline bool IsGifReady() { return gifSuccess; }
        //GetTempGif();
        inline Fractal::CutFunction* GetCutFunction() { return cutFunction; }
        std::string ConvertToStdString(System::String^ managedString);
#pragma endregion

    };
}