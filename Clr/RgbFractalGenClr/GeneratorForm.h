#pragma once
#include "FractalGenerator.h"
#include "DoubleBufferedPanel.h"

namespace RgbFractalGenCpp {

	using namespace System::Drawing;

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
	private: System::Windows::Forms::TextBox^ periodBox;
	private: System::Windows::Forms::Label^ delayLabel;
	private: System::Windows::Forms::TextBox^ delayBox;
	private: System::Windows::Forms::Button^ prevButton;
	private: System::Windows::Forms::Button^ nextButton;
	private: System::Windows::Forms::Button^ animateButton;
	private: System::Windows::Forms::Label^ voidLabel;
	private: System::Windows::Forms::Label^ dotLabel;
	private: System::Windows::Forms::Label^ threadsLabel;
	private: System::Windows::Forms::Label^ statusLabel;
	private: System::Windows::Forms::Label^ infoLabel;
	private: System::Windows::Forms::Button^ pngButton;
	private: System::Windows::Forms::Button^ gifButton;
	private: System::Windows::Forms::TextBox^ defaultZoom;
	private: System::Windows::Forms::TextBox^ defaultAngle;
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
	private: System::Windows::Forms::ComboBox^ parallelTypeSelect;

	private: System::Windows::Forms::TextBox^ ambBox;
	private: System::Windows::Forms::TextBox^ noiseBox;
	private: System::Windows::Forms::TextBox^ saturateBox;
	private: System::Windows::Forms::TextBox^ detailBox;
	private: System::Windows::Forms::TextBox^ bloomBox;
	private: System::Windows::Forms::TextBox^ blurBox;
	private: System::Windows::Forms::Label^ blurLabel;
	private: System::Windows::Forms::TextBox^ threadsBox;
	private: System::Windows::Forms::TextBox^ abortBox;
	private: System::Windows::Forms::TextBox^ brightnessBox;
	private: System::Windows::Forms::Label^ brightnessLabel;
	private: System::Windows::Forms::ComboBox^ zoomSelect;
	private: System::Windows::Forms::Button^ restartButton;
	private: System::Windows::Forms::ComboBox^ resSelect;
	private: System::Windows::Forms::ComboBox^ encodeSelect;
	private: System::Windows::Forms::CheckBox^ debugBox;
	private: System::Windows::Forms::Label^ debugLabel;
	private: System::Windows::Forms::Panel^ editorPanel;
	private: System::Windows::Forms::Button^ preButton;


	private: System::Windows::Forms::Button^ saveButton;

	private: System::Windows::Forms::Button^ loadButton;

	private: System::Windows::Forms::ComboBox^ addCut;

	private: System::Windows::Forms::Button^ removeColorButton;

	private: System::Windows::Forms::Button^ removeAngleButton;
	private: System::Windows::Forms::Button^ addColorButton;



	private: System::Windows::Forms::Button^ addAngleButton;
	private: System::Windows::Forms::Label^ addcutLabel;


	private: System::Windows::Forms::ComboBox^ colorBox;

	private: System::Windows::Forms::ComboBox^ angleBox;

	private: System::Windows::Forms::TextBox^ cutBox;

	private: System::Windows::Forms::TextBox^ minBox;

	private: System::Windows::Forms::TextBox^ maxBox;

	private: System::Windows::Forms::TextBox^ sizeBox;

	private: System::Windows::Forms::Label^ paramLabel;
	private: System::Windows::Forms::Panel^ pointPanel;
	private: System::Windows::Forms::Button^ addPoint;


	private: System::Windows::Forms::Label^ pointLabel;
	private: System::Windows::Forms::Button^ modeButton;
	private: System::Windows::Forms::Panel^ generatorPanel;
	private: System::Windows::Forms::TextBox^ voidBox;
	private: System::Windows::Forms::TextBox^ fpsBox;
	private: System::Windows::Forms::Button^ mp4Button;
private: System::Windows::Forms::SaveFileDialog^ saveFractal;
private: System::Windows::Forms::OpenFileDialog^ loadFractal;
private: System::Windows::Forms::SaveFileDialog^ saveMp4;
private: System::Windows::Forms::SaveFileDialog^ convertMp4;

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
		List<System::Windows::Forms::Control^>^
			MyControls = gcnew List<System::Windows::Forms::Control^>();
		List<bool>^
			MyControlsEnabled = gcnew List<bool>();
		FractalGenerator^ generator;		// The core ofthe app, the generator the generates the fractal animations
		CancellationTokenSource^ cancel;	// Cancellation Token Source
		Task^ gTask = nullptr;				// GIF Export Task
		Task^ aTask = nullptr;				// Abort Task
		bool queueAbort = false;			// Generator abortion queued
		uint16_t queueReset = 0;			// Couting time until generator Restart
		int32_t isGifReady = 0;

