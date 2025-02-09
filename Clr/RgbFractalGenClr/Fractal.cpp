#include "Fractal.h"

// Performance macros

#define Param(inFlags, flagbits, rulebits)\
	if (inFlags < 0) inFlags = ((-inFlags) & ((1 << rulebits) - 1)) << flagbits;\
	int32_t mask = ((1 << flagbits) - 1);\
	int32_t newFlags = inFlags & ~mask;\
	int32_t rules = (inFlags >> flagbits) & ((1 << rulebits) - 1);\
	int32_t flags = inFlags & mask;

#define FlakeTemplate(index, inFlags, bits)\
	Param(inFlags, (bits + 1), (bits + 2));\
	if (index == 0 && (inFlags & (1 << bits)) > 0) return -1;\
	if (index == 0) return newFlags;\
	else {\
		int32_t cb = 1 << bits;\
		if ((rules & cb) > 0) newFlags |= cb;\
		if ((rules & (2 << bits)) > 0) {\
			if ((inFlags & cb) > 0) newFlags &= ~cb;\
			else newFlags |= cb;\
		}\
	} if ((flags >> (index - 1) & 1) == 1) return -1;\
	int32_t m = ((1 << bits) - 1);\
	int32_t r = rules & m;\
	newFlags |= ((r >> (bits - index + 1)) | (r << (index - 1))) & m;\
	return newFlags;

namespace RgbFractalGenClr {
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
		std::tuple<std::string, CutFunction, int32_t*>* cutFunction
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
		nbdbits = (childCount / 2) * ((childCount - 1) / 2);
	}

