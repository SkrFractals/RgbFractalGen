#pragma once
#include <cstdint>
#include <utility>
#include <string>

namespace RgbFractalGenClr {

	//using namespace System;

	class Fractal {
	public:
		typedef int (*CutFunction)(int8_t index, int inFlags);
		// Properties
		std::string name;										// Fractal name (only for selection list)
		int8_t childCount;										// ChildCount of Self Similars Inside (must equal the length of all the following arrays)
		float childSize;										// Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)
		float maxSize;											// The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)
		float minSize;											// How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)
		float cutSize;											// A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance)
		float* childX;											// X coord shifts of Self Similars Inside
		float* childY;											// Y coord shifts of Self Similars Inside
		std::pair<std::string, float*>* childAngle;	// Angle shifts of Self Similars Inside
		std::pair<std::string, uint8_t*>* childColor;		// Color shifts of Self Similars Inside
		std::pair<std::string, CutFunction>* cutFunction;	// Function that takes a bitarray transforms it and decides to cut some specific patterns of Self Simmilars
		// Autoproperties
		float periodAngle;
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
			std::pair<std::string, float*>* childAngle,
		std::pair<std::string, uint8_t*>* childColor,
		std::pair<std::string, CutFunction>* cutFunction
		);

#pragma region CutFunctions_Param
	private:
		//static int FlakeTemplate(int8_t index, int inFlags, int bits);
	public:
		static int Trees_NoChildParam(int8_t index, int inFlags);
		static int TriComb_Param(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_Triflake
	public:
		static int Triflake_NoCornerParam(int8_t index, int inFlags);
		static int Triflake_NoBackDiag(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_TetraTriflake
	private:
		static int Tetraflake_CustomCornerParam(int8_t index, int inFlags, int8_t fb, int8_t fi);
	public:
		static int Tetraflake_NoCornerParam(int8_t index, int inFlags);
		static int Tetraflake_NoCornerRadHolesParam(int8_t index, int inFlags);
		static int Tetraflake_NoCornerCornerHolesParam(int8_t index, int inFlags);
		static int Tetraflake_NoCornerTriangleHolesParam(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_Carpet
	public:
		static int Carpet_NoCornerParam(int8_t index, int inFlags);
		static int Carpet_NoBackDiag(int8_t index, int inFlags);
		static int Carpet_NoBackDiag2(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_Pentaflake
	public:
		static int Pentaflake_NoCornerParam(int8_t index, int inFlags);
		static int Pentaflake_NoBackDiag(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_Hexaflake
	public:
		static int Hexaflake_NoCornerParam(int8_t index, int inFlags);
		static int Hexaflake_NoBackDiag(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_BeamTree
	public:
		static int Beamtree_OuterJoint(int8_t index, int inFlags);
		static int Beamtree_InnerJoint(int8_t index, int inFlags);
		static int Beamtree_NoBeam(int8_t index, int inFlags);
#pragma endregion

	};

}


/*#pragma region CutFunctions_Param
	public:
		static int Hexaflake_NoCornerParam(int8_t index, int inFlags);
		static int Pentaflake_NoCornerParam(int8_t index, int inFlags);
		static int Triflake_NoCornerParam(int8_t index, int inFlags);
		static int Trees_NoChildParam(int8_t index, int inFlags);
		static int TriComb_Param(int8_t index, int inFlags);
		static int Carpet_NoCornerParam(int8_t index, int inFlags);
		static int Tetraflake_NoCornerParam(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_NoKochs
		static int Pentaflake_NoKoch(int8_t index, int inFlags);
		static int Sierp_NoKoch2(int8_t index, int inFlags);
		static int Hexaflake_NoKoch(int8_t index, int inFlags);
		static int Sierp_NoKoch(int8_t index, int inFlags);
		static int Triflake_NoKoch(int8_t index, int inFlags);
#pragma endregion

#pragma region CutFunctions_Old
	public:
		static int Beamtree_OuterJoint(int8_t index, int inFlags);
		static int Beamtree_InnerJoint(int8_t index, int inFlags);
		static int Beamtree_InnerBeam(int8_t index, int inFlags);
		static int Beamtree_OuterBeam(int8_t index, int inFlags);
		static int Beamtree_NoBeam(int8_t index, int inFlags);
		static int Triflake_NoCenter(int8_t index, int inFlags);
#pragma endregion*/
/// <summary>
/// Copy constructor
/// </summary>
/// <param name="copy">Parent practal to copy properties from</param>
/// <param name="name">New copy name</param>
/// /// <param name="childCount">If not -1, replace childCount with this one</param>
/// <param name="childAngle">If not nullptr, replace childAngle with this one</param>
/// <param name="childColor">If not nullptr, replace childColor with this one</param>
/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
/*Fractal(
	const Fractal^ copy,
	System::String^ name,
	const int8_t childCount,
	float* childAngle,
	int8_t* childColor,
	CutFunction^ cutFunction
);*/
/// <summary>
/// Copy function
/// </summary>
/// <param name="name">New copy name</param>
/// <param name="childCount">If not -1, replace childCount with this one</param>
/// <param name="childAngle">If not nullptr, replace childAngle with this one</param>
/// <param name="childColor">If not nullptr, replace childColor with this one</param>
/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
/// <returns>Copy fractal</returns>
/*inline Fractal^ Copy(
	System::String^ name,
	const int8_t childCount,
	float* childAngle,
	int8_t* childColor,
	CutFunction^ cutFunction
) {
	return gcnew Fractal(this, name, childCount, childAngle, childColor, cutFunction);
}*/
/// <summary>
/// Copy function
/// </summary>
/// <param name="name">New copy name</param>
/// <param name="childAngle">If not nullptr, replace childAngle with this one</param>
/// <param name="childColor">If not nullptr, replace childColor with this one</param>
/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
/// <returns>Copy fractal</returns>
/*inline Fractal^ Copy(
	System::String^ name,
	float* childAngle,
	int8_t* childColor,
	CutFunction^ cutFunction
) {
	return gcnew Fractal(this, name, childCount, childAngle, childColor, cutFunction);
}*/
/// <summary>
/// Copy function
/// </summary>
/// <param name="name">New copy name</param>
/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
/// <returns>Copy fractal</returns>
/*inline Fractal^ Copy(
	System::String^ name,
	CutFunction^ cutFunction
) {
	return gcnew Fractal(this, name, childCount, nullptr, nullptr, cutFunction);
}*/