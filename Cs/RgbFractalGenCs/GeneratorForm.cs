// Starts the generator with special testing settings
//#define CustomDebugTest

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComputeSharp;

namespace RgbFractalGenCs;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class GeneratorForm : Form {

	#region Designer
	public GeneratorForm() {
		InitializeComponent();
		screenPanel = new() {
			Location = new(239, 13),
			Name = "screenPanel",
			Size = new Size(80, 80),
			TabIndex = 25
		};
		screenPanel.Paint += ScreenPanel_Paint;
		screenPanel.Click += AnimateButton_Click;
		Controls.Add(screenPanel);
		//MouseDown += Control_MouseDown; // Detect clicks on empty form space
		//RegisterMouseDownRecursive(this); // Detect clicks on all controls
	}
	#endregion

	//public const string charOn = "✓ ";
	//public const string charOff = "✕ ";

	//public const string charOn = "[■]";
	//public const string charOff = "[□]";

	public const string charOn = "[⬛] ";
	public const string charOff = "[⬜] ";
	private string[] filePostfix;

	#region Variables
	// Threading
	private bool bInit = true;
	private readonly List<Control>
		myControls = [];            // All persistent interactive controls
	private readonly List<bool>
		myControlsEnabled = [];     // Memory of which controls were enabled after we disabled all of them temporairly, to recover that alter
	private readonly FractalGenerator
		generator = new();          // The core of the app, the generator the generates the fractal animations
	private CancellationTokenSource
		xCancel, aCancel;           // Cancellation Token Sources
	private Task xTask;             // Export tasks
	private Task aTask;             // Abort Task
	private bool queueAbort;        // Generator abortion queued
	private short queueReset;       // Counting time until generator Restart
	private int isGifReady;
	private bool isPngsSaved = false;

	// Settings
	private bool animated = true;   // Animating preview or paused? (default animating)
	private bool
		modifySettings = true;      // Allows for modifying settings without it triggering Aborts and Generates
	private short width = -1, height = -1;
	private int maxTasks;           // Maximum tasks available
	private ushort abortDelay = 500; // Set time to restart generator
	private short restartTimer;
	private string gifPath = "";    // Gif export path name
	private string mp4Path = "";    // Mp4 export path name
	private Fractal toSave;
	private int batchIndex;
	private string batchFolder;

	// Display Variables
	private readonly DoubleBufferedPanel
		screenPanel;                // Display panel
	private Bitmap
		currentBitmap;              // Displayed Bitmap
	private int currentBitmapIndex; // Play frame index
	private int fx, fy;             // Memory of window size
	private int controlTabIndex;    // Iterator for tabIndexes - to make sure all the controls tab in the correct order even as I add new ones in the middle
	private int pointTabIndex;      // Iterator for tabIndexes for dynamically generated controls in editor
	private bool notify = true, notifyExp;
	private bool cacheLabelUpdate = false;

	// Editor
	private readonly List<(TextBox, TextBox, TextBox, TextBox, Button)>
		editorPoint = [];           // Point row buttons/textboxes in editor
	private readonly List<Button>
		editorSwitch = [];          // Switch buttons in editor
	private FractalGenerator.GenerationType
		memGenerate;                // Selected gen type before wsithing to editor, to recover when switching back
	private FractalGenerator.PngType
		memPng;
	private FractalGenerator.GifType
		memGif;
	private ushort memBlur, memAbort;
	private float memBloom;
	private double memDefaultHue;
	private bool performHash;
	private bool previewMode = true;

	// Config
	private ushort fileMask = 0, voidAmbientMax = 30, voidAmbientMul = 4, voidNoiseMax = 30, voidScaleMax = 300, detailMax = 10, saturateMax = 100, brightnessMax = 300, bloomMax = 40, blurMax = 40, l2Max = 4096, stripeMax = 0, binMax = 4096;
	private float bloomMul = .25f;
	private double detailMul = .1, threadsMul = 1.0, voidNoiseMul = .1;
	private string loadedBatch;
	private string[] runningBatch;

	#endregion

