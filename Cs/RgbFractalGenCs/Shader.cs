using ComputeSharp;
namespace RgbFractalGenCs;

#region Math
public readonly struct NoiseCoordData {
	public readonly Int4 i;
	public readonly Float4 b;
	public NoiseCoordData(Int4 i, Float4 b) {
		this.i = i;
		this.b = b;
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public static class ShaderMath {

	// Void:
	public static Float3 Noise(float voidAmb, float selectedAmbient, Float4 b, Float3 n0, Float3 n1, Float3 n2, Float3 n3) {
		var voidAmbS = selectedAmbient * voidAmb;
		return new Float3(voidAmbS, voidAmbS, voidAmbS)
			+ Hlsl.Mul((1.0f - voidAmb) * voidAmb, Hlsl.Mul(b.X, n0) + Hlsl.Mul(b.Y, n1) + Hlsl.Mul(b.Z, n2) + Hlsl.Mul(b.W, n3));
	}
	public static NoiseCoordData NoiseCoords(Int2 xy, int width, Int2 noiseP) {
		var fy = (float)xy.Y / (noiseP.Y);
		var startY = (int)fy;
		var ay = fy - startY;
		var oy = 1f - ay;
		var fx = (float)xy.X / noiseP.Y;
		var startX = (int)fx;
		int noiseI = startY * noiseP.X + startX;
		float ax = fx - startX, ox = 1f - ax, b0 = ox * oy, b1 = ax * oy, b2 = ox * ay, b3 = ax * ay;
		return new NoiseCoordData(new Int4(noiseI,noiseI + 1, noiseI + noiseP.X, noiseI + noiseP.X + 1), new Float4(b0, b1, b2, b3));
	}
	public static Float3 Ambient(I voidA, float voidDepthMax, int selectedAmbient) {
		float voidAmb = selectedAmbient * voidA.V / voidDepthMax;
		return new Float3(voidAmb, voidAmb, voidAmb);
	}
	// Saturation
	public static Float3 ApplySaturate(Float3 rgb, float selectedSaturate) {
		float min = Hlsl.Min(Hlsl.Min(rgb.X, rgb.Y), rgb.Z);
		float max = Hlsl.Max(Hlsl.Max(rgb.X, rgb.Y), rgb.Z);
		if (max <= min) return rgb;
		float mMax = max * selectedSaturate / (max - min);
		float mm = min * mMax;
		return Hlsl.Mul(mMax + 1 - selectedSaturate, rgb) - new Float3(mm,mm,mm);
	}
	public static Float3 Identity(Float3 v, Float3 _) => v;
	// Dither
	public static Float3 Dithering(Float3 rgb, int2 xy) {
		var frac = Hlsl.Frac(rgb); // fractional part of each channel
		return rgb - frac + Hlsl.Select(new Bool3(
			Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(new Float2(xy.X, xy.Y), new Float2(12.9898f, 78.233f))) * 43758.5453f) < frac.X,
			Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(new Float2(xy.X, xy.Y), new Float2(93.9898f, 67.345f))) * 43758.5453f) < frac.Y,
			Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(new Float2(xy.X, xy.Y), new Float2(44.123f, 99.321f))) * 43758.5453f) < frac.Z
		), new Float3(1,1,1), new Float3(0,0,0));
	}
	// Helpers:
	public static Float3 Normalize(Float3 rgb, float n) {
		var max = Hlsl.Max(Hlsl.Max(rgb.X, rgb.Y), rgb.Z);
		return n * max > 254.0f ?  Hlsl.Mul(254.0f / max, rgb) : Hlsl.Mul(n, rgb);
	}
	public static int ToBytes(Float3 p) {
		return (255 << 24) | ((int)p.Z << 16) | ((int)p.Y << 8) | (int)p.X;
	}
}
#endregion

#region Draw_Pipeline
/*[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeNoise : IComputeShader {
	// 1. Buffers grouped together and by type
	public readonly ReadWriteBuffer<Float3> output;
	public readonly ReadWriteBuffer<Float3> noise;
	public readonly ReadWriteBuffer<I> voidT;

	// 2. Uniforms grouped last
	public readonly int width;
	public readonly int selectedAmbient;
	public readonly int voidDepthMax;
	public readonly int noiseWidth;
	public readonly int allocVoid;

	public PipeNoise(
		ReadWriteBuffer<Float3> output,
		ReadWriteBuffer<Float3> noise,
		ReadWriteBuffer<I> voidT,
		int width,
		int selectedAmbient,
		int voidDepthMax,
		int noiseWidth,
		int allocVoid) {
		this.output = output;
		this.noise = noise;
		this.voidT = voidT;

		this.width = width;
		this.selectedAmbient = selectedAmbient;
		this.voidDepthMax = voidDepthMax;
		this.noiseWidth = noiseWidth;
		this.allocVoid = allocVoid;
	}
	public void Execute() {
	}
}*/

