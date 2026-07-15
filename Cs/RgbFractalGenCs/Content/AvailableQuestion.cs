#nullable enable
using System;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs.Content;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class AvailableQuestion : Form {
	private int controlTabIndex = 0;
	public AvailableQuestion(string caption, string message, string yes = "", string no = "", string abort = "", string cancel = "") {
		InitializeComponent();
		controlTabIndex = 0;
		Text = L(caption);
		SetupControl(messageLabel, L(message));
		SetupControl(yesButton, L(yes));
		SetupControl(noButton, L(no));
		SetupControl(abortButton, L(abort));
		SetupControl(cancelButton, L(cancel));
	}
	private void Finish(DialogResult r) {
		DialogResult = r;
		Close();
	}
	void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, L(tip));
		control.TabIndex = ++controlTabIndex;
		//myControls.Add(control);
	}

	private void YesButton_Click(object sender, EventArgs e) => Finish(DialogResult.Yes);
	private void NoButton_Click(object sender, EventArgs e) => Finish(DialogResult.No);
	private void AbortButton_Click(object sender, EventArgs e) => Finish(DialogResult.Abort);
	private void CancelButton_Click_1(object sender, EventArgs e) => Finish(DialogResult.Cancel);
}