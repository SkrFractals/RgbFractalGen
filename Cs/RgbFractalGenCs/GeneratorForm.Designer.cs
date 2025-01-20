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
			ambBar = new System.Windows.Forms.TrackBar();
			noiseBar = new System.Windows.Forms.TrackBar();
			detailBar = new System.Windows.Forms.TrackBar();
			saturateBar = new System.Windows.Forms.TrackBar();
			threadsBar = new System.Windows.Forms.TrackBar();
			parallelBox = new System.Windows.Forms.CheckBox();
			parallelTypeBox = new System.Windows.Forms.CheckBox();
			pngButton = new System.Windows.Forms.Button();
			gifButton = new System.Windows.Forms.Button();
			blurBar = new System.Windows.Forms.TrackBar();
			timer = new System.Windows.Forms.Timer(components);
			savePng = new System.Windows.Forms.SaveFileDialog();
			saveGif = new System.Windows.Forms.SaveFileDialog();
			fractalLabel = new System.Windows.Forms.Label();
			delayLabel = new System.Windows.Forms.Label();
			ambLabel = new System.Windows.Forms.Label();
			noiseLabel = new System.Windows.Forms.Label();
			detailLabel = new System.Windows.Forms.Label();
			saturateLabel = new System.Windows.Forms.Label();
			threadsLabel = new System.Windows.Forms.Label();
			statusLabel = new System.Windows.Forms.Label();
			infoLabel = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			defaultZoom = new System.Windows.Forms.TextBox();
			defaultAngle = new System.Windows.Forms.TextBox();
			encodeButton = new System.Windows.Forms.Button();
			cutparamBar = new System.Windows.Forms.TrackBar();
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
			((System.ComponentModel.ISupportInitialize)ambBar).BeginInit();
			((System.ComponentModel.ISupportInitialize)noiseBar).BeginInit();
			((System.ComponentModel.ISupportInitialize)detailBar).BeginInit();
			((System.ComponentModel.ISupportInitialize)saturateBar).BeginInit();
			((System.ComponentModel.ISupportInitialize)threadsBar).BeginInit();
			((System.ComponentModel.ISupportInitialize)blurBar).BeginInit();
			((System.ComponentModel.ISupportInitialize)cutparamBar).BeginInit();
			helpPanel.SuspendLayout();
			SuspendLayout();
			// 
			// resX
			// 
			resX.Location = new System.Drawing.Point(19, 190);
			resX.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			resX.Name = "resX";
			resX.Size = new System.Drawing.Size(46, 23);
			resX.TabIndex = 7;
			resX.Text = "1920";
			resX.TextChanged += resX_TextChanged;
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
			cutSelect.SelectedIndexChanged += cutSelect_SelectedIndexChanged;
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
			angleSelect.SelectedIndexChanged += angleSelect_SelectedIndexChanged;
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
			colorSelect.SelectedIndexChanged += colorSelect_SelectedIndexChanged;
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
			fractalSelect.SelectedIndexChanged += fractalSelect_SelectedIndexChanged;
			// 
			// resY
			// 
			resY.Location = new System.Drawing.Point(73, 190);
			resY.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			resY.Name = "resY";
			resY.Size = new System.Drawing.Size(46, 23);
			resY.TabIndex = 8;
			resY.Text = "1080";
			resY.TextChanged += resY_TextChanged;
			// 
			// cutparamBox
			// 
			cutparamBox.Location = new System.Drawing.Point(256, 101);
			cutparamBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cutparamBox.Name = "cutparamBox";
			cutparamBox.Size = new System.Drawing.Size(46, 23);
			cutparamBox.TabIndex = 5;
			cutparamBox.Text = "0";
			cutparamBox.TextChanged += cutparamBox_TextChanged;
			// 
			// previewBox
			// 
			previewBox.AutoSize = true;
			previewBox.Location = new System.Drawing.Point(127, 194);
			previewBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			previewBox.Name = "previewBox";
			previewBox.Size = new System.Drawing.Size(101, 19);
			previewBox.TabIndex = 9;
			previewBox.Text = "Preview Mode";
			previewBox.UseVisualStyleBackColor = true;
			previewBox.CheckedChanged += previewBox_CheckedChanged;
			// 
			// periodBox
			// 
			periodBox.Location = new System.Drawing.Point(19, 219);
			periodBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			periodBox.Name = "periodBox";
			periodBox.Size = new System.Drawing.Size(86, 23);
			periodBox.TabIndex = 10;
			periodBox.Text = "120";
			periodBox.TextChanged += periodBox_TextChanged;
			// 
			// delayBox
			// 
			delayBox.Location = new System.Drawing.Point(13, 727);
			delayBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			delayBox.Name = "delayBox";
			delayBox.Size = new System.Drawing.Size(67, 23);
			delayBox.TabIndex = 28;
			delayBox.Text = "5";
			delayBox.TextChanged += delayBox_TextChanged;
			// 
			// zoomButton
			// 
			zoomButton.Location = new System.Drawing.Point(19, 248);
			zoomButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			zoomButton.Name = "zoomButton";
			zoomButton.Size = new System.Drawing.Size(86, 27);
			zoomButton.TabIndex = 12;
			zoomButton.Text = "Zoom ->";
			zoomButton.UseVisualStyleBackColor = true;
			zoomButton.Click += zoomButton_Click;
			// 
			// prevButton
			// 
			prevButton.Location = new System.Drawing.Point(13, 756);
			prevButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			prevButton.Name = "prevButton";
			prevButton.Size = new System.Drawing.Size(66, 27);
			prevButton.TabIndex = 29;
			prevButton.Text = "<- Frame";
			prevButton.UseVisualStyleBackColor = true;
			prevButton.Click += prevButton_Click;
			// 
			// nextButton
			// 
			nextButton.Location = new System.Drawing.Point(116, 756);
			nextButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			nextButton.Name = "nextButton";
			nextButton.Size = new System.Drawing.Size(66, 27);
			nextButton.TabIndex = 30;
			nextButton.Text = "Frame ->";
			nextButton.UseVisualStyleBackColor = true;
			nextButton.Click += nextButton_Click;
			// 
			// animateButton
			// 
			animateButton.Location = new System.Drawing.Point(193, 756);
			animateButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			animateButton.Name = "animateButton";
			animateButton.Size = new System.Drawing.Size(109, 27);
			animateButton.TabIndex = 31;
			animateButton.Text = "Playing";
			animateButton.UseVisualStyleBackColor = true;
			animateButton.Click += animateButton_Click;
			// 
			// ambBar
			// 
			ambBar.LargeChange = 2;
			ambBar.Location = new System.Drawing.Point(131, 382);
			ambBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			ambBar.Maximum = 30;
			ambBar.Name = "ambBar";
			ambBar.Size = new System.Drawing.Size(171, 45);
			ambBar.TabIndex = 20;
			ambBar.Value = 20;
			ambBar.Scroll += ambBar_Scroll;
			// 
			// noiseBar
			// 
			noiseBar.LargeChange = 10;
			noiseBar.Location = new System.Drawing.Point(131, 441);
			noiseBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			noiseBar.Maximum = 30;
			noiseBar.Name = "noiseBar";
			noiseBar.Size = new System.Drawing.Size(171, 45);
			noiseBar.TabIndex = 21;
			noiseBar.Value = 20;
			noiseBar.Scroll += noiseBar_Scroll;
			// 
			// detailBar
			// 
			detailBar.LargeChange = 2;
			detailBar.Location = new System.Drawing.Point(131, 559);
			detailBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			detailBar.Maximum = 20;
			detailBar.Minimum = 1;
			detailBar.Name = "detailBar";
			detailBar.Size = new System.Drawing.Size(171, 45);
			detailBar.TabIndex = 23;
			detailBar.Value = 10;
			detailBar.Scroll += detailBar_Scroll;
			// 
			// saturateBar
			// 
			saturateBar.LargeChange = 2;
			saturateBar.Location = new System.Drawing.Point(131, 500);
			saturateBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			saturateBar.Name = "saturateBar";
			saturateBar.Size = new System.Drawing.Size(171, 45);
			saturateBar.TabIndex = 22;
			saturateBar.Value = 5;
			saturateBar.Scroll += saturateBar_Scroll;
			// 
			// threadsBar
			// 
			threadsBar.LargeChange = 10;
			threadsBar.Location = new System.Drawing.Point(127, 686);
			threadsBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			threadsBar.Maximum = 128;
			threadsBar.Name = "threadsBar";
			threadsBar.Size = new System.Drawing.Size(175, 45);
			threadsBar.TabIndex = 27;
			threadsBar.Scroll += threadsBar_Scroll;
			// 
			// parallelBox
			// 
			parallelBox.AutoSize = true;
			parallelBox.Checked = true;
			parallelBox.CheckState = System.Windows.Forms.CheckState.Checked;
			parallelBox.Location = new System.Drawing.Point(13, 661);
			parallelBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			parallelBox.Name = "parallelBox";
			parallelBox.Size = new System.Drawing.Size(134, 19);
			parallelBox.TabIndex = 25;
			parallelBox.Text = "Parallel Generation...";
			parallelBox.UseVisualStyleBackColor = true;
			parallelBox.CheckedChanged += parallelBox_CheckedChanged;
			// 
			// parallelTypeBox
			// 
			parallelTypeBox.AutoSize = true;
			parallelTypeBox.Checked = true;
			parallelTypeBox.CheckState = System.Windows.Forms.CheckState.Checked;
			parallelTypeBox.Location = new System.Drawing.Point(155, 661);
			parallelTypeBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			parallelTypeBox.Name = "parallelTypeBox";
			parallelTypeBox.Size = new System.Drawing.Size(89, 19);
			parallelTypeBox.TabIndex = 26;
			parallelTypeBox.Text = "...Of Images";
			parallelTypeBox.UseVisualStyleBackColor = true;
			parallelTypeBox.CheckedChanged += parallelTypeBox_CheckedChanged;
			// 
			// pngButton
			// 
			pngButton.Location = new System.Drawing.Point(116, 822);
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
			gifButton.Location = new System.Drawing.Point(193, 822);
			gifButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			gifButton.Name = "gifButton";
			gifButton.Size = new System.Drawing.Size(109, 27);
			gifButton.TabIndex = 35;
			gifButton.Text = "Save GIF";
			gifButton.UseVisualStyleBackColor = true;
			gifButton.Click += gif_Click;
			// 
			// blurBar
			// 
			blurBar.LargeChange = 2;
			blurBar.Location = new System.Drawing.Point(131, 610);
			blurBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			blurBar.Minimum = 1;
			blurBar.Name = "blurBar";
			blurBar.Size = new System.Drawing.Size(171, 45);
			blurBar.TabIndex = 24;
			blurBar.Value = 1;
			blurBar.Scroll += blurBar_Scroll;
			// 
			// timer
			// 
			timer.Enabled = true;
			timer.Interval = 20;
			timer.Tick += timer_Tick;
			// 
			// savePng
			// 
			savePng.DefaultExt = "png";
			savePng.Filter = "PNG files (*.png)|*.png";
			savePng.FileOk += savePng_FileOk;
			// 
			// saveGif
			// 
			saveGif.DefaultExt = "gif";
			saveGif.Filter = "GIF files (*.gif)|*.gif";
			saveGif.FileOk += saveGif_FileOk;
			// 
			// fractalLabel
			// 
			fractalLabel.AutoSize = true;
			fractalLabel.Location = new System.Drawing.Point(19, 17);
			fractalLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			fractalLabel.Name = "fractalLabel";
			fractalLabel.Size = new System.Drawing.Size(45, 15);
			fractalLabel.TabIndex = 0;
			fractalLabel.Text = "Fractal:";
			// 
			// delayLabel
			// 
			delayLabel.AutoSize = true;
			delayLabel.Location = new System.Drawing.Point(88, 730);
			delayLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			delayLabel.Name = "delayLabel";
			delayLabel.Size = new System.Drawing.Size(39, 15);
			delayLabel.TabIndex = 0;
			delayLabel.Text = "Delay:";
			// 
			// ambLabel
			// 
			ambLabel.AutoSize = true;
			ambLabel.Location = new System.Drawing.Point(19, 382);
			ambLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			ambLabel.Name = "ambLabel";
			ambLabel.Size = new System.Drawing.Size(82, 15);
			ambLabel.TabIndex = 0;
			ambLabel.Text = "Void Ambient:";
			// 
			// noiseLabel
			// 
			noiseLabel.AutoSize = true;
			noiseLabel.Location = new System.Drawing.Point(19, 441);
			noiseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			noiseLabel.Name = "noiseLabel";
			noiseLabel.Size = new System.Drawing.Size(66, 15);
			noiseLabel.TabIndex = 0;
			noiseLabel.Text = "Void Noise:";
			// 
			// detailLabel
			// 
			detailLabel.AutoSize = true;
			detailLabel.Location = new System.Drawing.Point(19, 559);
			detailLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			detailLabel.Name = "detailLabel";
			detailLabel.Size = new System.Drawing.Size(40, 15);
			detailLabel.TabIndex = 23;
			detailLabel.Text = "Detail:";
			// 
			// saturateLabel
			// 
			saturateLabel.AutoSize = true;
			saturateLabel.Location = new System.Drawing.Point(19, 500);
			saturateLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			saturateLabel.Name = "saturateLabel";
			saturateLabel.Size = new System.Drawing.Size(86, 15);
			saturateLabel.TabIndex = 0;
			saturateLabel.Text = "Super Saturate:";
			// 
			// threadsLabel
			// 
			threadsLabel.AutoSize = true;
			threadsLabel.Location = new System.Drawing.Point(15, 686);
			threadsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			threadsLabel.Name = "threadsLabel";
			threadsLabel.Size = new System.Drawing.Size(77, 15);
			threadsLabel.TabIndex = 0;
			threadsLabel.Text = "Max Threads:";
			// 
			// statusLabel
			// 
			statusLabel.AutoSize = true;
			statusLabel.Location = new System.Drawing.Point(13, 786);
			statusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(64, 15);
			statusLabel.TabIndex = 0;
			statusLabel.Text = "Initializing:";
			// 
			// infoLabel
			// 
			infoLabel.AutoSize = true;
			infoLabel.Location = new System.Drawing.Point(116, 786);
			infoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			infoLabel.Name = "infoLabel";
			infoLabel.Size = new System.Drawing.Size(28, 15);
			infoLabel.TabIndex = 0;
			infoLabel.Text = "Info";
			infoLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(19, 610);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(73, 15);
			label1.TabIndex = 0;
			label1.Text = "Motion Blur:";
			// 
			// defaultZoom
			// 
			defaultZoom.Location = new System.Drawing.Point(113, 251);
			defaultZoom.Name = "defaultZoom";
			defaultZoom.Size = new System.Drawing.Size(60, 23);
			defaultZoom.TabIndex = 13;
			defaultZoom.Text = "0";
			defaultZoom.TextChanged += defaultZoom_TextChanged;
			// 
			// defaultAngle
			// 
			defaultAngle.Location = new System.Drawing.Point(179, 301);
			defaultAngle.Name = "defaultAngle";
			defaultAngle.Size = new System.Drawing.Size(123, 23);
			defaultAngle.TabIndex = 16;
			defaultAngle.Text = "0";
			defaultAngle.TextChanged += defaultAngle_TextChanged;
			// 
			// encodeButton
			// 
			encodeButton.Location = new System.Drawing.Point(193, 789);
			encodeButton.Name = "encodeButton";
			encodeButton.Size = new System.Drawing.Size(109, 27);
			encodeButton.TabIndex = 32;
			encodeButton.Text = "Encode GIF";
			encodeButton.UseVisualStyleBackColor = true;
			encodeButton.Click += encodeButton_Click;
			// 
			// cutparamBar
			// 
			cutparamBar.LargeChange = 1;
			cutparamBar.Location = new System.Drawing.Point(15, 130);
			cutparamBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cutparamBar.Maximum = 0;
			cutparamBar.Name = "cutparamBar";
			cutparamBar.Size = new System.Drawing.Size(287, 45);
			cutparamBar.TabIndex = 6;
			cutparamBar.Scroll += cutparamBar_Scroll;
			// 
			// defaultHue
			// 
			defaultHue.Location = new System.Drawing.Point(179, 350);
			defaultHue.Name = "defaultHue";
			defaultHue.Size = new System.Drawing.Size(123, 23);
			defaultHue.TabIndex = 19;
			defaultHue.Text = "0";
			defaultHue.TextChanged += defaultHue_TextChanged;
			// 
			// periodMultiplierBox
			// 
			periodMultiplierBox.Location = new System.Drawing.Point(113, 219);
			periodMultiplierBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			periodMultiplierBox.Name = "periodMultiplierBox";
			periodMultiplierBox.Size = new System.Drawing.Size(60, 23);
			periodMultiplierBox.TabIndex = 11;
			periodMultiplierBox.Text = "1";
			periodMultiplierBox.TextChanged += periodMultiplierBox_TextChanged;
			// 
			// periodLabel
			// 
			periodLabel.AutoSize = true;
			periodLabel.Location = new System.Drawing.Point(183, 222);
			periodLabel.Name = "periodLabel";
			periodLabel.Size = new System.Drawing.Size(119, 15);
			periodLabel.TabIndex = 31;
			periodLabel.Text = "<-Period + Multiplier";
			// 
			// helpButton
			// 
			helpButton.Location = new System.Drawing.Point(13, 822);
			helpButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			helpButton.Name = "helpButton";
			helpButton.Size = new System.Drawing.Size(66, 27);
			helpButton.TabIndex = 33;
			helpButton.Text = "Help";
			helpButton.UseVisualStyleBackColor = true;
			helpButton.Click += helpButton_Click;
			// 
			// cutLabel
			// 
			cutLabel.AutoSize = true;
			cutLabel.Location = new System.Drawing.Point(19, 104);
			cutLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			cutLabel.Name = "cutLabel";
			cutLabel.Size = new System.Drawing.Size(29, 15);
			cutLabel.TabIndex = 34;
			cutLabel.Text = "Cut:";
			// 
			// angleLabel
			// 
			angleLabel.AutoSize = true;
			angleLabel.Location = new System.Drawing.Point(19, 46);
			angleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			angleLabel.Name = "angleLabel";
			angleLabel.Size = new System.Drawing.Size(46, 15);
			angleLabel.TabIndex = 36;
			angleLabel.Text = "Angles:";
			// 
			// colorLabel
			// 
			colorLabel.AutoSize = true;
			colorLabel.Location = new System.Drawing.Point(19, 75);
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
			helpPanel.Size = new System.Drawing.Size(763, 835);
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
			spinSelect.Location = new System.Drawing.Point(19, 301);
			spinSelect.Name = "spinSelect";
			spinSelect.Size = new System.Drawing.Size(86, 23);
			spinSelect.TabIndex = 14;
			spinSelect.Text = "Select Spin";
			spinSelect.SelectedIndexChanged += spinSelect_SelectedIndexChanged;
			// 
			// zoomLabel
			// 
			zoomLabel.AutoSize = true;
			zoomLabel.Location = new System.Drawing.Point(183, 254);
			zoomLabel.Name = "zoomLabel";
			zoomLabel.Size = new System.Drawing.Size(122, 15);
			zoomLabel.TabIndex = 40;
			zoomLabel.Text = "<-Zoom + First frame";
			// 
			// hueSelect
			// 
			hueSelect.FormattingEnabled = true;
			hueSelect.Items.AddRange(new object[] { "RGB (static)", "BGR (static)", "RGB->GBR", "BGR->RBG", "RGB->BRG", "BGR->GRB" });
			hueSelect.Location = new System.Drawing.Point(19, 350);
			hueSelect.Name = "hueSelect";
			hueSelect.Size = new System.Drawing.Size(86, 23);
			hueSelect.TabIndex = 17;
			hueSelect.Text = "Select  Hue";
			hueSelect.SelectedIndexChanged += hueSelect_SelectedIndexChanged;
			// 
			// spinLabel
			// 
			spinLabel.AutoSize = true;
			spinLabel.Location = new System.Drawing.Point(19, 283);
			spinLabel.Name = "spinLabel";
			spinLabel.Size = new System.Drawing.Size(261, 15);
			spinLabel.TabIndex = 41;
			spinLabel.Text = "Spin direction      Spin Speed           Default Angle";
			// 
			// hueLabel
			// 
			hueLabel.AutoSize = true;
			hueLabel.Location = new System.Drawing.Point(19, 332);
			hueLabel.Name = "hueLabel";
			hueLabel.Size = new System.Drawing.Size(252, 15);
			hueLabel.TabIndex = 42;
			hueLabel.Text = "Hue Select             HueSpeed           Default Hue";
			// 
			// spinSpeedBox
			// 
			spinSpeedBox.Location = new System.Drawing.Point(111, 301);
			spinSpeedBox.Name = "spinSpeedBox";
			spinSpeedBox.Size = new System.Drawing.Size(62, 23);
			spinSpeedBox.TabIndex = 43;
			spinSpeedBox.Text = "0";
			spinSpeedBox.TextChanged += spinSpeedBox_TextChanged;
			// 
			// hueSpeedBox
			// 
			hueSpeedBox.Location = new System.Drawing.Point(111, 350);
			hueSpeedBox.Name = "hueSpeedBox";
			hueSpeedBox.Size = new System.Drawing.Size(62, 23);
			hueSpeedBox.TabIndex = 44;
			hueSpeedBox.Text = "0";
			hueSpeedBox.TextChanged += hueSpeedBox_TextChanged;
			// 
			// GeneratorForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(1086, 858);
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
			Controls.Add(cutparamBar);
			Controls.Add(encodeButton);
			Controls.Add(defaultAngle);
			Controls.Add(defaultZoom);
			Controls.Add(label1);
			Controls.Add(blurBar);
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
			Controls.Add(ambLabel);
			Controls.Add(ambBar);
			Controls.Add(noiseLabel);
			Controls.Add(noiseBar);
			Controls.Add(detailLabel);
			Controls.Add(detailBar);
			Controls.Add(saturateLabel);
			Controls.Add(saturateBar);
			Controls.Add(threadsBar);
			Controls.Add(threadsLabel);
			Controls.Add(parallelBox);
			Controls.Add(parallelTypeBox);
			Controls.Add(statusLabel);
			Controls.Add(infoLabel);
			Controls.Add(pngButton);
			Controls.Add(gifButton);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "GeneratorForm";
			Text = "RGB Fractal Zoom Generator C# v1.73";
			FormClosing += GeneratorForm_FormClosing;
			Load += GeneratorForm_Load;
			((System.ComponentModel.ISupportInitialize)ambBar).EndInit();
			((System.ComponentModel.ISupportInitialize)noiseBar).EndInit();
			((System.ComponentModel.ISupportInitialize)detailBar).EndInit();
			((System.ComponentModel.ISupportInitialize)saturateBar).EndInit();
			((System.ComponentModel.ISupportInitialize)threadsBar).EndInit();
			((System.ComponentModel.ISupportInitialize)blurBar).EndInit();
			((System.ComponentModel.ISupportInitialize)cutparamBar).EndInit();
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
		private System.Windows.Forms.CheckBox parallelBox;
		private System.Windows.Forms.CheckBox parallelTypeBox;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.Button pngButton;
		private System.Windows.Forms.Button gifButton;
		private System.Windows.Forms.Label saturateLabel;
		private System.Windows.Forms.TrackBar saturateBar;
		private System.Windows.Forms.Label ambLabel;
		private System.Windows.Forms.TrackBar ambBar;
		private System.Windows.Forms.Label noiseLabel;
		private System.Windows.Forms.TrackBar noiseBar;
		private System.Windows.Forms.Label detailLabel;
		private System.Windows.Forms.TrackBar detailBar;
		private System.Windows.Forms.Label threadsLabel;
		private System.Windows.Forms.TrackBar threadsBar;
		private System.Windows.Forms.TrackBar blurBar;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox defaultZoom;
		private System.Windows.Forms.TextBox defaultAngle;
		private System.Windows.Forms.Button encodeButton;
		private System.Windows.Forms.TrackBar cutparamBar;
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
	}
}

