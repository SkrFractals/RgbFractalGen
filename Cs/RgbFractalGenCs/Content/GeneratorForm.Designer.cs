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
			timingBox = new TextBox();
			prevButton = new Button();
			nextButton = new Button();
			animateButton = new Button();
			exportButton = new Button();
			savePng = new SaveFileDialog();
			saveGif = new SaveFileDialog();
			loadFractal = new OpenFileDialog();
			saveFractal = new SaveFileDialog();
			voidAmbientLabel = new Label();
			saturateLabel = new Label();
			statusLabel = new Label();
			infoLabel = new Label();
			bloomLabel = new Label();
			defaultZoomBox = new TextBox();
			defaultAngleBox = new TextBox();
			defaultHue = new TextBox();
			periodMultiplierBox = new TextBox();
			periodLabel = new Label();
			spinSelect = new ComboBox();
			zoomLabel = new Label();
			hueSelect = new ComboBox();
			spinLabel0 = new Label();
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
			abortBox = new TextBox();
			brightnessBox = new TextBox();
			zoomSelect = new ComboBox();
			restartButton = new Button();
			generationSelect = new ComboBox();
			debugBox = new CheckBox();
			debugLabel = new Label();
			generatorPanel = new Panel();
			genControlPanel = new Panel();
			tasksButton = new Button();
			fileSelect = new ComboBox();
			genNameLabel = new Label();
			generationModeLabel = new Label();
			fileLabel = new Label();
			exportSelect = new ComboBox();
			nameBox = new TextBox();
			fractalSettingsPanel = new Panel();
			spinLabel2 = new Label();
			spinLabel1 = new Label();
			stripeBox = new TextBox();
			l2Label = new Label();
			l2Box = new TextBox();
			gpuDLabel = new Label();
			gpuVLabel = new Label();
			stripeLabel = new Label();
			binLabel = new Label();
			binBox = new TextBox();
			voidSelect = new ComboBox();
			drawSelect = new ComboBox();
			ditherBox = new CheckBox();
			ditherLabel = new Label();
			encodePngLabel = new Label();
			encodePngSelect = new ComboBox();
			encodeGifLabel = new Label();
			encodeGifSelect = new ComboBox();
			timingSelect = new ComboBox();
			abortDelayLabel = new Label();
			timingLabel = new Label();
			parallelLabel = new Label();
			zoomChildLabel = new Label();
			zoomChildBox = new TextBox();
			blurLabel = new Label();
			brightnessLabel = new Label();
			detailLabel = new Label();
			voidScaleLabel = new Label();
			voidNoiseLabel = new Label();
			voidBox = new TextBox();
			editorPanel = new Panel();
			fractalEditorPanel = new Panel();
			sizeLabel3 = new Label();
			maxBox = new TextBox();
			removeColorButton = new Button();
			sizeLabel2 = new Label();
			removeAngleButton = new Button();
			removeCutButton = new Button();
			addColorButton = new Button();
			sizeLabel1 = new Label();
			sizeBox = new TextBox();
			addAngleButton = new Button();
			cutBox = new TextBox();
			sizeLabel0 = new Label();
			colorBox = new TextBox();
			minBox = new TextBox();
			angleBox = new TextBox();
			addCut = new ComboBox();
			addCutLabel = new Label();
			pointLabel2 = new Label();
			pointLabel1 = new Label();
			pointLabel0 = new Label();
			pointPanel = new Panel();
			addPointButton = new Button();
			preButton = new Button();
			loadButton = new Button();
			saveButton = new Button();
			saveMp4 = new SaveFileDialog();
			paletteSelect = new ComboBox();
			paletteLabel = new Label();
			addPalette = new Button();
			removePalette = new Button();
			paletteDialog = new ColorDialog();
			debugsLabel = new Label();
			animBox = new CheckBox();
			pngBox = new CheckBox();
			gifBox = new CheckBox();
			loadExport = new OpenFileDialog();
			removeResolution = new Button();
			addResolution = new Button();
			toolGenPanel = new Panel();
			frameBox = new TextBox();
			hideButton = new Button();
			popupPanel = new Panel();
			debugPanel = new Panel();
			toolEditPanel = new Panel();
			generatorPanel.SuspendLayout();
			genControlPanel.SuspendLayout();
			fractalSettingsPanel.SuspendLayout();
			editorPanel.SuspendLayout();
			fractalEditorPanel.SuspendLayout();
			pointPanel.SuspendLayout();
			toolGenPanel.SuspendLayout();
			popupPanel.SuspendLayout();
			debugPanel.SuspendLayout();
			toolEditPanel.SuspendLayout();
			SuspendLayout();
			// 
			// fractalLabel
			// 
			fractalLabel.AutoSize = true;
			fractalLabel.ForeColor = System.Drawing.Color.White;
			fractalLabel.Location = new System.Drawing.Point(3, 6);
			fractalLabel.Margin = new Padding(4, 0, 4, 0);
			fractalLabel.Name = "fractalLabel";
			fractalLabel.Size = new System.Drawing.Size(48, 15);
			fractalLabel.TabIndex = 0;
			fractalLabel.Text = "[fractal]";
			// 
			// fractalSelect
			// 
			fractalSelect.FormattingEnabled = true;
			fractalSelect.Location = new System.Drawing.Point(57, 3);
			fractalSelect.Margin = new Padding(4, 3, 4, 3);
			fractalSelect.Name = "fractalSelect";
			fractalSelect.Size = new System.Drawing.Size(231, 23);
			fractalSelect.TabIndex = 1;
			fractalSelect.Text = "[ Fractal ]";
			fractalSelect.SelectedIndexChanged += FractalSelect_SelectedIndexChanged;
			fractalSelect.TextUpdate += FractalSelect_TextUpdate;
			// 
			// angleLabel
			// 
			angleLabel.AutoSize = true;
			angleLabel.ForeColor = System.Drawing.Color.White;
			angleLabel.Location = new System.Drawing.Point(3, 35);
			angleLabel.Margin = new Padding(4, 0, 4, 0);
			angleLabel.Name = "angleLabel";
			angleLabel.Size = new System.Drawing.Size(49, 15);
			angleLabel.TabIndex = 36;
			angleLabel.Text = "[angles]";
			// 
			// angleSelect
			// 
			angleSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			angleSelect.FormattingEnabled = true;
			angleSelect.Location = new System.Drawing.Point(57, 32);
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
			colorLabel.Location = new System.Drawing.Point(3, 64);
			colorLabel.Margin = new Padding(4, 0, 4, 0);
			colorLabel.Name = "colorLabel";
			colorLabel.Size = new System.Drawing.Size(47, 15);
			colorLabel.TabIndex = 38;
			colorLabel.Text = "[colors]";
			// 
			// colorSelect
			// 
			colorSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			colorSelect.FormattingEnabled = true;
			colorSelect.Location = new System.Drawing.Point(57, 61);
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
			cutLabel.Location = new System.Drawing.Point(3, 93);
			cutLabel.Margin = new Padding(4, 0, 4, 0);
			cutLabel.Name = "cutLabel";
			cutLabel.Size = new System.Drawing.Size(32, 15);
			cutLabel.TabIndex = 34;
			cutLabel.Text = "[cut]";
			// 
			// cutSelect
			// 
			cutSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			cutSelect.FormattingEnabled = true;
			cutSelect.Location = new System.Drawing.Point(57, 90);
			cutSelect.Margin = new Padding(4, 3, 4, 3);
			cutSelect.Name = "cutSelect";
			cutSelect.Size = new System.Drawing.Size(129, 23);
			cutSelect.TabIndex = 4;
			cutSelect.SelectedIndexChanged += CutSelect_SelectedIndexChanged;
			// 
			// cutparamBox
			// 
			cutparamBox.Location = new System.Drawing.Point(194, 90);
			cutparamBox.Margin = new Padding(4, 3, 4, 3);
			cutparamBox.Name = "cutparamBox";
			cutparamBox.Size = new System.Drawing.Size(94, 23);
			cutparamBox.TabIndex = 5;
			cutparamBox.Text = "0";
			cutparamBox.TextChanged += CutSeedBox_TextChanged;
			// 
			// resX
			// 
			resX.Location = new System.Drawing.Point(3, 119);
			resX.Margin = new Padding(4, 3, 4, 3);
			resX.Name = "resX";
			resX.Size = new System.Drawing.Size(46, 23);
			resX.TabIndex = 7;
			resX.Text = "1920";
			resX.TextChanged += ResolutionXY_Changed;
			// 
			// resY
			// 
			resY.Location = new System.Drawing.Point(57, 119);
			resY.Margin = new Padding(4, 3, 4, 3);
			resY.Name = "resY";
			resY.Size = new System.Drawing.Size(46, 23);
			resY.TabIndex = 8;
			resY.Text = "1080";
			resY.TextChanged += ResolutionXY_Changed;
			// 
			// resSelect
			// 
			resSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			resSelect.FormattingEnabled = true;
			resSelect.Items.AddRange(new object[] { "80x80", "Custom:" });
			resSelect.Location = new System.Drawing.Point(111, 119);
			resSelect.Margin = new Padding(4, 3, 4, 3);
			resSelect.Name = "resSelect";
			resSelect.Size = new System.Drawing.Size(124, 23);
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
			periodBox.Location = new System.Drawing.Point(103, 148);
			periodBox.Margin = new Padding(4, 3, 4, 3);
			periodBox.Name = "periodBox";
			periodBox.Size = new System.Drawing.Size(92, 23);
			periodBox.TabIndex = 10;
			periodBox.Text = "120";
			periodBox.TextChanged += PeriodBox_TextChanged;
			// 
			// timingBox
			// 
			timingBox.Location = new System.Drawing.Point(201, 575);
			timingBox.Margin = new Padding(4, 3, 4, 3);
			timingBox.Name = "timingBox";
			timingBox.Size = new System.Drawing.Size(59, 23);
			timingBox.TabIndex = 28;
			timingBox.Text = "60";
			timingBox.TextChanged += TimingBox_TextChanged;
			// 
			// prevButton
			// 
			prevButton.Location = new System.Drawing.Point(90, 3);
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
			nextButton.Location = new System.Drawing.Point(203, 3);
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
			animateButton.Location = new System.Drawing.Point(241, 3);
			animateButton.Margin = new Padding(4, 3, 4, 3);
			animateButton.Name = "animateButton";
			animateButton.Size = new System.Drawing.Size(30, 27);
			animateButton.TabIndex = 31;
			animateButton.Text = "▶︎";
			animateButton.UseVisualStyleBackColor = true;
			animateButton.Click += AnimateButton_Click;
			// 
			// exportButton
			// 
			exportButton.Location = new System.Drawing.Point(4, 58);
			exportButton.Margin = new Padding(4, 3, 4, 3);
			exportButton.Name = "exportButton";
			exportButton.Size = new System.Drawing.Size(100, 27);
			exportButton.TabIndex = 34;
			exportButton.Text = "[ Export ]";
			exportButton.UseVisualStyleBackColor = true;
			exportButton.Click += ExportButton_Click;
			// 
			// savePng
			// 
			savePng.DefaultExt = "png";
			savePng.Filter = "PNG files (*.png)|*.png";
			savePng.RestoreDirectory = true;
			savePng.FileOk += SavePng_FileOk;
			// 
			// saveGif
			// 
			saveGif.DefaultExt = "gif";
			saveGif.Filter = "GIF files (*.gif)|*.gif";
			saveGif.RestoreDirectory = true;
			saveGif.FileOk += SaveGif_FileOk;
			// 
			// loadFractal
			// 
			loadFractal.DefaultExt = "fractal";
			loadFractal.Filter = "Fractal files (*.fractal)|*.fractal";
			loadFractal.RestoreDirectory = true;
			loadFractal.FileOk += LoadFractal_FileOk;
			// 
			// saveFractal
			// 
			saveFractal.DefaultExt = "fractal";
			saveFractal.Filter = "Fractal files (*.fractal)|*.fractal";
			saveFractal.RestoreDirectory = true;
			saveFractal.FileOk += SaveFractal_FileOk;
			// 
			// voidAmbientLabel
			// 
			voidAmbientLabel.AutoSize = true;
			voidAmbientLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			voidAmbientLabel.ForeColor = System.Drawing.Color.White;
			voidAmbientLabel.Location = new System.Drawing.Point(6, 288);
			voidAmbientLabel.Margin = new Padding(4, 0, 4, 0);
			voidAmbientLabel.Name = "voidAmbientLabel";
			voidAmbientLabel.Size = new System.Drawing.Size(84, 15);
			voidAmbientLabel.TabIndex = 0;
			voidAmbientLabel.Text = "[voidAmbient]";
			// 
			// saturateLabel
			// 
			saturateLabel.AutoSize = true;
			saturateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			saturateLabel.ForeColor = System.Drawing.Color.White;
			saturateLabel.Location = new System.Drawing.Point(6, 404);
			saturateLabel.Margin = new Padding(4, 0, 4, 0);
			saturateLabel.Name = "saturateLabel";
			saturateLabel.Size = new System.Drawing.Size(57, 15);
			saturateLabel.TabIndex = 0;
			saturateLabel.Text = "[saturate]";
			// 
			// statusLabel
			// 
			statusLabel.AutoSize = true;
			statusLabel.ForeColor = System.Drawing.Color.White;
			statusLabel.Location = new System.Drawing.Point(4, 9);
			statusLabel.Margin = new Padding(4, 0, 4, 0);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(46, 15);
			statusLabel.TabIndex = 0;
			statusLabel.Text = "[status]";
			// 
			// infoLabel
			// 
			infoLabel.AutoSize = true;
			infoLabel.ForeColor = System.Drawing.Color.White;
			infoLabel.Location = new System.Drawing.Point(128, 9);
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
			bloomLabel.Location = new System.Drawing.Point(6, 462);
			bloomLabel.Margin = new Padding(4, 0, 4, 0);
			bloomLabel.Name = "bloomLabel";
			bloomLabel.Size = new System.Drawing.Size(50, 15);
			bloomLabel.TabIndex = 0;
			bloomLabel.Text = "[bloom]";
			// 
			// defaultZoomBox
			// 
			defaultZoomBox.Location = new System.Drawing.Point(201, 177);
			defaultZoomBox.Name = "defaultZoomBox";
			defaultZoomBox.Size = new System.Drawing.Size(59, 23);
			defaultZoomBox.TabIndex = 13;
			defaultZoomBox.Text = "0";
			defaultZoomBox.TextChanged += DefaultZoom_TextChanged;
			// 
			// defaultAngleBox
			// 
			defaultAngleBox.Location = new System.Drawing.Point(135, 256);
			defaultAngleBox.Name = "defaultAngleBox";
			defaultAngleBox.Size = new System.Drawing.Size(60, 23);
			defaultAngleBox.TabIndex = 16;
			defaultAngleBox.Text = "0";
			defaultAngleBox.TextChanged += DefaultAngle_TextChanged;
			// 
			// defaultHue
			// 
			defaultHue.Location = new System.Drawing.Point(242, 148);
			defaultHue.Name = "defaultHue";
			defaultHue.Size = new System.Drawing.Size(46, 23);
			defaultHue.TabIndex = 19;
			defaultHue.Text = "0";
			defaultHue.TextChanged += DefaultHue_TextChanged;
			// 
			// periodMultiplierBox
			// 
			periodMultiplierBox.Location = new System.Drawing.Point(201, 148);
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
			periodLabel.Location = new System.Drawing.Point(6, 151);
			periodLabel.Name = "periodLabel";
			periodLabel.Size = new System.Drawing.Size(49, 15);
			periodLabel.TabIndex = 31;
			periodLabel.Text = "[period]";
			// 
			// spinSelect
			// 
			spinSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			spinSelect.FormattingEnabled = true;
			spinSelect.Items.AddRange(new object[] { "Random", "Clock", "None", "Counterclock", "Antispin", "Peri Antispin" });
			spinSelect.Location = new System.Drawing.Point(3, 256);
			spinSelect.Name = "spinSelect";
			spinSelect.Size = new System.Drawing.Size(126, 23);
			spinSelect.TabIndex = 14;
			spinSelect.SelectedIndexChanged += SpinSelect_SelectedIndexChanged;
			// 
			// zoomLabel
			// 
			zoomLabel.AutoSize = true;
			zoomLabel.ForeColor = System.Drawing.Color.White;
			zoomLabel.Location = new System.Drawing.Point(6, 180);
			zoomLabel.Name = "zoomLabel";
			zoomLabel.Size = new System.Drawing.Size(45, 15);
			zoomLabel.TabIndex = 40;
			zoomLabel.Text = "[zoom]";
			// 
			// hueSelect
			// 
			hueSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			hueSelect.FormattingEnabled = true;
			hueSelect.Items.AddRange(new object[] { "Random", "Static", "->", "<-" });
			hueSelect.Location = new System.Drawing.Point(103, 206);
			hueSelect.Name = "hueSelect";
			hueSelect.Size = new System.Drawing.Size(92, 23);
			hueSelect.TabIndex = 17;
			hueSelect.SelectedIndexChanged += HueSelect_SelectedIndexChanged;
			// 
			// spinLabel0
			// 
			spinLabel0.AutoSize = true;
			spinLabel0.ForeColor = System.Drawing.Color.White;
			spinLabel0.Location = new System.Drawing.Point(6, 238);
			spinLabel0.Name = "spinLabel0";
			spinLabel0.Size = new System.Drawing.Size(95, 15);
			spinLabel0.TabIndex = 41;
			spinLabel0.Text = "[spin0_direction]";
			// 
			// hueLabel
			// 
			hueLabel.AutoSize = true;
			hueLabel.ForeColor = System.Drawing.Color.White;
			hueLabel.Location = new System.Drawing.Point(6, 209);
			hueLabel.Name = "hueLabel";
			hueLabel.Size = new System.Drawing.Size(35, 15);
			hueLabel.TabIndex = 42;
			hueLabel.Text = "[hue]";
			// 
			// spinSpeedBox
			// 
			spinSpeedBox.Location = new System.Drawing.Point(201, 256);
			spinSpeedBox.Name = "spinSpeedBox";
			spinSpeedBox.Size = new System.Drawing.Size(59, 23);
			spinSpeedBox.TabIndex = 43;
			spinSpeedBox.Text = "0";
			spinSpeedBox.TextChanged += SpinSpeedBox_TextChanged;
			// 
			// hueSpeedBox
			// 
			hueSpeedBox.Location = new System.Drawing.Point(201, 206);
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
			parallelTypeSelect.Location = new System.Drawing.Point(96, 604);
			parallelTypeSelect.Name = "parallelTypeSelect";
			parallelTypeSelect.Size = new System.Drawing.Size(164, 23);
			parallelTypeSelect.TabIndex = 27;
			parallelTypeSelect.SelectedIndexChanged += ParallelTypeSelect_SelectedIndexChanged;
			// 
			// ambBox
			// 
			ambBox.Location = new System.Drawing.Point(201, 285);
			ambBox.Name = "ambBox";
			ambBox.Size = new System.Drawing.Size(59, 23);
			ambBox.TabIndex = 20;
			ambBox.Text = "20";
			ambBox.TextChanged += AmbBox_TextChanged;
			// 
			// noiseBox
			// 
			noiseBox.Location = new System.Drawing.Point(201, 314);
			noiseBox.Name = "noiseBox";
			noiseBox.Size = new System.Drawing.Size(59, 23);
			noiseBox.TabIndex = 21;
			noiseBox.Text = "20";
			noiseBox.TextChanged += NoiseBox_TextChanged;
			// 
			// saturateBox
			// 
			saturateBox.Location = new System.Drawing.Point(201, 401);
			saturateBox.Name = "saturateBox";
			saturateBox.Size = new System.Drawing.Size(59, 23);
			saturateBox.TabIndex = 22;
			saturateBox.Text = "100";
			saturateBox.TextChanged += SaturateBox_TextChanged;
			// 
			// detailBox
			// 
			detailBox.Location = new System.Drawing.Point(201, 372);
			detailBox.Name = "detailBox";
			detailBox.Size = new System.Drawing.Size(59, 23);
			detailBox.TabIndex = 23;
			detailBox.Text = "3";
			detailBox.TextChanged += DetailBox_TextChanged;
			// 
			// blurBox
			// 
			blurBox.Location = new System.Drawing.Point(201, 488);
			blurBox.Name = "blurBox";
			blurBox.Size = new System.Drawing.Size(59, 23);
			blurBox.TabIndex = 24;
			blurBox.Text = "0";
			blurBox.TextChanged += BlurBox_TextChanged;
			// 
			// bloomBox
			// 
			bloomBox.Location = new System.Drawing.Point(201, 459);
			bloomBox.Name = "bloomBox";
			bloomBox.Size = new System.Drawing.Size(59, 23);
			bloomBox.TabIndex = 25;
			bloomBox.Text = "10";
			bloomBox.TextChanged += BloomBox_TextChanged;
			// 
			// abortBox
			// 
			abortBox.Location = new System.Drawing.Point(201, 546);
			abortBox.Margin = new Padding(4, 3, 4, 3);
			abortBox.Name = "abortBox";
			abortBox.Size = new System.Drawing.Size(59, 23);
			abortBox.TabIndex = 45;
			abortBox.Text = "50";
			abortBox.TextChanged += AbortBox_TextChanged;
			// 
			// brightnessBox
			// 
			brightnessBox.Location = new System.Drawing.Point(201, 430);
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
			zoomSelect.Location = new System.Drawing.Point(103, 177);
			zoomSelect.Name = "zoomSelect";
			zoomSelect.Size = new System.Drawing.Size(92, 23);
			zoomSelect.TabIndex = 48;
			zoomSelect.SelectedIndexChanged += ZoomSelect_SelectedIndexChanged;
			// 
			// restartButton
			// 
			restartButton.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
			restartButton.Location = new System.Drawing.Point(278, 3);
			restartButton.Name = "restartButton";
			restartButton.Size = new System.Drawing.Size(108, 27);
			restartButton.TabIndex = 49;
			restartButton.Text = "[ RESTART ]";
			restartButton.UseVisualStyleBackColor = false;
			restartButton.Click += RestartButton_Click;
			// 
			// generationSelect
			// 
			generationSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			generationSelect.FormattingEnabled = true;
			generationSelect.Items.AddRange(new object[] { "Only Image", "Animation", "All Seeds" });
			generationSelect.Location = new System.Drawing.Point(133, 32);
			generationSelect.Name = "generationSelect";
			generationSelect.Size = new System.Drawing.Size(127, 23);
			generationSelect.TabIndex = 51;
			generationSelect.SelectedIndexChanged += GenerationSelect_SelectedIndexChanged;
			// 
			// debugBox
			// 
			debugBox.AutoSize = true;
			debugBox.ForeColor = System.Drawing.Color.White;
			debugBox.Location = new System.Drawing.Point(54, 5);
			debugBox.Name = "debugBox";
			debugBox.Size = new System.Drawing.Size(68, 19);
			debugBox.TabIndex = 52;
			debugBox.Text = "[debug]";
			debugBox.UseVisualStyleBackColor = true;
			debugBox.CheckedChanged += DebugBox_CheckedChanged;
			// 
			// debugLabel
			// 
			debugLabel.AutoSize = true;
			debugLabel.ForeColor = System.Drawing.Color.White;
			debugLabel.Location = new System.Drawing.Point(2, 27);
			debugLabel.Name = "debugLabel";
			debugLabel.Size = new System.Drawing.Size(73, 15);
			debugLabel.TabIndex = 53;
			debugLabel.Text = "DebugString";
			// 
			// generatorPanel
			// 
			generatorPanel.Controls.Add(genControlPanel);
			generatorPanel.Controls.Add(fractalSettingsPanel);
			generatorPanel.Location = new System.Drawing.Point(3, 176);
			generatorPanel.Name = "generatorPanel";
			generatorPanel.Size = new System.Drawing.Size(286, 440);
			generatorPanel.TabIndex = 54;
			// 
			// genControlPanel
			// 
			genControlPanel.Controls.Add(tasksButton);
			genControlPanel.Controls.Add(fileSelect);
			genControlPanel.Controls.Add(genNameLabel);
			genControlPanel.Controls.Add(generationModeLabel);
			genControlPanel.Controls.Add(fileLabel);
			genControlPanel.Controls.Add(generationSelect);
			genControlPanel.Controls.Add(exportSelect);
			genControlPanel.Controls.Add(exportButton);
			genControlPanel.Controls.Add(nameBox);
			genControlPanel.Location = new System.Drawing.Point(3, 320);
			genControlPanel.Name = "genControlPanel";
			genControlPanel.Size = new System.Drawing.Size(280, 117);
			genControlPanel.TabIndex = 68;
			// 
			// tasksButton
			// 
			tasksButton.Location = new System.Drawing.Point(176, 59);
			tasksButton.Margin = new Padding(4, 3, 4, 3);
			tasksButton.Name = "tasksButton";
			tasksButton.Size = new System.Drawing.Size(85, 27);
			tasksButton.TabIndex = 67;
			tasksButton.Text = "[ Tasks]";
			tasksButton.UseVisualStyleBackColor = true;
			tasksButton.Click += TasksButton_Click;
			// 
			// fileSelect
			// 
			fileSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			fileSelect.FormattingEnabled = true;
			fileSelect.Items.AddRange(new object[] { "Fractal", "Angles - (Bitmask)", "Colors - (Bitmask)", "Function - (Type_Seed)", "Resolution - (W_H)", "Hues - (Palette_Hue_Shift_Speed)", "Period - (Frames_Multiplier)", "Zoom - (Direction_Child)", "Spin - (Direction_Default_Speed)", "Void - (Ambient_Noise_Scale)", "Image - (Satur_Bright_Bloom_Blur)", "Detail - (Dithering_Detail)" });
			fileSelect.Location = new System.Drawing.Point(52, 90);
			fileSelect.Margin = new Padding(4, 3, 4, 3);
			fileSelect.Name = "fileSelect";
			fileSelect.Size = new System.Drawing.Size(209, 23);
			fileSelect.TabIndex = 65;
			fileSelect.SelectedIndexChanged += FileSelect_SelectedIndexChanged;
			// 
			// genNameLabel
			// 
			genNameLabel.AutoSize = true;
			genNameLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			genNameLabel.ForeColor = System.Drawing.Color.White;
			genNameLabel.Location = new System.Drawing.Point(4, 7);
			genNameLabel.Margin = new Padding(4, 0, 4, 0);
			genNameLabel.Name = "genNameLabel";
			genNameLabel.Size = new System.Drawing.Size(67, 15);
			genNameLabel.TabIndex = 98;
			genNameLabel.Text = "[genName]";
			// 
			// generationModeLabel
			// 
			generationModeLabel.AutoSize = true;
			generationModeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			generationModeLabel.ForeColor = System.Drawing.Color.White;
			generationModeLabel.Location = new System.Drawing.Point(4, 35);
			generationModeLabel.Margin = new Padding(4, 0, 4, 0);
			generationModeLabel.Name = "generationModeLabel";
			generationModeLabel.Size = new System.Drawing.Size(103, 15);
			generationModeLabel.TabIndex = 76;
			generationModeLabel.Text = "[generationMode]";
			// 
			// fileLabel
			// 
			fileLabel.AutoSize = true;
			fileLabel.ForeColor = System.Drawing.Color.White;
			fileLabel.Location = new System.Drawing.Point(4, 93);
			fileLabel.Margin = new Padding(4, 0, 4, 0);
			fileLabel.Name = "fileLabel";
			fileLabel.Size = new System.Drawing.Size(31, 15);
			fileLabel.TabIndex = 66;
			fileLabel.Text = "[file]";
			// 
			// exportSelect
			// 
			exportSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			exportSelect.FormattingEnabled = true;
			exportSelect.Items.AddRange(new object[] { "Current PNG", "PNGs", "MP4", "Selected GIF", "GIF->MP4", "Load Export", "Import Code" });
			exportSelect.Location = new System.Drawing.Point(110, 61);
			exportSelect.Name = "exportSelect";
			exportSelect.Size = new System.Drawing.Size(60, 23);
			exportSelect.TabIndex = 61;
			exportSelect.SelectedIndexChanged += ExportSelect_SelectedIndexChanged;
			// 
			// nameBox
			// 
			nameBox.Location = new System.Drawing.Point(133, 3);
			nameBox.Margin = new Padding(4, 3, 4, 3);
			nameBox.Name = "nameBox";
			nameBox.Size = new System.Drawing.Size(128, 23);
			nameBox.TabIndex = 97;
			nameBox.TextChanged += NameBox_TextChanged;
			// 
			// fractalSettingsPanel
			// 
			fractalSettingsPanel.AutoScroll = true;
			fractalSettingsPanel.BackColor = System.Drawing.Color.Black;
			fractalSettingsPanel.Controls.Add(spinLabel2);
			fractalSettingsPanel.Controls.Add(spinLabel1);
			fractalSettingsPanel.Controls.Add(stripeBox);
			fractalSettingsPanel.Controls.Add(l2Label);
			fractalSettingsPanel.Controls.Add(l2Box);
			fractalSettingsPanel.Controls.Add(gpuDLabel);
			fractalSettingsPanel.Controls.Add(gpuVLabel);
			fractalSettingsPanel.Controls.Add(stripeLabel);
			fractalSettingsPanel.Controls.Add(binLabel);
			fractalSettingsPanel.Controls.Add(binBox);
			fractalSettingsPanel.Controls.Add(voidSelect);
			fractalSettingsPanel.Controls.Add(drawSelect);
			fractalSettingsPanel.Controls.Add(ditherBox);
			fractalSettingsPanel.Controls.Add(ditherLabel);
			fractalSettingsPanel.Controls.Add(encodePngLabel);
			fractalSettingsPanel.Controls.Add(encodePngSelect);
			fractalSettingsPanel.Controls.Add(encodeGifLabel);
			fractalSettingsPanel.Controls.Add(encodeGifSelect);
			fractalSettingsPanel.Controls.Add(timingSelect);
			fractalSettingsPanel.Controls.Add(abortDelayLabel);
			fractalSettingsPanel.Controls.Add(timingLabel);
			fractalSettingsPanel.Controls.Add(parallelLabel);
			fractalSettingsPanel.Controls.Add(zoomChildLabel);
			fractalSettingsPanel.Controls.Add(zoomChildBox);
			fractalSettingsPanel.Controls.Add(blurLabel);
			fractalSettingsPanel.Controls.Add(brightnessLabel);
			fractalSettingsPanel.Controls.Add(detailLabel);
			fractalSettingsPanel.Controls.Add(voidScaleLabel);
			fractalSettingsPanel.Controls.Add(voidNoiseLabel);
			fractalSettingsPanel.Controls.Add(abortBox);
			fractalSettingsPanel.Controls.Add(periodBox);
			fractalSettingsPanel.Controls.Add(voidBox);
			fractalSettingsPanel.Controls.Add(hueSelect);
			fractalSettingsPanel.Controls.Add(zoomLabel);
			fractalSettingsPanel.Controls.Add(zoomSelect);
			fractalSettingsPanel.Controls.Add(spinLabel0);
			fractalSettingsPanel.Controls.Add(brightnessBox);
			fractalSettingsPanel.Controls.Add(timingBox);
			fractalSettingsPanel.Controls.Add(saturateLabel);
			fractalSettingsPanel.Controls.Add(hueLabel);
			fractalSettingsPanel.Controls.Add(periodLabel);
			fractalSettingsPanel.Controls.Add(spinSelect);
			fractalSettingsPanel.Controls.Add(blurBox);
			fractalSettingsPanel.Controls.Add(spinSpeedBox);
			fractalSettingsPanel.Controls.Add(defaultAngleBox);
			fractalSettingsPanel.Controls.Add(parallelTypeSelect);
			fractalSettingsPanel.Controls.Add(bloomBox);
			fractalSettingsPanel.Controls.Add(hueSpeedBox);
			fractalSettingsPanel.Controls.Add(periodMultiplierBox);
			fractalSettingsPanel.Controls.Add(ambBox);
			fractalSettingsPanel.Controls.Add(saturateBox);
			fractalSettingsPanel.Controls.Add(voidAmbientLabel);
			fractalSettingsPanel.Controls.Add(defaultZoomBox);
			fractalSettingsPanel.Controls.Add(bloomLabel);
			fractalSettingsPanel.Controls.Add(noiseBox);
			fractalSettingsPanel.Controls.Add(detailBox);
			fractalSettingsPanel.Location = new System.Drawing.Point(3, 3);
			fractalSettingsPanel.Name = "fractalSettingsPanel";
			fractalSettingsPanel.Size = new System.Drawing.Size(280, 315);
			fractalSettingsPanel.TabIndex = 60;
			// 
			// spinLabel2
			// 
			spinLabel2.AutoSize = true;
			spinLabel2.ForeColor = System.Drawing.Color.White;
			spinLabel2.Location = new System.Drawing.Point(206, 238);
			spinLabel2.Name = "spinLabel2";
			spinLabel2.Size = new System.Drawing.Size(53, 15);
			spinLabel2.TabIndex = 69;
			spinLabel2.Text = "[spin2_s]";
			// 
			// spinLabel1
			// 
			spinLabel1.AutoSize = true;
			spinLabel1.ForeColor = System.Drawing.Color.White;
			spinLabel1.Location = new System.Drawing.Point(140, 238);
			spinLabel1.Name = "spinLabel1";
			spinLabel1.Size = new System.Drawing.Size(65, 15);
			spinLabel1.TabIndex = 68;
			spinLabel1.Text = "[spin1_def]";
			// 
			// stripeBox
			// 
			stripeBox.Location = new System.Drawing.Point(201, 691);
			stripeBox.Name = "stripeBox";
			stripeBox.Size = new System.Drawing.Size(59, 23);
			stripeBox.TabIndex = 88;
			stripeBox.Text = "0";
			stripeBox.TextChanged += StripeBox_TextChanged;
			// 
			// l2Label
			// 
			l2Label.AutoSize = true;
			l2Label.Font = new System.Drawing.Font("Segoe UI", 9F);
			l2Label.ForeColor = System.Drawing.Color.White;
			l2Label.Location = new System.Drawing.Point(6, 665);
			l2Label.Margin = new Padding(4, 0, 4, 0);
			l2Label.Name = "l2Label";
			l2Label.Size = new System.Drawing.Size(24, 15);
			l2Label.TabIndex = 96;
			l2Label.Text = "[l2]";
			// 
			// l2Box
			// 
			l2Box.Location = new System.Drawing.Point(201, 662);
			l2Box.Name = "l2Box";
			l2Box.Size = new System.Drawing.Size(59, 23);
			l2Box.TabIndex = 95;
			l2Box.Text = "0";
			l2Box.TextChanged += L2Box_TextChanged;
			// 
			// gpuDLabel
			// 
			gpuDLabel.AutoSize = true;
			gpuDLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			gpuDLabel.ForeColor = System.Drawing.Color.White;
			gpuDLabel.Location = new System.Drawing.Point(4, 93);
			gpuDLabel.Margin = new Padding(4, 0, 4, 0);
			gpuDLabel.Name = "gpuDLabel";
			gpuDLabel.Size = new System.Drawing.Size(44, 15);
			gpuDLabel.TabIndex = 94;
			gpuDLabel.Text = "[gpuD]";
			// 
			// gpuVLabel
			// 
			gpuVLabel.AutoSize = true;
			gpuVLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			gpuVLabel.ForeColor = System.Drawing.Color.White;
			gpuVLabel.Location = new System.Drawing.Point(4, 64);
			gpuVLabel.Margin = new Padding(4, 0, 4, 0);
			gpuVLabel.Name = "gpuVLabel";
			gpuVLabel.Size = new System.Drawing.Size(43, 15);
			gpuVLabel.TabIndex = 93;
			gpuVLabel.Text = "[gpuV]";
			// 
			// stripeLabel
			// 
			stripeLabel.AutoSize = true;
			stripeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			stripeLabel.ForeColor = System.Drawing.Color.White;
			stripeLabel.Location = new System.Drawing.Point(6, 694);
			stripeLabel.Margin = new Padding(4, 0, 4, 0);
			stripeLabel.Name = "stripeLabel";
			stripeLabel.Size = new System.Drawing.Size(44, 15);
			stripeLabel.TabIndex = 92;
			stripeLabel.Text = "[stripe]";
			// 
			// binLabel
			// 
			binLabel.AutoSize = true;
			binLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			binLabel.ForeColor = System.Drawing.Color.White;
			binLabel.Location = new System.Drawing.Point(6, 636);
			binLabel.Margin = new Padding(4, 0, 4, 0);
			binLabel.Name = "binLabel";
			binLabel.Size = new System.Drawing.Size(32, 15);
			binLabel.TabIndex = 91;
			binLabel.Text = "[bin]";
			// 
			// binBox
			// 
			binBox.Location = new System.Drawing.Point(201, 633);
			binBox.Name = "binBox";
			binBox.Size = new System.Drawing.Size(59, 23);
			binBox.TabIndex = 87;
			binBox.Text = "0";
			binBox.TextChanged += BinBox_TextChanged;
			// 
			// voidSelect
			// 
			voidSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			voidSelect.FormattingEnabled = true;
			voidSelect.Items.AddRange(new object[] { "CPU", "GPU BFS", "GPU JumpFlood", "GPU FastFlood" });
			voidSelect.Location = new System.Drawing.Point(152, 61);
			voidSelect.Name = "voidSelect";
			voidSelect.Size = new System.Drawing.Size(108, 23);
			voidSelect.TabIndex = 86;
			voidSelect.SelectedIndexChanged += VoidSelect_SelectedIndexChanged;
			// 
			// drawSelect
			// 
			drawSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			drawSelect.FormattingEnabled = true;
			drawSelect.Items.AddRange(new object[] { "CPU", "GPU Functions", "GPU Pipeline" });
			drawSelect.Location = new System.Drawing.Point(152, 90);
			drawSelect.Name = "drawSelect";
			drawSelect.Size = new System.Drawing.Size(108, 23);
			drawSelect.TabIndex = 85;
			drawSelect.SelectedIndexChanged += DrawSelect_SelectedIndexChanged;
			// 
			// ditherBox
			// 
			ditherBox.AutoSize = true;
			ditherBox.Checked = true;
			ditherBox.CheckState = CheckState.Checked;
			ditherBox.ForeColor = System.Drawing.Color.White;
			ditherBox.Location = new System.Drawing.Point(152, 121);
			ditherBox.Name = "ditherBox";
			ditherBox.Size = new System.Drawing.Size(65, 19);
			ditherBox.TabIndex = 84;
			ditherBox.Text = "[dither]";
			ditherBox.UseVisualStyleBackColor = true;
			ditherBox.CheckedChanged += DitherBox_CheckedChanged;
			// 
			// ditherLabel
			// 
			ditherLabel.AutoSize = true;
			ditherLabel.ForeColor = System.Drawing.Color.White;
			ditherLabel.Location = new System.Drawing.Point(6, 122);
			ditherLabel.Name = "ditherLabel";
			ditherLabel.Size = new System.Drawing.Size(46, 15);
			ditherLabel.TabIndex = 83;
			ditherLabel.Text = "[dither]";
			// 
			// encodePngLabel
			// 
			encodePngLabel.AutoSize = true;
			encodePngLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			encodePngLabel.ForeColor = System.Drawing.Color.White;
			encodePngLabel.Location = new System.Drawing.Point(4, 6);
			encodePngLabel.Margin = new Padding(4, 0, 4, 0);
			encodePngLabel.Name = "encodePngLabel";
			encodePngLabel.Size = new System.Drawing.Size(75, 15);
			encodePngLabel.TabIndex = 81;
			encodePngLabel.Text = "[encodePng]";
			// 
			// encodePngSelect
			// 
			encodePngSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			encodePngSelect.FormattingEnabled = true;
			encodePngSelect.Items.AddRange(new object[] { "No", "Yes" });
			encodePngSelect.Location = new System.Drawing.Point(152, 3);
			encodePngSelect.Name = "encodePngSelect";
			encodePngSelect.Size = new System.Drawing.Size(108, 23);
			encodePngSelect.TabIndex = 80;
			encodePngSelect.SelectedIndexChanged += EncodePngSelect_SelectedIndexChanged;
			// 
			// encodeGifLabel
			// 
			encodeGifLabel.AutoSize = true;
			encodeGifLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			encodeGifLabel.ForeColor = System.Drawing.Color.White;
			encodeGifLabel.Location = new System.Drawing.Point(4, 35);
			encodeGifLabel.Margin = new Padding(4, 0, 4, 0);
			encodeGifLabel.Name = "encodeGifLabel";
			encodeGifLabel.Size = new System.Drawing.Size(69, 15);
			encodeGifLabel.TabIndex = 78;
			encodeGifLabel.Text = "[encodeGif]";
			// 
			// encodeGifSelect
			// 
			encodeGifSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			encodeGifSelect.FormattingEnabled = true;
			encodeGifSelect.Items.AddRange(new object[] { "No", "Local", "Global" });
			encodeGifSelect.Location = new System.Drawing.Point(152, 32);
			encodeGifSelect.Name = "encodeGifSelect";
			encodeGifSelect.Size = new System.Drawing.Size(108, 23);
			encodeGifSelect.TabIndex = 77;
			encodeGifSelect.SelectedIndexChanged += EncodeGifSelect_SelectedIndexChanged;
			// 
			// timingSelect
			// 
			timingSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			timingSelect.FormattingEnabled = true;
			timingSelect.Items.AddRange(new object[] { "Delay", "Framerate" });
			timingSelect.Location = new System.Drawing.Point(96, 575);
			timingSelect.Name = "timingSelect";
			timingSelect.Size = new System.Drawing.Size(98, 23);
			timingSelect.TabIndex = 70;
			timingSelect.SelectedIndexChanged += TimingSelect_SelectedIndexChanged;
			// 
			// abortDelayLabel
			// 
			abortDelayLabel.AutoSize = true;
			abortDelayLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			abortDelayLabel.ForeColor = System.Drawing.Color.White;
			abortDelayLabel.Location = new System.Drawing.Point(6, 549);
			abortDelayLabel.Margin = new Padding(4, 0, 4, 0);
			abortDelayLabel.Name = "abortDelayLabel";
			abortDelayLabel.Size = new System.Drawing.Size(72, 15);
			abortDelayLabel.TabIndex = 69;
			abortDelayLabel.Text = "[abortDelay]";
			// 
			// timingLabel
			// 
			timingLabel.AutoSize = true;
			timingLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			timingLabel.ForeColor = System.Drawing.Color.White;
			timingLabel.Location = new System.Drawing.Point(6, 578);
			timingLabel.Margin = new Padding(4, 0, 4, 0);
			timingLabel.Name = "timingLabel";
			timingLabel.Size = new System.Drawing.Size(50, 15);
			timingLabel.TabIndex = 67;
			timingLabel.Text = "[timing]";
			// 
			// parallelLabel
			// 
			parallelLabel.AutoSize = true;
			parallelLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			parallelLabel.ForeColor = System.Drawing.Color.White;
			parallelLabel.Location = new System.Drawing.Point(6, 607);
			parallelLabel.Margin = new Padding(4, 0, 4, 0);
			parallelLabel.Name = "parallelLabel";
			parallelLabel.Size = new System.Drawing.Size(53, 15);
			parallelLabel.TabIndex = 66;
			parallelLabel.Text = "[parallel]";
			// 
			// zoomChildLabel
			// 
			zoomChildLabel.AutoSize = true;
			zoomChildLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			zoomChildLabel.ForeColor = System.Drawing.Color.White;
			zoomChildLabel.Location = new System.Drawing.Point(6, 520);
			zoomChildLabel.Margin = new Padding(4, 0, 4, 0);
			zoomChildLabel.Name = "zoomChildLabel";
			zoomChildLabel.Size = new System.Drawing.Size(73, 15);
			zoomChildLabel.TabIndex = 65;
			zoomChildLabel.Text = "[zoomChild]";
			// 
			// zoomChildBox
			// 
			zoomChildBox.Location = new System.Drawing.Point(201, 517);
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
			blurLabel.Location = new System.Drawing.Point(6, 491);
			blurLabel.Margin = new Padding(4, 0, 4, 0);
			blurLabel.Name = "blurLabel";
			blurLabel.Size = new System.Drawing.Size(36, 15);
			blurLabel.TabIndex = 63;
			blurLabel.Text = "[blur]";
			// 
			// brightnessLabel
			// 
			brightnessLabel.AutoSize = true;
			brightnessLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			brightnessLabel.ForeColor = System.Drawing.Color.White;
			brightnessLabel.Location = new System.Drawing.Point(6, 433);
			brightnessLabel.Margin = new Padding(4, 0, 4, 0);
			brightnessLabel.Name = "brightnessLabel";
			brightnessLabel.Size = new System.Drawing.Size(70, 15);
			brightnessLabel.TabIndex = 62;
			brightnessLabel.Text = "[brightness]";
			// 
			// detailLabel
			// 
			detailLabel.AutoSize = true;
			detailLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			detailLabel.ForeColor = System.Drawing.Color.White;
			detailLabel.Location = new System.Drawing.Point(6, 375);
			detailLabel.Margin = new Padding(4, 0, 4, 0);
			detailLabel.Name = "detailLabel";
			detailLabel.Size = new System.Drawing.Size(44, 15);
			detailLabel.TabIndex = 61;
			detailLabel.Text = "[detail]";
			// 
			// voidScaleLabel
			// 
			voidScaleLabel.AutoSize = true;
			voidScaleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			voidScaleLabel.ForeColor = System.Drawing.Color.White;
			voidScaleLabel.Location = new System.Drawing.Point(6, 346);
			voidScaleLabel.Margin = new Padding(4, 0, 4, 0);
			voidScaleLabel.Name = "voidScaleLabel";
			voidScaleLabel.Size = new System.Drawing.Size(65, 15);
			voidScaleLabel.TabIndex = 53;
			voidScaleLabel.Text = "[voidScale]";
			// 
			// voidNoiseLabel
			// 
			voidNoiseLabel.AutoSize = true;
			voidNoiseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			voidNoiseLabel.ForeColor = System.Drawing.Color.White;
			voidNoiseLabel.Location = new System.Drawing.Point(6, 317);
			voidNoiseLabel.Margin = new Padding(4, 0, 4, 0);
			voidNoiseLabel.Name = "voidNoiseLabel";
			voidNoiseLabel.Size = new System.Drawing.Size(68, 15);
			voidNoiseLabel.TabIndex = 49;
			voidNoiseLabel.Text = "[voidNoise]";
			// 
			// voidBox
			// 
			voidBox.Location = new System.Drawing.Point(201, 343);
			voidBox.Name = "voidBox";
			voidBox.Size = new System.Drawing.Size(59, 23);
			voidBox.TabIndex = 52;
			voidBox.Text = "8";
			voidBox.TextChanged += VoidBox_TextChanged;
			// 
			// editorPanel
			// 
			editorPanel.Controls.Add(fractalEditorPanel);
			editorPanel.Controls.Add(pointLabel2);
			editorPanel.Controls.Add(pointLabel1);
			editorPanel.Controls.Add(pointLabel0);
			editorPanel.Controls.Add(pointPanel);
			editorPanel.Location = new System.Drawing.Point(3, 670);
			editorPanel.Name = "editorPanel";
			editorPanel.Size = new System.Drawing.Size(286, 440);
			editorPanel.TabIndex = 56;
			// 
			// fractalEditorPanel
			// 
			fractalEditorPanel.Controls.Add(sizeLabel3);
			fractalEditorPanel.Controls.Add(maxBox);
			fractalEditorPanel.Controls.Add(removeColorButton);
			fractalEditorPanel.Controls.Add(sizeLabel2);
			fractalEditorPanel.Controls.Add(removeAngleButton);
			fractalEditorPanel.Controls.Add(removeCutButton);
			fractalEditorPanel.Controls.Add(addColorButton);
			fractalEditorPanel.Controls.Add(sizeLabel1);
			fractalEditorPanel.Controls.Add(sizeBox);
			fractalEditorPanel.Controls.Add(addAngleButton);
			fractalEditorPanel.Controls.Add(cutBox);
			fractalEditorPanel.Controls.Add(sizeLabel0);
			fractalEditorPanel.Controls.Add(colorBox);
			fractalEditorPanel.Controls.Add(minBox);
			fractalEditorPanel.Controls.Add(angleBox);
			fractalEditorPanel.Controls.Add(addCut);
			fractalEditorPanel.Controls.Add(addCutLabel);
			fractalEditorPanel.Location = new System.Drawing.Point(3, 294);
			fractalEditorPanel.Name = "fractalEditorPanel";
			fractalEditorPanel.Size = new System.Drawing.Size(280, 143);
			fractalEditorPanel.TabIndex = 69;
			// 
			// sizeLabel3
			// 
			sizeLabel3.AutoSize = true;
			sizeLabel3.ForeColor = System.Drawing.Color.White;
			sizeLabel3.Location = new System.Drawing.Point(205, 6);
			sizeLabel3.Margin = new Padding(4, 0, 4, 0);
			sizeLabel3.Name = "sizeLabel3";
			sizeLabel3.Size = new System.Drawing.Size(62, 15);
			sizeLabel3.TabIndex = 70;
			sizeLabel3.Text = "[size3_cut]";
			// 
			// maxBox
			// 
			maxBox.Location = new System.Drawing.Point(69, 24);
			maxBox.Name = "maxBox";
			maxBox.Size = new System.Drawing.Size(56, 23);
			maxBox.TabIndex = 66;
			maxBox.TextChanged += MaxBox_TextChanged;
			// 
			// removeColorButton
			// 
			removeColorButton.Location = new System.Drawing.Point(238, 82);
			removeColorButton.Name = "removeColorButton";
			removeColorButton.Size = new System.Drawing.Size(21, 23);
			removeColorButton.TabIndex = 63;
			removeColorButton.Text = "X";
			removeColorButton.UseVisualStyleBackColor = true;
			removeColorButton.Click += RemoveColorButton_Click;
			// 
			// sizeLabel2
			// 
			sizeLabel2.AutoSize = true;
			sizeLabel2.ForeColor = System.Drawing.Color.White;
			sizeLabel2.Location = new System.Drawing.Point(139, 6);
			sizeLabel2.Margin = new Padding(4, 0, 4, 0);
			sizeLabel2.Name = "sizeLabel2";
			sizeLabel2.Size = new System.Drawing.Size(66, 15);
			sizeLabel2.TabIndex = 69;
			sizeLabel2.Text = "[size2_min]";
			// 
			// removeAngleButton
			// 
			removeAngleButton.Location = new System.Drawing.Point(238, 53);
			removeAngleButton.Name = "removeAngleButton";
			removeAngleButton.Size = new System.Drawing.Size(21, 23);
			removeAngleButton.TabIndex = 62;
			removeAngleButton.Text = "X";
			removeAngleButton.UseVisualStyleBackColor = true;
			removeAngleButton.Click += RemoveAngleButton_Click;
			// 
			// removeCutButton
			// 
			removeCutButton.Location = new System.Drawing.Point(238, 111);
			removeCutButton.Name = "removeCutButton";
			removeCutButton.Size = new System.Drawing.Size(21, 23);
			removeCutButton.TabIndex = 69;
			removeCutButton.Text = "X";
			removeCutButton.UseVisualStyleBackColor = true;
			removeCutButton.Click += RemoveCutButton_Click;
			// 
			// addColorButton
			// 
			addColorButton.Location = new System.Drawing.Point(110, 82);
			addColorButton.Name = "addColorButton";
			addColorButton.Size = new System.Drawing.Size(122, 23);
			addColorButton.TabIndex = 61;
			addColorButton.Text = "[ AddColor ]";
			addColorButton.UseVisualStyleBackColor = true;
			addColorButton.Click += AddColorButton_Click;
			// 
			// sizeLabel1
			// 
			sizeLabel1.AutoSize = true;
			sizeLabel1.ForeColor = System.Drawing.Color.White;
			sizeLabel1.Location = new System.Drawing.Point(72, 6);
			sizeLabel1.Margin = new Padding(4, 0, 4, 0);
			sizeLabel1.Name = "sizeLabel1";
			sizeLabel1.Size = new System.Drawing.Size(68, 15);
			sizeLabel1.TabIndex = 68;
			sizeLabel1.Text = "[size1_max]";
			// 
			// sizeBox
			// 
			sizeBox.Location = new System.Drawing.Point(3, 24);
			sizeBox.Name = "sizeBox";
			sizeBox.Size = new System.Drawing.Size(56, 23);
			sizeBox.TabIndex = 57;
			sizeBox.TextChanged += SizeBox_TextChanged;
			// 
			// addAngleButton
			// 
			addAngleButton.Location = new System.Drawing.Point(110, 53);
			addAngleButton.Name = "addAngleButton";
			addAngleButton.Size = new System.Drawing.Size(122, 23);
			addAngleButton.TabIndex = 60;
			addAngleButton.Text = "[ AddAngle ]";
			addAngleButton.UseVisualStyleBackColor = true;
			addAngleButton.Click += AddAngleButton_Click;
			// 
			// cutBox
			// 
			cutBox.Location = new System.Drawing.Point(203, 24);
			cutBox.Name = "cutBox";
			cutBox.Size = new System.Drawing.Size(56, 23);
			cutBox.TabIndex = 64;
			cutBox.TextChanged += CutBox_TextChanged;
			// 
			// sizeLabel0
			// 
			sizeLabel0.AutoSize = true;
			sizeLabel0.ForeColor = System.Drawing.Color.White;
			sizeLabel0.Location = new System.Drawing.Point(5, 6);
			sizeLabel0.Margin = new Padding(4, 0, 4, 0);
			sizeLabel0.Name = "sizeLabel0";
			sizeLabel0.Size = new System.Drawing.Size(71, 15);
			sizeLabel0.TabIndex = 65;
			sizeLabel0.Text = "[size0_child]";
			// 
			// colorBox
			// 
			colorBox.Location = new System.Drawing.Point(3, 82);
			colorBox.Name = "colorBox";
			colorBox.Size = new System.Drawing.Size(101, 23);
			colorBox.TabIndex = 68;
			colorBox.Text = "[color]";
			// 
			// minBox
			// 
			minBox.Location = new System.Drawing.Point(137, 24);
			minBox.Name = "minBox";
			minBox.Size = new System.Drawing.Size(56, 23);
			minBox.TabIndex = 65;
			minBox.TextChanged += MinBox_TextChanged;
			// 
			// angleBox
			// 
			angleBox.Location = new System.Drawing.Point(3, 54);
			angleBox.Name = "angleBox";
			angleBox.Size = new System.Drawing.Size(101, 23);
			angleBox.TabIndex = 52;
			angleBox.Text = "[angle]";
			// 
			// addCut
			// 
			addCut.FormattingEnabled = true;
			addCut.Location = new System.Drawing.Point(110, 111);
			addCut.Margin = new Padding(4, 3, 4, 3);
			addCut.Name = "addCut";
			addCut.Size = new System.Drawing.Size(122, 23);
			addCut.TabIndex = 67;
			addCut.Text = "[addCut]";
			addCut.SelectedIndexChanged += AddCut_SelectedIndexChanged;
			// 
			// addCutLabel
			// 
			addCutLabel.AutoSize = true;
			addCutLabel.ForeColor = System.Drawing.Color.White;
			addCutLabel.Location = new System.Drawing.Point(3, 114);
			addCutLabel.Margin = new Padding(4, 0, 4, 0);
			addCutLabel.Name = "addCutLabel";
			addCutLabel.Size = new System.Drawing.Size(54, 15);
			addCutLabel.TabIndex = 57;
			addCutLabel.Text = "[addCut]";
			// 
			// pointLabel2
			// 
			pointLabel2.AutoSize = true;
			pointLabel2.ForeColor = System.Drawing.Color.White;
			pointLabel2.Location = new System.Drawing.Point(141, 6);
			pointLabel2.Margin = new Padding(4, 0, 4, 0);
			pointLabel2.Name = "pointLabel2";
			pointLabel2.Size = new System.Drawing.Size(83, 15);
			pointLabel2.TabIndex = 71;
			pointLabel2.Text = "[point2_angle]";
			pointLabel2.Click += pointLabel2_Click;
			// 
			// pointLabel1
			// 
			pointLabel1.AutoSize = true;
			pointLabel1.ForeColor = System.Drawing.Color.White;
			pointLabel1.Location = new System.Drawing.Point(87, 6);
			pointLabel1.Margin = new Padding(4, 0, 4, 0);
			pointLabel1.Name = "pointLabel1";
			pointLabel1.Size = new System.Drawing.Size(14, 15);
			pointLabel1.TabIndex = 70;
			pointLabel1.Text = "Y";
			// 
			// pointLabel0
			// 
			pointLabel0.AutoSize = true;
			pointLabel0.ForeColor = System.Drawing.Color.White;
			pointLabel0.Location = new System.Drawing.Point(29, 6);
			pointLabel0.Margin = new Padding(4, 0, 4, 0);
			pointLabel0.Name = "pointLabel0";
			pointLabel0.Size = new System.Drawing.Size(14, 15);
			pointLabel0.TabIndex = 52;
			pointLabel0.Text = "X";
			// 
			// pointPanel
			// 
			pointPanel.AutoScroll = true;
			pointPanel.BackColor = System.Drawing.Color.Black;
			pointPanel.Controls.Add(addPointButton);
			pointPanel.Location = new System.Drawing.Point(3, 24);
			pointPanel.Name = "pointPanel";
			pointPanel.Size = new System.Drawing.Size(280, 264);
			pointPanel.TabIndex = 59;
			// 
			// addPointButton
			// 
			addPointButton.Location = new System.Drawing.Point(15, 11);
			addPointButton.Name = "addPointButton";
			addPointButton.Size = new System.Drawing.Size(238, 23);
			addPointButton.TabIndex = 64;
			addPointButton.Text = "[ AddPoint ]";
			addPointButton.UseVisualStyleBackColor = true;
			addPointButton.Click += AddPoint_Click;
			// 
			// preButton
			// 
			preButton.Location = new System.Drawing.Point(275, 3);
			preButton.Name = "preButton";
			preButton.Size = new System.Drawing.Size(200, 27);
			preButton.TabIndex = 58;
			preButton.Text = "[ PREVIEW MODE ]";
			preButton.UseVisualStyleBackColor = true;
			preButton.Click += PreButton_Click;
			// 
			// loadButton
			// 
			loadButton.Location = new System.Drawing.Point(3, 3);
			loadButton.Name = "loadButton";
			loadButton.Size = new System.Drawing.Size(130, 27);
			loadButton.TabIndex = 57;
			loadButton.Text = "[ LOAD ]";
			loadButton.UseVisualStyleBackColor = true;
			loadButton.Click += LoadButton_Click;
			// 
			// saveButton
			// 
			saveButton.Location = new System.Drawing.Point(139, 3);
			saveButton.Name = "saveButton";
			saveButton.Size = new System.Drawing.Size(130, 27);
			saveButton.TabIndex = 58;
			saveButton.Text = "[ SAVE ]";
			saveButton.UseVisualStyleBackColor = true;
			saveButton.Click += SaveButton_Click;
			// 
			// saveMp4
			// 
			saveMp4.DefaultExt = "mp4";
			saveMp4.Filter = "MP4 files (*.mp4)|*.mp4";
			saveMp4.RestoreDirectory = true;
			saveMp4.FileOk += SaveMp4_FileOk;
			// 
			// paletteSelect
			// 
			paletteSelect.DrawMode = DrawMode.OwnerDrawFixed;
			paletteSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			paletteSelect.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
			paletteSelect.FormattingEnabled = true;
			paletteSelect.Items.AddRange(new object[] { "Random" });
			paletteSelect.Location = new System.Drawing.Point(57, 148);
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
			paletteLabel.Location = new System.Drawing.Point(3, 151);
			paletteLabel.Margin = new Padding(4, 0, 4, 0);
			paletteLabel.Name = "paletteLabel";
			paletteLabel.Size = new System.Drawing.Size(51, 15);
			paletteLabel.TabIndex = 58;
			paletteLabel.Text = "[palette]";
			// 
			// addPalette
			// 
			addPalette.Location = new System.Drawing.Point(218, 148);
			addPalette.Name = "addPalette";
			addPalette.Size = new System.Drawing.Size(18, 23);
			addPalette.TabIndex = 59;
			addPalette.Text = "+";
			addPalette.UseVisualStyleBackColor = true;
			addPalette.Click += AddPalette_Click;
			// 
			// removePalette
			// 
			removePalette.Location = new System.Drawing.Point(194, 148);
			removePalette.Name = "removePalette";
			removePalette.Size = new System.Drawing.Size(18, 23);
			removePalette.TabIndex = 60;
			removePalette.Text = "X";
			removePalette.UseVisualStyleBackColor = true;
			removePalette.Click += RemovePalette_Click;
			// 
			// debugsLabel
			// 
			debugsLabel.AutoSize = true;
			debugsLabel.ForeColor = System.Drawing.Color.White;
			debugsLabel.Location = new System.Drawing.Point(3, 6);
			debugsLabel.Name = "debugsLabel";
			debugsLabel.Size = new System.Drawing.Size(54, 15);
			debugsLabel.TabIndex = 61;
			debugsLabel.Text = "[debugs]";
			// 
			// animBox
			// 
			animBox.AutoSize = true;
			animBox.ForeColor = System.Drawing.Color.White;
			animBox.Location = new System.Drawing.Point(113, 5);
			animBox.Name = "animBox";
			animBox.Size = new System.Drawing.Size(61, 19);
			animBox.TabIndex = 62;
			animBox.Text = "[anim]";
			animBox.UseVisualStyleBackColor = true;
			animBox.CheckedChanged += DebugAnimBox_CheckedChanged;
			// 
			// pngBox
			// 
			pngBox.AutoSize = true;
			pngBox.ForeColor = System.Drawing.Color.White;
			pngBox.Location = new System.Drawing.Point(174, 5);
			pngBox.Name = "pngBox";
			pngBox.Size = new System.Drawing.Size(55, 19);
			pngBox.TabIndex = 63;
			pngBox.Text = "[png]";
			pngBox.UseVisualStyleBackColor = true;
			pngBox.CheckedChanged += DebugPngBox_CheckedChanged;
			// 
			// gifBox
			// 
			gifBox.AutoSize = true;
			gifBox.ForeColor = System.Drawing.Color.White;
			gifBox.Location = new System.Drawing.Point(230, 5);
			gifBox.Name = "gifBox";
			gifBox.Size = new System.Drawing.Size(48, 19);
			gifBox.TabIndex = 64;
			gifBox.Text = "[gif]";
			gifBox.UseVisualStyleBackColor = true;
			gifBox.CheckedChanged += DebugGifBox_CheckedChanged;
			// 
			// loadExport
			// 
			loadExport.Filter = "All files (*.*)|*.*";
			loadExport.RestoreDirectory = true;
			loadExport.FileOk += LoadExport_FileOk;
			// 
			// removeResolution
			// 
			removeResolution.Location = new System.Drawing.Point(241, 118);
			removeResolution.Name = "removeResolution";
			removeResolution.Size = new System.Drawing.Size(21, 23);
			removeResolution.TabIndex = 65;
			removeResolution.Text = "X";
			removeResolution.UseVisualStyleBackColor = true;
			removeResolution.Click += RemoveResolution_Click;
			// 
			// addResolution
			// 
			addResolution.Location = new System.Drawing.Point(267, 118);
			addResolution.Name = "addResolution";
			addResolution.Size = new System.Drawing.Size(21, 23);
			addResolution.TabIndex = 66;
			addResolution.Text = "+";
			addResolution.UseVisualStyleBackColor = true;
			addResolution.Click += AddResolution_Click;
			// 
			// toolGenPanel
			// 
			toolGenPanel.Controls.Add(frameBox);
			toolGenPanel.Controls.Add(hideButton);
			toolGenPanel.Controls.Add(prevButton);
			toolGenPanel.Controls.Add(animateButton);
			toolGenPanel.Controls.Add(infoLabel);
			toolGenPanel.Controls.Add(statusLabel);
			toolGenPanel.Controls.Add(nextButton);
			toolGenPanel.Controls.Add(restartButton);
			toolGenPanel.Location = new System.Drawing.Point(12, 12);
			toolGenPanel.MinimumSize = new System.Drawing.Size(480, 32);
			toolGenPanel.Name = "toolGenPanel";
			toolGenPanel.Size = new System.Drawing.Size(480, 32);
			toolGenPanel.TabIndex = 67;
			// 
			// frameBox
			// 
			frameBox.Location = new System.Drawing.Point(127, 5);
			frameBox.Name = "frameBox";
			frameBox.Size = new System.Drawing.Size(69, 23);
			frameBox.TabIndex = 68;
			frameBox.Text = "0";
			frameBox.TextChanged += FrameBox_TextChanged;
			// 
			// hideButton
			// 
			hideButton.Location = new System.Drawing.Point(393, 3);
			hideButton.Margin = new Padding(4, 3, 4, 3);
			hideButton.Name = "hideButton";
			hideButton.Size = new System.Drawing.Size(83, 27);
			hideButton.TabIndex = 50;
			hideButton.Text = "[ Hide ]";
			hideButton.UseVisualStyleBackColor = true;
			hideButton.Click += HideButton_Click;
			// 
			// popupPanel
			// 
			popupPanel.Controls.Add(debugPanel);
			popupPanel.Controls.Add(fractalSelect);
			popupPanel.Controls.Add(defaultHue);
			popupPanel.Controls.Add(editorPanel);
			popupPanel.Controls.Add(addResolution);
			popupPanel.Controls.Add(resY);
			popupPanel.Controls.Add(removeResolution);
			popupPanel.Controls.Add(resX);
			popupPanel.Controls.Add(fractalLabel);
			popupPanel.Controls.Add(cutparamBox);
			popupPanel.Controls.Add(cutSelect);
			popupPanel.Controls.Add(cutLabel);
			popupPanel.Controls.Add(removePalette);
			popupPanel.Controls.Add(angleSelect);
			popupPanel.Controls.Add(addPalette);
			popupPanel.Controls.Add(angleLabel);
			popupPanel.Controls.Add(paletteLabel);
			popupPanel.Controls.Add(colorSelect);
			popupPanel.Controls.Add(paletteSelect);
			popupPanel.Controls.Add(colorLabel);
			popupPanel.Controls.Add(resSelect);
			popupPanel.Controls.Add(generatorPanel);
			popupPanel.Location = new System.Drawing.Point(12, 88);
			popupPanel.Name = "popupPanel";
			popupPanel.Size = new System.Drawing.Size(293, 1117);
			popupPanel.TabIndex = 68;
			// 
			// debugPanel
			// 
			debugPanel.AutoScroll = true;
			debugPanel.Controls.Add(debugsLabel);
			debugPanel.Controls.Add(debugLabel);
			debugPanel.Controls.Add(debugBox);
			debugPanel.Controls.Add(animBox);
			debugPanel.Controls.Add(pngBox);
			debugPanel.Controls.Add(gifBox);
			debugPanel.Location = new System.Drawing.Point(3, 618);
			debugPanel.Name = "debugPanel";
			debugPanel.Size = new System.Drawing.Size(286, 49);
			debugPanel.TabIndex = 70;
			// 
			// toolEditPanel
			// 
			toolEditPanel.Controls.Add(loadButton);
			toolEditPanel.Controls.Add(saveButton);
			toolEditPanel.Controls.Add(preButton);
			toolEditPanel.Location = new System.Drawing.Point(12, 50);
			toolEditPanel.MinimumSize = new System.Drawing.Size(480, 32);
			toolEditPanel.Name = "toolEditPanel";
			toolEditPanel.Size = new System.Drawing.Size(480, 32);
			toolEditPanel.TabIndex = 69;
			// 
			// GeneratorForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
			ClientSize = new System.Drawing.Size(500, 1216);
			Controls.Add(toolEditPanel);
			Controls.Add(popupPanel);
			Controls.Add(toolGenPanel);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(4, 3, 4, 3);
			MinimumSize = new System.Drawing.Size(512, 480);
			Name = "GeneratorForm";
			Text = "[ GeneratorForm ]";
			FormClosing += GeneratorForm_FormClosing;
			MouseEnter += GeneratorForm_MouseEnter;
			MouseLeave += GeneratorForm_MouseLeave;
			MouseMove += GeneratorForm_MouseMove;
			generatorPanel.ResumeLayout(false);
			genControlPanel.ResumeLayout(false);
			genControlPanel.PerformLayout();
			fractalSettingsPanel.ResumeLayout(false);
			fractalSettingsPanel.PerformLayout();
			editorPanel.ResumeLayout(false);
			editorPanel.PerformLayout();
			fractalEditorPanel.ResumeLayout(false);
			fractalEditorPanel.PerformLayout();
			pointPanel.ResumeLayout(false);
			toolGenPanel.ResumeLayout(false);
			toolGenPanel.PerformLayout();
			popupPanel.ResumeLayout(false);
			popupPanel.PerformLayout();
			debugPanel.ResumeLayout(false);
			debugPanel.PerformLayout();
			toolEditPanel.ResumeLayout(false);
			ResumeLayout(false);
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
		private System.Windows.Forms.TextBox timingBox;
		private System.Windows.Forms.Button prevButton;
		private System.Windows.Forms.Button nextButton;
		private System.Windows.Forms.Button animateButton;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.Button exportButton;
		private System.Windows.Forms.Label saturateLabel;
		private System.Windows.Forms.Label voidAmbientLabel;
		private System.Windows.Forms.Label bloomLabel;
		private System.Windows.Forms.TextBox defaultZoomBox;
		private System.Windows.Forms.TextBox defaultAngleBox;
		private System.Windows.Forms.TextBox defaultHue;
		private System.Windows.Forms.TextBox periodMultiplierBox;
		private System.Windows.Forms.Label periodLabel;
		private System.Windows.Forms.ComboBox spinSelect;
		private System.Windows.Forms.Label zoomLabel;
		private System.Windows.Forms.ComboBox hueSelect;
		private System.Windows.Forms.Label spinLabel0;
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
		private System.Windows.Forms.TextBox abortBox;
		private System.Windows.Forms.TextBox brightnessBox;
		private System.Windows.Forms.ComboBox zoomSelect;
		private System.Windows.Forms.Button restartButton;
		private System.Windows.Forms.ComboBox generationSelect;
		private System.Windows.Forms.CheckBox debugBox;
		private System.Windows.Forms.Label debugLabel;
		private System.Windows.Forms.Panel generatorPanel;
		private System.Windows.Forms.Panel editorPanel;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Button loadButton;
		private System.Windows.Forms.Panel pointPanel;
		private System.Windows.Forms.Button addColorButton;
		private System.Windows.Forms.Button addAngleButton;
		private System.Windows.Forms.Button removeColorButton;
		private System.Windows.Forms.Button removeAngleButton;
		private System.Windows.Forms.Button addPointButton;
		private System.Windows.Forms.TextBox sizeBox;
		private System.Windows.Forms.TextBox cutBox;
		private System.Windows.Forms.TextBox minBox;
		private System.Windows.Forms.TextBox maxBox;
		private System.Windows.Forms.ComboBox addCut;
		private System.Windows.Forms.Label addCutLabel;
		private System.Windows.Forms.TextBox angleBox;
		private System.Windows.Forms.TextBox colorBox;
		private System.Windows.Forms.Label pointLabel0;
		private System.Windows.Forms.TextBox voidBox;
		private System.Windows.Forms.Label sizeLabel0;
		private System.Windows.Forms.SaveFileDialog saveMp4;
		private System.Windows.Forms.Button preButton;
		private System.Windows.Forms.Panel fractalSettingsPanel;
		private System.Windows.Forms.Label voidNoiseLabel;
		private System.Windows.Forms.Label detailLabel;
		private System.Windows.Forms.Label voidScaleLabel;
		private System.Windows.Forms.Label brightnessLabel;
		private System.Windows.Forms.Label blurLabel;
		private System.Windows.Forms.Label parallelLabel;
		private System.Windows.Forms.Label zoomChildLabel;
		private System.Windows.Forms.TextBox zoomChildBox;
		private System.Windows.Forms.Label abortDelayLabel;
		private System.Windows.Forms.Label timingLabel;
		private System.Windows.Forms.ComboBox paletteSelect;
		private System.Windows.Forms.Label paletteLabel;
		private System.Windows.Forms.Button addPalette;
		private System.Windows.Forms.Button removePalette;
		private System.Windows.Forms.ColorDialog paletteDialog;
		private ComboBox exportSelect;
		private ComboBox timingSelect;
		private Label generationModeLabel;
		private Label encodePngLabel;
		private ComboBox encodePngSelect;
		private Label encodeGifLabel;
		private ComboBox encodeGifSelect;
		private Label debugsLabel;
		private CheckBox animBox;
		private CheckBox pngBox;
		private CheckBox gifBox;
		private CheckBox ditherBox;
		private Label ditherLabel;
		private Label fileLabel;
		private ComboBox fileSelect;
		private OpenFileDialog loadExport;
		private TextBox stripeBox;
		private TextBox binBox;
		private ComboBox voidSelect;
		private ComboBox drawSelect;
		private Label gpuDLabel;
		private Label gpuVLabel;
		private Label stripeLabel;
		private Label binLabel;
		private Label l2Label;
		private TextBox l2Box;
		private Button removeCutButton;
		private Button removeResolution;
		private Button addResolution;
		private TextBox nameBox;
		private Label genNameLabel;
		private Panel toolGenPanel;
		private Button hideButton;
		private TextBox frameBox;
		private Label spinLabel2;
		private Label spinLabel1;
		private Label pointLabel2;
		private Label pointLabel1;
		private Label sizeLabel3;
		private Label sizeLabel2;
		private Label sizeLabel1;
		private Button tasksButton;
		private Panel genControlPanel;
		private Panel popupPanel;
		private Panel fractalEditorPanel;
		private Panel toolEditPanel;
		private Panel debugPanel;
	}
}

