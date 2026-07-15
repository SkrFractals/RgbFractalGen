using RgbFractalGenCs.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static RgbFractalGenCs.Content.Static.StaticContent;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs.Content;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class SettingsForm : Form {

	private static int controlTabIndex = 0;

	private readonly MainForm root;
	private readonly Dictionary<Control, string> myControls = [];
	private bool allowWarning = true;

	public SettingsForm(MainForm source) { root = source; InitializeComponent(); }

	public void Init() {
		SetupControl(localeBox, "localeBox");
		SetupControl(threadsBox, "threadsBox");
		SetupControl(cacheBox, "cacheBox");
		allowWarning = false;
		threadsBox.Text = Math.Max(1, MaxTasks - 2).ToString();
		LoadConfig();
		LoadLocale();
		localeBox.Items.AddRange([.. LocaleNames]);

		// Load Locale:
		if (LocaleData.Count == 0)
			throw new("No Locales found!");
		Locale = LocaleData.TryGetValue(SelectedLocale, out var found) ? found : LocaleData[SelectedLocale =
#if NULLABLE
			(string?)GetFirst(LocaleData.Keys) ?? ""];
#else
			(string)GetFirst(LocaleData.Keys)];
#endif
		//SubLocale = SubLocaleData.TryGetValue(profile.SelectedSubLocale, out var foundSub)
		//	? foundSub : SubLocaleData[profile.SelectedLocale = (string?)GetFirst(SubLocaleData.Keys) ?? ""];
		LoadResolutions();
		LoadSettings();
		ThreadsBox();
		CacheBox();
		LocaleBox();
		allowWarning = true;
	}
	internal static object
#if NULLABLE
?
#endif
	 GetFirst(IEnumerable<object> e) {
		var enumerator = e.GetEnumerator();
		return enumerator.MoveNext() ? enumerator.Current : null;
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

		// initiate from builtin file if save doesn't exist yet
		var fileR = "resolutions.txt";
		if (!File.Exists(file) && File.Exists(fileR))
			File.Copy(fileR, file);
		
		if (!File.Exists(file))
			return;
		var s = File.ReadAllLines("resolutions.txt");
		for (var i = 0; i < s.Length; i += 1) {
			if (s[i][0] == '/' || !s[i].Contains('='))
				continue;
			var c = s[i].Split('=');
			switch (c[0]) {
				case "": --i; break;
				case "resolution":
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
		File.WriteAllText(Path.Combine(GetRootSaveDir(), "palettes.txt"), file);

		// Save Resolutions
		List<string> fileRes = ["// Resolutions:"];
		for (int i = 1; ++i < ResolutionSelection.Count; fileRes.Add(ResolutionSelection[i])) { }
		File.WriteAllLines(Path.Combine(GetRootSaveDir(), "resolutions.txt"), fileRes);

		// Save Settings
		File.WriteAllText(Path.Combine(GetRootSaveDir(), "settings.txt"), 
			"locale|" + LocaleKeys[localeBox.SelectedIndex]
			+ "|threads|" + threadsBox.Text
			+ "|cache|" + (cacheBox.Checked ? 1 : 0));
	}
	internal void LoadSettings() {
		var settings = "settings.txt";
		var rootDir = GetRootSaveDir();
		var oldSettingsFile = File.Exists(settings);

		bool Is(string name, out string file) {
			file = Path.Combine(rootDir, name);
			if (!File.Exists(file) && oldSettingsFile)
				File.Copy(settings, file); // restore from old format settings
			return File.Exists(file);
		}
		if(Is("palettes.txt", out var pFile))
			LoadPalettes(File.ReadAllText(pFile).Split('|'));
		//if(Is("fractals.txt", out var file)) LoadFractals(File.ReadAllText(file).Split('|'));
		if (Is("settings.txt", out var sFile))
			LoadParams(File.ReadAllText(sFile).Split('|'));
		if (localeBox.SelectedIndex < 0)
			localeBox.SelectedIndex = 0;
	}
	private void LoadParams(string[] s) {
		for (var i = 0; i < s.Length - 1; i += 2) {
			var v = s[i + 1];
			var p = int.TryParse(v, out var n);
			switch (s[i]) {
				case "locale": localeBox.SelectedIndex = LocaleKeys.IndexOf(v); break;
				case "threads": threadsBox.Text = v; break;
				case "cache": if (p) cacheBox.Checked = n == 1; break;
				case "": --i; break;
			}
		}
	}

	private void SettingsForm_FormClosing(object s, FormClosingEventArgs e) {
		e.Cancel = true;
		Hide();
	}

	private void LocaleBox_SelectedIndexChanged(object s, EventArgs e) => LocaleBox();
	private void LocaleBox() {
		Locale = LocaleData[SelectedLocale = LocaleKeys[localeBox.SelectedIndex]];
		//Thread.CurrentThread.CurrentUICulture = new CultureInfo(SelectedLocale);
		//Thread.CurrentThread.CurrentCulture = new CultureInfo(SelectedLocale);
		root.UpdateLocale();
	}
	private void ThreadsBox_TextChanged(object s, EventArgs e) => ThreadsBox();
	private void ThreadsBox() {
		Tasks = ParseClampReText(threadsBox, (short)FractalGenerator.MinTasks, (short)Math.Max(Math.Max(FractalGenerator.MinTasks, 1), threadsMul * MaxTasks));
		if (allowWarning && Tasks > 1 && Tasks > Math.Max(Environment.ProcessorCount / 2, Environment.ProcessorCount - ReduceThreads - 2))
			_ = Error("saturatedThreads", "warning", MessageBoxIcon.Warning);
		foreach (var g in root.Gens.Gen) {
			g.Value.SetupParallel();
			// Number of threads can change the maximum of these:
			g.Value.BloomBox();
			g.Value.StripeBox();
		}
	}
	private void CacheBox_CheckedChanged(object s, EventArgs e) => CacheBox();
	private void CacheBox() {
		cacheBox.Text = (CacheChecked = cacheBox.Checked) ? "Enabled" : "Disabled";
		foreach (var g in root.Gens.Gen)
			g.Value.SetCache(CacheChecked);
	}
	internal void UpdateLocale() {
		Text = L("appName") + " - " + L("settings");
		foreach (var c in myControls)
			toolTips.SetToolTip(c.Key, c.Value);

		localeLabel.Text = L("localeLabel") + ": ";
		threadsLabel.Text = L("threadsLabel") + ": ";
		cacheLabel.Text = L("cacheLabel") + ": ";
	}

	void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		myControls.Add(control, tip);
		control.TabIndex = ++controlTabIndex;
		//myControls.Add(control);
	}

}
