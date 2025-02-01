//#define PARALLELDEBUG

#include "FractalGenerator.h"
#include <omp.h>
#include "GifEncoder.h"

#ifdef CUSTOMDEBUG
#include <iostream>
#include <fstream>
#endif

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
#define CHILDPARAMS std::pair<float,float>(selectWidth * .5f, selectHeight * .5f), std::pair<float,float>(angle, abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, static_cast<uint8_t>(0)
#define IFAPPLIEDDOT const auto& preIterated = task.preIterate[inDepth];const auto& inSize = std::get<0>(preIterated);\
	if (ApplyDot(inSize < selectDetail, task, inXY, std::get<1>(preIterated), inColor))
#define STARTITERATION const auto& newPreIterated = task.preIterate[++inDepth];auto i = f->childCount; while (0 <= --i)
#define ITERATECHILDREN if (cancelRequested) return;\
	const auto newFlags = CalculateFlags(i, inFlags);if (newFlags < 0) continue;\
	const auto& XY = std::get<2>(preIterated)[i];const auto cs = cos(inAngle.first), sn = sin(inAngle.first);\
	const std::pair<float, float> newXY = std::pair<float, float>(inXY.first + XY.first * cs - XY.second * sn, inXY.second - XY.second * cs - XY.first * sn);\
	if (TestSize(newXY, inSize))
#define NEWCHILD newXY, i == 0\
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

		std::random_device rd;
		std::mt19937 randGenerator(rd());
		std::uniform_real_distribution<float> randomDist(0.0f, 1.0f);

		selectCutparam = debug = selectDefaultHue = selectDefaultZoom = selectDefaultAngle = selectExtraSpin = selectExtraHue = 0;
		selectZoom = 1;
		selectFractal = selectChildColor = selectChildAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks = allocatedFrames = selectSpin = maxIterations = -1;
		selectParallelType = ParallelType::OfAnimation;
		selectGenerationType = GenerationType::EncodeGIF;
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
		startTime = &std::chrono::steady_clock::now();
