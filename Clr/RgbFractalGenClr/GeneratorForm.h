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
		FractalGenerator^ generator;		// The core ofthe app, the generator the generates the fractal animations
		CancellationTokenSource^ cancel;	// Cancellation Token Source
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
		int controlTabIndex = 0;			// Iterator for tabIndexes - to make sure all the controls tab in the correct order even as i add new ones in the middle
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
		/// <summary>
		/// Fractal definition selection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void fractalBox_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Fill the Color/Angle/CutFunction comboBoxes with available options for the selected fractal
		/// </summary>
		/// <returns></returns>
		System::Void FillSelects();
		/// <summary>
		/// Select child angles definition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void angleSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select child colors definition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void colorSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select CutFunction definition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void cutSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Fill the cutFunction seed parameter comboBox with available options for the selected CutFunction
		/// </summary>
		/// <returns></returns>
		System::Void FillCutParams();
		/// <summary>
		/// Change the CutFunction seed parameter through the textBox typing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void cutparamBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Change the CutFunction seed parameter through the trackBar scroll
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void cutparamBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Type X resolution - width
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void resX_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Type Y resolution - height
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void resY_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Toggle the "use the resolution", if unchecked, it will run in preview mode of firced 80x80 resolution regardless of typed resolution
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void previewBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Number Of Animation Frames to reach the center Self Similar (the total frames can be higher if the center child has a different color or rotation)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void periodBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Multiplies the number of loop frames, keeping the spin and huecycle the same speed (you can speed up either with options below)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void periodMultiplierBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Zoom direction (-> Forward zoom in, <- Backwards zoom out)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void zoomButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Default Zoom value on first frame (in skipped frames)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void defaultZoom_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select the spin mode (clockwise, counterclockwise, or antispin where the child spins in opposite direction)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void spinSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select the extra spin of symmentry angle per loop (so it spins faster)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void spinSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Default spin angle on first frame (in degrees)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void defaultAngle_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select the hue pallete and cycling
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void hueSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select the extra hue cycling speed of extra full 360° color loops per full animation loop (so it hue cycles spins faster)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void hueSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Defaul hue angle on first frame (in degrees)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void defaultHue_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// The strenghts (lightness) of the dark void outside between the fractal points
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void ambBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Level of void Noise of the dark void outside between the fractal points
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void noiseBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Saturation Setting - ramp up saturation to maximum if all the wat to the right
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void saturateBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Detail, how small the split fractal shaped have to get, until they draw a dot of their color to the image buffer (the smaller the finer)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void detailBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Level of blur smear frames (renders multiple fractals of slighlty increased time until the frame deltatime over each other)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void blurBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Iteration Threading - How many iterations deep have Self Similars a new thread
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void parallelBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Sets maximum threads as selected by user (th parallel checkBox and the maxThreads slider)
		/// </summary>
		System::Void SelectMaxThreads();
		/// <summary>
		/// Drawing Threading - Paralelizes the scanlines of drawing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void parallelTypeBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Toggles between parallelism of single images and parallelism of batching animation frames
		/// </summary>
		System::Void SelectParallelType();
		/// <summary>
		/// Maximum number of threads
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void threadsBar_Scroll(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Framerate Delay (for previes and for gif encode, so if encoding gif, it will restart the generation)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void delayBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select Previous frame to display
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void prevButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Select Next frame to display
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void nextButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Toggle display animation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void animateButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Toggle level of generation (SingleImage - Animation - GIF)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void encodeButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Show readme help
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void helpButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Save Frame
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void pngButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Save Gif
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void gifButton_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Start/Stop Animation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void screenPanel_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// Invalidation event of screen display - draws the current display frame bitmap.
		/// Get called repeatedly with new frame to animate the preview, if animation is toggled
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void screenPanel_Paint(System::Object^ sender, System::Windows::Forms::PaintEventArgs^ e);
#pragma endregion

#pragma region Output
	private:
		/// <summary>
		/// User inputed the path and name for saving PNG
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void savePng_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		/// <summary>
		/// User inputed the path and name for saving GIF
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void saveGif_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		/// <summary>
		/// Exports the animation into a GIF file
		/// Ackchyually - it just moves the already exported gifX.tmp to you desired location and name
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
