using System.Windows.Forms;

namespace RgbFractalGenCs {
	partial class GeneratorForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			components = new System.ComponentModel.Container();
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneratorForm));
			fractalLabel = new Label();
			fractalSelect = new ComboBox();
			angleLabel = new Label();
			angleSelect = new ComboBox();
			colorLabel = new Label();
			colorSelect = new ComboBox();
			cutLabel = new Label();
			cutSelect = new ComboBox();
			cutparamBox = new TextBox();
			resX = new TextBox();
			resY = new TextBox();
			resSelect = new ComboBox();
			toolTips = new ToolTip(components);
			timer = new Timer(components);
			periodBox = new TextBox();
			delayBox = new TextBox();
			prevButton = new Button();
			nextButton = new Button();
			animateButton = new Button();
			pngButton = new Button();
			gifButton = new Button();
			savePng = new SaveFileDialog();
			saveGif = new SaveFileDialog();
			loadFractal = new OpenFileDialog();
			saveFractal = new SaveFileDialog();
			voidAmbientLabel = new Label();
			dotLabel = new Label();
			statusLabel = new Label();
			infoLabel = new Label();
			bloomLabel = new Label();
			defaultZoom = new TextBox();
			defaultAngle = new TextBox();
			defaultHue = new TextBox();
			periodMultiplierBox = new TextBox();
			periodLabel = new Label();
			helpButton = new Button();
			helpPanel = new Panel();
			helpLabel = new Label();
			spinSelect = new ComboBox();
			zoomLabel = new Label();
			hueSelect = new ComboBox();
			spinLabel = new Label();
			hueLabel = new Label();
			spinSpeedBox = new TextBox();
			hueSpeedBox = new TextBox();
			parallelTypeSelect = new ComboBox();
			ambBox = new TextBox();
			noiseBox = new TextBox();
			saturateBox = new TextBox();
			detailBox = new TextBox();
			blurBox = new TextBox();
			bloomBox = new TextBox();
			threadsBox = new TextBox();
			abortBox = new TextBox();
			brightnessBox = new TextBox();
			zoomSelect = new ComboBox();
			restartButton = new Button();
			encodeSelect = new ComboBox();
			debugBox = new CheckBox();
			debugLabel = new Label();
			generatorPanel = new Panel();
			panel1 = new Panel();
			label2 = new Label();
			fpsLabel = new Label();
			label1 = new Label();
			fpsBox = new TextBox();
			parallelLabel = new Label();
			zoomChildLabel = new Label();
			zoomChildBox = new TextBox();
			blurLabel = new Label();
			brightnessLabel = new Label();
			detailLabel = new Label();
			voidScaleLabel = new Label();
			voidNoiseLabel = new Label();
			voidBox = new TextBox();
			mp4Button = new Button();
			modeButton = new Button();
			editorPanel = new Panel();
			preButton = new Button();
			pointLabel = new Label();
			sizeLabel = new Label();
			colorBox = new TextBox();
			angleBox = new TextBox();
			addcutLabel = new Label();
			maxBox = new TextBox();
			addCut = new ComboBox();
			pointPanel = new Panel();
			addPoint = new Button();
			minBox = new TextBox();
			saveButton = new Button();
			loadButton = new Button();
			cutBox = new TextBox();
			addAngleButton = new Button();
			sizeBox = new TextBox();
			addColorButton = new Button();
			removeAngleButton = new Button();
			removeColorButton = new Button();
			saveMp4 = new SaveFileDialog();
			convertMp4 = new SaveFileDialog();
			paletteSelect = new ComboBox();
			paletteLabel = new Label();
			addPalette = new Button();
			removePalette = new Button();
			paletteDialog = new ColorDialog();
			helpPanel.SuspendLayout();
			generatorPanel.SuspendLayout();
			panel1.SuspendLayout();
			editorPanel.SuspendLayout();
			pointPanel.SuspendLayout();
			SuspendLayout();
			// 
			// fractalLabel
			// 
			fractalLabel.AutoSize = true;
			fractalLabel.ForeColor = System.Drawing.Color.White;
			fractalLabel.Location = new System.Drawing.Point(17, 17);
			fractalLabel.Margin = new Padding(4, 0, 4, 0);
			fractalLabel.Name = "fractalLabel";
			fractalLabel.Size = new System.Drawing.Size(45, 15);
			fractalLabel.TabIndex = 0;
			fractalLabel.Text = "Fractal:";
			// 
			// fractalSelect
			// 
			fractalSelect.FormattingEnabled = true;
			fractalSelect.Location = new System.Drawing.Point(71, 14);
			fractalSelect.Margin = new Padding(4, 3, 4, 3);
			fractalSelect.Name = "fractalSelect";
			fractalSelect.Size = new System.Drawing.Size(178, 23);
			fractalSelect.TabIndex = 1;
			fractalSelect.Text = "Select Fractal";
			fractalSelect.SelectedIndexChanged += FractalSelect_SelectedIndexChanged;
			fractalSelect.TextUpdate += FractalSelect_TextUpdate;
			// 
			// angleLabel
			// 
			angleLabel.AutoSize = true;
			angleLabel.ForeColor = System.Drawing.Color.White;
			angleLabel.Location = new System.Drawing.Point(17, 46);
			angleLabel.Margin = new Padding(4, 0, 4, 0);
			angleLabel.Name = "angleLabel";
			angleLabel.Size = new System.Drawing.Size(46, 15);
			angleLabel.TabIndex = 36;
			angleLabel.Text = "Angles:";
			// 
			// angleSelect
			// 
			angleSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			angleSelect.FormattingEnabled = true;
			angleSelect.Location = new System.Drawing.Point(71, 43);
			angleSelect.Margin = new Padding(4, 3, 4, 3);
			angleSelect.Name = "angleSelect";
			angleSelect.Size = new System.Drawing.Size(231, 23);
			angleSelect.TabIndex = 2;
			angleSelect.SelectedIndexChanged += AngleSelect_SelectedIndexChanged;
			// 
			// colorLabel
			// 
			colorLabel.AutoSize = true;
			colorLabel.ForeColor = System.Drawing.Color.White;
			colorLabel.Location = new System.Drawing.Point(17, 75);
			colorLabel.Margin = new Padding(4, 0, 4, 0);
			colorLabel.Name = "colorLabel";
			colorLabel.Size = new System.Drawing.Size(44, 15);
			colorLabel.TabIndex = 38;
			colorLabel.Text = "Colors:";
			// 
			// colorSelect
			// 
			colorSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			colorSelect.FormattingEnabled = true;
			colorSelect.Location = new System.Drawing.Point(71, 72);
			colorSelect.Margin = new Padding(4, 3, 4, 3);
			colorSelect.Name = "colorSelect";
			colorSelect.Size = new System.Drawing.Size(231, 23);
			colorSelect.TabIndex = 3;
			colorSelect.SelectedIndexChanged += ColorSelect_SelectedIndexChanged;
			// 
			// cutLabel
			// 
			cutLabel.AutoSize = true;
			cutLabel.ForeColor = System.Drawing.Color.White;
			cutLabel.Location = new System.Drawing.Point(17, 104);
			cutLabel.Margin = new Padding(4, 0, 4, 0);
			cutLabel.Name = "cutLabel";
			cutLabel.Size = new System.Drawing.Size(29, 15);
			cutLabel.TabIndex = 34;
			cutLabel.Text = "Cut:";
			// 
			// cutSelect
			// 
			cutSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			cutSelect.FormattingEnabled = true;
			cutSelect.Location = new System.Drawing.Point(71, 101);
			cutSelect.Margin = new Padding(4, 3, 4, 3);
			cutSelect.Name = "cutSelect";
			cutSelect.Size = new System.Drawing.Size(129, 23);
			cutSelect.TabIndex = 4;
			cutSelect.SelectedIndexChanged += CutSelect_SelectedIndexChanged;
			// 
			// cutparamBox
			// 
			cutparamBox.Location = new System.Drawing.Point(208, 101);
			cutparamBox.Margin = new Padding(4, 3, 4, 3);
			cutparamBox.Name = "cutparamBox";
			cutparamBox.Size = new System.Drawing.Size(94, 23);
			cutparamBox.TabIndex = 5;
			cutparamBox.Text = "0";
			cutparamBox.TextChanged += CutparamBox_TextChanged;
			// 
			// resX
			// 
			resX.Location = new System.Drawing.Point(17, 130);
			resX.Margin = new Padding(4, 3, 4, 3);
			resX.Name = "resX";
			resX.Size = new System.Drawing.Size(46, 23);
			resX.TabIndex = 7;
			resX.Text = "1920";
			resX.TextChanged += Resolution_Changed;
			// 
			// resY
			// 
			resY.Location = new System.Drawing.Point(71, 130);
			resY.Margin = new Padding(4, 3, 4, 3);
			resY.Name = "resY";
			resY.Size = new System.Drawing.Size(46, 23);
			resY.TabIndex = 8;
			resY.Text = "1080";
			resY.TextChanged += Resolution_Changed;
			// 
			// resSelect
			// 
			resSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			resSelect.FormattingEnabled = true;
			resSelect.Items.AddRange(new object[] { "80x80", "Custom:", "256x256", "512x512", "640x480", "1024x768", "1280x720", "720x1280", "1920x1080", "1080x1920", "1600x900", "900x1600", "2560x1440", "1440x2560", "3840x2160", "2160x3840", "5120x2880", "2880x5120", "7680x4320", "4320x7680" });
			resSelect.Location = new System.Drawing.Point(125, 130);
			resSelect.Margin = new Padding(4, 3, 4, 3);
			resSelect.Name = "resSelect";
			resSelect.Size = new System.Drawing.Size(177, 23);
			resSelect.TabIndex = 50;
			resSelect.SelectedIndexChanged += Resolution_Changed;
			// 
			// timer
			// 
			timer.Enabled = true;
			timer.Interval = 20;
			timer.Tick += Timer_Tick;
			// 
			// periodBox
			// 
			periodBox.Location = new System.Drawing.Point(72, 3);
			periodBox.Margin = new Padding(4, 3, 4, 3);
			periodBox.Name = "periodBox";
			periodBox.Size = new System.Drawing.Size(86, 23);
			periodBox.TabIndex = 10;
			periodBox.Text = "120";
			periodBox.TextChanged += PeriodBox_TextChanged;
			// 
			// delayBox
			// 
			delayBox.Location = new System.Drawing.Point(164, 430);
			delayBox.Margin = new Padding(4, 3, 4, 3);
			delayBox.Name = "delayBox";
			delayBox.Size = new System.Drawing.Size(59, 23);
			delayBox.TabIndex = 28;
			delayBox.Text = "5";
			delayBox.TextChanged += DelayBox_TextChanged;
			// 
			// prevButton
			// 
			prevButton.Location = new System.Drawing.Point(14, 316);
			prevButton.Margin = new Padding(4, 3, 4, 3);
			prevButton.Name = "prevButton";
			prevButton.Size = new System.Drawing.Size(30, 27);
			prevButton.TabIndex = 29;
			prevButton.Text = "<-";
			prevButton.UseVisualStyleBackColor = true;
			prevButton.Click += PrevButton_Click;
			// 
			// nextButton
			// 
			nextButton.Location = new System.Drawing.Point(124, 316);
			nextButton.Margin = new Padding(4, 3, 4, 3);
			nextButton.Name = "nextButton";
			nextButton.Size = new System.Drawing.Size(30, 27);
			nextButton.TabIndex = 30;
			nextButton.Text = "->";
			nextButton.UseVisualStyleBackColor = true;
			nextButton.Click += NextButton_Click;
			// 
			// animateButton
			// 
			animateButton.Location = new System.Drawing.Point(52, 316);
			animateButton.Margin = new Padding(4, 3, 4, 3);
			animateButton.Name = "animateButton";
			animateButton.Size = new System.Drawing.Size(64, 27);
			animateButton.TabIndex = 31;
			animateButton.Text = "Playing";
			animateButton.UseVisualStyleBackColor = true;
			animateButton.Click += AnimateButton_Click;
			// 
			// pngButton
			// 
			pngButton.Location = new System.Drawing.Point(62, 378);
			pngButton.Margin = new Padding(4, 3, 4, 3);
			pngButton.Name = "pngButton";
			pngButton.Size = new System.Drawing.Size(66, 27);
			pngButton.TabIndex = 34;
			pngButton.Text = "Save PNG";
			pngButton.UseVisualStyleBackColor = true;
			pngButton.Click += PngButton_Click;
			// 
			// gifButton
			// 
			gifButton.Location = new System.Drawing.Point(136, 378);
			gifButton.Margin = new Padding(4, 3, 4, 3);
			gifButton.Name = "gifButton";
			gifButton.Size = new System.Drawing.Size(59, 27);
			gifButton.TabIndex = 35;
			gifButton.Text = "Save GIF";
			gifButton.UseVisualStyleBackColor = true;
			gifButton.Click += SaveVideoButton_Click;
			// 
			// savePng
			// 
			savePng.DefaultExt = "png";
			savePng.Filter = "PNG files (*.png)|*.png";
			savePng.FileOk += SavePng_FileOk;
			// 
			// saveGif
			// 
			saveGif.DefaultExt = "gif";
			saveGif.Filter = "GIF files (*.gif)|*.gif";
			saveGif.FileOk += SaveVideo_FileOk;
			// 
			// loadFractal
			// 
			loadFractal.DefaultExt = "fractal";
			loadFractal.Filter = "Fractal files (*.fractal)|*.fractal";
			loadFractal.FileOk += LoadFractal_FileOk;
			// 
			// saveFractal
			// 
			saveFractal.DefaultExt = "fractal";
			saveFractal.Filter = "Fractal files (*.fractal)|*.fractal";
			saveFractal.FileOk += SaveFractal_FileOk;
			// 
			// voidAmbientLabel
			// 
			voidAmbientLabel.AutoSize = true;
			voidAmbientLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			voidAmbientLabel.ForeColor = System.Drawing.Color.White;
			voidAmbientLabel.Location = new System.Drawing.Point(4, 143);
			voidAmbientLabel.Margin = new Padding(4, 0, 4, 0);
			voidAmbientLabel.Name = "voidAmbientLabel";
			voidAmbientLabel.Size = new System.Drawing.Size(116, 15);
			voidAmbientLabel.TabIndex = 0;
			voidAmbientLabel.Text = "Void Ambient (0-30):";
			// 
			// dotLabel
			// 
			dotLabel.AutoSize = true;
			dotLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			dotLabel.ForeColor = System.Drawing.Color.White;
			dotLabel.Location = new System.Drawing.Point(4, 259);
			dotLabel.Margin = new Padding(4, 0, 4, 0);
			dotLabel.Name = "dotLabel";
			dotLabel.Size = new System.Drawing.Size(87, 15);
			dotLabel.TabIndex = 0;
			dotLabel.Text = "Saturate (0-10):";
			// 
			// statusLabel
			// 
			statusLabel.AutoSize = true;
			statusLabel.ForeColor = System.Drawing.Color.White;
			statusLabel.Location = new System.Drawing.Point(14, 352);
			statusLabel.Margin = new Padding(4, 0, 4, 0);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(64, 15);
			statusLabel.TabIndex = 0;
			statusLabel.Text = "Initializing:";
			// 
			// infoLabel
			// 
			infoLabel.AutoSize = true;
			infoLabel.ForeColor = System.Drawing.Color.White;
			infoLabel.Location = new System.Drawing.Point(88, 352);
			infoLabel.Margin = new Padding(4, 0, 4, 0);
			infoLabel.Name = "infoLabel";
			infoLabel.Size = new System.Drawing.Size(28, 15);
			infoLabel.TabIndex = 0;
			infoLabel.Text = "Info";
			infoLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// bloomLabel
			// 
			bloomLabel.AutoSize = true;
			bloomLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			bloomLabel.ForeColor = System.Drawing.Color.White;
			bloomLabel.Location = new System.Drawing.Point(4, 317);
			bloomLabel.Margin = new Padding(4, 0, 4, 0);
			bloomLabel.Name = "bloomLabel";
			bloomLabel.Size = new System.Drawing.Size(79, 15);
			bloomLabel.TabIndex = 0;
			bloomLabel.Text = "Bloom (0-40):";
			// 
			// defaultZoom
			// 
			defaultZoom.Location = new System.Drawing.Point(164, 32);
			defaultZoom.Name = "defaultZoom";
			defaultZoom.Size = new System.Drawing.Size(59, 23);
			defaultZoom.TabIndex = 13;
			defaultZoom.Text = "0";
			defaultZoom.TextChanged += DefaultZoom_TextChanged;
			// 
			// defaultAngle
			// 
			defaultAngle.Location = new System.Drawing.Point(98, 111);
			defaultAngle.Name = "defaultAngle";
			defaultAngle.Size = new System.Drawing.Size(60, 23);
			defaultAngle.TabIndex = 16;
			defaultAngle.Text = "0";
			defaultAngle.TextChanged += DefaultAngle_TextChanged;
			// 
			// defaultHue
			// 
			defaultHue.Location = new System.Drawing.Point(256, 159);
			defaultHue.Name = "defaultHue";
			defaultHue.Size = new System.Drawing.Size(46, 23);
			defaultHue.TabIndex = 19;
			defaultHue.Text = "0";
			defaultHue.TextChanged += DefaultHue_TextChanged;
			// 
			// periodMultiplierBox
			// 
			periodMultiplierBox.Location = new System.Drawing.Point(164, 3);
			periodMultiplierBox.Margin = new Padding(4, 3, 4, 3);
			periodMultiplierBox.Name = "periodMultiplierBox";
			periodMultiplierBox.Size = new System.Drawing.Size(59, 23);
			periodMultiplierBox.TabIndex = 11;
			periodMultiplierBox.Text = "1";
			periodMultiplierBox.TextChanged += PeriodMultiplierBox_TextChanged;
			// 
			// periodLabel
			// 
			periodLabel.AutoSize = true;
			periodLabel.ForeColor = System.Drawing.Color.White;
			periodLabel.Location = new System.Drawing.Point(4, 6);
			periodLabel.Name = "periodLabel";
			periodLabel.Size = new System.Drawing.Size(44, 15);
			periodLabel.TabIndex = 31;
			periodLabel.Text = "Period:";
			// 
			// helpButton
			// 
			helpButton.Location = new System.Drawing.Point(14, 378);
			helpButton.Margin = new Padding(4, 3, 4, 3);
			helpButton.Name = "helpButton";
			helpButton.Size = new System.Drawing.Size(40, 27);
			helpButton.TabIndex = 33;
			helpButton.Text = "Help";
			helpButton.UseVisualStyleBackColor = true;
			helpButton.Click += HelpButton_Click;
			// 
			// helpPanel
			// 
			helpPanel.AutoScroll = true;
			helpPanel.BackColor = System.Drawing.Color.White;
			helpPanel.Controls.Add(helpLabel);
			helpPanel.Location = new System.Drawing.Point(309, 14);
			helpPanel.Name = "helpPanel";
			helpPanel.Size = new System.Drawing.Size(763, 586);
			helpPanel.TabIndex = 0;
			// 
			// helpLabel
			// 
			helpLabel.AutoSize = true;
			helpLabel.Location = new System.Drawing.Point(15, 8);
			helpLabel.Name = "helpLabel";
			helpLabel.Size = new System.Drawing.Size(30, 15);
			helpLabel.TabIndex = 0;
			helpLabel.Text = "help";
			// 
			// spinSelect
			// 
			spinSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			spinSelect.FormattingEnabled = true;
			spinSelect.Items.AddRange(new object[] { "Random", "Clock", "None", "Counterclock", "Antispin" });
			spinSelect.Location = new System.Drawing.Point(4, 111);
			spinSelect.Name = "spinSelect";
			spinSelect.Size = new System.Drawing.Size(86, 23);
			spinSelect.TabIndex = 14;
			spinSelect.SelectedIndexChanged += SpinSelect_SelectedIndexChanged;
			// 
			// zoomLabel
			// 
			zoomLabel.AutoSize = true;
			zoomLabel.ForeColor = System.Drawing.Color.White;
			zoomLabel.Location = new System.Drawing.Point(4, 35);
			zoomLabel.Name = "zoomLabel";
			zoomLabel.Size = new System.Drawing.Size(42, 15);
			zoomLabel.TabIndex = 40;
			zoomLabel.Text = "Zoom:";
			// 
			// hueSelect
			// 
			hueSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			hueSelect.FormattingEnabled = true;
			hueSelect.Items.AddRange(new object[] { "Random", "Static", "->", "<-" });
			hueSelect.Location = new System.Drawing.Point(72, 61);
			hueSelect.Name = "hueSelect";
			hueSelect.Size = new System.Drawing.Size(86, 23);
			hueSelect.TabIndex = 17;
			hueSelect.SelectedIndexChanged += HueSelect_SelectedIndexChanged;
			// 
			// spinLabel
			// 
			spinLabel.AutoSize = true;
			spinLabel.ForeColor = System.Drawing.Color.White;
			spinLabel.Location = new System.Drawing.Point(4, 93);
			spinLabel.Name = "spinLabel";
			spinLabel.Size = new System.Drawing.Size(201, 15);
			spinLabel.TabIndex = 41;
			spinLabel.Text = "Spin direction         Default        Speed";
			// 
			// hueLabel
			// 
			hueLabel.AutoSize = true;
			hueLabel.ForeColor = System.Drawing.Color.White;
			hueLabel.Location = new System.Drawing.Point(4, 64);
			hueLabel.Name = "hueLabel";
			hueLabel.Size = new System.Drawing.Size(59, 15);
			hueLabel.TabIndex = 42;
			hueLabel.Text = "Hue Shift:";
			// 
			// spinSpeedBox
			// 
			spinSpeedBox.Location = new System.Drawing.Point(164, 111);
			spinSpeedBox.Name = "spinSpeedBox";
			spinSpeedBox.Size = new System.Drawing.Size(59, 23);
			spinSpeedBox.TabIndex = 43;
			spinSpeedBox.Text = "0";
			spinSpeedBox.TextChanged += SpinSpeedBox_TextChanged;
			// 
			// hueSpeedBox
			// 
			hueSpeedBox.Location = new System.Drawing.Point(164, 61);
			hueSpeedBox.Name = "hueSpeedBox";
			hueSpeedBox.Size = new System.Drawing.Size(59, 23);
			hueSpeedBox.TabIndex = 44;
			hueSpeedBox.Text = "0";
			hueSpeedBox.TextChanged += HueSpeedBox_TextChanged;
			// 
			// parallelTypeSelect
			// 
			parallelTypeSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			parallelTypeSelect.FormattingEnabled = true;
			parallelTypeSelect.Items.AddRange(new object[] { "Of Animation", "Of Depth" });
			parallelTypeSelect.Location = new System.Drawing.Point(53, 401);
			parallelTypeSelect.Name = "parallelTypeSelect";
			parallelTypeSelect.Size = new System.Drawing.Size(105, 23);
			parallelTypeSelect.TabIndex = 27;
			parallelTypeSelect.SelectedIndexChanged += ParallelTypeSelect_SelectedIndexChanged;
			// 
			// ambBox
			// 
			ambBox.Location = new System.Drawing.Point(164, 140);
			ambBox.Name = "ambBox";
			ambBox.Size = new System.Drawing.Size(59, 23);
			ambBox.TabIndex = 20;
			ambBox.Text = "20";
			ambBox.TextChanged += AmbBox_TextChanged;
			// 
			// noiseBox
			// 
			noiseBox.Location = new System.Drawing.Point(164, 169);
			noiseBox.Name = "noiseBox";
			noiseBox.Size = new System.Drawing.Size(59, 23);
			noiseBox.TabIndex = 21;
			noiseBox.Text = "20";
			noiseBox.TextChanged += NoiseBox_TextChanged;
			// 
			// saturateBox
			// 
			saturateBox.Location = new System.Drawing.Point(164, 256);
			saturateBox.Name = "saturateBox";
			saturateBox.Size = new System.Drawing.Size(59, 23);
			saturateBox.TabIndex = 22;
			saturateBox.Text = "10";
			saturateBox.TextChanged += SaturateBox_TextChanged;
			// 
			// detailBox
			// 
			detailBox.Location = new System.Drawing.Point(164, 227);
			detailBox.Name = "detailBox";
			detailBox.Size = new System.Drawing.Size(59, 23);
			detailBox.TabIndex = 23;
			detailBox.Text = "5";
			detailBox.TextChanged += DetailBox_TextChanged;
			// 
			// blurBox
			// 
			blurBox.Location = new System.Drawing.Point(164, 343);
			blurBox.Name = "blurBox";
			blurBox.Size = new System.Drawing.Size(59, 23);
			blurBox.TabIndex = 24;
			blurBox.Text = "0";
			blurBox.TextChanged += BlurBox_TextChanged;
			// 
			// bloomBox
			// 
			bloomBox.Location = new System.Drawing.Point(164, 314);
			bloomBox.Name = "bloomBox";
			bloomBox.Size = new System.Drawing.Size(59, 23);
			bloomBox.TabIndex = 25;
			bloomBox.Text = "10";
			bloomBox.TextChanged += BloomBox_TextChanged;
			// 
			// threadsBox
			// 
			threadsBox.Location = new System.Drawing.Point(164, 401);
			threadsBox.Name = "threadsBox";
			threadsBox.Size = new System.Drawing.Size(59, 23);
			threadsBox.TabIndex = 28;
			threadsBox.Text = "0";
			threadsBox.TextChanged += Parallel_Changed;
			// 
			// abortBox
			// 
			abortBox.Location = new System.Drawing.Point(164, 488);
			abortBox.Margin = new Padding(4, 3, 4, 3);
			abortBox.Name = "abortBox";
			abortBox.Size = new System.Drawing.Size(59, 23);
			abortBox.TabIndex = 45;
			abortBox.Text = "50";
			abortBox.TextChanged += AbortBox_TextChanged;
			// 
			// brightnessBox
			// 
			brightnessBox.Location = new System.Drawing.Point(164, 285);
			brightnessBox.Name = "brightnessBox";
			brightnessBox.Size = new System.Drawing.Size(59, 23);
			brightnessBox.TabIndex = 46;
			brightnessBox.Text = "100";
			brightnessBox.TextChanged += BrightnessBox_TextChanged;
			// 
			// zoomSelect
			// 
			zoomSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			zoomSelect.FormattingEnabled = true;
			zoomSelect.Items.AddRange(new object[] { "Random", "Out", "None", "In" });
			zoomSelect.Location = new System.Drawing.Point(72, 32);
			zoomSelect.Name = "zoomSelect";
			zoomSelect.Size = new System.Drawing.Size(86, 23);
			zoomSelect.TabIndex = 48;
			zoomSelect.SelectedIndexChanged += ZoomSelect_SelectedIndexChanged;
			// 
			// restartButton
			// 
			restartButton.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
			restartButton.Location = new System.Drawing.Point(162, 316);
			restartButton.Name = "restartButton";
			restartButton.Size = new System.Drawing.Size(108, 27);
			restartButton.TabIndex = 49;
			restartButton.Text = "! RESTART !";
			restartButton.UseVisualStyleBackColor = false;
			restartButton.Click += RestartButton_Click;
			// 
			// encodeSelect
			// 
			encodeSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			encodeSelect.FormattingEnabled = true;
			encodeSelect.Items.AddRange(new object[] { "Only Image", "Animation RAM", "Local GIF", "Global GIF", "Encode Mp4", "AllSeeds" });
			encodeSelect.Location = new System.Drawing.Point(162, 349);
			encodeSelect.Name = "encodeSelect";
			encodeSelect.Size = new System.Drawing.Size(108, 23);
			encodeSelect.TabIndex = 51;
			encodeSelect.SelectedIndexChanged += EncodeSelect_SelectedIndexChanged;
			// 
			// debugBox
			// 
			debugBox.AutoSize = true;
			debugBox.ForeColor = System.Drawing.Color.White;
			debugBox.Location = new System.Drawing.Point(17, 606);
			debugBox.Name = "debugBox";
			debugBox.Size = new System.Drawing.Size(84, 19);
			debugBox.TabIndex = 52;
			debugBox.Text = "Debug Log";
			debugBox.UseVisualStyleBackColor = true;
			debugBox.CheckedChanged += DebugBox_CheckedChanged;
			// 
			// debugLabel
			// 
			debugLabel.AutoSize = true;
			debugLabel.ForeColor = System.Drawing.Color.White;
			debugLabel.Location = new System.Drawing.Point(17, 628);
			debugLabel.Name = "debugLabel";
			debugLabel.Size = new System.Drawing.Size(73, 15);
			debugLabel.TabIndex = 53;
			debugLabel.Text = "DebugString";
			// 
			// generatorPanel
			// 
			generatorPanel.Controls.Add(panel1);
			generatorPanel.Controls.Add(mp4Button);
			generatorPanel.Controls.Add(gifButton);
			generatorPanel.Controls.Add(pngButton);
			generatorPanel.Controls.Add(infoLabel);
			generatorPanel.Controls.Add(encodeSelect);
			generatorPanel.Controls.Add(statusLabel);
			generatorPanel.Controls.Add(restartButton);
			generatorPanel.Controls.Add(animateButton);
			generatorPanel.Controls.Add(nextButton);
			generatorPanel.Controls.Add(prevButton);
			generatorPanel.Controls.Add(helpButton);
			generatorPanel.Location = new System.Drawing.Point(17, 187);
			generatorPanel.Name = "generatorPanel";
			generatorPanel.Size = new System.Drawing.Size(286, 413);
			generatorPanel.TabIndex = 54;
			// 
			// panel1
			// 
			panel1.AutoScroll = true;
			panel1.Controls.Add(label2);
			panel1.Controls.Add(fpsLabel);
			panel1.Controls.Add(label1);
			panel1.Controls.Add(fpsBox);
			panel1.Controls.Add(parallelLabel);
			panel1.Controls.Add(zoomChildLabel);
			panel1.Controls.Add(zoomChildBox);
			panel1.Controls.Add(blurLabel);
			panel1.Controls.Add(brightnessLabel);
			panel1.Controls.Add(detailLabel);
			panel1.Controls.Add(voidScaleLabel);
			panel1.Controls.Add(voidNoiseLabel);
			panel1.Controls.Add(abortBox);
			panel1.Controls.Add(periodBox);
			panel1.Controls.Add(voidBox);
			panel1.Controls.Add(hueSelect);
			panel1.Controls.Add(zoomLabel);
			panel1.Controls.Add(zoomSelect);
			panel1.Controls.Add(spinLabel);
			panel1.Controls.Add(brightnessBox);
			panel1.Controls.Add(delayBox);
			panel1.Controls.Add(dotLabel);
			panel1.Controls.Add(threadsBox);
			panel1.Controls.Add(hueLabel);
			panel1.Controls.Add(periodLabel);
			panel1.Controls.Add(spinSelect);
			panel1.Controls.Add(blurBox);
			panel1.Controls.Add(spinSpeedBox);
			panel1.Controls.Add(defaultAngle);
			panel1.Controls.Add(parallelTypeSelect);
			panel1.Controls.Add(bloomBox);
			panel1.Controls.Add(hueSpeedBox);
			panel1.Controls.Add(periodMultiplierBox);
			panel1.Controls.Add(ambBox);
			panel1.Controls.Add(saturateBox);
			panel1.Controls.Add(voidAmbientLabel);
			panel1.Controls.Add(defaultZoom);
			panel1.Controls.Add(bloomLabel);
			panel1.Controls.Add(noiseBox);
			panel1.Controls.Add(detailBox);
			panel1.Location = new System.Drawing.Point(14, 10);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(256, 300);
			panel1.TabIndex = 60;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Segoe UI", 9F);
			label2.ForeColor = System.Drawing.Color.White;
			label2.Location = new System.Drawing.Point(4, 491);
			label2.Margin = new Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(72, 15);
			label2.TabIndex = 69;
			label2.Text = "Abort Delay:";
			// 
			// fpsLabel
			// 
			fpsLabel.AutoSize = true;
			fpsLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			fpsLabel.ForeColor = System.Drawing.Color.White;
			fpsLabel.Location = new System.Drawing.Point(4, 462);
			fpsLabel.Margin = new Padding(4, 0, 4, 0);
			fpsLabel.Name = "fpsLabel";
			fpsLabel.Size = new System.Drawing.Size(63, 15);
			fpsLabel.TabIndex = 68;
			fpsLabel.Text = "Framerate:";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Segoe UI", 9F);
			label1.ForeColor = System.Drawing.Color.White;
			label1.Location = new System.Drawing.Point(4, 433);
			label1.Margin = new Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(39, 15);
			label1.TabIndex = 67;
			label1.Text = "Delay:";
			// 
			// fpsBox
			// 
			fpsBox.Location = new System.Drawing.Point(164, 459);
			fpsBox.Margin = new Padding(4, 3, 4, 3);
			fpsBox.Name = "fpsBox";
			fpsBox.Size = new System.Drawing.Size(59, 23);
			fpsBox.TabIndex = 53;
			fpsBox.Text = "20";
			fpsBox.TextChanged += FpsBox_TextChanged;
			// 
			// parallelLabel
			// 
			parallelLabel.AutoSize = true;
			parallelLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			parallelLabel.ForeColor = System.Drawing.Color.White;
			parallelLabel.Location = new System.Drawing.Point(4, 404);
			parallelLabel.Margin = new Padding(4, 0, 4, 0);
			parallelLabel.Name = "parallelLabel";
			parallelLabel.Size = new System.Drawing.Size(48, 15);
			parallelLabel.TabIndex = 66;
			parallelLabel.Text = "Parallel:";
			// 
			// zoomChildLabel
			// 
			zoomChildLabel.AutoSize = true;
			zoomChildLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			zoomChildLabel.ForeColor = System.Drawing.Color.White;
			zoomChildLabel.Location = new System.Drawing.Point(4, 375);
			zoomChildLabel.Margin = new Padding(4, 0, 4, 0);
			zoomChildLabel.Name = "zoomChildLabel";
			zoomChildLabel.Size = new System.Drawing.Size(73, 15);
			zoomChildLabel.TabIndex = 65;
			zoomChildLabel.Text = "Zoom Child:";
			// 
			// zoomChildBox
			// 
			zoomChildBox.Location = new System.Drawing.Point(164, 372);
			zoomChildBox.Name = "zoomChildBox";
			zoomChildBox.Size = new System.Drawing.Size(59, 23);
			zoomChildBox.TabIndex = 64;
			zoomChildBox.Text = "0";
			zoomChildBox.TextChanged += ZoomChildBox_TextChanged;
			// 
			// blurLabel
			// 
			blurLabel.AutoSize = true;
			blurLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			blurLabel.ForeColor = System.Drawing.Color.White;
			blurLabel.Location = new System.Drawing.Point(4, 346);
			blurLabel.Margin = new Padding(4, 0, 4, 0);
			blurLabel.Name = "blurLabel";
			blurLabel.Size = new System.Drawing.Size(107, 15);
			blurLabel.TabIndex = 63;
			blurLabel.Text = "Motion Blur (0-40):";
			// 
			// brightnessLabel
			// 
			brightnessLabel.AutoSize = true;
			brightnessLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			brightnessLabel.ForeColor = System.Drawing.Color.White;
			brightnessLabel.Location = new System.Drawing.Point(4, 288);
			brightnessLabel.Margin = new Padding(4, 0, 4, 0);
			brightnessLabel.Name = "brightnessLabel";
			brightnessLabel.Size = new System.Drawing.Size(105, 15);
			brightnessLabel.TabIndex = 62;
			brightnessLabel.Text = "Brightness (0-300):";
			// 
			// detailLabel
			// 
			detailLabel.AutoSize = true;
			detailLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			detailLabel.ForeColor = System.Drawing.Color.White;
			detailLabel.Location = new System.Drawing.Point(4, 230);
			detailLabel.Margin = new Padding(4, 0, 4, 0);
			detailLabel.Name = "detailLabel";
			detailLabel.Size = new System.Drawing.Size(74, 15);
			detailLabel.TabIndex = 61;
			detailLabel.Text = "Detail (0-10):";
			// 
			// voidScaleLabel
			// 
			voidScaleLabel.AutoSize = true;
			voidScaleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			voidScaleLabel.ForeColor = System.Drawing.Color.White;
			voidScaleLabel.Location = new System.Drawing.Point(4, 201);
			voidScaleLabel.Margin = new Padding(4, 0, 4, 0);
			voidScaleLabel.Name = "voidScaleLabel";
			voidScaleLabel.Size = new System.Drawing.Size(103, 15);
			voidScaleLabel.TabIndex = 53;
			voidScaleLabel.Text = "Void Scale (0-300):";
			// 
			// voidNoiseLabel
			// 
			voidNoiseLabel.AutoSize = true;
			voidNoiseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			voidNoiseLabel.ForeColor = System.Drawing.Color.White;
			voidNoiseLabel.Location = new System.Drawing.Point(4, 172);
			voidNoiseLabel.Margin = new Padding(4, 0, 4, 0);
			voidNoiseLabel.Name = "voidNoiseLabel";
			voidNoiseLabel.Size = new System.Drawing.Size(100, 15);
			voidNoiseLabel.TabIndex = 49;
			voidNoiseLabel.Text = "Void Noise (0-30):";
			// 
			// voidBox
			// 
			voidBox.Location = new System.Drawing.Point(164, 198);
			voidBox.Name = "voidBox";
			voidBox.Size = new System.Drawing.Size(59, 23);
			voidBox.TabIndex = 52;
			voidBox.Text = "8";
			voidBox.TextChanged += VoidBox_TextChanged;
			// 
			// mp4Button
			// 
			mp4Button.Location = new System.Drawing.Point(201, 378);
			mp4Button.Name = "mp4Button";
			mp4Button.Size = new System.Drawing.Size(69, 27);
			mp4Button.TabIndex = 57;
			mp4Button.Text = "Save Mp4";
			mp4Button.UseVisualStyleBackColor = true;
			mp4Button.Click += Mp4Button_Click;
			// 
			// modeButton
			// 
			modeButton.Location = new System.Drawing.Point(256, 14);
			modeButton.Name = "modeButton";
			modeButton.Size = new System.Drawing.Size(47, 23);
			modeButton.TabIndex = 55;
			modeButton.Text = "EDIT";
			modeButton.UseVisualStyleBackColor = true;
			modeButton.Click += ModeButton_Click;
			// 
			// editorPanel
			// 
			editorPanel.Controls.Add(preButton);
			editorPanel.Controls.Add(pointLabel);
			editorPanel.Controls.Add(sizeLabel);
			editorPanel.Controls.Add(colorBox);
			editorPanel.Controls.Add(angleBox);
			editorPanel.Controls.Add(addcutLabel);
			editorPanel.Controls.Add(maxBox);
			editorPanel.Controls.Add(addCut);
			editorPanel.Controls.Add(pointPanel);
			editorPanel.Controls.Add(minBox);
			editorPanel.Controls.Add(saveButton);
			editorPanel.Controls.Add(loadButton);
			editorPanel.Controls.Add(cutBox);
			editorPanel.Controls.Add(addAngleButton);
			editorPanel.Controls.Add(sizeBox);
			editorPanel.Controls.Add(addColorButton);
			editorPanel.Controls.Add(removeAngleButton);
			editorPanel.Controls.Add(removeColorButton);
			editorPanel.Location = new System.Drawing.Point(17, 661);
			editorPanel.Name = "editorPanel";
			editorPanel.Size = new System.Drawing.Size(286, 413);
			editorPanel.TabIndex = 56;
			// 
			// preButton
			// 
			preButton.Location = new System.Drawing.Point(148, 380);
			preButton.Name = "preButton";
			preButton.Size = new System.Drawing.Size(122, 27);
			preButton.TabIndex = 58;
			preButton.Text = "PREVIEW MODE";
			preButton.UseVisualStyleBackColor = true;
			preButton.Click += PreButton_Click;
			// 
			// pointLabel
			// 
			pointLabel.AutoSize = true;
			pointLabel.ForeColor = System.Drawing.Color.White;
			pointLabel.Location = new System.Drawing.Point(29, 13);
			pointLabel.Margin = new Padding(4, 0, 4, 0);
			pointLabel.Name = "pointLabel";
			pointLabel.Size = new System.Drawing.Size(145, 15);
			pointLabel.TabIndex = 52;
			pointLabel.Text = "    X              Y             Angle";
			// 
			// sizeLabel
			// 
			sizeLabel.AutoSize = true;
			sizeLabel.ForeColor = System.Drawing.Color.White;
			sizeLabel.Location = new System.Drawing.Point(14, 244);
			sizeLabel.Margin = new Padding(4, 0, 4, 0);
			sizeLabel.Name = "sizeLabel";
			sizeLabel.Size = new System.Drawing.Size(252, 15);
			sizeLabel.TabIndex = 65;
			sizeLabel.Text = "ChildSize      MaxSize        MinSize          Cutsize ";
			// 
			// colorBox
			// 
			colorBox.Location = new System.Drawing.Point(14, 320);
			colorBox.Name = "colorBox";
			colorBox.Size = new System.Drawing.Size(101, 23);
			colorBox.TabIndex = 68;
			colorBox.Text = "NewColorsName";
			// 
			// angleBox
			// 
			angleBox.Location = new System.Drawing.Point(14, 292);
			angleBox.Name = "angleBox";
			angleBox.Size = new System.Drawing.Size(101, 23);
			angleBox.TabIndex = 52;
			angleBox.Text = "NewAnglesName";
			// 
			// addcutLabel
			// 
			addcutLabel.AutoSize = true;
			addcutLabel.ForeColor = System.Drawing.Color.White;
			addcutLabel.Location = new System.Drawing.Point(14, 352);
			addcutLabel.Margin = new Padding(4, 0, 4, 0);
			addcutLabel.Name = "addcutLabel";
			addcutLabel.Size = new System.Drawing.Size(101, 15);
			addcutLabel.TabIndex = 57;
			addcutLabel.Text = "Add CutFunction:";
			// 
			// maxBox
			// 
			maxBox.Location = new System.Drawing.Point(80, 262);
			maxBox.Name = "maxBox";
			maxBox.Size = new System.Drawing.Size(56, 23);
			maxBox.TabIndex = 66;
			maxBox.TextChanged += MaxBox_TextChanged;
			// 
			// addCut
			// 
			addCut.FormattingEnabled = true;
			addCut.Location = new System.Drawing.Point(121, 349);
			addCut.Margin = new Padding(4, 3, 4, 3);
			addCut.Name = "addCut";
			addCut.Size = new System.Drawing.Size(149, 23);
			addCut.TabIndex = 67;
			addCut.Text = "Select CutFunction";
			addCut.SelectedIndexChanged += AddCut_SelectedIndexChanged;
			// 
			// pointPanel
			// 
			pointPanel.AutoScroll = true;
			pointPanel.Controls.Add(addPoint);
			pointPanel.Location = new System.Drawing.Point(14, 31);
			pointPanel.Name = "pointPanel";
			pointPanel.Size = new System.Drawing.Size(256, 210);
			pointPanel.TabIndex = 59;
			// 
			// addPoint
			// 
			addPoint.Location = new System.Drawing.Point(15, 11);
			addPoint.Name = "addPoint";
			addPoint.Size = new System.Drawing.Size(205, 23);
			addPoint.TabIndex = 64;
			addPoint.Text = "ADD CHILD";
			addPoint.UseVisualStyleBackColor = true;
			addPoint.Click += AddPoint_Click;
			// 
			// minBox
			// 
			minBox.Location = new System.Drawing.Point(148, 262);
			minBox.Name = "minBox";
			minBox.Size = new System.Drawing.Size(56, 23);
			minBox.TabIndex = 65;
			minBox.TextChanged += MinBox_TextChanged;
			// 
			// saveButton
			// 
			saveButton.Location = new System.Drawing.Point(80, 380);
			saveButton.Name = "saveButton";
			saveButton.Size = new System.Drawing.Size(56, 27);
			saveButton.TabIndex = 58;
			saveButton.Text = "SAVE";
			saveButton.UseVisualStyleBackColor = true;
			saveButton.Click += SaveButton_Click;
			// 
			// loadButton
			// 
			loadButton.Location = new System.Drawing.Point(14, 380);
			loadButton.Name = "loadButton";
			loadButton.Size = new System.Drawing.Size(56, 27);
			loadButton.TabIndex = 57;
			loadButton.Text = "LOAD";
			loadButton.UseVisualStyleBackColor = true;
			loadButton.Click += LoadButton_Click;
			// 
			// cutBox
			// 
			cutBox.Location = new System.Drawing.Point(214, 262);
			cutBox.Name = "cutBox";
			cutBox.Size = new System.Drawing.Size(56, 23);
			cutBox.TabIndex = 64;
			cutBox.TextChanged += CutBox_TextChanged;
			// 
			// addAngleButton
			// 
			addAngleButton.Location = new System.Drawing.Point(121, 291);
			addAngleButton.Name = "addAngleButton";
			addAngleButton.Size = new System.Drawing.Size(122, 23);
			addAngleButton.TabIndex = 60;
			addAngleButton.Text = "ADD ANGLES";
			addAngleButton.UseVisualStyleBackColor = true;
			addAngleButton.Click += AddAngleButton_Click;
			// 
			// sizeBox
			// 
			sizeBox.Location = new System.Drawing.Point(14, 262);
			sizeBox.Name = "sizeBox";
			sizeBox.Size = new System.Drawing.Size(56, 23);
			sizeBox.TabIndex = 57;
			sizeBox.TextChanged += SizeBox_TextChanged;
			// 
			// addColorButton
			// 
			addColorButton.Location = new System.Drawing.Point(121, 320);
			addColorButton.Name = "addColorButton";
			addColorButton.Size = new System.Drawing.Size(122, 23);
			addColorButton.TabIndex = 61;
			addColorButton.Text = "ADD COLORS";
			addColorButton.UseVisualStyleBackColor = true;
			addColorButton.Click += AddColorButton_Click;
			// 
			// removeAngleButton
			// 
			removeAngleButton.Location = new System.Drawing.Point(249, 291);
			removeAngleButton.Name = "removeAngleButton";
			removeAngleButton.Size = new System.Drawing.Size(21, 23);
			removeAngleButton.TabIndex = 62;
			removeAngleButton.Text = "X";
			removeAngleButton.UseVisualStyleBackColor = true;
			removeAngleButton.Click += RemoveAngleButton_Click;
			// 
			// removeColorButton
			// 
			removeColorButton.Location = new System.Drawing.Point(249, 320);
			removeColorButton.Name = "removeColorButton";
			removeColorButton.Size = new System.Drawing.Size(21, 23);
			removeColorButton.TabIndex = 63;
			removeColorButton.Text = "X";
			removeColorButton.UseVisualStyleBackColor = true;
			removeColorButton.Click += RemoveColorButton_Click;
			// 
			// saveMp4
			// 
			saveMp4.DefaultExt = "mp4";
			saveMp4.Filter = "MP4 files (*.mp4)|*.mp4";
			saveMp4.FileOk += SaveVideo_FileOk;
			// 
			// convertMp4
			// 
			convertMp4.DefaultExt = "mp4";
			convertMp4.Filter = "MP4 files (*.mp4)|*.mp4";
			convertMp4.FileOk += ConvertMp4_FileOk;
			// 
			// paletteSelect
			// 
			paletteSelect.DrawMode = DrawMode.OwnerDrawFixed;
			paletteSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			paletteSelect.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			paletteSelect.FormattingEnabled = true;
			paletteSelect.Items.AddRange(new object[] { "Random" });
			paletteSelect.Location = new System.Drawing.Point(71, 159);
			paletteSelect.Margin = new Padding(4, 3, 4, 3);
			paletteSelect.Name = "paletteSelect";
			paletteSelect.Size = new System.Drawing.Size(129, 23);
			paletteSelect.TabIndex = 57;
			paletteSelect.DrawItem += PaletteSelect_DrawItem;
			paletteSelect.SelectedIndexChanged += PaletteSelect_SelectedIndexChanged;
			// 
			// paletteLabel
			// 
			paletteLabel.AutoSize = true;
			paletteLabel.ForeColor = System.Drawing.Color.White;
			paletteLabel.Location = new System.Drawing.Point(17, 162);
			paletteLabel.Margin = new Padding(4, 0, 4, 0);
			paletteLabel.Name = "paletteLabel";
			paletteLabel.Size = new System.Drawing.Size(46, 15);
			paletteLabel.TabIndex = 58;
			paletteLabel.Text = "Palette:";
			// 
			// addPalette
			// 
			addPalette.Location = new System.Drawing.Point(232, 159);
			addPalette.Name = "addPalette";
			addPalette.Size = new System.Drawing.Size(18, 23);
			addPalette.TabIndex = 59;
			addPalette.Text = "+";
			addPalette.UseVisualStyleBackColor = true;
			addPalette.Click += AddPalette_Click;
			// 
			// removePalette
			// 
			removePalette.Location = new System.Drawing.Point(208, 159);
			removePalette.Name = "removePalette";
			removePalette.Size = new System.Drawing.Size(18, 23);
			removePalette.TabIndex = 60;
			removePalette.Text = "X";
			removePalette.UseVisualStyleBackColor = true;
			removePalette.Click += RemovePalette_Click;
			// 
			// GeneratorForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
			ClientSize = new System.Drawing.Size(1383, 1178);
			Controls.Add(removePalette);
			Controls.Add(addPalette);
			Controls.Add(paletteLabel);
			Controls.Add(paletteSelect);
			Controls.Add(editorPanel);
			Controls.Add(modeButton);
			Controls.Add(generatorPanel);
			Controls.Add(debugLabel);
			Controls.Add(fractalSelect);
			Controls.Add(debugBox);
			Controls.Add(resSelect);
			Controls.Add(helpPanel);
			Controls.Add(colorLabel);
			Controls.Add(colorSelect);
			Controls.Add(angleLabel);
			Controls.Add(angleSelect);
			Controls.Add(cutLabel);
			Controls.Add(cutSelect);
			Controls.Add(cutparamBox);
			Controls.Add(fractalLabel);
			Controls.Add(resX);
			Controls.Add(resY);
			Controls.Add(defaultHue);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(4, 3, 4, 3);
			Name = "GeneratorForm";
			Text = "RGB Fractal Zoom Generator C# v1.10.0";
			FormClosing += GeneratorForm_FormClosing;
			helpPanel.ResumeLayout(false);
			helpPanel.PerformLayout();
			generatorPanel.ResumeLayout(false);
			generatorPanel.PerformLayout();
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			editorPanel.ResumeLayout(false);
			editorPanel.PerformLayout();
			pointPanel.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}
		#endregion
		private System.Windows.Forms.Label fractalLabel;
		private System.Windows.Forms.ComboBox fractalSelect;
		private System.Windows.Forms.Label angleLabel;
		private System.Windows.Forms.ComboBox angleSelect;
		private System.Windows.Forms.Label colorLabel;
		private System.Windows.Forms.ComboBox colorSelect;
		private System.Windows.Forms.Label cutLabel;
		private System.Windows.Forms.ComboBox cutSelect;
		private System.Windows.Forms.TextBox cutparamBox;
		private System.Windows.Forms.TextBox resX;
		private System.Windows.Forms.TextBox resY;
		private System.Windows.Forms.ComboBox resSelect;
		private System.Windows.Forms.ToolTip toolTips;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.SaveFileDialog savePng;
		private System.Windows.Forms.SaveFileDialog saveGif;
		private System.Windows.Forms.SaveFileDialog saveFractal;
		private System.Windows.Forms.OpenFileDialog loadFractal;
		private System.Windows.Forms.TextBox periodBox;
		private System.Windows.Forms.TextBox delayBox;
		private System.Windows.Forms.Button prevButton;
		private System.Windows.Forms.Button nextButton;
		private System.Windows.Forms.Button animateButton;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.Button pngButton;
		private System.Windows.Forms.Button gifButton;
		private System.Windows.Forms.Label dotLabel;
		private System.Windows.Forms.Label voidAmbientLabel;
		private System.Windows.Forms.Label bloomLabel;
		private System.Windows.Forms.TextBox defaultZoom;
		private System.Windows.Forms.TextBox defaultAngle;
		private System.Windows.Forms.TextBox defaultHue;
		private System.Windows.Forms.TextBox periodMultiplierBox;
		private System.Windows.Forms.Label periodLabel;
		private System.Windows.Forms.Button helpButton;
		private System.Windows.Forms.Panel helpPanel;
		private System.Windows.Forms.Label helpLabel;
		private System.Windows.Forms.ComboBox spinSelect;
		private System.Windows.Forms.Label zoomLabel;
		private System.Windows.Forms.ComboBox hueSelect;
		private System.Windows.Forms.Label spinLabel;
		private System.Windows.Forms.Label hueLabel;
		private System.Windows.Forms.TextBox spinSpeedBox;
		private System.Windows.Forms.TextBox hueSpeedBox;
		private System.Windows.Forms.ComboBox parallelTypeSelect;
		private System.Windows.Forms.TextBox ambBox;
		private System.Windows.Forms.TextBox noiseBox;
		private System.Windows.Forms.TextBox saturateBox;
		private System.Windows.Forms.TextBox detailBox;
		private System.Windows.Forms.TextBox blurBox;
		private System.Windows.Forms.TextBox bloomBox;
		private System.Windows.Forms.TextBox threadsBox;
		private System.Windows.Forms.TextBox abortBox;
		private System.Windows.Forms.TextBox brightnessBox;
		private System.Windows.Forms.ComboBox zoomSelect;
		private System.Windows.Forms.Button restartButton;
		private System.Windows.Forms.ComboBox encodeSelect;
		private System.Windows.Forms.CheckBox debugBox;
		private System.Windows.Forms.Label debugLabel;
		private System.Windows.Forms.Panel generatorPanel;
		private System.Windows.Forms.Button modeButton;
		private System.Windows.Forms.Panel editorPanel;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Button loadButton;
		private System.Windows.Forms.Panel pointPanel;
		private System.Windows.Forms.Button addColorButton;
		private System.Windows.Forms.Button addAngleButton;
		private System.Windows.Forms.Button removeColorButton;
		private System.Windows.Forms.Button removeAngleButton;
		private System.Windows.Forms.Button addPoint;
		private System.Windows.Forms.TextBox sizeBox;
		private System.Windows.Forms.TextBox cutBox;
		private System.Windows.Forms.TextBox minBox;
		private System.Windows.Forms.TextBox maxBox;
		private System.Windows.Forms.ComboBox addCut;
		private System.Windows.Forms.Label addcutLabel;
		private System.Windows.Forms.TextBox angleBox;
		private System.Windows.Forms.TextBox colorBox;
		private System.Windows.Forms.Label pointLabel;
		private System.Windows.Forms.TextBox voidBox;
		private System.Windows.Forms.Label sizeLabel;
		private System.Windows.Forms.Button mp4Button;
		private System.Windows.Forms.SaveFileDialog saveMp4;
		private System.Windows.Forms.TextBox fpsBox;
		private System.Windows.Forms.SaveFileDialog convertMp4;
		private System.Windows.Forms.Button preButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label voidNoiseLabel;
		private System.Windows.Forms.Label detailLabel;
		private System.Windows.Forms.Label voidScaleLabel;
		private System.Windows.Forms.Label brightnessLabel;
		private System.Windows.Forms.Label blurLabel;
		private System.Windows.Forms.Label parallelLabel;
		private System.Windows.Forms.Label zoomChildLabel;
		private System.Windows.Forms.TextBox zoomChildBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label fpsLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox paletteSelect;
		private System.Windows.Forms.Label paletteLabel;
		private System.Windows.Forms.Button addPalette;
		private System.Windows.Forms.Button removePalette;
		private System.Windows.Forms.ColorDialog paletteDialog;
	}
}

