// Allow debug code, comment this when releasing for slightly better performance
//#define CUSTOMDEBUG

//#define SMOOTHNESSDEBUG_XY

//#define SMOOTHDEBUG_DETAIL

using Gif.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;

namespace RgbFractalGenCs;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class FractalGenerator {

	public const int MINTASKS = 1;

	private enum BitmapState : byte {
		Queued = 0,				// Hasn't even started generating the dots yet (and it might stay this way if OnlyImage)
		Dots = 1,				// Started Generating Dots
		Void = 2,				// Started Dijksring the Void
		Drawing = 3,			// Started drawing
		DrawingFinished = 4,	// Finished drawing
		UnlockedRAM = 5,        // Unlocked bitmap without encoding
		UnlockedPNG = 6,        // Unlocked bitmap without encoding and saved PNG
		Encoding = 7,			// Started Encoding
		EncodingFinished = 8,	// Finished encoding
		FinishedBitmap = 9,		// Finished bitmap (finished drawing if not encodeGIF, or fnished encoding if encodeGIF)
		Unlocked = 10,			// Unlocked bitmap
		Error = 11				// Unused, unless error obviously
	}
	private enum TaskState : byte {
		Free = 0,	// The task has not been started yet, or already finished and joined and ready to be started again
		Done = 1,	// The task if finished and ready to join without waiting
		Running = 2 // The task is running
	}
	public enum ParallelType : byte {
		OfAnimation = 0,// Parallel batching of each animation image into it's own single thread. No pixel conflicts, and more likely to use all the threads consistently. Recommended for animations.
		OfDepth = 1,    // Parallelism of each single image, generates the single next image faster, but there might be rare conflicts of accessing pixels, and it's a bit slower overall if generating an animation. Used automatically for OnlyImage.
		OfRecursion = 2 // Deprecated older image parallelism, use OfDepth instead
	}
	public enum GenerationType : byte {
		OnlyImage = 0,      // Only generates the first single image, and then halts. Or stops generating animation, if selected during an animation generation and at least one frame has already been finished.
		AnimationRAM = 1,   // Will only generate the animation for preview, faster than encoding GIF, but cannot save the GIF when finished.
		AnimationMP4 = 2,   // Will export PNG series for a high quality MP4 export
		LocalGIF = 3,       // Will encode a GIF while generating the animation, will be slower than generating the animation without GIF.
		GlobalGIF = 4,      // Will encode a gif but only analyze the first frame for a color map, much faster, but not recommended for shifting hues or if you want the highest possible quality
		AllSeedsGIF = 5,	// Will generate one image for every CutFunction parameter seed, ignoring zooming, spinning etc.
		AllSeedsMP4 = 6,	// Like AllSeedsGIF, but instead of encoding a GIF, it will export PNG series for direct high quality MP4 export, like the AnimationMp4 does
		HashParam = 7		// Like AllSeeds, but without GIF encoding, and will export a hast.txt containing all unique seeds.
	}

	private class FractalTask {
		internal Task task;				// Parallel Animation Tasks
		internal TaskState state;		// States of Animation Tasks
		internal Vector3[][] buffer;    // Buffer for points to print into bmp
		internal short[][] voidDepth;   // Depths of Void
		internal Vector3[][] voidNoise;	// Randomized noise samples for the void noise
		internal Queue<(short, short)> 
			voidQueue;                  // Void depth calculating Dijktra queue
		internal Dictionary<long, Vector3[]> F = [];
		internal Dictionary<long, Vector3[]> 
			H = [];						// Mixed children color
		//internal Vector3[] 
		//	I = new Vector3[3];         // Pure parent color
		//internal double huemod;			// Global R->G->B index shift
		//internal bool taskStarted;		// Additional safety, could remove if it never gets triggered for a while
		internal short taskIndex;       // The task index
		internal int bitmapIndex;       // The bitmapIndex of which the task is working on
		//internal int palette2;
		internal short applyWidth, 
			applyHeight;				// can be smaller for previews
		internal short widthBorder, 
			heightBorder;				// slightly below the width and height
		internal double rightEnd, 
			downEnd;					// slightly beyond width and depth, to ensure even bloomed pixels don't cutoff too early
		internal double bloom0;
		internal double bloom1;			// = selectBloom + 1;
		internal double upleftStart;		// = -selectBloom;
		internal float lightNormalizer;	// maximum brightness found in the buffer, for normalizing the final image brightness
		internal float voidDepthMax;	// maximum reached void depth during the dijkstra search, for normalizing the void intensity
		internal (double, float, (double, double)[])[]
			preIterate;					// (childSize, childDetail, childSpread, (childX,childY)[])

