//#define PARALLELDEBUG

#include "FractalGenerator.h"
#include <omp.h>
#include "GifEncoder.h"

#define M_PI 3.14159265358979323846
//#define Y(V) = Vector<float>(gcnew array<float>(3){ V[2], V[0], V[1] })
//#define Z(V) = Vector<float>(gcnew array<float>(3){ V[1], V[2], V[0] })

#define FA(N) new std::pair<std::string, float*>[N]
#define F(N) new float[N]
#define BA(N) new std::pair<std::string, uint8_t*>[N]
#define B(N) new uint8_t[N]
#define CA(N) new std::pair<std::string, Fractal::CutFunction>[N]
#define FE { "", new float[0] { } }
#define BE { "", new uint8_t[0] { } }
#define CE { "", nullptr }
#define CHILDPARAMS taskIndex, std::pair<float,float>(width * .5f, height * .5f), std::pair<float,float>(angle, abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, static_cast<uint8_t>(0)
#define IFAPPLIEDDOT const FractalTask& task = tasks[taskIndex];\
	const auto& preIterated = task.preIterate[inDepth];const auto& inSize = std::get<0>(preIterated);\
	if (ApplyDot(inSize < selectDetail, task, inX, inY, std::get<1>(preIterated), inColor))
#define STARTITERATION const auto& newPreIterated = task.preIterate[++inDepth];auto i = f->childCount; while (0 <= --i)
#define ITERATECHILDREN if (cancelRequested) return;\
	const auto newFlags = CalculateFlags(i, inFlags);if (newFlags < 0) continue;\
	const auto& XY = std::get<2>(preIterated)[i];const auto cs = cos(inAngle.first), sn = sin(inAngle.first);\
	const std::pair<float, float> newXY = std::pair<float, float>(inXY.first + XY.first * cs - XY.second * sn, inXY.second - XY.second * cs - XY.first * sn);\
	if (TestSize(newXY, inSize))
#define NEWCHILD taskIndex, newXY, i == 0\
	? std::pair<float, float>(inAngle.first + childAngle[i] - inAngle.second, -inAngle.second)\
	: std::pair<float, float>(inAngle.first + childAngle[i], inAngle.second), (inColor + childColor[i]) % 3, newFlags, inDepth

