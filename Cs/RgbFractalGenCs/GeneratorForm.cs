// Starts the generator with special testing settings
//#define CUSTOMDEBUGTEST

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RgbFractalGenCs {
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public partial class GeneratorForm : Form {

		#region Designer
		public GeneratorForm() {
			InitializeComponent();
			screenPanel = new();
			screenPanel.Location = new(239, 13);
			screenPanel.Name = "screenPanel";
			screenPanel.Size = new System.Drawing.Size(80, 80);
			screenPanel.TabIndex = 25;
			screenPanel.Click += new(screenPanel_Click);
			screenPanel.Paint += new(screenPanel_Paint);
			Controls.Add(screenPanel);
		}
		#endregion

		#region Variables
		// Threading
		private FractalGenerator generator;     // The core ofthe app, the generator the generates the fractal animations
		private CancellationTokenSource cancel; // Cancellation Token Source
		private Task gTask;                     // CPU gif thread
		// Settings
		private bool previewMode = true;        // Preview mode for booting performance while setting up parameters
		private bool animated = true;           // Animating preview or paused? (default animating)
		private bool modifySettings = true;     // Allows for modifying settings without it triggering Aborts and Generates
		private string gifPath;					// Gif export path name
		// Display Variables
		private DoubleBufferedPanel screenPanel;// Display panel
		private Bitmap currentBitmap = null;    // Displayed Bitmap
		private int currentBitmapIndex;         // Play frame index
		private int fx, fy;                     // Memory of window size
		private int controlTabIndex = 0;		// Iterator for tabIndexes - to make sure all the controls tab in the correct order even as i add new ones in the middle

		#endregion

		#region Core
		/// <summary>
		/// Initializes all the variables, and the fractal generator
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private void GeneratorForm_Load(object sender, EventArgs e) {
			// Read the REDME.txt for the help button
			if (File.Exists("README.txt"))
				helpLabel.Text = File.ReadAllText("README.txt");
			helpPanel.Visible = false;

			// Setupd interactable controls (tooltips + tabIndex)
			SetupControl(fractalSelect, "Select the type of fractal to generate");
			SetupControl(angleSelect, "Select the children angles definition.");
			SetupControl(colorSelect, "Select the children colors definition.");
			SetupControl(cutSelect, "Select the cutfunction definition.");
			SetupControl(cutparamBox, "Type the cutfunction seed.");
			SetupControl(cutparamBar, "Select the cutfunction seed.");
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
			SetupControl(ambBar, "The strength of the ambient grey color in the empty spaces far away between the generated fractal dots.");
			SetupControl(noiseBar, "The strength of the random noise in the empty spaces far away between the generated fractal dots.");
			SetupControl(saturateBar, "Enhance the color saturation of the fractal dots.\nUseful when the fractal is too gray, like the Sierpinski Triangle (Triflake).");
			SetupControl(detailBar, "Level of Detail (The lower the finer).");
			SetupControl(blurBar, "Motion blur: Lowest no blur and fast generation, highest 10 smeared frames 10 times slower generation.");
			SetupControl(parallelBox, "Enable parallelism (and then tune with the Max Threads slider).\nSelect the type of parallelism with the followinf checkBox to the right.");
			SetupControl(parallelTypeBox, "Type of parallelism to be used if the left checkBox is enabled.\n...Of Images = parallel single image generation, recommmended for fast single images, 1 in a million pixels might be slightly wrong\n...Of Animation Frames = batching animation frames, recommended for Animations with perfect pixels.");
			SetupControl(threadsBar, "The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
			SetupControl(delayBox, "A delay between frames in 1/100 of seconds for the preview and exported GIF file.\nThe framerate will be roughly 100/delay");
			SetupControl(prevButton, "Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(nextButton, "Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
			SetupControl(animateButton, "Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
			SetupControl(encodeButton, "Only Image - only generates one image\nAnimation RAM - generated an animation without GIF encoding, faster but can't save GIF afterwards\nEncode GIF - encodes GIF while generating an animation - can save a GIF afterwards");
			SetupControl(helpButton, "Show README.txt.");
			SetupControl(pngButton, "Save the currently displayed frame into a PNG file.\nStop the animation and select the frame you wish to export with the buttons above.");
			SetupControl(gifButton, "Save the full animation into a GIF file.");

			// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restant the generator over and over
			generator = new();
			foreach (Fractal i in generator.GetFractals())
				fractalSelect.Items.AddRange([i.name]);
			SelectMaxThreads();
			fractalSelect.SelectedIndex = 0;
			var maxThreads = Environment.ProcessorCount - 2;
			threadsBar.Maximum = maxThreads;
			threadsBar.Value = maxThreads;
			SelectMaxThreads();
			periodBox_TextChanged(null, null);
			periodMultiplierBox_TextChanged(null, null);
			SelectParallelType();
			delayBox_TextChanged(null, null);
			defaultZoom_TextChanged(null, null);
			spinSpeedBox_TextChanged(null, null);
			hueSpeedBox_TextChanged(null, null);
			defaultHue_TextChanged(null, null);
			hueSelect.SelectedIndex = 0;
			ambBar_Scroll(null, null);
			noiseBar_Scroll(null, null);
			blurBar_Scroll(null, null);
			saturateBar_Scroll(null, null);

			// Setup bitmap and start generation
			modifySettings = false;
			ResizeAll();
			gTask = null;

			generator.StartGenerate();
		}
		/// <summary>
		/// Refreshing and preview animation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private void timer_Tick(object sender, EventArgs e) {
			// Fetch the state of generated bitmaps
			int b = generator.GetBitmapsFinished(), bt = generator.GetBitmapsTotal();
			if (bt <= 0)
				return;
			// Only Allow GIF Export when generation is finished
			gifButton.Enabled = generator.IsGifReady() && gTask == null;
			// Handle currentBitmap
			if (b > 0) {
				// Fetch bitmap, make sure the index is is range
				currentBitmapIndex = currentBitmapIndex % b;
				Bitmap bitmap = generator.GetBitmap(currentBitmapIndex);
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
			gifButton.Text = gTask == null ? "Save GIF" : "Saving GIF...";
			// Window Size Update
			WindowSizeRefresh();
		}
		private void GeneratorForm_FormClosing(object sender, FormClosingEventArgs e) {
			Abort();
		}
		private void Abort() {
			if (modifySettings)
				return;
			// Cancel gTask
			cancel?.Cancel();
			gTask?.Wait();
			// Cancel FractalGenerator threads
			generator.RequestCancel();
		}
		/// <summary>
		/// Reset prerendered bmp
		/// </summary>
		private void ResetGenerator() {
			if (modifySettings)
				return;
			// Resets the generator
			// (Abort should be called before this or else it will crash)
			// generator->StartGenerate(); should be called after
			gifButton.Enabled = false;
			currentBitmapIndex = 0;
			generator.ResetGenerator();
		}
		private void ResetGenerate() {
			if (modifySettings)
				return;
			// Just restart the generator without resizing (Abort should be called before this or else it will crash)
			ResetGenerator();
			generator.StartGenerate();
		}
		private void ResizeGenerate() {
			// Resize and restard generator (Abort should be called before this or else it will crash)
			if (modifySettings)
				return;
			ResizeAll();
			generator.StartGenerate();
		}
		private void AbortGenerate() {
			// Just Abort and regenerate with nothing inbetween
			if (modifySettings)
				return;
			Abort();
			ResizeGenerate();
		}
		private void SetupControl(Control control, string tip) {
			// Add tooltip and set the next tabIndex
			toolTips.SetToolTip(control, tip);
			control.TabIndex = ++controlTabIndex;
		}
		#endregion

		#region Size
		private void ResizeAll() {
			TryResize();
			// Update the size of the window and display
			SetMinimumSize();
			SetClientSizeCore(
				generator.width + 314,
				Math.Max(generator.height + 8, 300)
			);
			ResizeScreen();
			WindowSizeRefresh();
#if CUSTOMDEBUGTEST
			generator.DebugStart();animated = false;
#endif
			ResetGenerator();
			SizeAdapt();
		}
		/// <summary>
		/// Fetch the resolution
		/// </summary>
		/// <returns>Changed</returns>
		private bool TryResize() {
			previewMode = !previewBox.Checked;
			short w = 8, h = 8;
			if (!short.TryParse(resX.Text, out w) || w <= 8)
				generator.width = 8;
			if (!short.TryParse(resY.Text, out h) || h <= 8)
				generator.height = 8;
			previewBox.Text = "Resolution: " + w.ToString() + "x" + h.ToString();
			if (previewMode)
				w = h = 80;
			if (generator.width == w && generator.height == h) 
				return false;
			// resoltion is changed - request the fractal to resize the buffer and restart generation
			generator.width = w; 
			generator.height = h;
			return true;
		}
		private void WindowSizeRefresh() {
			if (fx == Width && fy == Height)
				return;
			// User has manually resized the window - strech the display
			ResizeScreen();
			SetMinimumSize();
			int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			SetClientSizeCore(
				Math.Max(/*ClientSize.Width*/Width - bw, 314 + Math.Max(screenPanel.Width, generator.width)),
				Math.Max(/*ClientSize.Height*/Height - bh, 8 + Math.Max(screenPanel.Height, generator.height))
			);
			SizeAdapt();
		}
		private void SizeAdapt() {
			fx = Width;
			fy = Height;
		}
		private void SetMinimumSize() {
			// bw = Width - ClientWidth = 16
			// bh = Height - ClientHeight = 39
			int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			MinimumSize = new(
				Math.Max(1100, bw + generator.width + 284),
				Math.Max(900, bh + Math.Max(460, generator.height + 8))
			);
			//debugLabel.Text = debugLabel.Text + " " + MinimumSize.Height.ToString();
		}
		private void ResizeScreen() {
			int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
			int screenHeight = Math.Max(generator.height, Math.Min(Height - bh - 8, (Width - bw - 314) * generator.height / generator.width));
			screenPanel.SetBounds(305, 4, screenHeight * generator.width / generator.height, screenHeight);
			screenPanel.Invalidate();
		}
		#endregion

		#region Input
		/// <summary>
		/// Fractal definition selection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void fractalSelect_SelectedIndexChanged(object sender, EventArgs e) {
			if (generator.SelectFractal((short)(Math.Max(0, fractalSelect.SelectedIndex))))
				return;
			// Fractal is different - load it, change the setting and restart generation
			Abort();
			generator.SetupFractal();
			// Fill the fractal's adjuistable definition combos
			if (!modifySettings) {
				modifySettings = true;
				FillSelects();
				detailBar_Scroll(null, null);
				modifySettings = false;
			} else FillSelects();
			// Fill the fractal's adjuistable cutfunction seed combos, and restart generation
			FillCutParams();
			ResetGenerate();
		}
		/// <summary>
		/// Fill the Color/Angle/CutFunction comboBoxes with available options for the selected fractal
		/// </summary>
		/// <returns></returns>
		private void FillSelects() {
			// Fill angle childred definitnions combobox
			angleSelect.Items.Clear();
			foreach (var (name, _) in generator.GetFractal().childAngle)
				angleSelect.Items.AddRange([name]);
			angleSelect.SelectedIndex = 0;
			// Fill color children definitnions combobox
			colorSelect.Items.Clear();
			foreach (var (name, _) in generator.GetFractal().childColor)
				colorSelect.Items.AddRange([name]);
			colorSelect.SelectedIndex = 0;
			// Fill cutfunction definitnions combobox
			cutSelect.Items.Clear();
			var cf = generator.GetFractal().cutFunction;
			if (cutSelect.Enabled = cf != null && cf.Length > 0) {
				foreach (var (name, _) in cf)
					cutSelect.Items.AddRange([name]);
				cutSelect.SelectedIndex = 0;
			}
		}
		/// <summary>
		/// Select child angles definition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void angleSelect_SelectedIndexChanged(object sender, EventArgs e) {
			if (generator.SelectAngle((short)Math.Max(0, angleSelect.SelectedIndex)))
				return;
			// Angle children definition is different - change the setting and restart generation
			AbortGenerate();
		}
		/// <summary>
		/// Select child colors definition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private void colorSelect_SelectedIndexChanged(object sender, EventArgs e) {
			if (generator.SelectColor((short)Math.Max(0, colorSelect.SelectedIndex)))
				return;
			// Color children definition is different - change the setting and restart generation
			Abort();
			generator.SelectColor();
			ResetGenerate();
		}
		/// <summary>
		/// Select CutFunction definition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private void cutSelect_SelectedIndexChanged(object sender, EventArgs e) {
			if (generator.SelectCutFunction((short)Math.Max(0, cutSelect.SelectedIndex)))
				return;
			// Cutfunction is different - change the setting and restart generation
			Abort();
			FillCutParams();
			ResetGenerate();
		}
		/// <summary>
		/// Fill the cutFunction seed parameter comboBox with available options for the selected CutFunction
		/// </summary>
		/// <returns></returns>
		private void FillCutParams() {
			var cf = generator.GetCutFunction();
			// query the number of seedss from the CutFunction
			var cm = cf == null || cf(0, -1) <= 0 ? 0 : (cf(0, 1 - (1 << 16)) + 1) / cf(0, -1);
			// set the maximum of the trackBar for the seed to that value
			if (modifySettings) {
				cutparamBar.Maximum = cm;
				cutparamBox.Text = "0";
				cutparamBar.Value = 0;
			} else {
				modifySettings = true;
				cutparamBar.Maximum = cm;
				cutparamBox.Text = "0";
				cutparamBar.Value = 0;
				modifySettings = false;
			}
			cutparamBar.Enabled = cutparamBar.Maximum > 0;
		}
		/// <summary>
		/// Change the CutFunction seed parameter through the textBox typing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cutparamBox_TextChanged(object sender, EventArgs e) {
			short newcutparam;
			if (!short.TryParse(cutparamBox.Text, out newcutparam) || newcutparam < 0 || newcutparam > cutparamBar.Maximum)
				newcutparam = 0;
			if (newcutparam == generator.cutparam)
				return;
			// Cutfunction seed is different - change the setting and restart generation
			Abort();
			// update the value in the trackBar for the seed
			if (modifySettings) {
				cutparamBar.Value = newcutparam;
			} else {
				modifySettings = true;
				cutparamBar.Value = newcutparam;
				modifySettings = false;
			}
			generator.cutparam = newcutparam;
			ResetGenerate();
		}
		/// <summary>
		/// Change the CutFunction seed parameter through the trackBar scroll
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cutparamBar_Scroll(object sender, EventArgs e) {
			short v = (short)cutparamBar.Value;
			if (v == generator.cutparam)
				return;
			// Cutfunction seed is different - change the setting and restart generation
			Abort();
			// update the value in the text box for the seed
			if (modifySettings) {
				cutparamBox.Text = v.ToString();
			} else {
				modifySettings = true;
				cutparamBox.Text = v.ToString();
				modifySettings = false;
			}
			generator.cutparam = v;
			ResetGenerate();
		}
		/// <summary>
		/// Type X resolution - width
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void resX_TextChanged(object sender, EventArgs e) {
			if(TryResize())
				AbortGenerate();
		}
		/// <summary>
		/// Type Y resolution - height
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void resY_TextChanged(object sender, EventArgs e) {
			if (TryResize())
				AbortGenerate();
		}
		/// <summary>
		/// Toggle the "use the resolution", if unchecked, it will run in preview mode of firced 80x80 resolution regardless of typed resolution
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void previewBox_CheckedChanged(object sender, EventArgs e) {
			if(TryResize())
				AbortGenerate();
		}
		/// <summary>
		/// Number Of Animation Frames to reach the center Self Similar (the total frames can be higher if the center child has a different color or rotation)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void periodBox_TextChanged(object sender, EventArgs e) {
			short newPeriod = 1;
			if (!short.TryParse(periodBox.Text, out newPeriod) || newPeriod <= 0)
				newPeriod = 120;
			if (generator.period == newPeriod)
				return;
			// period is different - change the setting and restart generation
			Abort();
			generator.period = newPeriod;
			ResetGenerate();
		}
		/// <summary>
		/// Multiplies the number of loop frames, keeping the spin and huecycle the same speed (you can speed up either with options below)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void periodMultiplierBox_TextChanged(object sender, EventArgs e) {
			short newPeriod = 1;
			if (!short.TryParse(periodMultiplierBox.Text, out newPeriod) || newPeriod <= 1)
				newPeriod = 1;
			if (generator.periodMultiplier == newPeriod)
				return;
			// period is different - change the setting and restart generation
			Abort();
			generator.periodMultiplier = newPeriod;
			ResetGenerate();
		}
		/// <summary>
		/// Zoom direction (-> Forward zoom in, <- Backwards zoom out)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void zoomButton_Click(object sender, EventArgs e) {
			// zoom is different - change the setting and restart generation
			Abort();
			generator.zoom = (short)(-generator.zoom);
			zoomButton.Text = "Zoom: " + ((generator.zoom > 0) ? "->" : "<-");
			ResetGenerate();
		}
		/// <summary>
		/// Default Zoom value on first frame (in skipped frames)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void defaultZoom_TextChanged(object sender, EventArgs e) {
			short newZoom = 0;
			if (!short.TryParse(defaultZoom.Text, out newZoom))
				newZoom = 0;
			short finalPeriod = (short)(generator.period * generator.GetFinalPeriod());
			while (newZoom < 0)
				newZoom += finalPeriod;
			while (newZoom >= finalPeriod)
				newZoom -= finalPeriod;
			if (generator.defaultZoom == newZoom)
				return;
			// default zoom is different - change the setting and restart generation
			Abort();
			generator.defaultZoom = newZoom;
			ResetGenerate();
		}
		/// <summary>
		/// Select the spin mode (clockwise, counterclockwise, or antispin where the child spins in opposite direction)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void spinSelect_SelectedIndexChanged(object sender, EventArgs e) {
			short newSpin = (short)(Math.Max(0, Math.Min(4, spinSelect.SelectedIndex)) - 2);
			if (generator.defaultSpin == newSpin)
				return;
			// spin type is different - change the setting and restart generation
			Abort();
			generator.defaultSpin = newSpin;
			ResetGenerate();
		}
		/// <summary>
		/// Select the extra spin of symmentry angle per loop (so it spins faster)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void spinSpeedBox_TextChanged(object sender, EventArgs e) {
			byte newSpeed = 0;
			if (!byte.TryParse(spinSpeedBox.Text, out newSpeed) || newSpeed < 0)
				newSpeed = 0;
			if (generator.extraSpin == newSpeed)
				return;
			// spin speed is different - change the setting and restart generation
			Abort();
			generator.extraSpin = newSpeed;
			ResetGenerate();
		}
		/// <summary>
		/// Default spin angle on first frame (in degrees)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void defaultAngle_TextChanged(object sender, EventArgs e) {
			short newAngle = 0;
			if (!short.TryParse(defaultAngle.Text, out newAngle))
				newAngle = 0;
			while (newAngle < 0)
				newAngle += 360;
			while (newAngle >= 360)
				newAngle -= 360;
			if (generator.defaultAngle == newAngle)
				return;
			// angle is different - change the setting and restart generation
			Abort();
			generator.defaultAngle = newAngle;
			ResetGenerate();
		}
		/// <summary>
		/// Select the hue pallete and cycling
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void hueSelect_SelectedIndexChanged(object sender, EventArgs e) {
			byte colorChoice = (byte)((hueSelect.SelectedIndex) % 6);
			short newHueCycle = (short)((colorChoice / 2 + 1) % 3 - 1);
			if (generator.SelectColorPalette((byte)(colorChoice % 2)) && newHueCycle == generator.hueCycle)
				return;
			Abort();
			// hue is different - change the setting and restart generation
			generator.hueCycle = newHueCycle;
			generator.SelectColor();
			ResetGenerate();
		}
		/// <summary>
		/// Select the extra hue cycling speed of extra full 360° color loops per full animation loop (so it hue cycles spins faster)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void hueSpeedBox_TextChanged(object sender, EventArgs e) {
			byte newSpeed = 0;
			if (!byte.TryParse(hueSpeedBox.Text, out newSpeed) || newSpeed < 0)
				newSpeed = 0;
			if (generator.extraHue == newSpeed)
				return;
			// hue speed is different - change the setting and if it's actually huecycling restart generation
			if (generator.hueCycle != 0) {
				Abort();
				generator.extraHue = newSpeed;
				ResetGenerate();
			} else
				generator.extraHue = newSpeed;
		}
		/// <summary>
		/// Defaul hue angle on first frame (in degrees)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void defaultHue_TextChanged(object sender, EventArgs e) {
			short newHue = 0;
			if (!short.TryParse(defaultHue.Text, out newHue))
				newHue = 0;
			while (newHue < 0)
				newHue += 360;
			while (newHue >= 360)
				newHue -= 360;
			if (generator.defaultHue == newHue)
				return;
			Abort();
			// Hue is different - change the setting and restart generation
			generator.defaultHue = newHue;
			ResetGenerate();
		}
		/// <summary>
		/// The strenghts (lightness) of the dark void outside between the fractal points
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ambBar_Scroll(object sender, EventArgs e) {
			var newAmb = (short)(ambBar.Value * 4);
			if (generator.amb == newAmb)
				return;
			Abort();
			// Ambient is different - change the setting and restart generation
			generator.amb = newAmb;
			ResetGenerate();
		}
		/// <summary>
		/// Level of void Noise of the dark void outside between the fractal points
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void noiseBar_Scroll(object sender, EventArgs e) {
			float newNoise = noiseBar.Value * .1f;
			if (generator.noise == newNoise)
				return;
			Abort();
			// Moise is different - change the setting and restart generation
			generator.noise = newNoise;
			ResetGenerate();
		}
		/// <summary>
		/// Saturation Setting - ramp up saturation to maximum if all the wat to the right
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saturateBar_Scroll(object sender, EventArgs e) {
			var newSat = saturateBar.Value * .1f;
			if (generator.saturate == newSat)
				return;
			Abort();
			// Saturation is different - change the setting and restart generation
			generator.saturate = newSat;
			ResetGenerate();
		}
		private void SelectSaturation() {
			
		}
		/// <summary>
		/// Detail, how small the split fractal shaped have to get, until they draw a dot of their color to the image buffer (the smaller the finer)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void detailBar_Scroll(object sender, EventArgs e) {
			var newDetail = detailBar.Value * .1f;
			if (generator.SelectDetail(newDetail))
				return;
			// Detail is different - change the setting and restart generation
			AbortGenerate();
		}
		/// <summary>
		/// Level of blur smear frames (renders multiple fractals of slighlty increased time until the frame deltatime over each other)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void blurBar_Scroll(object sender, EventArgs e) {
			if (generator.selectBlur == (byte)blurBar.Value)
				return;
			Abort();
			// Blur is different - change the setting and restart generation
			generator.selectBlur = (byte)blurBar.Value;
			ResetGenerate();
		}
		/// <summary>
		/// Iteration Threading - How many iterations deep have Self Similars a new thread
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void parallelBox_CheckedChanged(object sender, EventArgs e) {
			SelectMaxThreads();
			generator.SelectThreadingDepth();
		}
		/// <summary>
		/// Sets maximum threads as selected by user (th parallel checkBox and the maxThreads slider)
		/// </summary>
		private void SelectMaxThreads() {
			generator.maxTasks = (short)(parallelBox.Checked ? threadsBar.Value : -1);
			generator.maxGenerationTasks = (short)(generator.maxTasks - 1);
		}
		/// <summary>
		/// Drawing Threading - Paralelizes the scanlines of drawing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void parallelTypeBox_CheckedChanged(object sender, EventArgs e) {
			SelectParallelType();
		}
		/// <summary>
		/// Toggles between parallelism of single images and parallelism of batching animation frames
		/// </summary>
		private void SelectParallelType() {
			parallelTypeBox.Text = (generator.parallelType = parallelTypeBox.Checked) ? "...of Images" : "...of Animation Frames";
		}
		/// <summary>
		/// Maximum number of threads
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void threadsBar_Scroll(object sender, EventArgs e) {
			SelectMaxThreads();
			generator.SelectThreadingDepth();
		}
		/// <summary>
		/// Framerate Delay (for previes and for gif encode, so if encoding gif, it will restart the generation)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void delayBox_TextChanged(object sender, EventArgs e) {
			short newDelay = 1;
			if (!short.TryParse(delayBox.Text, out newDelay) || newDelay <= 0)
				newDelay = 5;
			if (generator.delay == newDelay)
				return;
			if (generator.encode == 2)
				Abort();
			// Delay is diffenret, change it, and restart the generation if ou were encoding a gif
			generator.delay = newDelay;
			int fpsrate = 100 / generator.delay;
			timer.Interval = generator.delay * 10;
			delayLabel.Text = "Delay: " + (generator.delay * 10).ToString() + "ms, Framerate: " + fpsrate.ToString();
			if (generator.encode == 2)
				ResetGenerate();
		}
		/// <summary>
		/// Select Previous frame to display
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void prevButton_Click(object sender, EventArgs e) {
			animated = false;
			int b = generator.GetBitmapsFinished();
			currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + b - 1) % b;
		}
		/// <summary>
		/// Select Next frame to display
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void nextButton_Click(object sender, EventArgs e) {
			animated = false;
			int b = generator.GetBitmapsFinished();
			currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + 1) % b;
		}
		/// <summary>
		/// Toggle display animation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void animateButton_Click(object sender, EventArgs e) {
			animateButton.Text = (animated = !animated) ? "Playing" : "Paused";
		}
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
						AbortGenerate();
					break;
			}
		}
		/// <summary>
		/// Show readme help
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void helpButton_Click(object sender, EventArgs e) {
			helpPanel.Visible = screenPanel.Visible;
			screenPanel.Visible = !screenPanel.Visible;
		}
		/// <summary>
		/// Save Frame
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void png_Click(object sender, EventArgs e) {
			savePng.ShowDialog();
		}
		/// <summary>
		/// Save Gif
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void gif_Click(object sender, EventArgs e) {
			saveGif.ShowDialog();
		}
		/// <summary>
		/// Start/Stop Animation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void screenPanel_Click(object sender, EventArgs e) {
			animated = !animated;
		}
		/// <summary>
		/// Invalidation event of screen display - draws the current display frame bitmap.
		/// Get called repeatedly with new frame to animate the preview, if animation is toggled
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void screenPanel_Paint(object sender, PaintEventArgs e) {
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
		private void savePng_FileOk(object sender, CancelEventArgs e) {
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
		private void saveGif_FileOk(object sender, CancelEventArgs e) {
			gifPath = ((SaveFileDialog)sender).FileName;
			gifButton.Enabled = false;
			// Gif Export Task
			gTask = Task.Run(() => ExportGif(), (cancel = new()).Token);
		}
		/// <summary>
		/// Exports the animation into a GIF file
		/// Ackchyually - it just moves the already exported gifX.tmp to you desired location and name
		/// </summary>
		/// <returns></returns>
		private void ExportGif() {
			int attempt = 0;
			while (++attempt <= 10 && !cancel.Token.IsCancellationRequested && generator.SaveGif(gifPath))
				Thread.Sleep(1000);
			gTask = null;
		}
		#endregion

	}
}