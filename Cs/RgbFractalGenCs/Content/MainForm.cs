using RgbFractalGenCs.Content;
using System;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class MainForm : Form {

	const string version = "1.14.0";
	const bool allowEditor = false;

	internal SettingsForm Settings;
	internal HelpForm ReadmeHelp;
	internal GeneratorsForm Gens;
	internal GeneratorForm
#if NULLABLE
	 ?
#endif
	Editor;
	
	private readonly System.Collections.Generic.Dictionary<Control, string> myControls = [];
	private int controlTabIndex = 0;
	
	public MainForm() {
		InitializeComponent();

		void SetupControl(Control control, string tip) {
			control.TabIndex = ++controlTabIndex;
			myControls.Add(control, tip);
		}
		SetupControl(setBut, "setBut");
		SetupControl(genBut, "genBut");
		SetupControl(editBut, "editBut");
		SetupControl(helpBut, "helpBut");

		Settings = new(this);
		Gens = new(this);
		Editor = allowEditor ? Gens.GetGen(0) : null;
		ReadmeHelp = new(this);
		Settings.Init();
		editBut.Enabled = allowEditor;
	}

	internal void UpdateLocale() {
		Text = L("appName") + " - " + version;
		foreach (var c in myControls) {
			c.Key.Text = L(c.Value + "Text");
			toolTips.SetToolTip(c.Key, c.Value);
		}
		Settings.UpdateLocale();
		Gens.UpdateLocale();
		ReadmeHelp.UpdateLocale();
	}

	#region Events
	private void UpdateTimer_Tick(object s, EventArgs e)
		=> Gens.Update(updateTimer.Interval);
	private void SetBut_Click(object s, EventArgs e) { Settings.Show(); Settings.Location = Location; }
	private void GenBut_Click(object s, EventArgs e) { Gens.Show(); Gens.Location = Location; }
	private void EditBut_Click(object s, EventArgs e) {
		if (Editor != null) {
			Editor.Show(); 
			Editor.Location = Location;
		}
	}
	private void HelpBut_Click(object sender, EventArgs e) { ReadmeHelp.Show(); ReadmeHelp.Location = Location; }
	private void MainForm_FormClosing(object s, FormClosingEventArgs e) {
		var cancel = false;
		foreach (var g in Gens.Gen)
			cancel |= g.Value.TryClose();
		if (e.Cancel = cancel)
			return;
		foreach (var g in Gens.Gen)
			g.Value.PerformClose();
		Settings.SaveSettings();
	}
	#endregion
}
