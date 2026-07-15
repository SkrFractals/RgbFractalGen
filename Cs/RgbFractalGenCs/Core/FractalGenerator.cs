// The defines below are for debugging, they should not be enabled to building the final product!

#define CustomDebug // Will enable complex performance logging measuring the runtimes of all the processed, will slightly decrease the performance

//#define SmoothnessDebugXy // Will disable the pre-split lerping toward the parent's XY

//#define SmoothnessDebugDetail // Will make the detail parameter huge to separate the deep dots to see how they are splitting themselves 

using ComputeSharp;
using Gif.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs.Core;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal partial class FractalGenerator {

	#region Enums
	internal enum ScheduledTask : byte {
		Png = 0,        // (Not a task) Export preview image as PNG
		Pngs = 1,       // Export PNG series (lossless animation quality)
		PngsToMp4 = 2,  // Convert PNG series into MP4
		Gif = 3,        // Export GIF
		GifToMp4 = 4,   // Convert GIF into MP4
		ImportCode = 5, // (Not a task) Import an ImportCode
		Close = 6,      // Close the generator
		None = 7        // Nothing
	}
	public enum ParallelType : byte {
		OfAnimation = 0,// Parallel batching of each animation image into its own single thread. No pixel conflicts, and more likely to use all the threads consistently. Recommended for animations.
		OfDepth = 1     // Parallelism of each single image, generates the single next image faster, but there might be rare conflicts of accessing pixels, and it's a bit slower overall if generating an animation. Used automatically for OnlyImage.
	}
	public enum GenerationType : byte {
		OnlyImage = 0,  // Will only render one still image. Cannot export PNG animation, or GIF animation, only a single image, the first one from a zoom animation.
		Animation = 1,  // Will render the animation as set up by your settings.
		AllSeeds = 2,   // Will not zoom/spin/shift at all, but instead cycle through all the available CutFunction seeds.
		HashSeeds = 3   // Same as All Seeds, but will also export a file with all detected unique seeds. (Only really useful for the developer, to hide the repeating seeds)
	}
	public enum PngType : byte {
		No = 0,			// Will not encode PNGs durign the generation, but will encode them just as fast if not faster when you do an export
		Yes = 1			// Will export animation as PNG series, that will enable you to export the PNGS or MP4 quicker after it's finished, but not really quicker overall.
	}
	public enum GifType : byte {
		No = 0,			// Will not encode a gif, you will not be able to export a gif afterwards, but you can switch to a Local/Global later and it will not fully reset the generator, just catches up with missing frames.
		Local = 1,		// Will encode a GIF during the generation, so you could save it when finished. With a local colormap (one for each frame). Higher color precision. Slow encoding, larger GIF file size. 
		Global = 2		// Also encodes a GIF. But with a global color map learned from a first frame. Not recommended for changing colors. Much faster encoding, smaller GIF file size
	}
	public enum GpuVoidType : byte {
		CPU = 0,				// Original CPU Dijkstra BFS
		VoidBFS = 1,			// GPU Distance flooding passes
		VoidJumpFlood = 2,		// GPU Jump Flooding Passes
		VoidJumpBoundless = 3   // GPU branchless/boundless Jump Flooding Passes
	}
	public enum GpuDrawType : byte {
		CPU = 0,		// Original CPU Drawing
		Functions = 1,	// GPU shader variant switching
		Pipeline = 2	// GPU multi-pass pipeline
	}
	private enum BitmapState : byte {
		Queued = 0,             // Hasn't even started generating the dots yet (and it might stay this way if OnlyImage)
		Dots = 1,               // Started Generating Dots
		Void = 2,               // Started Dijkstra of the Void
		Drawing = 3,            // Started drawing
		DrawingFinished = 4,    // Finished drawing
		Unlocked = 5,           // Unlocked bitmap without encoding
		Error = 6               // Unused, unless error obviously
	}
	private enum TaskState : byte {
		Free = 0,       // The task has not been started yet, or already finished and joined and ready to be started again
		Done = 1,       // The task if finished and ready to join without waiting
		Running = 2     // The task is running
	}
	#endregion

	#region Structs
	private class FractalTask {
		// evey first SingleDots call should set this (typically this.Buffer, but for MultiBuffer OfDepth, it can be redirected to generator.buffer[taskIndex]):
		internal Vector3[] UseBuffer = [];	// Reference which buffer to apply dots to

		private Task
#if NULLABLE
		?
#endif
			task;					// Parallel Animation Tasks
		internal TaskState State;			// States of Animation Tasks
		internal Vector3[] Buffer = [];		// Buffer for points to print into bmp
		internal float[] Kernel = [];
		internal int[] VoidDepth = [];      // Depths of Void
		internal int[] VoidDepthN = [];     // Depths of Void
		internal Vector3[]
#if NULLABLE
		?
#endif
			VoidNoise = null;	// Randomized noise samples for the void noise
		internal Float3[]
#if NULLABLE
		?
#endif
			VoidNoiseF = null;	// Randomized noise samples for the void nois
		internal int[] VoidQueue = [];		// Void depth calculating Dijkstra queue
		internal Dictionary<long, Vector3[]>
			ColorBlends = [];				// ???
		internal readonly Dictionary<long, Vector3[]>
			FinalColors = [];				// Mixed children color
		internal int BitmapIndex;			// The bitmapIndex of which the task is working on
		internal short TaskIndex,			// The task index
			ApplyWidth, ApplyHeight,		// can be smaller for previews
			WidthBorder, HeightBorder;		// slightly below the width and height
		internal float Bloom0, Bloom1;		// = selectBloom (0,+1);
		internal double
		RightEnd, DownEnd,					// slightly beyond width and depth, to ensure even bloomed pixels don't cut off too early
		ApplyDetail,						// allocated detail
		UpLeftStart;						// = -selectBloom;
		internal float LightNormalizer,		// maximum brightness found in the buffer, for normalizing the final image brightness
			VoidDepthMax;					// maximum reached void depth during the dijkstra search, for normalizing the void intensity
		internal (double, double, (double, double)[])[]
			PreIterate = [];				// (childSize, childDetail, childSpread, (childX,childY)[])


		internal ushort StripeHeight, StripeCount;
		internal uint PredictedBinsPerStripe;
		private List<(long, float, float, byte)[]>[]
#if NULLABLE
		?
#endif
			bin = null;
		private ushort[] filledBins = [];	// Which bidId is being filled in this stripeId?
		private uint[] filledStripes = [];	// How many dots are in the latest bin of this stripeId?
		internal uint BinSize;

		internal FractalTask() { }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool IsStillRunning() {
			return State == TaskState.Done ? Join() : State != TaskState.Free;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool Join() {
			Stop(); // Stop the thread
			State = TaskState.Free; // Mark it as free to be started again
			return false;//taskStarted = false; // (used this to be doubly sure when I had bugs in the control, but probably redundant at this point)
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Start(int bitmap, Action action) {
			Stop();
			//taskStarted = true;
			BitmapIndex = bitmap;
			State = TaskState.Running;
			task = Task.Run(action);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Stop() {
			if (task == null)
				return;
			task.Wait();
			task = null;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AddDot((long dotsKey, float dotsX, float dotsY, byte color) dot) {
			if (bin == null)
				throw new("bin NULL");
			ushort s = (ushort)(dot.dotsY / StripeHeight); // find stripeId of this dot
			if (filledStripes[s] >= BinSize) {
				filledStripes[s] = 0; // reset to fill the new bin from zero index again
				bin[s].Add(new (long, float, float, byte)[BinSize]); // increment binId (start new bin in this stripe)
				++filledBins[s];
			}
			bin[s][filledBins[s]][filledStripes[s]++] = dot;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void ApplyDot((long dotsKey, float dotsX, float dotsY, byte dotsColor) dot) {
			var xKernel = Kernel;
			var dotColor = FinalColors[dot.dotsKey][dot.dotsColor];
			float bloom0 = Bloom0, bloom1 = Bloom1;
			int endX = Math.Min(WidthBorder, (int)Math.Ceiling(dot.dotsX + bloom0)),
				endY = Math.Min(HeightBorder, (int)Math.Ceiling(dot.dotsY + bloom0)),
				startX = Math.Max(1, (int)Math.Floor(dot.dotsX - bloom0));

			// Precalculate bloom width kernel
			float dotsK = dot.dotsX - startX;
			for (int ix = endX - startX; ix >= 0; --ix)
				xKernel[ix] = bloom1 - Math.Abs(ix - dotsK);

			// This unsafe code might be a bit faster?
			unsafe {
				for (int y = Math.Max(1, (int)Math.Floor(dot.dotsY - bloom0)); y <= endY; ++y) {
					float yd = bloom1 - Math.Abs((float)(y - dot.dotsY));
					//var row = UseBuffer[y];
					fixed (Vector3* rowPtr = &UseBuffer[y * ApplyWidth])
					fixed (float* kernPtr = &xKernel[0]) {
						float* kx = kernPtr;
						Vector3* px = rowPtr + startX, e = rowPtr + endX;
						while (px <= e) {
							float w = yd * *kx++;
							*px++ += w * dotColor;
						}
					}
				}
			}

			/*for (var y = Math.Max(1, (int)Math.Floor(dotsY - bloom0)); y <= endY; ++y) {
				// gradient from 0 to max to 0 over the y range
				var yd = bloom1 - Math.Abs(y - dotsY);// Math.Max(0, task.bloom1 - Math.Abs(y - inY));
				var buffY = dotsBuffT[y];
				for (int ix = 0, x = startX; x <= endX; ++x, ++ix)
					// combine with gradient from 0 to max to 0 over the x range, apply with the computed color
					buffY[x] += (yd * xKernel[ix]) * dotColor;
			}*/

			// SIMD vector width (platform-dependent)
			/*int V = Vector<float>.Count;
			unsafe {
				// For each scanline
				for (int y = Math.Max(1, (int)Math.Floor(dotsY - bloom0)); y <= endY; ++y) {
					// scalar row weight (float)
					float yd = bloom1 - Math.Abs((float)(y - dotsY));
					if (yd <= 0f) continue; // optional if you know ranges are tight

					var row = dotsBuffT[y]; // Vector3[] row

					// pointers to row and kernel base
					fixed (Vector3* rowPtr = row)
					fixed (float* kernelBase = &xKernel[0]) // kernelBase[0] corresponds to startX
					{
						// pointer to first pixel to write
						Vector3* px = rowPtr + startX;
						// number of pixels in row to update
						int count = endX - startX + 1;

						// vectorize kernel * yd
						var ydVec = new Vector<float>(yd);

						int i = 0;
						// process blocks of V lanes
						for (; i <= count - V; i += V) {
							// load kernel[V] into a Vector<float> using managed array ctor
							// note: we index into managed array; the ctor copies contents into the vector
							var kVec = Unsafe.ReadUnaligned<Vector<float>>((byte*)(kernelBase + i));
							var wVec = kVec * ydVec;

							// extract lanes and apply per-pixel (we still do lane loop to update interleaved Vector3)
							// unroll manually for small V (optional)
							for (int lane = 0; lane < V; ++lane) {
								float w = wVec[lane];
								// perform fused multiply-add per component
								// px is current base + i + lane
								Vector3* p = px + i + lane;
								p->X += dotColor.X * w;
								p->Y += dotColor.Y * w;
								p->Z += dotColor.Z * w;
							}
						}

						// tail for remaining pixels
						for (; i < count; ++i) {
							float w = yd * xKernel[i];
							Vector3* p = px + i;
							p->X += dotColor.X * w;
							p->Y += dotColor.Y * w;
							p->Z += dotColor.Z * w;
						}
					} // fixed
				} // for y
			}*/
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void DrawStripesOfAnimation() {
			if (bin == null)
				throw new("bin NULL");
			for (int s = 0; s < StripeCount; ++s) { // draw each stripe sequentially
				int b = 0; // binId from 0, do it as while instead of for, so that we can keep the ending binId to draw the last unfilled bin
				var bins = bin[s];
				while (b < filledBins[s]) { // for each stripe, draw each bin sequentially
					var binsb = bins[b];
					for (int d = 0; d < BinSize; ++d) { // for each bin, draw all dots sequentially
						ApplyDot(binsb[d]); // draw this whole filled bin
					}
					++b; // next bin
				}
				var binsl = bins[b];
				for (int d = (int)filledStripes[s]; --d >= 0; ApplyDot(binsl[d])) { } // for each bin, draw all dots sequentially
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void DrawStripeOfDepth(FractalTask[] tasks, FractalTask dotsTask, ushort stripeId) {
			foreach (var task in tasks) { // draw each thread's same stripe bins sequentially
				if (task.bin == null)
					continue;
				var bins = task.bin[stripeId];
				//if (bins == null)
				//	continue;
				int b = 0; // binId from 0, do it as while instead of for, so that we can keep the ending binId to draw the last unfilled bin
				if (task.filledBins == null || task.filledStripes == null)
					throw new("filledBins/Stripes NULL");
				while (b < task.filledBins[stripeId]) { // for each stripe, draw each bin sequentially
					var binsb = bins[b];
					for (int d = 0; d < task.BinSize; ++d) { // for each bin, draw all dots sequentially
						dotsTask.ApplyDot(binsb[d]); // draw this whole filled bin
					}
					++b; // next bin
				}
				var binsl = bins[b];
				for (int d = (int)task.filledStripes[stripeId]; --d >= 0; dotsTask.ApplyDot(binsl[d])) { } // for each bin, draw all dots sequentially
			}
		}
		internal void NewBin(FractalTask task) {
			if (bin != null)
				return; // already defined (OfDepth reuse)
			StripeHeight = task.StripeHeight;
			BinSize = task.BinSize;
			bin = new List<(long, float, float, byte)[]>[StripeCount = task.StripeCount]; // allocate stripes of bins
			for (int i = 0; i < task.StripeCount; ++i) 
				(bin[i] = new List<(long, float, float, byte)[]>((int)PredictedBinsPerStripe)).Add(new (long, float, float, byte)[BinSize]);
			//foreach (ref var dbin in bin) // make a fresh list with a fresh bin for each stripe
			//	(dbin = new (long, float, float, byte)[PredictedBinsPerStripe][])[0] = new (long, float, float, byte)[task.BinSize];
			filledBins = new ushort[task.StripeCount];
			filledStripes = new uint[task.StripeCount];
			UseBuffer = task.Buffer;
		}
		internal void ClearBin() {
			bin = null;
		}
		internal string ChooseStripeAndBin(
			int MaxTasks,
			long L2bytes,           // per-thread L2
			out ushort StripeCount,
			out uint BinSize) {
			ushort bloomDiameter = (ushort)Math.Ceiling(2 * Bloom1 + 2);

			//Bloom limits:
			// bloomDiameter * BmpWidth * OnePixelByte < L2bytes // If bigger than this it would overflow cache even with a single ApplyDot
			// BmpHeight / bloomDiameter < .5f * MaxTasks // If bigger than this then we can't nicely alternate without racing (but this only applies to OfDepth)
			// Bloom1 < 1 // If out maximum bloom isn't even just 1 pixel, then we simply cannot optimize caching and could as well just use my original direct approach
			// Minimum StripeCount, where no slow down happens: MaxTasks * 5; // MaxTasks * 4 might just start to touch racing conditions, MaxTasks * 2 it might be 2x slower, MaxTasks *1 4x slower...
			// Phase 1 Cache Bytes = binBytes * StripeCount + ExtraCachePhase1; 
			// in phase 1, it can access 1 Dot Bin per every stripe
			// Phase 2 Cache Bytes = BufferLineBytes * (bloomDiameter + ApplyHeight / testStripeCount) + binBytes + ExtraCachePhase2; 
			// in Phase 2, it can access 1 Dot Bin + 1 horizonal stripe of Vector3 pixel buffer that can span from top of the stripe (- bloom radius) to the bottom of the stripe (+ bloom radius)
			// Must find some nice balance

			// safety margin of L2 cache size in bytes:
			float L2_eff = L2bytes * 0.85f; 
			// BinSize can't be way too small, or else it would explode
			ushort MinBinBytes = 4096;
			byte OnePixelBytes = 12; // Vector3 is 12 bytes
			int BufferLineBytes = OnePixelBytes * ApplyWidth;
			//int BufferTotalBytes = ApplyHeight * BufferLineBytes;
			// we should have at least enough stripes for tasks to fill them in parallel
			// also enough that drawing bloom radius scircles into the stripe woudln't overflow the L2 cache
			ushort minStripes = StripeCount = (ushort)Math.Max(ApplyHeight / (L2_eff / BufferLineBytes - bloomDiameter - 1), MaxTasks * 5);
			BinSize = 1024;
			if (bloomDiameter > L2_eff / (ApplyWidth * OnePixelBytes))
				return L("bloomLarge");
			if (bloomDiameter > ApplyHeight / (3.0f * MaxTasks))
				return L("bloomLarge2");

			// doesn't make sense for the stripes to be less than 1 pixel tall
			ushort maxStripes = (ushort)ApplyHeight;
			if (maxStripes < minStripes)
				return L("noStripeRange");

			float bestBalance = float.MaxValue;
			bool found = false;

			for (ushort testStripeCount = minStripes; testStripeCount <= maxStripes; testStripeCount++) {
				// How many pixel buffer lines can be accessed while drawing one stripe = size of the stripe + bloom diameter
				float bufferBytes = BufferLineBytes * (bloomDiameter + (float)Math.Ceiling((float)ApplyHeight / testStripeCount));
				// Phase1 = BinSize * StripeCount <= L2_eff
				// Phase2 = bufferSize + BinSize <= L2_eff
				float binBytes = Math.Min(L2_eff / testStripeCount, L2_eff - bufferBytes / testStripeCount);

				if (binBytes < MinBinBytes)
					continue; // too small to be safe

				float phase1 = binBytes * testStripeCount;
				float phase2 = bufferBytes + binBytes;

				if (phase1 > L2_eff || phase2 > L2_eff)
					continue; // overflow of cache in one of the phases, skip

				float balance = Math.Abs(phase1 - phase2);
				if (balance < bestBalance) {
					bestBalance = balance;
					StripeCount = testStripeCount;
					BinSize = Math.Max(1, (uint)(binBytes / (sizeof(float) * 2 + sizeof(long) + sizeof(byte))));
					found = true;
				}
			}

			return found == true ? "" : L("notFoundAnything");
		}
		internal interface IDotApplier {
			public void Apply(FractalTask task, (long dotsKey, float dotsX, float dotsY, byte dotsColor) dot);
		}
		internal struct ApplyDotImpl : IDotApplier {
			public readonly void Apply(FractalTask task, (long dotsKey, float dotsX, float dotsY, byte dotsColor) dot) => task.ApplyDot(dot);
		}
		internal struct AddDotImpl : IDotApplier {
			public readonly void Apply(FractalTask task, (long dotsKey, float dotsX, float dotsY, byte dotsColor) dot) => task.AddDot(dot);
		}

	}
	#endregion

	#region Constants
	public const int MinTasks = 1, MaxPngFails = 200;
	private const int DepthDiv = 6;               // multiples of maxThreads to queue for OfDepth parallelism
	#endregion

	#region Variables_Definitions
	internal double[]					// A copy of selected childAngle combinations
		ChildAngle = new double[MaxChildren];                
	internal byte[]                     // A copy of childColor for allowing BGR and combinations
		ChildColor = new byte[MaxChildren];	
	internal readonly Dictionary<string, uint>
		Hash = [];						// Seed hashes

	private Fractal f = Fractals[0];	// Selected fractal definition
	private readonly Dictionary<long, Vector3[]> 
		colorBlends = [];				// What mix of colors will a parent split into?
	#endregion

	#region Variables_Selected
	internal double
		SelectedDefaultHue = 0,         // Default hue angle (-1 - Colors.Length-1)
		SelectedDetail,                 // MinSize multiplier (1-10)
		SelectedNoise;                  // Void noise strength (0-3)
	internal float
		SelectedSaturate,               // Saturation boost level (0-1)
		SelectedBloom;                  // Dot bloom level (pixels)
	internal ulong
		SelectedChildAngles,            // Child angle bitmask
		SelectedChildColors;            // Child color bitmask
	internal int
		SelectedCutSeed = 0,            // CutSeed seed (0-CutSeed_Max)
		CutSeed_Max;                    // Maximum seed for the selected CutFunction
	internal short
		SelectedFractal = -1,           // Fractal definition (0-fractals.Length)
		SelectedChildAngle = -1,        // Child angle definition (0-childAngle.Length)
		SelectedChildColor = -1,        // Child color definition (0-childColor.Length)
		SelectedCut = -1,               // Selected CutFunction index (0-cutFunction.Length)
		SelectedPaletteType = 0,        // Color Palette (-1 - Colors.Length-1)
		SelectedZoom = 1,               // Zoom direction (-1 out, 0 random, 1 in)
		SelectedHue = -1,               // -1 = random, 0 = RGB, 1 = BGR, 2 = RGB->GBR, 3 = BGR->RBG, 4 = RGB->BRG, 5 = BGR->GRB
		SelectedSpin = -1,              // Spin direction (-2 Random, -1 AntiClockwise, 0 None, 1 Clockwise, 2 AntiSpin, 3 PeriAntiSpin)
		SelectedAmbient,                // Void ambient strength (-1 - MaxAmbient)
		SelectedMaxTasks;               // Maximum allowed total tasks
	internal ushort
		SelectedL2Kilobytes = 0,		// Override L2 cache size (0 = automatic guess)
		SelectedBinSize = 0,			// Override automatic BinSize (0 = automatic)
		SelectedStripeHeight = 0,		// Override automatic StripeHeight (0 = automatic) 
		SelectedMaxPreviewFrames = 16,	// How many preview frames are allowed? (0 to disable preview)
		SelectedLessPreviewFrames = 0,	// How many highest resolution preview frames get skipped?
		SelectedWidth,                  // Resolution width (1-X)
		SelectedHeight,                 // Resolution height (1-X)
		SelectedPeriod,                 // Parent to child frames period (1-X)
		SelectedPeriodMultiplier,       // Multiplier of frames period (1-X)
		SelectedDefaultZoom = 0,        // Default skipped frames of zoom (0-frames)
		SelectedExtraHue = 0,           // Extra hue angle speed (0-X)
		SelectedDefaultAngle = 0,       // Default spin angle (0-360)
		SelectedExtraSpin = 0,          // Extra spin speed (0-X)
		SelectedVoid,                   // Void Noise Size
		SelectedBrightness,             // Light normalizer brightness (0-MaxBrightness)
		SelectedBlur,                   // Dot blur level
		MaxZoomChild,                   // Maximum allowed zoom child
		SelectedDelay,                  // Animation frame delay
		SelectedFps = 1;                // Animation framerate
	internal ParallelType               // 0 = Animation, 1 = Depth
		SelectedParallelType = ParallelType.OfAnimation;          
	internal GenerationType             // 0 = Only Image, 1 = Animation, 2 = AllSeeds, 3 = HashSeeds
		SelectedGenerationType = GenerationType.Animation;         
	internal PngType
		SelectedPngType = PngType.No;   // 0 = No, 1 = Yes
	internal GifType
		SelectedGifType = GifType.No;   // 0 = No, 1 = Local, 2 = Global
	internal GpuVoidType
		SelectedGpuVoidType = GpuVoidType.CPU;
	internal GpuDrawType
		SelectedGpuDrawType = GpuDrawType.Functions;
	internal bool
		SelectedCacheOpt = false,		// Will we use the new cacche optimization algorithm?
		SelectedPreviewMode = false,    // Preview mode only renders a single smaller fractal with only one color shift at the highest level - for definition editor
		SelectedEditorMode = false;     // In editor mode, we will only show the selected childColors and childAngles

	private List<ushort> validZoomChildren = [];
	private Fractal.CutFunction
#if NULLABLE
?
#endif
		selectedCutFunction = null;            // Selected CutFunction pointer
	private ushort selectedZoomChild;   // Which child to zoom into
	private short maxIterations = -1;   // maximum depth of iteration (dependent on fractal/detail/resolution)
	internal bool Working = false;
#endregion

	#region Variables_Allocated
	// "alloc" are the selected settings below get copied here when the generator restarts
	// to ensure it doesn't crash because the settings changed mid-generation
	private Vector3[][]
#if NULLABLE
		?
#endif
		buffer;                 // Buffer for points to print into bmp - separated for OfDepth
	private float AverageChildCount;
	private FractalTask[]
		tasks = [];				// All available tasks
	private Bitmap[] 
		bitmap = [];			// Prerender as an array of bitmaps
	private BitmapState[] 
		bitmapState = [];       // What stage is the bitmap in? Created? Drawing? Exporting?
	private BitmapData[] 
		bitmapData = [];        // Locked Bits for bitmaps
	private bool[]
		lockedBmp = [];         // Is this bitmap locked?
	private Vector3[]
		allocPalette = [];
	private readonly Random
		random = new();         // Random generator
	private double
		logBase,                // Size Log Base
		allocPeriodAngle,       // Angle symmetry corrected for periodMultiplier
		allocDetail;       
	private long
		allocCutSeed,           // Allocated cutSeed (selected or random)
		startCutSeed = 0;       // What cut seed will the generator start with?
	private int
		allocFrames = -1,       // How many bitmap frames are currently allocated?
		nextBitmap,             // How many bitmaps have started generating? (next task should begin with this one)
		allocNoise;				// Normalizer for maximum void depth - Precomputed amb * noise
	private uint
		allocBinSize,           // Cache Bin Size
		bitmapsFinished;        // How many bitmaps are completely finished generating? (ready to display, encoded if possible)
	private short
		allocWidth = -1,        // How much buffer width is currently allocated?
		allocHeight = -1,       // How much buffer height is currently allocated?
		allocZoom = 0,          // Allocated zoom (selected or random)
		allocHue = 0,           // Allocated hue setting
		allocTasks = -1,        // How many buffer tasks are currently allocated?
		allocMaxIterations = -1,
		allocMaxTasks = -1,		// Safely copy maxTasks in here, so it doesn't change in the middle of generation
		isWritingBitmaps = 2,   // counter and lock to try writing bitmap to a file once every 2 threads
		isFinishingBitmaps = 2; // counter and lock to try finishing bitmaps once every 2 threads
	private ushort
		allocStripeHeight,      // Full resolution Cache Stripe Height
		allocStripeCount,		// Full resolution Cache Stripe Height
		allocLessPreviewFrames, // How many highest resolution preview frames get skipped?
		allocPeriodMultiplier,  // How much will the period get finally stretched? (calculated for seamless + user multiplier)
		allocExtraSpin,         // Allocated extra spin setting
		allocVoid,              // Allocated void scale setting
		noiseWidth,
		noiseHeight,
		allocBlur,              // Allocated blur
		allocDelay,
		debugPeriodOverride = 0;    // Debug frame count override
	private byte
		allocPalette2,          // allocPalette.Length * 2
		previewFrames,          // how many preview frames are we making for this resolution?
		hueCycleMultiplier;     // How fast should the hue shift to loop seamlessly?
	private bool
		allocDithering,
		allocCacheOpt,			// Will we use the new cacche optimization algorithm?
		allocPreviewMode;		// Will render a simpliefied fractal colored 1 layer deep definition showing it's structure
	private GenerationType
		allocGenerationType;
	private ParallelType
		allocParallelType;          // Safely copy parallelType in here, so it doesn't change in the middle of generation
	private PngType
		allocPngType;
	private GifType
		allocGifType;
	private GpuVoidType
		allocGpuVoidType;
	private GpuDrawType
		allocGpuDrawType;
	private (short, (double, double), (double, double), byte, long, byte)[]
		tuples = [];                // Queue struct for GenerateDots_OfDepth ((x,y), angle, aa, color, -cutSeed, depth);
									//private Queue<(short, (double, double), (double, double), short, long, byte)>
									//	ofDepthQueue = new(), ofDepth00 = new(), ofDepth01 = new(), ofDepth10 = new(), ofDepth11 = new(); 
									// these were helper buffers for experimental OfDepth implementations that didn't turn out faster than the current one
#endregion

	#region Variables_Export
	private readonly uint Index;
	private AnimatedGifEncoder
#if NULLABLE
		?
#endif
		gifEncoder = null;         // Export GIF encoder
	private MemoryStream
#if NULLABLE
		?
#endif
		[]
		msPngs = [];				// MemoryStreams for Png Exporting
	private short 
		gifSuccess,					// Temp GIF file "gif.tmp" successfully created
		tryPng;						// how many sequential Pngs have been exported?
	internal string
		GifTempPath = "";           // Temporary GIF file name
	private byte[]
		encodedGif = [],			// 0 - not encoded, 1 - encoding, 2 - encoded, 3 - written
		encodedPng = [];			// 0 - not encoded, 1 - encoding, 2 - encoded
	private int 
		tryGif;                     // how many sequential gif frames have been exported
	private ushort
		encodedMp4,					// report of how many mp4 frames have been encoded
		pngFailed;					// how many attempts to write a png have failed?
	private Rectangle
		rect;						// Bitmap rectangle
	private readonly string         // Specific file identifier
		filePrefix = "guid" + Guid.NewGuid().ToString("N");                
	private ScheduledTask 
		exportType = ScheduledTask.None;

	#endregion

	#region Variables_Debug
	internal bool DebugTasks = false;   // export task states to realtime debug string
	internal bool DebugAnim = false;    // export bitmap states to realtime debug string
	internal bool DebugPng = false;     // export png states to realtime debug string
	internal bool DebugGif = false;     // export gif states to realtime debug string
	internal string DebugString = "";	// realtime debug string

	private readonly short[] 
		counter = new short[7];			// counter of bitmap states
	private readonly short[] 
		counterE = new short[4];        // counter of encode states

#if CustomDebug
	//private double initTimes, iterTimes, voidTimes, drawTimes, gifsTimes;
	// Debug variables
	private string logString = "";      // Debug log
	private Stopwatch startTime = new();// Start Stopwatch
#endif

	#endregion

	#region Variables_Calls
	private readonly FractalTask.ApplyDotImpl applyDot = new();
	private readonly FractalTask.AddDotImpl addDot = new();

	internal event Action
#if NULLABLE
?
#endif
		UpdatePreview = null,
		UpdateCache = null;

	private readonly object
		taskLock = new();			// Monitor lock
	private CancellationTokenSource
		cancel = new();             // Cancellation Token Source
	private CancellationToken
		token;                      // Cancellation token
	private CancellationTokenSource
#if NULLABLE
		?
#endif
		gifCancel = new();          // Cancellation Token Source for gif encoder
	private CancellationToken
		gifToken;                   // Cancellation token for gif encoder
	internal Task
#if NULLABLE
		?
#endif
		mainTask;          // Main Generation Task

	unsafe private delegate*<Vector3, ref byte*, Random, void> ditherFunc = &NoDithering;
	unsafe private delegate*<Vector3, double, Vector3> saturateFunc = &Identity;
	unsafe private delegate*<FractalTask, int, ref byte*, FractalGenerator, void> rowFunc = &Noise;
#endregion

	#region Delegates
	// Row:
	unsafe private static void Noise(FractalTask drawTask, int y, ref byte* ptr, FractalGenerator self) {
		if (drawTask.VoidNoise is not Vector3[] voidNoise)
			throw new("voidNoise is null");
		fixed (int* voidY = &drawTask.VoidDepth[y]) {
			fixed (Vector3* buffY = &drawTask.Buffer[y]) {
				var fy = (float)y / (self.allocVoid * drawTask.ApplyWidth);
				var startY = (int)Math.Floor(fy);
				var ay = fy - startY;
				var oy = 1f - ay;
				fixed(Vector3* NoiseY = &voidNoise[startY * self.noiseWidth])
				for (var x = 0; x < drawTask.ApplyWidth; ++x) {
					var fx = (float)x / self.allocVoid;
					var startX = (int)Math.Floor(fx);
					float voidAmb = voidY[x] / drawTask.VoidDepthMax,
						ax = fx - startX, ox = 1f - ax, b0 = ox * oy, b1 = ax * oy, b2 = ox * ay, b3 = ax * ay;
					self.ditherFunc(ApplyAmbientNoise(
						self.saturateFunc(Normalize(buffY[x], drawTask.LightNormalizer), self.SelectedSaturate),
						voidAmb * self.SelectedAmbient, (1.0f - voidAmb) * voidAmb,
						// bilinear interpolation of the scaled void noise:
						b0 * NoiseY[startX] + 
						b1 * NoiseY[startX + 1] + 
						b2 * NoiseY[startX + self.noiseWidth] + 
						b3 * NoiseY[startX + self.noiseWidth + 1]
					), ref ptr, self.random);
				}
			}
		}
	}
	unsafe private static void NoNoise(FractalTask drawTask, int y, ref byte* ptr, FractalGenerator self) {
		fixed (int* voidY = &drawTask.VoidDepth[y]) {
			fixed (Vector3* buffY = &drawTask.Buffer[y]) {
				for (var x = 0; x < drawTask.ApplyWidth; ++x)
					self.ditherFunc(
						self.saturateFunc(Normalize(buffY[x], drawTask.LightNormalizer), self.SelectedSaturate
						) + new Vector3(self.SelectedAmbient * voidY[x] / drawTask.VoidDepthMax), ref ptr, self.random);
			}
		}
	}
	unsafe private static void NoAmbient(FractalTask drawTask, int y, ref byte* ptr, FractalGenerator self) {
		fixed (Vector3* buffY = &drawTask.Buffer[y]) {
			for (var x = 0; x < drawTask.ApplyWidth; ++x)
				self.ditherFunc(self.saturateFunc(Normalize(buffY[x], drawTask.LightNormalizer), self.SelectedSaturate), ref ptr, self.random);
		}
	}
	// Saturate:
	private static Vector3 ApplySaturate(Vector3 rgb, double selectedSaturate) {
		// The saturation equation boosting up the saturation of the pixel (powered by the saturation slider setting)
		float mMax, min = Math.Min(Math.Min(rgb.X, rgb.Y), rgb.Z), max = Math.Max(Math.Max(rgb.X, rgb.Y), rgb.Z);
		return max <= min ? rgb : ((mMax = max * (float)selectedSaturate / (max - min)) + 1 - (float)selectedSaturate) * rgb - new Vector3(min * mMax);
	}
	private static Vector3 Identity(Vector3 v, double _) => v;
	// Dither:
	unsafe private static void Dithering(Vector3 rgb, ref byte* ptr, Random r) {
		ptr[0] = Dither(rgb.Z, r);
		ptr[1] = Dither(rgb.Y, r);
		ptr[2] = Dither(rgb.X, r);
		ptr += 3;
	}
	unsafe private static void NoDithering(Vector3 rgb, ref byte* ptr, Random _) {
		ptr[0] = (byte)rgb.Z;
		ptr[1] = (byte)rgb.Y;
		ptr[2] = (byte)rgb.X;
		ptr += 3;
	}
	#endregion

	#region Unsorted
	BitmapData LockBits(int index, Rectangle rect) {
		if (lockedBmp[index])
			return bitmapData[index];
		lockedBmp[index] = true;
		return bitmapData[index] = bitmap[index].LockBits(rect, ImageLockMode.ReadWrite, 
			allocGpuDrawType == GpuDrawType.CPU ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb);
	}
	void UnlockBits(uint index) {
		if (!lockedBmp[index])
			return;
		bitmap[index].UnlockBits(bitmapData[index]);
		lockedBmp[index] = false;
	}
	internal bool GetGenerateAnimation() // Are we allowed to generate frames beyond the first one?
		=> allocGenerationType > GenerationType.OnlyImage && !allocPreviewMode;
	private int GetGenerateLength(bool askPriority = false) // How many images we want to generate? All or just at least first full resolution image?
		=> GetGenerateAnimation() && Working && (this == GeneratorsForm.Priority || askPriority)
		? bitmap.Length : previewFrames + 1; // Animation frames are only allowed for one unfinished generator at a time
	internal bool GetNoPriority(out int frames, bool askPriority = false) {
		frames = GetGenerateLength(askPriority);
		return (allocPngType == PngType.No || tryPng >= frames) && (allocGifType == GifType.No || tryGif >= frames) && bitmapsFinished >= frames;
	}
	unsafe internal void SelectDithering(bool enabled) {
		ditherFunc = (allocDithering = enabled) ? &Dithering : &NoDithering;
	}
	private static byte Dither(float value, Random r) {
		byte b = (byte)value;
		return r.NextDouble() < value - b ? (byte)(b + 1) : b;
	}
	private static Vector3 Normalize(Vector3 pixel, float lightNormalizer) {
		var max = Math.Max(pixel.X, Math.Max(pixel.Y, pixel.Z));
		return lightNormalizer * max > 254.0f ? 254.0f / max * pixel : lightNormalizer * pixel;
	}
	private static Vector3 ApplyAmbientNoise(Vector3 rgb, float amb, float noise, Vector3 rand)
		=> rgb + new Vector3(amb) + noise * rand;
	internal static Vector3 SampleColor(Vector3[] set, double i) {
		// sample interpolated color for the palette:
		var m = set.Length;
		var modI = i % m;
		var a = (int)Math.Floor(modI);
		return Vector3.Lerp(set[a], set[(a + 1) % m], (float)(modI - a));
	}
	#endregion

	#region Init
	internal static void WarmUpShaders() {
		var d = GraphicsDevice.GetDefault();
		// Minimal buffers just to trigger pipeline creation
		var wi = d.AllocateReadWriteBuffer<int>(9);
		var wf = d.AllocateReadWriteBuffer<Float3>(1);
		var rf = d.AllocateReadOnlyBuffer<Float3>(1);
		Int2 i2 = new(1, 1);
		d.For(1, 1, new VoidBfs(wi, wi, wi, 1));
		d.For(1, 1, new JumpFlood(wi, wi, 1, 1, 1));
		d.For(1, 1, new JumpFloodBoundlessOptimized(wi, wi, 1, 1));
		d.For(1, 1, new PipeAmbient(wf, 1, wi, 1, 1));

		d.For(1, 1, new PipeBytes(wf, wi, 1));
		d.For(1, 1, new PipeBytesDither(wf, wi, 1));
		d.For(1, 1, new PipeNoise(wf, 1, wi, 1, 1, rf, i2));
		d.For(1, 1, new PipeNormalize(wf, rf, 1, 1f));
		d.For(1, 1, new PipeNormalizeSaturate(wf, rf, 1, 1f, 1f));

		d.For(1, 1, new DrawAmbient(wi, rf, 1, 1f, wi, 1, 1f));
		d.For(1, 1, new DrawAmbientDither(wi, rf, 1, 1f, wi, 1, 1f));
		d.For(1, 1, new DrawAmbientSaturate(wi, rf, 1, 1f, wi, 1, 1f, 1f));
		d.For(1, 1, new DrawAmbientSaturateDither(wi, rf, 1, 1f, wi, 1, 1f, 1f));

		d.For(1, 1, new Draw(wi, rf, 1, 1f));
		d.For(1, 1, new DrawDither(wi, rf, 1, 1f));
		d.For(1, 1, new DrawSaturate(wi, rf, 1, 1f, 1f));
		d.For(1, 1, new DrawSaturateDither(wi, rf, 1, 1f, 1f));

		d.For(1, 1, new DrawNoise(wi, rf, 1, 1f, wi, 1, 1f, rf, i2));
		d.For(1, 1, new DrawNoiseDither(wi, rf, 1, 1f, wi, 1, 1f, rf, i2));
		d.For(1, 1, new DrawNoiseSaturate(wi, rf, 1, 1f, wi, 1, 1f, rf, i2, 1f));
		d.For(1, 1, new DrawNoiseSaturateDither(wi, rf, 1, 1f, wi, 1, 1f, rf, i2, 1f));

		wi.Dispose();
		wf.Dispose();
		rf.Dispose();
	}
	internal FractalGenerator(uint index) => Index = index;
	private string MakeTemp() {
		string path = Path.Combine(GetGensSaveDir(), Index.ToString());
		if (!Directory.Exists(path))
			_ = Directory.CreateDirectory(path);
		return path;
	}
	#endregion

	#region Tasks
	private static Stopwatch BeginTask() {
#if CustomDebug
		// start a debug task stopwatch
		Stopwatch stop = new(); stop.Start(); return stop;
#else
		return null;
#endif
	}
	private void FinishTask(FractalTask task, Stopwatch stop, string log) {
#if CustomDebug
		stop.Stop();
		var logTask = "";
		Log(ref logTask, log + " time = " + stop.Elapsed.TotalMilliseconds + " ms.");
		Monitor.Enter(taskLock);
		try {
			Log(ref logString, logTask);
		} finally { Monitor.Exit(taskLock); }
#endif
		task.State = TaskState.Done;
	}
	/* Parallel threading management - will keep creating new threads unless cancelled and return when all threads are finished
		 * 
		 * @param mainLoop - being called from the main loop and not the OnDepth
		 * @param operation - gets called when a task is free, should return true if it created a new task
		 */
	void FinishTasks(bool cancelTasks, bool mainLoop, Func<short, bool> lambda) {
		for (var i = 3; i > 0; --i) { // The must be no remaining tasks at least 3 times after checking all of them to consider the generator really task fee and exit
			for (var tasksRemaining = true; tasksRemaining; MakeDebugString()) {
				tasksRemaining = false;
				for (var t = allocMaxTasks; 0 <= --t;) { // Try all the tasks
					var task = tasks[t];
					tasksRemaining |= task.IsStillRunning()
						? mainLoop ||
						  task.BitmapIndex >= 0 &&
						  bitmapState[task.BitmapIndex] <=
						  BitmapState.Dots // Must finish all Dots threads, and if in main loop all secondary threads too (OnDepth can continue back to main loop when secondary threads are running, so it could start a new OnDepth loop)
						: !(IsCancelRequested() || cancelTasks || tryPng >= GetGenerateLength()) && // Cancel Request forbids any new threads to start
						  (	// changing these settings should exit, then they get updated and restart the main loop with them updated (except onDepth which must finish first)
							  !mainLoop || SelectedMaxTasks == allocMaxTasks && // need to reallocate task container
							  allocParallelType == SelectedParallelType && // need to change parallel type
							  SelectedGifType == allocGifType && // need to change gif encoding
							  SelectedDelay == allocDelay && // need to change delay
							  SelectedPngType == allocPngType && // need to change png encoding
							  SelectedGenerationType == allocGenerationType // need to change generation type
						  ) &&
						  (mainLoop && (TryWriteGifFrames(task) || TryFinishBitmaps(task) || TryPngBitmaps(task)) ||
						   lambda(t)); // in the main loop we try Bitmap finishing and writing secondary threads (onDepth loop would get stuck )
				}
				if (tasksRemaining)
					i = 3; // we've had some threads going on, reset the counter to 3
				if (pngFailed >= MaxPngFails)
					allocPngType = PngType.No; // png repeatedly failed, so turn that off
			}
		}
	}
	bool TryWriteGifFrames(FractalTask task) {
		if (IsCancelRequested() // Do not write gid frames when cancelled
			|| gifEncoder == null // ...or if gifEncoder doesn't exist
			|| gifEncoder.IsFinished() // ...or if it's finished
			|| allocGifType == GifType.No// ...or we are not supposed to encode a gif
			|| isWritingBitmaps <= 0 // ...or this task is already running
			|| --isWritingBitmaps > 0) // ...or we have already run it not too long ago
			return false;
		task.Start(-1, () => TryWriteGifFrame(task.TaskIndex));
		return true;
	}
	void TryWriteGifFrame(int taskIndex) {
		while (gifEncoder != null && !IsCancelRequested()) {
			if (bitmapsFinished >= bitmap.Length && !gifEncoder.IsFinished())
				_ = gifEncoder.Finish();
			//if (applyGenerationType == GenerationType.Mp4) {
			//} else {
			var unlock = gifEncoder.FinishedFrame();
			if (tasks == null)
				throw new("TryWriteGifFrame: tasks null");
			// Try to finalize the previous encoder tasks
			switch (gifEncoder.TryWrite(true)) {
				case TryWrite.Failed:
					// fallback to only display animation without encoding
					StopGif(tasks[taskIndex]);
					isWritingBitmaps = 2;
					tasks[taskIndex].State = TaskState.Done;
					return;
				case TryWrite.FinishedFrame:
					// mark the bitmap state as fully finished
					encodedGif[unlock + previewFrames] = 3;
					if (unlock + previewFrames + 1 == bitmap.Length)
						FinishGif();
					break;
				/*case TryWrite.FinishedAnimation:
					FinishGif();
					isWritingBitmaps = 2;
					tasks[taskIndex].State = TaskState.Done;
					return;*/
				default:
					// waiting or finished animation
					isWritingBitmaps = 2;
					tasks[taskIndex].State = TaskState.Done;
					return;
			}
			//}
		}
		isWritingBitmaps = 2;
		tasks[taskIndex].State = TaskState.Done;
	}
	bool TryFinishBitmaps(FractalTask task) {
		if (IsCancelRequested())
			return false;
		// do we want to encode gif?
		var gif = allocGifType != GifType.No && gifEncoder != null;
		if (gif) {
			// initial gif index after preview
			if (tryGif < previewFrames)
				tryGif = previewFrames;
			// increment the gif index to the first not encoded yet
			while (tryGif < bitmap.Length && encodedGif[tryGif] >= 1)
				++tryGif;
			// if that gif's image exists
			if (tryGif < bitmap.Length) {
				// try up to maxTasks images far away if any is available to start encoding
				var tryEncode = tryGif;
				var maxTry = Math.Min(bitmap.Length, bitmapsFinished + allocMaxTasks);
				while (tryEncode < maxTry) {
					// this image can be encoded, so do it:
					if (bitmapState[tryEncode] >= BitmapState.DrawingFinished && encodedGif[tryEncode] == 0) {
						_ = LockBits(tryEncode, rect);
						encodedGif[tryEncode] = 1;
						task.Start(tryEncode, () => GenerateGif(task));
						return true;
					}
					++tryEncode;
				}
			}
		}
		if (IsCancelRequested() // Do not write gid frames when cancelled
			//|| bitmapsFinished < previewFrames // Do not finish preview bitmaps from here, the OfDepth calls it instead
			|| bitmapsFinished >= bitmap.Length
			|| !CanFinishBitmap(gif)
			|| isFinishingBitmaps <= 0 // ...or this task is already running
			|| --isFinishingBitmaps > 0) // ...or we have already run it not too long ago
			return false;
		task.Start(-2, () => TryFinishBitmap(gif, task.TaskIndex));
		return true;
	}
	bool CanFinishBitmap(bool gif) => bitmapsFinished < bitmap.Length && bitmapState[bitmapsFinished] == BitmapState.DrawingFinished && (!gif || encodedGif[bitmapsFinished] == 3);
	void TryFinishBitmap(bool gif, short taskIndex) {
		Monitor.Enter(taskLock);
		var stop = BeginTask();
		try {
			var startFinished = bitmapsFinished;
			while (!IsCancelRequested() && CanFinishBitmap(gif)) {
				bitmapState[bitmapsFinished] = BitmapState.Unlocked;
				// Calculate the unique seed hash file
				if (allocGenerationType == GenerationType.HashSeeds)
					MakeSeedHashFile();
				UnlockBits(bitmapsFinished);
				// Let the form know it's ready
				if (bitmapsFinished++ <= previewFrames && !IsCancelRequested())
					UpdatePreview?.Invoke();
			}
			// Only called with bitmapIndex -2 is a unique task that needs to mark itself done, otherwise it's a previewImage called within another task
			var task = tasks[taskIndex];
			if (task.BitmapIndex < 0)
				FinishTask(task, stop, "UNLOCK:" + startFinished + "->" + bitmapsFinished );
		} finally { Monitor.Exit(taskLock); }
		isFinishingBitmaps = 2;
	}
	void MakeSeedHashFile() {
		using var sha256 = SHA256.Create();
		unchecked {
			unsafe {
				var byteNum = bitmapData[bitmapsFinished].Stride * SelectedHeight;
				var pixelData = new byte[byteNum];
				// Copy the raw pixel data from Scan0 to the byte array
				fixed (byte* dest = pixelData) {
					Buffer.MemoryCopy((void*)bitmapData[bitmapsFinished].Scan0, dest, byteNum, byteNum);
				}
				// make a hash and try to add it into the map if any same is not there yet:
				var key = BitConverter.ToString(SHA256.HashData(pixelData));
				_ = Hash.TryAdd(key, bitmapsFinished);
			}
		}
		// Finish hash file with all those found unique seeds:
		if (bitmapsFinished == bitmap.Length - 1) {
			var output = "";
			foreach (var i in Hash)
				output += "," + i.Value;
			File.WriteAllText("hash.txt", output);
		}
	}
	bool TryPngBitmaps(FractalTask task) {
		if (IsCancelRequested() || bitmapsFinished < previewFrames) // Do not write when cancelled
			return false;
		// Png is starting from scratch - cleanup the temp files and start png index from after the preview
		if (tryPng < 0) {
			tryPng = previewFrames;
			CleanupTempFiles();
		}
		if (allocPngType == PngType.No) {
			//tryPng = bitmapsFinished;
			return false;
		}
		// Increment png index to the first one that hasn;t been encoded yet
		while (tryPng < bitmap.Length && encodedPng[tryPng] >= 2)
			++tryPng;
		// are all pngs encoded? If so abort thes attempt as finished
		if (tryPng >= bitmap.Length)
			return false;
		// find an image avaiable to encode png from the first one thet needs to be encoded to that + maxTasks:
		var bitmapIndex = tryPng;
		for (var mx = Math.Min(bitmap.Length, tryPng + allocMaxTasks); bitmapIndex < mx && encodedPng[bitmapIndex] >= 1; ++bitmapIndex) { }
		if (bitmapIndex >= bitmap.Length || encodedPng[bitmapIndex] >= 1 || bitmapIndex >= bitmap.Length || bitmapState[bitmapIndex] < BitmapState.Unlocked)
			return false;
		// this one is avaiable, so do it:
		encodedPng[bitmapIndex] = 1;
		if (IsCancelRequested()) 
			return false; // Do not write gif frames when cancelled
		task.Start(-3, () => TryPngBitmap(task.TaskIndex, bitmapIndex));
		return true;
	}
	void TryPngBitmap(short taskIndex, int bitmapIndex) {
		var stop = BeginTask();
		//  try to save png, if it failed, decrease the png index to this one so it can be tried again later
		if (SavePng(bitmapIndex - previewFrames)) 
			tryPng = Math.Min(tryPng, (short)bitmapIndex);
		FinishTask(tasks[taskIndex], stop, "PNG:" + bitmapIndex);
	}
	void GenerateGif(FractalTask task) {
		var stop = BeginTask();
		// This wa an attempt to encode mp4 in house, but I just do in with ffmpeg.exe now...
		// Mp4 frame encode:
		/*if (applyGenerationType == GenerationType.Mp4) {
			if (mp4Encoder != null) {
				bitmapState[task.bitmapIndex] = BitmapState.Encoding; // Start encoding Frame to a temp GIF	
				unsafe {
					var d = bitmapData[task.bitmapIndex];
					byte* ptr = (byte*)(void*)d.Scan0;
					mp4Encoder.AddFrame(ptr, d.Stride);
					bitmapState[task.bitmapIndex] = BitmapState.EncodingFinished;
				}
			} else {
				StopGif(task);
				bitmapState[task.bitmapIndex] = BitmapState.DrawingFinished;
			}
		} else {*/
		// GIF frame encode:
		if (gifEncoder != null) {
			unsafe {
				var d = bitmapData[task.BitmapIndex];
				var ptr = (byte*)(void*)d.Scan0;
				// Start encoding this image as GIF
				gifEncoder.AddFrameParallel(ptr, d.Stride, gifToken, task.BitmapIndex - previewFrames/*, allocGpuDrawType == GpuDrawType.CPU*/);
				encodedGif[task.BitmapIndex] = 2;
			}
		} else {
			// no gif encoder - just cancel the gif encoding mode
			StopGif(task);
			encodedGif[task.BitmapIndex] = 0;
		}
		FinishTask(task, stop, "Gif:" + task.BitmapIndex);
	}
	#endregion

	private void GenerateAnimation() {

		#region GeneralAnimation_Implementation
#if CustomDebug
		// Start a new DebugLog
		logString = "";
		Log(ref logString, "New Generate()");
		startTime = new();
		startTime.Start();
#endif
		// copy childAngle and childColor sets for using (so it doesn't crash if you change them mid generation)
		
		if (f.ChildCount > 0) {
			if (SelectedEditorMode) {
				var ca = f.ChildAngle[SelectedChildAngle].Item2;
				var cc = f.ChildColor[SelectedChildColor].Item2;
				var i = f.ChildCount;
				while (0 <= --i) 
					ChildAngle[i] = ca[i];
				i = f.ChildCount;
				while (0 <= --i) 
					ChildColor[i] = cc[i];
			} else {
				// Prefill with zeroes
				var i = f.ChildCount;
				while (0 <= --i)
					ChildAngle[i] = 0;
				i = f.ChildCount;
				while (0 <= --i)
					ChildColor[i] = 0;
				// Add all selected angles
				var ai = SelectedChildAngles;
				int selectI = 0;
				while (ai > 0) {
					if ((ai & 1) == 1) {
						// This set of angles is selected, so add it
						var ca = f.ChildAngle[selectI].Item2;
						i = f.ChildCount;
						while (0 <= --i)
							ChildAngle[i] = (ChildAngle[i] + ca[i]) % (Math.PI * 4);
					}
					++selectI; // next set
					ai >>= 1; // next select bit
				}
				// Add all selected colors
				ai = SelectedChildColors;
				selectI = 0;
				while (ai > 0) {
					if ((ai & 1) == 1) {
						// This set of colors is selected, so add it
						var cc = f.ChildColor[selectI].Item2;
						i = f.ChildCount;
						while (0 <= --i)
							ChildColor[i] = (byte)((ChildColor[i] + cc[i]) % 6);
					}
					++selectI; // next set
					ai >>= 1; // next select bit
				}
			}
		}
		// make a copy or random zoom, cutsed, applyhue, gif+png encoding for the sme reason
		allocZoom = (short)(SelectedZoom > -2 ? SelectedZoom : random.NextDouble() < .5 ? -1 : 1);
		allocCutSeed = SelectedCutSeed < 0 ? random.Next(0, GetMaxCutSeed()) : SelectedCutSeed;
		allocHue = (short)(SelectedHue > -2 ? SelectedHue : random.Next(-1, 2));
		allocGifType = SelectedGifType;
		allocPngType = SelectedPngType;
		unsafe { saturateFunc = SelectedSaturate > 0.0 ? &ApplySaturate : &Identity; }
		// reset all coutner what we have done with bitmaps: to generate, generated, failed png attempts, and to encode png.
		nextBitmap = 0; bitmapsFinished = pngFailed = 0; tryPng = -1;

		// A complex expression to get the multiplier of the basic period required to get to a seamless loop, how many times to zoom to loop back:
		var m = (ushort)(allocHue == 0 && ChildColor[selectedZoomChild] > 0 && allocZoom != 0 ? SelectedPeriodMultiplier * allocPalette.Length : SelectedPeriodMultiplier);
		var asymmetric = ChildAngle[selectedZoomChild] < 2.0 * Math.PI;
		var doubled = (Math.Abs(SelectedSpin) > 1 && selectedZoomChild == 0 || SelectedSpin == 0 && asymmetric) && allocZoom != 0;
		m = (ushort)(doubled ? 2 * m : m);
		bool twice =  asymmetric && !doubled;
		if (selectedZoomChild == 0) { // when zooming to the center:
			allocPeriodMultiplier = m;
			// Calculate rotational symmetry (minimum rotation to get back to a seamless loop):
			allocPeriodAngle = f.ChildCount <= 0 ? 0 : ChildAngle[0] % (2.0 * Math.PI);
			// This is duplicated in all these conditions:
			allocPeriodAngle = SelectedPeriodMultiplier % 2 == 0 && twice ? allocPeriodAngle * 2 : allocPeriodAngle;
		} else {
			// Calculate rotational symmetry (minimum rotation to get back to a seamless loop), and recompute the period multiplier:
			var a = ChildAngle[selectedZoomChild] * m % (2 * Math.PI);
			if (SelectedSpin == 0) {
				for (allocPeriodMultiplier = 1; a is > 0.1 and < 2 * Math.PI - 0.1; ++allocPeriodMultiplier)
					a = (a + ChildAngle[selectedZoomChild] * m) % (2 * Math.PI);
				allocPeriodMultiplier *= m;
			} else 
				allocPeriodMultiplier = m;
			allocPeriodAngle = a == 0 ? 2 * Math.PI : a;
		}
		// add how many times we want to add another extra minimum spin to the loop
		allocExtraSpin = twice ? (ushort)(2 * SelectedExtraSpin) : SelectedExtraSpin;

		// A complex expression to calculate the minimum needed hue shift speed to match the loop: supporting the new custom palettes:
		var finalHueShift = allocPeriodMultiplier * ChildColor[selectedZoomChild] % allocPalette2;
		if (finalHueShift == 0 || allocZoom == 0) {
			hueCycleMultiplier = allocPalette2;
		} else {
			if (allocZoom > 0 != allocHue > 0)
				finalHueShift = (allocPalette2 - finalHueShift) % allocPalette2;
			hueCycleMultiplier = (byte)(allocPalette2 - finalHueShift);
		}
		
		// How many total frames will the naimation have?
		var frames = (allocGenerationType = SelectedGenerationType) switch {
			GenerationType.AllSeeds => GetMaxCutSeed() + 1,
			GenerationType.HashSeeds => CutSeed_Max + 1,
			_ => (short)(debugPeriodOverride > 0 ? debugPeriodOverride : SelectedPeriod * allocPeriodMultiplier)
		};

		// Dispose any previously used memory streams
		if (msPngs != null)
			for (int i = 0; i < msPngs.Length; ++i) {
				msPngs[i]?.Dispose();
				msPngs[i] = null;
			}

		previewFrames = (byte)Math.Max(0, Math.Min(SelectedMaxPreviewFrames,
			Math.Log2(Math.Min(SelectedWidth, SelectedHeight)) - 2 - (allocLessPreviewFrames = SelectedLessPreviewFrames)
		));
		// Allocate/reallocate image buffers
		if ((frames += previewFrames) != allocFrames) {
			bitmap = new Bitmap[allocFrames = frames];
			bitmapData = new BitmapData[frames];
			bitmapState = new BitmapState[frames + 1];
			encodedPng = new byte[frames];
			encodedGif = new byte[frames];
			lockedBmp = new bool[frames];
			msPngs = new MemoryStream[frames];
		} else {
			for (var i = frames; 0 <= --i; encodedPng[i] = 0) { }
			for (var i = frames; 0 <= --i; encodedGif[i] = 0) { }
			for (var i = frames; 0 <= --i; lockedBmp[i] = false) { }
		}

		// Setup reset BitmapStates
		for (var b = frames; b >= 0; bitmapState[b--] = BitmapState.Queued) { }

		// Initialzie a gif encoder if selected
		StartGif();

		// Initialize the starting default animation values
		double size = 2400, angle = SelectedDefaultAngle * Math.PI / 180.0;
		var hueAngle = SelectedDefaultHue;
		isWritingBitmaps = isFinishingBitmaps = 2;
		allocNoise = (int)(SelectedAmbient * SelectedNoise);
		var spin = SelectedSpin < -1 ? (short)random.Next(-2, 3) : SelectedSpin;
		allocBlur = (ushort)(SelectedBlur + 1);
		allocPreviewMode = SelectedPreviewMode;

		// Resize for a preview mode
		if (allocPreviewMode) {
			var w = Math.Max(SelectedWidth, SelectedHeight) * f.MaxSize * 0.1;
			size = w * f.ChildSize * .9;
		}

		// Prezoom the image by the default zoom value
		for (var i = SelectedDefaultZoom < 0
			     ? random.Next(0, SelectedPeriod * allocPeriodMultiplier)
			     : SelectedDefaultZoom % (SelectedPeriod * allocPeriodMultiplier);
		     0 <= --i;
		     IncFrameSize(ref size, SelectedPeriod)) { }

		// Pre-generate color blends
		if (allocGenerationType < GenerationType.AllSeeds)
			AverageChildCount = PreGenerateCutSeedAndBlends(0, colorBlends, out startCutSeed);
		allocDetail = SelectedDetail;
#if SmoothnessDebugDetail
		applyDetail = 10.0;
#endif
		// Generate the images
		while (!IsCancelRequested()) {
			void NewCache() {
				// Precalculate the caching:
				if (allocCacheOpt = SelectedCacheOpt) {
					var previewTask = tasks[0];
					previewTask.BitmapIndex = previewFrames;
					PreviewResolution(previewTask);
					CalculateCache(previewTask);
					allocBinSize = previewTask.BinSize;
					allocStripeHeight = previewTask.StripeHeight;
					allocStripeCount = previewTask.StripeCount;
				} else {
					allocBinSize = 0;
					allocStripeHeight = 0;
					allocStripeCount = 0;
				}
				UpdateCache?.Invoke();
			}
			// if we need to start/restart gif encoder:
			if (SelectedGifType != allocGifType || SelectedDelay != allocDelay) {
				FinishTasks(true, true, _ => false);
				StopGif(null);
				StartGif();
			}
			// Did we enable PNG encoding mid generation? So start to try encode all pngs that haven't been encoded yet
			if (SelectedPngType != allocPngType && (allocPngType = SelectedPngType) == PngType.Yes)
				tryPng = -1;
			// Copy selected ParallelType to be used, again to make sure it doens't change mid generation, it can change in this mid step though
			allocParallelType = SelectedParallelType;
			// Initialize buffers (delete and reset if size changed)
			if ((allocMaxTasks = Math.Max((short)MinTasks, SelectedMaxTasks)) != allocTasks) {
				if (allocTasks >= 0)
					for (var t = 0; t < allocTasks; ++t) //{
						tasks[t].Join();
				rect = new(0, 0, allocWidth = (short)SelectedWidth, allocHeight = (short)SelectedHeight);
				tasks = new FractalTask[allocTasks = allocMaxTasks];
				tuples = new (short, (double, double), (double, double), byte, long, byte)[allocMaxTasks * DepthDiv];
				allocVoid = (ushort)(SelectedVoid + 1); // already reallocated the noise buffer
				noiseWidth = (ushort)(SelectedWidth / allocVoid + 2);
				noiseHeight = (ushort)(SelectedHeight / allocVoid + 2);
				// Regular NeBuffer
				for (var t = allocMaxTasks; 0 <= --t;) {
					var task = tasks[t] = new FractalTask();
					task.TaskIndex = t;
					NewBuffer(task);
				}
				SetMaxIterations();
				NewCache();
			}
			
			/*if (SelectedWidth != allocWidth) {
				for (short t = 0; t < allocMaxTasks; tasks[t++].Kernel = new float[SelectedWidth]) { }
			}*/
			// width/height changed reallocates Y of buffers (with all X inside)
			if (SelectedHeight != allocHeight || SelectedWidth != allocWidth) {
				allocVoid = (ushort)(SelectedVoid + 1); // already reallocated the noise buffer
				noiseWidth = (ushort)(SelectedWidth / allocVoid + 2);
				noiseHeight = (ushort)(SelectedHeight / allocVoid + 2);
				for (short t = 0; t < allocMaxTasks; NewBuffer(tasks[t++])) { }
				rect = new(0, 0, allocWidth = (short)SelectedWidth, allocHeight = (short)SelectedHeight);
				NewCache();
			}
			// reallocate noise buffer if the noise resolution has changed (but YX did not, so keep tose buffers)
			if (allocVoid != SelectedVoid + 1) {
				allocVoid = (ushort)(SelectedVoid + 1); // already reallocated the noise buffer
				noiseWidth = (ushort)(SelectedWidth / allocVoid + 2);
				noiseHeight = (ushort)(SelectedHeight / allocVoid + 2);
				InitNoise();
			}
			if (SelectedGpuDrawType != allocGpuDrawType) {
				allocGpuDrawType = SelectedGpuDrawType;
				InitNoise();
			}
			// Are we using the new GPU optimizations?
			allocGpuVoidType = SelectedGpuVoidType;

			// If only Cache settings changed, then also update them
			//if (SelectedBinSize * 100 != allocBinSize || SelectedStripeHeight != allocStripeHeight
			//	|| SelectedCacheOpt != allocCacheOpt || SelectedBinSize * SelectedStripeHeight == 0) // switched on/off or automatic
			if ((SelectedBinSize * 100 != allocBinSize || SelectedStripeHeight != allocStripeHeight) && SelectedBinSize + SelectedStripeHeight > 0 // Changed overrride settings
				|| SelectedCacheOpt != allocCacheOpt || SelectedBinSize * SelectedStripeHeight == 0) // switched on/off or automatic
				NewCache();
			
			// remember how many iterations deep are we going to go.
			allocMaxIterations = maxIterations;
			// Let this master thread sleep fo a tiny bit if nothing needs to be done (no more frames to generate or encode)
			if (GetNoPriority(out frames)) {
				Thread.Sleep(50);
				continue;
			}
			// The magical task manager, it takes care to parallel run all the tasks for generation and encoding until everything we set to do is done.
			// It will exit is we abort the generation, or if we changed somethign that dones't need to restart it but should go redo the code above.
			FinishTasks(false, true, taskIndex => {
				if (nextBitmap >= GetGenerateLength())
					return false;// The task is finished, no need to wait for this one
				var bmp = nextBitmap++;
				double tempSize = bmp < previewFrames ? size / (1 << (previewFrames - bmp - 1 + allocLessPreviewFrames)) : size, tempAngle = angle;
				var tempHueAngle = hueAngle;
				var tempSpin = spin;
				//bitmapState[nextBitmap] = BitmapState.Dots; // i was getting queued state tasks, this solved that, so that just means they take a while to get started, not an error
				if (allocParallelType > ParallelType.OfAnimation || allocMaxTasks <= MinTasks /*|| bmp < previewFrames*/) // used to only let generate previews with OfDepth parallelism, but OfAnimation is OK too
					GenerateDots(bmp, (short)(-taskIndex - 1), tempSize, tempAngle, tempSpin, 0, tempHueAngle);
				else tasks[taskIndex].Start(bmp, () => GenerateDots(bmp, (short)(taskIndex + 1), tempSize, tempAngle, tempSpin, 0, tempHueAngle));
				if (bmp >= previewFrames)
					IncFrameParameters(ref size, ref angle, ref hueAngle, spin, 1);
				return true; // A task finished, but started another one - keep checking before new master loop
			}
			);
		}

		// The generation was finished or aborted:

		//Clear any bin that weren't cleared possibly due to aborting:
		foreach (var t in tasks)
			t.ClearBin();
		// task has finished:
		mainTask = null;
		return;
		#endregion

		#region InitData
		void NewBuffer(FractalTask task) {
			//task.Kernel = new float[SelectedWidth];
			// Initialized new buffer data (new task or height changed)
			var area = SelectedWidth * SelectedHeight;
			task.VoidQueue = new int[area];
			task.VoidDepth = new int[area];
			task.VoidDepthN = new int[area];
			task.Buffer = new Vector3[area];
			if (SelectedGpuDrawType == GpuDrawType.CPU) {
				task.VoidNoise = new Vector3[noiseWidth * noiseHeight];
			} else {
				task.VoidNoiseF = new Float3[noiseWidth * noiseHeight];
			}
			//for (var y = 0; y < SelectedHeight; buffT[y++] = new Vector3[SelectedWidth]) { } 
			//for (var y = 0; y < vh; noiseT[y++] = new Vector3[vw]) {}
		}
		void NewOfDepthBuffer(bool newTasks) {
			if (buffer == null)
				throw new("buffer is not null");
			if (newTasks)
				buffer = new Vector3[allocMaxTasks][];
			for (var t = allocMaxTasks; 0 <= --t; buffer[t] = new Vector3[allocWidth * allocHeight]) { }
				//var buffT = newHeight ? buffer[t] = new Vector3[allocHeight][] : buffer[t];
				//for (var y = 0; y < SelectedHeight; ++y) 
				//	buffT[y] = new Vector3[allocWidth];
		}
		void InitNoise() {
			if (SelectedGpuDrawType == GpuDrawType.CPU)
				for (short t = 0; t < allocMaxTasks; t++) {
					tasks[t].VoidNoise = new Vector3[noiseWidth * noiseHeight];
					tasks[t].VoidNoiseF = null;
				}
			else for (short t = 0; t < allocMaxTasks; t++) {
					tasks[t].VoidNoiseF = new Float3[noiseWidth * noiseHeight];
					tasks[t].VoidNoise = null;
				}
		}
		void CalculateCache(FractalTask task) {
			// This is very simplistic estimator of L2, could use something more direct and precise, like:
			/*using System.Management;
			static int GetL2CacheFromWMI() {
				using var searcher = new ManagementObjectSearcher("SELECT L2CacheSize FROM Win32_Processor");
				foreach (var item in searcher.Get()) {
					var sizeKB = (uint)item["L2CacheSize"];
					return (int)(sizeKB * 1024);
				}
			return 0;
			}*/

			if (SelectedBinSize > 0 && SelectedStripeHeight > 0) {
				task.BinSize = (uint)(SelectedBinSize * 100); // 
				task.StripeHeight = (ushort)Math.Ceiling((float)SelectedStripeHeight * task.ApplyHeight / SelectedHeight);
				task.StripeCount = (ushort)Math.Ceiling((float)task.ApplyHeight / task.StripeHeight);
			} else {
				task.ChooseStripeAndBin(allocMaxTasks, (long)(SelectedL2Kilobytes == 0 ? GetEstimatedL2CachePerCore() : 1024 * SelectedL2Kilobytes), out task.StripeCount, out task.BinSize);
				task.StripeHeight = (ushort)Math.Ceiling((float)task.ApplyHeight / task.StripeCount);
			}
		}
		void PreviewResolution(FractalTask task) {
			if (task.BitmapIndex < previewFrames) {
				// bitmaps from previewFrames back to zero have increasingly halved resolution
				var div = 1 << ((/*allocParallelType == ParallelType.OfDepth ? previewFrames - task.BitmapIndex :*/ previewFrames - task.BitmapIndex /*- 1*/) + allocLessPreviewFrames);
				task.ApplyWidth = (short)(allocWidth / div);
				task.ApplyHeight = (short)(allocHeight / div);
				// bloom gets halved too so the visible blur radius stays the same
				task.Bloom0 = SelectedBloom / div;
			} else {
				// full resolution
				task.Bloom0 = SelectedBloom;
				task.ApplyWidth = allocWidth;
				task.ApplyHeight = allocHeight;
			}
			// scaling constants
			task.WidthBorder = (short)(task.ApplyWidth - 2);
			task.HeightBorder = (short)(task.ApplyHeight - 2);
			task.Bloom1 = task.Bloom0 + 1;

			task.Kernel = new float[(int)task.Bloom1 * 2 + 2];

			/*task.UpLeftStart = -task.Bloom1;
			task.RightEnd = task.WidthBorder + task.Bloom1;
			task.DownEnd = task.HeightBorder + task.Bloom1;*/
			task.UpLeftStart = - task.Bloom0;
			task.RightEnd = task.WidthBorder + task.Bloom0;
			task.DownEnd = task.HeightBorder + task.Bloom0;
			task.ApplyDetail = allocDetail * task.Bloom1;
		}
		float PreGenerateCutSeedAndBlends(int bitmapIndex, Dictionary<long, Vector3[]> blends, out long startSeed) {
			int[] seeds;
			startSeed = allocGenerationType switch {
				// AllSeeds cutFunction seed selection - increments the set of unique seeds through bitmapIndex
				GenerationType.AllSeeds => f.ChildCutFunction.Count > 0 && (seeds = f.ChildCutFunction[SelectedCut].Item2) != null && seeds.Length > 0 && seeds[0] >= 0 ? -seeds[Math.Max(0, bitmapIndex - previewFrames)] : -Math.Max(0, bitmapIndex - previewFrames),
				// HashSeeds - increments the set of all seeds through bitmapIndex
				GenerationType.HashSeeds => -Math.Max(0, bitmapIndex - previewFrames),
				// Regular render - a single seed selected from the unique set through the user parameter
				_ => f.ChildCutFunction.Count > 0 && (seeds = f.ChildCutFunction[SelectedCut].Item2) != null && seeds.Length > 0 && seeds[0] >= 0 ? -seeds[allocCutSeed] : -allocCutSeed
			};
			//if (applyPreviewMode)
			//	return;
			// start searching for children sets at depth 3
			byte maxDepth = 3;
			blends.Clear();
			int prevCount;
			float r;
			do {
				prevCount = blends.Count;
				blends.Clear();
				// iterate to the "max" depth, and collect all unique sets of children a parent can split into at that depth
				r = PreGenerateColor(blends, 0, startSeed, 0, maxDepth);
				++maxDepth; // the next iteration would be 1 iteration deeper
			} while (prevCount != blends.Count); // if the set didn't grow anymore, then we already have all the possible sets, so let's stop and keep that

			return r;

			float PreGenerateColor(Dictionary<long, Vector3[]> refBlends, int index, long inFlags, byte inDepth, byte max = 2) {
				float r = 0;
				var i = f.ChildCount;
				if (++inDepth < max) {
					// simplified iteration loop to the "max" depth, where we look at the sets of colored children a parent can split into
					// we are only concerned with the cutFunction, so we don't compute XY or angles or color shifts
					while (0 <= --i) {
						var newFlags = CalculateFlags(i, inFlags);
						if (newFlags >= 0)
							r += PreGenerateColor(refBlends, i, newFlags, inDepth, max);
					}
					return r / f.ChildCount;
				} else {
					// initialize color counter
					var c = allocPalette;
					var cLength = c.Length;
					var cb = new Vector3[allocPalette2];
					for (var mi = 0; mi < cLength; ++mi)
						cb[mi] = new Vector3(0, 0, 0);
					// count the colors of all the children this parent will split into using the cutFunction
					while (0 <= --i)
						if (CalculateFlags(i, inFlags) >= 0) {
							++r;
							for (var mi = 0; mi < allocPalette2; ++mi)
								cb[mi] += SampleColor(c, .5 * (ChildColor[i] + mi));
						}
					// save the color mix to the map using the parent index and cutFunction memory as the key
					// every parent with the same index and cutFunction memory should spawn the same set of children relative to itself
					// so this will be an effective lookup of what mix of colors the parent will split into, so it prepare and color itself to that for seamless transition
					refBlends.TryAdd(index + f.ChildCount * (inFlags & ((1 << f.ChildCount) - 1)), cb);
					return r;
				}
			}
		}
		#endregion

		#region GenerateTasks
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (double, double) NewXY((double, double) parentXY, (double, double) childXY, double inAngle) {
			// rotate the scaled childXY around itself by childAngle
			double cs = Math.Cos(inAngle), sn = Math.Sin(inAngle);
			return (parentXY.Item1 + childXY.Item1 * cs - childXY.Item2 * sn, parentXY.Item2 - childXY.Item2 * cs - childXY.Item1 * sn);
		}
		// It's impossible to make hyperbolic fractals like this...
		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		(double, double) HyperbolicTransform((double, double) parentXY, (double, double) childXY, double inAngle) {
			// rotate the scaled childXY around itself by childAngle
			double cs = Math.Cos(inAngle), sn = Math.Sin(inAngle);
			var newXY = (childXY.Item1 * cs - childXY.Item2 * sn, -childXY.Item2 * cs - childXY.Item1 * sn);
			// Convert to complex numbers (Poincaré disk uses stereographic projection)
			var zParent = new Complex(parentXY.Item1, parentXY.Item2);
			var zChild = new Complex(newXY.Item1, newXY.Item2) * f.Hyperbolic;
			// Möbius transformation to shift child positions hyperbolically
			var transformed = (zChild + zParent) / (1 + zChild * Complex.Conjugate(zParent));
			return (transformed.Real, transformed.Imaginary);
		}*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		long CalculateFlags(int index, long inFlags) => selectedCutFunction == null ? 0 : selectedCutFunction(index, inFlags, f);
		void GenerateDots(int bitmapIndex, short stateIndex, double refSize, double refAngle, short refSpin, byte refColor, double refHueAngle) {
			#region GenerateDots_Implementation
#if CustomDebug
			var logTask = "";
			Stopwatch initTime = new();
			initTime.Start();
#endif
			// we have started generating dots for this bitmap
			bitmapState[bitmapIndex] = BitmapState.Dots;
			// get task pointers
			var dotsTaskIndex = (short)(Math.Abs(stateIndex) - 1);
			var state = stateIndex < 0 ? null : tasks[stateIndex - 1];
			var task = tasks[dotsTaskIndex];
			task.BitmapIndex = bitmapIndex;
			var buffT = task.Buffer;
			// prepare the resolution and scaling constants
			PreviewResolution(task);
			// Init buffer with zeroes
			bool ofDepthCond = allocParallelType == ParallelType.OfDepth /*|| bitmapIndex < previewFrames*/;
			var ofDepth = allocMaxTasks > MinTasks && ofDepthCond;
			/*for (var y = 0; y < task.ApplyHeight; ++y) {
				var buffY = buffT[y];
				for (var x = 0; x < task.ApplyWidth; buffY[x++] = Vector3.Zero) {}
			}*/
			for (var a = task.ApplyWidth * task.ApplyHeight; 0 <= --a; buffT[a] = Vector3.Zero) { }
			// Init MultiDepth OfDepth buffer with zeroes for non-preview images
			if (!allocCacheOpt && ofDepth && bitmapIndex >= previewFrames) {
				if(buffer == null || buffer.Length != allocMaxTasks)
					NewOfDepthBuffer(true);
				else if (buffer[0].Length != allocWidth * allocHeight)
					NewOfDepthBuffer(false);
				for (var t = 0; t < allocMaxTasks; ++t) {
					if (buffer == null)
						throw new("buffer is null");
					var tempBuffT = buffer[t];
					for (var a = task.ApplyWidth * task.ApplyHeight; 0 <= --a; tempBuffT[a] = Vector3.Zero) { }
					/*for (var y = 0; y < task.ApplyHeight; ++y) {
						var tempBuffY = tempBuffT[y];
						for (var x = 0; x < task.ApplyWidth; tempBuffY[x++] = Vector3.Zero) {}
					}*/
				}
			}

#if CustomDebug
			initTime.Stop();
			Log(ref logTask, "Init:" + bitmapIndex + " time = " + initTime.Elapsed.TotalMilliseconds + " ms.");
			Stopwatch iterTime = new();
			iterTime.Start();
#endif

			// Generate the fractal frame recursively
			if (IsCancelRequested()) {
				if (state != null) state.State = TaskState.Done;
				return;
			}
			for (var b = 0; b < allocBlur; ++b) {
				// puts the animation parameters into the correct range, scale, switch parent and children, to have it at a size that fills the screen
				ModFrameParameters(task.ApplyWidth, task.ApplyHeight, ref refSize, ref refAngle, ref refSpin, ref refColor, ref refHueAngle);
				// Prepare Color blending per one dot (hueShifting + iteration correction) and starting cutSeed
				// So that the color of the dot will slowly approach the combined colors of its children before it splits
				long startSeed;
				// if we are doing all params we will need to do this step fresh locally for each frame
				float _AverageChildCount;
				if (allocGenerationType >= GenerationType.AllSeeds)
					_AverageChildCount = PreGenerateCutSeedAndBlends(bitmapIndex, task.ColorBlends, out startSeed);
				else {
					task.ColorBlends = [];
					foreach (var kvp in colorBlends)
						task.ColorBlends[kvp.Key] = (Vector3[])kvp.Value.Clone();
					startSeed = startCutSeed; // otherwise we just use the pre-generated stuff we got previously, and just use the selected cut seed
					_AverageChildCount = AverageChildCount;
				}
				task.FinalColors.Clear();
				// Pre-iterate values that change the same way as iteration goes deeper, so they only get calculated once
				//var preIterateTask = task.PreIterate;
				if (/*preIterateTask == null || */task.PreIterate.Length != allocMaxIterations) {
					/*preIterateTask = */task.PreIterate = new (double, double, (double, double)[])[allocMaxIterations];
					//for (var i = 0; i < allocMaxIterations; task.PreIterate[i++] = (0.0, 0.0f, [])) { }
				}
				var inSize = refSize;
				// zooming into a non-center child needs to pre-iterate 6 levels deeper (or more precisely from 6 levels above)
				int totalMaxIterations = allocMaxIterations, PreSimulatedDepth = 0;
				//selectZoomChild > 0 ? applyMaxIterations : applyMaxIterations - 6;
				for (var i = 0; i < totalMaxIterations; ++i) {
					// get a new scale of the lower level, and also:
					// get the progress between splits (so that the parent wil smoothly turn into a preparation to split into children seamlessly)
					//var prei = preIterateTask[i];
					//prei.Item2 = Math.Log(task.ApplyDetail / (prei.Item1 = inSize)) / logBase;
					var inDetail = (float)Math.Log(task.ApplyDetail / inSize) / logBase;
					var inDetailSize = -inSize * Math.Max(-1, inDetail);
					//if (prei.Item3.Length == 0l || prei.Item3.Length < f.ChildCount)
					//	prei.Item3 = new (double, double)[f.ChildCount];
					// precalculate the children XY shifts, scaled with scale and collapsed into the parent's location when freshly spawned
					var xy = task.PreIterate.Length > i && task.PreIterate[i].Item3 != null && task.PreIterate[i].Item3.Length == f.ChildCount 
						? task.PreIterate[i].Item3 : new (double, double)[f.ChildCount];
					
					for (var c = 0; c < f.ChildCount; ++c)
#if SmoothnessDebugXy
						xy[c] = (f.ChildX[c] * inSize, f.ChildY[c] * inSize);
#else
						xy[c] = (f.ChildX[c] * inDetailSize, f.ChildY[c] * inDetailSize);
#endif
					task.PreIterate[i] = (inSize, inDetail, xy);
					if (inSize < task.ApplyDetail) {
						PreSimulatedDepth += i;
						// This is the final level, interpolate the detail here:
						foreach (var c in task.ColorBlends) {
							var interpolated = new Vector3[c.Value.Length];
							for (var di = 0; di < c.Value.Length; ++di) {
								interpolated[di] = allocPreviewMode
									? SampleColor(allocPalette, di * .5 + refHueAngle) // Preview just samples the pure palette
									: Vector3.Lerp(SampleColor(task.ColorBlends[c.Key], di + 2 * refHueAngle), SampleColor(allocPalette, di * .5 + refHueAngle), (float)inDetail); // otherwise we transform the pure palette into the children mix as we approach getting split
							}
							task.FinalColors[c.Key] = interpolated;
						}
						break;
					}
					inSize /= f.ChildSize;
				}
				double sX, sY;
				var zoomAngle = refAngle;
				var zoomSize = refSize; // Incorporating reference size
					// First three iterations into child 0
					for (var i = 0; i < 3; i++) {
						zoomAngle += ChildAngle[0];  // Accumulate rotation
						zoomSize /= f.ChildSize;     // Shrink for next step
					}
				// Compute infinite sum for zoomChild
				var infiniteSum = new Complex(f.ChildX[selectedZoomChild], -f.ChildY[selectedZoomChild])
					/ (new Complex(1.0, 0.0) - new Complex(Math.Cos(ChildAngle[selectedZoomChild]), Math.Sin(ChildAngle[selectedZoomChild])) / f.ChildSize);
				// Transform infinite sum into the new coordinate system
				var cosFinal = Math.Cos(-zoomAngle);
					var sinFinal = Math.Sin(-zoomAngle);
				// Calculate the location we are zooming into relative to the center, to shift it to the center
				sX = zoomSize * (cosFinal * infiniteSum.Real - sinFinal * infiniteSum.Imaginary);
				sY = zoomSize * (sinFinal * infiniteSum.Real + cosFinal * infiniteSum.Imaginary);
				
				if (allocCacheOpt) {
					// new cache optimizing algorithm:

					// TODO precompute allocs for full resolution image in GenerateAnimation:
					if (bitmapIndex >= previewFrames) {
						task.BinSize = allocBinSize;
						task.StripeHeight = allocStripeHeight;
						task.StripeCount = allocStripeCount;
					} else CalculateCache(task);
					
					/// <summary>
					/// Computes the area of a convex hull of a set of 2D points. Interior points are removed.
					/// </summary>
					float GetConvexHull() {
						List<Vector2> pts = [];
						// geometrically converge the scaling factor of the fractal "f"
						// refSize is the original scaling of distance from the fractal center to the center of the first child
						// childSize is how much is the next child's distance divided for the previous one
						int n = 0;
						for (var FractalAreaScale = refSize * f.ChildSize / (f.ChildSize - 1); n < f.ChildCount; ++n)
							pts.Add(new Vector2(
								(float)(FractalAreaScale * f.ChildX[n]),
								(float)(FractalAreaScale * f.ChildY[n])));
						if (n <= 1)
							return 0f;
						float ss = 0;
						foreach (var p in pts) {
							var d = p.Length();
							ss += d;
						}


							// Sort by X, then Y
							pts.Sort((p1, p2) => {
							int cx = p1.X.CompareTo(p2.X);
							return cx != 0 ? cx : p1.Y.CompareTo(p2.Y);
						});
						float Cross(Vector2 a, Vector2 b, Vector2 c)
							=> (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
						List<Vector2> lower = [];
						foreach (var p in pts) {
							while (lower.Count >= 2 &&
								   Cross(lower[^2], lower[^1], p) <= 0) {
								lower.RemoveAt(lower.Count - 1);
							}
							lower.Add(p);
						}
						List<Vector2> upper = [];
						for (int i = pts.Count - 1; i >= 0; i--) {
							var p = pts[i];
							while (upper.Count >= 2 &&
								   Cross(upper[^2], upper[^1], p) <= 0) {
								upper.RemoveAt(upper.Count - 1);
							}
							upper.Add(p);
						}
						// Remove duplicate endpoints
						lower.RemoveAt(lower.Count - 1);
						upper.RemoveAt(upper.Count - 1);
						List<Vector2> hull = [.. lower, .. upper];
						n = hull.Count;
						if (n < 3)
							return 0f;
						float sum = 0f;
						for (int i = 0; i < n; i++) {
							int j = (i + 1) % n;
							sum += hull[i].X * hull[j].Y - hull[j].X * hull[i].Y;
						}
						return sum / 2;
					}
					
					// Total number of dots divided by fractal convex hull area (or bitmap size if the area is smaller, but it shouldn't)
					float DotDensityPerPixel = (float)Math.Pow(_AverageChildCount, PreSimulatedDepth) / Math.Max(GetConvexHull(), task.ApplyWidth * task.ApplyHeight);
					// BinBytes = BinSize * (sizeof(float) * 2 + sizeof(long) + sizeof(byte)))
					// How many dots to be expected per stripe * 2 / bin capacity  // 2 to account for irregular distribution
					task.PredictedBinsPerStripe = (ushort)Math.Ceiling(6 * DotDensityPerPixel * task.StripeHeight * task.ApplyWidth / task.BinSize);
					// We will use OfDepth, if generating previews, or if we selected so and have enough allowed threads for that
					if (ofDepth) {
						// Of depth may split areas of bitmap to threads, so each thread will only acccess about a square root of thread count (at least)
						task.PredictedBinsPerStripe = (ushort)Math.Ceiling(task.PredictedBinsPerStripe / Math.Sqrt(allocMaxTasks));
						task.NewBin(task);
						tuples[0] = (dotsTaskIndex, (task.ApplyWidth * .5 - sX, task.ApplyHeight * .5 - sY), (refAngle, Math.Abs(refSpin) > 1 ? 2 * refAngle : 0), refColor, startSeed, 0);
						
						GenerateBufferOfDepth(bitmapIndex);
						foreach (var t in tasks)
							t.ClearBin();
					} else {
						task.NewBin(task);
						GenerateBufferSingleTask(task, (task.ApplyWidth * .5 - sX, task.ApplyHeight * .5 - sY), (refAngle, Math.Abs(refSpin) > 1 ? 2 * refAngle : 0), refColor, startSeed, 0);
						task.ClearBin();
					}
				} else {
					// original cacheless algorithm:
					if (ofDepth) {
						task.PredictedBinsPerStripe = (ushort)Math.Ceiling(task.PredictedBinsPerStripe / Math.Sqrt(allocMaxTasks));
						task.NewBin(task);
						// This was for the Grid implementation attempts, which ultimately didn't turn out to be faster than MultiDepth as I hoped for
						//ofDepthQueue.Clear();
						//ofDepthQueue.Enqueue((task.TaskIndex, (task.ApplyWidth * .5 - sX, task.ApplyHeight * .5 - sY), (refAngle, Math.Abs(refSpin) > 1 ? 2 * refAngle : 0), refColor, startSeed, 0));
						tuples[0] = (task.TaskIndex, (task.ApplyWidth * .5 - sX, task.ApplyHeight * .5 - sY), (refAngle, Math.Abs(refSpin) > 1 ? 2 * refAngle : 0), refColor, startSeed, 0);

						if (bitmapIndex >= previewFrames) {
							// Use a slightly slower but pixel perfect MultiDepth variant of OfDepth for final resolution images
							GenerateDotsOfDepth(bitmapIndex, true, applyDot);
							// OfDepth buffer merge
							var maxGenerationTasks = (short)Math.Max(1, allocMaxTasks - 1);
							var po = new ParallelOptions {
								MaxDegreeOfParallelism = maxGenerationTasks,
								CancellationToken = token
							};
							try {
								var result = Parallel.For(0, task.ApplyHeight, po, y => {
									for (var t = 0; t < allocMaxTasks; ++t) {
										if (buffer != null) {
											var tempBuffY = buffer[t][y];
											var buffY = buffT[y];
											for (var x = 0; x < task.ApplyWidth; ++x)
												buffY[x] += tempBuffY[x];
										}
									}
								});
								while (!result.IsCompleted)
									Thread.Sleep(10);
							} catch (Exception) {
								if (state != null) state.State = TaskState.Done;
								return;
							}
						} else {
							// Use a faster pixel racing variant of OfDepth for low-res previews
							GenerateDotsOfDepth(bitmapIndex, false, applyDot);
						}
					} else {
						task.UseBuffer = buffT;
						GenerateDotsSingleTask(task, task, (task.ApplyWidth * .5 - sX, task.ApplyHeight * .5 - sY), (refAngle, Math.Abs(refSpin) > 1 ? 2 * refAngle : 0), refColor, startSeed, 0, applyDot);
					}
				}

				// Increment the animation parameters (rotate and zoom a little further), but only for the actual animation, not between previews
				if (bitmapIndex >= previewFrames)
					IncFrameParameters(ref refSize, ref refAngle, ref refHueAngle, refSpin, allocBlur);
				if (!IsCancelRequested())
					continue;
				if (state != null) state.State = TaskState.Done;
				return;
			}
#if CustomDebug
			iterTime.Stop();
			Log(ref logTask, "Iter:" + task.BitmapIndex + " time = " + iterTime.Elapsed.TotalMilliseconds + " ms.");
			Monitor.Enter(taskLock);
			try {
				//initTimes += initTime.Elapsed.TotalMilliseconds;
				//iterTimes += iterTime.Elapsed.TotalMilliseconds;
				Log(ref logString, logTask);
			} finally { Monitor.Exit(taskLock); }
#endif
			if (state != null) // OfAnimation - continue directly with the next steps such as void and gif in this same task:
				GenerateImage(task);
			else // OfDepth - start continuation in a new task:
				task.Start(bitmapIndex, () => GenerateImage(task));
			#endregion

			#region GenerateDots_Inline
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			bool TestShapeSize(FractalTask dotsTask, double dotsX, double dotsY, double dotsSize) {
				// tests whether at least a part of this shape is within the image, if not, it will get skipped
				var testSize = dotsSize * f.CutSize;
				return Math.Min(dotsX, dotsY) + testSize > dotsTask.UpLeftStart 
					&& dotsX - testSize < dotsTask.RightEnd 
					&& dotsY - testSize < dotsTask.DownEnd;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			bool TestDotSize(FractalTask dotsTask, double dotsX, double dotsY, double dotsSize) {
				// tests whether at least a part of this shape is within the image, if not, it will get skipped
				var testSize = dotsSize * f.CutSize;
				return Math.Min(dotsX, dotsY) > dotsTask.UpLeftStart
					&& dotsX < dotsTask.RightEnd
					&& dotsY < dotsTask.DownEnd;
			}
			#endregion

			#region GenerateDots_NoCache
			/*void GenerateBufferSingleTask_NoCache(FractalTask dotsTask, Vector3[][] dotsBuffT,
				(double, double) dotsXy, (double, double) dotsAngle, byte dotsColor, long dotsFlags, byte dotsDepth
			) {
				var preIterated = dotsTask.PreIterate[dotsDepth]; // PreIterate = (dotsSize, dotsDetail, f.ChildX[i] * dotsSize, f.ChildY[i] * dotsSize)
				var newPreIterated = dotsTask.PreIterate[++dotsDepth]; // the same thing, but one depth level deeper (newDotsSize, newDotsDetail, f.ChildX[i] * newDotsSize, f.ChildY[i] * newDotsSize)
				if (newPreIterated.Item1 < dotsTask.ApplyDetail) {
					// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
					for (var i = 0; i < f.ChildCount; ++i) {
						if (IsCancelRequested())
							return;
						// Special Cutoff (uses a special algorithm to pick some children to disappear)
						var newFlags = CalculateFlags(i, dotsFlags);
						if (newFlags < 0)
							continue;
						// Take the pre-iterated xy offset at this depth level
						var xy = preIterated.Item3[i];
						var newXy = NewXY(dotsXy, xy, dotsAngle.Item1);
						// Outside View check, if inside view, it will continue iterating this child
						if (TestSize(dotsTask, newXy.Item1, newXy.Item2, preIterated.Item1))
							dotsTask.ApplyDot(dotsBuffT, (i + f.ChildCount * (newFlags & (((long)1 << f.ChildCount) - 1)), (float)newXy.Item1, (float)newXy.Item2,
								allocPreviewMode && dotsDepth > 1 ? dotsColor : (byte)((dotsColor + ChildColor[i]) % allocPalette2)));
					}
					return;
				}
				// Split parent deeper into new smaller parents
				for (var i = 0; i < f.ChildCount; ++i) {
					if (IsCancelRequested())
						return;
					// Special Cutoff (uses a special algorithm to pick some children to disappear)
					var newFlags = CalculateFlags(i, dotsFlags);
					if (newFlags < 0)
						continue;
					// Take the pre-iterated xy offset at this depth level
					var xy = preIterated.Item3[i];
					var newXy = NewXY(dotsXy, xy, dotsAngle.Item1);
					// Outside View check, if inside view, it will continue iterating this child
					if (TestSize(dotsTask, newXy.Item1, newXy.Item2, preIterated.Item1))
						GenerateBufferSingleTask_NoCache(dotsTask, dotsBuffT, newXy,
							i == 0
							? (dotsAngle.Item1 + ChildAngle[i] - dotsAngle.Item2, -dotsAngle.Item2)
							: (dotsAngle.Item1 + ChildAngle[i], SelectedSpin == 3 ? -dotsAngle.Item2 : dotsAngle.Item2),
							//: (dotsAngle.Item1 + ChildAngle[i], dotsAngle.Item2),
							allocPreviewMode && dotsDepth > 1 ? dotsColor : (byte)((dotsColor + ChildColor[i]) % allocPalette2), newFlags, dotsDepth);
				}
			}*/
			/*void GenerateDotsOfDepth_NoCache(int dotsBitmapIndex, bool MultiDepth) {
				int index = 0, insertTo = 1,
				max = allocMaxTasks * DepthDiv,
				maxCount = max - f.ChildCount - 1,
				count = (max + insertTo) % max;
				// keep splitting parents until we have a queue DivDepth * maxTasks large:
				FractalTask tupleTask = null;
				while (count > 0 && count < maxCount) {
					// take a parent form the queue to split into move parent to put back into the queue
					var (tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = tuples[index++];
					index %= max;
					tupleTask = tasks[tupleIndex];
					var preIterated = tupleTask.PreIterate[tupleDepth]; // PreIterate = (dotsSize, dotsDetail, f.ChildX[i] * dotSize, f.ChildY[i] * dotsSize)
					var newPreIterated = tupleTask.PreIterate[++tupleDepth]; // the same thing, but one depth level deeper
					if (newPreIterated.Item1 < tupleTask.ApplyDetail) {
						// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
						for (var i = 0; i < f.ChildCount; ++i) {
							if (IsCancelRequested())
								return;
							// Special Cutoff
							var newFlags = CalculateFlags(i, tupleFlags);
							if (newFlags < 0)
								continue;
							// Outside View
							var xy = preIterated.Item3[i]; // pre-iterated (f.ChildX[i] * dotsSize, f.ChildY[i] * dotsSize), since it's preiterated
							var newXy = NewXY(tupleXy, xy, tupleAngle.Item1);
							if (TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
								// DIFFERENCE: NoCache calls tupleTask.ApplyDot(tupleTask.Buffer, dot) instead of tupleTask.AddDot(dot)
								tupleTask.ApplyDot(tupleTask.Buffer, (i + f.ChildCount * (newFlags & ((1 << f.ChildCount) - 1)), (float)newXy.Item1, (float)newXy.Item2,
									allocPreviewMode && tupleDepth > 1 ? tupleColor : (byte)((tupleColor + ChildColor[i]) % allocPalette2)));
								// ENDDIFFERENCE
						}
						count = (max + insertTo - index) % max;
						continue;
					}
					// Split parent deeper into new smaller parents
					for (var i = 0; i < f.ChildCount; ++i) {
						if (IsCancelRequested())
							return;
						// Special Cutoff
						var newFlags = CalculateFlags(i, tupleFlags);
						if (newFlags < 0)
							continue;
						// Outside View
						var xy = preIterated.Item3[i];
						var newXy = NewXY(tupleXy, xy, tupleAngle.Item1);
						if (!TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
							continue;
						tuples[insertTo++] =
							(tupleIndex, newXy,
							i == 0
							? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2)
							: (tupleAngle.Item1 + ChildAngle[i], SelectedSpin == 3 ? -tupleAngle.Item2 : tupleAngle.Item2),
							allocPreviewMode && tupleDepth > 1 ? tupleColor : (byte)((tupleColor + ChildColor[i]) % allocPalette2), newFlags, tupleDepth);
						insertTo %= max; // we have added the new parent into the queue
					}
					// refresh the count before we ask if this count is enough
					count = (max + insertTo - index) % max;
				}
				// we now have a nice number of tasks to perform in parallel, so do that:
				// false argument makes sure that we only finish processing these iteration tasks, we can exit if other kinds of tasks are still running as we don't need to wait for these here
				FinishTasks(false, false, taskIndex => {
					// until the queue is empty
					if (count <= 0)
						return false;
					// take a parent from queue and iterate it normally with single task code, but as its own parallel task
					var tempTupleIndex = index++;
					tasks[taskIndex].Start(dotsBitmapIndex, () => {
						var (tupleBufferIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = tuples[tempTupleIndex];
						// DIFFERENCE: NoCache doesn't call any Bin stuff, and calls Single_NoCache instead:
						GenerateBufferSingleTask_NoCache(tupleBufferIndex, MultiDepth ? buffer[taskIndex] : task.Buffer, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth);
						// ENDDIFFERENCE
						tasks[taskIndex].State = TaskState.Done;
					});
					index %= max;
					count = (max + insertTo - index) % max;
					return true;
				});
			}*/
			#endregion

			#region GenerateDots_Cache
			void GenerateBufferSingleTask(FractalTask dotsTask,
				(double, double) dotsXy, (double, double) dotsAngle, byte dotsColor, long dotsFlags, byte dotsDepth
				) {
				dotsTask.UseBuffer = dotsTask.Buffer;
				GenerateDotsSingleTask(dotsTask, dotsTask, dotsXy, dotsAngle,dotsColor,dotsFlags,dotsDepth, addDot);
				dotsTask.DrawStripesOfAnimation();
			}
			void GenerateDotsSingleTask<TApplier>(FractalTask mainTask, FractalTask binTask,
			(double, double) dotsXy, (double, double) dotsAngle, byte dotsColor, long dotsFlags, byte dotsDepth, TApplier applier
			) where TApplier : struct, FractalTask.IDotApplier {
				var preIterated = mainTask.PreIterate[dotsDepth]; // PreIterate = (dotsSize, dotsDetail, f.ChildX[i] * dotsSize, f.ChildY[i] * dotsSize)
				var newPreIterated = mainTask.PreIterate[++dotsDepth]; // the same thing, but one depth level deeper (newDotsSize, newDotsDetail, f.ChildX[i] * newDotsSize, f.ChildY[i] * newDotsSize)
				if (newPreIterated.Item1 < mainTask.ApplyDetail) {
					// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
					for (var i = 0; i < f.ChildCount; ++i) {
						if (IsCancelRequested())
							return;
						// Special Cutoff (uses a special algorithm to pick some children to disappear)
						var newFlags = CalculateFlags(i, dotsFlags);
						if (newFlags < 0)
							continue;
						// Take the pre-iterated xy offset at this depth level
						var xy = preIterated.Item3[i];
						var newXy = NewXY(dotsXy, xy, dotsAngle.Item1);
						// Outside View check, if inside view, it will continue iterating this child
						if (TestDotSize(mainTask, newXy.Item1, newXy.Item2, preIterated.Item1))
							applier.Apply(binTask, (i + f.ChildCount * (newFlags & (((long)1 << f.ChildCount) - 1)), (float)newXy.Item1, (float)newXy.Item2,
								allocPreviewMode && dotsDepth > 1 ? dotsColor : (byte)((dotsColor + ChildColor[i]) % allocPalette2)));
					}
					return;
				}
				// Split parent deeper into new smaller parents
				for (var i = 0; i < f.ChildCount; ++i) {
					if (IsCancelRequested())
						return;
					// Special Cutoff (uses a special algorithm to pick some children to disappear)
					var newFlags = CalculateFlags(i, dotsFlags);
					if (newFlags < 0)
						continue;
					// Take the pre-iterated xy offset at this depth level
					var xy = preIterated.Item3[i];
					var newXy = NewXY(dotsXy, xy, dotsAngle.Item1);
					// Outside View check, if inside view, it will continue iterating this child
					if (TestShapeSize(mainTask, newXy.Item1, newXy.Item2, preIterated.Item1))
						GenerateDotsSingleTask(mainTask, binTask, newXy,
							i == 0
							? (dotsAngle.Item1 + ChildAngle[i] - dotsAngle.Item2, -dotsAngle.Item2)
							: (dotsAngle.Item1 + ChildAngle[i], SelectedSpin == 3 ? -dotsAngle.Item2 : dotsAngle.Item2),
							//: (dotsAngle.Item1 + ChildAngle[i], dotsAngle.Item2),
							allocPreviewMode && dotsDepth > 1 ? dotsColor : (byte)((dotsColor + ChildColor[i]) % allocPalette2), newFlags, dotsDepth, applier);
				}
			}
			void GenerateBufferOfDepth(int dotsBitmapIndex) {
				GenerateDotsOfDepth(dotsBitmapIndex, false, addDot);
				ushort pass = 0;
				ushort stripeId = 0;
				ushort passMod = (ushort)(1 + Math.Ceiling((2 * (task.Bloom1 + 1)) / task.StripeHeight));

				FinishTasks(false, false, taskIndex => {
					// until the queue is empty
					if (pass >= passMod)
						return false;

					var drawStripeId = stripeId;
					// take a parent from queue and iterate it normally with single task code, but as its own parallel task
					tasks[taskIndex].Start(dotsBitmapIndex, () => {
						FractalTask.DrawStripeOfDepth(tasks, task, drawStripeId);
						tasks[taskIndex].State = TaskState.Done;
					});
					stripeId += passMod;
					if (stripeId >= task.StripeCount)
						stripeId = ++pass;
					return true;
				});
			}
			void GenerateDotsOfDepth<TApplier>(int dotsBitmapIndex, bool MultiDepth, TApplier applier
			) where TApplier : struct, FractalTask.IDotApplier {
				int index = 0, insertTo = 1,
					max = allocMaxTasks * DepthDiv,
					maxCount = max - f.ChildCount - 1,
					count = (max + insertTo) % max;
				// keep splitting parents until we have a queue DivDepth * maxTasks large:
				var tupleTask = tasks[0];
				while (count > 0 && count < maxCount) {
					// take a parent form the queue to split into move parent to put back into the queue
					var (tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = tuples[index++];
					index %= max;
					tupleTask = tasks[tupleIndex];// tasks[tupleIndex];
					var preIterated = tupleTask.PreIterate[tupleDepth]; // PreIterate = (dotsSize, dotsDetail, f.ChildX[i] * dotSize, f.ChildY[i] * dotsSize)
					var newPreIterated = tupleTask.PreIterate[++tupleDepth]; // the same thing, but one depth level deeper
					if (newPreIterated.Item1 < tupleTask.ApplyDetail) {
						// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
						for (var i = 0; i < f.ChildCount; ++i) {
							if (IsCancelRequested())
								return;
							// Special Cutoff
							var newFlags = CalculateFlags(i, tupleFlags);
							if (newFlags < 0)
								continue;
							// Outside View
							var xy = preIterated.Item3[i]; // pre-iterated (f.ChildX[i] * dotsSize, f.ChildY[i] * dotsSize), since it's preiterated
							var newXy = NewXY(tupleXy, xy, tupleAngle.Item1);
							if (TestDotSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
								applier.Apply(tupleTask, (i + f.ChildCount * (newFlags & ((1 << f.ChildCount) - 1)), (float)newXy.Item1, (float)newXy.Item2,
									allocPreviewMode && tupleDepth > 1 ? tupleColor : (byte)((tupleColor + ChildColor[i]) % allocPalette2)));
						}
						count = (max + insertTo - index) % max;
						continue;
					}
					// Split parent deeper into new smaller parents
					for (var i = 0; i < f.ChildCount; ++i) {
						if (IsCancelRequested())
							return;
						// Special Cutoff
						var newFlags = CalculateFlags(i, tupleFlags);
						if (newFlags < 0)
							continue;
						// Outside View
						var xy = preIterated.Item3[i];
						var newXy = NewXY(tupleXy, xy, tupleAngle.Item1);
						if (!TestShapeSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
							continue;
						tuples[insertTo++] =
							(tupleIndex, newXy,
							i == 0 
							? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2) 
							: (tupleAngle.Item1 + ChildAngle[i], SelectedSpin == 3 ? -tupleAngle.Item2 : tupleAngle.Item2),
							allocPreviewMode && tupleDepth > 1 ? tupleColor : (byte)((tupleColor + ChildColor[i]) % allocPalette2), newFlags, tupleDepth);
						insertTo %= max; // we have added the new parent into the queue
					}
					// refresh the count before we ask if this count is enough
					count = (max + insertTo - index) % max;
				}
				// we now have a nice number of tasks to perform in parallel, so do that:
				// false argument makes sure that we only finish processing these iteration tasks, we can exit if other kinds of tasks are still running as we don't need to wait for these here
				FinishTasks(false, false, taskIndex => {
					// until the queue is empty
					if (count <= 0)
						return false;
					// take a parent from queue and iterate it normally with single task code, but as its own parallel task
					var tempTupleIndex = index++;
					tasks[taskIndex].Start(dotsBitmapIndex, () => {
						var (tupleBufferIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = tuples[tempTupleIndex];
						if (allocCacheOpt) {
							tasks[taskIndex].NewBin(tupleTask);
							var bufferTask = tasks[taskIndex];
							bufferTask.UseBuffer = task.Buffer;
							GenerateDotsSingleTask(tupleTask, tasks[taskIndex], tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth, addDot);
						} else {
							var bufferTask = tasks[tupleBufferIndex];
							if (buffer == null)
								return;
							bufferTask.UseBuffer = MultiDepth ? buffer[taskIndex] : task.Buffer;
							GenerateDotsSingleTask(bufferTask, bufferTask, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth, applyDot);
						}
						
						tasks[taskIndex].State = TaskState.Done;
					});
					index %= max;
					count = (max + insertTo - index) % max;
					return true;
				});
			}
			#endregion

			
		}
		void GenerateImage(FractalTask task) {
			var graphics = GraphicsDevice.GetDefault();
			ReadWriteBuffer<int>
#if NULLABLE
?
#endif
				v = null;
			void DisposeDraw() {
				// if we get cancelled we should dispose this buffer if we are still keeping it for GpuDraw
				v?.Dispose();
				task.State = TaskState.Done;
			}
#if CustomDebug
			var logTask = "";
			Stopwatch voidTime = new();
			voidTime.Start();
#endif
			// Generate the grey void areas
			bitmapState[task.BitmapIndex] = BitmapState.Void;
			var voidT = task.VoidDepth;
			var buffT = task.Buffer;
			var queueT = task.VoidQueue;
			task.LightNormalizer = 0.1f;
			uint queueS = 0, queueE = 0;
			int i = 0;
			Vector3 buffI;
			float max;
			bool IsAmbient = false;
			// find the highest seed value
			float GetMax(int i) {
				buffI = buffT[i];
				max = Math.Max(buffI.X, Math.Max(buffI.Y, buffI.Z));
				task.LightNormalizer = Math.Max(task.LightNormalizer, max);
				return max;
			}
			// Void Depth:
			if (allocGpuVoidType > GpuVoidType.CPU) {
				var voidTN = task.VoidDepthN;
				int seeds = 0;
				int maxSize = Math.Max(task.ApplyWidth, task.ApplyHeight) + 1;
				// prefill the buffer and also figure out if it's voidless
				seeds += task.ApplyWidth * 2 + task.ApplyHeight * 2 - 4; // add all edges
				for (int x = i + task.ApplyWidth; i < x; voidTN[i] = voidT[i++] = 0)
					GetMax(i); // top edge
				for (int y = i + task.ApplyWidth * (task.ApplyHeight - 2); i < y; voidTN[i] = voidT[i++] = 0) {
					// if (IsCancelRequested()) { task.State = TaskState.Done; return; }
					GetMax(i); voidTN[i] = voidT[i++] = 0; // left edge
					int x = i + task.ApplyWidth - 2; // insides
					while (i < x)
						if (GetMax(i) > 0) {
							voidTN[i] = voidT[i++] = 0; ++seeds;
						} else voidTN[i] = voidT[i++] = maxSize;
					GetMax(i); // right edge
				}
				for (int x = i + task.ApplyWidth; i < x; voidTN[i] = voidT[i++] = 0)
					GetMax(i); // bottom edge
				if (seeds < task.ApplyWidth * task.ApplyHeight) {
					IsAmbient = true;
					// if there was at least 1 pixel of void, do the BFS:
					task.VoidDepthMax = maxSize;
					int innerWidth = task.ApplyWidth - 2;
					int innerHeight = task.ApplyHeight - 2;
					var outB = new int[1] { 1 };
					// Wrap into GPU buffers
					v = graphics.AllocateReadWriteBuffer(voidT);
					var bufferVoidN = graphics.AllocateReadWriteBuffer(voidTN);
					var outMaxBuffer = graphics.AllocateReadWriteBuffer(outB);
					static void Swap<T>(ref T a, ref T b) => (b, a) = (a, b);
					void BFS() {
						outB[0] = 1;
						outMaxBuffer.CopyFrom(outB);
						graphics.For(innerWidth, innerHeight, new VoidBfs(v, bufferVoidN, outMaxBuffer, task.ApplyWidth));
						Swap(ref v, ref bufferVoidN);
						outMaxBuffer.CopyTo(outB);
						task.VoidDepthMax = outB[0];
					}
					void DisposeBuffers() {
						bufferVoidN.Dispose();
						outMaxBuffer.Dispose();
						v.Dispose();
						task.State = TaskState.Done;
					}
					if (allocGpuVoidType >= GpuVoidType.VoidJumpFlood) {
						if (allocGpuVoidType > GpuVoidType.VoidJumpBoundless) {
							for (int step = Math.Min(innerWidth, innerHeight) / 3; step > 1; step = step * 3 / 5) {
								if (IsCancelRequested()) {
									DisposeBuffers();
									return;
								}
								int w = task.ApplyWidth - 2 * step;
								int h = task.ApplyHeight - 2 * step;
								graphics.For(w, h, new JumpFloodBoundlessOptimized(v, bufferVoidN, task.ApplyWidth, step));
								Swap(ref v, ref bufferVoidN);
							}
							BFS();
						} else {
							// this works perfectly well, exactly like the boundless, but also has the edges fixed, looks indistinguishable from !GPU_JumpFlood:
							for (int step = Math.Min(innerWidth, innerHeight) / 3; step > 1; step = step * 3 / 5) {
								if (IsCancelRequested()) {
									DisposeBuffers();
									return;
								}
								graphics.For(innerWidth, innerHeight, new JumpFlood(v, bufferVoidN, task.ApplyWidth, task.ApplyHeight, step));
								Swap(ref v, ref bufferVoidN);
							}
						}
						BFS();
					} else {
						while (maxSize == task.VoidDepthMax) {
							if (IsCancelRequested()) {
								DisposeBuffers();
								return;
							}
							BFS();
						}
					}
					if (allocGpuDrawType == GpuDrawType.CPU) {
						// Copy and dispose if drawing will need to acess regular voidT, otherwise it will jsut reuse the buffer directly
						v.CopyTo(voidT);
						v.Dispose();
					}
					bufferVoidN.Dispose();
					outMaxBuffer.Dispose();
				}
			} else {
				task.VoidDepthMax = 1.0f;
				short w1 = (short)(task.ApplyWidth - 1), h1 = (short)(task.ApplyHeight - 1);
				if (SelectedAmbient > 0) {
					// Void Depth Seed points (no points, no borders), let the void depth generator know where to start incrementing the depth
					short voidMax = 0;
					for (int x = i + task.ApplyWidth; i < x; voidT[i++] = 0) {
						GetMax(i); queueT[queueE++] = i; // top edge
					}
					for (int y = i + task.ApplyWidth * (task.ApplyHeight - 2); i < y; voidT[i++] = 0) {
						// if (IsCancelRequested()) { task.State = TaskState.Done; return; }
						GetMax(i); queueT[queueE++] = i; voidT[i++] = 0; // left edge
						int x = i + task.ApplyWidth - 2; // insides
						while (i < x) {
							if (GetMax(i) > 0) {
								queueT[queueE++] = i;
								voidT[i++] = 0;
							} else voidT[i++] = -1;
						}
						GetMax(i);  // right edge
						queueT[queueE++] = i;
					}
					for (int x = i + task.ApplyWidth; i < x; voidT[i++] = 0) {
						GetMax(i); // bottom edge
						queueT[queueE++] = i;
					}
					// Depth of Void (fill the void with incrementally larger values of depth, that will generate the grey areas)
					if (queueE < task.ApplyWidth * task.ApplyHeight) {
						IsAmbient = true;
						// if fully filled then there are no voids at all, and we don't even need to dequeue anything
						while (queueS < queueE) {
							i = queueT[queueS++];
							//short ym = (short)(y - 1), yp = (short)(y + 1), xm = (short)(x - 1), xp = (short)(x + 1);
							int ip, voidYx = voidT[i] + 1;
							voidMax = Math.Max(voidMax, (short)voidYx);
							if ((ip = i + 1) % task.ApplyWidth > 0 && voidT[ip] == -1) { voidT[ip] = voidYx; queueT[queueE++] = ip; }
							if (i % task.ApplyWidth > 0 && voidT[ip = i - 1] == -1) { voidT[ip] = voidYx; queueT[queueE++] = ip; }
							if ((ip = i + task.ApplyWidth) < voidT.Length && voidT[ip] == -1) { voidT[ip] = voidYx; queueT[queueE++] = ip; }
							if ((ip = i - task.ApplyWidth) >= 0 && voidT[ip] == -1) { voidT[ip] = voidYx; queueT[queueE++] = ip; }
						}
					}
					task.VoidDepthMax = voidMax;
				} else
					for (i = task.ApplyHeight * task.ApplyHeight; 0 <= --i;) {
						buffI = buffT[i];
						task.LightNormalizer = Math.Max(task.LightNormalizer, Math.Max(buffI.X, Math.Max(buffI.Y, buffI.Z)));
						/*for (short x = 0; x < task.ApplyWidth; ++x) {
							var tempBuffYx = buffY[x];
							task.LightNormalizer = Math.Max(task.LightNormalizer, Math.Max(tempBuffYx.X, Math.Max(tempBuffYx.Y, tempBuffYx.Z)));
						}*/
					}
			}
			task.LightNormalizer = SelectedBrightness * 2.55f / task.LightNormalizer;
			if (IsCancelRequested()) {
				DisposeDraw();
				return;
			}
#if CustomDebug
			voidTime.Stop();
			Log(ref logTask, "Void:" + task.BitmapIndex + " time = " + voidTime.Elapsed.TotalMilliseconds + " ms.");
			Stopwatch drawTime = new();
			drawTime.Start();
#endif
			// Make a locked bitmap, remember the locked state
			bitmap[task.BitmapIndex]?.Dispose();
			var bmp = bitmap[task.BitmapIndex] = new(task.ApplyWidth, task.ApplyHeight);
			bitmapState[task.BitmapIndex] = BitmapState.Drawing;
			nint bytes = LockBits(task.BitmapIndex, 
				(/*allocParallelType == ParallelType.OfDepth ? task.BitmapIndex : task.BitmapIndex + 1*/ task.BitmapIndex) < previewFrames 
				? new Rectangle(0, 0, task.ApplyWidth, task.ApplyHeight) : rect).Scan0;
			//var vw = task.ApplyHeight / allocVoid + 2;
			int DrawType = allocNoise > 0 && allocGenerationType != GenerationType.HashSeeds && IsAmbient 
				? 0 : (IsAmbient ? 4 : 8);
			if (allocGpuDrawType == GpuDrawType.CPU) {
				if (task.VoidNoise is not Vector3[] voidNoise)
					throw new("voidNoise is null");
				// Draw the generated pixel to bitmap data with CPU
				unsafe {
					var p = (byte*)(void*)bytes;
					if (DrawType < 4) {
						// We are using Noise, so prepare the grid values for it:
						for (var n = 0; n < noiseWidth * noiseHeight; ++n)
							voidNoise[n] = new Vector3(random.Next(allocNoise), random.Next(allocNoise), random.Next(allocNoise));
						// and make it so we call the Noise function inside the loop, otherwise NoNoise
						rowFunc = &Noise;
					} else rowFunc = DrawType == 0 ? &NoAmbient : &NoNoise;
					// Draw the bitmap with the buffer that we calculated with GenerateFractal and Calculate void
					var stride = bitmapData[task.BitmapIndex].Stride;
					// switch whether we use or don't use Saturate
					delegate*<Vector3, double, Vector3> saturateFunc = SelectedSaturate > 0.0f ? &ApplySaturate : &Identity;
					// Row loop (single-threaded):
					for (short y = 0; y < task.ApplyHeight; ++y) {
						if (IsCancelRequested())
							break;
						byte* rp = p + y * stride;
						rowFunc(task, y * task.ApplyWidth, ref rp, this);
					}
				}
			} else {
				if (task.VoidNoiseF is not Float3[] voidNoiseF)
					throw new("voidNoiseF is null");
				// Draw the generated pixels to bitmap data with GPU
				ReadOnlyBuffer<Float3>
#if NULLABLE
?
#endif
					n = null;
				if (DrawType < 4) {
					unsafe {
						for (var a = 0; a < noiseWidth * noiseHeight; ++a)
							voidNoiseF[a] = new Float3(random.Next(allocNoise), random.Next(allocNoise), random.Next(allocNoise));
					}
					n = graphics.AllocateReadOnlyBuffer<Float3>(voidNoiseF);
				}
				// Output texture (same size as your bitmap)
				var o = graphics.AllocateReadWriteBuffer<int>(bmp.Width * bmp.Height);
				// Reinterpret task.Buffer as Float3[]
				var b = graphics.AllocateReadOnlyBuffer<Float3>(MemoryMarshal.Cast<Vector3, Float3>(task.Buffer.AsSpan()));

				if (allocGpuVoidType == GpuVoidType.CPU && IsAmbient)
					v = graphics.AllocateReadWriteBuffer(task.VoidDepth);
				Int2 np = new Int2(noiseWidth, allocVoid);
				if (allocGpuDrawType == GpuDrawType.Functions) {
					// GPU Type: Switch functions
					if (SelectedSaturate <= 0.0f)
						DrawType += 2;
					if (!allocDithering)
						++DrawType;
					switch (DrawType) {
						case 0:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawNoiseSaturateDither(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax, n!, np, SelectedSaturate)); break;
						case 1:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawNoiseSaturate(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax, n!, np, SelectedSaturate)); break;
						case 2:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawNoiseDither(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax, n!, np)); break;
						case 3:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawNoise(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax, n!, np)); break;
						case 4:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawAmbientSaturateDither(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax, SelectedSaturate)); break;
						case 5:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawAmbientSaturate(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax, SelectedSaturate)); break;
						case 6:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawAmbientDither(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax)); break;
						case 7:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawAmbient(o, b,
							task.ApplyWidth, task.LightNormalizer, v!, SelectedAmbient, task.VoidDepthMax)); break;

							// Without ambient (lacking voidDepth):
						case 8:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawSaturateDither(o, b,
							task.ApplyWidth, task.LightNormalizer, SelectedSaturate)); break;
						case 9:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawSaturate(o, b,
							task.ApplyWidth, task.LightNormalizer, SelectedSaturate)); break;
						case 10:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new DrawDither(o, b,
							task.ApplyWidth, task.LightNormalizer)); break;
						case 11:
							graphics.For(task.ApplyWidth, task.ApplyHeight, new Draw(o, b,
							task.ApplyWidth, task.LightNormalizer)); break;
					}
				} else {
					// GPU Type: Multi-pass pipeline
					var ping = graphics.AllocateReadWriteBuffer<Float3>(/*task.VoidNoiseF*/bmp.Width * bmp.Height);
					if (SelectedSaturate <= 0.0f)
						graphics.For(bmp.Width * bmp.Height, new PipeNormalize(ping, b, task.ApplyWidth, task.LightNormalizer));
					else graphics.For(bmp.Width * bmp.Height, new PipeNormalizeSaturate(ping, b, task.ApplyWidth, task.LightNormalizer, SelectedSaturate));
					if (v != null) {
						if (DrawType < 4)
							graphics.For(task.ApplyWidth, task.ApplyHeight, new PipeNoise(ping, task.ApplyWidth, v, SelectedAmbient, task.VoidDepthMax, n!, np));
						else
							graphics.For(task.ApplyWidth, task.ApplyHeight, new PipeAmbient(ping, task.ApplyWidth, v, SelectedAmbient, task.VoidDepthMax));
					}
					if (allocDithering)
						graphics.For(task.ApplyWidth, task.ApplyHeight, new PipeBytesDither(ping, o, task.ApplyWidth));
					else graphics.For(task.ApplyWidth, task.ApplyHeight, new PipeBytes(ping, o, task.ApplyWidth));
					ping.Dispose();
				}
				// Marshal the data into bitmap
				var ints = MemoryMarshal.Cast<int, byte>(o.ToArray().AsSpan());
				Marshal.Copy(ints.ToArray(), 0, bytes, ints.Length);
				// finally dispose the buffer to not leak memory
				v?.Dispose();
				n?.Dispose();
				b?.Dispose();
				o?.Dispose();
			}
			
			if (IsCancelRequested()) {
				task.State = TaskState.Done;
				return;
			}
#if CustomDebug
			drawTime.Stop();
			Log(ref logTask, "Draw:" + task.BitmapIndex + " time = " + drawTime.Elapsed.TotalMilliseconds + " ms.");
			Monitor.Enter(taskLock);
			try {
				//voidTimes += voidTime.Elapsed.TotalMilliseconds;
				//drawTimes += drawTime.Elapsed.TotalMilliseconds;
				Log(ref logString, logTask);
			} finally { Monitor.Exit(taskLock); }
#endif
			bitmapState[task.BitmapIndex] = BitmapState.DrawingFinished;
			if (task.BitmapIndex < previewFrames) {
				TryFinishBitmap(false, task.TaskIndex);
				task.State = TaskState.Done;
			} else {
				if (allocGifType != GifType.No) {
					encodedGif[task.BitmapIndex] = 1;
					GenerateGif(task);
				} else {
					task.State = TaskState.Done;
				}
			}
		}
#endregion

		#region AnimationParams
		void ModFrameParameters(short width, short height, ref double refSize, ref double refAngle, ref short refSpin, ref byte refColor, ref double refHueAngle) {
			var w = Math.Max(width, height) * f.MaxSize;
			var fp = f.ChildSize;
			if (allocPreviewMode)
				w *= 0.1;
			// Make sure the fractal is big enough to fill the screen even when I shift it to focus on the zoomChild
			if (selectedZoomChild > 0)
				w *= fp * fp * fp;// * fp * fp * fp;
								  // Modulo rotation
			while (refAngle > Math.PI * 2)
				refAngle -= Math.PI * 2;
			while (refAngle < 0)
				refAngle += Math.PI * 2;
			// Modulo hue shift
			var paletteLength = allocPalette.Length;
			while (refHueAngle >= paletteLength)
				refHueAngle -= paletteLength;
			while (refHueAngle < 0)
				refHueAngle += paletteLength;
			// Swap Parent<->CenterChild after a full period
			while (refSize >= w * fp) {
				// Grown by f.childSize, swap parent to it's child
				refSize /= fp;
				if (allocPreviewMode)
					continue;
				refAngle += selectedZoomChild == 0 ? ChildAngle[selectedZoomChild] : -ChildAngle[selectedZoomChild];
				SwitchParentChild(ref refAngle, ref refSpin, ref refColor, 1);
			}
			while (refSize < w) {
				// Shrank by f.childSize, swap child to it's parent
				refSize *= fp;
				if (allocPreviewMode)
					continue;
				refAngle += selectedZoomChild == 0 ? -ChildAngle[selectedZoomChild] : ChildAngle[selectedZoomChild];
				SwitchParentChild(ref refAngle, ref refSpin, ref refColor, -1);
			}

			return;

			void SwitchParentChild(ref double refAngle, ref short refSpin, ref byte refColor, short zoomDir) {
				if (allocPreviewMode)
					return;
				refColor = (byte)((allocPalette2 + refColor + zoomDir * ChildColor[selectedZoomChild]) % allocPalette2);
				if (Math.Abs(spin) <= 1)
					return;
				// reverse angle and spin when antiSpinning, or else the direction would change when parent and child switches
				refAngle = -refAngle;
				refSpin = (short)-refSpin;
			}
		}
		void IncFrameParameters(ref double refSize, ref double refAngle, ref double refHueAngle, double refSpin, ushort blur) {
			if (allocGenerationType >= GenerationType.AllSeeds)
				return;
			var blurPeriod = SelectedPeriod * blur;
			// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
			refAngle += (Math.Abs(refSpin) > 1 ? 2 * Math.Sign(refSpin) : refSpin) * (allocPeriodAngle * (1 + allocExtraSpin)) / (allocPeriodMultiplier * blurPeriod);
			refHueAngle += (hueCycleMultiplier + allocPalette2 * SelectedExtraHue) * (float)allocHue / (allocPeriodMultiplier * blurPeriod * 2);
			IncFrameSize(ref refSize, blurPeriod);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IncFrameSize(ref double refSize, int period) => refSize *= Math.Pow(f.ChildSize, allocZoom * 1.0 / period);
		#endregion
	}

	#region Gif
	private void StartGif() {
		// Open a temp file to pre-save GIF to - Use AnimatedGifEncoder
		gifSuccess = 0;
		gifEncoder = null;
		Hash.Clear();
		byte gifIndex = 0;
		tryGif = previewFrames;
		gifToken = (gifCancel = new()).Token;
		allocGifType = SelectedGifType;
		allocDelay = SelectedDelay;
		if (allocGifType != GifType.No) {

			gifEncoder = new();
			gifEncoder.SetDelay(allocDelay);// Framerate
			gifEncoder.SetRepeat(0);		// Loop
			gifEncoder.SetQuality(1);		// Highest quality
			gifEncoder.SetTransparent(SelectedAmbient < 0 ? Color.Black : Color.Empty);
			gifEncoder.SetPixelFormat(allocGpuDrawType == GpuDrawType.CPU ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb);
			_ = MakeTemp();

			while (gifIndex < 255) {
				GifTempPath = Path.Combine(MakeTemp(), filePrefix + "gif" + gifIndex + ".tmp");
				if (gifEncoder.Start(SelectedWidth, SelectedHeight, GifTempPath,
					    SelectedGifType == GifType.Global ? ColorTable.GlobalSingle : ColorTable.Local)
				   ) break;
				++gifIndex;
			}
			if (gifIndex == 255)
				gifEncoder = null;
		}
		// Flag the already encoded bitmaps to be encoded again;
		for (int b = previewFrames; b < bitmap.Length; ++b)
			if (encodedGif[b] >= 1) {
				_ = LockBits(b, rect);
				encodedGif[b] = 0;
			}
		tryGif = 0;
	}
	private void FinishGif() {
		// Save the temp GIF file
		gifSuccess = 0;
		if (!IsCancelRequested() && allocGifType != GifType.No) {
			if (gifEncoder != null) {
				if (!gifEncoder.IsFinished())
					gifEncoder.Finish();
				while (gifEncoder != null) {
					switch (gifEncoder.TryWrite()) {
						default:
						case TryWrite.Failed:
							StopGif(null);
							return;
						case TryWrite.Waiting:
							// This should never be hit, because the code reaches this loop only after getting cancelled or every frame finished
							Thread.Sleep(100);
							break;
						case TryWrite.FinishedFrame:
							break;
						case TryWrite.FinishedAnimation:
							gifSuccess = (short)Math.Max(SelectedWidth, SelectedHeight);
							// This will follow with gifEncoder.IsFinished()
							break;
					}
					if (gifEncoder.IsFinished())
						break;
				}
			}
			/*if (mp4Encoder != null) {
				mp4Encoder.Finish();
				gifSuccess = -1;
			}*/
		}
		gifEncoder = null;
		//mp4Encoder = null;
	}
	private void StopGif(FractalTask
#if NULLABLE
?
#endif
		task) {
		gifCancel?.Cancel();
		allocGifType = GifType.No;
		foreach (var t in tasks) {
			if (t.BitmapIndex < 0) {
				if (t != task && t != null)
					_ = t.Join();
			} else if (encodedGif[t.BitmapIndex] is >= 1 and <= 2) {
				if (t != task && t != null)
					_ = t.Join();
				bitmapState[t?.BitmapIndex ?? 0] = BitmapState.DrawingFinished;
			}
		}
		for (var i = 0; i < bitmap.Length; encodedGif[i++] = 0) { }
		tryGif = previewFrames;
		gifEncoder?.Abort();
		gifEncoder = null;
	}
#endregion

	#region Interface_Calls
	internal void CleanupTempFiles() {
		string path = MakeTemp();
		var files = Directory.GetFiles(path, $"{filePrefix}*");
		foreach (var file in files) {
			try {
				File.Delete(file);
			} catch (Exception) {
				// Handle exceptions if needed (e.g., log the error)
				//Console.WriteLine($"Error deleting file {file}: {ex.Message}");
			}
		}
	}
	// start the generator in a separate main thread so that the form can continue being responsive
	internal void StartGenerate() {
		// Setup palette: selected or random
		allocPalette = Colors[(short)(SelectedPaletteType < 0 ? random.Next(0, Colors.Count) : SelectedPaletteType)].Item2;
		allocPalette2 = (byte)(2 * allocPalette.Length);
		mainTask = Task.Run(GenerateAnimation, token = (cancel = new()).Token);
	}
	internal void GetValidZoomChildren() {

		if (SelectedGenerationType >= GenerationType.AllSeeds || 0 <= SelectedFractal) {
			validZoomChildren = [MaxZoomChild = 0];
			return;
		}
		validZoomChildren = [];
		int[] m;
		var s = GetFractal().ChildCutFunction.Count > 0 && (m = GetFractal().ChildCutFunction[SelectedCut].Item2) != null && m.Length > 0 && m[0] >= 0 ? -m[SelectedCutSeed] : -SelectedCutSeed;

		var cf = GetFractal().ChildCutFunction.Count <= 0 ? null : Fractal.CutFunctions[GetFractal().ChildCutFunction[SelectedCut].Item1].Item2;
		for (ushort i = 0; i < GetFractal().ChildCount; ++i)
			if (PreGenerateChildren(cf, i, s, 0))
				validZoomChildren.Add(i);
		MaxZoomChild = (ushort)(validZoomChildren.Count - 1);

		return; 
		
		bool PreGenerateChildren(Fractal.CutFunction
#if NULLABLE
?
#endif
			preCf, int preIndex, long preFlags, byte preDepth, byte max = 3) {
			long newFlags;
			return ++preDepth >= max || (newFlags = CalculateFlags(preCf, preIndex, preFlags)) >= 0 && PreGenerateChildren(preCf, preIndex, newFlags, preDepth, max);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			long CalculateFlags(Fractal.CutFunction
#if NULLABLE
?
#endif
			calcCf, int calcIndex, long calcFlags) => calcCf == null ? 0 : calcCf(calcIndex, calcFlags, GetFractal());
		}
	}

	internal void RequestCancel() {
		cancel?.Cancel();
		gifCancel?.Cancel();
		//try {
		mainTask?.Wait();
		//} catch (Exception) { }
	}
	internal int SaveGif(string gifPath) {
		try {
			// Try to save (move the temp) the gif file
			//gifEncoder?.Finish();
			//gifEncoder.Output(gifPath);
			File.Delete(gifPath);
			File.Move(GifTempPath, gifPath);
		} catch (IOException ex) {
			var exs = "SaveGif: An error occurred: " + ex.Message;
			Console.WriteLine(exs);
#if CustomDebug
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (UnauthorizedAccessException ex) {
			var exs = "SaveGif: Access denied: " + ex.Message;
			Console.WriteLine(exs);
#if CustomDebug
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (Exception ex) {
			var exs = "SaveGif: Unexpected error: " + ex.Message;
			Console.WriteLine(exs);
#if CustomDebug
			Log(ref logString, exs);
#endif
			return gifSuccess;
		}
		return gifSuccess = 0;
	}
	private (int, int, string) GetPngFormat() {
		int n = 1, nf = bitmap.Length - previewFrames;
		for (var number = nf; number >= 10; number /= 10)
			++n;
		return (n, nf, "D" + n);
	}
	private bool SavePng(int i) {
		int bmp = i + previewFrames;
		if (i < 0 || encodedPng[bmp] >= 2)
			return false;
		encodedPng[bmp] = 1;
		try {
			_ = MakeTemp();
			// old way of exporting to files
			//string fname = Path.Combine(MakeTemp(), $"{filePrefix}image_{i.ToString(d)}.");
			//using FileStream myStream = new(fname+"tmp", FileMode.Create);
			//bitmap[bmp].Save(myStream, ImageFormat.Png);
			//myStream.Flush();
			//myStream.Close();
			//File.Move(fname + "tmp", fname + "png");

			// new memorystream solution
			var m = msPngs[bmp] ??= new MemoryStream();
			bitmap[bmp].Save(m, System.Drawing.Imaging.ImageFormat.Png);
			msPngs[bmp]?.Flush();

			encodedPng[bmp] = 3;
			return false;
		} catch (Exception) {
			encodedPng[bmp] = 0;
			return true;
		}
	}
	private int SavePngs() {
		allocPngType = PngType.Yes; // ensures FinishTasks will want to start threads exporting PNGs
		pngFailed = 0; // reset failure attempt counter, every png write fail will increment it, and if it reaches 1000 it will cancel the FinishTasks
		//tryPng = previewFrames;

		for (int enc = previewFrames; !IsCancelRequested() && enc < bitmap.Length; Thread.Sleep(100))
			while (enc < bitmap.Length && encodedPng[enc] >= 2)
				++enc;
		//FinishTasks(true, true, (short _) => false); // FinishTasks will keep writing PNGs in parallel, until they are all finished (or cancel requested)

		return pngFailed >= MaxPngFails ? 2 : IsCancelRequested() ? 1 : 0; // If the export was cancelled from outside - terminate the ffmpeg process
	}
	internal string SavePngs(string pngPath) {
		exportType = ScheduledTask.Pngs;
		FinishTasks(true, true, _ => false); // Make sure there are no bitmap generation tasks still running
		if (SavePngs() == 2) 
			return Fail("PNG saving failed"); // failed to save
		if (IsCancelRequested())
			return ""; // just cancelled, return with no error
		pngPath = pngPath[..^4]; // remove the ".png"
		var (n, nf, d) = GetPngFormat();
		var maxGenerationTasks = Math.Max(1, allocMaxTasks - 1); // task count
		if (maxGenerationTasks <= 1) {
			for (int i = 0; i < bitmap.Length - previewFrames; ++i) {
				string /*fileFrom = Path.Combine(MakeTemp(),$"{filePrefix}image_{i.ToString(d)}.png"),*/ fileTo = $"{pngPath}_{i.ToString(d)}.png";
				for (var attempt = 0; attempt < 10; ++attempt) {
					try {
						// old file moving solution
						//File.Delete(fileTo);  // Delete existing file if present
						//File.Move(fileFrom, fileTo);

						// new memory stream solution
						using FileStream fs = new FileStream(fileTo, FileMode.OpenOrCreate, FileAccess.Write);
						if (msPngs[i + previewFrames] is MemoryStream ms) {
							ms.Position = 0;
							ms.CopyTo(fs);
						}
						fs.Flush();
						fs.Close();
						//fs.Dispose();
						break;
					} catch (Exception ex) {
						Thread.Sleep(10 + 10 * attempt * attempt); // wait and try again 10 times if failed
						if (attempt >= 100)
							return Fail("Move PNG Exception: " + ex.Message);
					}
				}
			}
			return ""; // finished
		}
		var po = new ParallelOptions {
			MaxDegreeOfParallelism = maxGenerationTasks,
			CancellationToken = token
		};
		string fail = "";
		var result = Parallel.For(0, bitmap.Length - previewFrames, (i, state) => {
			string /*fileFrom = Path.Combine(MakeTemp(), $"{filePrefix}image_{i.ToString(d)}.png"),*/ fileTo = $"{pngPath}_{i.ToString(d)}.png";
			for (var attempt = 0; attempt < 10; ++attempt) {
				try {
					// old file moving solution
					//File.Delete(fileTo);  // Delete existing file if present
					//File.Move(fileFrom, fileTo);

					// new memory stream solution
					using FileStream fs = new FileStream(fileTo, FileMode.OpenOrCreate, FileAccess.Write);
					if (msPngs[i + previewFrames] is MemoryStream ms) {
						ms.Position = 0;
						ms.CopyTo(fs);
					}
					fs.Flush();
					fs.Close();
					//fs.Dispose();
					break;
				} catch (Exception ex) {
					Thread.Sleep(10 + 10 * attempt * attempt);
					if (attempt >= 10) {
						fail = Fail("Move PNG Exception: " + ex.Message);
						state.Stop();
					}
				}
			}
		});
		while (!result.IsCompleted)
			Thread.Sleep(100); // Wait until p.for is finished
		return fail; // finished
	}
	internal string SaveGifToMp4(string gifPath, string mp4Path) {
		try {
			File.Delete(mp4Path);
		} catch (Exception ex) {
			return Fail("Delete existing Mp4: " + ex);
		}
			var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
		if (!File.Exists(ffmpegPath))
			return Fail("Ffmpeg not found");

		using var ffmpegProcess = new Process {
			StartInfo = new ProcessStartInfo {
				FileName = ffmpegPath,
				Arguments = $"-y -i \"{gifPath}\" -vf \"fps={SelectedFps},setpts=PTS*({1000.0 / (10 * allocDelay)}/{SelectedFps})\" -movflags +faststart -c:v libx264 -profile:v high444 -level 5.2 -preset veryslow -crf 18 -pix_fmt yuv444p \"{mp4Path}\"",
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardError = true,
				//RedirectStandardOutput = true, // not needed so far
				CreateNoWindow = true
			}
		};
		string fail = "";
		ffmpegProcess.ErrorDataReceived += (sender, e) => {
			if (string.IsNullOrEmpty(e.Data))
				return;
			// Regex to detect progress output
			if (Regex.IsMatch(e.Data, @"frame=\s*\d+") || Regex.IsMatch(e.Data, @"time=\d+:\d+:\d+\.\d+")) {
				// Extract frame count for progress display
				var match = Regex.Match(e.Data, @"frame=\s*(\d+)");
				if (match.Success)
					_ = ushort.TryParse(match.Groups[1].Value, out encodedMp4);
			} else fail += ";" + e.Data;
		};
		try {
			_ = ffmpegProcess.Start();
			// Begin reading error asynchronously
			ffmpegProcess.BeginErrorReadLine();
			// Wait for the process to exit
			ffmpegProcess.WaitForExit();
		} catch (Exception ex) {
			var exs = "ConvertMp4: Unexpected error: " + ex.Message;
			Console.WriteLine(exs);
			return Fail("Exception: " + ex);
		}
		return fail;
	}
	internal string SavePngsToMp4(string mp4Path) {
		exportType = ScheduledTask.PngsToMp4;
		encodedMp4 = 0;
		var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
		if (!File.Exists(ffmpegPath))
			return Fail("Ffmpeg.exe not found"); // if ffmpeg doesn't exist, return failure immediately
		FinishTasks(true, true, _ => false); // Make sure there are no bitmap generation tasks still running
		try {
			File.Delete(mp4Path);  // Delete existing file if present
		} catch (IOException ex) {
			return Fail("Failed to delete existing file: " + ex.Message); // return failure if deletion fails
		}
		// Start FFmpeg in a parallel process to encode the PNG sequence
		using var ffmpegProcess = new Process {
			StartInfo = new ProcessStartInfo {
				FileName = ffmpegPath,
				//Arguments = $"-y -framerate {SelectedFps} -i {temp}/{filePrefix}image_%0{n}d.png -vf \"scale=iw:ih\" -movflags +faststart -c:v libx264 -profile:v high444 -level 5.2 -preset veryslow -crf 18 -pix_fmt yuv444p -timeout {pngTime.Elapsed.TotalMilliseconds * 2000 + 2000000} \"{mp4Path}\"",
				Arguments = $"-y -framerate {SelectedFps} -f image2pipe -vcodec png -i pipe:0 -vf \"scale=iw:ih\" -movflags +faststart -c:v libx264 -profile:v high444 -level 5.2 -preset veryslow -crf 18 -pix_fmt yuv444p \"{mp4Path}\"",
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardError = true, // will get progress from this
				//RedirectStandardOutput = true, // not needed so far
				CreateNoWindow = true
			}
		};
		string fail = ""; // setup error listener
		try {
			// start pipe:
			//var pipeServer = new NamedPipeServerStream("mypipe", PipeDirection.Out);
			//pipeServer.WaitForConnection();
			
			// start ffmpeg
			if (!ffmpegProcess.Start())
				return Fail("Ffmpeg failed to start");
			//ffmpegProcess.BeginErrorReadLine();// Begin reading error asynchronously

			//ffmpegProcess.BeginOutputReadLine();  // Read standard output asynchronously
			allocPngType = PngType.Yes; // ensures FinishTasks will want to start threads exporting PNGs
			pngFailed = 0; // reset failure attempt counter, every png write fail will increment it, and if it reaches 1000 it will cancel the FinishTasks
						   //tryPng = previewFrames; // makes sure that we reexport any missing files, with these settings, the parallel thread elsewhere will export all the pngs as tmp first then rename to png

			// report completion
			var frameRegex = new Regex(@"frame=\s*(\d+)", RegexOptions.Compiled);
			ffmpegProcess.ErrorDataReceived += (s, e) => {
				if (e.Data == null) return;
				var match = frameRegex.Match(e.Data);
				if (match.Success) {
					encodedMp4 = ushort.Parse(match.Groups[1].Value);
				}
			};
			ffmpegProcess.BeginErrorReadLine();
			// will check if that other parallel thread elsewhere finished exporting all the pngs into the memory streams, and will dump these streams sequentially into the ffmpeg's input
			using (var inputStream = ffmpegProcess.StandardInput.BaseStream) {
				for (int enc = previewFrames; !IsCancelRequested() && enc < bitmap.Length; Thread.Sleep(100))
					while (enc < bitmap.Length && encodedPng[enc] >= 2) {
						if (msPngs[enc++] is MemoryStream ms) {
							ms.Position = 0;
							ms.CopyTo(inputStream);  // Write memory stream directly to FFmpeg's input stream
						}
					}
			}
			if (pngFailed >= MaxPngFails || IsCancelRequested()) { // If the export was cancelled from outside - terminate the ffmpeg process
				if (ffmpegProcess.StandardInput.BaseStream.CanWrite) {
					ffmpegProcess.StandardInput.Write("q");  // Send 'q' to FFmpeg to terminate gracefully
					ffmpegProcess.StandardInput.Flush();     // Ensure the command is sent
					Thread.Sleep(500);  // Give FFmpeg some time to exit gracefully
				}
				if (!ffmpegProcess.HasExited) 
					ffmpegProcess.Kill();  // Force terminate if graceful shutdown isn't possible
			}
			// Wait for the process to exit
			ffmpegProcess.WaitForExit();
		} catch (Exception ex) {
			return Fail("Exception: " + ex.Message); // return exception error
		}
		if (pngFailed >= MaxPngFails)
			fail += ";Failed to save PNGs";
		return fail != "" ? Fail("Ffmpeg errors: " + fail) : ""; // return fail or success
	}
	private static string Fail(string log) {
		Console.WriteLine(log);
/*#if CustomDebug
    Log(ref logString, log);
#endif*/
		return log;
	}

#if CustomDebug
	private void Log(ref string log, string line) {
		Debug.WriteLine(line);
		log += "\n" + line;
	}
#endif
	private void MakeDebugString() {
		if (!(DebugTasks || DebugAnim || DebugPng || DebugGif))
			return;
		if (IsCancelRequested()) {
			DebugString = "ABORTING";
			return;
		}
		var tempDebugString = "";
		int i = 0, li = 0;

		// Tasks States:
		if (DebugTasks) {
			tempDebugString = "TASKS:";
			while (i < allocMaxTasks) {
				var task = tasks[i];
				tempDebugString += "\n" + i++ + ": ";
				tempDebugString += task.State switch {
					TaskState.Done => "DONE",
					TaskState.Free => "FREE",
					_ => GetTaskState(task.BitmapIndex),
				};
			}
		}

		// Bitmap States:
		if (DebugAnim) {
			BitmapState state, lastState = BitmapState.Error;
			for (var c = i = 0; c < counter.Length; counter[c++] = 0) { }
			for (tempDebugString += "\n\nIMAGES COUNTER:"; i < bitmap.Length; ++i)
				++counter[(int)bitmapState[i]];
			var tempMemoryString = "\n";
			for (i = 0; i < bitmap.Length; ++i, lastState = state)
				if ((state = bitmapState[i]) != lastState) {
					if (i - 1 >= 0)
						tempMemoryString += (li == i - 1 ? li + ": " : li + "-" + (i - 1) + ": ") + GetBitmapState(lastState) + "\n";
					li = i;
				}
			tempMemoryString += (li == i - 1 ? li + ": " : li + "-" + (i - 1) + ": ") + GetBitmapState(lastState);
			for (var c = 0; c < counter.Length; ++c)
				tempDebugString += "\n" + counter[c] + "x: " + GetBitmapState((BitmapState)c);
			tempDebugString += "\n\nIMAGES STATES:" + tempMemoryString;
			tempDebugString = i < bitmap.Length ? tempDebugString + "\n" + i + "+: " + "QUEUED" : tempDebugString;
		}

		// PNGs States:
		if (DebugPng) 
			GetStates("PNG", encodedPng);

		// GIF States:
		if (DebugGif) 
			GetStates("GIF", encodedGif);

		DebugString = tempDebugString;

		return;
		
		void GetStates(string header, byte[] states) {
			byte iState, lastIState = 255;
			i = previewFrames;
			for (var c = 0; c < counterE.Length; counterE[c++] = 0) { }
			for (tempDebugString += "\n\n" + header + " " + L("debugCounter") + ":"; i < bitmap.Length; ++i)
				++counterE[states[i]];
			var tempMemoryString = "\n";
			for (i = previewFrames; i < bitmap.Length; ++i, lastIState = iState)
				if ((iState = states[i]) != lastIState) {
					if (i - 1 >= previewFrames)
						tempMemoryString += (li == i - 1 ? li + ": " : li + "-" + (i - 1) + ": ") + GetEncodeState(lastIState) + "\n";
					li = i;
				}
			tempMemoryString += (li == i - 1 ? li + ": " : li + "-" + (i - 1) + ": ") + GetEncodeState(lastIState);
			for (var c = 0; c < counterE.Length; ++c)
				tempDebugString += "\n" + counterE[c] + "x: " + GetEncodeState(c);
			tempDebugString += "\n\n" + header + " "  + L("debugStates") + " (0-" + (previewFrames-1) + "="+L("debugPreview") +"):" + tempMemoryString;
			tempDebugString = i < bitmap.Length ? tempDebugString + "\n" + i + "+: " + L("QUEUED") : tempDebugString;
		}

		string GetTaskState(int bmp) => bmp switch {
			-3 => L("debugExpPng"),
			-2 => L("debugFinishing"),
			- 1 =>  L("debugWriting"),
			_ => encodedGif[bmp] switch {
				1 => L("debugEncGif"),
				2 =>  L("debugWriteGif"),
				_ => encodedPng[bmp] switch {
					1 => L("debugEncPng"),
					_ => GetBitmapState(bitmapState[bmp])
				}
			}
		};
		string GetBitmapState(BitmapState displayState) => L("debugBmpState") + displayState.ToString();/*displayState switch {
			BitmapState.Queued => "QUEUED (NOT SPAWNED)",
			BitmapState.Dots => "GENERATING FRACTAL DOTS",
			BitmapState.Void => "GENERATING DIJKSTRA VOID",
			BitmapState.Drawing => "DRAWING BITMAP (LOCKED)",
			BitmapState.DrawingFinished => "DRAWING FINISHED (LOCKED)",
			BitmapState.Unlocked => "UNLOCKED",
			_ => "ERROR! (SHOULDN'T HAPPEN)"
		};*/
		string GetEncodeState(int s) => L("debugEncState") + s.ToString();/* s switch {
			0 => "QUEUED",
			1 => "ENCODING",
			2 => "ENCODED",
			3 => "FINISHED",
			_ => "ERROR! (SHOULDN'T HAPPEN)"
		};*/
	}
	internal void DebugStart() {
		// Need to enable the definition on top GeneratorForm.cpp to enable this
		// if enabled you will be unable to use the settings interface!
		_ = SelectFractal(1);
		SelectThreadingDepth();
		SelectedPeriod = debugPeriodOverride = 7;
		SelectedWidth = 8;//1920;
		SelectedHeight = 8;//1080;
		SelectedMaxTasks = -1;// 10;
		SelectedSaturate = 1.0f;
		SelectedDetail = .25f;
		SelectThreadingDepth();
		SelectedCut = SelectedChildAngle = SelectedChildColor = SelectedPaletteType = 0;
		SetupCutFunction();
	}
#endregion

	#region Interface_Settings
	internal bool SelectFractal(short selectFractal) {
		if (SelectedFractal == selectFractal)
			return true;
		// new fractal definition selected - let the form know to reset and restart me
		SelectedFractal = selectFractal;
		SelectedChildAngles = SelectedChildColors = 0;
		SelectedCut = SelectedChildColor = SelectedChildAngle = 0;
		return false;
	}
	internal void SetupFractal() {
		f = new(Fractals[SelectedFractal]); // take a copy to use (to prevent crashes if the fractal gets edited while in use)
		logBase = (float)Math.Log(f.ChildSize);
		SetMaxIterations();
	}
	internal void SetMaxIterations() {
		maxIterations = (short)(/*2*/ 8 + Math.Ceiling(Math.Log(Math.Max(SelectedWidth, SelectedHeight) * f.MaxSize / SelectedDetail) / logBase));
	}
	internal void SetupCutFunction() 
		=> selectedCutFunction = f.ChildCutFunction.Count <= 0 ? null : Fractal.CutFunctions[f.ChildCutFunction[SelectedCut].Item1].Item2;	
	internal bool SelectZoomChild(short z) {
		if (validZoomChildren == null || validZoomChildren.Count < 1 || validZoomChildren[z] == selectedZoomChild)
			return true;
		selectedZoomChild = validZoomChildren[z];
		return false;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void SelectThreadingDepth() {
		SetMaxIterations();
	}
	#endregion

	#region Interface_Getters
	internal static long GetEstimatedL2CachePerCore() {
		int cores = Environment.ProcessorCount;
		if (cores <= 4) return 256 * 1024;
		if (cores <= 8) return 512 * 1024;
		return 1024 * 1024; // 1 MB per core for modern CPUs
	}
	internal int GetMaxCutSeed()
	{
		if (SelectedCut < 0 || GetFractal().ChildCutFunction.Count <= 0)
			return CutSeed_Max;
		var c = GetFractal().ChildCutFunction[SelectedCut].Item2;
		return c != null && c.Length > 0 ? c[0] < 0 ? -c[0] : c.Length - 1 : CutSeed_Max;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Fractal GetFractal() => Fractals[SelectedFractal];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Vector3[] GetAllocPalette() => allocPalette;
	/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal byte GetAllocPalette2() => allocPalette2;*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Bitmap
#if NULLABLE
		?
#endif
		 GetBitmap(int index) => /*bitmap.Length == 0 ||*/ bitmap.Length <= index || bitmapState[index] < BitmapState.Unlocked || encodedPng[index] == 1 
		? null : bitmap[index + previewFrames];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Bitmap
#if NULLABLE
?
#endif
		GetPreviewBitmap() => bitmapsFinished < 1 ? null : bitmap[bitmapsFinished-1];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetFrames() => bitmap.Length == 0 ? 0 : SelectedGenerationType == GenerationType.OnlyImage ? 1 : bitmap.Length - previewFrames;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetPngFinished() => tryPng - previewFrames;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetGifFinished() => tryGif - previewFrames;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetBitmapsFinished() => (int)bitmapsFinished - previewFrames;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetMp4Encoded() => encodedMp4; 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int IsGifReady() => gifSuccess;
	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//internal bool IsMp4Ready() => mp4Success;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//GetTempGif();
	internal Fractal.CutFunction
#if NULLABLE
?
#endif
		GetCutFunction() 
		=> GetFractal().ChildCutFunction.Count <= 0 ? null : Fractal.CutFunctions[GetFractal().ChildCutFunction[SelectedCut].Item1].Item2;
	internal bool IsCancelRequested() => token.IsCancellationRequested;

	internal int GetCompleted() => exportType switch {
		ScheduledTask.PngsToMp4 => encodedMp4,
		ScheduledTask.GifToMp4 => encodedMp4,
		ScheduledTask.Pngs => GetPngFinished(),
		_ => 0,
	};
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal string GetBinSize() {
		uint b = allocBinSize, d = (b % 100) * 10;
		b /= 100;
		string p = "";
		if (b >= 1000) {
			d = b % 1000;
			b /= 1000;
			p = "K";
		}
		if(b >= 1000) {
			d = b % 1000;
			b /= 1000;
			p = "M";
		}
		if (b >= 1000) {
			d = b % 1000;
			b /= 1000;
			p = "B";
		}
		return p + b + "." + d.ToString("D4");
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal string GetStripeHeight() => allocStripeHeight.ToString();
	
#endregion
}

#region GenerateDots_Experimental
// Grid OfDepth implementations attempted to make fast OfDepth without racing pixel conditions by smartly separating the bitmap into cells.
// It did work, but was ultimately still not as fast as simmply using the MultiDepth Solution

// This implementation tries to separate odd and even rows/collumns of cells into concurrent tasks and combine the same cells into the same task
/*void GenerateDotsOfDepth(int dotsBitmapIndex) {
	// calculate the number of cells where each 1/4 batch could match the max number of threads
	var cellSize = MathF.Sqrt(task.ApplyWidth * task.ApplyHeight / (applyMaxTasks * 4.0f));
	cellSize = MathF.Min(
		task.ApplyWidth / Math.Max(1, MathF.Ceiling(task.ApplyWidth / cellSize)-1),
		task.ApplyHeight / Math.Max(1, MathF.Ceiling(task.ApplyHeight / cellSize)-1));
	var cellsX = 1+(int)MathF.Ceiling(task.ApplyWidth / cellSize);
	var cellsY = 1+(int)MathF.Ceiling(task.ApplyHeight / cellSize);
	// the maximum allowed radius that the shape can have so that it will fit into a 2x2 cell:
	var minSize = .5f * cellSize;
	List<(short, (double, double), (double, double), short, long, byte)>[,] cells 
		= new List<(short, (double, double), (double, double), short, long, byte)>[(int)cellsX, (int)cellsY];
	for (int x = 0; x < cellsX; ++x) for (int y = 0; y < cellsY; ++y) cells[x, y] = new();
	while (ofDepthQueue.Count > 0) {
		var (tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = ofDepthQueue.Dequeue();
		var tupleTask = tasks[tupleIndex];
		var preIterated = tupleTask.PreIterate[tupleDepth];
		var newPreIterated = tupleTask.PreIterate[++tupleDepth];
		// If we already reached final size of a single dot before we coudl start splitting tasks, so just draw them:
		if (newPreIterated.Item1 < tupleTask.ApplyDetail) {
			// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
			for (var i = 0; i < f.ChildCount; ++i) {
				if (IsCancelRequested())
					return;
				// Special Cutoff
				var newFlags = CalculateFlags(i, tupleFlags);
				if (newFlags < 0)
					continue;
				// Outside View
				var xy = preIterated.Item3[i];
				var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
				if (TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
					ApplyDot(tupleTask, task.Buffer, i + f.ChildCount * (newFlags & ((1 << f.ChildCount) - 1)), newXy.Item1, newXy.Item2,
						applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2));
			}
			continue;
		}
		// are the next deeper fractal children going to fit into a 2x2 cell?
		bool small = newPreIterated.Item1 + task.Bloom1 < minSize;
		// Split parent deeper into new smaller parents
		for (var i = 0; i < f.ChildCount; ++i) {
			if (IsCancelRequested())
				return;
			// Special Cutoff
			var newFlags = CalculateFlags(i, tupleFlags);
			if (newFlags < 0)
				continue;
			// Outside View
			var xy = preIterated.Item3[i];
			var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
			if (!TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
				continue;
			// small enough to fit into 2x2 so put it into the grid list, else put it back into queue to split further into smaller ones:
			if (small) {
				cells[
					(int)Math.Max(0, (tupleXy.Item1 - preIterated.Item1 - task.Bloom1) / cellSize),
					(int)Math.Max(0, (tupleXy.Item2 - preIterated.Item1 - task.Bloom1) / cellSize)
				].Add((tupleIndex, newXy,
				i == 0 ? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2) : (tupleAngle.Item1 + ChildAngle[i], tupleAngle.Item2),
				applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2), newFlags, tupleDepth));
			} else ofDepthQueue.Enqueue( 
				(tupleIndex, newXy,
				i == 0 ? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2) : (tupleAngle.Item1 + ChildAngle[i], tupleAngle.Item2),
				applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2), newFlags, tupleDepth));
		}
	}
	int qx = 0, qy = 0;
	// we now have a nice number of tasks to perform in parallel, so do that:
	// false argument makes sure that we only finish processing these iteration tasks, we can exit if other kinds of tasks are still running as we don't need to wait for these here
	for (int i = 0; i < 4; ++i) {
		bool finish = false;
		FinishTasks(false, false, taskIndex => {
			// this lambda is called when tasks[taskIndex] is free, and should return true if it started a new task here
			if (finish)
				return false; // we have finished
			int _qx = qx, _qy = qy;
			tasks[taskIndex].Start(dotsBitmapIndex, () => {
				// draw all overlapping shapes in teh same 2x2 cell in this single task:
				foreach (var t in cells[_qx, _qy])
					GenerateDotsSingleTask(t.Item1, task.Buffer, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6);
				// Let the master thread know this task is finished and can be freed and assigned a new task
				tasks[taskIndex].State = TaskState.Done;
			});
			qx += 2;
			if (qx >= cellsX) {
				qx %= 2; // go back to start of also the odd or even collumns
				qy += 2;
			}
			if (qy >= cellsY) {
				finish = true;
				if (qx == 0) { qx = 1; qy %= 2; } // do the collums mod 2 = 1 next
				else if (qy % 2 == 0) { qy = 1; qx = 0; }// do the rows mod 2 = 1 next
			}
			// task has been started:
			return true;
		});
	}
}*/

// This implementation tries to lock 2x2 cells
/*void GenerateDotsOfDepth(int dotsBitmapIndex) {
	var cellSize = MathF.Sqrt(task.ApplyWidth * task.ApplyHeight / (applyMaxTasks * 4.0f));
	var cellsX = MathF.Ceiling(task.ApplyWidth / cellSize);
	var cellsY = MathF.Ceiling(task.ApplyHeight / cellSize);
	// task radius must fit in this so it takes no more than 4 locked cells
	float cellWidth, cellHeight;
	var minSize = .5f *  MathF.Min(cellWidth = task.ApplyWidth / cellsX, cellHeight = task.ApplyHeight / cellsY);
	double s = 0;
	while(ofDepthQueue.Count > 0) {
		var (tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = ofDepthQueue.Dequeue();
		var tupleTask = tasks[tupleIndex];
		var preIterated = tupleTask.PreIterate[tupleDepth];
		var newPreIterated = tupleTask.PreIterate[++tupleDepth];
		// Draw Dots
		if (newPreIterated.Item1 < tupleTask.ApplyDetail) {
			// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
			for (var i = 0; i < f.ChildCount; ++i) {
				if (IsCancelRequested())
					return;
				// Special Cutoff
				var newFlags = CalculateFlags(i, tupleFlags);
				if (newFlags < 0)
					continue;
				// Outside View
				var xy = preIterated.Item3[i];
				var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
				if (TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
					ApplyDot(tupleTask, task.Buffer, i + f.ChildCount * (newFlags & ((1 << f.ChildCount) - 1)), newXy.Item1, newXy.Item2,
						applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2));
			}
			continue;
		}

		if ((s = preIterated.Item1 + task.Bloom1) < minSize) {
			ofDepthQueue.Enqueue((tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, --tupleDepth));
			break;
		}
		// Split parent deeper into new smaller parents
		for (var i = 0; i < f.ChildCount; ++i) {
			if (IsCancelRequested())
				return;
			// Special Cutoff
			var newFlags = CalculateFlags(i, tupleFlags);
			if (newFlags < 0)
				continue;
			// Outside View
			var xy = preIterated.Item3[i];
			var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
			if (!TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
				continue;
			ofDepthQueue.Enqueue(
				(tupleIndex, newXy,
				i == 0 ? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2) : (tupleAngle.Item1 + ChildAngle[i], tupleAngle.Item2),
				applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2), newFlags, tupleDepth));
		}
	}
	bool[,] locked = new bool[(int)cellsX + 1, (int)cellsY + 1];

	// we now have a nice number of tasks to perform in parallel, so do that:
	// false argument makes sure that we only finish processing these iteration tasks, we can exit if other kinds of tasks are still running as we don't need to wait for these here
	FinishTasks(false, false, taskIndex => {
		// until the queue is empty
		if (ofDepthQueue.Count <= 0)
			return false;
		var (tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = ofDepthQueue.Dequeue();
		var tupleTask = tasks[tupleIndex];
		var preIterated = tupleTask.PreIterate[tupleDepth];
		var lockX = (int)Math.Max(0, (tupleXy.Item1 - s) / cellWidth);
		var lockX2 = Math.Min(locked.GetLength(0) - 1, lockX + 1);
		var lockY = (int)Math.Max(0, (tupleXy.Item2 - s) / cellHeight);
		var lockY2 = Math.Min(locked.GetLength(1) - 1, lockY + 1);

		// if the teask would collide drawing with another, put it back at the end of the queue
		if (locked[lockX, lockY] || locked[lockX2, lockY] || locked[lockX, lockY2] || locked[lockX2, lockY2]) {
			ofDepthQueue.Enqueue((tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth));
			return false;
		}
		locked[lockX, lockY] = locked[lockX2, lockY] = locked[lockX, lockY2] = locked[lockX2, lockY2] = true;
		tasks[taskIndex].Start(dotsBitmapIndex, () => {
			GenerateDotsSingleTask(tupleIndex, task.Buffer, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth);
			// unlock monitors of the area this task was driving to
			locked[lockX, lockY] = locked[lockX2, lockY] = locked[lockX, lockY2] = locked[lockX2, lockY2] = false;
			tasks[taskIndex].State = TaskState.Done;
		});
		return true;
	});
}*/

// This implementation tries to group subtasks in the same cell regions to their own single thread, and lock 2x2 cells
/*void GenerateDotsOfDepth(int dotsBitmapIndex) {
	var cellSize = MathF.Sqrt(task.ApplyWidth * task.ApplyHeight / (applyMaxTasks * 4.0f));
	var cellsX = MathF.Ceiling(task.ApplyWidth / cellSize);
	var cellsY = MathF.Ceiling(task.ApplyHeight / cellSize);
	// task radius must fit in this so it takes no more than 4 locked cells
	float cellWidth, cellHeight;
	var minSize = .5f * MathF.Min(cellWidth = task.ApplyWidth / cellsX, cellHeight = task.ApplyHeight / cellsY);
	double s = 0;
	List<(short, (double, double), (double, double), short, long, byte)>[,] cells
		= new List<(short, (double, double), (double, double), short, long, byte)>[(int)cellsX, (int)cellsY];
	for (int x = 0; x < cellsX; ++x) for (int y = 0; y < cellsY; ++y) cells[x, y] = new();
	while (ofDepthQueue.Count > 0) {
		var (tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = ofDepthQueue.Dequeue();
		var tupleTask = tasks[tupleIndex];
		var preIterated = tupleTask.PreIterate[tupleDepth];
		var newPreIterated = tupleTask.PreIterate[++tupleDepth];
		// Draw Dots
		if (newPreIterated.Item1 < tupleTask.ApplyDetail) {
			// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
			for (var i = 0; i < f.ChildCount; ++i) {
				if (IsCancelRequested())
					return;
				// Special Cutoff
				var newFlags = CalculateFlags(i, tupleFlags);
				if (newFlags < 0)
					continue;
				// Outside View
				var xy = preIterated.Item3[i];
				var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
				if (TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
					ApplyDot(tupleTask, task.Buffer, i + f.ChildCount * (newFlags & ((1 << f.ChildCount) - 1)), newXy.Item1, newXy.Item2,
						applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2));
			}
			continue;
		}
		// are the next deeper fractal children going to fit into a 2x2 cell?
		bool small = (s = newPreIterated.Item1 + task.Bloom1) < minSize;
		// Split parent deeper into new smaller parents
		for (var i = 0; i < f.ChildCount; ++i) {
			if (IsCancelRequested())
				return;
			// Special Cutoff
			var newFlags = CalculateFlags(i, tupleFlags);
			if (newFlags < 0)
				continue;
			// Outside View
			var xy = preIterated.Item3[i];
			var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
			if (!TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
				continue;
			if (small) {
				cells[
					(int)Math.Max(0, (tupleXy.Item1 - preIterated.Item1 - task.Bloom1) / cellSize),
					(int)Math.Max(0, (tupleXy.Item2 - preIterated.Item1 - task.Bloom1) / cellSize)
				].Add((tupleIndex, newXy,
				i == 0 ? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2) : (tupleAngle.Item1 + ChildAngle[i], tupleAngle.Item2),
				applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2), newFlags, tupleDepth));
			} else ofDepthQueue.Enqueue(
				(tupleIndex, newXy,
				i == 0 ? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2) : (tupleAngle.Item1 + ChildAngle[i], tupleAngle.Item2),
				applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2), newFlags, tupleDepth));
		}
	}
	bool[,] locked = new bool[(int)cellsX + 1, (int)cellsY + 1];
	Queue<(int, int)> q = new();
	for (int x = 0; x < cellsX; ++x) for (int y = 0; y < cellsY; ++y) q.Enqueue((x, y));
	// we now have a nice number of tasks to perform in parallel, so do that:
	// false argument makes sure that we only finish processing these iteration tasks, we can exit if other kinds of tasks are still running as we don't need to wait for these here
	FinishTasks(false, false, taskIndex => {
		// until the queue is empty
		if (q.Count <= 0)
			return false;

		var (x, y) = q.Dequeue();
		int x1 = x + 1, y1 = y + 1;
		// if the teask would collide drawing with another, put it back at the end of the queue
		if (locked[x, y] || locked[x1, y] || locked[x, y1] || locked[x1,y1]) {
			q.Enqueue((x, y));
			return false;
		}
		locked[x, y] = locked[x1, y] = locked[x, y1] = locked[x1, y1] = true;
		tasks[taskIndex].Start(dotsBitmapIndex, () => {
			// draw all overlapping shapes in teh same 2x2 cell in this single task:
			foreach (var t in cells[x, y])
				GenerateDotsSingleTask(t.Item1, task.Buffer, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6);
			// unlock monitors of the area this task was driving to
			locked[x, y] = locked[x1, y] = locked[x, y1] = locked[x1, y1] = false;
			tasks[taskIndex].State = TaskState.Done;
		});
		return true;
	});
}*/

// This implementation attempted to combine the Grid and MultiDepth ideas, it still wasn't as fast as just MultiDepth
/*void GenerateDotsOfDepth(int dotsBitmapIndex) {
	int depth = -1, count = ofDepthQueue.Count, maxGenerationTasks = applyMaxTasks-1;
	// keep splitting parents until we have a queue DivDepth * maxTasks large:
	while (count > 0) {
		// take a parent form the queue to split into move parent to put back into the queue
		var (tupleIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) = ofDepthQueue.Dequeue();
		var tupleTask = tasks[tupleIndex];
		var preIterated = tupleTask.PreIterate[tupleDepth];
		var newPreIterated = tupleTask.PreIterate[++tupleDepth];
		// Draw Dots
		if (newPreIterated.Item1 < tupleTask.ApplyDetail) {
			// we are deep enough that the parent is within a pixel, so just split it one last time and draw its children as dots
			for (var i = 0; i < f.ChildCount; ++i) {
				if (IsCancelRequested())
					return;
				// Special Cutoff
				var newFlags = CalculateFlags(i, tupleFlags);
				if (newFlags < 0)
					continue;
				// Outside View
				var xy = preIterated.Item3[i];
				var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
				if (TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
					ApplyDot(tupleTask, tupleTask.Buffer, i + f.ChildCount * (newFlags & ((1 << f.ChildCount) - 1)), newXy.Item1, newXy.Item2,
						applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2));
			}
			count = ofDepthQueue.Count;
			continue;
		}
		// Split parent deeper into new smaller parents
		for (var i = 0; i < f.ChildCount; ++i) {
			if (IsCancelRequested())
				return;
			// Special Cutoff
			var newFlags = CalculateFlags(i, tupleFlags);
			if (newFlags < 0)
				continue;
			// Outside View
			var xy = preIterated.Item3[i];
			var newXy = NewXy(tupleXy, xy, tupleAngle.Item1);
			if (!TestSize(tupleTask, newXy.Item1, newXy.Item2, preIterated.Item1))
				continue;
			ofDepthQueue.Enqueue(
				(tupleIndex, newXy,
				i == 0 ? (tupleAngle.Item1 + ChildAngle[i] - tupleAngle.Item2, -tupleAngle.Item2) : (tupleAngle.Item1 + ChildAngle[i], tupleAngle.Item2),
				applyPreviewMode && tupleDepth > 1 ? tupleColor : (short)((tupleColor + ChildColor[i]) % applyPalette2), newFlags, tupleDepth));
		}

		var p = ofDepthQueue.Peek();
		if (p.Item6 > depth) {
			depth = p.Item6;
			count = ofDepthQueue.Count;
			if (count >= applyMaxTasks * DepthDiv)
				break;
		} else count = ofDepthQueue.Count;
	}
	int d = count / maxGenerationTasks;
	var m = count % maxGenerationTasks;
	int t = maxGenerationTasks;
	// we now have a nice number of tasks to perform in parallel, so do that:
	// false argument makes sure that we only finish processing these iteration tasks, we can exit if other kinds of tasks are still running as we don't need to wait for these here
	FinishTasks(false, false, taskIndex => {
		// until the queue is empty
		if (0 > --t)
			return false;
		int q = t < m ? d + 1 : d;
		var a = new (short, (double, double), (double, double), short, long, byte)[q];
		while (0 <= --q)
			a[q] = ofDepthQueue.Dequeue();
		// take a parent from queue and iterate it normally with single task code, but as its own parallel task
		tasks[taskIndex].Start(dotsBitmapIndex, () => {
		foreach (var (tupleBufferIndex, tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth) in a) {
				GenerateDotsSingleTask(tupleBufferIndex, buffer[taskIndex], tupleXy, tupleAngle, tupleColor, tupleFlags, tupleDepth);
				if (--count == 0)
					break;
			}
			tasks[taskIndex].State = TaskState.Done;
		});
		return true;
	});
}*/
#endregion