namespace RgbFractalGenCpp {

#ifdef CUSTOMDEBUG
	using namespace System::Diagnostics;
#endif
#ifdef PARALLELDEBUG
	using namespace System::Diagnostics;
#endif

#pragma unmanaged

#pragma region Init
	FractalGenerator::FractalGenerator() {
		selectCutparam = debug = selectDefaultHue = selectDefaultZoom = selectDefaultAngle = selectExtraSpin = selectExtraHue = selectParallelType = 0;
		selectZoom = 1;
		selectFractal = selectChildColor = selectChildAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks = allocatedFrames = selectSpin = maxIterations = selectColorPalette = -1;
		selectEncode = 2;
		gifEncoder = nullptr;
		bitmap = nullptr;
		cutFunction = nullptr;
		tuples = nullptr;
		childAngle = nullptr;
		childColor = nullptr;
		gifSuccess = false;
		//taskSnapshot = gcnew System::Collections::Generic::List<Task^>();
		emptyFloat = new float[1] { static_cast<float>(M_PI) };
		// Constants
		const float pi = static_cast<float>(M_PI), pi23 = 2 * pi / 3, pi43 = 4 * pi / 3, SYMMETRIC = 2 * pi,
			stt = sqrt(.75f), sqrt2 = sqrt(2.0f),
			pfs = 2 * (1 + static_cast<float>(cos(.4 * pi))),
			cosc = static_cast<float>(cos(.4 * pi)) / pfs,
			sinc = static_cast<float>(sin(.4 * pi)) / pfs,
			v = 2 * (sinc * sinc + cosc * (cosc + pfs)) / (2 * sinc),
			s0 = (2 + sqrt2) / sqrt2,
			sx = 2 + sqrt2,
			r = (1 / s0 + 1 / sx) / (1 / s0 + 1 / (2 * sx)),
			diag = sqrt2 / 3;
		// X, Y
		float* cx = new float[9], * cy = new float[9], * pfx = new float[6], * pfy = new float[6], * tfx = new float[4], * tfy = new float[4], * ttfx = new float[16], * ttfy = new float[16], * ofx = new float[9], * ofy = new float[9];
		// Carpets
		for (int i = 0; i < 4; ++i) {
			float ipi = i * pi / 2, icos = diag * cos(i * pi / 2), isin = diag * sin(i * pi / 2);
			cx[i * 2 + 1] = icos - isin;
			cy[i * 2 + 1] = isin + icos;
			cx[i * 2 + 2] = icos;
			cy[i * 2 + 2] = isin;
		}
		cx[0] = cy[0] = 0;
		// Pentaflakes
		for (int i = 1; i <= 5; i++) {
			pfx[i] = v * static_cast<float>(cos(.4f * (i + 3) * pi));
			pfy[i] = v * static_cast<float>(sin(.4f * (i + 3) * pi));
		}
		pfx[0] = pfy[0] = 0;
		// Triflakes
		for (auto i = 1; i <= 3; i++) {
			tfx[i] = .5f * static_cast<float>(cos(i * pi23));
			tfy[i] = .5f * static_cast<float>(sin(i * pi23));
		}
		tfx[0] = tfy[0] = 0;
		// Tetratriflakes
		for (auto i = 1; i <= 3; i++) {
			const auto ci = .5f * static_cast<float>(cos(i * pi23)), si = .5f * static_cast<float>(sin(i * pi23)),
				ci1 = .5f * static_cast<float>(cos((i + 1) * pi23)), si1 = .5f * static_cast<float>(sin((i + 1) * pi23)),
				ci2 = .5f * static_cast<float>(cos((i + 2) * pi23)), si2 = .5f * static_cast<float>(sin((i + 2) * pi23));
			ttfx[i] = -ci;
			ttfy[i] = -si;
			ttfx[i + 3] = ci - ci1;
			ttfy[i + 3] = si - si1;
			ttfx[i + 6] = ci - ci2;
			ttfy[i + 6] = si - si2;
			ttfx[i + 9] = 2 * ci - ci1 - ci2;
			ttfy[i + 9] = 2 * si - si1 - si2;
			ttfx[i + 12] = ci - ci1 - ci2;
			ttfy[i + 12] = si - si1 - si2;
		}
		ttfx[0] = ttfy[0] = 0;
		// Octaflakes (not working)
		for (auto i = 1; i <= 8; i++) {
			ofx[i] = r * static_cast<float>(cos(i * pi / 4.0));
			ofy[i] = r * static_cast<float>(sin(i * pi / 4.0));
		}

		// Rotations
		// childAngle[0] = SYMMETRIC + symmetry when 2*pi is symmetric!
		// childAngle[0] = symmetry when 2*pi is NOT symmetric!

		fractals = new Fractal * [10] {
			//new Fractal("Void", 0, 1000, 1, .1f, 1, { 0 }, { 0 }, FA(2) { { "N", F(1){ pi } }, FE }, BA(1) { { "N", B(1){0} }, BE }, nullptr),
			new Fractal("Void", 0, 1000, 1, .1f, 1, { 0 }, { 0 }, nullptr, nullptr, nullptr),
			new Fractal("TriTree", 10, 4, .2f, .05f, .9f,
				F(10) { 0, -stt, 2 * stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt },
				F(10) { 0, -1.5f, 0, 1.5f, 1.5f, 3, 1.5f, -1.5f, -3, -1.5f },
				FA(5) {
					{ "RingTree", F(10) { SYMMETRIC + pi23, pi, pi + pi23, pi + 2 * pi23, 0, 0, pi23, pi23, 2 * pi23, 2 * pi23 } },
					{ "BeamTree_Beams", F(10) { pi / 3, 0, pi23, pi43, pi, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43 } },
					{ "BeamTree_OuterJoint", F(10) { pi / 3, 0, pi23, pi43, pi + pi23, pi + pi23, pi, pi, pi + pi43, pi + pi43 } },
					{ "BeamTree_InnerJoint", F(10) { pi / 3, 0, pi23, pi43, pi, pi, pi, pi, pi, pi } },
					FE
				}, BA(3){
					{ "Center", B(10) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "CenterNeighbors", B(10) { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0 } },
					BE
				}, CA(5) {
					{ "NoChild", Fractal::Trees_NoChildParam },
					{ "NoBeam", Fractal::Beamtree_NoBeam },
					{ "OuterJoint", Fractal::Beamtree_OuterJoint },
					{ "InnerJoint", Fractal::Beamtree_InnerJoint },
					CE
				}
			), 
			new Fractal("TriComb", 13, 5, .2f, .05f, .9f,
				F(13) { 0, 2 * stt, -stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt, 2 * stt, -4 * stt, 2 * stt},
				F(13) { 0, 0, 1.5f, -1.5f, 1.5f, 3, 1.5f, -1.5f, -3, -1.5f, 3, 0, -3 },
				FA(2) { { "Classic",F(13) { pi / 3, 0, pi23, pi43, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43, pi, pi43, 0, pi23 } }, FE },
				BA(30) {
					{ "Center", B(13) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "Bridges", B(13) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2 } },
					BE
				},
				CA(2) { { "NoChild", Fractal::TriComb_Param }, CE }
			), 
			new Fractal("Triflake", 4, 2, 1.5f, .175f, .8f, tfx, tfy,
				FA(2) { { "Center", F(4) { pi / 3, 0, 0, 0 } }, FE },
				BA(2) { { "Center", B(4) { 1, 0, 0, 0 } }, BE },
				CA(3) {
					{ "NoChild", Fractal::Triflake_NoCornerParam },
					{ "NoBackDiag", Fractal::Triflake_NoBackDiag },
					CE
				}
			),
			new Fractal("TetraTriflake", 16, 4, 1.5f, .15f, .8f, ttfx, ttfy,
				FA(2) { { "Classic", F(16) { SYMMETRIC + pi23, pi, pi + pi23, pi + pi43, 0, pi23, pi43, 0, pi23, pi43, 0, pi23, pi43, pi, pi + pi23, pi + pi43 } }, FE },
				BA(14) {
					{ "Rad", B(16) { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1 } },
					{ "Corner", B(16) {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0 } },
					{ "Triangle", B(16) {0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "Swirl", B(16) { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "Center", B(16) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "Center Rad", B(16) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1 } },
					{ "Center Corner", B(16) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0 } },
					{ "Center Triangle", B(16) { 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "Center Swirl", B(16) { 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "Center Rad 2", B(16) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2 } },
					{ "Center Corner 2", B(16) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0 } },
					{ "Center Triangle 2", B(16) { 1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					{ "Center Swirl 2", B(16) { 1, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
					BE
				}, CA(5) {
					{ "NoCorner", Fractal::Tetraflake_NoCornerParam },
					{ "NoCorner + RadHoles", Fractal::Tetraflake_NoCornerRadHolesParam },
					{ "NoCorner + CornerHoles", Fractal::Tetraflake_NoCornerCornerHolesParam },
					{ "NoCorner + TriangleHoles", Fractal::Tetraflake_NoCornerTriangleHolesParam },
					CE
				}
			), 
			new Fractal("SierpinskiCarpet", 9, 3, 1.0f, .175f, .9f, cx, cy,
					FA(17) {
						{ "Classic", F(9) { SYMMETRIC + pi, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "H-I De Rivera O (opposites)", F(9) { SYMMETRIC + pi, 0, 0, 0, pi / 2, 0, 0, 0, pi / 2 } },
						{ "H-I De Rivera H (coloreds)", F(9) { SYMMETRIC + pi, 0, pi / 2, 0, 0, 0, pi / 2, 0, 0 } },
						{ "H-I De Rivera OH", F(9) { SYMMETRIC + pi, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2 } },
						{ "H-I De Rivera X (corners)", F(9) { SYMMETRIC + pi, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0 } },
						{ "H-I De Rivera XO", F(9) { SYMMETRIC + pi, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2 } },
						{ "H-I De Rivera XH", F(9) { SYMMETRIC + pi, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0 } },
						{ "H-I De Rivera / (diagonals)", F(9) { SYMMETRIC + pi, pi / 2, 0, 0, 0, pi / 2, 0, 0, 0 } },
						{ "H-I De Rivera C", F(9) { SYMMETRIC + pi / 2, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "H-I De Rivera CO", F(9) { SYMMETRIC + pi / 2, 0, 0, 0, pi / 2, 0, 0, 0, pi / 2 } },
						{ "H-I De Rivera CH", F(9) { SYMMETRIC + pi / 2, 0, pi / 2, 0, 0, 0, pi / 2, 0, 0 } },
						{ "H-I De Rivera COH", F(9) { SYMMETRIC + pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2 } },
						{ "H-I De Rivera CX", F(9) { SYMMETRIC + pi / 2, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0 } },
						{ "H-I De Rivera CXO", F(9) { SYMMETRIC + pi / 2, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2 } },
						{ "H-I De Rivera CXH", F(9) { SYMMETRIC + pi / 2, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0 } },
						{ "H-I De Rivera C/", F(9) { SYMMETRIC + pi / 2, pi / 2, 0, 0, 0, pi / 2, 0, 0, 0 } },
						FE
					}, BA(3) {
						{ "Sierpinski Carpet",  B(9) { 1, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "H-I De Rivera",  B(9) { 0, 0, 1, 0, 0, 0, 1, 0, 0 } },
						BE
					}, CA(4) {
						{ "NoChild", Fractal::Carpet_NoCornerParam },
						{ "NoBackDiag", Fractal::Carpet_NoBackDiag },
						{ "NoBackDiag2", Fractal::Carpet_NoBackDiag2 },
					 } 
				),
				new Fractal("Pentaflake", 6, pfs, .2f * pfs, .15f, .9f, pfx, pfy,
					FA(3) { 
						{ "Classic",  F(6) { 2 * pi / 10, 0, 0, 0, 0, 0 } }, 
						{ "No Center Rotation",  F(6) { 2 * pi + 2 * pi / 5, 0, 0, 0, 0, 0 } }, 
						FE 
					}, BA(2) { { "Center", B(6) { 1, 0, 0, 0, 0, 0 } }, BE },
					CA(3) {
						{ "NoChild", Fractal::Pentaflake_NoCornerParam },
						{ "NoBackDiag", Fractal::Pentaflake_NoBackDiag },
						CE
					}
				),
				new Fractal("Hexaflake", 7, 3, .5f, .1f, 1,
					F(7) { 0, 0, 2 * stt, 2 * stt, 0, -2 * stt, -2 * stt },
					F(7) { 0, -2, -1, 1, 2, 1, -1 },
					FA(2) { { "Classic", F(7) { SYMMETRIC + pi / 3, 0, 0, 0, 0, 0, 0 } }, FE },
					BA(3) {
						{ "Center", B(7) { 1, 0, 0, 0, 0, 0, 0 } },
						{ "Y", B(7) { 1, 0, 1, 0, 1, 0, 1 } },
						BE
					}, CA(3) {
						{ "NoChild", Fractal::Hexaflake_NoCornerParam },
						{ "NoBackDiag", Fractal::Hexaflake_NoBackDiag },
						CE
					}
				),
				new Fractal("HexaCircular", 19, 5, .2f, .05f, .9f,
					F(19) { 0, 2 * stt, stt, -stt, -2 * stt, -stt, stt, 4 * stt, 3 * stt, stt, -stt, -3 * stt, -4 * stt, -4 * stt, -3 * stt, -stt, stt, 3 * stt, 4 * stt},
					F(19) { 0, 0, 1.5f, 1.5f, 0, -1.5f, -1.5f, 1, 2.5f, 3.5f, 3.5f, 2.5f, 1, -1, -2.5f, -3.5f, -3.5f, -2.5f, -1},
					FA(3) {
						{ "180", F(19) { pi / 3, 0, 0, 0, 0, 0, 0, 0, pi, pi, 0, 0, pi, pi, 0, 0, pi, pi, 0 } },
						{ "Symmetric", F(19) { SYMMETRIC + pi23, 0, 0, 0, 0, 0, 0, 0, pi, pi, 0, 0, pi, pi, 0, 0, pi, pi, 0 } },
						FE
					}, BA(5) {
						{ "Center", B(19) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "Center Y", B(19) { 1, 0, 2, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "Y", B(19) { 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "Double Y", B(19) { 0, 2, 1, 2, 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						BE
				}, nullptr
			), nullptr
		};
		int maxChildren = 1;
		for (Fractal** i = fractals; *i != nullptr; ++i)
			if ((*i)->childCount > maxChildren)
				maxChildren = (*i)->childCount;
		childColor = new uint8_t[maxChildren];
	}
	void FractalGenerator::DeleteEncoder() {
		if (gifEncoder != nullptr) {
			delete gifEncoder;
			gifEncoder = nullptr;
		}
	}
	void FractalGenerator::DeleteBuffer(const FractalTask& task) const {
		const auto& buffT = task.buffer;
		const auto& voidT = task.voidDepth;
		for (auto y = 0; y < allocatedHeight; ++y) {
			delete buffT[y];
			delete voidT[y];
		}
		delete[] buffT;
		delete[] voidT;
	}
	void FractalGenerator::NewBuffer(FractalTask& task) const {
		const auto& voidT = task.voidDepth = new int16_t * [selectHeight];
		const auto& buffT = task.buffer = new Vector * [selectHeight];
		for (auto y = 0; y < selectHeight; voidT[y++] = new int16_t[selectWidth]) {
			const auto& buffY = buffT[y] = new Vector[selectWidth];
			for (auto x = 0; x < selectWidth; buffY[x++] = zero);
		}
	}
	/*void FractalGenerator::InitBuffer(int16_t taskIndex) {
		const auto& buffT = buffer[taskIndex];
		for (auto y = 0; y < height; ++y) {
			const auto& buffY = buffT[y];
			for (auto x = 0; x < width; buffY[x++] = zero);
		}
	}*/
	
#pragma endregion

#pragma region Generate_Tasks
	void FractalGenerator::GenerateAnimation() {

#ifdef PARALLELDEBUG
		Debug::WriteLine("GENERATEANIMATION STARTER");
#endif

#ifdef CUSTOMDEBUG
		// Start a new DebugLog
		logString = "";
		Log(logString, "New Generate()");
		long long time;
		auto generateTime = std::chrono::steady_clock::now();
		startTime = &std::chrono::steady_clock::now();
#endif
		// Open a temp file to presave GIF to - Use xiaozhuai's GifEncoder
		exportingGif = gifSuccess = false;
		DeleteEncoder();
		if (selectEncode >= 2) {
			gifEncoder = new GifEncoder();
			uint8_t gifIndex = 0;
			while (gifIndex < 255) {
				gifTempPath = "gif" + std::to_string(gifIndex) + ".tmp";
				if (!((GifEncoder*)gifEncoder)->open(gifTempPath, selectWidth, selectHeight, 1, false, 0, selectWidth * selectHeight * 3)) {
#ifdef CUSTOMDEBUG
					Log(logString, "Error opening gif file: " + gifTempPath);
#endif
					++gifIndex;
					continue;
				} else break;
			}
			if (gifIndex == 255)
				DeleteEncoder();
		}
		// Initialize the starting default animation values
		auto size = 2400.0f, angle = selectDefaultAngle * (float)M_PI / 180.0f, hueAngle = selectDefaultHue / 120.0f;
		uint8_t color = 0;
		widthBorder = selectWidth - 2;
		heightBorder = selectHeight - 2;
		bloom1 = selectBloom + 1;
		upleftStart = -selectBloom;
		rightEnd = widthBorder + bloom1;
		downEnd = heightBorder + bloom1;
		ambnoise = selectAmbient * selectNoise;
		int8_t spin = static_cast<int8_t>(selectSpin < -1 ? random.Next(-1, 3) : selectSpin);
		ModFrameParameters(size, angle, spin, hueAngle, color);
		for (int i = selectDefaultZoom; 0 <= --i; IncFrameSize(size, selectPeriod));
		// Generate the images
		while (!cancelRequested && bitmapsFinished < frames) {

			// Initialize buffers (delete and reset if size changed)
			applyMaxGenerationTasks = selectMaxGenerationTasks;
			const int16_t batchTasks = std::max(static_cast<int16_t>(1), applyMaxTasks = selectMaxTasks);
			if (batchTasks != allocatedTasks) {
				if (allocatedTasks >= 0) {
					for (int t = 0; t < allocatedTasks; ++t) {
						auto& task = tasks[t];
						if (task.taskStarted) {
#ifdef PARALLELDEBUG
							Debug::WriteLine("GenerateAnimation task still running: " + t);
#endif
							Join(task);
						}
						DeleteBuffer(task);
						delete[] task.buffer;
						delete[] task.voidDepth;
					}
					delete[] tasks;
					delete[] tuples;
				}
				// rect = System::Drawing::Rectangle(0, 0, allocatedWidth = width, allocatedHeight = height);
				allocatedWidth = selectWidth;
				allocatedHeight = selectHeight;
				tasks = new FractalTask[(allocatedTasks = batchTasks) + 2];
				tuples = new std::tuple<uint16_t, std::pair<float, float>, std::pair<float, float>, uint8_t, int, uint8_t>[std::max(1, applyMaxGenerationTasks * 8)];
				for (int t = 0; t < allocatedTasks; ++t) {
					auto& task = tasks[t];
					task.voidDepth = new int16_t * [batchTasks];
					task.buffer = new Vector * [batchTasks];
					task.index = t;
					NewBuffer(task);
				}
				SetMaxIterations(true);
			}
			if (selectHeight != allocatedHeight) {
				for (int t = 0; t < batchTasks; ++t) {
					auto& task = tasks[t];
					DeleteBuffer(task);
					NewBuffer(task);
				}	
				// rect = System::Drawing::Rectangle(0, 0, allocatedWidth = width, allocatedHeight = height);
				allocatedHeight = selectHeight;
				allocatedWidth = selectWidth;
			}
			if (selectWidth != allocatedWidth) {
				for (int t = 0; t < batchTasks; ++t) {
					const auto& task = tasks[t];
					const auto& buffT = task.buffer;
					const auto& voidT = task.voidDepth;
					for (auto y = 0; y < allocatedHeight; voidT[y++] = new int16_t[selectWidth]) {
						delete buffT[y];
						delete voidT[y];
						buffT[y] = new Vector[selectWidth];
					}
					for (auto y = allocatedHeight; y < selectHeight; voidT[y++] = new int16_t[selectWidth])
						buffT[y] = new Vector[selectWidth];
				}
				// rect = System::Drawing::Rectangle(0, 0, allocatedWidth = width, height);
				allocatedWidth = selectWidth;
			}
			const auto generateLength = (selectEncode > 0 ? static_cast<int16_t>(frames) : static_cast<int16_t>(1));
			// Wait if no more frames to generate
			if (bitmapsFinished >= generateLength)
				continue;
			
			// Image parallelism
			applyParallelType = selectParallelType;
			//imageTasks = applyParallelType == 2 ? gcnew ConcurrentBag<Task^>() : nullptr;
			// Animation Parallelism Task Count
			auto animationTaskCount = applyParallelType > 0 ? static_cast<int16_t>(0) : std::min(batchTasks, generateLength);
			FinishTasks(true, applyParallelType > 0 || applyMaxTasks <= 2
						? [generateLength](int taskIndex) -> bool {
							if (nextBitmap >= generateLength)
								return false;
							ModFrameParameters(size, angle, spin, hueAngle, color);
							GenerateImage(nextBitmap++, -1, size, angle, spin, hueAngle, color);
							IncFrameParameters(size, angle, spin, hueAngle, 1);
							return true;
						}
						: [](int taskIndex) -> bool {
							if (nextBitmap >= generateLength)
								return false; // The task is finished, no need to wait for this one
							auto& task = tasks[taskIndex];
							ModFrameParameters(size, angle, spin, hueAngle, color);
							//var _task = taskIndex;float _size = size, _angle = angle, _hueAngle = hueAngle; var _spin = spin; var _color = color;
							task.Start(0);
							parallelTasks[taskIndex] = Task::Factory->StartNew(
								gcnew Action<Object^>(this, &FractalGenerator::Task_Animation),
								gcnew array<System::Object^>{ nextBitmap++, taskIndex, size, angle, spin, hueAngle, color }
							);
							IncFrameParameters(size, angle, spin, hueAngle, 1);
							return true; // A task finished, but started another one - keep checking before new master loop
						});


			/*if (animationTaskCount <= 1) {
				// No Animation Parallelism:
				if (nextBitmap < generateLength) {
					ModFrameParameters(size, angle, spin, hueAngle, color);
					GenerateImage(nextBitmap++, batchTasks, size, angle, spin, hueAngle, color);
					IncFrameParameters(size, angle, spin, hueAngle, 1);
					tasks[batchTasks].state = 2;
				}
				auto& gif = tasks[applyMaxGenerationTasks];
				if (gif.FinishTask())
					TryGif(gif);
			} else {
				// Animation parallelism: Continue/Finish Animation and Gif tasks
				bool tasksRemaining = true;
				while (tasksRemaining) {
					tasksRemaining = false;
					// Check every task
					for (int16_t t = 0; t < batchTasks; ++t) {
						auto& task = tasks[t];
						if (task.FinishTask()) {
							TryGif(task);
							if (task.state >= 2 && nextBitmap < generateLength && !cancelRequested && selectMaxTasks == applyMaxTasks) {
								// Start another task when previous was finished
								task.state = 0;
								ModFrameParameters(size, angle, spin, hueAngle, color);
								task.Start();
								task.task = std::thread(&FractalGenerator::GenerateImage, this, nextBitmap++, t, size, angle, spin, hueAngle, color);
								IncFrameParameters(size, angle, spin, hueAngle, 1);
								tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
							}
						} else tasksRemaining = true; // A task not finished yet - keep checking before new master loop
					}
				}
			}*/
		}
		// Wait for threads to finish
		for (int t = allocatedTasks; t >= 0; --t)
			if (tasks[t].state < 2 && tasks[t].task.joinable())
				tasks[t].Join();
		gifSuccess = false;
		if (selectEncode >= 2 && gifEncoder != nullptr && ((GifEncoder*)gifEncoder)->close(&cancelRequested))
			gifSuccess = true;
#ifdef CUSTOMDEBUG
		else Log(logString, "Error closing gif file.");
		const auto generateElapsed = (std::chrono::steady_clock::now() - generateTime).count();
		Log(logString, "Generate time = " + time + "\n");
		long long n = bitmap->Length * 100000;
		logString = "CppManaged:\nInit: " + initTimes / n
			+ "\nIter: " + iterTimes / n
			+ "\nVoid: " + voidTimes / n
			+ "\nDraw: " + drawTimes / n
			+ "\nGifs: " + gifTimes / n
			+ "\n" + logString;
		File::WriteAllText("log.txt", logString);
#endif
		DeleteEncoder();
	}
	void FractalGenerator::GenerateImage(const uint16_t& bitmapIndex, const int16_t& stateIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color) {
#ifdef CUSTOMDEBUG
		System::String^ threadString = "";
		const auto initTime{ std::chrono::steady_clock::now() };
#endif


		int16_t taskIndex = stateIndex % applyMaxTasks;
		auto& task = tasks[stateIndex],
			& state = tasks[stateIndex];
		auto& buffT = tasks[taskIndex].buffer;
		for (auto y = 0; y < selectHeight; ++y) {
			const auto& buffY = buffT[y];
			for (auto x = 0; x < selectWidth; buffY[x++] = zero);
		}
		auto& voidT = tasks[taskIndex].voidDepth;
#ifdef CUSTOMDEBUG
		const auto initElapsed = (std::chrono::steady_clock::now() - initTime).count();
		Log(threadString, "Init:" + bitmapIndex + " time = " + initElapsed);
		const auto iterTime{ std::chrono::steady_clock::now() };
#endif
		if (cancelRequested) {
			state.state = 2;
			return;
		}
		// Generate the fractal frame recursively
		//std::tuple<Vector**, Vector, Vector> R;
		auto& HR = task.H;
		auto & IR = task.I;
		for (int b = 0; b < selectBlur; ++b) {
			ModFrameParameters(size, angle, spin, hueAngle, color);
			// Preiterate values that change the same way as iteration goes deeper, so they only get calculated once
			float inSize = size;
			for (int i = 0; i < maxIterations; ++i) {
				auto& preIterated = preIterate[i];
				const auto inDetail = -inSize * std::max(-1.0f, std::get<1>(preIterated) = logf(selectDetail / (std::get<0>(preIterated) = inSize)) / logBase);
				auto& XY = std::get<2>(preIterated);
				if (XY == nullptr)
					XY = new std::pair<float, float>[f->childCount];
				for (int c = 0; c < f->childCount; ++c)
					XY[c] = std::pair<float, float>(f->childX[c] * inDetail, f->childY[c] * inDetail);
				inSize /= f->childSize;
			}
			// Prepare Color blending per one dot (hueshifting + iteration correction)
			// So that the color of the dot will slowly approach the combined colors of its childer before it splits
			auto lerp = std::fmod(hueAngle, 1.0f);
			//auto& H = std::get<1>(R);
			//auto& I = std::get<2>(R);
			switch ((uint8_t)hueAngle % 3) {
			case 0:
				IR = Vector::Lerp(unitX, unitY, lerp);
				HR = Vector::Lerp(*colorBlend, Y(*colorBlend), lerp);
				break;
			case 1:
				IR = Vector::Lerp(unitY, unitZ, lerp);
				HR = Vector::Lerp(Y(*colorBlend), Z(*colorBlend), lerp);
				break;
			case 2:
				IR = Vector::Lerp(unitZ, unitX, lerp);
				HR = Vector::Lerp(Z(*colorBlend), *colorBlend, lerp);
				break;
			}
			if (applyMaxGenerationTasks <= 1)
				GenerateDots_SingleTask(CHILDPARAMS);
			else switch (applyParallelType) {
			case 0:
				GenerateDots_SingleTask(CHILDPARAMS);
				break;
			case 1:
				tuples[0] = { CHILDPARAMS };
				GenerateDots_OfDepth();
				break;
			case 2:
				// TODO make this work
				GenerateDots_OfRecursion(CHILDPARAMS);
				/*
				if (imageTasks == nullptr)
					return;
				// Wait for image parallelism threads to complete
				bool noMore;
				while (true) {
					taskSnapshot->Clear();
					for each (Task ^ task in imageTasks)
						taskSnapshot->Add(task);
					for each (Task ^ task in taskSnapshot)
						task->Wait();
					noMore = true;
					for each (Task ^ task in imageTasks)
						if (!taskSnapshot->Contains(task))
							noMore = false;
					if (noMore)
						break;
					//Thread::Sleep(10);
				}*/
				break;
			}
			IncFrameParameters(size, angle, spin, hueAngle, selectBlur);
			if (cancelRequested) {
				state.state = 2;
				return;
			}
		}
#ifdef CUSTOMDEBUG
		const auto iterElapsed = (std::chrono::steady_clock::now() - iterTime).count();
		Log(threadString, "Iter:" + bitmapIndex + " time = " + iterElapsed);
		const auto voidTime{ std::chrono::steady_clock::now() };
#endif
		// Generate the grey void areas
		auto lightNormalizer = 0.1f, voidDepthMax = 1.0f;
		auto& queueT = task.voidQueue;
		int16_t voidYX;
		const auto w1 = selectWidth - 1, h1 = selectHeight - 1;
		lightNormalizer = 0.1f;
		// Old SIMD vector code I couldn't get to work
		//Vector<float> *buffY;
		int16_t* voidY, * void0, * voidH, * voidP, * voidM;
		if (selectAmbient > 0) {
			// Void Depth Seed points (no points, no borders), leet the void depth generator know where to start incrementing the depth
			float lightMax;
			for (uint16_t y = 1; y < h1; ++y) {
				if (cancelRequested) {
					state.state = 2;
					std::swap(queueT, std::queue<std::pair<int16_t, int16_t>>()); // Fast swap-and-drop
					return;
				}
				const auto buffY = buffT[y];
				voidY = voidT[y];
				for (uint16_t x = 1; x < w1; ++x) {
					auto& buffYX = buffY[x];
					// Old SIMD vector code I couldn't get to work
					//lightNormalizer = std::max(lightNormalizer, lightMax = std::max(buffYX[0], std::max(buffYX[1], buffYX[2])));
					lightNormalizer = std::max(lightNormalizer, lightMax = buffYX.Max());
					if (lightMax > 0) {
						voidY[x] = 0;
						queueT.push({ y, x });
					} else voidY[x] = -1;
				}
				voidY[0] = voidY[w1] = 0;
				queueT.push({ y, 0 });
				queueT.push({ y, w1 });
			}
			void0 = voidT[0];
			voidH = voidT[h1];
			for (uint16_t x = 0; x < selectWidth; ++x) {
				void0[x] = voidH[x] = 0;
				queueT.push({ 0, x });
				queueT.push({ h1, x });
			}
			// Depth of Void (fill the void of incrementally larger values of depth, that will generate the grey areas)
			int16_t x, y, ym, yp, xm, xp, voidMax = 0;
			std::pair<int16_t, int16_t> qt;
			while (!queueT.empty()) {
				qt = queueT.front();
				queueT.pop();
				y = qt.first, ym = y - 1, yp = y + 1, x = qt.second, xm = x - 1, xp = x + 1;
				voidY = voidT[y];
				voidMax = std::max(voidMax, voidYX = voidY[x] + 1);
				if (xp < selectWidth && (voidY[xp] == -1)) { voidY[xp] = voidYX; queueT.push({ y, xp }); }
				if (yp < selectHeight && ((voidP = voidT[yp])[x] == -1)) { voidP[x] = voidYX;  queueT.push({ yp, x }); }
				if (xm >= 0 && (voidY[xm] == -1)) { voidY[xm] = voidYX; queueT.push({ y, xm }); }
				if (ym >= 0 && ((voidM = voidT[ym])[x] == -1)) { voidM[x] = voidYX;  queueT.push({ ym, x }); }
			}
			voidDepthMax = voidMax;
		} else
			for (uint16_t y = 0; y < selectHeight; y++) {
				if (cancelRequested) {
					state.state = 2;
					return;
				}
				const auto buffY = buffT[y];
				for (uint16_t x = 0; x < selectWidth; x++) {
					auto& buffYX = buffY[x];
					// Old SIMD vector code I couldn't get to work
					//lightNormalizer = std::max(lightNormalizer, std::max(buffYX[0], std::max(buffYX[1], buffYX[2])));
					lightNormalizer = std::max(lightNormalizer, buffYX.Max());
				}
			}
		lightNormalizer = 160.0f / lightNormalizer;
		if (cancelRequested) {
			state.state = 2;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto voidElapsed = (std::chrono::steady_clock::now() - voidTime).count();
		Log(threadString, "Void:" + bitmapIndex + " time = " + voidElapsed);
		const auto drawTime{ std::chrono::steady_clock::now() };
#endif
		// Draw the generated pixel to bitmap data
		// Make a locked bitmap, remember the locked state
		/*uint8_t* p = (uint8_t*)(void*)((bitmapData[bitmapIndex] = (bitmap[bitmapIndex] = gcnew Bitmap(width, height))->LockBits(rect,
			ImageLockMode::WriteOnly,
			System::Drawing::Imaging::PixelFormat::Format24bppRgb))->Scan0);*/
		uint8_t* p = bitmap[bitmapIndex];
		bitmapState[bitmapIndex] = 1;
		// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
		// Switch between th selected settings such as saturation, noise, image parallelism...
		const auto& cbuffT = const_cast<const Vector**&>(buffT);
		const auto& cvoidT = const_cast<const int16_t**&>(voidT);
		if (applyParallelType > 0 && applyMaxGenerationTasks > 1) {
			// Multi Threaded:
			const auto stride = 3 * selectWidth;
			if (ambnoise > 0) {
				if (seletSaturate > 0.0) {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancelRequested)
							continue;
						auto rp = p + y * stride;
						NoiseSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancelRequested)
							continue;
						uint8_t* rp = p + y * stride;
						NoiseNoSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				}
			} else {
				if (seletSaturate > 0.0) {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancelRequested)
							continue;
						auto rp = p + y * stride;
						NoNoiseSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancelRequested)
							continue;
						auto rp = p + y * stride;
						NoNoiseNoSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				}
			}
		} else {
			// Single Threaded:
			if (ambnoise > 0) {
				if (seletSaturate > 0.0) for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancelRequested)
						continue;
					NoiseSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				} else for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancelRequested)
						continue;
					NoiseNoSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				}
			} else {
				if (seletSaturate > 0.0) for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancelRequested)
						continue;
					NoNoiseSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				} else for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancelRequested)
						continue;
					NoNoiseNoSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				}
			}
		}
#ifdef CUSTOMDEBUG
		const auto drawElapsed = (std::chrono::steady_clock::now() - drawTime).count();
		Log(threadString, "Draw:" + bitmapIndex + " time = " + voidElapsed);

