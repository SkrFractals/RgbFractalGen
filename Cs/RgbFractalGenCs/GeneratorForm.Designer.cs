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
			fractalLabel = new System.Windows.Forms.Label();
			fractalSelect = new System.Windows.Forms.ComboBox();
			angleLabel = new System.Windows.Forms.Label();
			angleSelect = new System.Windows.Forms.ComboBox();
			colorLabel = new System.Windows.Forms.Label();
			colorSelect = new System.Windows.Forms.ComboBox();
			cutLabel = new System.Windows.Forms.Label();
			cutSelect = new System.Windows.Forms.ComboBox();
			cutparamBox = new System.Windows.Forms.TextBox();
			resX = new System.Windows.Forms.TextBox();
			resY = new System.Windows.Forms.TextBox();
			resSelect = new System.Windows.Forms.ComboBox();
			toolTips = new System.Windows.Forms.ToolTip(components);
			timer = new System.Windows.Forms.Timer(components);
			periodBox = new System.Windows.Forms.TextBox();
			delayBox = new System.Windows.Forms.TextBox();
			prevButton = new System.Windows.Forms.Button();
			nextButton = new System.Windows.Forms.Button();
			animateButton = new System.Windows.Forms.Button();
			pngButton = new System.Windows.Forms.Button();
			gifButton = new System.Windows.Forms.Button();
			savePng = new System.Windows.Forms.SaveFileDialog();
			saveGif = new System.Windows.Forms.SaveFileDialog();
			loadFractal = new System.Windows.Forms.OpenFileDialog();
			saveFractal = new System.Windows.Forms.SaveFileDialog();
			delayLabel = new System.Windows.Forms.Label();
			voidLabel = new System.Windows.Forms.Label();
			dotLabel = new System.Windows.Forms.Label();
			statusLabel = new System.Windows.Forms.Label();
			infoLabel = new System.Windows.Forms.Label();
			blurLabel = new System.Windows.Forms.Label();
			defaultZoom = new System.Windows.Forms.TextBox();
			defaultAngle = new System.Windows.Forms.TextBox();
			defaultHue = new System.Windows.Forms.TextBox();
			periodMultiplierBox = new System.Windows.Forms.TextBox();
			periodLabel = new System.Windows.Forms.Label();
			helpButton = new System.Windows.Forms.Button();
			helpPanel = new System.Windows.Forms.Panel();
			helpLabel = new System.Windows.Forms.Label();
			spinSelect = new System.Windows.Forms.ComboBox();
			zoomLabel = new System.Windows.Forms.Label();
			hueSelect = new System.Windows.Forms.ComboBox();
			spinLabel = new System.Windows.Forms.Label();
			hueLabel = new System.Windows.Forms.Label();
			spinSpeedBox = new System.Windows.Forms.TextBox();
			hueSpeedBox = new System.Windows.Forms.TextBox();
			parallelTypeSelect = new System.Windows.Forms.ComboBox();
			ambBox = new System.Windows.Forms.TextBox();
			noiseBox = new System.Windows.Forms.TextBox();
			saturateBox = new System.Windows.Forms.TextBox();
			detailBox = new System.Windows.Forms.TextBox();
			blurBox = new System.Windows.Forms.TextBox();
			bloomBox = new System.Windows.Forms.TextBox();
			threadsBox = new System.Windows.Forms.TextBox();
			abortBox = new System.Windows.Forms.TextBox();
			threadsLabel = new System.Windows.Forms.Label();
			brightnessBox = new System.Windows.Forms.TextBox();
			brightnessLabel = new System.Windows.Forms.Label();
			zoomSelect = new System.Windows.Forms.ComboBox();
			restartButton = new System.Windows.Forms.Button();
			encodeSelect = new System.Windows.Forms.ComboBox();
			debugBox = new System.Windows.Forms.CheckBox();
			debugLabel = new System.Windows.Forms.Label();
			generatorPanel = new System.Windows.Forms.Panel();
			fpsBox = new System.Windows.Forms.TextBox();
			voidBox = new System.Windows.Forms.TextBox();
			modeButton = new System.Windows.Forms.Button();
			editorPanel = new System.Windows.Forms.Panel();
			preButton = new System.Windows.Forms.Button();
			pointLabel = new System.Windows.Forms.Label();
			sizeLabel = new System.Windows.Forms.Label();
			colorBox = new System.Windows.Forms.TextBox();
			angleBox = new System.Windows.Forms.TextBox();
			addcutLabel = new System.Windows.Forms.Label();
			maxBox = new System.Windows.Forms.TextBox();
			addCut = new System.Windows.Forms.ComboBox();
			pointPanel = new System.Windows.Forms.Panel();
			addPoint = new System.Windows.Forms.Button();
			minBox = new System.Windows.Forms.TextBox();
			saveButton = new System.Windows.Forms.Button();
			loadButton = new System.Windows.Forms.Button();
			cutBox = new System.Windows.Forms.TextBox();
			addAngleButton = new System.Windows.Forms.Button();
			sizeBox = new System.Windows.Forms.TextBox();
			addcolorButton = new System.Windows.Forms.Button();
			removeangleButton = new System.Windows.Forms.Button();
			removecolorButton = new System.Windows.Forms.Button();
			mp4Button = new System.Windows.Forms.Button();
			saveMp4 = new System.Windows.Forms.SaveFileDialog();
			convertMp4 = new System.Windows.Forms.SaveFileDialog();
			helpPanel.SuspendLayout();
			generatorPanel.SuspendLayout();
			editorPanel.SuspendLayout();
			pointPanel.SuspendLayout();
			SuspendLayout();
			// 
			// fractalLabel
			// 
			fractalLabel.AutoSize = true;
			fractalLabel.ForeColor = System.Drawing.Color.White;
			fractalLabel.Location = new System.Drawing.Point(17, 17);
			fractalLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			fractalLabel.Name = "fractalLabel";
			fractalLabel.Size = new System.Drawing.Size(45, 15);
			fractalLabel.TabIndex = 0;
			fractalLabel.Text = "Fractal:";
			// 
			// fractalSelect
			// 
			fractalSelect.FormattingEnabled = true;
			fractalSelect.Location = new System.Drawing.Point(71, 14);
			fractalSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
			angleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			angleLabel.Name = "angleLabel";
			angleLabel.Size = new System.Drawing.Size(46, 15);
			angleLabel.TabIndex = 36;
			angleLabel.Text = "Angles:";
			// 
			// angleSelect
			// 
			angleSelect.FormattingEnabled = true;
			angleSelect.Location = new System.Drawing.Point(71, 43);
			angleSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			angleSelect.Name = "angleSelect";
			angleSelect.Size = new System.Drawing.Size(231, 23);
			angleSelect.TabIndex = 2;
			angleSelect.Text = "Select Angles";
			angleSelect.SelectedIndexChanged += AngleSelect_SelectedIndexChanged;
			// 
			// colorLabel
			// 
			colorLabel.AutoSize = true;
			colorLabel.ForeColor = System.Drawing.Color.White;
			colorLabel.Location = new System.Drawing.Point(17, 75);
			colorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			colorLabel.Name = "colorLabel";
			colorLabel.Size = new System.Drawing.Size(44, 15);
			colorLabel.TabIndex = 38;
			colorLabel.Text = "Colors:";
			// 
			// colorSelect
			// 
			colorSelect.FormattingEnabled = true;
			colorSelect.Location = new System.Drawing.Point(71, 72);
			colorSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			colorSelect.Name = "colorSelect";
			colorSelect.Size = new System.Drawing.Size(231, 23);
			colorSelect.TabIndex = 3;
			colorSelect.Text = "Select Colors";
			colorSelect.SelectedIndexChanged += ColorSelect_SelectedIndexChanged;
			// 
			// cutLabel
			// 
			cutLabel.AutoSize = true;
			cutLabel.ForeColor = System.Drawing.Color.White;
			cutLabel.Location = new System.Drawing.Point(17, 104);
			cutLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			cutLabel.Name = "cutLabel";
			cutLabel.Size = new System.Drawing.Size(29, 15);
			cutLabel.TabIndex = 34;
			cutLabel.Text = "Cut:";
			// 
			// cutSelect
			// 
			cutSelect.FormattingEnabled = true;
			cutSelect.Location = new System.Drawing.Point(71, 101);
			cutSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cutSelect.Name = "cutSelect";
			cutSelect.Size = new System.Drawing.Size(152, 23);
			cutSelect.TabIndex = 4;
			cutSelect.Text = "Select CutFunction";
			cutSelect.SelectedIndexChanged += CutSelect_SelectedIndexChanged;
			// 
			// cutparamBox
			// 
			cutparamBox.Location = new System.Drawing.Point(231, 101);
			cutparamBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cutparamBox.Name = "cutparamBox";
			cutparamBox.Size = new System.Drawing.Size(71, 23);
			cutparamBox.TabIndex = 5;
			cutparamBox.Text = "0";
			cutparamBox.TextChanged += CutparamBox_TextChanged;
			// 
			// resX
			// 
			resX.Location = new System.Drawing.Point(17, 130);
			resX.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			resX.Name = "resX";
			resX.Size = new System.Drawing.Size(46, 23);
			resX.TabIndex = 7;
			resX.Text = "1920";
			resX.TextChanged += Resolution_Changed;
			// 
			// resY
			// 
			resY.Location = new System.Drawing.Point(71, 130);
			resY.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			resY.Name = "resY";
			resY.Size = new System.Drawing.Size(46, 23);
			resY.TabIndex = 8;
			resY.Text = "1080";
			resY.TextChanged += Resolution_Changed;
			// 
			// resSelect
			// 
			resSelect.FormattingEnabled = true;
			resSelect.Items.AddRange(new object[] { "80x80", "Custom:", "256x256", "512x512", "640x480", "1024x768", "1280x720", "720x1280", "1920x1080", "1080x1920", "1600x900", "900x1600", "2560x1440", "1440x2560", "3840x2160", "2160x3840", "5120x2880", "2880x5120", "7680x4320", "4320x7680" });
			resSelect.Location = new System.Drawing.Point(125, 130);
			resSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			resSelect.Name = "resSelect";
			resSelect.Size = new System.Drawing.Size(177, 23);
			resSelect.TabIndex = 50;
			resSelect.Text = "Select Resolution";
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
			periodBox.Location = new System.Drawing.Point(14, 11);
			periodBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			periodBox.Name = "periodBox";
			periodBox.Size = new System.Drawing.Size(86, 23);
			periodBox.TabIndex = 10;
			periodBox.Text = "120";
			periodBox.TextChanged += PeriodBox_TextChanged;
			// 
			// delayBox
			// 
			delayBox.Location = new System.Drawing.Point(174, 313);
			delayBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			delayBox.Name = "delayBox";
			delayBox.Size = new System.Drawing.Size(45, 23);
			delayBox.TabIndex = 28;
			delayBox.Text = "5";
			delayBox.TextChanged += DelayBox_TextChanged;
			// 
			// prevButton
			// 
			prevButton.Location = new System.Drawing.Point(14, 341);
			prevButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			prevButton.Name = "prevButton";
			prevButton.Size = new System.Drawing.Size(30, 27);
			prevButton.TabIndex = 29;
			prevButton.Text = "<-";
			prevButton.UseVisualStyleBackColor = true;
			prevButton.Click += PrevButton_Click;
			// 
			// nextButton
			// 
			nextButton.Location = new System.Drawing.Point(124, 341);
			nextButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			nextButton.Name = "nextButton";
			nextButton.Size = new System.Drawing.Size(30, 27);
			nextButton.TabIndex = 30;
			nextButton.Text = "->";
			nextButton.UseVisualStyleBackColor = true;
			nextButton.Click += NextButton_Click;
			// 
			// animateButton
			// 
			animateButton.Location = new System.Drawing.Point(52, 341);
			animateButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			animateButton.Name = "animateButton";
			animateButton.Size = new System.Drawing.Size(64, 27);
			animateButton.TabIndex = 31;
			animateButton.Text = "Playing";
			animateButton.UseVisualStyleBackColor = true;
			animateButton.Click += AnimateButton_Click;
			// 
			// pngButton
			// 
			pngButton.Location = new System.Drawing.Point(62, 403);
			pngButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			pngButton.Name = "pngButton";
			pngButton.Size = new System.Drawing.Size(66, 27);
			pngButton.TabIndex = 34;
			pngButton.Text = "Save PNG";
			pngButton.UseVisualStyleBackColor = true;
			pngButton.Click += PngButton_Click;
			// 
			// gifButton
			// 
			gifButton.Location = new System.Drawing.Point(136, 403);
			gifButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
			// delayLabel
			// 
			delayLabel.AutoSize = true;
			delayLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			delayLabel.ForeColor = System.Drawing.Color.White;
			delayLabel.Location = new System.Drawing.Point(14, 316);
			delayLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			delayLabel.Name = "delayLabel";
			delayLabel.Size = new System.Drawing.Size(105, 13);
			delayLabel.TabIndex = 0;
			delayLabel.Text = "Abort / Delay / FPS:";
			// 
			// voidLabel
			// 
			voidLabel.AutoSize = true;
			voidLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			voidLabel.ForeColor = System.Drawing.Color.White;
			voidLabel.Location = new System.Drawing.Point(14, 171);
			voidLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			voidLabel.Name = "voidLabel";
			voidLabel.Size = new System.Drawing.Size(143, 13);
			voidLabel.TabIndex = 0;
			voidLabel.Text = "Void Ambient/Noise (0-30):";
			// 
			// dotLabel
			// 
			dotLabel.AutoSize = true;
			dotLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			dotLabel.ForeColor = System.Drawing.Color.White;
			dotLabel.Location = new System.Drawing.Point(14, 200);
			dotLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			dotLabel.Name = "dotLabel";
			dotLabel.Size = new System.Drawing.Size(118, 13);
			dotLabel.TabIndex = 0;
			dotLabel.Text = "Saturate/Detail (0-10):";
			// 
			// statusLabel
			// 
			statusLabel.AutoSize = true;
			statusLabel.ForeColor = System.Drawing.Color.White;
			statusLabel.Location = new System.Drawing.Point(14, 377);
			statusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(64, 15);
			statusLabel.TabIndex = 0;
			statusLabel.Text = "Initializing:";
			// 
			// infoLabel
			// 
			infoLabel.AutoSize = true;
			infoLabel.ForeColor = System.Drawing.Color.White;
			infoLabel.Location = new System.Drawing.Point(88, 377);
			infoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			infoLabel.Name = "infoLabel";
			infoLabel.Size = new System.Drawing.Size(28, 15);
			infoLabel.TabIndex = 0;
			infoLabel.Text = "Info";
			infoLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// blurLabel
			// 
			blurLabel.AutoSize = true;
			blurLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			blurLabel.ForeColor = System.Drawing.Color.White;
			blurLabel.Location = new System.Drawing.Point(14, 229);
			blurLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			blurLabel.Name = "blurLabel";
			blurLabel.Size = new System.Drawing.Size(138, 13);
			blurLabel.TabIndex = 0;
			blurLabel.Text = "Bloom/Motion Blur (0-40):";
			// 
			// defaultZoom
			// 
			defaultZoom.Location = new System.Drawing.Point(108, 40);
			defaultZoom.Name = "defaultZoom";
			defaultZoom.Size = new System.Drawing.Size(60, 23);
			defaultZoom.TabIndex = 13;
			defaultZoom.Text = "0";
			defaultZoom.TextChanged += DefaultZoom_TextChanged;
			// 
			// defaultAngle
			// 
			defaultAngle.Location = new System.Drawing.Point(174, 90);
			defaultAngle.Name = "defaultAngle";
			defaultAngle.Size = new System.Drawing.Size(96, 23);
			defaultAngle.TabIndex = 16;
			defaultAngle.Text = "0";
			defaultAngle.TextChanged += DefaultAngle_TextChanged;
			// 
			// defaultHue
			// 
			defaultHue.Location = new System.Drawing.Point(174, 139);
			defaultHue.Name = "defaultHue";
			defaultHue.Size = new System.Drawing.Size(96, 23);
			defaultHue.TabIndex = 19;
			defaultHue.Text = "0";
			defaultHue.TextChanged += DefaultHue_TextChanged;
			// 
			// periodMultiplierBox
			// 
			periodMultiplierBox.Location = new System.Drawing.Point(108, 11);
			periodMultiplierBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			periodMultiplierBox.Name = "periodMultiplierBox";
			periodMultiplierBox.Size = new System.Drawing.Size(60, 23);
			periodMultiplierBox.TabIndex = 11;
			periodMultiplierBox.Text = "1";
			periodMultiplierBox.TextChanged += PeriodMultiplierBox_TextChanged;
			// 
			// periodLabel
			// 
			periodLabel.AutoSize = true;
			periodLabel.ForeColor = System.Drawing.Color.White;
			periodLabel.Location = new System.Drawing.Point(178, 14);
			periodLabel.Name = "periodLabel";
			periodLabel.Size = new System.Drawing.Size(54, 15);
			periodLabel.TabIndex = 31;
			periodLabel.Text = "<-Period";
			// 
			// helpButton
			// 
			helpButton.Location = new System.Drawing.Point(14, 403);
			helpButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
			spinSelect.FormattingEnabled = true;
			spinSelect.Items.AddRange(new object[] { "Random", "Clock", "None", "Counterclock", "Antispin" });
			spinSelect.Location = new System.Drawing.Point(14, 90);
			spinSelect.Name = "spinSelect";
			spinSelect.Size = new System.Drawing.Size(86, 23);
			spinSelect.TabIndex = 14;
			spinSelect.Text = "Select Spin";
			spinSelect.SelectedIndexChanged += SpinSelect_SelectedIndexChanged;
			// 
			// zoomLabel
			// 
			zoomLabel.AutoSize = true;
			zoomLabel.ForeColor = System.Drawing.Color.White;
			zoomLabel.Location = new System.Drawing.Point(178, 43);
			zoomLabel.Name = "zoomLabel";
			zoomLabel.Size = new System.Drawing.Size(52, 15);
			zoomLabel.TabIndex = 40;
			zoomLabel.Text = "<-Zoom";
			// 
			// hueSelect
			// 
			hueSelect.FormattingEnabled = true;
			hueSelect.Items.AddRange(new object[] { "Random", "RGB (static)", "BGR (static)", "RGB->GBR", "BGR->RBG", "RGB->BRG", "BGR->GRB" });
			hueSelect.Location = new System.Drawing.Point(14, 139);
			hueSelect.Name = "hueSelect";
			hueSelect.Size = new System.Drawing.Size(86, 23);
			hueSelect.TabIndex = 17;
			hueSelect.Text = "Select  Hue";
			hueSelect.SelectedIndexChanged += HueSelect_SelectedIndexChanged;
			// 
			// spinLabel
			// 
			spinLabel.AutoSize = true;
			spinLabel.ForeColor = System.Drawing.Color.White;
			spinLabel.Location = new System.Drawing.Point(14, 72);
			spinLabel.Name = "spinLabel";
			spinLabel.Size = new System.Drawing.Size(246, 15);
			spinLabel.TabIndex = 41;
			spinLabel.Text = "Spin direction      Spin Speed      Default Angle";
			// 
			// hueLabel
			// 
			hueLabel.AutoSize = true;
			hueLabel.ForeColor = System.Drawing.Color.White;
			hueLabel.Location = new System.Drawing.Point(14, 121);
			hueLabel.Name = "hueLabel";
			hueLabel.Size = new System.Drawing.Size(240, 15);
			hueLabel.TabIndex = 42;
			hueLabel.Text = "Hue Select             HueSpeed       Default Hue";
			// 
			// spinSpeedBox
			// 
			spinSpeedBox.Location = new System.Drawing.Point(106, 90);
			spinSpeedBox.Name = "spinSpeedBox";
			spinSpeedBox.Size = new System.Drawing.Size(62, 23);
			spinSpeedBox.TabIndex = 43;
			spinSpeedBox.Text = "0";
			spinSpeedBox.TextChanged += SpinSpeedBox_TextChanged;
			// 
			// hueSpeedBox
			// 
			hueSpeedBox.Location = new System.Drawing.Point(106, 139);
			hueSpeedBox.Name = "hueSpeedBox";
			hueSpeedBox.Size = new System.Drawing.Size(62, 23);
			hueSpeedBox.TabIndex = 44;
			hueSpeedBox.Text = "0";
			hueSpeedBox.TextChanged += HueSpeedBox_TextChanged;
			// 
			// parallelTypeSelect
			// 
			parallelTypeSelect.FormattingEnabled = true;
			parallelTypeSelect.Items.AddRange(new object[] { "Of Animation", "Of Depth" });
			parallelTypeSelect.Location = new System.Drawing.Point(111, 284);
			parallelTypeSelect.Name = "parallelTypeSelect";
			parallelTypeSelect.Size = new System.Drawing.Size(108, 23);
			parallelTypeSelect.TabIndex = 27;
			parallelTypeSelect.Text = "Parallelism Type";
			parallelTypeSelect.SelectedIndexChanged += ParallelTypeSelect_SelectedIndexChanged;
			// 
			// ambBox
			// 
			ambBox.Location = new System.Drawing.Point(174, 168);
			ambBox.Name = "ambBox";
			ambBox.Size = new System.Drawing.Size(45, 23);
			ambBox.TabIndex = 20;
			ambBox.Text = "20";
			ambBox.TextChanged += AmbBox_TextChanged;
			// 
			// noiseBox
			// 
			noiseBox.Location = new System.Drawing.Point(225, 168);
			noiseBox.Name = "noiseBox";
			noiseBox.Size = new System.Drawing.Size(45, 23);
			noiseBox.TabIndex = 21;
			noiseBox.Text = "20";
			noiseBox.TextChanged += NoiseBox_TextChanged;
			// 
			// saturateBox
			// 
			saturateBox.Location = new System.Drawing.Point(174, 197);
			saturateBox.Name = "saturateBox";
			saturateBox.Size = new System.Drawing.Size(45, 23);
			saturateBox.TabIndex = 22;
			saturateBox.Text = "5";
			saturateBox.TextChanged += SaturateBox_TextChanged;
			// 
			// detailBox
			// 
			detailBox.Location = new System.Drawing.Point(225, 197);
			detailBox.Name = "detailBox";
			detailBox.Size = new System.Drawing.Size(45, 23);
			detailBox.TabIndex = 23;
			detailBox.Text = "5";
			detailBox.TextChanged += DetailBox_TextChanged;
			// 
			// blurBox
			// 
			blurBox.Location = new System.Drawing.Point(225, 226);
			blurBox.Name = "blurBox";
			blurBox.Size = new System.Drawing.Size(45, 23);
			blurBox.TabIndex = 24;
			blurBox.Text = "0";
			blurBox.TextChanged += BlurBox_TextChanged;
			// 
			// bloomBox
			// 
			bloomBox.Location = new System.Drawing.Point(174, 226);
			bloomBox.Name = "bloomBox";
			bloomBox.Size = new System.Drawing.Size(45, 23);
			bloomBox.TabIndex = 25;
			bloomBox.Text = "0";
			bloomBox.TextChanged += BloomBox_TextChanged;
			// 
			// threadsBox
			// 
			threadsBox.Location = new System.Drawing.Point(225, 284);
			threadsBox.Name = "threadsBox";
			threadsBox.Size = new System.Drawing.Size(45, 23);
			threadsBox.TabIndex = 28;
			threadsBox.Text = "0";
			threadsBox.TextChanged += Parallel_Changed;
			// 
			// abortBox
			// 
			abortBox.Location = new System.Drawing.Point(124, 313);
			abortBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			abortBox.Name = "abortBox";
			abortBox.Size = new System.Drawing.Size(44, 23);
			abortBox.TabIndex = 45;
			abortBox.Text = "50";
			abortBox.TextChanged += AbortBox_TextChanged;
			// 
			// threadsLabel
			// 
			threadsLabel.AutoSize = true;
			threadsLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			threadsLabel.ForeColor = System.Drawing.Color.White;
			threadsLabel.Location = new System.Drawing.Point(14, 287);
			threadsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			threadsLabel.Name = "threadsLabel";
			threadsLabel.Size = new System.Drawing.Size(91, 13);
			threadsLabel.TabIndex = 0;
			threadsLabel.Text = "Parallel Threads:";
			// 
			// brightnessBox
			// 
			brightnessBox.Location = new System.Drawing.Point(174, 255);
			brightnessBox.Name = "brightnessBox";
			brightnessBox.Size = new System.Drawing.Size(45, 23);
			brightnessBox.TabIndex = 46;
			brightnessBox.Text = "100";
			brightnessBox.TextChanged += BrightnessBox_TextChanged;
			// 
			// brightnessLabel
			// 
			brightnessLabel.AutoSize = true;
			brightnessLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			brightnessLabel.ForeColor = System.Drawing.Color.White;
			brightnessLabel.Location = new System.Drawing.Point(14, 258);
			brightnessLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			brightnessLabel.Name = "brightnessLabel";
			brightnessLabel.Size = new System.Drawing.Size(160, 13);
			brightnessLabel.TabIndex = 47;
			brightnessLabel.Text = "Brightness/NoiseScale (0-300):";
			// 
			// zoomSelect
			// 
			zoomSelect.FormattingEnabled = true;
			zoomSelect.Items.AddRange(new object[] { "Random", "In", "Out" });
			zoomSelect.Location = new System.Drawing.Point(14, 40);
			zoomSelect.Name = "zoomSelect";
			zoomSelect.Size = new System.Drawing.Size(86, 23);
			zoomSelect.TabIndex = 48;
			zoomSelect.Text = "Select Zoom";
			zoomSelect.SelectedIndexChanged += ZoomSelect_SelectedIndexChanged;
			// 
			// restartButton
			// 
			restartButton.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
			restartButton.Location = new System.Drawing.Point(162, 341);
			restartButton.Name = "restartButton";
			restartButton.Size = new System.Drawing.Size(108, 27);
			restartButton.TabIndex = 49;
			restartButton.Text = "! RESTART !";
			restartButton.UseVisualStyleBackColor = false;
			restartButton.Click += RestartButton_Click;
			// 
			// encodeSelect
			// 
			encodeSelect.FormattingEnabled = true;
			encodeSelect.Items.AddRange(new object[] { "Only Image", "Animation RAM", "Local GIF", "Global GIF", "Encode Mp4", "AllSeeds" });
			encodeSelect.Location = new System.Drawing.Point(162, 374);
			encodeSelect.Name = "encodeSelect";
			encodeSelect.Size = new System.Drawing.Size(108, 23);
			encodeSelect.TabIndex = 51;
			encodeSelect.Text = "Generation Type";
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
			generatorPanel.Controls.Add(mp4Button);
			generatorPanel.Controls.Add(fpsBox);
			generatorPanel.Controls.Add(voidBox);
			generatorPanel.Controls.Add(zoomSelect);
			generatorPanel.Controls.Add(gifButton);
			generatorPanel.Controls.Add(pngButton);
			generatorPanel.Controls.Add(infoLabel);
			generatorPanel.Controls.Add(encodeSelect);
			generatorPanel.Controls.Add(statusLabel);
			generatorPanel.Controls.Add(threadsLabel);
			generatorPanel.Controls.Add(restartButton);
			generatorPanel.Controls.Add(dotLabel);
			generatorPanel.Controls.Add(voidLabel);
			generatorPanel.Controls.Add(brightnessLabel);
			generatorPanel.Controls.Add(animateButton);
			generatorPanel.Controls.Add(brightnessBox);
			generatorPanel.Controls.Add(nextButton);
			generatorPanel.Controls.Add(abortBox);
			generatorPanel.Controls.Add(prevButton);
			generatorPanel.Controls.Add(threadsBox);
			generatorPanel.Controls.Add(delayBox);
			generatorPanel.Controls.Add(delayLabel);
			generatorPanel.Controls.Add(bloomBox);
			generatorPanel.Controls.Add(blurBox);
			generatorPanel.Controls.Add(periodBox);
			generatorPanel.Controls.Add(detailBox);
			generatorPanel.Controls.Add(blurLabel);
			generatorPanel.Controls.Add(saturateBox);
			generatorPanel.Controls.Add(defaultZoom);
			generatorPanel.Controls.Add(noiseBox);
			generatorPanel.Controls.Add(defaultAngle);
			generatorPanel.Controls.Add(ambBox);
			generatorPanel.Controls.Add(defaultHue);
			generatorPanel.Controls.Add(parallelTypeSelect);
			generatorPanel.Controls.Add(periodMultiplierBox);
			generatorPanel.Controls.Add(hueSpeedBox);
			generatorPanel.Controls.Add(periodLabel);
			generatorPanel.Controls.Add(spinSpeedBox);
			generatorPanel.Controls.Add(helpButton);
			generatorPanel.Controls.Add(hueLabel);
			generatorPanel.Controls.Add(spinSelect);
			generatorPanel.Controls.Add(spinLabel);
			generatorPanel.Controls.Add(zoomLabel);
			generatorPanel.Controls.Add(hueSelect);
			generatorPanel.Location = new System.Drawing.Point(17, 159);
			generatorPanel.Name = "generatorPanel";
			generatorPanel.Size = new System.Drawing.Size(286, 441);
			generatorPanel.TabIndex = 54;
			// 
			// fpsBox
			// 
			fpsBox.Location = new System.Drawing.Point(225, 313);
			fpsBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			fpsBox.Name = "fpsBox";
			fpsBox.Size = new System.Drawing.Size(45, 23);
			fpsBox.TabIndex = 53;
			fpsBox.Text = "20";
			fpsBox.TextChanged += FpsBox_TextChanged;
			// 
			// voidBox
			// 
			voidBox.Location = new System.Drawing.Point(225, 255);
			voidBox.Name = "voidBox";
			voidBox.Size = new System.Drawing.Size(45, 23);
			voidBox.TabIndex = 52;
			voidBox.Text = "8";
			voidBox.TextChanged += VoidBox_TextChanged;
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
			editorPanel.Controls.Add(addcolorButton);
			editorPanel.Controls.Add(removeangleButton);
			editorPanel.Controls.Add(removecolorButton);
			editorPanel.Location = new System.Drawing.Point(17, 661);
			editorPanel.Name = "editorPanel";
			editorPanel.Size = new System.Drawing.Size(286, 441);
			editorPanel.TabIndex = 56;
			// 
			// preButton
			// 
			preButton.Location = new System.Drawing.Point(148, 410);
			preButton.Name = "preButton";
			preButton.Size = new System.Drawing.Size(122, 27);
			preButton.TabIndex = 58;
			preButton.Text = "PREVIEW MODE";
			preButton.UseVisualStyleBackColor = true;
			preButton.Click += preButton_Click;
			// 
			// pointLabel
			// 
			pointLabel.AutoSize = true;
			pointLabel.ForeColor = System.Drawing.Color.White;
			pointLabel.Location = new System.Drawing.Point(29, 13);
			pointLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			pointLabel.Name = "pointLabel";
			pointLabel.Size = new System.Drawing.Size(145, 15);
			pointLabel.TabIndex = 52;
			pointLabel.Text = "    X              Y             Angle";
			// 
			// sizeLabel
			// 
			sizeLabel.AutoSize = true;
			sizeLabel.ForeColor = System.Drawing.Color.White;
			sizeLabel.Location = new System.Drawing.Point(14, 274);
			sizeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			sizeLabel.Name = "sizeLabel";
			sizeLabel.Size = new System.Drawing.Size(252, 15);
			sizeLabel.TabIndex = 65;
			sizeLabel.Text = "ChildSize      MaxSize        MinSize          Cutsize ";
			// 
			// colorBox
			// 
			colorBox.Location = new System.Drawing.Point(14, 350);
			colorBox.Name = "colorBox";
			colorBox.Size = new System.Drawing.Size(101, 23);
			colorBox.TabIndex = 68;
			colorBox.Text = "NewColorsName";
			// 
			// angleBox
			// 
			angleBox.Location = new System.Drawing.Point(14, 322);
			angleBox.Name = "angleBox";
			angleBox.Size = new System.Drawing.Size(101, 23);
			angleBox.TabIndex = 52;
			angleBox.Text = "NewAnglesName";
			// 
			// addcutLabel
			// 
			addcutLabel.AutoSize = true;
			addcutLabel.ForeColor = System.Drawing.Color.White;
			addcutLabel.Location = new System.Drawing.Point(14, 382);
			addcutLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			addcutLabel.Name = "addcutLabel";
			addcutLabel.Size = new System.Drawing.Size(101, 15);
			addcutLabel.TabIndex = 57;
			addcutLabel.Text = "Add CutFunction:";
			// 
			// maxBox
			// 
			maxBox.Location = new System.Drawing.Point(80, 292);
			maxBox.Name = "maxBox";
			maxBox.Size = new System.Drawing.Size(56, 23);
			maxBox.TabIndex = 66;
			maxBox.TextChanged += MaxBox_TextChanged;
			// 
			// addCut
			// 
			addCut.FormattingEnabled = true;
			addCut.Location = new System.Drawing.Point(121, 379);
			addCut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
			pointPanel.Size = new System.Drawing.Size(256, 240);
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
			minBox.Location = new System.Drawing.Point(214, 292);
			minBox.Name = "minBox";
			minBox.Size = new System.Drawing.Size(56, 23);
			minBox.TabIndex = 65;
			minBox.TextChanged += MinBox_TextChanged;
			// 
			// saveButton
			// 
			saveButton.Location = new System.Drawing.Point(80, 410);
			saveButton.Name = "saveButton";
			saveButton.Size = new System.Drawing.Size(56, 27);
			saveButton.TabIndex = 58;
			saveButton.Text = "SAVE";
			saveButton.UseVisualStyleBackColor = true;
			saveButton.Click += SaveButton_Click;
			// 
			// loadButton
			// 
			loadButton.Location = new System.Drawing.Point(14, 408);
			loadButton.Name = "loadButton";
			loadButton.Size = new System.Drawing.Size(56, 27);
			loadButton.TabIndex = 57;
			loadButton.Text = "LOAD";
			loadButton.UseVisualStyleBackColor = true;
			loadButton.Click += LoadButton_Click;
			// 
			// cutBox
			// 
			cutBox.Location = new System.Drawing.Point(148, 292);
			cutBox.Name = "cutBox";
			cutBox.Size = new System.Drawing.Size(56, 23);
			cutBox.TabIndex = 64;
			cutBox.TextChanged += CutBox_TextChanged;
			// 
			// addAngleButton
			// 
			addAngleButton.Location = new System.Drawing.Point(121, 321);
			addAngleButton.Name = "addAngleButton";
			addAngleButton.Size = new System.Drawing.Size(122, 23);
			addAngleButton.TabIndex = 60;
			addAngleButton.Text = "ADD ANGLES";
			addAngleButton.UseVisualStyleBackColor = true;
			addAngleButton.Click += AddAngleButton_Click;
			// 
			// sizeBox
			// 
			sizeBox.Location = new System.Drawing.Point(14, 292);
			sizeBox.Name = "sizeBox";
			sizeBox.Size = new System.Drawing.Size(56, 23);
			sizeBox.TabIndex = 57;
			sizeBox.TextChanged += SizeBox_TextChanged;
			// 
			// addcolorButton
			// 
			addcolorButton.Location = new System.Drawing.Point(121, 350);
			addcolorButton.Name = "addcolorButton";
			addcolorButton.Size = new System.Drawing.Size(122, 23);
			addcolorButton.TabIndex = 61;
			addcolorButton.Text = "ADD COLORS";
			addcolorButton.UseVisualStyleBackColor = true;
			addcolorButton.Click += AddcolorButton_Click;
			// 
			// removeangleButton
			// 
			removeangleButton.Location = new System.Drawing.Point(249, 321);
			removeangleButton.Name = "removeangleButton";
			removeangleButton.Size = new System.Drawing.Size(21, 23);
			removeangleButton.TabIndex = 62;
			removeangleButton.Text = "X";
			removeangleButton.UseVisualStyleBackColor = true;
			removeangleButton.Click += RemoveangleButton_Click;
			// 
			// removecolorButton
			// 
			removecolorButton.Location = new System.Drawing.Point(249, 350);
			removecolorButton.Name = "removecolorButton";
			removecolorButton.Size = new System.Drawing.Size(21, 23);
			removecolorButton.TabIndex = 63;
			removecolorButton.Text = "X";
			removecolorButton.UseVisualStyleBackColor = true;
			removecolorButton.Click += RemovecolorButton_Click;
			// 
			// mp4Button
			// 
			mp4Button.Location = new System.Drawing.Point(201, 403);
			mp4Button.Name = "mp4Button";
			mp4Button.Size = new System.Drawing.Size(69, 27);
			mp4Button.TabIndex = 57;
			mp4Button.Text = "Save Mp4";
			mp4Button.UseVisualStyleBackColor = true;
			mp4Button.Click += Mp4Button_Click;
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
			// GeneratorForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
			ClientSize = new System.Drawing.Size(1085, 1178);
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
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "GeneratorForm";
			Text = "RGB Fractal Zoom Generator C# v1.92";
			FormClosing += GeneratorForm_FormClosing;
			helpPanel.ResumeLayout(false);
			helpPanel.PerformLayout();
			generatorPanel.ResumeLayout(false);
			generatorPanel.PerformLayout();
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
		private System.Windows.Forms.Label delayLabel;
		private System.Windows.Forms.TextBox delayBox;
		private System.Windows.Forms.Button prevButton;
		private System.Windows.Forms.Button nextButton;
		private System.Windows.Forms.Button animateButton;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.Button pngButton;
		private System.Windows.Forms.Button gifButton;
		private System.Windows.Forms.Label dotLabel;
		private System.Windows.Forms.Label voidLabel;
		private System.Windows.Forms.Label blurLabel;
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
		private System.Windows.Forms.Label threadsLabel;
		private System.Windows.Forms.TextBox brightnessBox;
		private System.Windows.Forms.Label brightnessLabel;
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
		private System.Windows.Forms.Button addcolorButton;
		private System.Windows.Forms.Button addAngleButton;
		private System.Windows.Forms.Button removecolorButton;
		private System.Windows.Forms.Button removeangleButton;
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
	}
}

