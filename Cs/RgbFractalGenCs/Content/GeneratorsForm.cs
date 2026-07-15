#nullable enable
using RgbFractalGenCs.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;
//using static RgbFractalGenCs.Content.Static.StaticContent;

namespace RgbFractalGenCs;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class GeneratorsForm : Form {

	private readonly MainForm root;
	private int controlTabIndex = 0, pointTabIndex;
	private readonly List<Control>
		myControls = [];            // All persistent interactive controls (Generator)
	private readonly List<string>
		myTips = [];                // Locale codes (Generator)
	private readonly List<Control>
		myEditControls = [];        // All persistent interactive controls (Editor)
	private readonly List<string>
		myEditTips = [];            // Locale codes (Editor)

	internal Dictionary<uint, GeneratorForm> Gen = [];
	internal Dictionary<int, uint> GenLineToKey = [];
	internal static FractalGenerator? Priority = null;
	internal List<(Button, Button, Button, Label, Button, Button)>
		lineList = [];              // delete tasks state of duplicate name : generator line buttons
	//private readonly List<Button>
	//	lineListSwitch = [];        // switch: Switch buttons, not implemented becaues it's a dictionary, not a list

	public GeneratorsForm(MainForm source) {
		InitializeComponent();
		root = source;
		var dir = GetGensSaveDir();
		var files = Directory.GetFiles(dir);

		pointPanel.SuspendLayout();
		foreach (var f in files)
			if (uint.TryParse(Path.GetFileNameWithoutExtension(f), out uint i))
				_ = GetGen(i, false); // load all saved gens
		SetupControl(addButton, "addGenerator");
		pointPanel.ResumeLayout(false);
		pointPanel.PerformLayout();
	}
	internal void Update(int Interval) {

		//return;

		FractalGenerator? priority = null;
		foreach (var g in Gen) {
			var form = g.Value;
			FractalGenerator gen;
			if (form == null)
				continue;
			if (form.Visible) // Animate popup/toolbar of that generator
				form.UpdatePopups(1.0f / Interval);
			if ((gen = form.generator).mainTask == null)
				continue; // not generating at all or finished - do not count
			if (gen.GetGenerateAnimation())
				priority = gen; // is running and wants to generate animation
			if (gen.GetBitmapsFinished() < 1)
				break; // preview/first frames have forced priority (no animation get will be active if any active generator dones't have a first frame yet)
		}
		UpdateLines(false);
		Priority = priority;
	}
	private GeneratorForm NewGen(GeneratorForm? from = null) {
		uint i = 1;
		while (IsGen(i))
			++i;
		return GetGen(i, true, from);
	}
	private static bool IsGen(uint index) {
		var dir = GetGensSaveDir();
		var file = Path.Combine(dir, index.ToString() + ".gen");
		return File.Exists(file);
	}
	internal GeneratorForm GetGen(uint index, bool single = true, GeneratorForm? from = null) {
		if (!Gen.TryGetValue(index, out var gen)) {
			Gen.Add(index, gen = new(root, this, index, from));
			gen.SetEditor(index == 0);
			AddListEntry(index, gen, single);
			//++lines;
		}
		return gen;
	}
	private void RemoveGen(uint key, GeneratorForm gen) {
		if (gen.TryClose())
			return; // cancelled closing
		gen.PerformClose();
		_ = Gen.Remove(key);
		var dir = GetGensSaveDir();
		var file = Path.Combine(dir, key.ToString() + ".gen");
		if (File.Exists(file))
			File.Delete(file);
		FillListEntries(); // refill the lines
	}
	internal void UpdateLines(bool genFormsToo) {
		for(int i = 0; i < lineList.Count; ++i)
			if (GenLineToKey.TryGetValue(i, out var key) && Gen.TryGetValue(key, out var gen))
				LabelLine(gen, lineList[i], genFormsToo);
	}
	internal void UpdateLocale() {
		UpdateName();
		addButton.Text = L("addGenerator");
		for (int i = myControls.Count; 0 <= --i; toolTips.SetToolTip(myControls[i], L(myTips[i]))) { }
		for (int i = myEditControls.Count; 0 <= --i; toolTips.SetToolTip(myEditControls[i], L(myEditTips[i]))) { }
		UpdateLines(true);
	}
	private void UpdateName() => Text = L("appNameShort") + " - " + L("generators");
	private void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, L(tip));
		control.TabIndex = ++controlTabIndex;
		myControls.Add(control);
		myTips.Add(tip);
	}
	private void SetupEditControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		toolTips.SetToolTip(control, L(tip));
		control.TabIndex = ++pointTabIndex;
		myEditControls.Add(control);
		myEditTips.Add(tip);
	}
	private void FillListEntries() {
		pointPanel.SuspendLayout();
		UnFillListEntries();
		//for (var i = 0; i < lines; ++i)
		foreach(var dirGen in Gen)
			if(dirGen.Key != 0) // don't count Editor
				AddListEntry(dirGen.Key, dirGen.Value, false);
		pointPanel.ResumeLayout(false);
		pointPanel.PerformLayout();
	}
	private void UnFillListEntries() {

		// switch not implemented because it's a dictionary, not a list
		//foreach (var s in lineListSwitch) { pointPanel.Controls.Remove(s); _ = myControls.Remove(s); }
		//lineListSwitch.Clear();

		void R(Control _c) {
			pointPanel.Controls.Remove(_c);
			_ = myControls.Remove(_c);
			_c.Dispose();
		}
		foreach (var (r, t, s, o, d, g) in lineList) {
			R(r); R(t); R(s); R(o); R(g); R(d);
		}
		GenLineToKey.Clear();
		lineList.Clear();
		pointTabIndex = controlTabIndex;
		myEditControls.Clear();
		myEditTips.Clear();
	}
	private static void LabelLine(GeneratorForm gen, (Button r, Button t, Button s, Label o, Button d, Button g) l, bool genFormsToo = false) {
		l.t.Text = L("tasks") + ": " + gen.Scheduled.Count.ToString();
		l.s.Text = gen.statusText;
		l.s.BackColor = gen.Working ? Color.FromArgb(0, 255, 0) : Color.FromArgb(255, 0, 0);
		l.o.Text = gen.infoText; // how many frames are proicessed in the current generator/scheduled task?
		l.g.Text = gen.GetName();
		if (genFormsToo)
			gen.UpdateLocale();
	}
	private void AddListEntry(uint index, GeneratorForm gen, bool single = true) {
		var i = lineList.Count;
		GenLineToKey.Add(i, index);
		var l = (new Button(), new Button(), new Button(), new Label(), new Button(), new Button());
		lineList.Add(l);
		BindListEntry(l, i, index, gen, single);
		LabelLine(gen, l);
		//EditFractal();
	}
	private void BindListEntry((Button r, Button t, Button s, Label o, Button d, Button g) l, int i, uint key, GeneratorForm gen, bool single = true) {
		//const int textSize = 53;
		const int buttonSize = 23;
		if (single)
			pointPanel.SuspendLayout();
		int hor = 0;
		void NewControl(Control _c, int size, string name) {
			_c.Location = new Point(2 + hor, addButton.Location.Y + addButton.Size.Height + 2 + i * buttonSize);
			_c.Margin = new Padding(4, 3, 4, 3);
			_c.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
			_c.Size = new Size(size, buttonSize);
			_c.Name = name;
			pointPanel.Controls.Add(_c);
			//myControls.Add(_c);
			hor += size + 4;
		}
		Button NewButton(Button _b, int size, string name, string tool) {
			NewControl(_b, size, name);
			_b.BackColor = Color.FromArgb(192, 192, 192);
			SetupEditControl(_b, tool);
			return _b;
		}
		Label NewLabel(Label _l, int size, string name) {
			NewControl(_l, size, name);
			return _l;
		}
		// Switch: // cannot swtich generators, they are dictionaries
		/*if (i > 0) {
			lineListSwitch.Add(new());
			var s = lineListSwitch[^1];
			s.Text = "⇕";
			NewButton(s, buttonSize, "s" + i.ToString(), "genSwitch").Click += (_, _) => { swap };
		} hor = 0;*/

		// (remove, tasks, status, of, duplicate, generator):
		NewButton(l.r, buttonSize, "r" + i.ToString(), "removeGenerator").Click += (_, _) => RemoveGen(key, gen);
		l.r.Text = "X";
		NewButton(l.t, 128, "t" + i.ToString(), "tasksTip").Click += (_, _) => gen.OpenTasks();
		l.t.Text = L("tasks");
		NewButton(l.s, 128, "s" + i.ToString(), "pauseGeneratorTip").Click += (_, _) => gen.SetWorking(!gen.Working);
		_ = NewLabel(l.o, 64, "o" + i.ToString());
		NewButton(l.d, buttonSize, "d" + i.ToString(), "duplicateGeneratorTip").Click += (_, _) => NewGen(gen);
		l.d.Text = "⧉";
		NewButton(l.g, pointPanel.Width - (4 + hor), "g" + i.ToString(), "openGeneratorTip").Click += (_, _) => { gen.SetWorking(true); gen.Show(); };
		l.g.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // stretching to the rest ofthe window

		if (single) {
			pointPanel.ResumeLayout(false);
			pointPanel.PerformLayout();
		}
	}
	private void GeneratorsForm_FormClosing(object sender, FormClosingEventArgs e) {
		e.Cancel = true;
		Hide();
	}

	private void AddButton_Click(object sender, EventArgs e) => _ = NewGen();

	//private void GeneratorsForm_SizeChanged(object sender, EventArgs e) 
	//	=> pointPanel.Size = new(Width - 16 - 8, Height - 39 - 8);
}
