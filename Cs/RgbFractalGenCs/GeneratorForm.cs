// Starts the generator with special testing settings
//#define CUSTOMDEBUGTEST

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RgbFractalGenCs;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class GeneratorForm : Form {

	#region Designer
	public GeneratorForm() {
		InitializeComponent();
		screenPanel = new() {
			Location = new(239, 13),
			Name = "screenPanel",
			Size = new System.Drawing.Size(80, 80),
			TabIndex = 25
		};
		screenPanel.Paint += new(ScreenPanel_Paint);
		screenPanel.Click += new(AnimateButton_Click);
		Controls.Add(screenPanel);
	}
	#endregion

	#region Variables
	// Threading
	protected bool bInit = true;
	private readonly List<System.Windows.Forms.Control>
		MyControls = [];
	private readonly List<bool>
		MyControlsEnabled = [];
	internal FractalGenerator generator = new();// The core ofthe app, the generator the generates the fractal animations
	protected CancellationTokenSource cancel;   // Cancellation Token Source
	protected Task aTask = null;                // Abort Task
	protected bool queueAbort = false;          // Generator abortion queued
	protected short queueReset = 0;             // Counting time until generator Restart
	private Task gTask = null;                  // CPU gif thread

	// Settings
	protected bool animated = true;             // Animating preview or paused? (default animating)
	protected bool modifySettings = true;       // Allows for modifying settings without it triggering Aborts and Generates
	protected short width = -1, height = -1;
	protected int maxTasks = 0;                 // Maximum tasks available
	protected short abortDelay = 500;           // Set time to restart generator
	protected short restartTimer = 0;
	private string gifPath;                     // Gif export path name
	private Fractal tosave = null;

	// Display Variables
	internal readonly DoubleBufferedPanel
		screenPanel;                            // Display panel
	protected Bitmap currentBitmap = null;      // Displayed Bitmap
	protected int currentBitmapIndex;           // Play frame index
	protected int fx, fy;                       // Memory of window size
	protected int controlTabIndex = 0;          // Iterator for tabIndexes - to make sure all the controls tab in the correct order even as i add new ones in the middle
	protected int pointTabIndex = 0;

	// Editor
	private readonly List<(
		System.Windows.Forms.TextBox,
		System.Windows.Forms.TextBox,
		System.Windows.Forms.TextBox,
		System.Windows.Forms.Button,
		System.Windows.Forms.Button
		)> editorPoint = [];
	private readonly List<System.Windows.Forms.Button>
		editorSwitch = [];
	private FractalGenerator.ParallelType mem_parallel;
	private FractalGenerator.GenerationType mem_generate;
	private short mem_defaulthue, mem_hue;
	private bool performHash = false;

	#endregion

	#region Core
	void SetupEditControl(System.Windows.Forms.Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, tip);
		control.TabIndex = ++pointTabIndex;
		MyControls.Add(control);
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
			void SetupControl(System.Windows.Forms.Control control, string tip) {
				// Add tooltip and set the next tabIndex
				toolTips.SetToolTip(control, tip);
				control.TabIndex = ++controlTabIndex;
				MyControls.Add(control);
			}

			// Init the generator
			foreach (var i in generator.GetFractals())
				fractalSelect.Items.AddRange([i.name]);
			generator.selectFractal = -1;
			fractalSelect.SelectedIndex = 0;

			// Setup interactable controls (tooltips + tabIndex)
			SetupControl(sizeBox, "Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)");
			SetupControl(maxBox, "The root scale, if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance.\nIf pieces of the fractal are disappearing at the CORNERS, you should increase it, if not you can try decreasing a little.");
			SetupControl(minBox, "How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance).\n Try setting the detail parameter to maximum, and it the fractal starts dithering when zooming, you should decrease the minSize.");
			SetupControl(cutBox, "A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance).\nIf you see pieces of fractals diappearing near the EDGES, you should increase it.");
			SetupControl(angleBox, "Type the name for a new children angle set, if you wish to add one.");
			SetupControl(addAngleButton, "Add a new children angle set.");
			SetupControl(removeangleButton, "Remove the selected children angle set.");
			SetupControl(colorBox, "Type the name for a new children color set, if you wish to add one.");
			SetupControl(addcolorButton, "Add a new children color set.");
			SetupControl(removecolorButton, "Remove the selected children color set.");
			SetupControl(addCut, "Add a CutFunction (so far only the precoded are avaiable)");
			SetupControl(loadButton, "Load a fractal definition from a file.");
			SetupControl(saveButton, "Save the selected fractal definiton to a file.");


			SetupControl(fractalSelect, "Select the type of fractal to generate");
			SetupControl(modeButton, "Toggle between editor and generator.");
			SetupControl(angleSelect, "Select the children angles definition.");
			SetupControl(colorSelect, "Select the children colors definition.");
			SetupControl(cutSelect, "Select the cutfunction definition.");
			SetupControl(cutparamBox, "Type the cutfunction seed. (-1 for random)");
			SetupControl(resX, "Type the X resolution of the render (width)");
			SetupControl(resY, "Type the Y resolution of the render (height)");
			SetupControl(resSelect, "Select a rendering resolution (the second choise is the custom resolution you can type in the boxes to the left");
			SetupControl(periodBox, "How many frames for the self-similar center child to zoom in to the same size as the parent if you are zooming in.\nOr for the parent to zoom out to the same size as the child if you are zooming out.\nThis will equal the number of generated frames of the animation if the center child is the same color.\nOr it will be a third of the number of generated frames of the animation if the center child is a different color.");
			SetupControl(periodMultiplierBox, "Multiplies the frame count, slowing down the rotaion and hue shifts.");
			SetupControl(zoomSelect, "Choose in which direction you want the fractal zoom.");
			SetupControl(defaultZoom, "Type the initial zoom of the first image in number of skipped frames. -1 for random");
			SetupControl(spinSelect, "Choose in which direction you want the zoom animation to spin, or to not spin.");
			SetupControl(spinSpeedBox, "Type the extra speed on the spinning from the values possible for looping.");
			SetupControl(defaultAngle, "Type the initial angle of the first image (in degrees).");
			SetupControl(hueSelect, "Choose the color scheme of the fractal.\nAlso you can make the color hues slowly cycle as the fractal zoom.");
			SetupControl(hueSpeedBox, "Type the extra speed on the hue shifting from the values possible for looping.\nOnly possible if you have chosen color cycling on the left.");
			SetupControl(defaultHue, "Type the initial hue angle of the first image (in degrees).");
			SetupControl(ambBox, "The strength of the ambient grey color in the empty spaces far away between the generated fractal dots.\n-1 for transparent");
			SetupControl(noiseBox, "The strength of the random noise in the empty spaces far away between the generated fractal dots.");
			SetupControl(saturateBox, "Enhance the color saturation of the fractal dots.\nUseful when the fractal is too gray, like the Sierpinski Triangle (Triflake).");
			SetupControl(detailBox, "Level of Detail (The lower the finer).");
			SetupControl(bloomBox, "Bloom: 0 will be maximally crisp, but possibly dark with think fractals. Higher values wil blur/bloom out the fractal dots.");
			SetupControl(blurBox, "Motion blur: Lowest no blur and fast generation, highest 10 smeared frames 10 times slower generation.");
			SetupControl(brightnessBox, "Brightness level: 0% black, 100% normalized maximum, 300% overexposed 3x maximum.");
			SetupControl(voidBox, "Scale of the void noise, so it's more visible and compressable at higher resolutions.");
			SetupControl(parallelTypeSelect, "Select which parallelism to be used if the left checkBox is enabled.\nOf Animation = Batching animation frames, recommended for Animations with perfect pixels.\nOf Depth/Of Recursion = parallel single image generation, recommmended for fast single images, 1 in a million pixels might be slightly wrong");
			SetupControl(threadsBox, "The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
			SetupControl(abortBox, "How many millisecond of pause after the last settings change until the generator restarts?");
			SetupControl(delayBox, "A delay between frames in 1/100 of seconds for the preview and exported GIF file.\nThe framerate will be roughly 100/delay");
			SetupControl(prevButton, "Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(animateButton, "Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
			SetupControl(nextButton, "Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(restartButton, "Restarts the generaor. Maybe useful for randomized settings, but to be safe you have to click it twice in a row.");
			SetupControl(encodeSelect, "Only Image - only generates one image\nAnimation RAM - generated an animation without GIF encoding, faster but can't save GIF afterwards\nEncode GIF - encodes GIF while generating an animation - can save a GIF afterwards");
			SetupControl(helpButton, "Show README.txt.");
			SetupControl(pngButton, "Save the currently displayed frame into a PNG file.\nStop the animation and select the frame you wish to export with the buttons above.");
			SetupControl(gifButton, "Save the full animation into a GIF file.");
			SetupControl(debugBox, "shows a log of task and image states, to see what the generator is doing.");

			// Read the REDME.txt for the help button
			if (File.Exists("README.txt"))
				helpLabel.Text = File.ReadAllText("README.txt");

			// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restant the generator over and over
			resSelect.SelectedIndex = 0;
			AbortBox_TextChanged(null, null);
			PeriodBox_TextChanged(null, null);
			PeriodMultiplierBox_TextChanged(null, null);
			ParallelTypeSelect_SelectedIndexChanged(null, null);
			DelayBox_TextChanged(null, null);
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
			SetAnimate();
			parallelTypeSelect.SelectedIndex = 0;
			spinSelect.SelectedIndex = zoomSelect.SelectedIndex = hueSelect.SelectedIndex = 1;
			encodeSelect.SelectedIndex = 2;
			maxTasks = Math.Max(2, Environment.ProcessorCount - 2);

			SetupFractal();
			threadsBox.Text = maxTasks.ToString();

			// try to restory the last closed settings
			LoadSettings();

			bInit = modifySettings = helpPanel.Visible = false;
			pointTabIndex = controlTabIndex;

			// Init the editor:
			editorPanel.Location = generatorPanel.Location;
			editorPanel.Visible = false;
			FillEditor();

			

			// Start the generator
			TryResize();
			ResizeAll();
			aTask = gTask = null;
			generator.StartGenerate();

			// Load all extra fractal files
			string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // Get the app's directory
			string searchPattern = "*.fractal"; // Change to your desired file type
			string[] files = Directory.GetFiles(appDirectory, searchPattern);
			foreach (string file in files)
				_ = LoadFractal(file, false);

			// List all cutfunction to add in the editor
			addCut.Items.Add("Select CutFunction to Add");
			foreach (var c in Fractal.cutFunctions)
				addCut.Items.Add(c.Item1);
		}
		#endregion

		#region Size
		void SetMinimumSize() {
			// bw = Width - ClientWidth = 16
			// bh = Height - ClientHeight = 39
			int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			MinimumSize = new(
				Math.Max(640, bw + width + 284),
				Math.Max(Math.Max(640, debugLabel.Bounds.Bottom + bh), bh + Math.Max(460, height + 8))
			);
			//debugLabel.Text = debugLabel.Text + " " + MinimumSize.Height.ToString();
		}
		void WindowSizeRefresh() {
			if (fx == Width && fy == Height)
				return;
			// User has manually resized the window - strech the display
			ResizeScreen();
			SetMinimumSize();
			int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			SetClientSizeCore(
				Math.Max(Width - bw, 314 + Math.Max(screenPanel.Width, width)),
				Math.Max(Height - bh, 8 + Math.Max(screenPanel.Height, height))
			);
			SizeAdapt();
		}
		void ResizeAll() {
			generator.selectWidth = width;
			generator.selectHeight = height;
			generator.SetMaxIterations();
			// Update the size of the window and display
			SetMinimumSize();
			SetClientSizeCore(width + 314, Math.Max(height + 8, 300));
			ResizeScreen();
			WindowSizeRefresh();
#if CUSTOMDEBUGTEST
			generator.DebugStart(); animated = false;
#endif
			if (modifySettings)
				return;
			// Resets the generator
			// (Abort should be called before this or else it will crash)
			// generator.StartGenerate(); should be called after
			gifButton.Enabled = false;
			currentBitmapIndex = 0;
			generator.ResetGenerator();
			SizeAdapt();
		}
		void ResizeScreen() {
			int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			var screenHeight = Math.Max(height, Math.Min(Height - bh - 8, (Width - bw - 314) * height / width));
			screenPanel.SetBounds(305, 4, screenHeight * width / height, screenHeight);
			helpPanel.SetBounds(305, 4, Width - bw - 314, Height - bh - 8);
			screenPanel.Invalidate();
		}
		void SizeAdapt() {
			fx = Width;
			fy = Height;
		}
		#endregion

		if (bInit)
			Init();
		if (generator.debugmode) {
			debugLabel.Text = generator.debugString;
			SetMinimumSize();
		}
		// Window Size Update
		WindowSizeRefresh();
		if (queueReset > 0) {
			if (!(gTask == null && aTask == null))
				return;
			if (queueAbort) {
				aTask = Task.Run(Abort, (cancel = new()).Token);
				return;
			}
			if ((queueReset -= (short)timer.Interval) > 0)
				return;


			if (performHash) {
				// remember settings
				short mw = generator.selectWidth, mh = generator.selectHeight;
				var mg = generator.selectGenerationType;
				var mp = generator.selectParallelType;
				var mb = generator.selectBloom;
				var mbl = generator.selectBlur;
				var mbr = generator.selectBrightness;
				var ma = generator.selectAmbient;
				// hash settings
				generator.selectAmbient = 0;
				generator.selectBrightness = 100;
				generator.selectBlur = 0;
				generator.selectBloom = 0;
				generator.selectParallelType = FractalGenerator.ParallelType.OfAnimation;
				generator.selectWidth = generator.selectHeight = 20;
				generator.selectGenerationType = FractalGenerator.GenerationType.HashParam;

				SetupFractal();
				ResizeAll();
				restartButton.Enabled = true;
				ResetRestart();

				generator.StartGenerate();
				// Wait until finished
				while (generator.GetBitmapsFinished() < generator.GetFrames()) ;
				// collect the hashes
				int[] hash = new int[generator.hash.Count];
				int i = 0;
				foreach (var h in generator.hash)
					hash[i++] = h.Value;
				generator.GetFractal().cutFunction[^1] = (generator.GetFractal().cutFunction[^1].Item1, hash);
				// restore settings
				generator.selectAmbient = ma;
				generator.selectBrightness = mbr;
				generator.selectBlur = mbl;
				generator.selectBloom = mb;
				generator.selectParallelType = mp;
				generator.selectGenerationType = mg;
				generator.selectWidth = mw; generator.selectHeight = mh;
				// Restore enables
				for (i = MyControls.Count; 0 <= --i; MyControls[i].Enabled = MyControlsEnabled[i]) ;
				// restart generator
				performHash = false;
				QueueReset();
			} else {
				SetupFractal();
				ResizeAll();
				restartButton.Enabled = true;
				ResetRestart();
				generator.StartGenerate();
			}
		}
		if (restartTimer > 0 && (restartTimer -= (short)timer.Interval) <= 0)
			ResetRestart();
		// Fetch the state of generated bitmaps
		int bitmapsFinished = generator.GetBitmapsFinished(), bitmapsTotal = generator.GetFrames();
		if (bitmapsTotal <= 0)
			return;
		// Only Allow GIF Export when generation is finished
		if (gTask == null) {
			gifButton.Enabled = aTask == null && generator.IsGifReady();
			gifButton.Text = "Save GIF";
		} else if (gifButton.Text != "Cancel Saving GIF") {
			gifButton.Enabled = true;
			gifButton.Text = "Cancel Saving GIF";
		}
		// Handle currentBitmap
		if (bitmapsFinished > 0) {
			// Fetch bitmap, make sure the index is is range
			currentBitmapIndex %= bitmapsFinished;
			var bitmap = generator.GetBitmap(currentBitmapIndex);
			// Update the display with it if necessary
			if (currentBitmap != bitmap) {
				currentBitmap = bitmap;
				screenPanel.Invalidate();
			}
			// Animate the frame index
			if (animated)
				currentBitmapIndex = (currentBitmapIndex + 1) % bitmapsFinished;
		}
		// Info text refresh
		string infoText = " / " + bitmapsTotal.ToString();
		if (bitmapsFinished < bitmapsTotal) {
			statusLabel.Text = "Generating: ";
			infoText = bitmapsFinished.ToString() + infoText;
		} else {
			statusLabel.Text = "Finished: ";
			infoText = currentBitmapIndex.ToString() + infoText;
		}
		infoLabel.Text = infoText;
		gifButton.Text = gTask == null ? "Save GIF" : "Saving GIF...";
	}

	#endregion

	#region Input
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
		var c = "Custom:" + width.ToString() + "x" + height.ToString();
		if (resSelect.Items[1].ToString() != c) {
			resSelect.Items[1] = c;
		}
		//previewBox.Text = "Resolution: " + width.ToString() + "x" + height.ToString();
		//if (previewMode)
		//	width = height = 80;
		string[] rxy = resSelect.SelectedIndex is 1 or < 0 ? resSelect.Items[1].ToString().Split(':')[1].Split('x') : resSelect.Items[resSelect.SelectedIndex].ToString().Split('x');
		if (!short.TryParse(rxy[0], out width))
			width = 80;
		if (!short.TryParse(rxy[1], out height))
			height = 80;
		return generator.selectWidth != width || generator.selectHeight != height;
	}
	private void GeneratorForm_FormClosing(object sender, FormClosingEventArgs e) {
		Close(e);
	}
	private void Close(FormClosingEventArgs e) {
		if (gTask != null) {
			var result = MessageBox.Show(
				"Your GIF is still saving!\nAre you sure you want to close the application and potentially lose it?",
				"Confirm Exit",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			// Cancel closing if the user clicks "No"
			if (result == DialogResult.No)
				e.Cancel = true;
			return;
		}
		if (gifButton.Enabled && width > 80 && height > 80) {
			var result = MessageBox.Show(
				"You have encoded gif available to save.\nDo you want to save it?",
				"Confirm Exit",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			if (result == DialogResult.Yes) {
				saveGif.ShowDialog();
				e.Cancel = true;
				return;
			}
			
		}
		bool saved = false;
		foreach (var f in generator.GetFractals()) {
			if (f.edit) {
				var result = MessageBox.Show(
					"Fractal " + f.name + " has been edited. Do you want to save it before closing?",
					"Confirm Exit",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question);
				// Cancel closing if the user clicks "No"
				switch (result) {
					case DialogResult.Yes:
						tosave = f;
						saved = true;
						saveFractal.ShowDialog();
						continue;
					case DialogResult.No:
						f.edit = false;
						continue;
					case DialogResult.Cancel:
						e.Cancel = true;
						return;
				}
				return;
			}
		}
		if (saved) {
			e.Cancel = true;
			return;
		}
		
		cancel?.Cancel();
		gTask?.Wait();
		aTask?.Wait();
		Abort();
		SaveSettings();
	}
	protected void Abort() {
		queueAbort = false;
		// Cancel FractalGenerator threads
		generator.RequestCancel();
		/*foreach (var c in MyControls)
			c.Enabled = true;
		CutSelectEnabled(generator.GetFractal().cutFunction);
		FillCutParams();*/
		gifButton.Enabled = false;
		currentBitmapIndex = 0;
		aTask = null;
	}
	private void QueueReset(bool allow = true) {
		if (modifySettings || !allow)
			return;
		if (queueReset <= 0) {

			if (gifButton.Enabled && width > 80 && height > 80) {
				var result = MessageBox.Show(
					"You have encoded gif available to save.\nDo you want to save it?\nCacenl will turn off gif encoding so you won't keep getting this warning again.",
					"Saave GIF",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question);
				// Cancel closing if the user clicks "No"
				if (result == DialogResult.Yes)
					saveGif.ShowDialog();
				if (result == DialogResult.Cancel)
					encodeSelect.SelectedIndex = 1;
			}
			gifButton.Enabled = false;
			currentBitmapIndex = 0;

			if (TasksNotRunning())
				aTask = Task.Run(Abort, (cancel = new()).Token);
			else queueAbort = true;
		}
		ResetRestart();
		restartButton.Enabled = false;
		queueReset = abortDelay;
	}
	private void ResetRestart() {
		queueReset = restartTimer = 0;
		restartButton.Text = "! RESTART !";
	}
	private bool TasksNotRunning() => aTask == null && gTask == null;
	private static bool Clean(System.Windows.Forms.TextBox BOX) { string s = BOX.Text; s.Replace(';', ' '); s.Replace('|', ' '); s.Replace(':', ' ');  if (s != BOX.Text) { BOX.Text = s; return true; }; return false; }
	private static short ParseShort(System.Windows.Forms.TextBox BOX) { Clean(BOX);  return short.TryParse(BOX.Text, out var v) ? v : (short)0; }
	private static int ParseInt(System.Windows.Forms.TextBox BOX) { Clean(BOX); return int.TryParse(BOX.Text, out var v) ? v : 0; }
	private static float ParseFloat(System.Windows.Forms.TextBox BOX) { Clean(BOX); return float.TryParse(BOX.Text, out var v) ? v : 0.0f; }
	private static T Clamp<T>(T NEW, T MIN, T MAX) where T : struct, IComparable<T> => NEW.CompareTo(MIN) < 0 ? MIN : NEW.CompareTo(MAX) > 0 ? MAX : NEW;
	private static float Retext(System.Windows.Forms.TextBox BOX, float NEW) {
		BOX.Text = NEW == 0.0f ? (float.TryParse(BOX.Text, out _) ? "" : BOX.Text) : NEW.ToString("G6"); return NEW;
	}
	private static short Retext(System.Windows.Forms.TextBox BOX, short NEW) {
		BOX.Text = NEW == 0 ? (short.TryParse(BOX.Text, out _) ? "" : BOX.Text) : NEW.ToString(); return NEW;
	}
	private static int Retext(System.Windows.Forms.TextBox BOX, int NEW) {
		BOX.Text = NEW == 0 ? (int.TryParse(BOX.Text, out _) ? "" : BOX.Text) : NEW.ToString(); return NEW;
	}
	private static T Mod<T>(T NEW, T MIN, T MAX) where T : struct, IComparable<T> {
		var D = (dynamic)MAX - MIN; while (NEW.CompareTo(MIN) < 0) NEW = (T)(NEW + D); while (NEW.CompareTo(MAX) > 0) NEW = (T)(NEW - D); return NEW;
	}
	private static bool Diff<T>(T NEW, T GEN) where T : struct, IComparable<T> => GEN.CompareTo(NEW) == 0;
	private bool Apply<T>(T NEW, ref T GEN) { GEN = NEW; QueueReset(); return false; }
	private static short ParseClampRetext(System.Windows.Forms.TextBox BOX, short MIN, short MAX) => Retext(BOX, Clamp(ParseShort(BOX), MIN, MAX));
	private static int ParseClampRetext(System.Windows.Forms.TextBox BOX, int MIN, int MAX) => Retext(BOX, Clamp(ParseInt(BOX), MIN, MAX));
	private bool DiffApply<T>(T NEW, ref T GEN) where T : struct, IComparable<T> => Diff(NEW, GEN) || Apply(NEW, ref GEN);
	private bool ClampDiffApply<T>(T NEW, ref T GEN, T MIN, T MAX) where T : struct, IComparable<T> => DiffApply(Clamp(NEW, MIN, MAX), ref GEN);
	private bool ParseDiffApply(System.Windows.Forms.TextBox BOX, ref short GEN) => DiffApply(ParseShort(BOX), ref GEN);
	private bool ParseModDiffApply(System.Windows.Forms.TextBox BOX, ref short GEN, short MIN, short MAX) => DiffApply(Mod(ParseShort(BOX), MIN, MAX), ref GEN);
	private bool ParseRetextDiffApply(System.Windows.Forms.TextBox BOX, ref float GEN) => DiffApply(Retext(BOX, ParseFloat(BOX)), ref GEN);
	private bool ParseClampRetextDiffApply(System.Windows.Forms.TextBox BOX, ref short GEN, short MIN, short MAX) => DiffApply(ParseClampRetext(BOX, MIN, MAX), ref GEN);
	private bool ParseClampRetextDiffApply(System.Windows.Forms.TextBox BOX, ref int GEN, int MIN, int MAX) => DiffApply(ParseClampRetext(BOX, MIN, MAX), ref GEN);
	private bool ParseClampRetextMulDiffApply(System.Windows.Forms.TextBox BOX, ref short GEN, short MIN, short MAX, short MUL) => DiffApply((short)(ParseClampRetext(BOX, MIN, MAX) * MUL), ref GEN);
	private bool ParseClampRetextMulDiffApply(System.Windows.Forms.TextBox BOX, ref float GEN, short MIN, short MAX, float MUL) => DiffApply(ParseClampRetext(BOX, MIN, MAX) * MUL, ref GEN);
	protected void FractalSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (generator.SelectFractal((short)Math.Max(0, fractalSelect.SelectedIndex)))
			return;
		// Fractal is different - load it, change the setting and restart generation
		// Fill the fractal's adjuistable definition combos
		// Fill the fractal's adjuistable cutfunction seed combos, and restart generation
		SetupSelects();
		FillEditor();
	}
	private void SetupSelects() {
		void FillSelects() {
			var f = generator.GetFractal();
			// Fill angle childred definitnions combobox
			angleSelect.Items.Clear();
			foreach (var (name, _) in f.childAngle)
				angleSelect.Items.AddRange([name]);
			angleSelect.SelectedIndex = 0;
			// Fill color children definitnions combobox
			colorSelect.Items.Clear();
			foreach (var (name, _) in f.childColor)
				colorSelect.Items.AddRange([name]);
			colorSelect.SelectedIndex = 0;
			// Fill cutfunction definitnions combobox
			cutSelect.Items.Clear();
			var cf = f.cutFunction;
			if (CutSelectEnabled(cf)) {
				foreach (var (index, _) in cf)
					cutSelect.Items.Add(Fractal.cutFunctions[index].Item1);
				cutSelect.SelectedIndex = 0;
			}
		}
		if (!modifySettings) {
			modifySettings = true;
			FillSelects();
			FillCutParams();
			cutparamBox.Text = "0";
			modifySettings = false;
			QueueReset();
		} else {
			FillSelects();
			FillCutParams();
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
		generator.SetupAngle();
		generator.SetupCutFunction();
	}

	private void SetupParallel(short newThreads) {
		generator.selectMaxTasks = (short)(newThreads > 1 ? newThreads : 2);
		generator.SelectThreadingDepth();
	}
	private void AngleSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (DiffApply((short)Math.Max(0, angleSelect.SelectedIndex), ref generator.selectChildAngle))
			return;
		SwitchChildAngle();

	}
	private void ColorSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (DiffApply((short)Math.Max(0, colorSelect.SelectedIndex), ref generator.selectChildColor))
			return;
		SwitchChildColor();
	}
	private void CutSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (!DiffApply((short)Math.Max(0, cutSelect.SelectedIndex), ref generator.selectCut)) FillCutParams();
	}
	private void CutparamBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(cutparamBox, ref generator.selectCutparam, -1, generator.GetMaxCutparam());
	private bool CutSelectEnabled(List<(int, int[])> cf) => cutSelect.Enabled = cf != null && cf.Count > 0;
	// Query the number of seeds from the CutFunction
	private bool CutParamBoxEnabled(Fractal.CutFunction cf) => cutparamBox.Enabled = 0 < (generator.cutparamMaximum = (int)(cf == null || cf(0, -1, generator.GetFractal()) <= 0 ? 0 : (cf(0, 1 - (1 << 30), generator.GetFractal()) + 1) / cf(0, -1, generator.GetFractal())));
	/*{
		if (cf == null || cf(0, -1, generator.GetFractal()) <= 0) {
			generator.cutparamMaximum = 0;
		} else {
			long n = cf(0, 1 - (1 << 32), generator.GetFractal()) + 1;
			long d = cf(0, -1, generator.GetFractal());
			generator.cutparamMaximum = (int)(n / d);
		}
		
		return cutparamBox.Enabled = 0 < (generator.cutparamMaximum);
	}*/


	//	=> cutparamBox.Enabled = 0 < (generator.cutparamMaximum = (short)(cf == null || cf(0, -1, generator.GetFractal()) <= 0 ? 0 : (cf(0, 1 - (1 << 16), generator.GetFractal()) + 1) / cf(0, -1, generator.GetFractal())));
	/// <summary>
	/// Fill the cutFunction seed parameter comboBox with available options for the selected CutFunction
	/// </summary>
	private void FillCutParams() { 
		CutParamBoxEnabled(generator.GetCutFunction());
		cutparamBox.Text = "0";
	}
	private void Resolution_Changed(object sender, EventArgs e) => QueueReset(TryResize());
	private void PeriodBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(periodBox, ref generator.selectPeriod, -1, 1000);
	private void PeriodMultiplierBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(periodMultiplierBox, ref generator.selectPeriodMultiplier, 1, 10);
	private void ZoomSelect_SelectedIndexChanged(object sender, EventArgs e) => DiffApply((short)((zoomSelect.SelectedIndex + 1) % 3 - 1), ref generator.selectZoom);
	private void DefaultZoom_TextChanged(object sender, EventArgs e) => ParseDiffApply(defaultZoom, ref generator.selectDefaultZoom);
	private void SpinSelect_SelectedIndexChanged(object sender, EventArgs e) => ClampDiffApply((short)(spinSelect.SelectedIndex - 2), ref generator.selectSpin, (short)-2, (short)2);
	private void SpinSpeedBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(spinSpeedBox, ref generator.selectExtraSpin, 0, 255);
	private void DefaultAngle_TextChanged(object sender, EventArgs e) => ParseModDiffApply(defaultAngle, ref generator.selectDefaultAngle, 0, 360);
	private void HueSelect_SelectedIndexChanged(object sender, EventArgs e) => DiffApply((short)(hueSelect.SelectedIndex == 0 ? -1 : (hueSelect.SelectedIndex - 1) % 6), ref generator.selectHue);
	private void HueSpeedBox_TextChanged(object sender, EventArgs e) {
		short newSpeed = ParseClampRetext(hueSpeedBox, (short)0, (short)255);
		if (Diff(newSpeed, generator.selectExtraHue))
			return;
		// hue speed is different - change the setting and if it's actually huecycling restart generation
		if (generator.selectHue is not 0 and not 1)
			Apply(newSpeed, ref generator.selectExtraHue);
		else generator.selectExtraHue = newSpeed;
	}
	private void DefaultHue_TextChanged(object sender, EventArgs e) => ParseModDiffApply(defaultHue, ref generator.selectDefaultHue, 0, 360);
	private void AmbBox_TextChanged(object sender, EventArgs e) => ParseClampRetextMulDiffApply(ambBox, ref generator.selectAmbient, -1, 30, 4);
	private void NoiseBox_TextChanged(object sender, EventArgs e) => ParseClampRetextMulDiffApply(noiseBox, ref generator.selectNoise, 0, 30, .1f);
	private void SaturateBox_TextChanged(object sender, EventArgs e) => ParseClampRetextMulDiffApply(saturateBox, ref generator.selectSaturate, 0, 10, .1f);
	private void DetailBox_TextChanged(object sender, EventArgs e) {
		if (!ParseClampRetextMulDiffApply(detailBox, ref generator.selectDetail, 0, 10, .1f * generator.GetFractal().minSize)) generator.SetMaxIterations();
	}
	private void BloomBox_TextChanged(object sender, EventArgs e) => ParseClampRetextMulDiffApply(bloomBox, ref generator.selectBloom, 0, 40, .25f);
	private void BlurBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(blurBox, ref generator.selectBlur, 0, 40);
	private void BrightnessBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(brightnessBox, ref generator.selectBrightness, 0, 300);
	private void VoidBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(voidBox, ref generator.selectVoid, 0, 300);

	/*private void Parallel_Changed(object sender, EventArgs e) {
		SetupParallel((short)(Environment.ProcessorCount - 2));
	}*/
	private void Parallel_Changed(object sender, EventArgs e) {
		SetupParallel(ParseClampRetext(threadsBox, (short)2, (short)maxTasks));
	}
	private void ParallelTypeSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if ((FractalGenerator.ParallelType)parallelTypeSelect.SelectedIndex == FractalGenerator.ParallelType.OfDepth) {
			MessageBox.Show(
				"Sorry but this parallelism mode is currently broken and unavailable, try again in a later release.",
				"Unavailable",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return;
		}
		generator.selectParallelType = (FractalGenerator.ParallelType)parallelTypeSelect.SelectedIndex;
	}
	private void AbortBox_TextChanged(object sender, EventArgs e) => abortDelay = ParseClampRetext(abortBox, (short)0, (short)10000);
	private void DelayBox_TextChanged(object sender, EventArgs e) {
		var newDelay = ParseClampRetext(delayBox, (short)1, (short)500);
		if (generator.selectDelay == newDelay)
			return;
		// Delay is diffenret, change it, and restart the generation if ou were encoding a gif
		generator.selectDelay = newDelay;
		var fpsrate = 100 / generator.selectDelay;
		timer.Interval = generator.selectDelay * 10;
		delayLabel.Text = "Abort / FPS: " + fpsrate.ToString();
		if (generator.selectGenerationType >= FractalGenerator.GenerationType.EncodeGIF)
			QueueReset();
	}
	private void MoveFrame(int move) { animated = false; var b = generator.GetBitmapsFinished(); currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + b + move) % b; }
	private void PrevButton_Click(object sender, EventArgs e) => MoveFrame(-1);
	private void AnimateButton_Click(object sender, EventArgs e) {
		animateButton.Text = (animated = !animated) ? "Playing" : "Paused";
		SetAnimate();
	}
	private void SetAnimate() => animateButton.BackColor = animated ? Color.FromArgb(128, 255, 128) : Color.FromArgb(255, 128, 128);
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
		QueueReset(true);
	}
	private void EncodeSelect_SelectedIndexChanged(object sender, EventArgs e) {
		var prev = generator.selectGenerationType;
		if ((generator.selectGenerationType = (FractalGenerator.GenerationType)Math.Max(0, encodeSelect.SelectedIndex)) >= FractalGenerator.GenerationType.EncodeGIF
			&& (!generator.IsGifReady() || prev != generator.selectGenerationType)
		) QueueReset();
	}
	private void HelpButton_Click(object sender, EventArgs e) {
		helpPanel.Visible = screenPanel.Visible;
		screenPanel.Visible = !screenPanel.Visible;
	}
	private void PngButton_Click(object sender, EventArgs e) => savePng.ShowDialog();
	private void GifButton_Click(object sender, EventArgs e) => saveGif.ShowDialog();
	private void DebugBox_CheckedChanged(object sender, EventArgs e) {
		if (!(generator.debugmode = debugBox.Checked))
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
		byte tryAttempt = 0;
		while (tryAttempt < 5) {
			try {
				e.Graphics.DrawImage(currentBitmap, new System.Drawing.Rectangle(0, 0, screenPanel.Width, screenPanel.Height));
				tryAttempt = 5;
			} catch (Exception) {
				++tryAttempt;
				Thread.Sleep(100);
			}
		}
	}
	/// <summary>
	/// User inputed the path and name for saving PNG
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void SavePng_FileOk(object sender, CancelEventArgs e) {
		Stream myStream;
		if ((myStream = savePng.OpenFile()) != null) {
			currentBitmap.Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
			myStream.Close();
		}
	}
	/// <summary>
	/// User inputed the path and name for saving GIF
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void SaveGif_FileOk(object sender, CancelEventArgs e) {
		gifButton.Enabled = false;
		if (gTask != null) {
			cancel?.Cancel();
			return;
		}
		gifPath = ((SaveFileDialog)sender).FileName;
		// Gif Export Task
		//foreach (var c in MyControls)c.Enabled = false;
		gTask = Task.Run(ExportGif, (cancel = new()).Token);
	}
	/// <summary>
	/// Exports the animation into a GIF file
	/// Ackchyually - it just moves the already exported gifX.tmp to you desired location and name
	/// </summary>
	/// <returns></returns>
	private void ExportGif() {
		var attempt = 0;
		while (++attempt <= 10 && !cancel.Token.IsCancellationRequested && generator.SaveGif(gifPath))
			Thread.Sleep(1000);
		gTask = null;
	}
	#endregion

	#region Editor
	private void FillEditor() {
		pointTabIndex = controlTabIndex;
		pointPanel.SuspendLayout();
		UnfillEditor();
		var f = generator.GetFractal();
		for (int i = 0; i < f.childCount; ++i) {
			AddEditorPoint(f.childX, f.childY, f.childAngle[generator.selectChildAngle].Item2, f.childColor[generator.selectChildColor].Item2, false);
		}
		bool e = f.edit;
		sizeBox.Text = f.childSize.ToString();
		cutBox.Text = f.cutSize.ToString();
		minBox.Text = f.minSize.ToString();
		maxBox.Text = f.maxSize.ToString();
		f.edit = e;
		pointPanel.ResumeLayout(false);
		pointPanel.PerformLayout();

	}
	private void UnfillEditor() {
		foreach (var s in editorSwitch) {
			pointPanel.Controls.Remove(s);
			MyControls.Remove(s);
		}
		foreach ((var x, var y, var a, var c, var d) in editorPoint) {
			pointPanel.Controls.Remove(x);
			pointPanel.Controls.Remove(y);
			pointPanel.Controls.Remove(a);
			pointPanel.Controls.Remove(c);
			pointPanel.Controls.Remove(d);
			MyControls.Remove(x);
			MyControls.Remove(y);
			MyControls.Remove(a);
			MyControls.Remove(c);
			MyControls.Remove(d);
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
		UnfillEditor();
		FillEditor();

		var f = generator.GetFractal();
		var e = f.edit;
		for (int i = 0; i < f.childCount; ++i)
			editorPoint[i].Item3.Text = f.childAngle[generator.selectChildAngle].Item2[i].ToString();
		f.edit = e;
	}
	private void SwitchChildColor() {
		//UnfillEditor();
		//FillEditor();
		var f = generator.GetFractal();
		var e = f.edit;
		for (int i = 0; i < f.childCount; ++i)
			editorPoint[i].Item4.BackColor = f.childColor[generator.selectChildColor].Item2[i] switch {
				0 => Color.Red,
				1 => Color.Green,
				_ => Color.Blue
			};
		f.edit = e;
	}
	private void ModeButton_Click(object sender, EventArgs e) {
		if (editorPanel.Visible) {
			// SelectParallelMode();
			generator.selectParallelType = mem_parallel;
			generator.selectGenerationType = mem_generate;
			generator.selectHue = mem_hue;
			generator.selectDefaultHue = mem_defaulthue;
		} else {
			// TODO set this after OfDepth mode is fixed
			mem_parallel = generator.selectParallelType;
			mem_generate = generator.selectGenerationType;
			mem_hue = generator.selectHue;
			mem_defaulthue = generator.selectDefaultHue;
			//generator.selectParallelType = FractalGenerator.ParallelType.OfDepth;
			generator.selectGenerationType = FractalGenerator.GenerationType.AnimationRAM;
			generator.selectHue = 0;
		}
		editorPanel.Visible = !(generatorPanel.Visible = editorPanel.Visible);
	}
	private void AddEditorPoint(float[] cx, float[] cy, float[] ca, byte[] cc, bool single = true) {
		int i = editorPoint.Count;
		editorPoint.Add((new(), new(), new(), new(), new()));
		(var x, var y, var a, var c, var d) = editorPoint[i];
		x.Text = cx[i].ToString();
		y.Text = cy[i].ToString();
		a.Text = ca[i].ToString();
		c.Text = "";
		d.Text = "X";
		c.BackColor = cc[i] switch {
			0 => Color.Red,
			1 => Color.Green,
			_ => Color.Blue
		};
		BindPoint(x, y, a, c, d, i, single);
	}
	private void BindPoint(
		System.Windows.Forms.TextBox x,
		System.Windows.Forms.TextBox y,
		System.Windows.Forms.TextBox a,
		System.Windows.Forms.Button c,
		System.Windows.Forms.Button d,
		int i, bool single = true
	) {
		int textsize = 53;
		int butsize = 23;
		if (single)
			pointPanel.SuspendLayout();
		if (i > 1) {
			editorSwitch.Add(new());
			var s = editorSwitch[^1];
			s.Text = "⇕";
			s.Location = new System.Drawing.Point(10 + 3 * textsize + 2 * butsize, 30 + (i * 2 - 1) * butsize / 2);
			s.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			s.Name = "switch" + i;
			s.Size = new System.Drawing.Size(butsize, butsize);
			//s.TabIndex = ++pointTabIndex;
			pointPanel.Controls.Add(s);
			MyControls.Add(s);
			SetupEditControl(s, "Switch these two points in the list. (Only has effects on cutfunctions, or if you switch the main first one.)");
			s.Click += (object sender, EventArgs e) => {
				var f = generator.GetFractal();
				var ni = f.childCount;
				var cx = f.childX;
				var cy = f.childY;
				var ca = f.childAngle[generator.selectChildAngle].Item2;
				var cc = f.childColor[generator.selectChildColor].Item2;
				(cx[i], cx[i - 1], cy[i], cy[i - 1], ca[i], ca[i - 1], cc[i], cc[i - 1])
					= (cx[i - 1], cx[i], cy[i - 1], cy[i], ca[i - 1], ca[i], cc[i - 1], cc[i]);
				var (xi, yi, ai, ci, _) = editorPoint[i];
				xi.Text = cx[i].ToString();
				yi.Text = cy[i].ToString();
				ai.Text = ca[i].ToString();
				ci.BackColor = cc[i] switch {
					0 => Color.Red,
					1 => Color.Green,
					_ => Color.Blue
				};
				var (xi1, yi1, ai1, ci1, _) = editorPoint[i - 1];
				xi1.Text = cx[i - 1].ToString();
				yi1.Text = cy[i - 1].ToString();
				ai1.Text = ca[i - 1].ToString();
				ci1.BackColor = cc[i - 1] switch {
					0 => Color.Red,
					1 => Color.Green,
					_ => Color.Blue
				};
				f.edit = true;
				QueueReset();
			};
		}
		pointPanel.Controls.Add(x);
		pointPanel.Controls.Add(y);
		pointPanel.Controls.Add(a);
		pointPanel.Controls.Add(c);
		pointPanel.Controls.Add(d);
		MyControls.Add(x);
		SetupEditControl(x, "X coordinate of the child.");
		MyControls.Add(y);
		SetupEditControl(y, "Y coordinate of the child.");
		MyControls.Add(a);
		SetupEditControl(a, "Angle of the child.");
		MyControls.Add(c);
		SetupEditControl(c, "Color shift of the child (red = no shift, green = shift forward RGB->GBR, blue = shift backwards RGB->BRG)");
		MyControls.Add(d);
		SetupEditControl(d, "Remove this point.");
		if (single) {
			pointPanel.ResumeLayout(false);
			pointPanel.PerformLayout();
		}
		x.Location = new System.Drawing.Point(10, 30 + i * butsize);
		x.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
		x.Name = "x" + i;
		x.Size = new System.Drawing.Size(textsize, butsize);
		//x.TabIndex = ++pointTabIndex;
		if (x.Enabled = i > 0)
			x.TextChanged += (object sender, EventArgs e)
				=> { 
					if (ParseRetextDiffApply((System.Windows.Forms.TextBox)sender, ref generator.GetFractal().childX[i])) return; 
					generator.GetFractal().edit = true; 
				};

		y.Location = new System.Drawing.Point(10 + textsize, 30 + i * butsize);
		y.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
		y.Name = "y" + y;
		y.Size = new System.Drawing.Size(textsize, butsize);
		//y.TabIndex = ++pointTabIndex;
		if (y.Enabled = i > 0)
			y.TextChanged += (object sender, EventArgs e) => { if (ParseRetextDiffApply((System.Windows.Forms.TextBox)sender, ref generator.GetFractal().childY[i])) return;
				generator.GetFractal().edit = true; 
			};

	a.Location = new System.Drawing.Point(10 + 2 * textsize, 30 + i * butsize);
		a.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
		a.Name = "a" + i;
		a.Size = new System.Drawing.Size(textsize, butsize);
		//a.TabIndex = ++pointTabIndex;
		a.TextChanged += (object sender, EventArgs e) => { if (ParseRetextDiffApply((System.Windows.Forms.TextBox)sender, ref generator.GetFractal().childAngle[generator.selectChildAngle].Item2[i])) return;
			generator.GetFractal().edit = true;
		};

c.Location = new System.Drawing.Point(10 + 3 * textsize, 30 + i * butsize);
		c.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
		c.Name = "c" + i;
		c.Size = new System.Drawing.Size(butsize, butsize);
		//c.TabIndex = ++pointTabIndex;
		c.Click += (object sender, EventArgs e) => {
			byte color = generator.GetFractal().childColor[generator.selectChildColor].Item2[i];
			generator.GetFractal().childColor[generator.selectChildColor].Item2[i] = color = (byte)((color + 1) % 3);
			c.BackColor = color switch {
				0 => Color.Red,
				1 => Color.Green,
				_ => Color.Blue
			};
			generator.GetFractal().edit = true;
			QueueReset();
		};
		d.Location = new System.Drawing.Point(10 + 3 * textsize + butsize, 30 + i * butsize);
		d.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
		d.Name = "d" + i;
		d.Size = new System.Drawing.Size(23, 23);
		//d.TabIndex = ++pointTabIndex;
		if (d.Enabled = i > 0)
			d.Click += (object sender, EventArgs e) => {
				var f = generator.GetFractal();
				var ni = --f.childCount;
				float[] nx = new float[ni];
				float[] ny = new float[ni];
				float[] na = new float[ni];
				byte[] nc = new byte[ni];
				var cx = f.childX;
				var cy = f.childY;
				var ca2 = f.childAngle[generator.selectChildAngle];
				var ca = ca2.Item2;
				var cc2 = f.childColor[generator.selectChildColor];
				var cc = cc2.Item2;
				
				pointPanel.SuspendLayout();
				UnfillEditor();
				pointPanel.ResumeLayout(false);
				for (int ci = 0; ci < i; ++ci) {
					nx[ci] = cx[ci];
					ny[ci] = cy[ci];
					na[ci] = ca[ci];
					nc[ci] = cc[ci];
					AddEditorPoint(nx, ny, na, nc);
				}
				for (int ci = i; ci < ni;) {
					var xp = ci + 1;
					nx[ci] = cx[xp];
					ny[ci] = cy[xp];
					na[ci] = ca[xp];
					nc[ci] = cc[xp];
					ci = xp;
					AddEditorPoint(nx, ny, na, nc);
				}
				f.childX = nx;
				f.childY = ny;
				f.childAngle[generator.selectChildAngle] = (ca2.Item1, na);
				f.childColor[generator.selectChildColor] = (cc2.Item1, nc);
				f.edit = true;
				QueueReset();
			};
		addPoint.Location = new(10, 30 + (i + 1) * butsize);
	}
	private void AddPoint_Click(object sender, EventArgs e) {
		int i = editorPoint.Count;
		editorPoint.Add((new(), new(), new(), new(), new()));
		(var x, var y, var a, var c, var d) = editorPoint[i];
		x.Text = y.Text = a.Text = "0";
		d.Text = "X";
		c.Text = "";
		c.BackColor = Color.Red;

		var f = generator.GetFractal();
		var ni = f.childCount++;
		if (generator.childColor.Length < f.childCount)
			generator.childColor = new byte[f.childCount];
		float[] nx = new float[ni + 1];
		float[] ny = new float[ni + 1];
		float[] na = new float[ni + 1];
		byte[] nc = new byte[ni + 1];
		var cx = f.childX;
		var cy = f.childY;
		var ca2 = f.childAngle[generator.selectChildAngle];
		var ca = ca2.Item2;
		var cc2 = f.childColor[generator.selectChildColor];
		var cc = cc2.Item2;
		for (int ci = 0; ci < ni; ++ci) {
			nx[ci] = cx[ci];
			ny[ci] = cy[ci];
			na[ci] = ca[ci];
			nc[ci] = cc[ci];
		}
		nx[ni] = ny[ni] = na[ni] = nc[ni] = 0;
		f.childX = nx;
		f.childY = ny;
		f.childAngle[generator.selectChildAngle] = (ca2.Item1, na);
		f.childColor[generator.selectChildColor] = (cc2.Item1, nc);
		BindPoint(x, y, a, c, d, i, true);
		f.edit = true;
		QueueReset();
	}
	private void LoadButton_Click(object sender, EventArgs e) {
		_ = loadFractal.ShowDialog();
	}
	private void SaveButton_Click(object sender, EventArgs e) {
		tosave = generator.GetFractal();
		_ = saveFractal.ShowDialog();
	}
	private void FractalSelect_TextUpdate(object sender, EventArgs e) {
		// TODO verify it actually gets selected
		if (fractalSelect.Items.Contains(fractalSelect.Text))
			fractalSelect.SelectedIndex = fractalSelect.Items.IndexOf(fractalSelect.Text);
		else if (File.Exists(fractalSelect.Text + ".fractal"))
			_ = LoadFractal(fractalSelect.Text + ".fractal");
	}
	private void LoadFractal_FileOk(object sender, CancelEventArgs e) {
		var filename = ((OpenFileDialog)sender).FileName;
		var fracname = filename[..^8];
		int i;
		while ((i = fracname.IndexOf('/')) >= 0)
			fracname = fracname[(i + 1)..];
		while ((i = fracname.IndexOf('\\')) >= 0)
			fracname = fracname[(i + 1)..];
		fracname.Replace(';', '.');
		fracname.Replace(':', '.');
		fracname.Replace('|', '.');
		if (fractalSelect.Items.Contains(fracname)) {
			_ = Error("There already is a loaded fractal of the same name, selecting it.", "Already exists");
			fractalSelect.SelectedIndex = fractalSelect.Items.IndexOf(fracname);
		} else if (File.Exists(filename))
			_ = LoadFractal(filename);
	}
	private void SaveFractal_FileOk(object sender, CancelEventArgs e) {
		var f = tosave;
		string fracname = f.path = ((SaveFileDialog)sender).FileName;
		int index;
		while ((index = fracname.IndexOf('/')) >= 0)
			fracname = fracname[(index + 1)..];
		while ((index = fracname.IndexOf('\\')) >= 0)
			fracname = fracname[(index + 1)..];
		fracname.Replace(';', '.');
		fracname.Replace(':', '.');
		fracname.Replace('|', '.');
		f.name = fracname;
		// Consts
		string p, a, s = f.name + "|" + f.childCount + "|" + f.childSize + "|" + f.maxSize + "|" + f.minSize + "|" + f.cutSize;
		// XY
		p = "|";
		foreach (var x in f.childX)
			p += x + ";";
		s += p[..^1];
		p = "|";
		foreach (var x in f.childY)
			p += x + ";";
		s += p[..^1];
		// Angles
		p = "|";
		foreach (var (an, ai) in f.childAngle) {
			a = an + ":";
			foreach (var i in ai)
				a += i + ":";
			p += a[..^1] + ";";
		}
		s += p[..^1];
		// Colors
		p = "|";
		foreach (var (an, ai) in f.childColor) {
			a = an + ":";
			foreach (var i in ai)
				a += i + ":";
			p += a[..^1] + ";";
		}
		s += p[..^1];
		// Cuts
		p = "|";
		foreach (var (an, ai) in f.cutFunction) {
			a = an + ":";
			foreach (var i in ai)
				a += i + ":";
			p += (a != "" ? a[..^1] : "") + ";";
		}
		s += p == "|" ? "|" : p[..^1];
		File.WriteAllText(f.path, s);
		f.edit = false;
	}
	private void SaveSettings() {
		var f = generator.GetFractal();
		File.WriteAllText("settings.txt", "fractal|" + fractalSelect.Text + "|path|" + f.path + "|edit|" + (editorPanel.Visible ? 1 : 0) + "|angle|" + angleSelect.SelectedIndex + "|color|" + colorSelect.SelectedIndex + "|cut|" + cutSelect.SelectedIndex + "|seed|" + cutparamBox.Text
			+ "|w|" + resX.Text + "|h|" + resY.Text + "|res|" + resSelect.SelectedIndex + "|period|" + periodBox.Text + "|periodmul|" + periodMultiplierBox.Text + "|zoom|" + zoomSelect.SelectedIndex + "|defaultzoom|" + defaultZoom.Text
			+ "|spin|" + spinSelect.SelectedIndex + "|spinmul|" + spinSpeedBox.Text + "|defaultangle|" + defaultAngle.Text + "|hue|" + hueSelect.SelectedIndex + "|huemul|" + hueSpeedBox.Text + "|defaulthue|" + defaultHue.Text + "|amb|" + ambBox.Text
			+ "|noise|" + noiseBox.Text + "|saturate|" + saturateBox.Text + "|detail|" + detailBox.Text + "|bloom|" + bloomBox.Text + "|blur|" + blurBox.Text + "|brightness|" + brightnessBox.Text + "|parallel|" + parallelTypeSelect.SelectedIndex
			+ "|threads|" + threadsBox.Text + "|abort|" + abortBox.Text + "|delay|" + delayBox.Text + "|ani|" + (animated ? 1 : 0) + "|gen|" + encodeSelect.SelectedIndex);
	}
	private void LoadSettings() {
		gifButton.Enabled = false;
		if (!File.Exists("settings.txt"))
			return;
		var s = File.ReadAllText("settings.txt").Split('|');
		for (int i = 0; i < s.Length - 1; i += 2) {
			string v = s[i+1];
			bool p = int.TryParse(v, out int n);
			switch (s[i]) {
				case "path": if (v != "" && File.Exists(v)) _ = LoadFractal(v, true); break;
				case "fractal": fractalSelect.SelectedItem = v; break;
				case "edit": if(p) generatorPanel.Visible = !(editorPanel.Visible = n > 0); break;
				case "angle": angleSelect.SelectedIndex = n; break;
				case "color": colorSelect.SelectedIndex = n; break;
				case "cut": cutSelect.SelectedIndex = n;	break;
				case "seed": cutparamBox.Text = v; break;
				case "w": width = (short)n; break;
				case "h": height = (short)n; break;
				case "res": resSelect.SelectedIndex = n; break;
				case "period": periodBox.Text = v; break;
				case "periodmul": periodMultiplierBox.Text = v; break;
				case "zoom": zoomSelect.SelectedIndex = n; break;
				case "defaultzoom": defaultZoom.Text = v; break;
				case "spin": spinSelect.SelectedIndex = n; break;
				case "spinmul": spinSpeedBox.Text = v; break;
				case "defaultangle": defaultAngle.Text = v; break;
				case "hue": hueSelect.SelectedIndex = n; break;
				case "huemul": hueSpeedBox.Text = v; break;
				case "defaulthue": defaultHue.Text = v; break;
				case "amb": ambBox.Text = v; break;
				case "noise": noiseBox.Text = v; break;
				case "saturate": saturateBox.Text = v; break;
				case "detail": detailBox.Text = v; break;
				case "bloom": bloomBox.Text = v; break;	
				case "blur": blurBox.Text = v; break;
				case "brightness": brightnessBox.Text = v; break;
				case "parallel": parallelTypeSelect.SelectedIndex = n; break;
				case "threads": threadsBox.Text = v; break;
				case "abort": abortBox.Text = v; break;
				case "delay": delayBox.Text = v; break;
				case "ani": if (p) animated = i <= 0; AnimateButton_Click(null, null); break;
				case "gen": encodeSelect.SelectedIndex = n; break;
			}
		}
		SetupFractal();
	}
	private bool Error(string text, string caption) {
		_ = MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		return false;
	}
	private bool LoadFractal(string file, bool select = true) {
		if (!File.Exists(file))
			return Error("No such file", "Cannot load");
		string content = File.ReadAllText(file);
		string[] arr = content.Split('|');
		// Name
		if (arr[0] == "")
			return Error("No fractal name", "Cannot load");
		if (fractalSelect.Items.Contains(fractalSelect.Text))
			return Error("Fractal with the same name already loaded in the list.", "Cannot load");
		// Consts
		if (!int.TryParse(arr[1], out int count) || count < 1)
			return Error("Invalid children count: " + count, "Cannot load");
		if (!float.TryParse(arr[2], out float size) || size <= 1)
			return Error("Invalid children size: " + size, "Cannot load");
		if (!float.TryParse(arr[3], out float maxsize))
			return Error("Invalid max size: " + maxsize, "Cannot load");
		if (!float.TryParse(arr[4], out float minsize))
			return Error("Invalid min size: " + minsize, "Cannot load");
		if (!float.TryParse(arr[5], out float cutsize))
			return Error("Invalid cut size: " + cutsize, "Cannot load");

		float x;
		// ChildX
		string[] s = arr[6].Split(';');
		if (s.Length < count)
			return Error("Insufficent children X count: " + s.Length + " / " + count, "Cannot load");
		float[] childX = new float[count];
		for (int i = count; --i >= 0; childX[i] = x)
			if (!float.TryParse(s[i], out x))
				return Error("Invalid child X: " + s[i], "Cannot load");
		// ChildY
		s = arr[7].Split(';');
		if (s.Length < count)
			return Error("Insufficient children Y count: " + s.Length + " / " + count, "Cannot load");
		float[] childY = new float[count];
		for (int i = count; --i >= 0; childY[i] = x)
			if (!float.TryParse(s[i], out x))
				return Error("Invalid child Y: " + s[i], "Cannot load");
		// ChildAngles
		s = arr[8].Split(';');
		if (s.Length < 1)
			return Error("No set of child angles", "Cannot load");
		List<(string, float[])> childAngle = [];
		foreach (var c in s) {
			string[] angleSet = c.Split(':');
			if (angleSet[0] == "")
				return Error("Empty angle set name", "Cannot load");
			if (angleSet.Length < count + 1)
				return Error("Insufficient child angle count of " + angleSet[0] + ": " + (angleSet.Length - 1) + " / " + count, "Cannot load");
			float[] angle = new float[count];
			for (int i = count; i > 0; angle[i] = x)
				if (!float.TryParse(angleSet[i--], out x))
					return Error("Invalid angle in set" + angleSet[0] + ": " + angleSet[i + 1], "Cannot load");
			childAngle.Add((angleSet[0], angle));
		}
		// ChildColors
		byte b;
		s = arr[9].Split(';');
		if (s.Length < 1)
			return Error("No set of child colors", "Cannot load");
		List<(string, byte[])> childColor = [];
		foreach (var c in s) {
			string[] colorSet = c.Split(':');
			if (colorSet[0] == "")
				return Error("Empty color set name", "Cannot load");
			if (colorSet.Length < count + 1)
				return Error("Insufficient child color count of " + colorSet[0] + ": " + (colorSet.Length - 1) + " / " + count, "Cannot load");
			byte[] color = new byte[count];
			for (int i = count; i > 0; color[i] = b)
				if (!byte.TryParse(colorSet[i--], out b))
					return Error("Invalid color in set " + colorSet[0] + ": " + colorSet[i + 1], "Cannot load");
			childColor.Add((colorSet[0], color));
		}
		// Cuts
		int intparse;
		s = arr[10].Split(';');
		List<(int, int[])> cutFunction = [];
		foreach (var c in s) {
			string[] cutints = c.Split(':');
			if (!int.TryParse(cutints[0], out int cutindex))
				return Error("Invalid cutfuncion index: " + cutints[0], "Cannot load");
			int[] cuthash = new int[cutints.Length - 1];
			for (int i = cuthash.Length; i > 0; cuthash[i] = intparse)
				if (!int.TryParse(cutints[i--], out intparse))
					return Error("Invalid cutfuncion seed: " + cutints[i + 1], "Cannot load");
			cutFunction.Add((cutindex, cuthash));
		}
		Fractal f = new Fractal(arr[0], count, size, maxsize, minsize, cutsize, childX, childY, childAngle, childColor, cutFunction) {
			path = file
		};
		generator.GetFractals().Add(f);
		if (select)
			fractalSelect.SelectedIndex = fractalSelect.Items.Add(arr[0]);
		else
			_ = fractalSelect.Items.Add(arr[0]);
		return true;
	}
	private void AddAngleButton_Click(object sender, EventArgs e) {
		if (angleBox.Text == "") {
			_ = Error(
			"Type a unique name for the new set of children angles to the box on the left.",
			"Cannot add");
			return;
		}
		foreach (var (n, _) in generator.GetFractal().childColor)
			if (n == angleBox.Text) {
				_ = Error(
			"There already is a set of children angles of the same name.\nType a new unique name to the box on the left.",
			"Cannot add");
				return;
			}
		generator.GetFractal().childAngle.Add((angleBox.Text, new float[generator.GetFractal().childCount]));
		//SetupSelects();
		angleSelect.SelectedIndex = angleSelect.Items.Add(angleBox.Text);
		generator.GetFractal().edit = true;
		FillEditor();
	}
	private void AddcolorButton_Click(object sender, EventArgs e) {
		if (colorBox.Text == "") {
			_ = Error(
			"Type a unique name for the new set of children colors to the box on the left.",
			"Cannot add");
			return;
		}
		foreach (var (n, _) in generator.GetFractal().childColor)
			if (n == colorBox.Text) {
				_ = Error(
			"There already is a set of children colors of the same name.\nType a new unique name to the box on the left.",
			"Cannot add");
				return;
			}
		generator.GetFractal().childColor.Add((colorBox.Text, new byte[generator.GetFractal().childCount]));
		//SetupSelects();
		colorSelect.SelectedIndex = colorSelect.Items.Add(colorBox.Text);
		generator.GetFractal().edit = true;
		FillEditor();
	}
	private void RemoveangleButton_Click(object sender, EventArgs e) {
		if (generator.GetFractal().childAngle.Count <= 1) {
			_ = Error(
		"This is the only set of children angles, so it cannot be removed.",
		"Cannot remove");
			return;
		}
		generator.GetFractal().childAngle.RemoveAt(generator.selectChildAngle);
		generator.selectChildAngle = Math.Min((short)(generator.GetFractal().childAngle.Count - 1), generator.selectChildAngle);
		SwitchChildAngle();
		generator.GetFractal().edit = true;
		QueueReset();
	}
	private void RemovecolorButton_Click(object sender, EventArgs e) {
		if (generator.GetFractal().childColor.Count <= 1) {
			_ = Error(
		"This is the only set of children colors, so it cannot be removed.",
		"Cannot remove");
			return;
		}
		generator.GetFractal().childColor.RemoveAt(generator.selectChildColor);
		generator.selectChildColor = Math.Min((short)(generator.GetFractal().childColor.Count - 1), generator.selectChildColor);
		SwitchChildColor();
		generator.GetFractal().edit = true;
		QueueReset();
	}
	private void AddCut_SelectedIndexChanged(object sender, EventArgs e) {
		if (addCut.SelectedIndex < 1)
			return;
		int n = addCut.SelectedIndex-1;
		foreach (var c in generator.GetFractal().cutFunction)
			if (c.Item1 == n)
				return;
		generator.GetFractal().cutFunction.Add((n, []));
		//SetupSelects();
		cutSelect.SelectedIndex = cutSelect.Items.Add(Fractal.cutFunctions[addCut.SelectedIndex - 1].Item1);
		addCut.SelectedIndex = 0;
		generator.GetFractal().edit = true;
		//_ = Task.Run(Hash);
		//Remember and disable all controls
		MyControlsEnabled.Clear();
		foreach (var c in MyControls) {
			MyControlsEnabled.Add(c.Enabled);
			c.Enabled = false;
		}
		// perform hash
		performHash = true;
		QueueReset();

		
	}
	private void SizeBox_TextChanged(object sender, EventArgs e) { 
		if(ParseRetextDiffApply(sizeBox, ref generator.GetFractal().childSize))return; 
		generator.GetFractal().edit = true; 
	}
	private void CutBox_TextChanged(object sender, EventArgs e) { 
		if(ParseRetextDiffApply(cutBox, ref generator.GetFractal().cutSize))return; 
		generator.GetFractal().edit = true; 
	}
	private void MinBox_TextChanged(object sender, EventArgs e) {
		if(ParseRetextDiffApply(minBox, ref generator.GetFractal().minSize)) return; 
		generator.GetFractal().edit = true; 
	}
	private void MaxBox_TextChanged(object sender, EventArgs e) {
		if(ParseRetextDiffApply(maxBox, ref generator.GetFractal().maxSize)) return;
		generator.GetFractal().edit = true; 
	}
	private void Hash() {
		
	}
	#endregion


}
