using System;
using System.ComponentModel;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class ImportForm : Form {

	internal GeneratorForm MyForm;

	private int controlTabIndex = 0;
	private readonly uint Index;
	private readonly System.Collections.Generic.Dictionary<Control, string> myControls = [];

	public ImportForm(GeneratorForm myForm) { 
		InitializeComponent();
		Index = (MyForm = myForm).Index;

		controlTabIndex = 0;
		// Setup interactable controls (tooltips + tabIndex)
		void SetupControl(Control control, string tip) {
			myControls.Add(control, tip);
			control.TabIndex = ++controlTabIndex;
		}
		SetupControl(textBox, "importCodeTextBox");
		SetupControl(copyButton, "copyBox");
		SetupControl(importFileButton, "importFileButton");
		SetupControl(pasteButton, "pasteBox");
	}

	#region Update
	internal void Update(string codeName) {
		if (textBox.Text != codeName)
			textBox.Text = codeName;
		UpdateLocale();
	}
	internal void UpdateLocale() {
		UpdateName();
		foreach(var c in myControls)
			toolTips.SetToolTip(c.Key, c.Value);
		copyButton.Text = L("copy");
		importFileButton.Text = L("importFileButtonText");
		pasteButton.Text = L("paste");
		loadExport.Filter = L("allFiles") + "(*.*)|*.*";
	}
	internal void UpdateName() {
		Text = L("appNameShort") + " - " + L("importCodeTextBox") + " - " + (MyForm.MyName == "" ? Index.ToString() : MyForm.MyName);
	}
	#endregion

	#region Events
	private void TextBox_TextChanged(object sender, EventArgs e)
	=> MyForm.LoadCodeName(textBox.Text);
	private void CopyButton_Click(object sender, EventArgs e)
		=> Clipboard.SetText(textBox.Text);
	private void PasteButton_Click(object sender, EventArgs e)
		=> textBox.Text = Clipboard.GetText();
	private void FileButton_Click(object sender, EventArgs e) {
		loadExport.DefaultExt = "";
		loadExport.Filter = L("allFiles") + " (*.*)|*.*";
		_ = loadExport.ShowDialog();
	}
	private void LoadExport_FileOk(object sender, CancelEventArgs e)
		=> MyForm.LoadCodeName(System.IO.Path.GetFileNameWithoutExtension(loadExport.FileName));
	private void ImportForm_FormClosing(object sender, FormClosingEventArgs e) {
		e.Cancel = true;
		Hide();
	}
	#endregion
}
