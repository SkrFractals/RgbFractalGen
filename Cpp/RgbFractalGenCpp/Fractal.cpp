#include "Fractal.h"

// Performance macros

#define Param(inFlags, flagbits, rulebits)\
	if (inFlags < 0) inFlags = ((-inFlags) & ((1 << rulebits) - 1)) << flagbits;\
	int mask = ((1 << flagbits) - 1);\
	int newFlags = inFlags & ~mask;\
	int rules = (inFlags >> flagbits) & ((1 << rulebits) - 1);\
	int flags = inFlags & mask;

#define FlakeTemplate(index, inFlags, bits)\
	Param(inFlags, (bits + 1), (bits + 2));\
	if (index == 0 && (inFlags & (1 << bits)) > 0) return -1;\
	if (index == 0) return newFlags;\
	else {\
		int cb = 1 << bits;\
		if ((rules & cb) > 0) newFlags |= cb;\
		if ((rules & (2 << bits)) > 0) {\
			if ((inFlags & cb) > 0) newFlags &= ~cb;\
			else newFlags |= cb;\
		}\
	} if ((flags >> (index - 1) & 1) == 1) return -1;\
	int m = ((1 << bits) - 1);\
	int r = rules & m;\
	newFlags |= ((r >> (bits - index + 1)) | (r << (index - 1))) & m;\
	return newFlags;

namespace RgbFractalGenCpp {
	// Constructor
	Fractal::Fractal(
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
	) {
		this->name = name;
		this->childCount = childCount;
		this->childSize = childSize;
		this->maxSize = maxSize;
		this->minSize = minSize;
		this->cutSize = cutSize;
		this->childX = childX;
		this->childY = childY;
		this->childAngle = childAngle;
		this->childColor = childColor;
		this->cutFunction = cutFunction;
	}


#pragma region CutFunctions_Param
	int Fractal::Trees_NoChildParam(int8_t index, int inFlags) {
		// Not a very good one...
		Param(inFlags, 9, 9);
		if (index == 0)
			return newFlags;
		if ((flags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= rules;
		return newFlags;
	}
	int Fractal::TriComb_Param(int8_t index, int inFlags) {
		Param(inFlags, 13, 6);
		if ((inFlags >> index & 1) == 1)
			return -1;
		if ((rules & 1) > 0 && index > 3 && index < 10) // Med
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9; // Inner
		if ((rules & 2) > 0 && index > 3 && index < 10) // Med
			newFlags |= 1 << 7 | 1 << 6 | 1 << 11; // Outer
		if ((rules & 4) > 0 && index > 9 && index < 13) // Out
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		if ((rules & 8) > 0 && index > 0 && index < 4) // In
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		if ((rules & 16) > 0 && index > 0 && index < 4)
			newFlags |= 1 << 0;
		if ((rules & 32) > 0 && index > 3 && index < 10)
			newFlags |= 1 << 0;
		return newFlags;
	}
#pragma endregion

#pragma region CutFunctions_Triflake
	int Fractal::Triflake_NoCornerParam(int8_t index, int inFlags) {
		FlakeTemplate(index, inFlags, 3);
	}
	int Fractal::Triflake_NoBackDiag(int8_t index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index) % 3 & inFlags >> (index + 1) % 3 & 1) == 1)
			return -1;
		if ((inFlags >> (index + 0) % 3 & 1) == 1)
			newFlags |= 1 << (index + 0) % 3;
		if ((inFlags >> (index + 1) % 3 & 1) == 1)
			newFlags |= 1 << (index + 1) % 3;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}
#pragma endregion

#pragma region CutFunctions_TetraTriflake
	int Fractal::Tetraflake_CustomCornerParam(int8_t index, int inFlags, int8_t fb, int8_t fi) {
		Param(inFlags, fb, fb);
		if (index == 0)
			return newFlags;
		if ((flags >> (index - fi) & 1) == 1)
			return -1;
		newFlags |= rules;
		return newFlags;
	}
	int Fractal::Tetraflake_NoCornerParam(int8_t index, int inFlags) {
		return Tetraflake_CustomCornerParam(index, inFlags, 15, 1);
	}
	int Fractal::Tetraflake_NoCornerRadHolesParam(int8_t index, int inFlags) {
		if (index >= 13)
			return -1;
		return Tetraflake_CustomCornerParam(index, inFlags, 12, 1);
	}
	int Fractal::Tetraflake_NoCornerCornerHolesParam(int8_t index, int inFlags) {
		if (index >= 10 && index < 13)
			return -1;
		return Tetraflake_CustomCornerParam(index, inFlags, 9, 1);
	}
	int Fractal::Tetraflake_NoCornerTriangleHolesParam(int8_t index, int inFlags) {
		if (index > 0 && index <= 3)
			return -1;
		return Tetraflake_CustomCornerParam(index, inFlags, 12, 4);
	}
