using System.Windows.Forms;

namespace RgbFractalGenCs.Content;

partial class SchedulerForm {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulerForm));
		toolTips = new ToolTip(components);
		pointPanel = new Panel();
		SuspendLayout();
		// 
		// pointPanel
		// 
		pointPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		pointPanel.Location = new System.Drawing.Point(12, 12);
		pointPanel.Name = "pointPanel";
		pointPanel.Size = new System.Drawing.Size(380, 57);
		pointPanel.TabIndex = 0;
		// 
		// SchedulerForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = RgbFractalGenCs.Core.StaticCore.Background;
		ClientSize = new System.Drawing.Size(404, 81);
		Controls.Add(pointPanel);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		MinimumSize = new System.Drawing.Size(420, 120);
		Name = "SchedulerForm";
		Text = "SchedulerForm";
		ResumeLayout(false);
	}

	#endregion
	private ToolTip toolTips;
	private Panel pointPanel;
}