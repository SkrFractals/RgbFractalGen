// Starts the generator with special testing settings
//#define CUSTOMDEBUGTEST

#include "GeneratorForm.h"
#include <msclr/marshal_cppstd.h>

#define DN nullptr, nullptr

using namespace System;
using namespace System::Windows::Forms;

[STAThread]
//int main(Platform::Array<Platform::String^>^ argv)
void main(array<String^>^ args) {
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false);
	RgbFractalGenClr::GeneratorForm form;
	Application::Run(% form);
}

namespace RgbFractalGenCpp {

	using namespace System::IO;
	using namespace System::Drawing::Imaging;
	using namespace System::Diagnostics;

#pragma region Designer
	GeneratorForm::GeneratorForm(void) {
		InitializeComponent();
		this->screenPanel = gcnew DoubleBufferedPanel();
		this->screenPanel->Location = System::Drawing::Point(239, 13);
		this->screenPanel->Name = L"screenPanel";
		this->screenPanel->Size = System::Drawing::Size(80, 80);
		this->screenPanel->TabIndex = 25;
		this->screenPanel->Click += gcnew System::EventHandler(this, &GeneratorForm::AnimateButton_Click);
		this->screenPanel->Paint += gcnew System::Windows::Forms::PaintEventHandler(this, &GeneratorForm::ScreenPanel_Paint);
		this->Controls->Add(this->screenPanel);
	}
	GeneratorForm::~GeneratorForm() {
		if (components) {
			delete components;
		}
	}
#pragma endregion

#pragma region Windows Form Designer generated code
	void GeneratorForm::InitializeComponent(void) {
		this->components = (gcnew System::ComponentModel::Container());
		System::ComponentModel::ComponentResourceManager^ resources = (gcnew System::ComponentModel::ComponentResourceManager(GeneratorForm::typeid));
		this->toolTips = (gcnew System::Windows::Forms::ToolTip(this->components));
		this->fractalSelect = (gcnew System::Windows::Forms::ComboBox());
		this->resX = (gcnew System::Windows::Forms::TextBox());
		this->resY = (gcnew System::Windows::Forms::TextBox());
		this->periodBox = (gcnew System::Windows::Forms::TextBox());
		this->delayBox = (gcnew System::Windows::Forms::TextBox());
		this->prevButton = (gcnew System::Windows::Forms::Button());
		this->nextButton = (gcnew System::Windows::Forms::Button());
		this->animateButton = (gcnew System::Windows::Forms::Button());
		this->pngButton = (gcnew System::Windows::Forms::Button());
		this->gifButton = (gcnew System::Windows::Forms::Button());
		this->timer = (gcnew System::Windows::Forms::Timer(this->components));
		this->savePng = (gcnew System::Windows::Forms::SaveFileDialog());
		this->saveGif = (gcnew System::Windows::Forms::SaveFileDialog());
		this->fractalLabel = (gcnew System::Windows::Forms::Label());
		this->delayLabel = (gcnew System::Windows::Forms::Label());
		this->voidLabel = (gcnew System::Windows::Forms::Label());
		this->dotLabel = (gcnew System::Windows::Forms::Label());
		this->threadsLabel = (gcnew System::Windows::Forms::Label());
		this->statusLabel = (gcnew System::Windows::Forms::Label());
		this->infoLabel = (gcnew System::Windows::Forms::Label());
		this->defaultZoom = (gcnew System::Windows::Forms::TextBox());
		this->defaultAngle = (gcnew System::Windows::Forms::TextBox());
		this->cutparamBox = (gcnew System::Windows::Forms::TextBox());
		this->defaultHue = (gcnew System::Windows::Forms::TextBox());
		this->periodMultiplierBox = (gcnew System::Windows::Forms::TextBox());
		this->periodLabel = (gcnew System::Windows::Forms::Label());
		this->helpPanel = (gcnew System::Windows::Forms::Panel());
		this->helpLabel = (gcnew System::Windows::Forms::Label());
		this->angleSelect = (gcnew System::Windows::Forms::ComboBox());
		this->colorSelect = (gcnew System::Windows::Forms::ComboBox());
		this->cutSelect = (gcnew System::Windows::Forms::ComboBox());
		this->helpButton = (gcnew System::Windows::Forms::Button());
		this->angleLabel = (gcnew System::Windows::Forms::Label());
		this->colorLabel = (gcnew System::Windows::Forms::Label());
		this->cutLabel = (gcnew System::Windows::Forms::Label());
		this->zoomLabel = (gcnew System::Windows::Forms::Label());
		this->spinSelect = (gcnew System::Windows::Forms::ComboBox());
		this->hueSelect = (gcnew System::Windows::Forms::ComboBox());
		this->spinLabel = (gcnew System::Windows::Forms::Label());
		this->label1 = (gcnew System::Windows::Forms::Label());
		this->spinSpeedBox = (gcnew System::Windows::Forms::TextBox());
		this->hueSpeedBox = (gcnew System::Windows::Forms::TextBox());
		this->parallelTypeSelect = (gcnew System::Windows::Forms::ComboBox());
		this->ambBox = (gcnew System::Windows::Forms::TextBox());
		this->noiseBox = (gcnew System::Windows::Forms::TextBox());
		this->saturateBox = (gcnew System::Windows::Forms::TextBox());
		this->detailBox = (gcnew System::Windows::Forms::TextBox());
		this->bloomBox = (gcnew System::Windows::Forms::TextBox());
		this->blurBox = (gcnew System::Windows::Forms::TextBox());
		this->blurLabel = (gcnew System::Windows::Forms::Label());
		this->threadsBox = (gcnew System::Windows::Forms::TextBox());
		this->abortBox = (gcnew System::Windows::Forms::TextBox());
		this->brightnessBox = (gcnew System::Windows::Forms::TextBox());
		this->brightnessLabel = (gcnew System::Windows::Forms::Label());
		this->zoomSelect = (gcnew System::Windows::Forms::ComboBox());
		this->restartButton = (gcnew System::Windows::Forms::Button());
		this->resSelect = (gcnew System::Windows::Forms::ComboBox());
		this->encodeSelect = (gcnew System::Windows::Forms::ComboBox());
		this->debugBox = (gcnew System::Windows::Forms::CheckBox());
		this->debugLabel = (gcnew System::Windows::Forms::Label());
		this->editorPanel = (gcnew System::Windows::Forms::Panel());
		this->preButton = (gcnew System::Windows::Forms::Button());
		this->saveButton = (gcnew System::Windows::Forms::Button());
		this->loadButton = (gcnew System::Windows::Forms::Button());
		this->addCut = (gcnew System::Windows::Forms::ComboBox());
		this->removeColorButton = (gcnew System::Windows::Forms::Button());
		this->removeAngleButton = (gcnew System::Windows::Forms::Button());
		this->addColorButton = (gcnew System::Windows::Forms::Button());
		this->addAngleButton = (gcnew System::Windows::Forms::Button());
		this->addcutLabel = (gcnew System::Windows::Forms::Label());
		this->colorBox = (gcnew System::Windows::Forms::ComboBox());
		this->angleBox = (gcnew System::Windows::Forms::ComboBox());
		this->cutBox = (gcnew System::Windows::Forms::TextBox());
		this->minBox = (gcnew System::Windows::Forms::TextBox());
		this->maxBox = (gcnew System::Windows::Forms::TextBox());
		this->sizeBox = (gcnew System::Windows::Forms::TextBox());
		this->paramLabel = (gcnew System::Windows::Forms::Label());
		this->pointPanel = (gcnew System::Windows::Forms::Panel());
		this->addPoint = (gcnew System::Windows::Forms::Button());
		this->pointLabel = (gcnew System::Windows::Forms::Label());
		this->modeButton = (gcnew System::Windows::Forms::Button());
		this->generatorPanel = (gcnew System::Windows::Forms::Panel());
		this->mp4Button = (gcnew System::Windows::Forms::Button());
		this->fpsBox = (gcnew System::Windows::Forms::TextBox());
		this->voidBox = (gcnew System::Windows::Forms::TextBox());
		this->saveFractal = (gcnew System::Windows::Forms::SaveFileDialog());
		this->loadFractal = (gcnew System::Windows::Forms::OpenFileDialog());
		this->saveMp4 = (gcnew System::Windows::Forms::SaveFileDialog());
		this->convertMp4 = (gcnew System::Windows::Forms::SaveFileDialog());
		this->helpPanel->SuspendLayout();
		this->editorPanel->SuspendLayout();
		this->pointPanel->SuspendLayout();
		this->generatorPanel->SuspendLayout();
		this->SuspendLayout();
		// 
		// fractalSelect
		// 
		this->fractalSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																 static_cast<System::Byte>(238)));
		this->fractalSelect->FormattingEnabled = true;
		this->fractalSelect->Location = System::Drawing::Point(71, 14);
		this->fractalSelect->Name = L"fractalSelect";
		this->fractalSelect->Size = System::Drawing::Size(178, 23);
		this->fractalSelect->TabIndex = 1;
		this->fractalSelect->Text = L"Select Fractal";
		this->fractalSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::FractalSelect_SelectedIndexChanged);
		this->fractalSelect->TextUpdate += gcnew System::EventHandler(this, &GeneratorForm::fractalSelect_TextUpdate);
		// 
		// resX
		// 
		this->resX->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														static_cast<System::Byte>(238)));
		this->resX->Location = System::Drawing::Point(17, 130);
		this->resX->Name = L"resX";
		this->resX->Size = System::Drawing::Size(46, 23);
		this->resX->TabIndex = 4;
		this->resX->Text = L"1920";
		this->resX->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::ResolutionChanged);
		// 
		// resY
		// 
		this->resY->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														static_cast<System::Byte>(238)));
		this->resY->Location = System::Drawing::Point(71, 130);
		this->resY->Name = L"resY";
		this->resY->Size = System::Drawing::Size(46, 23);
		this->resY->TabIndex = 5;
		this->resY->Text = L"1080";
		this->resY->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::ResolutionChanged);
		// 
		// periodBox
		// 
		this->periodBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->periodBox->Location = System::Drawing::Point(14, 11);
		this->periodBox->Name = L"periodBox";
		this->periodBox->Size = System::Drawing::Size(86, 23);
		this->periodBox->TabIndex = 7;
		this->periodBox->Text = L"120";
		this->periodBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::PeriodBox_TextChanged);
		// 
		// delayBox
		// 
		this->delayBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->delayBox->Location = System::Drawing::Point(174, 313);
		this->delayBox->Name = L"delayBox";
		this->delayBox->Size = System::Drawing::Size(45, 23);
		this->delayBox->TabIndex = 23;
		this->delayBox->Text = L"5";
		this->delayBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::DelayBox_TextChanged);
		// 
		// prevButton
		// 
		this->prevButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->prevButton->Location = System::Drawing::Point(14, 341);
		this->prevButton->Name = L"prevButton";
		this->prevButton->Size = System::Drawing::Size(30, 27);
		this->prevButton->TabIndex = 24;
		this->prevButton->Text = L"<-";
		this->prevButton->UseVisualStyleBackColor = true;
		this->prevButton->Click += gcnew System::EventHandler(this, &GeneratorForm::PrevButton_Click);
		// 
		// nextButton
		// 
		this->nextButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->nextButton->Location = System::Drawing::Point(124, 341);
		this->nextButton->Name = L"nextButton";
		this->nextButton->Size = System::Drawing::Size(30, 27);
		this->nextButton->TabIndex = 25;
		this->nextButton->Text = L"->";
		this->nextButton->UseVisualStyleBackColor = true;
		this->nextButton->Click += gcnew System::EventHandler(this, &GeneratorForm::NextButton_Click);
		// 
		// animateButton
		// 
		this->animateButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->animateButton->Location = System::Drawing::Point(52, 341);
		this->animateButton->Name = L"animateButton";
		this->animateButton->Size = System::Drawing::Size(64, 27);
		this->animateButton->TabIndex = 26;
		this->animateButton->Text = L"Playing";
		this->animateButton->UseVisualStyleBackColor = true;
		this->animateButton->Click += gcnew System::EventHandler(this, &GeneratorForm::AnimateButton_Click);
		// 
		// pngButton
		// 
		this->pngButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->pngButton->Location = System::Drawing::Point(62, 403);
		this->pngButton->Name = L"pngButton";
		this->pngButton->Size = System::Drawing::Size(66, 27);
		this->pngButton->TabIndex = 28;
		this->pngButton->Text = L"Save PNG";
		this->pngButton->UseVisualStyleBackColor = true;
		this->pngButton->Click += gcnew System::EventHandler(this, &GeneratorForm::PngButton_Click);
		// 
		// gifButton
		// 
		this->gifButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->gifButton->Location = System::Drawing::Point(136, 403);
		this->gifButton->Name = L"gifButton";
		this->gifButton->Size = System::Drawing::Size(59, 27);
		this->gifButton->TabIndex = 29;
		this->gifButton->Text = L"Save GIF";
		this->gifButton->UseVisualStyleBackColor = true;
		this->gifButton->Click += gcnew System::EventHandler(this, &GeneratorForm::GifButton_Click);
		// 
		// timer
		// 
		this->timer->Enabled = true;
		this->timer->Interval = 20;
		this->timer->Tick += gcnew System::EventHandler(this, &GeneratorForm::timer_Tick);
		// 
		// savePng
		// 
		this->savePng->DefaultExt = L"png";
		this->savePng->Filter = L"PNG files (*.png)|*.png";
		this->savePng->FileOk += gcnew System::ComponentModel::CancelEventHandler(this, &GeneratorForm::SavePng_FileOk);
		// 
		// saveGif
		// 
		this->saveGif->DefaultExt = L"gif";
		this->saveGif->Filter = L"GIF files (*.gif)|*.gif";
		this->saveGif->FileOk += gcnew System::ComponentModel::CancelEventHandler(this, &GeneratorForm::SaveVideo_FileOk);
		// 
		// fractalLabel
		// 
		this->fractalLabel->AutoSize = true;
		this->fractalLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->fractalLabel->ForeColor = System::Drawing::Color::White;
		this->fractalLabel->Location = System::Drawing::Point(17, 17);
		this->fractalLabel->Name = L"fractalLabel";
		this->fractalLabel->Size = System::Drawing::Size(45, 15);
		this->fractalLabel->TabIndex = 0;
		this->fractalLabel->Text = L"Fractal:";
		// 
		// delayLabel
		// 
		this->delayLabel->AutoSize = true;
		this->delayLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->delayLabel->ForeColor = System::Drawing::Color::White;
		this->delayLabel->Location = System::Drawing::Point(14, 316);
		this->delayLabel->Name = L"delayLabel";
		this->delayLabel->Size = System::Drawing::Size(105, 13);
		this->delayLabel->TabIndex = 0;
		this->delayLabel->Text = L"Abort / Delay / FPS:";
		// 
		// voidLabel
		// 
		this->voidLabel->AutoSize = true;
		this->voidLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->voidLabel->ForeColor = System::Drawing::Color::White;
		this->voidLabel->Location = System::Drawing::Point(14, 171);
		this->voidLabel->Name = L"voidLabel";
		this->voidLabel->Size = System::Drawing::Size(143, 13);
		this->voidLabel->TabIndex = 0;
		this->voidLabel->Text = L"Void Ambient/Noise (0-30):";
		// 
		// dotLabel
		// 
		this->dotLabel->AutoSize = true;
		this->dotLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->dotLabel->ForeColor = System::Drawing::Color::White;
		this->dotLabel->Location = System::Drawing::Point(14, 200);
		this->dotLabel->Name = L"dotLabel";
		this->dotLabel->Size = System::Drawing::Size(118, 13);
		this->dotLabel->TabIndex = 0;
		this->dotLabel->Text = L"Saturate/Detail (0-10):";
		// 
		// threadsLabel
		// 
		this->threadsLabel->AutoSize = true;
		this->threadsLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->threadsLabel->ForeColor = System::Drawing::Color::White;
		this->threadsLabel->Location = System::Drawing::Point(14, 287);
		this->threadsLabel->Name = L"threadsLabel";
		this->threadsLabel->Size = System::Drawing::Size(91, 13);
		this->threadsLabel->TabIndex = 0;
		this->threadsLabel->Text = L"Parallel Threads:";
		// 
		// statusLabel
		// 
		this->statusLabel->AutoSize = true;
		this->statusLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->statusLabel->ForeColor = System::Drawing::Color::White;
		this->statusLabel->Location = System::Drawing::Point(14, 377);
		this->statusLabel->Name = L"statusLabel";
		this->statusLabel->Size = System::Drawing::Size(64, 15);
		this->statusLabel->TabIndex = 0;
		this->statusLabel->Text = L"Initializing:";
		// 
		// infoLabel
		// 
		this->infoLabel->AutoSize = true;
		this->infoLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->infoLabel->ForeColor = System::Drawing::Color::White;
		this->infoLabel->Location = System::Drawing::Point(88, 377);
		this->infoLabel->Name = L"infoLabel";
		this->infoLabel->Size = System::Drawing::Size(28, 15);
		this->infoLabel->TabIndex = 0;
		this->infoLabel->Text = L"info";
		// 
		// defaultZoom
		// 
		this->defaultZoom->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->defaultZoom->Location = System::Drawing::Point(108, 40);
		this->defaultZoom->Name = L"defaultZoom";
		this->defaultZoom->Size = System::Drawing::Size(60, 23);
		this->defaultZoom->TabIndex = 12;
		this->defaultZoom->Text = L"0";
		this->defaultZoom->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::DefaultZoom_TextChanged);
		// 
		// defaultAngle
		// 
		this->defaultAngle->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->defaultAngle->Location = System::Drawing::Point(174, 90);
		this->defaultAngle->Name = L"defaultAngle";
		this->defaultAngle->Size = System::Drawing::Size(96, 23);
		this->defaultAngle->TabIndex = 13;
		this->defaultAngle->Text = L"0";
		this->defaultAngle->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::DefaultAngle_TextChanged);
		// 
		// cutparamBox
		// 
		this->cutparamBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->cutparamBox->Location = System::Drawing::Point(231, 101);
		this->cutparamBox->Name = L"cutparamBox";
		this->cutparamBox->Size = System::Drawing::Size(71, 23);
		this->cutparamBox->TabIndex = 2;
		this->cutparamBox->Text = L"0";
		this->cutparamBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::CutparamBox_TextChanged);
		// 
		// defaultHue
		// 
		this->defaultHue->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->defaultHue->Location = System::Drawing::Point(174, 139);
		this->defaultHue->Name = L"defaultHue";
		this->defaultHue->Size = System::Drawing::Size(96, 23);
		this->defaultHue->TabIndex = 14;
		this->defaultHue->Text = L"0";
		this->defaultHue->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::DefaultHue_TextChanged);
		// 
		// periodMultiplierBox
		// 
		this->periodMultiplierBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																	   static_cast<System::Byte>(238)));
		this->periodMultiplierBox->Location = System::Drawing::Point(108, 11);
		this->periodMultiplierBox->Name = L"periodMultiplierBox";
		this->periodMultiplierBox->Size = System::Drawing::Size(60, 23);
		this->periodMultiplierBox->TabIndex = 8;
		this->periodMultiplierBox->Text = L"1";
		this->periodMultiplierBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::PeriodMultiplierBox_TextChanged);
		// 
		// periodLabel
		// 
		this->periodLabel->AutoSize = true;
		this->periodLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(0)));
		this->periodLabel->ForeColor = System::Drawing::Color::White;
		this->periodLabel->Location = System::Drawing::Point(178, 14);
		this->periodLabel->Name = L"periodLabel";
		this->periodLabel->Size = System::Drawing::Size(57, 15);
		this->periodLabel->TabIndex = 30;
		this->periodLabel->Text = L"<- Period";
		// 
		// helpPanel
		// 
		this->helpPanel->AutoScroll = true;
		this->helpPanel->BackColor = System::Drawing::Color::White;
		this->helpPanel->Controls->Add(this->helpLabel);
		this->helpPanel->Location = System::Drawing::Point(309, 14);
		this->helpPanel->Name = L"helpPanel";
		this->helpPanel->Size = System::Drawing::Size(763, 586);
		this->helpPanel->TabIndex = 0;
		// 
		// helpLabel
		// 
		this->helpLabel->AutoSize = true;
		this->helpLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->helpLabel->Location = System::Drawing::Point(15, 8);
		this->helpLabel->Name = L"helpLabel";
		this->helpLabel->Size = System::Drawing::Size(30, 15);
		this->helpLabel->TabIndex = 0;
		this->helpLabel->Text = L"help";
		// 
		// angleSelect
		// 
		this->angleSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->angleSelect->FormattingEnabled = true;
		this->angleSelect->Location = System::Drawing::Point(71, 43);
		this->angleSelect->Name = L"angleSelect";
		this->angleSelect->Size = System::Drawing::Size(231, 23);
		this->angleSelect->TabIndex = 32;
		this->angleSelect->Text = L"Select Angles";
		this->angleSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::AngleSelect_SelectedIndexChanged);
		// 
		// colorSelect
		// 
		this->colorSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->colorSelect->FormattingEnabled = true;
		this->colorSelect->Location = System::Drawing::Point(71, 72);
		this->colorSelect->Name = L"colorSelect";
		this->colorSelect->Size = System::Drawing::Size(231, 23);
		this->colorSelect->TabIndex = 33;
		this->colorSelect->Text = L"Select Colors";
		this->colorSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::ColorSelect_SelectedIndexChanged);
		// 
		// cutSelect
		// 
		this->cutSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->cutSelect->FormattingEnabled = true;
		this->cutSelect->Location = System::Drawing::Point(71, 101);
		this->cutSelect->Name = L"cutSelect";
		this->cutSelect->Size = System::Drawing::Size(152, 23);
		this->cutSelect->TabIndex = 34;
		this->cutSelect->Text = L"Select CutFunction";
		this->cutSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::CutSelect_SelectedIndexChanged);
		// 
		// helpButton
		// 
		this->helpButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->helpButton->Location = System::Drawing::Point(14, 403);
		this->helpButton->Name = L"helpButton";
		this->helpButton->Size = System::Drawing::Size(40, 27);
		this->helpButton->TabIndex = 35;
		this->helpButton->Text = L"Help";
		this->helpButton->UseVisualStyleBackColor = true;
		this->helpButton->Click += gcnew System::EventHandler(this, &GeneratorForm::HelpButton_Click);
		// 
		// angleLabel
		// 
		this->angleLabel->AutoSize = true;
		this->angleLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->angleLabel->ForeColor = System::Drawing::Color::White;
		this->angleLabel->Location = System::Drawing::Point(17, 46);
		this->angleLabel->Name = L"angleLabel";
		this->angleLabel->Size = System::Drawing::Size(46, 15);
		this->angleLabel->TabIndex = 36;
		this->angleLabel->Text = L"Angles:";
		// 
		// colorLabel
		// 
		this->colorLabel->AutoSize = true;
		this->colorLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->colorLabel->ForeColor = System::Drawing::Color::White;
		this->colorLabel->Location = System::Drawing::Point(17, 75);
		this->colorLabel->Name = L"colorLabel";
		this->colorLabel->Size = System::Drawing::Size(41, 15);
		this->colorLabel->TabIndex = 37;
		this->colorLabel->Text = L"Colors";
		// 
		// cutLabel
		// 
		this->cutLabel->AutoSize = true;
		this->cutLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->cutLabel->ForeColor = System::Drawing::Color::White;
		this->cutLabel->Location = System::Drawing::Point(17, 104);
		this->cutLabel->Name = L"cutLabel";
		this->cutLabel->Size = System::Drawing::Size(29, 15);
		this->cutLabel->TabIndex = 38;
		this->cutLabel->Text = L"Cut:";
		// 
		// zoomLabel
		// 
		this->zoomLabel->AutoSize = true;
		this->zoomLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(0)));
		this->zoomLabel->ForeColor = System::Drawing::Color::White;
		this->zoomLabel->Location = System::Drawing::Point(178, 43);
		this->zoomLabel->Name = L"zoomLabel";
		this->zoomLabel->Size = System::Drawing::Size(55, 15);
		this->zoomLabel->TabIndex = 39;
		this->zoomLabel->Text = L"<- Zoom";
		// 
		// spinSelect
		// 
		this->spinSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->spinSelect->FormattingEnabled = true;
		this->spinSelect->Items->AddRange(gcnew cli::array< System::Object^  >(5) {
			L"Random", L"Clock", L"None", L"Counterclock",
				L"Antispin"
		});
		this->spinSelect->Location = System::Drawing::Point(14, 90);
		this->spinSelect->Name = L"spinSelect";
		this->spinSelect->Size = System::Drawing::Size(86, 23);
		this->spinSelect->TabIndex = 40;
		this->spinSelect->Text = L"Select Spin";
		this->spinSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::SpinSelect_SelectedIndexChanged);
		// 
		// hueSelect
		// 
		this->hueSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->hueSelect->FormattingEnabled = true;
		this->hueSelect->Items->AddRange(gcnew cli::array< System::Object^  >(7) {
			L"Random", L"RGB (static)", L"BGR (static)", L"RGB->GBR",
				L"BGR->RBG", L"RGB->BRG", L"BGR->GRB"
		});
		this->hueSelect->Location = System::Drawing::Point(14, 139);
		this->hueSelect->Name = L"hueSelect";
		this->hueSelect->Size = System::Drawing::Size(86, 23);
		this->hueSelect->TabIndex = 42;
		this->hueSelect->Text = L"Select Hue";
		this->hueSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::HueSelect_SelectedIndexChanged);
		// 
		// spinLabel
		// 
		this->spinLabel->AutoSize = true;
		this->spinLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->spinLabel->ForeColor = System::Drawing::Color::White;
		this->spinLabel->Location = System::Drawing::Point(14, 72);
		this->spinLabel->Name = L"spinLabel";
		this->spinLabel->Size = System::Drawing::Size(249, 15);
		this->spinLabel->TabIndex = 44;
		this->spinLabel->Text = L"Spin direction    Spin Speed         Default Angle";
		// 
		// label1
		// 
		this->label1->AutoSize = true;
		this->label1->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														  static_cast<System::Byte>(238)));
		this->label1->ForeColor = System::Drawing::Color::White;
		this->label1->Location = System::Drawing::Point(14, 121);
		this->label1->Name = L"label1";
		this->label1->Size = System::Drawing::Size(246, 15);
		this->label1->TabIndex = 45;
		this->label1->Text = L"Hue Select           HueSpeed           Default Hue";
		// 
		// spinSpeedBox
		// 
		this->spinSpeedBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->spinSpeedBox->Location = System::Drawing::Point(106, 90);
		this->spinSpeedBox->Name = L"spinSpeedBox";
		this->spinSpeedBox->Size = System::Drawing::Size(62, 23);
		this->spinSpeedBox->TabIndex = 46;
		this->spinSpeedBox->Text = L"0";
		this->spinSpeedBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::SpinSpeedBox_TextChanged);
		// 
		// hueSpeedBox
		// 
		this->hueSpeedBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->hueSpeedBox->Location = System::Drawing::Point(106, 139);
		this->hueSpeedBox->Name = L"hueSpeedBox";
		this->hueSpeedBox->Size = System::Drawing::Size(62, 23);
		this->hueSpeedBox->TabIndex = 47;
		this->hueSpeedBox->Text = L"0";
		this->hueSpeedBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::HueSpeedBox_TextChanged);
		// 
		// parallelTypeSelect
		// 
		this->parallelTypeSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																	  static_cast<System::Byte>(238)));
		this->parallelTypeSelect->FormattingEnabled = true;
		this->parallelTypeSelect->Items->AddRange(gcnew cli::array< System::Object^  >(2) { L"Of Animation", L"Of Depth" });
		this->parallelTypeSelect->Location = System::Drawing::Point(111, 284);
		this->parallelTypeSelect->Name = L"parallelTypeSelect";
		this->parallelTypeSelect->Size = System::Drawing::Size(108, 23);
		this->parallelTypeSelect->TabIndex = 21;
		this->parallelTypeSelect->Text = L"Parallelism Type";
		this->parallelTypeSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::ParallelTypeSelect_SelectedIndexChanged);
		// 
		// ambBox
		// 
		this->ambBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														  static_cast<System::Byte>(238)));
		this->ambBox->Location = System::Drawing::Point(174, 168);
		this->ambBox->Name = L"ambBox";
		this->ambBox->Size = System::Drawing::Size(45, 23);
		this->ambBox->TabIndex = 48;
		this->ambBox->Tag = L"";
		this->ambBox->Text = L"20";
		this->ambBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::AmbBox_TextChanged);
		// 
		// noiseBox
		// 
		this->noiseBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->noiseBox->Location = System::Drawing::Point(225, 168);
		this->noiseBox->Name = L"noiseBox";
		this->noiseBox->Size = System::Drawing::Size(45, 23);
		this->noiseBox->TabIndex = 49;
		this->noiseBox->Tag = L"";
		this->noiseBox->Text = L"20";
		this->noiseBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::NoiseBox_TextChanged);
		// 
		// saturateBox
		// 
		this->saturateBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->saturateBox->Location = System::Drawing::Point(174, 197);
		this->saturateBox->Name = L"saturateBox";
		this->saturateBox->Size = System::Drawing::Size(45, 23);
		this->saturateBox->TabIndex = 50;
		this->saturateBox->Tag = L"";
		this->saturateBox->Text = L"10";
		this->saturateBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::SaturateBox_TextChanged);
		// 
		// detailBox
		// 
		this->detailBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->detailBox->Location = System::Drawing::Point(225, 197);
		this->detailBox->Name = L"detailBox";
		this->detailBox->Size = System::Drawing::Size(45, 23);
		this->detailBox->TabIndex = 51;
		this->detailBox->Tag = L"";
		this->detailBox->Text = L"5";
		this->detailBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::DetailBox_TextChanged);
		// 
		// bloomBox
		// 
		this->bloomBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->bloomBox->Location = System::Drawing::Point(174, 226);
		this->bloomBox->Name = L"bloomBox";
		this->bloomBox->Size = System::Drawing::Size(45, 23);
		this->bloomBox->TabIndex = 52;
		this->bloomBox->Tag = L"";
		this->bloomBox->Text = L"10";
		this->bloomBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::BloomBox_TextChanged);
		// 
		// blurBox
		// 
		this->blurBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														   static_cast<System::Byte>(238)));
		this->blurBox->Location = System::Drawing::Point(225, 226);
		this->blurBox->Name = L"blurBox";
		this->blurBox->Size = System::Drawing::Size(45, 23);
		this->blurBox->TabIndex = 53;
		this->blurBox->Tag = L"";
		this->blurBox->Text = L"0";
		this->blurBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::BlurBox_TextChanged);
		// 
		// blurLabel
		// 
		this->blurLabel->AutoSize = true;
		this->blurLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->blurLabel->ForeColor = System::Drawing::Color::White;
		this->blurLabel->Location = System::Drawing::Point(14, 229);
		this->blurLabel->Name = L"blurLabel";
		this->blurLabel->Size = System::Drawing::Size(138, 13);
		this->blurLabel->TabIndex = 54;
		this->blurLabel->Text = L"Bloom/Motion Blur (0-40):";
		// 
		// threadsBox
		// 
		this->threadsBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->threadsBox->Location = System::Drawing::Point(225, 284);
		this->threadsBox->Name = L"threadsBox";
		this->threadsBox->Size = System::Drawing::Size(45, 23);
		this->threadsBox->TabIndex = 55;
		this->threadsBox->Tag = L"";
		this->threadsBox->Text = L"0";
		this->threadsBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::Parallel_Changed);
		// 
		// abortBox
		// 
		this->abortBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->abortBox->Location = System::Drawing::Point(123, 313);
		this->abortBox->Name = L"abortBox";
		this->abortBox->Size = System::Drawing::Size(45, 23);
		this->abortBox->TabIndex = 56;
		this->abortBox->Text = L"50";
		this->abortBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::AbortBox_TextChanged);
		// 
		// brightnessBox
		// 
		this->brightnessBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																 static_cast<System::Byte>(238)));
		this->brightnessBox->Location = System::Drawing::Point(174, 255);
		this->brightnessBox->Name = L"brightnessBox";
		this->brightnessBox->Size = System::Drawing::Size(45, 23);
		this->brightnessBox->TabIndex = 57;
		this->brightnessBox->Tag = L"";
		this->brightnessBox->Text = L"100";
		this->brightnessBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::BrightnessBox_TextChanged);
		// 
		// brightnessLabel
		// 
		this->brightnessLabel->AutoSize = true;
		this->brightnessLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																   static_cast<System::Byte>(238)));
		this->brightnessLabel->ForeColor = System::Drawing::Color::White;
		this->brightnessLabel->Location = System::Drawing::Point(14, 258);
		this->brightnessLabel->Name = L"brightnessLabel";
		this->brightnessLabel->Size = System::Drawing::Size(160, 13);
		this->brightnessLabel->TabIndex = 58;
		this->brightnessLabel->Text = L"Brightness/NoiseScale (0-300):";
		// 
		// zoomSelect
		// 
		this->zoomSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->zoomSelect->FormattingEnabled = true;
		this->zoomSelect->Items->AddRange(gcnew cli::array< System::Object^  >(3) { L"Random", L"In", L"Out" });
		this->zoomSelect->Location = System::Drawing::Point(14, 40);
		this->zoomSelect->Name = L"zoomSelect";
		this->zoomSelect->Size = System::Drawing::Size(86, 23);
		this->zoomSelect->TabIndex = 59;
		this->zoomSelect->Text = L"Select Zoom";
		this->zoomSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::ZoomSelect_SelectedIndexChanged);
		// 
		// restartButton
		// 
		this->restartButton->BackColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(255)), static_cast<System::Int32>(static_cast<System::Byte>(128)),
																		  static_cast<System::Int32>(static_cast<System::Byte>(128)));
		this->restartButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->restartButton->Location = System::Drawing::Point(162, 341);
		this->restartButton->Name = L"restartButton";
		this->restartButton->Size = System::Drawing::Size(108, 27);
		this->restartButton->TabIndex = 60;
		this->restartButton->Text = L"! RESTART !";
		this->restartButton->UseVisualStyleBackColor = false;
		this->restartButton->Click += gcnew System::EventHandler(this, &GeneratorForm::RestartButton_Click);
		// 
		// resSelect
		// 
		this->resSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->resSelect->FormattingEnabled = true;
		this->resSelect->Items->AddRange(gcnew cli::array< System::Object^  >(20) {
			L"80x80", L"Custom:", L"256x256", L"512x512", L"640x480",
				L"1024x768", L"1280x720", L"720x1280", L"1920x1080", L"1080x1920", L"1600x900", L"900x1600", L"2560x1440", L"1440x2560", L"3840x2160",
				L"2160x3840", L"5120x2880", L"2880x5120", L"7680x4320", L"4320x7680"
		});
		this->resSelect->Location = System::Drawing::Point(125, 130);
		this->resSelect->Name = L"resSelect";
		this->resSelect->Size = System::Drawing::Size(177, 23);
		this->resSelect->TabIndex = 61;
		this->resSelect->Text = L"Select Resolution";
		this->resSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::ResolutionChanged);
		// 
		// encodeSelect
		// 
		this->encodeSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->encodeSelect->FormattingEnabled = true;
		this->encodeSelect->Items->AddRange(gcnew cli::array< System::Object^  >(4) {
			L"Only Image", L"Animation RAM", L"Local GIF",
				L"Global GIF"
		});
		this->encodeSelect->Location = System::Drawing::Point(162, 374);
		this->encodeSelect->Name = L"encodeSelect";
		this->encodeSelect->Size = System::Drawing::Size(108, 23);
		this->encodeSelect->TabIndex = 62;
		this->encodeSelect->Text = L"Generation Type";
		this->encodeSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::EncodeSelect_SelectedIndexChanged);
		// 
		// debugBox
		// 
		this->debugBox->AutoSize = true;
		this->debugBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->debugBox->ForeColor = System::Drawing::Color::White;
		this->debugBox->Location = System::Drawing::Point(17, 606);
		this->debugBox->Name = L"debugBox";
		this->debugBox->Size = System::Drawing::Size(84, 19);
		this->debugBox->TabIndex = 63;
		this->debugBox->Text = L"Debug Log";
		this->debugBox->UseVisualStyleBackColor = true;
		this->debugBox->CheckedChanged += gcnew System::EventHandler(this, &GeneratorForm::DebugBox_CheckedChanged);
		// 
		// debugLabel
		// 
		this->debugLabel->AutoSize = true;
		this->debugLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->debugLabel->ForeColor = System::Drawing::Color::White;
		this->debugLabel->Location = System::Drawing::Point(17, 628);
		this->debugLabel->Name = L"debugLabel";
		this->debugLabel->Size = System::Drawing::Size(73, 15);
		this->debugLabel->TabIndex = 67;
		this->debugLabel->Text = L"DebugString";
		// 
		// editorPanel
		// 
		this->editorPanel->Controls->Add(this->preButton);
		this->editorPanel->Controls->Add(this->saveButton);
		this->editorPanel->Controls->Add(this->loadButton);
		this->editorPanel->Controls->Add(this->addCut);
		this->editorPanel->Controls->Add(this->removeColorButton);
		this->editorPanel->Controls->Add(this->removeAngleButton);
		this->editorPanel->Controls->Add(this->addColorButton);
		this->editorPanel->Controls->Add(this->addAngleButton);
		this->editorPanel->Controls->Add(this->addcutLabel);
		this->editorPanel->Controls->Add(this->colorBox);
		this->editorPanel->Controls->Add(this->angleBox);
		this->editorPanel->Controls->Add(this->cutBox);
		this->editorPanel->Controls->Add(this->minBox);
		this->editorPanel->Controls->Add(this->maxBox);
		this->editorPanel->Controls->Add(this->sizeBox);
		this->editorPanel->Controls->Add(this->paramLabel);
		this->editorPanel->Controls->Add(this->pointPanel);
		this->editorPanel->Controls->Add(this->pointLabel);
		this->editorPanel->Location = System::Drawing::Point(17, 661);
		this->editorPanel->Name = L"editorPanel";
		this->editorPanel->Size = System::Drawing::Size(286, 441);
		this->editorPanel->TabIndex = 68;
		// 
		// preButton
		// 
		this->preButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->preButton->Location = System::Drawing::Point(148, 410);
		this->preButton->Name = L"preButton";
		this->preButton->Size = System::Drawing::Size(122, 27);
		this->preButton->TabIndex = 77;
		this->preButton->Text = L"ADD CHILD";
		this->preButton->UseVisualStyleBackColor = true;
		this->preButton->Click += gcnew System::EventHandler(this, &GeneratorForm::preButton_Click);
		// 
		// saveButton
		// 
		this->saveButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->saveButton->Location = System::Drawing::Point(80, 410);
		this->saveButton->Name = L"saveButton";
		this->saveButton->Size = System::Drawing::Size(56, 27);
		this->saveButton->TabIndex = 77;
		this->saveButton->Text = L"SAVE";
		this->saveButton->UseVisualStyleBackColor = true;
		this->saveButton->Click += gcnew System::EventHandler(this, &GeneratorForm::saveButton_Click);
		// 
		// loadButton
		// 
		this->loadButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->loadButton->Location = System::Drawing::Point(14, 410);
		this->loadButton->Name = L"loadButton";
		this->loadButton->Size = System::Drawing::Size(56, 27);
		this->loadButton->TabIndex = 76;
		this->loadButton->Text = L"LOAD";
		this->loadButton->UseVisualStyleBackColor = true;
		this->loadButton->Click += gcnew System::EventHandler(this, &GeneratorForm::loadButton_Click);
		// 
		// addCut
		// 
		this->addCut->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														  static_cast<System::Byte>(238)));
		this->addCut->FormattingEnabled = true;
		this->addCut->Items->AddRange(gcnew cli::array< System::Object^  >(5) { L"Random", L"Clock", L"None", L"Counterclock", L"Antispin" });
		this->addCut->Location = System::Drawing::Point(121, 379);
		this->addCut->Name = L"addCut";
		this->addCut->Size = System::Drawing::Size(149, 23);
		this->addCut->TabIndex = 74;
		this->addCut->Text = L"Select CutFunction";
		this->addCut->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::addCut_SelectedIndexChanged);
		// 
		// removeColorButton
		// 
		this->removeColorButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->removeColorButton->Location = System::Drawing::Point(249, 350);
		this->removeColorButton->Name = L"removeColorButton";
		this->removeColorButton->Size = System::Drawing::Size(21, 23);
		this->removeColorButton->TabIndex = 76;
		this->removeColorButton->Text = L"X";
		this->removeColorButton->UseVisualStyleBackColor = true;
		this->removeColorButton->Click += gcnew System::EventHandler(this, &GeneratorForm::removeColorButton_Click);
		// 
		// removeAngleButton
		// 
		this->removeAngleButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->removeAngleButton->Location = System::Drawing::Point(249, 321);
		this->removeAngleButton->Name = L"removeAngleButton";
		this->removeAngleButton->Size = System::Drawing::Size(21, 23);
		this->removeAngleButton->TabIndex = 70;
		this->removeAngleButton->Text = L"X";
		this->removeAngleButton->UseVisualStyleBackColor = true;
		this->removeAngleButton->Click += gcnew System::EventHandler(this, &GeneratorForm::removeAngleButton_Click);
		// 
		// addColorButton
		// 
		this->addColorButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->addColorButton->Location = System::Drawing::Point(121, 350);
		this->addColorButton->Name = L"addColorButton";
		this->addColorButton->Size = System::Drawing::Size(122, 23);
		this->addColorButton->TabIndex = 75;
		this->addColorButton->Text = L"ADD COLORS";
		this->addColorButton->UseVisualStyleBackColor = true;
		this->addColorButton->Click += gcnew System::EventHandler(this, &GeneratorForm::addColorButton_Click);
		// 
		// addAngleButton
		// 
		this->addAngleButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->addAngleButton->Location = System::Drawing::Point(121, 321);
		this->addAngleButton->Name = L"addAngleButton";
		this->addAngleButton->Size = System::Drawing::Size(122, 23);
		this->addAngleButton->TabIndex = 70;
		this->addAngleButton->Text = L"ADD ANGLES";
		this->addAngleButton->UseVisualStyleBackColor = true;
		this->addAngleButton->Click += gcnew System::EventHandler(this, &GeneratorForm::addAngleButton_Click);
		// 
		// addcutLabel
		// 
		this->addcutLabel->AutoSize = true;
		this->addcutLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->addcutLabel->ForeColor = System::Drawing::Color::White;
		this->addcutLabel->Location = System::Drawing::Point(14, 382);
		this->addcutLabel->Name = L"addcutLabel";
		this->addcutLabel->Size = System::Drawing::Size(101, 15);
		this->addcutLabel->TabIndex = 74;
		this->addcutLabel->Text = L"Add CutFunction:";
		// 
		// colorBox
		// 
		this->colorBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->colorBox->FormattingEnabled = true;
		this->colorBox->Items->AddRange(gcnew cli::array< System::Object^  >(5) { L"Random", L"Clock", L"None", L"Counterclock", L"Antispin" });
		this->colorBox->Location = System::Drawing::Point(14, 350);
		this->colorBox->Name = L"colorBox";
		this->colorBox->Size = System::Drawing::Size(101, 23);
		this->colorBox->TabIndex = 73;
		this->colorBox->Text = L"NewColorsName";
		// 
		// angleBox
		// 
		this->angleBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->angleBox->FormattingEnabled = true;
		this->angleBox->Items->AddRange(gcnew cli::array< System::Object^  >(5) { L"Random", L"Clock", L"None", L"Counterclock", L"Antispin" });
		this->angleBox->Location = System::Drawing::Point(14, 322);
		this->angleBox->Name = L"angleBox";
		this->angleBox->Size = System::Drawing::Size(101, 23);
		this->angleBox->TabIndex = 69;
		this->angleBox->Text = L"NewAngleName";
		// 
		// cutBox
		// 
		this->cutBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														  static_cast<System::Byte>(238)));
		this->cutBox->Location = System::Drawing::Point(214, 292);
		this->cutBox->Name = L"cutBox";
		this->cutBox->Size = System::Drawing::Size(56, 23);
		this->cutBox->TabIndex = 70;
		this->cutBox->Text = L"0";
		this->cutBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::cutBox_TextChanged);
		// 
		// minBox
		// 
		this->minBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														  static_cast<System::Byte>(238)));
		this->minBox->Location = System::Drawing::Point(148, 292);
		this->minBox->Name = L"minBox";
		this->minBox->Size = System::Drawing::Size(56, 23);
		this->minBox->TabIndex = 72;
		this->minBox->Text = L"0";
		this->minBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::minBox_TextChanged);
		// 
		// maxBox
		// 
		this->maxBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														  static_cast<System::Byte>(238)));
		this->maxBox->Location = System::Drawing::Point(80, 292);
		this->maxBox->Name = L"maxBox";
		this->maxBox->Size = System::Drawing::Size(56, 23);
		this->maxBox->TabIndex = 71;
		this->maxBox->Text = L"0";
		this->maxBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::maxBox_TextChanged);
		// 
		// sizeBox
		// 
		this->sizeBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														   static_cast<System::Byte>(238)));
		this->sizeBox->Location = System::Drawing::Point(14, 292);
		this->sizeBox->Name = L"sizeBox";
		this->sizeBox->Size = System::Drawing::Size(56, 23);
		this->sizeBox->TabIndex = 69;
		this->sizeBox->Text = L"0";
		this->sizeBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::sizeBox_TextChanged);
		// 
		// paramLabel
		// 
		this->paramLabel->AutoSize = true;
		this->paramLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->paramLabel->ForeColor = System::Drawing::Color::White;
		this->paramLabel->Location = System::Drawing::Point(14, 274);
		this->paramLabel->Name = L"paramLabel";
		this->paramLabel->Size = System::Drawing::Size(252, 15);
		this->paramLabel->TabIndex = 70;
		this->paramLabel->Text = L"ChildSize      MaxSize        MinSize          Cutsize ";
		// 
		// pointPanel
		// 
		this->pointPanel->Controls->Add(this->addPoint);
		this->pointPanel->Location = System::Drawing::Point(14, 31);
		this->pointPanel->Name = L"pointPanel";
		this->pointPanel->Size = System::Drawing::Size(256, 240);
		this->pointPanel->TabIndex = 69;
		// 
		// addPoint
		// 
		this->addPoint->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->addPoint->Location = System::Drawing::Point(15, 11);
		this->addPoint->Name = L"addPoint";
		this->addPoint->Size = System::Drawing::Size(205, 23);
		this->addPoint->TabIndex = 69;
		this->addPoint->Text = L"ADD CHILD";
		this->addPoint->UseVisualStyleBackColor = true;
		this->addPoint->Click += gcnew System::EventHandler(this, &GeneratorForm::addPoint_Click);
		// 
		// pointLabel
		// 
		this->pointLabel->AutoSize = true;
		this->pointLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->pointLabel->ForeColor = System::Drawing::Color::White;
		this->pointLabel->Location = System::Drawing::Point(29, 13);
		this->pointLabel->Name = L"pointLabel";
		this->pointLabel->Size = System::Drawing::Size(145, 15);
		this->pointLabel->TabIndex = 69;
		this->pointLabel->Text = L"    X              Y             Angle";
		// 
		// modeButton
		// 
		this->modeButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->modeButton->Location = System::Drawing::Point(256, 14);
		this->modeButton->Name = L"modeButton";
		this->modeButton->Size = System::Drawing::Size(47, 23);
		this->modeButton->TabIndex = 69;
		this->modeButton->Text = L"EDIT";
		this->modeButton->UseVisualStyleBackColor = true;
		this->modeButton->Click += gcnew System::EventHandler(this, &GeneratorForm::modeButton_Click);
		// 
		// generatorPanel
		// 
		this->generatorPanel->Controls->Add(this->mp4Button);
		this->generatorPanel->Controls->Add(this->fpsBox);
		this->generatorPanel->Controls->Add(this->voidBox);
		this->generatorPanel->Controls->Add(this->defaultHue);
		this->generatorPanel->Controls->Add(this->infoLabel);
		this->generatorPanel->Controls->Add(this->statusLabel);
		this->generatorPanel->Controls->Add(this->threadsLabel);
		this->generatorPanel->Controls->Add(this->dotLabel);
		this->generatorPanel->Controls->Add(this->encodeSelect);
		this->generatorPanel->Controls->Add(this->voidLabel);
		this->generatorPanel->Controls->Add(this->animateButton);
		this->generatorPanel->Controls->Add(this->restartButton);
		this->generatorPanel->Controls->Add(this->prevButton);
		this->generatorPanel->Controls->Add(this->zoomSelect);
		this->generatorPanel->Controls->Add(this->nextButton);
		this->generatorPanel->Controls->Add(this->brightnessLabel);
		this->generatorPanel->Controls->Add(this->delayBox);
		this->generatorPanel->Controls->Add(this->brightnessBox);
		this->generatorPanel->Controls->Add(this->delayLabel);
		this->generatorPanel->Controls->Add(this->abortBox);
		this->generatorPanel->Controls->Add(this->periodBox);
		this->generatorPanel->Controls->Add(this->threadsBox);
		this->generatorPanel->Controls->Add(this->gifButton);
		this->generatorPanel->Controls->Add(this->blurLabel);
		this->generatorPanel->Controls->Add(this->pngButton);
		this->generatorPanel->Controls->Add(this->blurBox);
		this->generatorPanel->Controls->Add(this->defaultZoom);
		this->generatorPanel->Controls->Add(this->bloomBox);
		this->generatorPanel->Controls->Add(this->defaultAngle);
		this->generatorPanel->Controls->Add(this->detailBox);
		this->generatorPanel->Controls->Add(this->periodMultiplierBox);
		this->generatorPanel->Controls->Add(this->saturateBox);
		this->generatorPanel->Controls->Add(this->periodLabel);
		this->generatorPanel->Controls->Add(this->noiseBox);
		this->generatorPanel->Controls->Add(this->helpButton);
		this->generatorPanel->Controls->Add(this->ambBox);
		this->generatorPanel->Controls->Add(this->zoomLabel);
		this->generatorPanel->Controls->Add(this->parallelTypeSelect);
		this->generatorPanel->Controls->Add(this->spinSelect);
		this->generatorPanel->Controls->Add(this->hueSpeedBox);
		this->generatorPanel->Controls->Add(this->hueSelect);
		this->generatorPanel->Controls->Add(this->spinSpeedBox);
		this->generatorPanel->Controls->Add(this->spinLabel);
		this->generatorPanel->Controls->Add(this->label1);
		this->generatorPanel->Location = System::Drawing::Point(17, 159);
		this->generatorPanel->Name = L"generatorPanel";
		this->generatorPanel->Size = System::Drawing::Size(286, 441);
		this->generatorPanel->TabIndex = 70;
		// 
		// mp4Button
		// 
		this->mp4Button->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->mp4Button->Location = System::Drawing::Point(201, 403);
		this->mp4Button->Name = L"mp4Button";
		this->mp4Button->Size = System::Drawing::Size(69, 27);
		this->mp4Button->TabIndex = 65;
		this->mp4Button->Text = L"Save Mp4";
		this->mp4Button->UseVisualStyleBackColor = true;
		this->mp4Button->Click += gcnew System::EventHandler(this, &GeneratorForm::Mp4Button_Click);
		// 
		// fpsBox
		// 
		this->fpsBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->fpsBox->Location = System::Drawing::Point(225, 313);
		this->fpsBox->Name = L"fpsBox";
		this->fpsBox->Size = System::Drawing::Size(45, 23);
		this->fpsBox->TabIndex = 64;
		this->fpsBox->Text = L"20";
		this->fpsBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::FpsBox_TextChanged);
		// 
		// voidBox
		// 
		this->voidBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														   static_cast<System::Byte>(238)));
		this->voidBox->Location = System::Drawing::Point(225, 255);
		this->voidBox->Name = L"voidBox";
		this->voidBox->Size = System::Drawing::Size(45, 23);
		this->voidBox->TabIndex = 63;
		this->voidBox->Tag = L"";
		this->voidBox->Text = L"8";
		this->voidBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::VoidBox_TextChanged);
		// 
		// saveFractal
		// 
		this->saveFractal->DefaultExt = L"fractal";
		this->saveFractal->Filter = L"FRACTAL files (*.fractal)|*.fractal";
		// 
		// loadFractal
		// 
		this->loadFractal->DefaultExt = L"fractal";
		this->loadFractal->Filter = L"FRACTAL files (*.fractal)|*.fractal";
		// 
		// saveMp4
		// 
		this->saveMp4->DefaultExt = L"mp4";
		this->saveMp4->Filter = L"MP4 files (*.mp4)|*.mp4";
		this->saveMp4->FileOk += gcnew System::ComponentModel::CancelEventHandler(this, &GeneratorForm::SaveVideo_FileOk);
		// 
		// convertMp4
		// 
		this->convertMp4->DefaultExt = L"mp4";
		this->convertMp4->Filter = L"MP4 files (*.mp4)|*.mp4";
		this->convertMp4->FileOk += gcnew System::ComponentModel::CancelEventHandler(this, &GeneratorForm::ConvertMp4_FileOk);
		// 
		// GeneratorForm
		// 
		this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
		this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
		this->BackColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(64)), static_cast<System::Int32>(static_cast<System::Byte>(64)),
														   static_cast<System::Int32>(static_cast<System::Byte>(64)));
		this->ClientSize = System::Drawing::Size(1079, 1249);
		this->Controls->Add(this->generatorPanel);
		this->Controls->Add(this->modeButton);
		this->Controls->Add(this->editorPanel);
		this->Controls->Add(this->debugLabel);
		this->Controls->Add(this->debugBox);
		this->Controls->Add(this->resSelect);
		this->Controls->Add(this->cutLabel);
		this->Controls->Add(this->colorLabel);
		this->Controls->Add(this->angleLabel);
		this->Controls->Add(this->cutSelect);
		this->Controls->Add(this->colorSelect);
		this->Controls->Add(this->angleSelect);
		this->Controls->Add(this->helpPanel);
		this->Controls->Add(this->cutparamBox);
		this->Controls->Add(this->fractalLabel);
		this->Controls->Add(this->fractalSelect);
		this->Controls->Add(this->resX);
		this->Controls->Add(this->resY);
		this->Icon = (cli::safe_cast<System::Drawing::Icon^>(resources->GetObject(L"$this.Icon")));
		this->Name = L"GeneratorForm";
		this->Text = L"RGB Fractal Zoom Generator Clr v1.84";
		this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &GeneratorForm::GeneratorForm_FormClosing);
		this->Load += gcnew System::EventHandler(this, &GeneratorForm::GeneratorForm_Load);
		this->helpPanel->ResumeLayout(false);
		this->helpPanel->PerformLayout();
		this->editorPanel->ResumeLayout(false);
		this->editorPanel->PerformLayout();
		this->pointPanel->ResumeLayout(false);
		this->generatorPanel->ResumeLayout(false);
		this->generatorPanel->PerformLayout();
		this->ResumeLayout(false);
		this->PerformLayout();

	}