		// Settings
		bool previewMode = true;			// Preview mode for booting performance while setting up parameters
		bool animated = true;				// Animating preview or paused? (default animating)
		bool modifySettings = true;			// Allows for modifying settings without it triggering Aborts and Generates
		int16_t width = -1, height = -1;
		System::String^ gifPath = "";		// Gif export path name
		System::String^ mp4Path = "";    // Mp4 export path name
		Fractal* tosave = nullptr;
		uint16_t maxTasks = 0;				// Maximum tasks available
		uint16_t abortDelay = 500;			// Set time to restart generator
		uint16_t restartTimer = 0;

		// Bitmaps
		array<Bitmap^>^ bitmap;// = gcnew array<Bitmap^>(0);		    // Prerender as an array of bitmaps
		array<System::Drawing::Imaging::BitmapData^>^
			bitmapData;// = gcnew array<BitmapData^>();	                // Locked Bits for bitmaps
		System::Drawing::Rectangle rect;

		// Display  Variables
		DoubleBufferedPanel^ screenPanel;	// Display panel
		Bitmap^ currentBitmap = nullptr;	// Displayed Bitmap
		int32_t currentBitmapIndex;			// Play frame index
		int32_t fx, fy;						// Memory of window size
		int32_t controlTabIndex = 0;		// Iterator for tabIndexes - to make sure all the controls tab in the correct order even as i add new ones in the middle
		int32_t pointTabIndex = 0;

		// Editor
		List<System::Windows::Forms::TextBox^>^
			editorPointX = gcnew List<System::Windows::Forms::TextBox^>();
		List<System::Windows::Forms::TextBox^>^
			editorPointY = gcnew List<System::Windows::Forms::TextBox^>();
		List<System::Windows::Forms::TextBox^>^
			editorPointA = gcnew List<System::Windows::Forms::TextBox^>();
		List<System::Windows::Forms::Button^>^
			editorPointC = gcnew List<System::Windows::Forms::Button^>();
		List<System::Windows::Forms::Button^>^
			editorPointD = gcnew List<System::Windows::Forms::Button^>();
		/*array<std::tuple<
			System::Windows::Forms::TextBox^,
			System::Windows::Forms::TextBox^,
			System::Windows::Forms::TextBox^,
			System::Windows::Forms::Button^,
			System::Windows::Forms::Button^
			>)>^ editorPoint = gcnew array<>(0);*/
		List<System::Windows::Forms::Button^>^
			editorSwitch = gcnew List<System::Windows::Forms::Button^>();
		uint8_t mem_generate;
		uint16_t mem_blur, mem_defaulthue, mem_hue, mem_abort;
		float mem_bloom;
		bool performHash = false;
#pragma endregion

#pragma region Bitmaps
		System::Void UnlockBitmaps();
		System::Void ReencodeBitmaps();
		System::Void SetRect();
		System::Void NewBitmap(const uint16_t bitmapIndex, const uint16_t w, const uint16_t h);
		System::Void AllocateBitmaps();
		System::Void ReLockBits(const uint16_t bitmapIndex);
		System::Void UnlockBits(const uint16_t bitmapsFinished);
#pragma endregion

#pragma region Core
	private:
		inline bool IsTaskNotRunning(Task^ t) { return t == nullptr || t->IsCanceled || t->IsCompleted || t->IsFaulted; }
		/// <summary>
		/// Displays and Error message
		/// </summary>
		/// <param name="text">text</param>
		/// <param name="caption">caption</param>
		/// <returns>false</returns>
		static bool Error(System::String^ text, System::String^ caption);
		/// <summary>
		/// Updates the preview Image
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		System::Void UpdateBitmap(Bitmap^ bitmap);
		/// <summary>
		/// Fetches and Updates the preview Image
		/// </summary>
		/// <returns></returns>
		System::Void UpdatePreview();
		/// <summary>
		/// Setups the tooltip and tabindex of the interactable control
		/// </summary>
		System::Void SetupControl(System::Windows::Forms::Control^ control, System::String^ tip);
		/// <summary>
		/// Setups the tooltip and tabindex of the interactable dynamic editor control
		/// </summary>
		System::Void SetupEditControl(System::Windows::Forms::Control^ control, System::String^ tip);
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
		System::Void SaveSettings();
		System::Void LoadSettings();
#pragma endregion

#pragma region Input
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
		/// <summary>
		/// Cancels all the threads so it can close gracefully
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void GeneratorForm_FormClosing(System::Object^ sender, System::Windows::Forms::FormClosingEventArgs^ e);
		System::Void Close(System::Windows::Forms::FormClosingEventArgs^ e);
		/// <summary>
		/// Cancel all the threads
		/// </summary>
		/// <returns></returns>
		System::Void Abort();
		System::Void QueueReset(bool allow);
		System::Void ResetRestart();
		inline bool TasksNotRunning() { return IsTaskNotRunning(aTask) && IsTaskNotRunning(gTask); }
		static bool Clean(System::Windows::Forms::TextBox^ BOX) {
			auto s = BOX->Text;
			s = s->Replace(';', ' ')->Replace('|', ' ')->Replace(':', ' ');
			if (s != BOX->Text) {
				BOX->Text = s;
				return true;
			}
			return false;
		}

