// Issues:

// Crashes when aborting with Paralel Iterations
// Sometimes crashes when generating with Paralel Iteration
// Paralel iterations make it like 20x slower instead of faster
// ven single threaded generation is somehow slower that the managed versions

// TODO:

// Debug Performance
// Replace Aborts with something more graceful

#pragma once
#include "FractalGenerator.h"
#include "DoubleBufferedPanel.h"

namespace RgbFractalGenClr {

	public ref class GeneratorForm : public System::Windows::Forms::Form {

#pragma region Designer
	public:
		GeneratorForm(void);
	protected:
		~GeneratorForm();

	private: System::Windows::Forms::ToolTip^ toolTips;
	private: System::Windows::Forms::Timer^ timer;
	private: System::Windows::Forms::SaveFileDialog^ savePng;
	private: System::Windows::Forms::SaveFileDialog^ saveGif;
	private: System::Windows::Forms::Label^ fractalLabel;
	private: System::Windows::Forms::ComboBox^ fractalSelect;

	private: System::Windows::Forms::TextBox^ resX;
	private: System::Windows::Forms::TextBox^ resY;
	private: System::Windows::Forms::CheckBox^ previewBox;
	private: System::Windows::Forms::TextBox^ periodBox;
	private: System::Windows::Forms::Label^ delayLabel;
	private: System::Windows::Forms::TextBox^ delayBox;
	private: System::Windows::Forms::Button^ zoomButton;


	private: System::Windows::Forms::Button^ prevButton;
	private: System::Windows::Forms::Button^ nextButton;
	private: System::Windows::Forms::Button^ animateButton;
	private: System::Windows::Forms::Label^ ambLabel;
	private: System::Windows::Forms::TrackBar^ ambBar;
	private: System::Windows::Forms::Label^ noiseLabel;
	private: System::Windows::Forms::TrackBar^ noiseBar;
	private: System::Windows::Forms::Label^ detailLabel;
	private: System::Windows::Forms::TrackBar^ detailBar;
	private: System::Windows::Forms::Label^ saturateLabel;
	private: System::Windows::Forms::TrackBar^ saturateBar;
	private: System::Windows::Forms::Label^ threadsLabel;
	private: System::Windows::Forms::TrackBar^ threadsBar;
	private: System::Windows::Forms::CheckBox^ parallelBox;
	private: System::Windows::Forms::CheckBox^ parallelTypeBox;
	private: System::Windows::Forms::Label^ statusLabel;
	private: System::Windows::Forms::Label^ infoLabel;
	private: System::Windows::Forms::Button^ pngButton;
	private: System::Windows::Forms::Button^ gifButton;
	private: System::Windows::Forms::TrackBar^ blurBar;
	private: System::Windows::Forms::Label^ blurLabel;
	private: System::Windows::Forms::TextBox^ defaultZoom;
	private: System::Windows::Forms::TextBox^ defaultAngle;
	private: System::Windows::Forms::Button^ encodeButton;
	private: System::Windows::Forms::TrackBar^ cutparamBar;
	private: System::Windows::Forms::TextBox^ cutparamBox;
	private: System::Windows::Forms::TextBox^ defaultHue;
	private: System::Windows::Forms::TextBox^ periodMultiplierBox;
	private: System::Windows::Forms::Label^ periodLabel;
	private: System::Windows::Forms::Panel^ helpPanel;

	private: System::Windows::Forms::Label^ helpLabel;

	private: System::Windows::Forms::ComboBox^ angleSelect;
	private: System::Windows::Forms::ComboBox^ colorSelect;
	private: System::Windows::Forms::ComboBox^ cutSelect;
	private: System::Windows::Forms::Button^ helpButton;

	private: System::Windows::Forms::Label^ angleLabel;
	private: System::Windows::Forms::Label^ colorLabel;
	private: System::Windows::Forms::Label^ cutLabel;
	private: System::Windows::Forms::Label^ zoomLabel;
	private: System::Windows::Forms::ComboBox^ spinSelect;

	private: System::Windows::Forms::ComboBox^ hueSelect;

	private: System::Windows::Forms::Label^ spinLabel;
	private: System::Windows::Forms::Label^ label1;
	private: System::Windows::Forms::TextBox^ spinSpeedBox;
	private: System::Windows::Forms::TextBox^ hueSpeedBox;






	private: System::ComponentModel::IContainer^ components;

	protected:

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
#pragma endregion

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void);
#pragma endregion

#pragma region Variables
	private:
		// Threading
		FractalGenerator^ generator;
		CancellationTokenSource^ cancel;
		Task^ gTask;						// GIF Export Task
		// Settings
		bool previewMode = true;			// Preview mode for booting performance while setting up parameters
		bool animated = true;				// Animating preview or paused? (default animating)
		bool modifySettings = true;			// Allows for modifying settings without it triggering Aborts and Generates
		System::String^ gifPath = "";		// Gif export path name
		// Display  Variables
		DoubleBufferedPanel^ screenPanel;	// Display panel
		Bitmap^ currentBitmap = nullptr;	// Displayed Bitmap
		int currentBitmapIndex;				// Play frame index
		int fx, fy;							// Memory of window size
		int controlTabIndex = 0;
#pragma endregion

#pragma region Core
	private:
		/// <summary>
		/// Initializes all the variables, and the fractal generator
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void GeneratorForm_Load(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Refreshing and preview animation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void timer_Tick(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Cancels all the threads so it can close gracefully
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void GeneratorForm_FormClosing(System::Object^ sender, System::Windows::Forms::FormClosingEventArgs^ e);
		/// <summary>
		/// Cancel all the threads
		/// </summary>
		/// <returns></returns>
		System::Void Abort();
		/// <summary>
		/// Resets the generator, so it starts generating from frame 0 again
		/// </summary>
		/// <returns></returns>
		System::Void ResetGenerator();
		/// <summary>
		/// Restarts the generator (needs to call Abort before)
		/// </summary>
		/// <returns></returns>
		System::Void ResetGenerate();
		/// <summary>
		/// Resizes the resolution, and restarts the generator (needs to call Abort before)
		/// </summary>
		/// <returns></returns>
		System::Void ResizeGenerate();
		/// <summary>
		/// Aborts the generator, resizes the resolution, and restarts the generator
		/// </summary>
		/// <returns></returns>
		System::Void AbortGenerate();
		/// <summary>
		/// Setups the tooltip and tabindex of the interactable control
		/// </summary>
		System::Void SetupControl(Control^ control, System::String^ tip);
#pragma endregion

#pragma region Size
	private:
		/// <summary>
		/// Call this after changing the resolution settings, it computes the new desired resolution, resizes the window and display, and resets the generator 
		/// </summary>
		/// <returns></returns>
		System::Void ResizeAll();
		/// <summary>
		/// Fetch the resolution
		/// </summary>
		/// <returns>Changed</returns>
		bool TryResize();
		/// <summary>
		/// This continuously checks for user manually resizing the window, and it will stretch the display to fit it
		/// </summary>
		/// <returns></returns>
		System::Void WindowSizeRefresh();
		/// <summary>
		/// Remembers thewindow size
		/// </summary>
		/// <returns></returns>
		System::Void SizeAdapt();
		/// <summary>
		/// Sets the minimum window size that could still fit the display
		/// </summary>
		/// <returns></returns>
		System::Void SetMinimumSize();
		/// <summary>
		/// Resizes the display to fit the window
		/// </summary>
		/// <returns></returns>
		System::Void ResizeScreen();
#pragma endregion

#pragma region Input
	private:
		System::Void fractalBox_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectFractal();
		System::Void FillSelects();
		System::Void angleSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void colorSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void cutSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void FillCutParams();
		System::Void cutparamBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void cutparamBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		System::Void resX_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void resY_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void previewBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void periodBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectPeriod();
		System::Void periodMultiplierBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void zoomButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectZoom();
		System::Void defaultZoom_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void spinSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void spinSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void defaultAngle_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void hueSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void hueSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void defaultHue_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void ambBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectAmb();
		System::Void noiseBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectNoise();
		System::Void saturateBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectSaturation();
		System::Void detailBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectDetail();
		System::Void blurBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectBlur();
		System::Void parallelBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectMaxThreads();
		System::Void parallelTypeBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void SelectParallelType();
		System::Void threadsBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		System::Void delayBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void prevButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void nextButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void animateButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void encodeButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void helpButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void pngButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void gifButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void screenPanel_Paint(System::Object^ sender, System::Windows::Forms::PaintEventArgs^ e);
		System::Void screenPanel_Click(System::Object^ sender, System::EventArgs^ e);
#pragma endregion

#pragma region Output
	private:
		System::Void savePng_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		System::Void saveGif_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		/// <summary>
		/// Exports the animation into a GIF file
		/// </summary>
		/// <returns></returns>
		System::Void ExportGif();
		/// <summary>
		/// System::String^ to std:string, Using Marshal
		/// </summary>
		/// <param name="managedString">converted std:string</param>
		/// <returns></returns>
		//std::string ConvertToStdString(System::String^ managedString);
#pragma endregion

};
}
