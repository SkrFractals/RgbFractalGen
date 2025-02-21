//#define PARALLELDEBUG

#include "FractalGenerator.h"
#include <omp.h>
#include "GifEncoder.h"

// CLR VS CPP:
#ifdef CLR
#define PACK(X,Y) pack(X, Y)
#define CANCEL token.IsCancellationRequested
#define GETTUPLEPACK const double& inXY = std::get<1>(params); const double& AA = std::get<2>(params);
#define GIFCANCEL true, &gifCancel->Token
#define EXCEPTION Exception^
#define ABS Math::Abs
#define MIN Math::Min
#define MAX Math::Max
#define LOG Math::Log
#define FLOOR Math::Floor
#define CEIL Math::Ceiling
#define TOSTRING(X) X
#else
#define PACK(X,Y) { X, Y }
#define CANCEL cancelRequested
#define GETTUPLEPACK const std::pair<float, float>& inXY = std::get<1>(params); const std::pair<float, float>& AA = std::get<2>(params);
#define GIFCANCEL false, &gifCancelRequested
#define EXCEPTION Exception
#define ABS std::abs
#define MIN std::min
#define MAX std::max
#define LOG std::log
#define FLOOR std::floor
#define CEIL std::ceil
#define TOSTRING(X) std::to_string(X)
#endif

// UNIVERSAL:
#define M_PI 3.14159265358979323846
//#define Y(V) = Vector<float>(gcnew array<float>(3){ V[2], V[0], V[1] })
//#define Z(V) = Vector<float>(gcnew array<float>(3){ V[1], V[2], V[0] })
#define FA(N) std::vector<std::string, float*>>(N)
#define F(N) new float[N]
#define BA(N) std::vector<std::pair<std::string, uint8_t*>>(N)
#define B(N) new uint8_t[N]
#define CA(N) std::vector<std::pair<int32_t, std::vector<int32_t>><(N)
#define FE { "", new float[0] { } }
#define BE { "", new uint8_t[0] { } }
#define CE { -1 , empty }
#define CHILDPARAMS PACK(task.applyWidth * .5f, task.applyHeight * .5f), PACK(angle, ABS(spin) > 1 ? 2 * angle : 0), color, startParam, 0
#define STARTITERATION GETPREITERATE UNPACKXY UNPACKAA IFDOT
#define UNPACKXY float inX, inY; unpack(inXY, inX, inY);
#define UNPACKAA float inAngle, inAntiAngle; unpack(AA, inAngle, inAntiAngle);
#define FORCHILD for (int i = 0; i < f->childCount; ++i)
#define IFDOT if (std::get<0>(newPreIterated) < applyDetail)
#define GETPREITERATE const auto& preIterated = task.preIterate[inDepth]; const auto& newPreIterated = task.preIterate[++inDepth];
#define GETXY const auto& XY = std::get<2>(preIterated)[i];
#define NEWXY const float newX = inX + XY.first * cs - XY.second * sn, newY = inY - XY.second * cs - XY.first * sn;
#define NEWCOLOR applyPreviewMode && inDepth > 1 ? inColor : (uint8_t)((inColor + childColor[i]) % 3)
#define SINCOS const auto cs = cos(inAngle), sn = sin(inAngle);
#define CALCFLAGS if (CANCEL) return; const auto newFlags = CalculateFlags(i, inFlags); if (newFlags < 0) continue;
#define TESTSIZE if (TestSize(task, newX, newY, std::get<0>(preIterated)))
#define APPLYDOT ApplyDot(task, i + f->childCount * (newFlags & ((static_cast<int64_t>(1) << f->childCount) - 1)), newX, newY, std::get<1>(newPreIterated), NEWCOLOR);
#define ITERATECHILDREN CALCFLAGS GETXY SINCOS NEWXY TESTSIZE
#define NEWCHILD PACK(newX, newY), i == 0 ? PACK(inAngle + childAngle[i] - inAntiAngle, -inAntiAngle) : PACK(inAngle + childAngle[i], inAntiAngle), NEWCOLOR, newFlags, inDepth
#define FINISHTASKS(M) bool tasksRemaining = true; uint8_t i = 3; uint16_t taskIndex = 0; while(FinishTasks(M, i, tasksRemaining, taskIndex))
#define CANCELDOTS { if (stateIndex >= 0) tasks[stateIndex].state = TaskState::Done; return; }

