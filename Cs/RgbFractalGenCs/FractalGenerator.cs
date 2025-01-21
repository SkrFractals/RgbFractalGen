// Allow debug code, comment this when releasing for slightly better performance
//#define CUSTOMDEBUG

namespace RgbFractalGenCs {

#if CUSTOMDEBUG
	using System.Diagnostics;
#endif

	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Collections.Concurrent;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Drawing;
	using System.Drawing.Imaging;
	using Gif.Components;
	using System.Numerics;
	using System.DirectoryServices.ActiveDirectory;
	using static System.Runtime.InteropServices.JavaScript.JSType;
	using System.Drawing.Printing;
	using System.Windows.Media;
	using System.Drawing.Drawing2D;
	using System.Collections;
	using System.Reflection.Metadata;
	using System.Reflection;
	using System.Windows.Media.Media3D;

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	internal class FractalGenerator {

#if CUSTOMDEBUG
		// Debug variables
		private string logString;						// Debug log
		Stopwatch startTime;							// Start Stopwatch
		private double initTimes, iterTimes, voidTimes, drawTimes, gifTimes;
#endif

		// Fractal definitions
		private Fractal[] fractals;                     // Fractal definitions
		private Fractal f;                              // Selected fractal definition

		// Generated data
		private Vector3[][][] buffer;                   // Buffer for points to print into bmp
		private short[][][] voidDepth;                  // Depths of Void
		private Queue<(short, short)>[] voidQueue;
		private byte[] childColor;                      // A copy of childColor for allowing BGR
		private float[] childAngle;                     // A copy of selected childAngle
		private Bitmap[] bitmap;                        // Prerender as an array of bitmaps
		private byte[] bitmapState;                     // 0 - not exists, 1 - spawned and locked, 2 - finished drawing locked, 3 - started gif locked, 4 - finished unlocked
		private BitmapData[] bitmapData;                // Locked Bits for bitmaps

		// Threading
		private readonly object taskLock = new();       // Monitor lock
		private CancellationTokenSource cancel;         // Cancellation Token Source
		private Task mainTask;                          // Main Generation Task
		private ConcurrentBag<Task> imageTasks;         // Parallel Image Tasks
		private Task[] parallelTasks;					// Parallel Animation Tasks
		private byte[] parallelTaskFinished;			// States of Animation Tasks
		(float, float, float, float, float, byte, int)[] tuples;
		List<Task> taskSnapshot;                        // Snapshot for safely checking imageTasks Wait

		// Generation variables
		private byte hueCycleMultiplier, applyParallelType;				
		private short selectColorPalette,
			bitmapsFinished, nextBitmap, finalPeriodMultiplier,
			select, selectColor, selectAngle, selectCut, allocatedWidth, allocatedHeight, allocatedTasks, allocatedFrames, applyMaxTasks, applyMaxGenerationTasks,
			ambnoise;
		private Fractal.CutFunction cutFunction;		// Selected CutFunction pointer
		private bool gifSuccess, exportingGif;          // Temp GIF file "gif.tmp" successfuly created | flag only allowing one GIF Encoding thread
		private AnimatedGifEncoder gifEncoder;          // Export GIF encoder
		private float logBase, periodAngle;             // Normalizer to keep fractal luminance constant | Normalizer for maximum void depth | Size Log Base 
		private Vector3 colorBlend;                     // Color blend of a selected fractal for more seamless loop (between single color to the sum of children)
		private System.Drawing.Rectangle rect;          // Bitmap rectangle TODO implement
		private string gifTempPath;						// Temporary GIF file name
		private Random random = new();                  // Random generator

		// Settings
		internal float detail, noise, saturate;
		internal byte selectBlur, encode, extraHue, extraSpin, parallelType;
		internal short zoom, defaultSpin, hueCycle, maxDepth, periodMultiplier;
		internal short debug, width, height, period, delay, amb, defaultZoom, defaultAngle, defaultHue;
		internal short maxTasks, maxGenerationTasks, cutparam;

		#region Init
		internal FractalGenerator() {
			applyParallelType = parallelType = 0;
			debug = defaultHue = defaultZoom = defaultAngle = extraSpin = extraHue = 0;
			zoom = 1;
			select = selectColor = selectAngle = selectCut = allocatedWidth = allocatedHeight = allocatedTasks = allocatedFrames = defaultSpin = selectColorPalette = -1;
			encode = 2;
			gifEncoder = null;
			bitmap = null;
			gifSuccess = false;
			taskSnapshot = [];
			InitFractalDefinitions();
		}
		private void InitFractalDefinitions() {
			// Constants
			float pi = MathF.PI, pi23 = 2 * pi / 3, pi43 = 4 * pi / 3, SYMMETRIC = 2 * pi,
				stt = MathF.Sqrt(.75f),
				pfs = 2 * (1 + (float)MathF.Cos(.4f * pi)),
				cosc = MathF.Cos(.4f * pi) / pfs,
				sinc = MathF.Sin(.4f * pi) / pfs,
				v = 2 * (sinc * sinc + cosc * (cosc + pfs)) / (2 * sinc),
				s0 = ((2 + MathF.Sqrt(2)) / MathF.Sqrt(2)),
				sx = 2 + MathF.Sqrt(2),
				r = (1 / s0 + 1 / sx) / (1 / s0 + 1 / (2 * sx)),
				diag = (MathF.Sqrt(2) / 3);
			// X, Y
			float[] cx = new float[9], cy = new float[9], pfx = new float[6], pfy = new float[6], tfx = new float[4], tfy = new float[4], ttfx = new float[16], ttfy = new float[16], ofx = new float[9], ofy = new float[9];
			// Carpets
			for (int i = 0; i < 4; ++i) {
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

				new("TriTree", 10, 4, .2f, .05f, .9f,
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
				new("TriComb", 13, 5, .2f, .05f, .9f,
				[0, 2 * stt, -stt, -stt, 3 * stt, 0, -3 * stt, -3 * stt, 0, 3 * stt, 2 * stt, -4 * stt, 2 * stt],
				[0, 0, 1.5f, -1.5f, 1.5f, 3, 1.5f, -1.5f, -3, -1.5f, 3, 0, -3],
				[("Classic", [pi / 3, 0, pi23, pi43, pi, pi + pi23, pi + pi23, pi + pi43, pi + pi43, pi, pi43, 0, pi23])],
				[
					("Center", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]),
					("Bridges", [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2])
				],
				[("NoChild", Fractal.TriComb_Param)]
				),
				new("Triflake", 4, 2, 1.5f, .175f, .8f, tfx, tfy,
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
				new("SierpinskiCarpet", 9, 3, .9f, .175f, .9f, cx, cy,
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
				new("Pentaflake", 6, pfs, .2f * pfs, .15f, .9f, pfx, pfy,
				[
					("Classic", [2 * pi / 10, 0, 0, 0, 0, 0]),
					("No Center Rotation", [2 * pi + 2 * pi / 5, 0, 0, 0, 0, 0])
				], [("Center", [1, 0, 0, 0, 0, 0])],
				[
					("NoChild",Fractal.Pentaflake_NoCornerParam),
					("NoBackDiag", Fractal.Pentaflake_NoBackDiag)
				]
				),
				new("Hexaflake", 7, 3, .5f, .1f, 1,
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
				new("HexaCircular", 19, 5, .2f, .05f, .9f,
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
				], []
				)
				];
			int maxChildren = 1;
			foreach (var i in fractals) {
				if (i.childCount > maxChildren)
					maxChildren = i.childCount;
			}
			childColor = new byte[maxChildren];
		}
		private void NewBuffer(short taskIndex) {
			var voidT = voidDepth[taskIndex] = new short[height][];
			var buffT = buffer[taskIndex] = new Vector3[height][];
			for (var y = 0; y < height; voidT[y++] = new short[width]) {
				var buffY = buffT[y] = new Vector3[width];
				for (var x = 0; x < width; buffY[x++] = Vector3.Zero) ;
			}
		}
		private void InitBuffer(short taskIndex) {
			var buffT = buffer[taskIndex];
			for (var y = 0; y < height; ++y) {
				var buffY = buffT[y];
				for (var x = 0; x < width; buffY[x++] = Vector3.Zero) ;
			}
		}
		private void SetupColorBlend() {
			float[] colorBlendF = [0, 0, 0];
			foreach (var i in childColor)
				colorBlendF[i] += 1.0f / f.childCount;
			colorBlend = new(colorBlendF[0], colorBlendF[1], colorBlendF[2]);
		}
		#endregion

		#region Generate_Tasks
		private void GenerateAnimation() {
#if CUSTOMDEBUG
			// Start a new DebugLog
			logString = "";
			Log(ref logString, "New Generate()");
			double time;
			var generateTime = new();
			generateTime.Start();
			startTime = new();
			startTime.Start();
#endif
			// Open a temp file to presave GIF to - Use AnimatedGifEncoder
			exportingGif = gifSuccess = false;
			gifEncoder = null;
			if (encode >= 2) {
				gifEncoder = new();
				gifEncoder.SetDelay(delay); // Framerate
				gifEncoder.SetRepeat(0);    // Loop
				gifEncoder.SetQuality(1);   // Highest quality
				byte gifIndex = 0;
				while (gifIndex < 255) {
					gifTempPath = "gif" + gifIndex.ToString() + ".tmp";
					if (!gifEncoder.Start(gifTempPath, width, height)) {
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
			float size = 2400, angle = defaultAngle * (float)Math.PI / 180.0f, hueAngle = defaultHue / 120.0f;
			byte color = 0;
			ambnoise = (short)(amb * noise);
			var spin = defaultSpin;
			ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
			for (int i = defaultZoom; 0 <= --i; IncFrameSize(ref size, period)) ;
			// Generate the images
			while (!cancel.Token.IsCancellationRequested && bitmapsFinished < bitmap.Length) {

				// Initialize buffers (delete and reset if size changed)
				applyMaxGenerationTasks = maxGenerationTasks;
				var batchTasks = Math.Max((short)1, applyMaxTasks = maxTasks);
				if (batchTasks != allocatedTasks) {
					rect = new(0, 0, allocatedWidth = width, allocatedHeight = height);
					tuples = new (float, float, float, float, float, byte, int)[Math.Max(1, applyMaxGenerationTasks * 8)];
					voidDepth = new short[batchTasks][][];
					voidQueue = new Queue<(short, short)>[batchTasks];
					parallelTasks = new Task[batchTasks + 1];
					parallelTaskFinished = new byte[batchTasks + 1];
					buffer = new Vector3[batchTasks][][];
					for (short t = 0; t < batchTasks; NewBuffer(t++))
						voidQueue[t] = new();
				}
				if (height != allocatedHeight) {
					rect = new(0, 0, allocatedWidth = width, allocatedHeight = height);
					for (short t = 0; t < batchTasks; NewBuffer(t++)) ;
				}
				if (width != allocatedWidth) {
					rect = new(0, 0, allocatedWidth = width, height);
					for (int t = 0; t < batchTasks; ++t) {
						var voidT = voidDepth[t];
						var buffT = buffer[t];
						for (short y = 0; y < height; voidT[y++] = new short[width])
							buffT[y] = new Vector3[width];
					}
				}
				for (int t = 0; t <= batchTasks; parallelTaskFinished[t++] = 2)
					parallelTasks[t] = null;

				var generateLength = (encode > 0 ? bitmap.Length : 1);
				// Wait if no more frames to generate
				if (bitmapsFinished >= generateLength)
					continue;
				// Image parallelism
				imageTasks = (applyParallelType = parallelType) == 2 ? new ConcurrentBag<Task>() : null;
				// Animation Parallelism Task Count
				var animationTaskCount = applyParallelType > 0 ? (short)0 : Math.Min(batchTasks, (short)generateLength);
				if (animationTaskCount <= 1) {
					// No Animation Parallelism:
					if (nextBitmap < generateLength) {
						ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
						GenerateImage(nextBitmap++, batchTasks, size, angle, spin, hueAngle, color);
						IncrameParameters(ref size, ref angle, spin, ref hueAngle, ref color, 1);
						parallelTaskFinished[batchTasks] = 2;
					}
					var gifTaskIndex = Math.Max((short)0, applyMaxGenerationTasks);
					if (FinishTask(gifTaskIndex))
						TryGif(gifTaskIndex);
				} else {
					// Animation parallelism: Spawn initial tasks
					/*while (0 <= --animationTaskCount) {
						if (animationTaskFinished[animationTaskCount] >= 2 && nextBitmap < generateLength) {
							animationTaskFinished[animationTaskCount] = 0;
							ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
							var _bitmap = nextBitmap++;
							var _task = animationTaskCount;
							var _size = size;
							var _angle = angle;
							var _spin = spin;
							var _hueAngle = hueAngle;
							var _color = color;
							//GenerateThread(_bitmap, _task, _size, _angle, _hueAngle, _color);
							animationTasks[_task] = (Task.Run(() => GenerateThread(_bitmap, _task, _size, _angle, _spin, _hueAngle, _color)));
							IncrementFrameParameters(ref size, ref angle, spin, ref hueAngle, ref color, 1);
						}
					}*/
					// Animation parallelism: Continue/Finish Animation and Gif tasks
					bool tasksRemaining = true;
					while (tasksRemaining) {
						tasksRemaining = false;
						// Check every task
						for (var task = 0; task < batchTasks; ++task) {
							if (FinishTask(task)) {
								TryGif((short)task);
								if (parallelTaskFinished[task] >= 2 && nextBitmap < generateLength && !cancel.Token.IsCancellationRequested && maxTasks == applyMaxTasks) {
									// Start another task when previous was finished
									parallelTaskFinished[task] = 0;
									ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
									var _bitmap = nextBitmap++;
									var _task = (short)task;
									var _size = size;
									var _angle = angle;
									var _spin = spin;
									var _hueAngle = hueAngle;
									var _color = color;
									//GenerateThread(_bitmap, _task, _size, _angle, _hueAngle, _color);
									parallelTasks[_task] = (Task.Run(() => GenerateImage(_bitmap, _task, _size, _angle, _spin, _hueAngle, _color)));
									tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
									IncrameParameters(ref size, ref angle, spin, ref hueAngle, ref color, 1);
								}
							} else tasksRemaining = true; // A task not finished yet - keep checking before new master loop
						}
					}
				}
			}
			// Wait for threads to finish
			//WaitForThreads();
			for (int i = parallelTasks.Length; 0 <= --i; parallelTasks[i]?.Wait()) ;
			// Unlock unfinished bitmaps:
			for (int i = 0; i < bitmap.Length; ++i)
				if (bitmapState[i] > 0 && bitmapState[i] < 4) {
					try {
						bitmap[i]?.UnlockBits(bitmapData[i]);
					} catch (Exception) { }
				}
			// Save the temp GIF file
			if (encode >= 2 && gifEncoder != null && gifEncoder.Finish())
				gifSuccess = true;
#if CUSTOMDEBUG
			else Log(ref logString, "Error closing gif file.");
			generateTime.Stop();
			time = generateTime.Elapsed.TotalMilliseconds;
			Log(ref logString, "Generate time = " + time + " ms.\n");
			double n = bitmap.Length / .1;
			logString = "Cs:\nInit: " + Math.Floor(initTimes / n) 
				+ "\nIter: " + Math.Floor(iterTimes / n) 
				+ "\nVoid: " + Math.Floor(voidTimes / n) 
				+ "\nDraw: " + Math.Floor(drawTimes / n) 
				+ "\nGifs: " + Math.Floor(gifTimes / n)
				+ "\n" + logString;
			File.WriteAllText("log.txt", logString);
#endif
			gifEncoder = null;
			mainTask = null;
		}
		private void GenerateImage(short bitmapIndex, short taskIndex, float size, float angle, short spin, float hueAngle, byte color) {
#if CUSTOMDEBUG
			string threadString = "";
			var initTime = new();
			initTime.Start();
#endif
			short bufferIndex = (short)(taskIndex % applyMaxTasks);
			InitBuffer(bufferIndex);
			var buffT = buffer[bufferIndex];
			var voidT = voidDepth[bufferIndex];

#if CUSTOMDEBUG
			initTime.Stop();
			Log(ref threadString, "Init:" + bitmapIndex + " time = " + initTime.Elapsed.TotalMilliseconds + " ms.");
			var iterTime = new();
			iterTime.Start();
#endif
			// Generate the fractal frame recursively
			VecRefWrapper R = new VecRefWrapper(buffT, Vector3.Zero, Vector3.Zero);
			for (int b = 0; b < selectBlur; ++b) {
				if (cancel.Token.IsCancellationRequested) {
					parallelTaskFinished[taskIndex] = 2;
					return;
				}
				ModFrameParameters(ref size, ref angle, ref spin, ref hueAngle, ref color);
				SetupFrameColorBlend(hueAngle, ref R);
				if (applyMaxGenerationTasks <= 1)
					GenerateDots_SingleTask(R, width * .5f, height * .5f, angle, Math.Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam);
				else switch (applyParallelType) {
					case 0:
						GenerateDots_SingleTask(R, width * .5f, height * .5f, angle, Math.Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam);
						break;
					case 1:
						tuples[0] = ( width * .5f, height * .5f, angle, Math.Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam );
						GenerateDots_OfDepth(R);
						break;
					case 2:
						GenerateDots_OfRecursion(R, width * .5f, height * .5f, angle, Math.Abs(spin) > 1 ? 2 * angle : 0, size, color, -cutparam, 0);
						WaitForRecursiveTasks();
						break;
				}
				IncrameParameters(ref size, ref angle, spin, ref hueAngle, ref color, selectBlur);
			}
			if (cancel.Token.IsCancellationRequested) {
				parallelTaskFinished[taskIndex] = 2;
				return;
			}
#if CUSTOMDEBUG
			iterTime.Stop();
			Log(ref threadString, "Iter:" + bitmapIndex + " time = " + iterTime.Elapsed.TotalMilliseconds + " ms.");
			var voidTime = new();
			voidTime.Start();
#endif
			// Generate the grey void areas
			float lightNormalizer = 0.1f, voidDepthMax = 1.0f;
			GenerateVoid(voidQueue[bufferIndex], buffT, voidT, ref lightNormalizer, ref voidDepthMax);
#if CUSTOMDEBUG
			voidTime.Stop();
			Log(ref threadString, "Void:" + bitmapIndex + " time = " + voidTime.Elapsed.TotalMilliseconds + " ms.");
			var drawTime = new();
			drawTime.Start();
#endif
			if (cancel.Token.IsCancellationRequested) {
				parallelTaskFinished[taskIndex] = 2;
				return;
			}
			// Draw the generated pixel to bitmap data
			GenerateBitmap(bitmapIndex, buffT, voidT, lightNormalizer, voidDepthMax);
#if CUSTOMDEBUG
			drawTime.Stop();
			Log(ref threadString, "Draw:" + bitmapIndex + " time = " + drawTime.Elapsed.TotalMilliseconds + " ms.");

			Monitor.Enter(taskLock);
			try {
				initTimes += initTime.Elapsed.TotalMilliseconds;
				iterTimes += iterTime.Elapsed.TotalMilliseconds;
				voidTimes += drawTime.Elapsed.TotalMilliseconds;
				drawTimes += drawTime.Elapsed.TotalMilliseconds;
				Log(ref logString, threadString);
			} finally { Monitor.Exit(taskLock); }
#endif
			// Lets the generator know it's finished so it can encode gif it it's allowed to, if not it just lets the form know it can diplay it
			bitmapState[bitmapIndex] = 2;
			parallelTaskFinished[taskIndex] = 1;
		}
		private void GenerateDots_SingleTask(VecRefWrapper R,
			float inX, float inY, float inAngle, float inAntiAngle, float inSize,
			byte inColor, int inFlags
		) {
			if (inSize < detail) {
				ApplyDot(R, inX, inY, inSize, inColor);
				return;
			}
			// Split Iteration Deeper
			var newSize = inSize / f.childSize;
			var i = f.childCount;
			while (0 <= --i) {
				if (cancel.Token.IsCancellationRequested)
					return;
				// Special Cutoff
				int newFlags = CalculateFlags(i, inFlags);
				if (newFlags < 0)
					continue;
				// Outside View
				var newX = NewX(i, inX, inAngle, inSize);
				var newY = NewY(i, inY, inAngle, inSize);
				if (TestSize(newX, newY, inSize)) 
					continue;
				// Iteration Tasks
				if (i == 0)
					GenerateDots_SingleTask(R, newX, newY, inAngle + childAngle[i] - inAntiAngle, -inAntiAngle, newSize, (byte)((inColor + childColor[i]) % 3), newFlags);
				else
					GenerateDots_SingleTask(R, newX, newY, inAngle + childAngle[i], inAntiAngle, newSize, (byte)((inColor + childColor[i]) % 3), newFlags);
			}
		}
		private void GenerateDots_OfDepth(VecRefWrapper R) {
			int index = 0, insertTo = 1, max = applyMaxGenerationTasks * 8, maxcount = max - f.childCount - 1, count = (max + insertTo - index) % max;
			while (count > 0 && count < maxcount) {
				(var inX, var inY, var inAngle, var inAntiAngle, var inSize, var inColor, var inFlags) = tuples[index++];
				if (inSize < detail) {
					ApplyDot(R, inX, inY, inSize, inColor);
					continue;
				}
				// Split Iteration Deeper
				var newSize = inSize / f.childSize;
				var i = f.childCount;
				while (0 <= --i) {
					if (cancel.Token.IsCancellationRequested)
						return;
					// Special Cutoff
					int newFlags = CalculateFlags(i, inFlags);
					if (newFlags < 0)
						continue;
					// Outside View
					var newX = NewX(i, inX, inAngle, inSize);
					var newY = NewY(i, inY, inAngle, inSize);
					if (TestSize(newX, newY, inSize))
						continue;
					var newAntiAngle = inAntiAngle;
					var newAngle = NewAngle(i, inAngle, inAntiAngle, ref newAntiAngle);
					byte newColor = NewColor(i, inColor);
					tuples[insertTo++] = ( newX, newY, newAngle, newAntiAngle, newSize, newColor, newFlags ); 
					insertTo %= max;
				}
				count = (max + insertTo - index) % max;
			}
			while (count > 0 && !cancel.Token.IsCancellationRequested) {
				// more parallels to compute
				bool tasksRemaining = true;
				while (tasksRemaining) {
					if (FinishTask(applyMaxGenerationTasks))
						TryGif(applyMaxGenerationTasks);
					tasksRemaining = false;
					// Check every task
					for (int task = 0; task < applyMaxGenerationTasks; ++task) {
						if (FinishTask(task)) {
							if (parallelTaskFinished[task] >= 2 && count > 0 && !cancel.Token.IsCancellationRequested) {
								// Start another task when previous was finished
								parallelTaskFinished[task] = 0;
								var t = task;
								var i = index++;
								parallelTasks[task] = (Task.Run(() => Task_OfDepth(R, t, i)));
								index %= max;
								count = (max + insertTo - index) % max;
								tasksRemaining = true; // A task finished, but started another one - keep checking before new master loop
							}
						} else tasksRemaining = true; // A task not finished yet - keep checking before new master loop
					}
				}
			}
		}
		private void GenerateDots_OfRecursion(VecRefWrapper R,
			float inX, float inY, float inAngle, float inAntiAngle, float inSize,
			byte inColor, int inFlags, byte inDepth
		) {
			if (inSize < detail) {
				ApplyDot(R, inX, inY, inSize, inColor);
				return;
			}
			// Split Iteration Deeper
			var newDepth = (byte)(inDepth + 1);
			var newSize = inSize / f.childSize;
			var i = f.childCount;
			while (0 <= --i) {
				if (cancel.Token.IsCancellationRequested)
					return;
				// Special Cutoff
				int newFlags = CalculateFlags(i, inFlags);
				if (newFlags < 0)
					continue;
				// Outside View
				var newX = NewX(i, inX, inAngle, inSize);
				var newY = NewY(i, inY, inAngle, inSize);
				if (TestSize(newX, newY, inSize))
					continue;
				var newAntiAngle = inAntiAngle;
				var newAngle = NewAngle(i, inAngle, inAntiAngle, ref newAntiAngle);
				byte newColor = NewColor(i, inColor);
				if (imageTasks != null && imageTasks.Count < applyMaxGenerationTasks && inDepth < maxDepth)
					imageTasks.Add(Task.Run(() => GenerateDots_OfRecursion(R, newX, newY, newAngle, newAntiAngle, newSize, newColor, newFlags, newDepth)));
				else
					GenerateDots_OfRecursion(R, newX, newY, newAngle, newAntiAngle, newSize, newColor, newFlags, newDepth);
			}
		}
		private void GenerateVoid(Queue<(short, short)> queueT, Vector3[][] buffT, short[][] voidT, ref float lightNormalizer, ref float voidDepthMax) {
			short voidYX, w1 = (short)(width - 1), h1 = (short)(height - 1);
			if (cancel.Token.IsCancellationRequested)
				return;
			if (amb > 0) {
				// Void Depth Seed points (no points, no borders), leet the void depth generator know where to start incrementing the depth
				float lightMax;
				for (short y = 1; y < h1; ++y) {
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
				for (short x = 0; x < width; ++x) {
					void0[x] = voidH[x] = 0;
					queueT.Enqueue((0, x));
					queueT.Enqueue((h1, x));
				}
				// Depth of Void (fill the void of incrementally larger values of depth, that will generate the grey areas)
				short voidMax = 0;
				while (queueT.Count > 0) {
					(short y, short x) = queueT.Dequeue();
					short ym = (short)(y - 1), yp = (short)(y + 1), xm = (short)(x - 1), xp = (short)(x + 1);
					voidMax = Math.Max(voidMax, voidYX = (short)(voidT[y][x] + 1));
					if (xp < width && voidT[y][xp] == -1) { voidT[y][xp] = voidYX; queueT.Enqueue((y, xp)); }
					if (yp < height && voidT[yp][x] == -1) { voidT[yp][x] = voidYX; queueT.Enqueue((yp, x)); }
					if (xm >= 0 && voidT[y][xm] == -1) { voidT[y][xm] = voidYX; queueT.Enqueue((y, xm)); }
					if (ym >= 0 && voidT[ym][x] == -1) { voidT[ym][x] = voidYX; queueT.Enqueue((ym, x)); }
				}
				voidDepthMax = voidMax;
			} else
				for (short y = 0; y < height; ++y) {
					var buffY = buffT[y];
					for (short x = 0; x < width; ++x) {
						var buffYX = buffY[x];
						lightNormalizer = MathF.Max(lightNormalizer, MathF.Max(buffYX.X, MathF.Max(buffYX.Y, buffYX.Z)));
					}
				}
			lightNormalizer = 160.0f / lightNormalizer;
		}
		private void GenerateBitmap(short bitmapIndex, Vector3[][] buffT, short[][] voidT, float lightNormalizer, float voidDepthMax) {
			if (cancel.Token.IsCancellationRequested)
				return;
			unsafe {
				// Make a locked bitmap, remember the locked state
				byte* p = (byte*)(void*)((bitmapData[bitmapIndex] = (bitmap[bitmapIndex] = new(width, height)).LockBits(rect,
					ImageLockMode.WriteOnly,
					System.Drawing.Imaging.PixelFormat.Format24bppRgb)).Scan0);
				bitmapState[bitmapIndex] = 1;
				// Draw the bitmap with the buffer dat we calculated with GenerateFractal and Calculate void
				// Switch between th selected settings such as saturation, noise, image parallelism...
				if (applyParallelType > 0 && applyMaxGenerationTasks > 1) {
					// Multi Threaded
					var stride = 3 * width;
					ParallelOptions po = new ParallelOptions {
						MaxDegreeOfParallelism = applyMaxGenerationTasks,
						CancellationToken = cancel.Token
					};
					try {
						if (ambnoise > 0) {
							var threadRand = new ThreadLocal<Random>(() => new(Guid.NewGuid().GetHashCode()));
							if (saturate > 0.0) Parallel.For(0, height, po, y => {
								var rp = p + y * stride;
								NoiseSaturate(buffT[y], voidT[y], ref rp, threadRand.Value, lightNormalizer, voidDepthMax);
							});
							else Parallel.For(0, height, po, y => {
								var rp = p + y * stride;
								NoiseNoSaturate(buffT[y], voidT[y], ref rp, threadRand.Value, lightNormalizer, voidDepthMax);
							});
						} else {
							if (saturate > 0.0) Parallel.For(0, height, po, y => {
								var rp = p + y * stride;
								NoNoiseSaturate(buffT[y], voidT[y], ref rp, lightNormalizer, voidDepthMax);
							});
							else Parallel.For(0, height, po, y => {
								var rp = p + y * stride;
								NoNoiseNoSaturate(buffT[y], voidT[y], ref rp, lightNormalizer, voidDepthMax);
							});
						}
					} catch (Exception) { }
				} else {
					// Single Threaded
					if (ambnoise > 0) {
						if (saturate > 0.0) for (short y = 0; y < height; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoiseSaturate(buffT[y], voidT[y], ref p, random, lightNormalizer, voidDepthMax);
							}
						else for (short y = 0; y < height; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoiseNoSaturate(buffT[y], voidT[y], ref p, random, lightNormalizer, voidDepthMax);
							}
					} else {
						if (saturate > 0.0) for (short y = 0; y < height; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoNoiseSaturate(buffT[y], voidT[y], ref p, lightNormalizer, voidDepthMax);
							}
						else for (short y = 0; y < height; ++y) {
								if (cancel.Token.IsCancellationRequested)
									continue;
								NoNoiseNoSaturate(buffT[y], voidT[y], ref p, lightNormalizer, voidDepthMax);
							}
					}
				}
			}
		}
		private void GenerateGif(short taskIndex) {
			// Sequentially encode a finished GIF frame, then unlock the bitmap data
			if (cancel.Token.IsCancellationRequested) {
				parallelTaskFinished[taskIndex] = 2;
				return;
			}
#if CUSTOMDEBUG
			string threadString = "";
			var gifTime = new();
			gifTime.Start();
#endif
			unsafe {
				// Save Frame to a temp GIF
				if (encode >= 2 && !cancel.Token.IsCancellationRequested && gifEncoder != null && !gifEncoder.AddFrame(cancel.Token, (byte*)(void*)(bitmapData[bitmapsFinished].Scan0))) {
#if CUSTOMDEBUG
				Log(ref threadString, "Error writing gif frame.");
#endif
					gifEncoder.Finish();
					gifEncoder = null;
				}
			}
#if CUSTOMDEBUG
			gifTime.Stop();
			Log(ref threadString, "Gifs:" + bitmapsFinished + " time = " + gifTime.Elapsed.TotalMilliseconds + " ms.");
			Monitor.Enter(taskLock);
			try{
				gifTimes += gifTime.Elapsed.TotalMilliseconds;
				Log(ref logString, threadString);
			} finally {
				Monitor.Exit(taskLock);
			}
#endif
			//Save BMP to RAM
			if (bitmapState[bitmapsFinished] < 4) {
				bitmap[bitmapsFinished].UnlockBits(bitmapData[bitmapsFinished]); // Lets me generate next one, and lets the GeneratorForm know that this one is ready
				bitmapState[bitmapsFinished++] = 4;
			}
			exportingGif = false;
			parallelTaskFinished[taskIndex] = 1;
		}
		private bool FinishTask(int taskIndex) {
			if (parallelTaskFinished[taskIndex] == 1) {
				parallelTasks[taskIndex]?.Wait();
				parallelTasks[taskIndex] = null;
				parallelTaskFinished[taskIndex] = 2;
			}
			return parallelTaskFinished[taskIndex] >= 2;
		}
		private void TryGif(short taskIndex) {
			if (bitmapState[bitmapsFinished] != 2)
				return;
			if (gifEncoder != null && !exportingGif && !cancel.Token.IsCancellationRequested && maxTasks == applyMaxTasks) {
				exportingGif = true;
				parallelTaskFinished[taskIndex] = 0;
				bitmapState[bitmapsFinished] = 3;
				parallelTasks[taskIndex] = (Task.Run(() => GenerateGif(taskIndex)));
			} else {
				bitmap[bitmapsFinished].UnlockBits(bitmapData[bitmapsFinished]);
				bitmapState[bitmapsFinished++] = 4;
			}
		}
		private void WaitForRecursiveTasks() {
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
		}
		#endregion

		#region Generate_Inline
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ApplyDot(VecRefWrapper R, float inX, float inY, float inSize, float inColor) {
			Vector3 dotColor = Vector3.Lerp(R.H, R.I, (float)Math.Log(detail / inSize) / logBase);
			switch (inColor) {
				case 1:
					dotColor = new(dotColor.Z, dotColor.X, dotColor.Y);
					break;
				case 2:
					dotColor = new(dotColor.Y, dotColor.Z, dotColor.X);
					break;
			}
			// Iterated deep into a single point - Interpolate inbetween 4 pixels and Stop
			int ys = (int)MathF.Floor(inY), xe = (int)MathF.Ceiling(inX), ye = (int)MathF.Ceiling(inY);
			for (int y, x = (int)MathF.Floor(inX); x <= xe; x++)
				for (y = ys; y <= ye; y++)
					R.buffT[y][x] += ((1.0f - MathF.Abs(x - inX)) * (1.0f - MathF.Abs(y - inY))) * dotColor;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int CalculateFlags(int index, int inFlags) { return cutFunction == null ? inFlags : cutFunction(index, inFlags); }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		float NewX(int index, float inX, float inAngle, float inSize) {
			return inX + inSize * (f.childX[index] * MathF.Cos(inAngle) - f.childY[index] * (float)Math.Sin(inAngle));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		float NewY(int index, float inY, float inAngle, float inSize) {
			return inY - inSize * (f.childY[index] * MathF.Cos(inAngle) + f.childX[index] * (float)Math.Sin(inAngle));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool TestSize(float newX, float newY, float inSize) {
			var testSize = inSize * f.cutSize; 
			return ((Math.Min(newX, newY) + testSize <= 2) || (newX - testSize >= width - 3) || (newY - testSize >= height - 3));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		float NewAngle(int index, float inAngle, float inAntiAngle, ref float newAntiAngle) {
			return index == 0 ? inAngle + childAngle[index] + (newAntiAngle = -inAntiAngle) : inAngle + childAngle[index];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		byte NewColor(int index, byte inColor) {
			return (byte)((inColor + childColor[index]) % 3);
		}
		Vector3 ApplyAmbientNoise(Vector3 rgb, float Amb, float Noise, Random rand) {
			rgb.X += Noise * rand.Next(ambnoise) + Amb;
			rgb.Y += Noise * rand.Next(ambnoise) + Amb;
			rgb.Z += Noise * rand.Next(ambnoise) + Amb;
			return rgb;
			//rgb += ((1.0f - voidAmb) * voidAmb) * new Vector3(rand.Next(ambnoise), rand.Next(ambnoise), rand.Next(ambnoise)) + new Vector3(voidAmb * amb);
		}
		unsafe private Vector3 ApplySaturate(Vector3 rgb) {
			// The saturation equation boosting up the saturation of the pixel (powered by the saturation slider setting)
			float m, min = MathF.Min(MathF.Min(rgb.X, rgb.Y), rgb.Z), max = MathF.Max(MathF.Max(rgb.X, rgb.Y), rgb.Z);
			return max <= min ? rgb : ((m = max * saturate / (max - min)) + 1 - saturate) * rgb - new Vector3(min * m);
		}
		unsafe private void ApplyRGBToBytePointer(Vector3 rgb, ref byte* p) {
			// Without gamma:
			p[0] = (byte)(rgb.Z);
			p[1] = (byte)(rgb.Y);
			p[2] = (byte)(rgb.X);
			// With gamma:
			/*
			p[0] = (byte)(255f * Math.Sqrt(rgb.Z / 255f));
			p[1] = (byte)(255f * Math.Sqrt(rgb.Y / 255f));
			p[2] = (byte)(255f * Math.Sqrt(rgb.X / 255f));
			*/
			p += 3;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		unsafe private void NoiseSaturate(Vector3[] buffY, short[] voidY, ref byte* p, Random rand, float lightNormalizer, float voidDepthMax) {
			if (amb <= 0)
				for (var x = 0; x < width; ApplyRGBToBytePointer(ApplySaturate(lightNormalizer * buffY[x++]), ref p)) ;
			else for (var x = 0; x < width; ++x) {
					var voidAmb = voidY[x] / voidDepthMax;
					ApplyRGBToBytePointer(ApplyAmbientNoise(ApplySaturate(lightNormalizer * buffY[x]), voidAmb * amb, (1.0f - voidAmb) * voidAmb, rand), ref p);
				}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		unsafe private void NoNoiseSaturate(Vector3[] buffY, short[] voidY, ref byte* p, float lightNormalizer, float voidDepthMax) {
			if (amb <= 0)
				for (var x = 0; x < width; ApplyRGBToBytePointer(ApplySaturate(lightNormalizer * buffY[x++]), ref p)) ;
			else for (var x = 0; x < width; ++x)
					ApplyRGBToBytePointer(new Vector3(amb * voidY[x] / voidDepthMax) + ApplySaturate(lightNormalizer * buffY[x]), ref p);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		unsafe private void NoiseNoSaturate(Vector3[] buffY, short[] voidY, ref byte* p, Random rand, float lightNormalizer, float voidDepthMax) {
			if (amb <= 0)
				for (var x = 0; x < width; ApplyRGBToBytePointer(lightNormalizer * buffY[x++], ref p)) ;
			else for (var x = 0; x < width; x++) {
					var voidAmb = voidY[x] / voidDepthMax;
					ApplyRGBToBytePointer(ApplyAmbientNoise(lightNormalizer * buffY[x], voidAmb * amb, (1.0f - voidAmb) * voidAmb, rand), ref p);
				}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		unsafe private void NoNoiseNoSaturate(Vector3[] buffY, short[] voidY, ref byte* p, float lightNormalizer, float voidDepthMax) {
			if (amb <= 0) for (var x = 0; x < width; x++)
					ApplyRGBToBytePointer(lightNormalizer * buffY[x], ref p);
			else for (var x = 0; x < width; x++)
					ApplyRGBToBytePointer(new Vector3(amb * voidY[x] / voidDepthMax) + lightNormalizer * buffY[x], ref p);
		}
		#endregion

		#region TaskWrappers
		private void Task_OfDepth(VecRefWrapper R, int taskindex, int tupleIndex) {
			(var inX, var inY, var inAngle, var inAntiAngle, var inSize, var inColor, var inFlags) = tuples[tupleIndex];
			GenerateDots_SingleTask(R, inX, inY, inAngle, inAntiAngle, inSize, inColor, inFlags);
			parallelTaskFinished[taskindex] = 1;

		}
		#endregion

		#region AnimationParameters
		private void SwitchParentChild(ref float angle, ref short spin, ref byte color) {
			if (Math.Abs(spin) > 1) {
				// reverse angle and spin when antispinning, or else the direction would change when parent and child switches
				angle = -angle;
				spin = (short)(-spin);
			}
			color = (byte)((3 + color + zoom * childColor[0]) % 3);
		}
		private void ModFrameParameters(ref float size, ref float angle, ref short spin, ref float hueAngle, ref byte color) {
			var fp = f.childSize;
			var w = Math.Max(width, height) * f.maxSize;
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
		private void IncrameParameters(ref float size, ref float angle, float spin, ref float hueAngle, ref byte color, byte blur) {
			var blurPeriod = period * blur;
			// Zoom Rotation angle and Zoom Hue Cycle and zoom Size
			angle += spin * (periodAngle * (1 + extraSpin)) / (finalPeriodMultiplier * blurPeriod);
			hueAngle += (hueCycleMultiplier + 3 * extraHue) * (float)hueCycle / (finalPeriodMultiplier * blurPeriod);
			IncFrameSize(ref size, blurPeriod);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncFrameSize(ref float size, int period) {
			// Zoom Size
			size *= MathF.Pow(f.childSize, zoom * 1.0f / period);
		}
		private void SetupFrameColorBlend(float hueAngle, ref VecRefWrapper R) {
			// Prepare Color blending per one dot (hueshifting + iteration correction)
			// So that the color of the dot will slowly approach the combined colors of its childer before it splits
			var lerp = hueAngle % 1.0f;
			switch ((byte)hueAngle % 3) {
				case 0:
					R.I = Vector3.Lerp(Vector3.UnitX, Vector3.UnitY, lerp);
					R.H = Vector3.Lerp(colorBlend, new Vector3(colorBlend.Z, colorBlend.X, colorBlend.Y), lerp);
					break;
				case 1:
					R.I = Vector3.Lerp(Vector3.UnitY, Vector3.UnitZ, lerp);
					R.H = Vector3.Lerp(new Vector3(colorBlend.Z, colorBlend.X, colorBlend.Y), new Vector3(colorBlend.Y, colorBlend.Z, colorBlend.X), lerp);
					break;
				case 2:
					R.I = Vector3.Lerp(Vector3.UnitZ, Vector3.UnitX, lerp);
					R.H = Vector3.Lerp(new Vector3(colorBlend.Y, colorBlend.Z, colorBlend.X), colorBlend, lerp);
					break;
			}
		}
		#endregion

		#region Interface_Calls
		internal void StartGenerate() {
			// start the generator in a separate main thread so that the form can continue being responsive
			mainTask = Task.Run(() => GenerateAnimation(), (cancel = new()).Token);
		}
		internal void ResetGenerator() {
			finalPeriodMultiplier = GetFinalPeriod();
			// A complex expression to calculate the minimum needed hue shift speed to match the loop:
			hueCycleMultiplier = (byte)(hueCycle == 0 ? 0 : childColor[0] % 3 == 0 ? 2 : 1 +
					(childColor[0] % 3 == 1 == (1 == hueCycle) == (1 == zoom) ? 29999 - finalPeriodMultiplier : 2 + finalPeriodMultiplier) % 3 
			);
			// setup bitmap data
			bitmapsFinished = nextBitmap = 0;
			short frames = (short)(debug > 0 ? debug : period * finalPeriodMultiplier);
			if (frames != allocatedFrames) {
				bitmap = new Bitmap[allocatedFrames = frames];
				bitmapData = new BitmapData[frames];
				bitmapState = new byte[frames + 1];
			}
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
				if (gifEncoder != null)
					gifEncoder.Finish();
				//gifEncoder.Output(gifPath);
				File.Move(gifTempPath, gifPath);
			} catch (IOException ex) {
				string exs = "SaveGif: An error occurred: " + ex.Message;
#if CUSTOMDEBUG
				Log(ref logString, exs);
#endif
				return true;
			} catch (UnauthorizedAccessException ex) {
				string exs = "SaveGif: Access denied: " + ex.Message;
#if CUSTOMDEBUG
				Log(ref logString, exs);
#endif
				return true;
			} catch (Exception ex) {
				string exs = "SaveGif: Unexpected error: " + ex.Message;
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
			logString += "\n" + line;
		}
#endif
		internal void DebugStart() {
			// debug for testing, starts the generator with predetermined setting for easy breakpointing
			// Need to enable the definition on top GeneratorForm.cpp to enable this
			// if enabled you will be unable to use the settings interface!
			SelectFractal(44);
			SelectThreadingDepth();
			SelectDetail(1);
			period = debug = 7;
			width = 8;//1920;
			height = 8;//1080;
			parallelType = 1;
			maxDepth = -1;//= 2;
			maxGenerationTasks = maxTasks = -1;// 10;
			saturate = 1.0f;
			SelectDetail(.25f);
		}
		#endregion

		#region Interface_Settings
		internal bool SelectFractal(short select) {
			if (this.select == select)
				return true;
			// new fractal definition selected - let the form know to reset and restart me
			this.select = select;
			return false;
		}
		internal void SetupFractal() {
			// setup the new fractal definition, with all parameters reset to 0
			f = fractals[select];
			logBase = (float)Math.Log(f.childSize);
			SelectThreadingDepth();
			selectCut = selectAngle = selectColor = -1;
			if (!SelectColor(0))
				SetupColor();
			SelectAngle(0);
			SelectCutFunction(0);
		}
		internal bool SelectAngle(short selectAngle) {
			if (this.selectAngle == selectAngle)
				return true;
			this.selectAngle = selectAngle;
			(_, childAngle) = f.childAngle[selectAngle];
			periodAngle = f.childCount <= 0 ? 0 : childAngle[0] % (2.0f * (float)Math.PI);
			return false;
		}
		internal bool SelectColor(short selectColor) {
			if (this.selectColor == selectColor)
				return true;
			this.selectColor = selectColor;
			return false;
		}
		internal bool SelectColorPalette(byte selectColorPalette) {
			if (this.selectColorPalette == selectColorPalette)
				return true;
			this.selectColorPalette = selectColorPalette;
			return false;
		}
		internal void SetupColor() {
			// Setup colors
			if (f.childCount > 0) {
				(_, byte[] ca) = f.childColor[selectColor];
				for (int i = f.childCount; 0 <= --i; childColor[i] = ca[i]) ;
				// Setup palette
				for (int i = 0; i < f.childCount; ++i)
					childColor[i] = (selectColorPalette == 0) ? childColor[i] : (byte)((3 - childColor[i]) % 3);
			}
			// Prepare subiteration color blend
			SetupColorBlend();
		}
		internal bool SelectCutFunction(short selectCut) {
			if (this.selectCut == selectCut) {
				return true;
			}
			this.selectCut = selectCut;
			if (f.cutFunction == null || f.cutFunction.Length <= 0)
				cutFunction = null;
			else (_, cutFunction) = f.cutFunction[selectCut];
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool SelectDetail(float detail) {
			if (this.detail == detail * f.minSize)
				return true;
			// detail is different, let the form know to restart me
			this.detail = detail * f.minSize;
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void SelectThreadingDepth() {
			maxDepth = 0;
			if (f.childCount <= 0)
				return;
			for (int n = 1, threadCount = 0; (threadCount += n) < maxGenerationTasks; n *= f.childCount)
				++maxDepth;
		}
		#endregion

		#region Interface_Getters
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Fractal[] GetFractals() { return fractals; }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Fractal GetFractal() { return f; }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Bitmap GetBitmap(int index) { return bitmap == null || bitmap.Length <= index ? null : bitmap[index]; }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal short GetFinalPeriod() {
			// get the multiplier of the basic period required to get to a seamless loop
			int m = hueCycle == 0 && childColor[0] > 0 ? periodMultiplier * 3 : periodMultiplier;
			return (short)(Math.Abs(defaultSpin) > 1 || (defaultSpin == 0 && childAngle[0] < 2 * Math.PI) ? 2 * m : m);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal int GetFrames() { return bitmap == null ? 0 : bitmap.Length; }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal int GetBitmapsFinished() { return bitmapsFinished; }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool IsGifReady() { return gifSuccess; }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//GetTempGif();
		internal Fractal.CutFunction GetCutFunction() { return cutFunction; }
		#endregion
	}
}
