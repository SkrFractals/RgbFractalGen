namespace RgbFractalGenCs {
	internal class DoubleBufferedPanel : System.Windows.Forms.Panel {
		[System.Runtime.Versioning.SupportedOSPlatform("windows")]
		// Constructor to enable double buffering
		internal DoubleBufferedPanel() {
			SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer |
				System.Windows.Forms.ControlStyles.UserPaint |
				System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
			UpdateStyles();
		}
	}
}
