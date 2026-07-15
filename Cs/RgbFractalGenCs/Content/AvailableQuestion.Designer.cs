using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

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
		panel1 = new Panel();
		panel1.SuspendLayout();
		SuspendLayout();
		// 
		// messageLabel
		// 
		messageLabel.Anchor = AnchorStyles.None;
		messageLabel.AutoSize = true;
		messageLabel.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		messageLabel.Location = new System.Drawing.Point(130, 7);
		messageLabel.Name = "messageLabel";
		messageLabel.Size = new System.Drawing.Size(61, 15);
		messageLabel.TabIndex = 0;
		messageLabel.Text = "[message]";
		// 
		// yesButton
		// 
		yesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		yesButton.Location = new System.Drawing.Point(12, 46);
		yesButton.Name = "yesButton";
		yesButton.Size = new System.Drawing.Size(75, 23);
		yesButton.TabIndex = 1;
		yesButton.Text = "[ Yes ]";
		yesButton.UseVisualStyleBackColor = true;
		yesButton.Click += YesButton_Click;
		// 
		// noButton
		// 
		noButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		noButton.Location = new System.Drawing.Point(93, 46);
		noButton.Name = "noButton";
		noButton.Size = new System.Drawing.Size(75, 23);
		noButton.TabIndex = 2;
		noButton.Text = "[ No ]";
		noButton.UseVisualStyleBackColor = true;
		noButton.Click += NoButton_Click;
		// 
		// abortButton
		// 
		abortButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		abortButton.Location = new System.Drawing.Point(174, 46);
		abortButton.Name = "abortButton";
		abortButton.Size = new System.Drawing.Size(75, 23);
		abortButton.TabIndex = 3;
		abortButton.Text = "[ Abort ]";
		abortButton.UseVisualStyleBackColor = true;
		abortButton.Click += AbortButton_Click;
		// 
		// cancelButton
		// 
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		cancelButton.Location = new System.Drawing.Point(255, 46);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new System.Drawing.Size(75, 23);
		cancelButton.TabIndex = 4;
		cancelButton.Text = "[ Cancel ]";
		cancelButton.UseVisualStyleBackColor = true;
		cancelButton.Click += CancelButton_Click_1;
		// 
		// panel1
		// 
		panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		panel1.AutoSize = true;
		panel1.Controls.Add(messageLabel);
		panel1.Location = new System.Drawing.Point(12, 9);
		panel1.Name = "panel1";
		panel1.Size = new System.Drawing.Size(318, 29);
		panel1.TabIndex = 5;
		// 
		// AvailableQuestion
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		BackColor = RgbFractalGenCs.Core.StaticCore.Background;
		ClientSize = new System.Drawing.Size(342, 81);
		Controls.Add(panel1);
		Controls.Add(cancelButton);
		Controls.Add(abortButton);
		Controls.Add(noButton);
		Controls.Add(yesButton);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		MinimumSize = new System.Drawing.Size(358, 120);
		Name = "AvailableQuestion";
		Text = "AvailableQuestion";
		panel1.ResumeLayout(false);
		panel1.PerformLayout();
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
	private Panel panel1;
}