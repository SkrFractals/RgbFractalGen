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
	public static NoiseCoordData NoiseCoords(Int2 xy, Int2 noiseP) {
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
	public static Float3 Ambient(int voidA, float voidDepthMax, int selectedAmbient) {
		float voidAmb = selectedAmbient * voidA/ voidDepthMax;
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

#region Blur
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct HorizontalBlur(
	ReadWriteBuffer<Float3> input, ReadWriteBuffer<Float3> output, ReadOnlyBuffer<float> kernel, int width, int radius, int kernelWidth
) : IComputeShader {
	public void Execute() {
		/*var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = input[i];*/

		// kernelWidth = kernel.Length = must be 1 + 2*radius!
		// sum of kernel values must be 1!
		var xy = ThreadIds.XY;
		Float3 sum = 0f;
		// Start of this row
		int yw = xy.Y * width,
		// After the end of this row (will decrement back)
		fromRowEnd = yw + width,
		// how many pixels of the left radius overflow over left border?
		leftOver = Hlsl.Max(radius - xy.X, 0),
		// how many pixels of the right radius overflow over right border?
		rightOver = Hlsl.Max(xy.X + 1 + radius - width, 0),
		// how many pixels fit int the central kernel run (clamped from both overflow sides, the claped parts will attempt to get reflected)
		fit = kernelWidth - leftOver - rightOver,
		// how much would the reflected left overflow still overflow again over the right border? (this will really get clamped)
		leftStop = Hlsl.Max(0, leftOver - fit),
		// how much would the reflected right overflow still overflow again over the left border?
		rightStop = Hlsl.Max(0, rightOver - fit),
		// what input index will the central run start from?
		startCentral = yw + xy.X + leftOver - radius, // *1 right
		// kernel index, starts on clamp size of the left overflow.
		kerneli = leftStop;
		// reflected left overflow run, clamped by stopLeft if overflown again:
		// from the end of left mirrored kernel decrementing to left border
		for (int i = leftOver - leftStop; i > 0; sum += kernel[kerneli++] * input[--i + yw]) ;
		// central run:
		// from startcentral incrementing right to the right end of the central run
		for (int i = 0; i < fit; sum += kernel[kerneli++] * input[startCentral + i++]) ;
		// reflected right overflow run, clamped by stopRight if overflown again:
		// from the right border, decrement left to the end of right mirrored kernel
		for (int totalRightReflect = rightOver - rightStop, i = 0; i < totalRightReflect; sum += kernel[kerneli++] * input[fromRowEnd - ++i]);
		// put the sum of the kerneled runs into output:
		output[yw + xy.X] = sum;
	} 
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct VerticalBlur(
	ReadWriteBuffer<Float3> input, ReadWriteBuffer<Float3> output, ReadOnlyBuffer<float> kernel, int width, int height, int radius, int kernelWidth
) : IComputeShader {
	public void Execute() {
		/*var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = input[i];*/
		
		// kernelWidth = kernel.Length = must be 1 + 2*radius!
		// sum of kernel values must be 1!
		var xy = ThreadIds.XY;
		Float3 sum = 0f;
		// Start of this row
		int yw = xy.Y * width,
		// Below the end of this collumn (will row-decrement back)
		fromColEnd = width * height + xy.X,
		// how many pixels of the up radius overflow over up border?
		upOver = Hlsl.Max(radius - xy.Y, 0),
		// how many pixels of the down radius overflow over down border?
		downOver = Hlsl.Max(xy.Y + 1 + radius - height, 0),
		// how many pixels fit int the central kernel run (clamped from both overflow sides, the claped parts will attempt to get reflected)
		fit = kernelWidth - upOver - downOver,
		// how much would the reflected up overflow still overflow again over the down border? (this will really get clamped)
		upStop = Hlsl.Max(0, upOver - fit),
		// how much would the reflected down overflow still overflow again over the up border?
		downStop = Hlsl.Max(0, downOver - fit),
		// what input index will the central run start from?
		startCentral = yw + xy.X + (upOver - radius) * width, // *width down
		// kernel index, starts on clamp size of the up overflow.
		kerneli = upStop;
		// reflected up overflow run, clamped by stopLeft if overflown again:
		// from the end of upper mirrored kernel row-decrementing to up border
		for (int i = upOver - upStop; i > 0; sum += kernel[kerneli++] * input[xy.X + width * --i]) ; 
		// central run:
		// from startcentral row-incrementing down to the bottom end of the central run
		for (int i = 0; i < fit; sum += kernel[kerneli++] * input[startCentral + width * i++]) ;
		// reflected down overflow run, clamped by stopRight if overflown again:
		// from the bottom border, row-decrement up to the end of bottom mirrored kernel
		for (int totalBottomReflect = downOver - downStop, i = 0; i < totalBottomReflect; sum += kernel[kerneli++] * input[fromColEnd - width * ++i]) ; 
		// put the sum of the kerneled runs into output:
		output[yw + xy.X] = sum;
	}
}
#endregion

#region Draw_Pipeline
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeNormalizeSaturate(
	ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		buffer[i] = ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeNormalize(
	ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		buffer[i] = ShaderMath.Normalize(buffer[i], lightNormalizer);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeNoise(
	ReadWriteBuffer<Float3> ping, int width,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, noiseP);
		ping[i] = ping[i] + ShaderMath.Noise(voidT[i] / voidDepthMax, selectedAmbient, nc.b, noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W]);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeAmbient(
	ReadWriteBuffer<Float3> ping, int width,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		ping[i] = ping[i] + ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient);
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeBytesDither(
	ReadWriteBuffer<Float3> ping, ReadWriteBuffer<int> output, int width
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(ping[i], xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PipeBytes(
	ReadWriteBuffer<Float3> ping, ReadWriteBuffer<int> output, int width
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ping[i]);
	}
}
#endregion

#region Noise
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoiseSaturateDither(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, noiseP);
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount)
			+ ShaderMath.Noise(voidT[i] / voidDepthMax, selectedAmbient, nc.b,
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W]), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoiseSaturate(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, noiseP);
		output[i] = ShaderMath.ToBytes(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount)
			+ ShaderMath.Noise(voidT[i] / voidDepthMax, selectedAmbient, nc.b,
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W]));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoiseDither(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, noiseP);
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.Normalize(buffer[i], lightNormalizer)
			+ ShaderMath.Noise(voidT[i] / voidDepthMax, selectedAmbient, nc.b,
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W]), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawNoise(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax,
	ReadOnlyBuffer<Float3> noise, Int2 noiseP
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		var nc = ShaderMath.NoiseCoords(xy, noiseP);
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Normalize(buffer[i], lightNormalizer)
			+ ShaderMath.Noise(voidT[i] / voidDepthMax, selectedAmbient, nc.b,
				noise[nc.i.X], noise[nc.i.Y], noise[nc.i.Z], noise[nc.i.W]));
	}
}
#endregion