namespace RgbFractalGenClr {

#ifdef CLR
	using namespace System::IO;
#ifdef CUSTOMDEBUG
	using namespace System::Diagnostics;
#endif
#ifdef PARALLELDEBUG
	using namespace System::Diagnostics;
#endif
#endif

#pragma region Init
	FractalGenerator::FractalGenerator() {
		Fractal::initialize();
		std::random_device rd;
		randomGenerator = new std::mt19937(rd());
		randomDist = new std::uniform_real_distribution<float>(0.0f, 1.0f);
		hash = new std::map<std::string, uint32_t>();
		colorBlends = new std::map<int64_t, std::array<Vector, 3>>();
		selectCutparam = debug = selectDefaultHue = selectDefaultZoom = selectDefaultAngle = selectExtraSpin = selectExtraHue = 0;
		selectZoom = 1;
		selectFractal = selectChildColor = selectChildAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks = allocatedFrames = selectSpin = selectMaxIterations = -1;
		selectParallelType = ParallelType::OfAnimation;
		selectGenerationType = GenerationType::EncodeGIF;
		f = nullptr;
		gifEncoder = nullptr;
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

		/*const std::string& name,
			const int8_t& childCount,
			const float& childSize,
			const float& maxSize,
			const float& minSize,
			const float& cutSize,
			float* childX,
			float* childY,
			std::vector<std::pair<std::string, float*>> childAngle,
			std::vector<std::pair<std::string, uint8_t*>> childColor,
			std::vector<std::pair<int32_t, std::vector<int32_t>>> cutFunction*/

		std::vector<int32_t> empty;

		fractals = new std::vector<Fractal*>(0);
		fractals->reserve(10);

		fractals->push_back(new Fractal("Void", 0, 1000, 1, .1f, 1, F(1) { 0 }, F(1) { 0 }, { { "X", F(1) { 0.0f } } }, { { "X", B(1) { 0 } } }, { { -1, { 0 } } }));

		fractals->push_back(new Fractal("TriTree", 10, 4, .2f, .05f, .9f,
			F(10) { 0, -stt, 2 * stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt },
			F(10) { 0, -1.5f, 0, 1.5f, 1.5f, 3, 1.5f, -1.5f, -3, -1.5f },
			{ 
			{ "RingTree", F(10) { SYMMETRIC + pi23, pi, pi + pi23, pi + 2 * pi23, 0, 0, pi23, pi23, 2 * pi23, 2 * pi23 } },
			{ "BeamTree_Beams", F(10) { pi / 3, 0, pi23, pi43, pi, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43 } },
			{ "BeamTree_OuterJoint", F(10) { pi / 3, 0, pi23, pi43, pi + pi23, pi + pi23, pi, pi, pi + pi43, pi + pi43 } },
			{ "BeamTree_InnerJoint", F(10) { pi / 3, 0, pi23, pi43, pi, pi, pi, pi, pi, pi } }
			},
			{
			{ "Center", B(10) { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
			{ "CenterNeighbors", B(10) { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0 } }
			},						   
			{
				{ 1, { -1534 } },	// NoChildComplex
				{ 8, { } },			// NoBeam
				{ 9, { } },			// OuterJoint
				{ 10, { } }			// InnerJoint
			}
		));



		/*fractals = new Fractal * [10] {
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
					{ "NoChild", Fractal::NoChildComplexParam, nullptr },
					{ "NoBackDiag", Fractal::NoBackDiag, nullptr },
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
					{ "NoCorner", Fractal::Tetraflake_NoCornerParam, nullptr },
					{ "NoCorner + RadHoles", Fractal::Tetraflake_NoCornerRadHolesParam, nullptr },
					{ "NoCorner + CornerHoles", Fractal::Tetraflake_NoCornerCornerHolesParam, nullptr },
					{ "NoCorner + TriangleHoles", Fractal::Tetraflake_NoCornerTriangleHolesParam, nullptr },
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
					}, CA(3) {
						{ "NoChild", Fractal::NoChildComplexParam, nullptr },
						{ "NoBackDiag", Fractal::NoBackDiag, nullptr },
						CE
					 } 
				),
				new Fractal("Pentaflake", 6, pfs, .2f * pfs, .15f, .9f, pfx, pfy,
					FA(3) { 
						{ "Classic",  F(6) { 2 * pi / 10, 0, 0, 0, 0, 0 } }, 
						{ "No Center Rotation",  F(6) { 2 * pi + 2 * pi / 5, 0, 0, 0, 0, 0 } }, 
						FE 
					}, BA(2) { { "Center", B(6) { 1, 0, 0, 0, 0, 0 } }, BE },
					CA(3) {
						{ "NoChild", Fractal::NoChildComplexParam, nullptr },
						{ "NoBackDiag", Fractal::NoBackDiag, nullptr },
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
						{ "NoChild", Fractal::NoChildComplexParam, nullptr },
						{ "NoBackDiag", Fractal::NoBackDiag, nullptr },
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
		};*/
		maxChildren = 1;
		for (int32_t i = static_cast<int32_t>(fractals->size()); 0 <= --i;)
			if ((*fractals)[i]->childCount > maxChildren)
				maxChildren = (*fractals)[i]->childCount;
		childColor = new uint8_t[maxChildren];
	}
	VOIDTYPE FractalGenerator::DeleteEncoder() {
		if (gifEncoder != nullptr) {
			delete gifEncoder;
			gifEncoder = nullptr;
		}
	}
	VOIDTYPE FractalGenerator::DeleteBuffer(const FractalTask& task, const uint16_t vh) {
		const auto& buffT = task.buffer;
		const auto& voidT = task.voidDepth;
		const auto& noiseT = task.voidNoise;
		for (auto y = 0; y < allocatedHeight; delete voidT[y++])
			delete buffT[y];
		for (auto y = 0; y < vh; delete noiseT[y++]);
		delete[] buffT;
		delete[] voidT;
		delete[] noiseT;
	}
	VOIDTYPE FractalGenerator::NewBuffer(FractalTask& task, const uint16_t vw, const uint16_t vh) {
		const auto& voidT = task.voidDepth = new int16_t * [selectHeight];
		const auto& buffT = task.buffer = new Vector * [selectHeight];
		const auto& noiseT = task.voidNoise = new Vector*[vh];
		for (auto y = 0; y < selectHeight; voidT[y++] = new int16_t[selectWidth]) {
			const auto& buffY = buffT[y] = new Vector[selectWidth];
			for (auto x = 0; x < selectWidth; buffY[x++] = zero);
		}
		for (auto y = 0; y < vh; noiseT[y++] = new Vector[vw]);
	}
#pragma endregion

#pragma region Generate_Tasks
	VOIDTYPE FractalGenerator::GenerateAnimation() {
#ifdef PARALLELDEBUG
		Debug::WriteLine("GENERATEANIMATION STARTER");
#endif
#ifdef CUSTOMDEBUG
		// Start a new DebugLog
		logString = "";
		Log(logString, "New Generate()");
		startTime = &std::chrono::steady_clock::now();
#endif
		StartGif();
		// Initialize the starting default animation values
		auto size = 2400.0f, angle = selectDefaultAngle * (float)M_PI / 180.0f, hueAngle = selectDefaultHue / 120.0f;
		uint8_t color = 0;
		isWritingBitmaps = 2;
		bloom1 = selectBloom + 1;
		ambnoise = selectAmbient * selectNoise;
		auto& dist = *randomDist;
		auto& gen = *randomGenerator;
		int8_t spin = static_cast<int8_t>(selectSpin < -1 ? (int8_t)(dist(gen) * 4 - 1) : selectSpin);
		applyBlur = (short)(selectBlur + 1);
		if (applyPreviewMode = selectPreviewMode) {
			const auto w = MAX(selectWidth, selectHeight) * f->maxSize * 0.1f;
			size = w * f->childSize * .9f;
		}
		auto toFinishAnimation = true;
		for (auto i = selectDefaultZoom < 0
			 ? (int8_t)(dist(gen) * selectPeriod * finalPeriodMultiplier)
			 : (selectDefaultZoom % (selectPeriod * finalPeriodMultiplier));
			 0 <= --i; IncFrameSize(size, selectPeriod));
		// Pregenerate color blends
		int64_t _startCutParam = startCutParam;
		if (applyGenerationType < GenerationType::AllParam) 
			PregenerateParam(static_cast<int32_t>(0), colorBlends, _startCutParam);
		startCutParam = _startCutParam;
		// Generate the images
		while (!CANCEL) {
			toFinishAnimation |= ApplyGenerationType();
			if (restartGif) {
				restartGif = false;
				toFinishAnimation = true;
				// wait for all tasks to finish to preserve integrity, especially including gifs, and only return true if it tries to start new ones, so they actually finish:
				FINISHTASKS(true);
				StopGif(-1);
				StartGif();
			}
			if (bitmapsFinished < frames) {
				// Initialize buffers (delete and reset if size changed)
				if ((applyMaxTasks = MAX(static_cast<int16_t>(MINTASKS), selectMaxTasks)) != allocatedTasks) {
					if (allocatedTasks >= 0) {
						for (uint16_t t = 0; t < allocatedTasks; ++t) {
							auto& task = tasks[t];
							if (task.taskStarted) {
#ifdef PARALLELDEBUG
								Debug::WriteLine("GenerateAnimation task still running: " + t);
#endif
								Join(task);
							}
							DeleteBuffer(task, allocatedHeight / applyVoid + 2);
							delete[] task.buffer;
							delete[] task.voidDepth;
						}
						delete[] tasks;
						delete[] tuples;
					}
					allocatedWidth = selectWidth; 
					allocatedHeight = selectHeight;
					SetRect();
					tasks = new FractalTask[allocatedTasks = applyMaxTasks];
					tuples = new std::tuple<uint16_t, TUPLEPACK, uint8_t, int64_t, uint8_t>[applyMaxTasks * 8];
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					uint32_t vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					for (int16_t t = applyMaxTasks; 0 <= --t;) {
						auto& task = tasks[t];
						task.taskIndex = t;
						NewBuffer(task, vw, vh);
					}
					parallelTasks = gcnew array<Task^>(applyMaxTasks);
					for (uint16_t t = 0; t < applyMaxTasks; parallelTasks[t++] = nullptr);
					SetMaxIterations();
					
				}
				if (selectHeight != allocatedHeight) {
					uint32_t ah = allocatedHeight / applyVoid + 2;
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					uint32_t vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					for (uint16_t t = 0; t < applyMaxTasks; ++t) {
						auto& task = tasks[t];
						DeleteBuffer(task, ah);
						NewBuffer(task, vw, vh);
					}
					allocatedWidth = selectWidth;
					allocatedHeight = selectHeight;
					SetRect();
				}
				if (selectWidth != allocatedWidth) {
					uint32_t ah = allocatedHeight / applyVoid + 2;
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					uint32_t vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					for (uint16_t t = 0; t < applyMaxTasks; ++t) {
						const auto& task = tasks[t];
						const auto& buffT = task.buffer;
						const auto& voidT = task.voidDepth;
						const auto& noiseT = task.voidNoise;
						// Reallocate voidDepth and buffer
						for (uint16_t y = 0; y < allocatedHeight; voidT[y++] = new int16_t[selectWidth]) {
							delete buffT[y];
							delete voidT[y];
							buffT[y] = new Vector[selectWidth];
						}
						for (uint16_t y = allocatedHeight; y < selectHeight; voidT[y++] = new int16_t[selectWidth])
							buffT[y] = new Vector[selectWidth];
						// Reallocate voidNoise
						for (uint16_t y = 0; y < ah; noiseT[y++] = new Vector[vw])
							delete noiseT[y];
						for (uint16_t y = ah; y < vh; noiseT[y++] = new Vector[vw]);
					}
					allocatedWidth = selectWidth;
					SetRect();
				}
				// reallocate noise buffer if the noise resolution has changed
				if (applyVoid != selectVoid + 1) {
					uint32_t ah = allocatedHeight / applyVoid + 2;
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					uint32_t vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					for (uint16_t t = 0; t < applyMaxTasks; ++t) {
						const auto& task = tasks[t];
						const auto& noiseT = task.voidNoise;
						// Reallocate voidNoise
						for (uint16_t y = 0; y < ah; noiseT[y++] = new Vector[vw])
							delete noiseT[y];
						for (uint16_t y = ah; y < vh; noiseT[y++] = new Vector[vw]);
					}
				}
				applyDetail = selectDetail;
				if (selectMaxIterations != applyMaxIterations) {
					applyMaxIterations = selectMaxIterations;
					for (int t = 0; t < allocatedTasks; InitPreiterate(tasks[t++].preIterate));
				}
				// Wait if no more frames to generate
				if (bitmapsFinished >= GetGenerateLength())
					continue;
				// Use the selected ParallelType, or OfDepth if OnlyImage (if you want a GenerateDots_SingleTask like OfAnimation with OnlyImage, set the maxTasks to <= 2)
				applyParallelType = selectParallelType;
				// Image parallelism
				//imageTasks = applyParallelType == 2 ? [] : null;
				
				FractalTask task;
				for (int8_t i = 3; i > 0; --i) {
					for (auto tasksRemaining = true; tasksRemaining; MakeDebugString()) {
						tasksRemaining = false;
						for (int16_t t = applyMaxTasks; 0 <= --t;) {
							auto& task = tasks[t];
							if (IsTaskStillRunning(task))
								tasksRemaining = true; // Must finish all Dots threads, and if in main loop all secondary threads too (OnDepth can continu back to main loop when secondary threads are running so it could start a new OnDepth loop)
							else {
								if (token.IsCancellationRequested  // Cancel Request forbids any new threads to start
									|| selectMaxTasks != applyMaxTasks // changing these settings yout exit, then they get updated and restart the main loop with them updated (except onDepth which must finish first)
									|| applyParallelType != selectParallelType
									|| selectGenerationType != applyGenerationType
								) continue;
								if (TryWriteBitmaps(task) || TryFinishBitmap(task))
									tasksRemaining = true;  // in the main loop we try Bitmap finishing and writing secondary threads (onDepth loop would get stuck )
								else {
									if (nextBitmap >= GetGenerateLength())
										return;// The task is finished, no need to wait for this one
									const auto bmp = nextBitmap++;
									//ModFrameParameters(task.applyWidth, task.applyHeight, bmp >= previewFrames, ref size, ref angle, ref spin, ref hueAngle, ref color);
									const float _size = bmp < previewFrames ? size / (1 << (previewFrames - bmp - 1)) : size;
									//_angle = angle, _hueAngle = hueAngle;var _spin = spin;var _color = color;
									//bitmapState[nextBitmap] = BitmapState.Dots; // i was getting queued state tasks, this solved that, so that just means they take a while to get started, not an error
									if (applyParallelType > ParallelType::OfAnimation || applyMaxTasks <= MINTASKS || bmp < previewFrames)
										GenerateDots(bmp, static_cast<int16_t>(-t - 1), _size, angle, spin, hueAngle, color);
									else
#ifdef CLR
										Start(t, bmp, gcnew Action<Object^>(this, &FractalGenerator::Task_Dots), gcnew array<System::Object^>{ bmp, static_cast<int16_t>(t + 1), size, angle, spin, hueAngle, color });
#else
										tasks[t].Start(bmp).task = std::thread(&FractalGenerator::GenerateDots, this, bmp, static_cast<int16_t>(t + 1), size, angle, spin, hueAngle, color);
#endif
									if (bmp >= previewFrames)
										IncFrameParameters(size, angle, spin, hueAngle, 1);
									tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
								}
							}
						}
						if (tasksRemaining)
							i = 3;
					}
					ApplyGenerationType();
				}


				/*
				FINISHTASKS(true) {
					if (nextBitmap >= GetGenerateLength())
						return;// The task is finished, no need to wait for this one
					const auto bmp = nextBitmap++;
					//ModFrameParameters(task.applyWidth, task.applyHeight, bmp >= previewFrames, ref size, ref angle, ref spin, ref hueAngle, ref color);
					const float _size = bmp < previewFrames ? size / (1 << (previewFrames - bmp - 1)) : size;
					//_angle = angle, _hueAngle = hueAngle;var _spin = spin;var _color = color;
					//bitmapState[nextBitmap] = BitmapState.Dots; // i was getting queued state tasks, this solved that, so that just means they take a while to get started, not an error
					if (applyParallelType > ParallelType::OfAnimation || applyMaxTasks <= MINTASKS || bmp < previewFrames)
						GenerateDots(bmp, -static_cast<int16_t>(taskIndex) - 1, _size, angle, spin, hueAngle, color);
					else
#ifdef CLR
						Start(taskIndex, bmp, gcnew Action<Object^>(this, &FractalGenerator::Task_Dots), gcnew array<System::Object^>{ bmp, static_cast<int16_t>(taskIndex) + 1, size, angle, spin, hueAngle, color });
#else
						tasks[taskIndex].Start(bmp).task = std::thread(&FractalGenerator::GenerateDots, this, bmp, static_cast<int16_t>(taskIndex) + 1, size, angle, spin, hueAngle, color);
#endif
					if (bmp >= previewFrames)
						IncFrameParameters(size, angle, spin, hueAngle, 1);
					tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
				}
				*/
			} else if (toFinishAnimation) {
				toFinishAnimation = false;
				//TestEncoder();
				FinishAnimation();
#ifdef CUSTOMDEBUG
				long long n = frames * 100000;
				logString = "CppManaged:\nInit: " + initTimes / n
					+ "\nIter: " + iterTimes / n
					+ "\nVoid: " + voidTimes / n
					+ "\nDraw: " + drawTimes / n
					+ "\nGifs: " + gifsTimes / n
					+ "\n" + logString;
				File::WriteAllText("log.txt", logString);
#endif
			}
		}
	}
	bool FractalGenerator::ApplyGenerationType() {
		if (applyGenerationType == selectGenerationType)
			return false;
		if (applyGenerationType >= GenerationType::OnlyImage && applyGenerationType <= GenerationType::AnimationRAM && selectGenerationType >= GenerationType::EncodeGIF) {
			bitmapsFinished = MIN(static_cast<uint32_t>(previewFrames), bitmapsFinished);
			if (gifEncoder == nullptr)
				StartGif();
		}
		applyGenerationType = selectGenerationType;
		return true;
	}
	VOIDTYPE FractalGenerator::FinishAnimation() {
		// Wait for threads to finish
		bool tasksRunning = true;
		while (tasksRunning) {
			for (auto t = allocatedTasks; 0 <= --t; Join(tasks[t]));
			tasksRunning = false;
			for (auto t = allocatedTasks; 0 <= --t; tasksRunning |= tasks[t].taskStarted);
		}
		// Unlock unfinished bitmaps:
		UnlockBitmaps();
		FinishGif();
		MakeDebugString();
		if (applyGenerationType == GenerationType::HashParam) {
			// TODO HASH
			/*std::string output = "";
			foreach(var i in hash)
				output += "," + i.Value;
			File.WriteAllText("hash.txt", output);*/
		}
	}
	VOIDTYPE FractalGenerator::StartGif() {
		// Open a temp file to presave GIF to - Use xiaozhuai's GifEncoder
		gifSuccess = false;
		DeleteEncoder();
		hash->clear();
		uint8_t gifIndex = 0;
#ifdef CLR
		gifToken = (gifCancel = gcnew CancellationTokenSource())->Token;
#endif
		switch (selectGenerationType) {
		case GenerationType::EncodeGIF:
		case GenerationType::GlobalGIF:
		case GenerationType::AllParam:

			GifEncoder* e;
			gifEncoder = e = new GifEncoder();
			while (gifIndex < 255) {
				gifTempPath = "gif" + gifIndex.ToString() + ".tmp";
				if (!e->open_parallel(Fractal::ConvertToStdString(gifTempPath), selectWidth, selectHeight, 1, applyGenerationType == GenerationType::GlobalGIF, 0, GIFCANCEL)) {
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
			break;
		case GenerationType::Mp4:
			// TODO MP4
			/*mp4Encoder = new();
			while (gifIndex < 255) {
				gifTempPath = "gif" + gifIndex.ToString() + ".tmp";
				if (!mp4Encoder.Start(selectWidth, selectHeight, gifTempPath, selectFps)) {
					++gifIndex;
					continue;
				} else break;
			}
			if (gifIndex == 255)
				gifEncoder = null;*/
			break;
		}
		// Flag the already encoded bitmaps to be encoded again;
		ReencodeBitmaps();
		bitmapsFinished = MIN(static_cast<uint32_t>(previewFrames), bitmapsFinished);
	}
	VOIDTYPE FractalGenerator::FinishGif() {
		// Save the temp GIF file
		gifSuccess = 0;
		if (!CANCEL && applyGenerationType >= GenerationType::EncodeGIF && applyGenerationType <= GenerationType::AllParam) {
			if (gifEncoder != nullptr) {
				GifEncoder* e = (GifEncoder*)gifEncoder;
				if (!e->isFinishedAnimation())
					e->close(true, GIFCANCEL);

				while (gifEncoder != nullptr) {
					switch (e->tryWrite(true)) {
					case GifEncoderTryWriteResult::Failed:
						StopGif(-1);
						return;
					case GifEncoderTryWriteResult::Waiting:
						// This should never be hit, because the code reaches this loop only after getting cancelled or every frame finished
						Thread::Sleep(100);
						break;
					case GifEncoderTryWriteResult::FinishedFrame:
						break;
					case GifEncoderTryWriteResult::FinishedAnimation:
						gifSuccess = MAX(selectWidth, selectHeight);
						// This will follow with gifEncoder.IsFinished()
						break;
					}
					if (e->isFinishedAnimation())
						break;
				}
			}
			if (mp4Encoder != nullptr) {
				// TODO MP4
				/*mp4Encoder.Finish();
				gifSuccess = -1;*/
			}
		}
		DeleteEncoder();
		// TODO MP4
		mp4Encoder = nullptr;
	}
	VOIDTYPE FractalGenerator::StopGif(const int16_t taskIndex) {
#ifdef CLR
		if(gifCancel != nullptr)
			gifCancel->Cancel();
#else
		gifCancelRequested = true;
#endif
		applyGenerationType = GenerationType::AnimationRAM;
		for (int t = 0; t < taskIndex; ++t) {
			auto& task = tasks[t];
			if (task.bitmapIndex < 0) {
				if (taskIndex != t)
					Join(task);
			} else if (bitmapState[task.bitmapIndex] >= BitmapState::Encoding && bitmapState[task.bitmapIndex] <= BitmapState::EncodingFinished) {
				if (taskIndex != t)
					Join(task);
				bitmapState[task.bitmapIndex] = BitmapState::DrawingFinished;
			}
		}
		((GifEncoder*)gifEncoder)->abort();
		DeleteEncoder();
#ifndef CLR
		gifCancelRequested = false;
#endif
	}
	VOIDTYPE FractalGenerator::GenerateDots(const uint32_t& bitmapIndex, const int16_t& stateIndex, float size, float angle, int8_t spin, float hueAngle, uint8_t color) {
#ifdef CUSTOMDEBUG
		STRING threadString = "";
		const auto initTime{ std::chrono::steady_clock::now() };
#endif
		bitmapState[bitmapIndex] = BitmapState::Dots;
		// Init buffer with zeroes
		uint16_t taskIndex = ABS(stateIndex) - 1;
		auto& task = tasks[taskIndex];
		task.bitmapIndex = bitmapIndex;
		auto& buffT = task.buffer;
		PreviewResolution(task);
		for (auto y = 0; y < task.applyHeight; ++y) {
			const auto& buffY = buffT[y];
			for (auto x = 0; x < task.applyWidth; buffY[x++] = zero);
		}
#ifdef CUSTOMDEBUG
		const auto initElapsed = (std::chrono::steady_clock::now() - initTime).count();
		Log(threadString, "Init:" + bitmapIndex + " time = " + initElapsed);
		const auto iterTime{ std::chrono::steady_clock::now() };
#endif
		if (CANCEL) CANCELDOTS
		// Generate the fractal frame recursively
		//auto& H = task.H;
		//auto& I = task.I;
		for (int b = 0; b < applyBlur; ++b) {
			ModFrameParameters(task.applyWidth, task.applyHeight, size, angle, spin, hueAngle, color);
			// Preiterate values that change the same way as iteration goes deeper, so they only get calculated once
			float inSize = size;
			auto& preIterateTask = task.preIterate;
			if (preIterateTask == nullptr) 
				InitPreiterate(preIterateTask);
			for (int i = 0; i < applyMaxIterations; ++i) {
				auto& preIterated = preIterateTask[i];
				const auto inDetail = -inSize * MAX(-1.0f, std::get<1>(preIterated) = static_cast<float>(LOG(applyDetail / (std::get<0>(preIterated) = inSize))) / logBase);
				auto& XY = std::get<2>(preIterated);
				if (XY == nullptr)
					XY = new std::pair<float, float>[maxChildren];
				for (int c = 0; c < f->childCount; ++c)
					XY[c] = std::pair<float, float>(f->childX[c] * inDetail, f->childY[c] * inDetail);
				inSize /= f->childSize;
			}
			// Prepare Color blending per one dot (hueshifting + iteration correction) and starting cutparameter
			// So that the color of the dot will slowly approach the combined colors of its childer before it splits
			int64_t startParam = 0;
			auto F = applyGenerationType >= GenerationType::AllParam ? &task.F : colorBlends;
			if (applyGenerationType >= GenerationType::AllParam)
				PregenerateParam(bitmapIndex, F, startParam);
			else startParam = startCutParam;
			auto lerp = std::fmod(hueAngle, 1.0f);
			task.H.clear();
			task.huemod = static_cast<uint8_t>(std::fmod(hueAngle, 3.0f));
			task.I[0] = Vector::Lerp(unitX, unitY, lerp); 
			task.I[1] = Vector::Lerp(unitY, unitZ, lerp);
			task.I[2] = Vector::Lerp(unitZ, unitX, lerp);
			for(auto& c : *F) {
				const auto& v = c.second;
				task.H[c.first] = std::array<Vector, 3> { Vector::Lerp(v[0], v[1], lerp), Vector::Lerp(v[1], v[2], lerp), Vector::Lerp(v[2], v[0], lerp) };
			}
			if (applyMaxTasks <= MINTASKS || applyParallelType != ParallelType::OfDepth && bitmapIndex >= previewFrames)
				GenerateDots_SingleTask(task, CHILDPARAMS);
			else {
				tuples[0] = { task.taskIndex, CHILDPARAMS };
				GenerateDots_OfDepth(bitmapIndex);
			}
			/* //This OfRecursion parallelism mode is deprecated, use OfDepth instead, it's better anyway
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
			if (bitmapIndex >= previewFrames)
				IncFrameParameters(size, angle, spin, hueAngle, applyBlur);
			if (CANCEL) CANCELDOTS
		}
#ifdef CUSTOMDEBUG
		const auto iterElapsed = (std::chrono::steady_clock::now() - iterTime).count();
		Log(threadString, "Iter:" + bitmapIndex + " time = " + iterElapsed);
#ifdef CLR
		Monitor::Enter(taskLock);
		try {
#else
		taskLock.lock();
#endif
			initTimes += initElapsed;
			iterTimes += iterElapsed;
			Log(logString, threadString);
#ifdef CLR
		} finally { Monitor::Exit(taskLock); }
#else
		taskLock.unlock();
#endif
#endif
		if (stateIndex >= 0) // OfAnimation - continue directly with the nexts steps such as void and gif in this same task:
			GenerateImage(taskIndex);
		else // OfDepth - start continuation in a new task:
#ifdef CLR
			Start(taskIndex, bitmapIndex, gcnew Action<Object^>(this, &FractalGenerator::Task_Image), gcnew array<System::Object^>{ taskIndex });
#else
			task.Start(bmp).task = std::thread(&FractalGenerator::GenerateImage, this, taskIndex);
#endif
	}
	VOIDTYPE FractalGenerator::GenerateImage(const uint16_t taskIndex) {
#ifdef CUSTOMDEBUG
		STRING threadString = "";
		const auto voidTime{ std::chrono::steady_clock::now() };
#endif
		auto& task = tasks[taskIndex];
		// Generate the grey void areas
		bitmapState[task.bitmapIndex] = BitmapState::Void;
		auto& voidT = task.voidDepth;
		auto& buffT = task.buffer;
		auto& queueT = task.voidQueue;
		task.lightNormalizer = 0.1f;
		task.voidDepthMax = 1.0f;
		int16_t voidYX;
		const uint16_t w1 = task.applyWidth - 1, h1 = task.applyHeight - 1;
		// Old SIMD vector code I couldn't get to work
		//Vector<float> *buffY;
		int16_t* voidY, * void0, * voidH, * voidP, * voidM;
		if (selectAmbient > 0) {
			// Void Depth Seed points (no points, no borders), leet the void depth generator know where to start incrementing the depth
			float lightMax;
			for (uint16_t y = 1; y < h1; ++y) {
				if (CANCEL) {
					task.state = TaskState::Done;
					std::swap(queueT, std::queue<std::pair<int16_t, int16_t>>()); // Fast swap-and-drop
					return;
				}
				const auto buffY = buffT[y];
				voidY = voidT[y];
				for (uint16_t x = 1; x < w1; ++x) {
					auto& buffYX = buffY[x];
					// Old SIMD vector code I couldn't get to work
					//lightNormalizer = MAX(lightNormalizer, lightMax = MAX(buffYX[0], MAX(buffYX[1], buffYX[2])));
					task.lightNormalizer = MAX(task.lightNormalizer, lightMax = buffYX.Max());
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
			for (uint16_t x = 0; x < task.applyWidth; ++x) {
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
				voidMax = MAX(voidMax, voidYX = voidY[x] + 1);
				if (xp < task.applyWidth && (voidY[xp] == -1)) { voidY[xp] = voidYX; queueT.push({ y, xp }); }
				if (yp < task.applyHeight && ((voidP = voidT[yp])[x] == -1)) { voidP[x] = voidYX;  queueT.push({ yp, x }); }
				if (xm >= 0 && (voidY[xm] == -1)) { voidY[xm] = voidYX; queueT.push({ y, xm }); }
				if (ym >= 0 && ((voidM = voidT[ym])[x] == -1)) { voidM[x] = voidYX;  queueT.push({ ym, x }); }
			}
			task.voidDepthMax = voidMax;
		} else
			for (uint16_t y = 0; y < selectHeight; y++) {
				if (CANCEL) {
					task.state = TaskState::Done;
					return;
				}
				const auto buffY = buffT[y];
				for (uint16_t x = 0; x < task.applyWidth; x++) {
					auto& buffYX = buffY[x];
					// Old SIMD vector code I couldn't get to work
					//lightNormalizer = MAX(lightNormalizer, MAX(buffYX[0], MAX(buffYX[1], buffYX[2])));
					task.lightNormalizer = MAX(task.lightNormalizer, buffYX.Max());
				}
			}
		task.lightNormalizer = selectBrightness * 2.55f / task.lightNormalizer;
		if (CANCEL) {
			task.state = TaskState::Done;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto voidElapsed = (std::chrono::steady_clock::now() - voidTime).count();
		Log(threadString, "Void:" + task.bitmapIndex + " time = " + voidElapsed);
		const auto drawTime{ std::chrono::steady_clock::now() };
#endif
		// Make a locked bitmap, remember the locked state
		NewBitmap(task.bitmapIndex, task.applyWidth, task.applyHeight);


		uint8_t* p = bitmap[task.bitmapIndex];
		bitmapState[task.bitmapIndex] = BitmapState::Drawing;
		// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
		// Switch between th selected settings such as saturation, noise, image parallelism...
		const auto& cbuffT = const_cast<const Vector**&>(buffT);
		const auto& cvoidT = const_cast<const int16_t**&>(voidT);
		int16_t maxGenerationTasks = MAX(static_cast<int16_t>(1), static_cast<int16_t>(applyMaxTasks - 1));
		uint16_t wv = task.applyWidth / applyVoid + 2;
		auto stride = strides[task.bitmapIndex];//3 * selectWidth;
		if ((applyParallelType > ParallelType::OfAnimation || task.bitmapIndex <= previewFrames) && maxGenerationTasks > 1) {
			// Multi Threaded:
			if (ambnoise > 0 && applyGenerationType != GenerationType::HashParam) {
#pragma omp parallel for num_threads(maxGenerationTasks)
				for (int y = 0; y < task.applyHeight / applyVoid + 2; ++y) {
					std::random_device rd;
					std::mt19937 randGen(rd());
					std::uniform_real_distribution<float> dist(0.0f, ambnoise);
					auto& v = task.voidNoise[y];
					for (int x = 0; x < wv; v[x++] = Vector(dist(randGen), dist(randGen), dist(randGen)));
				}
				if (selectSaturate > 0.0) {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < task.applyHeight; ++y) {
						if (CANCEL)
							continue;
						auto rp = p + y * stride;
						NoiseSaturate(task, y, rp);
					}
				} else {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < task.applyHeight; ++y) {
						if (CANCEL)
							continue;
						uint8_t* rp = p + y * stride;
						NoiseNoSaturate(task, y, rp);
					}
				}
			} else {
				if (selectSaturate > 0.0) {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < task.applyHeight; ++y) {
						if (CANCEL)
							continue;
						auto rp = p + y * stride;
						NoNoiseSaturate(task, y, rp);
					}
				} else {
#pragma omp parallel for num_threads(maxGenerationTasks)
					for (int16_t y = 0; y < task.applyHeight; ++y) {
						if (CANCEL)
							continue;
						auto rp = p + y * stride;
						NoNoiseNoSaturate(task, y, rp);
					}
				}
			}
		} else {
			uint8_t* rp;
			// Single Threaded:
			if (ambnoise > 0) {
				for (int y = 0; y < task.applyHeight / applyVoid + 2; ++y) {
					auto& v = task.voidNoise[y];
					auto& dist = *randomDist;
					auto& randGen = *randomGenerator;
					for (int x = 0; x < wv; ++x)
						v[x] = ambnoise * Vector(dist(randGen), dist(randGen), dist(randGen));
				};
				if (selectSaturate > 0.0) for (uint16_t y = 0; y < task.applyHeight; ++y) {
					if (CANCEL)
						continue;
					rp = p + y * stride;
					NoiseSaturate(task, y, rp);
				} else for (uint16_t y = 0; y < task.applyHeight; ++y) {
					if (CANCEL)
						continue;
					rp = p + y * stride;
					NoiseNoSaturate(task, y, rp);
				}
			} else {
				if (selectSaturate > 0.0) for (uint16_t y = 0; y < task.applyHeight; ++y) {
					if (CANCEL)
						continue;
					rp = p + y * stride;
					NoNoiseSaturate(task, y, rp);
				} else for (uint16_t y = 0; y < task.applyHeight; ++y) {
					if (CANCEL)
						continue;
					rp = p + y * stride;
					NoNoiseNoSaturate(task, y, rp);
				}
			}
		}
		if (CANCEL) {
			task.state = TaskState::Done;
			return;
		}
#ifdef CUSTOMDEBUG
		const auto drawElapsed = (std::chrono::steady_clock::now() - drawTime).count();
		Log(threadString, "Draw:" + task.bitmapIndex + " time = " + voidElapsed);
#ifdef CLR
		Monitor::Enter(taskLock);
		try {
#else
		taskLock.lock();
#endif
		voidTimes += voidElapsed;
		drawTimes += drawElapsed;
		Log(logString, threadString);
#ifdef CLR
		} finally { Monitor::Exit(taskLock); }
#else
		taskLock.unlock();
#endif
#endif
		if (task.bitmapIndex < previewFrames) {
			bitmapState[task.bitmapIndex] = BitmapState::FinishedBitmap;
			task.state = TaskState::Done;
			TryFinishBitmaps(false);
		} else {
			if (applyGenerationType >= GenerationType::EncodeGIF && applyGenerationType <= GenerationType::AllParam && applyGenerationType != GenerationType::Mp4)
				GenerateGif(task.taskIndex);
			else {
				bitmapState[task.bitmapIndex] = BitmapState::DrawingFinished;
				task.state = TaskState::Done;
			}
		}
	}
	VOIDTYPE FractalGenerator::GenerateGif(const uint16_t taskIndex) {
#ifdef CUSTOMDEBUG
		STRING threadString = "";
		const auto gifsTime{ std::chrono::steady_clock::now() };
#endif
		auto& task = tasks[taskIndex];
		if (applyGenerationType == GenerationType::Mp4) {
			if (mp4Encoder != nullptr) {
				bitmapState[task.bitmapIndex] = BitmapState::Encoding; // Start encoding Frame to a temp GIF
				//const auto d = bitmapData[task.bitmapIndex];
				//uint8_t* ptr = (uint8_t*)(void*)d->Scan0;
				// TODO MP4
				//mp4Encoder.AddFrame(bitmap[task.bitmapIndex], strides[task.bitmapindex]);
				bitmapState[task.bitmapIndex] = BitmapState::EncodingFinished;
			} else {
				StopGif(task.taskIndex);
				bitmapState[task.bitmapIndex] = BitmapState::DrawingFinished;
			}
		} else {
			if (gifEncoder != nullptr) {
				bitmapState[task.bitmapIndex] = BitmapState::Encoding; // Start encoding Frame to a temp GIF	
				//const auto d = bitmapData[task.bitmapIndex];
				//uint8_t* ptr = (uint8_t*)(void*)d->Scan0;
				((GifEncoder*)gifEncoder)->push_parallel(
					bitmap[task.bitmapIndex],
					selectWidth, selectHeight, 
					selectDelay, task.bitmapIndex - previewFrames, 
					strides[task.bitmapIndex], GIFCANCEL);
				//gifEncoder.AddFrameParallel(ptr, d.Stride, gifToken, task.bitmapIndex - previewFrames);
				bitmapState[task.bitmapIndex] = BitmapState::EncodingFinished;
			} else {
				StopGif(task.taskIndex);
				bitmapState[task.bitmapIndex] = BitmapState::DrawingFinished;
			}
		}
#ifdef CUSTOMDEBUG
		const auto gifsElapsed = (std::chrono::steady_clock::now() - gifsTime).count();
		Log(threadString, "Gifs:" + task.bitmapIndex + " time = " + gifsElapsed);
#ifdef CLR
		Monitor::Enter(taskLock);
		try {
#else
		taskLock.lock();
#endif
		gifsTimes += gifsElapsed;
		Log(logString, threadString);
#ifdef CLR
		} finally { Monitor::Exit(taskLock); }
#else
		taskLock.unlock();
#endif
#endif
		if (applyGenerationType == GenerationType::Mp4)
			bitmapState[task.bitmapIndex] = BitmapState::FinishedBitmap;
		gifThread = false;
		task.state = TaskState::Done;
	}
	VOIDTYPE FractalGenerator::GenerateDots_SingleTask(const FractalTask& task, double inXY, double AA, const uint8_t inColor, const int64_t inFlags, uint8_t inDepth) {
		STARTITERATION {
			FORCHILD { ITERATECHILDREN APPLYDOT }
			return;
		}
		FORCHILD { ITERATECHILDREN GenerateDots_SingleTask(task, NEWCHILD); }
	}
	VOIDTYPE FractalGenerator::GenerateDots_OfDepth(const uint32_t bitmapIndex) {

		uint8_t index = 0, insertTo = 1,
			max = applyMaxTasks * depthdiv,
			maxcount = max - f->childCount - 1,
			count = (max + insertTo - index) % max;
		while (count > 0 && count < maxcount) {
			const auto& params = tuples[index++];
			index %= max;
			const uint16_t& taskIndex = std::get<0>(params);
			GETTUPLEPACK
			const uint8_t& inColor = std::get<3>(params);
			const int64_t& inFlags = std::get<4>(params);
			uint8_t inDepth = std::get<5>(params);
			const auto& task = tasks[taskIndex];
			STARTITERATION {
				FORCHILD { ITERATECHILDREN APPLYDOT }
				count = (max + insertTo - index) % max;
				continue;
			}
			// Split Iteration Deeper
			FORCHILD {
				ITERATECHILDREN tuples[insertTo++] = { taskIndex, NEWCHILD };
				insertTo %= max;
			}
			count = (max + insertTo - index) % max;
		}

		FINISHTASKS(false) {
			if (count <= 0)
				continue;
			const auto tupleIndex = index++;
#ifdef CLR
			Start(taskIndex, bitmapIndex, gcnew Action<Object^>(this, &FractalGenerator::Task_OfDepth), gcnew array<System::Object^>{ taskIndex, index++ });
#else
			tasks[taskIndex].Start(bitmapIndex).task = std::thread(&FractalGenerator::TaskOfDepth, this, taskIndex, index++);
#endif
			index %= max;
			count = (max + insertTo - index) % max;
			tasksRemaining = true;
		}
	}
	/*VOIDTYPE FractalGenerator::GenerateDots_OfRecursion(
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
	bool FractalGenerator::TryWriteBitmaps(FractalTask& task) {
		if (CANCEL // Do not write gid frames when cancelled
			|| gifEncoder == nullptr // ...or if gifencoder doens't exist
			|| ((GifEncoder*)gifEncoder)->isFinishedAnimation() // ...or if it's finished
			|| applyGenerationType < GenerationType::EncodeGIF || applyGenerationType > GenerationType::AllParam || applyGenerationType == GenerationType::Mp4 // ...or we are not supposed to encode a gif
			|| isWritingBitmaps <= 0 // ...or this task is already running
			|| --isWritingBitmaps > 0) // ...or we have already ran it not too long ago
			return false;
#ifdef CLR
		Start(task.taskIndex, -1, gcnew Action<Object^>(this, &FractalGenerator::Task_TryWriteBitmap), gcnew array<System::Object^>{ task.taskIndex });
#else
		task.Start(-1).task = std::thread(&FractalGenerator::TryWriteBitmap, this, task.taskIndex);
#endif
		return true;
	}
	VOIDTYPE FractalGenerator::TryWriteBitmap(const uint16_t taskIndex) {
		auto& task = tasks[taskIndex];
		while (!CANCEL) {
			if (bitmapsFinished >= frames && !((GifEncoder*)gifEncoder)->isFinishedAnimation())
				((GifEncoder*)gifEncoder)->close(true, GIFCANCEL);
			int unlock = ((GifEncoder*)gifEncoder)->getFinishedFrame();
			// Try to finalize the previous encoder tasks
			switch (((GifEncoder*)gifEncoder)->tryWrite()) {
			case GifEncoderTryWriteResult::Failed:
				// fallback to only display animation without encoding
				StopGif(taskIndex);
				isWritingBitmaps = 2;
				task.state = TaskState::Done;
				return;
			case GifEncoderTryWriteResult::FinishedFrame:
				// mark the bitmap state as fully finished
				bitmapState[unlock + previewFrames] = BitmapState::FinishedBitmap;
				break;
			default:
				// waiting or finished animation
				isWritingBitmaps = 2;
				task.state = TaskState::Done;
				return;
			}
		}
		isWritingBitmaps = 2;
		task.state = TaskState::Done;
	}
	bool FractalGenerator::TryFinishBitmap(FractalTask& task) {
		if (CANCEL)
			return false;
		bool gif = applyGenerationType >= GenerationType::EncodeGIF && applyGenerationType <= GenerationType::AllParam;
		if (gif && bitmapsFinished < frames && bitmapsFinished >= previewFrames && !gifThread) {
			int tryEncode = bitmapsFinished;
			int32_t maxTry = MIN(static_cast<int32_t>(frames), static_cast<int32_t>(bitmapsFinished) + static_cast<int32_t>(applyMaxTasks));
			while (tryEncode < maxTry) {
				if (bitmapState[tryEncode] >= BitmapState::DrawingFinished && bitmapState[tryEncode] <= BitmapState::UnlockedRAM) {
					if (bitmapState[tryEncode] == BitmapState::UnlockedRAM)
						ReLockBits(tryEncode);
					bitmapState[tryEncode] = BitmapState::Encoding;
					gifThread = applyGenerationType == GenerationType::Mp4;
#ifdef CLR
					Start(task.taskIndex, tryEncode, gcnew Action<Object^>(this, &FractalGenerator::Task_GenerateGif), gcnew array<System::Object^>{ task.taskIndex });
#else
					task.Start(tryEncode).task = std::thread(&FractalGenerator::GenerateGif, this, task.taskIndex);
#endif
					return true;
				}
				++tryEncode;
			}
		}
		TryFinishBitmaps(gif);
		return false;
	}
	VOIDTYPE FractalGenerator::TryFinishBitmaps(bool gif) {
#ifdef CLR
		Monitor::Enter(taskLock);
		try {
#else
		taskLock.lock();
#endif
		while (bitmapsFinished < frames && bitmapState[bitmapsFinished] >= (gif || bitmapState[bitmapsFinished] == BitmapState::Encoding ? BitmapState::FinishedBitmap : BitmapState::DrawingFinished)) {
			bitmapState[bitmapsFinished] = gif || bitmapsFinished < previewFrames ? BitmapState::Unlocked : BitmapState::UnlockedRAM;
			if (applyGenerationType == GenerationType::HashParam) {
				// TODO HASH
				/*using SHA256 sha256 = SHA256.Create();
				int bytenum = bitmapData[bitmapsFinished].Stride * selectHeight;
				byte[] pixelData = new byte[bytenum];

				// Copy the raw pixel data from Scan0 to the byte array
				fixed(byte * dest = pixelData) {
					Buffer.MemoryCopy((void*)bitmapData[bitmapsFinished].Scan0, dest, bytenum, bytenum);
				}
				string key = BitConverter.ToString(sha256.ComputeHash(pixelData));
				if (!hash.ContainsKey(key))
					hash.Add(key, bitmapsFinished);*/
			}
			UnlockBits(bitmapsFinished);
			if (bitmapsFinished++ <= previewFrames && !CANCEL && UpdatePreview != nullptr)
				UpdatePreview();
		}
#ifdef CLR
		} finally { Monitor::Exit(taskLock); }
#else
		taskLock.unlock();
#endif
	}
#pragma endregion

#pragma region Generate_Inline
	//static const Vector& GetVec(const std::map<int64_t, std::array<Vector, 3>> H, const int64_t key, const int8_t i) {const auto it = H.find(key);return it != H.end() ? it->second[i] : zero;}
	VOIDTYPE FractalGenerator::ApplyDot(const FractalTask& task, const int64_t key, const float& inX, const float& inY, const float& inDetail, const uint8_t& inColor) {
		const auto colorindex = (inColor + task.huemod) % 3;
		std::map<int64_t, std::array<Vector, 3>>::const_iterator it;
		const auto dotColor = applyPreviewMode ? task.I[colorindex] : Vector::Lerp(
			(it = task.H.find(key)) != task.H.end() ? it->second[colorindex] : zero,
			task.I[colorindex], inDetail);
		auto& buffT = task.buffer;
		// Iterated deep into a single point - Interpolate inbetween 4 pixels and Stop
		const auto startX = MAX(static_cast<int16_t>(1), static_cast<int16_t>(FLOOR(inX - task.bloom0))),
			endX = MIN(task.widthBorder, static_cast<int16_t>(CEIL(inX + task.bloom0))),
			endY = MIN(task.heightBorder, static_cast<int16_t>(CEIL(inY + task.bloom0)));
		for (int16_t x, y = MAX(static_cast<int16_t>(1), static_cast<int16_t>(FLOOR(inY - task.bloom0))); y <= endY; ++y) {
			const auto yd = bloom1 - ABS(y - inY);
			const auto& buffY = buffT[y];
			for (x = startX; x <= endX; ++x)
				buffY[x] += (yd * (bloom1 - ABS(x - inX))) * dotColor;
		}
	}
	inline bool FractalGenerator::FractalGenerator::TestSize(const FractalTask& task, const float& newX, const float& newY, const float& inSize) {
		const auto testSize = inSize * f->cutSize;
		return MIN(newX, newY) + testSize > task.upleftStart && newX - testSize < task.rightEnd && newY - testSize < task.downEnd;
	}
	const Vector FractalGenerator::Normalize(const Vector& pixel, const float lightNormalizer) {
		const float max = pixel.Max();
		return lightNormalizer * max > 254.0f ? (254.0f / max) * pixel : lightNormalizer * pixel;
	}
	// old code that should work with SIMD vectors but couldn't get it to work
	/*inline VOIDTYPE FractalGenerator::ApplyAmbientNoise(Vector<float>& rgb,  const float Amb, const float Noise, std::uniform_real_distribution<float>& dist, std::mt19937& randGen) {
		rgb += Noise * Vector<float>(gcnew array<float>(3) { dist(randGen), dist(randGen), dist(randGen)}) + Vector<float>(amb);
	}
	inline VOIDTYPE FractalGenerator::ApplySaturate(Vector<float>& rgb, const float d, uint8_t*& p) {
		float m; const float min = MIN(MIN(rgb[0], rgb[1]), rgb[2]), max = MAX(MAX(rgb[0], rgb[1]), rgb[2]);
		return max <= min ? rgb : ((m = max * saturate / (max - min)) + 1 - saturate) * rgb - Vector<float>(min * m);
	}
	inline VOIDTYPE FractalGenerator::ApplyNoSaturate(Vector<float>& rgb, uint8_t*& p) {
		p[0] = static_cast<uint8_t>(rgb[2]);	// Blue
		p[1] = static_cast<uint8_t>(rgb[1]);	// Green
		p[2] = static_cast<uint8_t>(rgb[0]);	// Red
		p += 3;
	}*/
	const Vector FractalGenerator::ApplyAmbientNoise(const Vector& rgb, const float Amb, const float Noise, const Vector& rand) {
		return rgb + Amb + Noise * rand;
	}
	const Vector FractalGenerator::ApplySaturate(const Vector& rgb) {
		// The saturation equation boosting up the saturation of the pixel (powered by the saturation slider setting)
		float m; const auto min = MIN(MIN(rgb.X, rgb.Y), rgb.Z), max = MAX(MAX(rgb.X, rgb.Y), rgb.Z);
		return max <= min ? rgb : Vector::MultiplyMinus(rgb, (m = max * selectSaturate / (max - min)) + 1 - selectSaturate, min * m);
	}
	VOIDTYPE FractalGenerator::ApplyRGBToBytePointer(const Vector& rgb, uint8_t*& p) {
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
	inline VOIDTYPE FractalGenerator::NoiseSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p) {
		const auto& voidY = task.voidDepth[y];
		const auto& buffY = task.buffer[y];
		const auto fy = static_cast<float>(y) / applyVoid;
		const auto startY = static_cast<uint16_t>(FLOOR(fy));
		const auto alphaY = fy - startY;
		//if (selectAmbient <= 0) // noise is always 0 if ambient is zero, so it should't even get to this function
		//	for (var x = 0; x < task.applyWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), ref p)) ;
		//else 
		for (auto x = 0; x < task.applyWidth; ++x) {
			const auto voidAmb = voidY[x] / task.voidDepthMax;
			const auto fx = static_cast<float>(x) / applyVoid;
			const auto startX = static_cast<uint16_t>(FLOOR(fx));
			const auto alphaX = fx - startX;
			const auto& vy0 = task.voidNoise[startY];
			const auto& vy1 = task.voidNoise[startY + 1];
			ApplyRGBToBytePointer(ApplyAmbientNoise(ApplySaturate(Normalize(buffY[x], task.lightNormalizer)), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb,
				alphaY * (alphaX * vy1[startX + 1] + (1 - alphaX) * vy1[startX]) + (1 - alphaY) * (alphaX * vy0[startX + 1] + (1 - alphaX) * vy0[startX])), p);
		}
	}
	inline VOIDTYPE FractalGenerator::NoiseNoSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p) {
		const auto& voidY = task.voidDepth[y];
		const auto& buffY = task.buffer[y];
		const auto fy = static_cast<float>(y) / applyVoid;
		const auto startY = static_cast<uint16_t>(FLOOR(fy));
		const auto alphaY = fy - startY;
		//if (selectAmbient <= 0) // noise is always 0 if ambient is zero, so it should't even get to this function
		//	for (var x = 0; x < task.applyWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), ref p)) ;
		//else 
		for (auto x = 0; x < task.applyWidth; ++x) {
			const auto voidAmb = voidY[x] / task.voidDepthMax;
			const auto fx = static_cast<float>(x) / applyVoid;
			const auto startX = static_cast<uint16_t>(FLOOR(fx));
			const auto alphaX = fx - startX;
			const auto& vy0 = task.voidNoise[startY];
			const auto& vy1 = task.voidNoise[startY + 1];
			ApplyRGBToBytePointer(ApplyAmbientNoise(Normalize(buffY[x], task.lightNormalizer), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb,
				alphaY * (alphaX * vy1[startX + 1] + (1 - alphaX) * vy1[startX]) + (1 - alphaY) * (alphaX * vy0[startX + 1] + (1 - alphaX) * vy0[startX])), p);
		}
	}
	inline VOIDTYPE FractalGenerator::NoNoiseSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p) {
		const auto& buffY = task.buffer[y];
		if (selectAmbient > 0) {
			const auto& voidY = task.voidDepth[y];
			for (auto x = 0; x < task.applyWidth; ++x)
				ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x] / task.voidDepthMax) + ApplySaturate(Normalize(buffY[x], task.lightNormalizer)), p);
		} else for (auto x = 0; x < task.applyWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], task.lightNormalizer)), p));
	}
	inline VOIDTYPE FractalGenerator::NoNoiseNoSaturate(const FractalTask& task, const uint16_t y, uint8_t*& p) {
		const auto& buffY = task.buffer[y];
		if (selectAmbient > 0) {
			const auto& voidY = task.voidDepth[y];
			for (auto x = 0; x < task.applyWidth; ++x)
				ApplyRGBToBytePointer(Vector(selectAmbient * voidY[x] / task.voidDepthMax) + Normalize(buffY[x], task.lightNormalizer), p);
		} else for (auto x = 0; x < task.applyWidth; x++)
			ApplyRGBToBytePointer(Normalize(buffY[x], task.lightNormalizer), p);
	}