#pragma region CutFunctions_Param
	int32_t Fractal::NoChildSimpleParam(int8_t index, int32_t inFlags, const Fractal& f) {
		// Not a very good one...
		Param(inFlags, 9, 9);
		if (index == 0)
			return newFlags;
		if ((flags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= rules;
		return newFlags;
	}
	int32_t Fractal::NoChildComplexParam(int8_t index, int32_t inFlags, const Fractal& f) {
		Param(inFlags, f.childCount, f.childCount + 1);
		uint8_t c = f.childCount - 1;
		if (index == 0 && (inFlags & (1 << c)) > 0)
			return -1;
		if (index == 0)
			return newFlags;
		else {
			const uint32_t cb = 1 << c;
			if ((rules & cb) > 0)
				newFlags |= cb;
			if ((rules & (2 << c)) > 0) {
				if ((inFlags & cb) > 0)
					newFlags &= ~cb;
				else
					newFlags |= cb;
			}
		}
		if ((flags >> (index - 1) & 1) == 1)
			return -1;
		uint32_t m = (1 << c) - 1, r = rules & m;
		newFlags |= ((r >> (c - index + 1)) | (r << (index - 1))) & m;
		return newFlags;
	}
	int32_t Fractal::NoBackDiag(int index, int32_t inFlags, const Fractal& f) {
		uint8_t c = f.childCount - 1;
		Param(inFlags, c, f.nbdbits);
		if (index == 0)
			return newFlags;
		uint32_t p, m, n = 1, ic = index + c - 1;
		for (uint8_t i = 1; i <= c - 2; ++i)
			for (uint8_t b = i / 2; ++b <= i; n *= 2)
				if ((rules & n) > 0) {
					p = b; m = i - b + 1;
					if (((flags >> (ic - p) % c & flags >> (ic + m) % c) & 1) == 1)
						return -1;
					if (((flags >> (ic + p) % c & flags >> (ic - m) % c) & 1) == 1)
						return -1;
				}
		--ic;
		//rules += n; // skipped a lot of empty params at the beginning, but not needed anymore with hashes
		for (uint8_t i = c / 2; 0 <= --i; n *= 2)
			if ((rules & n) > 0) {
				if ((inFlags >> (index + i) % c & 1) == 1)
					newFlags |= 1 << (index + i) % c;
				if ((inFlags >> (ic - i) % c & 1) == 1)
					newFlags |= 1 << (ic - i) % c;
			}
		newFlags |= 1 << (index - 1);
		return newFlags;
	}
	int32_t Fractal::TriComb_Param(int8_t index, int32_t inFlags, const Fractal& f) {
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

#pragma region CutFunctions_Special
	int32_t Fractal::Beamtree_OuterJoint(int8_t index, int32_t inFlags, const Fractal& f) {
		if (inFlags < 0) inFlags = 0; // No params
		int32_t newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		newFlags |= 1 << (12 - index) % 6;
		return newFlags;
	}
	int32_t Fractal::Beamtree_InnerJoint(int8_t index, int32_t inFlags, const Fractal& f{
		if (inFlags < 0) inFlags = 0; // No params
		int32_t newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		newFlags |= 1 << (index + 5) % 6;
		return newFlags;
	}
	int32_t Fractal::Beamtree_NoBeam(int8_t index, int32_t inFlags, const Fractal& f) {
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
	int32_t Fractal::Tetraflake_CustomCornerParam(int8_t index, int32_t inFlags, const Fractal& f, int8_t fb, int8_t fi) {
		Param(inFlags, fb, fb);
		if (index == 0)
			return newFlags;
		if ((flags >> (index - fi) & 1) == 1)
			return -1;
		newFlags |= rules;
		return newFlags;
	}
	int32_t Fractal::Tetraflake_NoCornerParam(int8_t index, int32_t inFlags, const Fractal& f) {
		return Tetraflake_CustomCornerParam(index, inFlags, f, 15, 1);
	}
	int32_t Fractal::Tetraflake_NoCornerRadHolesParam(int8_t index, int32_t inFlags, const Fractal& f) {
		if (index >= 13)
			return -1;
		return Tetraflake_CustomCornerParam(index, inFlags, f, 12, 1);
	}
	int32_t Fractal::Tetraflake_NoCornerCornerHolesParam(int8_t index, int32_t inFlags, const Fractal& f) {
		if (index >= 10 && index < 13)
			return -1;
		return Tetraflake_CustomCornerParam(index, inFlags, f, 9, 1);
	}
	int32_t Fractal::Tetraflake_NoCornerTriangleHolesParam(int8_t index, int32_t inFlags, const Fractal& f) {
		if (index > 0 && index <= 3)
			return -1;
		return Tetraflake_CustomCornerParam(index, inFlags, f, 12, 4);
	}
#pragma endregion

}

/*int32_t Fractal::Hexaflake_NoCornerParam(int8_t index, int32_t inFlags, const Fractal& f) {
		FlakeTemplate(index, inFlags, 6);
	}
	int32_t Fractal::Hexaflake_NoBackDiag(int8_t index, int32_t inFlags, const Fractal& f) {
		if (inFlags < 0) inFlags = 0; // No params
		int32_t newFlags = 0;
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
	int32_t Fractal::Pentaflake_NoCornerParam(int8_t index, int32_t inFlags, const Fractal& f) {
		FlakeTemplate(index, inFlags, 5);
	}
	int32_t Fractal::Pentaflake_NoBackDiag(int8_t index, int32_t inFlags, const Fractal& f) {
		if (inFlags < 0) inFlags = 0; // No params
		int32_t newFlags = 0;
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
	int32_t Fractal::Carpet_NoCornerParam(int8_t index, int32_t inFlags, const Fractal& f) {
		FlakeTemplate(index, inFlags, 8);
	}
	int32_t Fractal::Carpet_NoBackDiag(int8_t index, int32_t inFlags, const Fractal& f) {
		if (inFlags < 0) inFlags = 0; // No params
		int32_t newFlags = 0;
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
	int32_t Fractal::Carpet_NoBackDiag2(int8_t index, int32_t inFlags, const Fractal& f) {
		if (inFlags < 0) inFlags = 0; // No params
		int32_t newFlags = 0;
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
	*/