#region Ambient
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientSaturateDither(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount)
			+ ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientSaturate(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax,
	float saturateAmount
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(
			ShaderMath.ApplySaturate(ShaderMath.Normalize(buffer[i], lightNormalizer), saturateAmount)
			+ ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbientDither(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(ShaderMath.Dithering(
			ShaderMath.Normalize(buffer[i], lightNormalizer)
			+ ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawAmbient(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
	ReadWriteBuffer<int> voidT, int selectedAmbient, float voidDepthMax
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Normalize(buffer[i], lightNormalizer)
			+ ShaderMath.Ambient(voidT[i], voidDepthMax, selectedAmbient));
	}
}
#endregion

#region NoVoid
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DrawSaturateDither(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
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
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer,
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
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Dithering(ShaderMath.Normalize(buffer[i], lightNormalizer), xy));
	}
}
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct Draw(
	ReadWriteBuffer<int> output, ReadWriteBuffer<Float3> buffer, int width, float lightNormalizer
) : IComputeShader {
	public void Execute() {
		var xy = ThreadIds.XY;
		var i = xy.Y * width + xy.X;
		output[i] = ShaderMath.ToBytes(
			ShaderMath.Normalize(buffer[i], lightNormalizer));
	}
}
#endregion

#region Void
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct VoidBfs(
	ReadWriteBuffer<int> input,
	ReadWriteBuffer<int> output,
	ReadWriteBuffer<int> outMax,
	int width
) : IComputeShader {
	public void Execute() {
		int x = ThreadIds.X + 1;
		int y = ThreadIds.Y + 1;
		int idx = y * width + x;
		int d = input[idx];
		if (d == 0) { output[idx] = 0; return; }
		int best = d;
		best = Hlsl.Min(best, input[idx - 1] + 1);
		best = Hlsl.Min(best, input[idx + 1] + 1);
		best = Hlsl.Min(best, input[idx - width] + 1);
		best = Hlsl.Min(best, input[idx + width] + 1);
		output[idx] = best;
		Hlsl.InterlockedMax(ref outMax[0], best);
	}
}

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
[ThreadGroupSize(16, 16, 1)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct JumpFloodBoundlessOptimized(
	ReadWriteBuffer<int> input,
	ReadWriteBuffer<int> output,
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
		output[idx] = Hlsl.Min(input[idx], Hlsl.Min(Hlsl.Min(input[yw + mx], input[yw + px]), Hlsl.Min(input[my * width + x], input[py * width + x])) + step);
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
	ReadWriteBuffer<int> input,
	ReadWriteBuffer<int> output,
	int width,
	int height,
	int step
) : IComputeShader {
	public void Execute() {
		int x = ThreadIds.X + 1;
		int y = ThreadIds.Y + 1;
		int idx = y * width + x;
		int best = input[idx];
		int nx = x - step;
		if (nx >= 0)
			best = Hlsl.Min(best, input[y * width + nx] + step);
		nx = x + step;
		if (nx < width)
			best = Hlsl.Min(best, input[y * width + nx] + step);
		int ny = y - step;
		if (ny >= 0)
			best = Hlsl.Min(best, input[ny * width + x] + step);
		ny = y + step;
		if (ny < height)
			best = Hlsl.Min(best, input[ny * width + x] + step);
		output[idx] = best;
	}
}
#endregion