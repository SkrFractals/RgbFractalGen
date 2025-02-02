﻿// Allow debug code, comment this when releasing for slightly better performance
//#define CUSTOMDEBUG

#if CUSTOMDEBUG
using System.Diagnostics;
#endif

using Gif.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RgbFractalGenCs;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class FractalGenerator {

	private enum BitmapState : byte {
		Queued = 0,				// Hasn't even started generating the dots yet (and it might stay this way if OnlyImage)
		Dots = 1,				// Started Generating Dots
		Void = 2,				// Started Dijksring the Void
		Drawing = 3,			// Started drawing
		Encoding = 4,			// Started Encoding
		Finished = 5,			// Finished bitmap (finished drawing if not encodeGIF, or fnished encoding if encodeGIF)
		Error = 6				// Unused, unless error obviously
	}
	private enum TaskState : byte {
		Free = 0,		// The task has not been started yet, or already finished and joined and ready to be started again
		Done = 1,	// The task if finished and ready to join without waiting
		Running = 2,    // The task is running
	}
	public enum ParallelType : byte {
		OfAnimation = 0,// Parallel batching of each animation image into it's own single thread. No pixel conflicts, and more likely to use all the threads consistently. Recommended for animations.
		OfDepth = 1,    // Parallelism of each single image, generates the single next image faster, but there might be rare conflicts of accessing pixels, and it's a bit slower overall if generating an animation. Used automatically for OnlyImage.
		OfRecursion = 2 // Deprecated older image parallelism, use OfDepth instead
	}
	public enum GenerationType : byte {
		OnlyImage = 0,      // Only generates the first single image, and then halts. Or stops generating animation, if selected during an animation generation and at least one frame has already been finished.
		AnimationRAM = 1,   // Will only generate the animation for preview, faster than encoding GIF, but cannot save the GIF when finished.
		EncodeGIF = 2,      // Will encode a GIF while generating the animation, will be slower than generating the animation without GIF.
	}

	private class FractalTask {
		internal Task task;				// Parallel Animation Tasks
		internal TaskState state;		// States of Animation Tasks
		internal Vector3[][] buffer;    // Buffer for points to print into bmp
		internal short[][] voidDepth;	// Depths of Void
		internal Queue<(short, short)> 
			voidQueue;					// Void depth calculating Dijktra queue
		internal Vector3 H;				// Mixed children color
		internal Vector3 I;				// Pure parent color
		internal bool taskStarted;		// Additional safety, could remove if it never gets triggered for a while
		internal short taskIndex;           // The task index
		internal short bitmapIndex;		// The bitmapIndex of which the task is working on
		//internal BitmapState type;		// 0 = gen, 1 = gif

		internal (float, float, (float, float)[])[]
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
			Stop();
			state = TaskState.Free;
			return taskStarted = false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Start(short bitmap, Action action) {
			Stop();
			taskStarted = true;
			bitmapIndex = bitmap;
			//this.type = type;
			state = TaskState.Running;
			task = Task.Run(action);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Stop() {
			if (taskStarted) {
				if (task != null) {
					task.Wait();
					task = null;
				}// else { }
			}// else { }
			
		}
	}

#if CUSTOMDEBUG
	// Debug variables
	private string logString;						// Debug log
	Stopwatch startTime;							// Start Stopwatch
	private double initTimes, iterTimes, voidTimes, drawTimes, gifsTimes;
#endif

	// Definitions
	private readonly Fractal[] fractals;// Fractal definitions
	private Fractal f;                  // Selected fractal definition
	private float[] childAngle;         // A copy of selected childAngle
	private float selectPeriodAngle;    // Angle symmetry of current fractal
	private float applyPeriodAngle;     // Angle symmetry corrected for periodMultiplier
	private readonly byte[] childColor; // A copy of childColor for allowing BGR
	private Vector3 colorBlend;         // Color blend of a selected fractal for more seamless loop (between single color to the sum of children)
	private Fractal.CutFunction 
		cutFunction;					// Selected CutFunction pointer
	private short applyCutparam;		// Applied cutparam (selected or random)
	private short maxIterations = -1;   // maximum depth of iteration (dependent on fractal/detail/resolution)
	private float logBase;				// Size Log Base

	// Resolution
	private short allocatedWidth;	// How much buffer width is currently allocated?
	private short allocatedHeight;	// How much buffer height is currently allocated?
	private short widthBorder;		// Precomputed maximum x coord fot pixel input
	private short heightBorder;		// Precomputed maximum y coord for pixel input
	private float upleftStart;		// Precomputed minimum x/y for dot iteration/application
	private float rightEnd;			// Precomputed maximum -x- for dot iteration/application
	private float downEnd;			// Precomputed maximum -y- for dot iteration/application

	// Frames
	private Bitmap[] bitmap;			// Prerender as an array of bitmaps
	private BitmapState[] bitmapState;	// 0 - not exists, 1 - generating and not exists, 2 - spawned and locked, 3 - finished drawing locked, 4 - started gif locked, 5 - finished unlocked
	private BitmapData[] bitmapData;	// Locked Bits for bitmaps
	private short finalPeriodMultiplier;// How much will the period get finally stretched? (calculated for seamless + user multiplier)
	private short debug;				// Debug frame count override
	private short bitmapsFinished;      // How many bitmaps are completely finished generating? (ready to display, encoded if possible)
	//private short bitmapToEncode;		// How many bitmaps haave at least begun encoding?
	private short nextBitmap;			// How many bitmaps have started generating? (next task should begin with this one)
	private short allocatedFrames;      // How many bitmap frames are currently allocated?
	private short applyZoom;            // Applied zoom (selected or random)
	private GenerationType applyGenerationType;

	// Color
	private short applyBlur;
	private short applyColorPalette;    // RGB or BGR? (0/1)
	private short applyHueCycle;		// Hue Cycling Direction (-1,0,1)
	private byte hueCycleMultiplier;	// How fast should the hue shift to loop seamlessly?

	// Void
	private short ambnoise;                 // Normalizer for maximum void depth - Precomputed amb * noise
	private float bloom1;					// Precomputed bloom+1
	private readonly Random random = new(); // Random generator

	// Threading
	private FractalTask[] tasks;
	private ParallelType 
		applyParallelType;	// Safely copy parallelType in here so it doesn't change in the middle of generation
	private short applyMaxTasks;	// Safely copy maxTasks in here so it doesn't change in the middle of generation
	
	private short maxDepth;			// Maximum depth for Recusrion parallelism
	private short allocatedTasks;	// How many buffer tasks are currently allocated?
	private readonly object 
		taskLock = new();			// Monitor lock
	private CancellationTokenSource 
		cancel;						// Cancellation Token Source
	private Task mainTask;			// Main Generation Task
	//private ConcurrentBag<Task> 
	//	imageTasks;					// Parallel Image Tasks
	//private readonly List<Task> 
	//	taskSnapshot;				// Snapshot for safely checking imageTasks Wait
	(short, (float, float), (float, float), byte, int, byte)[] 
		tuples;						// Queue struct for GenerateDots_OfDepth ((x,y), angle, aa, color, -cutparam, depth);

	
	// Export
	private AnimatedGifEncoder 
		gifEncoder;					// Export GIF encoder
	private bool gifSuccess;        // Temp GIF file "gif.tmp" successfuly created
	private string gifTempPath;		// Temporary GIF file name
	private System.Drawing.Rectangle 
		rect;						// Bitmap rectangle TODO implement

	// Selected Settings
	internal short selectFractal,		// Fractal definition (0-fractals.Length)
		selectChildAngle,				// Child angle definition (0-childAngle.Length)
		selectChildColor,				// Child color definition (0-childColor.Length)
		selectCut,						// Selected CutFunction index (0-cutFunction.Length)
		selectCutparam,					// Cutparam seed (0-maxCutparam)
		selectWidth,					// Resolution width (1-X)
		selectHeight,					// Resolution height (1-X)
		selectPeriod,					// Parent to child frames period (1-X)
		selectPeriodMultiplier,			// Multiplier of frames period (1-X)
		selectZoom,						// Zoom direction (-1 out, 0 random, 1 in)
		selectDefaultZoom,				// Default skipped frames of zoom (0-frames)
		selectExtraSpin,				// Extra spin speed (0-X)
		selectDefaultAngle,				// Default spin angle (0-360)
		selectSpin,						// Spin direction (-2 random, -1 anticlockwise, 0 none, 1 clockwise, 2 antispin)
		selectHue,						// -1 = random, 0 = RGB, 1 = BGR, 2 = RGB->GBR, 3 = BGR->RBG, 4 =RGB->BRG, 5 = BGR->GRB
		selectExtraHue,					// Extra hue angle speed (0-X)
		selectDefaultHue,				// Default hue angle (0-360)
		selectAmbient;					// Void ambient strength (0-120)
	internal float selectNoise,			// Void noise strength (0-3)
		selectSaturate,					// Saturation boost level (0-1)
		selectDetail,					// MinSize multiplier (1-10)
		selectBloom;					// Dot bloom level (pixels)
	internal short selectBlur,			// Dot blur level
		selectBrightness;				// Light normalizer brightness (0-300)
	internal ParallelType 
		selectParallelType;				// 0 = Animation, 1 = Depth, 2 = Recursion
	internal short selectMaxTasks,		// Maximum allowed total tasks
		selectDelay;					// Animation frame delay
	internal GenerationType 
		selectGenerationType;			// 0 = Only Image, 1 = Animation, 2 = Animation + GIF
	internal short cutparamMaximum;     // Maximum seed for the selected CutFunction

	internal bool debugmode = false;
	internal string debugString = "";
	private readonly short[] counter = new short[8];

	#region Init
	internal FractalGenerator() {
		selectParallelType = ParallelType.OfAnimation;
		selectGenerationType = GenerationType.EncodeGIF;
		selectCutparam = debug = selectDefaultHue = selectDefaultZoom = selectDefaultAngle = selectExtraSpin = selectExtraHue = 0;
		selectZoom = 1;
		selectFractal = selectChildColor = selectChildAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks = allocatedFrames = selectSpin = selectHue = -1;
		gifEncoder = null;
		bitmap = null;
		gifSuccess = false;
		//taskSnapshot = [];
		// Constants
		float pi = MathF.PI, pi23 = 2 * pi / 3, pi43 = 4 * pi / 3, SYMMETRIC = 2 * pi,
			stt = MathF.Sqrt(.75f),
			pfs = 2 * (1 + (float)MathF.Cos(.4f * pi)),
			cosc = MathF.Cos(.4f * pi) / pfs,
			sinc = MathF.Sin(.4f * pi) / pfs,
			v = 2 * (sinc * sinc + cosc * (cosc + pfs)) / (2 * sinc),
			s0 = (2 + MathF.Sqrt(2)) / MathF.Sqrt(2),
			sx = 2 + MathF.Sqrt(2),
			r = (1 / s0 + 1 / sx) / (1 / s0 + 1 / (2 * sx)),
			diag = MathF.Sqrt(2) / 3;
		// X, Y
		float[] cx = new float[9], cy = new float[9], pfx = new float[6], pfy = new float[6], tfx = new float[4], tfy = new float[4], ttfx = new float[16], ttfy = new float[16], ofx = new float[9], ofy = new float[9];
		// Carpets
		for (var i = 0; i < 4; ++i) {
			float ipi = i * pi / 2, icos = diag * MathF.Cos(i * pi / 2), isin = diag * MathF.Sin(i * pi / 2);
			cx[i * 2 + 1] = -icos + isin;
			cy[i * 2 + 1] = -isin - icos;
			cx[i * 2 + 2] = isin;
			cy[i * 2 + 2] = -icos;
		}
		cx[0] = cy[0] = 0;
		// Pentaflakes
		for (var i = 1; i <= 5; ++i) {
			pfx[i] = v * MathF.Cos(.4f * (i + 3) * pi);
			pfy[i] = v * MathF.Sin(.4f * (i + 3) * pi);
		}
		// Triflakes
		for (var i = 1; i <= 3; ++i) {
			tfx[i] = .5f * MathF.Cos(i * pi23);
			tfy[i] = .5f * MathF.Sin(i * pi23);
		}
		// Tetraflakes
		for (var i = 1; i <= 3; ++i) {
			float ci = .5f * MathF.Cos(i * pi23), si = .5f * MathF.Sin(i * pi23),
				ci1 = .5f * MathF.Cos((i + 1) * pi23), si1 = .5f * MathF.Sin((i + 1) * pi23),
				ci2 = .5f * MathF.Cos((i + 2) * pi23), si2 = .5f * MathF.Sin((i + 2) * pi23);
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
		for (var i = 1; i <= 8; ++i) {
			ofx[i] = r * MathF.Cos(i * pi / 4);
			ofy[i] = r * MathF.Sin(i * pi / 4);
		}

		// Rotations
		// childAngle[0] = SYMMETRIC + symmetry when 2*pi is symmetric!
		// childAngle[0] = symmetry when 2*pi is NOT symmetric!

		// Fractal definitions list
		fractals = [
			new("Void", 0, 1000, 1, .1f, 1, [0], [0], [("N",[pi])], [("N",[0])], null),

			new("TriTree", 10, 4, .2f, .1f, .9f,
			[0, -stt, 2 * stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt],
			[0, -1.5f, 0, 1.5f, 1.5f, 3, 1.5f, -1.5f, -3, -1.5f],
			[
				("RingTree",[SYMMETRIC + pi23, pi, pi + pi23, pi + 2 * pi23, 0, 0, pi23, pi23, 2 * pi23, 2 * pi23]),
				("BeamTree_Beams", [pi / 3, 0, pi23, pi43, pi, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43]),
				("BeamTree_OuterJoint", [pi / 3, 0, pi23, pi43, pi + pi23, pi + pi23, pi, pi, pi + pi43, pi + pi43]),
				("BeamTree_InnerJoint", [pi / 3, 0, pi23, pi43, pi, pi, pi, pi, pi, pi])
			], [
				("Center", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("CenterNeighbors", [0, 1, 1, 1, 0, 0, 0, 0, 0, 0])
			], [
				("NoChild", Fractal.Trees_NoChildParam),
				("NoBeam", Fractal.Beamtree_NoBeam),
				("OuterJoint", Fractal.Beamtree_OuterJoint),
				("InnerJoint", Fractal.Beamtree_InnerJoint)
				]
			),
			new("TriComb", 13, 5, .2f, .1f, .9f,
			[0, 2 * stt, -stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt, 2 * stt, -4 * stt, 2 * stt],
			[0, 0, 1.5f, -1.5f, 1.5f, 3, 1.5f, -1.5f, -3, -1.5f, 3, 0, -3],
			[("Classic", [pi / 3, 0, pi23, pi43, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43, pi, pi43, 0, pi23])],
			[
				("Center", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Bridges", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2])
			],
			[("NoChild", Fractal.TriComb_Param)]
			),
			new("Triflake", 4, 2, 1.5f, .25f, .8f, tfx, tfy,
			[("Center", [pi / 3, 0, 0, 0])],
			[("Center", [1, 0, 0, 0])], [
				("NoChild", Fractal.Triflake_NoCornerParam),
				("NoBackDiag", Fractal.Triflake_NoBackDiag) // also try nocenter flag
			]
			),
			new("TetraTriflake", 16, 4, 1.5f, .15f, .8f, ttfx, ttfy,
			[("Classic", [SYMMETRIC + pi23, pi, pi + pi23, pi + pi43, 0, pi23, pi43, 0, pi23, pi43, 0, pi23, pi43, pi, pi + pi23, pi + pi43])],
			[
				("Rad", [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1]),
				("Corner", [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0]),
				("Triangle",[0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Swirl",[0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center Rad", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1]),
				("Center Corner", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0]),
				("Center Triangle",[1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center Swirl",[1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center Rad 2", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2]),
				("Center Corner 2", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0]),
				("Center Triangle 2",[1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center Swirl 2",[1, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0])
			], [
				("NoCorner", Fractal.Tetraflake_NoCornerParam),
				("NoCorner + RadHoles", Fractal.Tetraflake_NoCornerRadHolesParam),
				("NoCorner + CornerHoles", Fractal.Tetraflake_NoCornerCornerHolesParam),
				("NoCorner + TriangleHoles", Fractal.Tetraflake_NoCornerTriangleHolesParam),
				]
			),
			new("SierpinskiCarpet", 9, 3, 1.0f, .25f, .9f, cx, cy,
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
				("Sierpinski Carpet",[1, 0, 0, 0, 0, 0, 0, 0, 0]),
				("H-I De Rivera", [0, 0, 1, 0, 0, 0, 1, 0, 0])
			], [
				("NoChild", Fractal.Carpet_NoCornerParam),
				("NoBackDiag", Fractal.Carpet_NoBackDiag),
				("NoBackDiag2", Fractal.Carpet_NoBackDiag2)
			]),
			new("Pentaflake", 6, pfs, .2f * pfs, .25f, .9f, pfx, pfy,
			[
				("Classic", [2 * pi / 10, 0, 0, 0, 0, 0]),
				("No Center Rotation", [2 * pi + 2 * pi / 5, 0, 0, 0, 0, 0])
			], [("Center", [1, 0, 0, 0, 0, 0])],
			[
				("NoChild",Fractal.Pentaflake_NoCornerParam),
				("NoBackDiag", Fractal.Pentaflake_NoBackDiag)
			]
			),
			new("Hexaflake", 7, 3, .5f, .2f, 1,
			[0, 0, 2 * stt, 2 * stt, 0, -2 * stt, -2 * stt],
			[0, -2, -1, 1, 2, 1, -1],
			[("Classic", [SYMMETRIC + pi / 3, 0, 0, 0, 0, 0, 0])],
			[
				("Center", [1, 0, 0, 0, 0, 0, 0]),
				("Y", [1, 0, 1, 0, 1, 0, 1])
			], [
				("NoChild", Fractal.Hexaflake_NoCornerParam),
				("NoBackDiag", Fractal.Hexaflake_NoBackDiag)
			]
			),
			new("HexaCircular", 19, 5, .3f, .05f, .9f,
			[0, 2 * stt, stt, -stt, -2 * stt, -stt, stt, 4 * stt, 3 * stt, stt, -stt, -3 * stt, -4 * stt, -4 * stt, -3 * stt, -stt, stt, 3 * stt, 4 * stt],
			[0, 0, 1.5f, 1.5f, 0, -1.5f, -1.5f, 1, 2.5f, 3.5f, 3.5f, 2.5f, 1, -1, -2.5f, -3.5f, -3.5f, -2.5f, -1],
			[
				("180", [pi / 3, 0, 0, 0, 0, 0, 0, 0, pi, pi, 0, 0, pi, pi, 0, 0, pi, pi, 0]),
				("Symmetric", [SYMMETRIC + pi23, 0, 0, 0, 0, 0, 0, 0, pi, pi, 0, 0, pi, pi, 0, 0, pi, pi, 0])
			], [
				("Center", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Center Y", [1, 0, 2, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Y", [0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
				("Double Y", [0, 2, 1, 2, 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0])
			], null
			)
			];
		var maxChildren = 1;
		foreach (var i in fractals) {
			if (i.childCount > maxChildren)
				maxChildren = i.childCount;
		}
		childColor = new byte[maxChildren];
	}
	#endregion

	#region Generate_Tasks
	private void GenerateAnimation() {
		#region InitData
		void NewBuffer(FractalTask task) {
			var voidT = task.voidDepth = new short[selectHeight][];
			var buffT = task.buffer = new Vector3[selectHeight][];
			for (var y = 0; y < selectHeight; voidT[y++] = new short[selectWidth]) {
				var buffY = buffT[y] = new Vector3[selectWidth];
				for (var x = 0; x < selectWidth; buffY[x++] = Vector3.Zero) ;
			}
		}
		#endregion
		#region GenerateTasks
		void GenerateDots(short bitmapIndex, short stateIndex, float size, float angle, short spin, float hueAngle, byte color) {
			#region GenerateDots_Inline
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			bool ApplyDot(bool apply, FractalTask task, float inX, float inY, float inDetail, float inColor) {
				if (apply) {
					Vector3 GetDotColor() => Vector3.Lerp(task.H, task.I, inDetail);
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					Vector3 Y(Vector3 X) => new(X.Z, X.X, X.Y);
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					Vector3 Z(Vector3 X) => new(X.Y, X.Z, X.X);
					var dotColor = inColor switch { 1 => Y(GetDotColor()), 2 => Z(GetDotColor()), _ => GetDotColor() };
					var buffT = task.buffer;
					// Iterated deep into a single point - Interpolate inbetween 4 pixels and Stop
					int startX = Math.Max(1, (int)MathF.Floor(inX - selectBloom)), endX = Math.Min(widthBorder, (int)MathF.Ceiling(inX + selectBloom)), endY = Math.Min(heightBorder, (int)MathF.Ceiling(inY + selectBloom));
					for (int x, y = Math.Max(1, (int)MathF.Floor(inY - selectBloom)); y <= endY; ++y) {
						var yd = bloom1 - MathF.Abs(y - inY);
						var buffY = buffT[y];
						for (x = startX; x <= endX; ++x)
							buffY[x] += (yd * (bloom1 - MathF.Abs(x - inX))) * dotColor;
					}
					return true;
				}
				return false;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			int CalculateFlags(int index, int inFlags) => cutFunction == null ? inFlags : cutFunction(index, inFlags);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			(float, float) NewXY((float, float) inXY, (float, float) XY, float inAngle) {
				float cs = (float)Math.Cos(inAngle), sn = (float)Math.Sin(inAngle);
				return (inXY.Item1 + XY.Item1 * cs - XY.Item2 * sn, inXY.Item2 - XY.Item2 * cs - XY.Item1 * sn);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			bool TestSize(float newX, float newY, float inSize) {
				var testSize = inSize * f.cutSize;
				return Math.Min(newX, newY) + testSize > upleftStart && newX - testSize < rightEnd && newY - testSize < downEnd;
			}
			#endregion
			#region GenerateDots
			void GenerateDots_SingleTask(short taskIndex,
				(float, float) inXY, (float, float) inAngle, byte inColor, int inFlags, byte inDepth
			) {
				var task = tasks[taskIndex];
				var preIterated = task.preIterate[inDepth];
				if (ApplyDot(preIterated.Item1 < selectDetail, task, inXY.Item1, inXY.Item2, preIterated.Item2, inColor))
					return;
				// Split Iteration Deeper
				var newPreIterated = task.preIterate[++inDepth];
				//var i = f.childCount;
				//while (0 <= --i) {
				for (int i = 0; i < f.childCount; ++i) {
					if (cancel.Token.IsCancellationRequested)
						return;
					// Special Cutoff
					var newFlags = CalculateFlags(i, inFlags);
					if (newFlags < 0)
						continue;
					// Outside View
					var XY = preIterated.Item3[i];
					var newXY = NewXY(inXY, XY, inAngle.Item1);
					if (TestSize(newXY.Item1, newXY.Item2, preIterated.Item1))
						GenerateDots_SingleTask(taskIndex, newXY,
							i == 0
							? (inAngle.Item1 + childAngle[i] - inAngle.Item2, -inAngle.Item2)
							: (inAngle.Item1 + childAngle[i], inAngle.Item2),
							(byte)((inColor + childColor[i]) % 3), newFlags, inDepth);
				}
			}
			void GenerateDots_OfDepth(short bitmapIndex) {
				int index = 0, insertTo = 1,
					max = applyMaxTasks * 8,
					maxcount = max - f.childCount - 1,
					count = (max + insertTo - index) % max;
				while (count > 0 && count < maxcount) {
					(var taskIndex, var inXY, var inAngle, var inColor, var inFlags, var inDepth) = tuples[index++];
					var task = tasks[taskIndex];
					var preIterated = task.preIterate[inDepth];
					if (ApplyDot(preIterated.Item1 < selectDetail, task, inXY.Item1, inXY.Item2, preIterated.Item2, inColor))
						continue;
					// Split Iteration Deeper
					var newPreIterated = task.preIterate[++inDepth];
					var i = f.childCount;
					while (0 <= --i) {
						if (cancel.Token.IsCancellationRequested)
							return;
						// Special Cutoff
						var newFlags = CalculateFlags(i, inFlags);
						if (newFlags < 0)
							continue;
						// Outside View
						var XY = preIterated.Item3[i];
						var newXY = NewXY(inXY, XY, inAngle.Item1);
						if (TestSize(newXY.Item1, newXY.Item2, preIterated.Item1))
							tuples[insertTo++] = (taskIndex, newXY, i == 0 ? (inAngle.Item1 + childAngle[i] - inAngle.Item2, -inAngle.Item2) : (inAngle.Item1 + childAngle[i], inAngle.Item2), (byte)((inColor + childColor[i]) % 3), newFlags, inDepth);
						insertTo %= max;
					}
					count = (max + insertTo - index) % max;
				}
				FinishTasks(false, (short taskIndex) => {
					if (count <= 0)
						return false;
					var tupleIndex = index++;
					tasks[taskIndex].Start(bitmapIndex, () => {
						(var bufferIndex, var inXY, var inAngle, var inColor, var inFlags, var inDepth) = tuples[tupleIndex];
						GenerateDots_SingleTask(bufferIndex, inXY, inAngle, inColor, inFlags, inDepth);
						tasks[taskIndex].state = TaskState.Done;
					});
					index %= max;
					count = (max + insertTo - index) % max;
					return true;
				});
			}
			// OfRecursion is deprecated, use OfDepth instead, it's better anyway
			/*void GenerateDots_OfRecursion(short taskIndex,
				(float, float) inXY, (float, float) inAngle,
				byte inColor, int inFlags, byte inDepth
			) {
				var task = tasks[taskIndex];
				var preIterated = task.preIterate[inDepth];
				if (ApplyDot(preIterated.Item1 < selectDetail, task, inXY.Item1, inXY.Item2, preIterated.Item2, inColor))
					return;
				// Split Iteration Deeper
				var newPreIterated = task.preIterate[++inDepth];
				var i = f.childCount;
				while (0 <= --i) {
					if (cancel.Token.IsCancellationRequested)
						return;
					// Special Cutoff
					var newFlags = CalculateFlags(i, inFlags);
					if (newFlags < 0)
						continue;
					// Outside View
					var XY = preIterated.Item3[i];
					var newXY = NewXY(inXY, XY, inAngle.Item1);
					if (TestSize(newXY.Item1, newXY.Item2, preIterated.Item1)) {
						if (imageTasks != null && imageTasks.Count < applyMaxGenerationTasks && inDepth < maxDepth) {
							int index = i;
							imageTasks.Add(Task.Run(() => GenerateDots_OfRecursion(taskIndex, newXY, index == 0 ? (inAngle.Item1 + childAngle[index] - inAngle.Item2, -inAngle.Item2) : (inAngle.Item1 + childAngle[index], inAngle.Item2), (byte)((inColor + childColor[index]) % 3), newFlags, inDepth)));
						} else GenerateDots_OfRecursion(taskIndex, newXY, i == 0 ? (inAngle.Item1 + childAngle[i] - inAngle.Item2, -inAngle.Item2) : (inAngle.Item1 + childAngle[i], inAngle.Item2), (byte)((inColor + childColor[i]) % 3), newFlags, inDepth);
					}
				}
			}*/
			#endregion
			
#if CUSTOMDEBUG
			var threadString = "";
			Stopwatch initTime = new();
			initTime.Start();
#endif
			bitmapState[bitmapIndex] = BitmapState.Dots;
			// Init buffer with zeroes
			var taskIndex = Math.Abs(stateIndex);
			var state = stateIndex < 0 ? null : tasks[stateIndex];
			var task = tasks[taskIndex];
			var buffT = task.buffer;
			for (var y = 0; y < selectHeight; ++y) {
				var buffY = buffT[y];
				for (var x = 0; x < selectWidth; buffY[x++] = Vector3.Zero) ;
			}

#if CUSTOMDEBUG
			initTime.Stop();
			Log(ref threadString, "Init:" + bitmapIndex + " time = " + initTime.Elapsed.TotalMilliseconds + " ms.");
			Stopwatch iterTime = new();
			iterTime.Start();
#endif
			// Generate the fractal frame recursively
			if (cancel.Token.IsCancellationRequested) {
				if (state != null)
					state.state = TaskState.Done;
				return;
			}
			for (var b = 0; b < applyBlur; ++b) {
				ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
				// Preiterate values that change the same way as iteration goes deeper, so they only get calculated once
				var preIterateTask = task.preIterate;
				float inSize = size;
				for (int i = 0; i < maxIterations; ++i) {
					preIterateTask[i].Item2 = (float)Math.Log(selectDetail / (preIterateTask[i].Item1 = inSize)) / logBase;
					//var inDetail = inSize;
					var inDetail = -inSize * Math.Max(-1, preIterateTask[i].Item2 = (float)Math.Log(selectDetail / (preIterateTask[i].Item1 = inSize)) / logBase);
					if (preIterateTask[i].Item3 == null || preIterateTask[i].Item3.Length < f.childCount)
						preIterateTask[i].Item3 = new (float, float)[f.childCount];
					for (int c = 0; c < f.childCount; ++c)
						preIterateTask[i].Item3[c] = (f.childX[c] * inDetail, f.childY[c] * inDetail);
					inSize /= f.childSize;
				}
				// Prepare Color blending per one dot (hueshifting + iteration correction)
				// So that the color of the dot will slowly approach the combined colors of its childer before it splits
				var lerp = hueAngle % 1.0f;
				switch ((byte)hueAngle % 3) {
					case 0:
						task.I = Vector3.Lerp(Vector3.UnitX, Vector3.UnitY, lerp);
						task.H = Vector3.Lerp(colorBlend, new Vector3(colorBlend.Z, colorBlend.X, colorBlend.Y), lerp);
						break;
					case 1:
						task.I = Vector3.Lerp(Vector3.UnitY, Vector3.UnitZ, lerp);
						task.H = Vector3.Lerp(new Vector3(colorBlend.Z, colorBlend.X, colorBlend.Y), new Vector3(colorBlend.Y, colorBlend.Z, colorBlend.X), lerp);
						break;
					case 2:
						task.I = Vector3.Lerp(Vector3.UnitZ, Vector3.UnitX, lerp);
						task.H = Vector3.Lerp(new Vector3(colorBlend.Y, colorBlend.Z, colorBlend.X), colorBlend, lerp);
						break;
				}
				if (applyMaxTasks <= 2 || applyParallelType != ParallelType.OfDepth)
					GenerateDots_SingleTask(taskIndex, (selectWidth * .5f, selectHeight * .5f), (angle, Math.Abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, 0);
				else {
					tuples[0] = (0, (selectWidth * .5f, selectHeight * .5f), (angle, spin > 1 ? 2 * angle : 0), color, -applyCutparam, 0);
					GenerateDots_OfDepth(bitmapIndex);
				}
				// OfRecursion is deprecated, use OfDepth instead, it's better anyway
				/*if (applyMaxTasks <= 2)
					GenerateDots_SingleTask(taskIndex, (selectWidth * .5f, selectHeight * .5f), (angle, Math.Abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, 0);
				else switch (applyParallelType) {
						default:
							GenerateDots_SingleTask(taskIndex, (selectWidth * .5f, selectHeight * .5f), (angle, Math.Abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, 0);
							break;
						case 1:
							tuples[0] = (0, (selectWidth * .5f, selectHeight * .5f), (angle, Math.Abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, 0);
							GenerateDots_OfDepth();
							break;
						case 2:
							GenerateDots_OfRecursion(taskIndex, (selectWidth * .5f, selectHeight * .5f), (angle, Math.Abs(spin) > 1 ? 2 * angle : 0), color, -applyCutparam, 0);
							if (imageTasks == null)
								return;
							// Wait for image parallelism threads to complete
							bool noMore;
							taskSnapshot.Clear();
							while (true) {
								foreach (var m in imageTasks)
									taskSnapshot.Add(m);
								foreach (var m in taskSnapshot)
									m.Wait();
								noMore = true;
								foreach (var m in imageTasks)
									if (!taskSnapshot.Contains(m))
										noMore = false;
								if (noMore)
									break;
								//Thread.Sleep(10);
							}
							break;*/
				IncFrameParameters(ref size, ref angle, spin, ref hueAngle, applyBlur);
				if (cancel.IsCancellationRequested) {
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
			float lightNormalizer = 0.1f, voidDepthMax = 1.0f;
			short voidYX, w1 = (short)(selectWidth - 1), h1 = (short)(selectHeight - 1);
			if (selectAmbient > 0) {
				// Void Depth Seed points (no points, no borders), leet the void depth generator know where to start incrementing the depth
				float lightMax;
				for (short y = 1; y < h1; ++y) {
					if (cancel.IsCancellationRequested) {
						queueT.Clear();
						task.state = TaskState.Done;
						return;
					}
					var voidY = voidT[y];
					var buffY = buffT[y];
					for (short x = 1; x < w1; ++x) {
						var buffYX = buffY[x];
						lightNormalizer = MathF.Max(lightNormalizer, lightMax = MathF.Max(buffYX.X, MathF.Max(buffYX.Y, buffYX.Z)));
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
				for (short x = 0; x < selectWidth; ++x) {
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
					if (xp < selectWidth && voidT[y][xp] == -1) { voidT[y][xp] = voidYX; queueT.Enqueue((y, xp)); }
					if (yp < selectHeight && voidT[yp][x] == -1) { voidT[yp][x] = voidYX; queueT.Enqueue((yp, x)); }
					if (xm >= 0 && voidT[y][xm] == -1) { voidT[y][xm] = voidYX; queueT.Enqueue((y, xm)); }
					if (ym >= 0 && voidT[ym][x] == -1) { voidT[ym][x] = voidYX; queueT.Enqueue((ym, x)); }
				}
				voidDepthMax = voidMax;
			} else
				for (short y = 0; y < selectHeight; ++y) {
					if (cancel.IsCancellationRequested) {
						task.state = TaskState.Done;
						return;
					}
					var buffY = buffT[y];
					for (short x = 0; x < selectWidth; ++x) {
						var buffYX = buffY[x];
						lightNormalizer = MathF.Max(lightNormalizer, MathF.Max(buffYX.X, MathF.Max(buffYX.Y, buffYX.Z)));
					}
				}
			lightNormalizer = selectBrightness * 2.55f / lightNormalizer;
			if (cancel.IsCancellationRequested) {
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
			//GenerateBitmap(bitmapIndex, buffT, voidT, lightNormalizer, voidDepthMax);
			unsafe {
				#region GenerateBitmap_Inline
				Vector3 ApplyAmbientNoise(Vector3 rgb, float Amb, float Noise, Random rand) {
					rgb.X += Noise * rand.Next(ambnoise) + Amb;
					rgb.Y += Noise * rand.Next(ambnoise) + Amb;
					rgb.Z += Noise * rand.Next(ambnoise) + Amb;
					return rgb;
					//rgb += ((1.0f - voidAmb) * voidAmb) * new Vector3(rand.Next(ambnoise), rand.Next(ambnoise), rand.Next(ambnoise)) + new Vector3(voidAmb * amb);
				}
				unsafe Vector3 ApplySaturate(Vector3 rgb) {
					// The saturation equation boosting up the saturation of the pixel (powered by the saturation slider setting)
					float m, min = MathF.Min(MathF.Min(rgb.X, rgb.Y), rgb.Z), max = MathF.Max(MathF.Max(rgb.X, rgb.Y), rgb.Z);
					return max <= min ? rgb : ((m = max * selectSaturate / (max - min)) + 1 - selectSaturate) * rgb - new Vector3(min * m);
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
				Vector3 Normalize(Vector3 pixel, float lightNormalizer) {
					//float max = MathF.Max(pixel.X, MathF.Max(pixel.Y, pixel.Z)) / 254.0f;
					//return lightNormalizer * max > 1.0f ? (1.0f / max) * pixel : lightNormalizer * pixel;
					float max = MathF.Max(pixel.X, MathF.Max(pixel.Y, pixel.Z));
					return lightNormalizer * max > 254.0f ? (254.0f / max) * pixel : lightNormalizer * pixel;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoiseSaturate(Vector3[] buffY, short[] voidY, ref byte* p, Random rand, float lightNormalizer, float voidDepthMax) {
					if (selectAmbient <= 0)
						for (var x = 0; x < selectWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), ref p)) ;
					else for (var x = 0; x < selectWidth; ++x) {
							var voidAmb = voidY[x] / voidDepthMax;
							ApplyRGBToBytePointer(ApplyAmbientNoise(ApplySaturate(Normalize(buffY[x], lightNormalizer)), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb, rand), ref p);
						}
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoiseNoSaturate(Vector3[] buffY, short[] voidY, ref byte* p, Random rand, float lightNormalizer, float voidDepthMax) {
					if (selectAmbient <= 0)
						for (var x = 0; x < selectWidth; ApplyRGBToBytePointer(Normalize(buffY[x++], lightNormalizer), ref p)) ;
					else for (var x = 0; x < selectWidth; ++x) {
							var voidAmb = voidY[x] / voidDepthMax;
							ApplyRGBToBytePointer(ApplyAmbientNoise(Normalize(buffY[x], lightNormalizer), voidAmb * selectAmbient, (1.0f - voidAmb) * voidAmb, rand), ref p);
						}
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoNoiseSaturate(Vector3[] buffY, short[] voidY, ref byte* p, float lightNormalizer, float voidDepthMax) {
					if (selectAmbient <= 0)
						for (var x = 0; x < selectWidth; ApplyRGBToBytePointer(ApplySaturate(Normalize(buffY[x++], lightNormalizer)), ref p)) ;
					else for (var x = 0; x < selectWidth; ++x)
							ApplyRGBToBytePointer(new Vector3(selectAmbient * voidY[x] / voidDepthMax) + ApplySaturate(Normalize(buffY[x], lightNormalizer)), ref p);
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void NoNoiseNoSaturate(Vector3[] buffY, short[] voidY, ref byte* p, float lightNormalizer, float voidDepthMax) {
					if (selectAmbient <= 0) for (var x = 0; x < selectWidth; x++)
							ApplyRGBToBytePointer(Normalize(buffY[x], lightNormalizer), ref p);
					else for (var x = 0; x < selectWidth; ++x)
							ApplyRGBToBytePointer(new Vector3(selectAmbient * voidY[x] / voidDepthMax) + Normalize(buffY[x], lightNormalizer), ref p);
				}
				#endregion
				// Make a locked bitmap, remember the locked state
				var p = (byte*)(void*)(bitmapData[task.bitmapIndex] = (bitmap[task.bitmapIndex] = new(selectWidth, selectHeight)).LockBits(rect,
					ImageLockMode.ReadWrite,
					System.Drawing.Imaging.PixelFormat.Format24bppRgb)).Scan0;
				bitmapState[task.bitmapIndex] = BitmapState.Drawing;
				// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
				// Switch between th selected settings such as saturation, noise, image parallelism...
				var maxGenerationTasks = (short)Math.Max(1, applyMaxTasks-1); 
				if (applyParallelType > 0 && maxGenerationTasks > 1) {
					// Multi Threaded
					var stride = 3 * selectWidth;
					var po = new ParallelOptions {
						MaxDegreeOfParallelism = maxGenerationTasks,
						CancellationToken = cancel.Token
					};
					try {
						if (ambnoise > 0) {
							var threadRand = new ThreadLocal<Random>(() => new(Guid.NewGuid().GetHashCode()));
							if (selectSaturate > 0.0) Parallel.For(0, selectHeight, po, y => {
								var rp = p + y * stride;
								NoiseSaturate(buffT[y], voidT[y], ref rp, threadRand.Value, lightNormalizer, voidDepthMax);
							});
							else Parallel.For(0, selectHeight, po, y => {
								var rp = p + y * stride;
								NoiseNoSaturate(buffT[y], voidT[y], ref rp, threadRand.Value, lightNormalizer, voidDepthMax);
							});
						} else {
							if (selectSaturate > 0.0) Parallel.For(0, selectHeight, po, y => {
								var rp = p + y * stride;
								NoNoiseSaturate(buffT[y], voidT[y], ref rp, lightNormalizer, voidDepthMax);
							});
							else Parallel.For(0, selectHeight, po, y => {
								var rp = p + y * stride;
								NoNoiseNoSaturate(buffT[y], voidT[y], ref rp, lightNormalizer, voidDepthMax);
							});
						}
					} catch (Exception) { }
				} else {
					// Single Threaded
					if (ambnoise > 0) {
						if (selectSaturate > 0.0) for (short y = 0; y < selectHeight; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoiseSaturate(buffT[y], voidT[y], ref p, random, lightNormalizer, voidDepthMax);
							}
						else for (short y = 0; y < selectHeight; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoiseNoSaturate(buffT[y], voidT[y], ref p, random, lightNormalizer, voidDepthMax);
							}
					} else {
						if (selectSaturate > 0.0) for (short y = 0; y < selectHeight; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoNoiseSaturate(buffT[y], voidT[y], ref p, lightNormalizer, voidDepthMax);
							}
						else for (short y = 0; y < selectHeight; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoNoiseNoSaturate(buffT[y], voidT[y], ref p, lightNormalizer, voidDepthMax);
							}
					}
				}
			}
			if (cancel.IsCancellationRequested) {
				task.state = TaskState.Done;
				return;
			}
#if CUSTOMDEBUG
			drawTime.Stop();
			Log(ref threadString, "Draw:" + task.bitmapIndex + " time = " + drawTime.Elapsed.TotalMilliseconds + " ms.");
			Stopwatch gifsTime = new(); gifsTime.Start();
#endif
			bitmapState[task.bitmapIndex] = BitmapState.Encoding; // Start encoding Frame to a temp GIF			
			if (applyGenerationType >= GenerationType.EncodeGIF && gifEncoder != null) 
				unsafe { 
					gifEncoder.AddFrameParallel((byte*)(void*)bitmapData[task.bitmapIndex].Scan0, cancel.Token, task.bitmapIndex); 
				}
			else bitmapState[task.bitmapIndex] = BitmapState.Finished;
#if CUSTOMDEBUG
			gifsTime.Stop(); 
			Log(ref threadString, "Gifs:" + task.bitmapIndex + " time = " + gifsTime.Elapsed.TotalMilliseconds + " ms.");
			Monitor.Enter(taskLock);
			try {
				voidTimes += voidTime.Elapsed.TotalMilliseconds;
				drawTimes += drawTime.Elapsed.TotalMilliseconds;
				gifsTimes += gifsTime.Elapsed.TotalMilliseconds;
				Log(ref logString, threadString);
			} finally { Monitor.Exit(taskLock); }
#endif
#if CUSTOMDEBUG
			Stopwatch gifsTime = new(); gifsTime.Start();
#endif
			task.state = TaskState.Done;
		}
		/** Tries to encode a gif in a separate parallel thread
		 * 
		 * @param task - a free task to which we can start and assign the encoding thread
		 * @return - failed to create the thread for any reason - so that this task can still be user for something else
		 */
		/*bool TryGif(FractalTask task, short tryEncode = -1) {
			// mark as sequentilly finished and unlock any bitmaps that have a finished state and all of the previous bitmaps before them were finished as well
			while (bitmapsFinished < bitmap.Length && bitmapState[bitmapsFinished] == BitmapState.Finished) {
				bitmap[bitmapsFinished].UnlockBits(bitmapData[bitmapsFinished]);
				++bitmapsFinished;
			}
			if (tryEncode < 0) {
				// OfAnimation type encodes the gif within it's generate dots task
				if (applyParallelType == ParallelType.OfAnimation)
					return true;
				// Find a next bitmap to encode:
				while (bitmapToEncode < bitmap.Length && bitmapState[bitmapToEncode] >= BitmapState.Encoding)
					++bitmapToEncode;
				tryEncode = bitmapToEncode;
				while (tryEncode <= bitmapToEncode + applyMaxTasks && tryEncode < bitmap.Length && bitmapState[tryEncode] != BitmapState.DrawingFinished)
					++tryEncode;
			}
			if (applyGenerationType >= GenerationType.EncodeGIF && gifEncoder != null) {
				bool tryWrite = true;
				while (tryWrite) {
					int unlock = gifEncoder.FinishedFrame();
					// Try to finalize the previous encoder tasks
					switch (gifEncoder.TryWrite()) {
						default:
							tryWrite = false;
							break;
						case TryWrite.Failed:
							// Failed, or FinishedAnimation which should never happen
							if (bitmapState[tryEncode] >= BitmapState.DrawingFinished)
							bitmapState[tryEncode] = BitmapState.Finished;
							tryWrite = false;
							return true;
						case TryWrite.Waiting:
							tryWrite = false;
							break;
						case TryWrite.FinishedFrame:
							bitmapState[unlock] = BitmapState.Finished;
							break;
					}
				}
				if (bitmapState[tryEncode] != BitmapState.DrawingFinished)
					return true;
				//exportingGif = true;
				bitmapState[tryEncode] = BitmapState.Encoding; // Started encoding state
				//task.Start();
				GenerateGif(task, tryEncode);
				return false;
			} else if (bitmapState[tryEncode] == BitmapState.DrawingFinished)
				bitmapState[tryEncode] = BitmapState.Finished;
			return true;
		}*/
		/** Parallel threading management - will keep creating new threads unless cancelled and return when all threads are finished
		 * 
		 * @param includingGif - gif threads can still keep running when we return
		 * @param operation - gets called when a task is free, should return true if it created a new task
		 */
		void FinishTasks(bool includingAnimation, Func<short, bool> lambda) {
			FractalTask task;
			for (var tasksRemaining = true; tasksRemaining; MakeDebugString()) {
				TryFinishBitmaps();
				while (bitmapsFinished < bitmap.Length && bitmapState[bitmapsFinished] >= (applyGenerationType >= GenerationType.EncodeGIF ? BitmapState.Finished : BitmapState.Encoding)) {
					bitmap[bitmapsFinished].UnlockBits(bitmapData[bitmapsFinished]);
					++bitmapsFinished;
				}
				tasksRemaining = false;
				for (short t = applyMaxTasks; 0 <= --t;)
					tasksRemaining |= (task = tasks[t]).IsStillRunning()
						? includingAnimation || bitmapState[task.bitmapIndex] <= BitmapState.Dots
						: !cancel.Token.IsCancellationRequested && selectMaxTasks == applyMaxTasks && lambda(t);
				//if ((task = tasks[t]).IsStillRunning()) tasksRemaining |= includingAnimation || bitmapState[task.bitmapIndex] <= BitmapState.Dots; // Task not finished yet
				//else if (!cancel.Token.IsCancellationRequested && selectMaxTasks == applyMaxTasks) { if (TryGif(task)) tasksRemaining |= lambda(t); else if (includingAnimation) tasksRemaining = true; }
			}
		}
		void TryFinishBitmaps() {
			while (applyGenerationType >= GenerationType.EncodeGIF) {
				int unlock = gifEncoder.FinishedFrame();
				// Try to finalize the previous encoder tasks
				switch (gifEncoder.TryWrite()) {
					case TryWrite.Failed:
						// fallback to only display animation without encoding
						applyGenerationType = GenerationType.AnimationRAM;
						return;
					case TryWrite.FinishedFrame:
						// mark the bitmap state as fully finished
						bitmapState[unlock] = BitmapState.Finished;
						break;
					default:
						// waiting or finished animation
						return;
				}
			}
		}

		#endregion
		#region AnimationParams
		void IncFrameParameters(ref float size, ref float angle, float spin, ref float hueAngle, short blur) {
			var blurPeriod = selectPeriod * blur;
			// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
			angle += spin * (applyPeriodAngle * (1 + selectExtraSpin)) / (finalPeriodMultiplier * blurPeriod);
			hueAngle += (hueCycleMultiplier + 3 * selectExtraHue) * (float)applyHueCycle / (finalPeriodMultiplier * blurPeriod);
			IncFrameSize(ref size, blurPeriod);
		}
		void ModFrameParameters(ref float size, ref float angle, ref short spin, ref float hueAngle, ref byte color) {
			void SwitchParentChild(ref float angle, ref short spin, ref byte color) {
				if (Math.Abs(spin) > 1) {
					// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
					angle = -angle;
					spin = (short)-spin;
				}
				color = (byte)((3 + color + applyZoom * childColor[0]) % 3);
			}
			var fp = f.childSize;
			var w = Math.Max(selectWidth, selectHeight) * f.maxSize;
			// Zoom Rotation
			while (angle > Math.PI * 2)
				angle -= (float)Math.PI * 2;
			while (angle < 0)
				angle += (float)Math.PI * 2;
			// Zom Hue Cycle
			while (hueAngle >= 3)
				hueAngle -= 3;
			while (hueAngle < 0)
				hueAngle += 3;
			// Swap Parent<->CenterChild after a full period
			while (size >= w * fp) {
				size /= fp;
				angle += childAngle[0];
				SwitchParentChild(ref angle, ref spin, ref color);
			}
			while (size < w) {
				size *= fp;
				angle -= childAngle[0];
				SwitchParentChild(ref angle, ref spin, ref color);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IncFrameSize(ref float size, int period) => size *= MathF.Pow(f.childSize, applyZoom * 1.0f / period);
		#endregion
#if CUSTOMDEBUG
		// Start a new DebugLog
		logString = "";
		Log(ref logString, "New Generate()");
		double time;
		Stopwatch generateTime = new();
		generateTime.Start();
		startTime = new();
		startTime.Start();
#endif
		// Open a temp file to presave GIF to - Use AnimatedGifEncoder
		gifSuccess = false;
		gifEncoder = null;
		if ((applyGenerationType = selectGenerationType) >= GenerationType.EncodeGIF) {
			gifEncoder = new();
			gifEncoder.SetDelay(selectDelay); // Framerate
			gifEncoder.SetRepeat(0);    // Loop
			gifEncoder.SetQuality(1);   // Highest quality
			gifEncoder.SetTransparent(selectAmbient < 0 ? Color.Black : Color.Empty);
			byte gifIndex = 0;
			while (gifIndex < 255) {
				gifTempPath = "gif" + gifIndex.ToString() + ".tmp";
				if (!gifEncoder.Start(selectWidth, selectHeight, gifTempPath)) {
#if CUSTOMDEBUG
					Log(ref logString, "Error opening gif file.");
#endif
					++gifIndex;
					continue;
				} else break;
			}
			if (gifIndex == 255)
				gifEncoder = null;
		}
		// Initialize the starting default animation values
		float size = 2400, angle = selectDefaultAngle * (float)Math.PI / 180.0f, hueAngle = selectDefaultHue / 120.0f;
		byte color = 0;
		widthBorder = (short)(selectWidth - 2);
		heightBorder = (short)(selectHeight - 2);
		bloom1 = selectBloom + 1;
		upleftStart = -selectBloom;
		rightEnd = widthBorder + bloom1;
		downEnd = heightBorder + bloom1;
		ambnoise = (short)(selectAmbient * selectNoise);
		var spin = selectSpin < -1 ? (short)random.Next(-2, 2) : selectSpin;
		ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
		applyBlur = (short)(selectBlur + 1);
		for (var i = selectDefaultZoom < 0 ? random.Next(0, selectPeriod * finalPeriodMultiplier) : (selectDefaultZoom % (selectPeriod * finalPeriodMultiplier)); 0 <= --i; IncFrameSize(ref size, selectPeriod)) ;
		// Generate the images
		while (!cancel.Token.IsCancellationRequested && bitmapsFinished < bitmap.Length) {
			// Initialize buffers (delete and reset if size changed)
			if ((applyMaxTasks = Math.Max((short)2, selectMaxTasks)) != allocatedTasks) {
				if (allocatedTasks >= 0)
					for (var t = 0; t < allocatedTasks; ++t) {
						var task = tasks[t];
						if (task.taskStarted)
							task.Join();
					}
				rect = new(0, 0, allocatedWidth = selectWidth, allocatedHeight = selectHeight);
				tasks = new FractalTask[allocatedTasks = applyMaxTasks];
				tuples = new (short, (float, float), (float, float), byte, int, byte)[applyMaxTasks * 8];
				//voidDepth = new short[batchTasks][][];
				//voidQueue = new Queue<(short, short)>[batchTasks];
				//parallelTasks = new Task[batchTasks + 1];
				//taskStarted = new bool[batchTasks + 1];
				//parallelTaskFinished = new byte[batchTasks + 1];
				//buffer = new Vector3[allocatedTasks = batchTasks][][];

				for (short t = applyMaxTasks; 0 <= --t;) {
					var task = tasks[t] = new FractalTask();
					task.voidDepth = new short[applyMaxTasks][];
					task.buffer = new Vector3[applyMaxTasks][];
					task.taskIndex = t;
					NewBuffer(task);
				}
				SetMaxIterations(true);
			}
			if (selectHeight != allocatedHeight) {
				for (short t = 0; t < applyMaxTasks; NewBuffer(tasks[t++])) ;
				rect = new(0, 0, allocatedWidth = selectWidth, allocatedHeight = selectHeight);
			}
			if (selectWidth != allocatedWidth) {
				for (short t = 0; t < applyMaxTasks; ++t) {
					var task = tasks[t];
					var buffT = task.buffer;
					var voidT = task.voidDepth;
					for (short y = 0; y < selectHeight; voidT[y++] = new short[selectWidth]) 
						buffT[y] = new Vector3[selectWidth];	
				}
				rect = new(0, 0, allocatedWidth = selectWidth, selectHeight);
			}
			var generateLength = applyGenerationType > GenerationType.OnlyImage ? bitmap.Length : 1;
			// Wait if no more frames to generate
			if (bitmapsFinished >= generateLength)
				continue;
			// Use the selected ParallelType, or OfDepth if OnlyImage (if you want a GenerateDots_SingleTask like OfAnimation with OnlyImage, set the maxTasks to <= 2)
			applyParallelType = applyGenerationType == GenerationType.OnlyImage ? ParallelType.OfDepth : selectParallelType;
			// Image parallelism
			//imageTasks = applyParallelType == 2 ? [] : null;
			FinishTasks(true, applyParallelType > 0 || applyMaxTasks <= 2 
				? (short taskIndex) => {
					if (nextBitmap >= generateLength)
						return false;// The task is finished, no need to wait for this one
					ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
					GenerateDots(nextBitmap++, (short)-taskIndex, size, angle, spin, hueAngle, color);
					IncFrameParameters(ref size, ref angle, spin, ref hueAngle, 1);
					return true; // A task finished, but started another one - keep checking before new master loop
				} : (short taskIndex) => {
					if (nextBitmap >= generateLength)
						return false;// The task is finished, no need to wait for this one
					var task = tasks[taskIndex];
					ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
					var bmp = nextBitmap++;
					float _size = size, _angle = angle, _hueAngle = hueAngle; 
					var _spin = spin; 
					var _color = color;
					task.Start(bmp, () => GenerateDots(bmp, taskIndex, _size, _angle, _spin, _hueAngle, _color));
					IncFrameParameters(ref size, ref angle, spin, ref hueAngle, 1);
					return true; // A task finished, but started another one - keep checking before new master loop
				});
		}
		// Wait for threads to finish
		for (var t = allocatedTasks; 0 <= --t; tasks[t].Join()) ;
		// Unlock unfinished bitmaps:
		for (var b = 0; b < bitmap.Length; ++b)
			if (bitmapState[b] is > BitmapState.Dots and < BitmapState.Finished) {
				try {
					//bitmapState[bitmapsFinished] = BitmapState.Finished;
					bitmap[b]?.UnlockBits(bitmapData[b]);
				} catch (Exception) { }
			}
		// Save the temp GIF file
		gifSuccess = false;
		if (applyGenerationType >= GenerationType.EncodeGIF && gifEncoder != null && gifEncoder.Finish()) {
			while (/*!cancel.Token.IsCancellationRequested && */gifEncoder != null) {

				switch (gifEncoder.TryWrite()) {
					case TryWrite.Failed:
						// we alrady unlocked the bitmaps above
						//for (gifEncoder = null; bitmapsFinished < bitmap.Length; ++bitmapsFinished) {
						//if (bitmapState[bitmapsFinished] is > BitmapState.Dots and < BitmapState.Finished)
						//	bitmap[bitmapsFinished].UnlockBits(bitmapData[bitmapsFinished]);
						//bitmapState[bitmapsFinished] = BitmapState.Finished;
						//}
						gifEncoder = null;
						return;
					case TryWrite.Waiting:
						// This should never be hit, because the coe reches this loop only after getting cancelled or every frame finished
						Thread.Sleep(100);
						break;
					case TryWrite.FinishedFrame:
					// This should never be hit, because the coe reches this loop only after getting cancelled or every frame finished
					// we alrady unlocked the bitmaps above
					//int unlock = gifEncoder.FinishedFrame() - 1;
					//bitmap[unlock].UnlockBits(bitmapData[unlock]);
					//bitmapState[unlock] = BitmapState.Finished;
					//while (bitmapState[bitmapsFinished] == BitmapState.Finished)
					//	++bitmapsFinished;
					break;
					case TryWrite.FinishedAnimation:
						gifSuccess = true;
						// This will follow with gifEncoder.IsFinished()
						break;
				}
				if (gifEncoder.IsFinished())
					break;
			}
		}
#if CUSTOMDEBUG
		else Log(ref logString, "Error closing gif file.");
		generateTime.Stop();
		time = generateTime.Elapsed.TotalMilliseconds;
		Log(ref logString, "Generate time = " + time + " ms.\n");
		var n = bitmap.Length / .1f;
		logString = "Cs:\nInit: " + Math.Floor(initTimes / n) 
			+ "\nIter: " + Math.Floor(iterTimes / n) 
			+ "\nVoid: " + Math.Floor(voidTimes / n) 
			+ "\nDraw: " + Math.Floor(drawTimes / n) 
			+ "\nGifs: " + Math.Floor(gifsTimes / n)
			+ "\n" + logString;
		File.WriteAllText("log.txt", logString);
#endif
		MakeDebugString();
		cancel.Cancel(); // If the gifEncoder failed and got nulled but is still running some tasks
		gifEncoder = null;
		mainTask = null;
	}
	#endregion

	#region Interface_Calls
	// start the generator in a separate main thread so that the form can continue being responsive
	internal void StartGenerate() => mainTask = Task.Run(GenerateAnimation, (cancel = new()).Token);
	internal void ResetGenerator() {
		applyZoom = (short)(selectZoom != 0 ? selectZoom : random.NextDouble() < .5f ? -1 : 1);
		applyCutparam = selectCutparam < 0 ? (short)random.Next(0, cutparamMaximum) : selectCutparam;
		SetupColor();
		// Get the multiplier of the basic period required to get to a seamless loop
		var m = applyHueCycle == 0 && childColor[0] > 0 ? selectPeriodMultiplier * 3 : selectPeriodMultiplier;
		bool asymmetric = childAngle[0] < 2 * Math.PI;
		bool doubled = Math.Abs(selectSpin) > 1 || selectSpin == 0 && asymmetric;
		finalPeriodMultiplier = (short)(doubled ? 2 * m : m);
		// A complex expression to calculate the minimum needed hue shift speed to match the loop:
		hueCycleMultiplier = (byte)(applyHueCycle == 0 ? 0 : childColor[0] % 3 == 0 ? 3 : 1 +
				(childColor[0] % 3 == 1 == (1 == applyHueCycle) == (1 == applyZoom) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3
		);
		applyPeriodAngle = selectPeriodMultiplier % 2 == 0 && asymmetric && !doubled ? selectPeriodAngle * 2 : selectPeriodAngle;
		// setup bitmap data
		bitmapsFinished = nextBitmap = 0;
		var frames = (short)(debug > 0 ? debug : selectPeriod * finalPeriodMultiplier);
		if (frames != allocatedFrames) {
			bitmap = new Bitmap[allocatedFrames = frames];
			bitmapData = new BitmapData[frames];
			bitmapState = new BitmapState[frames + 1];
		}
		for (int b = frames; b >= 0; bitmapState[b--] = BitmapState.Queued);
	}
	internal void RequestCancel() {
		cancel?.Cancel();
		try {
			mainTask?.Wait();
		} catch (Exception) { }
	}
	internal bool SaveGif(string gifPath) {
		try {
			// Try to save (move the temp) the gif file
			gifEncoder?.Finish();
			//gifEncoder.Output(gifPath);
			File.Move(gifTempPath, gifPath);
		} catch (IOException ex) {
			var exs = "SaveGif: An error occurred: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return true;
		} catch (UnauthorizedAccessException ex) {
			var exs = "SaveGif: Access denied: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return true;
		} catch (Exception ex) {
			var exs = "SaveGif: Unexpected error: " + ex.Message;
#if CUSTOMDEBUG
			Log(ref logString, exs);
#endif
			return true;
		}
		return gifSuccess = false;
	}
#if CUSTOMDEBUG
	private void Log(ref string log, string line) {
		Debug.WriteLine(line);
		log += "\n" + line;
	}
#endif
	private void MakeDebugString() {
		static string GetBitmapState(BitmapState state) => state switch {
			BitmapState.Queued => "QUEUED (NOT SPAWNED)",
			BitmapState.Dots => "GENERATING FRACTAL DOTS (NOT SPAWNED)",
			BitmapState.Void => "GENERATING DIJKSTRA VOID (NOT SPAWNED)",
			BitmapState.Drawing => "DRAWING (LOCKED)",
			BitmapState.Encoding => "ENCODING (LOCKED)",
			BitmapState.Finished => "FINISHED (UNLOCKED)",
			_ => "ERROR! (SHOULDN'T HAPPEN)",
		};

		if (!debugmode)
			return;
		if (cancel.Token.IsCancellationRequested) {
			debugString = "ABORTING";
			return;
		}
		string _debugString = "TASKS:";
		for (var t = 0; t < applyMaxTasks; ++t) {
			var task = tasks[t];
			_debugString += "\n" + t + ": ";
			switch (task.state) {
				case TaskState.Running:
					_debugString += "RUN: " + GetBitmapState(bitmapState[task.bitmapIndex]);
					break;
				case TaskState.Done:
					_debugString += "DONE: " + GetBitmapState(bitmapState[task.bitmapIndex]);
					break;
				case TaskState.Free:
					_debugString += "FREE";
					break;
			}
		}
		var laststate = BitmapState.Error;
		var b = 0;
		for (var i = 0; i < 8; counter[i++] = 0) ;
		for (_debugString += "\n\nIMAGES:"; b < bitmapsFinished; ++b)
			++counter[(int)bitmapState[b]];
		var _memoryString = "";
		while (b < bitmap.Length && bitmapState[b] > BitmapState.Queued) {
			var state = bitmapState[b];
			if (state != laststate) {
				if (laststate != BitmapState.Error) 
					_memoryString += "-" + (b - 1) + ": " + GetBitmapState(laststate);
				_memoryString += "\n" + b;
				laststate = state;
			}
			++counter[(int)state];
			++b;
		}
		if (bitmapState[bitmapsFinished] > BitmapState.Queued) 
			_memoryString += "-" + (b - 1) + ": " + GetBitmapState(laststate);
		for (int c = 0; c < 8; ++c) 
			_debugString += "\n" + counter[c] +"x: "+ GetBitmapState((BitmapState)c);
		_debugString += "\n" + _memoryString;
		debugString = b < bitmap.Length ? _debugString + "\n" + b + "+: " + "QUEUED" : _debugString;
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
		selectSaturate = 1.0f;
		selectDetail = .25f;
		SelectThreadingDepth();
		selectCut = selectChildAngle = selectChildColor = 0;
		SetupColor();
		SetupAngle();
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
		f = fractals[selectFractal];
		logBase = (float)Math.Log(f.childSize);
		SetMaxIterations();
	}
	internal void SetMaxIterations(bool forcedReset = false) {
		short newMaxIterations = (short)(2 + Math.Ceiling(Math.Log(Math.Max(selectWidth, selectHeight) * f.maxSize / selectDetail) / logBase));
		if (newMaxIterations <= maxIterations && !forcedReset) {
			maxIterations = newMaxIterations;
			return;
		}
		maxIterations = newMaxIterations;
		for (int t = 0; t < allocatedTasks; ++t) {
			var preIterateTask = tasks[t].preIterate = new (float, float, (float, float)[])[maxIterations];
			for (int i = 0; i < maxIterations; preIterateTask[i++] = (0.0f, 0.0f, null));
		}
	}
	internal void SetupAngle() {
		childAngle = f.childAngle[selectChildAngle].Item2;
		selectPeriodAngle = f.childCount <= 0 ? 0 : childAngle[0] % (2.0f * (float)Math.PI);
	}
	internal void SetupColor() {
		// Unpack the color palette and hue cycling
		if (selectHue < 0) {
			applyHueCycle = (short)random.Next(-1, 2);
			applyColorPalette = (short)random.Next(0, 2);
		} else {
			applyHueCycle = (short)((selectHue / 2 + 1) % 3 - 1);
			applyColorPalette = (short)(selectHue % 2);
		}
		// Setup colors
		if (f.childCount > 0) {
			var ca = f.childColor[selectChildColor].Item2;
			for (var i = f.childCount; 0 <= --i; childColor[i] = ca[i]) ;
			// Setup palette
			for (var i = 0; i < f.childCount; ++i)
				childColor[i] = (applyColorPalette == 0) ? childColor[i] : (byte)((3 - childColor[i]) % 3);
		}
		// Prepare subiteration color blend
		//SetupColorBlend();
		float[] colorBlendF = [0, 0, 0];
		for (int i = 0; i < f.childCount; ++i)
			colorBlendF[childColor[i]] += 1.0f / f.childCount;
		colorBlend = new(colorBlendF[0], colorBlendF[1], colorBlendF[2]);
	}
	internal void SetupCutFunction() => cutFunction = f.cutFunction == null || f.cutFunction.Length <= 0 ? null : f.cutFunction[selectCut].Item2;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void SelectThreadingDepth() {
		//preIterate = new (float, float, (float, float)[])[Math.Max((short)1,selectMaxTasks)][];
		SetMaxIterations(true);
		maxDepth = 0;
		if (f.childCount <= 0)
			return;
		for (int n = 1, threadCount = 0; (threadCount += n) < selectMaxTasks - 1; n *= f.childCount)
			++maxDepth;
	}
	#endregion

	#region Interface_Getters
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Fractal[] GetFractals() => fractals;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Fractal GetFractal() => fractals[selectFractal];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Bitmap GetBitmap(int index) => bitmap == null || bitmap.Length <= index ? null : bitmap[index];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetFrames() => bitmap == null ? 0 : bitmap.Length;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetBitmapsFinished() => bitmapsFinished;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsGifReady() => gifSuccess;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//GetTempGif();
	internal Fractal.CutFunction GetCutFunction() => fractals[selectFractal].cutFunction?[selectCut].Item2;
	#endregion
}