		Monitor::Enter(taskLock);
		try {
			initTimes += initElapsed;
			iterTimes += iterElapsed;
			voidTimes += drawElapsed;
			drawTimes += drawElapsed;
			Log(logString, threadString);
		} finally { Monitor::Exit(taskLock); }
#endif
		// Lets the generator know it's finished so it can encode gif it it's allowed to, if not it just lets the form know it can diplay it
		bitmapState[bitmapIndex] = 2;
		state.state = 1;
	}
	void FractalGenerator::GenerateDots_SingleTask(const uint16_t taskIndex, const std::pair<float, float> inXY, const std::pair<float, float> inAngle, const uint8_t inColor, const int inFlags, uint8_t inDepth) {
		IFAPPLIEDDOT return;
		STARTITERATION{
			ITERATECHILDREN GenerateDots_SingleTask(NEWCHILD);
		}
	}
	void FractalGenerator::GenerateDots_OfDepth() {
		uint16_t index = 0, insertTo = 1, max = applyMaxGenerationTasks * 8, maxcount = max - f->childCount - 1;
		int16_t count = (max + insertTo - index) % max;
		while (count > 0 && count < maxcount) {
			const auto& params = tuples[index++];
			const uint16_t& taskIndex = std::get<0>(params);
			const std::pair<float, float>& inXY = std::get<1>(params);
			const std::pair<float, float>& inAngle = std::get<2>(params);
			const uint8_t& inColor = std::get<3>(params);
			const int& inFlags = std::get<4>(params);
			uint8_t inDepth = std::get<5>(params);
			IFAPPLIEDDOT continue;
			STARTITERATION{
				ITERATECHILDREN {
					tuples[insertTo++] = { NEWCHILD };
					insertTo %= max;
				}
			}
			count = (max + insertTo - index) % max;
		}
		while (count > 0 && !cancelRequested) {
			// more parallels to compute
			bool tasksRemaining = true;
			while (tasksRemaining) {
				auto& gif = tasks[applyMaxGenerationTasks];
				if (FinishTask(gif))
					TryGif(gif);
				tasksRemaining = false;
				// Check every task
				for (int16_t t = 0; t < applyMaxGenerationTasks; ++t) {
					auto& task = tasks[t];
					if (FinishTask(task)) {
						if (task.state >= 2 && count > 0 && !cancelRequested) {
							// Start another task when previous was finished
							task.state = 0;
							Start(task);
							task.task = std::thread(&FractalGenerator::Task_OfDepth, this, t, index++);
							index %= max;
							count = (max + insertTo - index) % max;
							tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
						}
					} else tasksRemaining = true; // A task not finished yet - keep checking before new master loop
				}
			}
		}
	}
	void FractalGenerator::GenerateDots_OfRecursion(const uint16_t taskIndex, const std::pair<float, float> inXY, const std::pair<float, float> inAngle, const uint8_t inColor, const int inFlags, uint8_t inDepth) {
		IFAPPLIEDDOT return;
		STARTITERATION{
			ITERATECHILDREN {
				/*if (imageTasks != nullptr && imageTasks->Count < applyMaxGenerationTasks && inDepth < maxDepth)
					imageTasks->Add(Task::Factory->StartNew(
						gcnew Action<Object^>(this, &FractalGenerator::Task_OfRecursion),
						gcnew array<System::Object^>{ R, NEWCHILD }
					));
				else*/ GenerateDots_OfRecursion(NEWCHILD);
			}
		}
	}
	void FractalGenerator::GenerateGif(const int16_t taskIndex) {
		auto& task = tasks[taskIndex];
		// Sequentially encode a finished GIF frame, then unlock the bitmap data
		if (cancelRequested) {
			task.state = 2;
			return;
		}
#ifdef CUSTOMDEBUG
		std::string threadString = "";
		const auto gifTime{ std::chrono::steady_clock::now() };
#endif
		// Save Frame to a temp GIF
		if (selectEncode >= 2
			&& !cancelRequested
			&& gifEncoder != nullptr
			&& !((GifEncoder*)gifEncoder)->push(&cancelRequested, GifEncoder::PIXEL_FORMAT_BGR, bitmap[bitmapsFinished], selectWidth, selectHeight, selectDelay)) {
#ifdef CUSTOMDEBUG
			Log(threadString, "Error writing gif frame.");
#endif
			DeleteEncoder();
		}
#ifdef CUSTOMDEBUG
		const auto gifElapsed = (std::chrono::steady_clock::now() - gifTime).count();
		Log(threadString, "Gifs:" + bitmapsFinished + " time = " + gifElapsed);
		Monitor::Enter(taskLock);
		try {
			gifTimes += gifElapsed;
			Log(logString, threadString);
		} finally {
			Monitor::Exit(taskLock);
		}
#endif
		//Save BMP to RAM
		//bitmap[bitmapsFinished]->UnlockBits(bitmapData[bitmapsFinished]); // Lets me generate next one, and lets the GeneratorForm know that this one is ready
		bitmapState[bitmapsFinished++] = 4;
		exportingGif = false;
		task.state = 1;
	}
	void FractalGenerator::FinishTasks(bool includingGif, const std::function<bool(int)>& lambda) {
		for (auto tasksRemaining = true; tasksRemaining; MakeDebugString()) {
			tasksRemaining = false;
			for (short t = applyMaxTasks; 0 <= --t; ) {
				FractalTask& task = tasks[t];
				if (IsStillRunning(task)) tasksRemaining |= includingGif || task.type == 0; // Task not finished yet
				else if (!cancel->Token.IsCancellationRequested && selectMaxTasks == applyMaxTasks) {
					if (TryGif(task)) tasksRemaining |= lambda(t);
					else if (includingGif) tasksRemaining = true;
				}
			}
		}
	}
	void FractalGenerator::TryGif(FractalTask& task) {
		if (bitmapState[bitmapsFinished] != 2)
			return;
		if (gifEncoder != nullptr 
			&& !exportingGif 
			&& !cancelRequested 
			&& selectMaxTasks == applyMaxTasks
		) {
			exportingGif = true;
			task.state = 0;
			bitmapState[bitmapsFinished] = 3;
			task.Start();
			task.task = std::thread(&FractalGenerator::GenerateGif, this, task.index);
		} else {
			//bitmap[bitmapsFinished]->UnlockBits(bitmapData[bitmapsFinished]);
			bitmapState[bitmapsFinished++] = 4;
		}
	}
