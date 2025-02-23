using System;
using System.Windows.Forms;

namespace RgbFractalGenCs;
internal static class Program {
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static void Main() {
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new GeneratorForm());
	}
}
