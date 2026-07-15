namespace RgbFractalGenCs.Content;

partial class HelpForm {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpForm));
		helpLabel = new System.Windows.Forms.Label();
		panel1 = new System.Windows.Forms.Panel();
		panel1.SuspendLayout();
		SuspendLayout();
		// 
		// helpLabel
		// 
		helpLabel.AutoSize = true;
		helpLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		helpLabel.Location = new System.Drawing.Point(4, 4);
		helpLabel.Name = "helpLabel";
		helpLabel.Size = new System.Drawing.Size(58, 15);
		helpLabel.TabIndex = 0;
		helpLabel.Text = "helpLabel";
		// 
		// panel1
		// 
		panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		panel1.AutoScroll = true;
		panel1.Controls.Add(helpLabel);
		panel1.Location = new System.Drawing.Point(12, 12);
		panel1.Name = "panel1";
		panel1.Size = new System.Drawing.Size(680, 257);
		panel1.TabIndex = 1;
		// 
		// HelpForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		AutoScroll = true;
		AutoSize = true;
		BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
		ClientSize = new System.Drawing.Size(704, 281);
		Controls.Add(panel1);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Name = "HelpForm";
		Text = "HelpForm";
		panel1.ResumeLayout(false);
		panel1.PerformLayout();
		ResumeLayout(false);
	}

	#endregion

	private System.Windows.Forms.Label helpLabel;
	private System.Windows.Forms.Panel panel1;
}