#pragma endregion

#pragma region Bitmaps
	System::Void GeneratorForm::UnlockBitmaps() {
		for (auto b = 0; b < generator->GetTotalFrames(); ++b)
			if (generator->GetBitmapState(b) >= BitmapState::Drawing && generator->GetBitmapState(b) < BitmapState::UnlockedRAM) {
				try {
					//bitmapState[bitmapsFinished] = BitmapState.Finished;
					if (bitmap[b] != nullptr) bitmap[b]->UnlockBits(bitmapData[b]);
				} catch (Exception^) {}
			}
	}
	System::Void GeneratorForm::ReencodeBitmaps() {
		for (int b = generator->GetPreviewFrames(); b < generator->GetTotalFrames(); ++b)
			if (generator->GetBitmapState(b) >= BitmapState::Encoding) {
				if (generator->GetBitmapState(b) == BitmapState::Unlocked)
					bitmapData[b] = bitmap[b]->LockBits(rect, ImageLockMode::ReadOnly, PixelFormat::Format24bppRgb);
				generator->GetBitmapState(b) = BitmapState::DrawingFinished;
			}
	}
	System::Void GeneratorForm::SetRect() {
		rect = System::Drawing::Rectangle(0, 0, generator->SelectedWidth, generator->SelectedHeight);
	}
	System::Void GeneratorForm::NewBitmap(const uint16_t bitmapIndex, const uint16_t w, const uint16_t h) {
		generator->bitmap[bitmapIndex] = (uint8_t*)(void*)((bitmapData[bitmapIndex] = (bitmap[bitmapIndex] = gcnew Bitmap(w,h))->LockBits( // make a new bitmaps and lock it's bits
			(generator->GetApplyParallelType() == ParallelType::OfDepth ? bitmapIndex : bitmapIndex + 1) < generator->GetPreviewFrames() ? System::Drawing::Rectangle(0, 0, w, h) : rect, // The ractangle (use a new smaller one for smaller previews)
			ImageLockMode::ReadWrite, // Writing now, possibly reading for encoding gif later
			System::Drawing::Imaging::PixelFormat::Format24bppRgb))->Scan0); // BGR format (no alpha, if the gif is transparent, the transparency is just the color black)
		generator->strides[bitmapIndex] = bitmapData[bitmapIndex]->Stride;
	}
	System::Void GeneratorForm::AllocateBitmaps() {
		bitmap = gcnew array<Bitmap^>(generator->GetTotalFrames());
		bitmapData = gcnew array<BitmapData^>(generator->GetTotalFrames());
	}
	System::Void GeneratorForm::ReLockBits(const uint16_t bitmapIndex) {
		bitmapData[bitmapIndex] = bitmap[bitmapIndex]->LockBits(rect, ImageLockMode::ReadOnly, PixelFormat::Format24bppRgb);
	}
	System::Void GeneratorForm::UnlockBits(const uint16_t bitmapsFinished) {
		bitmap[bitmapsFinished]->UnlockBits(bitmapData[bitmapsFinished]);
	}
