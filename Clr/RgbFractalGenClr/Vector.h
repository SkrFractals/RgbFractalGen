#pragma once
public struct Vector {

	float X, Y, Z;
	Vector() {
		X = Y = Z = 0;
	}
	Vector(const float v) {
		X = Y = Z = v;
	}
	Vector(const float x, const float y, const float z) {
		X = x; Y = y; Z = z;
	}
	static Vector Lerp(const Vector& A, const Vector& B, const float alpha) {
		//return Vector(A.X + alpha * (B.X - A.X), A.Y + alpha * (B.Y - A.Y), A.Z + alpha * (B.Z - A.Z));
		//return A + alpha * (B - A);
		return (1.0f - alpha) * A + alpha * B;
	}
	static friend Vector operator+(const Vector& lhs, const Vector& rhs) {
		return Vector(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
	}
	static friend Vector operator*(const float lhs, const Vector& rhs) {
		return Vector(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z);
	}
	static friend Vector operator+(const float lhs, const Vector& rhs) {
		return Vector(lhs + rhs.X, lhs + rhs.Y, lhs + rhs.Z);
	}
	static Vector MultiplyMinus(const Vector& V, const float mul, const float min) {
		return Vector(mul * V.X - min, mul * V.Y - min, mul * V.Z - min);
	}
	Vector& operator+=(const Vector& rhs) {
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

	inline float Max() const {
		return System::Math::Max(System::Math::Max(X, Y), Z);
	}
	inline float Min() const {
		return System::Math::Min(System::Math::Min(X, Y), Z);
	}
	inline float Sum() const {
		return X + Y + Z;
	}
};
static Vector unitX(1, 0, 0);
static Vector unitY(0, 1, 0);
static Vector unitZ(0, 0, 1);
static Vector zero(0, 0, 0);

static Vector Y(const Vector& X) { return Vector(X.Z, X.X, X.Y); }
static Vector Z(const Vector& X) { return Vector(X.Y, X.Z, X.X); }

public ref class ManagedVector {
public:
	float X, Y, Z;
	ManagedVector(float x, float y, float z) : X(x), Y(y), Z(z) {}

	// Convert back to native Vector
	Vector ToNative() {
		Vector v;
		v.X = X;
		v.Y = Y;
		v.Z = Z;
		return v;
	}
	void FromVector(const Vector& V) {
		X = V.X;
		Y = V.Y;
		Z = V.Z;
	}
};

public ref struct VecRefWrapper {
	Vector**& T;
	ManagedVector^ I;
	ManagedVector^ H;
	//uint16_t Tasks;
	VecRefWrapper(Vector**& buffT, ManagedVector^ blendI, ManagedVector^ blendH) : T(buffT) {
		I = blendI;
		H = blendH;
	}
};

