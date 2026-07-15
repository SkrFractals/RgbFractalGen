using System.Windows.Forms;

namespace RgbFractalGenCs.Content.Basic;

internal class DoubleBufferedPanel : Panel {
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	// Constructor to enable double buffering
	internal DoubleBufferedPanel() {
		SetStyle(ControlStyles.DoubleBuffer |
			ControlStyles.UserPaint |
			ControlStyles.AllPaintingInWmPaint, true);
		UpdateStyles();
	}
}