		template <typename T> static T Clamp(T NEW, T MIN, T MAX);
		/*
		template <typename T> static T Parse(System::Windows::Forms::TextBox^ BOX);

		template <typename T> static T Retext(System::Windows::Forms::TextBox^ BOX, T NEW);
		template <typename T> static T Mod(T NEW, T MIN, T MAX);
		template <typename T> static bool Diff(T NEW, T GEN);
		template <typename T> bool Apply(T NEW, interior_ptr<T> GEN);
		template <typename T> static T ParseClampRetext(System::Windows::Forms::TextBox^ BOX, T MIN, T MAX);
		template <typename T> bool DiffApply(T NEW, interior_ptr<T> GEN);
		template <typename T> bool ClampDiffApply(T NEW, interior_ptr<T> GEN, T MIN, T MAX);
		template <typename T> bool ParseDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<T> GEN);
		template <typename T> bool ParseModDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<T> GEN, T MIN, T MAX);
		template <typename T> bool ParseClampRetextDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<T> GEN, T MIN, T MAX);
		template <typename T, typename F> bool ParseClampRetextMulDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<F> GEN, T MIN, T MAX, F MUL);*/

		int16_t Parse16(System::Windows::Forms::TextBox^ BOX);
		int32_t Parse32(System::Windows::Forms::TextBox^ BOX);
		float ParseF(System::Windows::Forms::TextBox^ BOX);
		int16_t Retext(System::Windows::Forms::TextBox^ BOX, int16_t NEW);
		int16_t Mod(int16_t NEW, int16_t MIN, int16_t MAX);
		bool Diff(int16_t NEW, int16_t GEN);
		bool Diff(int32_t NEW, int32_t GEN);
		bool Diff(float NEW, float GEN);
		bool Apply(int16_t NEW, interior_ptr<int16_t> GEN);
		bool Apply(int32_t NEW, interior_ptr<int32_t> GEN);
		bool Apply(float NEW, interior_ptr<float> GEN);
		int16_t ParseClampRetext(System::Windows::Forms::TextBox^ BOX, int16_t MIN, int16_t MAX);
		int32_t ParseClampRetext(System::Windows::Forms::TextBox^ BOX, int32_t MIN, int32_t MAX);
		bool DiffApply(int16_t NEW, interior_ptr<int16_t> GEN);
		bool DiffApply(int32_t NEW, interior_ptr<int32_t> GEN);
		bool DiffApply(float NEW, interior_ptr<float> GEN);
		bool ClampDiffApply(int16_t NEW, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX);
		bool ParseDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN);
		bool ParseDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<float> GEN);
		bool ParseModDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX);
		bool ParseClampRetextDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX);
		bool ParseClampRetextDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int32_t> GEN, int32_t MIN, int32_t MAX);
		bool ParseClampRetextMulDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX, int16_t MUL);
		bool ParseClampRetextMulDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<float> GEN, int16_t MIN, int16_t MAX, float MUL);




		System::Void FractalSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		// TODO CPP
		System::Void fractalSelect_TextUpdate(System::Object^ sender, System::EventArgs^ e) {
			if (fractalSelect->Items->Contains(fractalSelect->Text))
				fractalSelect->SelectedIndex = fractalSelect->Items->IndexOf(fractalSelect->Text);
			else if (System::IO::File::Exists(fractalSelect->Text + ".fractal"))
				LoadFractal(fractalSelect->Text + ".fractal", true);
		};
		System::Void SetupSelects();
		System::Void SetupFractal();
		System::Void FillSelects();
		//TODO CPP
		System::Void SetupParallel(short newThreads) {
			generator->SelectedMaxTasks = (short)(newThreads > 1 ? newThreads : 1);
			generator->SelectThreadingDepth();
		}
		System::Void AngleSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void ColorSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void CutSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void CutparamBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		bool CutSelectEnabled(std::vector<std::pair<int32_t, std::vector<int32_t>>>& cf);
		bool CutParamBoxEnabled(Fractal::CutFunction* cf);
		System::Void FillCutParams();
		inline System::Void ResolutionChanged(System::Object^ sender, System::EventArgs^ e) { QueueReset(TryResize()); }
		System::Void PeriodBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void PeriodMultiplierBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void ZoomSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void DefaultZoom_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void SpinSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void SpinSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void DefaultAngle_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void HueSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void HueSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void DefaultHue_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void AmbBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void NoiseBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void SaturateBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void DetailBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void BloomBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void BlurBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void BrightnessBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void VoidBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void Parallel_Changed(System::Object^ sender, System::EventArgs^ e);
		System::Void ParallelTypeSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void AbortBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void DelayBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void FpsBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void MoveFrame(int16_t move);
		System::Void PrevButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void AnimateButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void SetAnimate();
		System::Void NextButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void RestartButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void EncodeSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void HelpButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void DebugBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e);