#pragma endregion

#pragma region Generate_Inline
	bool FractalGenerator::ApplyDot(const bool apply, const FractalTask& task, const std::pair<float, float>& inXY, const float& inDetail, const uint8_t& inColor) const {
		if (apply) {
			//Vector<float> dotColor = (1 - lerp) * colorBlendH + lerp * colorBlendI;
			Vector dotColor = Vector::Lerp(task.H, task.I, inDetail);
			switch (inColor) {
			case 1: dotColor = Y(dotColor); break;
			case 2: dotColor = Z(dotColor); break;
			}
			// Iterated deep into a single point - Interpolate inbetween 4 pixels and Stop
			const auto& buffT = task.buffer;
			const auto& inX = inXY.first, inY = inXY.second;
			const auto startX = std::max(static_cast<int16_t>(1), static_cast<int16_t>(floor(inX - selectBloom))),
				endX = std::min(widthBorder, static_cast<int16_t>(ceil(inX + selectBloom))),
				endY = std::min(heightBorder, static_cast<int16_t>(ceil(inY + selectBloom)));
			for (int16_t x, y = std::max(static_cast<int16_t>(1), static_cast<int16_t>(floor(inY - selectBloom))); y <= endY; ++y) {
				const auto yd = bloom1 - abs(y - inY);
				const auto& buffY = buffT[y];
				for (int16_t y, x = startX; x <= endX; ++x)
					buffY[x] += (yd * (bloom1 - abs(x - inX))) * dotColor; //buffT[y][x] += Vector<float>((1.0f - abs(x - inX)) * (1.0f - abs(y - inY))) * dotColor;
			}
			return true;
		}
		return false;
	}
	Vector FractalGenerator::Normalize(const Vector& pixel, const float lightNormalizer) {
		const float max = pixel.Max();
		return lightNormalizer * max > 254.0f ? (254.0f / max) * pixel : lightNormalizer * pixel;
	}
	// old code that should work with SIMD vectors but couldn't get it to work
	/*inline void FractalGenerator::ApplyAmbientNoise(Vector<float>& rgb,  const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen) {
		rgb += Noise * Vector<float>(gcnew array<float>(3) { dist(randGen), dist(randGen), dist(randGen)}) + Vector<float>(amb);
	}
	inline void FractalGenerator::ApplySaturate(Vector<float>& rgb, const float d, uint8_t*& p) {
		float m; const float min = std::min(std::min(rgb[0], rgb[1]), rgb[2]), max = std::max(std::max(rgb[0], rgb[1]), rgb[2]);
		return max <= min ? rgb : ((m = max * saturate / (max - min)) + 1 - saturate) * rgb - Vector<float>(min * m);
	}
	inline void FractalGenerator::ApplyNoSaturate(Vector<float>& rgb, uint8_t*& p) {
		p[0] = static_cast<uint8_t>(rgb[2]);	// Blue
		p[1] = static_cast<uint8_t>(rgb[1]);	// Green
		p[2] = static_cast<uint8_t>(rgb[0]);	// Red
		p += 3;
	}*/
	Vector& FractalGenerator::ApplyAmbientNoise(Vector& rgb, const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen) {
		rgb.X += Noise * dist(randGen) + Amb;
		rgb.Y += Noise * dist(randGen) + Amb;
		rgb.Z += Noise * dist(randGen) + Amb;
		return rgb;
	}
	Vector FractalGenerator::ApplySaturate(const Vector& rgb) {
		// The saturation equation boosting up the saturation of the pixel (powered by the saturation slider setting)
		float m; const auto min = std::min(std::min(rgb.X, rgb.Y), rgb.Z), max = std::max(std::max(rgb.X, rgb.Y), rgb.Z);
		return max <= min ? rgb : Vector::MultiplyMinus(rgb, (m = max * seletSaturate / (max - min)) + 1 - seletSaturate, min * m);
	}
	void FractalGenerator::ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p) {
		// Without gamma:
		p[0] = static_cast<uint8_t>(rgb.Z);	// Blue
		p[1] = static_cast<uint8_t>(rgb.Y);	// Green
		p[2] = static_cast<uint8_t>(rgb.X);	// // With gamma:
		// With gamma:
			/*
			p[0] = static_cast<uint8_t>(255.0f * Math.Sqrt(rgb.Z / 255.0f));
			p[1] = static_cast<uint8_t>(255.0f * Math.Sqrt(rgb.Y / 255.0f));
			p[2] = static_cast<uint8_t>(255.0f * Math.Sqrt(rgb.X / 255.0f));
			*/
		p += 3;
	}
	inline void FractalGenerator::NoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0)
			for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), p));
		else {
			std::random_device rd;
			std::mt19937 randGen(rd());
			std::uniform_real_distribution<float> dist(0.0f, ambnoise);
			for (uint16_t x = 0; x < selectWidth; ++x) {
				const auto voidAmb = voidY[x] / voidDepthMax;
				//auto rgb = ApplySaturate(lightNormalizer * buffY[x]);
				ApplyRGBToBytePointer(ApplyAmbientNoise(ApplySaturate(Normalize(buffY[x], lightNormalizer)), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb, dist, randGen), p);
			}
		}
	}
	inline void FractalGenerator::NoNoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0) for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), p));
		else for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x++] / voidDepthMax) + ApplySaturate(Normalize(buffY[x], lightNormalizer)), p));

	}
	inline void FractalGenerator::NoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0)
			for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), p));
		else {
			std::random_device rd;
			std::mt19937 randGen(rd());
			std::uniform_real_distribution<float> dist(0.0f, ambnoise);
			for (uint16_t x = 0; x < selectWidth; x++) {
				const auto voidAmb = voidY[x] / voidDepthMax;
				ApplyRGBToBytePointer(ApplyAmbientNoise(Normalize(buffY[x], lightNormalizer), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb, dist, randGen), p);
			}
		}
	}
	inline void FractalGenerator::NoNoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0) 
			for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), p));
		else for (uint16_t x = 0; x < selectWidth; x++)
			ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x] / voidDepthMax) + Normalize(buffY[x], lightNormalizer), p);
	}