/*[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeNoise(
	ReadWriteBuffer<Float3> output,
	ReadWriteBuffer<int> voidT, 
	ReadWriteBuffer<Float3> noise,
	int width, int selectedAmbient, int voidDepthMax, int noiseWidth, int allocVoid
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		output[xy.Y * width + xy.X] = new Float3(0, 0, 0);
		output[xy.Y * width + xy.X] = ShaderMath.Noise(voidT, noise, xy, width, selectedAmbient, voidDepthMax, noiseWidth, allocVoid);
	}
}*/


#endregion

#region Noise
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoiseSaturateDither(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP,
	float saturateAmount
) : IComputeShader {
	public void Execute() {

		/*var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;

		var fy = (float)xy.Y / (noiseP.Y * width);
		var startY = (int)fy;
		var ay = fy - startY;
		var oy = 1f - ay;
		var fx = (float)xy.X / noiseP.Y;
		var startX = (int)fx;
		int noiseI = startY * noiseP.X + startX;
		float ax = fx - startX, ox = 1f - ax, b0 = ox * oy, b1 = ax * oy, b2 = ox * ay, b3 = ax * ay;
		var nc = new NoiseCoordData(new Int4(noiseI, noiseI + 1, noiseI + noiseP.X, noiseI + noiseP.X + 1), new Float4(b0, b1, b2, b3));

		var voidAmb = voidT[i].V / voidDepthMax;
		var voidAmbS = selectedAmbient * voidAmb;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount)
			+ new Float3(voidAmbS, voidAmbS, voidAmbS)
			+ Hlsl.Mul((1.0f - voidAmb) * voidAmb, Hlsl.Mul(nc.b.X, noise[nc.i.X]) + Hlsl.Mul(nc.b.Y, noise[nc.i.Y]) + Hlsl.Mul(nc.b.Z, noise[nc.i.Z]) + Hlsl.Mul(nc.b.W, noise[nc.i.W]))
			, xy));*/

		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, width, noiseP);
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.Noise(voidT[i].V / voidDepthMax, selectedAmbient, nc.b, 
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W])
			+ ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoiseSaturate(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, width, noiseP);
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Noise(voidT[i].V / voidDepthMax, selectedAmbient, nc.b, 
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W])
			+ ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoiseDither(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, width, noiseP);
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.Noise(voidT[i].V / voidDepthMax, selectedAmbient, nc.b, 
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W])
			+ ShaderMath.Normalize(buffer[i], lightNormalizer), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoise(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, width, noiseP);
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Noise(voidT[i].V / voidDepthMax, selectedAmbient, nc.b, 
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W])
			+ ShaderMath.Normalize(buffer[i], lightNormalizer));
	}
}
#endregion

#region Ambient
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientSaturateDither(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient)
			+ ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientSaturate(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient)
			+ ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientDither(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient)
			+ ShaderMath.Normalize(buffer[i], lightNormalizer), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbient(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<I> voidT, int selectedAmbient, float voidDepthMax
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient)
			+ ShaderMath.Normalize(buffer[i], lightNormalizer));
	}
}
#endregion

