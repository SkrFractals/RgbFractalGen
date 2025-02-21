#pragma once
#include <cstdint>
#include <utility>
#include <string>
#include <vector>
//#include <list>

#define CFP int8_t index, int64_t inFlags, const Fractal& f

namespace RgbFractalGenClr {

	//using namespace System::Collections::Generic;

	class Fractal {
	public:

		typedef int64_t(*CutFunction)(CFP);

		static std::vector<std::pair<std::string, CutFunction>> cutFunctions;

		static void initialize() {
			cutFunctions.push_back({ "NoChildSimple", NoChildSimple });												// 0
			cutFunctions.push_back({ "NoChildComplexAngled", NoChildComplexAngled });								// 1
			cutFunctions.push_back({ "NoBackDiag", NoBackDiag });													// 2
			cutFunctions.push_back({ "TriComb", TriComb });															// 3
			cutFunctions.push_back({ "Tetraflake", Tetraflake_Symmetric });											// 4
			cutFunctions.push_back({ "Tetraflake_Symmetric_RadHoles", Tetraflake_Symmetric_RadHoles });				// 5
			cutFunctions.push_back({ "Tetraflake_Symmetric_CornerHoles", Tetraflake_Symmetric_CornerHoles });		// 6
			cutFunctions.push_back({ "Tetraflake_Symmetric_TriangleHoles", Tetraflake_Symmetric_TriangleHoles });	// 7
			cutFunctions.push_back({ "Beamtree_NoBeam", Beamtree_NoBeam });											// 8
			cutFunctions.push_back({ "Beamtree_OuterJoint", Beamtree_OuterJoint });									// 9
			cutFunctions.push_back({ "Beamtree_InnerJoint", Beamtree_InnerJoint });									// 10
			cutFunctions.push_back({ "Carpet_Symmetric", Carpet_Symmetric });										// 11
			cutFunctions.push_back({ "NoChildComplexNoAngled", NoChildComplexNoAngled });							// 12
		}
		
		// Properties
		std::string name;	// Fractal name (only for selection list)
		int8_t childCount;	// ChildCount of Self Similars Inside (must equal the length of all the following arrays)
		float childSize;	// Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)
		float maxSize;		// The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)
		float minSize;		// How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)
		float cutSize;		// A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance)
		float* childX;		// X coord shifts of Self Similars Inside
		float* childY;		// Y coord shifts of Self Similars Inside
		std::vector<std::pair<std::string, float*>> 
			childAngle;		// Angle shifts of Self Similars Inside
		std::vector < std::pair<std::string, uint8_t*>>
			childColor;		// Color shifts of Self Similars Inside
		std::vector<std::pair<int32_t, std::vector<int32_t>>>
			cutFunction;	// Function that takes a bitarray transforms it and decides to cut some specific patterns of Self Simmilars
		std::string 
			path = "";		// where was it loaded from?
		bool edit = false;	// is it edited?

		// Autoproperties
		uint8_t nbdbits;

		/// <summary>
		/// Definition contructor
		/// </summary>
		/// <param name="name">Fractal name (only for selection list)</param>
		/// <param name="childCount">ChildCount of Self Similars Inside (must equal the length of all the following arrays)</param>
		/// <param name="childSize">Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)</param>
		/// <param name="maxSize">The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)</param>
		/// <param name="minSize">How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)</param>
		/// <param name="cutSize">A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance)</param>
		/// <param name="childX">X coord shifts of Self Similars Inside</param>
		/// <param name="childY">Y coord shifts of Self Similars Inside</param>
		/// <param name="childAngle">Angle shifts of Self Similars Inside</param>
		/// <param name="childColor">Color shifts of Self Similars Inside</param>
		/// <param name="cutFunction">Function that takes a bitarray transforms it and decides to cut some specific patterns of Self Simmilars</param>
		Fractal(
			const std::string& name,
			const int8_t& childCount,
			const float& childSize,
			const float& maxSize,
			const float& minSize,
			const float& cutSize,
			float* childX,
			float* childY,
			std::vector<std::pair<std::string, float*>> childAngle,
			std::vector<std::pair<std::string, uint8_t*>> childColor,
			std::vector<std::pair<int32_t, std::vector<int32_t>>> cutFunction
		);

