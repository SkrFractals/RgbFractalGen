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
		helpBut = new Button();
		toolTips = new ToolTip(components);
		updateTimer = new Timer(components);
		SuspendLayout();
		// 
		// setBut
		// 
		setBut.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		setBut.Location = new System.Drawing.Point(12, 12);
		setBut.Name = "setBut";
		setBut.Size = new System.Drawing.Size(240, 23);
		setBut.TabIndex = 0;
		setBut.Text = "SETTINGS";
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
		genBut.Text = "GENERATORS";
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
		editBut.Text = "EDITOR";
		editBut.UseVisualStyleBackColor = true;
		editBut.Click += EditBut_Click;
		// 
		// helpBut
		// 
		helpBut.Location = new System.Drawing.Point(12, 99);
		helpBut.Name = "helpBut";
		helpBut.Size = new System.Drawing.Size(240, 23);
		helpBut.TabIndex = 2;
		helpBut.Text = "HELP";
		helpBut.UseVisualStyleBackColor = true;
		helpBut.Click += EditBut_Click;
		// 
		// updateTimer
		// 
		updateTimer.Enabled = true;
		updateTimer.Tick += UpdateTimer_Tick;
		// 
		// MainForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		ClientSize = new System.Drawing.Size(264, 105);
		Controls.Add(editBut);
		Controls.Add(genBut);
		Controls.Add(setBut);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Margin = new Padding(4, 3, 4, 3);
		MinimumSize = new System.Drawing.Size(280, 144);
		Name = "MainForm";
		Text = "[ RGB Fractal Animation Generator - <version> ]";
		FormClosing += MainForm_FormClosing;
		Load += MainForm_Load;
		ResumeLayout(false);
	}

	#endregion

	private System.Windows.Forms.Button setBut;
	private System.Windows.Forms.Button genBut;
	private System.Windows.Forms.Button editBut;
	private System.Windows.Forms.Button helpBut;
	private System.Windows.Forms.ToolTip toolTips;
	private Timer updateTimer;
}