#endif
		// Open a temp file to presave GIF to - Use xiaozhuai's GifEncoder
		gifSuccess = false;
		DeleteEncoder();
		if ((applyGenerationType = selectGenerationType) >= GenerationType::EncodeGIF) {
			GifEncoder* e;
			gifEncoder = e = new GifEncoder();
			uint8_t gifIndex = 0;
			while (gifIndex < 255) {
				gifTempPath = "gif" + std::to_string(gifIndex) + ".tmp";
				if (!e->open_parallel(gifTempPath, selectWidth, selectHeight, 1, 0, false, &cancelRequested)) {
#ifdef CUSTOMDEBUG
					Log(logString, "Error opening gif file: " + gifTempPath);
#endif
					++gifIndex;
					continue;
				} else break;
			}
			if (gifIndex == 255)
				DeleteEncoder();
			else if (selectAmbient < 0)
				e->setTransparent(0, 0, 0);
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
		int8_t spin = static_cast<int8_t>(selectSpin < -1 ? (int8_t)(randomDist(randomGenerator) * 4 - 1) : selectSpin);
		ModFrameParameters(size, angle, spin, hueAngle, color);
		applyBlur = (short)(selectBlur + 1);
		for (int i = selectDefaultZoom < 0 
			 ? (int8_t)(randomDist(randomGenerator) * selectPeriod * finalPeriodMultiplier) 
			 : (selectDefaultZoom % (selectPeriod * finalPeriodMultiplier)); 
			 0 <= --i; IncFrameSize(size, selectPeriod));
		// Generate the images
		for (int i = selectDefaultZoom; 0 <= --i; IncFrameSize(size, selectPeriod));
		// Generate the images
		while (!cancelRequested && bitmapsFinished < frames) {
			// Initialize buffers (delete and reset if size changed)
			if ((applyMaxTasks = std::max(static_cast<int16_t>(1), selectMaxTasks)) != allocatedTasks) {
				if (allocatedTasks >= 0) {
					for (int t = 0; t < allocatedTasks; ++t) {
						auto& task = tasks[t];
						if (task.taskStarted) {
#ifdef PARALLELDEBUG
							Debug::WriteLine("GenerateAnimation task still running: " + t);
#endif
							task.Join();
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
				tasks = new FractalTask[allocatedTasks = applyMaxTasks];
				tuples = new std::tuple<uint16_t, std::pair<float, float>, std::pair<float, float>, uint8_t, int, uint8_t>[applyMaxTasks * 8];
				for (int t = 0; t < applyMaxTasks; ++t) {
					auto& task = tasks[t];
					task.taskIndex = t;
					NewBuffer(task);
				}
				SetMaxIterations(true);
			}
			if (selectHeight != allocatedHeight) {
				for (int t = 0; t < applyMaxTasks; ++t) {
					auto& task = tasks[t];
					DeleteBuffer(task);
					NewBuffer(task);
				}	
				// rect = System::Drawing::Rectangle(0, 0, allocatedWidth = width, allocatedHeight = height);
				allocatedHeight = selectHeight;
				allocatedWidth = selectWidth;
			}
			if (selectWidth != allocatedWidth) {
				for (int t = 0; t < applyMaxTasks; ++t) {
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
			const auto generateLength = (applyGenerationType > GenerationType::OnlyImage
										 ? static_cast<int16_t>(frames) : static_cast<int16_t>(1));
			// Wait if no more frames to generate
			if (bitmapsFinished >= generateLength)
				continue;
			
			applyParallelType = applyGenerationType == GenerationType::OnlyImage ? ParallelType::OfDepth : selectParallelType;
			// Image parallelism
			//imageTasks = applyParallelType == 2 ? gcnew ConcurrentBag<Task^>() : nullptr;
			// The other implementation are using the FinishTasks lambda argument function, but I couldn't get that to work in this managed class, so I had to unpack it:

			for (auto tasksRemaining = true; tasksRemaining; MakeDebugString()) {
				TryFinishBitmaps();
				while (bitmapsFinished < frames && bitmapState[bitmapsFinished] >= (applyGenerationType >= GenerationType::EncodeGIF ? BitmapState::Finished : BitmapState::Encoding)) {
					//bitmap[bitmapsFinished]->UnlockBits(bitmapData[bitmapsFinished]); // TODO return this!
					++bitmapsFinished;
				}
				tasksRemaining = false;
				for (short taskIndex = applyMaxTasks; 0 <= --taskIndex; ) {
					FractalTask& task = tasks[taskIndex];
					if (task.IsTaskStillRunning()) tasksRemaining = true;
					else if (!cancelRequested && selectMaxTasks == applyMaxTasks) {
						if (applyParallelType > 0 || applyMaxTasks <= 2) {
							if (nextBitmap >= generateLength)
								continue;
							ModFrameParameters(size, angle, spin, hueAngle, color);
							GenerateDots(nextBitmap++, -taskIndex, size, angle, spin, hueAngle, color);
							IncFrameParameters(size, angle, spin, hueAngle, 1);
							tasksRemaining = true;
						} else {
							if (nextBitmap >= generateLength)
								continue;
							ModFrameParameters(size, angle, spin, hueAngle, color);
							const auto& bmp = nextBitmap++;
							task.Start(bmp).task = std::thread(&FractalGenerator::GenerateDots, this, bmp, taskIndex, size, angle, spin, hueAngle, color);
							IncFrameParameters(size, angle, spin, hueAngle, 1);
							tasksRemaining = true;
						}
					}
				}
			}
		}
		// Wait for threads to finish
		for (int t = allocatedTasks; t >= 0; --t)
			if (tasks[t].state < 2 && tasks[t].task.joinable())
				tasks[t].Join();
		// (Bitmaps get unlocked in the Form)
		
		//TestEncoder();

		// Save the temp GIF file
		gifSuccess = false;
		if (applyGenerationType >= GenerationType::EncodeGIF && gifEncoder != nullptr && ((GifEncoder*)gifEncoder)->close(false, &cancelRequested))
			while (/*!cancel->Token.IsCancellationRequested &&*/ gifEncoder != nullptr) {
				auto& encoder = *((GifEncoder*)gifEncoder);
				switch (encoder.tryWrite()) {
				case GifEncoderTryWriteResult::Failed:
					DeleteEncoder();
					break;
				case GifEncoderTryWriteResult::Waiting:
					// This should never be hit, because the coe reches this loop only after getting cancelled or every frame finished
					//Thread::Sleep(100);
					break;
				case GifEncoderTryWriteResult::FinishedAnimation:
					gifSuccess = true;
					// This will follow with gifEncoder.IsFinished()
					break;
				}
				if (encoder.isFinishedAnimation())
					break;
			}
#ifdef CUSTOMDEBUG
		else Log(logString, "Error closing gif file.");
		const auto generateElapsed = (std::chrono::steady_clock::now() - *startTime).count();
		Log(logString, "Generate time = " + std::to_string(generateElapsed) + "\n");
		long long n = frames * 100000;
		logString = "CppManaged:\nInit: " + std::to_string(initTimes / n)
			+ "\nIter: " + std::to_string(iterTimes / n)
			+ "\nVoid: " + std::to_string(voidTimes / n)
			+ "\nDraw: " + std::to_string(drawTimes / n)
			+ "\nGifs: " + std::to_string(gifsTimes / n)
			+ "\n" + logString;

		std::ofstream logfile;
		if (logfile) { // Check if the file opened successfully
			logfile.open("log.txt");
			logfile << logString;
			logfile.close();
		} else 
			std::cerr << "Error opening file for writing!" << std::endl;
#endif
		DeleteEncoder();
	}
	void FractalGenerator::FractalGenerator::GenerateDots(const uint16_t& bitmapIndex, const int16_t& stateIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color) {
#ifdef CUSTOMDEBUG
		std::string threadString = "";
		const auto initTime{ std::chrono::steady_clock::now() };
#endif
		bitmapState[bitmapIndex] = BitmapState::Dots;
		// Init buffer with zeroes
		auto taskIndex = std::abs(stateIndex);
		auto& task = tasks[taskIndex];
		auto& buffT = task.buffer;
		for (auto y = 0; y < selectHeight; ++y) {
			const auto& buffY = buffT[y];
			for (auto x = 0; x < selectWidth; buffY[x++] = zero);
		}
#ifdef CUSTOMDEBUG
		const auto initElapsed = (std::chrono::steady_clock::now() - initTime).count();
		Log(threadString, "Init:" + std::to_string(bitmapIndex) + " time = " + std::to_string(initElapsed));
		const auto iterTime{ std::chrono::steady_clock::now() };
#endif
		if (cancelRequested) {
			if (stateIndex >= 0)
				tasks[stateIndex].state = TaskState::Done;
			return;
		}
		// Generate the fractal frame recursively
		auto& H = task.H;
		auto& I = task.I;
		for (int b = 0; b < applyBlur; ++b) {
			ModFrameParameters(size, angle, spin, hueAngle, color);
			// Preiterate values that change the same way as iteration goes deeper, so they only get calculated once
			float inSize = size;
			auto& preIterateTask = task.preIterate;
			for (int i = 0; i < maxIterations; ++i) {
				auto& preIterated = preIterateTask[i];
				const auto inDetail = -inSize * std::max(-1.0f, std::get<1>(preIterated) = std::logf(selectDetail / (std::get<0>(preIterated) = inSize)) / logBase);
				auto& XY = std::get<2>(preIterated);
				if (XY == nullptr)
					XY = new std::pair<float, float>[maxChildren];
				for (int c = 0; c < f->childCount; ++c)
					XY[c] = std::pair<float, float>(f->childX[c] * inDetail, f->childY[c] * inDetail);
				inSize /= f->childSize;
			}
			// Prepare Color blending per one dot (hueshifting + iteration correction)
			// So that the color of the dot will slowly approach the combined colors of its childer before it splits
			auto lerp = std::fmod(hueAngle, 1.0f);
			switch ((uint8_t)hueAngle % 3) {
			case 0:
				I = Vector::Lerp(unitX, unitY, lerp);
				H = Vector::Lerp(*colorBlend, Y(*colorBlend), lerp);
				//R->I->FromVector(Vector::Lerp(unitX, unitY, lerp));
				//R->H->FromVector(Vector::Lerp(*colorBlend, Y(*colorBlend), lerp));
				break;
			case 1:
				I = Vector::Lerp(unitY, unitZ, lerp);
				H = Vector::Lerp(Y(*colorBlend), Z(*colorBlend), lerp);
				//R->I->FromVector(Vector::Lerp(unitY, unitZ, lerp));
				//R->H->FromVector(Vector::Lerp(Y(*colorBlend), Z(*colorBlend), lerp));
				break;
			case 2:
				I = Vector::Lerp(unitZ, unitX, lerp);
				H = Vector::Lerp(Z(*colorBlend), *colorBlend, lerp);
				//R->I->FromVector(Vector::Lerp(unitZ, unitX, lerp));
				//R->H->FromVector(Vector::Lerp(Z(*colorBlend), *colorBlend, lerp));
				break;
			}
			if (applyMaxTasks <= 2 || applyParallelType != ParallelType::OfDepth)
				GenerateDots_SingleTask(task, CHILDPARAMS);
			else {
				tuples[0] = { taskIndex, CHILDPARAMS };
				GenerateDots_OfDepth(bitmapIndex);
			}
			/*
			if(applyMaxGenerationTasks <= 1)
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
				GenerateDots_OfRecursion(CHILDPARAMS);
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
				}
				break;
				*/

			IncFrameParameters(size, angle, spin, hueAngle, applyBlur);
			if (cancelRequested) {
				if (stateIndex >= 0)
					tasks[stateIndex].state = TaskState::Done;
				return;
			}
		}
#ifdef CUSTOMDEBUG
		const auto iterElapsed = (std::chrono::steady_clock::now() - iterTime).count();
		Log(threadString, "Iter:" + std::to_string(bitmapIndex) + " time = " + std::to_string(iterElapsed));
		taskLock.lock();
		initTimes += initElapsed;
		iterTimes += iterElapsed;
		Log(logString, threadString);
		taskLock.unlock();
#endif
		if (stateIndex >= 0) // OfAnimation - continue directly with the nexts steps such as void and gif in this same task:
			GenerateImage(task);
		else // OfDepth - start continuation in a new task:
			task.Start(bitmapIndex).task = std::thread(&FractalGenerator::GenerateImage, this, std::ref(task));
	}
	void FractalGenerator::GenerateImage(FractalTask& task) {
#ifdef CUSTOMDEBUG
		std::string threadString = "";
		const auto voidTime{ std::chrono::steady_clock::now() };
#endif
		bitmapState[task.bitmapIndex] = BitmapState::Void;
		auto& voidT = task.voidDepth;
		auto& buffT = task.buffer;
		auto& queueT = task.voidQueue;
		auto lightNormalizer = 0.1f, voidDepthMax = 1.0f;
		int16_t voidYX;
		const auto w1 = selectWidth - 1, h1 = selectHeight - 1;
		// Old SIMD vector code I couldn't get to work
		//Vector<float> *buffY;
		int16_t* voidY, * void0, * voidH, * voidP, * voidM;
		if (selectAmbient > 0) {
			// Void Depth Seed points (no points, no borders), leet the void depth generator know where to start incrementing the depth
			float lightMax;
			for (uint16_t y = 1; y < h1; ++y) {
				if (cancelRequested) {
					task.state = TaskState::Done;
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
					task.state = TaskState::Done;
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
		lightNormalizer = selectBrightness * 2.55f / lightNormalizer;
		if (cancelRequested) {
			task.state = TaskState::Done;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto voidElapsed = (std::chrono::steady_clock::now() - voidTime).count();
		Log(threadString, "Void:" + std::to_string(task.bitmapIndex) + " time = " + std::to_string(voidElapsed));
		const auto drawTime{ std::chrono::steady_clock::now() };
#endif
		// Draw the generated pixel to bitmap data
		// Make a locked bitmap, remember the locked state
		/*uint8_t* p = (uint8_t*)(void*)((bitmapData[bitmapIndex] = (bitmap[bitmapIndex] = gcnew Bitmap(width, height))->LockBits(rect,
			ImageLockMode::WriteOnly,
			System::Drawing::Imaging::PixelFormat::Format24bppRgb))->Scan0);*/
		uint8_t* mp, *p = mp = bitmap[task.bitmapIndex];
		bitmapState[task.bitmapIndex] = BitmapState::Drawing;
		// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
		// Switch between th selected settings such as saturation, noise, image parallelism...
		const auto& cbuffT = const_cast<const Vector**&>(buffT);
		const auto& cvoidT = const_cast<const int16_t**&>(voidT);
		auto maxGenerationTasks = static_cast<int16_t>(std::max(1, applyMaxTasks - 1));
		if (applyParallelType > 0 && maxGenerationTasks > 1) {
			// Multi Threaded:
			const auto stride = 3 * selectWidth;
			if (ambnoise > 0) {
				if (selectSaturate > 0.0) {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancelRequested)
							continue;
						auto rp = p + y * stride;
						NoiseSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancelRequested)
							continue;
						uint8_t* rp = p + y * stride;
						NoiseNoSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				}
			} else {
				if (selectSaturate > 0.0) {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancelRequested)
							continue;
						auto rp = p + y * stride;
						NoNoiseSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(maxGenerationTasks)
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
				if (selectSaturate > 0.0) for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancelRequested)
						continue;
					NoiseSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				} else for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancelRequested)
						continue;
					NoiseNoSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				}
			} else {
				if (selectSaturate > 0.0) for (uint16_t y = 0; y < selectHeight; ++y) {
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
		if (cancelRequested) {
			task.state = TaskState::Done;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto drawElapsed = (std::chrono::steady_clock::now() - drawTime).count();
		Log(threadString, "Draw:" + std::to_string(task.bitmapIndex) + " time = " + std::to_string(voidElapsed));
		const auto gifsTime{ std::chrono::steady_clock::now() };
#endif
		bitmapState[task.bitmapIndex] = BitmapState::Encoding; // Start encoding Frame to a temp GIF		
		if (applyGenerationType >= GenerationType::EncodeGIF && gifEncoder != nullptr)
			((GifEncoder*)gifEncoder)->push_parallel(mp, selectWidth, selectHeight, selectDelay, task.bitmapIndex, false, &cancelRequested);
#ifdef CUSTOMDEBUG
		const auto gifsElapsed = (std::chrono::steady_clock::now() - gifsTime).count();
		Log(threadString, "Gifs:" + std::to_string(task.bitmapIndex) + " time = " + std::to_string(gifsElapsed));
		taskLock.lock();
		voidTimes += voidElapsed;
		drawTimes += drawElapsed;
		gifsTimes += gifsElapsed;
		Log(logString, threadString);
		taskLock.unlock();
#endif
		task.state = TaskState::Done;
	}
	void FractalGenerator::GenerateDots_SingleTask(const FractalTask& task, const std::pair<float, float> inXY, const std::pair<float, float> inAngle, const uint8_t inColor, const int inFlags, uint8_t inDepth) {
		IFAPPLIEDDOT return;
		STARTITERATION{
			ITERATECHILDREN GenerateDots_SingleTask(task, NEWCHILD);
		}
	}
	void FractalGenerator::GenerateDots_OfDepth(const uint16_t bitmapIndex) {
		uint16_t index = 0, insertTo = 1, max = applyMaxTasks * 8, maxcount = max - f->childCount - 1;
		int16_t count = (max + insertTo - index) % max;
		while (count > 0 && count < maxcount) {
			const auto& params = tuples[index++];
			index %= max;
			const uint16_t& taskIndex = std::get<0>(params);
			const std::pair<float, float>& inXY = std::get<1>(params);
			const std::pair<float, float>& inAngle = std::get<2>(params);
			const uint8_t& inColor = std::get<3>(params);
			const int& inFlags = std::get<4>(params);
			uint8_t inDepth = std::get<5>(params);
			const auto& task = tasks[taskIndex];
			IFAPPLIEDDOT continue;
			STARTITERATION{
				ITERATECHILDREN {
					tuples[insertTo++] = { taskIndex, NEWCHILD };
					insertTo %= max;
				}
			}
			count = (max + insertTo - index) % max;
		}
		for (auto tasksRemaining = true; tasksRemaining; MakeDebugString()) {
			tasksRemaining = false;
			for (short taskIndex = applyMaxTasks; 0 <= --taskIndex; ) {
				FractalTask& task = tasks[taskIndex];
				if (task.IsTaskStillRunning()) tasksRemaining |= bitmapState[task.bitmapIndex] <= BitmapState::Dots; // Task not finished yet
				else if (!cancelRequested && selectMaxTasks == applyMaxTasks) {
					if (count <= 0)
						continue;
					task.Start(bitmapIndex).task = std::thread(&FractalGenerator::Task_OfDepth, this, taskIndex, index++);
					index %= max;
					count = (max + insertTo - index) % max;
					tasksRemaining = true;
				}
			}
		}
	}
	/*void FractalGenerator::GenerateDots_OfRecursion(const FractalTask& task, const std::pair<float, float> inXY, const std::pair<float, float> inAngle, const uint8_t inColor, const int inFlags, uint8_t inDepth) {
		IFAPPLIEDDOT return;
		STARTITERATION{
			ITERATECHILDREN {
				if (imageTasks != nullptr && imageTasks->Count < applyMaxGenerationTasks && inDepth < maxDepth)
					imageTasks->Add(Task::Factory->StartNew(
						gcnew Action<Object^>(this, &FractalGenerator::Task_OfRecursion),
						gcnew array<System::Object^>{ R, NEWCHILD }
					));
				else GenerateDots_OfRecursion(task, NEWCHILD);
			}
		}
	}*/
	void FractalGenerator::TryFinishBitmaps() {
		while (applyGenerationType >= GenerationType::EncodeGIF) {
			int unlock = ((GifEncoder*)gifEncoder)->getFinishedFrame();
			// Try to finalize the previous encoder tasks
			switch (((GifEncoder*)gifEncoder)->tryWrite()) {
			case GifEncoderTryWriteResult::Failed:
				// fallback to only display animation without encoding
				applyGenerationType = GenerationType::AnimationRAM;
				return;
			case GifEncoderTryWriteResult::FinishedFrame:
				// mark the bitmap state as fully finished
				bitmapState[unlock] = std::max(bitmapState[unlock], BitmapState::Finished);
				break;
			default:
				// waiting or finished animation
				return;
			}
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
			const auto startX = std::max(static_cast<int16_t>(1), static_cast<int16_t>(std::floor(inX - selectBloom))),
				endX = std::min(widthBorder, static_cast<int16_t>(ceil(inX + selectBloom))),
				endY = std::min(heightBorder, static_cast<int16_t>(ceil(inY + selectBloom)));
			for (int16_t x, y = std::max(static_cast<int16_t>(1), static_cast<int16_t>(std::floor(inY - selectBloom))); y <= endY; ++y) {
				const auto yd = bloom1 - std::abs(y - inY);
				const auto& buffY = buffT[y];
				for (x = startX; x <= endX; ++x)
					buffY[x] += (yd * (bloom1 - std::abs(x - inX))) * dotColor; //buffT[y][x] += Vector<float>((1.0f - Math::Abs(x - inX)) * (1.0f - Math::Abs(y - inY))) * dotColor;
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
	Vector FractalGenerator::ApplySaturate(const Vector& rgb) const {
		// The saturation equation boosting up the saturation of the pixel (powered by the saturation slider setting)
		float m; const auto min = std::min(std::min(rgb.X, rgb.Y), rgb.Z), max = std::max(std::max(rgb.X, rgb.Y), rgb.Z);
		return max <= min ? rgb : Vector::MultiplyMinus(rgb, (m = max * selectSaturate / (max - min)) + 1 - selectSaturate, min * m);
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
				ApplyRGBToBytePointer(ApplyAmbientNoise(ApplySaturate(Normalize(buffY[x], lightNormalizer)), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb, dist, randGen), p);
			}
		}
	}
	inline void FractalGenerator::NoNoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0) for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), p));
		else for (uint16_t x = 0; x < selectWidth; ++x)
			ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x] / voidDepthMax) + ApplySaturate(Normalize(buffY[x], lightNormalizer)), p);
	}
	inline void FractalGenerator::NoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0)
			for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), p));
		else {
			std::random_device rd;
			std::mt19937 randGen(rd());
			std::uniform_real_distribution<float> dist(0.0f, ambnoise);
			for (uint16_t x = 0; x < selectWidth; ++x) {
				const auto voidAmb = voidY[x] / voidDepthMax;
				ApplyRGBToBytePointer(ApplyAmbientNoise(Normalize(buffY[x], lightNormalizer), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb, dist, randGen), p);
			}
		}
	}
	inline void FractalGenerator::NoNoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) const {
		if (selectAmbient <= 0) 
			for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), p));
		else for (uint16_t x = 0; x < selectWidth; ++x)
			ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x] / voidDepthMax) + Normalize(buffY[x], lightNormalizer), p);
	}