		internal FractalTask() {
			voidQueue = new Queue<(short, short)>();
			// this should be already by default, it's not c++:
			//taskStarted = false;state = TaskState.Free;	type = BitmapState.Queued;buffer = null;voidDepth = null;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool IsStillRunning() {
			return state == TaskState.Done ? Join() : state != TaskState.Free;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool Join() {
			Stop(); // Stop the thread
			state = TaskState.Free; // Mark it as free to be started again
			return false;//taskStarted = false; // (used this to be doubly sure when I had bugs in the control, but probably rendundant at this point)
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Start(int bitmap, Action action) {
			Stop();
			//taskStarted = true;
			bitmapIndex = bitmap;
			state = TaskState.Running;
			task = Task.Run(action);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Stop() {
			//if (taskStarted) {
				if (task != null) {
					task.Wait();
					task = null;
				}// else { }
			//}// else { }
		}
	}

#if CUSTOMDEBUG
	// Debug variables
	private string logString;						// Debug log
	Stopwatch startTime;							// Start Stopwatch
	private double initTimes, iterTimes, voidTimes, drawTimes, gifsTimes;
#endif

	#region Variables
	// Definitions
	private readonly List<Fractal> fractals;// Fractal definitions
	private Fractal f;                  // Selected fractal definition
	public short maxZoomChild;
	public double[] childAngle;         // A copy of selected childAngle
	public short[] childColor;         // A copy of childColor for allowing BGR
	private double applyPeriodAngle;     // Angle symmetry corrected for periodMultiplier
	private Fractal.CutFunction 
		cutFunction;					// Selected CutFunction pointer
	private long applyCutparam;			// Applied cutparam (selected or random)
	private short selectMaxIterations = -1;   // maximum depth of iteration (dependent on fractal/detail/resolution)
	private float logBase;              // Size Log Base
	private long startCutParam;
	public readonly Dictionary<string, int> hash = [];
	private readonly Dictionary<long, Vector3[]> colorBlends = [];
	private Vector3[] applyPalette;
	private int applyPalette2;
	private short selectZoomChild;      // Which cdhild to zoom into

	// Resolution
	private short allocatedWidth;	// How much buffer width is currently allocated?
	private short allocatedHeight;	// How much buffer height is currently allocated?

	// Frames
	private Bitmap[] bitmap;            // Prerender as an array of bitmaps
	private BitmapState[] bitmapState;	// 0 - not exists, 1 - generating and not exists, 2 - spawned and locked, 3 - finished drawing locked, 4 - started gif locked, 5 - finished unlocked
	private BitmapData[] bitmapData;    // Locked Bits for bitmaps
	private bool[] mp4Png;				// Was a PNG of this frame exported?
	private short finalPeriodMultiplier;// How much will the period get finally stretched? (calculated for seamless + user multiplier)
	private short debug;				// Debug frame count override
	private int bitmapsFinished;        // How many bitmaps are completely finished generating? (ready to display, encoded if possible)
	private int previewFrames;			// how many preview frames are we making for this resolution?
	private int nextBitmap;				// How many bitmaps have started generating? (next task should begin with this one)
	private int allocatedFrames;		 // How many bitmap frames are currently allocated?
	private short applyZoom;            // Applied zoom (selected or random)
	private short applyMaxIterations;
	private double applyDetail;
	private GenerationType applyGenerationType;
	private short applyPaletteType;

	// Color
	public readonly List<(string, Vector3[])> Colors = [
		("RGB", [new Vector3(255, 0, 0), new Vector3(0, 255, 0), new Vector3(0, 0, 255)]),
		("BGR", [new Vector3(0, 0, 255), new Vector3(0, 255, 0), new Vector3(255, 0, 0)]),
		("->", [new Vector3(255, 0, 0), new Vector3(255, 255, 0), new Vector3(0, 255, 0), new Vector3(0, 255, 255), new Vector3(0, 0, 255), new Vector3(255, 0, 255)]),
		("<-", [new Vector3(255, 0, 255), new Vector3(0, 0, 255), new Vector3(0, 255, 255), new Vector3(0, 255, 0), new Vector3(255, 255, 0), new Vector3(255, 0, 0)]),
		("Colorblind ", [new Vector3(255, 0, 0), new Vector3(0, 0, 255), new Vector3(0, 255, 0), new Vector3(0, 0, 255)]),
		("ColorblindLike", [new Vector3(255, 255, 0), new Vector3(0, 0, 255)]),
		("WhiteTransp", [new Vector3(255, 255, 255), new Vector3(0, 0, 0)]),								
		("WhiteBlack", [new Vector3(255, 255, 255), new Vector3(1, 1, 1)]),
		("RedTransp", [new Vector3(255, 0, 0), new Vector3(0, 0, 0)]),
		("GreenTransp", [new Vector3(0, 255, 0), new Vector3(0, 0, 0)]),
		("BlueTransp", [new Vector3(0, 0, 255), new Vector3(0, 0, 0)]),
		("RedBlack", [new Vector3(255, 0, 0), new Vector3(1, 0, 0)]),
		("GreenBlack", [new Vector3(0, 255, 0), new Vector3(0, 1, 0)]),
		("BlueBlack", [new Vector3(0, 0, 255), new Vector3(0, 0, 1)]),
		("GreyUp", [new Vector3(1, 1, 1), new Vector3(128, 128, 128), new Vector3(255, 255, 255)]),
		("GreyDown", [new Vector3(255, 255, 255), new Vector3(128, 128, 128), new Vector3(1, 1, 1)]),
		("WhiteWave", [new Vector3(1, 1, 1), new Vector3(128, 128, 128), new Vector3(255, 255, 255), new Vector3(128, 128, 128)]),
		("RedWave", [new Vector3(1, 0, 0), new Vector3(128, 0, 0), new Vector3(255, 0, 0), new Vector3(128, 0, 0)]),
		("GreenWave", [new Vector3(0, 1, 0), new Vector3(0, 128, 0), new Vector3(0, 255, 0), new Vector3(0, 128, 0)]),
		("BlueWave", [new Vector3(0, 0, 1), new Vector3(0, 0, 128), new Vector3(0, 0, 255), new Vector3(0, 0, 128)])
	];
	private short applyBlur, applyHue;
	//private short applyColorPalette;// RGB or BGR? (0/1)
	//private short applyHueCycle;	// Hue Cycling Direction (-1,0,1)
	private byte hueCycleMultiplier;// How fast should the hue shift to loop seamlessly?
	private bool applyPreviewMode;

	// Void
	private short ambnoise;		// Normalizer for maximum void depth - Precomputed amb * noise
	private short applyVoid;	// applied void setting
	private readonly Random 
		random = new();         // Random generator

	// Threading
	internal Vector3[][][] buffer;  // Buffer for points to print into bmp - separated for OfDepth
	private FractalTask[] tasks;
	private ParallelType 
		applyParallelType;			// Safely copy parallelType in here so it doesn't change in the middle of generation
	private short applyMaxTasks;    // Safely copy maxTasks in here so it doesn't change in the middle of generation
	private const int depthdiv = 6; // multiples of maxThreads to queue for OfDepth parallelism
	private short maxDepth;			// Maximum depth for Recusrion parallelism
	private short allocatedTasks;	// How many buffer tasks are currently allocated?
	private readonly object 
		taskLock = new();           // Monitor lock
	private readonly object
		bmpLock = new();			// Monitor lock
	private CancellationTokenSource 
		cancel;                     // Cancellation Token Source
	private CancellationToken
		token;                      // Cancellation token
	private CancellationTokenSource
		gifCancel;                  // Cancellation Token Source for gif encoder
	private CancellationToken
		gifToken;                   // Cancellation token for gif encoder
	private Task mainTask;          // Main Generation Task
	private short 
		isWritingBitmaps = 2,       // counter and lock to try writing bitmap to a file once every 2 threads
		isFinishingBitmaps = 2;     // counter and lock to try finishing bitmaps once every 2 threads
	(short, (double, double), (double, double), short, long, byte)[] 
		tuples;                     // Queue struct for GenerateDots_OfDepth ((x,y), angle, aa, color, -cutparam, depth);
	internal event Action UpdatePreview;

	// Export
	private AnimatedGifEncoder 
		gifEncoder;						// Export GIF encoder
	private Mp4Encoder mp4Encoder;
	private int gifSuccess;				// Temp GIF file "gif.tmp" successfuly created
	private bool gifThread = false;
	internal string gifTempPath;        // Temporary GIF file name
	private int tryPng;
	private System.Drawing.Rectangle 
		rect;							// Bitmap rectangle TODO implement

	// Selected Settings
	internal short selectFractal,       // Fractal definition (0-fractals.Length)
		selectChildAngle,               // Child angle definition (0-childAngle.Length)
		selectChildColor,               // Child color definition (0-childColor.Length)
		selectCut;                      // Selected CutFunction index (0-cutFunction.Length)
	internal int selectCutparam;        // Cutparam seed (0-maxCutparam)
	internal short selectWidth,         // Resolution width (1-X)
		selectHeight,                   // Resolution height (1-X)
		selectPeriod,                   // Parent to child frames period (1-X)
		selectPeriodMultiplier,         // Multiplier of frames period (1-X)
		selectZoom,                     // Zoom direction (-1 out, 0 random, 1 in)
		selectDefaultZoom,              // Default skipped frames of zoom (0-frames)
		selectSpin,                     // Spin direction (-2 random, -1 anticlockwise, 0 none, 1 clockwise, 2 antispin)
		selectDefaultAngle,             // Default spin angle (0-360)
		selectExtraSpin,                // Extra spin speed (0-X)
		selectPaletteType,              // Color Palette (0 - Colors.Length-1)
		selectHue;                      // -1 = random, 0 = RGB, 1 = BGR, 2 = RGB->GBR, 3 = BGR->RBG, 4 =RGB->BRG, 5 = BGR->GRB
	internal double
		selectDefaultHue;               // Default hue angle (-1 - Colors.Length-1)
	internal short
		selectExtraHue,                 // Extra hue angle speed (0-X)
		selectAmbient;                  // Void ambient strength (0-120)
	internal double selectNoise;        // Void noise strength (0-3)
	internal short selectVoid;			// Void Noise Size
	internal double selectSaturate,		// Saturation boost level (0-1)
		selectDetail,					// MinSize multiplier (1-10)
		selectBloom;                    // Dot bloom level (pixels)
	internal short selectBlur,          // Dot blur level
		selectBrightness;               // Light normalizer brightness (0-300)
	internal ParallelType 
		selectParallelType;				// 0 = Animation, 1 = Depth, 2 = Recursion
	internal short selectMaxTasks,		// Maximum allowed total tasks
		selectDelay,					// Animation frame delay
		selectFps = 1;
	internal GenerationType 
		selectGenerationType;			// 0 = Only Image, 1 = Animation, 2 = Animation + GIF
	internal int cutparamMaximum;		// Maximum seed for the selected CutFunction
	internal bool 
		restartGif = false;				// Makes me restart the gif encoder (called when delay is changed, which should restart the encoder, but not toss the finished bitmaps)
	internal bool 
		selectPreviewMode = false;		// Preview mode only renders a single smaller fractal with only one color shift at the highest level - for definition editor
	
	// Debug
	internal bool debugmode = false;
	internal string debugString = "";
	private readonly short[] counter = new short[12];

	private readonly string filePrefix;

	#endregion

	#region Init
	internal FractalGenerator() {
		filePrefix = "guid" + Guid.NewGuid().ToString("N");
		selectParallelType = ParallelType.OfAnimation;
		selectGenerationType = GenerationType.LocalGIF;
		selectDefaultHue = selectCutparam = selectDefaultZoom = selectDefaultAngle = selectExtraSpin = selectExtraHue = debug = 0;
		selectZoom = 1;
		allocatedFrames = selectSpin = selectHue = selectFractal = selectChildColor = selectChildAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks =  -1;
		gifEncoder = null;
		bitmap = null;
		gifSuccess = 0;
		//taskSnapshot = [];
		// Constants
		double pi = Math.PI, pi23 = 2 * pi / 3, pi43 = 4 * pi / 3, SYMMETRIC = 2 * pi,
			stt = Math.Sqrt(.75f),
			pfs = 2 * (1 + Math.Cos(.4 * pi)),
			cosc = Math.Cos(.4 * pi) / pfs,
			sinc = Math.Sin(.4 * pi) / pfs,
			v = 2 * (sinc * sinc + cosc * (cosc + pfs)) / (2 * sinc),
			//s0 = (2 + Math.Sqrt(2)) / Math.Sqrt(2),
			//sx = 2 + Math.Sqrt(2),
			//r = (1 / s0 + 1 / sx) / (1 / s0 + 1 / (2 * sx)),
			diag = Math.Sqrt(2) / 3;

		// X, Y
		double[] cx = new double[9], cy = new double[9], pfx = new double[6], pfy = new double[6], tfx = new double[4], tfy = new double[4], tfxE = new double[3], tfyE = new double[3], ttfx = new double[16], ttfy = new double[16];//, ofx = new double[9], ofy = new double[9];
		// Carpets
		cx[0] = cy[0] = 0;
		for (var i = 0; i < 4; ++i) {
			double ipi = i * pi / 2, icos = diag * Math.Cos(ipi), isin = diag * Math.Sin(ipi);
			cx[i * 2 + 1] = -icos + isin;
			cy[i * 2 + 1] = -isin - icos;
			cx[i * 2 + 2] = isin;
			cy[i * 2 + 2] = -icos;
		}
		// Pentaflakes
		pfx[0] = pfy[0] = 0;
		for (var i = 1; i <= 5; ++i) {
			pfx[i] = v * Math.Cos(.4 * (i + 3) * pi);
			pfy[i] = v * Math.Sin(.4 * (i + 3) * pi);
		}
		// Triflakes
		tfx[0] = tfy[0] = 0;
		for (var i = 1; i <= 3; ++i) {
			tfx[i] = .5 * Math.Cos(i * pi23);
			tfy[i] = .5 * Math.Sin(i * pi23);
		}
		// Tried a regular sierpinski triangle, didn't work
		/*for (var i = 0; i < 3; ++i) {
			tfxE[i] = .5 * Math.Sin(i * pi23);
			tfyE[i] = .5 * Math.Cos(i * pi23);
		}*/
		// Tetraflakes
		ttfx[0] = ttfy[0] = 0;
		for (var i = 1; i <= 3; ++i) {
			double ci = .25f * Math.Cos(i * pi23), si = .25f * Math.Sin(i * pi23),
				ci1 = .25f * Math.Cos((i + 1) * pi23), si1 = .25f * Math.Sin((i + 1) * pi23),
				ci2 = .25f * Math.Cos((i + 2) * pi23), si2 = .25f * Math.Sin((i + 2) * pi23);
			ttfx[i] = -ci;
			ttfy[i] = -si;
			ttfx[i + 3] = ci - ci1;
			ttfy[i + 3] = si - si1;
			ttfx[i + 6] = ci - ci2;
			ttfy[i + 6] = si - si2;
			ttfx[i + 9] = 2 * ci - ci1 - ci2;
			ttfy[i + 9] = 2 * si - si1 - si2;
			ttfx[i + 12] = ci - ci1 - ci2;
			ttfy[i + 12] = si - si1 - si2;
		}
		// Ocataflakes (not working)
		/*for (var i = 1; i <= 8; ++i) {
			ofx[i] = r * Math.Cos(i * pi / 4);
			ofy[i] = r * Math.Sin(i * pi / 4);
		}*/

		// Rotations
		// childAngle[0] = SYMMETRIC + symmetry when 2*pi is symmetric!
		// childAngle[0] = symmetry when 2*pi is NOT symmetric!

		// Fractal definitions list
		fractals = [
			new("Void", 0, 1000, 1, .1, 1, [0], [0], [("N",[pi])], [("N",[0])], null),

			new("TriTree", 10, 4, .2, .1, 1.0,
			[0, -stt, 2 * stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt],
			[0, -1.5, 0, 1.5, 1.5, 3, 1.5, -1.5, -3, -1.5],
			[
				("RingTree",[SYMMETRIC + pi23, pi, pi + pi23, pi + pi43, 0, 0, pi23, pi23, pi43, pi43]),
				("BeamTree_Beams", [pi / 3, 0, pi23, pi43, pi, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43]),
				("BeamTree_OuterJoint", [pi / 3, 0, pi23, pi43, pi + pi23, pi + pi23, pi, pi, pi + pi43, pi + pi43]),
				("BeamTree_InnerJoint", [pi / 3, 0, pi23, pi43, pi, pi, pi, pi, pi, pi])
			], [
				("Center", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Y", [0, 2, 2, 2, 0, 0, 0, 0, 0, 0]),
				("Center_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Y_3/2", [0, 3, 3, 3, 0, 0, 0, 0, 0, 0])
			], [
				(1, [-1534]),	// NoChildComplex
				(8, []),		// NoBeam
				(9, []),		// OuterJoint
				(10, [])		// InnerJoint
				]
			),

			new("FlakeTree", 13, 4, .2, .1, .9,
			[0, -stt, 2 * stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt, stt, -2 * stt, stt],
			[0, -1.5, 0, 1.5, 1.5, 3, 1.5, -1.5, -3, -1.5, 1.5, 0, -1.5],
			[
				("FlakeTree", [pi / 3, 0, pi23, pi43, pi, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43, 0, pi23, pi43]),
			], [
				("Center", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Y", [0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Corners", [0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0]),
				("Center_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Y_3/2", [0, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Corners_3/2", [0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 0])
			], null
			),

			new("TriComb", 13, 5, .2, .1, .9,
			[0, 2 * stt, -stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt, 2 * stt, -4 * stt, 2 * stt],
			[0, 0, 1.5, -1.5, 1.5, 3, 1.5, -1.5, -3, -1.5, 3, 0, -3],
			[("Classic", [pi / 3, 0, pi23, pi43, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43, pi, pi43, 0, pi23])],
			[
				("Center", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Bridges", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2]),
				("Bridges2", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 4]),
				("Center_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Bridges_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3])
			],
			[(3, [])] // TriComb_Seeds
			),

			new("Triflake", 4, 2, 1.5, .25f, .8, tfx, tfy,
			[
				("NoAngles", [pi / 3, SYMMETRIC, SYMMETRIC, SYMMETRIC]),
				("Angles", [pi / 3, SYMMETRIC + 0, SYMMETRIC + pi23, SYMMETRIC + 2 * pi23])],
			[
				("Center", [2, 0, 0, 0]),
				("Center_3/2", [3, 0, 0, 0])
			], [
				(1, [-22]),	// NoChildComplex
				(2, [0,3])	// NoBackDiag
			]
			),

			/*new("Sierpinski triangle", 3, 2, 1.5, .25f, .8, tfxE, tfyE,
			[
				("NoAngles", [2*pi, 0, 0]),
				("Angles", [2 * pi, 2*pi23, 0])],
			[
				("Center", [1, 0, 0])
			], [
				(1, []),	// NoChildComplex
			]
			),*/

			new("TetraTriflake", 16, 4, 1.5, .15f, .8, ttfx, ttfy,
			[
				("Div", [SYMMETRIC + pi23, pi, pi + pi23, pi + pi43, pi43, 0, pi23, pi23, pi43, 0, 0, pi23, pi43, pi, pi + pi23, pi + pi43]),
				("In", [SYMMETRIC + pi23, pi, pi + pi23, pi + pi43, pi23, pi43, 0, pi43, 0, pi23, 0, pi23, pi43, pi, pi + pi23, pi + pi43]),
				("Out", [SYMMETRIC + pi23, pi, pi + pi23, pi + pi43, 0, pi23, pi43, 0, pi23, pi43, 0, pi23, pi43, pi, pi + pi23, pi + pi43]),
				("NoAngles", [SYMMETRIC + pi23, pi, pi, pi, 0, 0, 0, 0, 0, 0, 0, 0, 0, pi, pi, pi])
			],
			[
				("Rad", [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2]),
				("Corner", [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0]),
				("Triangle",[0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Swirl",[0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Rad", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2]),
				("Center_Corner", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0]),
				("Center_Triangle",[2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Swirl",[2, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Rad 2", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 4]),
				("Center_Corner 2", [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 4, 0, 0, 0]),
				("Center_Triangle 2",[2, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Swirl 2",[2, 0, 0, 0, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0]),

				("Rad_3/2", [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3]),
				("Corner_3/2", [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 0, 0, 0]),
				("Triangle_3/2",[0, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Swirl_3/2",[0, 0, 0, 0, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Rad_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3]),
				("Center_Corner_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 0, 0, 0]),
				("Center_Triangle_3/2",[3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Swirl_3/2",[3, 0, 0, 0, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0])
			], [
				(4, []),// NoChildSymmetric
				(5, []),// NoChildRad
				(6, [0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,128,129,130,
					131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,
					156,157,158,159,256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,
					277,278,279,280,281,282,283,284,285,286,287,384,385,386,387,388,389,390,391,392,393,394,395,396,397,
					398,399,400,401,402,403,404,405,406,407,408,409,410,411,412,413,414,415]),// NoChildCorner
				(7, [0,4,8,12,16,20,24,28,32,36,40,44,48,52,56,60,64,68,72,76,80,84,88,92,96,100,104,108,112,116,120,124,
					128,132,136,140,144,148,152,156,160,164,168,172,176,180,184,188,192,196,200,204,208,212,216,220,224,
					228,232,236,240,244,248,252,256,260,264,268,272,276,280,284,288,292,296,300,304,308,312,316,320,324,
					328,332,336,340,344,348,352,356,360,364,368,372,376,380,384,388,392,396,400,404,408,412,416,420,424,
					428,432,436,440,444,448,452,456,460,464,468,472,476,480,484,488,492,496,500,504,508]),// NoChildTriangle
				//(2, []),	// NoBackDiag
				(12,[-98303]) // NoChildComplex
				]
			),

			new("SierpinskiCarpet", 9, 3, 1.0, .25f, .9, cx, cy,
			[
			("Classic", [SYMMETRIC + pi, 0, 0, 0, 0, 0, 0, 0, 0]),
			("H-I De Rivera O (opposites)", [SYMMETRIC + pi, 0, 0, 0, pi / 2, 0, 0, 0, pi / 2]),
			("H-I De Rivera H (coloreds)", [SYMMETRIC + pi, 0, pi / 2, 0, 0, 0, pi / 2, 0, 0]),
			("H-I De Rivera OH", [SYMMETRIC + pi, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2]),
			("H-I De Rivera X (corners)", [SYMMETRIC + pi, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0]),
			("H-I De Rivera XO", [SYMMETRIC + pi, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2]),
			("H-I De Rivera XH", [SYMMETRIC + pi, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0]),
			("H-I De Rivera / (diagonals)", [SYMMETRIC + pi, pi / 2, 0, 0, 0, pi / 2, 0, 0, 0]),
			("H-I De Rivera C", [SYMMETRIC + pi / 2, 0, 0, 0, 0, 0, 0, 0, 0]),
			("H-I De Rivera CO", [SYMMETRIC + pi / 2, 0, 0, 0, pi / 2, 0, 0, 0, pi / 2]),
			("H-I De Rivera CH", [SYMMETRIC + pi / 2, 0, pi / 2, 0, 0, 0, pi / 2, 0, 0]),
			("H-I De Rivera COH", [SYMMETRIC + pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2]),
			("H-I De Rivera CX", [SYMMETRIC + pi / 2, pi / 2, 0, pi / 2, 0, pi / 2, 0, pi / 2, 0]),
			("H-I De Rivera CXO", [SYMMETRIC + pi / 2, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2]),
			("H-I De Rivera CXH", [SYMMETRIC + pi / 2, pi / 2, pi / 2, pi / 2, 0, pi / 2, pi / 2, pi / 2, 0]),
			("H-I De Rivera C/", [SYMMETRIC + pi / 2, pi / 2, 0, 0, 0, pi / 2, 0, 0, 0])
			], [
				("Sierpinski_Carpet",[2, 0, 0, 0, 0, 0, 0, 0, 0]),
				("H-I_De_Rivera", [0, 0, 2, 0, 0, 0, 2, 0, 0]),
				("Sierpinski_Carpet_3/2",[3, 0, 0, 0, 0, 0, 0, 0, 0]),
				("H-I_De_Rivera_3/2", [0, 0, 3, 0, 0, 0, 3, 0, 0])
			], [
				// Symmetric
				(11, [-95]),
				// NoBackDiag (AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA...
				(2, [0,4100,4104,4108,8194,8208,8210,8224,8226,8240,8242,12290,12292,12294,12296,12298,12300,12302,12304,12306,12308,12310,12312,12314,12316,12318,12320,12322,12324,12326,12328,12330,12332,12334,12336,12338,12340,12342,12344,12346,12348,12350,
					16385,16448,16449,16512,16513,16576,16577,16640,16641,16704,16705,16768,16769,16832,16833,20481,20484,20485,20488,20489,20492,20493,20544,20545,20548,20549,20552,20553,20556,20557,20608,20609,20612,20613,20616,20617,20620,20621,20672,20673,
					20676,20677,20680,20681,20684,20685,20736,20737,20740,20741,20744,20745,20748,20749,20800,20804,20805,20808,20809,20812,20813,20864,20865,20868,20869,20872,20873,20876,20877,20928,20929,20932,20933,20936,20937,20940,20941,24577,24578,24579,
					24592,24593,24594,24595,24608,24609,24610,24611,24624,24625,24626,24627,24640,24641,24642,24643,24656,24657,24658,24659,24672,24673,24674,24675,24688,24689,24690,24691,24704,24705,24706,24707,24720,24721,24722,24723,24736,24737,24738,24739,
					24752,24753,24754,24755,24768,24769,24770,24771,24784,24785,24786,24787,24800,24801,24802,24803,24816,24817,24818,24819,24832,24833,24834,24835,24848,24849,24850,24851,24864,24865,24866,24867,24880,24881,24882,24883,24896,24897,24898,24899,
					24912,24913,24914,24915,24928,24929,24930,24931,24944,24945,24946,24947,24960,24961,24962,24963,24976,24977,24978,24979,24992,24993,24994,24995,25008,25009,25010,25011,25024,25025,25026,25027,25040,25041,25042,25043,25056,25057,25058,25059,
					25072,25073,25074,25075,28673,28674,28675,28676,28677,28678,28679,28680,28681,28682,28683,28684,28685,28686,28687,28688,28689,28690,28691,28692,28693,28694,28695,28696,28697,28698,28699,28700,28701,28702,28703,28704,28705,28706,28707,28708,
					28709,28710,28711,28712,28713,28714,28715,28716,28717,28718,28719,28720,28721,28722,28723,28724,28725,28726,28727,28728,28729,28730,28731,28732,28733,28734,28735,28736,28737,28738,28739,28740,28741,28742,28743,28744,28745,28746,28747,28748,
					28749,28750,28751,28752,28753,28754,28755,28756,28757,28758,28759,28760,28761,28762,28763,28764,28765,28766,28767,28768,28769,28770,28771,28772,28773,28774,28775,28776,28777,28778,28779,28780,28781,28782,28783,28784,28785,28786,28787,28788,
					28789,28790,28791,28792,28793,28794,28795,28796,28797,28798,28799,28800,28801,28802,28803,28804,28805,28806,28807,28808,28809,28810,28811,28812,28813,28814,28815,28816,28817,28818,28819,28820,28821,28822,28823,28824,28825,28826,28827,28828,
					28829,28830,28831,28832,28833,28834,28835,28836,28837,28838,28839,28840,28841,28842,28843,28844,28845,28846,28847,28848,28849,28850,28851,28852,28853,28854,28855,28856,28857,28858,28859,28860,28861,28862,28863,28864,28865,28866,28867,28868,
					28869,28870,28871,28872,28873,28874,28875,28876,28877,28878,28879,28880,28881,28882,28883,28884,28885,28886,28887,28888,28889,28890,28891,28892,28893,28894,28895,28896,28897,28898,28899,28900,28901,28902,28903,28904,28905,28906,28907,28908,
					28909,28910,28911,28912,28913,28914,28915,28916,28917,28918,28919,28920,28921,28922,28923,28924,28925,28926,28927,28928,28929,28930,28931,28932,28933,28934,28935,28936,28937,28938,28939,28940,28941,28942,28943,28944,28945,28946,28947,28948,
					28949,28950,28951,28952,28953,28954,28955,28956,28957,28958,28959,28960,28961,28962,28963,28964,28965,28966,28967,28968,28969,28970,28971,28972,28973,28974,28975,28976,28977,28978,28979,28980,28981,28982,28983,28984,28985,28986,28987,28988,
					28989,28990,28991,28992,28993,28994,28995,28996,28997,28998,28999,29000,29001,29002,29003,29004,29005,29006,29007,29008,29009,29010,29011,29012,29013,29014,29015,29016,29017,29018,29019,29020,29021,29022,29023,29024,29025,29026,29027,29028,
					29029,29030,29031,29032,29033,29034,29035,29036,29037,29038,29039,29040,29041,29042,29043,29044,29045,29046,29047,29048,29049,29050,29051,29052,29053,29054,29055,29056,29057,29058,29059,29060,29061,29062,29063,29064,29065,29066,29067,29068,
					29069,29070,29071,29072,29073,29074,29075,29076,29077,29078,29079,29080,29081,29082,29083,29084,29085,29086,29087,29088,29089,29090,29091,29092,29093,29094,29095,29096,29097,29098,29099,29100,29101,29102,29103,29104,29105,29106,29107,29108,
					29109,29110,29111,29112,29113,29114,29115,29116,29117,29118,29119,29120,29121,29122,29123,29124,29125,29126,29127,29128,29129,29130,29131,29132,29133,29134,29135,29136,29137,29138,29139,29140,29141,29142,29143,29144,29145,29146,29147,29148,
					29149,29150,29151,29152,29153,29154,29155,29156,29157,29158,29159,29160,29161,29162,29163,29164,29165,29166,29167,29168,29169,29170,29171,29172,29173,29174,29175,29176,29177,29178,29179,29180,29181,29182,29183,33280,33792,34304,34816,35328,
					35840,36352,36868,36872,36876,37376,37380,37384,37388,37888,37892,37896,37900,38400,38404,38408,38412,38912,38916,38920,38924,39424,39428,39432,39436,39936,39940,39944,39948,40448,40452,40456,40460,40962,40976,40978,40992,40994,41008,41010,
					41472,41474,41488,41490,41504,41506,41520,41522,41984,41986,42000,42002,42016,42018,42032,42034,42496,42498,42512,42514,42528,42530,42544,42546,43008,43010,43024,43026,43040,43042,43056,43058,43520,43522,43536,43538,43552,43554,43568,43570,
					44032,44034,44048,44050,44064,44066,44080,44082,44544,44546,44560,44562,44576,44578,44592,44594,45058,45060,45062,45064,45066,45068,45070,45072,45074,45076,45078,45080,45082,45084,45086,45088,45090,45092,45094,45096,45098,45100,45102,45104,
					45106,45108,45110,45112,45114,45116,45118,45568,45570,45572,45574,45576,45578,45580,45582,45584,45586,45588,45590,45592,45594,45596,45598,45600,45602,45604,45606,45608,45610,45612,45614,45616,45618,45620,45622,45624,45626,45628,45630,46080,
					46082,46084,46086,46088,46090,46092,46094,46096,46098,46100,46102,46104,46106,46108,46110,46112,46114,46116,46118,46120,46122,46124,46126,46128,46130,46132,46134,46136,46138,46140,46142,46592,46594,46596,46598,46600,46602,46604,46606,46608,
					46610,46612,46614,46616,46618,46620,46622,46624,46626,46628,46630,46632,46634,46636,46638,46640,46642,46644,46646,46648,46650,46652,46654,47104,47106,47108,47110,47112,47114,47116,47118,47120,47122,47124,47126,47128,47130,47132,47134,47136,
					47138,47140,47142,47144,47146,47148,47150,47152,47154,47156,47158,47160,47162,47164,47166,47616,47618,47620,47622,47624,47626,47628,47630,47632,47634,47636,47638,47640,47642,47644,47646,47648,47650,47652,47654,47656,47658,47660,47662,47664,
					47666,47668,47670,47672,47674,47676,47678,48128,48130,48132,48134,48136,48138,48140,48142,48144,48148,48150,48152,48154,48156,48158,48160,48162,48164,48166,48168,48170,48172,48174,48176,48178,48180,48182,48184,48186,48188,48190,48640,48642,
					48644,48646,48648,48650,48652,48654,48656,48658,48660,48662,48664,48666,48668,48670,48672,48674,48676,48678,48680,48682,48684,48686,48688,48690,48692,48694,48696,48698,48700,48702,49153,49216,49217,49280,49281,49344,49345,49408,49409,49472,
					49473,49536,49537,49600,49601,49664,49665,49728,49729,49792,49793,49856,49857,49920,49921,49984,49985,50048,50049,50112,50113,50176,50177,50240,50241,50304,50305,50368,50369,50432,50433,50496,50497,50560,50561,50624,50625,50688,50689,50752,
					50753,50816,50817,50880,50881,50944,50945,51008,51009,51072,51073,51136,51137,51200,51201,51264,51265,51328,51329,51392,51393,51456,51457,51520,51521,51584,51585,51648,51649,51712,51713,51776,51777,51840,51841,51904,51905,51968,51969,52032,
					52033,52096,52097,52160,52161,52224,52225,52288,52289,52352,52353,52416,52417,52480,52481,52544,52545,52608,52609,52672,52673,52736,52737,52800,52801,52864,52865,52928,52929,52992,52993,53056,53057,53120,53121,53184,53185,53249,53252,53253,
					53256,53257,53260,53261,53312,53313,53316,53317,53320,53321,53324,53325,53376,53377,53380,53381,53384,53385,53388,53389,53440,53441,53444,53445,53448,53449,53452,53453,53504,53505,53508,53509,53512,53513,53516,53517,53568,53569,53572,53573,
					53576,53577,53580,53581,53632,53633,53636,53637,53640,53641,53644,53645,53696,53697,53700,53701,53704,53705,53708,53709,53760,53761,53764,53765,53768,53769,53772,53773,53824,53825,53828,53829,53832,53833,53836,53837,53888,53889,53892,53893,
					53896,53897,53900,53901,53952,53953,53956,53957,53960,53961,53964,53965,54016,54017,54020,54021,54024,54025,54028,54029,54080,54081,54084,54085,54088,54089,54092,54093,54144,54145,54148,54149,54152,54153,54156,54157,54208,54209,54212,54213,
					54216,54217,54220,54221,54272,54273,54276,54277,54280,54281,54284,54285,54336,54337,54340,54341,54344,54345,54348,54349,54400,54401,54404,54405,54408,54409,54412,54413,54464,54465,54468,54469,54472,54473,54476,54477,54528,54529,54532,54533,
					54536,54537,54540,54541,54592,54593,54596,54597,54600,54601,54604,54605,54656,54657,54660,54661,54664,54665,54668,54669,54720,54721,54724,54725,54728,54729,54732,54733,54784,54785,54788,54789,54792,54793,54796,54797,54848,54849,54852,54853,
					54856,54857,54860,54861,54912,54913,54916,54917,54920,54921,54924,54925,54976,54977,54980,54981,54984,54985,54988,54989,55040,55041,55044,55045,55048,55049,55052,55053,55104,55105,55108,55109,55112,55113,55116,55117,55168,55169,55172,55173,
					55176,55177,55180,55181,55232,55233,55236,55237,55240,55241,55244,55245,55296,55297,55300,55301,55304,55305,55308,55309,55360,55361,55364,55365,55368,55369,55372,55373,55424,55425,55428,55429,55432,55433,55436,55437,55488,55489,55492,55493,
					55496,55497,55500,55501,55552,55553,55556,55557,55560,55561,55564,55565,55616,55617,55620,55621,55624,55625,55628,55629,55680,55681,55684,55685,55688,55689,55692,55693,55744,55745,55748,55749,55752,55753,55756,55757,55808,55809,55812,55813,
					55816,55817,55820,55821,55872,55873,55876,55877,55880,55881,55884,55885,55936,55937,55940,55941,55944,55945,55948,55949,56000,56001,56004,56005,56008,56009,56012,56013,56064,56065,56068,56069,56072,56073,56076,56077,56128,56129,56132,56133,
					56136,56137,56140,56141,56192,56193,56196,56197,56200,56201,56204,56205,56256,56257,56260,56261,56264,56265,56268,56269,56320,56321,56324,56325,56328,56329,56332,56333,56384,56385,56388,56389,56392,56393,56396,56397,56448,56449,56452,56453,
					56456,56457,56460,56461,56512,56513,56516,56517,56520,56521,56524,56525,56576,56577,56580,56581,56584,56585,56588,56589,56640,56641,56644,56645,56648,56649,56652,56653,56704,56705,56708,56709,56712,56713,56716,56717,56768,56769,56772,56773,
					56776,56777,56780,56781,56832,56833,56836,56837,56840,56841,56844,56845,56896,56897,56900,56901,56904,56905,56908,56909,56960,56961,56964,56965,56968,56969,56972,56973,57024,57025,57028,57029,57032,57033,57036,57037,57088,57089,57092,57093,
					57096,57097,57100,57101,57152,57153,57156,57157,57160,57161,57164,57165,57216,57217,57220,57221,57224,57225,57228,57229,57280,57281,57284,57285,57288,57289,57292,57293,57345,57346,57347,57360,57361,57362,57363,57376,57377,57378,57379,57392,
					57393,57394,57395,57408,57409,57410,57411,57424,57425,57426,57427,57440,57441,57442,57443,57456,57457,57458,57459,57472,57473,57474,57475,57488,57489,57490,57491,57504,57505,57506,57507,57520,57521,57522,57523,57536,57537,57538,57539,57552,
					57553,57554,57555,57568,57569,57570,57571,57584,57585,57586,57587,57600,57601,57602,57603,57616,57617,57618,57619,57632,57633,57634,57635,57648,57649,57650,57651,57664,57665,57666,57667,57680,57681,57682,57683,57696,57697,57698,57699,57712,
					57713,57714,57715,57728,57729,57730,57731,57744,57745,57746,57747,57760,57761,57762,57763,57776,57777,57778,57779,57792,57793,57794,57795,57808,57809,57810,57811,57824,57825,57826,57827,57840,57841,57842,57843,57856,57857,57858,57859,57872,
					57873,57874,57875,57888,57889,57890,57891,57904,57905,57906,57907,57920,57921,57922,57923,57936,57937,57938,57939,57952,57953,57954,57955,57968,57969,57970,57971,57984,57985,57986,57987,58000,58001,58002,58003,58016,58017,58018,58019,58032,
					58033,58034,58035,58048,58049,58050,58051,58064,58065,58066,58067,58080,58081,58082,58083,58096,58097,58098,58099,58112,58113,58114,58115,58128,58129,58130,58131,58144,58145,58146,58147,58160,58161,58162,58163,58176,58177,58178,58179,58192,
					58193,58194,58195,58208,58209,58210,58211,58224,58225,58226,58227,58240,58241,58242,58243,58256,58257,58258,58259,58272,58273,58274,58275,58288,58289,58290,58291,58304,58305,58306,58307,58320,58321,58322,58323,58336,58337,58338,58339,58352,
					58353,58354,58355,58368,58369,58370,58371,58384,58385,58386,58387,58400,58401,58402,58403,58416,58417,58418,58419,58432,58433,58434,58435,58448,58449,58450,58451,58464,58465,58466,58467,58480,58481,58482,58483,58496,58497,58498,58499,58512,
					58513,58514,58515,58528,58529,58530,58531,58544,58545,58546,58547,58560,58561,58562,58563,58576,58577,58578,58579,58592,58593,58594,58595,58608,58609,58610,58611,58624,58625,58626,58627,58640,58641,58642,58643,58656,58657,58658,58659,58672,
					58673,58674,58675,58688,58689,58690,58691,58704,58705,58706,58707,58720,58721,58722,58723,58736,58737,58738,58739,58752,58753,58754,58755,58768,58769,58770,58771,58784,58785,58786,58787,58800,58801,58802,58803,58816,58817,58818,58819,58832,
					58833,58834,58835,58848,58849,58850,58851,58864,58865,58866,58867,58880,58881,58882,58883,58896,58897,58898,58899,58912,58913,58914,58915,58928,58929,58930,58931,58944,58945,58946,58947,58960,58961,58962,58963,58976,58977,58978,58979,58992,
					58993,58994,58995,59008,59009,59010,59011,59024,59025,59026,59027,59040,59041,59042,59043,59056,59057,59058,59059,59072,59073,59074,59075,59088,59089,59090,59091,59104,59105,59106,59107,59120,59121,59122,59123,59136,59137,59138,59139,59152,
					59153,59154,59155,59168,59169,59170,59171,59184,59185,59186,59187,59200,59201,59202,59203,59216,59217,59218,59219,59232,59233,59234,59235,59248,59249,59250,59251,59264,59265,59266,59267,59280,59281,59282,59283,59296,59297,59298,59299,59312,
					59313,59314,59315,59328,59329,59330,59331,59344,59345,59346,59347,59360,59361,59362,59363,59376,59377,59378,59379,59392,59393,59394,59395,59408,59409,59410,59411,59424,59425,59426,59427,59440,59441,59442,59443,59456,59457,59458,59459,59472,
					59473,59474,59475,59488,59489,59490,59491,59504,59505,59506,59507,59520,59521,59522,59523,59536,59537,59538,59539,59552,59553,59554,59555,59568,59569,59570,59571,59584,59585,59586,59587,59600,59601,59602,59603,59616,59617,59618,59619,59632,
					59633,59634,59635,59648,59649,59650,59651,59664,59665,59666,59667,59680,59681,59682,59683,59696,59697,59698,59699,59712,59713,59714,59715,59728,59729,59730,59731,59744,59745,59746,59747,59760,59761,59762,59763,59776,59777,59778,59779,59792,
					59793,59794,59795,59808,59809,59810,59811,59824,59825,59826,59827,59840,59841,59842,59843,59856,59857,59858,59859,59872,59873,59874,59875,59888,59889,59890,59891,59904,59905,59906,59907,59920,59921,59922,59923,59936,59937,59938,59939,59952,
					59953,59954,59955,59968,59969,59970,59971,59984,59985,59986,59987,60000,60001,60002,60003,60016,60017,60018,60019,60032,60033,60034,60035,60048,60049,60050,60051,60064,60065,60066,60067,60080,60081,60082,60083,60096,60097,60098,60099,60112,
					60113,60114,60115,60128,60129,60130,60131,60144,60145,60146,60147,60160,60161,60162,60163,60176,60177,60178,60179,60192,60193,60194,60195,60208,60209,60210,60211,60224,60225,60226,60227,60240,60241,60242,60243,60256,60257,60258,60259,60272,
					60273,60274,60275,60288,60289,60290,60291,60304,60305,60306,60307,60320,60321,60322,60323,60336,60337,60338,60339,60352,60353,60354,60355,60368,60369,60370,60371,60384,60385,60386,60387,60400,60401,60402,60403,60416,60417,60418,60419,60432,
					60433,60434,60435,60448,60449,60450,60451,60464,60465,60466,60467,60480,60481,60482,60483,60496,60497,60498,60499,60512,60513,60514,60515,60528,60529,60530,60531,60544,60545,60546,60547,60560,60561,60562,60563,60576,60577,60578,60579,60592,
					60593,60594,60595,60608,60609,60610,60611,60624,60625,60626,60627,60640,60641,60642,60643,60656,60657,60658,60659,60672,60673,60674,60675,60688,60689,60690,60691,60704,60705,60706,60707,60720,60721,60722,60723,60736,60737,60738,60739,60752,
					60753,60754,60755,60768,60769,60770,60771,60784,60785,60786,60787,60800,60801,60802,60803,60816,60817,60818,60819,60832,60833,60834,60835,60848,60849,60850,60851,60864,60865,60866,60867,60880,60881,60882,60883,60896,60897,60898,60899,60912,
					60913,60914,60915,60928,60929,60930,60931,60944,60945,60946,60947,60960,60961,60962,60963,60976,60977,60978,60979,60992,60993,60994,60995,61008,61009,61010,61011,61024,61025,61026,61027,61040,61041,61042,61043,61056,61057,61058,61059,61072,
					61073,61074,61075,61088,61089,61090,61091,61104,61105,61106,61107,61120,61121,61122,61123,61136,61137,61138,61139,61152,61153,61154,61155,61168,61169,61170,61171,61184,61185,61186,61187,61200,61201,61202,61203,61216,61217,61218,61219,61232,
					61233,61234,61235,61248,61249,61250,61251,61264,61265,61266,61267,61280,61281,61282,61283,61296,61297,61298,61299,61312,61313,61314,61315,61328,61329,61330,61331,61344,61345,61346,61347,61360,61361,61362,61363,61376,61377,61378,61379,61392,
					61393,61394,61395,61408,61409,61410,61411,61424,61425,61426,61427,61441,61442,61443,61444,61445,61446,61447,61448,61449,61450,61451,61452,61453,61454,61455,61456,61457,61458,61459,61460,61461,61462,61463,61464,61465,61466,61467,61468,61469,
					61470,61471,61472,61473,61474,61475,61476,61477,61478,61479,61480,61481,61482,61483,61484,61485,61486,61487,61488,61489,61490,61491,61492,61493,61494,61495,61496,61497,61498,61499,61500,61501,61502,61503,61504,61505,61506,61507,61508,61509,
					61510,61511,61512,61513,61514,61515,61516,61517,61518,61519,61520,61521,61522,61523,61524,61525,61526,61527,61528,61529,61530,61531,61532,61533,61534,61535,61536,61537,61538,61539,61540,61541,61542,61543,61544,61545,61546,61547,61548,61549,
					61550,61551,61552,61553,61554,61555,61556,61557,61558,61559,61560,61561,61562,61563,61564,61565,61566,61567,61568,61569,61570,61571,61572,61573,61574,61575,61576,61577,61578,61579,61580,61581,61582,61583,61584,61585,61586,61587,61588,61589,
					61590,61591,61592,61593,61594,61595,61596,61597,61598,61599,61600,61601,61602,61603,61604,61605,61606,61607,61608,61609,61610,61611,61612,61613,61614,61615,61616,61617,61618,61619,61620,61621,61622,61623,61624,61625,61626,61627,61628,61629,
					61630,61631,61632,61633,61634,61635,61636,61637,61638,61639,61640,61641,61642,61643,61644,61645,61646,61647,61648,61649,61650,61651,61652,61653,61654,61655,61656,61657,61658,61659,61660,61661,61662,61663,61664,61665,61666,61667,61668,61669,
					61670,61671,61672,61673,61674,61675,61676,61677,61678,61679,61680,61681,61682,61683,61684,61685,61686,61687,61688,61689,61690,61691,61692,61693,61694,61695,61696,61697,61698,61699,61700,61701,61702,61703,61704,61705,61706,61707,61708,61709,
					61710,61711,61712,61713,61714,61715,61716,61717,61718,61719,61720,61721,61722,61723,61724,61725,61726,61727,61728,61729,61730,61731,61732,61733,61734,61735,61736,61737,61738,61739,61740,61741,61742,61743,61744,61745,61746,61747,61748,61749,
					61750,61751,61752,61753,61754,61755,61756,61757,61758,61759,61760,61762,61763,61764,61765,61766,61767,61768,61769,61770,61771,61772,61773,61774,61775,61776,61777,61778,61779,61780,61781,61782,61783,61784,61785,61786,61787,61788,61789,61790,
					61791,61792,61793,61794,61795,61796,61797,61798,61799,61800,61801,61802,61803,61804,61805,61806,61807,61808,61809,61810,61811,61812,61813,61814,61815,61816,61817,61818,61819,61820,61821,61822,61823,61824,61825,61826,61827,61828,61829,61830,
					61831,61832,61833,61834,61835,61836,61837,61838,61839,61840,61841,61842,61843,61844,61845,61846,61847,61848,61849,61850,61851,61852,61853,61854,61855,61856,61857,61858,61859,61860,61861,61862,61863,61864,61865,61866,61867,61868,61869,61870,
					61871,61872,61873,61874,61875,61876,61877,61878,61879,61880,61881,61882,61883,61884,61885,61886,61887,61888,61889,61890,61891,61892,61893,61894,61895,61896,61897,61898,61899,61900,61901,61902,61903,61904,61905,61906,61907,61908,61909,61910,
					61911,61912,61913,61914,61915,61916,61917,61918,61919,61920,61921,61922,61923,61924,61925,61926,61927,61928,61929,61930,61931,61932,61933,61934,61935,61936,61937,61938,61939,61940,61941,61942,61943,61944,61945,61946,61947,61948,61949,61950,
					61951,61952,61953,61954,61955,61956,61957,61958,61959,61960,61961,61962,61963,61964,61965,61966,61967,61968,61969,61970,61971,61972,61973,61974,61975,61976,61977,61978,61979,61980,61981,61982,61983,61984,61985,61986,61987,61988,61989,61990,
					61991,61992,61993,61994,61995,61996,61997,61998,61999,62000,62001,62002,62003,62004,62005,62006,62007,62008,62009,62010,62011,62012,62013,62014,62015,62016,62017,62018,62019,62020,62021,62022,62023,62024,62025,62026,62027,62028,62029,62030,
					62031,62032,62033,62034,62035,62036,62037,62038,62039,62040,62041,62042,62043,62044,62045,62046,62047,62048,62049,62050,62051,62052,62053,62054,62055,62056,62057,62058,62059,62060,62061,62062,62063,62064,62065,62066,62067,62068,62069,62070,
					62071,62072,62073,62074,62075,62076,62077,62078,62079,62080,62081,62082,62083,62084,62085,62086,62087,62088,62089,62090,62091,62092,62093,62094,62095,62096,62097,62098,62099,62100,62101,62102,62103,62104,62105,62106,62107,62108,62109,62110,
					62111,62112,62113,62114,62115,62116,62117,62118,62119,62120,62121,62122,62123,62124,62125,62126,62127,62128,62129,62130,62131,62132,62133,62134,62135,62136,62137,62138,62139,62140,62141,62142,62143,62144,62145,62146,62147,62148,62149,62150,
					62151,62152,62153,62154,62155,62156,62157,62158,62159,62160,62161,62162,62163,62164,62165,62166,62167,62168,62169,62170,62171,62172,62173,62174,62175,62176,62177,62178,62179,62180,62181,62182,62183,62184,62185,62186,62187,62188,62189,62190,
					62191,62192,62193,62194,62195,62196,62197,62198,62199,62200,62201,62202,62203,62204,62205,62206,62207,62208,62209,62210,62211,62212,62213,62214,62215,62216,62217,62218,62219,62220,62221,62222,62223,62224,62225,62226,62227,62228,62229,62230,
					62231,62232,62233,62234,62235,62236,62237,62238,62239,62240,62241,62242,62243,62244,62245,62246,62247,62248,62249,62250,62251,62252,62253,62254,62255,62256,62257,62258,62259,62260,62261,62262,62263,62264,62265,62266,62267,62268,62269,62270,
					62271,62272,62273,62274,62275,62276,62277,62278,62279,62280,62281,62282,62283,62284,62285,62286,62287,62288,62289,62290,62291,62292,62293,62294,62295,62296,62297,62298,62299,62300,62301,62302,62303,62304,62305,62306,62307,62308,62309,62310,
					62311,62312,62313,62314,62315,62316,62317,62318,62319,62320,62321,62322,62323,62324,62325,62326,62327,62328,62329,62330,62331,62332,62333,62334,62335,62336,62337,62338,62339,62340,62341,62342,62343,62344,62345,62346,62347,62348,62349,62350,
					62351,62352,62353,62354,62355,62356,62357,62358,62359,62360,62361,62362,62363,62364,62365,62366,62367,62368,62369,62370,62371,62372,62373,62374,62375,62376,62377,62378,62379,62380,62381,62382,62383,62384,62385,62386,62387,62388,62389,62390,
					62391,62392,62393,62394,62395,62396,62397,62398,62399,62400,62401,62402,62403,62404,62405,62406,62407,62408,62409,62410,62411,62412,62413,62414,62415,62416,62417,62418,62419,62420,62421,62422,62423,62424,62425,62426,62427,62428,62429,62430,
					62431,62432,62433,62434,62435,62436,62437,62438,62439,62440,62441,62442,62443,62444,62445,62446,62447,62448,62449,62450,62451,62452,62453,62454,62455,62456,62457,62458,62459,62460,62461,62462,62463,62464,62465,62466,62467,62468,62469,62470,
					62471,62472,62473,62474,62475,62476,62477,62478,62479,62480,62481,62482,62483,62484,62485,62486,62487,62488,62489,62490,62491,62492,62493,62494,62495,62496,62497,62498,62499,62500,62501,62502,62503,62504,62505,62506,62507,62508,62509,62510,
					62511,62512,62513,62514,62515,62516,62517,62518,62519,62520,62521,62522,62523,62524,62525,62526,62527,62528,62529,62530,62531,62532,62533,62534,62535,62536,62537,62538,62539,62540,62541,62542,62543,62544,62545,62546,62547,62548,62549,62550,
					62551,62552,62553,62554,62555,62556,62557,62558,62559,62560,62561,62562,62563,62564,62565,62566,62567,62568,62569,62570,62571,62572,62573,62574,62575,62576,62577,62578,62579,62580,62581,62582,62583,62584,62585,62586,62587,62588,62589,62590,
					62591,62592,62593,62594,62595,62596,62597,62598,62599,62600,62601,62602,62603,62604,62605,62606,62607,62608,62609,62610,62611,62612,62613,62614,62615,62616,62617,62618,62619,62620,62621,62622,62623,62624,62625,62626,62627,62628,62629,62630,
					62631,62632,62633,62634,62635,62636,62637,62638,62639,62640,62641,62642,62643,62644,62645,62646,62647,62648,62649,62650,62651,62652,62653,62654,62655,62656,62657,62658,62659,62660,62661,62662,62663,62664,62665,62666,62667,62668,62669,62670,
					62671,62672,62673,62674,62675,62676,62677,62678,62679,62680,62681,62682,62683,62684,62685,62686,62687,62688,62689,62690,62691,62692,62693,62694,62695,62696,62697,62698,62699,62700,62701,62702,62703,62704,62705,62706,62707,62708,62709,62710,
					62711,62712,62713,62714,62715,62716,62717,62718,62719,62720,62721,62722,62723,62724,62725,62726,62727,62728,62729,62730,62731,62732,62733,62734,62735,62736,62737,62738,62739,62740,62741,62742,62743,62744,62745,62746,62747,62748,62749,62750,
					62751,62752,62753,62754,62755,62756,62757,62758,62759,62760,62761,62762,62763,62764,62765,62766,62767,62768,62769,62770,62771,62772,62773,62774,62775,62776,62777,62778,62779,62780,62781,62782,62783,62784,62785,62786,62787,62788,62789,62790,
					62791,62792,62793,62794,62795,62796,62797,62798,62799,62800,62801,62802,62803,62804,62805,62806,62807,62808,62809,62810,62811,62812,62813,62814,62815,62816,62817,62818,62819,62820,62821,62822,62823,62824,62825,62826,62827,62828,62829,62830,
					62831,62832,62833,62834,62835,62836,62837,62838,62839,62840,62841,62842,62843,62844,62845,62846,62847,62848,62849,62850,62851,62852,62853,62854,62855,62856,62857,62858,62859,62860,62861,62862,62863,62864,62865,62866,62867,62868,62869,62870,
					62871,62872,62873,62874,62875,62876,62877,62878,62879,62880,62881,62882,62883,62884,62885,62886,62887,62888,62889,62890,62891,62892,62893,62894,62895,62896,62897,62898,62899,62900,62901,62902,62903,62904,62905,62906,62907,62908,62909,62910,
					62911,62912,62913,62914,62915,62916,62917,62918,62919,62920,62921,62922,62923,62924,62925,62926,62927,62928,62929,62930,62931,62932,62933,62934,62935,62936,62937,62938,62939,62940,62941,62942,62943,62944,62945,62946,62947,62948,62949,62950,
					62951,62952,62953,62954,62955,62956,62957,62958,62959,62960,62961,62962,62963,62964,62965,62966,62967,62968,62969,62970,62971,62972,62973,62974,62975,62976,62977,62978,62979,62980,62981,62982,62983,62984,62985,62986,62987,62988,62989,62990,
					62991,62992,62993,62994,62995,62996,62997,62998,62999,63000,63001,63002,63003,63004,63005,63006,63007,63008,63009,63010,63011,63012,63013,63014,63015,63016,63017,63018,63019,63020,63021,63022,63023,63024,63025,63026,63027,63028,63029,63030,
					63031,63032,63033,63034,63035,63036,63037,63038,63039,63040,63041,63042,63043,63044,63045,63046,63047,63048,63049,63050,63051,63052,63053,63054,63055,63056,63057,63058,63059,63060,63061,63062,63063,63064,63065,63066,63067,63068,63069,63070,
					63071,63072,63073,63074,63075,63076,63077,63078,63079,63080,63081,63082,63083,63084,63085,63086,63087,63088,63089,63090,63091,63092,63093,63094,63095,63096,63097,63098,63099,63100,63101,63102,63103,63104,63105,63106,63107,63108,63109,63110,
					63111,63112,63113,63114,63115,63116,63117,63118,63119,63120,63121,63122,63123,63124,63125,63126,63127,63128,63129,63130,63131,63132,63133,63134,63135,63136,63137,63138,63139,63140,63141,63142,63143,63144,63145,63146,63147,63148,63149,63150,
					63151,63152,63153,63154,63155,63156,63157,63158,63159,63160,63161,63162,63163,63164,63165,63166,63167,63168,63169,63170,63171,63172,63173,63174,63175,63176,63177,63178,63179,63180,63181,63182,63183,63184,63185,63186,63187,63188,63189,63190,
					63191,63192,63193,63194,63195,63196,63197,63198,63199,63200,63201,63202,63203,63204,63205,63206,63207,63208,63209,63210,63211,63212,63213,63214,63215,63216,63217,63218,63219,63220,63221,63222,63223,63224,63225,63226,63227,63228,63229,63230,
					63231,63232,63233,63234,63235,63236,63237,63238,63239,63240,63241,63242,63243,63244,63245,63246,63247,63248,63249,63250,63251,63252,63253,63254,63255,63256,63257,63258,63259,63260,63261,63262,63263,63264,63265,63266,63267,63268,63269,63270,
					63271,63272,63273,63274,63275,63276,63277,63278,63279,63280,63281,63282,63283,63284,63285,63286,63287,63288,63289,63290,63291,63292,63293,63294,63295,63296,63297,63298,63299,63300,63301,63302,63303,63304,63305,63306,63307,63308,63309,63310,
					63311,63312,63313,63314,63315,63316,63317,63318,63319,63320,63321,63322,63323,63324,63325,63326,63327,63328,63329,63330,63331,63332,63333,63334,63335,63336,63337,63338,63339,63340,63341,63342,63343,63344,63345,63346,63347,63348,63349,63350,
					63351,63352,63353,63354,63355,63356,63357,63358,63359,63360,63361,63362,63363,63364,63365,63366,63367,63368,63369,63370,63371,63372,63373,63374,63375,63376,63377,63378,63379,63380,63381,63382,63383,63384,63385,63386,63387,63388,63389,63390,
					63391,63392,63393,63394,63395,63396,63397,63398,63399,63400,63401,63402,63403,63404,63405,63406,63407,63408,63409,63410,63411,63412,63413,63414,63415,63416,63417,63418,63419,63420,63421,63422,63423,63424,63425,63426,63427,63428,63429,63430,
					63431,63432,63433,63434,63435,63436,63437,63438,63439,63440,63441,63442,63443,63444,63445,63446,63447,63448,63449,63450,63451,63452,63453,63454,63455,63456,63457,63458,63459,63460,63461,63462,63463,63464,63465,63466,63467,63468,63469,63470,
					63471,63472,63473,63474,63475,63476,63477,63478,63479,63480,63481,63482,63483,63484,63485,63486,63487,63488,63489,63490,63491,63492,63493,63494,63495,63496,63497,63498,63499,63500,63501,63502,63503,63504,63505,63506,63507,63508,63509,63510,
					63511,63512,63513,63514,63515,63516,63517,63518,63519,63520,63521,63522,63523,63524,63525,63526,63527,63528,63529,63530,63531,63532,63533,63534,63535,63536,63537,63538,63539,63540,63541,63542,63543,63544,63545,63546,63547,63548,63549,63550,
					63551,63552,63553,63554,63555,63556,63557,63558,63559,63560,63561,63562,63563,63564,63565,63566,63567,63568,63569,63570,63571,63572,63573,63574,63575,63576,63577,63578,63579,63580,63581,63582,63583,63584,63585,63586,63587,63588,63589,63590,
					63591,63592,63593,63594,63595,63596,63597,63598,63599,63600,63601,63602,63603,63604,63605,63606,63607,63608,63609,63610,63611,63612,63613,63614,63615,63616,63617,63618,63619,63620,63621,63622,63623,63624,63625,63626,63627,63628,63629,63630,
					63631,63632,63633,63634,63635,63636,63637,63638,63639,63640,63641,63642,63643,63644,63645,63646,63647,63648,63649,63650,63651,63652,63653,63654,63655,63656,63657,63658,63659,63660,63661,63662,63663,63664,63665,63666,63667,63668,63669,63670,
					63671,63672,63673,63674,63675,63676,63677,63678,63679,63680,63681,63682,63683,63684,63685,63686,63687,63688,63689,63690,63691,63692,63693,63694,63695,63696,63697,63698,63699,63700,63701,63702,63703,63704,63705,63706,63707,63708,63709,63710,
					63711,63712,63713,63714,63715,63716,63717,63718,63719,63720,63721,63722,63723,63724,63725,63726,63727,63728,63729,63730,63731,63732,63733,63734,63735,63736,63737,63738,63739,63740,63741,63742,63743,63744,63745,63746,63747,63748,63749,63750,
					63751,63752,63753,63754,63755,63756,63757,63758,63759,63760,63761,63762,63763,63764,63765,63766,63767,63768,63769,63770,63771,63772,63773,63774,63775,63776,63777,63778,63779,63780,63781,63782,63783,63784,63785,63786,63787,63788,63789,63790,
					63791,63792,63793,63794,63795,63796,63797,63798,63799,63800,63801,63802,63803,63804,63805,63806,63807,63808,63809,63810,63811,63812,63813,63814,63815,63816,63817,63818,63819,63820,63821,63822,63823,63824,63825,63826,63827,63828,63829,63830,
					63831,63832,63833,63834,63835,63836,63837,63838,63839,63840,63841,63842,63843,63844,63845,63846,63847,63848,63849,63850,63851,63852,63853,63854,63855,63856,63857,63858,63859,63860,63861,63862,63863,63864,63865,63866,63867,63868,63869,63870,
					63871,63872,63873,63874,63875,63876,63877,63878,63879,63880,63881,63882,63883,63884,63885,63886,63887,63888,63889,63890,63891,63892,63893,63894,63895,63896,63897,63898,63899,63900,63901,63902,63903,63904,63905,63906,63907,63908,63909,63910,
					63911,63912,63913,63914,63915,63916,63917,63918,63919,63920,63921,63922,63923,63924,63925,63926,63927,63928,63929,63930,63931,63932,63933,63934,63935,63936,63937,63938,63939,63940,63941,63942,63943,63944,63945,63946,63947,63948,63949,63950,
					63951,63952,63953,63954,63955,63956,63957,63958,63959,63960,63961,63962,63963,63964,63965,63966,63967,63968,63969,63970,63971,63972,63973,63974,63975,63976,63977,63978,63979,63980,63981,63982,63983,63984,63985,63986,63987,63988,63989,63990,
					63991,63992,63993,63994,63995,63996,63997,63998,63999,64000,64001,64002,64003,64004,64005,64006,64007,64008,64009,64010,64011,64012,64013,64014,64015,64016,64017,64018,64019,64020,64021,64022,64023,64024,64025,64026,64027,64028,64029,64030,
					64031,64032,64033,64034,64035,64036,64037,64038,64039,64040,64041,64042,64043,64044,64045,64046,64047,64048,64049,64050,64051,64052,64053,64054,64055,64056,64057,64058,64059,64060,64061,64062,64063,64064,64065,64066,64067,64068,64069,64070,
					64071,64072,64073,64074,64075,64076,64077,64078,64079,64080,64081,64082,64083,64084,64085,64086,64087,64088,64089,64090,64091,64092,64093,64094,64095,64096,64097,64098,64099,64100,64101,64102,64103,64104,64105,64106,64107,64108,64109,64110,
					64111,64112,64113,64114,64115,64116,64117,64118,64119,64120,64121,64122,64123,64124,64125,64126,64127,64128,64129,64130,64131,64132,64133,64134,64135,64136,64137,64138,64139,64140,64141,64142,64143,64144,64145,64146,64147,64148,64149,64150,
					64151,64152,64153,64154,64155,64156,64157,64158,64159,64160,64161,64162,64163,64164,64165,64166,64167,64168,64169,64170,64171,64172,64173,64174,64175,64176,64177,64178,64179,64180,64181,64182,64183,64184,64185,64186,64187,64188,64189,64190,
					64191,64192,64193,64194,64195,64196,64197,64198,64199,64200,64201,64202,64203,64204,64205,64206,64207,64208,64209,64210,64211,64212,64213,64214,64215,64216,64217,64218,64219,64220,64221,64222,64223,64224,64225,64226,64227,64228,64229,64230,
					64231,64232,64233,64234,64235,64236,64237,64238,64239,64240,64241,64242,64243,64244,64245,64246,64247,64248,64249,64250,64251,64252,64253,64254,64255,64256,64257,64258,64259,64260,64261,64262,64263,64264,64265,64266,64267,64268,64269,64270,
					64271,64272,64273,64274,64275,64276,64277,64278,64279,64280,64281,64282,64283,64284,64285,64286,64287,64288,64289,64290,64291,64292,64293,64294,64295,64296,64297,64298,64299,64300,64301,64302,64303,64304,64305,64306,64307,64308,64309,64310,
					64311,64312,64313,64314,64315,64316,64317,64318,64319,64320,64321,64322,64323,64324,64325,64326,64327,64328,64329,64330,64331,64332,64333,64334,64335,64336,64337,64338,64339,64340,64341,64342,64343,64344,64345,64346,64347,64348,64349,64350,
					64351,64352,64353,64354,64355,64356,64357,64358,64359,64360,64361,64362,64363,64364,64365,64366,64367,64368,64369,64370,64371,64372,64373,64374,64375,64376,64377,64378,64379,64380,64381,64382,64383,64384,64385,64386,64387,64388,64389,64390,
					64391,64392,64393,64394,64395,64396,64397,64398,64399,64400,64401,64402,64403,64404,64405,64406,64407,64408,64409,64410,64411,64412,64413,64414,64415,64416,64417,64418,64419,64420,64421,64422,64423,64424,64425,64426,64427,64428,64429,64430,
					64431,64432,64433,64434,64435,64436,64437,64438,64439,64440,64441,64442,64443,64444,64445,64446,64447,64448,64449,64450,64451,64452,64453,64454,64455,64456,64457,64458,64459,64460,64461,64462,64463,64464,64465,64466,64467,64468,64469,64470,
					64471,64472,64473,64474,64475,64476,64477,64478,64479,64480,64481,64482,64483,64484,64485,64486,64487,64488,64489,64490,64491,64492,64493,64494,64495,64496,64497,64498,64499,64500,64501,64502,64503,64504,64505,64506,64507,64508,64509,64510,
					64511,64512,64513,64514,64515,64516,64517,64518,64519,64520,64521,64522,64523,64524,64525,64526,64527,64528,64529,64531,64532,64533,64534,64535,64536,64537,64538,64539,64540,64541,64542,64543,64544,64545,64546,64547,64548,64549,64550,64551,
					64552,64553,64554,64555,64556,64557,64558,64559,64560,64561,64562,64563,64564,64565,64566,64567,64568,64569,64570,64571,64572,64573,64574,64575,64576,64577,64578,64579,64580,64581,64582,64583,64584,64585,64586,64587,64588,64589,64590,64591,
					64592,64593,64594,64595,64596,64597,64598,64599,64600,64601,64602,64603,64604,64605,64606,64607,64608,64609,64610,64611,64612,64613,64614,64615,64616,64617,64618,64619,64620,64621,64622,64623,64624,64625,64626,64627,64628,64629,64630,64631,
					64632,64633,64634,64635,64636,64637,64638,64639,64640,64641,64642,64643,64644,64645,64646,64647,64648,64649,64650,64651,64652,64653,64654,64655,64656,64657,64658,64659,64660,64661,64662,64663,64664,64665,64666,64667,64668,64669,64670,64671,
					64672,64673,64674,64675,64676,64677,64678,64679,64680,64681,64682,64683,64684,64685,64686,64687,64688,64689,64690,64691,64692,64693,64694,64695,64696,64697,64698,64699,64700,64701,64702,64703,64704,64705,64706,64707,64708,64709,64710,64711,
					64712,64713,64714,64715,64716,64717,64718,64719,64720,64721,64722,64723,64724,64725,64726,64727,64728,64729,64730,64731,64732,64733,64734,64735,64736,64737,64738,64739,64740,64741,64742,64743,64744,64745,64746,64747,64748,64749,64750,64751,
					64752,64753,64754,64755,64756,64757,64758,64759,64760,64761,64762,64763,64764,64765,64766,64767,64768,64769,64770,64771,64772,64773,64774,64775,64776,64777,64778,64779,64780,64781,64782,64783,64784,64785,64786,64787,64788,64789,64790,64791,
					64792,64793,64794,64795,64796,64797,64798,64799,64800,64801,64802,64803,64804,64805,64806,64807,64808,64809,64810,64811,64812,64813,64814,64815,64816,64817,64818,64819,64820,64821,64822,64823,64824,64825,64826,64827,64828,64829,64830,64831,
					64832,64833,64834,64835,64836,64837,64838,64839,64840,64841,64842,64843,64844,64845,64846,64847,64848,64849,64850,64852,64853,64854,64855,64856,64857,64858,64859,64860,64861,64862,64863,64864,64865,64866,64867,64868,64869,64870,64871,64872,
					64873,64874,64875,64876,64877,64878,64879,64880,64881,64882,64883,64884,64885,64886,64887,64888,64889,64890,64891,64892,64893,64894,64895,64896,64897,64898,64899,64900,64901,64902,64903,64904,64905,64906,64907,64908,64909,64910,64911,64912,
					64913,64914,64915,64916,64917,64918,64919,64920,64921,64922,64923,64924,64925,64926,64927,64928,64929,64930,64931,64932,64933,64934,64935,64936,64937,64938,64939,64940,64941,64942,64943,64944,64945,64946,64947,64948,64949,64950,64951,64952,
					64953,64954,64955,64956,64957,64958,64959,64960,64961,64962,64963,64964,64965,64966,64967,64968,64969,64970,64971,64972,64973,64974,64975,64976,64977,64978,64979,64980,64981,64982,64983,64984,64985,64986,64987,64988,64989,64990,64991,64992,
					64993,64994,64995,64996,64997,64998,64999,65000,65001,65002,65003,65004,65005,65006,65007,65008,65009,65010,65011,65012,65013,65014,65015,65016,65017,65018,65019,65020,65021,65022,65023,65024,65025,65026,65027,65028,65029,65030,65031,65032,
					65033,65034,65035,65036,65037,65038,65039,65040,65041,65042,65043,65044,65045,65046,65047,65048,65049,65050,65051,65052,65053,65054,65055,65056,65057,65058,65059,65060,65061,65062,65063,65064,65065,65066,65067,65068,65069,65070,65071,65072,
					65073,65074,65075,65076,65077,65078,65079,65080,65081,65082,65083,65084,65085,65086,65087,65088,65089,65090,65091,65092,65093,65094,65095,65096,65097,65098,65099,65100,65101,65102,65103,65104,65105,65106,65107,65108,65109,65110,65111,65112,
					65113,65114,65115,65116,65117,65118,65119,65120,65121,65122,65123,65124,65125,65126,65127,65128,65129,65130,65131,65132,65133,65134,65135,65136,65137,65138,65139,65140,65141,65142,65143,65144,65145,65146,65147,65148,65149,65150,65151,65152,
					65153,65154,65155,65156,65157,65158,65159,65160,65161,65162,65163,65164,65165,65166,65167,65168,65169,65170,65171,65172,65173,65174,65175,65176,65177,65178,65179,65180,65181,65182,65183,65184,65185,65186,65187,65188,65189,65190,65191,65192,
					65193,65194,65195,65196,65197,65198,65199,65200,65201,65202,65203,65204,65205,65206,65207,65208,65209,65210,65211,65212,65213,65214,65215,65216,65217,65218,65219,65220,65221,65222,65223,65224,65225,65226,65227,65228,65229,65230,65231,65232,
					65233,65234,65235,65236,65237,65238,65239,65240,65241,65242,65243,65244,65245,65246,65247,65248,65249,65250,65251,65252,65253,65254,65255,65256,65257,65258,65259,65260,65261,65262,65263,65264,65265,65266,65267,65268,65269,65270,65271,65272,
					65273,65274,65275,65276,65277,65278,65279,65280,65281,65282,65283,65284,65285,65286,65287,65288,65289,65290,65291,65292,65293,65294,65295,65296,65297,65298,65299,65300,65301,65302,65303,65304,65305,65306,65307,65308,65309,65310,65311,65312,
					65313,65314,65315,65316,65317,65318,65319,65320,65321,65322,65323,65324,65325,65326,65327,65328,65329,65330,65331,65332,65333,65334,65335,65336,65337,65338,65339,65340,65341,65342,65343,65344,65345,65346,65347,65348,65349,65350,65351,65352,
					65353,65354,65355,65356,65357,65358,65359,65360,65361,65362,65363,65364,65365,65366,65367,65368,65369,65370,65371,65372,65373,65374,65375,65376,65377,65378,65379,65380,65381,65382,65383,65384,65385,65386,65387,65388,65389,65390,65391,65392,
					65393,65394,65395,65396,65397,65398,65399,65400,65401,65402,65403,65404,65405,65406,65407,65408,65409,65410,65411,65412,65413,65414,65415,65416,65417,65418,65419,65420,65421,65422,65423,65424,65425,65426,65427,65428,65429,65430,65431,65432,
					65433,65434,65435,65436,65437,65438,65439,65440,65441,65442,65443,65444,65445,65446,65447,65448,65449,65450,65451,65452,65453,65454,65455,65456,65457,65458,65459,65460,65461,65462,65463,65464,65465,65466,65467,65468,65469,65470,65471,65472,
					65473,65474,65475,65476,65477,65478,65479,65480,65481,65482,65483,65484,65485,65486,65487,65488,65489,65490,65491,65492,65493,65494,65495,65496,65497,65498,65499,65500,65501,65502,65503,65504,65505,65506,65507,65508,65509,65510,65511,65512,
					65513,65514,65515,65516,65517,65518,65519,65520,65521,65522,65523,65524,65525,65526,65527,65528,65529,65530,65531,65532,65533,65534,65535]),//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA...)
				// NoChildComplex
				(1, [-766])
			]),

			new("Pentaflake", 6, pfs, .2 * pfs, .25f, .9, pfx, pfy,
			[
				("Classic", [2 * pi / 10, 0, 0, 0, 0, 0]),
				("No Center Rotation", [2 * pi + 2 * pi / 5, 0, 0, 0, 0, 0])
			], [
				("Center", [2, 0, 0, 0, 0, 0]),
				("Center_3/2", [3, 0, 0, 0, 0, 0])
			],
			[
				// NoChildComplex
				(1, [-94]),	
				// NoBackDiag
				(2, [0,17,18,19,36,40,44,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63])
			]
			),

			new("Hexaflake", 7, 3, .5, .2, 1,
			[0, 0, 2 * stt, 2 * stt, 0, -2 * stt, -2 * stt],
			[0, -2, -1, 1, 2, 1, -1],
			[("Classic", [SYMMETRIC + pi / 3, 0, 0, 0, 0, 0, 0])],
			[
				("Center", [2, 0, 0, 0, 0, 0, 0]),
				("Y", [2, 0, 2, 0, 2, 0, 2]),
				("AntiY", [2, 0, 4, 0, 4, 0, 4]),
				("Center_3/2", [3, 0, 0, 0, 0, 0, 0]),
				("Y_3/2", [3, 0, 3, 0, 3, 0, 3]),
			], [
				// NoChildComplex
				(1, [-190]),
				// NoBackDiag
				(2, [0, 66, 129, 132, 133, 136, 137, 140, 141, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 272, 288, 304, 322, 336, 338, 352, 354, 368, 370, 385, 388, 389, 392, 393, 396, 397, 400, 401, 404, 405, 408, 409, 412, 413, 416, 417, 420, 421, 424, 425, 428, 429, 432, 433, 436, 437, 440, 441, 444, 445, 449, 450, 451, 452, 453, 454, 455, 456, 457, 458, 459, 460, 461, 462, 463, 464, 465, 466, 467, 468, 469, 470, 471, 472, 473, 474, 475, 476, 477, 478, 479, 480, 481, 482, 483, 484, 486, 487, 488, 489, 490, 491, 492, 493, 494, 495, 496, 497, 498, 499, 500, 501, 502, 503, 504, 505, 506, 507, 508, 509, 510])
			]
			),

			new("HexaCircular", 19, 5, .2, .05f, .9,
			[0, 2 * stt, stt, -stt, -2 * stt, -stt, stt, 4 * stt, 3 * stt, stt, -stt, -3 * stt, -4 * stt, -4 * stt, -3 * stt, -stt, stt, 3 * stt, 4 * stt],
			[0, 0, 1.5, 1.5, 0, -1.5, -1.5, 1, 2.5, 3.5, 3.5, 2.5, 1, -1, -2.5, -3.5, -3.5, -2.5, -1],
			[
				("180", [pi / 3, 0, 0, 0, 0, 0, 0, 0, pi, pi, 0, 0, pi, pi, 0, 0, pi, pi, 0]),
				("Symmetric", [SYMMETRIC + pi23, 0, 0, 0, 0, 0, 0, 0, pi, pi, 0, 0, pi, pi, 0, 0, pi, pi, 0])
			], [
				("Center", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Y", [1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Y2", [1, 0, 2, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Y", [0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Double_Y", [0, 2, 1, 2, 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Double_Y", [1, 2, 1, 2, 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_3/2", [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center_Y_3/2", [3, 0, 3, 0, 3, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Y_3/2", [0, 0, 3, 0, 3, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),

			], null
			)
			];
		var maxChildren = 1;
		foreach (var i in fractals) {
			if (i.childCount > maxChildren)
				maxChildren = i.childCount;
		}
		childColor = new short[maxChildren];
		childAngle = new double[maxChildren];
	}
	private void MakeTemp() {
		if (!Directory.Exists("temp"))
			_ = Directory.CreateDirectory("temp");
	}
	#endregion

	#region Generate_Tasks
	private static Vector3 SampleColor(Vector3[] set, double i) {
		var m = set.Length;
		var modi = i % m;
		var a = (int)Math.Floor(modi);
		return Vector3.Lerp(set[a], set[(a + 1) % m], (float)(modi - a));
	}
	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//private static Vector3 Y(Vector3 X) => new(X.Z, X.X, X.Y);
	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//private static Vector3 Z(Vector3 X) => new(X.Y, X.Z, X.X);
	private int GetGenerateLength() => applyGenerationType > GenerationType.OnlyImage && !applyPreviewMode ? bitmap.Length : previewFrames + 1;
	private void GenerateAnimation() {

#if CUSTOMDEBUG
		// Start a new DebugLog
		logString = "";
		Log(ref logString, "New Generate()");
		startTime = new();
		startTime.Start();
#endif
		restartGif = false;
		applyPalette = Colors[applyPaletteType = (short)(selectPaletteType < 0 ? random.Next(0, Colors.Count) : selectPaletteType)].Item2;
		applyPalette2 = 2 * applyPalette.Length;
		var cc = f.childColor[selectChildColor].Item2;
		if (f.childCount > 0) {
			var ca = f.childAngle[selectChildAngle].Item2;
			for (var i = f.childCount; 0 <= --i; childAngle[i] = ca[i]) ;
			for (var i = f.childCount; 0 <= --i; childColor[i] = cc[i]) ;
		}
		applyZoom = (short)(selectZoom > -2 ? selectZoom : random.NextDouble() < .5 ? -1 : 1);
		applyCutparam = selectCutparam < 0 ? random.Next(0, GetMaxCutparam()) : selectCutparam;
		applyHue = (short)(selectHue > -2 ? selectHue : random.Next(-1, 2));

		var m = (short)(applyHue == 0 && childColor[selectZoomChild] > 0 && applyZoom != 0 ? selectPeriodMultiplier * applyPalette.Length : selectPeriodMultiplier);
		bool asymmetric = childAngle[selectZoomChild] < 2.0 * Math.PI;
		bool doubled = (Math.Abs(selectSpin) > 1 && selectZoomChild == 0 || selectSpin == 0 && asymmetric) && applyZoom != 0;
		m = (short)(doubled ? 2 * m : m);
		// Get the multiplier of the basic period required to get to a seamless loop
		if (selectZoomChild == 0) {
			finalPeriodMultiplier = m;
			applyPeriodAngle = f.childCount <= 0 ? 0 : childAngle[0] % (2.0 * Math.PI);
			applyPeriodAngle = selectPeriodMultiplier % 2 == 0 && asymmetric && !doubled ? applyPeriodAngle * 2 : applyPeriodAngle;
		} else {
			double a = (childAngle[selectZoomChild] * m) % (2 * Math.PI);
			if (selectSpin == 0) {
				for (finalPeriodMultiplier = 1; a is > 0.1 and < 2 * Math.PI - 0.1; ++finalPeriodMultiplier)
					a = (a + childAngle[selectZoomChild] * m) % (2 * Math.PI);
				finalPeriodMultiplier *= m;
			} else {
				finalPeriodMultiplier = m;
			}
			applyPeriodAngle = a == 0 ? 2 * Math.PI : a;
		}
		// A complex expression to calculate the minimum needed hue shift speed to match the loop: supporting the new custom palettes:
		var finalHueShift = (finalPeriodMultiplier * cc[selectZoomChild]) % applyPalette2;
		if (finalHueShift == 0 || applyZoom == 0) {
			hueCycleMultiplier = (byte)applyPalette2;
		} else {
			if ((applyZoom > 0) != (applyHue > 0))
				finalHueShift = (applyPalette2 - finalHueShift) % applyPalette2;
			hueCycleMultiplier = (byte)(applyPalette2 - finalHueShift);
			//hueCycleMultiplier = 1;
			//for (int cycle = 0; (cycle = (cycle + finalHueShift) % applyPalette2) != 0; ++hueCycleMultiplier);
		}
		// A complex expression to calculate the minimum needed hue shift speed to match the loop: OLD
		//hueCycleMultiplier = (byte)(applyHue == 0 ? 0 : childColor[selectZoomChild] % applyPalette.Length == 0 ? 3 : 1 +
		//		(childColor[selectZoomChild] % applyPalette.Length == 1 == (1 == applyHue) == (1 == applyZoom) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3
		//);

		// setup bitmap data
		bitmapsFinished = nextBitmap = 0;
		tryPng = -1;
		var frames = (applyGenerationType = selectGenerationType) switch {
			GenerationType.AllSeedsGIF => GetMaxCutparam() + 1,
			GenerationType.AllSeedsMP4 => GetMaxCutparam() + 1,
			GenerationType.HashParam => cutparamMaximum + 1,
			_ => (short)(debug > 0 ? debug : selectPeriod * finalPeriodMultiplier),
		};
		if ((frames += previewFrames = Math.Max(0, (int)Math.Log2(Math.Min(selectWidth, selectHeight)) - 2)) != allocatedFrames) {
			bitmap = new Bitmap[allocatedFrames = frames];
			bitmapData = new BitmapData[frames];
			bitmapState = new BitmapState[frames + 1];
			
		}
		// Setup reseted PNG export states
		if (mp4Png == null || mp4Png.Length != frames - previewFrames)
			mp4Png = new bool[frames - previewFrames];
		else for (int i = frames - previewFrames; 0 <= --i; mp4Png[i] = false) ;
		// Setup reseted BitmapStates
		for (int b = frames; b >= 0; bitmapState[b--] = BitmapState.Queued) ;

		StartGif();
		// Initialize the starting default animation values
		double size = 2400, angle = selectDefaultAngle * Math.PI / 180.0;
		double hueAngle = selectDefaultHue;
		short color = 0;
		isWritingBitmaps = isFinishingBitmaps = 2;
		ambnoise = (short)(selectAmbient * selectNoise);
		var spin = selectSpin < -1 ? (short)random.Next(-2, 2) : selectSpin;
		applyBlur = (short)(selectBlur + 1);
		if (applyPreviewMode = selectPreviewMode) {
			var w = Math.Max(selectWidth, selectHeight) * f.maxSize * 0.1;
			size = w * f.childSize * .9;
		}
		bool toFinishAnimation = true;
		for (var i = selectDefaultZoom < 0 
			? random.Next(0, selectPeriod * finalPeriodMultiplier) 
			: (selectDefaultZoom % (selectPeriod * finalPeriodMultiplier)); 
			0 <= --i; IncFrameSize(ref size, selectPeriod)) ;
		// Pregenerate color blends
		if (applyGenerationType < GenerationType.AllSeedsGIF) 
			PregenerateParam(0, colorBlends, ref startCutParam);
		applyDetail = selectDetail;
#if SMOOTHDEBUG_DETAIL
		applyDetail = 10.0;
#endif
		// Generate the images
		while (!token.IsCancellationRequested) {

			toFinishAnimation |= ApplyGenerationType();
			if (restartGif) {
				restartGif = false;
				toFinishAnimation = true;
				// wait for all tasks to finish to preserve integrity, especially including gifs, and only return true if it tries to start new ones, so they actually finish:
				FinishTasks(true, true, (short taskIndex) => false);
				StopGif(null);
				StartGif();
			}
			if (tryPng < bitmap.Length) {
				// Use the selected ParallelType, or OfDepth if OnlyImage (if you want a GenerateDots_SingleTask like OfAnimation with OnlyImage, set the maxTasks to <= MINTASKS)
				applyParallelType = selectParallelType;
				// Initialize buffers (delete and reset if size changed)
				if ((applyMaxTasks = Math.Max((short)MINTASKS, selectMaxTasks)) != allocatedTasks) {
					if (allocatedTasks >= 0)
						for (var t = 0; t < allocatedTasks; ++t) //{
							tasks[t].Join();
							/*var task = tasks[t];
							if (task.taskStarted)
								task.Join();
						}*/
					rect = new(0, 0, allocatedWidth = selectWidth, allocatedHeight = selectHeight);
					tasks = new FractalTask[allocatedTasks = applyMaxTasks];
					tuples = new (short, (double, double), (double, double), short, long, byte)[applyMaxTasks * depthdiv];
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					if (applyMaxTasks > MINTASKS && (applyParallelType == ParallelType.OfDepth || previewFrames > 0))
						NewOfDepthBuffer(true, true);
					int vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					// Regular NeBuffer
					for (short t = applyMaxTasks; 0 <= --t;) {
						var task = tasks[t] = new FractalTask();
						task.taskIndex = t;
						NewBuffer(task, vw, vh);
					}
					SetMaxIterations();	
				}
				if (selectHeight != allocatedHeight) {
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					if (applyMaxTasks > MINTASKS && (applyParallelType == ParallelType.OfDepth || previewFrames > 0))
						NewOfDepthBuffer(false, true);
					int vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					for (short t = 0; t < applyMaxTasks; NewBuffer(tasks[t++], vw, vh)) ;
					rect = new(0, 0, allocatedWidth = selectWidth, allocatedHeight = selectHeight);
				}
				if (selectWidth != allocatedWidth) {
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					if (applyMaxTasks > MINTASKS && (applyParallelType == ParallelType.OfDepth || previewFrames > 0))
						NewOfDepthBuffer(false, false);

					int vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					for (short t = 0; t < applyMaxTasks; ++t) {
						var task = tasks[t];
						var buffT = task.buffer;
						var voidT = task.voidDepth;
						var noiseT = task.voidNoise;
						for (short y = 0; y < selectHeight; voidT[y++] = new short[selectWidth])
							buffT[y] = new Vector3[selectWidth];
						for (short y = 0; y < vh; noiseT[y++] = new Vector3[vw]) ;
					}
					rect = new(0, 0, allocatedWidth = selectWidth, selectHeight);
				}
				// reallocate noise buffer if the noise resolution has changed
				if (applyVoid != selectVoid + 1) {
					applyVoid = (short)(selectVoid + 1); // already reallocated the noise buffer
					int vw = selectWidth / applyVoid + 2, vh = selectHeight / applyVoid + 2;
					for (short t = 0; t < applyMaxTasks; t++) {
						var noiseT = tasks[t].voidNoise = new Vector3[vh][];
						for (var y = 0; y < vh; noiseT[y++] = new Vector3[vw]) ;
					}
				}
				applyMaxIterations = selectMaxIterations;
				// Wait if no more frames to generate
				if (tryPng >= GetGenerateLength())
					continue;
				
				// Image parallelism
				//imageTasks = applyParallelType == 2 ? [] : null;
				FinishTasks(false, true, (short taskIndex) => {
					if (nextBitmap >= GetGenerateLength())
						return false;// The task is finished, no need to wait for this one
					var bmp = nextBitmap++;
					double _size = bmp < previewFrames ? size / (1 << (previewFrames - bmp - 1)) : size, _angle = angle;
					double _hueAngle = hueAngle;
					var _spin = spin;
					var _color = color;
					//bitmapState[nextBitmap] = BitmapState.Dots; // i was getting queued state tasks, this solved that, so that just means they take a while to get started, not an error
					if (applyParallelType > ParallelType.OfAnimation || applyMaxTasks <= MINTASKS || bmp < previewFrames)
						GenerateDots(bmp, (short)(-taskIndex - 1), _size, _angle, _spin, _color, _hueAngle);
					else tasks[taskIndex].Start(bmp, () => GenerateDots(bmp, (short)(taskIndex + 1), _size, _angle, _spin, _color, _hueAngle));
					if (bmp >= previewFrames)
						IncFrameParameters(ref size, ref angle, ref hueAngle, spin, 1);
					return true; // A task finished, but started another one - keep checking before new master loop
				}
				);
			} else if (toFinishAnimation) {
				toFinishAnimation = false;
				FinishAnimation();
#if CUSTOMDEBUG
		var n = bitmap.Length / .1;
		logString = "Cs:\nInit: " + Math.Floor(initTimes / n) 
			+ "\nIter: " + Math.Floor(iterTimes / n) 
			+ "\nVoid: " + Math.Floor(voidTimes / n) 
			+ "\nDraw: " + Math.Floor(drawTimes / n) 
			+ "\nGifs: " + Math.Floor(gifsTimes / n)
			+ "\n" + logString;
		File.WriteAllText("log.txt", logString);
#endif
			}
		}
		mainTask = null;

		#region InitData
		void NewBuffer(FractalTask task, int vw, int vh) {
			// Initialized new buffer data (new task or height changed)
			var voidT = task.voidDepth = new short[selectHeight][];
			var buffT = task.buffer = new Vector3[selectHeight][];
			var noiseT = task.voidNoise = new Vector3[vh][];
			for (var y = 0; y < selectHeight; voidT[y++] = new short[selectWidth]) {
				var buffY = buffT[y] = new Vector3[selectWidth];
				//for (var x = 0; x < selectWidth; buffY[x++] = Vector3.Zero) ;
			}
			for (var y = 0; y < vh; noiseT[y++] = new Vector3[vw]) ;
		}
		void NewOfDepthBuffer(bool newTasks, bool newHeight) {
			if (newTasks)
				buffer = new Vector3[applyMaxTasks][][];
			for (short t = applyMaxTasks; 0 <= --t;) {
				var buffT = newHeight ? buffer[t] = new Vector3[selectHeight][] : buffer[t];
				for (var y = 0; y < selectHeight; ++y) {
					var buffY = buffT[y] = new Vector3[selectWidth];
					//for (var x = 0; x < selectWidth; buffY[x++] = Vector3.Zero) ;
				}
			}
		}
		#endregion
		#region GenerateTasks
		void PreviewResolution(FractalTask task) {
			if (task.bitmapIndex < previewFrames) {
				// bitmaps from previewFrames back to zero have increasingly halved resolution
				int div = 1 << (applyParallelType == ParallelType.OfDepth ? previewFrames - task.bitmapIndex : previewFrames - task.bitmapIndex - 1);
				task.applyWidth = (short)(selectWidth / div);
				task.applyHeight = (short)(selectHeight / div);
				// bloom gets halved too so the visible blur radius stays the same
				task.bloom0 = selectBloom / div;
			} else {
				// full resolution
				task.bloom0 = selectBloom;
				task.applyWidth = selectWidth;
				task.applyHeight = selectHeight;
			}
			// scaling constants
			task.widthBorder = (short)(task.applyWidth - 2);
			task.heightBorder = (short)(task.applyHeight - 2);
			task.bloom1 = task.bloom0 + 1;
			task.upleftStart = -task.bloom1;
			task.rightEnd = task.widthBorder + task.bloom1;
			task.downEnd = task.heightBorder + task.bloom1;
		}
		void PregenerateParam(int bitmapIndex, Dictionary<long, Vector3[]> blends, ref long startParam) {
			int[] m;
			startParam = applyGenerationType switch {
				// AllSeeds cutfunction seed selection - increments the set of unique seeds through bitmapIndex
				GenerationType.AllSeedsGIF => f.cutFunction != null && (m = f.cutFunction[selectCut].Item2) != null && m.Length > 0 && m[0] >= 0 ? -m[bitmapIndex] : -bitmapIndex,
				GenerationType.AllSeedsMP4 => f.cutFunction != null && (m = f.cutFunction[selectCut].Item2) != null && m.Length > 0 && m[0] >= 0 ? -m[bitmapIndex] : -bitmapIndex,
				// HashParam - increments the set of all seeds thorugh bitmapindex
				GenerationType.HashParam => -bitmapIndex,
				// Regular render - a single seed selected from the unique set through the user parameter
				_ => f.cutFunction != null && (m = f.cutFunction[selectCut].Item2) != null && m.Length > 0 && m[0] >= 0 ? -m[applyCutparam] : -applyCutparam,
			};
			//if (applyPreviewMode)
			//	return;
			// start searching for children sets at depth 3
			byte max = 3;
			blends.Clear();
			int prevcount;
			do {
				prevcount = blends.Count;
				blends.Clear();
				// iterato to the "max" depth, and collect all unique sets of children a parent can split into at that depth
				PregenerateColor(blends, 0, startParam, 0, max);
				++max; // the next iteration would be 1 iteration deeper
			} while (prevcount != blends.Count); // if the set didn't grow anymore, then we already have all the possible sets, so let's stop and keep that

			void PregenerateColor(Dictionary<long, Vector3[]> blends, int index, long inFlags, byte inDepth, byte max = 2) {
				int i = f.childCount;
				long newFlags;
				if (++inDepth < max) {
					// siplified iteration loop to the "max" depth, where we look at the sets of colored children a parent can split into
					// we are only concerned with the cutfunction, so we don't compute XY or angles or color shifts
					while (0 <= --i)
						if ((newFlags = CalculateFlags(i, inFlags)) >= 0)
							PregenerateColor(blends, i, newFlags, inDepth, max);
				} else {
					// initialize color counter
					var c = applyPalette;
					int m = c.Length;
					Vector3[] cb = new Vector3[applyPalette2];
					for (var mi = 0; mi < m; ++mi)
						cb[mi] = new Vector3(0, 0, 0);
					int n = 0;
					// count the colors of all the children this parent will split into using the cutfunction
					while (0 <= --i)
						if ((newFlags = CalculateFlags(i, inFlags)) >= 0) {
							for (var mi = 0; mi < applyPalette2; ++mi) {
								cb[mi] += SampleColor(c, .5 * (childColor[i] + mi));
							}
							++n;
						}
					// save the color mix to the map using the parent index and cutfunction memeory as the key
					// every parent with the same index and cutfunction memory should spawn the same set of children relative to itself
					// so this will be an effective lookup of what mix of colors the parent will split into so it prepare and color itself to that for seamlessness
					blends.TryAdd(index + f.childCount * (inFlags & ((1 << f.childCount) - 1)), cb);
				}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		(double, double) NewXY((double, double) inXY, (double, double) XY, double inAngle) {
			// scale and rotate the child XY from the parent
			double cs = Math.Cos(inAngle), sn = Math.Sin(inAngle);
			return (inXY.Item1 + XY.Item1 * cs - XY.Item2 * sn, inXY.Item2 - XY.Item2 * cs - XY.Item1 * sn);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		long CalculateFlags(int index, long inFlags) => cutFunction == null ? /*inFlags*/ 0 : cutFunction(index, inFlags, f);
		void GenerateDots(int bitmapIndex, short stateIndex, double size, double angle, short spin, short color, double hueAngle) {

#if CUSTOMDEBUG
			var threadString = "";
			Stopwatch initTime = new();
			initTime.Start();
#endif
			// we have started generating dots for this bitmap
			bitmapState[bitmapIndex] = BitmapState.Dots;
			// get task pointers
			var taskIndex = (short)(Math.Abs(stateIndex) - 1);
			var state = stateIndex < 0 ? null : tasks[stateIndex - 1];
			var task = tasks[taskIndex];
			task.bitmapIndex = bitmapIndex;
			var buffT = task.buffer;
			// prepare the resolution and scaling constants
			PreviewResolution(task);
			// Init buffer with zeroes
			bool ofdepth = applyMaxTasks > MINTASKS && (applyParallelType == ParallelType.OfDepth || bitmapIndex < previewFrames);
			for (var y = 0; y < task.applyHeight; ++y) {
				var buffY = buffT[y];
				for (var x = 0; x < task.applyWidth; buffY[x++] = Vector3.Zero) ;
			}
			// Init OfDepth buffer with zeroes
			if (ofdepth) {
				for (var t = 0; t < applyMaxTasks; ++t) {
					var _buffT = buffer[t];
					for (var y = 0; y < task.applyHeight; ++y) {
						var _buffY = _buffT[y];
						for (var x = 0; x < task.applyWidth; _buffY[x++] = Vector3.Zero) ;
					}
				}
			}
#if CUSTOMDEBUG
			initTime.Stop();
			Log(ref threadString, "Init:" + bitmapIndex + " time = " + initTime.Elapsed.TotalMilliseconds + " ms.");
			Stopwatch iterTime = new();
			iterTime.Start();
#endif
			// Generate the fractal frame recursively
			if (token.IsCancellationRequested) {
				if (state != null)
					state.state = TaskState.Done;
				return;
			}
			for (var b = 0; b < applyBlur; ++b) {
				// puts the animation parameters into the correct range (scale, switch parent and childs, to have it at a size that fills the screen)
				ModFrameParameters(task.applyWidth, task.applyHeight, ref size, ref angle, ref spin, ref color, ref hueAngle);

				// Prepare Color blending per one dot (hueshifting + iteration correction) and starting cutparameter
				// So that the color of the dot will slowly approach the combined colors of its childer before it splits
				long startParam = 0;
				//task.H = applyGenerationType >= GenerationType.AllSeeds ? task.F : colorBlends;
				// if we are doing all params we will need to do this step fresh locally for each frame



				if (applyGenerationType >= GenerationType.AllSeedsGIF)
					PregenerateParam(bitmapIndex, task.F, ref startParam);
				else {
					task.F = [];
					foreach (var kvp in colorBlends)
						task.F[kvp.Key] = (Vector3[])kvp.Value.Clone();
					startParam = startCutParam; // otherwise we just use the pregenerated stuff we got at the beginning, and just use the selected cut seed
				}
				task.H.Clear();
				// Preiterate values that change the same way as iteration goes deeper, so they only get calculated once
				var preIterateTask = task.preIterate;
				if (preIterateTask == null || preIterateTask.Length != applyMaxIterations) {
					preIterateTask = task.preIterate = new (double, float, (double, double)[])[applyMaxIterations];
					for (int i = 0; i < applyMaxIterations; preIterateTask[i++] = (0.0, 0.0f, null)) ;
				}
				double inSize = size;
				// zooming into a non center chld needs to preitarate 6 levels deeper (or more precisely from 6 levels above)
				int totalMaxIterations = applyMaxIterations;//selectZoomChild > 0 ? applyMaxIterations : applyMaxIterations - 6;
				for (int i = 0; i < totalMaxIterations; ++i) {
					// get a new scale of the lower level, and also from that:
					// get a progress between splits (so that the parent wil lsmoothly turn into a preparation to split into children seamlessly)
					preIterateTask[i].Item2 = (float)Math.Log(applyDetail / (preIterateTask[i].Item1 = inSize)) / logBase;
					var inDetail = preIterateTask[i].Item2 = (float)Math.Log(applyDetail / (preIterateTask[i].Item1 = inSize)) / logBase;
					var inDetailSize = -inSize * Math.Max(-1, inDetail);
					if (preIterateTask[i].Item3 == null || preIterateTask[i].Item3.Length < f.childCount)
						preIterateTask[i].Item3 = new (double, double)[f.childCount];
					// precalculate the children XY shifts (scaled with scale and collapsed into the parent's location when freshly spawned)
					for (int c = 0; c < f.childCount; ++c)
#if SMOOTHNESSDEBUG_XY
						preIterateTask[i].Item3[c] = (f.childX[c] * inSize, f.childY[c] * inSize);
#else
						preIterateTask[i].Item3[c] = (f.childX[c] * inDetailSize, f.childY[c] * inDetailSize);
#endif
					if (inSize < applyDetail) {
						// This is the final level, lerp the detail here:
						foreach (var C in task.F) {
							var Lerped = new Vector3[C.Value.Length];
							for (int di = 0; di < C.Value.Length; ++di) {
								Lerped[di] = applyPreviewMode
									? SampleColor(applyPalette, di * .5 + hueAngle) // Preview just samples the pure palette
									: Vector3.Lerp(SampleColor(task.F[C.Key], di + 2 * hueAngle), SampleColor(applyPalette, di * .5 + hueAngle), inDetail); // otherwise we transform the pure palette into the children mixes as we approach getting split
							}
							task.H[C.Key] = Lerped;
						}
						break;
					}
					inSize /= f.childSize;
				}
				// ZoomChild shift:
				double _angle = angle;
				double _size = size;
				double posX = 0, posY = 0;
				// First three iterations into child 0
				for (int i = 0; i < 3; i++) {
					_angle += childAngle[0];  // Accumulate rotation
					_size /= f.childSize;        // Shrink for next step
				}
				// Compute infinite sum for zoomChild
				var infiniteSum = new Complex(f.childX[selectZoomChild], f.childY[selectZoomChild])
					/ (new Complex(1.0, 0.0) - new Complex(Math.Cos(childAngle[selectZoomChild]), Math.Sin(childAngle[selectZoomChild])) / f.childSize);
				// Transform infinite sum into the new coordinate system
				double cosFinal = Math.Cos(-_angle);
				double sinFinal = Math.Sin(-_angle);
				// Calculate the location of the child we are zooming into relative to the center, to shift it to the center
				double sX = posX + _size * (cosFinal * infiniteSum.Real - sinFinal * infiniteSum.Imaginary);
				double sY = posY + _size * (sinFinal * infiniteSum.Real + cosFinal * infiniteSum.Imaginary);
				// We will use OfDepth, if generating previews, or if we selected so and have enough allowed threads for that
				if (ofdepth) {
					tuples[0] = (task.taskIndex, (task.applyWidth * .5 - sX, task.applyHeight * .5 - sY), (angle, Math.Abs(spin) > 1 ? 2 * angle : 0), color, startParam, 0);
					GenerateDots_OfDepth(bitmapIndex);
					// OfDepth buffer merge
					var maxGenerationTasks = (short)Math.Max(1, applyMaxTasks - 1);
					var po = new ParallelOptions {
						MaxDegreeOfParallelism = maxGenerationTasks,
						CancellationToken = token
					};
					try {
						Parallel.For(0, task.applyHeight, po, y => {
							//for (var y = 0; y < task.applyHeight; ++y) {
							for (var t = 0; t < applyMaxTasks; ++t) {
								var _buffY = buffer[t][y];
								var buffY = buffT[y];
								for (var x = 0; x < task.applyWidth; ++x)
									buffY[x] += _buffY[x];
							}
						});
					} catch (Exception) { }
				} else GenerateDots_SingleTask(taskIndex, buffT, (task.applyWidth * .5 - sX, task.applyHeight * .5 - sY), (angle, Math.Abs(spin) > 1 ? 2 * angle : 0), color, startParam, 0);
				// Increment the animation parameters (rotate and zoom a little further), but only for the actual animation, not between previews
				if (bitmapIndex >= previewFrames)
					IncFrameParameters(ref size, ref angle, ref hueAngle, spin, applyBlur);
				if (token.IsCancellationRequested) {
					if (state != null)
						state.state = TaskState.Done;
					return;
				}
			}
#if CUSTOMDEBUG
			iterTime.Stop();
			Log(ref threadString, "Iter:" + task.bitmapIndex + " time = " + iterTime.Elapsed.TotalMilliseconds + " ms.");
			Monitor.Enter(taskLock);
			try {
				initTimes += initTime.Elapsed.TotalMilliseconds;
				iterTimes += iterTime.Elapsed.TotalMilliseconds;
				Log(ref logString, threadString);
			} finally { Monitor.Exit(taskLock); }
#endif
			if (state != null) // OfAnimation - continue directly with the nexts steps such as void and gif in this same task:
				GenerateImage(task);
			else // OfDepth - start continuation in a new task:
				task.Start(bitmapIndex, () => GenerateImage(task));

			#region GenerateDots_Inline
			void ApplyDot(FractalTask task, Vector3[][] buffT, long key, double inX, double inY, float inDetail, short inColor) {
				var dotColor = task.H[key][inColor];
				//var buffT = task.buffer;
				// make the range of X and Y values for the dot to paint
				// maybe just bloom0 is enough for these bounds...?
				int startX = Math.Max(1, (int)Math.Floor(inX - task.bloom0)),
					endX = Math.Min(task.widthBorder, (int)Math.Ceiling(inX + task.bloom0)),
					endY = Math.Min(task.heightBorder, (int)Math.Ceiling(inY + task.bloom0));
				for (int x, y = Math.Max(1, (int)Math.Floor(inY - task.bloom0)); y <= endY; ++y) {
					// gradient from 0 to max to 0 over the y range
					var yd = task.bloom1 - Math.Abs(y - inY);// Math.Max(0, task.bloom1 - Math.Abs(y - inY));
					var buffY = buffT[y];
					for (x = startX; x <= endX; ++x)
						// combine with gradient from 0 to max to 0 over the x range, apply with the computed color
						buffY[x] += (float)(yd * (task.bloom1 - Math.Abs(x - inX))) * dotColor;
					//buffY[x] += (float)(yd * Math.Max(0, task.bloom1 - Math.Abs(x - inX))) * dotColor;
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			bool TestSize(FractalTask task, double newX, double newY, double inSize) {
				// tests whether at least a part of this shape is within the image, if not, it will get skipped
				var testSize = inSize * f.cutSize;
				return Math.Min(newX, newY) + testSize > task.upleftStart && newX - testSize < task.rightEnd && newY - testSize < task.downEnd;
			}
			#endregion
			#region GenerateDots
			void GenerateDots_SingleTask(short taskIndex, Vector3[][] buffT,
				(double, double) inXY, (double, double) inAngle, short inColor, long inFlags, byte inDepth
			) {
				var task = tasks[taskIndex];
				var preIterated = task.preIterate[inDepth];
				var newPreIterated = task.preIterate[++inDepth];
				if (newPreIterated.Item1 < applyDetail) {
					// we are deep enough that the parent is within a pixel, so jsut split it one last time and draw it's children as dots
					for (int i = 0; i < f.childCount; ++i) {
						if (token.IsCancellationRequested)
							return;
						// Special Cutoff
						var newFlags = CalculateFlags(i, inFlags);
						if (newFlags < 0)
							continue;
						// Outside View
						var XY = preIterated.Item3[i];
						var newXY = NewXY(inXY, XY, inAngle.Item1);
						if (TestSize(task, newXY.Item1, newXY.Item2, preIterated.Item1))
							ApplyDot(task, buffT, i + f.childCount * (newFlags & (((long)1 << f.childCount) - 1)), newXY.Item1, newXY.Item2, newPreIterated.Item2, applyPreviewMode && inDepth > 1 ? inColor : (short)((inColor + childColor[i]) % applyPalette2));
					}
					return;
				}
				// Split parent deeper into new smaller parents
				for (int i = 0; i < f.childCount; ++i) {
					if (token.IsCancellationRequested)
						return;
					// Special Cutoff
					var newFlags = CalculateFlags(i, inFlags);
					if (newFlags < 0)
						continue;
					// Outside View
					var XY = preIterated.Item3[i];
					var newXY = NewXY(inXY, XY, inAngle.Item1);
					if (TestSize(task, newXY.Item1, newXY.Item2, preIterated.Item1))
						GenerateDots_SingleTask(taskIndex, buffT, newXY,
							i == 0
							? (inAngle.Item1 + childAngle[i] - inAngle.Item2, -inAngle.Item2)
							: (inAngle.Item1 + childAngle[i], inAngle.Item2),
							applyPreviewMode && inDepth > 1 ? inColor : (short)((inColor + childColor[i]) % applyPalette2), newFlags, inDepth);
				}
			}
			void GenerateDots_OfDepth(int bitmapIndex) {
				int index = 0, insertTo = 1,
					max = applyMaxTasks * depthdiv,
					maxcount = max - f.childCount - 1,
					count = (max + insertTo - index) % max;
				// keep spliting parents until we have a queue divdepth * maxTasks large:
				while (count > 0 && count < maxcount) {
					// take a parent form the queue to split into move parent to put back into the queue
					var (taskIndex, inXY, inAngle, inColor, inFlags, inDepth) = tuples[index++];
					index %= max;
					var task = tasks[taskIndex];
					var preIterated = task.preIterate[inDepth];
					var newPreIterated = task.preIterate[++inDepth];
					// Draw Dots
					if (newPreIterated.Item1 < applyDetail) {
						// we are deep enough that the parent is within a pixel, so jsut split it one last time and draw it's children as dots
						for (int i = 0; i < f.childCount; ++i) {
							if (token.IsCancellationRequested)
								return;
							// Special Cutoff
							var newFlags = CalculateFlags(i, inFlags);
							if (newFlags < 0)
								continue;
							// Outside View
							var XY = preIterated.Item3[i];
							var newXY = NewXY(inXY, XY, inAngle.Item1);
							if (TestSize(task, newXY.Item1, newXY.Item2, preIterated.Item1))
								ApplyDot(task, task.buffer, i + f.childCount * (newFlags & ((1 << f.childCount) - 1)), newXY.Item1, newXY.Item2, newPreIterated.Item2, applyPreviewMode && inDepth > 1 ? inColor : (short)((inColor + childColor[i]) % applyPalette2));
						}
						count = (max + insertTo - index) % max;
						continue;
					}
					// Split parent deeper into new smaller parents
					for (int i = 0; i < f.childCount; ++i) {
						if (token.IsCancellationRequested)
							return;
						// Special Cutoff
						var newFlags = CalculateFlags(i, inFlags);
						if (newFlags < 0)
							continue;
						// Outside View
						var XY = preIterated.Item3[i];
						var newXY = NewXY(inXY, XY, inAngle.Item1);
						if (TestSize(task, newXY.Item1, newXY.Item2, preIterated.Item1)) {
							tuples[insertTo++] =
								(taskIndex, newXY,
								i == 0 ? (inAngle.Item1 + childAngle[i] - inAngle.Item2, -inAngle.Item2) : (inAngle.Item1 + childAngle[i], inAngle.Item2),
								applyPreviewMode && inDepth > 1 ? inColor : (short)((inColor + childColor[i]) % applyPalette2), newFlags, inDepth);
							insertTo %= max; // we have added the new parent into the queue
						}
					}
					// refresh the count before we ask if this count is enough
					count = (max + insertTo - index) % max;
				}
				// we now have a nice number of tasks to perform in parallel, so do that:
				// false argument makes sure that we only finish processing these iteration tasks, we can exit if other kinds of tasks are still running as we don't need to wait for these here
				FinishTasks(false, false, (short taskIndex) => {
					// until the queue is empty
					if (count <= 0)
						return false;
					// take a parent from queue and iterate it normally with single taks code, but as its own parallel task
					var tupleIndex = index++;
					tasks[taskIndex].Start(bitmapIndex, () => {
						var (bufferIndex, inXY, inAngle, inColor, inFlags, inDepth) = tuples[tupleIndex];
						GenerateDots_SingleTask(bufferIndex, buffer[taskIndex], inXY, inAngle, inColor, inFlags, inDepth);
						tasks[taskIndex].state = TaskState.Done;
					});
					index %= max;
					count = (max + insertTo - index) % max;
					return true;
				});
			}
			#endregion
		}
		void GenerateImage(FractalTask task) {
#if CUSTOMDEBUG
			var threadString = "";
			Stopwatch voidTime = new();
			voidTime.Start();
#endif
			// Generate the grey void areas
			bitmapState[task.bitmapIndex] = BitmapState.Void;
			var voidT = task.voidDepth;
			var buffT = task.buffer;
			var queueT = task.voidQueue;
			task.lightNormalizer = 0.1f;
			task.voidDepthMax = 1.0f;
			short voidYX, w1 = (short)(task.applyWidth - 1), h1 = (short)(task.applyHeight - 1);
			if (selectAmbient > 0) {
				// Void Depth Seed points (no points, no borders), leet the void depth generator know where to start incrementing the depth
				float lightMax;
				for (short y = 1; y < h1; ++y) {
					if (token.IsCancellationRequested) {
						queueT.Clear();
						task.state = TaskState.Done;
						return;
					}
					var voidY = voidT[y];
					var buffY = buffT[y];
					for (short x = 1; x < w1; ++x) {
						var buffYX = buffY[x];
						task.lightNormalizer = Math.Max(task.lightNormalizer, lightMax = Math.Max(buffYX.X, Math.Max(buffYX.Y, buffYX.Z)));
						if (lightMax > 0) {
							voidY[x] = 0;
							queueT.Enqueue((y, x));
						} else voidY[x] = -1;
					}
					voidY[0] = voidY[w1] = 0;
					queueT.Enqueue((y, 0));
					queueT.Enqueue((y, w1));
				}
				var void0 = voidT[0];
				var voidH = voidT[h1];
				for (short x = 0; x < task.applyWidth; ++x) {
					void0[x] = voidH[x] = 0;
					queueT.Enqueue((0, x));
					queueT.Enqueue((h1, x));
				}
				// Depth of Void (fill the void of incrementally larger values of depth, that will generate the grey areas)
				short voidMax = 0;
				while (queueT.Count > 0) {
					(var y, var x) = queueT.Dequeue();
					short ym = (short)(y - 1), yp = (short)(y + 1), xm = (short)(x - 1), xp = (short)(x + 1);
					voidMax = Math.Max(voidMax, voidYX = (short)(voidT[y][x] + 1));
					if (xp < task.applyWidth && voidT[y][xp] == -1) { voidT[y][xp] = voidYX; queueT.Enqueue((y, xp)); }
					if (yp < task.applyHeight && voidT[yp][x] == -1) { voidT[yp][x] = voidYX; queueT.Enqueue((yp, x)); }
					if (xm >= 0 && voidT[y][xm] == -1) { voidT[y][xm] = voidYX; queueT.Enqueue((y, xm)); }
					if (ym >= 0 && voidT[ym][x] == -1) { voidT[ym][x] = voidYX; queueT.Enqueue((ym, x)); }
				}
				task.voidDepthMax = voidMax;
			} else
				for (short y = 0; y < task.applyHeight; ++y) {
					if (token.IsCancellationRequested) {
						task.state = TaskState.Done;
						return;
					}
					var buffY = buffT[y];
					for (short x = 0; x < task.applyWidth; ++x) {
						var buffYX = buffY[x];
						task.lightNormalizer = Math.Max(task.lightNormalizer, Math.Max(buffYX.X, Math.Max(buffYX.Y, buffYX.Z)));
					}
				}
			task.lightNormalizer = selectBrightness * 2.55f / task.lightNormalizer;
			if (token.IsCancellationRequested) {
				task.state = TaskState.Done;
				return;
			}
#if CUSTOMDEBUG
			voidTime.Stop();
			Log(ref threadString, "Void:" + task.bitmapIndex + " time = " + voidTime.Elapsed.TotalMilliseconds + " ms.");
			Stopwatch drawTime = new();
			drawTime.Start();
#endif
			// Draw the generated pixel to bitmap data
			unsafe {

				// Make a locked bitmap, remember the locked state
				var p = (byte*)(void*)(bitmapData[task.bitmapIndex] = (bitmap[task.bitmapIndex] = new(task.applyWidth, task.applyHeight)).LockBits( // make a new bitmaps and lock it's bits
						(applyParallelType == ParallelType.OfDepth ? task.bitmapIndex : task.bitmapIndex + 1) < previewFrames ? new Rectangle(0, 0, task.applyWidth, task.applyHeight) : rect, // The ractangle (use a new smaller one for smaller previews)
						ImageLockMode.ReadWrite, // Writing now, possibly reading for encoding gif later
						System.Drawing.Imaging.PixelFormat.Format24bppRgb // BGR format (no alpha, if the gif is transparent, the transparency is just the color black)
					)).Scan0;
				bitmapState[task.bitmapIndex] = BitmapState.Drawing;
				// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
				// Switch between th selected settings such as saturation, noise, image parallelism...
				var maxGenerationTasks = (short)Math.Max(1, applyMaxTasks - 1);
				int wv = task.applyWidth / applyVoid + 2;
				var stride = bitmapData[task.bitmapIndex].Stride;
				byte* rp;
				// Single Threaded
				if (ambnoise > 0 && applyGenerationType != GenerationType.HashParam) {
					for (int y = 0; y < task.applyHeight / applyVoid + 2; ++y) {
						var r = random;
						var v = task.voidNoise[y];
						for (int x = 0; x < wv; ++x)
							v[x] = new Vector3(r.Next(ambnoise), r.Next(ambnoise), r.Next(ambnoise));
					};
					if (selectSaturate > 0.0) for (short y = 0; y < task.applyHeight; ++y) {
							if (token.IsCancellationRequested)
								break;
							rp = p + y * stride;
							NoiseSaturate(task, y, ref rp);
						}
					else for (short y = 0; y < task.applyHeight; ++y) {
							if (token.IsCancellationRequested)
								break;
							rp = p + y * stride;
							NoiseNoSaturate(task, y, ref rp);
						}
				} else {
					if (selectSaturate > 0.0) for (short y = 0; y < task.applyHeight; ++y) {
							if (token.IsCancellationRequested)
								break;
							rp = p + y * stride;
							NoNoiseSaturate(task, y, ref rp);
						}
					else for (short y = 0; y < task.applyHeight; ++y) {
							if (token.IsCancellationRequested)
								break;
							rp = p + y * stride;
							NoNoiseNoSaturate(task, y, ref rp);
						}
				}
				//}
				#region GenerateBitmap_Inline
				Vector3 Normalize(Vector3 pixel, float lightNormalizer) {
					float max = Math.Max(pixel.X, Math.Max(pixel.Y, pixel.Z));
					return lightNormalizer * max > 254.0f ? (254.0f / max) * pixel : lightNormalizer * pixel;
				}
				Vector3 ApplyAmbientNoise(Vector3 rgb, float Amb, float Noise, Vector3 rand)
					=> rgb += new Vector3(Amb) + Noise * rand;
				unsafe Vector3 ApplySaturate(Vector3 rgb) {
					// The saturation equation boosting up the saturation of the pixel (powered by the saturation slider setting)
					float m, min = Math.Min(Math.Min(rgb.X, rgb.Y), rgb.Z), max = Math.Max(Math.Max(rgb.X, rgb.Y), rgb.Z);
					return max <= min ? rgb : ((m = max * (float)selectSaturate / (max - min)) + 1 - (float)selectSaturate) * rgb - new Vector3(min * m);
				}
				unsafe void ApplyRGBToBytePointer(Vector3 rgb, ref byte* p) {
					// Without gamma:
					p[0] = (byte)rgb.Z;
					p[1] = (byte)rgb.Y;
					p[2] = (byte)rgb.X;
					// With gamma:
					/*
					p[0] = (byte)(255f * Math.Sqrt(rgb.Z / 255f));
					p[1] = (byte)(255f * Math.Sqrt(rgb.Y / 255f));
					p[2] = (byte)(255f * Math.Sqrt(rgb.X / 255f));
					*/
					p += 3;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoiseSaturate(FractalTask task, int y, ref byte* p) {
					var voidY = task.voidDepth[y];
					var buffY = task.buffer[y];
					var fy = (float)y / applyVoid;
					int startY = (int)Math.Floor(fy);
					var alphaY = fy - startY;
					//if (selectAmbient <= 0) // noise is always 0 if ambient is zero, so it should't even get to this function
					//	for (var x = 0; x < task.applyWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), ref p)) ;
					//else 
					for (var x = 0; x < task.applyWidth; ++x) {
						var voidAmb = voidY[x] / task.voidDepthMax;
						var fx = (float)x / applyVoid;
						int startX = (int)Math.Floor(fx);
						var alphaX = fx - startX;
						var vy0 = task.voidNoise[startY];
						var vy1 = task.voidNoise[startY + 1];
						ApplyRGBToBytePointer(ApplyAmbientNoise(ApplySaturate(Normalize(buffY[x], task.lightNormalizer)), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb,
							alphaY * (alphaX * vy1[startX + 1] + (1 - alphaX) * vy1[startX]) + (1 - alphaY) * (alphaX * vy0[startX + 1] + (1 - alphaX) * vy0[startX])), ref p);
					}
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoiseNoSaturate(FractalTask task, int y, ref byte* p) {
					var voidY = task.voidDepth[y];
					var buffY = task.buffer[y];
					var fy = (float)y / applyVoid;
					int startY = (int)Math.Floor(fy);
					var alphaY = fy - startY;
					//if (selectAmbient <= 0) // noise is always 0 if ambient is zero, so it should't even get to this function
					//	for (var x = 0; x < task.applyWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), ref p)) ;
					//else 
					for (var x = 0; x < task.applyWidth; ++x) {
						var voidAmb = voidY[x] / task.voidDepthMax;
						var fx = (float)x / applyVoid;
						int startX = (int)Math.Floor(fx);
						var alphaX = fx - startX;
						var vy0 = task.voidNoise[startY];
						var vy1 = task.voidNoise[startY + 1];
						ApplyRGBToBytePointer(ApplyAmbientNoise(Normalize(buffY[x], task.lightNormalizer), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb,
							alphaY * (alphaX * vy1[startX + 1] + (1 - alphaX) * vy1[startX]) + (1 - alphaY) * (alphaX * vy0[startX + 1] + (1 - alphaX) * vy0[startX])), ref p);
					}
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoNoiseSaturate(FractalTask task, int y, ref byte* p) {
					var buffY = task.buffer[y];
					if (selectAmbient > 0) {
						var voidY = task.voidDepth[y];
						for (var x = 0; x < task.applyWidth; ++x)
							ApplyRGBToBytePointer(new Vector3(selectAmbient * voidY[x] / task.voidDepthMax) + ApplySaturate(Normalize(buffY[x], task.lightNormalizer)), ref p);
					} else for (var x = 0; x < task.applyWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], task.lightNormalizer)), ref p)) ;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoNoiseNoSaturate(FractalTask task, int y, ref byte* p) {
					var buffY = task.buffer[y];
					if (selectAmbient > 0) {
						var voidY = task.voidDepth[y];
						for (var x = 0; x < task.applyWidth; ++x)
							ApplyRGBToBytePointer(new Vector3(selectAmbient * voidY[x] / task.voidDepthMax) + Normalize(buffY[x], task.lightNormalizer), ref p);
					} else for (var x = 0; x < task.applyWidth; x++)
							ApplyRGBToBytePointer(Normalize(buffY[x], task.lightNormalizer), ref p);
				}
				#endregion
			}
			if (token.IsCancellationRequested) {
				task.state = TaskState.Done;
				return;
			}
#if CUSTOMDEBUG
			drawTime.Stop();
			Log(ref threadString, "Draw:" + task.bitmapIndex + " time = " + drawTime.Elapsed.TotalMilliseconds + " ms.");
			Monitor.Enter(taskLock);
			try {
				voidTimes += voidTime.Elapsed.TotalMilliseconds;
				drawTimes += drawTime.Elapsed.TotalMilliseconds;
				Log(ref logString, threadString);
			} finally { Monitor.Exit(taskLock); }
#endif
			if (task.bitmapIndex < previewFrames) {
				bitmapState[task.bitmapIndex] = BitmapState.FinishedBitmap;
				TryFinishBitmap(false, task.taskIndex);
				task.state = TaskState.Done;
			} else {
				if (applyGenerationType is >= GenerationType.LocalGIF and <= GenerationType.AllSeedsGIF)
					GenerateGif(task);
				else {
					bitmapState[task.bitmapIndex] = BitmapState.DrawingFinished;
					task.state = TaskState.Done;
				}
			}
		}
		void GenerateGif(FractalTask task) {
#if CUSTOMDEBUG
			var threadString = "";
			Stopwatch gifsTime = new(); gifsTime.Start();
#endif
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
				if (gifEncoder != null) {
					bitmapState[task.bitmapIndex] = BitmapState.Encoding; // Start encoding Frame to a temp GIF	
					unsafe {
						var d = bitmapData[task.bitmapIndex];
						byte* ptr = (byte*)(void*)d.Scan0;
						gifEncoder.AddFrameParallel(ptr, d.Stride, gifToken, task.bitmapIndex - previewFrames);
						bitmapState[task.bitmapIndex] = BitmapState.EncodingFinished;
					}
				} else {
					StopGif(task);
					bitmapState[task.bitmapIndex] = BitmapState.DrawingFinished;
				}
			//}
#if CUSTOMDEBUG
			gifsTime.Stop();
			Log(ref threadString, "Gifs:" + task.bitmapIndex + " time = " + gifsTime.Elapsed.TotalMilliseconds + " ms.");
			Monitor.Enter(taskLock);
			try {
				gifsTimes += gifsTime.Elapsed.TotalMilliseconds;
				Log(ref logString, threadString);
			} finally { Monitor.Exit(taskLock); }
#endif
			//if (applyGenerationType == GenerationType.Mp4) bitmapState[task.bitmapIndex] = BitmapState.FinishedBitmap;
			gifThread = false;
			task.state = TaskState.Done;
		}
		/** Parallel threading management - will keep creating new threads unless cancelled and return when all threads are finished
		 * 
		 * @param mainLoop - being called from the main loop and not the OnDepth
		 * @param operation - gets called when a task is free, should return true if it created a new task
		 */
		void FinishTasks(bool cancel, bool mainLoop, Func<short, bool> lambda) {
			FractalTask task;
			for (int i = 3; i > 0; --i) {
				for (var tasksRemaining = true; tasksRemaining; MakeDebugString()) {
					tasksRemaining = false;
					for (short t = applyMaxTasks; 0 <= --t;)
						tasksRemaining |= (task = tasks[t]).IsStillRunning()
							? mainLoop || task.bitmapIndex >= 0 && bitmapState[task.bitmapIndex] <= BitmapState.Dots // Must finish all Dots threads, and if in main loop all secondary threads too (OnDepth can continu back to main loop when secondary threads are running so it could start a new OnDepth loop)
							: !(token.IsCancellationRequested || cancel) && ( // Cancel Request forbids any new threads to start
								!mainLoop || selectMaxTasks == applyMaxTasks && applyParallelType == selectParallelType && selectGenerationType == applyGenerationType // changing these settings yout exit, then they get updated and restart the main loop with them updated (except onDepth which must finish first)
							) && (mainLoop && (TryWriteBitmaps(task) || TryFinishBitmaps(task) || TryPngBitmaps(task)) || lambda(t)); // in the main loop we try Bitmap finishing and writing secondary threads (onDepth loop would get stuck )
					if (tasksRemaining)
						i = 3;
				}
				ApplyGenerationType();
			}
		}
		bool TryWriteBitmaps(FractalTask task) {
			if (token.IsCancellationRequested // Do not write gid frames when cancelled
				|| gifEncoder == null // ...or if gifencoder doens't exist
				|| gifEncoder.IsFinished() // ...or if it's finished
				|| applyGenerationType is < GenerationType.LocalGIF or > GenerationType.AllSeedsGIF // ...or we are not supposed to encode a gif
				|| isWritingBitmaps <= 0 // ...or this task is already running
				|| --isWritingBitmaps > 0) // ...or we have already ran it not too long ago
				return false;
			task.Start(-1, () => TryWriteBitmap(task.taskIndex));
			return true;
		}
		void TryWriteBitmap(int taskIndex) {
			while (!token.IsCancellationRequested) {
				if (bitmapsFinished >= bitmap.Length && !gifEncoder.IsFinished())
					gifEncoder.Finish();
				//if (applyGenerationType == GenerationType.Mp4) {
				//} else {
				int unlock = gifEncoder.FinishedFrame();
				// Try to finalize the previous encoder tasks
				switch (gifEncoder.TryWrite(true)) {
					case TryWrite.Failed:
						// fallback to only display animation without encoding
						StopGif(tasks[taskIndex]);
						isWritingBitmaps = 2;
						tasks[taskIndex].state = TaskState.Done;
						return;
					case TryWrite.FinishedFrame:
						// mark the bitmap state as fully finished
						bitmapState[unlock + previewFrames] = BitmapState.FinishedBitmap;
						break;
					default:
						// waiting or finished animation
						isWritingBitmaps = 2;
						tasks[taskIndex].state = TaskState.Done;
						return;
				}
				//}
			}
			isWritingBitmaps = 2;
			tasks[taskIndex].state = TaskState.Done;
		}
		bool TryFinishBitmaps(FractalTask task) {
			if (token.IsCancellationRequested)
				return false;
			bool gif = applyGenerationType is >= GenerationType.LocalGIF and <= GenerationType.AllSeedsGIF;
			if (gif && bitmapsFinished < bitmap.Length && bitmapsFinished >= previewFrames && !gifThread) {
				int tryEncode = bitmapsFinished;
				int maxTry = Math.Min(bitmap.Length, bitmapsFinished + applyMaxTasks);
				while (tryEncode < maxTry) {
					if (bitmapState[tryEncode] is >= BitmapState.DrawingFinished and <= BitmapState.UnlockedPNG) {
						if (bitmapState[tryEncode] is >= BitmapState.UnlockedRAM and <= BitmapState.UnlockedPNG)
							bitmapData[tryEncode] = bitmap[tryEncode].LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
						bitmapState[tryEncode] = BitmapState.Encoding;
						gifThread = false;//applyGenerationType == GenerationType.Mp4;
						task.Start(tryEncode, () => GenerateGif(task));
						return true;
					}
					++tryEncode;
				}
			}
			if (token.IsCancellationRequested // Do not write gid frames when cancelled
				|| bitmapsFinished < previewFrames // Do not finish preview bitmaps from here, the OfDepth calls it instead
				|| isFinishingBitmaps <= 0 // ...or this task is already running
				|| --isFinishingBitmaps > 0) // ...or we have already ran it not too long ago
				return false;
			task.Start(-2, () => TryFinishBitmap(gif, task.taskIndex));
			return true;
		}
		void TryFinishBitmap(bool gif, short taskIndex) {
			Monitor.Enter(taskLock);
			try {
				while (!token.IsCancellationRequested && bitmapsFinished < bitmap.Length && bitmapState[bitmapsFinished] >= (gif || bitmapState[bitmapsFinished] == BitmapState.Encoding ? BitmapState.FinishedBitmap : BitmapState.DrawingFinished)) {
					bitmapState[bitmapsFinished] = gif || bitmapsFinished < previewFrames ? BitmapState.Unlocked : BitmapState.UnlockedRAM;
					if (applyGenerationType == GenerationType.HashParam) {
						using SHA256 sha256 = SHA256.Create();
						unchecked {
							unsafe {
								int bytenum = bitmapData[bitmapsFinished].Stride * selectHeight;
								byte[] pixelData = new byte[bytenum];

								// Copy the raw pixel data from Scan0 to the byte array
								fixed (byte* dest = pixelData) {
									Buffer.MemoryCopy((void*)bitmapData[bitmapsFinished].Scan0, dest, bytenum, bytenum);
								}
								string key = BitConverter.ToString(sha256.ComputeHash(pixelData));
								hash.TryAdd(key, bitmapsFinished);
							}
						}
					}
					bitmap[bitmapsFinished].UnlockBits(bitmapData[bitmapsFinished]);
					// If not exporting PNGs jsut mark the PNG export as finished
					if (applyGenerationType is not GenerationType.AnimationMP4 and not GenerationType.AllSeedsMP4)
						bitmapState[bitmapsFinished] = BitmapState.UnlockedPNG;

					// Let the form know it's ready
					if (bitmapsFinished++ <= previewFrames && !token.IsCancellationRequested)
						UpdatePreview?.Invoke();
				}
				// Only called with bitmapIndex -2 is a unique task that needs to mark itself done, otherwise its a previewImage called within another task
				var task = tasks[taskIndex];
				if (task.bitmapIndex < 0)
					task.state = TaskState.Done;
			} finally { Monitor.Exit(taskLock); }
			isFinishingBitmaps = 2;
		}
		bool TryPngBitmaps(FractalTask task) {
			if (token.IsCancellationRequested || bitmapsFinished < previewFrames) // Do not write gid frames when cancelled
				return false;
			if (tryPng < 0) {
				tryPng = previewFrames;
				CleanupTempFiles();
			}
			if (applyGenerationType is not GenerationType.AnimationMP4 and not GenerationType.AllSeedsMP4) {
				tryPng = bitmapsFinished;
				return false;
			}
			while (tryPng < bitmap.Length && bitmapState[tryPng] == BitmapState.UnlockedPNG)
				++tryPng;
			if (tryPng >= bitmap.Length)
				return false;
			//var task = tasks[taskIndex];
			int bitmapIndex = tryPng;
			for (int mx = Math.Min(bitmap.Length, tryPng + applyMaxTasks); bitmapIndex < mx && bitmapState[bitmapIndex] != BitmapState.UnlockedRAM; ++bitmapIndex) ;
			if (bitmapState[bitmapIndex] != BitmapState.UnlockedRAM)
				return false;
			bitmapState[bitmapIndex] = BitmapState.UnlockedPNG;
			if (token.IsCancellationRequested) // Do not write gid frames when cancelled
				return false;
			task.Start(-3, () => TryPngBitmap(task.taskIndex, bitmapIndex));
			return true;
		}
		void TryPngBitmap(short taskIndex, int bitmapIndex) {
			int n = 1;
			for (int f = bitmap.Length - previewFrames; f >= 10; f /= 10)
				++n;
			string d = "D" + n.ToString();
			if (SaveMp4Png(bitmapIndex - previewFrames, d)) {
				tryPng = bitmapIndex;
				bitmapState[bitmapIndex] = BitmapState.UnlockedRAM;
			}
			tasks[taskIndex].state = TaskState.Done;
		}
		#endregion
		#region AnimationParams
		void ModFrameParameters(short width, short height, ref double size, ref double angle, ref short spin, ref short color, ref double hueAngle) {
			var w = Math.Max(width, height) * f.maxSize;
			var fp = f.childSize;
			if (applyPreviewMode)
				w *= 0.1;
			// Make sure the fractal is big enough to fill the screen even when i shift it to focus on the zoomChild
			if (selectZoomChild > 0)
				w *= fp * fp * fp;// * fp * fp * fp;
								  // Modulo rotation
			while (angle > Math.PI * 2)
				angle -= Math.PI * 2;
			while (angle < 0)
				angle += Math.PI * 2;
			// Modulo hue shift
			int m = applyPalette.Length;
			while (hueAngle >= m)
				hueAngle -= m;
			while (hueAngle < 0)
				hueAngle += m;
			// Swap Parent<->CenterChild after a full period
			while (size >= w * fp) {
				// Grown by f.childSize, swap parent to it's child
				size /= fp;
				if (applyPreviewMode)
					continue;
				angle += selectZoomChild == 0 ? childAngle[selectZoomChild] : -childAngle[selectZoomChild];
				SwitchParentChild(ref angle, ref spin, ref color, 1);
			}
			while (size < w) {
				// Shrank by f.childSize, swap child to it's parent
				size *= fp;
				if (applyPreviewMode)
					continue;
				angle += selectZoomChild == 0 ? -childAngle[selectZoomChild] : childAngle[selectZoomChild];
				SwitchParentChild(ref angle, ref spin, ref color, -1);
			}

			void SwitchParentChild(ref double angle, ref short spin, ref short color, short z) {
				if (applyPreviewMode)
					return;
				color = (short)((applyPalette2 + color + z * childColor[selectZoomChild]) % applyPalette2);
				if (Math.Abs(spin) <= 1)
					return;
				// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
				angle = -angle;
				spin = (short)-spin;
			}
		}
		void IncFrameParameters(ref double size, ref double angle, ref double hueAngle, double spin, short blur) {
			if (applyGenerationType is >= GenerationType.AllSeedsGIF and <= GenerationType.HashParam)
				return;
			var blurPeriod = selectPeriod * blur;
			// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
			angle += spin * (applyPeriodAngle * (1 + selectExtraSpin)) / (finalPeriodMultiplier * blurPeriod);
			hueAngle += (hueCycleMultiplier + applyPalette2 * selectExtraHue) * (float)applyHue / (finalPeriodMultiplier * blurPeriod * 2);
			IncFrameSize(ref size, blurPeriod);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IncFrameSize(ref double size, int period) => size *= Math.Pow(f.childSize, applyZoom * 1.0 / period);
		#endregion
	}
	bool ApplyGenerationType() {
		if (applyGenerationType == selectGenerationType)
			return false;

		if (applyGenerationType is >= GenerationType.OnlyImage and <= GenerationType.AnimationMP4 or >= GenerationType.AllSeedsMP4 && selectGenerationType is >= GenerationType.LocalGIF and <= GenerationType.AllSeedsGIF) {
			bitmapsFinished = Math.Min(previewFrames, bitmapsFinished);
			if (gifEncoder == null)
				StartGif();
		}
		applyGenerationType = selectGenerationType;
		return true;
	}
	void FinishAnimation() {
		// Wait for threads to finish
		bool tasksRunning = true;
		while (tasksRunning) {
			for (var t = allocatedTasks; 0 <= --t; tasks[t].Join()) ;
			tasksRunning = false;
			for (var t = allocatedTasks; 0 <= --t; tasksRunning |= tasks[t].state != TaskState.Free) ;
		}
		// Unlock unfinished bitmaps:
		for (var b = 0; b < bitmap.Length; ++b)
			if (bitmapState[b] is >= BitmapState.Drawing and < BitmapState.UnlockedRAM) {
				try {
					//bitmapState[bitmapsFinished] = BitmapState.Finished;
					bitmap[b]?.UnlockBits(bitmapData[b]);
				} catch (Exception) { }
			}
		FinishGif();
		MakeDebugString();
		if (applyGenerationType == GenerationType.HashParam) {
			string output = "";
			foreach (var i in hash)
				output += "," + i.Value;
			File.WriteAllText("hash.txt", output);
		}
	}
	void StartGif() {
		// Open a temp file to presave GIF to - Use AnimatedGifEncoder
		gifSuccess = 0;
		gifEncoder = null;
		hash.Clear();
		byte gifIndex = 0;
		gifToken = (gifCancel = new()).Token;
		switch (applyGenerationType = selectGenerationType) {
			case GenerationType.LocalGIF:
			case GenerationType.GlobalGIF:
			case GenerationType.AllSeedsGIF:

			gifEncoder = new();
			gifEncoder.SetDelay(selectDelay); // Framerate
			gifEncoder.SetRepeat(0);    // Loop
			gifEncoder.SetQuality(1);   // Highest quality
			gifEncoder.SetTransparent(selectAmbient < 0 ? Color.Black : Color.Empty);

			MakeTemp();

			while (gifIndex < 255) {
				gifTempPath = "temp/"+ filePrefix + "gif" + gifIndex.ToString() + ".tmp";
				if (!gifEncoder.Start(selectWidth, selectHeight, gifTempPath,
					selectGenerationType == GenerationType.GlobalGIF ? Gif.Components.ColorTable.GlobalSingle : Gif.Components.ColorTable.Local)
				) {
					++gifIndex;
					continue;
				} else break;
			}
			if (gifIndex == 255)
				gifEncoder = null;
				break;
			/*case GenerationType.Mp4:
				mp4Encoder = new();
				while (gifIndex < 255) {
					gifTempPath = "gif" + gifIndex.ToString() + ".tmp";
					if (!mp4Encoder.Start(selectWidth, selectHeight, gifTempPath, selectFps)) {
						++gifIndex;
						continue;
					} else break;
				}
				if (gifIndex == 255)
					gifEncoder = null;
				break;*/
		}
		// Flag the already encoded bitmaps to be encoded again;
		for (int b = previewFrames; b < bitmap.Length; ++b)
			if (bitmapState[b] >= BitmapState.Encoding) {
				if (bitmapState[b] == BitmapState.Unlocked)
					bitmapData[b] = bitmap[b].LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				bitmapState[b] = BitmapState.DrawingFinished;
			}
		bitmapsFinished = Math.Min(previewFrames, bitmapsFinished);
	}
	void FinishGif() {
		// Save the temp GIF file
		gifSuccess = 0;
		if (!token.IsCancellationRequested && applyGenerationType is >= GenerationType.LocalGIF and <= GenerationType.AllSeedsGIF) {
			if (gifEncoder != null) {

				if (!gifEncoder.IsFinished())
					gifEncoder.Finish();
			
				while (gifEncoder != null) {
					switch (gifEncoder.TryWrite()) {
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
							gifSuccess = Math.Max(selectWidth, selectHeight);
							// This will follow with gifEncoder.IsFinished()
							break;
					}
					if (gifEncoder.IsFinished())
						break;
				}
			}
			if (mp4Encoder != null) {
				mp4Encoder.Finish();
				gifSuccess = -1;
			}
		}
		gifEncoder = null;
		mp4Encoder = null;
	}
	void StopGif(FractalTask task) {
		gifCancel?.Cancel();
		applyGenerationType = GenerationType.AnimationRAM;
		foreach (var t in tasks) {
			if (t.bitmapIndex < 0) {
				if (t != task)
					t.Join();
			} else if (bitmapState[t.bitmapIndex] is >= BitmapState.Encoding and <= BitmapState.EncodingFinished) {
				if (t != task)
					t.Join();
				bitmapState[t.bitmapIndex] = BitmapState.DrawingFinished;
			}
		}
		gifEncoder?.Abort();
		gifEncoder = null;
	}
	#endregion

	#region Interface_Calls
	internal void CleanupTempFiles() {
		MakeTemp();
		string[] files = Directory.GetFiles("temp/", $"{filePrefix}*");
		foreach (string file in files) {
			try {
				File.Delete(file);
			} catch (Exception) {
				// Handle exceptions if needed (e.g., log the error)
				//Console.WriteLine($"Error deleting file {file}: {ex.Message}");
			}
		}
	}
	// start the generator in a separate main thread so that the form can continue being responsive
	internal void StartGenerate() => mainTask = Task.Run(GenerateAnimation, token = (cancel = new()).Token);
	internal short GetValidZoomChildren() {

		if (selectGenerationType >= GenerationType.AllSeedsGIF || f == null) {
			validZoomChildren = [0];
			return maxZoomChild = 0;
		}
		validZoomChildren = [];
		int[] m;


		long s = GetFractal().cutFunction != null && (m = GetFractal().cutFunction[selectCut].Item2) != null && m.Length > 0 && m[0] >= 0 ? -m[selectCutparam] : -selectCutparam;

		var cf = cutFunction = GetFractal().cutFunction == null || GetFractal().cutFunction.Count <= 0 ? null : Fractal.cutFunctions[GetFractal().cutFunction[selectCut].Item1].Item2;
		for (short i = 0; i < GetFractal().childCount; ++i)
			if (PregenerateChildren(cf, i, s, 0, 3))
				validZoomChildren.Add(i);
		return maxZoomChild = (short)(validZoomChildren.Count - 1);

		bool PregenerateChildren(Fractal.CutFunction cf, int index, long inFlags, byte inDepth, byte max = 3) {
			long newFlags;
			return ++inDepth >= max || (newFlags = CalculateFlags(cf, index, inFlags)) >= 0 && PregenerateChildren(cf, index, newFlags, inDepth, max);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			long CalculateFlags(Fractal.CutFunction cf, int index, long inFlags) => cf == null ? 0 : cf(index, inFlags, GetFractal());
		}
	}
	private List<short> validZoomChildren;

	internal void ResetGenerator() { }
	internal void RequestCancel() {
		cancel?.Cancel();
		gifCancel?.Cancel();
		try {
			mainTask?.Wait();
		} catch (Exception) { }
	}
	internal int SaveGif(string gifPath) {
		try {
			// Try to save (move the temp) the gif file
			//gifEncoder?.Finish();
			//gifEncoder.Output(gifPath);
			File.Delete(gifPath);
			File.Move(gifTempPath, gifPath);
		} catch (IOException ex) {
			var exs = "SaveGif: An error occurred: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (UnauthorizedAccessException ex) {
			var exs = "SaveGif: Access denied: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (Exception ex) {
			var exs = "SaveGif: Unexpected error: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		}
		return gifSuccess = 0;
	}
	internal int ConvertMp4(string gifPath, string mp4Path) {
		try {
			File.Delete(mp4Path);
			string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
			if (!File.Exists(ffmpegPath))
				return gifSuccess = 0;
			double gifFps = 1000.0 / (10 * selectDelay); // Convert to frames per second
			string arguments = $"-y -i \"{gifPath}\" -vf \"fps={selectFps},setpts=PTS*({gifFps}/{selectFps})\" -movflags +faststart -c:v libx264 -crf 0 -preset veryslow \"{mp4Path}\"";
			using Process ffmpeg = new Process();
			ffmpeg.StartInfo.FileName = ffmpegPath;
			ffmpeg.StartInfo.Arguments = arguments;
			ffmpeg.StartInfo.UseShellExecute = false;
			ffmpeg.StartInfo.RedirectStandardError = true;
			ffmpeg.StartInfo.RedirectStandardOutput = true;
			ffmpeg.StartInfo.CreateNoWindow = true;
			//ffmpeg.ErrorDataReceived += (sender, e) =>{if (!string.IsNullOrEmpty(e.Data)) {Console.WriteLine("FFmpeg error: " + e.Data);}};
			ffmpeg.Start();
			// Begin reading error asynchronously
			ffmpeg.BeginErrorReadLine();
			// Wait for the process to exit
			ffmpeg.WaitForExit();
			//File.Delete(gifTempPath); // do not delete the gif.tmp so the user could save the gif too if they wanted
		} catch (IOException ex) {
			var exs = "ConvertMp4: An error occurred: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (UnauthorizedAccessException ex) {
			var exs = "ConvertMp4: Access denied: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		} catch (Exception ex) {
			var exs = "ConvertMp4: Unexpected error: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return gifSuccess;
		}
		return gifSuccess = 0;
	}
	private bool SaveMp4Png(int i, string d) {
		if(i < 0 || mp4Png[i])
			return false;
		mp4Png[i] = true;
		try {
			MakeTemp();
			FileStream myStream = new("temp/"+ filePrefix + "image_" + i.ToString(d) + ".png", FileMode.Create);
			if (myStream == null)
				return true;
			bitmap[i + previewFrames].Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
			myStream.Close();
			return false;
		} catch (Exception) {
			mp4Png[i] = false;
			return true;
		}
	}
	internal int SaveMp4(string mp4Path) {
		try {
			File.Delete(mp4Path);
			int n = 1, nf = bitmap.Length - previewFrames;
			for (int f = nf; f >= 10; f /= 10) 
				++n;
			string d = "D" + n.ToString();
			int attempt = 0;
			for (int i = 0; i < nf; ++i)
				while (SaveMp4Png(i, d)) {
					if(++attempt > 10)
						return 0;
					Thread.Sleep(100);
				}
			string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
			if (!File.Exists(ffmpegPath))
				return 0;
			//string arguments = $"-y -framerate {selectFps} -i temp/"+ filePrefix + "image_%0{n}d.png -vf \"scale=iw:ih\" -movflags +faststart -c:v libx264 -profile:v main -preset veryslow -crf 18 -pix_fmt yuv420p \"{mp4Path}\"";

			string arguments = $"-y -framerate {selectFps} -i temp/{filePrefix}image_%0{n}d.png -vf \"scale=iw:ih\" -movflags +faststart -c:v libx264 -profile:v main -preset veryslow -crf 18 -pix_fmt yuv420p \"{mp4Path}\"";
			using Process ffmpeg = new Process();
			ffmpeg.StartInfo.FileName = ffmpegPath;
			ffmpeg.StartInfo.Arguments = arguments;
			ffmpeg.StartInfo.UseShellExecute = false;
			ffmpeg.StartInfo.RedirectStandardError = true;
			ffmpeg.StartInfo.RedirectStandardOutput = true;
			ffmpeg.StartInfo.CreateNoWindow = true;
			//ffmpeg.ErrorDataReceived += (sender, e) =>{if (!string.IsNullOrEmpty(e.Data)) {Console.WriteLine("FFmpeg error: " + e.Data);}};
			ffmpeg.Start();
			// Begin reading error asynchronously
			ffmpeg.BeginErrorReadLine();
			// Wait for the process to exit
			ffmpeg.WaitForExit();
			/*MakeTemp();
			for (int i = 0; i < nf; ++i) {
				File.Delete("temp/" + filePrefix + "image_" + i.ToString(d) + ".png");
			}*/

			/*arguments = $"-y -i temp/temp.mp4 -r 60 -c copy -movflags faststart \"{mp4Path}\"";
			using Process remux = new Process();
			remux.StartInfo.FileName = ffmpegPath;
			remux.StartInfo.Arguments = arguments;
			remux.StartInfo.UseShellExecute = false;
			remux.StartInfo.RedirectStandardError = true;
			remux.StartInfo.RedirectStandardOutput = true;
			remux.StartInfo.CreateNoWindow = true;
			//ffmpeg.ErrorDataReceived += (sender, e) =>{if (!string.IsNullOrEmpty(e.Data)) {Console.WriteLine("FFmpeg error: " + e.Data);}};
			remux.Start();
			// Begin reading error asynchronously
			remux.BeginErrorReadLine();
			// Wait for the process to exit
			remux.WaitForExit();*/
		} catch (IOException ex) {
			var exs = "SaveMp4: An error occurred: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return 0;
		} catch (UnauthorizedAccessException ex) {
			var exs = "SaveMp4: Access denied: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return 0;
		} catch (Exception ex) {
			var exs = "SaveMp4: Unexpected error: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return 1;
		}
		return 0;
	}


#if CUSTOMDEBUG
	private void Log(ref string log, string line) {
		Debug.WriteLine(line);
		log += "\n" + line;
	}
#endif
	private void MakeDebugString() {
		string GetTaskState(int bmp) => bmp switch {
			-3 => "EXPORTING PNG",
			-2 => "FINISHING",
			-1 => "WRITING",
			_ => GetBitmapState(bitmapState[bmp])
		};
		string GetBitmapState(BitmapState state) => state switch {
			BitmapState.Queued => "QUEUED (NOT SPAWNED)",
			BitmapState.Dots => "GENERATING FRACTAL DOTS",
			BitmapState.Void => "GENERATING DIJKSTRA VOID",
			BitmapState.Drawing => "DRAWING BITMAP (LOCKED)",
			BitmapState.DrawingFinished => "DRAWING FINISHED (LOCKED)",
			BitmapState.Encoding => "ENCODING (LOCKED)",
			BitmapState.EncodingFinished => "ENCODING FINISHED (LOCKED)",
			BitmapState.FinishedBitmap => "BITMAP FINISHED (LOCKED)",
			BitmapState.UnlockedRAM => "UNLOCKED_RAM",
			BitmapState.UnlockedPNG => "UNLOCKED_PNG",
			BitmapState.Unlocked => "UNLOCKED_ENCODED",
			_ => "ERROR! (SHOULDN'T HAPPEN)",
		};

		if (!debugmode)
			return;
		if (token.IsCancellationRequested) {
			debugString = "ABORTING";
			return;
		}
		string _debugString = "TASKS:";
		int i = 0, li = 0;
		while (i < applyMaxTasks) {
			var task = tasks[i];
			_debugString += "\n" + i++ + ": ";
			switch (task.state) {
				case TaskState.Running:
					_debugString += GetTaskState(task.bitmapIndex);
					break;
				case TaskState.Done:
					_debugString += "DONE";
					break;
				case TaskState.Free:
					_debugString += "FREE";
					break;
			}
		}
		BitmapState state, laststate = BitmapState.Error;
		for (var c = i = 0; c < counter.Length; counter[c++] = 0) ;
		for (_debugString += "\n\nIMAGES:"; i < bitmap.Length; ++i)
			++counter[(int)bitmapState[i]];
		string _memoryString = "\n";
		for (i = 0; i < bitmap.Length; ++i, laststate = state)
			if ((state = bitmapState[i]) != laststate) {
				_memoryString += (li == i - 1 ? li + ": " : li + "-" + (i - 1) + ": ") + GetBitmapState(laststate) + "\n";
				li = i;
			}
		_memoryString += (li == i - 1 ? li + ": " : li + "-" + (i - 1) + ": ") + GetBitmapState(laststate);
		for (int c = 0; c < counter.Length; ++c) 
			_debugString += "\n" + counter[c] +"x: "+ GetBitmapState((BitmapState)c);
		_debugString += "\n\nANIMATION:" + _memoryString;
		debugString = i < bitmap.Length ? _debugString + "\n" + i + "+: " + "QUEUED" : _debugString;
	}
	internal void DebugStart() {
		// debug for testing, starts the generator with predetermined setting for easy breakpointing
		// Need to enable the definition on top GeneratorForm.cpp to enable this
		// if enabled you will be unable to use the settings interface!
		SelectFractal(1);
		SelectThreadingDepth();
		selectPeriod = debug = 7;
		selectWidth = 8;//1920;
		selectHeight = 8;//1080;
		maxDepth = -1;//= 2;
		selectMaxTasks = -1;// 10;
		selectSaturate = 1.0;
		selectDetail = .25f;
		SelectThreadingDepth();
		selectCut = selectChildAngle = selectChildColor = selectPaletteType = 0;
		SetupCutFunction();
	}
	#endregion

	#region Interface_Settings
	internal bool SelectFractal(short selectFractal) {
		if (this.selectFractal == selectFractal)
			return true;
		// new fractal definition selected - let the form know to reset and restart me
		this.selectFractal = selectFractal;
		selectCut = selectChildColor = selectChildAngle = 0;
		return false;
	}
	internal void SetupFractal() {
		f = new(fractals[selectFractal]); // take a copy to use (to prevent crashes if the fractal gets edited while in use)
		logBase = (float)Math.Log(f.childSize);
		SetMaxIterations();
	}
	internal void SetMaxIterations() {
		selectMaxIterations = (short)(/*2*/ 8 + Math.Ceiling(Math.Log(Math.Max(selectWidth, selectHeight) * f.maxSize / selectDetail) / logBase));
	}
	/*internal void SetupAngle() {
		childAngle = f.childAngle[selectChildAngle].Item2;
	}*/
	internal void SetupCutFunction() 
		=> cutFunction = f.cutFunction == null || f.cutFunction.Count <= 0 ? null : Fractal.cutFunctions[f.cutFunction[selectCut].Item1].Item2;
	internal bool SelectZoomChild(short z) {
		if (validZoomChildren == null || validZoomChildren.Count < 1 || validZoomChildren[z] == selectZoomChild)
			return true;
		selectZoomChild = validZoomChildren[z];
		return false;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void SelectThreadingDepth() {
		//preIterate = new (double, double, (double, double)[])[Math.Max((short)1,selectMaxTasks)][];
		SetMaxIterations();
		maxDepth = 0;
		if (f.childCount <= 0)
			return;
		for (int n = 1, threadCount = 0; (threadCount += n) < selectMaxTasks - 1; n *= f.childCount)
			++maxDepth;
	}
	#endregion

	#region Interface_Getters
	internal int GetMaxCutparam() {
		if (selectCut >= 0 && GetFractal().cutFunction != null) {
			var c = GetFractal().cutFunction[selectCut].Item2;
			if(c != null && c.Length > 0)
				return c[0] < 0 ? -c[0] : c.Length - 1;
		}
		return cutparamMaximum;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal List<Fractal> GetFractals() => fractals;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Fractal GetFractal() => fractals[selectFractal];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Bitmap GetBitmap(int index) => bitmap == null || bitmap.Length <= index || bitmapState[index] == BitmapState.UnlockedRAM ? null : bitmap[index + previewFrames];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Bitmap GetPreviewBitmap() => bitmapsFinished < 1 ? null : bitmap[bitmapsFinished-1];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetFrames() => bitmap == null ? 0 : bitmap.Length - previewFrames;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetBitmapsFinished() => bitmapsFinished - previewFrames;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int IsGifReady() => gifSuccess;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//GetTempGif();
	internal Fractal.CutFunction GetCutFunction() 
		=> GetFractal().cutFunction == null? null : Fractal.cutFunctions[GetFractal().cutFunction[selectCut].Item1].Item2;
	#endregion
}
