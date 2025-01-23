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
using static System.Net.Mime.MediaTypeNames;

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
	private short queueReset = 0;			// Counting time until generator Restart

	// Settings
	private bool previewMode = true;        // Preview mode for booting performance while setting up parameters
	private bool animated = true;           // Animating preview or paused? (default animating)
	private bool modifySettings = true;     // Allows for modifying settings without it triggering Aborts and Generates
	private short width = -1, height = -1;
	private string gifPath;                 // Gif export path name
	private int cutparamMaximum = 0;        // Maximum cutparam seed of this CutFunction
	private int maxTasks = 0;               // Maximum tasks available
	private short abortDelay = 500;         // Set time to restart generator

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
			SetupControl(cutparamBox, "Type the cutfunction seed.");
			//SetupControl(cutparamBar, "Select the cutfunction seed.");
			SetupControl(resX, "Type the X resolution of the render (width)");
			SetupControl(resY, "Type the Y resolution of the render (height)");
			SetupControl(previewBox, "If checked, the resolution will be only 80x80 for fast preview render.\nUncheck it when you are done with preparing the settings and want to render it in full resolution");
			SetupControl(periodBox, "How many frames for the self-similar center child to zoom in to the same size as the parent if you are zooming in.\nOr for the parent to zoom out to the same size as the child if you are zooming out.\nThis will equal the number of generated frames of the animation if the center child is the same color.\nOr it will be a third of the number of generated frames of the animation if the center child is a different color.");
			SetupControl(periodMultiplierBox, "Multiplies the frame count, slowing down the rotaion and hue shifts.");
			SetupControl(zoomButton, "Toggle in which direction you want the fractal zoom . ZoomIn, or <- ZoomOut");
			SetupControl(defaultZoom, "Type the initial zoom of the first image (in number of skipped frames).");
			SetupControl(spinSelect, "Choose in which direction you want the zoom animation to spin, or to not spin.");
			SetupControl(spinSpeedBox, "Type the extra speed on the spinning from the values possible for looping.");
			SetupControl(defaultAngle, "Type the initial angle of the first image (in degrees).");
			SetupControl(hueSelect, "Choose the color scheme of the fractal.\nAlso you can make the color hues slowly cycle as the fractal zoom.");
			SetupControl(hueSpeedBox, "Type the extra speed on the hue shifting from the values possible for looping.\nOnly possible if you have chosen color cycling on the left.");
			SetupControl(defaultHue, "Type the initial hue angle of the first image (in degrees).");
			SetupControl(ambBox, "The strength of the ambient grey color in the empty spaces far away between the generated fractal dots.");
			SetupControl(noiseBox, "The strength of the random noise in the empty spaces far away between the generated fractal dots.");
			SetupControl(saturateBox, "Enhance the color saturation of the fractal dots.\nUseful when the fractal is too gray, like the Sierpinski Triangle (Triflake).");
			SetupControl(detailBox, "Level of Detail (The lower the finer).");
			SetupControl(bloomBox, "Bloom: 0 will be maximally crisp, but possibly dark with think fractals. Higher values wil blur/bloom out the fractal dots.");
			SetupControl(blurBox, "Motion blur: Lowest no blur and fast generation, highest 10 smeared frames 10 times slower generation.");
			SetupControl(parallelBox, "Enable parallelism (and then tune with the Max Threads slider).\nSelect the type of parallelism with the followinf checkBox to the right.");
			SetupControl(parallelTypeBox, "Select which parallelism to be used if the left checkBox is enabled.\nOf Animation = Batching animation frames, recommended for Animations with perfect pixels.\nOf Depth/Of Recursion = parallel single image generation, recommmended for fast single images, 1 in a million pixels might be slightly wrong");
			SetupControl(threadsBox, "The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
			SetupControl(abortBox, "How many millisecond of pause after the last settings change until the generator restarts?");
			SetupControl(delayBox, "A delay between frames in 1/100 of seconds for the preview and exported GIF file.\nThe framerate will be roughly 100/delay");
			SetupControl(prevButton, "Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(nextButton, "Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(animateButton, "Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
			SetupControl(encodeButton, "Only Image - only generates one image\nAnimation RAM - generated an animation without GIF encoding, faster but can't save GIF afterwards\nEncode GIF - encodes GIF while generating an animation - can save a GIF afterwards");
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
			generator.select = -1;
			fractalSelect.SelectedIndex = 0;
			// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restant the generator over and over
			threadsBox.Text = maxTasks.ToString();
			abortBox_TextChanged(null, null);
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
			parallelTypeBox.SelectedIndex = 0;
			hueSelect.SelectedIndex = 0;
			maxTasks = Environment.ProcessorCount - 2;
			SetupFractal();
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
			generator.SetupColor();
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
				Math.Max(640, bh + Math.Max(460, height + 8))
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
			generator.width = width;
			generator.height = height;
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
			queueReset = 0;
			generator.StartGenerate();
		}
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
	/// <summary>
	/// Fetch the resolution
	/// </summary>
	/// <returns>Changed</returns>
	private bool TryResize() {
		previewMode = !previewBox.Checked;
		width = 8;
		height = 8;
		if (!short.TryParse(resX.Text, out width) || width <= 8)
			width = 8;
		if (!short.TryParse(resY.Text, out height) || height <= 8)
			height = 8;
		previewBox.Text = "Resolution: " + width.ToString() + "x" + height.ToString();
		if (previewMode)
			width = height = 80;
		return generator.width != width || generator.height != height;
	}
	#endregion

	#region Input
	private void QueueReset() {
		if (modifySettings)
			return;
		if (queueReset <= 0) {
			gifButton.Enabled = false;
			currentBitmapIndex = 0;
			if (gTask == null && aTask == null)
				aTask = Task.Run(Abort, (cancel = new()).Token);
			else queueAbort = true;
		}
		queueReset = abortDelay;
	}
	bool CutSelectEnabled((string, Fractal.CutFunction)[] cf) => cutSelect.Enabled = cf != null && cf.Length > 0;
	/// <summary>
	/// Fractal definition selection
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
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
		//ResetGenerate();
	}
	/// <summary>
	/// Select child angles definition
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void AngleSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (generator.SelectAngle((short)Math.Max(0, angleSelect.SelectedIndex)))
			return;
		// Angle children definition is different - change the setting and restart generation
		QueueReset();
	}
	/// <summary>
	/// Select child colors definition
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void ColorSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (generator.SelectColor((short)Math.Max(0, colorSelect.SelectedIndex)))
			return;
		// Color children definition is different - change the setting and restart generation
		QueueReset();
	}
	/// <summary>
	/// Select CutFunction definition
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private void CutSelect_SelectedIndexChanged(object sender, EventArgs e) {
		if (generator.SelectCutFunction((short)Math.Max(0, cutSelect.SelectedIndex)))
			return;
		// Cutfunction is different - change the setting and restart generation
		//Abort();
		FillCutParams();
		QueueReset();
	}
	/// <summary>
	/// Fill the cutFunction seed parameter comboBox with available options for the selected CutFunction
	/// </summary>
	/// <returns></returns>

	private void FillCutParams() => CutParamBoxEnabled(generator.GetCutFunction());
	// query the number of seeds from the CutFunction
	bool CutParamBoxEnabled(Fractal.CutFunction cf) => cutparamBox.Enabled = 0 < (cutparamMaximum = cf == null || cf(0, -1) <= 0 ? 0 : (cf(0, 1 - (1 << 16)) + 1) / cf(0, -1));


	/*T ClampParam<T>(TextBox BOX, T MIN, T MAX) where T : struct, IComparable<T> {
		T.TryParse(BOX.Text, out T NEW);
		cutparamBox.Text = (NEW = Math.Clamp(NEW, (T)0, (T)MAX)).ToString();
		return NEW;
	}*/

	void ApplyClampParam<T>(T NEW, ref T GEN, T MIN, T MAX, TextBox TEXT) where T : struct, IComparable<T> {
		if (NEW.CompareTo(MIN) < 0)
			NEW = MIN;
		if (NEW.CompareTo(MAX) > 0)
			NEW = MAX;
		if(TEXT != null)
			TEXT.Text = NEW.ToString();
		ApplyDiffParam(NEW, ref GEN);
	}
	void ApplyModParam<T>(T NEW, ref T GEN, T MIN, T MAX) where T : struct, IComparable<T> {
		var D = (dynamic)MAX - MIN;
		while (NEW.CompareTo(MIN) < 0)
			NEW = (T)(NEW + D);
		while (NEW.CompareTo(MAX) > 0)
			NEW = (T)(NEW - D);
		ApplyDiffParam(NEW, ref GEN);
	}
	bool DiffParam<T>(T NEW, T GEN) where T : struct, IComparable<T> {
		return GEN.CompareTo(NEW) == 0;
	}
	void ApplyDiffParam<T>(T NEW, ref T GEN) where T : struct, IComparable<T> {
		if (DiffParam(NEW, GEN))
			return;
		ApplyParam(NEW, ref GEN);
	}
	void ApplyParam<T>(T NEW, ref T GEN) {
		GEN = NEW; QueueReset();
	}

	/// <summary>
	/// Change the CutFunction seed parameter through the textBox typing
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void CutparamBox_TextChanged(object sender, EventArgs e) {
		if(!short.TryParse(cutparamBox.Text, out var newcutparam))
			newcutparam = 0;
		ApplyClampParam(newcutparam, ref generator.cutparam, (short)0, (short)cutparamMaximum, cutparamBox);
	}
	/// <summary>
	/// Resolution Changed Event
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Resolution_Changed(object sender, EventArgs e) {
		if (TryResize())
			QueueReset();//AbortGenerate();
	}
	/// <summary>
	/// Number Of Animation Frames to reach the center Self Similar (the total frames can be higher if the center child has a different color or rotation)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void PeriodBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(periodBox.Text, out var newPeriod))
			newPeriod = 120;
		ApplyClampParam(newPeriod, ref generator.period, (short)1, (short)1000, periodBox);
	}
	/// <summary>
	/// Multiplies the number of loop frames, keeping the spin and huecycle the same speed (you can speed up either with options below)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void PeriodMultiplierBox_TextChanged(object sender, EventArgs e) {
		if(!short.TryParse(periodMultiplierBox.Text, out var newPeriod))
			newPeriod = 1;
		ApplyClampParam(newPeriod, ref generator.periodMultiplier, (short)1, (short)10, periodMultiplierBox);
	}
	/// <summary>
	/// Zoom direction (-> Forward zoom in, <- Backwards zoom out)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ZoomButton_Click(object sender, EventArgs e) {
		// zoom is different - change the setting and restart generation
		//Abort();
		generator.zoom = (short)-generator.zoom;
		zoomButton.Text = "Zoom: " + ((generator.zoom > 0) ? "->" : "<-");
		QueueReset();//ResetGenerate();
	}
	/// <summary>
	/// Default Zoom value on first frame (in skipped frames)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void DefaultZoom_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(defaultZoom.Text, out var newZoom))
			newZoom = 0;
		var finalPeriod = (short)(generator.period * generator.GetFinalPeriod());
		ApplyModParam(newZoom, ref generator.defaultZoom, (short)0, finalPeriod);
	}
	/// <summary>
	/// Select the spin mode (clockwise, counterclockwise, or antispin where the child spins in opposite direction)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void SpinSelect_SelectedIndexChanged(object sender, EventArgs e) {
		var newSpin = (short)(spinSelect.SelectedIndex - 2);
		ApplyClampParam(newSpin, ref generator.defaultSpin, (short)-2, (short)2, null);
	}
	/// <summary>
	/// Select the extra spin of symmentry angle per loop (so it spins faster)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void SpinSpeedBox_TextChanged(object sender, EventArgs e) {
		if(!short.TryParse(spinSpeedBox.Text, out var newSpeed))
			newSpeed = 0;
		//byte newSpeedByte = (byte)Math.Clamp(newSpeed, (short)0, (short)255);
		spinSpeedBox.Text = newSpeed.ToString();
		ApplyClampParam(newSpeed, ref generator.extraSpin, (short)0, (short)255, spinSpeedBox);
	}
	/// <summary>
	/// Default spin angle on first frame (in degrees)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void DefaultAngle_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(defaultAngle.Text, out var newAngle))
			newAngle = 0;
		ApplyModParam(newAngle, ref generator.defaultAngle, (short)0, (short)360);
	}
	/// <summary>
	/// Select the hue pallete and cycling
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void HueSelect_SelectedIndexChanged(object sender, EventArgs e) {
		var colorChoice = (byte)(hueSelect.SelectedIndex % 6);
		var newHueCycle = (short)((colorChoice / 2 + 1) % 3 - 1);
		if (generator.SelectColorPalette((byte)(colorChoice % 2)) && newHueCycle == generator.hueCycle)
			return;
		ApplyParam(newHueCycle, ref generator.hueCycle);
	}
	/// <summary>
	/// Select the extra hue cycling speed of extra full 360° color loops per full animation loop (so it hue cycles spins faster)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void HueSpeedBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(hueSpeedBox.Text, out var newSpeed) || newSpeed < 0)
			newSpeed = 0;
		byte newSpeedByte = (byte)Math.Clamp(newSpeed, (short)0, (short)255);
		hueSpeedBox.Text = newSpeedByte.ToString();
		if (DiffParam(newSpeedByte, generator.extraHue))
			return;
		// hue speed is different - change the setting and if it's actually huecycling restart generation
		if (generator.hueCycle != 0) 
			ApplyParam(newSpeedByte, ref generator.extraHue);
		else generator.extraHue = newSpeedByte;
	}
	/// <summary>
	/// Defaul hue angle on first frame (in degrees)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void DefaultHue_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(defaultHue.Text, out var newHue))
			newHue = 0;
		ApplyModParam(newHue,ref generator.defaultHue, (short)0, (short)360);
	}
	/// <summary>
	/// The strenghts (lightness) of the dark void outside between the fractal points
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void AmbBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(ambBox.Text, out var newAmb))
			newAmb = 0;
		ambBox.Text = (newAmb = Math.Clamp(newAmb, (byte)0, (byte)30)).ToString();
		ApplyDiffParam((short)(4 * newAmb), ref generator.amb);
	}
	/// <summary>
	/// Level of void Noise of the dark void outside between the fractal points
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void NoiseBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(noiseBox.Text, out var newNoise))
			newNoise = 0;
		noiseBox.Text = (newNoise = Math.Clamp(newNoise, (short)0, (short)30)).ToString();
		ApplyDiffParam(newNoise * .1f, ref generator.noise);
	}
	/// <summary>
	/// Saturation Setting - ramp up saturation to maximum if all the wat to the right
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void SaturateBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(saturateBox.Text, out var newSaturate))
			newSaturate = 0;
		saturateBox.Text = (newSaturate = Math.Clamp(newSaturate, (short)0, (short)10)).ToString();
		ApplyDiffParam(newSaturate * .1f, ref generator.saturate);
	}
	/// <summary>
	/// Detail, how small the split fractal shaped have to get, until they draw a dot of their color to the image buffer (the smaller the finer)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void DetailBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(detailBox.Text, out var newDetail))
			newDetail = 0;
		detailBox.Text = (newDetail = Math.Clamp(newDetail, (short)0, (short)10)).ToString();
		ApplyDiffParam(newDetail * .1f * generator.GetFractal().minSize, ref generator.detail);
	}
	/// <summary>
	/// Bloom strength. 0 = crisp, more - expanded and blurry
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void BloomBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(bloomBox.Text, out var newBloom))
			newBloom = 0;
		bloomBox.Text = (newBloom = Math.Clamp(newBloom, (byte)0, (byte)40)).ToString();
		ApplyDiffParam(newBloom * .25f, ref generator.bloom);
	}
	/// <summary>
	/// Level of blur smear frames (renders multiple fractals of slighlty increased time until the frame deltatime over each other)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void BlurBox_TextChanged(object sender, EventArgs e) {
		if(!short.TryParse(blurBox.Text, out var newBlur))
			newBlur = 0;
		blurBox.Text = (newBlur = Math.Clamp(newBlur, (byte)0, (byte)40)).ToString();
		ApplyDiffParam(++newBlur, ref generator.selectBlur);
	}
	/// <summary>
	/// Iteration Threading - How many iterations deep have Self Similars a new thread
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Parallel_Changed(object sender, EventArgs e) {
		if(!short.TryParse(threadsBox.Text, out var newThreads))
			newThreads = 0;
		threadsBox.Text = (newThreads = (short)Math.Clamp(newThreads, 0, maxTasks)).ToString();
		threadsLabel.Text = "Maximum threads (0-" + maxTasks + "):";
		generator.maxTasks = (short)(parallelBox.Checked && newThreads > 0 ? newThreads : -1);
		generator.maxGenerationTasks = (short)(generator.maxTasks - 1);
		generator.SelectThreadingDepth();
	}
	/// <summary>
	/// Toggles between parallelism of single images and parallelism of batching animation frames
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ParallelTypeBox_SelectedIndexChanged(object sender, EventArgs e) => generator.parallelType = (byte)parallelTypeBox.SelectedIndex;
	/// <summary>
	/// Milisecond delay from settings change to generator restart
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void abortBox_TextChanged(object sender, EventArgs e) {
		short.TryParse(abortBox.Text, out abortDelay);
		abortBox.Text = (abortDelay = Math.Clamp(abortDelay, (short)1, (short)10000)).ToString();
	}
	/// <summary>
	/// Framerate Delay (for previes and for gif encode, so if encoding gif, it will restart the generation)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void DelayBox_TextChanged(object sender, EventArgs e) {
		if (!short.TryParse(delayBox.Text, out var newDelay) || newDelay <= 0)
			newDelay = 5;
		delayBox.Text = newDelay.ToString();
		if (generator.delay == newDelay)
			return;
		// Delay is diffenret, change it, and restart the generation if ou were encoding a gif
		generator.delay = newDelay;
		var fpsrate = 100 / generator.delay;
		timer.Interval = generator.delay * 10;
		delayLabel.Text = "Abort / FPS: " + fpsrate.ToString();
		if (generator.encode == 2)
			QueueReset();//ResetGenerate();
	}
	/// <summary>
	/// Select Previous frame to display
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void PrevButton_Click(object sender, EventArgs e) {
		animated = false;
		var b = generator.GetBitmapsFinished();
		currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + b - 1) % b;
	}
	/// <summary>
	/// Select Next frame to display
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void NextButton_Click(object sender, EventArgs e) {
		animated = false;
		var b = generator.GetBitmapsFinished();
		currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + 1) % b;
	}
	/// <summary>
	/// Toggle display animation
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void AnimateButton_Click(object sender, EventArgs e) => animateButton.Text = (animated = !animated) ? "Playing" : "Paused";
	/// <summary>
	/// Toggle level of generation (SingleImage - Animation - GIF)
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void encodeButton_Click(object sender, EventArgs e) {
		switch (generator.encode = (byte)((generator.encode + 1) % 3)) {
			case 0:
				// Only generates one image
				encodeButton.Text = "Only Image";
				break;
			case 1:
				// Generates an animation for you to see faster, but without encoding a Gif to export
				encodeButton.Text = "RAM Animation";
				break;
			case 2:
				// Full generation including GIF encoding
				encodeButton.Text = "Encode GIF";
				if (!generator.IsGifReady())
					QueueReset();//AbortGenerate();
				break;
		}
	}
	/// <summary>
	/// Show readme help
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void HelpButton_Click(object sender, EventArgs e) {
		helpPanel.Visible = screenPanel.Visible;
		screenPanel.Visible = !screenPanel.Visible;
	}
	/// <summary>
	/// Save Frame
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void png_Click(object sender, EventArgs e) => savePng.ShowDialog();
	/// <summary>
	/// Save Gif
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Gif_Click(object sender, EventArgs e) => saveGif.ShowDialog();
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
	#endregion

	#region Output
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