#pragma endregion

#pragma region Core
	bool GeneratorForm::Error(System::String^ text, System::String^ caption) {
		MessageBox::Show(text, caption, MessageBoxButtons::OK, MessageBoxIcon::Error);
		return false;
	}
	System::Void GeneratorForm::UpdateBitmap(Bitmap^ bitmap) {
		if (currentBitmap == bitmap)
			return;
		// Update the display with the bitmap when it's not loaded
		currentBitmap = bitmap;
		if (screenPanel != nullptr)
			screenPanel->Invalidate();
	}
	System::Void GeneratorForm::UpdatePreview() {
		Monitor::Enter(this);
		try {
			int bitmapsFinished = generator->GetBitmapsFinished();
			// Fetch a bitmap to display						
			UpdateBitmap(bitmapsFinished > 0
						 ? bitmap[generator->GetPreviewFrames() + (currentBitmapIndex = (animated ? currentBitmapIndex + 1 : currentBitmapIndex) % bitmapsFinished)] // Make sure the index is is range
						 : (generator->GetTotalFinished() < 1 ? nullptr : bitmap[generator->GetTotalFinished() - 1]) // Try preview bitmap if none of the main ones are generated yet
			);
		} finally { Monitor::Exit(this); }
	}
	System::Void GeneratorForm::SetupControl(Control^ control, System::String^ tip) {
		// Add tooltip and set the next tabIndex
		toolTips->SetToolTip(control, tip);
		control->TabIndex = ++controlTabIndex;
		MyControls->Add(control);
	}
	System::Void GeneratorForm::SetupEditControl(System::Windows::Forms::Control^ control, System::String^ tip) {
		// Add tooltip and set the next tabIndex
		toolTips->SetToolTip(control, tip);
		control->TabIndex = ++pointTabIndex;
		MyControls->Add(control);
	}
	System::Void GeneratorForm::GeneratorForm_Load(System::Object^ sender, System::EventArgs^ e) {

		// Create the Generator
		generator = gcnew FractalGenerator();
		auto& fractals = generator->GetFractals();
		for (int32_t i = 0; i < static_cast<int32_t>(fractals.size()); ++i)
			fractalSelect->Items->Add(gcnew String((fractals[i]->name).c_str()));
			
		generator->SelectedFractal = -1;
		generator->restartGif = false;
		generator->UpdatePreview += gcnew Action(this, &GeneratorForm::UpdatePreview);
		generator->UnlockBitmaps += gcnew Action(this, &GeneratorForm::UnlockBitmaps);
		generator->ReencodeBitmaps += gcnew Action(this, &GeneratorForm::ReencodeBitmaps);
		generator->SetRect += gcnew Action(this, &GeneratorForm::SetRect);
		generator->NewBitmap += gcnew Action<const uint16_t, const uint16_t, const uint16_t>(this, &GeneratorForm::NewBitmap);
		generator->AllocateBitmaps += gcnew Action(this, &GeneratorForm::AllocateBitmaps);
		generator->ReLockBits += gcnew Action<const uint16_t>(this, &GeneratorForm::ReLockBits);
		generator->UnlockBits += gcnew Action<const uint16_t>(this, &GeneratorForm::UnlockBits);

		//System::Void NewBitmap(const uint16_t bitmapIndex, const uint16_t w, const uint16_t h);
		//System::Void GeneratorForm::UnlockBits(const uint16_t bitmapsFinished);
		//System::Void GeneratorForm::ReLockBits(const uint16_t bitmapsFinished);

		
		// Setup interactable controls (tooltips + tabIndex)
		SetupControl(sizeBox, L"Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)");
		SetupControl(maxBox, L"The root scale, if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance.\nIf pieces of the fractal are disappearing at the CORNERS, you should increase it, if not you can try decreasing a little.");
		SetupControl(minBox, L"How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance).\n Try setting the detail parameter to maximum, and it the fractal starts dithering when zooming, you should decrease the minSize.");
		SetupControl(cutBox, L"A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance).\nIf you see pieces of fractals diappearing near the EDGES, you should increase it.");
		SetupControl(angleBox, L"Type the name for a new children angle set, if you wish to add one.");
		SetupControl(addAngleButton, L"Add a new children angle set.");
		SetupControl(removeAngleButton, L"Remove the selected children angle set.");
		SetupControl(colorBox, L"Type the name for a new children color set, if you wish to add one.");
		SetupControl(addColorButton, L"Add a new children color set.");
		SetupControl(removeColorButton, L"Remove the selected children color set.");
		SetupControl(addCut, L"Add a CutFunction (so far only the precoded are avaiable)");
		SetupControl(loadButton, L"Load a fractal definition from a file.");
		SetupControl(saveButton, L"Save the selected fractal definiton to a file.");
		SetupControl(fractalSelect, L"Select the type of fractal to generate");
		SetupControl(modeButton, L"Toggle between editor and generator.");
		SetupControl(angleSelect, L"Select the children angles definition.");
		SetupControl(colorSelect, L"Select the children colors definition.");
		SetupControl(cutSelect, L"Select the cutfunction definition.");
		SetupControl(cutparamBox, L"Type the cutfunction seed. (-1 for random)");
		SetupControl(resX, L"Type the X resolution of the render (width)");
		SetupControl(resY, L"Type the Y resolution of the render (height)");
		SetupControl(resSelect, L"Select a rendering resolution (the second choise is the custom resolution you can type in the boxes to the left");
		SetupControl(periodBox, L"How many frames for the self-similar center child to zoom in to the same size as the parent if you are zooming in.\nOr for the parent to zoom out to the same size as the child if you are zooming out.\nThis will equal the number of generated frames of the animation if the center child is the same color.\nOr it will be a third of the number of generated frames of the animation if the center child is a different color.");
		SetupControl(periodMultiplierBox, L"Multiplies the frame count, slowing down the rotaion and hue shifts.");
		SetupControl(zoomSelect, L"Choose in which direction you want the fractal zoom.");
		SetupControl(defaultZoom, L"Type the initial zoom of the first image in number of skipped frames. -1 for random");
		SetupControl(spinSelect, L"Choose in which direction you want the zoom animation to spin, or to not spin.");
		SetupControl(spinSpeedBox, L"Type the extra speed on the spinning from the values possible for looping.");
		SetupControl(defaultAngle, L"Type the initial angle of the first image (in degrees).");
		SetupControl(hueSelect, L"Choose the color scheme of the fractal.\nAlso you can make the color hues slowly cycle as the fractal zoom.");
		SetupControl(hueSpeedBox, L"Type the extra speed on the hue shifting from the values possible for looping.\nOnly possible if you have chosen color cycling on the left.");
		SetupControl(defaultHue, L"Type the initial hue angle of the first image (in degrees).");
		SetupControl(ambBox, L"The strength of the ambient grey color in the empty spaces far away between the generated fractal dots.\n-1 for transparent");
		SetupControl(noiseBox, L"The strength of the random noise in the empty spaces far away between the generated fractal dots.");
		SetupControl(saturateBox, L"Enhance the color saturation of the fractal dots.\nUseful when the fractal is too gray, like the Sierpinski Triangle (Triflake).");
		SetupControl(detailBox, L"Level of Detail (The lower the finer).");
		SetupControl(bloomBox, L"Bloom: 0 will be maximally crisp, but possibly dark with think fractals. Higher values wil blur/bloom out the fractal dots.");
		SetupControl(blurBox, L"Motion blur: Lowest no blur and fast generation, highest 10 smeared frames 10 times slower generation.");
		SetupControl(brightnessBox, L"Brightness level: 0% black, 100% normalized maximum, 300% overexposed 3x maximum.");
		SetupControl(voidBox, L"Scale of the void noise, so it's more visible and compressable at higher resolutions.");
		SetupControl(parallelTypeSelect, L"Select which parallelism to be used if the left checkBox is enabled.\nOf Animation = Batching animation frames, recommended for Animations with perfect pixels.\nOf Depth/Of Recursion = parallel single image generation, recommmended for fast single images, 1 in a million pixels might be slightly wrong");
		SetupControl(threadsBox, L"The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
		SetupControl(abortBox, L"How many millisecond of pause after the last settings change until the generator restarts?");
		SetupControl(delayBox, L"A delay between frames in 1/100 of seconds for the preview and exported GIF file.\nThe framerate will be roughly 100/delay");
		SetupControl(prevButton, L"Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
		SetupControl(animateButton, L"Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
		SetupControl(nextButton, L"Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
		SetupControl(restartButton, L"Restarts the generaor. Maybe useful for randomized settings, but to be safe you have to click it twice in a row.");
		SetupControl(encodeSelect, L"Only Image - only generates one image\nAnimation RAM - generated an animation without GIF encoding, faster but can't save GIF afterwards\nEncode GIF - encodes GIF while generating an animation - can save a GIF afterwards");
		SetupControl(helpButton, L"Show README.txt.");
		SetupControl(pngButton, L"Save the currently displayed frame into a PNG file.\nStop the animation and select the frame you wish to export with the buttons above.");
		SetupControl(gifButton, L"Save the full animation into a GIF file.");
		SetupControl(debugBox, L"shows a log of task and image states, to see what the generator is doing.");

		// Read the REDME.txt for the help button
		if (File::Exists("README.txt"))
			helpLabel->Text = File::ReadAllText("README.txt");

		// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restant the generator over and over
		fractalSelect->SelectedIndex = 0;
		resSelect->SelectedIndex = 0;
		AbortBox_TextChanged(DN);
		PeriodBox_TextChanged(DN);
		PeriodMultiplierBox_TextChanged(DN);
		ParallelTypeSelect_SelectedIndexChanged(DN);
		DelayBox_TextChanged(DN);
		FpsBox_TextChanged(DN);
		DefaultZoom_TextChanged(DN);
		SpinSpeedBox_TextChanged(DN);
		HueSpeedBox_TextChanged(DN);
		DefaultHue_TextChanged(DN);
		AmbBox_TextChanged(DN);
		NoiseBox_TextChanged(DN);
		BloomBox_TextChanged(DN);
		BlurBox_TextChanged(DN);
		SaturateBox_TextChanged(DN);
		BrightnessBox_TextChanged(DN);
		VoidBox_TextChanged(DN);
		SetAnimate();
		parallelTypeSelect->SelectedIndex = 0;
		spinSelect->SelectedIndex = 1;
		zoomSelect->SelectedIndex = 1;
		hueSelect->SelectedIndex = 1;
		encodeSelect->SelectedIndex = 2;
		maxTasks = Math::Max(MINTASKS, Environment::ProcessorCount - 2);

		SetupFractal();
		threadsBox->Text = (maxTasks).ToString();

		// try to restory the last closed settings and init the editor
		editorPanel->Visible = false;
		pointTabIndex = controlTabIndex;
		editorPanel->Location = generatorPanel->Location;
		LoadSettings();
		FillEditor();

		// Start the generator
		helpPanel->Visible = modifySettings = false;
		TryResize();
		ResizeAll();
		aTask = gTask = nullptr;
		generator->StartGenerate();

		// Load all extra fractal files
		System::String^ appDirectory = AppDomain::CurrentDomain->BaseDirectory; // Get the app's directory
		System::String^ searchPattern = "*.fractal"; // Change to your desired file type
		auto files = Directory::GetFiles(appDirectory, searchPattern);
		for(int i = 0; i < files->Length; ++i)
			LoadFractal(files[i], false);

		// List all cutfunction to add in the editor
		addCut->Items->Add("Select CutFunction to Add");
		for(auto& c : Fractal::cutFunctions)
			addCut->Items->Add(gcnew System::String(c.first.c_str()));
	}
	System::Void GeneratorForm::timer_Tick(System::Object^ sender, System::EventArgs^ e) {
		if (generator->debugmode) {
			debugLabel->Text = generator->logString;
			SetMinimumSize();
		}
		// Window Size Update
		WindowSizeRefresh();
		const auto gTaskNotRunning = IsTaskNotRunning(gTask);
		if (queueReset > 0) {
			if (!(gTaskNotRunning && IsTaskNotRunning(aTask)))
				return;
			if (queueAbort) {
				aTask = Task::Run(gcnew Action(this, &GeneratorForm::Abort), (cancel = gcnew CancellationTokenSource())->Token);
				return;
			}
			if ((queueReset -= (short)timer->Interval) > 0)
				return;
			SetupFractal();
			ResizeAll();
			restartButton->Enabled = true;
			ResetRestart();
			generator->StartGenerate();
		}
		if (restartTimer > 0 && (restartTimer -= timer->Interval) <= 0)
			ResetRestart();
		// Fetch the state of generated bitmaps
		const auto bitmapsFinished = generator->GetBitmapsFinished(), bitmapsTotal = generator->GetFrames();
		if (bitmapsTotal <= 0)
			return;
		// Only Allow GIF Export when generation is finished
		gifButton->Enabled = generator->IsGifReady() && gTaskNotRunning;
		UpdatePreview();
		// Info text refresh
		System::String^ infoText = " / " + bitmapsTotal.ToString();
		if (bitmapsFinished < bitmapsTotal) {
			statusLabel->Text = "Generating: ";
			infoText = bitmapsFinished.ToString() + infoText;
		} else {
			statusLabel->Text = "Finished: ";
			infoText = currentBitmapIndex.ToString() + infoText;
		}
		infoLabel->Text = infoText;
		gifButton->Text = gTaskNotRunning ? "Save GIF" : "Saving GIF...";
	}
	System::Void GeneratorForm::SaveSettings() {
		auto f = generator->GetFractal();
		File::WriteAllText("settings.txt", "fractal|" + fractalSelect->Text + "|path|" + gcnew System::String(f->path.c_str()) + "|preview|" + (previewMode ? 1 : 0) + "|edit|" + (editorPanel->Visible ? 1 : 0) + "|angle|" + angleSelect->SelectedIndex + "|color|" + colorSelect->SelectedIndex + "|cut|" + cutSelect->SelectedIndex + "|seed|" + cutparamBox->Text
						  + "|w|" + resX->Text + "|h|" + resY->Text + "|res|" + resSelect->SelectedIndex + "|period|" + periodBox->Text + "|periodmul|" + periodMultiplierBox->Text + "|zoom|" + zoomSelect->SelectedIndex + "|defaultzoom|" + defaultZoom->Text
						  + "|spin|" + spinSelect->SelectedIndex + "|spinmul|" + spinSpeedBox->Text + "|defaultangle|" + defaultAngle->Text + "|hue|" + hueSelect->SelectedIndex + "|huemul|" + hueSpeedBox->Text + "|defaulthue|" + defaultHue->Text + "|amb|" + ambBox->Text
						  + "|noise|" + noiseBox->Text + "|saturate|" + saturateBox->Text + "|detail|" + detailBox->Text + "|bloom|" + bloomBox->Text + "|blur|" + blurBox->Text + "|brightness|" + brightnessBox->Text + "|parallel|" + parallelTypeSelect->SelectedIndex
						  + "|threads|" + threadsBox->Text + "|abort|" + abortBox->Text + "|delay|" + delayBox->Text + "|ani|" + (animated ? 1 : 0) + "|gen|" + encodeSelect->SelectedIndex);
	}
	System::Void GeneratorForm::LoadSettings() {
		gifButton->Enabled = false;
		if (!File::Exists("settings->txt"))
			return;
		auto s = File::ReadAllText("settings->txt")->Split('|');
		for (int i = 0; i < s->Length - 1; i += 2) {
			auto v = s[i + 1];
			int n;
			bool p = int::TryParse(v, n);
			auto si = s[i];
			if(si == "path") {if (v != "" && File::Exists(v)) LoadFractal(v, true); }
			else if(si == "fractal") {if (fractalSelect->Items->Contains(v)) fractalSelect->SelectedItem = v; }
			else if(si == "preview") {if (p) previewMode = n > 0; }
			else if (si == "edit") { if (p) editorPanel->Visible = n > 0; generatorPanel->Visible = n <= 0; }
			else if(si == "angle") {if (p) angleSelect->SelectedIndex = Math::Min(angleSelect->Items->Count - 1, n); }
			else if(si == "color") {if (p) colorSelect->SelectedIndex = Math::Min(colorSelect->Items->Count - 1, n); }
			else if(si == "cut") {if (p) cutSelect->SelectedIndex = Math::Min(cutSelect->Items->Count - 1, n); }
			else if(si == "seed") {cutparamBox->Text = v; }
			else if(si == "w") {if (p) width = (short)n; }
			else if(si == "h") {if (p) height = (short)n; }
			else if(si == "res") {if (p) resSelect->SelectedIndex = Math::Min(resSelect->Items->Count - 1, n); }
			else if(si == "period") {periodBox->Text = v; }
			else if(si == "periodmul") {periodMultiplierBox->Text = v; }
			else if(si == "zoom") {if (p) zoomSelect->SelectedIndex = Math::Min(zoomSelect->Items->Count - 1, n); }
			else if(si == "defaultzoom") {defaultZoom->Text = v; }
			else if(si == "spin") {if (p) spinSelect->SelectedIndex = Math::Min(spinSelect->Items->Count - 1, n); }
			else if(si == "spinmul") {spinSpeedBox->Text = v; }
			else if(si == "defaultangle") {defaultAngle->Text = v; }
			else if(si == "hue") {if (p) hueSelect->SelectedIndex = n; }
			else if(si == "huemul") {hueSpeedBox->Text = v; }
			else if(si == "defaulthue") {defaultHue->Text = v; }
			else if(si == "amb") {ambBox->Text = v; }
			else if(si == "noise") {noiseBox->Text = v; }
			else if(si == "saturate") {saturateBox->Text = v; }
			else if(si == "detail") {detailBox->Text = v; }
			else if(si == "bloom") {bloomBox->Text = v; }
			else if(si == "blur") {blurBox->Text = v; }
			else if(si == "brightness") {brightnessBox->Text = v; }
			else if(si == "parallel") {parallelTypeSelect->SelectedIndex = Math::Min(parallelTypeSelect->Items->Count - 1, n); }
			else if(si == "threads") {threadsBox->Text = v; }
			else if(si == "abort") {abortBox->Text = v; }
			else if(si == "delay") {delayBox->Text = v; }
			else if(si == "ani") {if (p) animated = i <= 0; AnimateButton_Click(DN); }
			else if(si == "gen") {if (p) encodeSelect->SelectedIndex = Math::Min(encodeSelect->Items->Count - 1, n); }
		}
		if (editorPanel->Visible) {
			
			mem_generate = encodeSelect->SelectedIndex;
			mem_blur = generator->SelectedBlur;
			mem_bloom = generator->SelectedBloom;
			mem_hue = (short)hueSelect->SelectedIndex;

			generator->SelectedGenerationType = GenerationType::Animation;
			generator->SelectedBloom = generator->SelectedBlur = 0;
			generator->SelectedHue = generator->SelectedDefaultHue = 0;
			generator->SelectedPreviewMode = previewMode;
			abortDelay = 10;

			uint16_t n;
			if (uint16_t::TryParse(defaultHue->Text, n))
				mem_defaulthue = n;
			if (uint16_t::TryParse(abortBox->Text, n))
				mem_abort = n;
		} else {
			generator->SelectedPreviewMode = false;
		}
		SetupFractal();
	}
#pragma endregion

#pragma region Input
	System::Void GeneratorForm::ResizeAll() {
		generator->SelectedWidth = width;
		generator->SelectedHeight = height;
		generator->SetMaxIterations();
		// Update the size of the window and display
		SetMinimumSize();
		SetClientSizeCore(width + 314, Math::Max(height + 8, 320));
		ResizeScreen();
		WindowSizeRefresh();
#ifdef CUSTOMDEBUGTEST
		generator->DebugStart(); animated = false;
#endif
		if (modifySettings)
			return;
		// Resets the generator
		// (Abort should be called before this or else it will crash, generator->StartGenerate should be called after)
		gifButton->Enabled = false;
		currentBitmapIndex = 0;
		generator->ResetGenerator();
		SizeAdapt();
	}
	bool GeneratorForm::TryResize() {
		//previewMode = !previewBox->Checked;
		width = 8;
		height = 8;
		if (!int16_t::TryParse(resX->Text, width) || width <= 8)
			width = 8;
		if (!int16_t::TryParse(resY->Text, height) || height <= 0)
			height = 8;
		auto c = "Custom:" + width.ToString() + "x" + height.ToString();;
		if (resSelect->Items[1]->ToString() != c) {
			resSelect->Items[1] = c;
		}
		//previewBox->Text = "Resolution: " + width.ToString() + "x" + height.ToString();
		//if (previewMode)
		//	width = height = 80;
		auto rxy = resSelect->SelectedIndex == 1 || resSelect->SelectedIndex < 0 ? resSelect->Items[1]->ToString()->Split(':')[1]->Split('x') : resSelect->Items[resSelect->SelectedIndex]->ToString()->Split('x');
		if (!int16_t::TryParse(rxy[0], width))
			width = 80;
		if (!int16_t::TryParse(rxy[1], height))
			height = 80;
		return generator->SelectedWidth != width || generator->SelectedHeight != height;
	}
	System::Void GeneratorForm::WindowSizeRefresh() {
		if (fx == Width && fy == Height)
			return;
		// User has manually resized the window - strech the display
		ResizeScreen();
		SetMinimumSize();
		int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
		SetClientSizeCore(
			Math::Max(Width - bw, 314 + Math::Max(screenPanel->Width, (int)width)),
			Math::Max(Height - bh, 8 + Math::Max(screenPanel->Height, (int)height))
		);
		SizeAdapt();
	}
	System::Void GeneratorForm::SizeAdapt() {
		fx = Width;
		fy = Height;
	}
	System::Void GeneratorForm::SetMinimumSize() {
		// bw = Width - ClientWidth = 16
			// bh = Height - ClientHeight = 39
		int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
		MinimumSize = System::Drawing::Size(
			Math::Max(640, bw + width + 284),
			Math::Max(Math::Max(640, debugLabel->Bounds.Bottom + bh), bh + Math::Max(460, height + 8))
		);
	}
	System::Void GeneratorForm::ResizeScreen() {
		int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
		const auto screenHeight = Math::Max((int)height, Math::Min(Height - bh - 8, (Width - bw - 314) * (int)height / (int)width));
		screenPanel->SetBounds(305, 4, screenHeight * width / height, screenHeight);
		helpPanel->SetBounds(305, 4, Width - bw - 314, Height - bh - 8);
		screenPanel->Invalidate();
	}
	System::Void GeneratorForm::GeneratorForm_FormClosing(System::Object^ sender, System::Windows::Forms::FormClosingEventArgs^ e) {
		Close(e);
	}
	System::Void GeneratorForm::Close(System::Windows::Forms::FormClosingEventArgs^ e) {
		const auto gTaskRunning = !IsTaskNotRunning(gTask);
		if (gTaskRunning) {
			auto result = MessageBox::Show(
				"Your GIF is still saving!\nAre you sure you want to close the application and potentially lose it?",
				"Confirm Exit",
				MessageBoxButtons::YesNo,
				MessageBoxIcon::Question);
			// Cancel closing if the user clicks "No"
			if (result == System::Windows::Forms::DialogResult::No)
				e->Cancel = true;
			return;
		}
		if (isGifReady > 80) {
			auto result = MessageBox::Show(
				"You have encoded gif available to save.\nDo you want to save it?",
				"Confirm Exit",
				MessageBoxButtons::YesNo,
				MessageBoxIcon::Question);
			if (result == System::Windows::Forms::DialogResult::Yes) {
				saveGif->ShowDialog();
				e->Cancel = true;
				return;
			}
		}
		bool saved = false;

		auto& fractals = generator->GetFractals();
		for (int32_t i = static_cast<int32_t>(fractals.size()); 0 <= --i;) {
			auto& f = *fractals[i];
			if (f.edit) {
				auto result = MessageBox::Show(
					"Fractal " + gcnew System::String(f.name.c_str()) + " has been edited. Do you want to save it before closing?",
					"Confirm Exit",
					MessageBoxButtons::YesNoCancel,
					MessageBoxIcon::Question);
				// Cancel closing if the user clicks "No"
				switch (result) {
				case System::Windows::Forms::DialogResult::Yes:
					tosave = &f;
					saved = true;
					saveFractal->ShowDialog();
					continue;
				case System::Windows::Forms::DialogResult::No:
					f.edit = false;
					continue;
				case System::Windows::Forms::DialogResult::Cancel:
					e->Cancel = true;
					return;
				}
				return;
			}
		}
		if (saved) {
			e->Cancel = true;
			return;
		}

		if (cancel != nullptr)
			cancel->Cancel();
		if (gTaskRunning)
			gTask->Wait();
		if (!IsTaskNotRunning(aTask))
			aTask->Wait();
		Abort();
		SaveSettings();
	}
	System::Void GeneratorForm::Abort() {
		queueAbort = false;
		// Cancel FractalGenerator threads
		generator->RequestCancel();
		/*foreach (var c in MyControls)
			c.Enabled = true;
		CutSelectEnabled(generator.GetFractal().cutFunction);
		FillCutParams();*/
		gifButton->Enabled = false;
		//aTask = null;
		currentBitmapIndex = 0;
	}
	System::Void GeneratorForm::QueueReset(bool allow) {
		if (modifySettings || !allow)
			return;
		if (queueReset <= 0) {
			gifButton->Enabled = false;
			currentBitmapIndex = 0;
			//if (gTask == nullptr && aTask == nullptr)
			if (IsTaskNotRunning(gTask) && IsTaskNotRunning(aTask))
				aTask = Task::Run(gcnew Action(this, &GeneratorForm::Abort), (cancel = gcnew CancellationTokenSource())->Token);
			else queueAbort = true;
		}
		ResetRestart();
		queueReset = abortDelay;
		restartButton->Enabled = false;
	}
	System::Void GeneratorForm::ResetRestart() {
		queueReset = restartTimer = 0;
		restartButton->Text = "! RESTART !";
	}

	template <typename T> T GeneratorForm::Clamp(T NEW, T MIN, T MAX) { return Math::Max(MIN, Math::Min(MAX, NEW)); }

	/*template <typename T> T GeneratorForm::Parse(System::Windows::Forms::TextBox^ BOX) { T v = 0; return T::TryParse(BOX->Text, v) ? v : (T)0; }
	template <typename T> T GeneratorForm::Retext(System::Windows::Forms::TextBox^ BOX, T NEW) { BOX->Text = NEW == 0 ? "" : NEW.ToString(); return NEW; }
	template <typename T> T GeneratorForm::Mod(T NEW, T MIN, T MAX) { const auto D = MAX - MIN; while (NEW < MIN) NEW += D; while (NEW > MAX) NEW -= D; return NEW; }
	template <typename T> bool GeneratorForm::Diff(T NEW, T GEN) { return GEN == NEW; }
	template <typename T> bool GeneratorForm::Apply(T NEW, interior_ptr<T> GEN) { *GEN = NEW; QueueReset(true); return false; }
	template <typename T> T GeneratorForm::ParseClampRetext(System::Windows::Forms::TextBox^ BOX, T MIN, T MAX) { return Retext(BOX, Clamp(Parse(BOX), MIN, MAX)); }
	template <typename T> bool GeneratorForm::DiffApply(T NEW, interior_ptr<T> GEN) { return Diff(NEW, *GEN) || Apply(NEW, GEN); }
	template <typename T> bool GeneratorForm::ClampDiffApply(T NEW,  interior_ptr<T> GEN, T MIN, T MAX) { return DiffApply(Clamp(NEW, MIN, MAX), GEN); }
	template <typename T> bool GeneratorForm::ParseDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<T> GEN) { return DiffApply(Parse(BOX), GEN); }
	template <typename T> bool GeneratorForm::ParseModDiffApply(System::Windows::Forms::TextBox^ BOX,  interior_ptr<T> GEN, T MIN, T MAX) { return DiffApply(Mod(Parse(BOX), MIN, MAX), GEN); }
	template <typename T> bool GeneratorForm::ParseClampRetextDiffApply(System::Windows::Forms::TextBox^ BOX,  interior_ptr<T> GEN, T MIN, T MAX) { return DiffApply(ParseClampRetext(BOX, MIN, MAX), GEN); }
	template <typename T, typename F> bool GeneratorForm::ParseClampRetextMulDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<F> GEN, T MIN, T MAX, F MUL) { return  DiffApply((T)(ParseClampRetext(BOX, MIN, MAX) * MUL), GEN); }*/

	int16_t GeneratorForm::Parse16(System::Windows::Forms::TextBox^ BOX) { int16_t v = 0; return int16_t::TryParse(BOX->Text, v) ? v : static_cast<int16_t>(0); }
	int32_t GeneratorForm::Parse32(System::Windows::Forms::TextBox^ BOX) { int32_t v = 0; return int32_t::TryParse(BOX->Text, v) ? v : static_cast<int32_t>(0); }
	float GeneratorForm::ParseF(System::Windows::Forms::TextBox^ BOX) { float v = 0.0f; return float::TryParse(BOX->Text, v) ? v : 0.0f; }
	//int16_t GeneratorForm::Clamp(int16_t NEW, int16_t MIN, int16_t MAX) { return Math::Max(MIN, Math::Min(MAX, NEW)); }
	int16_t GeneratorForm::Retext(System::Windows::Forms::TextBox^ BOX, int16_t NEW) { int16_t t; BOX->Text = NEW == 0 ? (int16_t::TryParse(BOX->Text, t) ? "" : BOX->Text) : NEW.ToString(); return NEW; }
	int16_t GeneratorForm::Mod(int16_t NEW, int16_t MIN, int16_t MAX) { const auto D = MAX - MIN; while (NEW < MIN) NEW += D; while (NEW > MAX) NEW -= D; return NEW; }
	bool GeneratorForm::Diff(int16_t NEW, int16_t GEN) { return GEN == NEW; }
	bool GeneratorForm::Diff(int32_t NEW, int32_t GEN) { return GEN == NEW; }
	bool GeneratorForm::Diff(float NEW, float GEN) { return GEN == NEW; }
	bool GeneratorForm::Apply(int16_t NEW, interior_ptr<int16_t> GEN) { *GEN = NEW; QueueReset(true); return false; }
	bool GeneratorForm::Apply(int32_t NEW, interior_ptr<int32_t> GEN) { *GEN = NEW; QueueReset(true); return false; }
	bool GeneratorForm::Apply(float NEW, interior_ptr<float> GEN) { *GEN = NEW; QueueReset(true); return false; }
	int16_t GeneratorForm::ParseClampRetext(System::Windows::Forms::TextBox^ BOX, int16_t MIN, int16_t MAX) { return Retext(BOX, Clamp(Parse16(BOX), MIN, MAX)); }
	int32_t GeneratorForm::ParseClampRetext(System::Windows::Forms::TextBox^ BOX, int32_t MIN, int32_t MAX) { return Retext(BOX, Clamp(Parse32(BOX), MIN, MAX)); }
	bool GeneratorForm::DiffApply(int16_t NEW, interior_ptr<int16_t> GEN) { return Diff(NEW, *GEN) || Apply(NEW, GEN); }
	bool GeneratorForm::DiffApply(int32_t NEW, interior_ptr<int32_t> GEN) { return Diff(NEW, *GEN) || Apply(NEW, GEN); }
	bool GeneratorForm::DiffApply(float NEW, interior_ptr<float> GEN) { return Diff(NEW, *GEN) || Apply(NEW, GEN); }
	bool GeneratorForm::ClampDiffApply(int16_t NEW, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX) { return DiffApply(Clamp(NEW, MIN, MAX), GEN); }
	bool GeneratorForm::ParseDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN) { return DiffApply(Parse16(BOX), GEN); }
	bool GeneratorForm::ParseDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<float> GEN) { return DiffApply(ParseF(BOX), GEN); }
	bool GeneratorForm::ParseModDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX) { return DiffApply(Mod(Parse16(BOX), MIN, MAX), GEN); }
	bool GeneratorForm::ParseClampRetextDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX) { return DiffApply(ParseClampRetext(BOX, MIN, MAX), GEN); }
	bool GeneratorForm::ParseClampRetextDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int32_t> GEN, int32_t MIN, int32_t MAX) { return DiffApply(ParseClampRetext(BOX, MIN, MAX), GEN); }
	bool GeneratorForm::ParseClampRetextMulDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<int16_t> GEN, int16_t MIN, int16_t MAX, int16_t MUL) { return DiffApply(ParseClampRetext(BOX, MIN, MAX) * MUL, GEN); }
	bool GeneratorForm::ParseClampRetextMulDiffApply(System::Windows::Forms::TextBox^ BOX, interior_ptr<float> GEN, int16_t MIN, int16_t MAX, float MUL) { return DiffApply(ParseClampRetext(BOX, MIN, MAX) * MUL, GEN); }

	System::Void GeneratorForm::FractalSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectFractal(Math::Max(0, fractalSelect->SelectedIndex)))
			return;
		// Fractal is different - load it, change the setting and restart generation
		// Fill the fractal's adjuistable definition combos
		// Fill the fractal's adjuistable cutfunction seed combos, and restart generation

		FillEditor();
		SetupSelects();
	}
	System::Void GeneratorForm::SetupSelects() {
		if (!modifySettings) {
			modifySettings = true;
			FillSelects();
			FillCutParams();
			cutparamBox->Text = "0";
			modifySettings = false;
			QueueReset(true);
		} else {
			FillSelects();
			FillCutParams();
		}
	}
	System::Void GeneratorForm::SetupFractal() {
		generator->SetupFractal();
		if (!modifySettings) {
			modifySettings = true;
			Parallel_Changed(DN);
			DetailBox_TextChanged(DN);
			modifySettings = false;
		} else {
			Parallel_Changed(DN);
			DetailBox_TextChanged(DN);
		}
		generator->SetupAngle();
		generator->SetupCutFunction();
	}
	System::Void GeneratorForm::FillSelects() {
		auto f = generator->GetFractal();
		// Fill angle childred definitnions combobox
		angleSelect->Items->Clear();
		for(int i = 0; i < f->childAngle.size(); ++i)
			angleSelect->Items->Add(gcnew System::String(f->childAngle[i].first.c_str()));
		if(angleSelect->Items->Count > 0)
			angleSelect->SelectedIndex = 0;
		// Fill color children definitnions combobox
		colorSelect->Items->Clear();
		for (int i = 0; i < f->childColor.size(); ++i)
			colorSelect->Items->Add(gcnew System::String(f->childColor[i].first.c_str()));
		if (colorSelect->Items->Count > 0)
			colorSelect->SelectedIndex = 0;
		// Fill cutfunction definitnions combobox
		cutSelect->Items->Clear();
		auto& cf = f->cutFunction;
		if (CutSelectEnabled(cf)) {
			for (int i = 0; i < cf.size(); ++i)
				if(cf[i].first >= 0)
					cutSelect->Items->Add(gcnew System::String(Fractal::cutFunctions[cf[i].first].first.c_str()));
			if (cutSelect->Items->Count > 0)
				cutSelect->SelectedIndex = 0;
		}
	}
	System::Void GeneratorForm::AngleSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		DiffApply(static_cast<int16_t>(Math::Max(0, angleSelect->SelectedIndex)), &generator->SelectedChildAngle);
		SwitchChildAngle();
	}
	System::Void GeneratorForm::ColorSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		DiffApply(static_cast<int16_t>(Math::Max(0, colorSelect->SelectedIndex)), &generator->SelectesChildColor);
		SwitchChildColor();
	}
	System::Void GeneratorForm::CutSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if(!DiffApply(static_cast<int16_t>(Math::Max(0, cutSelect->SelectedIndex)), &generator->SelectedCut)) FillCutParams();
	}
	/*
#define DIFF_PARAM(NEW, GEN) if (generator->GEN == NEW) return;
#define APPLY_PARAM(NEW, GEN) generator->GEN = NEW; QueueReset(true);
#define APPLY_DIFF_PARAM(NEW, GEN) DIFF_PARAM(NEW, GEN) APPLY_PARAM(NEW, GEN)
#define CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX) TYPE NEW; if(!TYPE::TryParse(BOX->Text, NEW)) NEW = MIN; \
	BOX->Text = (NEW = Clamp(NEW, static_cast<TYPE>(MIN), static_cast<TYPE>(MAX))).ToString();
#define APPLY_CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX, GEN) CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX) APPLY_DIFF_PARAM(NEW, GEN)
#define APPLY_DCLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX, TYPE2, NEW2, MUL, GEN) CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX)\
	const TYPE2 NEW2 = static_cast<TYPE2>(NEW * MUL); APPLY_DIFF_PARAM(NEW2, GEN)
#define APPLY_MOD_PARAM(TYPE, BOX, NEW, MIN, MAX, GEN)TYPE NEW; if(!TYPE::TryParse(BOX->Text, NEW)) NEW = MIN; \
	while (NEW < MIN)NEW += (MAX-MIN); while (NEW >= MAX)NEW -= (MAX-MIN); APPLY_DIFF_PARAM(NEW, GEN)
	*/
	System::Void GeneratorForm::CutparamBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextDiffApply(cutparamBox, &generator->SelectedCutSeed, static_cast<int32_t>(-1), generator->GetMaxCutparam());
	}
	bool GeneratorForm::CutSelectEnabled(std::vector<std::pair<int32_t, std::vector<int32_t>>>& cf) {
		bool e = cf.size() > 0;
		cutSelect->Enabled = e;
		return e;
	}
	bool GeneratorForm::CutParamBoxEnabled(Fractal::CutFunction* cf) {
		bool e = 0 < (generator->CutSeed_Max = (int)(cf == nullptr || (*cf)(0, -1, *generator->GetFractal()) <= 0 ? 0 : ((*cf)(0, 1 - (1 << 30), *generator->GetFractal()) + 1) / (*cf)(0, -1, *generator->GetFractal())));
		cutparamBox->Enabled = e;
		return e;
	}
	System::Void GeneratorForm::FillCutParams() {
		CutParamBoxEnabled(generator->GetCutFunction());
		cutparamBox->Text = "0";
	}
	System::Void GeneratorForm::PeriodBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextDiffApply(periodBox, &generator->SelectedPeriod, static_cast<int16_t>(-1), static_cast<int16_t>(1000));
	}
	System::Void GeneratorForm::PeriodMultiplierBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextDiffApply(periodMultiplierBox, &generator->SelectedPeriodMultiplier, static_cast<int16_t>(1), static_cast<int16_t>(10));
	}
	System::Void GeneratorForm::ZoomSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		DiffApply(static_cast<int16_t>((zoomSelect->SelectedIndex + 1) % 3 - 1), &generator->SelectedZoom);
	}
	System::Void GeneratorForm::DefaultZoom_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseDiffApply(defaultZoom, &generator->SelectedDefaultZoom);
	}
	System::Void GeneratorForm::SpinSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		ClampDiffApply(static_cast<int16_t>(spinSelect->SelectedIndex - 2), &generator->SelectedSpin, static_cast<int16_t>(-2), static_cast<int16_t>(2));
	}
	System::Void GeneratorForm::SpinSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextDiffApply(spinSpeedBox, &generator->SelectedExtraSpin, static_cast<int16_t>(0), static_cast<int16_t>(255));
	}
	System::Void GeneratorForm::DefaultAngle_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseModDiffApply(defaultAngle, &generator->SelectedDefaultAngle, static_cast<int16_t>(0), static_cast<int16_t>(360));
	}
	System::Void GeneratorForm::HueSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		DiffApply(static_cast<int16_t>(hueSelect->SelectedIndex == 0 ? -1 : (hueSelect->SelectedIndex - 1) % 6), &generator->SelectedHue);
	}
	System::Void GeneratorForm::HueSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		const auto newSpeed = ParseClampRetext(hueSpeedBox, static_cast<int16_t>(0), static_cast<int16_t>(255));
		if (Diff(newSpeed, generator->SelectedExtraHue))
			return;
		// hue speed is different - change the setting and if it's actually huecycling restart generation
		if (generator->SelectedHue < 0 || generator->SelectedHue > 1)
			Apply(newSpeed, &generator->SelectedExtraHue);
		else generator->SelectedExtraHue = newSpeed;
	}
	System::Void GeneratorForm::DefaultHue_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseModDiffApply(defaultHue, &generator->SelectedDefaultHue, static_cast<int16_t>(0), static_cast<int16_t>(360));
	}
	System::Void GeneratorForm::AmbBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextMulDiffApply(ambBox, &generator->SelectedAmbient, static_cast<int16_t>(-1), static_cast<int16_t>(30), static_cast<int16_t>(4));
	}
	System::Void GeneratorForm::NoiseBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextMulDiffApply(noiseBox, &generator->SelectedNoise, static_cast<int16_t>(0), static_cast<int16_t>(30), .1f);
	}
	System::Void GeneratorForm::SaturateBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextMulDiffApply(saturateBox, &generator->SelectedSaturate, static_cast<int16_t>(0), static_cast<int16_t>(10), .1f);
	}
	System::Void GeneratorForm::DetailBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		if (!ParseClampRetextMulDiffApply(detailBox, &generator->SelectedDetail, static_cast<int16_t>(0), static_cast<int16_t>(10), .1f * generator->GetFractal()->minSize)) 
			generator->SetMaxIterations();
	}
	System::Void GeneratorForm::BloomBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextMulDiffApply(bloomBox, &generator->SelectedBloom, static_cast<int16_t>(0), static_cast<int16_t>(40), .25f);
	}
	System::Void GeneratorForm::BlurBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextDiffApply(blurBox, &generator->SelectedBlur, static_cast<int16_t>(0), static_cast<int16_t>(40));
	}
	System::Void GeneratorForm::BrightnessBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextDiffApply(brightnessBox, &generator->SelectedBrightness, static_cast<int16_t>(0), static_cast<int16_t>(300));
	}
	System::Void GeneratorForm::VoidBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		ParseClampRetextDiffApply(voidBox, &generator->SelectedVoid, static_cast<int16_t>(0), static_cast<int16_t>(300));
	}
	System::Void GeneratorForm::Parallel_Changed(System::Object^ sender, System::EventArgs^ e) {
		SetupParallel(ParseClampRetext(threadsBox, static_cast<int16_t>(MINTASKS), static_cast<int16_t>(maxTasks)));
	}
	System::Void GeneratorForm::ParallelTypeSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if ((ParallelType)parallelTypeSelect->SelectedIndex == ParallelType::OfDepth) {
			MessageBox::Show(
				"Warning: this parallelism mode might be fast at rendering a single image, but it messes up few pixels.\nSo if you want highest quality the OfAnimation is recommended.",
				"Warning",
				MessageBoxButtons::OK,
				MessageBoxIcon::Warning);
		}
		generator->SelectedParallelType = (ParallelType)parallelTypeSelect->SelectedIndex;
	}
	System::Void GeneratorForm::AbortBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		abortDelay = ParseClampRetext(abortBox, static_cast<int16_t>(0), static_cast<int16_t>(10000));
	}
	System::Void GeneratorForm::DelayBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		auto newDelay = ParseClampRetext(delayBox, static_cast<int16_t>(1), static_cast<int16_t>(500));
		if (generator->SelectedDelay == newDelay)
			return;
		// Delay is diffenret, change it, and restart the generation if ou were encoding a gif
		generator->SelectedDelay = newDelay;
		if (100 / generator->SelectedFps != generator->SelectedDelay)
			generator->SelectedFps = (short)(100 / generator->SelectedDelay);
		timer->Interval = generator->SelectedDelay * 10;
		if (generator->SelectedGenerationType >= GenerationType::EncodeGIF && generator->SelectedGenerationType <= GenerationType::AllSeeds)
			generator->restartGif = true;
	}
	System::Void GeneratorForm::FpsBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		auto newFps = ParseClampRetext(fpsBox, static_cast<int16_t>(1), static_cast<int16_t>(120));
		if (generator->SelectedFps == newFps)
			return;
		generator->SelectedFps = newFps;
		delayBox->Text = (100 / newFps).ToString();
		timer->Interval = 1000 / generator->SelectedFps;
	}
	System::Void GeneratorForm::MoveFrame(int16_t move) {
		animated = false; const auto b = generator->GetBitmapsFinished(); currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + b + move) % b;
	}
	System::Void GeneratorForm::PrevButton_Click(System::Object^ sender, System::EventArgs^ e) { MoveFrame(-1); }
	System::Void GeneratorForm::AnimateButton_Click(System::Object^ sender, System::EventArgs^ e) {
		animated = !animated;
		SetAnimate();
	}
	System::Void GeneratorForm::SetAnimate() { 
		animateButton->Text = animated ? "Playing" : "Paused";
		animateButton->BackColor = animated ? Color::FromArgb(128, 255, 128) : Color::FromArgb(255, 128, 128); 
	
	}
	System::Void GeneratorForm::NextButton_Click(System::Object^ sender, System::EventArgs^ e) { MoveFrame(1); }
	System::Void GeneratorForm::RestartButton_Click(System::Object^ sender, System::EventArgs^ e) {
		if (restartButton->Text == "! RESTART !") {
			restartButton->Text = "ARE YOU SURE?";
			restartTimer = 2000;
			return;
		}
		restartTimer = 0;
		restartButton->Text = "! RESTART !";
		restartButton->Enabled = false;
		QueueReset(true);
	}
	System::Void GeneratorForm::EncodeSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if ((GenerationType)encodeSelect->SelectedIndex == GenerationType::Mp4) {
			encodeSelect->SelectedIndex = 2;
			Error("Sorry but direct Mp4 encoding is currently broken and unavailable, try again in a later release.\nFor now you can use Local GIF and then press the Save Mp4 button instead.",
					  "Unavailable");
			return;
		}
		if ((GenerationType)encodeSelect->SelectedIndex == GenerationType::HashSeeds) {
			MessageBox::Show(
				"This mode is not really meant for the end user, it only generates all parameters and export a hash.txt file will all the unique ones.\nIf you actually want an animation of all seeds, AllParam is recommended instead as that doesn't waste resources doing the hashing and encodes the animation for export.",
				"Warning",
				MessageBoxButtons::OK,
				MessageBoxIcon::Warning);
		}
		auto prev = generator->SelectedGenerationType;
		auto now = generator->SelectedGenerationType = (GenerationType)Math::Max(0, encodeSelect->SelectedIndex);
		if ((now >= GenerationType::OnlyImage && now <= GenerationType::Mp4) != (prev >= GenerationType::OnlyImage && prev <= GenerationType::Mp4))
			QueueReset(true);
	}
	System::Void GeneratorForm::HelpButton_Click(System::Object^ sender, System::EventArgs^ e) {
		helpPanel->Visible = screenPanel->Visible;
		screenPanel->Visible = !screenPanel->Visible;
	}
	System::Void GeneratorForm::DebugBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e) {
		if (!(generator->debugmode = debugBox->Checked)) 
			debugLabel->Text = "";
	}