#pragma endregion

#pragma region TaskWrappers
	VOIDTYPE FractalGenerator::Task_Dots(System::Object^ obj) {
		// Unpack arguments
		const auto args = (array<System::Object^>^)obj;
		const auto bitmapIndex = static_cast<uint32_t>(args[0]);
		const auto taskIndex = static_cast<int16_t>(args[1]);
		const auto size = static_cast<float>(args[2]);
		const auto angle = static_cast<float>(args[3]);
		const auto spin = static_cast<int8_t>(args[4]);
		const auto hueAngle = static_cast<float>(args[5]);
		const auto color = static_cast<uint8_t>(args[6]);
		GenerateDots(bitmapIndex, taskIndex, size, angle, spin, hueAngle, color);
	}
	VOIDTYPE FractalGenerator::Task_OfDepth(System::Object^ obj) {
		const auto args = (array<System::Object^>^)obj;
		TaskOfDepth(static_cast<int16_t>(args[0]), static_cast<uint16_t>(args[1]));
	}
	VOIDTYPE FractalGenerator::TaskOfDepth(const uint16_t taskIndex, const uint16_t tupleIndex) {
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
	inline VOIDTYPE FractalGenerator::Start(const uint16_t taskIndex, int16_t bitmap, Action<Object^>^ action, Object^ state) {
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
	VOIDTYPE FractalGenerator::Task_Image(System::Object^ obj) {
		GenerateImage(static_cast<uint16_t>(((array<System::Object^>^)obj)[0]));
	}
	VOIDTYPE FractalGenerator::Task_GenerateGif(System::Object^ obj) {
		GenerateGif(static_cast<uint16_t>(((array<System::Object^>^)obj)[0]));
	}
	VOIDTYPE FractalGenerator::Task_TryWriteBitmap(System::Object^ obj) {
		TryWriteBitmap(static_cast<uint16_t>(((array<System::Object^>^)obj)[0]));
	}

	/*VOIDTYPE FractalGenerator::Task_OfRecursion(System::Object^ obj) {
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
		STRING start = "Thread:x" + (int)floor(inX) + "y" + (int)floor(inY) + "s" + (int)floor(inSize) + " start = " + (threadTime - *startTime).count();
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
	VOIDTYPE FractalGenerator::Task_Gif(System::Object^ obj) {}*/
#pragma endregion

#pragma region AnimationParameters
	VOIDTYPE FractalGenerator::SwitchParentChild(float& angle, int8_t& spin, uint8_t& color) {
		if (applyPreviewMode)
			return;
		color = (3 + color + applyZoom * childColor[0]) % 3;
		if (ABS(spin) <= 1)
			return;
		// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
		angle = -angle;
		spin = -spin;
	}
	VOIDTYPE FractalGenerator::ModFrameParameters(const uint16_t width, const uint16_t height, float& size, float& angle, int8_t& spin, float& hueAngle, uint8_t& color) {
		auto w = MAX(width, width) * f->maxSize;
		const auto fp = f->childSize;
		if (applyPreviewMode)
			w *= 0.1f;
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
			if (applyPreviewMode)
				continue;
			angle += childAngle[0];
			SwitchParentChild(angle, spin, color);
		}
		while (size < w) {
			size *= fp;
			if (applyPreviewMode)
				continue;
			angle -= childAngle[0];
			SwitchParentChild(angle, spin, color);
		}
	}
	VOIDTYPE FractalGenerator::IncFrameParameters(float& size, float& angle, const int8_t& spin, float& hueAngle, const uint16_t blur) {
		if (applyGenerationType >= GenerationType::AllParam && applyGenerationType <= GenerationType::HashParam)
			return;
		const auto blurPeriod = selectPeriod * blur;
		// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
		angle += spin * (applyPeriodAngle * (1 + selectExtraSpin)) / (finalPeriodMultiplier * blurPeriod);
		hueAngle += (hueCycleMultiplier + 3 * selectExtraHue) * (float)applyHueCycle / (finalPeriodMultiplier * blurPeriod);
		IncFrameSize(size, blurPeriod);
	}
#pragma endregion

#pragma region Interface_Calls
	VOIDTYPE FractalGenerator::StartGenerate() {
		// start the generator in a separate main thread so that the form can continue being responsive
#ifdef CLR
		token = (cancel = gcnew CancellationTokenSource())->Token;
		mainTask = Task::Run(gcnew Action(this, &FractalGenerator::GenerateAnimation), token);
#else
		mainTask = std::thread(&FractalGenerator::GenerateAnimation, this);
#endif
	}
	VOIDTYPE FractalGenerator::ResetGenerator() {
		restartGif = false;
		auto& dist = *randomDist;
		auto& gen = *randomGenerator;
		applyZoom = static_cast<int16_t>(selectZoom != 0 ? selectZoom : dist(gen) < .5f ? -1 : 1);
		applyCutparam = selectCutparam < 0 ? static_cast<int64_t>(dist(gen) * GetMaxCutparam()) : selectCutparam;
		SetupColor();
		// Get the multiplier of the basic period required to get to a seamless loop
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
		switch(applyGenerationType = selectGenerationType) {
			case GenerationType::AllParam: frames = GetMaxCutparam() + 1; break;
			case GenerationType::HashParam: frames = cutparamMaximum + 1; break;
			default: frames = debug > 0 ? debug : selectPeriod * finalPeriodMultiplier; break;
		};
		if ((frames += previewFrames = MAX(static_cast<uint8_t>(0), static_cast<uint8_t>(LOG(MIN(selectWidth, selectHeight)) / LOG(2) - 2))) != allocatedFrames) {
			if (allocatedFrames >= 0) {
				delete[] bitmapState;
				delete[] bitmap;
				delete[] strides;
			}
			allocatedFrames = frames;
			AllocateBitmaps();
			bitmapState = new BitmapState[frames + 1];
			bitmap = new uint8_t * [frames];
			strides = new uint16_t[frames];
		}
		for (int16_t b = frames; b >= 0; bitmapState[b--] = BitmapState::Queued);
	}
	VOIDTYPE FractalGenerator::RequestCancel() {
#ifdef CLR
		if (cancel != nullptr)
			cancel->Cancel();
		if (gifCancel != nullptr)
			gifCancel->Cancel();
		try {
			if (mainTask != nullptr)
				mainTask->Wait();
		} catch (EXCEPTION) {}
#else
		cancelRequested = true;
		gifCancelRequested = true;
		if (mainTask.joinable())
			mainTask.join();
		cancelRequested = false;
		giCancelRequested = false;
#endif
	}
	bool FractalGenerator::SaveGif() {
		return gifSuccess = 0;
	}
	// TODO MP4
	/*int SaveMp4(string gifPath, string mp4Path) {
		try {
			File.Delete(mp4Path);
			string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
			if (!File.Exists(ffmpegPath))
				return gifSuccess = 0;
			double gifFps = 1000.0 / (10 * selectDelay); // Convert to frames per second
			string arguments = $"-y -i \"{gifPath}\" -vf \"fps={selectFps},setpts=PTS*({gifFps}/{selectFps})\" -c:v libx264 -crf 0 -preset veryslow \"{mp4Path}\"";
			using Process ffmpeg = new Process();
			ffmpeg.StartInfo.FileName = ffmpegPath;
			ffmpeg.StartInfo.Arguments = arguments;
			ffmpeg.StartInfo.UseShellExecute = false;
			ffmpeg.StartInfo.RedirectStandardError = true;
			ffmpeg.StartInfo.RedirectStandardOutput = true;
			ffmpeg.StartInfo.CreateNoWindow = true;
			//ffmpeg.ErrorDataReceived += (sender, e) =>{if (!string.IsNullOrEmpty(e.Data)) {Console.WriteLine("FFmpeg error: " + e.Data);}};
			ffmpeg.Start();
			// Begin reading error asynchronously
			ffmpeg.BeginErrorReadLine();
			// Wait for the process to exit
			ffmpeg.WaitForExit();
			//File.Delete(gifTempPath); // do not delete the gif.tmp so the user could save the gif too if they wanted
		} catch (IOException ex) {
			var exs = "SaveGif: An error occurred: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (UnauthorizedAccessException ex) {
			var exs = "SaveGif: Access denied: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (Exception ex) {
			var exs = "SaveGif: Unexpected error: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		}
		return gifSuccess = 0;
	}*/
#ifdef CUSTOMDEBUG
	VOIDTYPE FractalGenerator::Log(STRING log, STRING line) {
		Debug::WriteLine(line);
		log += "\n" + line;
	}
#endif
	VOIDTYPE FractalGenerator::DebugStart() {
		// debug for testing, starts the generator with predetermined setting for easy breakpointing
		// Need to enable the definition on top GeneratorForm.cpp to enable this
		// if enabled you will be unable to use the settings interface!
		SelectFractal(1);
		SelectThreadingDepth();
		selectPeriod = debug = 1;
		selectWidth = 8;//1920;
		selectHeight = 8;//1080;
		maxDepth = -1;//= 2;
		selectMaxTasks = MINTASKS;// 10;
		selectSaturate = 1.0f;
		selectDetail = .25f;
		SelectThreadingDepth();
		selectCut = selectChildAngle = selectChildColor = 0;
		SetupColor();
		SetupAngle();
		SetupCutFunction();
	}
	VOIDTYPE FractalGenerator::MakeDebugString() {
		if (!debugmode)
			return;
		if (CANCEL) {
			debugString = "ABORTING";
			return;
		}
		STRING _debugString = "TASKS:";
		auto i = 0, li = 0;
		while (i < applyMaxTasks) {
			auto& task = tasks[i];
			_debugString += "\n" + TOSTRING(i++) + ": ";
			switch (task.state) {
			case TaskState::Running:
				_debugString += GetBitmapState(bitmapState[task.bitmapIndex]);
				break;
			case TaskState::Done:
				_debugString += "DONE";
				break;
			case TaskState::Free:
				_debugString += "FREE";
				break;
			}
		}
		BitmapState state, laststate = bitmapState[0];
		for(auto c = i = 0; c < 11; counter[c++] = 0);
		for (_debugString += "\n\nIMAGES:"; i < frames; ++i)
			++counter[(int)bitmapState[i]];
		STRING _memoryString = "\n";
		for (i = 0; i < frames; ++i, laststate = state) 
			if ((state = bitmapState[i]) != laststate) {
				if(i >= 0)
					_memoryString += (li == i - 1 ? TOSTRING(li) + ": " : TOSTRING(li) + "-" + TOSTRING((i - 1)) + ": ") + GetBitmapState(laststate) + "\n";
				li = i;
			}
		_memoryString += (li == i - 1 ? TOSTRING(li) + ": " : TOSTRING(li) + "-" + TOSTRING((i - 1)) + ": ") + GetBitmapState(laststate);
		for (int c = 0; c < 11; ++c)
			_debugString += "\n" + TOSTRING(counter[c]) + "x: " + GetBitmapState((BitmapState)c);
		_debugString += "\n\nANIMATION:" + _memoryString;
		debugString = i < frames ? _debugString + "\n" + TOSTRING(i) + "+: " + "QUEUED" : _debugString;
	}
	/*VOIDTYPE FractalGenerator::TestEncoder(array<Bitmap^>^ bitmap) {

		// init
		auto& dist = *randomDist;
		auto& gen = *randomGenerator;
		GifEncoder* se = new GifEncoder();
		GifEncoder* pe = new GifEncoder();
		GifEncoder* se_out = new GifEncoder();
		GifEncoder* pe_out = new GifEncoder();
		List<int>^ l = gcnew List<int>();

		// get byte pointers if we don't have already:
		array<BitmapData^>^ bgr = gcnew array<BitmapData^>(frames);
		array<BitmapData^>^ bgra = gcnew array<BitmapData^>(frames);
		for (int i = 0; i < frames; ++i) {
			bgr[i] = bitmap[i]->LockBits(Rectangle(0, 0, bitmap[i]->Width, bitmap[i]->Height), ImageLockMode::ReadOnly, PixelFormat::Format24bppRgb); // get BRG (also used for RGB)
			bgra[i] = bitmap[i]->LockBits(Rectangle(0, 0, bitmap[i]->Width, bitmap[i]->Height), ImageLockMode::ReadOnly, PixelFormat::Format32bppArgb); // get BRGA (also used for RGBA)
		}

		// BGR/BGRA:
		se->open("s_bgr_inorder.gif", selectWidth, selectHeight, 1, true, 0);
		pe->open_parallel("p_bgr_inorder.gif", selectWidth, selectHeight, 1, 0);
		se_out->open("s_bgr_outoforder.gif", selectWidth, selectHeight, 1, true, 0);
		pe_out->open_parallel("p_bgr_outoforder.gif", selectWidth, selectHeight, 1, 0);
		for (int i = 0; i < frames; l->Add(i++)) {
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
			int i = l[static_cast<uint16_t>(dist(gen) * l->Count)];
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
		for (int i = 0; i <= frames; ++i) {
			pe->tryWrite();
			pe_out->tryWrite();
		}

		// RGB/RGBA:
		uint8_t** rgb = new uint8_t * [frames];
		uint8_t** rgba = new uint8_t * [frames];
		for (int i = 0; i < frames; ++i) {
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
		for (int i = 0; i < frames; l->Add(i++)) {
			// RGB copy:
			se->push(GifEncoderPixelFormat::PIXEL_FORMAT_RGB, rgb[i], selectWidth, selectHeight, 20);
			pe->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_RGB, rgb[i], selectWidth, selectHeight, 20);
			// RGBA copy:
			//se->push(GifEncoderPixelFormat::PIXEL_FORMAT_RGBA, rgba[i], selectWidth, selectHeight, 20);
			//pe->push_parallel(GifEncoderPixelFormat::PIXEL_FORMAT_RGBA, rgba[i], selectWidth, selectHeight, 20);
		}
		while (l->Count > 0) {
			int i = l[static_cast<uint16_t>(dist(gen) * l->Count)];
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
		for (int i = 0; i <= frames; ++i) {
			pe->tryWrite();
			pe_out->tryWrite();
		}

		// release byte pointers and encoders:
		for (int i = 0; i < frames; ++i)
			bitmap[i]->UnlockBits(bitmapData[i]);
		delete se;
		delete pe;
		delete se_out;
		delete pe_out;
	}*/
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
	VOIDTYPE FractalGenerator::SetupFractal() {
		if (f != nullptr)
			delete f;
		f = new Fractal(*(*fractals)[selectFractal]);
		logBase = static_cast<float>(LOG(f->childSize));
		SetMaxIterations();
	}
	VOIDTYPE FractalGenerator::SetMaxIterations() {
		selectMaxIterations = 2 + static_cast<int16_t>(CEIL(LOG(MAX(selectWidth, selectHeight) * f->maxSize / selectDetail) / logBase));
	}
	VOIDTYPE FractalGenerator::SetupAngle() {
		childAngle = f->childAngle[selectChildAngle].second;
		selectPeriodAngle = f->childCount <= 0 ? 0.0f : std::fmod(childAngle[0], 2.0f * static_cast<float>(M_PI));
	}
	VOIDTYPE FractalGenerator::SetupColor() {
		// Unpack the color palette and hue cycling
		if (selectHue < 0) {
			auto& dist = *randomDist;
			auto& gen = *randomGenerator;
			applyHueCycle = static_cast<int16_t>(dist(gen) * 2 - 1);
			applyColorPalette = static_cast<int16_t>(dist(gen) * 2);
		} else {
			applyHueCycle = static_cast<int16_t>((selectHue / 2 + 1) % 3 - 1);
			applyColorPalette = static_cast<int16_t>(selectHue % 2);
		}
		// Setup colors
		if (f->childCount > 0) {
			for (int i = f->childCount; 0 <= --i; childColor[i] = f->childColor[selectChildColor].second[i]);
			// Setup palette
			for (int i = 0; i < f->childCount; ++i)
				childColor[i] = applyColorPalette == 0 ? childColor[i] : (3 - childColor[i]) % 3;
		}
		// Prepare subiteration color blend
		/*float* colorBlendF = new float[3];
		colorBlendF[0] = colorBlendF[1] = colorBlendF[2] = 0;
		for (auto i = f->childCount; 0 <= --i; colorBlendF[childColor[i]] += 1.0f / f->childCount);
		colorBlend = new Vector(colorBlendF[0], colorBlendF[1], colorBlendF[2]);*/
	}

#pragma endregion

}
