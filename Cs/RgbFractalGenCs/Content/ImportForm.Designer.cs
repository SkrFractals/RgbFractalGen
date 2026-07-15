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
		SuspendLayout();
		// 
		// textBox
		// 
		textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		textBox.Location = new System.Drawing.Point(12, 12);
		textBox.Name = "textBox";
		textBox.Size = new System.Drawing.Size(600, 23);
		textBox.TabIndex = 0;
		textBox.TextChanged += TextBox_TextChanged;
		// 
		// pasteButton
		// 
		pasteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		pasteButton.Location = new System.Drawing.Point(137, 41);
		pasteButton.Name = "pasteButton";
		pasteButton.Size = new System.Drawing.Size(475, 23);
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
		// ImportForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		AutoSize = true;
		BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		ClientSize = new System.Drawing.Size(624, 76);
		Controls.Add(copyButton);
		Controls.Add(pasteButton);
		Controls.Add(textBox);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Margin = new Padding(4, 3, 4, 3);
		MinimumSize = new System.Drawing.Size(640, 115);
		Name = "ImportForm";
		Text = "RGB Fractal Animation Generator C# - Import Code - ";
		FormClosing += ImportForm_FormClosing;
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private System.Windows.Forms.TextBox textBox;
	private System.Windows.Forms.Button pasteButton;
	private System.Windows.Forms.Button copyButton;
	private ToolTip toolTips;
}