#pragma endregion

#pragma region Output
	System::Void GeneratorForm::ScreenPanel_Paint(System::Object^ sender, System::Windows::Forms::PaintEventArgs^ e) {
		if (currentBitmap == nullptr)
			return;
		// Faster rendering with crisp pixels
		e->Graphics->InterpolationMode = System::Drawing::Drawing2D::InterpolationMode::NearestNeighbor;
		// some safety code to ensure no crashes
		uint8_t tryAttempt = 0;
		while (tryAttempt < 5) {
			try {
				e->Graphics->DrawImage(currentBitmap, System::Drawing::Rectangle(0, 0, screenPanel->Width, screenPanel->Height));
				tryAttempt = 5;
			} catch (Exception^) {
				++tryAttempt;
				Thread::Sleep(100);
			}
		}
	}
	System::Void GeneratorForm::PngButton_Click(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectedParallelType == ParallelType::OfDepth) {
			auto result = MessageBox::Show(
				"Warning: You have used OfDepth parallelism type. This can mess up some of the pixels, if you want to export, I highly recommend using OfAnimation instead.\nSwitch to OfAnimation to regenerate in high quality?",
				"Warning",
				MessageBoxButtons::YesNo,
				MessageBoxIcon::Warning);
			if (result == System::Windows::Forms::DialogResult::Yes) {
				parallelTypeSelect->SelectedIndex = 0;
				return;
			}
		}
		auto b = generator->GetBitmapsFinished();
		if (b < 1) {
			Error("This is only a low resolution preview image, please wait until the full resolution you have selected if finished.",
				  "Please wait");
			return;
		}
		// Make sure the bitmap is actually loaded
		UpdateBitmap(bitmap[generator->GetPreviewFrames() + (currentBitmapIndex %= b)]);
		savePng->ShowDialog();
	}
	System::Void GeneratorForm::GifButton_Click(System::Object^ sender, System::EventArgs^ e) {
		SaveVideo();
	}
	System::Windows::Forms::DialogResult GeneratorForm::SaveVideo() {
		if (generator->SelectedParallelType == ParallelType::OfDepth) {
			System::Windows::Forms::DialogResult result = MessageBox::Show(
				"Warning: You have used OfDepth parallelism type. This can mess up some of the pixels, if you want to export, I highly recommend using OfAnimation instead.\nSwitch to OfAnimation to regenerate in high quality?",
				"Warning",
				MessageBoxButtons::YesNo,
				MessageBoxIcon::Warning);
			if (result == System::Windows::Forms::DialogResult::Yes) {
				parallelTypeSelect->SelectedIndex = 0;
				return System::Windows::Forms::DialogResult::Cancel;
			}
		}
		return generator->SelectedGenerationType == GenerationType::Mp4 ? saveMp4->ShowDialog() : saveGif->ShowDialog();
	}
	System::Void GeneratorForm::Mp4Button_Click(System::Object^ sender, System::EventArgs^ e) {
		auto ffmpegPath = Path::Combine(AppDomain::CurrentDomain->BaseDirectory, "ffmpeg.exe");
		if (!File::Exists(ffmpegPath)) {
			Error("ffmpeg.exe not found. Are you sure you have downloaded the full release with ffmpeg included?",
					  "Unavailable");
			return;
		}
		if (!IsTaskNotRunning(gTask)) {
			Error("GIF or Mp4 is still saving, either wait until it's finished, or click this button before saving the GIF.",
					  "Unavailable");
			return;
		}
		if (gifButton->Enabled) {
			convertMp4->ShowDialog();
			return;
		}
		if (gifPath != "") {
			convertMp4->ShowDialog();
			return;
		}
	}
	System::Void GeneratorForm::SavePng_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e) {
		Stream^ myStream;
		if ((myStream = savePng->OpenFile()) != nullptr) {
			currentBitmap->Save(myStream, System::Drawing::Imaging::ImageFormat::Png);
			myStream->Close();
		}
	}
	System::Void GeneratorForm::SaveVideo_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e) {
		gifButton->Enabled = false;
		if (!IsTaskNotRunning(gTask)) {
			cancel->Cancel();
			return;
		}
		gifPath = ((SaveFileDialog^)sender)->FileName;
		// Gif Export Task
		gTask = Task::Run(gcnew Action(this, &GeneratorForm::ExportGif), (cancel = gcnew CancellationTokenSource())->Token);
	}
	System::Void GeneratorForm::ConvertMp4_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e) {
		auto ffmpegPath = Path::Combine(AppDomain::CurrentDomain->BaseDirectory, "ffmpeg.exe");
		if (!File::Exists(ffmpegPath)) {
			Error("ffmpeg.exe not found. Are you sure you have downloaded the full release with ffmpeg included?",
					  "Unavailable");
			return;
		}
		if (!IsTaskNotRunning(gTask)) {
			Error("GIF or Mp4 is still saving, either wait until it's finished, or click this button before saving the GIF.",
					  "Unavailable");
			encodeSelect->SelectedIndex = 2;
			return;
		}
		if (gifButton->Enabled) {
			gifButton->Enabled = false;
			gifPath = generator->GetTempGif();
			mp4Path = ((SaveFileDialog^)sender)->FileName;
			gTask = Task::Run(gcnew Action(this, &GeneratorForm::ExportMp4), (cancel = gcnew CancellationTokenSource())->Token);
			return;
		}
		if (gifPath != "") {
			mp4Path = ((SaveFileDialog^)sender)->FileName;
			gTask = Task::Run(gcnew Action(this, &GeneratorForm::ExportMp4), (cancel = gcnew CancellationTokenSource())->Token);
			return;
		}
	}
	System::Void GeneratorForm::ExportGif() {
		generator->SaveGif();
		while (!cancel->Token.IsCancellationRequested) {
			try {
#ifdef CLR
				System::String^ temp = generator->GetTempGif();
#else
				System::String^ temp = gcnew String(generator->GetTempGif().c_str());
#endif
				File::Move(temp, gifPath);
			} catch (IOException^ ex) {
				Console::WriteLine("An error occurred: {0}", ex->Message);
				System::Threading::Thread::Sleep(1000);
				continue;
			} catch (UnauthorizedAccessException^ ex) {
				Console::WriteLine("Access denied: {0}", ex->Message);
				System::Threading::Thread::Sleep(1000);
				continue;
			} catch (Exception^ ex) {
				Console::WriteLine("Unexpected error: {0}", ex->Message);
				System::Threading::Thread::Sleep(1000);
				continue;
			}
			break;
		}
	}
	System::Void GeneratorForm::ExportMp4() {
		isGifReady = 0;
		generator->SaveGif();
		try {
			File::Delete(mp4Path);
			auto ffmpegPath = Path::Combine(AppDomain::CurrentDomain->BaseDirectory, "ffmpeg.exe");
			if (!File::Exists(ffmpegPath))
				return;
			double gifFps = 1000.0 / (10 * generator->SelectedDelay); // Convert to frames per second
			auto arguments = System::String::Format(
				"-y -i \"{0}\" -vf \"fps={1},setpts=PTS*({2}/{1})\" -c:v libx264 -crf 0 -preset veryslow \"{3}\"",
				gifPath, generator->SelectedFps, gifFps, mp4Path);
			Process^ ffmpeg = gcnew Process();
			ffmpeg->StartInfo->FileName = ffmpegPath;
			ffmpeg->StartInfo->Arguments = arguments;
			ffmpeg->StartInfo->UseShellExecute = false;
			ffmpeg->StartInfo->RedirectStandardError = true;
			ffmpeg->StartInfo->RedirectStandardOutput = true;
			ffmpeg->StartInfo->CreateNoWindow = true;
			ffmpeg->Start();
			ffmpeg->BeginErrorReadLine();
			ffmpeg->WaitForExit();
		} catch (IOException^ ex) {
			//System::String^ exs = "SaveGif: An error occurred: " + ex->Message;
			return;
		} catch (UnauthorizedAccessException^ ex) {
			//System::String^ exs = "SaveGif: Access denied: " + ex->Message;
			return;
		} catch (Exception^ ex) {
			//System::String^ exs = "SaveGif: Unexpected error: " + ex->Message;
			return;
		}
		gifPath = "";
		mp4Path = "";
	}