		Fractal(const Fractal& copy);

		static inline std::string ConvertToStdString(System::String^ managedString) {
			using namespace System::Runtime::InteropServices;
			const auto chars = (const char*)(Marshal::StringToHGlobalAnsi(managedString)).ToPointer();
			const auto nativeString(chars);
			Marshal::FreeHGlobal(System::IntPtr((void*)chars));
			return nativeString;
		}

#pragma region CutFunctions_UniversalSeeds
	private:
		static int64_t ApplyNoChildSimple(const int8_t index, const int64_t rules, const int64_t flags);
		static int64_t ApplyNoChildComplexAngle(const int8_t index, const int64_t rules, const int64_t flags, const int8_t lastRule, const int8_t lastIndex);
		static int64_t ApplyNoChildComplexNoAngle(const int8_t index, const int64_t rules, const int64_t flags, const int8_t lastRule, const int8_t lastIndex);
	public:
		static int64_t NoChildSimple(CFP);
		static int64_t NoChildComplexAngled(CFP);
		static int64_t NoChildComplexNoAngled(CFP);
		static int64_t NoBackDiag(CFP);
#pragma endregion

#pragma region CutFunctions_Seeds
	private:
		static int64_t GetCarpetSymmetricRules(const int64_t rules);
		static int64_t GetTetraflakeSymmetricRules(const int64_t rules);
	public:
		static int64_t TriComb(CFP);
		static int64_t Carpet_Symmetric(CFP);
		static int64_t Tetraflake_Symmetric(CFP);
		static int64_t Tetraflake_Symmetric_RadHoles(CFP);
		static int64_t Tetraflake_Symmetric_CornerHoles(CFP);
		static int64_t Tetraflake_Symmetric_TriangleHoles(CFP);
		static int64_t Beamtree_NoBeam(CFP);
#pragma endregion

#pragma region CutFunctions_Special
	public:
		static int64_t Beamtree_OuterJoint(CFP);
		static int64_t Beamtree_InnerJoint(CFP);
#pragma endregion


/*#pragma region CutFunctions_Param
	private:
		//static int FlakeTemplate(int8_t index, int inFlags, int bits);
	public:
		static int32_t NoChildSimpleParam(int8_t index, int32_t inFlags, const Fractal& f);
		static int32_t NoChildComplexParam(int8_t index, int32_t inFlags, const Fractal& f);
		static int32_t NoBackDiag(int index, int32_t inFlags, const Fractal& f);
		static int32_t TriComb_Param(int8_t index, int32_t inFlags, const Fractal& f);
#pragma endregion

#pragma region CutFunctions_Specials
	public:
		static int32_t Beamtree_OuterJoint(int8_t index, int32_t inFlags, const Fractal& f);
		static int32_t Beamtree_InnerJoint(int8_t index, int32_t inFlags, const Fractal& f);
		static int32_t Beamtree_NoBeam(int8_t index, int32_t inFlags, const Fractal& f);
	private:
		static int32_t Tetraflake_CustomCornerParam(int8_t index, int32_t inFlags, const Fractal& f, int8_t fb, int8_t fi);
	public:
		static int32_t Tetraflake_NoCornerParam(int8_t index, int32_t inFlags, const Fractal& f);
		static int32_t Tetraflake_NoCornerRadHolesParam(int8_t index, int32_t inFlags, const Fractal& f);
		static int32_t Tetraflake_NoCornerCornerHolesParam(int8_t index, int32_t inFlags, const Fractal& f);
		static int32_t Tetraflake_NoCornerTriangleHolesParam(int8_t index, int32_t inFlags, const Fractal& f);
#pragma endregion*/

	};

}
