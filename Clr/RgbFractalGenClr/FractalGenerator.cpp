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
#define CHILDPARAMS pack(selectWidth * .5f, selectHeight * .5f), pack(angle, Math::Abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, static_cast<uint8_t>(0)
#define IFAPPLIEDDOT float inX, inY; unpack(inXY, inX, inY); const auto& preIterated = task.preIterate[inDepth]; const auto& inSize = std::get<0>(preIterated);\
	if (ApplyDot(inSize < selectDetail, task, inX, inY, std::get<1>(preIterated), inColor))
#define STARTITERATION const auto& newPreIterated = task.preIterate[++inDepth];auto i = f->childCount; while (0 <= --i)
#define ITERATECHILDREN if (cancel->Token.IsCancellationRequested) return;\
	const auto newFlags = CalculateFlags(i, inFlags);if (newFlags < 0) continue;\
	const auto& XY = std::get<2>(preIterated)[i];float inAngle, inAntiAngle; unpack(AA, inAngle, inAntiAngle); const auto cs = cos(inAngle), sn = sin(inAngle);\
	const float newX = inX + XY.first * cs - XY.second * sn, newY = inY - XY.second * cs - XY.first * sn;\
	if (TestSize(newX, newY, inSize))
#define NEWCHILD pack(newX, newY), i == 0\
	? pack(inAngle + childAngle[i] - inAntiAngle, -inAntiAngle)\
	: pack(inAngle + childAngle[i], inAntiAngle), (inColor + childColor[i]) % 3, newFlags, inDepth

namespace RgbFractalGenClr {

#ifdef CUSTOMDEBUG
	using namespace System::Diagnostics;
#endif
#ifdef PARALLELDEBUG
	using namespace System::Diagnostics;
#endif