	#region Core
	private static bool Error(string text, string caption) {
		_ = MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		return false;
	}
	private void UpdateBitmap(Bitmap bitmap) {
		if (currentBitmap == bitmap || bitmap == null)
			return;
		// Update the display with the bitmap when it's not loaded
		currentBitmap = bitmap;
		screenPanel?.Invalidate();
	}
	private void UpdatePreview() {
		Monitor.Enter(this);
		try {
			var bitmapsFinished = generator.GetBitmapsFinished();
			// Fetch a bitmap to display						
			UpdateBitmap(bitmapsFinished > 0
				? generator.GetBitmap(currentBitmapIndex = (animated ? currentBitmapIndex + 1 : currentBitmapIndex) % bitmapsFinished) // Make sure the index is in range
				: generator.GetPreviewBitmap() // Try preview bitmap if none of the main ones are generated yet
			);
		} finally { Monitor.Exit(this); }
	}
	private void UpdateCache() {
		cacheLabelUpdate = true;
	}
	private void SetupEditControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, tip);
		control.TabIndex = ++pointTabIndex;
		myControls.Add(control);
	}
	/// <summary>
	/// Refreshing and preview animation
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void Timer_Tick(object sender, EventArgs e) {
		#region Init
		void Init() {

			FractalGenerator.WarmUpShaders();

			bInit = false;

			fractalSelect.MouseWheel += ComboBox_MouseWheel;
			angleSelect.MouseWheel += ComboBox_MouseWheel;
			colorSelect.MouseWheel += ComboBox_MouseWheel;
			cutSelect.MouseWheel += ComboBox_MouseWheel;
			paletteSelect.MouseWheel += ComboBox_MouseWheel;
			zoomSelect.MouseWheel += ComboBox_MouseWheel;
			hueSelect.MouseWheel += ComboBox_MouseWheel;
			spinSelect.MouseWheel += ComboBox_MouseWheel;
			parallelTypeSelect.MouseWheel += ComboBox_MouseWheel;
			encodePngSelect.MouseWheel += ComboBox_MouseWheel;
			encodeGifSelect.MouseWheel += ComboBox_MouseWheel;
			generationSelect.MouseWheel += ComboBox_MouseWheel;
			timingSelect.MouseWheel += ComboBox_MouseWheel;
			fileSelect.MouseWheel += ComboBox_MouseWheel;
			voidSelect.MouseWheel += ComboBox_MouseWheel;
			drawSelect.MouseWheel += ComboBox_MouseWheel;

			// prepare the file encoder
			filePostfix = new string[fileSelect.Items.Count];
			for (int i = fileSelect.Items.Count; 0 <= --i; filePostfix[i] = fileSelect.Items[i].ToString()) { }
			fileMask = (ushort)((1 << (filePostfix.Length - 1)) - 1);
			FillSelectFile();

			colorSelect.DrawItem += BitmaskComboBox_DrawItem;
			angleSelect.DrawItem += BitmaskComboBox_DrawItem;
			fileSelect.DrawItem += BitmaskComboBox_DrawItem;
			colorSelect.DrawMode = angleSelect.DrawMode = fileSelect.DrawMode = DrawMode.OwnerDrawFixed;

			// Init the generator
			foreach (var i in generator.GetFractals())
				fractalSelect.Items.AddRange([i.Name]);
			generator.SelectedFractal = -1;
			//generator.RestartGif = false;
			generator.UpdatePreview += UpdatePreview;
			generator.UpdateCache += UpdateCache;

			// Setup interactable controls (tooltips + tabIndex)
			SetupControl(sizeBox, "Scale Of children inside (how much to scale the image when switching parent<->child)");
			SetupControl(maxBox, "The root scale, if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance.\nIf pieces of the fractal are disappearing at the CORNERS, you should increase it, if not you can try decreasing a little.");
			SetupControl(minBox, "How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance).\n Try setting the detail parameter to maximum, and it the fractal starts dithering when zooming, you should decrease the minSize.");
			SetupControl(cutBox, "A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not you can decrease to boost performance).\nIf you see pieces of fractals disappearing near the EDGES, you should increase it.");
			SetupControl(angleBox, "Type the name for a new children angle set, if you wish to add one.");
			SetupControl(addAngleButton, "Add a new children angle set.");
			SetupControl(removeAngleButton, "Remove the selected children angle set.");
			SetupControl(colorBox, "Type the name for a new children color set, if you wish to add one.");
			SetupControl(addColorButton, "Add a new children color set.");
			SetupControl(removeColorButton, "Remove the selected children color set.");
			SetupControl(addCut, "Add a CutFunction (so far only the pre-coded are available)");
			SetupControl(loadButton, "Load a fractal definition from a file.");
			SetupControl(saveButton, "Save the selected fractal definition to a file.");
			SetupControl(fractalSelect, "Select the type of fractal to generate");
			SetupControl(modeButton, "Toggle between editor and generator.");
			SetupControl(angleSelect, "Select the children angles definition.");
			SetupControl(colorSelect, "Select the children colors definition.");
			SetupControl(cutSelect, "Select the cutFunction definition.");
			SetupControl(cutparamBox, "Type the cutFunction seed. (-1 for random)");
			SetupControl(resX, "Type the X resolution of the render (width)");
			SetupControl(resY, "Type the Y resolution of the render (height)");
			SetupControl(resSelect, "Select a rendering resolution (the second choice is the custom resolution you can type in the boxes to the left");
			SetupControl(paletteSelect, "Choose the color palette. Then the children colors set will determine how the colors from this palette progress deep into the iteration");
			SetupControl(removePalette, "Removes this selected palette from the list.\nIf it's a default one, it will be restored on the next launch.");
			SetupControl(addPalette, "Add your own custom palette to the list. You will be prompted with series of ColorDialogs, cancelling another color pick will finish the palette.");
			SetupControl(defaultHue, "Type the initial hue angle of the first image (in degrees).");
			SetupControl(ditherBox, "Temporal Dithering: if the real color is in-between 2 byte values, it evaluates randomly to the lower or higher one, with the probability respectively.");
			SetupControl(periodBox, "How many frames for the self-similar center child to zoom in to the same size as the parent if you are zooming in.\nOr for the parent to zoom out to the same size as the child if you are zooming out.\nThis will equal the number of generated frames of the animation if the center child is the same color.\nOr it will be a third of the number of generated frames of the animation if the center child is a different color.");
			SetupControl(periodMultiplierBox, "Multiplies the frame count, slowing down the rotation and hue shifts.");
			SetupControl(zoomSelect, "Choose in which direction you want the fractal zoom.");
			SetupControl(defaultZoomBox, "Type the initial zoom of the first image in number of skipped frames. -1 for random");
			SetupControl(hueSelect, "Choose the color scheme of the fractal.\nAlso you can make the color hues slowly cycle as the fractal zoom.");
			SetupControl(hueSpeedBox, "Type the extra speed on the hue shifting from the values possible for looping.\nOnly possible if you have chosen color cycling on the left.");
			SetupControl(spinSelect, "Choose in which direction you want the zoom animation to spin, or to not spin.");
			SetupControl(defaultAngleBox, "Type the initial angle of the first image (in degrees).");
			SetupControl(spinSpeedBox, "Type the extra speed on the spinning from the values possible for looping.");
			SetupControl(ambBox, "The strength of the ambient grey color in the empty spaces far away between the generated fractal dots.\n-1 for transparent");
			SetupControl(noiseBox, "The strength of the random noise in the empty spaces far away between the generated fractal dots.");
			SetupControl(voidBox, "Scale of the void noise, so it's more visible and compressible at higher resolutions.");
			SetupControl(detailBox, "Level of Detail (The lower the finer).");
			SetupControl(saturateBox, "Enhance the color saturation of the fractal dots.\nUseful when the fractal is too gray, like the Sierpinski Triangle (TriFlake).");
			SetupControl(brightnessBox, "Brightness level: 0% black, 100% normalized maximum, 300% overexposed 3x maximum.");
			SetupControl(bloomBox, "Bloom: 0 will be maximally crisp, but possibly dark with think fractals. Higher values wil blur/bloom out the fractal dots.");
			SetupControl(blurBox, "Motion blur: Lowest no blur and fast generation, highest 10 smeared frames 10 times slower generation.");
			SetupControl(zoomChildBox, "Which child to focus and zoom into? 0 is center.");
			SetupControl(abortBox, "How many millisecond of pause after the last settings change until the generator restarts?");
			SetupControl(timingSelect, "Choose if you want to set a delay or framerate. Delay will be precisely used for GIF export, and framerate from MP4 export.");
			SetupControl(timingBox, ""); // This one is updated dynamically based on the timingSelect selection
			SetupControl(parallelTypeSelect, "Select which parallelism to be used if the left checkBox is enabled.\nOf Animation = Batching animation frames, recommended for Animations with perfect pixels.\nOf Depth/Of Recursion = parallel single image generation, recommended for fast single images, 1 in a million pixels might be slightly wrong");
			SetupControl(threadsBox, "The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
			SetupControl(cacheBox, "Should the fractal be generated with special new algorithm that aim to be friendly with cache optimization?");
			SetupControl(stripeBox, "Cache optimization parameter: Into how large horizontal stripes wouldthe image be subdivided and the rendering sorted?\n There should fit at least twice as many striped into image's height. And it should be small enough to fit into cache.\nIf either Bin or Stripe are left at zero, the app wil ltry to guess some good estimation of both.");
			SetupControl(binBox, "Cache optimization parameter: How many hundreds of dots will fit into one bin on each stripe, before a next bin is initialized?(1 dotis about 18 bytes)\nIf either Bin or Stripe are left at zero, the app will try to guess some good estimation of both.");
			SetupControl(l2Box, "Cache optimization parameter: How large is the L2 cache of one thread of your CPU? (in kilobytes)\nThis is hard tfor the app to know. If left zero, it will only do an educated guess.");
			SetupControl(voidSelect, "What and how will render the depth of void? Just original CPU, or some of the GPU variants? \n BFS should have identical quality to CPU, Fast Flood should be the fastest. Jump Flood in-between.");
			SetupControl(drawSelect, "What and how will draw the finished image? Just original CPU, or some of the GPU variants? Functions recommended for performance.");
			SetupControl(encodePngSelect, "Yes: Will export animation as PNG series, that will enable you to export the PNGS or MP4 quicker after it's finished, but not really quicker overall.");
			SetupControl(encodeGifSelect, "No: Will not encode a gif, you will not be able to export a gif afterwards, but you can switch to a Local/Global later and it will not fully reset the generator, just catches up with missing frames.\nLocal: Will encode a GIF during the generation, so you could save it when finished. With a local colormap (one for each frame). Higher color precision. Slow encoding, larger GIF file size.\nGlobal: Also encodes a GIF. But with a global color map learned from a first frame. Not recommended for changing colors. Much faster encoding, smaller GIF file size.");
			SetupControl(generationSelect, "Only Image: Will only render one still image. Cannot exprt PNG animation, or GIF animation, only a single image, the first one from a zoom animation.\nAnimation: Will render the animation as sett up by your settings.\nAll Seeds: Will not zoom/spin/shift at all, but instead cycle through all the available CutFunction seeds.");
			SetupControl(loadBatchButton, "");
			SetupControl(addBatchButton, "");
			SetupControl(saveBatchButton, "");
			SetupControl(prevButton, "Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(animateButton, "Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
			SetupControl(nextButton, "Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(restartButton, "Restarts the generator. Maybe useful for randomized settings, but to be safe you have to click it twice in a row.");
			SetupControl(helpButton, "Show README.txt.");
			SetupControl(exportButton, "");
			SetupControl(exportSelect, "Select what you want to save with the button on the left.\nHover over that button after the selection to get more info about the selection.");
			SetupControl(fileSelect, "Toggle what selected propeerties should be used for a default export file name.");
			SetupControl(debugBox, "shows a log of task and image states, to see what the generator is doing.");

			// Read the README.txt for the help button
			if (File.Exists("README.txt"))
				helpLabel.Text = File.ReadAllText("README.txt");

			// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restart the generator over and over
			generator.SelectedFps = 60;
			generator.SelectedDelay = 100 / 60;
			FillPalette();
			voidSelect.SelectedIndex = 3;
			drawSelect.SelectedIndex = 1;
			fractalSelect.SelectedIndex = 0;
			resSelect.SelectedIndex = 0;
			zoomSelect.SelectedIndex = 3;
			CacheBox_CheckedChanged(null, null);
			StripeBox_TextChanged(null, null);
			BinBox_TextChanged(null, null);
			L2Box_TextChanged(null, null);
			AbortBox_TextChanged(null, null);
			DitherBox_CheckedChanged(null, null);
			PeriodBox_TextChanged(null, null);
			PeriodMultiplierBox_TextChanged(null, null);
			ParallelTypeSelect_SelectedIndexChanged(null, null);
			TimingBox_TextChanged(null, null);
			//FpsBox_TextChanged(null, null);
			DefaultZoom_TextChanged(null, null);
			SpinSpeedBox_TextChanged(null, null);
			HueSpeedBox_TextChanged(null, null);
			DefaultHue_TextChanged(null, null);
			AmbBox_TextChanged(null, null);
			NoiseBox_TextChanged(null, null);
			BloomBox_TextChanged(null, null);
			BlurBox_TextChanged(null, null);
			SaturateBox_TextChanged(null, null);
			BrightnessBox_TextChanged(null, null);
			VoidBox_TextChanged(null, null);
			ZoomChildBox_TextChanged(null, null);
			SetAnimate();
			encodePngSelect.SelectedIndex = encodeGifSelect.SelectedIndex = parallelTypeSelect.SelectedIndex = 0;
			generationSelect.SelectedIndex = timingSelect.SelectedIndex = spinSelect.SelectedIndex = hueSelect.SelectedIndex = paletteSelect.SelectedIndex = 1;
			exportSelect.SelectedIndex = 2;
			maxTasks = Math.Max(FractalGenerator.MinTasks, Environment.ProcessorCount - 2);

			SetupFractal();
			threadsBox.Text = maxTasks.ToString();

			// try to restore the last closed settings and init the editor
			editorPanel.Visible = false;
			pointTabIndex = controlTabIndex;
			editorPanel.Location = generatorPanel.Location;
			LoadConfig();

			voidAmbientLabel.Text = "Void Ambient (0-" + voidAmbientMax + "):";
			voidNoiseLabel.Text = "Void Noise (0-" + voidNoiseMax + "):";
			voidScaleLabel.Text = "Void Scale (0-" + voidScaleMax + "):";
			detailLabel.Text = "Detail (0-" + detailMax + "):";
			saturateLabel.Text = "Saturate (0-" + saturateMax + "):";
			brightnessLabel.Text = "Brightness (0-" + brightnessMax + "):";
			blurLabel.Text = "Motion Blur (0-" + blurMax + "):";

			LoadSettings();
			FillEditor();


			// TODO remove debug
			//maxTasks = 1;
			//threadsBox.Text = maxTasks.ToString();


			// Start the generator
			runningBatch = [];
			modifySettings = helpPanel.Visible = false;
			TryResize();
			ResizeAll();

			aTask = xTask = null;
			generator.StartGenerate();

			// Load all extra fractal files
			var appDirectory = AppDomain.CurrentDomain.BaseDirectory; // Get the app's directory
			const string searchPattern = "*.fractal"; // Change to your desired file type
			var files = Directory.GetFiles(appDirectory, searchPattern);
			foreach (var file in files)
				_ = LoadFractal(file, false);

			// List all cutFunction to add in the editor
			addCut.Items.Add("Select CutFunction to Add");
			foreach (var c in Fractal.CutFunctions)
				addCut.Items.Add(c.Item1);

			SetFileMask();

			return;

			void SetupControl(Control control, string tip) {
				// Add tooltip and set the next tabIndex
				toolTips.SetToolTip(control, tip);
				control.TabIndex = ++controlTabIndex;
				myControls.Add(control);
			}
		}
		#endregion

		#region Size
		void SetMinimumSize() {
			// bw = Width - ClientWidth = 16
			// bh = Height - ClientHeight = 39
			const int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			MinimumSize = new(
				Math.Max(640, bw + width + 284),
				Math.Max(Math.Max(640, debugLabel.Bounds.Bottom + bh), bh + Math.Max(460, height + 8))
			);
			//debugLabel.Text = debugLabel.Text + " " + MinimumSize.Height.ToString();
		}
		void WindowSizeRefresh() {
			if (fx == Width && fy == Height)
				return;
			// User has manually resized the window - stretch the display
			ResizeScreen();
			SetMinimumSize();
			const int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			SetClientSizeCore(
				Math.Max(Width - bw, 314 + Math.Max(screenPanel.Width, width)),
				Math.Max(Height - bh, 8 + Math.Max(screenPanel.Height, height))
			);
			SizeAdapt();
		}
		void ResizeAll() {
			generator.SelectedWidth = (ushort)width;
			generator.SelectedHeight = (ushort)height;
			generator.SetMaxIterations();
			// Update the size of the window and display
			SetMinimumSize();
			SetClientSizeCore(width + 314, Math.Max(height + 8, 300));
			ResizeScreen();
			WindowSizeRefresh();
#if CustomDebugTest
			generator.DebugStart(); animated = false;
#endif
			if (modifySettings)
				return;
			// Resets the generator
			// (Abort should be called before this or else it will crash)
			// generator.StartGenerate(); should be called after
			//gifButton.Enabled = false;
			isGifReady = 0;
			currentBitmapIndex = 0;
			SizeAdapt();
		}
		void ResizeScreen() {
			const int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			var screenHeight = Math.Max(height, Math.Min(Height - bh - 8, (Width - bw - 314) * height / width));
			screenPanel.SetBounds(305, 4, screenHeight * width / height, screenHeight);
			helpPanel.SetBounds(305, 4, Width - bw - 314, Height - bh - 8);
			screenPanel?.Invalidate();
		}
		void SizeAdapt() {
			fx = Width;
			fy = Height;
		}
		#endregion

		if (bInit)
			Init();
		if (generator.DebugTasks || generator.DebugAnim || generator.DebugPng || generator.DebugGif) {
			debugLabel.Text = generator.DebugString;
			SetMinimumSize();
		}
		// Window Size Update
		WindowSizeRefresh();
		if (queueReset > 0) {
			if (!(xTask == null && aTask == null))
				return;
			if (queueAbort) {
				aTask = Task.Run(Abort, (aCancel = new()).Token);
				return;
			}
			if ((queueReset -= (short)timer.Interval) > 0)
				return;

			runningBatch = [];
			restartButton.Enabled = true;
			ResetRestart();
			//generator.RestartGif = false;
			isPngsSaved = notifyExp = false;
			if (performHash) {
				// remember settings
				ushort mw = generator.SelectedWidth, mh = generator.SelectedHeight;
				var mg = generator.SelectedGenerationType;
				var mp = generator.SelectedParallelType;
				var mb = generator.SelectedBloom;
				var mbl = generator.SelectedBlur;
				var mbr = generator.SelectedBrightness;
				var ma = generator.SelectedAmbient;
				// hash settings
				generator.SelectedAmbient = 0;
				generator.SelectedBrightness = 100;
				generator.SelectedBlur = 0;
				generator.SelectedBloom = 0;
				generator.SelectedParallelType = FractalGenerator.ParallelType.OfAnimation;
				generator.SelectedWidth = generator.SelectedHeight = 20;
				generator.SelectedGenerationType = FractalGenerator.GenerationType.HashSeeds;

				SetupFractal();
				ResizeAll();
				//restartButton.Enabled = true;
				//ResetRestart();
				//generator.RestartGif = false;
				generator.StartGenerate();
				// Wait until finished
				while (generator.GetBitmapsFinished() < generator.GetFrames()) { }
				// collect the hashes
				var hash = new int[generator.Hash.Count];
				var i = 0;
				foreach (var h in generator.Hash)
					hash[i++] = (int)h.Value;
				generator.GetFractal().ChildCutFunction[^1] = (generator.GetFractal().ChildCutFunction[^1].Item1, hash);
				// restore settings
				generator.SelectedAmbient = ma;
				generator.SelectedBrightness = mbr;
				generator.SelectedBlur = mbl;
				generator.SelectedBloom = mb;
				generator.SelectedParallelType = mp;
				generator.SelectedGenerationType = mg;
				generator.SelectedWidth = mw; generator.SelectedHeight = mh;
				// Restore enables
				for (i = myControls.Count; 0 <= --i; myControls[i].Enabled = myControlsEnabled[i]) {
				}

				// restart generator
				performHash = false;
				QueueReset();
			} else {
				SetupFractal();
				ResizeAll();
				generator.StartGenerate();
			}
		}
		if (restartTimer > 0 && (restartTimer -= (short)timer.Interval) <= 0)
			ResetRestart();
		// Fetch the state of generated bitmaps
		int bitmapsFinished = generator.GetBitmapsFinished(), bitmapsTotal = generator.GetFrames();
		if (bitmapsTotal <= 0)
			return;
		if (bitmapsFinished < bitmapsTotal) {
			BackColor = Color.FromArgb(64, 64, 64);
			notify = true;
		}
		if (bitmapsFinished == bitmapsTotal && notify && !generator.IsCancelRequested()) {
			BackColor = Color.FromArgb(128, 128, 64);
			notify = false;
			isPngsSaved = false;
			//	or FractalGenerator.GenerationType.AllSeedsPng;
			// Igf we're runnig batches, export immediately
			if (runningBatch.Length > 0) {

				switch (exportSelect.SelectedIndex) {
					case 0: // PNG 
						savePng.FileName = batchFolder + batchIndex + ".png";
						SavePng_FileOk(savePng, null);
						break;
					case 1: // PNGs
						saveMp4.DefaultExt = "png";
						saveMp4.Filter = "PNG video (*.png)|*.png";
						saveMp4.FileName = batchFolder + batchIndex + ".png";
						SaveMp4_FileOk(saveMp4, null);
						break;
					case 2: // MP4
						saveMp4.DefaultExt = "mp4";
						saveMp4.Filter = "MP4 video (*.mp4)|*.mp4";
						saveMp4.FileName = batchFolder + batchIndex + ".mp4";
						SaveMp4_FileOk(saveMp4, null);
						break;
					case 3: // GIF
						saveGif.DefaultExt = "gif";
						saveGif.Filter = "GIF video (*.gif)|*.gif";
						saveGif.FileName = batchFolder + batchIndex + ".gif";
						SaveGif_FileOk(saveGif, null);
						break;
					case 4: // GIF->Mp4
						saveGif.DefaultExt = "mp4";
						saveGif.Filter = "MP4 video (*.mp4)|*.mp4";
						saveGif.FileName = batchFolder + batchIndex + ".mp4";
						SaveGif_FileOk(saveGif, null);
						break;
				}
			}
		}
		if (notifyExp) {
			notifyExp = false;
			FinishedExporting();
		}

		// Only Allow GIF Export when generation is finished
		//string v = generator.selectGenerationType == FractalGenerator.GenerationType.Mp4 ? "Mp4" : "Gif";
		isGifReady = generator.IsGifReady();
		exportButton.Text = xTask != null
			? "Saving"
			: exportSelect.SelectedIndex switch {
				0 => "Save PNG",
				1 => "Save PNGs",
				2 => "Save MP4",
				3 => "Save GIF",
				4 => "Save MP4",
				5 => "Load File",
				_ => "Undefined",
			};

		/*if (gTask == null) {
			gifButton.Enabled = aTask == null && (isGifReady = generator.IsGifReady()) != 0;
			gifButton.Text = "Save " + v;
		} else if (gifButton.Text != "Cancel") {
			gifButton.Enabled = true;
			gifButton.Text = "Cancel";
		}*/
		// Fetch a bitmap to display
		UpdatePreview();
		if (cacheLabelUpdate) {
			stripeLabel.Text = "Stripe Height: " + generator.GetStripeHeight();
			binLabel.Text = "Bin Size: " + generator.GetBinSize();
			cacheLabelUpdate = false;
		}
		// Info text refresh
		var infoText = " / " + bitmapsTotal;
		if (bitmapsFinished < bitmapsTotal) {
			statusLabel.Text = "Generating: ";
			infoText = bitmapsFinished + infoText;
		} else {
			if (xTask != null) {
				statusLabel.Text = "Saving: ";
				infoText = generator.GetCompleted() + infoText;
				//infoText = generator.GetPngFinished() + infoText;
			} else {
				statusLabel.Text = "Finished: ";
				infoText = currentBitmapIndex + infoText;
			}
		}
		infoLabel.Text = infoText;
		//gifButton.Text = gTask == null ? "Save GIF" : "Saving";
	}

	private void LoadConfig() {
		if (!File.Exists("config.txt"))
			return;
		var s = File.ReadAllLines("config.txt");

		//var s = File.ReadAllText("config.txt").Split('|');
		for (var i = 0; i < s.Length; i += 1) {
			if (s[i][0] == '/' || !s[i].Contains('='))
				continue;
			var c = s[i].Split('=');
			if (c[0] == "resolution") {
				// skip and continue if the resolution is not a valid format <int>x<int>
				if (!c[1].Contains('x'))
					continue;
				var r = c[1].Split('x');
				if (r.Length != 2 || !(int.TryParse(r[0], out _) && int.TryParse(r[1], out _)))
					continue;
				_ = resSelect.Items.Add(c[1]);
				continue;
			}
			bool isN = ushort.TryParse(c[1], out var n);
			if (isN) {
				switch (c[0]) {
					case "voidAmbientMax": voidAmbientMax = n; break;
					case "voidAmbientMul": voidAmbientMul = n; break;
					case "voidNoiseMax": voidNoiseMax = n; break;
					case "voidScaleMax": voidScaleMax = n; break;
					case "detailMax": detailMax = n; break;
					case "saturateMax": saturateMax = n; break;
					case "brightnessMax": brightnessMax = n; break;
					case "binMax": binMax = n; break;
					case "L2Max": l2Max = n; break;
					case "blurMax": blurMax = n; break;
				}
			}
			bool isF = float.TryParse(c[1], out var f);
			if (isF) {
				switch (c[0]) {
					case "detailMul": detailMul = f; break;
					case "bloomMul": bloomMul = f; break;
					case "voidNoiseMul": voidNoiseMul = f; break;
					case "threadsMul": threadsMul = f; break;
				}
			}
		}
	}
	private void SaveSettings() {
		var f = generator.GetFractal();
		var file = "";
		foreach (var c in generator.Colors) {
			var p = "palette|" + c.Item1 + ";";
			foreach (var i in c.Item2)
				p += i.X + ":" + i.Y + ":" + i.Z + ";";
			file += p[..^1] + "|";
		}
		file += "fractal|" + fractalSelect.Text + "|path|" + f.Path
			+ "|preview|" + (previewMode ? 1 : 0) + "|edit|" + (editorPanel.Visible ? 1 : 0)
			+ "|angle|" + generator.SelectedChildAngle + "|color|" + generator.SelectedChildColor
			+ "|angles|" + generator.SelectedChildAngles + "|colors|" + generator.SelectedChildColors
			+ "|cut|" + cutSelect.SelectedIndex + "|seed|" + cutparamBox.Text
			+ "|w|" + resX.Text + "|h|" + resY.Text + "|res|" + resSelect.SelectedIndex
			+ "|paletteSelect|" + paletteSelect.SelectedIndex + "|dithering|" + (ditherBox.Enabled ? 1 : 0)
			+ "|period|" + periodBox.Text + "|periodMul|" + periodMultiplierBox.Text
			+ "|zoom|" + zoomSelect.SelectedIndex + "|defaultZoom|" + defaultZoomBox.Text
			+ "|hue|" + hueSelect.SelectedIndex + "|defaultHue|" + defaultHue.Text + "|hueMul|" + hueSpeedBox.Text
			+ "|spin|" + spinSelect.SelectedIndex + "|defaultAngle|" + defaultAngleBox.Text + "|spinMul|" + spinSpeedBox.Text
			+ "|amb|" + ambBox.Text + "|noise|" + noiseBox.Text + "|void|" + voidBox.Text
			+ "|detail|" + detailBox.Text + "|saturate|" + saturateBox.Text + "|brightness|" + brightnessBox.Text
			+ "|bloom|" + bloomBox.Text + "|blur|" + blurBox.Text
			+ "|child|" + zoomChildBox.Text + "|abort|" + abortBox.Text
			+ "|delay|" + generator.SelectedDelay + "|fps|" + generator.SelectedFps + "|timing|" + timingSelect.SelectedIndex
			+ "|parallel|" + parallelTypeSelect.SelectedIndex + "|threads|" + threadsBox.Text
			+ "|cache|" + (cacheBox.Checked ? 1 : 0) + "|cachestripe|" + stripeBox.Text + "|cachebin|" + binBox.Text + "|cachesize|" + l2Box.Text
			+ "|gpuvoid|" + voidSelect.SelectedIndex + "|gpudraw|" + drawSelect.SelectedIndex
			+ "|png|" + encodePngSelect.SelectedIndex + "|gif|" + encodeGifSelect.SelectedIndex + "|gen|" + generationSelect.SelectedIndex
			+ "|ani|" + (animated ? 1 : 0) + "|exp|" + exportSelect.SelectedIndex + "|file|" + fileMask;

		File.WriteAllText("settings.txt", file);
	}
	private void LoadSettings() {
		isGifReady = 0;
		//gifButton.Enabled = false;
		if (!File.Exists("settings.txt"))
			return;
		LoadParams(File.ReadAllText("settings.txt").Split('|'));
		if (generator.SelectedEditorMode = editorPanel.Visible) {

			memPng = (FractalGenerator.PngType)encodePngSelect.SelectedIndex;
			memGif = (FractalGenerator.GifType)encodeGifSelect.SelectedIndex;
			memGenerate = (FractalGenerator.GenerationType)generationSelect.SelectedIndex;
			memBlur = generator.SelectedBlur;
			memBloom = generator.SelectedBloom;
			//mem_hue = (short)hueSelect.SelectedIndex;

			generator.SelectedGenerationType = FractalGenerator.GenerationType.Animation;
			generator.SelectedPngType = FractalGenerator.PngType.No;
			generator.SelectedGifType = FractalGenerator.GifType.No;
			generator.SelectedBloom = generator.SelectedBlur = 0;
			//generator.selectHue = generator.selectDefaultHue = 0;
			generator.SelectedPreviewMode = previewMode;
			abortDelay = 10;

			//if (short.TryParse(defaultHue.Text, out var n))
			//	memDefaultHue = n;
			if (ushort.TryParse(abortBox.Text, out var n))
				memAbort = n;
		} else {
			generator.SelectedPreviewMode = false;
		}
		SetupFractal();
	}
	private void LoadParams(string[] s) {
		for (var i = 0; i < s.Length - 1; i += 2) {
			var v = s[i + 1];
			var p = int.TryParse(v, out var n);
			switch (s[i]) {
				case "palette":
					var si = s[i + 1].Split(';');
					if (si.Length < 2)
						break;
					var pn = si[0];
					var c = new Vector3[si.Length - 1];
					var ci = 1;
					while (ci < si.Length) {
						var sv = si[ci].Split(':');
						Vector3 v3 = new();
						if (!(sv.Length == 3 && float.TryParse(sv[0], out v3.X) && float.TryParse(sv[1], out v3.Y) && float.TryParse(sv[2], out v3.Z)))
							break;
						c[ci - 1] = v3;
						++ci;
					}

					// Did we not fail at any parsing?
					if (ci == si.Length) {
						// Reject it ijf it's already in the list:
						var same = false;
						foreach (var pal in generator.Colors) {
							if (pal.Item2.Length != c.Length)
								continue;
							int t;
							for (t = 0; t < c.Length; ++t)
								if (c[t].X != pal.Item2[t].X && c[t].Y != pal.Item2[t].Y && c[t].Z != pal.Item2[t].Z)
									break;
							if (t < c.Length)
								continue;
							same = true;
							break;
						}
						if (same)
							break;
						// Not already in the list - add it
						generator.Colors.Add((pn, c));
					}
					break;
				case "path": if (v != "" && File.Exists(v)) _ = LoadFractal(v); break;
				case "fractal": if (fractalSelect.Items.Contains(v)) fractalSelect.SelectedItem = v; break;
				case "preview": if (p) previewMode = n > 0; break;
				case "edit": if (p) generatorPanel.Visible = !(generator.SelectedEditorMode = editorPanel.Visible = n > 0); break;
				case "angle":
					if (p) generator.SelectedChildAngle = (short)Math.Min(angleSelect.Items.Count - 2, n); {
						var f = generator.GetFractal();
						angleSelect.Items[0] = "Selected: " + f.ChildAngle[generator.SelectedChildAngle].Item1;
					}
					break;
				case "color":
					if (p) generator.SelectedChildColor = (short)Math.Min(colorSelect.Items.Count - 2, n); {
						var f = generator.GetFractal();
						colorSelect.Items[0] = "Selected: " + f.ChildColor[generator.SelectedChildColor].Item1;
					}
					break;
				case "angles":
					if (p) {
						generator.SelectedChildAngles = (ulong)n;
						FillSelectAngles();
					}
					break;
				case "colors":
					if (p) {
						generator.SelectedChildColors = (ulong)n;
						FillSelectColors();
					}
					break;
				case "cut": if (p) cutSelect.SelectedIndex = Math.Min(cutSelect.Items.Count - 1, n); break;
				case "seed": cutparamBox.Text = v; break;
				case "w": if (p) resX.Text = v; break;
				case "h": if (p) resY.Text = v; break;
				case "res": if (p) resSelect.SelectedIndex = Math.Min(resSelect.Items.Count - 1, n); break;
				case "paletteSelect": FillPalette(); if (p) paletteSelect.SelectedIndex = Math.Min(paletteSelect.Items.Count - 1, n); break;
				case "defaultHue": defaultHue.Text = v; break;
				case "dithering": if (p) ditherBox.Checked = n == 1; break;
				case "period": periodBox.Text = v; break;
				case "periodMul": periodMultiplierBox.Text = v; break;
				case "zoom": if (p) zoomSelect.SelectedIndex = Math.Min(zoomSelect.Items.Count - 1, n); break;
				case "defaultZoom": defaultZoomBox.Text = v; break;
				case "hue": if (p) hueSelect.SelectedIndex = Math.Min(hueSelect.Items.Count - 1, n); break;
				case "hueMul": hueSpeedBox.Text = v; break;
				case "spin": if (p) spinSelect.SelectedIndex = Math.Min(spinSelect.Items.Count - 1, n); break;
				case "defaultAngle": defaultAngleBox.Text = v; break;
				case "spinMul": spinSpeedBox.Text = v; break;
				case "amb": ambBox.Text = v; break;
				case "noise": noiseBox.Text = v; break;
				case "void": voidBox.Text = v; break;
				case "detail": detailBox.Text = v; break;
				case "saturate": saturateBox.Text = v; break;
				case "brightness": brightnessBox.Text = v; break;
				case "bloom": bloomBox.Text = v; break;
				case "blur": blurBox.Text = v; break;
				case "child": zoomChildBox.Text = v; break;
				case "abort": abortBox.Text = v; break;
				case "delay": generator.SelectedDelay = (ushort)n; break;
				case "fps": generator.SelectedFps = (ushort)n; break;
				case "timing": timingSelect.SelectedIndex = Math.Min(parallelTypeSelect.Items.Count - 1, n); TimingSelect_SelectedIndexChanged(null, null); break;
				case "parallel": parallelTypeSelect.SelectedIndex = Math.Min(parallelTypeSelect.Items.Count - 1, n); break;
				case "threads": threadsBox.Text = v; break;
				case "cache": if (p) cacheBox.Checked = n == 1; break;
				case "cachestripe": if (p) stripeBox.Text = v; break;
				case "cachebin": if (p) binBox.Text = v; break;
				case "cachesize": if (p) l2Box.Text = v; break;
				case "gpuvoid": if (p) voidSelect.SelectedIndex = Math.Min(voidSelect.Items.Count - 1, n); break;
				case "gpudraw": if (p) drawSelect.SelectedIndex = Math.Min(drawSelect.Items.Count - 1, n); break;
				case "png": if (p) encodePngSelect.SelectedIndex = Math.Min(encodePngSelect.Items.Count - 1, n); break;
				case "gif": if (p) encodeGifSelect.SelectedIndex = Math.Min(encodeGifSelect.Items.Count - 1, n); break;
				case "gen": if (p) generationSelect.SelectedIndex = Math.Min(generationSelect.Items.Count - 1, n); break;
				case "ani": if (p) animated = n <= 0; AnimateButton_Click(null, null); break;
				case "exp": if (p) exportSelect.SelectedIndex = Math.Min(exportSelect.Items.Count - 1, n); break;
				case "file":
					if (p) fileMask = (ushort)n;
					if (p) {
						fileMask = (ushort)n;
						FillSelectFile();
					}
					break;
			}
		}
		BitmaskComboBox_MinimumSelection();
	}
	private void FillSelectColors() {
		var cc = generator.GetFractal().ChildColor;
		var ai = generator.SelectedChildColors &= ((ulong)1 << cc.Count) - 1;
		int selectI = 0;
		while (ai > 0) {
			colorSelect.Items[selectI + 1] = ((ai & 1) == 1 ? charOn : charOff) + cc[selectI].Item1;
			++selectI;
			ai >>= 1;
		}
	}
	private void FillSelectAngles() {
		var ca = generator.GetFractal().ChildAngle;
		var ai = generator.SelectedChildAngles &= ((ulong)1 << ca.Count) - 1;
		int selectI = 0;
		while (ai > 0) {
			angleSelect.Items[selectI + 1] = ((ai & 1) == 1 ? charOn : charOff) + ca[selectI].Item1;
			++selectI;
			ai >>= 1;
		}
	}
	private void FillSelectFile() {
		var ai = fileMask;
		int selectI = 0;
		while (ai > 0) {
			++selectI;
			fileSelect.Items[selectI] = ((ai & 1) == 1 ? charOn : charOff) + filePostfix[selectI];
			ai >>= 1;
		}
	}
	#endregion

	#region InputHelpers
	/// <summary>
	/// Fetch the resolution
	/// </summary>
	/// <returns>Changed</returns>
	private bool TryResize() {
		//previewMode = !previewBox.Checked;
		width = 8;
		height = 8;
		if (!short.TryParse(resX.Text, out width) || width <= 8)
			width = 8;
		if (!short.TryParse(resY.Text, out height) || height <= 8)
			height = 8;
		var c = "Custom:" + width + "x" + height;
		if (resSelect.Items[1].ToString() != c) {
			resSelect.Items[1] = c;
		}
		//previewBox.Text = "Resolution: " + width.ToString() + "x" + height.ToString();
		//if (previewMode)
		//	width = height = 80;
		var rxy = resSelect.SelectedIndex is 1 or < 0 ? resSelect.Items[1].ToString().Split(':')[1].Split('x') : resSelect.Items[resSelect.SelectedIndex].ToString().Split('x');
		if (!short.TryParse(rxy[0], out width))
			width = 80;
		if (!short.TryParse(rxy[1], out height))
			height = 80;
		if (generator.SelectedWidth != width || generator.SelectedHeight != height) {
			// Number of threads can change the maximum of these:
			BloomBox_TextChanged(null, null);
			StripeBox_TextChanged(null, null);
			return true;
		}
		return false;
	}
	private void GeneratorForm_FormClosing(object sender, FormClosingEventArgs e) {
		Close(e);
	}
	private void Close(FormClosingEventArgs e) {
		if (xTask != null) {
			var result = MessageBox.Show(
				"Your export is still saving!\nAre you sure you want to close the application and potentially lose it?",
				"Confirm Exit",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			// Cancel closing if the user clicks "No"
			if (result == DialogResult.No)
				e.Cancel = true;
			return;
		}
		if (isGifReady > 80) {
			var result = MessageBox.Show(
				"You have encoded gif available to save.\nDo you want to save it?",
				"Confirm Save",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			// Save if the user clicks "Yes"
			if (result == DialogResult.Yes) {
				saveGif.DefaultExt = "gif";
				saveGif.Filter = "GIF video (*.gif)|*.gif|MP4 video (*.mp4)|*.mp4";
				_ = saveGif.ShowDialog();
				e.Cancel = true;
				return;
			}
		}
		if (!isPngsSaved && generator.SelectedPngType == FractalGenerator.PngType.Yes && generator.GetBitmapsFinished() >= generator.GetFrames() && MessageBox.Show(
			"You have PNGs available to save or convert to Mp4.\nDo you want to save it?",
			"Confirm Save",
			MessageBoxButtons.YesNo,
			// Save if the user clicks "Yes"
			MessageBoxIcon.Question) == DialogResult.Yes) {
			saveMp4.DefaultExt = "mp4";
			saveMp4.Filter = "MP4 video (*.mp4)|*.mp4|PNG video (*.png)|*.png";
			_ = saveMp4.ShowDialog();
			e.Cancel = true;
			return;
		}
		var saved = false;
		foreach (var f in generator.GetFractals()) {
			if (!f.Edit)
				continue;
			var result = MessageBox.Show(
				"Fractal " + f.Name + " has been edited. Do you want to save it before closing?",
				"Confirm Exit",
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Question);
			// Cancel closing if the user clicks "No"
			switch (result) {
				case DialogResult.Yes:
					toSave = f;
					saved = true;
					_ = saveFractal.ShowDialog();
					continue;
				case DialogResult.No:
					f.Edit = false;
					continue;
				case DialogResult.Cancel:
					e.Cancel = true;
					return;
			}
			return;
		}

		if (saved) {
			e.Cancel = true;
			return;
		}

		aCancel?.Cancel();
		xCancel?.Cancel();
		aTask?.Wait();
		xTask?.Wait();
		Abort();
		SaveSettings();
		generator.CleanupTempFiles();
	}
	private void Abort() {
		queueAbort = false;
		// Cancel FractalGenerator threads
		generator.RequestCancel();
		isGifReady = 0;
		currentBitmapIndex = 0;
		aTask = null;
	}
	private bool QueueReset(bool allow = true) {
		if (modifySettings || !allow)
			return false;
		if (queueReset <= 0) {



			if (isGifReady > 80 && xTask == null) {
				var result = MessageBox.Show(
					"You have encoded gif available to save.\nDo you want to save it?\nCancel will turn off gif encoding so you won't keep getting this warning again.",
					"Save GIF",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question);
				switch (result) {
					case DialogResult.Yes:
						saveGif.DefaultExt = "gif";
						saveGif.Filter = "GIF video (*.gif)|*.gif|MP4 video (*.mp4)|*.mp4";
						_ = saveGif.ShowDialog();
						break;
					case DialogResult.Cancel:
						generationSelect.SelectedIndex = 1;
						break;
				}
			}
			if (!isPngsSaved && generator.SelectedPngType == FractalGenerator.PngType.Yes && generator.GetBitmapsFinished() >= generator.GetFrames() && xTask == null) {
				var result = MessageBox.Show(
					"You have PNGs available to save or convert to Mp4.\nDo you want to save it?\nCancel will turn off mp4 encoding so you won't keep getting this warning again.",
					"Save MP4/PNGs",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question);
				switch (result) {
					case DialogResult.Yes:
						saveMp4.DefaultExt = "mp4";
						saveMp4.Filter = "MP4 video (*.mp4)|*.mp4|PNG video (*.png)|*.png";
						_ = saveMp4.ShowDialog();
						break;
					case DialogResult.Cancel:
						encodePngSelect.SelectedIndex = 0;
						break;
				}
			}
			//gifButton.Enabled = false;
			isGifReady = 0;
			currentBitmapIndex = 0;

			if (TasksNotRunning())
				aTask = Task.Run(Abort, (aCancel = new()).Token);
			else queueAbort = true;
		}
		ResetRestart();
		restartButton.Enabled = false;
		queueReset = (short)abortDelay;
		return true;
	}
	private void ResetRestart() {
		queueReset = restartTimer = 0;
		restartButton.Text = "! RESTART !";
	}
	private bool TasksNotRunning() => aTask == null && xTask == null;
	private void NextBatch() {
		if (batchIndex >= runningBatch.Length) {
			// All batches complete, unlock the controls, and stop the batching
			for (var i = myControls.Count; 0 <= --i; myControls[i].Enabled = myControlsEnabled[i]) {
			}
			return;
		}
		isGifReady = 0;
		LoadParams(runningBatch[batchIndex].Split('|'));
		generator.SelectedPreviewMode = false;
		generatorPanel.Visible = !(generator.SelectedEditorMode = editorPanel.Visible = false);
		QueueReset();
		++batchIndex;
	}
	private void SetupSelects() {

		if (modifySettings) {
			FillSelects();
			FillCutSeeds();
			return;
		}
		modifySettings = true;
		FillSelects();
		FillCutSeeds();
		modifySettings = false;
		QueueReset();

		return;

		void FillSelects() {
			var f = generator.GetFractal();
			// Fill angle children definitions combobox
			angleSelect.Items.Clear();
			angleSelect.Items.Add("Selected: " + f.ChildAngle[generator.SelectedChildAngle].Item1);
			foreach (var (name, _) in f.ChildAngle)
				angleSelect.Items.Add(charOff + name);
			angleSelect.SelectedIndex = 0;
			// Fill color children definitions combobox
			colorSelect.Items.Clear();
			colorSelect.Items.Add("Selected: " + f.ChildColor[generator.SelectedChildColor].Item1);
			foreach (var (name, _) in f.ChildColor)
				colorSelect.Items.Add(charOff + name);
			colorSelect.SelectedIndex = 0;
			// Fill cutFunction definitions combobox
			cutSelect.Items.Clear();
			var cf = f.ChildCutFunction;
			if (!CutSelectEnabled(cf))
				return;
			foreach (var (index, _) in cf)
				cutSelect.Items.Add(Fractal.CutFunctions[index].Item1);
			cutSelect.SelectedIndex = 0;
		}
	}
	private void SetupFractal() {
		generator.SetupFractal();
		if (!modifySettings) {
			modifySettings = true;
			Parallel_Changed(null, null);
			DetailBox_TextChanged(null, null);
			modifySettings = false;
		} else {
			Parallel_Changed(null, null);
			DetailBox_TextChanged(null, null);
		}
		generator.SetupCutFunction();
	}
	private void GetValidZoomChildren() {
		generator.GetValidZoomChildren();
		zoomChildBox.Text = "0";
	}
	private void SetupParallel(short newThreads) {
		generator.SelectedMaxTasks = (short)(newThreads > 1 ? newThreads : FractalGenerator.MinTasks);
		generator.SelectThreadingDepth();
	}
	private bool CutSelectEnabled(List<(int, int[])> cf)
	=> cutSelect.Enabled = cf is { Count: > 0 };
	// Query the number of seeds from the CutFunction
	private bool CutSeedBoxEnabled(Fractal.CutFunction cf)
		=> cutparamBox.Enabled = 0 < (generator.CutSeed_Max = (int)(cf == null || cf(0, -1, generator.GetFractal()) <= 0 ? 0 : (cf(0, 1 - (1 << 30), generator.GetFractal()) + 1) / cf(0, -1, generator.GetFractal())));
	/// <summary>
	/// Fill the cutFunction seed parameter comboBox with available options for the selected CutFunction
	/// </summary>
	private void FillCutSeeds() {
		_ = CutSeedBoxEnabled(generator.GetCutFunction());
		cutparamBox.Text = "0";
		GetValidZoomChildren();
	}
	private int FillPalette() {
		var r = 0;
		paletteSelect.Items.Clear();
		paletteSelect.Items.Add("Random");
		for (var i = 0; i < generator.Colors.Count; ++i)
			r = paletteSelect.Items.Add(generator.Colors[i].Item1);
		return r;
	}
	private void MoveFrame(int move) {
		animated = false;
		var b = generator.GetBitmapsFinished();
		currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + b + move) % b;
	}
	private void SetAnimate() {
		animateButton.Text = animated ? "Playing" : "Paused";
		animateButton.BackColor = animated ? Color.FromArgb(128, 255, 128) : Color.FromArgb(255, 128, 128);
	}
	private static bool Clean(TextBox box) {
		var s = box.Text;
		s = s.Replace(';', ' ').Replace('|', ' ').Replace(':', ' ');
		if (s == box.Text)
			return false;
		box.Text = s;
		return true;
	}

	private static T ParseValue<T>(TextBox box) where T : struct, IParsable<T> {
		_ = Clean(box);
		return T.TryParse(box.Text, null, out var value) ? value : default;
	}
	/*private static byte ParseByte(TextBox box) { _ = Clean(box); return byte.TryParse(box.Text, out var v) ? v : (byte)0; }
	private static ushort ParseUShort(TextBox box) { _ = Clean(box); return ushort.TryParse(box.Text, out var v) ? v : (ushort)0; }
	private static short ParseShort(TextBox box) { _ = Clean(box); return short.TryParse(box.Text, out var v) ? v : (short)0; }
	private static int ParseInt(TextBox box) { _ = Clean(box); return int.TryParse(box.Text, out var v) ? v : 0; }
	private static double ParseDouble(TextBox box) { _ = Clean(box); return double.TryParse(box.Text, out var v) ? v : 0.0; }
	private static float ParseFloat(TextBox box) { _ = Clean(box); return float.TryParse(box.Text, out var v) ? v : 0.0f; }*/
	private static T Clamp<T>(T n, T min, T max) where T : struct, IComparable<T>
		=> n.CompareTo(min) < 0 ? min : n.CompareTo(max) > 0 ? max : n;

	private static T ReText<T>(TextBox box, T n) where T : struct, IComparable<T>, IParsable<T> {
		if (n.CompareTo(default) == 0) {
			if (!T.TryParse(box.Text, null, out _))
				box.Text = box.Focused ? "" : "0";
		} else {
			box.Text = n.ToString();
		}

		return n;
	}
	private static T Mod<T>(T n, T min, T max) where T : struct, IComparable<T> {
		var d = (dynamic)max - min; while (n.CompareTo(min) < 0) n = (T)(n + d); while (n.CompareTo(max) > 0) n = (T)(n - d); return n;
	}
	private static bool Diff<T>(T n, T gen) where T : struct, IComparable<T>
		=> gen.CompareTo(n) == 0;
	private bool Apply<T>(T n, out T gen) {
		gen = n;
		QueueReset();
		return false;
	}
	private static T ParseClampReText<T>(TextBox box, T min, T max) where T : struct, IComparable<T>, IParsable<T>
		=> ReText(box, Clamp(ParseValue<T>(box), min, max));
	private bool DiffApply<T>(T n, ref T gen) where T : struct, IComparable<T>
		=> Diff(n, gen) || Apply(n, out gen);
	private bool MaskApply(short n, ref short select, ref ulong mask) {
		if (n < 0)
			return true;
		select = n;
		mask ^= (ulong)1 << n;
		QueueReset();
		return false;
	}
	private bool ClampDiffApply<T>(T n, ref T gen, T min, T max) where T : struct, IComparable<T>
		=> DiffApply(Clamp(n, min, max), ref gen);
	private bool ParseDiffApply<T>(TextBox box, ref T gen) where T : struct, IComparable<T>, IParsable<T>
		=> DiffApply(ParseValue<T>(box), ref gen);
	private bool ParseModDiffApply<T>(TextBox box, ref T gen, T min, T max) where T : struct, IComparable<T>, IParsable<T>
		=> DiffApply(Mod(ParseValue<T>(box), min, max), ref gen);
	private bool ParseClampReTextDiffApply<T>(TextBox box, ref T gen, T min, T max) where T : struct, IComparable<T>, IParsable<T>
		=> DiffApply(ParseClampReText(box, min, max), ref gen);
	private bool ParseClampReTextMulDiffApply<T>(TextBox box, ref T gen, T min, T max, T mul) where T : struct, INumber<T>
		=> DiffApply(ParseClampReText(box, min, max) * mul, ref gen);
	#endregion

	#region Input_Select
	private static void ComboBox_MouseWheel(object sender, MouseEventArgs e) {
		((HandledMouseEventArgs)e).Handled = true;
	}
	private void FractalSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (generator.SelectFractal((short)Math.Max(0, fractalSelect.SelectedIndex)))
			return;
		// Fractal is different - load it, change the setting and restart generation
		FillEditor();
		SetupSelects();
		BitmaskComboBox_MinimumSelection();
	}
	private void FractalSelect_TextUpdate(object sender, EventArgs e) {
		if (fractalSelect.Items.Contains(fractalSelect.Text))
			fractalSelect.SelectedIndex = fractalSelect.Items.IndexOf(fractalSelect.Text);
		else if (File.Exists(fractalSelect.Text + ".fractal"))
			_ = LoadFractal(fractalSelect.Text + ".fractal");
	}
	private bool BitmaskComboBox_MinimumSelection() {
		bool b = false;
		if (generator.SelectedChildColors == 0) {
			colorSelect.SelectedIndex = 1;
			b = true;
		}
		if (generator.SelectedChildAngles == 0) {
			angleSelect.SelectedIndex = 1;
			b = true;
		}
		return b;
	}
	private static void BitmaskComboBox_DrawItem(object sender, DrawItemEventArgs e) {
		if (e.Index < 0 || sender is not ComboBox combo)
			return;
		e.DrawBackground();
		string text = combo.Items[e.Index].ToString();
		var style = text.StartsWith("[⬛]") ? FontStyle.Bold : FontStyle.Regular;
		using (var font = new Font(combo.Font, style)) {
			TextRenderer.DrawText(
				e.Graphics,
				text,
				font,
				e.Bounds,
				e.Index == 0 ? SystemColors.GrayText : SystemColors.ControlText,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter
			);
		}
		e.DrawFocusRectangle();
	}
	private void AngleSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (MaskApply((short)Math.Max(-1, angleSelect.SelectedIndex - 1), ref generator.SelectedChildAngle, ref generator.SelectedChildAngles))
			return;
		// put selection back to 0, rename the selected, and change the checkmark:
		angleSelect.SelectedIndex = 0;
		var n = generator.GetFractal().ChildAngle[generator.SelectedChildAngle].Item1;
		angleSelect.Items[0] = "Selected: " + n;
		angleSelect.Items[generator.SelectedChildAngle + 1] = (((generator.SelectedChildAngles >> generator.SelectedChildAngle) & 1) == 1 ? charOn : charOff) + n;
		if (BitmaskComboBox_MinimumSelection())
			return;
		SwitchChildAngle();
	}
	private void ColorSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (MaskApply((short)Math.Max(-1, colorSelect.SelectedIndex - 1), ref generator.SelectedChildColor, ref generator.SelectedChildColors))
			return;
		// put selection back to 0, rename the selected, and change the checkmark:
		colorSelect.SelectedIndex = 0;
		var n = generator.GetFractal().ChildColor[generator.SelectedChildColor].Item1;
		colorSelect.Items[0] = "Selected: " + n;
		colorSelect.Items[generator.SelectedChildColor + 1] = (((generator.SelectedChildColors >> generator.SelectedChildColor) & 1) == 1 ? charOn : charOff) + n;
		if (BitmaskComboBox_MinimumSelection())
			return;
		SwitchChildColor();
	}
	private void CutSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (!DiffApply((short)Math.Max(0, cutSelect.SelectedIndex), ref generator.SelectedCut)) FillCutSeeds();
	}
	private void CutSeedBox_TextChanged(object sender, EventArgs e) {
		if (ParseClampReTextDiffApply(cutparamBox, ref generator.SelectedCutSeed, -1, generator.GetMaxCutSeed()))
			return;
		GetValidZoomChildren();
	}
	private void Resolution_Changed(object sender, EventArgs e) => QueueReset(TryResize());
	private void PaletteSelect_DrawItem(object sender, DrawItemEventArgs e) {
		if (e.Index < 0)
			return;
		e.DrawBackground();
		e.DrawFocusRectangle();
		using var font = new Font(e.Font ?? new Font(new FontFamily(GenericFontFamilies.Monospace), 8), FontStyle.Regular);
		float x = e.Bounds.Left;
		float y = e.Bounds.Top;
		using Brush black = new SolidBrush(Color.Black);
		if (e.Index > 0) {
			var c = generator.Colors[e.Index - 1].Item2;
			for (var i = 0; i < c.Length; ++i) {
				if (c[i].X + c[i].Y + c[i].Z <= 0) {
					e.Graphics.DrawString("░", font, black, x, y);
					x += e.Graphics.MeasureString("░", font).Width; // Move position
				} else {
					using Brush b = new SolidBrush(Color.FromArgb(255, (int)c[i].X, (int)c[i].Y, (int)c[i].Z));
					e.Graphics.DrawString("█", font, b, x, y);
					x += e.Graphics.MeasureString("█", font).Width; // Move position
				}
			}
		}

		e.Graphics.DrawString(e.Index == 0 ? "Random" : generator.Colors[e.Index - 1].Item1, font, black, x, y);
	}
	private void PaletteSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (DiffApply((short)Math.Max(-1, paletteSelect.SelectedIndex - 1), ref generator.SelectedPaletteType))
			return;
		defaultHue.Text = "0";
	}
	private void RemovePalette_Click(object sender, EventArgs e) {
		if (generator.Colors.Count <= 1)
			return;
		var i = paletteSelect.SelectedIndex;
		generator.Colors.RemoveAt(generator.SelectedPaletteType);
		FillPalette();
		paletteSelect.SelectedIndex = i;
		QueueReset();
	}
	private void AddPalette_Click(object sender, EventArgs e) {
		List<Vector3> newPalette = [];
		var ok = DialogResult.OK;
		while (ok == DialogResult.OK) {
			ok = paletteDialog.ShowDialog();
			if (ok == DialogResult.OK) {
				newPalette.Add(new Float3(paletteDialog.Color.R, paletteDialog.Color.G, paletteDialog.Color.B));
			}
		}
		var p = new Vector3[newPalette.Count];
		for (var i = 0; i < p.Length; ++i)
			p[i] = newPalette[i];
		// TODO name the palette
		generator.Colors.Add(("Custom palette", p));
		paletteSelect.SelectedIndex = FillPalette();
	}
	private void DefaultHue_TextChanged(object sender, EventArgs e)
		=> DiffApply(ParseValue<float>(defaultHue), ref generator.SelectedDefaultHue);
	#endregion

	#region Input_Settings
	private void DitherBox_CheckedChanged(object sender, EventArgs e) {
		ditherBox.Text = ditherBox.Checked ? "Enabled" : "Disabled";
		generator.SelectDithering(ditherBox.Checked);
		QueueReset();
	}
	private void PeriodBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextDiffApply(periodBox, ref generator.SelectedPeriod, (ushort)1, (ushort)1000);
	private void PeriodMultiplierBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextDiffApply(periodMultiplierBox, ref generator.SelectedPeriodMultiplier, (ushort)1, (ushort)10);
	private void ZoomSelect_SelectedIndexChanged(object sender, EventArgs e)
		=> DiffApply((short)(zoomSelect.SelectedIndex - 2), ref generator.SelectedZoom);
	private void DefaultZoom_TextChanged(object sender, EventArgs e)
		=> ParseDiffApply(defaultZoomBox, ref generator.SelectedDefaultZoom);
	private void HueSelect_SelectedIndexChanged(object sender, EventArgs e)
		=> DiffApply((short)(hueSelect.SelectedIndex == 0 ? -2 : hueSelect.SelectedIndex % 3 - 1), ref generator.SelectedHue);
	private void HueSpeedBox_TextChanged(object sender, EventArgs e) {
		var newSpeed = ParseClampReText(hueSpeedBox, (ushort)0, (ushort)255);
		if (Diff(newSpeed, generator.SelectedExtraHue))
			return;
		// hue speed is different - change the setting and if it's actually hueCycling restart generation
		if (generator.SelectedHue != 0)
			_ = Apply(newSpeed, out generator.SelectedExtraHue);
		else generator.SelectedExtraHue = newSpeed;
	}
	private void SpinSelect_SelectedIndexChanged(object sender, EventArgs e)
		=> ClampDiffApply((short)(spinSelect.SelectedIndex - 2), ref generator.SelectedSpin, (short)-2, (short)3);
	private void DefaultAngle_TextChanged(object sender, EventArgs e)
		=> ParseModDiffApply(defaultAngleBox, ref generator.SelectedDefaultAngle, (ushort)0, (ushort)360);
	private void SpinSpeedBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextDiffApply(spinSpeedBox, ref generator.SelectedExtraSpin, (ushort)0, (ushort)255);
	private void AmbBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextMulDiffApply(ambBox, ref generator.SelectedAmbient, (short)-1, (short)voidAmbientMax, (short)voidAmbientMul);
	private void NoiseBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextMulDiffApply(noiseBox, ref generator.SelectedNoise, 0, voidNoiseMax, voidNoiseMul);
	private void VoidBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextDiffApply(voidBox, ref generator.SelectedVoid, (ushort)0, voidScaleMax);
	private void DetailBox_TextChanged(object sender, EventArgs e) {
		if (!ParseClampReTextMulDiffApply(detailBox, ref generator.SelectedDetail, 0.0, detailMax, detailMul * generator.GetFractal().MinSize))
			generator.SetMaxIterations();
	}
	private void SaturateBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextMulDiffApply(saturateBox, ref generator.SelectedSaturate, 0, saturateMax, 1.0f / saturateMax);
	private void BrightnessBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextDiffApply(brightnessBox, ref generator.SelectedBrightness, (ushort)0, brightnessMax);
	private void BloomBox_TextChanged(object sender, EventArgs e) {
		AdjustBloomMax();
		if (ParseClampReTextMulDiffApply(bloomBox, ref generator.SelectedBloom, 0, bloomMax, bloomMul))
			return;
		// Number of threads can change the maximum of these:
		StripeBox_TextChanged(null, null);
	}
	private void AdjustBloomMax() {
		if (width + height <= 0)
			return;
		byte OnePixelBytes = 12;
		var L2_eff = .85f * (generator.SelectedL2Kilobytes == 0 ? FractalGenerator.GetEstimatedL2CachePerCore() : generator.SelectedL2Kilobytes);
		var maxCache = (L2_eff / (OnePixelBytes * width) - 2) / 2;

		if (maxTasks <= 0) {
			bloomMax = (ushort)maxCache;
			bloomLabel.Text = "Bloom (0-" + bloomMax + "):";
			return;
		}
		bloomMax = (ushort)(Math.Min(
			Math.Max(1,height / (2 * maxTasks) - 1), // Should be enough run all the threads of each in parallel
			maxCache // the cache size should fit a strip wide as Width, and Tall as BloomDiameter+StripHeight
		) / bloomMul);
		bloomLabel.Text = "Bloom (0-" + bloomMax + "):";
	}
	private void BlurBox_TextChanged(object sender, EventArgs e)
		=> ParseClampReTextDiffApply(blurBox, ref generator.SelectedBlur, (ushort)0, blurMax);
	private void ZoomChildBox_TextChanged(object sender, EventArgs e) {
		var n = ParseClampReText(zoomChildBox, (short)0, (short)Math.Max(0, Math.Min(generator.MaxZoomChild, generator.GetFractal().ChildCount - 1)));
		if (generator.SelectZoomChild(n))
			return;
		QueueReset();
	}
	private void AbortBox_TextChanged(object sender, EventArgs e)
	=> abortDelay = ParseClampReText(abortBox, (ushort)0, (ushort)10000);
	private void EncodePngSelect_SelectedIndexChanged(object sender, EventArgs e) {
		generator.SelectedPngType = (FractalGenerator.PngType)Math.Max(0, encodePngSelect.SelectedIndex);
	}
	private void TimingSelect_SelectedIndexChanged(object sender, EventArgs e) {
		toolTips.SetToolTip(timingBox, timingSelect.SelectedIndex switch {
			0 => "A delay between frames in 1/100 of seconds for the preview and exported GIF file, so if you want to export a GIF, make sure to set this instead of framerate to avoid rounding errors.\nThe framerate will be roughly 100/delay",
			1 => "The framerate per second (roughly 100/delay). Used for exporting Mp4, so if you want to export MP4, make sure to set this instead of delay to avoid rounding errors",
			_ => ""
		});
		timingBox.Text = (timingSelect.SelectedIndex == 0 ? generator.SelectedDelay : generator.SelectedFps).ToString();
	}
	private void TimingBox_TextChanged(object sender, EventArgs e) {
		// TODO try to remove the GIF restart (was bugged the last time I tried)
		switch (timingSelect.SelectedIndex) {
			case 0:
				var newDelay = ParseClampReText(timingBox, (ushort)1, (ushort)500);
				timer.Interval = generator.SelectedDelay * 10;
				if (generator.SelectedDelay == newDelay)
					return;
				// Delay is different, change it, and restart the generation if ou were encoding a gif
				generator.SelectedDelay = newDelay;
				generator.SelectedFps = (ushort)(100 / generator.SelectedDelay);

				if (generator.SelectedGifType != FractalGenerator.GifType.No)
					QueueReset();


				break;
			case 1:
				var newFps = ParseClampReText(timingBox, (ushort)1, (ushort)120);
				timer.Interval = 1000 / generator.SelectedFps;
				if (generator.SelectedFps == newFps)
					return;
				generator.SelectedFps = newFps;
				generator.SelectedDelay = (ushort)(100 / newFps);
				if (generator.SelectedGifType != FractalGenerator.GifType.No)
					QueueReset();
				break;
		}
	}
	private void ParallelTypeSelect_SelectedIndexChanged(object sender, EventArgs e) {
		/*if ((FractalGenerator.ParallelType)parallelTypeSelect.SelectedIndex == FractalGenerator.ParallelType.OfDepth) {
			_ = MessageBox.Show(
				"Warning: this parallelism mode might be fast at rendering a single image, but it messes up few pixels.\nSo if you want highest quality the OfAnimation is recommended.",
				"Warning",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}*/
		generator.SelectedParallelType = (FractalGenerator.ParallelType)parallelTypeSelect.SelectedIndex;
	}
	private void Parallel_Changed(object sender, EventArgs e) {
		SetupParallel(ParseClampReText(threadsBox, (short)FractalGenerator.MinTasks, (short)Math.Max(1, threadsMul * maxTasks)));
		// Number of threads can change the maximum of these:
		BloomBox_TextChanged(null, null);
		StripeBox_TextChanged(null, null);
	}
	private void CacheBox_CheckedChanged(object sender, EventArgs e) {
		cacheBox.Text = cacheBox.Checked ? "Enabled" : "Disabled";
		generator.SelectedCacheOpt = cacheBox.Checked;
		QueueReset();
	}
	private void StripeBox_TextChanged(object sender, EventArgs e) {
		AdjustStripeMax();
		ParseClampReTextDiffApply(stripeBox, ref generator.SelectedStripeHeight, (ushort)0, stripeMax);
		BloomBox_TextChanged(null, null);
	}
	private void AdjustStripeMax() {
		byte OnePixelBytes = 12;
		var L2_eff = (generator.SelectedL2Kilobytes == 0 ? FractalGenerator.GetEstimatedL2CachePerCore() : generator.SelectedL2Kilobytes) *.85f;
		var minCachableStripe = (ushort)Math.Max(1, L2_eff / (OnePixelBytes * width) - 2 * generator.SelectedBloom - 2);

		if (maxTasks <= 0) {
			stripeMax = minCachableStripe;
			return;
		}
		stripeMax = Math.Min(
			(ushort)(0.75f * height / maxTasks), // Should be enough run all the threads of each in parallel
			minCachableStripe // the cache size should fit a strip wide as Width, and Tall as BloomDiameter+StripHeight
		);
	}
	private void BinBox_TextChanged(object sender, EventArgs e) {
		ParseClampReTextDiffApply(binBox, ref generator.SelectedBinSize, (ushort)0, binMax);
	}
	private void L2Box_TextChanged(object sender, EventArgs e) {
		ParseClampReTextDiffApply(l2Box, ref generator.SelectedL2Kilobytes, (ushort)0, l2Max);
	}
	private void VoidSelect_SelectedIndexChanged(object sender, EventArgs e) {
		var now = (FractalGenerator.GpuVoidType)Math.Max(0, voidSelect.SelectedIndex);
		if (generator.SelectedGpuVoidType == now)
			return;
		generator.SelectedGpuVoidType = now;
		//QueueReset(); // can be updated mid-generation
	}
	private void DrawSelect_SelectedIndexChanged(object sender, EventArgs e) {
		var now = (FractalGenerator.GpuDrawType)Math.Max(0, drawSelect.SelectedIndex);
		if (generator.SelectedGpuDrawType == now)
			return;
		generator.SelectedGpuDrawType = now;
		//QueueReset(); // can be updated mid-generation
	}
	private void EncodeGifSelect_SelectedIndexChanged(object sender, EventArgs e) {
		var prev = generator.SelectedGifType;
		var now = (FractalGenerator.GifType)Math.Max(0, encodeGifSelect.SelectedIndex);
		if (generator.SelectedGifType == now)
			return;
		generator.SelectedGifType = now;
		// TODO remove reset, but the last time I tried it wasw bugged
		if (now != FractalGenerator.GifType.No && now != prev)
			QueueReset();
	}
	private void GenerationSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if ((FractalGenerator.GenerationType)generationSelect.SelectedIndex == FractalGenerator.GenerationType.HashSeeds) {
			_ = MessageBox.Show(
				"This mode is not really meant for the end user, it only generates all parameters and export a hash.txt file will all the unique ones.\nIf you actually want an animation of all seeds, AllSeeds is recommended instead as that doesn't waste resources doing the hashing and encodes the animation for export.",
				"Warning",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}
		generator.SelectedGenerationType = (FractalGenerator.GenerationType)Math.Max(0, generationSelect.SelectedIndex);
		QueueReset();
	}
	private void LoadBatchButton_Click(object sender, EventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		_ = loadBatch.ShowDialog();
	}
	private void LoadBatch_FileOk(object sender, CancelEventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		loadedBatch = File.ReadAllText(loadBatch.FileName);
	}
	private void AddBatchButton_Click(object sender, EventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		var f = generator.GetFractal();
		if (loadedBatch != "")
			loadedBatch += ";";
		loadedBatch += "fractal|" + fractalSelect.Text + "|path|" + f.Path
			+ "|angle|" + angleSelect.SelectedIndex + "|color|" + colorSelect.SelectedIndex + "|cut|" + cutSelect.SelectedIndex + "|seed|" + cutparamBox.Text
			+ "|paletteSelect|" + paletteSelect.SelectedIndex
			+ "|period|" + periodBox.Text + "|periodMul|" + periodMultiplierBox.Text
			+ "|zoom|" + zoomSelect.SelectedIndex + "|defaultZoom|" + defaultZoomBox.Text
			+ "|hue|" + hueSelect.SelectedIndex + "|defaultHue|" + defaultHue.Text + "|hueMul|" + hueSpeedBox.Text
			+ "|spin|" + spinSelect.SelectedIndex + "|defaultAngle|" + defaultAngleBox.Text + "|spinMul|" + spinSpeedBox.Text
			+ "|amb|" + ambBox.Text + "|noise|" + noiseBox.Text + "|void|" + voidBox.Text
			+ "|detail|" + detailBox.Text + "|saturate|" + saturateBox.Text + "|brightness|" + brightnessBox.Text
			+ "|bloom|" + bloomBox.Text + "|blur|" + blurBox.Text
			+ "|child|" + zoomChildBox.Text;
	}
	private void SaveBatchButton_Click(object sender, EventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		_ = saveBatch.ShowDialog();
	}
	private void SaveBatch_FileOk(object sender, CancelEventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		File.WriteAllText(saveBatch.FileName, loadedBatch);
	}
	private void BatchBox_TextChanged(object sender, EventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		var batches = loadedBatch.Split(';');
		if (batches.Length <= 0)
			return;
		ParseClampReTextDiffApply(batchBox, ref batchIndex, 0, batches.Length - 1);
		LoadParams(batches[batchIndex].Split('|'));
		QueueReset();
	}
	private void UpdateBatchButton_Click(object sender, EventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		// TODO update batch entry
	}
	private void RunBatchButton_Click(object sender, EventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		switch (exportSelect.SelectedIndex) {
			case 0: // PNG
				runBatch.DefaultExt = "png";
				runBatch.Filter = "PNG file (*.png)|*.png";
				break;

			case 1: // PNGs
				runBatch.DefaultExt = "png";
				runBatch.Filter = "PNG video (*.png)|*.png";
				break;

			case 2: // MP4
				if (NoFfmpeg()) {
					_ = Error("Not Available", "Ffmpeg.exe not found, make sure you have Ffmpeg.exe in the app's root folder.");
					return;
				}
				runBatch.DefaultExt = "mp4";
				runBatch.Filter = "MP4 video (*.mp4)|*.mp4";
				break;

			case 3: // GIF
				if (generator.SelectedGifType == FractalGenerator.GifType.No) {
					_ = Error("Wrong Generation Type", "You must select LocalGif or GlobalGif or AllSeedsGif generation type for this.");
					return;
				}
				runBatch.DefaultExt = "gif";
				runBatch.Filter = "GIF video (*.gif)|*.gif";
				break;
			case 4: // GIF->MP4
				if (generator.SelectedGifType == FractalGenerator.GifType.No) {
					_ = Error("Wrong Generation Type", "You must select LocalGif or GlobalGif or AllSeedsGif generation type for this.");
					return;
				}
				runBatch.DefaultExt = "mp4";
				runBatch.Filter = "MP4 video (*.mp4)|*.mp4";
				break;

		}
		_ = runBatch.ShowDialog();
	}
	private void RunBatch_FileOk(object sender, CancelEventArgs e) {
		// TODO implement/test
		_ = MessageBox.Show(
				"Batching is coming soon. Still a bit of dev and testing needed.",
				"Coming Soon",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		return;
		batchFolder = runBatch.FileName[..^4];
		// Initiate the batch queue
		runningBatch = loadedBatch.Split(';');
		batchIndex = 0;
		// Lock All Controls
		myControlsEnabled.Clear();
		foreach (var c in myControls) {
			myControlsEnabled.Add(c.Enabled);
			c.Enabled = false;
		}
		NextBatch();
	}
	private void PrevButton_Click(object sender, EventArgs e) => MoveFrame(-1);
	private void AnimateButton_Click(object sender, EventArgs e) {
		animated = !animated;
		SetAnimate();
	}
	private void NextButton_Click(object sender, EventArgs e) => MoveFrame(1);
	private void RestartButton_Click(object sender, EventArgs e) {
		if (restartButton.Text == "! RESTART !") {
			restartButton.Text = "ARE YOU SURE?";
			restartTimer = 2000;
			return;
		}
		restartTimer = 0;
		restartButton.Text = "! RESTART !";
		restartButton.Enabled = false;
		QueueReset();
	}
	private void HelpButton_Click(object sender, EventArgs e) {
		helpPanel.Visible = screenPanel.Visible;
		screenPanel.Visible = !screenPanel.Visible;
	}
	private void FileSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (fileSelect.SelectedIndex <= 0)
			return;

		var m = (ushort)(1 << (fileSelect.SelectedIndex - 1));
		fileMask ^= m;

		/*fileSelect.SelectedIndex = 0;
		
		string s = fileSelect.Items[fileSelect.SelectedIndex].ToString();
		fileSelect.Items[fileSelect.SelectedIndex] = (fileMask >> (fileSelect.SelectedIndex - 1) & 1) == 1 ? s.Replace(charOff, charOn) : s.Replace(charOn, charOff);*/

		SetFileMask();
	}
	private void SetFileMask() {
		fileSelect.SelectedIndex = 0;
		int a = fileMask;
		int s = 1;
		string f = "Fractal_";
		while (s < fileSelect.Items.Count) {
			if ((a & 1) == 1) {
				f += filePostfix[s].ToString()[0];
				fileSelect.Items[s] = charOn + filePostfix[s];
			} else {
				fileSelect.Items[s] = charOff + filePostfix[s];
			}

			++s;
			a >>= 1;
		}
		fileSelect.Items[0] = f;
		fileSelect.SelectedIndex = 0;
	}
	private void DebugBox_CheckedChanged(object sender, EventArgs e) {
		generator.DebugTasks = debugBox.Checked;
		if (!(debugBox.Checked || debugAnimBox.Checked || debugPngBox.Checked || debugGifBox.Checked))
			debugLabel.Text = "";
	}
	private void debugAnimBox_CheckedChanged(object sender, EventArgs e) {
		generator.DebugAnim = debugAnimBox.Checked;
		if (!(debugBox.Checked || debugAnimBox.Checked || debugPngBox.Checked || debugGifBox.Checked))
			debugLabel.Text = "";
	}
	private void debugPngBox_CheckedChanged(object sender, EventArgs e) {
		generator.DebugPng = debugPngBox.Checked;
		if (!(debugBox.Checked || debugAnimBox.Checked || debugPngBox.Checked || debugGifBox.Checked))
			debugLabel.Text = "";
	}
	private void debugGifBox_CheckedChanged(object sender, EventArgs e) {
		generator.DebugGif = debugGifBox.Checked;
		if (!(debugBox.Checked || debugAnimBox.Checked || debugPngBox.Checked || debugGifBox.Checked))
			debugLabel.Text = "";
	}
	#endregion

	#region Output
	/// <summary>
	/// Invalidation event of screen display - draws the current display frame bitmap.
	/// Get called repeatedly with new frame to animate the preview, if animation is toggled
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ScreenPanel_Paint(object sender, PaintEventArgs e) {
		if (currentBitmap == null)
			return;
		// Faster rendering with crisp pixels
		e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		// some safety code to ensure no crashes
		byte attempt = 0;
		while (attempt < 10) {
			try {
				e.Graphics.DrawImage(currentBitmap, new Rectangle(0, 0, screenPanel.Width, screenPanel.Height));
				attempt = 10;
			} catch (Exception) {
				++attempt;
				Thread.Sleep(10 + 10 * attempt * attempt);
			}
		}
	}
	private void ExportButton_Click(object sender, EventArgs e) {
		// somehow this being checked was messing with my dialog focus:
		debugBox.Checked = debugAnimBox.Checked = debugPngBox.Checked = debugGifBox.Checked = false;
		var b = generator.GetBitmapsFinished();
		var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

		if (xTask != null) {
			var result = MessageBox.Show("Cancel saving the file(s)?", "Cancel Saving?", MessageBoxButtons.YesNo);
			if (result == DialogResult.Yes)
				xCancel?.Cancel();
			return;
		}

		switch (exportSelect.SelectedIndex) {
			case 0: // Current PNG
				if (b < 1) {
					_ = Error("This is only a low resolution preview image, please wait until the full resolution you have selected if finished.",
						"Please wait");
					return;
				}
				// Make sure the bitmap is actually loaded
				UpdateBitmap(generator.GetBitmap(currentBitmapIndex %= b));
				SetFileName(savePng, "png");
				_ = savePng.ShowDialog();
				break;
			case 1: // Animation PNG
				if (b < generator.GetFrames()) {
					_ = Error("Animation is not finished generating yet.",
						"Please wait");
					return;
				}
				SetFileName(saveMp4, "png");
				saveMp4.DefaultExt = "png";
				saveMp4.Filter = "PNG file (*.png)|*.png";
				_ = saveMp4.ShowDialog();
				break;
			case 2: // PNGs->MP4
				if (!File.Exists(ffmpegPath)) {
					_ = Error("ffmpeg.exe not found. Are you sure you have downloaded the full release with ffmpeg included?",
						"Unavailable");
					return;
				}
				if (b < generator.GetFrames()) {
					_ = Error("Animation is not finished generating yet.",
						"Please wait");
					return;
				}
				SetFileName(saveMp4, "mp4");
				saveMp4.DefaultExt = "mp4";
				saveMp4.Filter = "MP4 video (*.mp4)|*.mp4";
				_ = saveMp4.ShowDialog();
				break;
			case 3: // GIF

				if (generator.SelectedGifType == FractalGenerator.GifType.No) {
					_ = Error("You do not have GIF encoding enabled, pick a Local of Global GIF, and try again after it's done encoding.",
						"Not selected");
					return;
				}
				if (b < generator.GetFrames()) {
					_ = Error("Animation is not finished generating yet.",
						"Please wait");
					return;
				}
				if (isGifReady == 0) {
					_ = Error("Encoded GIF is not available.",
						"Not available");
					return;
				}
				SetFileName(saveGif, "gif");
				saveGif.DefaultExt = "gif";
				saveGif.Filter = "GIF video (*.gif)|*.gif";
				_ = saveGif.ShowDialog();
				break;
			case 4: // GIF->MP4
				if (!File.Exists(ffmpegPath)) {
					_ = Error("ffmpeg.exe not found. Are you sure you have downloaded the full release with ffmpeg included?",
						"Unavailable");
					return;
				}
				if (generator.SelectedGifType == FractalGenerator.GifType.No) {
					_ = Error("You do not have GIF encoding enabled, pick a Local of Global GIF, and try again after it's done encoding.",
						"Not selected");
					return;
				}
				if (b < generator.GetFrames()) {
					_ = Error("Animation is not finished generating yet.",
						"Please wait");
					return;
				}
				if (isGifReady == 0 || gifPath == "") {
					_ = Error("Encoded GIF is not available.",
						"Not available");
					return;
				}
				SetFileName(saveGif, "mp4");
				saveGif.DefaultExt = "mp4";
				saveGif.Filter = "MP4 video (*.mp4)|*.mp4";
				_ = saveGif.ShowDialog();
				break;
			case 5: // IMPORT
				loadExport.DefaultExt = "";
				loadExport.Filter = "All files (*.*)|*.*";
				_ = loadExport.ShowDialog();
				break;
		}
	}
	private void ExportSelect_SelectedIndexChanged(object sender, EventArgs e) {
		// TODO fix this
		if (exportSelect.SelectedIndex == 4) {
			_ = Error("This Export type is currently broken and wil be fixed later, or removed.\nJust use regular MP4 export that converts PNGs, it has better quality anyway.", "Not yet availble");
			return;
		}
		toolTips.SetToolTip(exportButton, exportSelect.SelectedIndex switch {
			0 => "Export the currently displayed frame into a PNG file.\nStop the animation and select the frame you wish to export with the buttons above.",
			1 => "Export the full animation into series of PNG files",
			2 => "Export the full animation into MP4 video",
			3 => "Export the full animation into a GIF file.\nMust have GIF encoding enabled.",
			4 => "Convert a GIF file into Mp4.\nMust have GIF encoding enabled.",
			5 => "This will import a fractaal instead.\nIf you select any exported fractal it will attempt to load everything it can to replicate it.",
			_ => ""
		});
	}
	/// <summary>
	/// User input the path and name for saving PNG
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void SavePng_FileOk(object sender, CancelEventArgs e) {
		BackColor = Color.FromArgb(64, 64, 64);
		using var myStream = savePng.OpenFile();
		currentBitmap.Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
		myStream.Close();
		FinishedExporting();
	}
	private void FinishedExporting() {
		BackColor = Color.FromArgb(64, 64, 128);
		if (runningBatch.Length > 0)
			NextBatch();
	}
	/// <summary>
	/// User input the path and name for saving GIF/Mp4
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void SaveGif_FileOk(object sender, CancelEventArgs e) {

		if (xTask != null) {
			_ = Error("Export is still saving, either wait until it's finished, or click this button before saving a GIF.",
				"Please wait");
			return;
		}
		string ext = saveGif.FileName[^3..];
		if (ext == "gif") {
			// save gif:
			BackColor = Color.FromArgb(64, 64, 64);
			gifPath = ((SaveFileDialog)sender).FileName;
			// Gif Export Task
			//foreach (var c in MyControls)c.Enabled = false;
			xTask = Task.Run(ExportGif, (xCancel = new()).Token);
			return;
		}
		// convert gif->mp4
		if (NoFfmpeg())
			return;
		mp4Path = ((SaveFileDialog)sender).FileName;
		if (isGifReady > 0) {
			BackColor = Color.FromArgb(64, 64, 64);
			gifPath = generator.GifTempPath;
			xTask = Task.Run(ConvertMp4, (xCancel = new()).Token);
			return;
		}
		if (gifPath == "")
			return;
		BackColor = Color.FromArgb(64, 64, 64);
		xTask = Task.Run(ConvertMp4, (xCancel = new()).Token);
	}
	private static bool NoFfmpeg() {
		var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
		if (!File.Exists(ffmpegPath)) {
			_ = Error("ffmpeg.exe not found. Are you sure you have downloaded the full release with ffmpeg included?",
				"Unavailable");
			return true;
		}
		return false;
	}
	/// <summary>
	/// User input the path and name for converting GIF -> Mp4
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void SaveMp4_FileOk(object sender, CancelEventArgs e) {
		if (xTask != null) {
			_ = Error("Export is still saving, wait until it's finished.",
				"Please wait");
			return;
		}
		if (generator.SelectedPngType == FractalGenerator.PngType.No) {
			encodePngSelect.SelectedIndex = 1;
		}
		string ext = saveMp4.FileName[^3..];
		if (ext == "png") {
			// save pngs:
			BackColor = Color.FromArgb(64, 64, 64);
			mp4Path = ((SaveFileDialog)sender).FileName;
			xTask = Task.Run(ExportPngs, (xCancel = new()).Token);
			return;
		}
		// save mp4:
		var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
		if (!File.Exists(ffmpegPath)) {
			_ = Error("ffmpeg.exe not found. Are you sure you have downloaded the full release with ffmpeg included?",
				"Unavailable");
			return;
		}
		BackColor = Color.FromArgb(64, 64, 64);
		mp4Path = ((SaveFileDialog)sender).FileName;
		xTask = Task.Run(ExportMp4, (xCancel = new()).Token);
	}
	private void LoadExport_FileOk(object sender, CancelEventArgs e) {
		LoadExportFile();
	}
	private bool LoadExportFile() {
		var s = Path.GetFileNameWithoutExtension(loadExport.FileName).Split(['_', '(', ')']);
		var f = generator.GetFractals();

		// load fractal type:
		short fi = 0;
		while (fi < f.Count()) {
			if (f[fi].Name == s[0]) {
				fractalSelect.SelectedIndex = fi;
				break;
			}
			++fi;
		}
		if (fi == f.Count())
			return false; // not valid file name, nothing can be loaded
		fi = 1;
		short _s;
		while (fi < s.Count()) {
			if (s[fi].Length < 1)
				break;
			switch (s[fi][0]) {
				case 'A': // Angles
						  // Child Angles Bitmask
					if (ulong.TryParse(s[++fi], out generator.SelectedChildAngles)) {
						FillSelectAngles();
						++fi;
					} else return QueueReset();
					break;
				case 'C': // Colors
						  // Child Colors Bitmask:
					if (ulong.TryParse(s[++fi], out generator.SelectedChildColors)) {
						FillSelectColors();
						++fi;
					} else return QueueReset();
					break;
				case 'F': // Cut Function
						  // Cut Function Select:
					if (short.TryParse(s[++fi], out _s)) {
						cutSelect.SelectedIndex = _s;
					} else break;
					// Cut Function Seed:
					if (long.TryParse(s[++fi], out long _l)) {
						cutparamBox.Text = _l.ToString();
					} else break;
					++fi;
					break;
				case 'R': // Resolution
						  // Width:
					if (ushort.TryParse(s[++fi], out generator.SelectedWidth)) {
						resX.Text = generator.SelectedWidth.ToString();
					} else break;
					// Height:
					if (ushort.TryParse(s[++fi], out generator.SelectedHeight)) {
						resY.Text = generator.SelectedHeight.ToString();
					} else break;
					resSelect.SelectedIndex = 1;
					++fi;
					break;
				case 'H': // Hues
					short hi = 0;
					var cc = generator.Colors;
					++fi;
					// Palette:
					while (hi < f.Count()) {
						if (cc[hi].Item1 == s[fi]) {
							paletteSelect.SelectedIndex = hi + 1;
							break;
						}
						++hi;
					}
					// Select Hue:
					if (short.TryParse(s[++fi], out _s)) {
						hueSelect.SelectedIndex = _s;
					} else break;
					// Default Hue:
					if (double.TryParse(s[++fi], out var _d)) {
						defaultHue.Text = _d.ToString();
					} else break;
					// Extra Hue:
					if (short.TryParse(s[++fi], out _s)) {
						hueSpeedBox.Text = _s.ToString();
					} else break;
					++fi;
					break;
				case 'P': // Period Settings
						  // Period:
					if (short.TryParse(s[++fi], out _s)) {
						periodBox.Text = _s.ToString();
					} else break;
					// Period Multiplier:
					if (short.TryParse(s[++fi], out _s)) {
						periodMultiplierBox.Text = _s.ToString();
					} else break;
					// Period Multiplier:
					if (short.TryParse(s[++fi], out _s)) {
						if (_s < 0) {
							timingSelect.SelectedIndex = 0;
							timingBox.Text = (-_s).ToString();
						} else {
							timingSelect.SelectedIndex = 1;
							timingBox.Text = _s.ToString();
						}
					} else break;
					++fi;
					break;
				case 'Z': // Zoom Settings
						  // Zoom Direction:
					if (short.TryParse(s[++fi], out _s)) {
						zoomSelect.SelectedIndex = _s;
					} else break;
					// Zoom Direction:
					if (short.TryParse(s[++fi], out _s)) {
						defaultZoomBox.Text = _s.ToString();
					} else break;
					// Zoom Child:
					if (short.TryParse(s[++fi], out _s)) {
						zoomChildBox.Text = _s.ToString();
					} else break;
					++fi;
					break;
				case 'S': // Spin Settings
						  // Spin Direction:
					if (short.TryParse(s[++fi], out _s)) {
						spinSelect.SelectedIndex = _s;
					} else break;
					// Default Angle:
					if (short.TryParse(s[++fi], out _s)) {
						defaultAngleBox.Text = _s.ToString();
					} else break;
					// Extra Spin:
					if (short.TryParse(s[++fi], out _s)) {
						spinSpeedBox.Text = _s.ToString();
					} else break;
					++fi;
					break;
				case 'V': // Void Settings
						  // Ambient:
					if (short.TryParse(s[++fi], out _s)) {
						ambBox.Text = _s.ToString();
					} else break;
					// Noise:
					if (short.TryParse(s[++fi], out _s)) {
						noiseBox.Text = _s.ToString();
					} else break;
					// Scale:
					if (short.TryParse(s[++fi], out _s)) {
						voidBox.Text = _s.ToString();
					} else break;
					++fi;
					break;
				case 'I': // Image Post Processing
						  // Saturation
					if (short.TryParse(s[++fi], out _s)) {
						saturateBox.Text = _s.ToString();
					} else break;
					// Brightness
					if (short.TryParse(s[++fi], out _s)) {
						brightnessBox.Text = _s.ToString();
					} else break;
					// Bloom
					if (short.TryParse(s[++fi], out _s)) {
						bloomBox.Text = _s.ToString();
					} else break;
					// Motion Blur
					if (short.TryParse(s[++fi], out _s)) {
						blurBox.Text = _s.ToString();
					} else break;
					++fi;
					break;
				case 'D': // Details
					if (short.TryParse(s[++fi], out _s)) {
						ditherBox.Checked = _s == 1;
					} else break;
					// Detail:
					if (short.TryParse(s[++fi], out _s)) {
						detailBox.Text = _s.ToString();
					} else break;
					++fi;
					break;
				default:
					return QueueReset();
			}
			++fi; // skips the empty strings inbetween ")_"
		}
		return QueueReset();
	}
	/// <summary>
	/// Exports the animation into a GIF file
	/// Actually - it just moves the already exported gifX.tmp to you desired location and name
	/// </summary>
	/// <returns></returns>
	private void ExportGif() {
		var attempt = 0;
		while (++attempt <= 10 && !xCancel.Token.IsCancellationRequested && generator.SaveGif(gifPath) > 0)
			Thread.Sleep(10 + 10 * attempt * attempt);
		if (!xCancel.Token.IsCancellationRequested)
			isGifReady = 0;
		notifyExp = true;
		xTask = null;
	}
	/// <summary>
	/// Exports the animation into a MP4 file
	/// Actually - it just converts a gif file into MP4
	/// </summary>
	/// <returns></returns>
	private void ConvertMp4() {
		string result = generator.SaveGifToMp4(gifPath, mp4Path);
		if (xCancel.Token.IsCancellationRequested) {
			xTask = null;
			return;
		}
		if (result == "") {
			isGifReady = 0;
			notifyExp = true;
			gifPath = "";
			mp4Path = "";
			xTask = null;
			return;
		}
		_ = Error(result, "Failed GIF export");
	}
	/// <summary>
	/// Exports the animation into a MP4 file
	/// Actually - it just exports PNG series and converts those into MP4
	/// </summary>
	/// <returns></returns>
	private void ExportPngs() {
		string result = generator.SavePngs(mp4Path);
		if (xCancel.Token.IsCancellationRequested) {
			xTask = null;
			return;
		}
		if (result == "") {
			notifyExp = true;
			mp4Path = "";
			isPngsSaved = true;
			xTask = null;
			return;
		}
		_ = Error(result, "Failed PNGs export");
	}
	/// <summary>
	/// Exports the animation into a MP4 file
	/// Actually - it just exports PNG series and converts those into MP4
	/// </summary>
	/// <returns></returns>
	private void ExportMp4() {
		string result = generator.SavePngsToMp4(mp4Path);
		if (xCancel.Token.IsCancellationRequested) {
			xTask = null;
			return;
		}
		if (result == "") {
			notifyExp = true;
			mp4Path = "";
			isPngsSaved = true;
			xTask = null;
			return;
		}
		_ = Error(result, "Failed MP4 export");
	}
	void SetFileName(SaveFileDialog dialog, string extension) {
		//var prev = dialog.FileName;
		string f = "";
		f += generator.GetFractal().Name;
		if ((fileMask & 1) > 0)
			f += "_A(" + generator.SelectedChildAngles + ")";
		if ((fileMask & 2) > 0)
			f += "_C(" + generator.SelectedChildColors + ")";
		if ((fileMask & 4) > 0)
			f += "_F(" + cutSelect.SelectedIndex + "_" + cutparamBox.Text + ")";
		if ((fileMask & 8) > 0)
			f += "_R(" + generator.SelectedWidth + "_" + generator.SelectedHeight + ")";
		if ((fileMask & 16) > 0)
			f += "_H(" + generator.Colors[generator.SelectedPaletteType].Item1 + "_" + hueSelect.SelectedIndex + "_" + defaultHue.Text + "_" + hueSpeedBox.Text + ")";
		if ((fileMask & 32) > 0)
			f += "_P(" + periodBox.Text + "_" + periodMultiplierBox.Text + "_" + (timingSelect.SelectedIndex == 0 ? "-" + timingBox.Text : timingBox.Text) + ")";
		if ((fileMask & 64) > 0)
			f += "_Z(" + zoomSelect.SelectedIndex + "_" + defaultZoomBox.Text + "_" + zoomChildBox.Text + ")";
		if ((fileMask & 128) > 0)
			f += "_S(" + spinSelect.SelectedIndex + "_" + defaultAngleBox.Text + "_" + spinSpeedBox.Text + ")";
		if ((fileMask & 256) > 0)
			f += "_V(" + ambBox.Text + "_" + noiseBox.Text + "_" + voidBox.Text + ")";
		if ((fileMask & 512) > 0)
			f += "_I(" + saturateBox.Text + "_" + brightnessBox.Text + "_" + bloomBox.Text + "_" + blurBox.Text + ")";
		if ((fileMask & 1024) > 0)
			f += "_D(" + (ditherBox.Checked ? "1" : "0") + "_" + detailBox.Text + ")";
		dialog.FileName = f + "." + extension;
	}
	#endregion

	#region Editor
	private void FillEditor() {
		pointTabIndex = controlTabIndex;
		pointPanel.SuspendLayout();
		UnFillEditor();
		var f = generator.GetFractal();
		for (var i = 0; i < f.ChildCount; ++i)
			AddEditorPoint(f.ChildX, f.ChildY, f.ChildAngle[generator.SelectedChildAngle].Item2, f.ChildColor[generator.SelectedChildColor].Item2, false);
		var e = f.Edit;
		sizeBox.Text = f.ChildSize.ToString(CultureInfo.InvariantCulture);
		cutBox.Text = f.CutSize.ToString(CultureInfo.InvariantCulture);
		minBox.Text = f.MinSize.ToString(CultureInfo.InvariantCulture);
		maxBox.Text = f.MaxSize.ToString(CultureInfo.InvariantCulture);
		f.Edit = e;
		pointPanel.ResumeLayout(false);
		pointPanel.PerformLayout();
	}
	private void UnFillEditor() {
		addPoint.Location = new(10, 10);

		foreach (var s in editorSwitch) {
			pointPanel.Controls.Remove(s);
			myControls.Remove(s);
		}
		foreach (var (x, y, a, c, d) in editorPoint) {
			pointPanel.Controls.Remove(x);
			pointPanel.Controls.Remove(y);
			pointPanel.Controls.Remove(a);
			pointPanel.Controls.Remove(c);
			pointPanel.Controls.Remove(d);
			myControls.Remove(x);
			myControls.Remove(y);
			myControls.Remove(a);
			myControls.Remove(c);
			myControls.Remove(d);
			x.Dispose();
			y.Dispose();
			a.Dispose();
			c.Dispose();
			d.Dispose();
		}
		editorSwitch.Clear();
		editorPoint.Clear();
		pointTabIndex = controlTabIndex;
	}
	private void SwitchChildAngle() {
		var f = generator.GetFractal();
		var e = f.Edit;
		for (var i = 0; i < f.ChildCount; ++i)
			editorPoint[i].Item3.Text = f.ChildAngle[generator.SelectedChildAngle].Item2[i].ToString(CultureInfo.InvariantCulture);
		f.Edit = e;
	}
	private void SwitchChildColor() {
		var f = generator.GetFractal();
		var e = f.Edit;
		for (var i = 0; i < f.ChildCount; ++i)
			editorPoint[i].Item4.BackColor = f.ChildColor[generator.SelectedChildColor].Item2[i] switch {
				0 => Color.Red,
				1 => Color.Green,
				_ => Color.Blue
			};
		f.Edit = e;
	}
	private void AddEditorPoint(double[] cx, double[] cy, double[] ca, byte[] cc, bool single = true) {
		var i = editorPoint.Count;
		editorPoint.Add((new(), new(), new(), new(), new()));
		var (x, y, a, c, d) = editorPoint[i];
		x.Text = cx[i].ToString(CultureInfo.InvariantCulture);
		y.Text = cy[i].ToString(CultureInfo.InvariantCulture);
		a.Text = ca[i].ToString(CultureInfo.InvariantCulture);
		c.Text = cc[i].ToString(CultureInfo.InvariantCulture);
		d.Text = "X";
		/*c.BackColor = cc[i] switch {
			0 => Color.Red,
			1 => Color.Green,
			_ => Color.Blue
		};*/
		BindPoint(x, y, a, c, d, i, single);
	}
	private void BindPoint(TextBox x, TextBox y, TextBox a, TextBox c, Button d, int i, bool single = true) {
		const int textSize = 53;
		const int buttonSize = 23;
		if (single)
			pointPanel.SuspendLayout();
		if (i > 1) {
			editorSwitch.Add(new());
			var s = editorSwitch[^1];
			s.Text = "⇕";
			s.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
			s.Location = new Point(10 + 3 * textSize + 2 * buttonSize, 10 + (i * 2 - 1) * buttonSize / 2);
			s.Margin = new Padding(4, 3, 4, 3);
			s.Name = "switch" + i;
			s.Size = new Size(buttonSize, buttonSize);
			pointPanel.Controls.Add(s);
			myControls.Add(s);
			SetupEditControl(s, "Switch these two points in the list. (Only has effects on cutFunctions, or if you switch the main first one.)");
			s.Click += (_, _) => {
				var f = generator.GetFractal();
				//var ni = f.ChildCount;
				var cx = f.ChildX;
				var cy = f.ChildY;
				(cx[i], cx[i - 1], cy[i], cy[i - 1])
					= (cx[i - 1], cx[i], cy[i - 1], cy[i]);
				for (var l = 0; l < f.ChildAngle.Count; ++l) {
					var ca = f.ChildAngle[l].Item2;
					(ca[i], ca[i - 1]) = (ca[i - 1], ca[i]);
				}
				for (var l = 0; l < f.ChildColor.Count; ++l) {
					var cc = f.ChildColor[l].Item2;
					(cc[i], cc[i - 1]) = (cc[i - 1], cc[i]);
				}
				var (xi, yi, ai, ci, _) = editorPoint[i];
				xi.Text = cx[i].ToString(CultureInfo.InvariantCulture);
				yi.Text = cy[i].ToString(CultureInfo.InvariantCulture);
				ai.Text = f.ChildAngle[generator.SelectedChildColor].Item2[i].ToString(CultureInfo.InvariantCulture);
				ci.Text = f.ChildColor[generator.SelectedChildColor].Item2[i].ToString(CultureInfo.InvariantCulture);
				var (xi1, yi1, ai1, ci1, _) = editorPoint[i - 1];
				xi1.Text = cx[i - 1].ToString(CultureInfo.InvariantCulture);
				yi1.Text = cy[i - 1].ToString(CultureInfo.InvariantCulture);
				ai1.Text = f.ChildAngle[generator.SelectedChildColor].Item2[i - 1].ToString(CultureInfo.InvariantCulture);
				ci1.Text = f.ChildColor[generator.SelectedChildColor].Item2[i - 1].ToString(CultureInfo.InvariantCulture);
				f.Edit = true;
				QueueReset();
			};
		}
		pointPanel.Controls.Add(x);
		pointPanel.Controls.Add(y);
		pointPanel.Controls.Add(a);
		pointPanel.Controls.Add(c);
		pointPanel.Controls.Add(d);
		myControls.Add(x);
		SetupEditControl(x, "X coordinate of the child.");
		myControls.Add(y);
		SetupEditControl(y, "Y coordinate of the child.");
		myControls.Add(a);
		SetupEditControl(a, "Angle of the child.");
		myControls.Add(c);
		SetupEditControl(c, "Color shift of the child in halves. (2 in RGB: RGB->GBR, 4 in RGB: RGB->BRG, 3 in RGB: RGB->CPY, 2 in YB: YB->BY)");
		myControls.Add(d);
		SetupEditControl(d, "Remove this point.");
		if (single) {
			pointPanel.ResumeLayout(false);
			pointPanel.PerformLayout();
		}
		x.Location = new Point(10, 10 + i * buttonSize);
		x.Margin = new Padding(4, 3, 4, 3);
		x.Name = "x" + i;
		x.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
		x.Size = new Size(textSize, buttonSize);
		x.Enabled = i > 0;
		if (x.Enabled)
			x.TextChanged += (sender, _) => {
				if (ParseDiffApply((TextBox)sender, ref generator.GetFractal().ChildX[i])) return;
				generator.GetFractal().Edit = true;
			};

		y.Location = new Point(10 + textSize, 10 + i * buttonSize);
		y.Margin = new Padding(4, 3, 4, 3);
		y.Name = "y" + y;
		y.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
		y.Size = new Size(textSize, buttonSize);
		y.Enabled = i > 0;
		if (y.Enabled)
			y.TextChanged += (sender, _) => {
				if (ParseDiffApply((TextBox)sender, ref generator.GetFractal().ChildY[i])) return;
				generator.GetFractal().Edit = true;
			};

		a.Location = new Point(10 + 2 * textSize, 10 + i * buttonSize);
		a.Margin = new Padding(4, 3, 4, 3);
		a.Name = "a" + i;
		a.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
		a.Size = new Size(textSize, buttonSize);
		a.TextChanged += (sender, _) => {
			if (ParseDiffApply((TextBox)sender, ref generator.GetFractal().ChildAngle[generator.SelectedChildAngle].Item2[i])) return;
			generator.GetFractal().Edit = true;
		};

		c.Location = new Point(10 + 3 * textSize, 10 + i * buttonSize);
		c.Margin = new Padding(4, 3, 4, 3);
		c.Name = "c" + i;
		c.Size = new Size(buttonSize, buttonSize);
		c.TextChanged += (sender, _) => {
			if (DiffApply(ReText((TextBox)sender, ParseValue<byte>((TextBox)sender)), ref generator.GetFractal().ChildColor[generator.SelectedChildColor].Item2[i]))
				return;
			generator.GetFractal().Edit = true;
		};
		d.Location = new Point(10 + 3 * textSize + buttonSize, 10 + i * buttonSize);
		d.Margin = new Padding(4, 3, 4, 3);
		d.Name = "d" + i;
		d.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
		d.Size = new Size(23, 23);
		d.Enabled = i > 0;
		if (d.Enabled)
			d.Click += (_, _) => {
				var f = generator.GetFractal();
				var ni = --f.ChildCount;
				var nx = new double[ni];
				var ny = new double[ni];
				var na = new double[f.ChildAngle.Count][];
				var nc = new byte[f.ChildColor.Count][];
				var cx = f.ChildX;
				var cy = f.ChildY;
				pointPanel.SuspendLayout();
				UnFillEditor();
				pointPanel.ResumeLayout(false);
				int xp;
				// RemoveAt from XY
				for (var ci = 0; ci < i; ++ci) {
					nx[ci] = cx[ci];
					ny[ci] = cy[ci];
				}
				for (var ci = i; ci < ni; ci = xp) {
					xp = ci + 1;
					nx[ci] = cx[xp];
					ny[ci] = cy[xp];
				}
				f.ChildX = nx;
				f.ChildY = ny;
				// RemoveAt from Angle
				for (var l = 0; l < f.ChildAngle.Count; ++l) {
					var nal = na[l] = new double[ni];
					var (can, ca) = f.ChildAngle[l];
					for (var ci = 0; ci < i; ++ci)
						nal[ci] = ca[ci];
					for (var ci = i; ci < ni; ci = xp) {
						xp = ci + 1;
						nal[ci] = ca[xp];
					}
					f.ChildAngle[l] = (can, nal);
				}
				// RemoveAt from Color
				for (var l = 0; l < f.ChildColor.Count; ++l) {
					var ncl = nc[l] = new byte[ni];
					var (ccn, cc) = f.ChildColor[l];
					for (var ci = 0; ci < i; ++ci)
						ncl[ci] = cc[ci];
					for (var ci = i; ci < ni; ci = xp) {
						xp = ci + 1;
						ncl[ci] = cc[xp];
					}
					f.ChildColor[l] = (ccn, ncl);
				}
				// RemoveAt from editor points
				for (var ci = 0; ci < ni; ++ci)
					AddEditorPoint(nx, ny, na[generator.SelectedChildAngle], nc[generator.SelectedChildAngle], false);
				// Finish edit
				f.Edit = true;
				QueueReset();
			};
		addPoint.Location = new(10, 10 + (i + 1) * buttonSize);
	}
	private void SaveFractal_FileOk(object sender, CancelEventArgs e) {
		var f = toSave;
		var fractalName = f.Path = ((SaveFileDialog)sender).FileName;
		int index;
		while ((index = fractalName.IndexOf('/')) >= 0)
			fractalName = fractalName[(index + 1)..];
		while ((index = fractalName.IndexOf('\\')) >= 0)
			fractalName = fractalName[(index + 1)..];
		f.Name = fractalName.Replace('|', '.').Replace(':', '.').Replace(';', '.');
		// Constants
		string p = "|", a, s = f.Name + "|" + f.ChildCount + "|" + f.ChildSize + "|" + f.MaxSize + "|" + f.MinSize + "|" + f.CutSize;
		// XY
		foreach (var x in f.ChildX)
			p += x + ";";
		s += p[..^1];
		p = "|";
		foreach (var x in f.ChildY)
			p += x + ";";
		s += p[..^1];
		// Angles
		p = "|";
		foreach (var (an, ai) in f.ChildAngle) {
			a = an + ":";
			foreach (var i in ai)
				a += i + ":";
			p += a[..^1] + ";";
		}
		s += p[..^1];
		// Colors
		p = "|";
		foreach (var (an, ai) in f.ChildColor) {
			a = an + ":";
			foreach (var i in ai)
				a += i + ":";
			p += a[..^1] + ";";
		}
		s += p[..^1];
		// Cuts
		p = "|";
		foreach (var (an, ai) in f.ChildCutFunction) {
			a = an + ":";
			foreach (var i in ai)
				a += i + ":";
			p += (a != "" ? a[..^1] : "") + ";";
		}
		s += p == "|" ? "|" : p[..^1];
		File.WriteAllText(f.Path, s);
		f.Edit = false;
	}
	private void LoadFractal_FileOk(object sender, CancelEventArgs e) {
		var filename = ((OpenFileDialog)sender).FileName;
		var fractalName = filename[..^8];
		int i;
		while ((i = fractalName.IndexOf('/')) >= 0)
			fractalName = fractalName[(i + 1)..];
		while ((i = fractalName.IndexOf('\\')) >= 0)
			fractalName = fractalName[(i + 1)..];
		fractalName = fractalName.Replace('|', '.').Replace(':', '.').Replace(';', '.');
		if (fractalSelect.Items.Contains(fractalName)) {
			_ = Error("There already is a loaded fractal of the same name, selecting it.", "Already exists");
			fractalSelect.SelectedIndex = fractalSelect.Items.IndexOf(fractalName);
		} else if (File.Exists(filename))
			_ = LoadFractal(filename);
	}
	private bool LoadFractal(string file, bool select = true) {
		if (!File.Exists(file))
			return Error("No such file", "Cannot load");
		var content = File.ReadAllText(file);
		var arr = content.Split('|');
		// Name
		if (arr[0] == "")
			return Error("No fractal name", "Cannot load");
		if (fractalSelect.Items.Contains(fractalSelect.Text))
			return Error("Fractal with the same name already loaded in the list.", "Cannot load");
		// Constants
		if (!int.TryParse(arr[1], out var count) || count < 1)
			return Error("Invalid children count: " + count, "Cannot load");
		if (!double.TryParse(arr[2], out var size) || size <= 1)
			return Error("Invalid children size: " + size, "Cannot load");
		if (!double.TryParse(arr[3], out var maxsize))
			return Error("Invalid max size: " + maxsize, "Cannot load");
		if (!double.TryParse(arr[4], out var minSize))
			return Error("Invalid min size: " + minSize, "Cannot load");
		if (!double.TryParse(arr[5], out var cutSize))
			return Error("Invalid cut size: " + cutSize, "Cannot load");

		double x;
		// ChildX
		var s = arr[6].Split(';');
		if (s.Length < count)
			return Error("Insufficient children X count: " + s.Length + " / " + count, "Cannot load");
		var childX = new double[count];
		for (var i = count; --i >= 0; childX[i] = x)
			if (!double.TryParse(s[i], out x))
				return Error("Invalid child X: " + s[i], "Cannot load");
		// ChildY
		s = arr[7].Split(';');
		if (s.Length < count)
			return Error("Insufficient children Y count: " + s.Length + " / " + count, "Cannot load");
		var childY = new double[count];
		for (var i = count; --i >= 0; childY[i] = x)
			if (!double.TryParse(s[i], out x))
				return Error("Invalid child Y: " + s[i], "Cannot load");
		// ChildAngles
		s = arr[8].Split(';');
		if (s.Length < 1)
			return Error("No set of child angles", "Cannot load");
		List<(string, double[])> childAngle = [];
		foreach (var c in s) {
			var angleSet = c.Split(':');
			if (angleSet[0] == "")
				return Error("Empty angle set name", "Cannot load");
			if (angleSet.Length < count + 1)
				return Error("Insufficient child angle count of " + angleSet[0] + ": " + (angleSet.Length - 1) + " / " + count, "Cannot load");
			var angle = new double[count];
			for (var i = count; i > 0; angle[i] = x)
				if (!double.TryParse(angleSet[i--], out x))
					return Error("Invalid angle in set" + angleSet[0] + ": " + angleSet[i + 1], "Cannot load");
			childAngle.Add((angleSet[0], angle));
		}
		// ChildColors
		s = arr[9].Split(';');
		if (s.Length < 1)
			return Error("No set of child colors", "Cannot load");
		List<(string, byte[])> childColor = [];
		foreach (var c in s) {
			byte ss;
			var colorSet = c.Split(':');
			if (colorSet[0] == "")
				return Error("Empty color set name", "Cannot load");
			if (colorSet.Length < count + 1)
				return Error("Insufficient child color count of " + colorSet[0] + ": " + (colorSet.Length - 1) + " / " + count, "Cannot load");
			var color = new byte[count];
			for (var i = count; i > 0; color[i] = ss)
				if (!byte.TryParse(colorSet[i--], out ss))
					return Error("Invalid color in set " + colorSet[0] + ": " + colorSet[i + 1], "Cannot load");
			childColor.Add((colorSet[0], color));
		}
		// Cuts

		s = arr[10].Split(';');
		List<(int, int[])> cutFunction = [];
		foreach (var c in s) {
			int intParse;
			var cutInt = c.Split(':');
			if (!int.TryParse(cutInt[0], out var cutIndex))
				return Error("Invalid cutFunction index: " + cutInt[0], "Cannot load");
			var cutHash = new int[cutInt.Length - 1];
			for (var i = cutHash.Length; i > 0; cutHash[i] = intParse)
				if (!int.TryParse(cutInt[i--], out intParse))
					return Error("Invalid cutFunction seed: " + cutInt[i + 1], "Cannot load");
			cutFunction.Add((cutIndex, cutHash));
		}
		var f = new Fractal(arr[0], count, size, maxsize, minSize, cutSize, childX, childY, childAngle, childColor, cutFunction) {
			Path = file
		};
		generator.GetFractals().Add(f);
		if (select)
			fractalSelect.SelectedIndex = fractalSelect.Items.Add(arr[0]);
		else
			_ = fractalSelect.Items.Add(arr[0]);
		return true;
	}
	private void ModeButton_Click(object sender, EventArgs e) {
		if (editorPanel.Visible) {
			// SelectParallelMode();
			generator.SelectedBlur = memBlur;
			generator.SelectedBloom = memBloom;
			generator.SelectedGenerationType = memGenerate;
			generator.SelectedPngType = memPng;
			generator.SelectedGifType = memGif;
			//generator.selectHue = mem_hue;
			generator.SelectedDefaultHue = memDefaultHue;
			abortDelay = memAbort;
			generator.SelectedPreviewMode = false;

		} else {
			memBlur = generator.SelectedBlur;
			memBloom = generator.SelectedBloom;
			memPng = generator.SelectedPngType;
			memGif = generator.SelectedGifType;
			memGenerate = generator.SelectedGenerationType;
			//mem_hue = generator.selectHue;
			memDefaultHue = generator.SelectedDefaultHue;
			memAbort = abortDelay;
			abortDelay = 10;
			generator.SelectedPngType = FractalGenerator.PngType.No;
			generator.SelectedGifType = FractalGenerator.GifType.No;
			generator.SelectedGenerationType = FractalGenerator.GenerationType.Animation;
			generator.SelectedBlur = 0;
			generator.SelectedBloom = generator.SelectedHue = 0;// generator.selectDefaultHue = 0;
			generator.SelectedPreviewMode = previewMode;
		}
		generator.SelectedEditorMode = editorPanel.Visible = !(generatorPanel.Visible = editorPanel.Visible);
		QueueReset();
	}
	private void AddPoint_Click(object sender, EventArgs e) {
		var i = editorPoint.Count;
		editorPoint.Add((new(), new(), new(), new(), new()));
		var (x, y, a, c, d) = editorPoint[i];
		x.Text = y.Text = a.Text = "0";
		d.Text = "X";
		c.Text = "";
		c.BackColor = Color.Red;
		var f = generator.GetFractal();
		var ni = f.ChildCount++;
		if (generator.ChildColor.Length < f.ChildCount) {
			generator.ChildAngle = new double[f.ChildCount];
			generator.ChildColor = new byte[f.ChildCount];
		}
		var nx = new double[ni + 1];
		var ny = new double[ni + 1];
		var na = new double[f.ChildAngle.Count][];
		var nc = new byte[f.ChildColor.Count][];
		var cx = f.ChildX;
		var cy = f.ChildY;
		for (var ci = 0; ci < ni; ++ci) {
			nx[ci] = cx[ci];
			ny[ci] = cy[ci];
		}
		nx[ni] = ny[ni] = 0;
		f.ChildX = nx;
		f.ChildY = ny;
		for (var l = 0; l < f.ChildAngle.Count; ++l) {
			var nal = na[l] = new double[ni];
			var (can, ca) = f.ChildAngle[l];
			for (var ci = 0; ci < ni; ++ci)
				nal[ci] = ca[ci];
			nal[ni] = 0;
			f.ChildAngle[l] = (can, nal);
		}
		// RemoveAt from Color
		for (var l = 0; l < f.ChildColor.Count; ++l) {
			var ncl = nc[l] = new byte[ni];
			var (ccn, cc) = f.ChildColor[l];
			for (var ci = 0; ci < ni; ++ci)
				ncl[ci] = cc[ci];
			ncl[ni] = 0;
			f.ChildColor[l] = (ccn, ncl);
		}
		BindPoint(x, y, a, c, d, i);
		f.Edit = true;
		QueueReset();
	}
	private void SizeBox_TextChanged(object sender, EventArgs e) {
		if (ParseDiffApply(sizeBox, ref generator.GetFractal().ChildSize)) return;
		generator.GetFractal().Edit = true;
	}
	private void MaxBox_TextChanged(object sender, EventArgs e) {
		if (ParseDiffApply(maxBox, ref generator.GetFractal().MaxSize)) return;
		generator.GetFractal().Edit = true;
	}
	private void MinBox_TextChanged(object sender, EventArgs e) {
		if (ParseDiffApply(minBox, ref generator.GetFractal().MinSize)) return;
		generator.GetFractal().Edit = true;
	}
	private void CutBox_TextChanged(object sender, EventArgs e) {
		if (ParseDiffApply(cutBox, ref generator.GetFractal().CutSize)) return;
		generator.GetFractal().Edit = true;
	}
	private void AddAngleButton_Click(object sender, EventArgs e) {
		if (angleBox.Text == "") {
			_ = Error(
			"Type a unique name for the new set of children angles to the box on the left.",
			"Cannot add");
			return;
		}
		foreach (var (n, _) in generator.GetFractal().ChildColor)
			if (n == angleBox.Text) {
				_ = Error(
			"There already is a set of children angles of the same name.\nType a new unique name to the box on the left.",
			"Cannot add");
				return;
			}
		generator.GetFractal().ChildAngle.Add((angleBox.Text, new double[generator.GetFractal().ChildCount]));
		//SetupSelects();
		angleSelect.SelectedIndex = angleSelect.Items.Add(charOff + angleBox.Text);
		generator.GetFractal().Edit = true;
		FillEditor();
	}
	private void RemoveAngleButton_Click(object sender, EventArgs e) {
		if (generator.GetFractal().ChildAngle.Count <= 1) {
			_ = Error(
		"This is the only set of children angles, so it cannot be removed.",
		"Cannot remove");
			return;
		}
		generator.GetFractal().ChildAngle.RemoveAt(generator.SelectedChildAngle);
		generator.SelectedChildAngle = Math.Min((short)(generator.GetFractal().ChildAngle.Count - 1), generator.SelectedChildAngle);
		generator.SelectedChildAngles = 0;
		SetupSelects(); // this was missing here, maybe i just forgot, or did the item in the selectors got removed elsewhere?
		SwitchChildAngle();
		generator.GetFractal().Edit = true;
		QueueReset();
	}
	private void AddColorButton_Click(object sender, EventArgs e) {
		if (colorBox.Text == "") {
			_ = Error(
			"Type a unique name for the new set of children colors to the box on the left.",
			"Cannot add");
			return;
		}
		foreach (var (n, _) in generator.GetFractal().ChildColor)
			if (n == colorBox.Text) {
				_ = Error(
			"There already is a set of children colors of the same name.\nType a new unique name to the box on the left.",
			"Cannot add");
				return;
			}
		generator.GetFractal().ChildColor.Add((colorBox.Text, new byte[generator.GetFractal().ChildCount]));
		//SetupSelects();
		colorSelect.SelectedIndex = colorSelect.Items.Add("✕ " + colorBox.Text);
		generator.GetFractal().Edit = true;
		FillEditor();
	}
	private void RemoveColorButton_Click(object sender, EventArgs e) {
		if (generator.GetFractal().ChildColor.Count <= 1) {
			_ = Error(
		"This is the only set of children colors, so it cannot be removed.",
		"Cannot remove");
			return;
		}
		generator.GetFractal().ChildColor.RemoveAt(generator.SelectedChildColor);
		generator.SelectedChildColor = Math.Min((short)(generator.GetFractal().ChildColor.Count - 1), generator.SelectedChildColor);
		generator.SelectedChildColors = 0;
		SetupSelects(); // this was missing here, maybe i just forgot, or did the item in the selectors got removed elsewhere?
		SwitchChildColor();
		generator.GetFractal().Edit = true;
		QueueReset();
	}
	private void AddCut_SelectedIndexChanged(object sender, EventArgs e) {
		if (addCut.SelectedIndex < 1)
			return;
		var n = addCut.SelectedIndex - 1;
		foreach (var c in generator.GetFractal().ChildCutFunction)
			if (c.Item1 == n)
				return;
		generator.GetFractal().ChildCutFunction.Add((n, []));
		//SetupSelects();
		cutSelect.SelectedIndex = cutSelect.Items.Add(Fractal.CutFunctions[addCut.SelectedIndex - 1].Item1);
		addCut.SelectedIndex = 0;
		generator.GetFractal().Edit = true;
		//_ = Task.Run(Hash);
		//Remember and disable all controls
		myControlsEnabled.Clear();
		foreach (var c in myControls) {
			myControlsEnabled.Add(c.Enabled);
			c.Enabled = false;
		}
		// perform hash
		performHash = true;
		QueueReset();
	}
	private void LoadButton_Click(object sender, EventArgs e) {
		_ = loadFractal.ShowDialog();
	}
	private void SaveButton_Click(object sender, EventArgs e) {
		toSave = generator.GetFractal();
		_ = saveFractal.ShowDialog();
	}
	private void PreButton_Click(object sender, EventArgs e) {
		generator.SelectedPreviewMode = previewMode = !previewMode;
		QueueReset();
	}
	#endregion

}
