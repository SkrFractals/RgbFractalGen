#pragma once

//#define SIMD_ENABLED

#ifdef SIMD_ENABLED
#include <cmath>
#include <algorithm>
#include <immintrin.h> // for SSE/AVX intrinsics

struct alignas(16) Vector {
	float X, Y, Z, W; // W = padding, ignore it

	constexpr Vector() noexcept : X(0), Y(0), Z(0), W(0) {}
	constexpr Vector(float v) noexcept : X(v), Y(v), Z(v), W(0) {}
	constexpr Vector(float x, float y, float z) noexcept : X(x), Y(y), Z(z), W(0) {}

	/*static Vector Lerp(const Vector& A, const Vector& B, float alpha) noexcept {
		return (1.0f - alpha) * A + alpha * B;
	}*/

	static inline Vector Lerp(const Vector& A, const Vector& B, float alpha) noexcept {
		__m128 a = _mm_load_ps(&A.X);
		__m128 b = _mm_load_ps(&B.X);
		__m128 t = _mm_set1_ps(alpha);
		__m128 one_minus_t = _mm_sub_ps(_mm_set1_ps(1.0f), t);
		__m128 res = _mm_add_ps(_mm_mul_ps(one_minus_t, a), _mm_mul_ps(t, b));
		Vector result;
		_mm_store_ps(&result.X, res);
		return result;
	}

	friend constexpr Vector operator+(const Vector& a, const Vector& b) noexcept {
		return { a.X + b.X, a.Y + b.Y, a.Z + b.Z };
	}

	friend constexpr Vector operator*(float s, const Vector& v) noexcept {
		return { s * v.X, s * v.Y, s * v.Z };
	}

	friend constexpr Vector operator*(const Vector& v, float s) noexcept {
		return { v.X * s, v.Y * s, v.Z * s };
	}

	Vector& operator+=(const Vector& rhs) noexcept {
		X += rhs.X; Y += rhs.Y; Z += rhs.Z; return *this;
	}

	inline Vector Frac() const noexcept {
		return { X - static_cast<uint8_t>(X), Y - static_cast<uint8_t>(Y), Z - static_cast<uint8_t>(Z) };
	}

	inline float Max() const noexcept { return std::max({ X, Y, Z }); }
	inline float Min() const noexcept { return std::min({ X, Y, Z }); }
	inline float Sum() const noexcept { return X + Y + Z; }
};

#else // SIMD_ENABLED
public struct Vector {

	float X, Y, Z;
	constexpr Vector() noexcept : X(0), Y(0), Z(0) {}
	constexpr Vector(const float v) noexcept : X(v), Y(v), Z(v) {}
	constexpr Vector(const float x, const float y, const float z) noexcept : X(x), Y(y), Z(z) {}
	static Vector Lerp(const Vector& A, const Vector& B, const float alpha) noexcept {
		//return Vector(A.X + alpha * (B.X - A.X), A.Y + alpha * (B.Y - A.Y), A.Z + alpha * (B.Z - A.Z));
		//return A + alpha * (B - A);
		return (1.0f - alpha) * A + alpha * B;
	}
	static friend Vector operator+(const Vector& lhs, const Vector& rhs) noexcept {
		return Vector(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
	}
	static friend Vector operator*(const float lhs, const Vector& rhs) noexcept {
		return Vector(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z);
	}
	static friend Vector operator+(const float lhs, const Vector& rhs) noexcept {
		return Vector(lhs + rhs.X, lhs + rhs.Y, lhs + rhs.Z);
	}
	static Vector MultiplyMinus(const Vector& V, const float mul, const float min) noexcept {
		return Vector(mul * V.X - min, mul * V.Y - min, mul * V.Z - min);
	}
	Vector& operator+=(const Vector& rhs) noexcept {
		X += rhs.X;
		Y += rhs.Y;
		Z += rhs.Z;
		return *this;
	}
	/*Vector& MultiAdd(const float Multiply, const float Add) {
		X = Multiply * X + Add;
		Y = Multiply * Y + Add;
		Z = Multiply * Z + Add;
		return *this;
	}

	Vector& MultiAdd(const float MultiplyVector, const Vector& MultipliedVector, const float Add) {
		X += MultiplyVector * MultipliedVector.X + Add;
		Y += MultiplyVector * MultipliedVector.Y + Add;
		Z += MultiplyVector * MultipliedVector.Z + Add;
		return *this;
	}*/
	inline Vector Frac() const noexcept {
		return Vector(X - static_cast<uint8_t>(X), Y - static_cast<uint8_t>(Y), Z - static_cast<uint8_t>(Z));
	}
	inline float Max() const noexcept {
		return System::Math::Max(System::Math::Max(X, Y), Z);
	}
	inline float Min() const noexcept {
		return System::Math::Min(System::Math::Min(X, Y), Z);
	}
	inline float Sum() const noexcept {
		return X + Y + Z;
	}
};
static Vector unitX(1, 0, 0);
static Vector unitY(0, 1, 0);
static Vector unitZ(0, 0, 1);
static Vector zero(0, 0, 0);

static Vector Y(const Vector& X) { return Vector(X.Z, X.X, X.Y); }
static Vector Z(const Vector& X) { return Vector(X.Y, X.Z, X.X); }
#endif // SIMD_ENABLED