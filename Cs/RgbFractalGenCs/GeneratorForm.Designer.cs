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
			generationSelect = new ComboBox();
			debugBox = new CheckBox();
			debugLabel = new Label();
			generatorPanel = new Panel();
			fileLabel = new Label();
			exportSelect = new ComboBox();
			fileSelect = new ComboBox();
			panel1 = new Panel();
			gpuDLabel = new Label();
			gpuVLabel = new Label();
			stripeLabel = new Label();
			binLabel = new Label();
			cacheLabel = new Label();
			cacheBox = new CheckBox();
			stripeBox = new TextBox();
			binBox = new TextBox();
			voidSelect = new ComboBox();
			drawSelect = new ComboBox();
			ditherBox = new CheckBox();
			ditherLabel = new Label();
			updateBatchButton = new Button();
			encodePngLabel = new Label();
			encodePngSelect = new ComboBox();
			encodeGifLabel = new Label();
			encodeGifSelect = new ComboBox();
			generationModeLabel = new Label();
			batchBox = new TextBox();
			batchLabel = new Label();
			saveBatchButton = new Button();
			loadBatchButton = new Button();
			addBatchButton = new Button();
			runBatchButton = new Button();
			timingSelect = new ComboBox();
			label2 = new Label();
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
			paletteSelect = new ComboBox();
			paletteLabel = new Label();
			addPalette = new Button();
			removePalette = new Button();
			paletteDialog = new ColorDialog();
			loadBatch = new OpenFileDialog();
			saveBatch = new SaveFileDialog();
			runBatch = new SaveFileDialog();
			debugsLabel = new Label();
			debugAnimBox = new CheckBox();
			debugPngBox = new CheckBox();
			debugGifBox = new CheckBox();
			loadExport = new OpenFileDialog();
			l2Box = new TextBox();
			l2Label = new Label();
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
			cutparamBox.TextChanged += CutSeedBox_TextChanged;
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
			resSelect.Items.AddRange(new object[] { "80x80", "Custom:" });
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
			periodBox.Location = new System.Drawing.Point(72, 38);
			periodBox.Margin = new Padding(4, 3, 4, 3);
			periodBox.Name = "periodBox";
			periodBox.Size = new System.Drawing.Size(86, 23);
			periodBox.TabIndex = 10;
			periodBox.Text = "120";
			periodBox.TextChanged += PeriodBox_TextChanged;
			// 
			// timingBox
			// 
			timingBox.Location = new System.Drawing.Point(164, 465);
			timingBox.Margin = new Padding(4, 3, 4, 3);
			timingBox.Name = "timingBox";
			timingBox.Size = new System.Drawing.Size(59, 23);
			timingBox.TabIndex = 28;
			timingBox.Text = "60";
			timingBox.TextChanged += TimingBox_TextChanged;
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
			// exportButton
			// 
			exportButton.Location = new System.Drawing.Point(62, 378);
			exportButton.Margin = new Padding(4, 3, 4, 3);
			exportButton.Name = "exportButton";
			exportButton.Size = new System.Drawing.Size(92, 27);
			exportButton.TabIndex = 34;
			exportButton.Text = "Export:";
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
			voidAmbientLabel.Location = new System.Drawing.Point(4, 178);
			voidAmbientLabel.Margin = new Padding(4, 0, 4, 0);
			voidAmbientLabel.Name = "voidAmbientLabel";
			voidAmbientLabel.Size = new System.Drawing.Size(116, 15);
			voidAmbientLabel.TabIndex = 0;
			voidAmbientLabel.Text = "Void Ambient (0-30):";
			// 
			// saturateLabel
			// 
			saturateLabel.AutoSize = true;
			saturateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			saturateLabel.ForeColor = System.Drawing.Color.White;
			saturateLabel.Location = new System.Drawing.Point(4, 294);
			saturateLabel.Margin = new Padding(4, 0, 4, 0);
			saturateLabel.Name = "saturateLabel";
			saturateLabel.Size = new System.Drawing.Size(87, 15);
			saturateLabel.TabIndex = 0;
			saturateLabel.Text = "Saturate (0-10):";
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
			infoLabel.Location = new System.Drawing.Point(129, 352);
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
			bloomLabel.Location = new System.Drawing.Point(4, 352);
			bloomLabel.Margin = new Padding(4, 0, 4, 0);
			bloomLabel.Name = "bloomLabel";
			bloomLabel.Size = new System.Drawing.Size(79, 15);
			bloomLabel.TabIndex = 0;
			bloomLabel.Text = "Bloom (0-40):";
			// 
			// defaultZoomBox
			// 
			defaultZoomBox.Location = new System.Drawing.Point(164, 67);
			defaultZoomBox.Name = "defaultZoomBox";
			defaultZoomBox.Size = new System.Drawing.Size(59, 23);
			defaultZoomBox.TabIndex = 13;
			defaultZoomBox.Text = "0";
			defaultZoomBox.TextChanged += DefaultZoom_TextChanged;
			// 
			// defaultAngleBox
			// 
			defaultAngleBox.Location = new System.Drawing.Point(98, 146);
			defaultAngleBox.Name = "defaultAngleBox";
			defaultAngleBox.Size = new System.Drawing.Size(60, 23);
			defaultAngleBox.TabIndex = 16;
			defaultAngleBox.Text = "0";
			defaultAngleBox.TextChanged += DefaultAngle_TextChanged;
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
			periodMultiplierBox.Location = new System.Drawing.Point(164, 38);
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
			periodLabel.Location = new System.Drawing.Point(4, 41);
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
			helpPanel.BackColor = System.Drawing.Color.Black;
			helpPanel.Controls.Add(helpLabel);
			helpPanel.Location = new System.Drawing.Point(309, 14);
			helpPanel.Name = "helpPanel";
			helpPanel.Size = new System.Drawing.Size(763, 586);
			helpPanel.TabIndex = 0;
			// 
			// helpLabel
			// 
			helpLabel.AutoSize = true;
			helpLabel.ForeColor = System.Drawing.Color.White;
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
			spinSelect.Items.AddRange(new object[] { "Random", "Clock", "None", "Counterclock", "Antispin", "Peri Antispin" });
			spinSelect.Location = new System.Drawing.Point(4, 146);
			spinSelect.Name = "spinSelect";
			spinSelect.Size = new System.Drawing.Size(86, 23);
			spinSelect.TabIndex = 14;
			spinSelect.SelectedIndexChanged += SpinSelect_SelectedIndexChanged;
			// 
			// zoomLabel
			// 
			zoomLabel.AutoSize = true;
			zoomLabel.ForeColor = System.Drawing.Color.White;
			zoomLabel.Location = new System.Drawing.Point(4, 70);
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
			hueSelect.Location = new System.Drawing.Point(72, 96);
			hueSelect.Name = "hueSelect";
			hueSelect.Size = new System.Drawing.Size(86, 23);
			hueSelect.TabIndex = 17;
			hueSelect.SelectedIndexChanged += HueSelect_SelectedIndexChanged;
			// 
			// spinLabel
			// 
			spinLabel.AutoSize = true;
			spinLabel.ForeColor = System.Drawing.Color.White;
			spinLabel.Location = new System.Drawing.Point(4, 128);
			spinLabel.Name = "spinLabel";
			spinLabel.Size = new System.Drawing.Size(201, 15);
			spinLabel.TabIndex = 41;
			spinLabel.Text = "Spin direction         Default        Speed";
			// 
			// hueLabel
			// 
			hueLabel.AutoSize = true;
			hueLabel.ForeColor = System.Drawing.Color.White;
			hueLabel.Location = new System.Drawing.Point(4, 99);
			hueLabel.Name = "hueLabel";
			hueLabel.Size = new System.Drawing.Size(59, 15);
			hueLabel.TabIndex = 42;
			hueLabel.Text = "Hue Shift:";
			// 
			// spinSpeedBox
			// 
			spinSpeedBox.Location = new System.Drawing.Point(164, 146);
			spinSpeedBox.Name = "spinSpeedBox";
			spinSpeedBox.Size = new System.Drawing.Size(59, 23);
			spinSpeedBox.TabIndex = 43;
			spinSpeedBox.Text = "0";
			spinSpeedBox.TextChanged += SpinSpeedBox_TextChanged;
			// 
			// hueSpeedBox
			// 
			hueSpeedBox.Location = new System.Drawing.Point(164, 96);
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
			parallelTypeSelect.Location = new System.Drawing.Point(59, 494);
			parallelTypeSelect.Name = "parallelTypeSelect";
			parallelTypeSelect.Size = new System.Drawing.Size(99, 23);
			parallelTypeSelect.TabIndex = 27;
			parallelTypeSelect.SelectedIndexChanged += ParallelTypeSelect_SelectedIndexChanged;
			// 
			// ambBox
			// 
			ambBox.Location = new System.Drawing.Point(164, 175);
			ambBox.Name = "ambBox";
			ambBox.Size = new System.Drawing.Size(59, 23);
			ambBox.TabIndex = 20;
			ambBox.Text = "20";
			ambBox.TextChanged += AmbBox_TextChanged;
			// 
			// noiseBox
			// 
			noiseBox.Location = new System.Drawing.Point(164, 204);
			noiseBox.Name = "noiseBox";
			noiseBox.Size = new System.Drawing.Size(59, 23);
			noiseBox.TabIndex = 21;
			noiseBox.Text = "20";
			noiseBox.TextChanged += NoiseBox_TextChanged;
			// 
			// saturateBox
			// 
			saturateBox.Location = new System.Drawing.Point(164, 291);
			saturateBox.Name = "saturateBox";
			saturateBox.Size = new System.Drawing.Size(59, 23);
			saturateBox.TabIndex = 22;
			saturateBox.Text = "100";
			saturateBox.TextChanged += SaturateBox_TextChanged;
			// 
			// detailBox
			// 
			detailBox.Location = new System.Drawing.Point(164, 262);
			detailBox.Name = "detailBox";
			detailBox.Size = new System.Drawing.Size(59, 23);
			detailBox.TabIndex = 23;
			detailBox.Text = "3";
			detailBox.TextChanged += DetailBox_TextChanged;
			// 
			// blurBox
			// 
			blurBox.Location = new System.Drawing.Point(164, 378);
			blurBox.Name = "blurBox";
			blurBox.Size = new System.Drawing.Size(59, 23);
			blurBox.TabIndex = 24;
			blurBox.Text = "0";
			blurBox.TextChanged += BlurBox_TextChanged;
			// 
			// bloomBox
			// 
			bloomBox.Location = new System.Drawing.Point(164, 349);
			bloomBox.Name = "bloomBox";
			bloomBox.Size = new System.Drawing.Size(59, 23);
			bloomBox.TabIndex = 25;
			bloomBox.Text = "10";
			bloomBox.TextChanged += BloomBox_TextChanged;
			// 
			// threadsBox
			// 
			threadsBox.Location = new System.Drawing.Point(164, 494);
			threadsBox.Name = "threadsBox";
			threadsBox.Size = new System.Drawing.Size(59, 23);
			threadsBox.TabIndex = 28;
			threadsBox.Text = "0";
			threadsBox.TextChanged += Parallel_Changed;
			// 
			// abortBox
			// 
			abortBox.Location = new System.Drawing.Point(164, 436);
			abortBox.Margin = new Padding(4, 3, 4, 3);
			abortBox.Name = "abortBox";
			abortBox.Size = new System.Drawing.Size(59, 23);
			abortBox.TabIndex = 45;
			abortBox.Text = "50";
			abortBox.TextChanged += AbortBox_TextChanged;
			// 
			// brightnessBox
			// 
			brightnessBox.Location = new System.Drawing.Point(164, 320);
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
			zoomSelect.Location = new System.Drawing.Point(72, 67);
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
			// generationSelect
			// 
			generationSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			generationSelect.FormattingEnabled = true;
			generationSelect.Items.AddRange(new object[] { "Only Image", "Animation", "All Seeds" });
			generationSelect.Location = new System.Drawing.Point(115, 750);
			generationSelect.Name = "generationSelect";
			generationSelect.Size = new System.Drawing.Size(108, 23);
			generationSelect.TabIndex = 51;
			generationSelect.SelectedIndexChanged += GenerationSelect_SelectedIndexChanged;
			// 
			// debugBox
			// 
			debugBox.AutoSize = true;
			debugBox.ForeColor = System.Drawing.Color.White;
			debugBox.Location = new System.Drawing.Point(69, 636);
			debugBox.Name = "debugBox";
			debugBox.Size = new System.Drawing.Size(53, 19);
			debugBox.TabIndex = 52;
			debugBox.Text = "Tasks";
			debugBox.UseVisualStyleBackColor = true;
			debugBox.CheckedChanged += DebugBox_CheckedChanged;
			// 
			// debugLabel
			// 
			debugLabel.AutoSize = true;
			debugLabel.ForeColor = System.Drawing.Color.White;
			debugLabel.Location = new System.Drawing.Point(17, 658);
			debugLabel.Name = "debugLabel";
			debugLabel.Size = new System.Drawing.Size(73, 15);
			debugLabel.TabIndex = 53;
			debugLabel.Text = "DebugString";
			// 
			// generatorPanel
			// 
			generatorPanel.Controls.Add(fileLabel);
			generatorPanel.Controls.Add(exportSelect);
			generatorPanel.Controls.Add(fileSelect);
			generatorPanel.Controls.Add(panel1);
			generatorPanel.Controls.Add(exportButton);
			generatorPanel.Controls.Add(infoLabel);
			generatorPanel.Controls.Add(statusLabel);
			generatorPanel.Controls.Add(restartButton);
			generatorPanel.Controls.Add(animateButton);
			generatorPanel.Controls.Add(nextButton);
			generatorPanel.Controls.Add(prevButton);
			generatorPanel.Controls.Add(helpButton);
			generatorPanel.Location = new System.Drawing.Point(17, 187);
			generatorPanel.Name = "generatorPanel";
			generatorPanel.Size = new System.Drawing.Size(286, 440);
			generatorPanel.TabIndex = 54;
			// 
			// fileLabel
			// 
			fileLabel.AutoSize = true;
			fileLabel.ForeColor = System.Drawing.Color.White;
			fileLabel.Location = new System.Drawing.Point(14, 413);
			fileLabel.Margin = new Padding(4, 0, 4, 0);
			fileLabel.Name = "fileLabel";
			fileLabel.Size = new System.Drawing.Size(28, 15);
			fileLabel.TabIndex = 66;
			fileLabel.Text = "File:";
			// 
			// exportSelect
			// 
			exportSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			exportSelect.FormattingEnabled = true;
			exportSelect.Items.AddRange(new object[] { "Current PNG", "PNGs", "MP4", "Selected GIF", "GIF->MP4", "Load Export" });
			exportSelect.Location = new System.Drawing.Point(162, 381);
			exportSelect.Name = "exportSelect";
			exportSelect.Size = new System.Drawing.Size(108, 23);
			exportSelect.TabIndex = 61;
			exportSelect.SelectedIndexChanged += ExportSelect_SelectedIndexChanged;
			// 
			// fileSelect
			// 
			fileSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			fileSelect.FormattingEnabled = true;
			fileSelect.Items.AddRange(new object[] { "Fractal", "Angles - (Bitmask)", "Colors - (Bitmask)", "Function - (Type_Seed)", "Resolution - (W_H)", "Hues - (Palette_Hue_Shift_Speed)", "Period - (Frames_Multiplier)", "Zoom - (Direction_Child)", "Spin - (Direction_Default_Speed)", "Void - (Ambient_Noise_Scale)", "Image - (Satur_Bright_Bloom_Blur)", "Detail - (Dithering_Detail)" });
			fileSelect.Location = new System.Drawing.Point(62, 410);
			fileSelect.Margin = new Padding(4, 3, 4, 3);
			fileSelect.Name = "fileSelect";
			fileSelect.Size = new System.Drawing.Size(209, 23);
			fileSelect.TabIndex = 65;
			fileSelect.SelectedIndexChanged += FileSelect_SelectedIndexChanged;
			// 
			// panel1
			// 
			panel1.AutoScroll = true;
			panel1.BackColor = System.Drawing.Color.Black;
			panel1.Controls.Add(l2Label);
			panel1.Controls.Add(l2Box);
			panel1.Controls.Add(gpuDLabel);
			panel1.Controls.Add(gpuVLabel);
			panel1.Controls.Add(stripeLabel);
			panel1.Controls.Add(binLabel);
			panel1.Controls.Add(cacheLabel);
			panel1.Controls.Add(cacheBox);
			panel1.Controls.Add(stripeBox);
			panel1.Controls.Add(binBox);
			panel1.Controls.Add(voidSelect);
			panel1.Controls.Add(drawSelect);
			panel1.Controls.Add(ditherBox);
			panel1.Controls.Add(ditherLabel);
			panel1.Controls.Add(updateBatchButton);
			panel1.Controls.Add(encodePngLabel);
			panel1.Controls.Add(encodePngSelect);
			panel1.Controls.Add(encodeGifLabel);
			panel1.Controls.Add(encodeGifSelect);
			panel1.Controls.Add(generationModeLabel);
			panel1.Controls.Add(batchBox);
			panel1.Controls.Add(batchLabel);
			panel1.Controls.Add(saveBatchButton);
			panel1.Controls.Add(loadBatchButton);
			panel1.Controls.Add(generationSelect);
			panel1.Controls.Add(addBatchButton);
			panel1.Controls.Add(runBatchButton);
			panel1.Controls.Add(timingSelect);
			panel1.Controls.Add(label2);
			panel1.Controls.Add(timingLabel);
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
			panel1.Controls.Add(timingBox);
			panel1.Controls.Add(saturateLabel);
			panel1.Controls.Add(threadsBox);
			panel1.Controls.Add(hueLabel);
			panel1.Controls.Add(periodLabel);
			panel1.Controls.Add(spinSelect);
			panel1.Controls.Add(blurBox);
			panel1.Controls.Add(spinSpeedBox);
			panel1.Controls.Add(defaultAngleBox);
			panel1.Controls.Add(parallelTypeSelect);
			panel1.Controls.Add(bloomBox);
			panel1.Controls.Add(hueSpeedBox);
			panel1.Controls.Add(periodMultiplierBox);
			panel1.Controls.Add(ambBox);
			panel1.Controls.Add(saturateBox);
			panel1.Controls.Add(voidAmbientLabel);
			panel1.Controls.Add(defaultZoomBox);
			panel1.Controls.Add(bloomLabel);
			panel1.Controls.Add(noiseBox);
			panel1.Controls.Add(detailBox);
			panel1.Location = new System.Drawing.Point(14, 10);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(256, 300);
			panel1.TabIndex = 60;
			// 
			// gpuDLabel
			// 
			gpuDLabel.AutoSize = true;
			gpuDLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			gpuDLabel.ForeColor = System.Drawing.Color.White;
			gpuDLabel.Location = new System.Drawing.Point(4, 666);
			gpuDLabel.Margin = new Padding(4, 0, 4, 0);
			gpuDLabel.Name = "gpuDLabel";
			gpuDLabel.Size = new System.Drawing.Size(63, 15);
			gpuDLabel.TabIndex = 94;
			gpuDLabel.Text = "GPU Draw:";
			// 
			// gpuVLabel
			// 
			gpuVLabel.AutoSize = true;
			gpuVLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			gpuVLabel.ForeColor = System.Drawing.Color.White;
			gpuVLabel.Location = new System.Drawing.Point(4, 637);
			gpuVLabel.Margin = new Padding(4, 0, 4, 0);
			gpuVLabel.Name = "gpuVLabel";
			gpuVLabel.Size = new System.Drawing.Size(59, 15);
			gpuVLabel.TabIndex = 93;
			gpuVLabel.Text = "GPU Void:";
			// 
			// stripeLabel
			// 
			stripeLabel.AutoSize = true;
			stripeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			stripeLabel.ForeColor = System.Drawing.Color.White;
			stripeLabel.Location = new System.Drawing.Point(4, 550);
			stripeLabel.Margin = new Padding(4, 0, 4, 0);
			stripeLabel.Name = "stripeLabel";
			stripeLabel.Size = new System.Drawing.Size(112, 15);
			stripeLabel.TabIndex = 92;
			stripeLabel.Text = "Stripe Height";
			// 
			// binLabel
			// 
			binLabel.AutoSize = true;
			binLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			binLabel.ForeColor = System.Drawing.Color.White;
			binLabel.Location = new System.Drawing.Point(4, 579);
			binLabel.Margin = new Padding(4, 0, 4, 0);
			binLabel.Name = "binLabel";
			binLabel.Size = new System.Drawing.Size(108, 15);
			binLabel.TabIndex = 91;
			binLabel.Text = "Bin Size:";
			// 
			// cacheLabel
			// 
			cacheLabel.AutoSize = true;
			cacheLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			cacheLabel.ForeColor = System.Drawing.Color.White;
			cacheLabel.Location = new System.Drawing.Point(4, 523);
			cacheLabel.Margin = new Padding(4, 0, 4, 0);
			cacheLabel.Name = "cacheLabel";
			cacheLabel.Size = new System.Drawing.Size(115, 15);
			cacheLabel.TabIndex = 90;
			cacheLabel.Text = "Cache Optimization:";
			// 
			// cacheBox
			// 
			cacheBox.AutoSize = true;
			cacheBox.Checked = true;
			cacheBox.CheckState = CheckState.Checked;
			cacheBox.ForeColor = System.Drawing.SystemColors.Control;
			cacheBox.Location = new System.Drawing.Point(155, 523);
			cacheBox.Name = "cacheBox";
			cacheBox.Size = new System.Drawing.Size(68, 19);
			cacheBox.TabIndex = 89;
			cacheBox.Text = "Enabled";
			cacheBox.UseVisualStyleBackColor = true;
			cacheBox.CheckedChanged += CacheBox_CheckedChanged;
			// 
			// stripeBox
			// 
			stripeBox.Location = new System.Drawing.Point(164, 547);
			stripeBox.Name = "stripeBox";
			stripeBox.Size = new System.Drawing.Size(59, 23);
			stripeBox.TabIndex = 88;
			stripeBox.Text = "0";
			stripeBox.TextChanged += StripeBox_TextChanged;
			// 
			// binBox
			// 
			binBox.Location = new System.Drawing.Point(164, 576);
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
			voidSelect.Location = new System.Drawing.Point(115, 634);
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
			drawSelect.Location = new System.Drawing.Point(115, 663);
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
			ditherBox.Location = new System.Drawing.Point(72, 13);
			ditherBox.Name = "ditherBox";
			ditherBox.Size = new System.Drawing.Size(68, 19);
			ditherBox.TabIndex = 84;
			ditherBox.Text = "Enabled";
			ditherBox.UseVisualStyleBackColor = true;
			ditherBox.CheckedChanged += DitherBox_CheckedChanged;
			// 
			// ditherLabel
			// 
			ditherLabel.AutoSize = true;
			ditherLabel.ForeColor = System.Drawing.Color.White;
			ditherLabel.Location = new System.Drawing.Point(4, 12);
			ditherLabel.Name = "ditherLabel";
			ditherLabel.Size = new System.Drawing.Size(59, 15);
			ditherLabel.TabIndex = 83;
			ditherLabel.Text = "Dithering:";
			// 
			// updateBatchButton
			// 
			updateBatchButton.Location = new System.Drawing.Point(151, 807);
			updateBatchButton.Name = "updateBatchButton";
			updateBatchButton.Size = new System.Drawing.Size(72, 23);
			updateBatchButton.TabIndex = 82;
			updateBatchButton.Text = "Update";
			updateBatchButton.UseVisualStyleBackColor = true;
			updateBatchButton.Click += UpdateBatchButton_Click;
			// 
			// encodePngLabel
			// 
			encodePngLabel.AutoSize = true;
			encodePngLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			encodePngLabel.ForeColor = System.Drawing.Color.White;
			encodePngLabel.Location = new System.Drawing.Point(4, 695);
			encodePngLabel.Margin = new Padding(4, 0, 4, 0);
			encodePngLabel.Name = "encodePngLabel";
			encodePngLabel.Size = new System.Drawing.Size(87, 15);
			encodePngLabel.TabIndex = 81;
			encodePngLabel.Text = "PNG Encoding:";
			// 
			// encodePngSelect
			// 
			encodePngSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			encodePngSelect.FormattingEnabled = true;
			encodePngSelect.Items.AddRange(new object[] { "No", "Yes" });
			encodePngSelect.Location = new System.Drawing.Point(115, 692);
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
			encodeGifLabel.Location = new System.Drawing.Point(4, 724);
			encodeGifLabel.Margin = new Padding(4, 0, 4, 0);
			encodeGifLabel.Name = "encodeGifLabel";
			encodeGifLabel.Size = new System.Drawing.Size(80, 15);
			encodeGifLabel.TabIndex = 78;
			encodeGifLabel.Text = "GIF Encoding:";
			// 
			// encodeGifSelect
			// 
			encodeGifSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			encodeGifSelect.FormattingEnabled = true;
			encodeGifSelect.Items.AddRange(new object[] { "No", "Local", "Global" });
			encodeGifSelect.Location = new System.Drawing.Point(115, 721);
			encodeGifSelect.Name = "encodeGifSelect";
			encodeGifSelect.Size = new System.Drawing.Size(108, 23);
			encodeGifSelect.TabIndex = 77;
			encodeGifSelect.SelectedIndexChanged += EncodeGifSelect_SelectedIndexChanged;
			// 
			// generationModeLabel
			// 
			generationModeLabel.AutoSize = true;
			generationModeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			generationModeLabel.ForeColor = System.Drawing.Color.White;
			generationModeLabel.Location = new System.Drawing.Point(4, 753);
			generationModeLabel.Margin = new Padding(4, 0, 4, 0);
			generationModeLabel.Name = "generationModeLabel";
			generationModeLabel.Size = new System.Drawing.Size(102, 15);
			generationModeLabel.TabIndex = 76;
			generationModeLabel.Text = "Generation Mode:";
			// 
			// batchBox
			// 
			batchBox.Location = new System.Drawing.Point(86, 808);
			batchBox.Margin = new Padding(4, 3, 4, 3);
			batchBox.Name = "batchBox";
			batchBox.Size = new System.Drawing.Size(59, 23);
			batchBox.TabIndex = 75;
			batchBox.Text = "0";
			batchBox.TextChanged += BatchBox_TextChanged;
			// 
			// batchLabel
			// 
			batchLabel.AutoSize = true;
			batchLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			batchLabel.ForeColor = System.Drawing.Color.White;
			batchLabel.Location = new System.Drawing.Point(4, 812);
			batchLabel.Margin = new Padding(4, 0, 4, 0);
			batchLabel.Name = "batchLabel";
			batchLabel.Size = new System.Drawing.Size(74, 15);
			batchLabel.TabIndex = 74;
			batchLabel.Text = "Select Batch:";
			// 
			// saveBatchButton
			// 
			saveBatchButton.Location = new System.Drawing.Point(151, 779);
			saveBatchButton.Name = "saveBatchButton";
			saveBatchButton.Size = new System.Drawing.Size(72, 23);
			saveBatchButton.TabIndex = 73;
			saveBatchButton.Text = "Save Batch";
			saveBatchButton.UseVisualStyleBackColor = true;
			saveBatchButton.Click += SaveBatchButton_Click;
			// 
			// loadBatchButton
			// 
			loadBatchButton.Location = new System.Drawing.Point(4, 779);
			loadBatchButton.Name = "loadBatchButton";
			loadBatchButton.Size = new System.Drawing.Size(75, 23);
			loadBatchButton.TabIndex = 72;
			loadBatchButton.Text = "Load Batch";
			loadBatchButton.UseVisualStyleBackColor = true;
			loadBatchButton.Click += LoadBatchButton_Click;
			// 
			// addBatchButton
			// 
			addBatchButton.Location = new System.Drawing.Point(85, 779);
			addBatchButton.Name = "addBatchButton";
			addBatchButton.Size = new System.Drawing.Size(60, 23);
			addBatchButton.TabIndex = 71;
			addBatchButton.Text = "+ Batch";
			addBatchButton.UseVisualStyleBackColor = true;
			addBatchButton.Click += AddBatchButton_Click;
			// 
			// runBatchButton
			// 
			runBatchButton.Location = new System.Drawing.Point(6, 837);
			runBatchButton.Name = "runBatchButton";
			runBatchButton.Size = new System.Drawing.Size(217, 23);
			runBatchButton.TabIndex = 62;
			runBatchButton.Text = "Run Batch";
			runBatchButton.UseVisualStyleBackColor = true;
			runBatchButton.Click += RunBatchButton_Click;
			// 
			// timingSelect
			// 
			timingSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			timingSelect.FormattingEnabled = true;
			timingSelect.Items.AddRange(new object[] { "Delay", "Framerate" });
			timingSelect.Location = new System.Drawing.Point(59, 465);
			timingSelect.Name = "timingSelect";
			timingSelect.Size = new System.Drawing.Size(98, 23);
			timingSelect.TabIndex = 70;
			timingSelect.SelectedIndexChanged += TimingSelect_SelectedIndexChanged;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Segoe UI", 9F);
			label2.ForeColor = System.Drawing.Color.White;
			label2.Location = new System.Drawing.Point(4, 439);
			label2.Margin = new Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(72, 15);
			label2.TabIndex = 69;
			label2.Text = "Abort Delay:";
			// 
			// timingLabel
			// 
			timingLabel.AutoSize = true;
			timingLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			timingLabel.ForeColor = System.Drawing.Color.White;
			timingLabel.Location = new System.Drawing.Point(4, 468);
			timingLabel.Margin = new Padding(4, 0, 4, 0);
			timingLabel.Name = "timingLabel";
			timingLabel.Size = new System.Drawing.Size(47, 15);
			timingLabel.TabIndex = 67;
			timingLabel.Text = "Timing:";
			// 
			// parallelLabel
			// 
			parallelLabel.AutoSize = true;
			parallelLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			parallelLabel.ForeColor = System.Drawing.Color.White;
			parallelLabel.Location = new System.Drawing.Point(4, 497);
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
			zoomChildLabel.Location = new System.Drawing.Point(4, 410);
			zoomChildLabel.Margin = new Padding(4, 0, 4, 0);
			zoomChildLabel.Name = "zoomChildLabel";
			zoomChildLabel.Size = new System.Drawing.Size(73, 15);
			zoomChildLabel.TabIndex = 65;
			zoomChildLabel.Text = "Zoom Child:";
			// 
			// zoomChildBox
			// 
			zoomChildBox.Location = new System.Drawing.Point(164, 407);
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
			blurLabel.Location = new System.Drawing.Point(4, 381);
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
			brightnessLabel.Location = new System.Drawing.Point(4, 323);
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
			detailLabel.Location = new System.Drawing.Point(4, 265);
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
			voidScaleLabel.Location = new System.Drawing.Point(4, 236);
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
			voidNoiseLabel.Location = new System.Drawing.Point(4, 207);
			voidNoiseLabel.Margin = new Padding(4, 0, 4, 0);
			voidNoiseLabel.Name = "voidNoiseLabel";
			voidNoiseLabel.Size = new System.Drawing.Size(100, 15);
			voidNoiseLabel.TabIndex = 49;
			voidNoiseLabel.Text = "Void Noise (0-30):";
			// 
			// voidBox
			// 
			voidBox.Location = new System.Drawing.Point(164, 233);
			voidBox.Name = "voidBox";
			voidBox.Size = new System.Drawing.Size(59, 23);
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
			editorPanel.Controls.Add(addColorButton);
			editorPanel.Controls.Add(removeAngleButton);
			editorPanel.Controls.Add(removeColorButton);
			editorPanel.Location = new System.Drawing.Point(17, 691);
			editorPanel.Name = "editorPanel";
			editorPanel.Size = new System.Drawing.Size(286, 440);
			editorPanel.TabIndex = 56;
			// 
			// preButton
			// 
			preButton.Location = new System.Drawing.Point(148, 405);
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
			sizeLabel.Location = new System.Drawing.Point(14, 269);
			sizeLabel.Margin = new Padding(4, 0, 4, 0);
			sizeLabel.Name = "sizeLabel";
			sizeLabel.Size = new System.Drawing.Size(252, 15);
			sizeLabel.TabIndex = 65;
			sizeLabel.Text = "ChildSize      MaxSize        MinSize          Cutsize ";
			// 
			// colorBox
			// 
			colorBox.Location = new System.Drawing.Point(14, 345);
			colorBox.Name = "colorBox";
			colorBox.Size = new System.Drawing.Size(101, 23);
			colorBox.TabIndex = 68;
			colorBox.Text = "NewColorsName";
			// 
			// angleBox
			// 
			angleBox.Location = new System.Drawing.Point(14, 317);
			angleBox.Name = "angleBox";
			angleBox.Size = new System.Drawing.Size(101, 23);
			angleBox.TabIndex = 52;
			angleBox.Text = "NewAnglesName";
			// 
			// addcutLabel
			// 
			addcutLabel.AutoSize = true;
			addcutLabel.ForeColor = System.Drawing.Color.White;
			addcutLabel.Location = new System.Drawing.Point(14, 377);
			addcutLabel.Margin = new Padding(4, 0, 4, 0);
			addcutLabel.Name = "addcutLabel";
			addcutLabel.Size = new System.Drawing.Size(101, 15);
			addcutLabel.TabIndex = 57;
			addcutLabel.Text = "Add CutFunction:";
			// 
			// maxBox
			// 
			maxBox.Location = new System.Drawing.Point(80, 287);
			maxBox.Name = "maxBox";
			maxBox.Size = new System.Drawing.Size(56, 23);
			maxBox.TabIndex = 66;
			maxBox.TextChanged += MaxBox_TextChanged;
			// 
			// addCut
			// 
			addCut.FormattingEnabled = true;
			addCut.Location = new System.Drawing.Point(121, 374);
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
			pointPanel.BackColor = System.Drawing.Color.Black;
			pointPanel.Controls.Add(addPoint);
			pointPanel.Location = new System.Drawing.Point(14, 31);
			pointPanel.Name = "pointPanel";
			pointPanel.Size = new System.Drawing.Size(256, 235);
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
			minBox.Location = new System.Drawing.Point(148, 287);
			minBox.Name = "minBox";
			minBox.Size = new System.Drawing.Size(56, 23);
			minBox.TabIndex = 65;
			minBox.TextChanged += MinBox_TextChanged;
			// 
			// saveButton
			// 
			saveButton.Location = new System.Drawing.Point(80, 405);
			saveButton.Name = "saveButton";
			saveButton.Size = new System.Drawing.Size(56, 27);
			saveButton.TabIndex = 58;
			saveButton.Text = "SAVE";
			saveButton.UseVisualStyleBackColor = true;
			saveButton.Click += SaveButton_Click;
			// 
			// loadButton
			// 
			loadButton.Location = new System.Drawing.Point(14, 405);
			loadButton.Name = "loadButton";
			loadButton.Size = new System.Drawing.Size(56, 27);
			loadButton.TabIndex = 57;
			loadButton.Text = "LOAD";
			loadButton.UseVisualStyleBackColor = true;
			loadButton.Click += LoadButton_Click;
			// 
			// cutBox
			// 
			cutBox.Location = new System.Drawing.Point(214, 287);
			cutBox.Name = "cutBox";
			cutBox.Size = new System.Drawing.Size(56, 23);
			cutBox.TabIndex = 64;
			cutBox.TextChanged += CutBox_TextChanged;
			// 
			// addAngleButton
			// 
			addAngleButton.Location = new System.Drawing.Point(121, 316);
			addAngleButton.Name = "addAngleButton";
			addAngleButton.Size = new System.Drawing.Size(122, 23);
			addAngleButton.TabIndex = 60;
			addAngleButton.Text = "ADD ANGLES";
			addAngleButton.UseVisualStyleBackColor = true;
			addAngleButton.Click += AddAngleButton_Click;
			// 
			// sizeBox
			// 
			sizeBox.Location = new System.Drawing.Point(14, 287);
			sizeBox.Name = "sizeBox";
			sizeBox.Size = new System.Drawing.Size(56, 23);
			sizeBox.TabIndex = 57;
			sizeBox.TextChanged += SizeBox_TextChanged;
			// 
			// addColorButton
			// 
			addColorButton.Location = new System.Drawing.Point(121, 345);
			addColorButton.Name = "addColorButton";
			addColorButton.Size = new System.Drawing.Size(122, 23);
			addColorButton.TabIndex = 61;
			addColorButton.Text = "ADD COLORS";
			addColorButton.UseVisualStyleBackColor = true;
			addColorButton.Click += AddColorButton_Click;
			// 
			// removeAngleButton
			// 
			removeAngleButton.Location = new System.Drawing.Point(249, 316);
			removeAngleButton.Name = "removeAngleButton";
			removeAngleButton.Size = new System.Drawing.Size(21, 23);
			removeAngleButton.TabIndex = 62;
			removeAngleButton.Text = "X";
			removeAngleButton.UseVisualStyleBackColor = true;
			removeAngleButton.Click += RemoveAngleButton_Click;
			// 
			// removeColorButton
			// 
			removeColorButton.Location = new System.Drawing.Point(249, 345);
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
			// loadBatch
			// 
			loadBatch.DefaultExt = "btc";
			loadBatch.Filter = "Batch files (*.btc)|*.btc";
			loadBatch.RestoreDirectory = true;
			loadBatch.FileOk += LoadBatch_FileOk;
			// 
			// saveBatch
			// 
			saveBatch.DefaultExt = "btc";
			saveBatch.Filter = "Batch files (*.btc)|*.btc";
			saveBatch.RestoreDirectory = true;
			saveBatch.FileOk += SaveBatch_FileOk;
			// 
			// runBatch
			// 
			runBatch.DefaultExt = "png";
			runBatch.Filter = "PNG file (*.png)|*.png";
			runBatch.RestoreDirectory = true;
			runBatch.FileOk += RunBatch_FileOk;
			// 
			// debugsLabel
			// 
			debugsLabel.AutoSize = true;
			debugsLabel.ForeColor = System.Drawing.Color.White;
			debugsLabel.Location = new System.Drawing.Point(18, 637);
			debugsLabel.Name = "debugsLabel";
			debugsLabel.Size = new System.Drawing.Size(45, 15);
			debugsLabel.TabIndex = 61;
			debugsLabel.Text = "Debug:";
			// 
			// debugAnimBox
			// 
			debugAnimBox.AutoSize = true;
			debugAnimBox.ForeColor = System.Drawing.Color.White;
			debugAnimBox.Location = new System.Drawing.Point(128, 636);
			debugAnimBox.Name = "debugAnimBox";
			debugAnimBox.Size = new System.Drawing.Size(55, 19);
			debugAnimBox.TabIndex = 62;
			debugAnimBox.Text = "Anim";
			debugAnimBox.UseVisualStyleBackColor = true;
			debugAnimBox.CheckedChanged += debugAnimBox_CheckedChanged;
			// 
			// debugPngBox
			// 
			debugPngBox.AutoSize = true;
			debugPngBox.ForeColor = System.Drawing.Color.White;
			debugPngBox.Location = new System.Drawing.Point(189, 636);
			debugPngBox.Name = "debugPngBox";
			debugPngBox.Size = new System.Drawing.Size(50, 19);
			debugPngBox.TabIndex = 63;
			debugPngBox.Text = "PNG";
			debugPngBox.UseVisualStyleBackColor = true;
			debugPngBox.CheckedChanged += debugPngBox_CheckedChanged;
			// 
			// debugGifBox
			// 
			debugGifBox.AutoSize = true;
			debugGifBox.ForeColor = System.Drawing.Color.White;
			debugGifBox.Location = new System.Drawing.Point(245, 636);
			debugGifBox.Name = "debugGifBox";
			debugGifBox.Size = new System.Drawing.Size(43, 19);
			debugGifBox.TabIndex = 64;
			debugGifBox.Text = "GIF";
			debugGifBox.UseVisualStyleBackColor = true;
			debugGifBox.CheckedChanged += debugGifBox_CheckedChanged;
			// 
			// loadExport
			// 
			loadExport.Filter = "All files (*.*)|*.*";
			loadExport.RestoreDirectory = true;
			loadExport.FileOk += LoadExport_FileOk;
			// 
			// l2Box
			// 
			l2Box.Location = new System.Drawing.Point(164, 605);
			l2Box.Name = "l2Box";
			l2Box.Size = new System.Drawing.Size(59, 23);
			l2Box.TabIndex = 95;
			l2Box.Text = "0";
			l2Box.TextChanged += L2Box_TextChanged;
			// 
			// l2Label
			// 
			l2Label.AutoSize = true;
			l2Label.Font = new System.Drawing.Font("Segoe UI", 9F);
			l2Label.ForeColor = System.Drawing.Color.White;
			l2Label.Location = new System.Drawing.Point(4, 608);
			l2Label.Margin = new Padding(4, 0, 4, 0);
			l2Label.Name = "l2Label";
			l2Label.Size = new System.Drawing.Size(81, 15);
			l2Label.TabIndex = 96;
			l2Label.Text = "L2 Cache Size:";
			// 
			// GeneratorForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
			ClientSize = new System.Drawing.Size(1383, 1178);
			Controls.Add(debugGifBox);
			Controls.Add(debugPngBox);
			Controls.Add(debugAnimBox);
			Controls.Add(debugsLabel);
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
			Text = "RGB Fractal Zoom Generator C# v1.13.0";
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
		private System.Windows.Forms.ComboBox generationSelect;
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
		private System.Windows.Forms.SaveFileDialog saveMp4;
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
		private System.Windows.Forms.Label timingLabel;
		private System.Windows.Forms.ComboBox paletteSelect;
		private System.Windows.Forms.Label paletteLabel;
		private System.Windows.Forms.Button addPalette;
		private System.Windows.Forms.Button removePalette;
		private System.Windows.Forms.ColorDialog paletteDialog;
		private ComboBox exportSelect;
		private ComboBox timingSelect;
		private Button runBatchButton;
		private Button loadBatchButton;
		private Button addBatchButton;
		private OpenFileDialog loadBatch;
		private SaveFileDialog saveBatch;
		private Button saveBatchButton;
		private SaveFileDialog runBatch;
		private TextBox batchBox;
		private Label batchLabel;
		private Label generationModeLabel;
		private Label encodePngLabel;
		private ComboBox encodePngSelect;
		private Label encodeGifLabel;
		private ComboBox encodeGifSelect;
		private Button updateBatchButton;
		private Label debugsLabel;
		private CheckBox debugAnimBox;
		private CheckBox debugPngBox;
		private CheckBox debugGifBox;
		private CheckBox ditherBox;
		private Label ditherLabel;
		private Label fileLabel;
		private ComboBox fileSelect;
		private OpenFileDialog loadExport;
		private TextBox stripeBox;
		private TextBox binBox;
		private ComboBox voidSelect;
		private ComboBox drawSelect;
		private CheckBox cacheBox;
		private Label gpuDLabel;
		private Label gpuVLabel;
		private Label stripeLabel;
		private Label binLabel;
		private Label cacheLabel;
		private Label l2Label;
		private TextBox l2Box;
	}
}