#pragma endregion

#pragma region TaskWrappers
	void FractalGenerator::Task_OfDepth(const int16_t taskIndex, const uint16_t tupleIndex) {
		const auto& params = tuples[tupleIndex];
		GenerateDots_SingleTask(std::get<0>(params), std::get<1>(params), std::get<2>(params), std::get<3>(params), std::get<4>(params), std::get<5>(params));
		tasks[taskIndex].state = 1;
	}
#pragma endregion

#pragma region AnimationParameters
	void FractalGenerator::SwitchParentChild(float& angle, int8_t& spin, uint8_t& color) {
		if (abs(spin) > 1) {
			// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
			angle = -angle;
			spin = -spin;
		}
		color = (3 + color + selectZoom * childColor[0]) % 3;
	}
	void FractalGenerator::ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color) {
		const auto w = std::max(selectWidth, selectHeight) * f->maxSize, fp = f->childSize;
		// Zoom Rotation
		while (angle > 2 * M_PI)
			angle -= 2 * (float)M_PI;
		while (angle < 0)
			angle += 2 * (float)M_PI;
		// Zom Hue Cycle
		while (hueAngle >= 3)
			hueAngle -= 3;
		while (hueAngle < 0)
			hueAngle += 3;
		// Swap Parent<->CenterChild after a full period
		while (size >= w * fp) {
			size /= fp;
			angle += childAngle[0];
			SwitchParentChild(angle, spin, color);
		}
		while (size < w) {
			size *= fp;
			angle -= childAngle[0];
			SwitchParentChild(angle, spin, color);
		}
	}
	void FractalGenerator::IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const int16_t blur) {
		const auto blurPeriod = selectPeriod * blur;
		// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
		angle += spin * (periodAngle * (1 + selectExtraSpin)) / (finalPeriodMultiplier * blurPeriod);
		hueAngle += (hueCycleMultiplier + 3 * selectExtraHue) * (float)hueCycle / (finalPeriodMultiplier * blurPeriod);
		IncFrameSize(size, blurPeriod);
	}
