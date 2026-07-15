using System.Windows.Forms;

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
		helpLabel = new Label();
		panel1 = new Panel();
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
		panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
		AutoScaleMode = AutoScaleMode.Font;
		AutoScroll = true;
		AutoSize = true;
		BackColor = RgbFractalGenCs.Core.StaticCore.Background;
		ClientSize = new System.Drawing.Size(704, 281);
		Controls.Add(panel1);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		MinimumSize = new System.Drawing.Size(720, 320);
		Name = "HelpForm";
		Text = "HelpForm";
		panel1.ResumeLayout(false);
		panel1.PerformLayout();
		ResumeLayout(false);
	}

	#endregion

	private Label helpLabel;
	private Panel panel1;
}