#pragma once
#include <cstdint>
#include <utility>
#include <string>

namespace RgbFractalGenClr {

	//using namespace System;

	class Fractal {
	public:
		typedef int32_t (*CutFunction)(int8_t index, int32_t inFlags, const Fractal& f);
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
		std::tuple<std::string, CutFunction, int32_t*>* cutFunction;	// Function that takes a bitarray transforms it and decides to cut some specific patterns of Self Simmilars
		
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
			std::pair<std::string, float*>* childAngle,
			std::pair<std::string, uint8_t*>* childColor,
			std::tuple<std::string, CutFunction, int32_t*>* cutFunction
		);

#pragma region CutFunctions_Param
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
#pragma endregion

	};

}
