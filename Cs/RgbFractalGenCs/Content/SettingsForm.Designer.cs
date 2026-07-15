using System.Windows.Forms;

namespace RgbFractalGenCs.Content;

partial class SettingsForm {
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
		localeBox = new ComboBox();
		localeLabel = new Label();
		threadsLabel = new Label();
		cacheBox = new CheckBox();
		cacheLabel = new Label();
		threadsBox = new TextBox();
		toolTips = new ToolTip(components);
		SuspendLayout();
		// 
		// localeBox
		// 
		localeBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		localeBox.FormattingEnabled = true;
		localeBox.Location = new System.Drawing.Point(130, 12);
		localeBox.Name = "localeBox";
		localeBox.Size = new System.Drawing.Size(129, 23);
		localeBox.TabIndex = 0;
		localeBox.SelectedIndexChanged += LocaleBox_SelectedIndexChanged;
		// 
		// localeLabel
		// 
		localeLabel.AutoSize = true;
		localeLabel.Location = new System.Drawing.Point(12, 15);
		localeLabel.Name = "localeLabel";
		localeLabel.Size = new System.Drawing.Size(46, 15);
		localeLabel.TabIndex = 1;
		localeLabel.Text = "[locale]";
		localeLabel.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		// 
		// threadsLabel
		// 
		threadsLabel.AutoSize = true;
		threadsLabel.Location = new System.Drawing.Point(12, 41);
		threadsLabel.Name = "threadsLabel";
		threadsLabel.Size = new System.Drawing.Size(54, 15);
		threadsLabel.TabIndex = 2;
		threadsLabel.Text = "[threads]";
		threadsLabel.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		// 
		// cacheBox
		// 
		cacheBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		cacheBox.AutoSize = true;
		cacheBox.Location = new System.Drawing.Point(130, 72);
		cacheBox.Name = "cacheBox";
		cacheBox.Size = new System.Drawing.Size(65, 19);
		cacheBox.TabIndex = 3;
		cacheBox.Text = "[cache]";
		cacheBox.UseVisualStyleBackColor = true;
		cacheBox.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		// 
		// cacheLabel
		// 
		cacheLabel.AutoSize = true;
		cacheLabel.Location = new System.Drawing.Point(12, 71);
		cacheLabel.Name = "cacheLabel";
		cacheLabel.Size = new System.Drawing.Size(46, 15);
		cacheLabel.TabIndex = 4;
		cacheLabel.Text = "[cache]";
		cacheLabel.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		// 
		// threadsBox
		// 
		threadsBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		threadsBox.Location = new System.Drawing.Point(130, 41);
		threadsBox.Name = "threadsBox";
		threadsBox.Size = new System.Drawing.Size(129, 23);
		threadsBox.TabIndex = 5;
		threadsBox.TextChanged += ThreadsBox_TextChanged;
		// 
		// SettingsForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
		ClientSize = new System.Drawing.Size(271, 104);
		Controls.Add(localeLabel);
		Controls.Add(cacheLabel);
		Controls.Add(cacheBox);
		Controls.Add(threadsBox);
		Controls.Add(threadsLabel);
		Controls.Add(localeBox);
		Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		Margin = new Padding(4, 3, 4, 3);
		MaximumSize = new System.Drawing.Size(1920, 143);
		MinimumSize = new System.Drawing.Size(287, 0);
		Name = "SettingsForm";
		Text = "[ Settings ]";
		FormClosing += SettingsForm_FormClosing;
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion
	private CheckBox cacheBox;
	private Label cacheLabel;
	private ComboBox localeBox;
	private Label localeLabel;
	private Label threadsLabel;
	private TextBox threadsBox;
	private ToolTip toolTips;
}