#pragma endregion

#pragma region Output
	private:
		/// <summary>
		/// Invalidation event of screen display - draws the current display frame bitmap.
		/// Get called repeatedly with new frame to animate the preview, if animation is toggled
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		System::Void ScreenPanel_Paint(System::Object^ sender, System::Windows::Forms::PaintEventArgs^ e);
		System::Void PngButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void GifButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Windows::Forms::DialogResult SaveVideo();
		System::Void Mp4Button_Click(System::Object^ sender, System::EventArgs^ e);
		/// <summary>
		/// User inputed the path and name for saving PNG
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void SavePng_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		/// <summary>
		/// User inputed the path and name for saving GIF
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		System::Void SaveVideo_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		System::Void ConvertMp4_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		/// <summary>
		/// Exports the animation into a GIF file
		/// Ackchyually - it just moves the already exported gifX.tmp to you desired location and name
		/// </summary>
		/// <returns></returns>
		System::Void ExportGif();
		System::Void ExportMp4();
		/// <summary>
		/// System::String^ to std:string, Using Marshal
		/// </summary>
		/// <param name="managedString">converted std:string</param>
		/// <returns></returns>
		//std::string ConvertToStdString(System::String^ managedString);
#pragma endregion

#pragma region Editor
		System::Void FillEditor();
		System::Void UnfillEditor();
		System::Void SwitchChildAngle();
		System::Void SwitchChildColor();
		System::Void AddEditorPoint(float* cx, float* cy, float* ca, uint8_t* cc, bool single);
		System::Void BindPoint(
			System::Windows::Forms::TextBox^ x,
			System::Windows::Forms::TextBox^ y,
			System::Windows::Forms::TextBox^ a,
			System::Windows::Forms::Button^ c,
			System::Windows::Forms::Button^ d,
			int i, bool single
		);
		System::Void SaveFractal_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		System::Void LoadFractal_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e);
		bool LoadFractal(System::String^ file, bool select);
		System::Void modeButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void addPoint_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void sizeBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void maxBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void minBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void cutBox_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void addAngleButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void removeAngleButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void addColorButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void removeColorButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void addCut_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void loadButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void saveButton_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void preButton_Click(System::Object^ sender, System::EventArgs^ e);

		System::Void GeneratorForm::OnS_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void GeneratorForm::OnX_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void GeneratorForm::OnY_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void GeneratorForm::OnA_TextChanged(System::Object^ sender, System::EventArgs^ e);
		System::Void GeneratorForm::OnC_Click(System::Object^ sender, System::EventArgs^ e);
		System::Void GeneratorForm::OnD_Click(System::Object^ sender, System::EventArgs^ e);
#pragma endregion

	};
}
