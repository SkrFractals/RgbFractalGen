
#nullable enable
using System;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class ImportForm : Form {
	private int controlTabIndex;
	private readonly uint Index;
	public GeneratorForm MyForm;
	public ImportForm(GeneratorForm myForm) { InitializeComponent(); Index = (MyForm = myForm).Index; }
	public void SetUp(string codeName) {
		if (textBox.Text != codeName)
			textBox.Text = codeName;
		UpdateLocale();
	}
	private void TextBox_TextChanged(object sender, EventArgs e)
		=> MyForm.LoadCodeName(textBox.Text);
	private void CopyButton_Click(object sender, EventArgs e) 
		=> Clipboard.SetText(textBox.Text);
	private void PasteButton_Click(object sender, EventArgs e)
		=> textBox.Text = Clipboard.GetText();
	private void ImportForm_FormClosing(object sender, FormClosingEventArgs e) {
		e.Cancel = true;
		Hide();
	}
	internal void UpdateLocale() {
		UpdateName();
		controlTabIndex = 0;
		// Setup interactable controls (tooltips + tabIndex)
		SetupControl(textBox, L("importCodeTextBox"));
		SetupControl(copyButton, L("copyBox"));
		SetupControl(pasteButton, L("pasteBox"));
	}
	internal void UpdateName() {
		Text = L("appNameShort") + " - " + L("importCodeTextBox") + " - " + (MyForm.MyName == "" ? Index.ToString() : MyForm.MyName);
	}
	void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, tip);
		control.TabIndex = ++controlTabIndex;
		//myControls.Add(control);
	}
}