	using namespace System::IO;
	using namespace System::Drawing::Imaging;

#pragma region Init
	FractalGenerator::FractalGenerator() {
		selectCutparam = debug = selectDefaultHue = selectDefaultZoom = selectDefaultAngle = selectExtraSpin = selectExtraHue = 0;
		selectZoom = 1;
		selectFractal = selectChildColor = selectChildAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks = allocatedFrames = selectSpin = maxIterations = -1;
		selectParallelType = ParallelType::OfAnimation;
		selectGenerationType = GenerationType::EncodeGIF;
		gifEncoder = nullptr;
		bitmap = nullptr;
		cutFunction = nullptr;
		tuples = nullptr;
		parallelTasks = nullptr;
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
		maxChildren = 1;
		for (Fractal** i = fractals; *i != nullptr; ++i)
			if ((*i)->childCount > maxChildren)
				maxChildren = (*i)->childCount;
		childColor = new uint8_t[maxChildren];
	}
	System::Void FractalGenerator::DeleteEncoder() {
		if (gifEncoder != nullptr) {
			delete gifEncoder;
			gifEncoder = nullptr;
		}
	}
	System::Void FractalGenerator::DeleteBuffer(const FractalTask& task) {
		const auto& buffT = task.buffer;
		const auto& voidT = task.voidDepth;
		for (auto y = 0; y < allocatedHeight; ++y) {
			delete buffT[y];
			delete voidT[y];
		}
		delete[] buffT;
		delete[] voidT;
	}
	System::Void FractalGenerator::NewBuffer(FractalTask& task) {
		const auto& voidT = task.voidDepth = new int16_t * [selectHeight];
		const auto& buffT = task.buffer = new Vector * [selectHeight];
		for (auto y = 0; y < selectHeight; voidT[y++] = new int16_t[selectWidth]) {
			const auto& buffY = buffT[y] = new Vector[selectWidth];
			for (auto x = 0; x < selectWidth; buffY[x++] = zero);
		}
	}
#pragma endregion

#pragma region Generate_Tasks
	System::Void FractalGenerator::GenerateAnimation() {
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
				gifTempPath = "gif" + gifIndex.ToString() + ".tmp";
				if(!e->open_parallel(ConvertToStdString(gifTempPath), selectWidth, selectHeight, 1, 0, true, &cancel->Token)) {
#ifdef CUSTOMDEBUG
					Log(logString, "Error opening gif file: " + gifTempPath);
#endif
					++gifIndex;
					continue;
				} else break;
			}
			if (gifIndex == 255)
				DeleteEncoder();
			else if(selectAmbient < 0)
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
		int8_t spin = static_cast<int8_t>(selectSpin < -1 ? random.Next(-1, 3) : selectSpin);
		ModFrameParameters(size, angle, spin, hueAngle, color);
		applyBlur = (short)(selectBlur + 1);
		for (int i = selectDefaultZoom < 0 
			 ? random.Next(0, selectPeriod * finalPeriodMultiplier) 
			 : (selectDefaultZoom % (selectPeriod * finalPeriodMultiplier));
			 0 <= --i; IncFrameSize(size, selectPeriod));
		// Generate the images
		while (!cancel->Token.IsCancellationRequested && bitmapsFinished < bitmap->Length) {
			// Initialize buffers (delete and reset if size changed)
			//const int16_t batchTasks = Math::Max(static_cast<int16_t>(1), applyMaxTasks = selectMaxTasks);
			if ((applyMaxTasks = Math::Max(static_cast<int16_t>(1), selectMaxTasks)) != allocatedTasks) { 
				if (allocatedTasks >= 0) {
					for (uint16_t t = 0; t < allocatedTasks; ++t) {
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
				rect = System::Drawing::Rectangle(0, 0, allocatedWidth = selectWidth, allocatedHeight = selectHeight);
				tasks = new FractalTask[allocatedTasks = applyMaxTasks];
				tuples = new std::tuple<uint16_t, double, double, uint8_t, int, uint8_t>[applyMaxTasks * 8];
				for (int16_t t = applyMaxTasks; 0 <= --t;) {
					auto& task = tasks[t];
					task.taskIndex = t;
					NewBuffer(task);
				}
				parallelTasks = gcnew array<Task^>(applyMaxTasks);
				for (uint16_t t = 0; t < applyMaxTasks; parallelTasks[t++] = nullptr);
				SetMaxIterations(true);
				//tuples = new std::tuple<double, double, uint8_t, int, uint8_t>[Math::Max(1, applyMaxGenerationTasks * 8)];
				//voidDepth = new int16_t * *[applyMaxTasks];
				//voidQueue = new std::queue<std::pair<int16_t, int16_t>>* [applyMaxTasks];
				//taskStarted = new bool[applyMaxTasks];
				//parallelTaskFinished = new uint8_t[applyMaxTasks];
				//buffer = new Vector * *[allocatedTasks = applyMaxTasks];
				//for (int t = 0; t < applyMaxTasks; NewBuffer(t++))
				//	voidQueue[t] = new std::queue<std::pair<int16_t, int16_t>>();
			}
			if (selectHeight != allocatedHeight) {
				for (uint16_t t = 0; t < applyMaxTasks; ++t) {
					auto& task = tasks[t];
					DeleteBuffer(task);
					NewBuffer(task);
				}
				rect = System::Drawing::Rectangle(0, 0, allocatedWidth = selectWidth, allocatedHeight = selectHeight);
			}
			if (selectWidth != allocatedWidth) {
				for (uint16_t t = 0; t < applyMaxTasks; ++t) {
					const auto& task = tasks[t];
					const auto& buffT = task.buffer;
					const auto& voidT = task.voidDepth;
					for (uint16_t y = 0; y < allocatedHeight; voidT[y++] = new int16_t[selectWidth]) {
						delete buffT[y];
						delete voidT[y];
						buffT[y] = new Vector[selectWidth];
					}
					for (uint16_t y = allocatedHeight; y < selectHeight; voidT[y++] = new int16_t[selectWidth])
						buffT[y] = new Vector[selectWidth];
				}
				rect = System::Drawing::Rectangle(0, 0, allocatedWidth = selectWidth, selectHeight);
			}
			const auto generateLength = (applyGenerationType > GenerationType::OnlyImage 
										 ? static_cast<int16_t>(bitmap->Length) : static_cast<int16_t>(1));
			// Wait if no more frames to generate
			if (bitmapsFinished >= generateLength)
				continue;
			applyParallelType = applyGenerationType == GenerationType::OnlyImage ? ParallelType::OfDepth : selectParallelType;
			// Image parallelism
			//imageTasks = applyParallelType == 2 ? gcnew ConcurrentBag<Task^>() : nullptr;
			// The other implementation are using the FinishTasks lambda argument function, but I couldn't get that to work in this managed class, so I had to unpack it:

			for (auto tasksRemaining = true; tasksRemaining; MakeDebugString()) {
				TryFinishBitmaps();
				while (bitmapsFinished < bitmap->Length && bitmapState[bitmapsFinished] >= (applyGenerationType >= GenerationType::EncodeGIF ? BitmapState::Finished : BitmapState::Encoding)) {
					bitmap[bitmapsFinished]->UnlockBits(bitmapData[bitmapsFinished]);
					++bitmapsFinished;
				}
				tasksRemaining = false;
				for (short taskIndex = applyMaxTasks; 0 <= --taskIndex; ) {
					FractalTask& task = tasks[taskIndex];
					if (IsTaskStillRunning(task)) tasksRemaining = true;
					else if (!cancel->Token.IsCancellationRequested && selectMaxTasks == applyMaxTasks) {
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
							Start(taskIndex, bmp, gcnew Action<Object^>(this, &FractalGenerator::Task_Dots), gcnew array<System::Object^>{ bmp, taskIndex, size, angle, spin, hueAngle, color });
							IncFrameParameters(size, angle, spin, hueAngle, 1);
							tasksRemaining = true;
						}
					}
				}
			}
		}
		// Wait for threads to finish
		for (int t = allocatedTasks; 0 <= --t; Join(tasks[t]));
		// Unlock unfinished bitmaps:
		for (int b = 0; b < bitmap->Length; ++b)
			if (bitmapState[b] > BitmapState::Dots && bitmapState[b] < BitmapState::Finished && bitmap[b] != nullptr) {
				try {
					bitmap[b]->UnlockBits(bitmapData[b]);
				} catch (Exception^) {}
			}

		//TestEncoder();

		// Save the temp GIF file
		gifSuccess = false;
		if (applyGenerationType >= GenerationType::EncodeGIF && gifEncoder != nullptr && ((GifEncoder*)gifEncoder)->close(true, &cancel->Token))
			while (/*!cancel->Token.IsCancellationRequested &&*/ gifEncoder != nullptr) {
				auto& encoder = *((GifEncoder*)gifEncoder);
				switch (encoder.tryWrite()) {
				case GifEncoderTryWriteResult::Failed:
					DeleteEncoder();
					break;
				case GifEncoderTryWriteResult::Waiting:
					// This should never be hit, because the coe reches this loop only after getting cancelled or every frame finished
					Thread::Sleep(100);
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
		Log(logString, "Generate time = " + generateElapsed + "\n");
		long long n = bitmap->Length * 100000;
		logString = "CppManaged:\nInit: " + initTimes / n
			+ "\nIter: " + iterTimes / n
			+ "\nVoid: " + voidTimes / n
			+ "\nDraw: " + drawTimes / n
			+ "\nGifs: " + gifsTimes / n
			+ "\n" + logString;
		File::WriteAllText("log.txt", logString);
#endif
		MakeDebugString();
		cancel->Cancel(); // If the gifEncoder failed and got nulled but is still running some tasks
		DeleteEncoder();
		//mainTask = nullptr;
	}
	System::Void FractalGenerator::GenerateDots(const uint16_t& bitmapIndex, const int16_t& stateIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color) {
#ifdef CUSTOMDEBUG
		System::String^ threadString = "";
		const auto initTime{ std::chrono::steady_clock::now() };
#endif
		bitmapState[bitmapIndex] = BitmapState::Dots;
		// Init buffer with zeroes
		auto taskIndex = Math::Abs(stateIndex);
		auto& task = tasks[taskIndex];
		auto& buffT = task.buffer;
		for (auto y = 0; y < selectHeight; ++y) {
			const auto& buffY = buffT[y];
			for (auto x = 0; x < selectWidth; buffY[x++] = zero);
		}
#ifdef CUSTOMDEBUG
		const auto initElapsed = (std::chrono::steady_clock::now() - initTime).count();
		Log(threadString, "Init:" + bitmapIndex + " time = " + initElapsed);
		const auto iterTime{ std::chrono::steady_clock::now() };
#endif
		if (cancel->Token.IsCancellationRequested) {
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
				const auto inDetail = -inSize * Math::Max(-1.0f, std::get<1>(preIterated) = (float)Math::Log(selectDetail / (std::get<0>(preIterated) = inSize)) / logBase);
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
			if (cancel->Token.IsCancellationRequested) {
				if (stateIndex >= 0)
					tasks[stateIndex].state = TaskState::Done;
				return;
			}
		}
#ifdef CUSTOMDEBUG
		const auto iterElapsed = (std::chrono::steady_clock::now() - iterTime).count();
		Log(threadString, "Iter:" + bitmapIndex + " time = " + iterElapsed);
		Monitor::Enter(taskLock);
		try {
			initTimes += initElapsed;
			iterTimes += iterElapsed;
			Log(logString, threadString);
		} finally { Monitor::Exit(taskLock); }
#endif
		if (stateIndex >= 0) // OfAnimation - continue directly with the nexts steps such as void and gif in this same task:
			GenerateImage(task);
		else // OfDepth - start continuation in a new task:
			Start(taskIndex, bitmapIndex, gcnew Action<Object^>(this, &FractalGenerator::Task_Image), gcnew array<System::Object^>{ taskIndex });
	}
	System::Void FractalGenerator::GenerateImage(FractalTask& task) {
#ifdef CUSTOMDEBUG
		System::String^ threadString = "";
		const auto voidTime{ std::chrono::steady_clock::now() };
#endif
		// Generate the grey void areas
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
				if (cancel->Token.IsCancellationRequested) {
					task.state = TaskState::Done;
					std::swap(queueT, std::queue<std::pair<int16_t, int16_t>>()); // Fast swap-and-drop
					return;
				}
				const auto buffY = buffT[y];
				voidY = voidT[y];
				for (uint16_t x = 1; x < w1; ++x) {
					auto& buffYX = buffY[x];
					// Old SIMD vector code I couldn't get to work
					//lightNormalizer = Math::Max(lightNormalizer, lightMax = Math::Max(buffYX[0], Math::Max(buffYX[1], buffYX[2])));
					lightNormalizer = Math::Max(lightNormalizer, lightMax = buffYX.Max());
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
				voidMax = Math::Max(voidMax, voidYX = voidY[x] + 1);
				if (xp < selectWidth && (voidY[xp] == -1)) { voidY[xp] = voidYX; queueT.push({ y, xp }); }
				if (yp < selectHeight && ((voidP = voidT[yp])[x] == -1)) { voidP[x] = voidYX;  queueT.push({ yp, x }); }
				if (xm >= 0 && (voidY[xm] == -1)) { voidY[xm] = voidYX; queueT.push({ y, xm }); }
				if (ym >= 0 && ((voidM = voidT[ym])[x] == -1)) { voidM[x] = voidYX;  queueT.push({ ym, x }); }
			}
			voidDepthMax = voidMax;
		} else
			for (uint16_t y = 0; y < selectHeight; y++) {
				if (cancel->Token.IsCancellationRequested) {
					task.state = TaskState::Done;
					return;
				}
				const auto buffY = buffT[y];
				for (uint16_t x = 0; x < selectWidth; x++) {
					auto& buffYX = buffY[x];
					// Old SIMD vector code I couldn't get to work
					//lightNormalizer = Math::Max(lightNormalizer, Math::Max(buffYX[0], Math::Max(buffYX[1], buffYX[2])));
					lightNormalizer = Math::Max(lightNormalizer, buffYX.Max());
				}
			}
		lightNormalizer = selectBrightness * 2.55f / lightNormalizer;
		if (cancel->Token.IsCancellationRequested) {
			task.state = TaskState::Done;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto voidElapsed = (std::chrono::steady_clock::now() - voidTime).count();
		Log(threadString, "Void:" + task.bitmapIndex + " time = " + voidElapsed);
		const auto drawTime{ std::chrono::steady_clock::now() };
#endif
		// Draw the generated pixel to bitmap data
		// Make a locked bitmap, remember the locked state
		uint8_t* p = (uint8_t*)(void*)((bitmapData[task.bitmapIndex] = (bitmap[task.bitmapIndex] = gcnew Bitmap(selectWidth, selectHeight))
										->LockBits(rect, ImageLockMode::ReadWrite, System::Drawing::Imaging::PixelFormat::Format24bppRgb))->Scan0);
		bitmapState[task.bitmapIndex] = BitmapState::Drawing;
		// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
		// Switch between th selected settings such as saturation, noise, image parallelism...
		const auto& cbuffT = const_cast<const Vector**&>(buffT);
		const auto& cvoidT = const_cast<const int16_t**&>(voidT);
		auto maxGenerationTasks = (int16_t)Math::Max(1, applyMaxTasks - 1);
		if (applyParallelType > 0 && maxGenerationTasks > 1) {
			// Multi Threaded:
			const auto stride = 3 * selectWidth;
			if (ambnoise > 0) {
				if (selectSaturate > 0.0) {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancel->Token.IsCancellationRequested)
							continue;
						auto rp = p + y * stride;
						NoiseSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancel->Token.IsCancellationRequested)
							continue;
						uint8_t* rp = p + y * stride;
						NoiseNoSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				}
			} else {
				if (selectSaturate > 0.0) {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancel->Token.IsCancellationRequested)
							continue;
						auto rp = p + y * stride;
						NoNoiseSaturate(cbuffT[y], cvoidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < selectHeight; ++y) {
						if (cancel->Token.IsCancellationRequested)
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
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoiseSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				} else for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoiseNoSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				}
			} else {
				if (selectSaturate > 0.0) for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoNoiseSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				} else for (uint16_t y = 0; y < selectHeight; ++y) {
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoNoiseNoSaturate(cbuffT[y], cvoidT[y], p, lightNormalizer, voidDepthMax);
				}
			}
		}
		if (cancel->IsCancellationRequested) {
			task.state = TaskState::Done;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto drawElapsed = (std::chrono::steady_clock::now() - drawTime).count();
		Log(threadString, "Draw:" + task.bitmapIndex + " time = " + voidElapsed);
		const auto gifsTime{ std::chrono::steady_clock::now() };
#endif
		bitmapState[task.bitmapIndex] = BitmapState::Encoding; // Start encoding Frame to a temp GIF		
		if (applyGenerationType >= GenerationType::EncodeGIF && gifEncoder != nullptr) 
			((GifEncoder*)gifEncoder)->push_parallel((uint8_t*)(void*)(bitmapData[task.bitmapIndex]->Scan0), selectWidth, selectHeight, selectDelay, task.bitmapIndex, true, &cancel->Token);
		else bitmapState[task.bitmapIndex] = BitmapState::Finished;
#ifdef CUSTOMDEBUG
		const auto gifsElapsed = (std::chrono::steady_clock::now() - gifsTime).count();
		Log(threadString, "Gifs:" + task.bitmapIndex + " time = " + gifsElapsed);
		Monitor::Enter(taskLock);
		try {
			voidTimes += voidElapsed;
			drawTimes += drawElapsed;
			gifsTimes += gifsElapsed;
			Log(logString, threadString);
		} finally { Monitor::Exit(taskLock); }
#endif
		task.state = TaskState::Done;
	}
	System::Void FractalGenerator::GenerateDots_SingleTask(const FractalTask& task, double inXY, double AA, const uint8_t inColor, const int inFlags, uint8_t inDepth) {
		IFAPPLIEDDOT return;
		STARTITERATION {
			ITERATECHILDREN GenerateDots_SingleTask(task, NEWCHILD);
		}
	}
	System::Void FractalGenerator::GenerateDots_OfDepth(const uint16_t bitmapIndex) {
		uint16_t index = 0, insertTo = 1, max = applyMaxTasks * 8, maxcount = max - f->childCount - 1;
		int16_t count = (max + insertTo - index) % max;
		while (count > 0 && count < maxcount) {
			const auto& params = tuples[index++];
			const uint16_t& taskIndex = std::get<0>(params);
			const double& inXY = std::get<1>(params);
			const double& AA = std::get<2>(params);
			const uint8_t& inColor = std::get<3>(params);
			const int& inFlags = std::get<4>(params);
			uint8_t inDepth = std::get<5>(params);
			const auto& task = tasks[taskIndex];
			IFAPPLIEDDOT continue;
			STARTITERATION {
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
				if (IsTaskStillRunning(task)) tasksRemaining |= bitmapState[task.bitmapIndex] <= BitmapState::Dots; // Task not finished yet
				else if (!cancel->Token.IsCancellationRequested && selectMaxTasks == applyMaxTasks) {
					if (count <= 0)
						continue;
					Start(taskIndex, bitmapIndex, gcnew Action<Object^>(this, &FractalGenerator::Task_OfDepth), gcnew array<System::Object^>{ taskIndex, index++ });
					index %= max;
					count = (max + insertTo - index) % max;
					tasksRemaining = true;
				}
			}
		}
	}
	/*System::Void FractalGenerator::GenerateDots_OfRecursion(
		const uint16_t taskIndex,
		double inXY, double AA,
		const uint8_t inColor, const int inFlags, uint8_t inDepth
	) {
		IFAPPLIEDDOT return;
		STARTITERATION {
			ITERATECHILDREN {
				if (imageTasks != nullptr && imageTasks->Count < applyMaxGenerationTasks && inDepth < maxDepth)
					imageTasks->Add(Task::Factory->StartNew(
						gcnew Action<Object^>(this, &FractalGenerator::Task_OfRecursion),
						gcnew array<System::Object^>{ NEWCHILD }
					));
				else GenerateDots_OfRecursion(NEWCHILD);
			}
		}
	}*/
	bool FractalGenerator::IsTaskStillRunning(FractalTask& task) {
		return task.state == TaskState::Done ? Join(task) : task.state != TaskState::Free;
	}
	System::Void FractalGenerator::TryFinishBitmaps() {
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
				bitmapState[unlock] = BitmapState::Finished;
				break;
			default:
				// waiting or finished animation
				return;
			}
		}
	}