#pragma endregion

#pragma region Editor
	System::Void GeneratorForm::FillEditor() {
		pointTabIndex = controlTabIndex;
		pointPanel->SuspendLayout();
		UnfillEditor();

		auto& fs = generator->GetFractals();


		auto f = generator->GetFractal();
		for (int i = 0; i < f->childCount; ++i)
			AddEditorPoint(f->childX, f->childY, f->childAngle[generator->SelectedChildAngle].second, f->childColor[generator->SelectesChildColor].second, false);
		
		bool e = f->edit;
		sizeBox->Text = f->childSize.ToString();
		cutBox->Text = f->cutSize.ToString();
		minBox->Text = f->minSize.ToString();
		maxBox->Text = f->maxSize.ToString();
		f->edit = e;
		pointPanel->ResumeLayout(false);
		pointPanel->PerformLayout();
	}
	System::Void GeneratorForm::UnfillEditor() {
		for each (auto x in editorSwitch) {
			pointPanel->Controls->Remove(x);
			MyControls->Remove(x);
		}
		for each (auto x in editorPointX) {
			pointPanel->Controls->Remove(x);
			MyControls->Remove(x);
			//x->Dispose();
		}
		for each (auto x in editorPointY) {
			pointPanel->Controls->Remove(x);
			MyControls->Remove(x);
			//x->Dispose();
		}
		for each (auto x in editorPointA) {
			pointPanel->Controls->Remove(x);
			MyControls->Remove(x);
			//x->Dispose();
		}
		for each (auto x in editorPointC) {
			pointPanel->Controls->Remove(x);
			MyControls->Remove(x);
		}
		for each (auto x in editorPointD) {
			pointPanel->Controls->Remove(x);
			MyControls->Remove(x);
		}
		editorSwitch->Clear();
		editorPointX->Clear();
		editorPointY->Clear();
		editorPointA->Clear();
		editorPointC->Clear();
		editorPointD->Clear();
		pointTabIndex = controlTabIndex;
	}
	System::Void GeneratorForm::SwitchChildAngle() {
		auto f = generator->GetFractal();
		auto e = f->edit;
		for (int i = 0; i < f->childCount; ++i)
			editorPointA[i]->Text = f->childAngle[generator->SelectedChildAngle].second[i].ToString();
		f->edit = e;
	}
	System::Void GeneratorForm::SwitchChildColor() {
		auto f = generator->GetFractal();
		auto e = f->edit;
		for (int i = 0; i < f->childCount; ++i) {
			switch (f->childColor[generator->SelectesChildColor].second[i]) {
			case 0: editorPointC[i]->BackColor = Color::Red; break;
			case 1: editorPointC[i]->BackColor = Color::Green; break;
			default: editorPointC[i]->BackColor = Color::Blue; break;
			}
		}
		f->edit = e;
	}
	System::Void GeneratorForm::AddEditorPoint(float* cx, float* cy, float* ca, uint8_t* cc, bool single) {
		int i = editorPointX->Count;
		System::Windows::Forms::TextBox ^x, ^y, ^a;
		System::Windows::Forms::Button ^c, ^d;
		editorPointX->Add(x = gcnew System::Windows::Forms::TextBox());
		editorPointY->Add(y = gcnew System::Windows::Forms::TextBox());
		editorPointA->Add(a = gcnew System::Windows::Forms::TextBox());
		editorPointC->Add(c = gcnew System::Windows::Forms::Button());
		editorPointD->Add(d = gcnew System::Windows::Forms::Button());
		x->Text = cx[i].ToString();
		y->Text = cy[i].ToString();
		a->Text = ca[i].ToString();
		c->Text = "";
		d->Text = "X";
		switch(cc[i])  {
			case 0: c->BackColor = Color::Red; break;
			case 1: c->BackColor = Color::Green; break;
			default: c->BackColor = Color::Blue; break;
		}
		BindPoint(x, y, a, c, d, i, single);
	}

	System::Void GeneratorForm::OnS_Click(System::Object^ sender, System::EventArgs^ e) {
		int i;
		int::TryParse(((Button^)sender)->Name, i);
		int i1 = i - 1;
		auto f = generator->GetFractal();
		auto ni = f->childCount;
		auto cx = f->childX;
		auto cy = f->childY;
		auto ca = f->childAngle[generator->SelectedChildAngle].second;
		auto cc = f->childColor[generator->SelectesChildColor].second;
		auto j = cx[i];
		cx[i] = cx[i1];
		cx[i1] = j;
		j = cy[i];
		cy[i] = cy[i1];
		cy[i1] = j;
		j = ca[i];
		ca[i] = ca[i1];
		ca[i1] = j;
		auto jc = cc[i];
		cc[i] = cc[i1];
		cc[i1] = jc;
		auto xi = editorPointX[i];
		auto yi = editorPointY[i];
		auto ai = editorPointA[i];
		auto ci = editorPointC[i];
		xi->Text = cx[i].ToString();
		yi->Text = cy[i].ToString();
		ai->Text = ca[i].ToString();
		switch (cc[i]) {
		case 0: ci->BackColor = Color::Red; break;
		case 1: ci->BackColor = Color::Green; break;
		default: ci->BackColor = Color::Blue; break;
		}
		auto xi1 = editorPointX[i1];
		auto yi1 = editorPointY[i1];
		auto ai1 = editorPointA[i1];
		auto ci1 = editorPointC[i1];

		xi1->Text = cx[i1].ToString();
		yi1->Text = cy[i1].ToString();
		ai1->Text = ca[i1].ToString();
		switch (cc[i1]) {
		case 0: ci1->BackColor = Color::Red; break;
		case 1: ci1->BackColor = Color::Green; break;
		default: ci1->BackColor = Color::Blue; break;
		}
		QueueReset(f->edit = true);
	}
	System::Void GeneratorForm::OnX_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		int i;
		int::TryParse(((Button^)sender)->Name, i);
		if (ParseDiffApply((System::Windows::Forms::TextBox^)sender, &generator->GetFractal()->childX[i])) return;
		generator->GetFractal()->edit = true;
	}
	System::Void GeneratorForm::OnY_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		int i;
		int::TryParse(((Button^)sender)->Name, i);
		if (ParseDiffApply((System::Windows::Forms::TextBox^)sender, &generator->GetFractal()->childY[i])) return;
		generator->GetFractal()->edit = true;
	}
	System::Void GeneratorForm::OnA_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		int i;
		int::TryParse(((Button^)sender)->Name, i);
		if (ParseDiffApply((System::Windows::Forms::TextBox^)sender, &generator->GetFractal()->childAngle[generator->SelectedChildAngle].second[i])) return;
		generator->GetFractal()->edit = true;
	}
	System::Void GeneratorForm::OnC_Click(System::Object^ sender, System::EventArgs^ e) {
		int i;
		auto c = ((Button^)sender);
		int::TryParse(c->Name, i);
		uint8_t color = generator->GetFractal()->childColor[generator->SelectesChildColor].second[i];
		generator->GetFractal()->childColor[generator->SelectesChildColor].second[i] = color = static_cast<uint8_t>((color + 1) % 3);
		switch (color) {
		case 0: c->BackColor = Color::Red; break;
		case 1: c->BackColor = Color::Green; break;
		default: c->BackColor = Color::Blue; break;
		}
		QueueReset(generator->GetFractal()->edit = true);
	}
	System::Void GeneratorForm::OnD_Click(System::Object^ sender, System::EventArgs^ e) {
		int i;
		auto d = ((Button^)sender);
		int::TryParse(d->Name, i);
		auto f = generator->GetFractal();
		auto ni = --f->childCount;
		auto nx = new float[ni];
		auto ny = new float[ni];
		auto na = new float[ni];
		auto nc = new uint8_t[ni];
		auto cx = f->childX;
		auto cy = f->childY;
		auto ca2 = f->childAngle[generator->SelectedChildAngle];
		auto ca = ca2.second;
		auto cc2 = f->childColor[generator->SelectesChildColor];
		auto cc = cc2.second;

		pointPanel->SuspendLayout();
		UnfillEditor();
		pointPanel->ResumeLayout(false);
		for (int ci = 0; ci < i; ++ci) {
			nx[ci] = cx[ci];
			ny[ci] = cy[ci];
			na[ci] = ca[ci];
			nc[ci] = cc[ci];
			AddEditorPoint(nx, ny, na, nc, false);
		}
		for (int ci = i; ci < ni;) {
			auto xp = ci + 1;
			nx[ci] = cx[xp];
			ny[ci] = cy[xp];
			na[ci] = ca[xp];
			nc[ci] = cc[xp];
			ci = xp;
			AddEditorPoint(nx, ny, na, nc, false);
		}
		delete[] cx;
		delete[] cy;
		delete[] ca;
		delete[] cc;

		f->childX = nx;
		f->childY = ny;
		f->childAngle[generator->SelectedChildAngle] = { ca2.first, na };
		f->childColor[generator->SelectesChildColor] = { cc2.first, nc };
		QueueReset(f->edit = true);
	}

	System::Void GeneratorForm::BindPoint(
		System::Windows::Forms::TextBox^ x,
		System::Windows::Forms::TextBox^ y,
		System::Windows::Forms::TextBox^ a,
		System::Windows::Forms::Button^ c,
		System::Windows::Forms::Button^ d,
		int i, bool single
	) {
		auto font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
												 static_cast<System::Byte>(238)));;
		int textsize = 53;
		int butsize = 23;
		if (single)
			pointPanel->SuspendLayout();
		if (i > 1) {
			System::Windows::Forms::Button^ s;
			editorSwitch->Add(s = gcnew System::Windows::Forms::Button());
			s->Text = "⇕";
			s->Font = font;
			s->Location = System::Drawing::Point(10 + 3 * textsize + 2 * butsize, 10 + (i * 2 - 1) * butsize / 2);
			s->Margin = System::Windows::Forms::Padding(4, 3, 4, 3);
			s->Name = i.ToString();
			s->Size = System::Drawing::Size(butsize, butsize);
			//s->TabIndex = ++pointTabIndex;
			pointPanel->Controls->Add(s);
			MyControls->Add(s);
			SetupEditControl(s, "Switch these two points in the list-> (Only has effects on cutfunctions, or if you switch the main first one->)");
			s->Click += gcnew System::EventHandler(this, &GeneratorForm::OnS_Click);
		}
		pointPanel->Controls->Add(x);
		pointPanel->Controls->Add(y);
		pointPanel->Controls->Add(a);
		pointPanel->Controls->Add(c);
		pointPanel->Controls->Add(d);
		MyControls->Add(x);
		SetupEditControl(x, "X coordinate of the child->");
		MyControls->Add(y);
		SetupEditControl(y, "Y coordinate of the child->");
		MyControls->Add(a);
		SetupEditControl(a, "Angle of the child->");
		MyControls->Add(c);
		SetupEditControl(c, "Color shift of the child (red = no shift, green = shift forward RGB->GBR, blue = shift backwards RGB->BRG)");
		MyControls->Add(d);
		SetupEditControl(d, "Remove this point->");
		if (single) {
			pointPanel->ResumeLayout(false);
			pointPanel->PerformLayout();
		}
		x->Location = System::Drawing::Point(10, 10 + i * butsize);
		x->Margin = System::Windows::Forms::Padding(4, 3, 4, 3);
		x->Name = i.ToString();
		x->Font = font;
		x->Size = System::Drawing::Size(textsize, butsize);
		//x->TabIndex = ++pointTabIndex;
		x->Enabled = i > 0;
		if (x->Enabled)
			x->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::OnX_TextChanged);

		y->Location = System::Drawing::Point(10 + textsize, 10 + i * butsize);
		y->Margin = System::Windows::Forms::Padding(4, 3, 4, 3);
		y->Name = i.ToString();
		y->Font = font;
		y->Size = System::Drawing::Size(textsize, butsize);
		//y->TabIndex = ++pointTabIndex;
		y->Enabled = i > 0;
		if (y->Enabled)
			y->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::OnY_TextChanged);

		a->Location = System::Drawing::Point(10 + 2 * textsize, 10 + i * butsize);
		a->Margin = System::Windows::Forms::Padding(4, 3, 4, 3);
		a->Name = i.ToString();
		a->Font = font;
		a->Size = System::Drawing::Size(textsize, butsize);
		//a->TabIndex = ++pointTabIndex;
		a->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::OnA_TextChanged);

		c->Location = System::Drawing::Point(10 + 3 * textsize, 10 + i * butsize);
		c->Margin = System::Windows::Forms::Padding(4, 3, 4, 3);
		c->Name = i.ToString();
		c->Size = System::Drawing::Size(butsize, butsize);
		//c->TabIndex = ++pointTabIndex;
		c->Click += gcnew System::EventHandler(this, &GeneratorForm::OnC_Click);

		d->Location = System::Drawing::Point(10 + 3 * textsize + butsize, 10 + i * butsize);
		d->Margin = System::Windows::Forms::Padding(4, 3, 4, 3);
		d->Name = i.ToString();
		d->Font = font;
		d->Size = System::Drawing::Size(23, 23);
		//d->TabIndex = ++pointTabIndex;
		d->Enabled = i > 0;
		if (d->Enabled)
			d->Click += gcnew System::EventHandler(this, &GeneratorForm::OnD_Click);
			
		addPoint->Location = System::Drawing::Point(10, 10 + (i + 1) * butsize);
	}
	System::Void GeneratorForm::SaveFractal_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e) {
		auto f = tosave;
		auto fracname = ((SaveFileDialog^)sender)->FileName;
		auto path = fracname;
		f->path = Fractal::ConvertToStdString(fracname);
		int index;
		while ((index = fracname->IndexOf('/')) >= 0)
			fracname = fracname->Substring(index + 1);
		while ((index = fracname->IndexOf('\\')) >= 0)
			fracname = fracname->Substring(index + 1);
		fracname = fracname->Replace('|', '.')->Replace(':', '.')->Replace(';', '.');
		f->name = Fractal::ConvertToStdString(fracname);
		// Consts
		System::String^ p, ^a, ^s = fracname + "|" + f->childCount + "|" + f->childSize + "|" + f->maxSize + "|" + f->minSize + "|" + f->cutSize;
		// XY
		p = "|";
		for(int i = 0; i < f->childCount; ++i)
			p += f->childX[i] + ";";
		s += p->Substring(0, p->Length - 1);
		p = "|";
		for (int i = 0; i < f->childCount; ++i)
			p += f->childY[i] + ";";
		s += p->Substring(0, p->Length - 1);
		// Angles
		p = "|";
		for (int i = 0; i < f->childAngle.size(); ++i) {
			a = gcnew System::String(f->childAngle[i].first.c_str()) + ":";
			for (int j = 0; j < f->childCount; ++j)
				a += f->childAngle[i].second[j] + ":";
			p += a->Substring(0, p->Length - 1) + ";";
		}
		s += p->Substring(0, p->Length - 1);
		// Colors
		p = "|";
		for (int i = 0; i < f->childColor.size(); ++i) {
			a = gcnew System::String(f->childColor[i].first.c_str()) + ":";
			for (int j = 0; j < f->childCount; ++j)
				a += f->childColor[i].second[j] + ":";
			p += a->Substring(0, p->Length - 1) + ";";
		}
		s += p->Substring(0, p->Length - 1);
		// Cuts
		p = "|";
		for (int i = 0; i < f->cutFunction.size(); ++i) {
			a = f->cutFunction[i].first + ":";
			for (int j = 0; j < f->cutFunction[i].second.size(); ++j)
				a += f->cutFunction[i].second[j] + ":";
			p += (a != "" ? a->Substring(0, p->Length - 1) : "") + ";";
		}
		s += p == "|" ? "|" : p->Substring(0, p->Length - 1);
		System::IO::File::WriteAllText(path, s);
		f->edit = false;
	}
	System::Void GeneratorForm::LoadFractal_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e) {
		auto filename = ((OpenFileDialog^)sender)->FileName;
		auto fracname = filename->Substring(0, filename->Length - 8);
		int i;
		while ((i = fracname->IndexOf('/')) >= 0)
			fracname = fracname->Substring(i + 1);
		while ((i = fracname->IndexOf('\\')) >= 0)
			fracname = fracname->Substring(i + 1);
		fracname = fracname->Replace('|', '.')->Replace(':', '.')->Replace(';', '.');
		if (fractalSelect->Items->Contains(fracname)) {
			Error("There already is a loaded fractal of the same name, selecting it->", "Already exists");
			fractalSelect->SelectedIndex = fractalSelect->Items->IndexOf(fracname);
		} else if (System::IO::File::Exists(filename))
			LoadFractal(filename, true);
	}
	bool GeneratorForm::LoadFractal(System::String^ file, bool select) {
		if (!System::IO::File::Exists(file))
			return Error("No such file", "Cannot load");
		auto content = System::IO::File::ReadAllText(file);
		auto arr = content->Split('|');
		// Name
		if (arr[0] == "")
			return Error("No fractal name", "Cannot load");
		if (fractalSelect->Items->Contains(fractalSelect->Text))
			return Error("Fractal with the same name already loaded in the list->", "Cannot load");
		// Consts
		uint8_t b;
		int count, intparse, cutindex;
		float x, size, maxsize, minsize, cutsize;
		if (!int::TryParse(arr[1], count) || count < 1)
			return Error("Invalid children count: " + count, "Cannot load");
		if (!float::TryParse(arr[2], size) || size <= 1)
			return Error("Invalid children size: " + size, "Cannot load");
		if (!float::TryParse(arr[3], maxsize))
			return Error("Invalid max size: " + maxsize, "Cannot load");
		if (!float::TryParse(arr[4], minsize))
			return Error("Invalid min size: " + minsize, "Cannot load");
		if (!float::TryParse(arr[5], cutsize))
			return Error("Invalid cut size: " + cutsize, "Cannot load");

		// ChildX
		auto s = arr[6]->Split(';');
		if (s->Length < count)
			return Error("Insufficent children X count: " + s->Length + " / " + count, "Cannot load");
		auto childX = new float[count];
		for (int i = count; --i >= 0; childX[i] = x)
			if (!float::TryParse(s[i], x))
				return Error("Invalid child X: " + s[i], "Cannot load");
		// ChildY
		s = arr[7]->Split(';');
		if (s->Length < count)
			return Error("Insufficient children Y count: " + s->Length + " / " + count, "Cannot load");
		auto childY = new float[count];
		for (int i = count; --i >= 0; childY[i] = x)
			if (!float::TryParse(s[i], x))
				return Error("Invalid child Y: " + s[i], "Cannot load");
		// ChildAngles
		s = arr[8]->Split(';');
		if (s->Length < 1)
			return Error("No set of child angles", "Cannot load");
		std::vector<std::pair<std::string, float*>> childAngle;
		childAngle.reserve(s->Length);
		for (int i = 0; i < s->Length; ++i) {
			auto c = s[i];
			auto angleSet = c->Split(':');
			if (angleSet[0] == "")
				return Error("Empty angle set name", "Cannot load");
			if (angleSet->Length < count + 1)
				return Error("Insufficient child angle count of " + angleSet[0] + ": " + (angleSet->Length - 1) + " / " + count, "Cannot load");
			auto angle = new float[count];
			for (int i = count; i > 0; angle[i] = x)
				if (!float::TryParse(angleSet[i--], x))
					return Error("Invalid angle in set" + angleSet[0] + ": " + angleSet[i + 1], "Cannot load");
			childAngle.push_back({ Fractal::ConvertToStdString(angleSet[0]), angle });
		}
		// ChildColors
		s = arr[9]->Split(';');
		if (s->Length < 1)
			return Error("No set of child colors", "Cannot load");
		std::vector<std::pair<std::string, uint8_t*>> childColor;
		childColor.reserve(s->Length);
		for (int i = 0; i < s->Length; ++i) {
			auto c = s[i];
			auto colorSet = c->Split(':');
			if (colorSet[0] == "")
				return Error("Empty color set name", "Cannot load");
			if (colorSet->Length < count + 1)
				return Error("Insufficient child color count of " + colorSet[0] + ": " + (colorSet->Length - 1) + " / " + count, "Cannot load");
			auto color = new uint8_t[count];
			for (int i = count; i > 0; color[i] = b)
				if (!uint8_t::TryParse(colorSet[i--], b))
					return Error("Invalid color in set " + colorSet[0] + ": " + colorSet[i + 1], "Cannot load");
			childColor.push_back({ Fractal::ConvertToStdString(colorSet[0]), color });
		}
		// Cuts
		s = arr[10]->Split(';');
		std::vector<std::pair<int32_t, std::vector<int32_t>>> cutFunction;
		cutFunction.reserve(s->Length);
		for (int i = 0; i < s->Length; ++i) {
			auto c = s[i];
			auto cutints = c->Split(':');
			if (!int::TryParse(cutints[0], cutindex))
				return Error("Invalid cutfuncion index: " + cutints[0], "Cannot load");
			std::vector<int32_t> cuthash;
			cuthash.reserve(cutints->Length - 1);
			for (int i = 1; i < cutints->Length; cuthash.push_back(intparse))
				if (!int::TryParse(cutints[i++], intparse))
					return Error("Invalid cutfuncion seed: " + cutints[i + 1], "Cannot load");
			cutFunction.push_back({ cutindex, cuthash });
		}
		Fractal* f = new Fractal(Fractal::ConvertToStdString(arr[0]), count, size, maxsize, minsize, cutsize, childX, childY, childAngle, childColor, cutFunction);
		f->path = Fractal::ConvertToStdString(file);
		generator->GetFractals().push_back(f);
		if (select)
			fractalSelect->SelectedIndex = fractalSelect->Items->Add(arr[0]);
		else
			fractalSelect->Items->Add(arr[0]);
		return true;
	}
	System::Void GeneratorForm::modeButton_Click(System::Object^ sender, System::EventArgs^ e) {
		if (editorPanel->Visible) {
			// SelectParallelMode();
			generator->SelectedBlur = mem_blur;
			generator->SelectedBloom = mem_bloom;
			generator->SelectedGenerationType = (GenerationType)mem_generate;
			generator->SelectedHue = mem_hue;
			generator->SelectedDefaultHue = mem_defaulthue;
			abortDelay = mem_abort;
			generator->SelectedPreviewMode = false;

		} else {
			mem_blur = generator->SelectedBlur;
			mem_bloom = generator->SelectedBloom;
			mem_generate = generator->SelectedGenerationType;
			mem_hue = generator->SelectedHue;
			mem_defaulthue = generator->SelectedDefaultHue;
			mem_abort = abortDelay;
			abortDelay = 10;
			generator->SelectedGenerationType = GenerationType::Animation;
			generator->SelectedBloom = generator->SelectedBlur = generator->SelectedHue = generator->SelectedDefaultHue = 0;
			generator->SelectedPreviewMode = previewMode;
		}
		generatorPanel->Visible = editorPanel->Visible;
		editorPanel->Visible = !generatorPanel->Visible;
		QueueReset(true);
	}
	System::Void GeneratorForm::addPoint_Click(System::Object^ sender, System::EventArgs^ e) {
		int i = editorPointX->Count;
		auto x = gcnew TextBox();
		auto y = gcnew TextBox();
		auto a = gcnew TextBox();
		auto c = gcnew Button();
		auto d = gcnew Button();
		editorPointX->Add(x);
		editorPointY->Add(y);
		editorPointA->Add(a);
		editorPointC->Add(c);
		editorPointD->Add(d);
		x->Text = "0";
		y->Text = "0";
		a->Text = "0";
		d->Text = "X";
		c->Text = "";
		c->BackColor = Color::Red;
		auto f = generator->GetFractal();
		auto ni = f->childCount++;
		delete generator->ChildColor;
		generator->ChildColor = new uint8_t[f->childCount];
		float* nx = new float[ni + 1];
		float* ny = new float[ni + 1];
		float* na = new float[ni + 1];
		uint8_t* nc = new byte[ni + 1];
		auto cx = f->childX;
		auto cy = f->childY;
		auto ca2 = f->childAngle[generator->SelectedChildAngle];
		auto ca = ca2.second;
		auto cc2 = f->childColor[generator->SelectesChildColor];
		auto cc = cc2.second;
		for (int ci = 0; ci < ni; ++ci) {
			nx[ci] = cx[ci];
			ny[ci] = cy[ci];
			na[ci] = ca[ci];
			nc[ci] = cc[ci];
		}
		nx[ni] = ny[ni] = na[ni] = nc[ni] = 0;
		delete[] cx;
		delete[] cy;
		delete[] ca;
		delete[] cc;
		f->childX = nx;
		f->childY = ny;
		f->childAngle[generator->SelectedChildAngle] = { ca2.first, na };
		f->childColor[generator->SelectesChildColor] = { cc2.first, nc };
		BindPoint(x, y, a, c, d, i, true);
		f->edit = true;
		QueueReset(true);
	}
	System::Void GeneratorForm::sizeBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		if (ParseDiffApply(sizeBox, &generator->GetFractal()->childSize)) return;
		generator->GetFractal()->edit = true;
	}
	System::Void GeneratorForm::maxBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		if (ParseDiffApply(maxBox, &generator->GetFractal()->maxSize)) return;
		generator->GetFractal()->edit = true;
	}
	System::Void GeneratorForm::minBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		if (ParseDiffApply(minBox, &generator->GetFractal()->minSize)) return;
		generator->GetFractal()->edit = true;
	}
	System::Void GeneratorForm::cutBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		if (ParseDiffApply(cutBox, &generator->GetFractal()->cutSize)) return;
		generator->GetFractal()->edit = true;
	}
	System::Void GeneratorForm::addAngleButton_Click(System::Object^ sender, System::EventArgs^ e) {
		if (angleBox->Text == "") {
			Error(
				"Type a unique name for the new set of children angles to the box on the left->",
				"Cannot add");
			return;
		}
		auto& c = generator->GetFractal()->childAngle;
		for (auto p = c.size(); 0 <= --p;)
			if (gcnew System::String(c[p].first.c_str()) == angleBox->Text) {
				Error(
					"There already is a set of children angles of the same name->\nType a new unique name to the box on the left->",
					"Cannot add");
				return;
			}
		c.push_back({ Fractal::ConvertToStdString(angleBox->Text), new float[generator->GetFractal()->childCount] });
		angleSelect->SelectedIndex = angleSelect->Items->Add(angleBox->Text);
		generator->GetFractal()->edit = true;
		FillEditor();
	}
	System::Void GeneratorForm::removeAngleButton_Click(System::Object^ sender, System::EventArgs^ e) {
		if (generator->GetFractal()->childColor.size() <= 1) {
			Error(
				"This is the only set of children angles, so it cannot be removed->",
				"Cannot remove");
			return;
		}
		auto& c = generator->GetFractal()->childAngle;
		const int16_t ns = c.size() - 1;
		for (int r = generator->SelectedChildAngle; r < ns; c[r] = c[++r]);
		c.resize(ns);
		generator->SelectedChildAngle = Math::Min(ns, generator->SelectedChildAngle);
		SwitchChildAngle();
		generator->GetFractal()->edit = true;
		QueueReset(true);
	}
	System::Void GeneratorForm::addColorButton_Click(System::Object^ sender, System::EventArgs^ e) {
		if (colorBox->Text == "") {
			Error(
				"Type a unique name for the new set of children colors to the box on the left->",
				"Cannot add");
			return;
		}
		auto& c = generator->GetFractal()->childColor;
		for (auto p = c.size(); 0 <= --p;)
			if (gcnew System::String(c[p].first.c_str()) == colorBox->Text) {
				Error(
					"There already is a set of children colors of the same name->\nType a new unique name to the box on the left->",
					"Cannot add");
				return;
			}
		c.push_back({ Fractal::ConvertToStdString(colorBox->Text), new uint8_t[generator->GetFractal()->childCount] });
		colorSelect->SelectedIndex = colorSelect->Items->Add(colorBox->Text);
		generator->GetFractal()->edit = true;
		FillEditor();
	}
	System::Void GeneratorForm::removeColorButton_Click(System::Object^ sender, System::EventArgs^ e) {
		if (generator->GetFractal()->childColor.size() <= 1) {
			Error(
				"This is the only set of children colors, so it cannot be removed->",
				"Cannot remove");
			return;
		}
		auto& c = generator->GetFractal()->childColor;
		const int16_t ns = c.size() - 1;
		for (int r = generator->SelectesChildColor; r < ns; c[r] = c[++r]);
		c.resize(ns);
		generator->SelectesChildColor = Math::Min(ns, generator->SelectesChildColor);
		SwitchChildColor();
		generator->GetFractal()->edit = true;
		QueueReset(true);
	}
	System::Void GeneratorForm::addCut_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (addCut->SelectedIndex < 1)
			return;
		int n = addCut->SelectedIndex - 1;
		for(auto& c : generator->GetFractal()->cutFunction)
			if (c.first == n)
				return;
		generator->GetFractal()->cutFunction.push_back({ n, { } });
		//SetupSelects();
		cutSelect->SelectedIndex = cutSelect->Items->Add(gcnew System::String(Fractal::cutFunctions[addCut->SelectedIndex - 1].first.c_str()));
		addCut->SelectedIndex = 0;
		generator->GetFractal()->edit = true;
		//_ = Task->Run(Hash);
		//Remember and disable all controls
		MyControlsEnabled->Clear();
		for each(auto c in MyControls) {
			MyControlsEnabled->Add(c->Enabled);
			c->Enabled = false;
		}
		// perform hash
		performHash = true;
		QueueReset(true);
	}
	System::Void GeneratorForm::loadButton_Click(System::Object^ sender, System::EventArgs^ e) {
		loadFractal->ShowDialog();
	}
	System::Void GeneratorForm::saveButton_Click(System::Object^ sender, System::EventArgs^ e) {
		tosave = generator->GetFractal();
		saveFractal->ShowDialog();
	}
	System::Void GeneratorForm::preButton_Click(System::Object^ sender, System::EventArgs^ e) {
		generator->SelectedPreviewMode = previewMode = !previewMode;
		QueueReset(true);
	}
#pragma endregion

}