#pragma endregion

#pragma region Interface_Calls
	void FractalGenerator::StartGenerate() {
		// start the generator in a separate main thread so that the form can continue being responsive
		mainTask = std::thread(&FractalGenerator::GenerateAnimation, this);
	}
	void FractalGenerator::ResetGenerator() {
		applyZoom = (short)(selectZoom != 0 ? selectZoom : random.NextDouble() < .5f ? -1 : 1);
		applyCutparam = selectCutparam < 0 ? (short)random.Next(0, cutparamMaximum) : selectCutparam;
		SetupColor();
		finalPeriodMultiplier = GetFinalPeriod();
		// A complex expression to calculate the minimum needed hue shift speed to match the loop:
		//hueCycleMultiplier = hueCycle == 0 ? 0 : ((childColor[0] % 3) == 0 ? 3 : ((((childColor[0] % 3) == 1) == (1 == hueCycle) == (zoom == 1) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3) + 1);
		hueCycleMultiplier = applyHueCycle == 0 ? 0 : childColor[0] % 3 == 0 ? 3 : 1 +
			(childColor[0] % 3 == 1 == (1 == applyHueCycle) == (1 == selectZoom) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3;
		// setup bitmap data
		bitmapsFinished = nextBitmap = 0;
		frames = debug > 0 ? debug : selectPeriod * finalPeriodMultiplier;
		if (allocatedFrames != frames) {
			if (allocatedFrames >= 0)
				delete[] bitmap;
			bitmap = new uint8_t * [allocatedFrames = frames];
		}
		//bitmapData = new uint8_t[BitmapData];
		bitmapState.reserve(frames + 1);
		for (int i = static_cast<int>(bitmapState.size()); 0 <= --i; bitmapState[i] = 0 );
		for (int i = static_cast<int>(bitmapState.size()) - 1; ++i <= frames; bitmapState.push_back(0));
		
	}
	void FractalGenerator::RequestCancel() {
		cancelRequested = true;
		if (mainTask.joinable())
			mainTask.join();
		cancelRequested = false;
	}
	bool FractalGenerator::SaveGif() {
		/*try {
			// Try to save (move the temp) the gif file
			if (gifEncoder != nullptr)
				((GifEncoder*)gifEncoder)->close(&cancel);
			File::Move(gcnew String(gifTempPath.c_str()), gcnew String(gifPath));
		} catch (IOException^ ex) {
			System::String^ exs = "SaveGif: An error occurred: " + ex->Message;
#ifdef CUSTOMDEBUG
			Log(logString, exs);
#endif
			return true;
		} catch (UnauthorizedAccessException^ ex) {
			System::String^ exs = "SaveGif: Access denied: " + ex->Message;
#ifdef CUSTOMDEBUG
			Log(logString, exs);
#endif
			return true;
		} catch (Exception^ ex) {
			System::String^ exs = "SaveGif: Unexpected error: " + ex->Message;
#ifdef CUSTOMDEBUG
			Log(logString, exs);
#endif
			return true;
		}*/
		return gifSuccess = false;
	}
	void FractalGenerator::DebugStart() {
		// debug for testing, starts the generator with predetermined setting for easy breakpointing
		// Need to enable the definition on top GeneratorForm.cpp to enable this
		// if enabled you will be unable to use the settings interface!
		SelectFractal(2);
		SetupFractal();
		SelectThreadingDepth();
		selectDetail = .25f;
		debug = 7;
		selectWidth = 80;
		selectHeight = 80;
		selectParallelType = 1;
		selectMaxTasks = 0;
		maxDepth = -1;//= 2;
		selectMaxGenerationTasks = selectMaxTasks = -1;// 10;
		selectSaturate = 1.0f;
	}
#pragma endregion

#pragma region Interface_Settings
	bool FractalGenerator::SelectFractal(const uint16_t select) {
		if (this->selectFractal == select)
			return true;
		// new fractal definition selected - let the form know to reset and restart me
		this->selectFractal = select;
		selectCut = selectChildAngle = selectChildColor = 0;
		return false;
	}
	void FractalGenerator::SetupFractal() {
		f = fractals[selectFractal];
		logBase = (float)Math::Log(f->childSize);
		SetMaxIterations(false);
	}
	void FractalGenerator::SetMaxIterations(bool forcedReset) {
		int16_t newMaxIterations = 2 + (int16_t)(Math::Ceiling(Math::Log(Math::Max(selectWidth, selectHeight) * f->maxSize / selectDetail) / logBase));
		if (newMaxIterations <= maxIterations && !forcedReset) {
			maxIterations = newMaxIterations;
			return;
		}
		maxIterations = newMaxIterations;
		for (int t = 0; t < allocatedTasks; ++t) {
			auto& preIterateTask = tasks[t].preIterate;
			preIterateTask = new std::tuple<float, float, std::pair<float, float>*>[maxIterations];
			for (int i = 0; i < maxIterations; preIterateTask[i++] = { 0.0f, 0.0f, nullptr });
		}
	}
	void FractalGenerator::SetupAngle() {
		childAngle = f->childAngle == nullptr ? new float[1] { (float)M_PI } : f->childAngle[selectChildAngle].second;
		periodAngle = f->childCount <= 0 ? 0.0f : std::fmod(childAngle[0], 2.0f * (float)M_PI);
	}
	void FractalGenerator::SetupColor() {
		// Unpack the color palette and hue cycling
		if (selectHue < 0) {
			applyHueCycle = (short)random.Next(-1, 2);
			applyColorPalette = (short)random.Next(0, 2);
		} else {
			applyHueCycle = (short)((selectHue / 2 + 1) % 3 - 1);
			applyColorPalette = (short)(selectHue % 2);
		}
		// Setup colors
		if (f->childCount > 0) {
			for (int i = f->childCount; 0 <= --i; childColor[i] = f->childColor[selectChildColor].second[i]);
			// Setup palette
			for (int i = 0; i < f->childCount; ++i)
				childColor[i] = applyColorPalette == 0 ? childColor[i] : (3 - childColor[i]) % 3;
		}
		// Prepare subiteration color blend
		float* colorBlendF = new float[3];
		colorBlendF[0] = colorBlendF[1] = colorBlendF[2] = 0;
		for (auto i = f->childCount; 0 <= --i; colorBlendF[childColor[i]] += 1.0f / f->childCount);
		colorBlend = new Vector(colorBlendF[0], colorBlendF[1], colorBlendF[2]);
	}
	bool FractalGenerator::SelectCutFunction(const uint16_t selectCut) {
		if (this->selectCut == selectCut)
			return true;
		this->selectCut = selectCut;
		return false;
	}
#pragma endregion

#pragma region Interface_Getters
	uint16_t FractalGenerator::GetFinalPeriod() {
		// get the multiplier of the basic period required to get to a seamless loop
		uint16_t m = hueCycle == 0 && childColor[0] > 0 ? selectPeriodMultiplier * 3 : selectPeriodMultiplier;
		return std::abs(selectSpin) > 1 || (selectSpin == 0 && childAngle[0] < 2 * M_PI) ? 2 * m : m;
	}

	/*std::string FractalGenerator::ConvertToStdString(System::String^ managedString) {
		using namespace System::Runtime::InteropServices;
		const auto chars = (const char*)(Marshal::StringToHGlobalAnsi(managedString)).ToPointer();
		const auto nativeString(chars);
		Marshal::FreeHGlobal(System::IntPtr((void*)chars));
		return nativeString;
	}*/
#pragma endregion

}
