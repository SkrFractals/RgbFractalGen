using System.Windows.Forms;

namespace RgbFractalGenCs;

partial class MainForm {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
		setBut = new Button();
		genBut = new Button();
		editBut = new Button();
		toolTips = new ToolTip(components);
		updateTimer = new Timer(components);
		helpBut = new Button();
		SuspendLayout();
		// 
		// setBut
		// 
		setBut.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		setBut.Location = new System.Drawing.Point(12, 12);
		setBut.Name = "setBut";
		setBut.Size = new System.Drawing.Size(240, 23);
		setBut.TabIndex = 0;
		setBut.Text = "[ Seetings ]";
		setBut.UseVisualStyleBackColor = true;
		setBut.Click += SetBut_Click;
		// 
		// genBut
		// 
		genBut.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		genBut.Location = new System.Drawing.Point(12, 41);
		genBut.Name = "genBut";
		genBut.Size = new System.Drawing.Size(240, 23);
		genBut.TabIndex = 1;
		genBut.Text = "[ Generator ]";
		genBut.UseVisualStyleBackColor = true;
		genBut.Click += GenBut_Click;
		// 
		// editBut
		// 
		editBut.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		editBut.Location = new System.Drawing.Point(12, 70);
		editBut.Name = "editBut";
		editBut.Size = new System.Drawing.Size(240, 23);
		editBut.TabIndex = 2;
		editBut.Text = "[ Editor]";
		editBut.UseVisualStyleBackColor = true;
		editBut.Click += EditBut_Click;
		// 
		// updateTimer
		// 
		updateTimer.Enabled = true;
		updateTimer.Interval = 40;
		updateTimer.Tick += UpdateTimer_Tick;
		// 
		// helpBut
		// 
		helpBut.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		helpBut.Location = new System.Drawing.Point(12, 99);
		helpBut.Name = "helpBut";
		helpBut.Size = new System.Drawing.Size(240, 23);
		helpBut.TabIndex = 3;
		helpBut.Text = "[ Help ]";
		helpBut.UseVisualStyleBackColor = true;
		helpBut.Click += HelpBut_Click;
		// 
		// MainForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = RgbFractalGenCs.Core.StaticCore.Background;
		ClientSize = new System.Drawing.Size(264, 134);
		Controls.Add(helpBut);
		Controls.Add(editBut);
		Controls.Add(genBut);
		Controls.Add(setBut);
		Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 238);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Margin = new Padding(4, 3, 4, 3);
		MinimumSize = new System.Drawing.Size(280, 173);
		Name = "MainForm";
		Text = "[ RGB Fractal Animation Generator - <version> ]";
		FormClosing += MainForm_FormClosing;
		ResumeLayout(false);
	}

	#endregion

	private Button setBut;
	private Button genBut;
	private Button editBut;
	private ToolTip toolTips;
	private Timer updateTimer;
	private Button helpBut;
}