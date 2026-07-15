#nullable enable

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
	internal GeneratorsForm Gens;

	internal GeneratorForm? Editor;
	internal HelpForm ReadmeHelp;

	private int controlTabIndex;
	public MainForm() {
		InitializeComponent();
		Settings = new(this);
		Gens = new(this);
		Editor = allowEditor ? Gens.GetGen(0) : null;
		ReadmeHelp = new(this);
		Settings.Init();
		editBut.Enabled = allowEditor;
	}
	private void SetBut_Click(object sender, EventArgs e) => Settings.Show();
	private void GenBut_Click(object sender, EventArgs e) => Gens.Show();
	private void EditBut_Click(object sender, EventArgs e) => Editor?.Show();
	private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
		var cancel = false;
		foreach (var g in Gens.Gen)
			cancel |= g.Value.TryClose();
		if (e.Cancel = cancel)
			return;
		foreach (var g in Gens.Gen)
			g.Value.PerformClose();
		Settings.SaveSettings();
	}
	internal void UpdateLocale() {

		setBut.Text = L("setBut");
		genBut.Text = L("genBut");
		editBut.Text = L("editBut");
		helpBut.Text = L("helpBut");

		Text = L("appName") + " - " + version;

		controlTabIndex = 0;

		// Setup interactable controls (tooltips + tabIndex)
		SetupControl(setBut, L("setButTip"));
		SetupControl(genBut, L("genButTip"));
		SetupControl(editBut, L("editButTip"));
		SetupControl(helpBut, L("helpButTip"));

		Settings.UpdateLocale();
		Gens.UpdateLocale();
		ReadmeHelp.UpdateLocale();

	}
	void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, tip);
		control.TabIndex = ++controlTabIndex;
		//myControls.Add(control);
	}
	private void MainForm_Load(object sender, EventArgs e) { }

	private void UpdateTimer_Tick(object sender, EventArgs e) => Gens.Update(updateTimer.Interval);
}