#pragma endregion

#pragma region CutFunctions_Carpet
	int Fractal::Carpet_NoCornerParam(int8_t index, int inFlags) {
		FlakeTemplate(index, inFlags, 8);
	}
	int Fractal::Carpet_NoBackDiag(int8_t index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index + 1) % 8 & inFlags >> (index + 4) % 8 & 1) == 1)
			return -1;
		if ((inFlags >> (index + 2) % 8 & inFlags >> (index + 5) % 8 & 1) == 1)
			return -1;

		if ((inFlags >> (index + 1) % 8 & inFlags >> (index + 5) % 8 & 1) == 1)
			return -1;

		if ((inFlags >> (index + 2) % 8 & inFlags >> (index + 6) % 8 & 1) == 1)
			return -1;
		if ((inFlags >> (index + 0) % 8 & inFlags >> (index + 4) % 8 & 1) == 1)
			return -1;

		if ((inFlags >> (index + 2) % 8 & 1) == 1)
			newFlags |= 1 << (index + 2) % 8;
		if ((inFlags >> (index + 3) % 8 & 1) == 1)
			newFlags |= 1 << (index + 3) % 8;
		if ((inFlags >> (index + 4) % 8 & 1) == 1)
			newFlags |= 1 << (index + 4) % 8;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}
	int Fractal::Carpet_NoBackDiag2(int8_t index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index + 1) % 8 & inFlags >> (index + 4) % 8 & 1) == 1)
			return -1;
		if ((inFlags >> (index + 2) % 8 & inFlags >> (index + 5) % 8 & 1) == 1)
			return -1;
		if ((inFlags >> (index + 2) % 8 & 1) == 1)
			newFlags |= 1 << (index + 2) % 8;
		if ((inFlags >> (index + 4) % 8 & 1) == 1)
			newFlags |= 1 << (index + 4) % 8;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}
#pragma endregion

#pragma region CutFunctions_Pentaflake
	int Fractal::Pentaflake_NoCornerParam(int8_t index, int inFlags) {
		FlakeTemplate(index, inFlags, 5);
	}
	int Fractal::Pentaflake_NoBackDiag(int8_t index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index + 2) % 5 & inFlags >> index % 5 & 1) == 1)
			return -1;
		if ((inFlags >> (index + 3) % 5 & inFlags >> (index + 1) % 5 & 1) == 1)
			return -1;
		if ((inFlags >> (index + 1) % 5 & 1) == 1)
			newFlags |= 1 << (index + 1) % 5;
		if ((inFlags >> (index + 2) % 5 & 1) == 1)
			newFlags |= 1 << (index + 2) % 5;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}
#pragma endregion

#pragma region CutFunctions_Hexaflake
	int Fractal::Hexaflake_NoCornerParam(int8_t index, int inFlags) {
		FlakeTemplate(index, inFlags, 6);
	}
	int Fractal::Hexaflake_NoBackDiag(int8_t index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index + 1) % 6 & inFlags >> (index + 3) % 6 & 1) == 1)
			return -1; // diagonals (LL+RR)
		if ((inFlags >> (index + 0) % 6 & inFlags >> (index + 3) % 6 & 1) == 1)
			return -1; // backwards (L+RR)
		if ((inFlags >> (index + 1) % 6 & inFlags >> (index + 4) % 6 & 1) == 1)
			return -1; // backwards (LL+R)
		if ((inFlags >> (index + 1) % 6 & 1) == 1)
			newFlags |= 1 << (index + 1) % 6; // backdiagonal
		if ((inFlags >> (index + 2) % 6 & 1) == 1)
			newFlags |= 1 << (index + 2) % 6; // backdiagonal
		if ((inFlags >> (index + 3) % 6 & 1) == 1)
			newFlags |= 1 << (index + 3) % 6; // backback

		newFlags |= 1 << (index - 1);
		return newFlags;
	}
#pragma endregion

#pragma region CutFunctions_BeamTree
	int Fractal::Beamtree_OuterJoint(int8_t index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		newFlags |= 1 << (12 - index) % 6;
		return newFlags;
	}
	int Fractal::Beamtree_InnerJoint(int8_t index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		newFlags |= 1 << (index + 5) % 6;
		return newFlags;
	}
	int Fractal::Beamtree_NoBeam(int8_t index, int inFlags) {
		Param(inFlags, 9, 2);
		//if (inFlags < 0) inFlags = 0; // No params
		//int newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		if ((rules & 1) > 0)
			newFlags |= 1 << (3 * index + 2) % 6;// 002244 InnerBeam
		if ((rules & 2) > 0)
			newFlags |= 1 << (3 * index + 5) % 6;// 002244 OuterBeam
		return newFlags;
	}
#pragma endregion

}
