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
#define IterationBody\
	if (cancel->Token.IsCancellationRequested) return;\
	const auto newFlags = CalculateFlags(i, inFlags);\
	if(newFlags < 0) continue;\
	const auto newX = inX + inSize * (f->childX[i] * cos(inAngle) - f->childY[i] * sin(inAngle)),\
	newY = inY - inSize * (f->childY[i] * cos(inAngle) + f->childX[i] * sin(inAngle));\
	if(TestSize(newX, newY, inSize)) continue;\
	if (i == 0)
#define CenterChild newX, newY, inAngle + childAngle[i] - inAntiAngle, -inAntiAngle, newSize, static_cast<uint8_t>((inColor + childColor[i]) % 3), newFlags
#define OtherChild newX, newY, inAngle + childAngle[i], inAntiAngle, newSize, static_cast<uint8_t>((inColor + childColor[i]) % 3), newFlags


namespace RgbFractalGenClr {

#ifdef CUSTOMDEBUG
	using namespace System::Diagnostics;
#endif

	using namespace System::IO;
	using namespace System::Drawing::Imaging;

#pragma region Init
	FractalGenerator::FractalGenerator() {
		cutparam = debug = defaultHue = defaultZoom = defaultAngle = extraSpin = extraHue = parallelType = 0;
		zoom = 1;
		select = selectColor = selectAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks = allocatedFrames = defaultSpin = selectColorPalette = -1;
		encode = 2;
		gifEncoder = nullptr;
		bitmap = nullptr;
		gifSuccess = false;
		taskSnapshot = gcnew System::Collections::Generic::List<Task^>();
		emptyFloat = new float[1] { static_cast<float>(M_PI) };
		InitFractalDefinitions();
	}
	void FractalGenerator::InitFractalDefinitions() {
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
			new Fractal("SierpinskiCarpet", 9, 3, .9f, .175f, .9f, cx, cy,
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
	System::Void FractalGenerator::DeleteEncoder() {
		if (gifEncoder != nullptr) {
			delete gifEncoder;
			gifEncoder = nullptr;
		}
	}
	System::Void FractalGenerator::DeleteBuffer(const uint16_t taskIndex) {
		const auto& buffT = buffer[taskIndex];
		const auto& voidT = voidDepth[taskIndex];
		for (auto y = 0; y < allocatedHeight; ++y) {
			delete buffT[y];
			delete voidT[y];
		}
		delete[] buffT;
		delete[] voidT;
	}
	System::Void FractalGenerator::NewBuffer(const uint16_t taskIndex) {
		const auto& voidT = voidDepth[taskIndex] = new int16_t * [height];
		const auto& buffT = buffer[taskIndex] = new Vector * [height];
		for (auto y = 0; y < height; voidT[y++] = new int16_t[width]) {
			const auto& buffY = buffT[y] = new Vector[width];
			for (auto x = 0; x < width; buffY[x++] = zero);
		}
	}
	System::Void FractalGenerator::InitBuffer(int16_t taskIndex) {
		const auto& buffT = buffer[taskIndex];
		for (auto y = 0; y < height; ++y) {
			const auto& buffY = buffT[y];
			for (auto x = 0; x < width; buffY[x++] = zero);
		}
	}
#pragma endregion

#pragma region Generate_Tasks
	System::Void FractalGenerator::GenerateAnimation() {
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
		if (encode >= 2) {
			gifEncoder = new GifEncoder();
			uint8_t gifIndex = 0;
			while (gifIndex < 255) {
				gifTempPath = "gif" + gifIndex.ToString() + ".tmp";
				if (!((GifEncoder*)gifEncoder)->open(ConvertToStdString(gifTempPath), width, height, 1, false, 0, width * height * 3)) {
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
		auto size = 2400.0f, angle = defaultAngle * (float)M_PI / 180.0f, hueAngle = defaultHue / 120.0f;
		uint8_t color = 0;
		ambnoise = amb * noise;
		auto spin = defaultSpin;
		ModFrameParameters(size, angle, spin, hueAngle, color);
		for (int i = defaultZoom; 0 <= --i; IncFrameSize(size, period));
		
		// Generate the images
		while (!cancel->Token.IsCancellationRequested && bitmapsFinished < bitmap->Length) {
			// Initialize buffers (delete and reset if size changed)
			applyMaxGenerationTasks = maxGenerationTasks;
			const int16_t batchTasks = Math::Max(static_cast<int16_t>(1), applyMaxTasks = maxTasks);
			if (batchTasks != allocatedTasks) {
				if (allocatedTasks >= 0) {
					// Delete old task arrays
					for (int t = 0; t < allocatedTasks; ++t) {
						DeleteBuffer(t);
						delete voidQueue[t];
					}
					delete[] buffer;
					delete[] voidDepth;
					delete[] voidQueue;
					delete[] parallelTaskFinished;
					delete[] tuples;
					//delete[] animationTasks;
				}
				tuples = new std::tuple<float, float, float, float, float, uint8_t, int>[Math::Max(1, applyMaxGenerationTasks * 8)];
				rect = System::Drawing::Rectangle(0, 0, allocatedWidth = width, allocatedHeight = height);
				voidDepth = new int16_t * *[batchTasks];
				voidQueue = new std::queue<std::pair<int16_t, int16_t>>* [batchTasks];
				parallelTasks = gcnew array<Task^>(batchTasks + 1);
				parallelTaskFinished = new uint8_t[batchTasks + 2];
				buffer = new Vector * *[allocatedTasks = batchTasks];
				for (int t = 0; t < batchTasks; NewBuffer(t++))
					voidQueue[t] = new std::queue<std::pair<int16_t, int16_t>>();
			}
			if (height != allocatedHeight) {
				for (int t = 0; t < batchTasks; NewBuffer(t++))
					DeleteBuffer(t);
				rect = System::Drawing::Rectangle(0, 0, allocatedWidth = width, allocatedHeight = height);
			}
			if (width != allocatedWidth) {
				for (int t = 0; t < batchTasks; ++t) {
					const auto& buffT = buffer[t];
					const auto& voidT = voidDepth[t];
					for (auto y = 0; y < allocatedHeight; voidT[y++] = new int16_t[width]) {
						delete buffT[y];
						delete voidT[y];
						buffT[y] = new Vector[width];
					}
					for (auto y = allocatedHeight; y < height; voidT[y++] = new int16_t[width])
						buffT[y] = new Vector[width];

				}
				rect = System::Drawing::Rectangle(0, 0, allocatedWidth = width, height);
			}
			for (int t = 0; t <= batchTasks; parallelTaskFinished[t++] = 2)
				parallelTasks[t] = nullptr;
			const auto generateLength = (encode > 0 ? static_cast<int16_t>(bitmap->Length) : static_cast<int16_t>(1));
			// Wait if no more frames to generate
			if (bitmapsFinished >= generateLength)
				continue;
			// Image parallelism
			imageTasks = (applyParallelType = parallelType) == 2 ? gcnew ConcurrentBag<Task^>() : nullptr;
			// Animation Parallelism Task Count
			auto animationTaskCount = applyParallelType > 0 ? static_cast<int16_t>(0) : Math::Min(batchTasks, generateLength);
			if (animationTaskCount <= 1) {
				// No Animation Parallelism:
				if (nextBitmap < generateLength) {
					ModFrameParameters(size, angle, spin, hueAngle, color);
					GenerateImage(nextBitmap++, batchTasks, size, angle, spin, hueAngle, color);
					IncFrameParameters(size, angle, spin, hueAngle, color, 1);
					parallelTaskFinished[batchTasks] = 2;
				}
				const auto gifTaskIndex = Math::Max(static_cast<int16_t>(0), applyMaxGenerationTasks);
				if (FinishTask(gifTaskIndex))
					TryGif(gifTaskIndex);
			} else {
				// Animation parallelism: Spawn initial tasks
				/*while (0 <= --animationTaskCount) {
					if (animationTaskFinished[animationTaskCount] >= 2 && nextBitmap < generateLength) {
						animationTaskFinished[animationTaskCount] = 0;
						ModFrameParameters(size, angle, spin, hueAngle, color);
						animationTasks[animationTaskCount] = Task::Factory->StartNew(
							gcnew Action<Object^>(this, &FractalGenerator::GenerateThreadTask),
							gcnew array<System::Object^>{ nextBitmap++, animationTaskCount, size, angle, spin, hueAngle, color }
						);
						IncrementFrameParameters(size, angle, spin, hueAngle, color, 1);
					}
				}*/
				// Animation parallelism: Continue/Finish Animation and Gif tasks
				bool tasksRemaining = true;
				while (tasksRemaining) {
					tasksRemaining = false;
					// Check every task
					for (int16_t task = 0; task < batchTasks; ++task) {
						if (FinishTask(task)) {
							TryGif(task);
							if (parallelTaskFinished[task] >= 2 && nextBitmap < generateLength && !cancel->Token.IsCancellationRequested && maxTasks == applyMaxTasks) {
								// Start another task when previous was finished
								parallelTaskFinished[task] = 0;
								ModFrameParameters(size, angle, spin, hueAngle, color);
								parallelTasks[task] = Task::Factory->StartNew(
									gcnew Action<Object^>(this, &FractalGenerator::Task_Animation),
									gcnew array<System::Object^>{ nextBitmap++, task, size, angle, spin, hueAngle, color }
								);
								IncFrameParameters(size, angle, spin, hueAngle, color, 1);
								tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
							}
						} else tasksRemaining = true; // A task not finished yet - keep checking before new master loop
					}
				}
			}
		}
		// Wait for threads to finish
			//WaitForThreads();
		for (int i = allocatedTasks; i >= 0; --i)
			if (parallelTasks[i] != nullptr)
				parallelTasks[i]->Wait();
		// Unlock unfinished bitmaps:
		for (int i = 0; i < bitmap->Length; ++i)
			if (bitmapState[i] > 0 && bitmapState[i] < 4 && bitmap[i] != nullptr) {
				try {
					bitmap[i]->UnlockBits(bitmapData[i]);
				} catch (Exception^) {}
			}
		// Save the temp GIF file
		gifSuccess = false;
		if (encode >= 2 && gifEncoder != nullptr && ((GifEncoder*)gifEncoder)->close(cancel->Token))
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
	System::Void FractalGenerator::GenerateImage(const uint16_t& bitmapIndex, const int16_t& taskIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color) {
#ifdef CUSTOMDEBUG
		System::String^ threadString = "";
		const auto initTime{ std::chrono::steady_clock::now() };
#endif
		int16_t bufferIndex = taskIndex % applyMaxTasks;
		InitBuffer(bufferIndex);
		auto& buffT = buffer[bufferIndex];
		auto& voidT = voidDepth[bufferIndex];
#ifdef CUSTOMDEBUG
		const auto initElapsed = (std::chrono::steady_clock::now() - initTime).count();
		Log(threadString, "Init:" + bitmapIndex + " time = " + initElapsed);
		const auto iterTime{ std::chrono::steady_clock::now() };
#endif
		// Generate the fractal frame recursively
		VecRefWrapper^ R = gcnew VecRefWrapper(buffT, gcnew ManagedVector(0,0,0), gcnew ManagedVector(0, 0, 0));
		for (int b = 0; b < selectBlur; ++b) {
			if (cancel->Token.IsCancellationRequested) {
				parallelTaskFinished[taskIndex] = 2;
				return;
			}
			ModFrameParameters(size, angle, spin, hueAngle, color);
			SetupFrameColorBlend(hueAngle, R);
			if(applyMaxGenerationTasks <= 1)
				GenerateDots_SingleTask(R, width * .5f, height * .5f, angle, Math::Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam);
			else switch (applyParallelType) {
			case 0:
				GenerateDots_SingleTask(R, width * .5f, height * .5f, angle, Math::Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam);
				break;
			case 1:
				tuples[0] = { width * .5f, height * .5f, angle, Math::Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam };
				GenerateDots_OfDepth(R);
				break;
			case 2:
				GenerateDots_OfRecursion(R, width * .5f, height * .5f, angle, Math::Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam, 0);
				WaitForRecursiveTasks();
				break;
			}
			IncFrameParameters(size, angle, spin, hueAngle, color, selectBlur);
		}
		if (cancel->Token.IsCancellationRequested) {
			parallelTaskFinished[taskIndex] = 2;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto iterElapsed = (std::chrono::steady_clock::now() - iterTime).count();
		Log(threadString, "Iter:" + bitmapIndex + " time = " + iterElapsed);
		const auto voidTime{ std::chrono::steady_clock::now() };
#endif
		// Generate the grey void areas
		auto lightNormalizer = 0.1f, voidDepthMax = 1.0f;
		auto& cbuffT = const_cast<const Vector**&>(buffT);
		GenerateVoid(*voidQueue[bufferIndex], cbuffT, voidT, lightNormalizer, voidDepthMax);
#ifdef CUSTOMDEBUG
		const auto voidElapsed = (std::chrono::steady_clock::now() - voidTime).count();
		Log(threadString, "Void:" + bitmapIndex + " time = " + voidElapsed);
		const auto drawTime{ std::chrono::steady_clock::now() };
#endif
		if (cancel->Token.IsCancellationRequested) {
			parallelTaskFinished[taskIndex] = 2;
			return;
		}
		// Draw the generated pixel to bitmap data
		GenerateBitmap(bitmapIndex, cbuffT, const_cast<const int16_t**&>(voidT), lightNormalizer, voidDepthMax);
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
		parallelTaskFinished[taskIndex] = 1;
	}
	System::Void FractalGenerator::GenerateDots_SingleTask(
		VecRefWrapper^ R,
		const float inX, const float inY, const float inAngle, const float inAntiAngle, const float inSize,
		const uint8_t inColor, const int inFlags
	) {
		if (inSize < detail) {
			ApplyDot(R, inX, inY, inSize, inColor);
			return;
		}
		// Split Iteration Deeper
		const auto newSize = inSize / f->childSize;
		auto i = f->childCount;
		while (0 <= --i) {
			IterationBody GenerateDots_SingleTask(R, CenterChild); else GenerateDots_SingleTask(R, OtherChild);
		}
	}
	System::Void FractalGenerator::GenerateDots_OfDepth(VecRefWrapper^ R) {
		uint16_t index = 0, insertTo = 1, max = applyMaxGenerationTasks * 8, maxcount = max - f->childCount - 1;
		int16_t count = (max + insertTo - index) % max;
		while (count > 0 && count < maxcount) {
			auto& params = tuples[index++];
			const float& inX = std::get<0>(params);
			const float& inY = std::get<1>(params);
			const float& inAngle = std::get<2>(params);
			const float& inAntiAngle = std::get<3>(params);
			const float& inSize = std::get<4>(params);
			const uint8_t& inColor = std::get<5>(params);
			const int& inFlags = std::get<6>(params);
			if (inSize < detail) {
				ApplyDot(R, inX, inY, inSize, inColor);
				continue;
			}
			// Split Iteration Deeper
			const auto newSize = inSize / f->childSize;
			auto i = f->childCount;
			while (0 <= --i) {
				IterationBody tuples[insertTo++] = { CenterChild }; else tuples[insertTo++] = { OtherChild };
				insertTo %= max;
			}
			count = (max + insertTo - index) % max;
		}
		while (count > 0 && !cancel->Token.IsCancellationRequested) {
			// more parallels to compute
			bool tasksRemaining = true;
			while (tasksRemaining) {
				if (FinishTask(applyMaxGenerationTasks))
					TryGif(applyMaxGenerationTasks);
				tasksRemaining = false;
				// Check every task
				for (int16_t task = 0; task < applyMaxGenerationTasks; ++task) {
					if (FinishTask(task)) {
						if (parallelTaskFinished[task] >= 2 && count > 0 && !cancel->Token.IsCancellationRequested) {
							// Start another task when previous was finished
							parallelTaskFinished[task] = 0;
							parallelTasks[task] = Task::Factory->StartNew(
								gcnew Action<Object^>(this, &FractalGenerator::Task_OfDepth),
								gcnew array<System::Object^>{ R, task, index++ }
							);
							index %= max;
							count = (max + insertTo - index) % max;
							tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
						}
					} else tasksRemaining = true; // A task not finished yet - keep checking before new master loop
				}
			}
		}
	}
	System::Void FractalGenerator::GenerateDots_OfRecursion(
		VecRefWrapper^ R,
		const float inX, const float inY, const float inAngle, const float inAntiAngle, const float inSize,
		const uint8_t inColor, const int inFlags, const uint16_t inDepth
	) {
		if (inSize < detail) {
			ApplyDot(R, inX, inY, inSize, inColor);
			return;
		}
		// Split Iteration Deeper
		uint8_t newDepth = inDepth + static_cast<uint8_t>(1);
		const auto newSize = inSize / f->childSize;
		auto i = f->childCount;
		while (0 <= --i) {
			// Iteration Tasks
			IterationBody {
				if (imageTasks != nullptr && imageTasks->Count < applyMaxGenerationTasks && inDepth < maxDepth)
					imageTasks->Add(Task::Factory->StartNew(
						gcnew Action<Object^>(this, &FractalGenerator::Task_OfRecursion),
						gcnew array<System::Object^>{ R, CenterChild, newDepth }
					));
				else GenerateDots_OfRecursion(R, CenterChild, newDepth);
			} else {
				if (imageTasks != nullptr && imageTasks->Count < applyMaxGenerationTasks && inDepth < maxDepth)
					imageTasks->Add(Task::Factory->StartNew(
						gcnew Action<Object^>(this, &FractalGenerator::Task_OfRecursion),
						gcnew array<System::Object^>{ R, OtherChild, newDepth }
					));
				else GenerateDots_OfRecursion(R, OtherChild, newDepth);
			}
		}
	}
	System::Void FractalGenerator::GenerateVoid(std::queue<std::pair<int16_t, int16_t>>& queueT, const Vector**& buffT, int16_t**& voidT, float& lightNormalizer, float& voidDepthMax) {
		int16_t voidYX;
		const auto w1 = width - 1, h1 = height - 1;
		if (cancel->Token.IsCancellationRequested)
			return;
		lightNormalizer = 0.1f;
		// Old SIMD vector code I couldn't get to work
		//Vector<float> *buffY;
		int16_t* voidY, * void0, * voidH, * voidP, * voidM;
		if (amb > 0) {
			// Void Depth Seed points (no points, no borders), leet the void depth generator know where to start incrementing the depth
			auto& queue = *voidQueue;
			float lightMax;
			for (uint16_t y = 1; y < h1; ++y) {
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
			for (uint16_t x = 0; x < width; ++x) {
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
				if (xp < width && (voidY[xp] == -1)) { voidY[xp] = voidYX; queueT.push({ y, xp }); }
				if (yp < height && ((voidP = voidT[yp])[x] == -1)) { voidP[x] = voidYX;  queueT.push({ yp, x }); }
				if (xm >= 0 && (voidY[xm] == -1)) { voidY[xm] = voidYX; queueT.push({ y, xm }); }
				if (ym >= 0 && ((voidM = voidT[ym])[x] == -1)) { voidM[x] = voidYX;  queueT.push({ ym, x }); }
			}
			voidDepthMax = voidMax;
		} else
			for (uint16_t y = 0; y < height; y++) {
				const auto buffY = buffT[y];
				for (uint16_t x = 0; x < width; x++) {
					auto& buffYX = buffY[x];
					// Old SIMD vector code I couldn't get to work
					//lightNormalizer = Math::Max(lightNormalizer, Math::Max(buffYX[0], Math::Max(buffYX[1], buffYX[2])));
					lightNormalizer = Math::Max(lightNormalizer, buffYX.Max());
				}
			}
		lightNormalizer = 160.0f / lightNormalizer;
	}
	System::Void FractalGenerator::GenerateBitmap(uint16_t bitmapIndex, const Vector**& buffT, const int16_t**& voidT, const float lightNormalizer, const float voidDepthMax) {
		if (cancel->Token.IsCancellationRequested)
			return;
		// Make a locked bitmap, remember the locked state
		//bitmap[bitmapIndex] = gcnew Bitmap(width, height);
		//bitmapData[bitmapIndex] = bitmap[bitmapIndex]->LockBits(rect, ImageLockMode::WriteOnly, System::Drawing::Imaging::PixelFormat::Format24bppRgb);
		//uint8_t* p = (uint8_t*)(void*)(bitmapData[bitmapIndex]->Scan0);
		uint8_t* p = (uint8_t*)(void*)((bitmapData[bitmapIndex] = (bitmap[bitmapIndex] = gcnew Bitmap(width, height))->LockBits(rect,
																																ImageLockMode::WriteOnly,
																																System::Drawing::Imaging::PixelFormat::Format24bppRgb))->Scan0);
		bitmapState[bitmapIndex] = 1;
		// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
		// Switch between th selected settings such as saturation, noise, image parallelism...
		if (applyParallelType > 0 && applyMaxGenerationTasks > 1) {
			// Multi Threaded:
			const auto stride = 3 * width;
			if (ambnoise > 0) {
				if (saturate > 0.0) {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < height; ++y) {
						if (cancel->Token.IsCancellationRequested)
							continue;
						auto rp = p + y * stride;
						NoiseSaturate(buffT[y], voidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < height; ++y) {
						if (cancel->Token.IsCancellationRequested)
							continue;
						uint8_t* rp = p + y * stride;
						NoiseNoSaturate(buffT[y], voidT[y], rp, lightNormalizer, voidDepthMax);
					}
				}
			} else {
				if (saturate > 0.0) {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < height; ++y) {
						if (cancel->Token.IsCancellationRequested)
							continue;
						auto rp = p + y * stride;
						NoNoiseSaturate(buffT[y], voidT[y], rp, lightNormalizer, voidDepthMax);
					}
				} else {
#pragma omp parallel for num_threads(applyMaxGenerationTasks)
					for (int16_t y = 0; y < height; ++y) {
						if (cancel->Token.IsCancellationRequested)
							continue;
						auto rp = p + y * stride;
						NoNoiseNoSaturate(buffT[y], voidT[y], rp, lightNormalizer, voidDepthMax);
					}
				}
			}
		} else {
			// Single Threaded:
			if (ambnoise > 0) {
				if (saturate > 0.0) for (uint16_t y = 0; y < height; ++y) {
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoiseSaturate(buffT[y], voidT[y], p, lightNormalizer, voidDepthMax);
				} else for (uint16_t y = 0; y < height; ++y) {
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoiseNoSaturate(buffT[y], voidT[y], p, lightNormalizer, voidDepthMax);
				}
			} else {
				if (saturate > 0.0) for (uint16_t y = 0; y < height; ++y) {
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoNoiseSaturate(buffT[y], voidT[y], p, lightNormalizer, voidDepthMax);
				} else for (uint16_t y = 0; y < height; ++y) {
					if (cancel->Token.IsCancellationRequested)
						continue;
					NoNoiseNoSaturate(buffT[y], voidT[y], p, lightNormalizer, voidDepthMax);
				}
			}
		}
	}
	System::Void FractalGenerator::GenerateGif(const int16_t taskIndex) {
		// Sequentially encode a finished GIF frame, then unlock the bitmap data
		if (cancel->Token.IsCancellationRequested) {
			parallelTaskFinished[taskIndex] = 2;
			return;
		}
#ifdef CUSTOMDEBUG
		System::String^ threadString = "";
		const auto gifTime{ std::chrono::steady_clock::now() };
#endif
		// Save Frame to a temp GIF
		if (encode >= 2
			&& !cancel->Token.IsCancellationRequested
			&& gifEncoder != nullptr
			&& !((GifEncoder*)gifEncoder)->push(cancel->Token, GifEncoder::PIXEL_FORMAT_BGR, (uint8_t*)(void*)(bitmapData[bitmapsFinished]->Scan0), width, height, delay)) {
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
		if (bitmapState[bitmapsFinished] < 4) {
			bitmap[bitmapsFinished]->UnlockBits(bitmapData[bitmapsFinished]); // Lets me generate next one, and lets the GeneratorForm know that this one is ready
			bitmapState[bitmapsFinished++] = 4;
		}
		exportingGif = false;
		parallelTaskFinished[taskIndex] = 1;
	}
	bool FractalGenerator::FinishTask(int16_t taskIndex) {
		if (parallelTaskFinished[taskIndex] == 1) {
			if (parallelTasks[taskIndex] != nullptr)
				parallelTasks[taskIndex]->Wait();
			parallelTasks[taskIndex] = nullptr;
			parallelTaskFinished[taskIndex] = 2;
		}
		return parallelTaskFinished[taskIndex] >= 2;
	}
	System::Void FractalGenerator::TryGif(int16_t taskIndex) {
		if (bitmapState[bitmapsFinished] != 2)
			return;
		if (gifEncoder != nullptr && !exportingGif && !cancel->Token.IsCancellationRequested && maxTasks == applyMaxTasks) {
			exportingGif = true;
			parallelTaskFinished[taskIndex] = 0;
			bitmapState[bitmapsFinished] = 3;
			parallelTasks[taskIndex] = Task::Factory->StartNew(
				gcnew Action<Object^>(this, &FractalGenerator::Task_Gif),
				gcnew array<System::Object^>{ taskIndex }
			);
		} else {
			bitmap[bitmapsFinished]->UnlockBits(bitmapData[bitmapsFinished]);
			bitmapState[bitmapsFinished++] = 4;
		}
	}
	System::Void FractalGenerator::WaitForRecursiveTasks() {
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
	}
#pragma endregion

#pragma region Generate_Inline
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
		return max <= min ? rgb : Vector::MultiplyMinus(rgb, (m = max * saturate / (max - min)) + 1 - saturate, min * m);
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
		if (amb <= 0)
			for (uint16_t x = 0; x < width; ApplyRGBToBytePointer(ApplySaturate(lightNormalizer * buffY[x++]), p));
		else {
			std::random_device rd;
			std::mt19937 randGen(rd());
			std::uniform_real_distribution<float> dist(0.0f, ambnoise);
			for (uint16_t x = 0; x < width; ++x) {
				const auto voidAmb = voidY[x] / voidDepthMax;
				//auto rgb = ApplySaturate(lightNormalizer * buffY[x]);
				ApplyRGBToBytePointer(ApplyAmbientNoise(ApplySaturate(lightNormalizer * buffY[x]), voidAmb * amb, (1.0f - voidAmb) * voidAmb, dist, randGen), p);
			}
		}
	}
	inline System::Void FractalGenerator::NoNoiseSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (amb <= 0) for (uint16_t x = 0; x < width; ApplyRGBToBytePointer(ApplySaturate(lightNormalizer * buffY[x++]), p));
		else for (uint16_t x = 0; x < width; ApplyRGBToBytePointer(Vector(amb * voidY[x++] / voidDepthMax) + ApplySaturate(lightNormalizer * buffY[x]), p));

	}
	inline System::Void FractalGenerator::NoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (amb <= 0)
			for (uint16_t x = 0; x < width; ApplyRGBToBytePointer(lightNormalizer * buffY[x++], p));
		else {
			std::random_device rd;
			std::mt19937 randGen(rd());
			std::uniform_real_distribution<float> dist(0.0f, ambnoise);
			for (uint16_t x = 0; x < width; x++) {
				const auto voidAmb = voidY[x] / voidDepthMax;
				ApplyRGBToBytePointer(ApplyAmbientNoise(lightNormalizer * buffY[x], voidAmb * amb, (1.0f - voidAmb) * voidAmb, dist, randGen), p);
			}
		}
	}
	inline System::Void FractalGenerator::NoNoiseNoSaturate(const Vector*& buffY, const int16_t* voidY, uint8_t*& p, const float lightNormalizer, const float voidDepthMax) {
		if (amb <= 0) for (uint16_t x = 0; x < width; x++)
			ApplyRGBToBytePointer(lightNormalizer * buffY[x], p);
		else for (uint16_t x = 0; x < width; x++)
			ApplyRGBToBytePointer(Vector(amb * voidY[x] / voidDepthMax) + lightNormalizer * buffY[x], p);
	}
#pragma endregion

#pragma region TaskWrappers
	System::Void FractalGenerator::Task_Animation(System::Object^ obj) {
		// Unpack arguments
		const auto args = (array<System::Object^>^)obj;
		const auto bitmapIndex = static_cast<uint16_t>(args[0]);
		const auto taskIndex = static_cast<int16_t>(args[1]);
		const auto size = static_cast<float>(args[2]);
		const auto angle = static_cast<float>(args[3]);
		const auto spin = static_cast<int8_t>(args[4]);
		const auto hueAngle = static_cast<float>(args[5]);
		const auto color = static_cast<uint8_t>(args[6]);
		GenerateImage(bitmapIndex, taskIndex, size, angle, spin, hueAngle, color);
	}
	System::Void FractalGenerator::Task_OfDepth(System::Object^ obj) {
		const auto args = (array<System::Object^>^)obj;
		const auto R = static_cast<VecRefWrapper^>(args[0]);
		const auto taskIndex = static_cast<int16_t>(args[1]);
		const auto tupleIndex = static_cast<uint16_t>(args[2]);
		auto& params = tuples[tupleIndex];
		GenerateDots_SingleTask(R, std::get<0>(params), std::get<1>(params), std::get<2>(params), std::get<3>(params), std::get<4>(params), std::get<5>(params), std::get<6>(params));
		parallelTaskFinished[taskIndex] = 1;
	}
	System::Void FractalGenerator::Task_OfRecursion(System::Object^ obj) {
		// Unpack arguments
		const auto args = (array<System::Object^>^)obj;
		VecRefWrapper^ R = (VecRefWrapper^)args[0];
		const auto inX = static_cast<float>(args[1]);
		const auto inY = static_cast<float>(args[2]);
		const auto inAngle = static_cast<float>(args[3]);
		const auto inAntiAngle = static_cast<float>(args[4]);
		const auto inSize = static_cast<float>(args[5]);
		const auto inColor = static_cast<uint8_t>(args[6]);
		const auto inFlags = static_cast<int>(args[7]);
		const auto inDepth = static_cast<uint8_t>(args[8]);
#ifdef CUSTOMDEBUG
		const auto threadTime{ std::chrono::steady_clock::now() };
		System::String^ start = "Thread:x" + (int)floor(inX) + "y" + (int)floor(inY) + "s" + (int)floor(inSize) + " start = " + (threadTime - *startTime).count();
#endif
		GenerateDots_OfRecursion(R, inX, inY, inAngle, inAntiAngle, inSize, inColor, inDepth, inFlags);
#ifdef CUSTOMDEBUG
		const auto threadEndTime{ std::chrono::steady_clock::now() };
		Monitor::Enter(taskLock);
		try {
			Log(logString, start + " end = " + (threadEndTime - *startTime).count() + " time = " + (threadEndTime - threadTime).count());
		} finally { Monitor::Exit(taskLock); }
#endif
	}
	System::Void FractalGenerator::Task_Gif(System::Object^ obj) {
		// Unpack arguments
		const auto args = (array<System::Object^>^)obj;
		const int16_t taskIndex = static_cast<int16_t>(args[0]);
		GenerateGif(taskIndex);
	}
#pragma endregion

#pragma region AnimationParameters
	System::Void FractalGenerator::ModFrameParameters(float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color) {
		const auto w = Math::Max(width, height) * f->maxSize, fp = f->childSize;
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
	System::Void FractalGenerator::SwitchParentChild(float& angle, int8_t& spin, uint8_t& color) {
		if (abs(spin) > 1) {
			// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
			angle = -angle;
			spin = -spin;
		}
		color = (3 + color + zoom * childColor[0]) % 3;
	}
	System::Void FractalGenerator::IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, uint8_t& color, const uint8_t blur) {
		const auto blurPeriod = period * blur;
		// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
		angle += spin * (periodAngle * (1 + extraSpin)) / (finalPeriodMultiplier * blurPeriod);
		hueAngle += (hueCycleMultiplier + 3 * extraHue) * (float)hueCycle / (finalPeriodMultiplier * blurPeriod);
		IncFrameSize(size, blurPeriod);
	}
	System::Void FractalGenerator::SetupFrameColorBlend(const float hueAngle, VecRefWrapper^ R) {
		// Prepare Color blending per one dot (hueshifting + iteration correction)
		// So that the color of the dot will slowly approach the combined colors of its childer before it splits
		auto lerp = std::fmod(hueAngle, 1.0f);
		switch ((uint8_t)hueAngle % 3) {
		case 0:
			R->I->FromVector(Vector::Lerp(unitX, unitY, lerp));
			R->H->FromVector(Vector::Lerp(*colorBlend, Y(*colorBlend), lerp));
			break;
		case 1:
			R->I->FromVector(Vector::Lerp(unitY, unitZ, lerp));
			R->H->FromVector(Vector::Lerp(Y(*colorBlend), Z(*colorBlend), lerp));
			break;
		case 2:
			R->I->FromVector(Vector::Lerp(unitZ, unitX, lerp));
			R->H->FromVector(Vector::Lerp(Z(*colorBlend), *colorBlend, lerp));
			break;
		}
	}
#pragma endregion

#pragma region Interface_Calls
	System::Void FractalGenerator::StartGenerate() {
		// start the generator in a separate main thread so that the form can continue being responsive
		mainTask = Task::Run(gcnew Action(this, &FractalGenerator::GenerateAnimation), (cancel = gcnew CancellationTokenSource())->Token);
	}
	System::Void FractalGenerator::ResetGenerator() {
		finalPeriodMultiplier = GetFinalPeriod();
		// A complex expression to calculate the minimum needed hue shift speed to match the loop:
		//hueCycleMultiplier = hueCycle == 0 ? 0 : ((childColor[0] % 3) == 0 ? 3 : ((((childColor[0] % 3) == 1) == (1 == hueCycle) == (zoom == 1) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3) + 1);
		hueCycleMultiplier = hueCycle == 0 ? 0 : childColor[0] % 3 == 0 ? 2 : 1 +
			(childColor[0] % 3 == 1 == (1 == hueCycle) == (1 == zoom) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3;
		// setup bitmap data
		bitmapsFinished = nextBitmap = 0;
		uint16_t frames = debug > 0 ? debug : period * finalPeriodMultiplier;
		if (frames != allocatedFrames) {
			if (allocatedFrames >= 0)
				delete[] bitmapState;
			bitmapState = new uint8_t[(allocatedFrames = frames) + 1];
		}
		bitmap = gcnew array<Bitmap^>(frames);
		bitmapData = gcnew array<BitmapData^>(frames);
		for (int i = frames; 0 <= i; bitmapState[i--] = 0);
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
			if (gifEncoder != nullptr)
				((GifEncoder*)gifEncoder)->close(cancel->Token);
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
	void FractalGenerator::DebugStart() {
		// debug for testing, starts the generator with predetermined setting for easy breakpointing
		// Need to enable the definition on top GeneratorForm.cpp to enable this
		// if enabled you will be unable to use the settings interface!
		SelectFractal(2);
		SetupFractal();
		SelectThreadingDepth();
		SelectDetail(1);
		debug = 7;
		width = 80;
		height = 80;
		parallelType = 1;
		maxTasks = 0;
		maxDepth = -1;//= 2;
		maxGenerationTasks = maxTasks = -1;// 10;
		saturate = 1.0f;
		SelectDetail(.25f);
	}
#pragma endregion

#pragma region Interface_Settings
	bool FractalGenerator::SelectFractal(const uint16_t select) {
		if (this->select == select)
			return true;
		// new fractal definition selected - let the form know to reset and restart me
		this->select = select;
		return false;
	}
	System::Void FractalGenerator::SetupFractal() {
		// setup the new fractal definition, with all parameters reset to 0
		f = fractals[select];
		logBase = logf(GetFractal()->childSize);
		SelectThreadingDepth();
		selectCut = selectAngle = selectColor = -1;
		if(!SelectColor(0))
			SetupColor();
		SelectAngle(0);
		SelectCutFunction(0);
	}
	bool FractalGenerator::SelectAngle(const uint16_t selectAngle) {
		if (this->selectAngle == selectAngle)
			return true;
		this->selectAngle = selectAngle;
		childAngle = f->childAngle == nullptr ? new float[1] { (float)M_PI } : f->childAngle[selectAngle].second;
		periodAngle = f->childCount <= 0 ? 0.0f : std::fmod(childAngle[0], 2.0f * (float)M_PI);
		return false;
	}
	bool FractalGenerator::SelectColor(const uint16_t selectColor) {
		if (this->selectColor == selectColor)
			return true;
		this->selectColor = selectColor;
		return false;
	}
	bool FractalGenerator::SelectColorPalette(const uint8_t selectColorPalette) {
		if (this->selectColorPalette == selectColorPalette)
			return true;
		this->selectColorPalette = selectColorPalette;
		return false;
	}
	System::Void FractalGenerator::SetupColor() {
		// Setup colors
		if (f->childCount > 0) {
			for (int i = f->childCount; 0 <= --i; childColor[i] = f->childColor[selectColor].second[i]);
			// Setup palette
			for (int i = 0; i < f->childCount; ++i)
				childColor[i] = selectColorPalette == 0 ? childColor[i] : (3 - childColor[i]) % 3;
		}
		// Prepare subiteration color blend
		InitColorBlend();
	}
	bool FractalGenerator::SelectCutFunction(const uint16_t selectCut) {
		if (this->selectCut == selectCut)
			return true;
		this->selectCut = selectCut;
		cutFunction = f->cutFunction == nullptr || f->cutFunction->first == "" ? nullptr
			: &f->cutFunction[selectCut].second;
		return false;
	}
#pragma endregion

#pragma region Interface_Getters
	uint16_t FractalGenerator::GetFinalPeriod() {
		// get the multiplier of the basic period required to get to a seamless loop
		uint16_t m = hueCycle == 0 && childColor[0] > 0 ? periodMultiplier * 3 : periodMultiplier;
		return Math::Abs(defaultSpin) > 1 || (defaultSpin == 0 && childAngle[0] < 2 * M_PI) ? 2 * m : m;
	}
	std::string FractalGenerator::ConvertToStdString(System::String^ managedString) {
		using namespace System::Runtime::InteropServices;
		const auto chars = (const char*)(Marshal::StringToHGlobalAnsi(managedString)).ToPointer();
		const auto nativeString(chars);
		Marshal::FreeHGlobal(System::IntPtr((void*)chars));
		return nativeString;
	}
#pragma endregion

}
