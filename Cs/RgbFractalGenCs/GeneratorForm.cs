// Starts the generator with special testing settings
//#define CUSTOMDEBUGTEST

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
		screenPanel.Click += new(AnimateButton_Click);
		screenPanel.Paint += new(ScreenPanel_Paint);
		Controls.Add(screenPanel);
	}
	#endregion

	#region Variables
	// Threading
	private bool bInit = true;

	//private readonly List<Control> MyControls = [];
	private FractalGenerator generator;     // The core ofthe app, the generator the generates the fractal animations
	private CancellationTokenSource cancel; // Cancellation Token Source
	private Task gTask = null;              // CPU gif thread
	private Task aTask = null;              // Abort Task
	private bool queueAbort = false;        // Generator abortion queued
	private short queueReset = 0;           // Counting time until generator Restart

	// Settings
	//private bool previewMode = true;        // Preview mode for booting performance while setting up parameters
	private bool animated = true;           // Animating preview or paused? (default animating)
	private bool modifySettings = true;     // Allows for modifying settings without it triggering Aborts and Generates
	private short width = -1, height = -1;
	private string gifPath;                 // Gif export path name
	private int maxTasks = 0;               // Maximum tasks available
	private short abortDelay = 500;         // Set time to restart generator
	private short restartTimer = 0;

	// Display Variables
	private readonly DoubleBufferedPanel screenPanel;// Display panel
	private Bitmap currentBitmap = null;    // Displayed Bitmap
	private int currentBitmapIndex;         // Play frame index
	private int fx, fy;                     // Memory of window size
	private int controlTabIndex = 0;        // Iterator for tabIndexes - to make sure all the controls tab in the correct order even as i add new ones in the middle
	#endregion

	#region Core
	/// <summary>
	/// Refreshing and preview animation
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void Timer_Tick(object sender, EventArgs e) {
		#region Init
		void Init() {
			void SetupControl(Control control, string tip) {
				// Add tooltip and set the next tabIndex
				toolTips.SetToolTip(control, tip);
				control.TabIndex = ++controlTabIndex;
				//MyControls.Add(control);
			}
			// Setup interactable controls (tooltips + tabIndex)
			SetupControl(fractalSelect, "Select the type of fractal to generate");
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
			SetupControl(brightnessBox, "Brightness level: 0% black, 100% normalized maximum, 300% overexposed 3x maximum");
			SetupControl(parallelTypeBox, "Select which parallelism to be used if the left checkBox is enabled.\nOf Animation = Batching animation frames, recommended for Animations with perfect pixels.\nOf Depth/Of Recursion = parallel single image generation, recommmended for fast single images, 1 in a million pixels might be slightly wrong");
			SetupControl(threadsBox, "The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
			SetupControl(abortBox, "How many millisecond of pause after the last settings change until the generator restarts?");
			SetupControl(delayBox, "A delay between frames in 1/100 of seconds for the preview and exported GIF file.\nThe framerate will be roughly 100/delay");
			SetupControl(prevButton, "Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(nextButton, "Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(animateButton, "Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
			SetupControl(encodeSelect, "Only Image - only generates one image\nAnimation RAM - generated an animation without GIF encoding, faster but can't save GIF afterwards\nEncode GIF - encodes GIF while generating an animation - can save a GIF afterwards");
			SetupControl(helpButton, "Show README.txt.");
			SetupControl(pngButton, "Save the currently displayed frame into a PNG file.\nStop the animation and select the frame you wish to export with the buttons above.");
			SetupControl(gifButton, "Save the full animation into a GIF file.");

			// Read the REDME.txt for the help button
			if (File.Exists("README.txt"))
				helpLabel.Text = File.ReadAllText("README.txt");

			// Create the Generator
			generator = new();
			foreach (var i in generator.GetFractals())
				fractalSelect.Items.AddRange([i.name]);
			generator.selectFractal = -1;
			fractalSelect.SelectedIndex = 0;
			resSelect.SelectedIndex = 0;

			// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restant the generator over and over
			AbortBox_TextChanged(null, null);
			PeriodBox_TextChanged(null, null);
			PeriodMultiplierBox_TextChanged(null, null);
			ParallelTypeBox_SelectedIndexChanged(null, null);
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
			parallelTypeBox.SelectedIndex = 0;
			spinSelect.SelectedIndex = zoomSelect.SelectedIndex = hueSelect.SelectedIndex = 1;
			encodeSelect.SelectedIndex = 2;
			SetupFractal();
			threadsBox.Text = (maxTasks = Math.Max(0, Environment.ProcessorCount - 2)).ToString();
			bInit = modifySettings = helpPanel.Visible = false;

			// Start the generator
			TryResize();
			ResizeAll();
			aTask = gTask = null;
			generator.StartGenerate();
		}
		void SetupFractal() {
			generator.SetupFractal();
			Parallel_Changed(null, null);
			DetailBox_TextChanged(null, null);
			generator.SetupAngle();
			generator.SetupCutFunction();
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
			// generator->StartGenerate(); should be called after
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
			if (queueAbort)
				aTask = Task.Run(Abort, (cancel = new()).Token);
			if ((queueReset -= (short)timer.Interval) > 0)
				return;
			SetupFractal();
			ResizeAll();
			restartButton.Enabled = true;
			ResetRestart();
			generator.StartGenerate();
		}
		if (restartTimer > 0 && (restartTimer -= (short)timer.Interval) <= 0)
			ResetRestart();
		// Fetch the state of generated bitmaps
		int b = generator.GetBitmapsFinished(), bt = generator.GetFrames();
		if (bt <= 0)
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
		if (b > 0) {
			// Fetch bitmap, make sure the index is is range
			currentBitmapIndex %= b;
			var bitmap = generator.GetBitmap(currentBitmapIndex);
			// Update the display with it if necessary
			if (currentBitmap != bitmap) {
				currentBitmap = bitmap;
				screenPanel.Invalidate();
			}
			// Animate the frame index
			if (animated)
				currentBitmapIndex = (currentBitmapIndex + 1) % b;
		}
		// Info text refresh
		statusLabel.Text = b < bt ? "Generating: " : "Finished: ";
		infoLabel.Text = (b < bt ? b.ToString() : currentBitmapIndex.ToString()) + " / " + bt.ToString();


	}
	private void GeneratorForm_FormClosing(object sender, FormClosingEventArgs e) {
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


		cancel?.Cancel();
		gTask?.Wait();
		aTask?.Wait();
		Abort();
	}
	private void Abort() {
		queueAbort = false;
		// Cancel FractalGenerator threads
		generator.RequestCancel();
		/*foreach (var c in MyControls)
			c.Enabled = true;
		CutSelectEnabled(generator.GetFractal().cutFunction);
		FillCutParams();*/
		gifButton.Enabled = false;
		aTask = null;
	}
	private void ResetRestart() {
		queueReset = restartTimer = 0;
		restartButton.Text = "! RESTART !";
	}
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
	#endregion

	#region Input
	private static short Parse(TextBox BOX) => short.TryParse(BOX.Text, out var v) ? v : (short)0;
	private static T Clamp<T>(T NEW, T MIN, T MAX) where T : struct, IComparable<T> => NEW.CompareTo(MIN) < 0 ? MIN : NEW.CompareTo(MAX) > 0 ? MAX : NEW;
	private static short Retext(TextBox BOX, short NEW) { BOX.Text = NEW == 0 ? (short.TryParse(BOX.Text, out short t) ? "" : BOX.Text) : NEW.ToString(); return NEW; }
	private static T Mod<T>(T NEW, T MIN, T MAX) where T : struct, IComparable<T> {
		var D = (dynamic)MAX - MIN; while (NEW.CompareTo(MIN) < 0) NEW = (T)(NEW + D); while (NEW.CompareTo(MAX) > 0) NEW = (T)(NEW - D); return NEW;
	}
	private static bool Diff<T>(T NEW, T GEN) where T : struct, IComparable<T> => GEN.CompareTo(NEW) == 0;
	private bool Apply<T>(T NEW, ref T GEN) { GEN = NEW; QueueReset(); return false; }
	private static short ParseClampRetext(TextBox BOX, short MIN, short MAX) => Retext(BOX, Clamp(Parse(BOX), MIN, MAX));
	private bool DiffApply<T>(T NEW, ref T GEN) where T : struct, IComparable<T> => Diff(NEW, GEN) || Apply(NEW, ref GEN);
	private bool ClampDiffApply<T>(T NEW, ref T GEN, T MIN, T MAX) where T : struct, IComparable<T> => DiffApply(Clamp(NEW, MIN, MAX), ref GEN);
	private bool ParseDiffApply(TextBox BOX, ref short GEN) => DiffApply(Parse(BOX), ref GEN);
	private bool ParseModDiffApply(TextBox BOX, ref short GEN, short MIN, short MAX) => DiffApply(Mod(Parse(BOX), MIN, MAX), ref GEN);
	private bool ParseClampRetextDiffApply(TextBox BOX, ref short GEN, short MIN, short MAX) => DiffApply(ParseClampRetext(BOX, MIN, MAX), ref GEN);
	private bool ParseClampRetextMulDiffApply(TextBox BOX, ref short GEN, short MIN, short MAX, short MUL) => DiffApply((short)(ParseClampRetext(BOX, MIN, MAX) * MUL), ref GEN);
	private bool ParseClampRetextMulDiffApply(TextBox BOX, ref float GEN, short MIN, short MAX, float MUL) => DiffApply(ParseClampRetext(BOX, MIN, MAX) * MUL, ref GEN);
	private void QueueReset(bool allow = true) {
		if (modifySettings || !allow)
			return;
		if (queueReset <= 0) {
			gifButton.Enabled = false;
			currentBitmapIndex = 0;
			if (gTask == null && aTask == null)
				aTask = Task.Run(Abort, (cancel = new()).Token);
			else queueAbort = true;
		}
		ResetRestart();
		queueReset = abortDelay;
		restartButton.Enabled = false;
	}
	bool CutSelectEnabled((string, Fractal.CutFunction)[] cf) => cutSelect.Enabled = cf != null && cf.Length > 0;
	// Query the number of seeds from the CutFunction
	private bool CutParamBoxEnabled(Fractal.CutFunction cf) => cutparamBox.Enabled = 0 < (generator.cutparamMaximum = (short)(cf == null || cf(0, -1) <= 0 ? 0 : (cf(0, 1 - (1 << 16)) + 1) / cf(0, -1)));
	/// <summary>
	/// Fill the cutFunction seed parameter comboBox with available options for the selected CutFunction
	/// </summary>
	private void FillCutParams() => CutParamBoxEnabled(generator.GetCutFunction());
	private void FractalSelect_SelectedIndexChanged(object sender, EventArgs e) {
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
				foreach (var (name, _) in cf)
					cutSelect.Items.AddRange([name]);
				cutSelect.SelectedIndex = 0;
			}
		}
		if (generator.SelectFractal((short)Math.Max(0, fractalSelect.SelectedIndex)))
			return;
		// Fractal is different - load it, change the setting and restart generation
		// Fill the fractal's adjuistable definition combos
		// Fill the fractal's adjuistable cutfunction seed combos, and restart generation
		if (!modifySettings) {
			modifySettings = true;
			FillSelects();
			FillCutParams();
			modifySettings = false;
			QueueReset();
		} else {
			FillSelects();
			FillCutParams();
		}
	}
	private void AngleSelect_SelectedIndexChanged(object sender, EventArgs e) => DiffApply((short)Math.Max(0, angleSelect.SelectedIndex), ref generator.selectChildAngle);
	private void ColorSelect_SelectedIndexChanged(object sender, EventArgs e) => DiffApply((short)Math.Max(0, colorSelect.SelectedIndex), ref generator.selectChildColor);
	private void CutSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (!DiffApply((short)Math.Max(0, cutSelect.SelectedIndex), ref generator.selectCut)) FillCutParams();
	}
	private void CutparamBox_TextChanged(object sender, EventArgs e) => ParseClampRetextDiffApply(cutparamBox, ref generator.selectCutparam, -1, generator.cutparamMaximum);
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
		var newSpeed = ParseClampRetext(hueSpeedBox, 0, 255);
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
	private void Parallel_Changed(object sender, EventArgs e) {
		short newThreads = ParseClampRetext(threadsBox, 0, (short)maxTasks);
		generator.selectMaxTasks = (short)(newThreads > 0 ? newThreads : -1);
		generator.SelectThreadingDepth();
	}
	private void ParallelTypeBox_SelectedIndexChanged(object sender, EventArgs e) => generator.selectParallelType = (FractalGenerator.ParallelType)parallelTypeBox.SelectedIndex;
	private void AbortBox_TextChanged(object sender, EventArgs e) => abortDelay = ParseClampRetext(abortBox, 0, 10000);
	private void DelayBox_TextChanged(object sender, EventArgs e) {
		var newDelay = ParseClampRetext(delayBox, 1, 500);
		if (generator.selectDelay == newDelay)
			return;
		// Delay is diffenret, change it, and restart the generation if ou were encoding a gif
		generator.selectDelay = newDelay;
		var fpsrate = 100 / generator.selectDelay;
		timer.Interval = generator.selectDelay * 10;
		delayLabel.Text = "Abort / FPS: " + fpsrate.ToString();
		if (generator.selectGenerationType == FractalGenerator.GenerationType.EncodeGIF)
			QueueReset();
	}
	private void MoveFrame(int move) { animated = false; var b = generator.GetBitmapsFinished(); currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + b + move) % b; }
	private void PrevButton_Click(object sender, EventArgs e) => MoveFrame(-1);
	private void AnimateButton_Click(object sender, EventArgs e) => animateButton.Text = (animated = !animated) ? "Playing" : "Paused";
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
		if (
			(generator.selectGenerationType = (FractalGenerator.GenerationType)Math.Max(0, encodeSelect.SelectedIndex)) == FractalGenerator.GenerationType.EncodeGIF 
			&& !generator.IsGifReady()
		) QueueReset();
	}
	private void HelpButton_Click(object sender, EventArgs e) {
		helpPanel.Visible = screenPanel.Visible;
		screenPanel.Visible = !screenPanel.Visible;
	}
	private void Png_Click(object sender, EventArgs e) => savePng.ShowDialog();
	private void Gif_Click(object sender, EventArgs e) => saveGif.ShowDialog();

	private void DebugBox_CheckedChanged(object sender, EventArgs e) {
		if (!(generator.debugmode = debugBox.Checked)) debugLabel.Text = "";
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

}
