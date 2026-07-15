#nullable enable

using RgbFractalGenCs.Core;
using System;
using System.IO;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;
using static RgbFractalGenCs.Content.Static.StaticContent;
using System.Collections.Generic;
namespace RgbFractalGenCs.Content;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class SettingsForm : Form {

	private static int controlTabIndex;

	private readonly MainForm root;

	public SettingsForm(MainForm source) { root = source; InitializeComponent(); }

	public void Init() {
		threadsBox.Text = MaxTasks.ToString();
		LoadConfig();
		LoadLocale();
		localeBox.Items.AddRange([.. LocaleNames]);

		// Load Locale:
		if (LocaleData.Count == 0)
			throw new("No Locales found!");
		Locale = LocaleData.TryGetValue(SelectedLocale, out var found)
			? found : LocaleData[SelectedLocale = (string?)GetFirst(LocaleData.Keys) ?? ""];
		//SubLocale = SubLocaleData.TryGetValue(profile.SelectedSubLocale, out var foundSub)
		//	? foundSub : SubLocaleData[profile.SelectedLocale = (string?)GetFirst(SubLocaleData.Keys) ?? ""];
		LoadResolutions();
		LoadSettings();
		ThreadsBox();
		CacheBox();
		LocaleBox();
	}
	internal static object? GetFirst(IEnumerable<object> e) {
		var enumerator = e.GetEnumerator();
		return enumerator.MoveNext() ? enumerator.Current : null;
	}
	private void ThreadsBox_TextChanged(object sender, EventArgs e) => ThreadsBox();
	private void ThreadsBox() {
		Tasks = ParseClampReText(threadsBox, (short)FractalGenerator.MinTasks, (short)Math.Max(1, threadsMul * MaxTasks));
		foreach (var g in root.Gens.Gen) {
			g.Value.SetupParallel();
			// Number of threads can change the maximum of these:
			g.Value.BloomBox();
			g.Value.StripeBox();
		}
	}
	private void CacheBox_CheckedChanged(object sender, EventArgs e) => CacheBox();
	private void CacheBox() {
		cacheBox.Text = (CacheChecked = cacheBox.Checked) ? "Enabled" : "Disabled";
		foreach (var g in root.Gens.Gen) 
			g.Value.SetCache(CacheChecked);
	}
	private static void LoadConfig() {
		if (!File.Exists("config.txt"))
			return;
		var s = File.ReadAllLines("config.txt");

		//var s = File.ReadAllText("config.txt").Split('|');
		for (var i = 0; i < s.Length; i += 1) {
			if (s[i][0] == '/' || !s[i].Contains('='))
				continue;
			var c = s[i].Split('=');
			bool isN = ushort.TryParse(c[1], out var n);
			if (isN) {
				switch (c[0]) {
					case "voidAmbientMax": voidAmbientMax = n; break;
					case "voidAmbientMul": voidAmbientMul = n; break;
					case "voidNoiseMax": voidNoiseMax = n; break;
					case "voidScaleMax": voidScaleMax = n; break;
					case "detailMax": detailMax = n; break;
					case "saturateMax": saturateMax = n; break;
					case "brightnessMax": brightnessMax = n; break;
					case "binMax": binMax = n; break;
					case "L2Max": l2Max = n; break;
					case "blurMax": blurMax = n; break;
				}
			}
			bool isF = float.TryParse(c[1], out var f);
			if (isF) {
				switch (c[0]) {
					case "detailMul": detailMul = f; break;
					case "bloomMul": bloomMul = f; break;
					case "voidNoiseMul": voidNoiseMul = f; break;
					case "threadsMul": threadsMul = f; break;
				}
			}
		}
	}
	private static void LoadResolutions() {

		ResolutionSelection.Add("80x80");
		ResolutionSelection.Add("Custom:");

		var settings = "settings.txt";
		var root = File.Exists(settings);
		var file = Path.Combine(GetRootSaveDir(), "resolutions.txt");
		if (!File.Exists(file) && root)
			File.Copy(settings, file);
		if (!File.Exists(file))
			return;
		var s = File.ReadAllLines("resolutions.txt");
		for (var i = 0; i < s.Length; i += 1) {
			if (s[i][0] == '/' || !s[i].Contains('='))
				continue;
			var c = s[i].Split('=');
			if (c[0] == "resolution") {
				// skip and continue if the resolution is not a valid format <int>x<int>
				if (!c[1].Contains('x'))
					continue;
				var r = c[1].Split('x');
				if (r.Length != 2 || !(int.TryParse(r[0], out _) && int.TryParse(r[1], out _)))
					continue;
				if (!ResolutionSelection.Contains(c[1]))
					ResolutionSelection.Add(c[1]);
				continue;
			}
		}
	}
	internal void SaveSettings() {

		// Save Palettes
		var file = "";
		foreach (var c in Colors) {
			var p = "palette|" + c.Item1 + ";";
			foreach (var i in c.Item2)
				p += i.X + ":" + i.Y + ":" + i.Z + ";";
			file += p[..^1] + "|";
		}
		File.WriteAllText(Path.Combine(GetRootSaveDir(), "palette.txt"), file);

		// Save Resolutions
		List<string> fileRes = ["// Resolutions:"];
		for (int i = 1; ++i < ResolutionSelection.Count; fileRes.Add(ResolutionSelection[i])) { }
		File.WriteAllLines(Path.Combine(GetRootSaveDir(), "resolutions.txt"), fileRes);

		// Save Settings
		File.WriteAllText(Path.Combine(GetRootSaveDir(), "settings.txt"),
			"|locale|" + LocaleKeys[localeBox.SelectedIndex]
			+ "|threads|" + threadsBox.Text
			+ "|cache|" + (cacheBox.Checked ? 1 : 0));
	}
	internal void LoadSettings() {
		var settings = "settings.txt";
		var root = File.Exists(settings);

		var file = Path.Combine(GetRootSaveDir(), "palettes.txt");
		if (!File.Exists(file) && root)
			File.Copy(settings, file);
		if (File.Exists(file))
			LoadPalettes(File.ReadAllText(file).Split('|'));

		/*file = Path.Combine(GetRootSaveDir(), "fractals.txt");
		if (!File.Exists(file) && root)
			File.Copy(settings, file);
		if (File.Exists(file))
			LoadFractals(File.ReadAllText(file).Split('|'));*/

		file = Path.Combine(GetRootSaveDir(), settings);
		if (!File.Exists(file) && root)
			File.Copy(settings, file);
		if (File.Exists(file))
			LoadParams(File.ReadAllText(file).Split('|'));

		if (localeBox.SelectedIndex < 0)
			localeBox.SelectedIndex = 0;
	}
	private void LoadParams(string[] s) {
		for (var i = 0; i < s.Length - 1; i += 2) {
			var v = s[i + 1];
			var p = int.TryParse(v, out var n);
			switch (s[i]) {
				case "locale": localeBox.SelectedItem = v; break;
				case "threads": threadsBox.Text = v; break;
				case "cache": if (p) cacheBox.Checked = n == 1; break;
			}
		}
	}

	private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
		e.Cancel = true;
		Hide();
	}

	private void LocaleBox_SelectedIndexChanged(object sender, EventArgs e) => LocaleBox();
	private void LocaleBox() {
		Locale = LocaleData[SelectedLocale = LocaleKeys[localeBox.SelectedIndex]];
		root.UpdateLocale();
	}
	internal void UpdateLocale() {
		localeLabel.Text = L("localeLabel") + ": ";
		threadsLabel.Text = L("threadsLabel") + ": ";
		cacheLabel.Text = L("cacheLabel") + ": ";

		controlTabIndex = 0;
		SetupControl(localeBox, L("localeBox"));
		SetupControl(threadsBox, L("threadsBox"));
		SetupControl(cacheBox, L("cacheBox"));

		Text = L("appName") + " - " + L("settings");
	}

	void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, tip);
		control.TabIndex = ++controlTabIndex;
		//myControls.Add(control);
	}

}
