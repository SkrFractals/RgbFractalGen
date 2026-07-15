using System;
using System.Numerics;
using System.Windows.Forms;
using static RgbFractalGenCs.Core.StaticCore;

namespace RgbFractalGenCs.Content.Static;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal static class StaticContent {

	internal static bool Error(string text, string caption, MessageBoxIcon ico = MessageBoxIcon.Error) {
		_ = MessageBox.Show(L(text), L(caption), MessageBoxButtons.OK, ico);
		return false;
	}

	internal static void LocComb(ComboBox o, string name, int n) {
		var c = o.Items; c.Clear();
		for (int i = 0; i < n; _ = c.Add(L(name + i++.ToString()))) { }
	}

	internal static bool Clean(TextBox
#if NULLABLE
		?
#endif       
		box) {
		if (box == null)
			return false;
		var s = box.Text;
		s = s.Replace(';', ' ').Replace('|', ' ').Replace(':', ' ').Replace(',', '.');
		if (s == box.Text)
			return false;
		box.Text = s;
		return true;
	}

	internal static T ParseValue<T>(TextBox
#if NULLABLE
		?
#endif
		box) where T : struct, IParsable<T> {
		if (box == null)
			return default;
		_ = Clean(box);
		return T.TryParse(box.Text, null, out var value) ? value : default;
	}
	/*private static byte ParseByte(TextBox box) { _ = Clean(box); return byte.TryParse(box.Text, out var v) ? v : (byte)0; }
	private static ushort ParseUShort(TextBox box) { _ = Clean(box); return ushort.TryParse(box.Text, out var v) ? v : (ushort)0; }
	private static short ParseShort(TextBox box) { _ = Clean(box); return short.TryParse(box.Text, out var v) ? v : (short)0; }
	private static int ParseInt(TextBox box) { _ = Clean(box); return int.TryParse(box.Text, out var v) ? v : 0; }
	private static double ParseDouble(TextBox box) { _ = Clean(box); return double.TryParse(box.Text, out var v) ? v : 0.0; }
	private static float ParseFloat(TextBox box) { _ = Clean(box); return float.TryParse(box.Text, out var v) ? v : 0.0f; }*/
	internal static T Clamp<T>(T n, T min, T max) where T : struct, IComparable<T>
		=> n.CompareTo(min) < 0 ? min : n.CompareTo(max) > 0 ? max : n;

	internal static T ReText<T>(TextBox
#if NULLABLE
		?
#endif       
		box, T n) where T : struct, IComparable<T>, IParsable<T> {
		if (box == null)
			return default;
		if (n.CompareTo(default) == 0) {
			if (!T.TryParse(box.Text, null, out _))
				box.Text = box.Focused ? "" : "0";
		} else {
			box.Text = n.ToString();
		}
		return n;
	}
	internal static T Mod<T>(T n, T min, T max) where T : struct, IComparable<T> {
		var d = (dynamic)max - min; while (n.CompareTo(min) < 0) n = (T)(n + d); while (n.CompareTo(max) > 0) n = (T)(n - d); return n;
	}
	internal static bool Diff<T>(T n, T gen) where T : struct, IComparable<T>
		=> gen.CompareTo(n) == 0;
	internal static bool Apply<T>(T n, out T gen) {
		gen = n;
		//QueueReset();
		return false;
	}
	internal static T ParseClampReText<T>(TextBox
#if NULLABLE
		?
#endif
		box, T min, T max) where T : struct, IComparable<T>, IParsable<T>
		=> ReText(box, Clamp(ParseValue<T>(box), min, max));
	
	internal static bool DiffApply<T>(T n, ref T gen, out T prev) where T : struct, IComparable<T> {
		prev = gen;
		return Diff(n, gen) || Apply(n, out gen);
	}
	internal static bool MaskApply(short n, ref short select, ref ulong mask) {
		if (n < 0)
			return true;
		select = n;
		mask ^= (ulong)1 << n;
		//QueueReset();
		return false;
	}
	internal static bool ClampDiffApply<T>(T n, ref T gen, T min, T max, out T prev) where T : struct, IComparable<T> 
		=> DiffApply(Clamp(n, min, max), ref gen, out prev);
	internal static bool ParseDiffApply<T>(TextBox
#if NULLABLE
		?
#endif
		box, ref T gen, out T prev) where T : struct, IComparable<T>, IParsable<T> 
		=> DiffApply(ParseValue<T>(box), ref gen, out prev);
	internal static bool ParseModDiffApply<T>(TextBox
#if NULLABLE
		?
#endif
		box, ref T gen, T min, T max, out T prev) where T : struct, IComparable<T>, IParsable<T> 
		=> DiffApply(Mod(ParseValue<T>(box), min, max), ref gen, out prev);
	internal static bool ParseClampReTextDiffApply<T>(TextBox
#if NULLABLE
		?
#endif
		box, ref T gen, T min, T max, out T prev) where T : struct, IComparable<T>, IParsable<T> 
		=> DiffApply(ParseClampReText(box, min, max), ref gen, out prev); 
	internal static bool ParseClampReTextMulDiffApply<T>(TextBox
#if NULLABLE
		?
#endif
		box, ref T gen, T min, T max, T mul, out T prev) where T : struct, INumber<T> 
		=> DiffApply(ParseClampReText(box, min, max) * mul, ref gen, out prev);
}
