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
        Fractal** fractals;                          // Fractal definitions
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
        ConcurrentBag<Task^>^ imageTasks;           // Parallel Iteration Tasks
        array<Task^>^ animationTasks;               // Parallel Animation Tasks
        uint8_t* animationTaskFinished;			    // States of Animation Tasks
        List<Task^>^ taskSnapshot;                  // Snapshot for safely checking imageTasks Wait
        // Generation variables
        uint8_t selectColorPalette;
        uint16_t select, selectCut, bitmapsFinished, nextBitmap, finalPeriodMultiplier, hueCycleMultiplier;
        int16_t selectColor, selectAngle;
        Fractal::CutFunction* cutFunction;
        bool gifSuccess, exportingGif;              // Temp GIF file "gif.tmp" successfuly created | flag only allowing one GIF Encoding thread
        void* gifEncoder;                           // Export GIF encoder
        float logBase, ambnoise, periodAngle;       // Normalizer to keep fractal luminance constant | Normalizer for maximum void depth | Size Log Base | Ambient*Noise multiplication
        Vector* colorBlend;                         // Color blend of a selected fractal for more seamless loop (between single color to the sum of children)
        System::Drawing::Rectangle rect;            // Bitmap rectangle TODO implement
        System::String^ gifTempPath;

    public:
        // Settings
        bool third, parallelType;
        float detail, noise, saturate;
        uint8_t selectBlur, encode, extraSpin, extraHue;
        int8_t zoom, defaultSpin, hueCycle, maxDepth;
        uint16_t periodMultiplier, debug, width, height, period, delay, amb, defaultZoom, defaultAngle, defaultHue;
        int16_t maxTasks, maxGenerationTasks, cutparam;
        

#pragma region Init
    public:
        FractalGenerator();
    private:
        System::Void InitFractals();
        System::Void InitBuffer(const int16_t taskIndex);
        System::Void DeleteEncoder();
#pragma endregion

#pragma region Generate
    private:
        System::Void Generate();
        System::Void GenerateThread(const uint16_t& bitmapIndex, const int16_t& taskIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color);
        System::Void GenerateFractal(
            Vector * *&buffT, const Vector &colorBlendI, const Vector &colorBlendH, 
            const float inX, const float inY, const float inAngle, const float inAntiAngle, const float inSize,
            const uint8_t inColor, const uint8_t inDepth, const int inFlags);
        
        bool FinishTask(const int16_t taskIndex);
        System::Void TryGif(const int16_t taskIndex);
#pragma endregion

#pragma region TaskWrappers
    private:
        System::Void GenerateFractalTask(System::Object^ obj);
        System::Void GenerateThreadTask(System::Object^ obj);
        System::Void EncodeGifFrameTask(System::Object^ obj);
#pragma endregion

#pragma region FrameParameters
    private:
        System::Void ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color);
        System::Void SwitchParentChild(float& angle, int8_t& spin, uint8_t& color);
        System::Void IncrementFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, uint8_t& color, const uint8_t blur);
        System::Void SetupColorBlend(const float hueAngle, Vector& colorBlendI, Vector& colorBlendH);
        inline System::Void IncrementFrameSize(float& size, const uint16_t period) {
            size *= powf(f->childSize, zoom * 1.0f / period);
        }
#pragma endregion

#pragma region Draw
    private:
        System::Void CalculateVoid(std::queue<std::pair<int16_t, int16_t>>& queueT, const Vector**& buffT, int16_t**& voidT, float& lightNormalizer, float& voidDepthMax);
        Vector& ApplyAmbientNoise(Vector& rgb, const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen);
        Vector ApplySaturate(const Vector& rgb);
        System::Void ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p);
        inline System::Void NoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline System::Void NoNoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline System::Void NoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        inline System::Void NoNoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax);
        System::Void DrawBitmap(uint16_t bitmapIndex, const Vector**& buffT, const int16_t**& voidT, const float lightNormalizer, const float voidDepthMax);
        System::Void EncodeGifFrame(const int16_t taskIndex);
#pragma endregion

#pragma region Interface
    public:
        System::Void StartGenerate();
        System::Void ResetGenerator();
        uint16_t GetFinalPeriod();
        System::Void WaitForThreads();
        System::Void RequestCancel();
        bool SaveGif(System::String^ gifPath);
        bool SelectFractal(const uint16_t select);
        System::Void SetupFractal();
        bool SelectColor(const uint16_t selectColor);
        bool SelectColorPalette(const uint8_t selectColorPalette);
        System::Void SelectColor();
        bool SelectAngle(const uint16_t selectAngle);
        inline Fractal::CutFunction* GetCutFunction() { return cutFunction; }
        bool SelectCutFunction(const uint16_t selectCut);
        inline System::Void InitColorBlend() {
            // Prepare subiteration color blend
            float* colorBlendF = new float[3];
            colorBlendF[0] = colorBlendF[1] = colorBlendF[2] = 0;
            for (auto i = f->childCount; 0 <= --i; colorBlendF[childColor[i]] += 1.0f / f->childCount);
            colorBlend = new Vector(colorBlendF[0], colorBlendF[1], colorBlendF[2]);
        }
        inline System::Void SelectThreadingDepth() {
            maxDepth = 0;
            if (f->childCount <= 0)
                return;
            for (auto n = 1, threadCount = 0; (threadCount += n) < maxGenerationTasks; n *= f->childCount)
                ++maxDepth;
        }
        inline bool SelectDetail(const float detail) { 
            if (this->detail == detail * f->minSize)
                return true;
            this->detail = detail * f->minSize;
            return false;
        }
        inline int GetBitmapsFinished() { return bitmapsFinished; }
        inline int GetBitmapsTotal() { return bitmap == nullptr ? 0 : (int)(bitmap->Length); }
        inline Fractal** GetFractals() { return fractals; }
        inline Fractal* GetFractal() { return f; }
        inline bool IsGifReady() { return gifSuccess; }
        inline Bitmap^ GetBitmap(const int index) { return bitmap == nullptr || bitmap->Length <= index ? nullptr : bitmap[index]; }
        std::string ConvertToStdString(System::String^ managedString);
        void DebugStart();
#ifdef CUSTOMDEBUG
        System::Void Log(System::String^ log, System::String^ line);
#endif
#pragma endregion

    };
}