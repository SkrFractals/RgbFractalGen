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
			toolTips = new System.Windows.Forms.ToolTip(components);
			resX = new System.Windows.Forms.TextBox();
			cutSelect = new System.Windows.Forms.ComboBox();
			angleSelect = new System.Windows.Forms.ComboBox();
			colorSelect = new System.Windows.Forms.ComboBox();
			fractalSelect = new System.Windows.Forms.ComboBox();
			resY = new System.Windows.Forms.TextBox();
			cutparamBox = new System.Windows.Forms.TextBox();
			previewBox = new System.Windows.Forms.CheckBox();
			periodBox = new System.Windows.Forms.TextBox();
			delayBox = new System.Windows.Forms.TextBox();
			zoomButton = new System.Windows.Forms.Button();
			prevButton = new System.Windows.Forms.Button();
			nextButton = new System.Windows.Forms.Button();
			animateButton = new System.Windows.Forms.Button();
			pngButton = new System.Windows.Forms.Button();
			gifButton = new System.Windows.Forms.Button();
			timer = new System.Windows.Forms.Timer(components);
			savePng = new System.Windows.Forms.SaveFileDialog();
			saveGif = new System.Windows.Forms.SaveFileDialog();
			fractalLabel = new System.Windows.Forms.Label();
			delayLabel = new System.Windows.Forms.Label();
			voidLabel = new System.Windows.Forms.Label();
			dotLabel = new System.Windows.Forms.Label();
			statusLabel = new System.Windows.Forms.Label();
			infoLabel = new System.Windows.Forms.Label();
			blurLabel = new System.Windows.Forms.Label();
			defaultZoom = new System.Windows.Forms.TextBox();
			defaultAngle = new System.Windows.Forms.TextBox();
			encodeButton = new System.Windows.Forms.Button();
			defaultHue = new System.Windows.Forms.TextBox();
			periodMultiplierBox = new System.Windows.Forms.TextBox();
			periodLabel = new System.Windows.Forms.Label();
			helpButton = new System.Windows.Forms.Button();
			cutLabel = new System.Windows.Forms.Label();
			angleLabel = new System.Windows.Forms.Label();
			colorLabel = new System.Windows.Forms.Label();
			helpPanel = new System.Windows.Forms.Panel();
			helpLabel = new System.Windows.Forms.Label();
			spinSelect = new System.Windows.Forms.ComboBox();
			zoomLabel = new System.Windows.Forms.Label();
			hueSelect = new System.Windows.Forms.ComboBox();
			spinLabel = new System.Windows.Forms.Label();
			hueLabel = new System.Windows.Forms.Label();
			spinSpeedBox = new System.Windows.Forms.TextBox();
			hueSpeedBox = new System.Windows.Forms.TextBox();
			parallelTypeBox = new System.Windows.Forms.ComboBox();
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
			label1 = new System.Windows.Forms.Label();
			helpPanel.SuspendLayout();
			SuspendLayout();
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
			// cutSelect
			// 
			cutSelect.FormattingEnabled = true;
			cutSelect.Location = new System.Drawing.Point(71, 101);
			cutSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cutSelect.Name = "cutSelect";
			cutSelect.Size = new System.Drawing.Size(177, 23);
			cutSelect.TabIndex = 4;
			cutSelect.Text = "Select CutFunction";
			cutSelect.SelectedIndexChanged += CutSelect_SelectedIndexChanged;
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
			// fractalSelect
			// 
			fractalSelect.FormattingEnabled = true;
			fractalSelect.Location = new System.Drawing.Point(71, 14);
			fractalSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			fractalSelect.Name = "fractalSelect";
			fractalSelect.Size = new System.Drawing.Size(231, 23);
			fractalSelect.TabIndex = 1;
			fractalSelect.Text = "Select Fractal";
			fractalSelect.SelectedIndexChanged += FractalSelect_SelectedIndexChanged;
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
			// cutparamBox
			// 
			cutparamBox.Location = new System.Drawing.Point(256, 101);
			cutparamBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cutparamBox.Name = "cutparamBox";
			cutparamBox.Size = new System.Drawing.Size(46, 23);
			cutparamBox.TabIndex = 5;
			cutparamBox.Text = "0";
			cutparamBox.TextChanged += CutparamBox_TextChanged;
			// 
			// previewBox
			// 
			previewBox.AutoSize = true;
			previewBox.Location = new System.Drawing.Point(125, 134);
			previewBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			previewBox.Name = "previewBox";
			previewBox.Size = new System.Drawing.Size(101, 19);
			previewBox.TabIndex = 9;
			previewBox.Text = "Preview Mode";
			previewBox.UseVisualStyleBackColor = true;
			previewBox.CheckedChanged += Resolution_Changed;
			// 
			// periodBox
			// 
			periodBox.Location = new System.Drawing.Point(17, 159);
			periodBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			periodBox.Name = "periodBox";
			periodBox.Size = new System.Drawing.Size(86, 23);
			periodBox.TabIndex = 10;
			periodBox.Text = "120";
			periodBox.TextChanged += PeriodBox_TextChanged;
			// 
			// delayBox
			// 
			delayBox.Location = new System.Drawing.Point(114, 463);
			delayBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			delayBox.Name = "delayBox";
			delayBox.Size = new System.Drawing.Size(66, 23);
			delayBox.TabIndex = 28;
			delayBox.Text = "5";
			delayBox.TextChanged += DelayBox_TextChanged;
			// 
			// zoomButton
			// 
			zoomButton.Location = new System.Drawing.Point(17, 188);
			zoomButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			zoomButton.Name = "zoomButton";
			zoomButton.Size = new System.Drawing.Size(86, 27);
			zoomButton.TabIndex = 12;
			zoomButton.Text = "Zoom ->";
			zoomButton.UseVisualStyleBackColor = true;
			zoomButton.Click += ZoomButton_Click;
			// 
			// prevButton
			// 
			prevButton.Location = new System.Drawing.Point(17, 492);
			prevButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			prevButton.Name = "prevButton";
			prevButton.Size = new System.Drawing.Size(66, 27);
			prevButton.TabIndex = 29;
			prevButton.Text = "<- Frame";
			prevButton.UseVisualStyleBackColor = true;
			prevButton.Click += PrevButton_Click;
			// 
			// nextButton
			// 
			nextButton.Location = new System.Drawing.Point(114, 492);
			nextButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			nextButton.Name = "nextButton";
			nextButton.Size = new System.Drawing.Size(66, 27);
			nextButton.TabIndex = 30;
			nextButton.Text = "Frame ->";
			nextButton.UseVisualStyleBackColor = true;
			nextButton.Click += NextButton_Click;
			// 
			// animateButton
			// 
			animateButton.Location = new System.Drawing.Point(191, 492);
			animateButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			animateButton.Name = "animateButton";
			animateButton.Size = new System.Drawing.Size(109, 27);
			animateButton.TabIndex = 31;
			animateButton.Text = "Playing";
			animateButton.UseVisualStyleBackColor = true;
			animateButton.Click += AnimateButton_Click;
			// 
			// pngButton
			// 
			pngButton.Location = new System.Drawing.Point(114, 558);
			pngButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			pngButton.Name = "pngButton";
			pngButton.Size = new System.Drawing.Size(66, 27);
			pngButton.TabIndex = 34;
			pngButton.Text = "Save PNG";
			pngButton.UseVisualStyleBackColor = true;
			pngButton.Click += png_Click;
			// 
			// gifButton
			// 
			gifButton.Location = new System.Drawing.Point(191, 558);
			gifButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			gifButton.Name = "gifButton";
			gifButton.Size = new System.Drawing.Size(109, 27);
			gifButton.TabIndex = 35;
			gifButton.Text = "Save GIF";
			gifButton.UseVisualStyleBackColor = true;
			gifButton.Click += Gif_Click;
			// 
			// timer
			// 
			timer.Enabled = true;
			timer.Interval = 20;
			timer.Tick += Timer_Tick;
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
			saveGif.FileOk += SaveGif_FileOk;
			// 
			// fractalLabel
			// 
			fractalLabel.AutoSize = true;
			fractalLabel.Location = new System.Drawing.Point(17, 17);
			fractalLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			fractalLabel.Name = "fractalLabel";
			fractalLabel.Size = new System.Drawing.Size(45, 15);
			fractalLabel.TabIndex = 0;
			fractalLabel.Text = "Fractal:";
			// 
			// delayLabel
			// 
			delayLabel.AutoSize = true;
			delayLabel.Location = new System.Drawing.Point(191, 466);
			delayLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			delayLabel.Name = "delayLabel";
			delayLabel.Size = new System.Drawing.Size(67, 15);
			delayLabel.TabIndex = 0;
			delayLabel.Text = "Abort/ FPS:";
			// 
			// voidLabel
			// 
			voidLabel.AutoSize = true;
			voidLabel.Location = new System.Drawing.Point(17, 322);
			voidLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			voidLabel.Name = "voidLabel";
			voidLabel.Size = new System.Drawing.Size(151, 15);
			voidLabel.TabIndex = 0;
			voidLabel.Text = "Void Ambient/Noise (0-30):";
			// 
			// dotLabel
			// 
			dotLabel.AutoSize = true;
			dotLabel.Location = new System.Drawing.Point(17, 351);
			dotLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			dotLabel.Name = "dotLabel";
			dotLabel.Size = new System.Drawing.Size(122, 15);
			dotLabel.TabIndex = 0;
			dotLabel.Text = "Saturate/Detail (0-10):";
			// 
			// statusLabel
			// 
			statusLabel.AutoSize = true;
			statusLabel.Location = new System.Drawing.Point(17, 531);
			statusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(64, 15);
			statusLabel.TabIndex = 0;
			statusLabel.Text = "Initializing:";
			// 
			// infoLabel
			// 
			infoLabel.AutoSize = true;
			infoLabel.Location = new System.Drawing.Point(114, 531);
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
			blurLabel.Location = new System.Drawing.Point(17, 380);
			blurLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			blurLabel.Name = "blurLabel";
			blurLabel.Size = new System.Drawing.Size(147, 15);
			blurLabel.TabIndex = 0;
			blurLabel.Text = "Bloom/Motion Blur (0-40):";
			// 
			// defaultZoom
			// 
			defaultZoom.Location = new System.Drawing.Point(111, 191);
			defaultZoom.Name = "defaultZoom";
			defaultZoom.Size = new System.Drawing.Size(60, 23);
			defaultZoom.TabIndex = 13;
			defaultZoom.Text = "0";
			defaultZoom.TextChanged += DefaultZoom_TextChanged;
			// 
			// defaultAngle
			// 
			defaultAngle.Location = new System.Drawing.Point(177, 241);
			defaultAngle.Name = "defaultAngle";
			defaultAngle.Size = new System.Drawing.Size(123, 23);
			defaultAngle.TabIndex = 16;
			defaultAngle.Text = "0";
			defaultAngle.TextChanged += DefaultAngle_TextChanged;
			// 
			// encodeButton
			// 
			encodeButton.Location = new System.Drawing.Point(191, 525);
			encodeButton.Name = "encodeButton";
			encodeButton.Size = new System.Drawing.Size(109, 27);
			encodeButton.TabIndex = 32;
			encodeButton.Text = "Encode GIF";
			encodeButton.UseVisualStyleBackColor = true;
			encodeButton.Click += encodeButton_Click;
			// 
			// defaultHue
			// 
			defaultHue.Location = new System.Drawing.Point(177, 290);
			defaultHue.Name = "defaultHue";
			defaultHue.Size = new System.Drawing.Size(123, 23);
			defaultHue.TabIndex = 19;
			defaultHue.Text = "0";
			defaultHue.TextChanged += DefaultHue_TextChanged;
			// 
			// periodMultiplierBox
			// 
			periodMultiplierBox.Location = new System.Drawing.Point(111, 159);
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
			periodLabel.Location = new System.Drawing.Point(181, 162);
			periodLabel.Name = "periodLabel";
			periodLabel.Size = new System.Drawing.Size(119, 15);
			periodLabel.TabIndex = 31;
			periodLabel.Text = "<-Period + Multiplier";
			// 
			// helpButton
			// 
			helpButton.Location = new System.Drawing.Point(17, 558);
			helpButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			helpButton.Name = "helpButton";
			helpButton.Size = new System.Drawing.Size(66, 27);
			helpButton.TabIndex = 33;
			helpButton.Text = "Help";
			helpButton.UseVisualStyleBackColor = true;
			helpButton.Click += HelpButton_Click;
			// 
			// cutLabel
			// 
			cutLabel.AutoSize = true;
			cutLabel.Location = new System.Drawing.Point(17, 104);
			cutLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			cutLabel.Name = "cutLabel";
			cutLabel.Size = new System.Drawing.Size(29, 15);
			cutLabel.TabIndex = 34;
			cutLabel.Text = "Cut:";
			// 
			// angleLabel
			// 
			angleLabel.AutoSize = true;
			angleLabel.Location = new System.Drawing.Point(17, 46);
			angleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			angleLabel.Name = "angleLabel";
			angleLabel.Size = new System.Drawing.Size(46, 15);
			angleLabel.TabIndex = 36;
			angleLabel.Text = "Angles:";
			// 
			// colorLabel
			// 
			colorLabel.AutoSize = true;
			colorLabel.Location = new System.Drawing.Point(17, 75);
			colorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			colorLabel.Name = "colorLabel";
			colorLabel.Size = new System.Drawing.Size(44, 15);
			colorLabel.TabIndex = 38;
			colorLabel.Text = "Colors:";
			// 
			// helpPanel
			// 
			helpPanel.AutoScroll = true;
			helpPanel.BackColor = System.Drawing.Color.White;
			helpPanel.Controls.Add(helpLabel);
			helpPanel.Location = new System.Drawing.Point(309, 14);
			helpPanel.Name = "helpPanel";
			helpPanel.Size = new System.Drawing.Size(763, 582);
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
			spinSelect.Items.AddRange(new object[] { "->|<-", "->", "X", "<-", "<-|->" });
			spinSelect.Location = new System.Drawing.Point(17, 241);
			spinSelect.Name = "spinSelect";
			spinSelect.Size = new System.Drawing.Size(86, 23);
			spinSelect.TabIndex = 14;
			spinSelect.Text = "Select Spin";
			spinSelect.SelectedIndexChanged += SpinSelect_SelectedIndexChanged;
			// 
			// zoomLabel
			// 
			zoomLabel.AutoSize = true;
			zoomLabel.Location = new System.Drawing.Point(181, 194);
			zoomLabel.Name = "zoomLabel";
			zoomLabel.Size = new System.Drawing.Size(122, 15);
			zoomLabel.TabIndex = 40;
			zoomLabel.Text = "<-Zoom + First frame";
			// 
			// hueSelect
			// 
			hueSelect.FormattingEnabled = true;
			hueSelect.Items.AddRange(new object[] { "RGB (static)", "BGR (static)", "RGB->GBR", "BGR->RBG", "RGB->BRG", "BGR->GRB" });
			hueSelect.Location = new System.Drawing.Point(17, 290);
			hueSelect.Name = "hueSelect";
			hueSelect.Size = new System.Drawing.Size(86, 23);
			hueSelect.TabIndex = 17;
			hueSelect.Text = "Select  Hue";
			hueSelect.SelectedIndexChanged += HueSelect_SelectedIndexChanged;
			// 
			// spinLabel
			// 
			spinLabel.AutoSize = true;
			spinLabel.Location = new System.Drawing.Point(17, 223);
			spinLabel.Name = "spinLabel";
			spinLabel.Size = new System.Drawing.Size(261, 15);
			spinLabel.TabIndex = 41;
			spinLabel.Text = "Spin direction      Spin Speed           Default Angle";
			// 
			// hueLabel
			// 
			hueLabel.AutoSize = true;
			hueLabel.Location = new System.Drawing.Point(17, 272);
			hueLabel.Name = "hueLabel";
			hueLabel.Size = new System.Drawing.Size(252, 15);
			hueLabel.TabIndex = 42;
			hueLabel.Text = "Hue Select             HueSpeed           Default Hue";
			// 
			// spinSpeedBox
			// 
			spinSpeedBox.Location = new System.Drawing.Point(109, 241);
			spinSpeedBox.Name = "spinSpeedBox";
			spinSpeedBox.Size = new System.Drawing.Size(62, 23);
			spinSpeedBox.TabIndex = 43;
			spinSpeedBox.Text = "0";
			spinSpeedBox.TextChanged += SpinSpeedBox_TextChanged;
			// 
			// hueSpeedBox
			// 
			hueSpeedBox.Location = new System.Drawing.Point(109, 290);
			hueSpeedBox.Name = "hueSpeedBox";
			hueSpeedBox.Size = new System.Drawing.Size(62, 23);
			hueSpeedBox.TabIndex = 44;
			hueSpeedBox.Text = "0";
			hueSpeedBox.TextChanged += HueSpeedBox_TextChanged;
			// 
			// parallelTypeBox
			// 
			parallelTypeBox.FormattingEnabled = true;
			parallelTypeBox.Items.AddRange(new object[] { "Of Animation", "Of Depth", "Of Recursion" });
			parallelTypeBox.Location = new System.Drawing.Point(111, 435);
			parallelTypeBox.Name = "parallelTypeBox";
			parallelTypeBox.Size = new System.Drawing.Size(123, 23);
			parallelTypeBox.TabIndex = 27;
			parallelTypeBox.Text = "Parallelism Type";
			parallelTypeBox.SelectedIndexChanged += ParallelTypeBox_SelectedIndexChanged;
			// 
			// ambBox
			// 
			ambBox.Location = new System.Drawing.Point(177, 319);
			ambBox.Name = "ambBox";
			ambBox.Size = new System.Drawing.Size(60, 23);
			ambBox.TabIndex = 20;
			ambBox.Text = "20";
			ambBox.TextChanged += AmbBox_TextChanged;
			// 
			// noiseBox
			// 
			noiseBox.Location = new System.Drawing.Point(240, 319);
			noiseBox.Name = "noiseBox";
			noiseBox.Size = new System.Drawing.Size(60, 23);
			noiseBox.TabIndex = 21;
			noiseBox.Text = "20";
			noiseBox.TextChanged += NoiseBox_TextChanged;
			// 
			// saturateBox
			// 
			saturateBox.Location = new System.Drawing.Point(177, 348);
			saturateBox.Name = "saturateBox";
			saturateBox.Size = new System.Drawing.Size(60, 23);
			saturateBox.TabIndex = 22;
			saturateBox.Text = "5";
			saturateBox.TextChanged += SaturateBox_TextChanged;
			// 
			// detailBox
			// 
			detailBox.Location = new System.Drawing.Point(240, 348);
			detailBox.Name = "detailBox";
			detailBox.Size = new System.Drawing.Size(60, 23);
			detailBox.TabIndex = 23;
			detailBox.Text = "5";
			detailBox.TextChanged += DetailBox_TextChanged;
			// 
			// blurBox
			// 
			blurBox.Location = new System.Drawing.Point(240, 377);
			blurBox.Name = "blurBox";
			blurBox.Size = new System.Drawing.Size(60, 23);
			blurBox.TabIndex = 24;
			blurBox.Text = "0";
			blurBox.TextChanged += BlurBox_TextChanged;
			// 
			// bloomBox
			// 
			bloomBox.Location = new System.Drawing.Point(177, 377);
			bloomBox.Name = "bloomBox";
			bloomBox.Size = new System.Drawing.Size(60, 23);
			bloomBox.TabIndex = 25;
			bloomBox.Text = "0";
			bloomBox.TextChanged += BloomBox_TextChanged;
			// 
			// threadsBox
			// 
			threadsBox.Location = new System.Drawing.Point(240, 435);
			threadsBox.Name = "threadsBox";
			threadsBox.Size = new System.Drawing.Size(60, 23);
			threadsBox.TabIndex = 28;
			threadsBox.Text = "0";
			threadsBox.TextChanged += Parallel_Changed;
			// 
			// abortBox
			// 
			abortBox.Location = new System.Drawing.Point(17, 463);
			abortBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			abortBox.Name = "abortBox";
			abortBox.Size = new System.Drawing.Size(66, 23);
			abortBox.TabIndex = 45;
			abortBox.Text = "500";
			abortBox.TextChanged += abortBox_TextChanged;
			// 
			// threadsLabel
			// 
			threadsLabel.AutoSize = true;
			threadsLabel.Location = new System.Drawing.Point(17, 438);
			threadsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			threadsLabel.Name = "threadsLabel";
			threadsLabel.Size = new System.Drawing.Size(77, 15);
			threadsLabel.TabIndex = 0;
			threadsLabel.Text = "Max Threads:";
			// 
			// brightnessBox
			// 
			brightnessBox.Location = new System.Drawing.Point(177, 406);
			brightnessBox.Name = "brightnessBox";
			brightnessBox.Size = new System.Drawing.Size(60, 23);
			brightnessBox.TabIndex = 46;
			brightnessBox.Text = "100";
			brightnessBox.TextChanged += BrightnessBox_TextChanged;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(17, 409);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(115, 15);
			label1.TabIndex = 47;
			label1.Text = "Brightness (0-300%):";
			// 
			// GeneratorForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(1086, 610);
			Controls.Add(label1);
			Controls.Add(brightnessBox);
			Controls.Add(abortBox);
			Controls.Add(threadsBox);
			Controls.Add(bloomBox);
			Controls.Add(blurBox);
			Controls.Add(detailBox);
			Controls.Add(saturateBox);
			Controls.Add(noiseBox);
			Controls.Add(ambBox);
			Controls.Add(parallelTypeBox);
			Controls.Add(hueSpeedBox);
			Controls.Add(spinSpeedBox);
			Controls.Add(hueLabel);
			Controls.Add(spinLabel);
			Controls.Add(hueSelect);
			Controls.Add(zoomLabel);
			Controls.Add(spinSelect);
			Controls.Add(helpPanel);
			Controls.Add(colorLabel);
			Controls.Add(colorSelect);
			Controls.Add(angleLabel);
			Controls.Add(angleSelect);
			Controls.Add(cutLabel);
			Controls.Add(cutSelect);
			Controls.Add(helpButton);
			Controls.Add(periodLabel);
			Controls.Add(periodMultiplierBox);
			Controls.Add(defaultHue);
			Controls.Add(cutparamBox);
			Controls.Add(encodeButton);
			Controls.Add(defaultAngle);
			Controls.Add(defaultZoom);
			Controls.Add(blurLabel);
			Controls.Add(fractalLabel);
			Controls.Add(fractalSelect);
			Controls.Add(resX);
			Controls.Add(resY);
			Controls.Add(previewBox);
			Controls.Add(periodBox);
			Controls.Add(delayLabel);
			Controls.Add(delayBox);
			Controls.Add(zoomButton);
			Controls.Add(prevButton);
			Controls.Add(nextButton);
			Controls.Add(animateButton);
			Controls.Add(voidLabel);
			Controls.Add(dotLabel);
			Controls.Add(threadsLabel);
			Controls.Add(statusLabel);
			Controls.Add(infoLabel);
			Controls.Add(pngButton);
			Controls.Add(gifButton);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "GeneratorForm";
			Text = "RGB Fractal Zoom Generator C# v1.82";
			FormClosing += GeneratorForm_FormClosing;
			helpPanel.ResumeLayout(false);
			helpPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
		#endregion
		private System.Windows.Forms.ToolTip toolTips;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.SaveFileDialog savePng;
		private System.Windows.Forms.SaveFileDialog saveGif;
		private System.Windows.Forms.Label fractalLabel;
		private System.Windows.Forms.ComboBox fractalSelect;
		private System.Windows.Forms.TextBox resX;
		private System.Windows.Forms.TextBox resY;
		private System.Windows.Forms.CheckBox previewBox;
		private System.Windows.Forms.TextBox periodBox;
		private System.Windows.Forms.Label delayLabel;
		private System.Windows.Forms.TextBox delayBox;
		private System.Windows.Forms.Button zoomButton;
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
		private System.Windows.Forms.Button encodeButton;
		private System.Windows.Forms.TextBox cutparamBox;
		private System.Windows.Forms.TextBox defaultHue;
		private System.Windows.Forms.TextBox periodMultiplierBox;
		private System.Windows.Forms.Label periodLabel;
		private System.Windows.Forms.Button helpButton;
		private System.Windows.Forms.ComboBox cutSelect;
		private System.Windows.Forms.Label cutLabel;
		private System.Windows.Forms.ComboBox angleSelect;
		private System.Windows.Forms.Label angleLabel;
		private System.Windows.Forms.ComboBox colorSelect;
		private System.Windows.Forms.Label colorLabel;
		private System.Windows.Forms.Panel helpPanel;
		private System.Windows.Forms.Label helpLabel;
		private System.Windows.Forms.ComboBox spinSelect;
		private System.Windows.Forms.Label zoomLabel;
		private System.Windows.Forms.ComboBox hueSelect;
		private System.Windows.Forms.Label spinLabel;
		private System.Windows.Forms.Label hueLabel;
		private System.Windows.Forms.TextBox spinSpeedBox;
		private System.Windows.Forms.TextBox hueSpeedBox;
		private System.Windows.Forms.ComboBox parallelTypeBox;
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
		private System.Windows.Forms.Label label1;
	}
}

