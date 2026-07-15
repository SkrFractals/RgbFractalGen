using System.Security.Principal;
using System.Windows.Forms;

namespace RgbFractalGenCs;

partial class ImportForm {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportForm));
		textBox = new TextBox();
		pasteButton = new Button();
		copyButton = new Button();
		toolTips = new ToolTip(components);
		importFileButton = new Button();
		loadExport = new OpenFileDialog();
		SuspendLayout();
		// 
		// textBox
		// 
		textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		textBox.Location = new System.Drawing.Point(12, 12);
		textBox.Name = "textBox";
		textBox.Size = new System.Drawing.Size(760, 23);
		textBox.TabIndex = 0;
		textBox.TextChanged += TextBox_TextChanged;
		// 
		// pasteButton
		// 
		pasteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		pasteButton.Location = new System.Drawing.Point(400, 41);
		pasteButton.Name = "pasteButton";
		pasteButton.Size = new System.Drawing.Size(372, 23);
		pasteButton.TabIndex = 1;
		pasteButton.Text = "[Paste]";
		pasteButton.UseVisualStyleBackColor = true;
		pasteButton.Click += PasteButton_Click;
		// 
		// copyButton
		// 
		copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		copyButton.Location = new System.Drawing.Point(12, 41);
		copyButton.Name = "copyButton";
		copyButton.Size = new System.Drawing.Size(119, 23);
		copyButton.TabIndex = 2;
		copyButton.Text = "[ Copy ]";
		copyButton.UseVisualStyleBackColor = true;
		copyButton.Click += CopyButton_Click;
		// 
		// importFileButton
		// 
		importFileButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		importFileButton.Location = new System.Drawing.Point(137, 41);
		importFileButton.Name = "importFileButton";
		importFileButton.Size = new System.Drawing.Size(257, 23);
		importFileButton.TabIndex = 3;
		importFileButton.Text = "[ Import File ]";
		importFileButton.UseVisualStyleBackColor = true;
		importFileButton.Click += FileButton_Click;
		// 
		// loadExport
		// 
		loadExport.Filter = "All files (*.*)|*.*";
		loadExport.RestoreDirectory = true;
		loadExport.FileOk += LoadExport_FileOk;
		// 
		// ImportForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		AutoSize = true;
		BackColor = RgbFractalGenCs.Core.StaticCore.Background;
		ClientSize = new System.Drawing.Size(784, 76);
		Controls.Add(importFileButton);
		Controls.Add(copyButton);
		Controls.Add(pasteButton);
		Controls.Add(textBox);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Margin = new Padding(4, 3, 4, 3);
		MaximumSize = new System.Drawing.Size(1920, 115);
		MinimumSize = new System.Drawing.Size(400, 115);
		Name = "ImportForm";
		Text = "RGB Fractal Animation Generator C# - Import Code - ";
		FormClosing += ImportForm_FormClosing;
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private TextBox textBox;
	private Button pasteButton;
	private Button copyButton;
	private ToolTip toolTips;
	private Button importFileButton;
	private OpenFileDialog loadExport;
}