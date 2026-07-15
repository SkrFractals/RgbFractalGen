using System.Windows.Forms;

namespace RgbFractalGenCs;

partial class GeneratorsForm {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneratorsForm));
		toolTips = new ToolTip(components);
		addButton = new Button();
		pointPanel = new Panel();
		pointPanel.SuspendLayout();
		SuspendLayout();
		// 
		// addButton
		// 
		addButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		addButton.Location = new System.Drawing.Point(3, 3);
		addButton.Name = "addButton";
		addButton.Size = new System.Drawing.Size(570, 23);
		addButton.TabIndex = 0;
		addButton.Text = "[Add]";
		addButton.UseVisualStyleBackColor = true;
		addButton.Click += AddButton_Click;
		// 
		// pointPanel
		// 
		pointPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		pointPanel.Controls.Add(addButton);
		pointPanel.Location = new System.Drawing.Point(4, 4);
		pointPanel.Name = "pointPanel";
		pointPanel.Size = new System.Drawing.Size(576, 373);
		pointPanel.TabIndex = 1;
		// 
		// GeneratorsForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		ClientSize = new System.Drawing.Size(584, 381);
		Controls.Add(pointPanel);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Margin = new Padding(4, 3, 4, 3);
		Name = "GeneratorsForm";
		Text = "[ GeneratorsForm ]";
		FormClosing += GeneratorsForm_FormClosing;
		pointPanel.ResumeLayout(false);
		ResumeLayout(false);
	}

	#endregion

	private System.Windows.Forms.ToolTip toolTips;
	private Button addButton;
	private Panel pointPanel;
}