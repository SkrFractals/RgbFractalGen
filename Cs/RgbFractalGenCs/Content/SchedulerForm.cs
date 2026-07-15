using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs.Content;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class SchedulerForm : Form {
	const int buttonSize = 23;

	internal bool Running = false;

	private int pointTabIndex = 0;
	private readonly Dictionary<Control, string>
		myEditControls = [];        // All persistent interactive controls (Editor)
	internal List<(Label t, Label s, Button r, Label f)>  // type state REMOVE (switch) filename : generator line buttons
		lineList = [];
	private readonly List<Button>
		lineListSwitch = [];        // switch: Switch buttons
	private readonly GeneratorForm gen;
	public SchedulerForm(GeneratorForm owner) {
		InitializeComponent();
		gen = owner;
	}
	internal void UpdateRunning() {
		if (gen.Scheduled.Count == 0 || !Running)
			return;
		// block swap (not delete, that will just cancel)
		//lineList[0].r.Enabled = false;
		if (lineListSwitch.Count > 0)
			lineListSwitch[0].Enabled = false;
		// update the status:
		var s = lineList[0].s;
		s.Tag = true;
		s.Text = gen.infoText;
	}
	internal void AddListEntry(GeneratorForm.Schedule task, bool single = true) {
		var i = lineList.Count;
		lineList.Add((new(), new(), new(), new()));
		var (t, s, r, f) = lineList[i];
		t.Tag = task.Type;
		t.Text = L("exportSelect" + ((byte)t.Tag).ToString());
		s.Tag = false;
		s.Text = L("debugQueued");
		r.Text = "X";
		f.Text = task.Filename;
		BindListEntry((t, s, r, f), i, single);
		UpdateRunning();
	}
	internal void FillListEntries() {
		pointPanel.SuspendLayout();
		UnFillListEntries();
		for(int i = 0; i < gen.Scheduled.Count; ++i)
			AddListEntry(gen.Scheduled[i], false);
		SetMinimumSize();
		pointPanel.ResumeLayout(false);
		pointPanel.PerformLayout();
		UpdateRunning();
	}
	private void UnFillListEntries() {
		foreach (var s in lineListSwitch) { 
			pointPanel.Controls.Remove(s); 
			s.Dispose();
		}
		lineListSwitch.Clear();
		void R(Control _c) {
			pointPanel.Controls.Remove(_c);
			_c.Dispose();
		}
		foreach (var (t, s, r, f) in lineList) {
			R(t); R(s); R(r); R(f);
		}
		lineList.Clear();
		pointTabIndex = 0;
		myEditControls.Clear();
	}
	private void BindListEntry((Label t, Label s, Button r, Label f) l, int i, bool single = true) {
		const int textSize = 128;

		if (single)
			pointPanel.SuspendLayout();
		var hor = 0;
		void NewControl(Control _c, int size, string name) {
			_c.Location = new Point(2 + hor, 2 + i * buttonSize);
			_c.Margin = new Padding(4, 3, 4, 3);
			_c.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 238);
			_c.Size = new Size(size, buttonSize);
			_c.Name = name;
			pointPanel.Controls.Add(_c);
			//myControls.Add(_c);
			hor += size + 4;
		}
		Button NewButton(Button _b, string name, string tool) {
			NewControl(_b, buttonSize, name);
			_b.BackColor = Color.FromArgb(192, 192, 192);
			SetupEditControl(_b, tool);
			return _b;
		}
		Label NewLabel(Label _l, int size, string name) {
			NewControl(_l, size, name);
			_l.ForeColor = Color.White;
			return _l;
		}
		// Swap:
		if (i > 0) {
			lineListSwitch.Add(new());
			var s = lineListSwitch[^1];
			s.Text = "⇕";
			NewButton(s, "s" + i.ToString(), "swapScheduled").Click += (_, _) => {
				(gen.Scheduled[i], gen.Scheduled[i - 1]) = (gen.Scheduled[i - 1], gen.Scheduled[i]);
				FillListEntries();
			};
			s.Location = new(3 * (textSize + 4) + 2 * (buttonSize + 4), 2 + (i * 2 - 1) * (buttonSize + 4) / 2);
		}
		hor = 0;
		// (type, status, remove, filename):
		_ = NewLabel(l.t, textSize, "t" + i.ToString());
		_ = NewLabel(l.s, textSize, "s" + i.ToString());
		NewButton(l.r, "r" + i.ToString(), "removeScheduled").Click += (_, _) => {
			if (i == 0 && Running) {
				// TODO make sure the task doesn't getfnihsed before the user clicks Yes so it won't cancel the next one or access l.r==null or something
				if (gen.CancelExport())
					l.r.Enabled = false;
				l.r.Text = L("cancelling");
			} else {
				gen.Scheduled.RemoveAt(i);
				FillListEntries();
			}
		};
		NewLabel(l.f, buttonSize, "f" + i.ToString()).AutoSize = true;
		if (single) {
			pointPanel.ResumeLayout(false);
			pointPanel.PerformLayout();
		}
	}
	private void SetMinimumSize() => MinimumSize =
		new(MinimumSize.Width, 12 + lineList.Count * buttonSize + Height - ClientRectangle.Height);
	private void SetupEditControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		myEditControls.Add(control, tip);
		toolTips.SetToolTip(control, L(tip));
		control.TabIndex = ++pointTabIndex;
	}
	internal void UpdateLocale() {
		foreach (var (t, s, r, _) in lineList) {
			t.Text = L("exportSelect" + ((byte)t.Tag).ToString());
			if (s.Tag is bool state)
				s.Text = state ? gen.infoText : L("debugQueued");
			/*if (s.Tag is FractalGenerator.ScheduledTask type)
				toolTips.SetToolTip(t, type switch { 
					_ => ""
				});
			toolTips.SetToolTip(s, L("stateScheduled"));*/
			toolTips.SetToolTip(r, L("removeScheduled"));
		}
		foreach (var s in lineListSwitch)
			toolTips.SetToolTip(s, L("swapScheduled"));
	}
}
