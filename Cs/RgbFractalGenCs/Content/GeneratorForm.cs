// Starts the generator with special testing settings
//#define CustomDebugTest

using ComputeSharp;
using RgbFractalGenCs.Content;
using RgbFractalGenCs.Content.Basic;
using RgbFractalGenCs.Core;
using System;
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
using static RgbFractalGenCs.Content.Static.StaticContent;
using static RgbFractalGenCs.Core.FractalGenerator;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class GeneratorForm : Form {

	internal struct Schedule(ScheduledTask type, string filename) {
		internal string Filename = filename;
		internal ScheduledTask Type = type;
	}


	#region Designer
	public GeneratorForm(MainForm source, GeneratorsForm sourceGens, uint index, GeneratorForm
#if NULLABLE
?
#endif
	from = null) {
			InitializeComponent();
		fromGen = from;
		root = source;
		gens = sourceGens;
		screenPanel = new() {
			Location = new(0, 0), // old = (239,13)
			Name = "screenPanel",
			Size = new Size(80, 80),
			TabIndex = 0,
			TabStop = false
		};
		screenPanel.Paint += ScreenPanel_Paint;
		screenPanel.Click += AnimateButton_Click;
		Controls.Add(screenPanel);
		//MouseDown += Control_MouseDown; // Detect clicks on empty form space
		//RegisterMouseDownRecursive(this); // Detect clicks on all controls
		importForm = new(this);
		scheduler = new(this);
		generator = new(Index = index);
	}
	#endregion

	private readonly MainForm root;
	private readonly GeneratorsForm gens;
	private readonly SchedulerForm scheduler;

	//public const string charOn = "✓ ";
	//public const string charOff = "✕ ";
	//public const string charOn = "[■]";
	//public const string charOff = "[□]";
	public const string charOn = "[⬛] ";
	public const string charOff = "[⬜] ";
	private readonly List<string> filePostfix = [];

	internal bool SetWorking(bool working) => generator.Working = Working = working;
	internal void OpenTasks() { scheduler.Show(); scheduler.Location = Location; }

	#region Variables
	// Threading
	internal string MyName = "";    // Name of this generator (how do you see it in the list and window header)
	private bool shown = true;
	private bool bInit = true;
	private readonly Dictionary<Control, string> myControls = [];
	private readonly Dictionary<Control, string> myEditControls = [];
	//private readonly List<Control>
	//	myControls = [];            // All persistent interactive controls (Generator)
	//private readonly List<string>
	//	myTips = [];                // Locale codes (Generator)
	//private readonly List<Control>
	//	myEditControls = [];        // All persistent interactive controls (Editor)
	//private readonly List<string>
	//	myEditTips = [];            // Locale codes (Editor)
	private readonly Dictionary<Control, bool>
		myControlsEnabled = [];     // Memory of which controls were enabled after we disabled all of them temporairly, to recover that alter
	internal readonly FractalGenerator
		generator;                  // The core of the app, the generator the generates the fractal animations
	private CancellationTokenSource
		xCancel = new(),
		aCancel = new();            // Cancellation Token Sources
	private Task
#if NULLABLE
?
#endif
		xTask, aTask;					// Export Task / Abort Task
	private bool queueAbort;        // Generator abortion queued
	private short queueReset;       // Counting time until generator Restart
	private int isGifReady;
	private bool isPngsSaved = false;
	private readonly ImportForm importForm;
	internal bool Working = false;
	internal List<Schedule> Scheduled { get; private set; } = [];

	// Settings
	private readonly GeneratorForm
#if NULLABLE
? 
#endif
		fromGen;
	private bool editorVisible = false;
	private bool animated = true;   // Animating preview or paused? (default animating)
	private bool
		modifySettings = true;      // Allows for modifying settings without it triggering Aborts and Generates
	private short width = -1, height = -1;
	private ushort abortDelay = 500; // Set time to restart generator
	private short restartTimer;
	private string gifPath = "";    // Gif export path name
	private string mp4Path = "";    // Mp4 export path name
	private Fractal toSave = Fractals[0];
	private int prevResSelect = 0;

	// Display Variables
	private readonly DoubleBufferedPanel
		screenPanel;                // Display panel
	private Bitmap
#if NULLABLE
? 
#endif
		currentBitmap;              // Displayed Bitmap
	private int currentBitmapIndex; // Play frame index
	private int fx, fy;             // Memory of window size
	private int controlTabIndex = 0;// Iterator for tabIndexes - to make sure all the controls tab in the correct order even as I add new ones in the middle
	private int pointTabIndex;      // Iterator for tabIndexes for dynamically generated controls in editor
	private bool notify = true, notifyExp;
	private bool cacheLabelUpdate = false;

	// Editor
	private readonly List<(TextBox, TextBox, TextBox, TextBox, Button)>
		lineList = [];           // x y angle color delete : Point row buttons/textboxes in editor
	private readonly List<Button>
		lineListSwitch = [];     // switch: Switch buttons in editor

	//private FractalGenerator.GenerationType
	//	memGenerate;                // Selected gen type before wsithing to editor, to recover when switching back
	//private FractalGenerator.PngType
	//	memPng;
	//private FractalGenerator.GifType
	//	memGif;
	//private ushort memBlur, memAbort;
	//private float memBloom;
	//private double memDefaultHue;
	private bool performHash;
	private bool previewMode = true;

	// Config
	private ushort fileMask = 0;
	internal uint Index = 0;
	internal string statusText = "", infoText = "", tasksText = "";
	private bool frameLabelUpdate = true;
	//private string loadedBatch;
	//private string[] runningBatch;
	//private int batchIndex;
	//private string batchFolder;

	#endregion

	#region Core
	private void StartGenerate() {
		generator.StartGenerate();
		SwitchChildColor();
	}
	private void UpdateBitmap(Bitmap
#if NULLABLE
?
#endif
		bitmap) {
		if (currentBitmap == bitmap || bitmap == null)
			return;
		// Update the display with the bitmap when it's not loaded
		currentBitmap = bitmap;
		if (!Visible)
			return;
		screenPanel?.Invalidate();
	}
	private void UpdatePreview() {
		if (!Visible)
			return;
		Monitor.Enter(this);
		try {
			var bitmapsFinished = generator.GetBitmapsFinished();
			// Fetch a bitmap to display						
			UpdateBitmap(bitmapsFinished > 0
				? generator.GetBitmap(currentBitmapIndex =
					(animated ? currentBitmapIndex + 1 : currentBitmapIndex) % bitmapsFinished) // Make sure the index is in range
				: generator.GetPreviewBitmap() // Try preview bitmap if none of the main ones are generated yet
			);
			frameLabelUpdate = true;
		} finally { Monitor.Exit(this); }
	}
	private void UpdateCache() {
		cacheLabelUpdate = true;
	}
	private void SetupEditControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		myEditControls.Add(control, tip);
		toolTips.SetToolTip(control, L(tip));
		control.TabIndex = ++pointTabIndex;
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
			SuspendLayout();
			FractalGenerator.WarmUpShaders();

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
			//filePostfix = new string[fileSelect.Items.Count];
			for (int i = fileSelect.Items.Count; 0 <= --i; filePostfix.Add(fileSelect.Items[i]?.ToString() ?? "")) { }
			fileMask = (ushort)((1 << (filePostfix.Count - 1)) - 1);
			FillSelectFile();

			colorSelect.DrawItem += BitmaskComboBox_DrawItem;
			angleSelect.DrawItem += BitmaskComboBox_DrawItem;
			fileSelect.DrawItem += BitmaskComboBox_DrawItem;
			colorSelect.DrawMode = angleSelect.DrawMode = fileSelect.DrawMode = DrawMode.OwnerDrawFixed;


			resSelect.Items.Clear();
			resSelect.Items.AddRange([.. ResolutionSelection]);
			paletteSelect.Items.Clear();
			paletteSelect.Items.AddRange([.. PaletteSelection]);

			// Init the generator
			SetWorking(false);
			fractalSelect.Items.Clear();
			foreach (var i in Fractals)
				fractalSelect.Items.AddRange([i.Name]);
			generator.SelectedFractal = -1;
			//generator.RestartGif = false;
			generator.UpdatePreview += UpdatePreview;
			generator.UpdateCache += UpdateCache;

			myControls.Clear();
			//myTips.Clear();
			// Setup interactable controls (tooltips + tabIndex):

			// Editor Bottom
			SetupControl(sizeBox, "sizeBox");
			SetupControl(maxBox, "maxBox");
			SetupControl(minBox, "minBox");
			SetupControl(cutBox, "cutBox");
			SetupControl(angleBox, "angleBox");
			SetupControl(addAngleButton, "addAngleButton");
			SetupControl(removeAngleButton, "removeAngleButton");
			SetupControl(colorBox, "colorBox");
			SetupControl(addColorButton, "addColorButton");
			SetupControl(removeColorButton, "removeColorButton");
			SetupControl(addCut, "addCut");
			SetupControl(removeCutButton, "removeCutButton");

			// Toolbar - Generator
			SetupControl(prevButton, "prevButton");
			SetupControl(frameBox, "frameBox");
			SetupControl(nextButton, "nextButton");
			SetupControl(animateButton, "animateButton");
			SetupControl(restartButton, "restartButton");
			SetupControl(hideButton, "hideButton");

			// Toolbar - Editor
			SetupControl(loadButton, "loadButton");
			SetupControl(saveButton, "saveButton");
			SetupControl(preButton, "preButton");

			// Popup Top
			SetupControl(fractalSelect, "fractalSelect");
			SetupControl(angleSelect, "angleSelect");
			SetupControl(colorSelect, "colorSelect");
			SetupControl(cutSelect, "cutSelect");
			SetupControl(cutparamBox, "cutparamBox");
			SetupControl(resX, "resX");
			SetupControl(resY, "resY");
			SetupControl(removeResolution, "removeResolution");
			SetupControl(addResolution, "addResolution");
			SetupControl(resSelect, "resSelect");
			SetupControl(paletteSelect, "paletteSelect");
			SetupControl(removePalette, "removePalette");
			SetupControl(addPalette, "addPalette");
			SetupControl(defaultHue, "defaultHue");

			// Popup Panel - Generator
			SetupControl(encodePngSelect, "encodePngSelect");
			SetupControl(encodeGifSelect, "encodeGifSelect");
			SetupControl(voidSelect, "voidSelect");
			SetupControl(drawSelect, "drawSelect");
			SetupControl(ditherBox, "ditherBox");
			SetupControl(periodBox, "periodBox");
			SetupControl(periodMultiplierBox, "periodMultiplierBox");
			SetupControl(zoomSelect, "zoomSelect");
			SetupControl(defaultZoomBox, "defaultZoomBox");
			SetupControl(hueSelect, "hueSelect");
			SetupControl(hueSpeedBox, "hueSpeedBox");
			SetupControl(spinSelect, "spinSelect");
			SetupControl(defaultAngleBox, "defaultAngleBox");
			SetupControl(spinSpeedBox, "spinSpeedBox");
			SetupControl(ambBox, "ambBox");
			SetupControl(noiseBox, "noiseBox");
			SetupControl(voidBox, "voidBox");
			SetupControl(detailBox, "detailBox");
			SetupControl(saturateBox, "saturateBox");
			SetupControl(brightnessBox, "brightnessBox");
			SetupControl(bloomBox, "bloomBox");
			SetupControl(blurBox, "blurBox");
			SetupControl(zoomChildBox, "zoomChildBox");
			SetupControl(abortBox, "abortBox");
			SetupControl(timingSelect, "timingSelect");
			SetupControl(timingBox, "timingBox");
			SetupControl(parallelTypeSelect, "parallelTypeSelect");
			SetupControl(stripeBox, "stripeBox");
			SetupControl(binBox, "binBox");
			SetupControl(l2Box, "l2Box");

			// Popup Bottom - Generator
			SetupControl(nameBox, "nameBox");
			SetupControl(generationSelect, "generationSelect");
			SetupControl(exportButton, "exportButton");
			SetupControl(exportSelect, "exportSelect");
			SetupControl(tasksButton, "tasksTip");
			SetupControl(fileSelect, "fileSelect");

			// Debugs
			SetupControl(debugBox, "debugBox");
			SetupControl(animBox, "debugAnimBox");
			SetupControl(pngBox, "debugPngBox");
			SetupControl(gifBox, "debugGifBox");

			// Editor Point List
			SetupControl(addPointButton, "addPoint");

			UpdateLocale();

			// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restart the generator over and over
			generator.SelectedFps = 60;
			generator.SelectedDelay = 100 / 60;
			FillPalette();
			voidSelect.SelectedIndex = 3;
			drawSelect.SelectedIndex = 1;
			fractalSelect.SelectedIndex = 0;
			resSelect.SelectedIndex = prevResSelect = 0;
			zoomSelect.SelectedIndex = 3;
			generator.SelectedCacheOpt = CacheChecked;//CacheBox();
			StripeBox();
			BinBox();
			L2Box();
			AbortBox();
			DitherBox();
			PeriodBox();
			PeriodMultiplierBox();
			ParallelTypeSelect();
			TimingBox();
			DefaultZoom();
			SpinSpeedBox();
			HueSpeedBox();
			DefaultHue();
			AmbBox();
			NoiseBox();
			BloomBox();
			BlurBox();
			SaturateBox();
			BrightnessBox();
			VoidBox();
			ZoomChildBox();
			UpdateAnimateLocale();
			encodePngSelect.SelectedIndex = encodeGifSelect.SelectedIndex = parallelTypeSelect.SelectedIndex = 0;
			generationSelect.SelectedIndex = 0; // default: OnlyImage
			timingSelect.SelectedIndex = spinSelect.SelectedIndex = hueSelect.SelectedIndex = paletteSelect.SelectedIndex = 1; // default: first non-random
			exportSelect.SelectedIndex = 2;

			SetupFractal();

			// try to restore the last closed settings and init the editor
			//editorPanel.Visible = false;
			pointTabIndex = controlTabIndex;
			editorPanel.Location = generatorPanel.Location;

			UpdateRangeLocale();

			if (fromGen == null)
				LoadSettings();
			else {
				LoadParams(fromGen.ExportSettings());
				FinishLoading();
			}
			FillListEntries();

			// Start the generator
			//runningBatch = [];
			modifySettings = false;
			TryResize(out var _, out var _);
			ResizeAll();

			aTask = xTask = null;
			StartGenerate();
			bInit = false;

			// Load all extra fractal files
			var appDirectory = AppDomain.CurrentDomain.BaseDirectory; // Get the app's directory
			const string searchPattern = "*.fractal"; // Change to your desired file type
			var files = Directory.GetFiles(appDirectory, searchPattern);
			if(gens != null)
				foreach (var file in files)
					_ = LoadFractal(gens, file, null);

			// List all cutFunction to add in the editor
			addCut.Items.Add(L("selectCutFunctionAdd"));
			foreach (var c in Fractal.CutFunctions)
				addCut.Items.Add(c.Item1);

			SetFileMask();
			ResumeLayout(false);
			PerformLayout();
			return;
		}
		#endregion

		#region Size
		/*void SetMinimumSize() {
			// bw = Width - ClientWidth = 16
			// bh = Height - ClientHeight = 39
			//const int bw = 16, bh = 39; // Have to do this because for some reason ClientSize was returning bullshit values all of a sudden
			int bw = Width - ClientRectangle.Width, bh = Height - ClientRectangle.Height;
			//debugLabel.Text = debugLabel.Text + " " + MinimumSize.Height.ToString();
		}*/
		void ResizeScreen() {
			//const int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			int bw = Width - ClientRectangle.Width, bh = Height - ClientRectangle.Height;
			var screenHeight = Math.Max(height, Math.Min(Height - bh, (Width - bw) * height / width));
			screenPanel.SetBounds(/*305,4*/0, 0, screenHeight * width / height, screenHeight);
			//helpPanel.SetBounds(305, 4, Width - bw - 314, Height - bh - 8);
			screenPanel?.Invalidate();
		}
		void WindowSizeRefresh() {
			if (fx == ClientRectangle.Width && fy == ClientRectangle.Height)
				return;
			// User has manually resized the window - stretch the display
			ResizeScreen();
			//SetMinimumSize();
			//const int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			//int bw = Width - ClientRectangle.Width, bh = Height - ClientRectangle.Height;
			SetClientSizeCore(
				Math.Max(ClientRectangle.Width,/*Width - bw, 314 +*/ Math.Max(screenPanel.Width, width)),
				Math.Max(ClientRectangle.Height,/*Height - bh, 8 +*/ Math.Max(screenPanel.Height, height))
			);
			SizeAdapt();
		}
		void ResizeAll() {
			generator.SelectedWidth = (ushort)width;
			generator.SelectedHeight = (ushort)height;
			generator.SetMaxIterations();
			// Update the size of the window and display
			//SetMinimumSize();
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
			// StartGenerate(); should be called after
			//gifButton.Enabled = false;
			isGifReady = 0;
			currentBitmapIndex = 0;
			frameLabelUpdate = true;
			SizeAdapt();
		}
		void SizeAdapt() {
			fx = ClientRectangle.Width;
			fy = ClientRectangle.Height;
		}
		#endregion

		//return;

		if (bInit)
			Init();
		if (generator.DebugTasks || generator.DebugAnim || generator.DebugPng || generator.DebugGif) {
			debugLabel.Text = generator.DebugString;
			//SetMinimumSize();
		}

		//return;

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

			//runningBatch = [];
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
				StartGenerate();
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
				foreach (var c in myControls) 
					c.Key.Enabled = myControlsEnabled[c.Key];

				// restart generator
				performHash = false;
				QueueReset();
			} else {
				SetupFractal();
				ResizeAll();
				StartGenerate();
			}
		}
		if (restartTimer > 0 && (restartTimer -= (short)timer.Interval) <= 0)
			ResetRestart();
		// Fetch the state of generated bitmaps
		int bitmapsFinished = generator.GetBitmapsFinished(), bitmapsTotal = generator.GetFrames();
		if (bitmapsTotal <= 0)
			return;
		if (bitmapsFinished < bitmapsTotal) {
			BackColor = Background;
			notify = true;
		}
		if (bitmapsFinished == bitmapsTotal && notify && !generator.IsCancelRequested()) {
			BackColor = Color.FromArgb(128, 128, 32);
			notify = false;
			isPngsSaved = false;
			FinishedTask();
			//	or FractalGenerator.GenerationType.AllSeedsPng;
			// If we're runnig batches, export immediately
				/*if (runningBatch.Length > 0) {

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
				}*/
		}
		if (notifyExp) {
			// TODO rewrite this into the GeneratorsForm scheduling somehow?
			notifyExp = false;
			FinishedTask();
		}
		if (scheduler.Running)
			scheduler.UpdateRunning();
		// Only Allow GIF Export when generation is finished
		//string v = generator.selectGenerationType == FractalGenerator.GenerationType.Mp4 ? "Mp4" : "Gif";
		isGifReady = generator.IsGifReady();
		UpdateExportLocale();

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
			UpdateCacheLocale();
			cacheLabelUpdate = false;
		}
		if (frameLabelUpdate) {
			frameLabel.Text = currentBitmapIndex.ToString();
			frameLabelUpdate = false;
		}
		// Info text refresh
		infoText = " / " + bitmapsTotal;
		if (bitmapsFinished < bitmapsTotal) {
			statusLabel.Tag = Working ? "generating" : "notWorking";
			infoText = bitmapsFinished + infoText;
		} else {
			if (xTask != null) {
				statusLabel.Tag = "saving";
				infoText = generator.GetCompleted() + infoText;
				//infoText = generator.GetPngFinished() + infoText;
			} else {
				statusLabel.Tag = generator.SelectedGenerationType == FractalGenerator.GenerationType.OnlyImage ? "generationSelect0" : "finished";
				infoText = currentBitmapIndex + infoText;
			}
		}
		infoLabel.Text = infoText;
		UpdateInfoTextLocale();
		//gifButton.Text = gTask == null ? "Save GIF" : "Saving";
	}
	internal string ExportSettings() {
		var f = generator.GetFractal();
		return "fractal|" + fractalSelect.Text + "|path|" + f.Path
			+ "|preview|" + (previewMode ? 1 : 0) //+ "|edit|" + (editorPanel.Visible ? 1 : 0)
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
			+ "|parallel|" + parallelTypeSelect.SelectedIndex + "|cachestripe|" + stripeBox.Text + "|cachebin|" + binBox.Text + "|cachesize|" + l2Box.Text
			+ "|gpuvoid|" + voidSelect.SelectedIndex + "|gpudraw|" + drawSelect.SelectedIndex
			+ "|png|" + encodePngSelect.SelectedIndex + "|gif|" + encodeGifSelect.SelectedIndex + "|gen|" + generationSelect.SelectedIndex
			+ "|ani|" + (animated ? 1 : 0) + "|exp|" + exportSelect.SelectedIndex + "|file|" + fileMask + "|name|" + MyName
			+ "|shown|" + (shown ? "1" : "0");// + "|working|" + (Working ? "1" : "0");
	}
	private void SaveSettings() 
		=> File.WriteAllText(Path.Combine(GetGensSaveDir(), Index.ToString() + GeneratorsForm.genExtension), ExportSettings());
	private void LoadSettings() {
		//gifButton.Enabled = false;
		var name = Index.ToString() + GeneratorsForm.genExtension;
		var file = Path.Combine(GetGensSaveDir(), name);
		if (!File.Exists(file) && File.Exists("settings.txt"))
			File.Copy("settings.txt", file);
		if (File.Exists(file))
			LoadParams(File.ReadAllText(file));
		FinishLoading();
	}
	internal void FinishLoading() {
		isGifReady = 0;
		if (generator.SelectedEditorMode = editorVisible) {

			/*memPng = (FractalGenerator.PngType)encodePngSelect.SelectedIndex;
			memGif = (FractalGenerator.GifType)encodeGifSelect.SelectedIndex;
			memGenerate = (FractalGenerator.GenerationType)generationSelect.SelectedIndex;
			memBlur = generator.SelectedBlur;
			memBloom = generator.SelectedBloom;*/
			//mem_hue = (short)hueSelect.SelectedIndex;

			generator.SelectedGenerationType = FractalGenerator.GenerationType.Animation;
			generator.SelectedPngType = FractalGenerator.PngType.No;
			generator.SelectedGifType = FractalGenerator.GifType.No;
			generator.SelectedBloom = generator.SelectedBlur = 0;
			//generator.selectHue = generator.selectDefaultHue = 0;
			SetPreviewMode(previewMode);
			abortDelay = 10;

			//if (short.TryParse(defaultHue.Text, out var n))
			//	memDefaultHue = n;
			/*if (ushort.TryParse(abortBox.Text, out var n))
				memAbort = n;*/
		} else {
			generator.SelectedPreviewMode = false;
		}
		SetupFractal();
	}
	internal void LoadParams(string _params) {
		var s = _params.Split('|');
		for (var i = 0; i < s.Length - 1; i += 2) {
			var v = s[i + 1];
			var p = int.TryParse(v, out var n);
			if(gens != null)
			switch (s[i]) {
				case "path": if (v != "" && File.Exists(v)) _ = LoadFractal(gens, v, this); break;
				case "fractal": if (fractalSelect.Items.Contains(v)) fractalSelect.SelectedItem = v; break;
				case "preview": if (p) SetPreviewMode(n > 0); break;
				//case "edit": if (p) SetEditor(n > 0); break;
				case "angle":
					if (p) generator.SelectedChildAngle = (short)Math.Min(angleSelect.Items.Count - 2, n); {
						UpdateAngleSelected();
					}
					break;
				case "color":
					if (p) generator.SelectedChildColor = (short)Math.Min(colorSelect.Items.Count - 2, n); {
						UpdateColorSelected();
					}
					break;
				case "angles":
					if (p)
						generator.SelectedChildAngles = (ulong)n;
					else {
						var c = v.Split('|');
						var ca = generator.GetFractal().ChildColor;
						ulong un = 0;
						for (int ic = ca.Count; 0 <= --ic; un |= (ulong)(c.Contains(ca[ic].Item1) ? 1 : 0) << ic) { }
						generator.SelectedChildColors = un;
					}
					FillSelectAngles();
					break;
				case "colors":
					if (p)
						generator.SelectedChildColors = (ulong)n;
					else {
						var c = v.Split('|');
						var ca = generator.GetFractal().ChildColor;
						ulong un = 0;
						for (int ic = ca.Count; 0 <= --ic; un |= (ulong)(c.Contains(ca[ic].Item1) ? 1 : 0) << ic) { }
						generator.SelectedChildColors = un;
					}
					FillSelectColors();
					break;
				case "cut": if (p) cutSelect.SelectedIndex = Math.Min(cutSelect.Items.Count - 1, n); break;
				case "seed": cutparamBox.Text = v; break;
				case "w": if (p) resX.Text = v; break;
				case "h": if (p) resY.Text = v; break;
				case "res":
					if (p)
						resSelect.SelectedIndex = prevResSelect = Math.Min(resSelect.Items.Count - 1, n);
					else
						prevResSelect = resSelect.Items.IndexOf(resSelect.SelectedItem = v);
					break;
				case "paletteSelect":
					_ = FillPalette();
					if (p)
						paletteSelect.SelectedIndex = Math.Min(paletteSelect.Items.Count - 1, n);
					else
						paletteSelect.SelectedItem = v;
					break;
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
				case "timing":
					timingSelect.SelectedIndex = Math.Min(parallelTypeSelect.Items.Count - 1, n);
					TimingSelect(); break;
				case "parallel": parallelTypeSelect.SelectedIndex = Math.Min(parallelTypeSelect.Items.Count - 1, n); break;
				case "cachestripe": if (p) stripeBox.Text = v; break;
				case "cachebin": if (p) binBox.Text = v; break;
				case "cachesize": if (p) l2Box.Text = v; break;
				case "gpuvoid": if (p) voidSelect.SelectedIndex = Math.Min(voidSelect.Items.Count - 1, n); break;
				case "gpudraw": if (p) drawSelect.SelectedIndex = Math.Min(drawSelect.Items.Count - 1, n); break;
				case "png": if (p) encodePngSelect.SelectedIndex = Math.Min(encodePngSelect.Items.Count - 1, n); break;
				case "gif": if (p) encodeGifSelect.SelectedIndex = Math.Min(encodeGifSelect.Items.Count - 1, n); break;
				case "gen": if (p) generationSelect.SelectedIndex = Math.Min(generationSelect.Items.Count - 1, n); break;
				case "ani": if (p) animated = n <= 0; AnimateButton(); break;
				case "exp": if (p) exportSelect.SelectedIndex = Math.Min(exportSelect.Items.Count - 1, n); break;
				case "file":
					if (p) fileMask = (ushort)n;
					if (p) {
						fileMask = (ushort)n;
						FillSelectFile();
					}
					break;
				case "name": MyName = v; break;
				case "shown": if (p) shown = n > 0; break;
				//case "working": if (p) _ = SetWorking(n > 0); break;
			}
		}
		_ = BitmaskComboBox_MinimumSelection();
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
	private bool TryResize(out short ow, out short oh) {
		//previewMode = !previewBox.Checked;
		ow = width;
		oh = height;
		width = 8;
		height = 8;
		if (!short.TryParse(resX.Text, out width) || width <= 8)
			width = 8;
		if (!short.TryParse(resY.Text, out height) || height <= 8)
			height = 8;
		UpdateResolutionLocale();
		//previewBox.Text = "Resolution: " + width.ToString() + "x" + height.ToString();
		//if (previewMode)
		//	width = height = 80;
		var rxy = resSelect.SelectedIndex is 1 or < 0
			? (resSelect.Items[1]?.ToString() ?? "").Split(':')[1].Split('x')
			: (resSelect.Items[resSelect.SelectedIndex]?.ToString() ?? "").Split('x');
		if (!short.TryParse(rxy[0], out width) || width <= 8)
			width = 8;
		if (!short.TryParse(rxy[1], out height) || height <= 8)
			height = 8;
		if (generator.SelectedWidth != width || generator.SelectedHeight != height) {
			// Number of threads can change the maximum of these:
			BloomBox();
			StripeBox();
			return true;
		}
		return false;
	}
	private void GeneratorForm_FormClosing(object sender, FormClosingEventArgs e) {
		e.Cancel = true;
		Hide();
	}
	internal bool TryClose(bool scheduled = false) {
		void ResetSchedule() {
			if (scheduled) {
				var result = MessageBox.Show(L("moveCloseTask"), L("moveClose"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
					AddScheduled(new(ScheduledTask.Close, ""));
			}
		}
		if (xTask != null) {
			var result = MessageBox.Show(L("exportStillSaving"), L("confirmExit"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			// Cancel closing if the user clicks "No"
			ResetSchedule();
			return result == DialogResult.No;
		}
		if (Scheduled.Count > 0) {
			var result = MessageBox.Show(L("resetSchedule"), L("confirmResetSchedule"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			// Cancel close if the user clicks "Yes"
			ResetSchedule();
			if (result == DialogResult.Yes)
				return false;
		}
		if (isGifReady > 80) {
			var result = MessageBox.Show(L("gifAvailable"), L("confirmSave"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			// Save if the user clicks "Yes"
			if (result == DialogResult.Yes) {
				saveGif.DefaultExt = "gif";
				saveGif.Filter = ("gifVideo") + " (*.gif)|*.gif|MP4 video (*.mp4)|*.mp4";
				_ = saveGif.ShowDialog();
				ResetSchedule();
				return true;
			}
		}
		if (!isPngsSaved && generator.SelectedPngType == FractalGenerator.PngType.Yes && generator.GetBitmapsFinished() >= generator.GetFrames()) {
			var result = MessageBox.Show(L("pngsAvailable"), L("confirmSave"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes) { // Save if the user clicks "Yes"
				saveMp4.DefaultExt = "mp4";
				saveMp4.Filter = L("mp4Video") + " (*.mp4)|*.mp4|PNG video (*.png)|*.png";
				_ = saveMp4.ShowDialog();
				ResetSchedule();
				return true;
			}
		}

		var saved = false;
		foreach (var f in Fractals) {
			if (!f.Edit)
				continue;
			var result = MessageBox.Show(
				L("fractal") + " " + f.Name + " " + L("fractalEdited"),
				L("confirmExit"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
					return true;
			}
			return false;
		}
		return saved;
	}
	internal void PerformClose() {

		aCancel?.Cancel();
		xCancel?.Cancel();
		aTask?.Wait();
		xTask?.Wait();
		Abort();
		SaveSettings();
		generator.CleanupTempFiles();
		// TODO also actually Close me - testif this dones't get cancelled
		Close();
	}
	private void Abort() {
		queueAbort = false;
		// Cancel FractalGenerator threads
		generator.RequestCancel();
		isGifReady = 0;
		frameLabelUpdate = true;
		currentBitmapIndex = 0;
		aTask = null;
	}
	internal bool QueueReset() => TryQueueReset(true, true);
	/*internal bool QueueReset<T>(ref T cancel, T to) where T : struct, IComparable<T>, IParsable<T> {
		if (TryQueueReset(true, true))
			return true;
		cancel = to;
		return false;
	}*/
	internal bool QueueResetText<T>(TextBox to, T cancel) where T : notnull {
		if (TryQueueReset(true, true))
			return true;
		var m = modifySettings;
		modifySettings = true;
		to.Text = cancel.ToString();
		modifySettings = m;
		return false;
	}
	internal bool QueueResetCombo<T>(ComboBox to, int cancel) where T : notnull {
		if (TryQueueReset(true, true))
			return true;
		var m = modifySettings;
		modifySettings = true;
		to.SelectedIndex = cancel;
		modifySettings = m;
		return false;
	}
	internal bool QueueResetCheck(CheckBox to) {
		if (TryQueueReset(true, true))
			return true;
		var m = modifySettings;
		modifySettings = true;
		to.Checked = !to.Checked;
		modifySettings = m;
		return false;
	}
	private bool TryQueueReset(bool allow = true, bool returnTrue = false) {
		if (modifySettings || !allow || bInit)
			return returnTrue;
		if (queueReset <= 0) {
			if (Scheduled.Count > 0) {
				var result = MessageBox.Show(L("resetSchedule"), L("confirmResetSchedule"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				// Cancel reset if the user clicks "Yes"
				if (result == DialogResult.Yes)
					return false;
			}
			if (isGifReady > 80
				&& xTask == null) {
				using AvailableQuestion mes = new(L(""), L("gifAvailable") + L("gifAvailableAbort"),
					L("selectDestination"), L("dontSave"), L("abortSave"), L("cancelSave"));
				switch (mes.ShowDialog()) {
					case DialogResult.Yes:
						saveGif.DefaultExt = "gif";
						saveGif.Filter = L("gifVideo") + " (*.gif)|*.gif|MP4 video (*.mp4)|*.mp4";
						_ = saveGif.ShowDialog();
						return false;
					case DialogResult.Cancel:
						return false;
					case DialogResult.Abort:
						encodeGifSelect.SelectedIndex = 0;
						break;
					default:
						break;
				}
			}
			if (!isPngsSaved
				&& generator.SelectedPngType == FractalGenerator.PngType.Yes
				&& generator.GetBitmapsFinished() >= generator.GetFrames()
				&& xTask == null) {
				using AvailableQuestion mes = new(L("saveMp4Pngs"), L("pngsAvailable") + L("pngsAvailableAbort"),
					L("selectDestination"), L("dontSave"), L("abortSave"), L("cancelSave"));
				switch (mes.ShowDialog()) {
					case DialogResult.Yes:
						saveMp4.DefaultExt = "mp4";
						saveMp4.Filter = L("mp4Video") + " (*.mp4)|*.mp4|PNG video (*.png)|*.png";
						_ = saveMp4.ShowDialog();
						return false;
					case DialogResult.Cancel:
						return false;
					case DialogResult.Abort:
						encodePngSelect.SelectedIndex = 0;
						break;
					default:
						break;
				}
			}

			//gifButton.Enabled = false;
			isGifReady = 0;
			frameLabelUpdate = true;
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
		restartButton.Tag = 0;
		UpdateRestartLocale();
	}
	private bool TasksNotRunning() => aTask == null && xTask == null;
	/*private void NextBatch() {
		if (batchIndex >= runningBatch.Length) {
			// All batches complete, unlock the controls, and stop the batching
			foreach (var c in myControls) 
					c.Key.Enabled = myControlsEnabled[c.Key];
			return;
		}
		isGifReady = 0;
		LoadParams(runningBatch[batchIndex].Split('|'));
		generator.SelectedPreviewMode = false;
		SetEditor(false);
		QueueReset();
		++batchIndex;
	}*/
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
		_ = QueueReset();

		return;

		void FillSelects() {
			var f = generator.GetFractal();
			// Fill angle children definitions combobox
			angleSelect.Items.Clear();
			UpdateAngleSelected();//angleSelect.Items.Add(L("selected") + ": " + f.ChildAngle[generator.SelectedChildAngle].Item1);
			foreach (var (name, _) in f.ChildAngle)
				_ = angleSelect.Items.Add(charOff + name);
			angleSelect.SelectedIndex = 0;
			// Fill color children definitions combobox
			colorSelect.Items.Clear();
			UpdateColorSelected();//colorSelect.Items.Add(L("selected") + ": " + f.ChildColor[generator.SelectedChildColor].Item1);
			foreach (var (name, _) in f.ChildColor)
				_ = colorSelect.Items.Add(charOff + name);
			colorSelect.SelectedIndex = 0;
			// Fill cutFunction definitions combobox
			cutSelect.Items.Clear();
			var cf = f.ChildCutFunction;
			if (!CutSelectEnabled(cf))
				return;
			foreach (var (index, _) in cf)
				_ = cutSelect.Items.Add(Fractal.CutFunctions[index].Item1);
			cutSelect.SelectedIndex = 0;
		}
	}
	private void SetupFractal() {
		generator.SetupFractal();
		if (!modifySettings) {
			modifySettings = true;
			SetupParallel();
			//Parallel_Changed(null, null);
			DetailBox();
			modifySettings = false;
		} else {
			SetupParallel();
			//Parallel_Changed(null, null);
			DetailBox();
		}
		generator.SetupCutFunction();
	}
	private void GetValidZoomChildren() {
		generator.GetValidZoomChildren();
		zoomChildBox.Text = "0";
	}
	internal void SetupParallel() {
		generator.SelectedMaxTasks = (short)(Tasks > 1 ? Tasks : FractalGenerator.MinTasks);
		generator.SelectThreadingDepth();
	}
	private bool CutSelectEnabled(List<(int, int[])> cf)
	=> cutSelect.Enabled = cf is { Count: > 0 };
	// Query the number of seeds from the CutFunction
	private bool CutSeedBoxEnabled(Fractal.CutFunction
#if NULLABLE
?
#endif
	cf) => cutparamBox.Enabled = 0 < (generator.CutSeed_Max = (int)(cf == null || cf(0, -1, generator.GetFractal()) <= 0 ? 0 : (cf(0, 1 - (1 << 30), generator.GetFractal()) + 1) / cf(0, -1, generator.GetFractal())));
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
		_ = paletteSelect.Items.Add(L("random"));
		for (var i = 0; i < Colors.Count; ++i)
			r = paletteSelect.Items.Add(Colors[i].Item1);
		return r;
	}
	private void MoveFrame(int move) {
		animated = true; AnimateButton();
		var b = generator.GetBitmapsFinished();
		(currentBitmapIndex = b == 0 ? -1 : (b + move) % b).ToString();
		frameLabelUpdate = modifySettings = true;
		frameBox.Text = currentBitmapIndex.ToString();
		modifySettings = false;
	}
	private void UpdateAnimateLocale() {
		animateButton.Text = L(animated ? "▶︎" : "||");
		animateButton.BackColor = animated ? Color.FromArgb(128, 255, 128) : Color.FromArgb(255, 128, 128);
	}
#endregion

	#region Input_ToolGen

	private void PrevButton_Click(object sender, EventArgs e) => MoveFrame(currentBitmapIndex - 1);
	private void FrameBox_TextChanged(object sender, EventArgs e) => MoveFrame(int.TryParse(frameBox.Text, out int f) ? f : 0);
	private void NextButton_Click(object sender, EventArgs e) => MoveFrame(currentBitmapIndex + 1);
	private void AnimateButton_Click(object sender, EventArgs e) => AnimateButton();
	private void AnimateButton() {
		// Toggle animation if GUI not hidden
		if (shown) {
			//infoLabel.Visible = animated = !(frameBox.Visible = animated);
			frameLabel.Visible = animated = !(frameBox.Visible = animated);
			UpdateAnimateLocale();
		} else shown = true; // unhide GUI if hidden
	}
	private void RestartButton_Click(object sender, EventArgs e) {
		if (((int?)restartButton.Tag ?? 0) == 0) {
			restartButton.Tag = 1;
			UpdateRestartLocale();
			restartTimer = 2000;
			return;
		}
		restartTimer = 0;
		restartButton.Tag = "0";
		UpdateRestartLocale();
		//restartButton.Enabled = false;
		_ = QueueReset();
	}
	private void HideButton_Click(object sender, EventArgs e) => shown = false;

	private static void ComboBox_MouseWheel(object sender, MouseEventArgs e) {
		((HandledMouseEventArgs)e).Handled = true;
	}
	#endregion

	#region Input_Gen
	private void FractalSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (QueueResetCombo<int>(fractalSelect, generator.SelectedFractal) && generator.SelectFractal((short)Math.Max(0, fractalSelect.SelectedIndex)))
			return;
		// Fractal is different - load it, change the setting and restart generation
		FillListEntries();
		SetupSelects();
		_ = BitmaskComboBox_MinimumSelection();
	}
	private void FractalSelect_TextUpdate(object sender, EventArgs e) {
		if (fractalSelect.Items.Contains(fractalSelect.Text))
			fractalSelect.SelectedIndex = fractalSelect.Items.IndexOf(fractalSelect.Text);
		else if (File.Exists(fractalSelect.Text + ".fractal"))
			_ = LoadFractal(gens, fractalSelect.Text + ".fractal", this);
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
		string text = combo.Items[e.Index]?.ToString() ?? "";
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
		if (MaskApply((short)Math.Max(-1, angleSelect.SelectedIndex - 1), ref generator.SelectedChildAngle, ref generator.SelectedChildAngles)
			|| QueueReset())
			return;
		// put selection back to 0, rename the selected, and change the checkmark:
		angleSelect.SelectedIndex = 0;
		UpdateAngleSelected(true);
		if (!BitmaskComboBox_MinimumSelection())
			SwitchChildAngle();
	}
	private void ColorSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (MaskApply((short)Math.Max(-1, colorSelect.SelectedIndex - 1), ref generator.SelectedChildColor, ref generator.SelectedChildColors)
			|| QueueReset())
			return;
		// put selection back to 0, rename the selected, and change the checkmark:
		colorSelect.SelectedIndex = 0;
		UpdateColorSelected(true);
		if (!BitmaskComboBox_MinimumSelection())
			SwitchChildColor();
	}
	private void CutSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (!DiffApply((short)Math.Max(0, cutSelect.SelectedIndex), ref generator.SelectedCut, out var prev)
			|| QueueResetCombo<short>(cutSelect, prev)
			) FillCutSeeds();
	}
	private void CutSeedBox_TextChanged(object sender, EventArgs e) {
		if (ParseClampReTextDiffApply(cutparamBox, ref generator.SelectedCutSeed, -1, generator.GetMaxCutSeed(), out var prev)
			|| QueueResetText(cutparamBox, prev))
			return;
		GetValidZoomChildren();
	}
	private void Resolution_Changed(object sender, EventArgs e) {
		if (TryResize(out var _, out var _)) {
			if (QueueResetCombo<int>(resSelect, prevResSelect)) return;
			prevResSelect = resSelect.SelectedIndex;
		}
	}
	private void ResolutionXY_Changed(object sender, EventArgs e) {
		if (TryResize(out var ow, out var oh)) {
			if (QueueReset()) return;
			var m = modifySettings;
			modifySettings = true;
			resSelect.SelectedIndex = prevResSelect = 1;
			width = ow;
			height = oh;
			resX.Text = ow.ToString();
			resY.Text = oh.ToString();
			modifySettings = m;
		}
	}
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
			var c = Colors[e.Index - 1].Item2;
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

		e.Graphics.DrawString(e.Index == 0 ? L("random") : Colors[e.Index - 1].Item1, font, black, x, y);
	}
	private void PaletteSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (DiffApply((short)Math.Max(-1, paletteSelect.SelectedIndex - 1), ref generator.SelectedPaletteType, out var prev) 
			|| QueueResetCombo<int>(paletteSelect, prev))
			return;
		defaultHue.Text = "0";
		SwitchChildColor();
	}
	private void RemovePalette_Click(object sender, EventArgs e) {
		if (Colors.Count <= 1)
			return;
		Colors.RemoveAt(generator.SelectedPaletteType);
		var i = paletteSelect.SelectedIndex;
		foreach (var g in gens.Gen)
			g.Value.RefreshPalette(i);
	}
	internal void RefreshPalette(int i) {
		if (paletteSelect.SelectedIndex < i) {
			_ = FillPalette();
			return;
		}
		void updatePalette() {
			_ = FillPalette();
			paletteSelect.SelectedIndex = Math.Min(Colors.Count - 1, i);
			SwitchChildColor();
		}
		if (paletteSelect.SelectedIndex == i) {
			updatePalette();
			_ = QueueReset();
		} else {
			modifySettings = true;
			updatePalette();
			modifySettings = false;
		}
	}
	private void AddPalette_Click(object sender, EventArgs e) {
		List<Vector3> newPalette = [];
		var ok = DialogResult.OK;
		while (ok == DialogResult.OK)
			if ((ok = paletteDialog.ShowDialog()) == DialogResult.OK)
				newPalette.Add(new Float3(paletteDialog.Color.R, paletteDialog.Color.G, paletteDialog.Color.B));
		var p = new Vector3[newPalette.Count];
		for (var i = 0; i < p.Length; ++i)
			p[i] = newPalette[i];
		string name = "";
		using (var dlg = new InputDialog() { Text = L("namePalette"), Tag = L("inputPaletteTextBox") }) {
			if (dlg.ShowDialog() == DialogResult.OK)
				name = dlg.GetText();
		}
		if (name == "") {
			_ = Error("cancelled", "cancelledText");
			return;
		}
		Colors.Add((name, p));
		paletteSelect.SelectedIndex = FillPalette();
		SwitchChildColor();
	}
	private void DefaultHue_TextChanged(object sender, EventArgs e) => DefaultHue();
	private void DefaultHue() {
		if (DiffApply(ParseValue<float>(defaultHue), ref generator.SelectedDefaultHue, out var prev) 
			|| QueueResetText(defaultHue, prev)) // Save/Cancel...?
			return;
		SwitchChildColor();
	}
	#endregion

	#region Input_GenSettings
	private void DitherBox_CheckedChanged(object sender, EventArgs e) => DitherBox();
	private void DitherBox() {
		if (QueueResetCheck(ditherBox))
			return;
		UpdateDitherLocale();
		generator.SelectDithering(ditherBox.Checked);
	}
	private void PeriodBox_TextChanged(object sender, EventArgs e) => PeriodBox();
	private bool PeriodBox()
	=> ParseClampReTextDiffApply(periodBox, ref generator.SelectedPeriod, (ushort)1, (ushort)1000, out var prev)
	 || QueueResetText(periodBox, prev);
	private void PeriodMultiplierBox_TextChanged(object sender, EventArgs e) => PeriodMultiplierBox();
	private bool PeriodMultiplierBox()
		=> ParseClampReTextDiffApply(periodMultiplierBox, ref generator.SelectedPeriodMultiplier, (ushort)1, (ushort)10, out var prev)
		 || QueueResetText<ushort>(periodMultiplierBox, prev);
	private void ZoomSelect_SelectedIndexChanged(object sender, EventArgs e) => ZoomSelect();
	private bool ZoomSelect()
	=> DiffApply((short)(zoomSelect.SelectedIndex - 2), ref generator.SelectedZoom, out var prev)
	 || QueueResetCombo<int>(zoomSelect, prev);
	private void DefaultZoom_TextChanged(object sender, EventArgs e) => DefaultZoom();
	private bool DefaultZoom()
		=> ParseDiffApply(defaultZoomBox, ref generator.SelectedDefaultZoom, out var prev)
		 || QueueResetText(defaultZoomBox, prev);
	private void HueSelect_SelectedIndexChanged(object sender, EventArgs e) => HueSelect();
	private bool HueSelect()
		=> DiffApply((short)(hueSelect.SelectedIndex == 0 ? -2 : hueSelect.SelectedIndex % 3 - 1), ref generator.SelectedHue, out var prev)
		 || QueueResetCombo<int>(hueSelect, prev);

	private void HueSpeedBox_TextChanged(object sender, EventArgs e) => HueSpeedBox();
	private void HueSpeedBox() {
		var newSpeed = ParseClampReText(hueSpeedBox, (ushort)0, (ushort)255);
		if (Diff(newSpeed, generator.SelectedExtraHue))
			return;
		// hue speed is different - change the setting and if it's actually hueCycling restart generation
		if (generator.SelectedHue != 0)
			_ = Apply(newSpeed, out generator.SelectedExtraHue) || QueueReset();
		else generator.SelectedExtraHue = newSpeed;
	}
	private void SpinSelect_SelectedIndexChanged(object sender, EventArgs e) => SpinSelect();
	private bool SpinSelect()
		=> ClampDiffApply((short)(spinSelect.SelectedIndex - 2), ref generator.SelectedSpin, (short)-2, (short)3, out var prev)
		 || QueueResetCombo<short>(spinSelect, prev);

	private void DefaultAngle_TextChanged(object sender, EventArgs e) => DefaultAngle();
	private bool DefaultAngle()
		=> ParseModDiffApply(defaultAngleBox, ref generator.SelectedDefaultAngle, (ushort)0, (ushort)360, out var prev)
		 || QueueResetText<ushort>(defaultAngleBox, prev);

	private void SpinSpeedBox_TextChanged(object sender, EventArgs e) => SpinSpeedBox();
	private bool SpinSpeedBox()
		=> ParseClampReTextDiffApply(spinSpeedBox, ref generator.SelectedExtraSpin, (ushort)0, (ushort)255, out var prev)
		 || QueueResetText<ushort>(spinSpeedBox, prev);

	private void AmbBox_TextChanged(object sender, EventArgs e) => AmbBox();
	private bool AmbBox()
		=> ParseClampReTextMulDiffApply(ambBox, ref generator.SelectedAmbient, (short)-1, (short)voidAmbientMax, (short)voidAmbientMul, out var prev)
		|| QueueResetText<short>(ambBox, prev);

	private void NoiseBox_TextChanged(object sender, EventArgs e) => NoiseBox();
	private bool NoiseBox()
		=> ParseClampReTextMulDiffApply(noiseBox, ref generator.SelectedNoise, 0, voidNoiseMax, voidNoiseMul, out var prev)
		|| QueueResetText<double>(noiseBox, prev);

	private void VoidBox_TextChanged(object sender, EventArgs e) => VoidBox();
	private bool VoidBox()
		=> ParseClampReTextDiffApply(voidBox, ref generator.SelectedVoid, (ushort)0, voidScaleMax, out var prev)
		|| QueueResetText<ushort>(voidBox, prev);

	private void DetailBox_TextChanged(object sender, EventArgs e) => DetailBox();
	private void DetailBox() {
		if (!ParseClampReTextMulDiffApply(detailBox, ref generator.SelectedDetail, 0.0, detailMax, detailMul * generator.GetFractal().MinSize, out var prev)
			 || QueueResetText<double>(detailBox, prev))
			generator.SetMaxIterations();
	}
	private void SaturateBox_TextChanged(object sender, EventArgs e) => SaturateBox();
	private bool SaturateBox()
		=> ParseClampReTextMulDiffApply(saturateBox, ref generator.SelectedSaturate, 0, saturateMax, 1.0f / saturateMax, out var prev)
		|| QueueResetText<double>(saturateBox, prev);

	private void BrightnessBox_TextChanged(object sender, EventArgs e) => BrightnessBox();
	private bool BrightnessBox()
		=> ParseClampReTextDiffApply(brightnessBox, ref generator.SelectedBrightness, (ushort)0, brightnessMax, out var prev)
		|| QueueResetText<ushort>(brightnessBox, prev);

	private void BloomBox_TextChanged(object sender, EventArgs e) => BloomBox();
	internal void BloomBox() {
		AdjustBloomMax();
		if (ParseClampReTextMulDiffApply(bloomBox, ref generator.SelectedBloom, 0, bloomMax, bloomMul, out var prev)
			|| QueueResetText<float>(bloomBox, prev))
			return;
		// Number of threads can change the maximum of these:
		StripeBox();
	}
	private void AdjustBloomMax() {
		if (width + height <= 0)
			return;
		byte OnePixelBytes = 12;
		var L2_eff = .85f * (generator.SelectedL2Kilobytes == 0 ? FractalGenerator.GetEstimatedL2CachePerCore() : generator.SelectedL2Kilobytes);
		var maxCache = (L2_eff / (OnePixelBytes * width) - 2) / 2;

		if (MaxTasks <= 0) {
			bloomMax = (ushort)maxCache;
			UpdateBloomLabel();
			return;
		}
		bloomMax = (ushort)(Math.Min(
			Math.Max(1, height / (2 * MaxTasks) - 1), // Should be enough run all the threads of each in parallel
			maxCache // the cache size should fit a strip wide as Width, and Tall as BloomDiameter+StripHeight
		) / bloomMul);
		UpdateBloomLabel();
	}
	private void BlurBox_TextChanged(object sender, EventArgs e) => BlurBox();
	private bool BlurBox()
		=> ParseClampReTextDiffApply(blurBox, ref generator.SelectedBlur, (ushort)0, blurMax, out var prev)
		|| QueueResetText<ushort>(blurBox, prev);

	private void ZoomChildBox_TextChanged(object sender, EventArgs e) => ZoomChildBox();
	private bool ZoomChildBox() {
		var prev = generator.SelectedZoom;
		var n = ParseClampReText<short>(zoomChildBox, (short)0, (short)Math.Max(0, Math.Min(generator.MaxZoomChild, generator.GetFractal().ChildCount - 1)));
		return !generator.SelectZoomChild(n) && QueueResetText<short>(zoomChildBox, prev);
	}

	private void AbortBox_TextChanged(object sender, EventArgs e) => AbortBox();
	private void AbortBox()
		=> abortDelay = ParseClampReText(abortBox, (ushort)0, (ushort)10000);
	private void EncodePngSelect_SelectedIndexChanged(object sender, EventArgs e)
		=> generator.SelectedPngType = (FractalGenerator.PngType)Math.Max(0, encodePngSelect.SelectedIndex);
	private void TimingSelect_SelectedIndexChanged(object sender, EventArgs e) => TimingSelect();
	private void TimingSelect() {
		UpdateTimeLocale();
		timingBox.Text = (timingSelect.SelectedIndex == 0 ? generator.SelectedDelay : generator.SelectedFps).ToString();
	}
	private void TimingBox_TextChanged(object sender, EventArgs e) => TimingBox();
	private void TimingBox() {
		// TODO LATER try to remove the GIF restart (was bugged the last time I tried)
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
					_ = QueueReset();


				break;
			case 1:
				var newFps = ParseClampReText(timingBox, (ushort)1, (ushort)120);
				timer.Interval = 1000 / generator.SelectedFps;
				if (generator.SelectedFps == newFps)
					return;
				generator.SelectedFps = newFps;
				generator.SelectedDelay = (ushort)(100 / newFps);
				if (generator.SelectedGifType != FractalGenerator.GifType.No)
					_ = QueueReset();
				break;
		}
	}
	private void ParallelTypeSelect_SelectedIndexChanged(object sender, EventArgs e) => ParallelTypeSelect();
	private void ParallelTypeSelect() {
		/*if ((FractalGenerator.ParallelType)parallelTypeSelect.SelectedIndex == FractalGenerator.ParallelType.OfDepth) {
			_ = MessageBox.Show(
				"Warning: this parallelism mode might be fast at rendering a single image, but it messes up few pixels.\nSo if you want highest quality the OfAnimation is recommended.",
				"Warning",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}*/
		generator.SelectedParallelType = (FractalGenerator.ParallelType)parallelTypeSelect.SelectedIndex;
	}
	private void VoidSelect_SelectedIndexChanged(object sender, EventArgs e) => VoidSelect();
	private void VoidSelect() {
		// no need to prev because it doesn't restart
		var now = (FractalGenerator.GpuVoidType)Math.Max(0, voidSelect.SelectedIndex);
		if (generator.SelectedGpuVoidType == now)
			return;
		generator.SelectedGpuVoidType = now;
		//QueueReset(); // can be updated mid-generation
	}
	private void DrawSelect_SelectedIndexChanged(object sender, EventArgs e) => DrawSelect();
	private void DrawSelect() {
		// no need to prev because it doesn't restart
		var now = (FractalGenerator.GpuDrawType)Math.Max(0, drawSelect.SelectedIndex);
		if (generator.SelectedGpuDrawType == now)
			return;
		generator.SelectedGpuDrawType = now;
		//QueueReset(); // can be updated mid-generation
	}
	private void EncodeGifSelect_SelectedIndexChanged(object sender, EventArgs e) => EncodeGifSelect();
	private void EncodeGifSelect() {
		var prev = generator.SelectedGifType;
		var now = (FractalGenerator.GifType)Math.Max(0, encodeGifSelect.SelectedIndex);
		if (generator.SelectedGifType == now)
			return;
		generator.SelectedGifType = now;
		// TODO LATER remove reset, but the last time I tried it was bugged
		if (now != FractalGenerator.GifType.No && now != prev)
			_ = QueueResetCombo<int>(encodeGifSelect, (int)prev);
	}

	// Caching:
	internal void SetCache(bool cache) {
		l2Label.Visible = binLabel.Visible = stripeLabel.Visible = l2Box.Visible = binBox.Visible = stripeBox.Visible = cache;
		generator.SelectedCacheOpt = cache;
		QueueReset();
	}

	private void StripeBox_TextChanged(object sender, EventArgs e) => StripeBox();
	internal void StripeBox() {
		AdjustStripeMax();
		if (ParseClampReTextDiffApply(stripeBox, ref generator.SelectedStripeHeight, (ushort)0, stripeMax, out var prev) 
			|| QueueResetText<ushort>(stripeBox,prev))
			return;
		BloomBox();
	}
	private void AdjustStripeMax() {
		byte OnePixelBytes = 12;
		var L2_eff = (generator.SelectedL2Kilobytes == 0 ? FractalGenerator.GetEstimatedL2CachePerCore() : generator.SelectedL2Kilobytes) * .85f;
		var minCachableStripe = (ushort)Math.Max(1, L2_eff / (OnePixelBytes * width) - 2 * generator.SelectedBloom - 2);
		if (MaxTasks <= 0) {
			stripeMax = minCachableStripe;
			return;
		}
		stripeMax = Math.Min(
			(ushort)(0.75f * height / MaxTasks), // Should be enough run all the threads of each in parallel
			minCachableStripe // the cache size should fit a strip wide as Width, and Tall as BloomDiameter+StripHeight
		);
	}
	private bool BinBox()
		=> ParseClampReTextDiffApply(binBox, ref generator.SelectedBinSize, (ushort)0, binMax, out ushort prev)
		|| QueueResetText<ushort>(binBox, prev);
	private void BinBox_TextChanged(object sender, EventArgs e) => BinBox();
	private bool L2Box()
		=> ParseClampReTextDiffApply(l2Box, ref generator.SelectedL2Kilobytes, (ushort)0, l2Max, out var prev)
		|| QueueResetText<ushort>(l2Box, prev);
	private void L2Box_TextChanged(object sender, EventArgs e) => L2Box();

	private void NameBox_TextChanged(object sender, EventArgs e) {
		_ = Clean(nameBox);
		MyName = nameBox.Text;
		UpdateName();
		importForm.UpdateName();
	}

	private void GenerationSelect_SelectedIndexChanged(object sender, EventArgs e) => GenerationSelect();
	private void GenerationSelect() {
		if ((FractalGenerator.GenerationType)generationSelect.SelectedIndex == FractalGenerator.GenerationType.HashSeeds) {
			_ = MessageBox.Show(L("notForEndUser"), L("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		var prev = (int)generator.SelectedGenerationType;
		generator.SelectedGenerationType = (FractalGenerator.GenerationType)Math.Max(0, generationSelect.SelectedIndex);
		_ = QueueResetCombo<int>(generationSelect, prev);
	}
	private void ExportButton_Click(object sender, EventArgs e) => _ = ExportButton();
	private bool ExportButton() {

		// somehow this being checked was messing with my dialog focus:
		debugBox.Checked = animBox.Checked = pngBox.Checked = gifBox.Checked = false;
		var b = generator.GetBitmapsFinished();
		var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

		if (IsExporting())
			return CancelExport() && false;

		bool StayedOnlyImage() {
			if (generator.SelectedGenerationType == GenerationType.OnlyImage) {
				var result = MessageBox.Show(L("cantExportOnlyImage"), L("switchtoAnimation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result == DialogResult.No)
					return true;
				generationSelect.SelectedIndex = 1;
			}
			return false;
		}
		switch ((ScheduledTask)exportSelect.SelectedIndex) {
			case ScheduledTask.Png:
				if (b < 1)
					return Error("onlyPreview", "pleaseWait");
				// Make sure the bitmap is actually loaded
				UpdateBitmap(generator.GetBitmap(currentBitmapIndex %= b));
				SetFileName(savePng, "png");
				_ = savePng.ShowDialog();
				break;
			case ScheduledTask.Pngs:
				if (StayedOnlyImage())
					return false;
				SetFileName(saveMp4, "png");
				saveMp4.DefaultExt = "png";
				saveMp4.Filter = L("pngImage") + " (*.png)|*.png";
				_ = saveMp4.ShowDialog();
				break;
			case ScheduledTask.PngsToMp4:
				if (NoFfmpeg(ffmpegPath))
					return false;
				if (StayedOnlyImage())
					return false;
				SetFileName(saveMp4, "mp4");
				saveMp4.DefaultExt = "mp4";
				saveMp4.Filter = L("mp4Video") + " (*.mp4)|*.mp4";
				_ = saveMp4.ShowDialog();
				break;
			case ScheduledTask.Gif:
				if (generator.SelectedGifType == FractalGenerator.GifType.No)
					return Error("gifDisabled", "notSelected");
				if (StayedOnlyImage())
					return false;
				if (isGifReady == 0)
					return Error(L("encodedGifNa"), L("notAvailable"));
				SetFileName(saveGif, "gif");
				saveGif.DefaultExt = "gif";
				saveGif.Filter = L("gifVideo") + " (*.gif)|*.gif";
				_ = saveGif.ShowDialog();
				break;
			case ScheduledTask.GifToMp4:
				if (NoFfmpeg(ffmpegPath))
					return false;
				if (generator.SelectedGifType == FractalGenerator.GifType.No)
					return Error("gifDisabled", "notSelected");
				if (StayedOnlyImage())
					return false;
				//if (isGifReady == 0 || gifPath == "")
				//	return Error("encodedGifNa", "notAvailable");
				SetFileName(saveGif, "mp4");
				saveGif.DefaultExt = "mp4";
				saveGif.Filter = L("mp4Video") + " (*.mp4)|*.mp4";
				_ = saveGif.ShowDialog();
				break;
			case ScheduledTask.ImportCode:
				importForm.Update(GetFileName());
				importForm.Show();
				importForm.Location = Location;
				break;
			case ScheduledTask.Close:
				if (Scheduled.Count == 0)
					return Error("noScheduleClose", "noSchedule");
				AddScheduled(new(ScheduledTask.Close, ""));
				break;
		}
		return true;
	}
	private void UpdateTasks() 
		=> tasksButton.Text = tasksText = L("tasks") + ": " + Scheduled.Count.ToString();
	internal void AddScheduled(Schedule schedule) {
		Scheduled.Add(schedule);
		scheduler.AddListEntry(schedule);
		UpdateTasks();
	}
	internal bool PerformSchedule() {
		UpdateTasks();
		if (Scheduled.Count <= 0) {
			scheduler.FillListEntries();
			return true;
		}
		void UpdateRunning() {
			BackColor = Background;
			scheduler.Running = true;
			scheduler.UpdateRunning();
		}
		switch (Scheduled[0].Type) {
			case ScheduledTask.Close:
				RemoveSchedule();
				UpdateTasks();
				if (TryClose(true))
					break;
				PerformClose();
				break;
			case ScheduledTask.Pngs:
				mp4Path = Scheduled[0].Filename;
				UpdateRunning();
				xTask = Task.Run(ExportPngs, (xCancel = new()).Token);
				break;
			case ScheduledTask.PngsToMp4:
				mp4Path = Scheduled[0].Filename;
				UpdateRunning();
				xTask = Task.Run(ExportMp4, (xCancel = new()).Token);
				
				break;
			case ScheduledTask.Gif:
				// save gif:
				UpdateRunning();
				gifPath = Scheduled[0].Filename;
				// Gif Export Task
				//foreach (var c in MyControls)c.Enabled = false;
				xTask = Task.Run(ExportGif, (xCancel = new()).Token);
				break;
				case ScheduledTask.GifToMp4:
				// convert gif->mp4
				if (isGifReady > 0)
					gifPath = generator.GifTempPath;
				if (gifPath == "") {
					// TODO LATER maybe give the user some choices to cancel the schedule if it failed like this...?
					_ = Error("encodedGifNa", "notAvailable");
					FinishScheduled();
					notifyExp = true;
					return false;
				}
				mp4Path = Scheduled[0].Filename;
				UpdateRunning();
				xTask = Task.Run(ConvertMp4, (xCancel = new()).Token);
				break;
		}
		return false;
	}
	private void RemoveSchedule() {
		Scheduled.RemoveAt(0);
		scheduler.FillListEntries();
		scheduler.Running = false;
	}
	private void FinishScheduled() {
		RemoveSchedule();
		notifyExp = true;
		xTask = null;
	}
	private void CancelSchedule() {
		RemoveSchedule();
		UpdateTasks();
		xTask = null;
	}
	private void ExportSelect_SelectedIndexChanged(object sender, EventArgs e) => _ = ExportSelect();
	private bool ExportSelect() {
		// TODO LATER fix this
		if (exportSelect.SelectedIndex == 4)
			return Error("gifToMp4Unavailable", "notAvailable");
		UpdateExportTipLocale();
		return true;
	}
	private void TasksButton_Click(object sender, EventArgs e) => OpenTasks();
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
				f += filePostfix[s].ToString()[1];
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
		if (!(debugBox.Checked || animBox.Checked || pngBox.Checked || gifBox.Checked))
			debugLabel.Text = "";
	}
	private void DebugAnimBox_CheckedChanged(object sender, EventArgs e) {
		generator.DebugAnim = animBox.Checked;
		if (!(debugBox.Checked || animBox.Checked || pngBox.Checked || gifBox.Checked))
			debugLabel.Text = "";
	}
	private void DebugPngBox_CheckedChanged(object sender, EventArgs e) {
		generator.DebugPng = pngBox.Checked;
		if (!(debugBox.Checked || animBox.Checked || pngBox.Checked || gifBox.Checked))
			debugLabel.Text = "";
	}
	private void DebugGifBox_CheckedChanged(object sender, EventArgs e) {
		generator.DebugGif = gifBox.Checked;
		if (!(debugBox.Checked || animBox.Checked || pngBox.Checked || gifBox.Checked))
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
	/// <summary>
	/// User input the path and name for saving PNG
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void SavePng_FileOk(object sender, CancelEventArgs e) {
		BackColor = Background;
		using var myStream = savePng.OpenFile();
		currentBitmap?.Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
		myStream.Close();
		FinishedTask();
	}
	private void FinishedTask() {
		if (PerformSchedule())
			BackColor = Color.FromArgb(32, 32, 128);
		//if (runningBatch.Length > 0)
		//	NextBatch();
	}
	/// <summary>
	/// User input the path and name for saving GIF/Mp4
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void SaveGif_FileOk(object sender, CancelEventArgs e) {
		if (xTask != null) {
			_ = Error("gifStillSaving", "pleaseWait");
			return;
		}
		if (sender is not SaveFileDialog s)
			return;
		string ext = saveGif.FileName[^3..];
		if (ext == "gif") {
			AddScheduled(new(ScheduledTask.Gif, s.FileName));
			return;
		}
		// convert gif->mp4
		if (NoFfmpeg())
			return;
		AddScheduled(new(ScheduledTask.GifToMp4, s.FileName));
	}

	private static bool NoFfmpeg() => NoFfmpeg(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"));
	private static bool NoFfmpeg(string ffmpegPath) {
		if (!File.Exists(ffmpegPath)) {
			_ = Error("ffmpeg", "notAvailable");
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
			_ = Error("mp4stillSaving", "pleaseWait");
			return;
		}
		if (sender is not SaveFileDialog s)
			return;
		if (generator.SelectedPngType == FractalGenerator.PngType.No) {
			encodePngSelect.SelectedIndex = 1;
		}
		string ext = saveMp4.FileName[^3..];
		if (ext == "png") { // save pngs:
			AddScheduled(new(ScheduledTask.Pngs, s.FileName));
			return;
		}
		// Save mp4:
		if (NoFfmpeg())
			return;
		AddScheduled(new(ScheduledTask.PngsToMp4, s.FileName));
	}


	public bool LoadCodeName(string codeName) {
		var s = codeName.Split(['_', '(', ')']);
		var f = Fractals;

		// load fractal type:
		short fi = 0;
		while (fi < f.Count) {
			if (f[fi].Name == s[0]) {
				fractalSelect.SelectedIndex = fi;
				break;
			}
			++fi;
		}
		if (fi == f.Count)
			return false; // not valid file name, nothing can be loaded
		fi = 1;
		short _s;
		while (fi < s.Length) {
			if (s[fi].Length < 1)
				break;
			switch (s[fi][0]) {
				case 'A': // Angles
						  // Child Angles Bitmask
					if (ulong.TryParse(s[++fi], out generator.SelectedChildAngles)) {
						FillSelectAngles();
						++fi;
					} else return TryQueueReset();
					break;
				case 'C': // Colors
						  // Child Colors Bitmask:
					if (ulong.TryParse(s[++fi], out generator.SelectedChildColors)) {
						FillSelectColors();
						++fi;
					} else return TryQueueReset();
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
					resSelect.SelectedIndex = prevResSelect = 1;
					++fi;
					break;
				case 'H': // Hues
					short hi = 0;
					var cc = Colors;
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
					return TryQueueReset();
			}
			++fi; // skips the empty strings inbetween ")_"
		}
		return TryQueueReset();
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
		if (!xCancel.Token.IsCancellationRequested) {
			isGifReady = 0;
			FinishScheduled();
		} else CancelSchedule();
	}
	/// <summary>
	/// Exports the animation into a MP4 file
	/// Actually - it just converts a gif file into MP4
	/// </summary>
	/// <returns></returns>
	private void ConvertMp4() {
		string result = generator.SaveGifToMp4(gifPath, mp4Path);
		if (xCancel.Token.IsCancellationRequested) {
			CancelSchedule();
			xTask = null;
			return;
		}
		if (result == "") {
			isGifReady = 0;
			gifPath = "";
			mp4Path = "";
			FinishScheduled();
			return;
		}
		_ = Error(result, "failedGif");
	}
	/// <summary>
	/// Exports the animation into a MP4 file
	/// Actually - it just exports PNG series and converts those into MP4
	/// </summary>
	/// <returns></returns>
	private void ExportPngs() {
		string result = generator.SavePngs(mp4Path);
		if (xCancel.Token.IsCancellationRequested) {
			CancelSchedule();
			xTask = null;
			return;
		}
		if (result == "") {
			mp4Path = "";
			isPngsSaved = true;
			FinishScheduled();
			return;
		}
		_ = Error(result, "failedPng");
	}
	/// <summary>
	/// Exports the animation into a MP4 file
	/// Actually - it just exports PNG series and converts those into MP4
	/// </summary>
	/// <returns></returns>
	private void ExportMp4() {
		string result = generator.SavePngsToMp4(mp4Path);
		if (xCancel.Token.IsCancellationRequested) {
			CancelSchedule();
			xTask = null;
			return;
		}
		if (result == "") {
			mp4Path = "";
			isPngsSaved = true;
			FinishScheduled();
			return;
		}
		_ = Error(result, "failedMp4");
	}
	string GetFileName() {
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
			f += "_H(" + Colors[generator.SelectedPaletteType].Item1 + "_" + hueSelect.SelectedIndex + "_" + defaultHue.Text + "_" + hueSpeedBox.Text + ")";
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
		return f;
	}
	void SetFileName(SaveFileDialog dialog, string extension) 
		=> dialog.FileName = GetFileName() + "." + extension;
	#endregion

	#region Editor
	private void SetPreviewMode(bool newPreviewMode)
		=> preButton.Text = L((generator.SelectedPreviewMode = previewMode = newPreviewMode) ? "structureModeText" : "previewModeText");
	private void FillListEntries() {
		pointPanel.SuspendLayout();
		UnFillListEntries();
		var f = generator.GetFractal();
		for (var i = 0; i < f.ChildCount; ++i)
			ReAddExistingListEntry(f.ChildX, f.ChildY, f.ChildAngle[generator.SelectedChildAngle].Item2, f.ChildColor[generator.SelectedChildColor].Item2, false);
		modifySettings = true;
		sizeBox.Text = f.ChildSize.ToString(CultureInfo.InvariantCulture);
		cutBox.Text = f.CutSize.ToString(CultureInfo.InvariantCulture);
		minBox.Text = f.MinSize.ToString(CultureInfo.InvariantCulture);
		maxBox.Text = f.MaxSize.ToString(CultureInfo.InvariantCulture);
		modifySettings = false;
		pointPanel.ResumeLayout(false);
		pointPanel.PerformLayout();
	}
	private void UnFillListEntries() {
		addPointButton.Location = new(10, 10);
		foreach (var s in lineListSwitch) {
			pointPanel.Controls.Remove(s);
			s.Dispose();
		}
		void R(Control _c) {
			pointPanel.Controls.Remove(_c);
			_c.Dispose();
		}
		foreach (var (x, y, a, c, d) in lineList) {
			R(x); R(y); R(a); R(c); R(d);
		}
		lineListSwitch.Clear();
		lineList.Clear();
		pointTabIndex = controlTabIndex;
		myEditControls.Clear();
	}
	private void SwitchChildAngle() {
		var f = generator.GetFractal();
		modifySettings = true;
		for (var i = 0; i < f.ChildCount; ++i)
			lineList[i].Item3.Text = f.ChildAngle[generator.SelectedChildAngle].Item2[i].ToString(CultureInfo.InvariantCulture);
		modifySettings = false;
	}
	private void SwitchChildColor() {
		var f = generator.GetFractal();
		modifySettings = true;
		var set = generator.GetAllocPalette();
		if (set != null)
			for (var i = 0; i < f.ChildCount; ++i) {
				var item = lineList[i].Item4;
				var color = FractalGenerator.SampleColor(set, generator.SelectedDefaultHue + .5 *
					f.ChildColor[generator.SelectedChildColor].Item2[i] /*% generator.GetAllocPalette2()*/);
				item.BackColor = Color.FromArgb((byte)color.X, (byte)color.Y, (byte)color.Z);
				item.Text = f.ChildColor[generator.SelectedChildColor].Item2[i].ToString();
			}
		modifySettings = false;
	}
	private void ReAddExistingListEntry(double[] cx, double[] cy, double[] ca, byte[] cc, bool single = true) {
		var i = lineList.Count;
		lineList.Add((new(), new(), new(), new(), new()));
		var (x, y, a, c, d) = lineList[i];
		x.Text = cx[i].ToString(CultureInfo.InvariantCulture);
		y.Text = cy[i].ToString(CultureInfo.InvariantCulture);
		a.Text = ca[i].ToString(CultureInfo.InvariantCulture);
		c.Text = cc[i].ToString(CultureInfo.InvariantCulture);
		BindListEntry((x, y, a, c, d), i, single);
	}
	private void BindListEntry((TextBox x, TextBox y, TextBox a, TextBox c, Button r) l, int i, bool single = true) {
		const int textSize = 53;
		const int buttonSize = 23;
		if (single)
			pointPanel.SuspendLayout();
		var hor = 0;
		void NewControl(Control _c, int size, string name, string tool) {
			_c.Location = new Point(2 + hor, 2 + i * (buttonSize+4));
			_c.Margin = new Padding(4, 3, 4, 3);
			_c.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
			_c.Size = new Size(size, buttonSize);
			_c.Name = name;
			_c.BackColor = Color.FromArgb(192, 192, 192);
			pointPanel.Controls.Add(_c);
			SetupEditControl(_c, tool);
			hor += size + 4;
		}
		TextBox NewText(TextBox _t, string name, string tool, int size = textSize) {
			NewControl(_t, textSize, name, tool);
			return _t;
		}
		Button NewButton(Button _b, string name, string tool) {
			NewControl(_b, buttonSize, name, tool);
			return _b;
		}

		// Swap
		if (i > 0) {
			lineListSwitch.Add(new());
			var s = lineListSwitch[^1];
			s.Text = "⇕";
			NewButton(s, "s" + i.ToString(), "editSwitch").Click += (_, _) => {
				var f = generator.GetFractal();
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
				var (xi, yi, ai, ci, _) = lineList[i];
				xi.Text = cx[i].ToString(CultureInfo.InvariantCulture);
				yi.Text = cy[i].ToString(CultureInfo.InvariantCulture);
				ai.Text = f.ChildAngle[generator.SelectedChildColor].Item2[i].ToString(CultureInfo.InvariantCulture);
				ci.Text = f.ChildColor[generator.SelectedChildColor].Item2[i].ToString(CultureInfo.InvariantCulture);
				var (xi1, yi1, ai1, ci1, _) = lineList[i - 1];
				xi1.Text = cx[i - 1].ToString(CultureInfo.InvariantCulture);
				yi1.Text = cy[i - 1].ToString(CultureInfo.InvariantCulture);
				ai1.Text = f.ChildAngle[generator.SelectedChildColor].Item2[i - 1].ToString(CultureInfo.InvariantCulture);
				ci1.Text = f.ChildColor[generator.SelectedChildColor].Item2[i - 1].ToString(CultureInfo.InvariantCulture);
				EditFractal();
			};
			s.Location = new(3 * (textSize + 4) + 2 * (buttonSize + 4), 2 + (i * 2 - 1) * (buttonSize + 4) / 2);
		}
		hor = 0;

		// (x, y, angle, color):
		NewText(l.x, "x" + i.ToString(), "editX").TextChanged += (sender, e) => {
			if (sender is TextBox s && !ParseDiffApply(s, ref generator.GetFractal().ChildX[i], out var _))
				EditFractal();
		};
		NewText(l.y, "y" + i.ToString(), "editY").TextChanged += (sender, e) => {
			if (sender is TextBox s && !ParseDiffApply(s, ref generator.GetFractal().ChildY[i], out var _))
				EditFractal();
		};
		NewText(l.a, "a" + i.ToString(), "editA").TextChanged += (sender, e) => {
			if (sender is TextBox s && !ParseDiffApply(s, ref generator.GetFractal().ChildAngle[generator.SelectedChildAngle].Item2[i], out var _))
				EditFractal();
		};
		NewText(l.c, "c" + i.ToString(), "editC", buttonSize).TextChanged += (sender, _) => {
			if (sender is TextBox s && !DiffApply(ReText(s, ParseValue<byte>(s)), ref generator.GetFractal().ChildColor[generator.SelectedChildColor].Item2[i], out var _))
				EditFractal();
		};
		
		// Remove:
		NewButton(l.r, "r" + i.ToString(), "editR").Click += (sender, e) => {
			var f = generator.GetFractal();
			if (f.ChildCount <= 1) {
				_ = Error("cannotRemoveLast", "cannotRemove");
				return;
			}
			var ni = --f.ChildCount;
			var nx = new double[ni];
			var ny = new double[ni];
			var na = new double[f.ChildAngle.Count][];
			var nc = new byte[f.ChildColor.Count][];
			var cx = f.ChildX;
			var cy = f.ChildY;
			pointPanel.SuspendLayout();
			UnFillListEntries();

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
				ReAddExistingListEntry(nx, ny, na[generator.SelectedChildAngle], nc[generator.SelectedChildAngle], false);
			pointPanel.ResumeLayout(false);
			// Finish edit
			EditFractal();
		};
		l.r.BackColor = Color.FromArgb(192, 192, 192);
		if (single) {
			pointPanel.ResumeLayout(false);
			pointPanel.PerformLayout();
		}
		addPointButton.Location = new(10, 10 + (i + 1) * buttonSize);
	}
	private void EditFractal() {
		if (modifySettings)
			return;
		var f = generator.GetFractal();
		f.Edit = true;
		foreach (var g in gens.Gen)
			if (g.Value.generator.GetFractal() == f)
				_ = g.Value.QueueReset();
	}
	private void SaveFractal_FileOk(object sender, CancelEventArgs e) {
		var f = toSave;
		if (sender is not SaveFileDialog save)
			return;
		var fractalName = Path.GetFileNameWithoutExtension(f.Path = save.FileName);
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
		if (sender is not OpenFileDialog open)
			return;
		var filename = open.FileName;
		var fractalName = filename[..^8];
		int i;
		while ((i = fractalName.IndexOf('/')) >= 0)
			fractalName = fractalName[(i + 1)..];
		while ((i = fractalName.IndexOf('\\')) >= 0)
			fractalName = fractalName[(i + 1)..];
		fractalName = fractalName.Replace('|', '.').Replace(':', '.').Replace(';', '.');
		if (FractalSelection.Contains(fractalName)) {
			_ = Error("alreadyFractal", "alreadyExists");
			fractalSelect.SelectedIndex = fractalSelect.Items.IndexOf(fractalName);
		} else if (File.Exists(filename) && gens != null)
			_ = LoadFractal(gens, filename, this);
	}
	internal void SetEditor(bool editor)
		=> toolGenPanel.Visible = generatorPanel.Visible = !(
		toolEditPanel.Visible = editorPanel.Visible = generator.SelectedEditorMode = editorVisible = editor);
	private void AddPoint_Click(object sender, EventArgs e) {
		NewListEntry();
	}
	private void NewListEntry() {
		var i = lineList.Count;
		lineList.Add((new(), new(), new(), new(), new()));
		var (x, y, a, c, d) = lineList[i];
		x.Text = y.Text = a.Text = "0";
		d.Text = "X";
		c.Text = "";
		//c.BackColor = Color.Red;
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
			var nal = na[l] = new double[ni + 1]; // make new incrementally bigger ChildAngle sets
			var (can, ca) = f.ChildAngle[l];
			for (var ci = 0; ci < ni; ++ci)
				nal[ci] = ca[ci]; // copy the previous angles to each set
			nal[ni] = 0; // put 0 as the childAngle of the new child to each new set
			f.ChildAngle[l] = (can, nal);
		}
		for (var l = 0; l < f.ChildColor.Count; ++l) {
			var ncl = nc[l] = new byte[ni + 1];
			var (ccn, cc) = f.ChildColor[l];
			for (var ci = 0; ci < ni; ++ci)
				ncl[ci] = cc[ci];
			ncl[ni] = 0;
			f.ChildColor[l] = (ccn, ncl);
		}
		BindListEntry((x, y, a, c, d), i);
		EditFractal();
	}
	private void SizeBox_TextChanged(object sender, EventArgs e) {
		if (!ParseClampReTextDiffApply(sizeBox, ref generator.GetFractal().ChildSize, Math.Sqrt(2), 128, out var _))
			EditFractal();
	}
	private void MaxBox_TextChanged(object sender, EventArgs e) {
		if (!ParseClampReTextDiffApply(maxBox, ref generator.GetFractal().MaxSize, 1.1, 128, out var _))
			EditFractal();
	}
	private void MinBox_TextChanged(object sender, EventArgs e) {
		if (!ParseClampReTextDiffApply(minBox, ref generator.GetFractal().MinSize, 0.05, 128, out var _))
			EditFractal();
	}
	private void CutBox_TextChanged(object sender, EventArgs e) {
		if (!ParseClampReTextDiffApply(cutBox, ref generator.GetFractal().CutSize, 0.1, 128, out var _))
			EditFractal();
	}
	private void AddAngleButton_Click(object sender, EventArgs e) => AddAngleButton();
	private bool AddAngleButton() {
		if (angleBox.Text == "")
			return Error("noAngles", "cannotAdd");
		foreach (var (n, _) in generator.GetFractal().ChildColor)
			if (n == angleBox.Text)
				return Error("uniqueAngles", "cannotAdd");
		generator.GetFractal().ChildAngle.Add((angleBox.Text, new double[generator.GetFractal().ChildCount]));
		//SetupSelects();
		angleSelect.SelectedIndex = angleSelect.Items.Add(charOff + angleBox.Text);
		EditFractal();
		FillListEntries();
		return true;
	}
	private void RemoveAngleButton_Click(object sender, EventArgs e) {
		if (generator.GetFractal().ChildAngle.Count <= 1) {
			_ = Error("cannotRemoveLast", "cannotRemove");
			return;
		}
		generator.GetFractal().ChildAngle.RemoveAt(generator.SelectedChildAngle);
		generator.SelectedChildAngle = Math.Min((short)(generator.GetFractal().ChildAngle.Count - 1), generator.SelectedChildAngle);
		generator.SelectedChildAngles = 0;
		SetupSelects(); // this was missing here, maybe i just forgot, or did the item in the selectors got removed elsewhere?
		SwitchChildAngle();
		EditFractal();
	}
	private void AddColorButton_Click(object sender, EventArgs e) => AddColorButton();
	private bool AddColorButton() {
		if (colorBox.Text == "")
			return Error("noColors","cannotAdd");
		foreach (var (n, _) in generator.GetFractal().ChildColor)
			if (n == colorBox.Text)
				return Error("uniqueColors", "cannotAdd");
		generator.GetFractal().ChildColor.Add((colorBox.Text, new byte[generator.GetFractal().ChildCount]));
		//SetupSelects();
		colorSelect.SelectedIndex = colorSelect.Items.Add("✕ " + colorBox.Text);
		EditFractal();
		FillListEntries();
		return true;
	}
	private void RemoveColorButton_Click(object sender, EventArgs e) {
		if (generator.GetFractal().ChildColor.Count <= 1) {
			_ = Error("cannotRemoveLast", "cannotRemove");
			return;
		}
		generator.GetFractal().ChildColor.RemoveAt(generator.SelectedChildColor);
		generator.SelectedChildColor = Math.Min((short)(generator.GetFractal().ChildColor.Count - 1), generator.SelectedChildColor);
		generator.SelectedChildColors = 0;
		SetupSelects(); // this was missing here, maybe i just forgot, or did the item in the selectors got removed elsewhere?
		SwitchChildColor();
		EditFractal();
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
		EditFractal();
		//_ = Task.Run(Hash);
		//Remember and disable all controls
		myControlsEnabled.Clear();
		foreach (var c in myControls) {
			myControlsEnabled.Add(c.Key, c.Key.Enabled);
			c.Key.Enabled = false;
		}
		// perform hash
		performHash = true;
	}
	private void RemoveCutButton_Click(object sender, EventArgs e) {
		if (generator.GetFractal().ChildCutFunction.Count == 0) {
			_ = Error("noCutFunctions", "cannotRemove");
			return;
		}
		generator.GetFractal().ChildCutFunction.RemoveAt(generator.SelectedCut);
		generator.SelectedCut = (short)Math.Max(0, Math.Min(generator.GetFractal().ChildCutFunction.Count - 1, generator.SelectedCut));
		generator.SelectedCutSeed = 0;
		SetupSelects();
		EditFractal();
	}
	private void LoadButton_Click(object sender, EventArgs e) {
		_ = loadFractal.ShowDialog();
	}
	private void SaveButton_Click(object sender, EventArgs e) {
		toSave = generator.GetFractal();
		_ = saveFractal.ShowDialog();
	}
	private void PreButton_Click(object sender, EventArgs e) {
		SetPreviewMode(!previewMode);
		_ = QueueReset();
	}
	#endregion

	internal ComboBox GetFractalSelect() => fractalSelect;
	internal void UpdateLocale() {
		UpdateName();

		foreach (var c in myControls)
			toolTips.SetToolTip(c.Key, L(c.Value));
		foreach (var c in myEditControls)
			toolTips.SetToolTip(c.Key, L(c.Value));
		UpdateTimeLocale();
		UpdateExportLocale();
		UpdateExportTipLocale();
		UpdateRangeLocale();
		UpdateAngleSelected();
		UpdateColorSelected();
		UpdateInfoTextLocale();
		UpdateCacheLocale();
		UpdateDitherLocale();
		UpdateResolutionLocale();
		UpdateBloomLabel();
		SetPreviewMode(previewMode);

		importForm.UpdateLocale();
		scheduler.UpdateLocale();

		// Update Label Locale
		if (generator.SelectedFractal >= 0)
			fractalSelect.Text = L("fractalSelectTip)");
		fractalLabel.Text = L("fractalSelectLabel") + ":";
		angleLabel.Text = L("angleLabel") + ":";
		colorLabel.Text = L("colorLabel") + ":";
		cutLabel.Text = L("cutLabel") + ":";
		periodLabel.Text = L("periodLabel") + ":";
		zoomLabel.Text = L("zoomLabel") + ":";
		hueLabel.Text = L("hueLabel") + ":";
		fileLabel.Text = L("file") + ":";
		genNameLabel.Text = L("genNameLabel") + ":";
		l2Label.Text = L("l2Label") + ":";
		gpuDLabel.Text = L("gpuDLabel") + ":";
		gpuVLabel.Text = L("GPU Void") + ":";
		stripeLabel.Text = L("stripeLabel") + ":";
		binLabel.Text = L("binLabel") + ":";
		ditherLabel.Text = L("ditherLabel") + ":";
		encodePngLabel.Text = L("encodePngLabel Encoding") + ":";
		encodeGifLabel.Text = L("encodeGifLabel Encoding") + ":";
		generationModeLabel.Text = L("generationModeLabel") + ":";
		abortDelayLabel.Text = L("abortDelayLabel") + ":";
		timingLabel.Text = L("timingLabel") + ":";
		UpdateTasks();
		parallelLabel.Text = L("parallelLabel") + ":";
		debugsLabel.Text = L("debugsLabel") + ":";
		zoomChildLabel.Text = L("zoomChildLabel") + ":";
		paletteLabel.Text = L("paletteLabel") + ":";
		addCutLabel.Text = L("addcutLabel CutFunction") + ":";
		addCut.Text = L("addCutTip");
		hideButton.Text = L("hideButtonText");
		addPointButton.Text = L("addPointText");
		saveButton.Text = L("saveButtonText");
		loadButton.Text = L("loadButtonText");
		addAngleButton.Text = L("addAngleButtonText");
		addColorButton.Text = L("addColorButtonText");
		colorBox.Text = L("colorBoxText");
		angleBox.Text = L("angleBoxText");
		animBox.Text = L("Anim");
		pngBox.Text = L("PNG");
		gifBox.Text = L("GIF");

		//Update ComboBox Selection Locales
		modifySettings = true;
		LocComb(spinSelect, "spinSelect", 6);
		LocComb(parallelTypeSelect, "parallelTypeSelect", 2);
		LocComb(generationSelect, "generationSelect", 3);
		LocComb(exportSelect, "exportSelect", 7);
		LocComb(fileSelect, "fileSelect", 12);
		LocComb(voidSelect, "voidSelect", 4);
		LocComb(drawSelect, "drawSelect", 3);
		LocComb(encodePngSelect, "encodePngSelect", 2);
		LocComb(encodeGifSelect, "encodeGifSelect", 3);
		LocComb(timingSelect, "timingSelect", 2);
		if (addCut.Items.Count > 0)
			addCut.Items[0] = L("selectCutFunctionAdd");
		if (paletteSelect.Items.Count > 0)
			paletteSelect.Items[0] = L("random");
		modifySettings = false;

		saveFractal.Filter = loadFractal.Filter = L("Fractal files") + "(*.fractal)|*.fractal";

		spinLabel0.Text = L("spinLabel0");
		sizeLabel0.Text = L("sizeLabel0");
		spinLabel1.Text = L("spinLabel1");
		sizeLabel1.Text = L("sizeLabel1");
		spinLabel2.Text = L("spinLabel2");
		pointLabel2.Text = L("pointLabel2");
		sizeLabel2.Text = L("sizeLabel2");
		sizeLabel3.Text = L("sizeLabel3");

	}
	internal string GetName() => MyName == "" ? Index.ToString() : MyName;
	private void UpdateName() => Text = L("appNameShort") + " - " + L("generator") + " - " + GetName();
	private void UpdateTimeLocale() => toolTips.SetToolTip(timingBox, L("timing" + timingSelect.SelectedIndex.ToString()));
	private bool IsExporting()
		=> xTask != null
		&& Scheduled.Count > 0
		&& (byte)Scheduled[0].Type == exportSelect.SelectedIndex;
	internal bool CancelExport() {
		var result = MessageBox.Show(L("cancelSavingText"), L("cancelSaving"), MessageBoxButtons.YesNo);
		if (result == DialogResult.Yes) {
			xCancel?.Cancel();
			return true;
		}
		return false;
	}
	private void UpdateExportLocale() 
		=> exportButton.Text = IsExporting() ? L("saving") : L("export" + exportSelect.SelectedIndex);
	private void UpdateExportTipLocale() => toolTips.SetToolTip(exportButton, L("exportButton" + exportSelect.SelectedIndex));
	private void UpdateRangeLocale() {
		voidAmbientLabel.Text = L("voidAmbientLabel") + "(0-" + voidAmbientMax + "):";
		voidNoiseLabel.Text = L("voidNoiseLabel") + "(0-" + voidNoiseMax + "):";
		voidScaleLabel.Text = L("voidScaleLabel") + "(0-" + voidScaleMax + "):";
		detailLabel.Text = L("detailLabel") + "(0-" + detailMax + "):";
		saturateLabel.Text = L("saturateLabel") + "(0-" + saturateMax + "):";
		brightnessLabel.Text = L("brightnessLabel") + "(0-" + brightnessMax + "):";
		blurLabel.Text = L("blurLabel") + "(0-" + blurMax + "):";
		//bloomLabel.Text = L("Bloom:)" + "(0-40)") + ":"; // separately
	}
	private void UpdateCacheLocale() {
		stripeLabel.Text = L("stripeHeightLabel") + ": " + generator.GetStripeHeight();
		binLabel.Text = L("binSizeLabel") + ": " + generator.GetBinSize();
	}
	private void UpdateInfoTextLocale() => statusLabel.Text = statusText = L(statusLabel.Tag?.ToString() ?? "") + ": ";
	private void UpdateAngleSelected(bool s = false) {
		if (generator.SelectedFractal < 0)
			return;
		var f = generator.GetFractal();
		var n = f.ChildAngle[generator.SelectedChildAngle].Item1;
		var sel = L("selected") + ": " + n;
		if (angleSelect.Items.Count < 1)
			_ = angleSelect.Items.Add(sel);
		else
			angleSelect.Items[0] = sel;
		if (s)
			angleSelect.Items[generator.SelectedChildAngle + 1] = (((generator.SelectedChildColors >> generator.SelectedChildColor) & 1) == 1 ? charOn : charOff) + n;
	}
	private void UpdateColorSelected(bool s = false) {
		if (generator.SelectedFractal < 0)
			return;
		var f = generator.GetFractal();
		var n = f.ChildColor[generator.SelectedChildColor].Item1;

		var sel = L("selected") + ": " + n;
		if (colorSelect.Items.Count < 1)
			_ = colorSelect.Items.Add(sel);
		else
			colorSelect.Items[0] = sel;
		if (s)
			colorSelect.Items[generator.SelectedChildColor + 1] = (((generator.SelectedChildColors >> generator.SelectedChildColor) & 1) == 1 ? charOn : charOff) + n;
	}
	private void UpdateResolutionLocale() {
		if (resSelect.Items.Count < 2)
			return;
		var c = "Custom:" + width + "x" + height;
		if (resSelect.Items[1]?.ToString() != c) {
			resSelect.Items[1] = c;
		}
	}
	private void UpdateRestartLocale()
		=> restartButton.Text = restartButton.Tag switch {
			0 => "! " + L("restart") + " !",
			1 => L("areYouSure"),
			_ => ""
		};
	private void UpdateDitherLocale() => ditherBox.Text = GetEnabledLocale(ditherBox.Checked);
	private void UpdateBloomLabel() => bloomLabel.Text = ("bloomLabel") + " (0-" + bloomMax + "):";
	private static string GetEnabledLocale(bool enabled) => L(enabled ? "enabled" : "disabled");

	private void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		myControls.Add(control, tip);
		toolTips.SetToolTip(control, L(tip));
		control.TabIndex = ++controlTabIndex;
	}

	private void RemoveResolution_Click(object sender, EventArgs e) {
		_ = Error("notImplemented", "notAvailable");
		// TODO implement
	}

	private void AddResolution_Click(object sender, EventArgs e) {
		_ = Error("notImplemented", "notAvailable");
		// TODO implement
	}

	// Popup Animations
	private bool showPopup = false;
	private float popupA = 1, popupTransientX = 1000000, toolA = 1, toolTransientY = 0;
	internal void UpdatePopups(float deltaTime) {
		var p = PointToClient(Cursor.Position);
		var client = ClientRectangle;
		var inside = client.Contains(p);

		static float Lerp(float a, float b, float t) => b * t + a * (1.0f - t);

		deltaTime *= 4;
		// switch which panels to animate - generator or editor?
		(var tool, var switchable, var bottom, var scrollable) = editorVisible
			? (toolEditPanel, editorPanel, fractalEditorPanel, pointPanel)
			: (toolGenPanel, generatorPanel, genControlPanel, fractalSettingsPanel);
		//const int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
		int bw = Width - client.Width, bh = Height - client.Height;
		int windowWidth = Width - bw, windowHeight = Height - bh;
		// desired X location of the popup (windowWidth - popupWidth)
		//popupPanel.Visible = true;
		var hysteresisOff = windowWidth - popupPanel.Width;
		// does toolbar want to be shown? If so, set its target Y
		bool showTool = shown && inside;
		var toolH = tool.MinimumSize.Height;
		// location in the middle of the animation
		var popupMiddle = windowWidth - popupPanel.Width / 2;
		// Location of the hidden popup
		var hiddenPopup = Math.Clamp(Lerp(windowWidth, popupMiddle, (float)p.X/popupMiddle), popupMiddle, windowWidth);
		// does popup want to be shown? (if popup visible and cursor hysteringly on the right side)
		showPopup = showTool && p.X > (showPopup ? hysteresisOff : popupMiddle);
		// animate toolbar's Y (along the shown/hidden axis)
		toolA = showTool ? Math.Min(1, toolA + deltaTime) : Math.Max(0, toolA - deltaTime);//toolA = Math.Clamp(showTool ? 1 : 0, toolA - deltaTime, toolA + deltaTime);
		toolTransientY = (MathF.Cos(toolA * MathF.PI)*.5f + .5f) * -toolH;
		tool.Location = new(0, (int)toolTransientY);
		// animate popup's X (along the shown/hidden axis, shown=hysteresisOff or hidden=hiddenPopup)
		popupA = showPopup ? Math.Min(1, popupA + deltaTime) : Math.Max(0, popupA - deltaTime);//Math.Clamp(showPopup ? 1 : 0, popupA - deltaTime, popupA + deltaTime);
		var cA = MathF.Cos(popupA * MathF.PI) * .5f + .5f;
		// set popup's target X 
		popupTransientX = Lerp(hysteresisOff, hiddenPopup, cA);
		// if the toolbar's width does not fit the windowWidth-popupWidth, then it will stretch over the whole windowWidth, and the popup.Y will be under it
		(int w, int y) = hysteresisOff < tool.MinimumSize.Width
			? (windowWidth, (int)toolTransientY + toolH)  // vertical (popup under tool)
			: ((int)popupTransientX, 0); // horizontal (tool stretched to popup)
		tool.Size = new(w, toolH);
		// move and resize the whole popup Panel
		popupPanel.Location = new((int)popupTransientX, y);
		popupPanel.Size = new(popupPanel.MinimumSize.Width, windowHeight - y);
		// Rescale the whole switchable parts of the popups (generator/editor)
		switchable.Size = new(switchable.Size.Width, Math.Min(
			popupPanel.Size.Height - switchable.Location.Y - 2, // maximum stretch from the bottom of the top controls, to the bottom of the window
			scrollable.PreferredSize.Height + scrollable.Location.Y + bottom.Size.Height + 6 // preferred size
			));
		// Move the bottom panels to be at the bottom the rescaled switachles
		bottom.Location = new(bottom.Location.X, switchable.Size.Height - bottom.Height - 2);
		// Resize the autoscrollable black panels (fractal settings / editable points)
		scrollable.Size = new(scrollable.Size.Width, bottom.Location.Y - scrollable.Location.Y - 2);

		debugPanel.Location = new(debugPanel.Location.X, switchable.Location.Y + switchable.Size.Height + 2);
		debugPanel.Size = new(debugPanel.Size.Width, popupPanel.Size.Height - debugPanel.Location.Y - 2);

		Size newMin = new(
			Math.Max(Math.Max(popupPanel.MinimumSize.Width, tool.MinimumSize.Width), width) + bw,
			Math.Max(popupPanel.MinimumSize.Height + tool.MinimumSize.Height, height) + bh
		//Math.Max(Math.Max(640, debugLabel.Bounds.Bottom + bh), bh + Math.Max(460, height + 8))
		);
		//bool changedSize = Size.Width < newMin.Width || Size.Height < newMin.Height;
		MinimumSize = newMin;
		//if (changedSize)
		//	ResizeScreen();
	}

}

/* Batching: (obsolete)
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
		myControlsEnabled.Add(c.Key.Enabled);
		c.Enabled = false;
	}
	NextBatch();
}*/