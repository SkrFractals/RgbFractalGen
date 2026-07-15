using System.Windows.Forms;

namespace RgbFractalGenCs.Content;

partial class InputDialog {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(InputDialog));
		textBox = new TextBox();
		cancelButton = new Button();
		pasteButton = new Button();
		okButton = new Button();
		toolTips = new ToolTip(components);
		SuspendLayout();
		// 
		// textBox
		// 
		textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		textBox.Location = new System.Drawing.Point(12, 12);
		textBox.Name = "textBox";
		textBox.Size = new System.Drawing.Size(295, 23);
		textBox.TabIndex = 0;
		textBox.TextChanged += TextBox_TextChanged;
		// 
		// cancelButton
		// 
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		cancelButton.Location = new System.Drawing.Point(12, 41);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new System.Drawing.Size(60, 23);
		cancelButton.TabIndex = 2;
		cancelButton.Text = "[ Cancel ]";
		cancelButton.UseVisualStyleBackColor = true;
		cancelButton.Click += CancelButton_Click;
		// 
		// pasteButton
		// 
		pasteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		pasteButton.Location = new System.Drawing.Point(78, 41);
		pasteButton.Name = "pasteButton";
		pasteButton.Size = new System.Drawing.Size(64, 23);
		pasteButton.TabIndex = 1;
		pasteButton.Text = "[ Paste ]";
		pasteButton.UseVisualStyleBackColor = true;
		pasteButton.Click += PasteButton_Click;
		// 
		// okButton
		// 
		okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		okButton.Location = new System.Drawing.Point(148, 41);
		okButton.Name = "okButton";
		okButton.Size = new System.Drawing.Size(159, 23);
		okButton.TabIndex = 2;
		okButton.Text = "[ OK ]";
		okButton.UseVisualStyleBackColor = true;
		okButton.Click += OkButton_Click;
		// 
		// InputDialog
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		ClientSize = new System.Drawing.Size(319, 76);
		Controls.Add(cancelButton);
		Controls.Add(pasteButton);
		Controls.Add(okButton);
		Controls.Add(textBox);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Margin = new Padding(4, 3, 4, 3);
		Name = "InputDialog";
		Text = "[ Input ]";
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private System.Windows.Forms.TextBox textBox;
	private System.Windows.Forms.Button cancelButton;
	private System.Windows.Forms.Button pasteButton;
	private System.Windows.Forms.Button okButton;
	private System.Windows.Forms.ToolTip toolTips;
}