#pragma endregion

#pragma region Generate_Inline
	bool FractalGenerator::ApplyDot(const bool apply, const FractalTask& task, const float& inX, const float& inY, const float& inDetail, const uint8_t& inColor) {
		if (apply) {
			Vector dotColor = Vector::Lerp(task.H, task.I, inDetail);
			switch (inColor) {
			case 1: dotColor = Y(dotColor); break;
			case 2: dotColor = Z(dotColor); break;
			}
			// Iterated deep into a single point - Interpolate inbetween 4 pixels and Stop
			const auto& buffT = task.buffer;
			const auto startX = Math::Max(static_cast<int16_t>(1), static_cast<int16_t>(Math::Floor(inX - selectBloom))),
				endX = Math::Min(widthBorder, static_cast<int16_t>(Math::Ceiling(inX + selectBloom))),
				endY = Math::Min(heightBorder, static_cast<int16_t>(Math::Ceiling(inY + selectBloom)));
			for (int16_t x, y = Math::Max(static_cast<int16_t>(1), static_cast<int16_t>(Math::Floor(inY - selectBloom))); y <= endY; ++y) {
				const auto yd = bloom1 - Math::Abs(y - inY);
				const auto& buffY = buffT[y];
				for (x = startX; x <= endX; ++x)
					buffY[x] += (yd * (bloom1 - Math::Abs(x - inX))) * dotColor; //buffT[y][x] += Vector<float>((1.0f - Math::Abs(x - inX)) * (1.0f - Math::Abs(y - inY))) * dotColor;
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
	/*inline System::Void FractalGenerator::ApplyAmbientNoise(Vector<float>& rgb,  const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen) {
		rgb += Noise * Vector<float>(gcnew array<float>(3) { dist(randGen), dist(randGen), dist(randGen)}) + Vector<float>(amb);
	}
	inline System::Void FractalGenerator::ApplySaturate(Vector<float>& rgb, const float d, uint8_t*& p) {
		float m; const float min = Math::Min(Math::Min(rgb[0], rgb[1]), rgb[2]), max = Math::Max(Math::Max(rgb[0], rgb[1]), rgb[2]);
		return max <= min ? rgb : ((m = max * saturate / (max - min)) + 1 - saturate) * rgb - Vector<float>(min * m);
	}
	inline System::Void FractalGenerator::ApplyNoSaturate(Vector<float>& rgb, uint8_t*& p) {
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
		float m; const auto min = Math::Min(Math::Min(rgb.X, rgb.Y), rgb.Z), max = Math::Max(Math::Max(rgb.X, rgb.Y), rgb.Z);
		return max <= min ? rgb : Vector::MultiplyMinus(rgb, (m = max * selectSaturate / (max - min)) + 1 - selectSaturate, min * m);
	}
	System::Void FractalGenerator::ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p) {
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
	inline System::Void FractalGenerator::NoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
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
	inline System::Void FractalGenerator::NoNoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0) for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), p));
		else for (uint16_t x = 0; x < selectWidth; ++x)
			ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x] / voidDepthMax) + ApplySaturate(Normalize(buffY[x], lightNormalizer)), p);

	}
	inline System::Void FractalGenerator::NoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
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
	inline System::Void FractalGenerator::NoNoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (selectAmbient <= 0) 
			for (uint16_t x = 0; x < selectWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), p));
		else for (uint16_t x = 0; x < selectWidth; ++x)
			ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x] / voidDepthMax) + Normalize(buffY[x], lightNormalizer), p);
	}
#pragma endregion

#pragma region TaskWrappers
	System::Void FractalGenerator::Task_Dots(System::Object^ obj) {
		// Unpack arguments
		const auto args = (array<System::Object^>^)obj;
		const auto bitmapIndex = static_cast<uint16_t>(args[0]);
		const auto taskIndex = static_cast<int16_t>(args[1]);
		const auto size = static_cast<float>(args[2]);
		const auto angle = static_cast<float>(args[3]);
		const auto spin = static_cast<int8_t>(args[4]);
		const auto hueAngle = static_cast<float>(args[5]);
		const auto color = static_cast<uint8_t>(args[6]);
		GenerateDots(bitmapIndex, taskIndex, size, angle, spin, hueAngle, color);
	}
	System::Void FractalGenerator::Task_OfDepth(System::Object^ obj) {
		const auto args = (array<System::Object^>^)obj;
		const auto taskIndex = static_cast<int16_t>(args[0]);
		const auto tupleIndex = static_cast<uint16_t>(args[1]);
		const auto& params = tuples[tupleIndex];
		GenerateDots_SingleTask(tasks[std::get<0>(params)], std::get<1>(params), std::get<2>(params), std::get<3>(params), std::get<4>(params), std::get<5>(params));
		tasks[taskIndex].state = TaskState::Done;
	}
	inline bool FractalGenerator::Join(FractalTask& task) {
		if (task.taskStarted) {
			if (parallelTasks[task.taskIndex] != nullptr) {
#ifdef PARALLELDEBUG
				Debug::WriteLine("join" + task.index);
#endif
				parallelTasks[task.taskIndex]->Wait();
				parallelTasks[task.taskIndex] = nullptr;
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
		task.state = TaskState::Free;
		return task.taskStarted = false;
	}
	inline System::Void FractalGenerator::Start(const uint16_t taskIndex, uint16_t bitmap, Action<Object^>^ action, Object^ state) {
		auto& task = tasks[taskIndex];
		if (task.taskStarted) {
#ifdef PARALLELDEBUG
			Debug::WriteLine("ERROR start: task already running " + task.index);
#endif

			if (parallelTasks[task.taskIndex] != nullptr) {
				parallelTasks[task.taskIndex]->Wait();
				parallelTasks[task.taskIndex] = nullptr;
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
		task.bitmapIndex = bitmap;
		task.state = TaskState::Running;
		parallelTasks[taskIndex] = Task::Factory->StartNew(action, state);
	}
	System::Void FractalGenerator::Task_Image(System::Object^ obj) {
		GenerateImage(tasks[static_cast<int16_t>(((array<System::Object^>^)obj)[0])]);
	}
	/*System::Void FractalGenerator::Task_OfRecursion(System::Object^ obj) {
		// Unpack arguments
		const auto args = (array<System::Object^>^)obj;
		const auto taskIndex = static_cast<uint16_t>(args[0]);
		const auto inXY = static_cast<double>(args[1]);
		const auto inAngle = static_cast<double>(args[2]);
		const auto inColor = static_cast<uint8_t>(args[3]);
		const auto inFlags = static_cast<int>(args[4]);
		const auto inDepth = static_cast<uint8_t>(args[5]);
#ifdef CUSTOMDEBUG
		const auto threadTime{ std::chrono::steady_clock::now() };
		System::String^ start = "Thread:x" + (int)floor(inX) + "y" + (int)floor(inY) + "s" + (int)floor(inSize) + " start = " + (threadTime - *startTime).count();
#endif
		GenerateDots_OfRecursion(taskIndex, inXY, inAngle, inColor, inFlags, inDepth);
#ifdef CUSTOMDEBUG
		const auto threadEndTime{ std::chrono::steady_clock::now() };
		Monitor::Enter(taskLock);
		try {
			Log(logString, start + " end = " + (threadEndTime - *startTime).count() + " time = " + (threadEndTime - threadTime).count());
		} finally { Monitor::Exit(taskLock); }
#endif
	}
	System::Void FractalGenerator::Task_Gif(System::Object^ obj) {}*/
#pragma endregion

#pragma region AnimationParameters
	System::Void FractalGenerator::SwitchParentChild(float& angle, int8_t& spin, uint8_t& color) {
		if (Math::Abs(spin) > 1) {
			// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
			angle = -angle;
			spin = -spin;
		}
		color = (3 + color + applyZoom * childColor[0]) % 3;
	}
	System::Void FractalGenerator::ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color) {
		const auto w = Math::Max(selectWidth, selectHeight) * f->maxSize, fp = f->childSize;
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
	System::Void FractalGenerator::IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const uint16_t blur) {
		const auto blurPeriod = selectPeriod * blur;
		// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
		angle += spin * (applyPeriodAngle * (1 + selectExtraSpin)) / (finalPeriodMultiplier * blurPeriod);
		hueAngle += (hueCycleMultiplier + 3 * selectExtraHue) * (float)applyHueCycle / (finalPeriodMultiplier * blurPeriod);
		IncFrameSize(size, blurPeriod);
	}
#pragma endregion

#pragma region Interface_Calls
	System::Void FractalGenerator::StartGenerate() {
		// start the generator in a separate main thread so that the form can continue being responsive
		mainTask = Task::Run(gcnew Action(this, &FractalGenerator::GenerateAnimation), (cancel = gcnew CancellationTokenSource())->Token);
	}
	System::Void FractalGenerator::ResetGenerator() {
		applyZoom = (short)(selectZoom != 0 ? selectZoom : random.NextDouble() < .5f ? -1 : 1);
		applyCutparam = selectCutparam < 0 ? (short)random.Next(0, cutparamMaximum) : selectCutparam;
		SetupColor();

		// get the multiplier of the basic period required to get to a seamless loop
		uint16_t m = applyHueCycle == 0 && childColor[0] > 0 ? selectPeriodMultiplier * 3 : selectPeriodMultiplier;
		bool asymmetric = childAngle[0] < 2 * M_PI;
		bool doubled = Math::Abs(selectSpin) > 1 || selectSpin == 0 && asymmetric;
		finalPeriodMultiplier = doubled ? 2 * m : m;
		// A complex expression to calculate the minimum needed hue shift speed to match the loop:
		hueCycleMultiplier = applyHueCycle == 0 ? 0 : childColor[0] % 3 == 0 ? 3 : 1 +
			(childColor[0] % 3 == 1 == (1 == applyHueCycle) == (1 == applyZoom) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3;
		// setup bitmap data
		applyPeriodAngle = selectPeriodMultiplier % 2 == 0 && asymmetric && !doubled ? selectPeriodAngle * 2 : selectPeriodAngle;
		bitmapsFinished = nextBitmap = 0;
		uint16_t frames = debug > 0 ? debug : selectPeriod * finalPeriodMultiplier;
		if (frames != allocatedFrames) {
			if (allocatedFrames >= 0)
				delete[] bitmapState;
			bitmapState = new BitmapState[(allocatedFrames = frames) + 1];
		}
		bitmap = gcnew array<Bitmap^>(frames);
		bitmapData = gcnew array<BitmapData^>(frames);
		for (int i = frames; 0 <= i; bitmapState[i--] = BitmapState::Queued);
	}
	System::Void FractalGenerator::RequestCancel() {
		if (cancel != nullptr)
			cancel->Cancel();
		try {
			if (mainTask != nullptr)
				mainTask->Wait();
		} catch (Exception^) {}
	}
	bool FractalGenerator::SaveGif(System::String^ gifPath) {
		try {
			// Try to save (move the temp) the gif file
			//if (gifEncoder != nullptr)
			//	((GifEncoder*)gifEncoder)->close();
			try { 
				File::Delete(gifPath); 
			} 
			catch (Exception^) {}
			File::Move(gifTempPath, gifPath);
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
		}
		return gifSuccess = false;
	}
#ifdef CUSTOMDEBUG
	System::Void FractalGenerator::Log(System::String^ log, System::String^ line) {
		Debug::WriteLine(line);
		log += "\n" + line;
	}
#endif
	System::Void FractalGenerator::DebugStart() {
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
	System::Void FractalGenerator::MakeDebugString() {
		if (!debugmode)
			return;
		if (cancel->Token.IsCancellationRequested) {
			debugString = "ABORTING";
			return;
		}
		System::String^ _debugString = "TASKS:";
		for (auto t = 0; t < applyMaxTasks; ++t) {
			auto& task = tasks[t];
			_debugString += "\n" + t + ": ";
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
		for(auto i = 0; i < 8; counter[i++] = 0);
		for (_debugString += "\n\nIMAGES:"; b < bitmapsFinished; ++b)
			++counter[(int)bitmapState[b]];
		System::String^ _memoryString = "";
		while (b < bitmap->Length && bitmapState[b] > BitmapState::Queued) {
			auto state = bitmapState[b];
			if (state != laststate) {
				if (laststate != BitmapState::Error) 
					_memoryString += "-" + (b - 1) + ": " + GetBitmapState(laststate);
				_memoryString += "\n" + b;
				laststate = state;
			}
			++counter[(int)state];
			++b;
		}
		if (bitmapState[bitmapsFinished] > BitmapState::Queued) 
			_memoryString += "-" + (b - 1) + ": " + GetBitmapState(laststate);
		for (int c = 0; c < 8; ++c) 
			_debugString += "\n" + counter[c] + "x: " + GetBitmapState((BitmapState)c);
		_debugString += "\n" + _memoryString;
		debugString = b < bitmap->Length ? _debugString + "\n" + b + "+: " + "QUEUED" : _debugString;
	}
	System::Void FractalGenerator::TestEncoder(array<Bitmap^>^ bitmap) {

		// init
		GifEncoder* se = new GifEncoder();
		GifEncoder* pe = new GifEncoder();
		GifEncoder* se_out = new GifEncoder();
		GifEncoder* pe_out = new GifEncoder();
		List<int>^ l = gcnew List<int>();

		// get byte pointers if we don't have already:
		array<BitmapData^>^ bgr = gcnew array<BitmapData^>(bitmap->Length);
		array<BitmapData^>^ bgra = gcnew array<BitmapData^>(bitmap->Length);
		for (int i = 0; i < bitmap->Length; ++i) {
			bgr[i] = bitmap[i]->LockBits(Rectangle(0, 0, bitmap[i]->Width, bitmap[i]->Height), ImageLockMode::ReadOnly, PixelFormat::Format24bppRgb); // get BRG (also used for RGB)
			bgra[i] = bitmap[i]->LockBits(Rectangle(0, 0, bitmap[i]->Width, bitmap[i]->Height), ImageLockMode::ReadOnly, PixelFormat::Format32bppArgb); // get BRGA (also used for RGBA)
		}

		// BGR/BGRA:
		se->open("s_bgr_inorder.gif", selectWidth, selectHeight, 1, true, 0);
		pe->open_parallel("p_bgr_inorder.gif", selectWidth, selectHeight, 1, 0);
		se_out->open("s_bgr_outoforder.gif", selectWidth, selectHeight, 1, true, 0);
		pe_out->open_parallel("p_bgr_outoforder.gif", selectWidth, selectHeight, 1, 0);
		for (int i = 0; i < bitmap->Length; l->Add(i++)) {
			uint8_t* frame_bgr = (uint8_t*)(void*)bgr[i]->Scan0;
			uint8_t* frame_bgra = (uint8_t*)(void*)bgra[i]->Scan0;
			// BGR NATIVE:
			se->push(frame_bgr, selectWidth, selectHeight, 20);
			pe->push_parallel(frame_bgr, selectWidth, selectHeight, 20);
			// BGR copy:
			//se->push(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame_bgr, selectWidth, selectHeight, 20);
			//pe->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame_bgr, selectWidth, selectHeight, 20);
			// BGRA copy:
			//se->push(GifEncoderPixelFormat::PIXEL_FORMAT_BGRA, frame_bgra, selectWidth, selectHeight, 20);
			//pe->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGRA, frame_bgra, selectWidth, selectHeight, 20);
		}
		while (l->Count > 0) {
			int i = l[random.Next(0, l->Count)];
			l->Remove(i);
			uint8_t* frame_bgr = (uint8_t*)(void*)bgr[i]->Scan0;
			uint8_t* frame_bgra = (uint8_t*)(void*)bgra[i]->Scan0;
			// BGR NATIVE:
			se_out->push(frame_bgr, selectWidth, selectHeight, 20, i);
			pe_out->push_parallel(frame_bgr, selectWidth, selectHeight, 20, i);
			// BGR copy:
			//se_out->push(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame_bgr, selectWidth, selectHeight, 20, i);
			//pe_out->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGR, frame_bgr, selectWidth, selectHeight, 20, i);
			// BGRA copy:
			//se_out->push(GifEncoderPixelFormat::PIXEL_FORMAT_BGRA, frame_bgra, selectWidth, selectHeight, 20, i);
			//pe_out->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_BGRA, frame_bgra, selectWidth, selectHeight, 20, i);
		}
		se->close();
		pe->close();
		se_out->close();
		pe_out->close();
		for (int i = 0; i <= bitmap->Length; ++i) {
			pe->tryWrite();
			pe_out->tryWrite();
		}

		// RGB/RGBA:
		uint8_t** rgb = new uint8_t * [bitmap->Length];
		uint8_t** rgba = new uint8_t * [bitmap->Length];
		for (int i = 0; i < bitmap->Length; ++i) {
			// RGB
			uint8_t* p = rgb[i] = new uint8_t[selectWidth * selectHeight * 3],
				* s = (uint8_t*)(void*)bgr[i]->Scan0;
			for (int i = selectWidth * selectHeight; 0 <= --i; p += 3, s += 3) {
				p[0] = s[2];
				p[1] = s[1];
				p[2] = s[0];
			}
			// RBGA
			p = rgba[i] = new uint8_t[selectWidth * selectHeight * 4];
			s = (uint8_t*)(void*)bgra[i]->Scan0;
			for (int i = selectWidth * selectHeight; 0 <= --i; p += 4, s += 4) {
				p[0] = s[2];
				p[1] = s[1];
				p[2] = s[0];
				p[3] = s[3];
			}
		}
		se->open("s_rgb_inorder.gif", selectWidth, selectHeight, 1, true, 0);
		pe->open_parallel("p_rgb_inorder.gif", selectWidth, selectHeight, 1, 0);
		se_out->open("s_rgb_outoforder.gif", selectWidth, selectHeight, 1, false, 0); // try both false and true global color map
		pe_out->open_parallel("p_rgb_outoforder.gif", selectWidth, selectHeight, 1, 0);
		for (int i = 0; i < bitmap->Length; l->Add(i++)) {
			// RGB copy:
			se->push(GifEncoderPixelFormat::PIXEL_FORMAT_RGB, rgb[i], selectWidth, selectHeight, 20);
			pe->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_RGB, rgb[i], selectWidth, selectHeight, 20);
			// RGBA copy:
			//se->push(GifEncoderPixelFormat::PIXEL_FORMAT_RGBA, rgba[i], selectWidth, selectHeight, 20);
			//pe->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_RGBA, rgba[i], selectWidth, selectHeight, 20);
		}
		while (l->Count > 0) {
			int i = l[random.Next(0, l->Count)];
			l->Remove(i);
			// RGB copy:
			se_out->push(GifEncoderPixelFormat::PIXEL_FORMAT_RGB, rgb[i], selectWidth, selectHeight, 20, i);
			pe_out->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_RGB, rgb[i], selectWidth, selectHeight, 20, i);
			// RGBA copy: 
			//se_out->push(GifEncoderPixelFormat::PIXEL_FORMAT_RGBA, rgba[i], selectWidth, selectHeight, 20, i);
			//pe_out->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_RGBA, rgba[i], selectWidth, selectHeight, 20, i);
		}
		se->close();
		pe->close();
		se_out->close();
		pe_out->close();
		for (int i = 0; i <= bitmap->Length; ++i) {
			pe->tryWrite();
			pe_out->tryWrite();
		}

		// release byte pointers and encoders:
		for (int i = 0; i < bitmap->Length; ++i)
			bitmap[i]->UnlockBits(bitmapData[i]);
		delete se;
		delete pe;
		delete se_out;
		delete pe_out;
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
	System::Void FractalGenerator::SetupFractal() {
		f = fractals[selectFractal];
		logBase = (float)Math::Log(f->childSize);
		SetMaxIterations(false);
	}
	System::Void FractalGenerator::SetMaxIterations(bool forcedReset) {
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
	System::Void FractalGenerator::SetupAngle() {
		childAngle = f->childAngle == nullptr ? new float[1] { (float)M_PI } : f->childAngle[selectChildAngle].second;
		selectPeriodAngle = f->childCount <= 0 ? 0.0f : std::fmod(childAngle[0], 2.0f * (float)M_PI);
	}
	System::Void FractalGenerator::SetupColor() {
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

#pragma endregion

#pragma region Interface_Getters
	std::string FractalGenerator::ConvertToStdString(System::String^ managedString) {
		using namespace System::Runtime::InteropServices;
		const auto chars = (const char*)(Marshal::StringToHGlobalAnsi(managedString)).ToPointer();
		const auto nativeString(chars);
		Marshal::FreeHGlobal(System::IntPtr((void*)chars));
		return nativeString;
	}
#pragma endregion


}
