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

        // Threading
        Object^ taskLock = gcnew Object();          // Monitor lock
        CancellationTokenSource^ cancel;            // Cancellation Token Source
        Task^ mainTask;                             // Main Generation Task
        std::tuple<float, float, float, float, float, uint8_t, int>* tuples;
        ConcurrentBag<Task^>^ imageTasks;           // Parallel Iteration Tasks
        array<Task^>^ parallelTasks;                // Parallel Animation Tasks
        uint8_t* parallelTaskFinished;			    // States of Animation Tasks
        List<Task^>^ taskSnapshot;                  // Snapshot for safely checking imageTasks Wait

        // Generation variables
        uint8_t hueCycleMultiplier, applyParallelType;
        int8_t selectColorPalette;
        uint16_t bitmapsFinished, nextBitmap, finalPeriodMultiplier;
        int16_t select, selectColor, selectAngle, selectCut, allocatedWidth, allocatedHeight, allocatedTasks, allocatedFrames, applyMaxTasks, applyMaxGenerationTasks;
        Fractal::CutFunction* cutFunction;          // Selected CutFunction pointer
        bool gifSuccess, exportingGif;              // Temp GIF file "gif.tmp" successfuly created | flag only allowing one GIF Encoding thread
        void* gifEncoder;                           // Export GIF encoder
        float logBase, ambnoise, periodAngle;       // Normalizer to keep fractal luminance constant | Normalizer for maximum void depth | Size Log Base | Ambient*Noise multiplication
        Vector* colorBlend;                         // Color blend of a selected fractal for more seamless loop (between single color to the sum of children)
        System::Drawing::Rectangle rect;            // Bitmap rectangle TODO implement
        System::String^ gifTempPath;                // Temporary GIF file name
        float* emptyFloat;

        bool* taskStarted;

    public:
        // Settings
        float detail, noise, saturate;
        uint8_t parallelType, selectBlur, encode, extraSpin, extraHue;
        int8_t zoom, defaultSpin, hueCycle, maxDepth;
        uint16_t periodMultiplier, debug, width, height, period, delay, amb, defaultZoom, defaultAngle, defaultHue;
        int16_t maxTasks, maxGenerationTasks, cutparam;

#pragma region Init
    public:
        FractalGenerator();
    private:
        System::Void InitFractalDefinitions();
        System::Void DeleteEncoder();
        System::Void DeleteBuffer(const uint16_t taskIndex);
        System::Void NewBuffer(const uint16_t taskIndex);
        System::Void InitBuffer(const int16_t taskIndex);
        inline System::Void InitColorBlend() {
            // Prepare subiteration color blend
            float* colorBlendF = new float[3];
            colorBlendF[0] = colorBlendF[1] = colorBlendF[2] = 0;
            for (auto i = f->childCount; 0 <= --i; colorBlendF[childColor[i]] += 1.0f / f->childCount);
            colorBlend = new Vector(colorBlendF[0], colorBlendF[1], colorBlendF[2]);
        }
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
        System::Void GenerateVoid(std::queue<std::pair<int16_t, int16_t>>& queueT, const Vector**& buffT, int16_t**& voidT, float& lightNormalizer, float& voidDepthMax);
        System::Void GenerateBitmap(uint16_t bitmapIndex, const Vector**& buffT, const int16_t**& voidT, const float lightNormalizer, const float voidDepthMax);
        System::Void GenerateGif(const int16_t taskIndex);
        bool FinishTask(const int16_t taskIndex);
        System::Void TryGif(const int16_t taskIndex);
        System::Void WaitForRecursiveTasks();
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
            const auto startY = static_cast<uint16_t>(floor(inY)), endX = static_cast<uint16_t>(ceil(inX)), endY = static_cast<uint16_t>(ceil(inY));
            for (int16_t y, x = static_cast<uint16_t>(floor(inX)); x <= endX; x++)
                for (y = startY; y <= endY; y++)
                    R->T[y][x] += ((1.0f - abs(x - inX)) * (1.0f - abs(y - inY))) * dotColor; //buffT[y][x] += Vector<float>((1.0f - abs(x - inX)) * (1.0f - abs(y - inY))) * dotColor;
        }
        inline int CalculateFlags(const int& index, const int& inFlags) { return cutFunction == nullptr ? inFlags : (*cutFunction)(index, inFlags); }
        inline bool TestSize(const float& newX, const float& newY, const float& inSize) {
            const auto testSize = inSize * f->cutSize; 
            return ((Math::Min(newX, newY) + testSize <= 2) || (newX - testSize >= width - 3) || (newY - testSize >= height - 3) );
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
#pragma endregion

#pragma region AnimationParameters
    private:
        System::Void SwitchParentChild(float& angle, int8_t& spin, uint8_t& color);
        System::Void ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color);
        System::Void IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, uint8_t& color, const uint8_t blur);
        inline System::Void IncFrameSize(float& size, const uint16_t period) {
            size *= powf(f->childSize, zoom * 1.0f / period);
        }
        System::Void SetupFrameColorBlend(const float hueAngle, VecRefWrapper^ R);
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
        bool SelectColor(const uint16_t selectColor);
        bool SelectColorPalette(const uint8_t selectColorPalette);
        System::Void SetupColor();
        bool SelectCutFunction(const uint16_t selectCut);
        inline bool SelectDetail(const float detail) {
            if (this->detail == detail * f->minSize)
                return true;
            this->detail = detail * f->minSize;
            return false;
        }
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
        inline Fractal* GetFractal() { return f; }
        inline Bitmap^ GetBitmap(const int index) { return bitmap == nullptr || bitmap->Length <= index ? nullptr : bitmap[index]; }
        inline uint16_t GetFinalPeriod();
        inline int GetFrames() { return bitmap == nullptr ? 0 : (int)(bitmap->Length); }
        inline int GetBitmapsFinished() { return bitmapsFinished; }
        inline bool IsGifReady() { return gifSuccess; }
        //GetTempGif();
        inline Fractal::CutFunction* GetCutFunction() { return cutFunction; }
        std::string ConvertToStdString(System::String^ managedString);
#pragma endregion

        inline void Join(int i);
        inline void Start(int i);

    };
}