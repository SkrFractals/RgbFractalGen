#nullable enable
using System.IO;
using System.Windows.Forms;

namespace RgbFractalGenCs.Content;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class HelpForm : Form {
	private readonly MainForm root;
	public HelpForm(MainForm source) { 
		InitializeComponent();
		root = source; UpdateLocale();
	}
	internal void UpdateLocale() {
		Text = Core.StaticCore.L("help");
		var f = Core.StaticCore.SelectedLocale + ".README.txt";
		if (File.Exists(f)) 
			helpLabel.Text = File.ReadAllText(f);
		if (File.Exists("README.txt"))
			helpLabel.Text = File.ReadAllText("README.txt");
		// TODO localize this
	}
}
