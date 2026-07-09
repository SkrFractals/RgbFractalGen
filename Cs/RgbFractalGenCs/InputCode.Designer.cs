using System.Security.Principal;

namespace RgbFractalGenCs;

partial class InputCode {
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
		textBox = new System.Windows.Forms.TextBox();
		pasteButton = new System.Windows.Forms.Button();
		SuspendLayout();
		// 
		// textBox
		// 
		textBox.Location = new System.Drawing.Point(12, 12);
		textBox.Name = "textBox";
		textBox.Size = new System.Drawing.Size(1160, 23);
		textBox.TabIndex = 0;
		textBox.TextChanged += textBox_TextChanged;
		// 
		// pasteButton
		// 
		pasteButton.Location = new System.Drawing.Point(12, 41);
		pasteButton.Name = "pasteButton";
		pasteButton.Size = new System.Drawing.Size(1160, 23);
		pasteButton.TabIndex = 1;
		pasteButton.Text = "Paste Import Code";
		pasteButton.UseVisualStyleBackColor = true;
		pasteButton.Click += pasteButton_Click;
		// 
		// InputCode
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		ClientSize = new System.Drawing.Size(1184, 77);
		Controls.Add(pasteButton);
		Controls.Add(textBox);
		Name = "InputCode";
		Text = "InputCode";
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private System.Windows.Forms.TextBox textBox;
	private System.Windows.Forms.Button pasteButton;
}