#nullable enable

using System;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs.Content;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class InputDialog : Form {
	private int controlTabIndex;
	public InputDialog() {
		InitializeComponent(); 
		controlTabIndex = 0;
		SetupControl(textBox, (string?)Tag ?? "");
		SetupControl(cancelButton, "cancelBox");
		SetupControl(pasteButton, "pasteBox");
		SetupControl(okButton, "okBox");
	}
	private void Finish(DialogResult r) {
		DialogResult = r;
		Close();
	}
	internal string GetText() => textBox.Text;
	private void CancelButton_Click(object sender, EventArgs e) => Finish(DialogResult.Cancel);
	private void PasteButton_Click(object sender, EventArgs e) => textBox.Text = Clipboard.GetText();
	private void OkButton_Click(object sender, EventArgs e) => Finish(DialogResult.OK);
	private void TextBox_TextChanged(object sender, EventArgs e)
		=> Static.StaticContent.Clean(textBox);
	void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, L(tip));
		control.TabIndex = ++controlTabIndex;
		//myControls.Add(control);
	}
}
