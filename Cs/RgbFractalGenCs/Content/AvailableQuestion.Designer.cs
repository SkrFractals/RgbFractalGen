using System.Windows.Forms;

namespace RgbFractalGenCs.Content;

partial class AvailableQuestion {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(AvailableQuestion));
		toolTips = new ToolTip(components);
		messageLabel = new Label();
		yesButton = new Button();
		noButton = new Button();
		abortButton = new Button();
		cancelButton = new Button();
		SuspendLayout();
		// 
		// messageLabel
		// 
		messageLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		messageLabel.AutoSize = true;
		messageLabel.Location = new System.Drawing.Point(140, 9);
		messageLabel.Name = "messageLabel";
		messageLabel.Size = new System.Drawing.Size(61, 15);
		messageLabel.TabIndex = 0;
		messageLabel.Text = "[message]";
		messageLabel.ForeColor = System.Drawing.Color.FromArgb(255,255,255);
		// 
		// yesButton
		// 
		yesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		yesButton.Location = new System.Drawing.Point(12, 36);
		yesButton.Name = "yesButton";
		yesButton.Size = new System.Drawing.Size(75, 23);
		yesButton.TabIndex = 1;
		yesButton.Text = "[ Yes ]";
		yesButton.UseVisualStyleBackColor = true;
		yesButton.Click += YesButton_Click;
		// 
		// noButton
		// 
		noButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		noButton.Location = new System.Drawing.Point(93, 36);
		noButton.Name = "noButton";
		noButton.Size = new System.Drawing.Size(75, 23);
		noButton.TabIndex = 2;
		noButton.Text = "[ No ]";
		noButton.UseVisualStyleBackColor = true;
		noButton.Click += NoButton_Click;
		// 
		// abortButton
		// 
		abortButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		abortButton.Location = new System.Drawing.Point(174, 36);
		abortButton.Name = "abortButton";
		abortButton.Size = new System.Drawing.Size(75, 23);
		abortButton.TabIndex = 3;
		abortButton.Text = "[ Abort ]";
		abortButton.UseVisualStyleBackColor = true;
		abortButton.Click += AbortButton_Click;
		// 
		// cancelButton
		// 
		cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		cancelButton.Location = new System.Drawing.Point(255, 36);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new System.Drawing.Size(75, 23);
		cancelButton.TabIndex = 4;
		cancelButton.Text = "[ Cancel ]";
		cancelButton.UseVisualStyleBackColor = true;
		cancelButton.Click += CancelButton_Click_1;
		// 
		// AvailableQuestion
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		AutoSize = true;
		AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
		ClientSize = new System.Drawing.Size(342, 71);
		Controls.Add(cancelButton);
		Controls.Add(abortButton);
		Controls.Add(noButton);
		Controls.Add(yesButton);
		Controls.Add(messageLabel);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Name = "AvailableQuestion";
		Text = "AvailableQuestion";
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private ToolTip toolTips;
	private Label messageLabel;
	private Button yesButton;
	private Button noButton;
	private Button abortButton;
	private Button cancelButton;
}