#pragma endregion

#pragma region TaskWrappers
	void FractalGenerator::Task_OfDepth(const int16_t taskIndex, const uint16_t tupleIndex) {
		const auto& params = tuples[tupleIndex];
		GenerateDots_SingleTask(tasks[std::get<0>(params)], std::get<1>(params), std::get<2>(params), std::get<3>(params), std::get<4>(params), std::get<5>(params));
		tasks[taskIndex].state = TaskState::Done;
	}
#pragma endregion

#pragma region AnimationParameters
	void FractalGenerator::SwitchParentChild(float& angle, int8_t& spin, uint8_t& color) {
		if (abs(spin) > 1) {
			// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
			angle = -angle;
			spin = -spin;
		}
		color = (3 + color + applyZoom * childColor[0]) % 3;
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
		angle += spin * (applyPeriodAngle * (1 + selectExtraSpin)) / (finalPeriodMultiplier * blurPeriod);
		hueAngle += (hueCycleMultiplier + 3 * selectExtraHue) * (float)applyHueCycle / (finalPeriodMultiplier * blurPeriod);
		IncFrameSize(size, blurPeriod);
	}
#pragma endregion

#pragma region Interface_Calls
	void FractalGenerator::StartGenerate() {
		// start the generator in a separate main thread so that the form can continue being responsive
		mainTask = std::thread(&FractalGenerator::GenerateAnimation, this);
	}
	void FractalGenerator::ResetGenerator() {
		applyZoom = (short)(selectZoom != 0 ? selectZoom : randomDist(randomGenerator) < .5f ? -1 : 1);
		applyCutparam = selectCutparam < 0 ? (int16_t)(randomDist(randomGenerator) * cutparamMaximum) : selectCutparam;
		SetupColor();

		// get the multiplier of the basic period required to get to a seamless loop
		uint16_t m = applyHueCycle == 0 && childColor[0] > 0 ? selectPeriodMultiplier * 3 : selectPeriodMultiplier;
		bool asymmetric = childAngle[0] < 2 * M_PI;
		bool doubled = std::abs(selectSpin) > 1 || selectSpin == 0 && asymmetric;
		finalPeriodMultiplier = doubled ? 2 * m : m;
		// A complex expression to calculate the minimum needed hue shift speed to match the loop:
		hueCycleMultiplier = applyHueCycle == 0 ? 0 : childColor[0] % 3 == 0 ? 3 : 1 +
			(childColor[0] % 3 == 1 == (1 == applyHueCycle) == (1 == applyZoom) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3;
		// setup bitmap data
		applyPeriodAngle = selectPeriodMultiplier % 2 == 0 && asymmetric && !doubled ? selectPeriodAngle * 2 : selectPeriodAngle;
		bitmapsFinished = nextBitmap = 0;
		frames = debug > 0 ? debug : selectPeriod * finalPeriodMultiplier;
		if (allocatedFrames != frames) {
			if (allocatedFrames >= 0)
				delete[] bitmap;
			bitmap = new uint8_t * [allocatedFrames = frames];
		}
		//bitmapData = new uint8_t[BitmapData];
		bitmapState.reserve(frames + 1);
		for (int i = static_cast<int>(bitmapState.size()); 0 <= --i; bitmapState[i] = BitmapState::Queued );
		for (int i = static_cast<int>(bitmapState.size()) - 1; ++i <= frames; bitmapState.push_back(BitmapState::Queued));
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
#ifdef CUSTOMDEBUG
	void FractalGenerator::Log(std::string& log, const std::string& line) {
		std::cout << line << std::endl;
		log += "\n" + line;
	}
#endif
	void FractalGenerator::DebugStart() {
		// debug for testing, starts the generator with predetermined setting for easy breakpointing
		// Need to enable the definition on top GeneratorForm.cpp to enable this
		// if enabled you will be unable to use the settings interface!
		SelectFractal(1);
		SelectThreadingDepth();
		selectPeriod = debug = 1;
		selectWidth = 8;//1920;
		selectHeight = 8;//1080;
		maxDepth = -1;//= 2;
		selectMaxTasks = 2;// 10;
		selectSaturate = 1.0f;
		selectDetail = .25f;
		SelectThreadingDepth();
		selectCut = selectChildAngle = selectChildColor = 0;
		SetupColor();
		SetupAngle();
		SetupCutFunction();
	}
	void FractalGenerator::MakeDebugString() {
		if (!debugmode)
			return;
		if (cancelRequested) {
			debugString = "ABORTING";
			return;
		}
		std::string _debugString = "TASKS:";
		for (auto t = 0; t < applyMaxTasks; ++t) {
			auto& task = tasks[t];
			_debugString += "\n" + std::to_string(t) + ": ";
			switch (task.state) {
			case TaskState::Running:
				_debugString += "RUN: " + GetBitmapState(bitmapState[task.bitmapIndex]);
				break;
			case TaskState::Done:
				_debugString += "DONE: " + GetBitmapState(bitmapState[task.bitmapIndex]);
				break;
			case TaskState::Free:
				_debugString += "FREE";
				break;
			}
		}
		auto laststate = BitmapState::Error;
		auto b = 0;;
		for (auto i = 0; i < 9; counter[i++] = 0);
		for (_debugString += "\n\nIMAGES:"; b < bitmapsFinished; ++b)
			++counter[(int)bitmapState[b]];
		std::string _memoryString = "";
		while (b < frames && bitmapState[b] > BitmapState::Queued) {
			auto state = bitmapState[b];
			if (state != laststate) {
				if (laststate != BitmapState::Error)
					_memoryString += "-" + std::to_string(b - 1) + ": " + GetBitmapState(laststate);
				_memoryString += "\n" + std::to_string(b);
				laststate = state;
			}
			++counter[(int)state];
			++b;
		}
		if (bitmapState[bitmapsFinished] > BitmapState::Queued)
			_memoryString += "-" + std::to_string(b - 1) + ": " + GetBitmapState(laststate);
		for (int c = 0; c < 9; ++c)
			_debugString += "\n" + std::to_string(counter[c]) + "x: " + GetBitmapState((BitmapState)c);
		_debugString += "\n" + _memoryString;
		debugString = b < frames ? _debugString + "\n" + std::to_string(b) + "+: " + "QUEUED" : _debugString;
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
		logBase = std::logf(f->childSize);
		SetMaxIterations(false);
	}
	void FractalGenerator::SetMaxIterations(bool forcedReset) {
		int16_t newMaxIterations = 2 + (int16_t)(std::ceil(std::logf(std::max(selectWidth, selectHeight) * f->maxSize / selectDetail) / logBase));
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
		selectPeriodAngle = f->childCount <= 0 ? 0.0f : std::fmod(childAngle[0], 2.0f * (float)M_PI);
	}
	void FractalGenerator::SetupColor() {
		// Unpack the color palette and hue cycling
		if (selectHue < 0) {
			applyHueCycle = (int16_t)(randomDist(randomGenerator) * 2 - 1);
			applyColorPalette = (int16_t)(randomDist(randomGenerator) * 2);
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
#pragma endregion

#pragma region Interface_Getters
	/*std::string FractalGenerator::ConvertToStdString(System::String^ managedString) {
		using namespace System::Runtime::InteropServices;
		const auto chars = (const char*)(Marshal::StringToHGlobalAnsi(managedString)).ToPointer();
		const auto nativeString(chars);
		Marshal::FreeHGlobal(System::IntPtr((void*)chars));
		return nativeString;
	}*/
#pragma endregion

}
