using System;
using System.Windows.Forms;

namespace RgbFractalGenCs;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class InputCode : Form {
	public GeneratorForm MyForm = null;
	public InputCode() => InitializeComponent();
	public void SetUp(GeneratorForm myForm, string codeName) {
		MyForm = myForm;
		textBox.Text = codeName;
	}
	private void pasteButton_Click(object sender, EventArgs e)
		=> textBox.Text = Clipboard.GetText();
	private void textBox_TextChanged(object sender, EventArgs e) 
		=> MyForm.LoadCodeName(textBox.Text);
}