#region NoVoid
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawSaturateDither(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawSaturate(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawDither(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(ShaderMath.Normalize(buffer[i], lightNormalizer), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct Draw(
	ReadWriteBuffer<int> output, ReadOnlyBuffer<Float3> buffer, int width, float lightNormalizer
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Normalize(buffer[i], lightNormalizer));
	}
}
#endregion

/*
#region Noise_Old




#endregion

#region Ambient
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientSaturateDither(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer,
	ReadOnlyBuffer<I> voidT, int selectedAmbient, int voidDepthMax,
	Float3 saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(ShaderMath.Dithering(
			ShaderMath.Ambient(voidT, xy, width, selectedAmbient, voidDepthMax)
			+ ShaderMath.ApplySaturate(ShaderMath.Normalize(rgb, lightNormalizer), saturateAmount), xy).XYZ, 1f);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientSaturate(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer,
	ReadOnlyBuffer<I> voidT, int selectedAmbient, int voidDepthMax,
	Float3 saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(
			(ShaderMath.Ambient(voidT, xy, width, selectedAmbient, voidDepthMax)
			+ ShaderMath.ApplySaturate(ShaderMath.Normalize(rgb, lightNormalizer), saturateAmount)).XYZ, 1f);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientDithering(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer,
	ReadOnlyBuffer<I> voidT, int selectedAmbient, int voidDepthMax
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(ShaderMath.Dithering(
			ShaderMath.Ambient(voidT, xy, width, selectedAmbient, voidDepthMax)
			+ ShaderMath.Normalize(rgb, lightNormalizer), xy).XYZ, 1f);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbient(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer,
	ReadOnlyBuffer<I> voidT, int selectedAmbient, int voidDepthMax
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(
			(ShaderMath.Ambient(voidT, xy, width, selectedAmbient, voidDepthMax)
			+ ShaderMath.Normalize(rgb, lightNormalizer)).XYZ, 1f);
	}
}
#endregion

#region NoAmbient
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawSaturateDither(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer,
	Float3 saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(ShaderMath.Dithering(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(rgb, lightNormalizer), saturateAmount), xy).XYZ, 1f);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawSaturate(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer,
	Float3 saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY; var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(rgb, lightNormalizer), saturateAmount).XYZ, 1f);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawDithering(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY; var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(ShaderMath.Dithering(ShaderMath.Normalize(rgb, lightNormalizer), xy).XYZ, 1f);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct Draw(
	ReadOnlyBuffer<Float3> buffer, int width, ReadWriteTexture2D<Float4> output, Float3 lightNormalizer
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY; var rgb = buffer[xy.Y * width + xy.X];
		output[xy] = new Float4(ShaderMath.Normalize(rgb, lightNormalizer).XYZ, 1f);
	}
}
#endregion
*/

#region Void
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct VoidBfs(
	ReadWriteBuffer<I> input, 
	ReadWriteBuffer<I> output,
	ReadWriteBuffer<I> outMax, 
	int width
) : IComputeShader {
	public void Execute() {
		int x = ThreadIds.X + 1;
		int y = ThreadIds.Y + 1;
		int idx = y * width + x;
		int d = input[idx].V;
		if (d == 0) { output[idx].V = 0; return; }
		int best = d;
		best = Hlsl.Min(best, input[idx - 1].V + 1);
		best = Hlsl.Min(best, input[idx + 1].V + 1);
		best = Hlsl.Min(best, input[idx - width].V + 1);
		best = Hlsl.Min(best, input[idx + width].V + 1);
		output[idx].V = best;
		Hlsl.InterlockedMax(ref outMax[0].V, best);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct JumpFloodBoundlessOptimized(
	ReadWriteBuffer<I> input,
	ReadWriteBuffer<I> output,
	int width,
	int step
) : IComputeShader {
	public void Execute() {
		// this is optimized in reducing the number of step addition and subtractions, and branchless smaller kernel
		int mx = ThreadIds.X;
		int my = ThreadIds.Y;
		int x = mx + step;
		int y = my + step;
		int px = x + step;
		int py = y + step;
		int yw = y * width;
		int idx = yw + x;
		output[idx].V = Hlsl.Min(input[idx].V, Hlsl.Min(Hlsl.Min(input[yw + mx].V, input[yw + px].V), Hlsl.Min(input[my * width + x].V, input[py * width + x].V)) + step);
	}
}
/*[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct JumpFloodBoundlessReadable(
	ReadWriteBuffer<int> input,
	ReadWriteBuffer<int> output,
	int width,
	int step
) : IComputeShader {
	public void Execute() {
		// this is a more readable varaint of JumpFloodBoundlessOptimized, but does exactly the same thing
		int x = ThreadIds.X;
		int y = ThreadIds.Y;
		int ywidth = y * width;
		int idx = ywidth + x;
		int best = input[idx];
		best = Hlsl.Min(best, input[ywidth + (x - step)] + step);
		best = Hlsl.Min(best, input[ywidth + (x + step)] + step);
		best = Hlsl.Min(best, input[(y - step) * width + x] + step);
		best = Hlsl.Min(best, input[(y + step) * width + x] + step);
		output[idx] = best;
	}
}*/ // Readable version of Optimized
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct JumpFlood(
	ReadWriteBuffer<I> input,
	ReadWriteBuffer<I> output,
	int width,
	int height,
	int step
) : IComputeShader {
	public void Execute() {
		int x = ThreadIds.X + 1;
		int y = ThreadIds.Y + 1;
		int idx = y * width + x;
		int best = input[idx].V;
		int nx = x - step;
		if (nx >= 0)
			best = Hlsl.Min(best, input[y * width + nx].V + step);
		nx = x + step;
		if (nx < width) 
			best = Hlsl.Min(best, input[y * width + nx].V + step);
		int ny = y - step;
		if (ny >= 0)
			best = Hlsl.Min(best, input[ny * width + x].V + step);
		ny = y + step;
		if (ny < height)
			best = Hlsl.Min(best, input[ny * width + x].V + step);
		output[idx].V = best;
	}
}
#endregion