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
	RgbFractalGenCpp::GeneratorForm form;
	Application::Run(% form);
}

namespace RgbFractalGenCpp {

	using namespace System::IO;
	using namespace System::Drawing::Imaging;

#pragma region Designer
	GeneratorForm::GeneratorForm(void) {
		InitializeComponent();
		this->screenPanel = gcnew DoubleBufferedPanel();
		this->screenPanel->Location = System::Drawing::Point(239, 13);
		this->screenPanel->Name = L"screenPanel";
		this->screenPanel->Size = System::Drawing::Size(80, 80);
		this->screenPanel->TabIndex = 25;
		this->screenPanel->Click += gcnew System::EventHandler(this, &GeneratorForm::animateButton_Click);
		this->screenPanel->Paint += gcnew System::Windows::Forms::PaintEventHandler(this, &GeneratorForm::screenPanel_Paint);
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
		this->previewBox = (gcnew System::Windows::Forms::CheckBox());
		this->periodBox = (gcnew System::Windows::Forms::TextBox());
		this->delayBox = (gcnew System::Windows::Forms::TextBox());
		this->zoomButton = (gcnew System::Windows::Forms::Button());
		this->prevButton = (gcnew System::Windows::Forms::Button());
		this->nextButton = (gcnew System::Windows::Forms::Button());
		this->animateButton = (gcnew System::Windows::Forms::Button());
		this->parallelBox = (gcnew System::Windows::Forms::CheckBox());
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
		this->encodeButton = (gcnew System::Windows::Forms::Button());
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
		this->parallelTypeBox = (gcnew System::Windows::Forms::ComboBox());
		this->ambBox = (gcnew System::Windows::Forms::TextBox());
		this->noiseBox = (gcnew System::Windows::Forms::TextBox());
		this->saturateBox = (gcnew System::Windows::Forms::TextBox());
		this->detailBox = (gcnew System::Windows::Forms::TextBox());
		this->bloomBox = (gcnew System::Windows::Forms::TextBox());
		this->blurBox = (gcnew System::Windows::Forms::TextBox());
		this->blurLabel = (gcnew System::Windows::Forms::Label());
		this->threadsBox = (gcnew System::Windows::Forms::TextBox());
		this->abortBox = (gcnew System::Windows::Forms::TextBox());
		this->restartButton = (gcnew System::Windows::Forms::Button());
		this->helpPanel->SuspendLayout();
		this->SuspendLayout();
		// 
		// fractalSelect
		// 
		this->fractalSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																 static_cast<System::Byte>(238)));
		this->fractalSelect->FormattingEnabled = true;
		this->fractalSelect->Location = System::Drawing::Point(71, 14);
		this->fractalSelect->Name = L"fractalSelect";
		this->fractalSelect->Size = System::Drawing::Size(225, 23);
		this->fractalSelect->TabIndex = 1;
		this->fractalSelect->Text = L"Select Fractal";
		this->fractalSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::fractalBox_SelectedIndexChanged);
		// 
		// resX
		// 
		this->resX->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														static_cast<System::Byte>(238)));
		this->resX->Location = System::Drawing::Point(17, 131);
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
		this->resY->Location = System::Drawing::Point(71, 131);
		this->resY->Name = L"resY";
		this->resY->Size = System::Drawing::Size(46, 23);
		this->resY->TabIndex = 5;
		this->resY->Text = L"1080";
		this->resY->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::ResolutionChanged);
		// 
		// previewBox
		// 
		this->previewBox->AutoSize = true;
		this->previewBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->previewBox->ForeColor = System::Drawing::Color::White;
		this->previewBox->Location = System::Drawing::Point(125, 135);
		this->previewBox->Name = L"previewBox";
		this->previewBox->Size = System::Drawing::Size(101, 19);
		this->previewBox->TabIndex = 6;
		this->previewBox->Text = L"Preview Mode";
		this->previewBox->UseVisualStyleBackColor = true;
		this->previewBox->CheckedChanged += gcnew System::EventHandler(this, &GeneratorForm::ResolutionChanged);
		// 
		// periodBox
		// 
		this->periodBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->periodBox->Location = System::Drawing::Point(17, 160);
		this->periodBox->Name = L"periodBox";
		this->periodBox->Size = System::Drawing::Size(86, 23);
		this->periodBox->TabIndex = 7;
		this->periodBox->Text = L"120";
		this->periodBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::periodBox_TextChanged);
		// 
		// delayBox
		// 
		this->delayBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->delayBox->Location = System::Drawing::Point(91, 465);
		this->delayBox->Name = L"delayBox";
		this->delayBox->Size = System::Drawing::Size(67, 23);
		this->delayBox->TabIndex = 23;
		this->delayBox->Text = L"5";
		this->delayBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::delayBox_TextChanged);
		// 
		// zoomButton
		// 
		this->zoomButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->zoomButton->Location = System::Drawing::Point(17, 189);
		this->zoomButton->Name = L"zoomButton";
		this->zoomButton->Size = System::Drawing::Size(86, 27);
		this->zoomButton->TabIndex = 9;
		this->zoomButton->Text = L"Zoom ->";
		this->zoomButton->UseVisualStyleBackColor = true;
		this->zoomButton->Click += gcnew System::EventHandler(this, &GeneratorForm::zoomButton_Click);
		// 
		// prevButton
		// 
		this->prevButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->prevButton->Location = System::Drawing::Point(17, 494);
		this->prevButton->Name = L"prevButton";
		this->prevButton->Size = System::Drawing::Size(30, 27);
		this->prevButton->TabIndex = 24;
		this->prevButton->Text = L"<-";
		this->prevButton->UseVisualStyleBackColor = true;
		this->prevButton->Click += gcnew System::EventHandler(this, &GeneratorForm::prevButton_Click);
		// 
		// nextButton
		// 
		this->nextButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->nextButton->Location = System::Drawing::Point(128, 494);
		this->nextButton->Name = L"nextButton";
		this->nextButton->Size = System::Drawing::Size(30, 27);
		this->nextButton->TabIndex = 25;
		this->nextButton->Text = L"->";
		this->nextButton->UseVisualStyleBackColor = true;
		this->nextButton->Click += gcnew System::EventHandler(this, &GeneratorForm::nextButton_Click);
		// 
		// animateButton
		// 
		this->animateButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->animateButton->Location = System::Drawing::Point(53, 494);
		this->animateButton->Name = L"animateButton";
		this->animateButton->Size = System::Drawing::Size(69, 27);
		this->animateButton->TabIndex = 26;
		this->animateButton->Text = L"Playing";
		this->animateButton->UseVisualStyleBackColor = true;
		this->animateButton->Click += gcnew System::EventHandler(this, &GeneratorForm::animateButton_Click);
		// 
		// parallelBox
		// 
		this->parallelBox->AutoSize = true;
		this->parallelBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->parallelBox->ForeColor = System::Drawing::Color::White;
		this->parallelBox->Location = System::Drawing::Point(24, 409);
		this->parallelBox->Name = L"parallelBox";
		this->parallelBox->Size = System::Drawing::Size(125, 19);
		this->parallelBox->TabIndex = 20;
		this->parallelBox->Text = L"Parallel Generation";
		this->parallelBox->UseVisualStyleBackColor = true;
		this->parallelBox->CheckedChanged += gcnew System::EventHandler(this, &GeneratorForm::parallel_Changed);
		// 
		// pngButton
		// 
		this->pngButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->pngButton->Location = System::Drawing::Point(91, 560);
		this->pngButton->Name = L"pngButton";
		this->pngButton->Size = System::Drawing::Size(67, 27);
		this->pngButton->TabIndex = 28;
		this->pngButton->Text = L"Save PNG";
		this->pngButton->UseVisualStyleBackColor = true;
		this->pngButton->Click += gcnew System::EventHandler(this, &GeneratorForm::pngButton_Click);
		// 
		// gifButton
		// 
		this->gifButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->gifButton->Location = System::Drawing::Point(175, 560);
		this->gifButton->Name = L"gifButton";
		this->gifButton->Size = System::Drawing::Size(121, 27);
		this->gifButton->TabIndex = 29;
		this->gifButton->Text = L"Save GIF";
		this->gifButton->UseVisualStyleBackColor = true;
		this->gifButton->Click += gcnew System::EventHandler(this, &GeneratorForm::gifButton_Click);
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
		this->savePng->FileOk += gcnew System::ComponentModel::CancelEventHandler(this, &GeneratorForm::savePng_FileOk);
		// 
		// saveGif
		// 
		this->saveGif->DefaultExt = L"gif";
		this->saveGif->Filter = L"GIF files (*.gif)|*.gif";
		this->saveGif->FileOk += gcnew System::ComponentModel::CancelEventHandler(this, &GeneratorForm::saveGif_FileOk);
		// 
		// fractalLabel
		// 
		this->fractalLabel->AutoSize = true;
		this->fractalLabel->ForeColor = System::Drawing::Color::White;
		this->fractalLabel->Location = System::Drawing::Point(15, 18);
		this->fractalLabel->Name = L"fractalLabel";
		this->fractalLabel->Size = System::Drawing::Size(42, 13);
		this->fractalLabel->TabIndex = 0;
		this->fractalLabel->Text = L"Fractal:";
		// 
		// delayLabel
		// 
		this->delayLabel->AutoSize = true;
		this->delayLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->delayLabel->ForeColor = System::Drawing::Color::White;
		this->delayLabel->Location = System::Drawing::Point(190, 468);
		this->delayLabel->Name = L"delayLabel";
		this->delayLabel->Size = System::Drawing::Size(70, 15);
		this->delayLabel->TabIndex = 0;
		this->delayLabel->Text = L"Abort / FPS:";
		// 
		// voidLabel
		// 
		this->voidLabel->AutoSize = true;
		this->voidLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->voidLabel->ForeColor = System::Drawing::Color::White;
		this->voidLabel->Location = System::Drawing::Point(21, 323);
		this->voidLabel->Name = L"voidLabel";
		this->voidLabel->Size = System::Drawing::Size(157, 15);
		this->voidLabel->TabIndex = 0;
		this->voidLabel->Text = L"Void Ambient / Noise (0-30):";
		// 
		// dotLabel
		// 
		this->dotLabel->AutoSize = true;
		this->dotLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->dotLabel->ForeColor = System::Drawing::Color::White;
		this->dotLabel->Location = System::Drawing::Point(21, 352);
		this->dotLabel->Name = L"dotLabel";
		this->dotLabel->Size = System::Drawing::Size(128, 15);
		this->dotLabel->TabIndex = 0;
		this->dotLabel->Text = L"Saturate / Detail (0-10):";
		// 
		// threadsLabel
		// 
		this->threadsLabel->AutoSize = true;
		this->threadsLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->threadsLabel->ForeColor = System::Drawing::Color::White;
		this->threadsLabel->Location = System::Drawing::Point(21, 439);
		this->threadsLabel->Name = L"threadsLabel";
		this->threadsLabel->Size = System::Drawing::Size(77, 15);
		this->threadsLabel->TabIndex = 0;
		this->threadsLabel->Text = L"Max Threads:";
		// 
		// statusLabel
		// 
		this->statusLabel->AutoSize = true;
		this->statusLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->statusLabel->ForeColor = System::Drawing::Color::White;
		this->statusLabel->Location = System::Drawing::Point(21, 524);
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
		this->infoLabel->Location = System::Drawing::Point(91, 524);
		this->infoLabel->Name = L"infoLabel";
		this->infoLabel->Size = System::Drawing::Size(28, 15);
		this->infoLabel->TabIndex = 0;
		this->infoLabel->Text = L"info";
		// 
		// defaultZoom
		// 
		this->defaultZoom->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->defaultZoom->Location = System::Drawing::Point(109, 192);
		this->defaultZoom->Name = L"defaultZoom";
		this->defaultZoom->Size = System::Drawing::Size(60, 23);
		this->defaultZoom->TabIndex = 12;
		this->defaultZoom->Text = L"0";
		this->defaultZoom->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::defaultZoom_TextChanged);
		// 
		// defaultAngle
		// 
		this->defaultAngle->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->defaultAngle->Location = System::Drawing::Point(175, 242);
		this->defaultAngle->Name = L"defaultAngle";
		this->defaultAngle->Size = System::Drawing::Size(121, 23);
		this->defaultAngle->TabIndex = 13;
		this->defaultAngle->Text = L"0";
		this->defaultAngle->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::defaultAngle_TextChanged);
		// 
		// encodeButton
		// 
		this->encodeButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->encodeButton->Location = System::Drawing::Point(175, 527);
		this->encodeButton->Name = L"encodeButton";
		this->encodeButton->Size = System::Drawing::Size(121, 27);
		this->encodeButton->TabIndex = 27;
		this->encodeButton->Text = L"Encode GIF";
		this->encodeButton->UseVisualStyleBackColor = true;
		this->encodeButton->Click += gcnew System::EventHandler(this, &GeneratorForm::encodeButton_Click);
		// 
		// cutparamBox
		// 
		this->cutparamBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->cutparamBox->Location = System::Drawing::Point(250, 101);
		this->cutparamBox->Name = L"cutparamBox";
		this->cutparamBox->Size = System::Drawing::Size(46, 23);
		this->cutparamBox->TabIndex = 2;
		this->cutparamBox->Text = L"0";
		this->cutparamBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::cutparamBox_TextChanged);
		// 
		// defaultHue
		// 
		this->defaultHue->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->defaultHue->Location = System::Drawing::Point(175, 291);
		this->defaultHue->Name = L"defaultHue";
		this->defaultHue->Size = System::Drawing::Size(121, 23);
		this->defaultHue->TabIndex = 14;
		this->defaultHue->Text = L"0";
		this->defaultHue->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::defaultHue_TextChanged);
		// 
		// periodMultiplierBox
		// 
		this->periodMultiplierBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																	   static_cast<System::Byte>(238)));
		this->periodMultiplierBox->Location = System::Drawing::Point(109, 160);
		this->periodMultiplierBox->Name = L"periodMultiplierBox";
		this->periodMultiplierBox->Size = System::Drawing::Size(60, 23);
		this->periodMultiplierBox->TabIndex = 8;
		this->periodMultiplierBox->Text = L"1";
		this->periodMultiplierBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::periodMultiplierBox_TextChanged);
		// 
		// periodLabel
		// 
		this->periodLabel->AutoSize = true;
		this->periodLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(0)));
		this->periodLabel->ForeColor = System::Drawing::Color::White;
		this->periodLabel->Location = System::Drawing::Point(174, 163);
		this->periodLabel->Name = L"periodLabel";
		this->periodLabel->Size = System::Drawing::Size(122, 15);
		this->periodLabel->TabIndex = 30;
		this->periodLabel->Text = L"<- Period + Multiplier";
		// 
		// helpPanel
		// 
		this->helpPanel->AutoScroll = true;
		this->helpPanel->BackColor = System::Drawing::Color::White;
		this->helpPanel->Controls->Add(this->helpLabel);
		this->helpPanel->Location = System::Drawing::Point(302, 14);
		this->helpPanel->Name = L"helpPanel";
		this->helpPanel->Size = System::Drawing::Size(763, 573);
		this->helpPanel->TabIndex = 0;
		// 
		// helpLabel
		// 
		this->helpLabel->AutoSize = true;
		this->helpLabel->Location = System::Drawing::Point(14, 10);
		this->helpLabel->Name = L"helpLabel";
		this->helpLabel->Size = System::Drawing::Size(35, 13);
		this->helpLabel->TabIndex = 0;
		this->helpLabel->Text = L"label1";
		// 
		// angleSelect
		// 
		this->angleSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->angleSelect->FormattingEnabled = true;
		this->angleSelect->Location = System::Drawing::Point(71, 43);
		this->angleSelect->Name = L"angleSelect";
		this->angleSelect->Size = System::Drawing::Size(225, 23);
		this->angleSelect->TabIndex = 32;
		this->angleSelect->Text = L"Select Angles";
		this->angleSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::angleSelect_SelectedIndexChanged);
		// 
		// colorSelect
		// 
		this->colorSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->colorSelect->FormattingEnabled = true;
		this->colorSelect->Location = System::Drawing::Point(71, 72);
		this->colorSelect->Name = L"colorSelect";
		this->colorSelect->Size = System::Drawing::Size(225, 23);
		this->colorSelect->TabIndex = 33;
		this->colorSelect->Text = L"Select Colors";
		this->colorSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::colorSelect_SelectedIndexChanged);
		// 
		// cutSelect
		// 
		this->cutSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->cutSelect->FormattingEnabled = true;
		this->cutSelect->Location = System::Drawing::Point(71, 101);
		this->cutSelect->Name = L"cutSelect";
		this->cutSelect->Size = System::Drawing::Size(173, 23);
		this->cutSelect->TabIndex = 34;
		this->cutSelect->Text = L"Select CutFunction";
		this->cutSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::cutSelect_SelectedIndexChanged);
		// 
		// helpButton
		// 
		this->helpButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->helpButton->Location = System::Drawing::Point(17, 560);
		this->helpButton->Name = L"helpButton";
		this->helpButton->Size = System::Drawing::Size(68, 27);
		this->helpButton->TabIndex = 35;
		this->helpButton->Text = L"Help";
		this->helpButton->UseVisualStyleBackColor = true;
		this->helpButton->Click += gcnew System::EventHandler(this, &GeneratorForm::helpButton_Click);
		// 
		// angleLabel
		// 
		this->angleLabel->AutoSize = true;
		this->angleLabel->ForeColor = System::Drawing::Color::White;
		this->angleLabel->Location = System::Drawing::Point(15, 47);
		this->angleLabel->Name = L"angleLabel";
		this->angleLabel->Size = System::Drawing::Size(42, 13);
		this->angleLabel->TabIndex = 36;
		this->angleLabel->Text = L"Angles:";
		// 
		// colorLabel
		// 
		this->colorLabel->AutoSize = true;
		this->colorLabel->ForeColor = System::Drawing::Color::White;
		this->colorLabel->Location = System::Drawing::Point(15, 76);
		this->colorLabel->Name = L"colorLabel";
		this->colorLabel->Size = System::Drawing::Size(36, 13);
		this->colorLabel->TabIndex = 37;
		this->colorLabel->Text = L"Colors";
		// 
		// cutLabel
		// 
		this->cutLabel->AutoSize = true;
		this->cutLabel->ForeColor = System::Drawing::Color::White;
		this->cutLabel->Location = System::Drawing::Point(15, 105);
		this->cutLabel->Name = L"cutLabel";
		this->cutLabel->Size = System::Drawing::Size(26, 13);
		this->cutLabel->TabIndex = 38;
		this->cutLabel->Text = L"Cut:";
		// 
		// zoomLabel
		// 
		this->zoomLabel->AutoSize = true;
		this->zoomLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(0)));
		this->zoomLabel->ForeColor = System::Drawing::Color::White;
		this->zoomLabel->Location = System::Drawing::Point(175, 195);
		this->zoomLabel->Name = L"zoomLabel";
		this->zoomLabel->Size = System::Drawing::Size(125, 15);
		this->zoomLabel->TabIndex = 39;
		this->zoomLabel->Text = L"<- Zoom + First frame";
		// 
		// spinSelect
		// 
		this->spinSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->spinSelect->FormattingEnabled = true;
		this->spinSelect->Items->AddRange(gcnew cli::array< System::Object^  >(5) { L"->|<-", L"->", L"X", L"<-", L"<-|->" });
		this->spinSelect->Location = System::Drawing::Point(18, 242);
		this->spinSelect->Name = L"spinSelect";
		this->spinSelect->Size = System::Drawing::Size(85, 23);
		this->spinSelect->TabIndex = 40;
		this->spinSelect->Text = L"Select Spin";
		this->spinSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::spinSelect_SelectedIndexChanged);
		// 
		// hueSelect
		// 
		this->hueSelect->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->hueSelect->FormattingEnabled = true;
		this->hueSelect->Items->AddRange(gcnew cli::array< System::Object^  >(6) {
			L"RGB (static)", L"BGR (static)", L"RGB->GBR",
				L"BGR->RBG", L"RGB->BRG", L"BGR->GRB"
		});
		this->hueSelect->Location = System::Drawing::Point(18, 291);
		this->hueSelect->Name = L"hueSelect";
		this->hueSelect->Size = System::Drawing::Size(85, 23);
		this->hueSelect->TabIndex = 42;
		this->hueSelect->Text = L"Select Hue";
		this->hueSelect->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::hueSelect_SelectedIndexChanged);
		// 
		// spinLabel
		// 
		this->spinLabel->AutoSize = true;
		this->spinLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->spinLabel->ForeColor = System::Drawing::Color::White;
		this->spinLabel->Location = System::Drawing::Point(21, 223);
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
		this->label1->Location = System::Drawing::Point(17, 273);
		this->label1->Name = L"label1";
		this->label1->Size = System::Drawing::Size(246, 15);
		this->label1->TabIndex = 45;
		this->label1->Text = L"Hue Select           HueSpeed           Default Hue";
		// 
		// spinSpeedBox
		// 
		this->spinSpeedBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->spinSpeedBox->Location = System::Drawing::Point(109, 242);
		this->spinSpeedBox->Name = L"spinSpeedBox";
		this->spinSpeedBox->Size = System::Drawing::Size(60, 23);
		this->spinSpeedBox->TabIndex = 46;
		this->spinSpeedBox->Text = L"0";
		this->spinSpeedBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::spinSpeedBox_TextChanged);
		// 
		// hueSpeedBox
		// 
		this->hueSpeedBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->hueSpeedBox->Location = System::Drawing::Point(109, 291);
		this->hueSpeedBox->Name = L"hueSpeedBox";
		this->hueSpeedBox->Size = System::Drawing::Size(60, 23);
		this->hueSpeedBox->TabIndex = 47;
		this->hueSpeedBox->Text = L"0";
		this->hueSpeedBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::hueSpeedBox_TextChanged);
		// 
		// parallelTypeBox
		// 
		this->parallelTypeBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																   static_cast<System::Byte>(238)));
		this->parallelTypeBox->FormattingEnabled = true;
		this->parallelTypeBox->Items->AddRange(gcnew cli::array< System::Object^  >(2) { L"Of Animation", L"Of Depth" });
		this->parallelTypeBox->Location = System::Drawing::Point(177, 407);
		this->parallelTypeBox->Name = L"parallelTypeBox";
		this->parallelTypeBox->Size = System::Drawing::Size(119, 23);
		this->parallelTypeBox->TabIndex = 21;
		this->parallelTypeBox->Text = L"Parallelism Type";
		this->parallelTypeBox->SelectedIndexChanged += gcnew System::EventHandler(this, &GeneratorForm::parallelTypeBox_SelectedIndexChanged);
		// 
		// ambBox
		// 
		this->ambBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														  static_cast<System::Byte>(238)));
		this->ambBox->Location = System::Drawing::Point(175, 320);
		this->ambBox->Name = L"ambBox";
		this->ambBox->Size = System::Drawing::Size(60, 23);
		this->ambBox->TabIndex = 48;
		this->ambBox->Tag = L"";
		this->ambBox->Text = L"20";
		this->ambBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::ambBox_TextChanged);
		// 
		// noiseBox
		// 
		this->noiseBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->noiseBox->Location = System::Drawing::Point(236, 320);
		this->noiseBox->Name = L"noiseBox";
		this->noiseBox->Size = System::Drawing::Size(60, 23);
		this->noiseBox->TabIndex = 49;
		this->noiseBox->Tag = L"";
		this->noiseBox->Text = L"20";
		this->noiseBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::noiseBox_TextChanged);
		// 
		// saturateBox
		// 
		this->saturateBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->saturateBox->Location = System::Drawing::Point(175, 349);
		this->saturateBox->Name = L"saturateBox";
		this->saturateBox->Size = System::Drawing::Size(60, 23);
		this->saturateBox->TabIndex = 50;
		this->saturateBox->Tag = L"";
		this->saturateBox->Text = L"5";
		this->saturateBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::saturateBox_TextChanged);
		// 
		// detailBox
		// 
		this->detailBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->detailBox->Location = System::Drawing::Point(236, 349);
		this->detailBox->Name = L"detailBox";
		this->detailBox->Size = System::Drawing::Size(60, 23);
		this->detailBox->TabIndex = 51;
		this->detailBox->Tag = L"";
		this->detailBox->Text = L"5";
		this->detailBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::detailBox_TextChanged);
		// 
		// bloomBox
		// 
		this->bloomBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->bloomBox->Location = System::Drawing::Point(175, 378);
		this->bloomBox->Name = L"bloomBox";
		this->bloomBox->Size = System::Drawing::Size(60, 23);
		this->bloomBox->TabIndex = 52;
		this->bloomBox->Tag = L"";
		this->bloomBox->Text = L"0";
		this->bloomBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::bloomBox_TextChanged);
		// 
		// blurBox
		// 
		this->blurBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														   static_cast<System::Byte>(238)));
		this->blurBox->Location = System::Drawing::Point(236, 378);
		this->blurBox->Name = L"blurBox";
		this->blurBox->Size = System::Drawing::Size(60, 23);
		this->blurBox->TabIndex = 53;
		this->blurBox->Tag = L"";
		this->blurBox->Text = L"0";
		this->blurBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::blurBox_TextChanged);
		// 
		// blurLabel
		// 
		this->blurLabel->AutoSize = true;
		this->blurLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->blurLabel->ForeColor = System::Drawing::Color::White;
		this->blurLabel->Location = System::Drawing::Point(21, 381);
		this->blurLabel->Name = L"blurLabel";
		this->blurLabel->Size = System::Drawing::Size(153, 15);
		this->blurLabel->TabIndex = 54;
		this->blurLabel->Text = L"Bloom / Motion Blur (0-40):";
		// 
		// threadsBox
		// 
		this->threadsBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->threadsBox->Location = System::Drawing::Point(175, 436);
		this->threadsBox->Name = L"threadsBox";
		this->threadsBox->Size = System::Drawing::Size(121, 23);
		this->threadsBox->TabIndex = 55;
		this->threadsBox->Tag = L"";
		this->threadsBox->Text = L"0";
		this->threadsBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::parallel_Changed);
		// 
		// abortBox
		// 
		this->abortBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->abortBox->Location = System::Drawing::Point(18, 465);
		this->abortBox->Name = L"abortBox";
		this->abortBox->Size = System::Drawing::Size(67, 23);
		this->abortBox->TabIndex = 56;
		this->abortBox->Text = L"500";
		this->abortBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::abortBox_TextChanged);
		// 
		// restartButton
		// 
		this->restartButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->restartButton->Location = System::Drawing::Point(175, 494);
		this->restartButton->Name = L"restartButton";
		this->restartButton->Size = System::Drawing::Size(121, 27);
		this->restartButton->TabIndex = 57;
		this->restartButton->Text = L"! RESTART !";
		this->restartButton->UseVisualStyleBackColor = true;
		this->restartButton->Click += gcnew System::EventHandler(this, &GeneratorForm::Restart_Click);
		// 
		// GeneratorForm
		// 
		this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
		this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
		this->BackColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(64)), static_cast<System::Int32>(static_cast<System::Byte>(64)),
														   static_cast<System::Int32>(static_cast<System::Byte>(64)));
		this->ClientSize = System::Drawing::Size(1079, 605);
		this->Controls->Add(this->restartButton);
		this->Controls->Add(this->abortBox);
		this->Controls->Add(this->threadsBox);
		this->Controls->Add(this->blurLabel);
		this->Controls->Add(this->blurBox);
		this->Controls->Add(this->bloomBox);
		this->Controls->Add(this->detailBox);
		this->Controls->Add(this->saturateBox);
		this->Controls->Add(this->noiseBox);
		this->Controls->Add(this->ambBox);
		this->Controls->Add(this->parallelTypeBox);
		this->Controls->Add(this->hueSpeedBox);
		this->Controls->Add(this->spinSpeedBox);
		this->Controls->Add(this->label1);
		this->Controls->Add(this->spinLabel);
		this->Controls->Add(this->hueSelect);
		this->Controls->Add(this->spinSelect);
		this->Controls->Add(this->zoomLabel);
		this->Controls->Add(this->cutLabel);
		this->Controls->Add(this->colorLabel);
		this->Controls->Add(this->angleLabel);
		this->Controls->Add(this->helpButton);
		this->Controls->Add(this->cutSelect);
		this->Controls->Add(this->colorSelect);
		this->Controls->Add(this->angleSelect);
		this->Controls->Add(this->helpPanel);
		this->Controls->Add(this->periodLabel);
		this->Controls->Add(this->periodMultiplierBox);
		this->Controls->Add(this->defaultHue);
		this->Controls->Add(this->cutparamBox);
		this->Controls->Add(this->encodeButton);
		this->Controls->Add(this->defaultAngle);
		this->Controls->Add(this->defaultZoom);
		this->Controls->Add(this->pngButton);
		this->Controls->Add(this->gifButton);
		this->Controls->Add(this->fractalLabel);
		this->Controls->Add(this->fractalSelect);
		this->Controls->Add(this->resX);
		this->Controls->Add(this->resY);
		this->Controls->Add(this->previewBox);
		this->Controls->Add(this->periodBox);
		this->Controls->Add(this->delayLabel);
		this->Controls->Add(this->delayBox);
		this->Controls->Add(this->zoomButton);
		this->Controls->Add(this->nextButton);
		this->Controls->Add(this->prevButton);
		this->Controls->Add(this->animateButton);
		this->Controls->Add(this->voidLabel);
		this->Controls->Add(this->dotLabel);
		this->Controls->Add(this->threadsLabel);
		this->Controls->Add(this->parallelBox);
		this->Controls->Add(this->statusLabel);
		this->Controls->Add(this->infoLabel);
		this->Icon = (cli::safe_cast<System::Drawing::Icon^>(resources->GetObject(L"$this.Icon")));
		this->Name = L"GeneratorForm";
		this->Text = L"RGB Fractal Zoom Generator Cpp v1.83";
		this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &GeneratorForm::GeneratorForm_FormClosing);
		this->Load += gcnew System::EventHandler(this, &GeneratorForm::GeneratorForm_Load);
		this->helpPanel->ResumeLayout(false);
		this->helpPanel->PerformLayout();
		this->ResumeLayout(false);
		this->PerformLayout();

	}
#pragma endregion

#pragma region Core
	System::Void GeneratorForm::GeneratorForm_Load(System::Object^ sender, System::EventArgs^ e) {
		// Setup interactable controls (tooltips + tabIndex)
		SetupControl(fractalSelect, L"Select the type of fractal to generate");
		SetupControl(angleSelect, L"Select the children angles definition.");
		SetupControl(colorSelect, L"Select the children colors definition.");
		SetupControl(cutSelect, L"Select the cutfunction definition.");
		SetupControl(cutparamBox, L"Type the cutfunction seed.");
		//SetupControl(cutparamBar, L"Select the cutfunction seed.");
		SetupControl(resX, L"Type the X resolution of the render (width)");
		SetupControl(resY, L"Type the Y resolution of the render (height)");
		SetupControl(previewBox, L"If checked, the resolution will be only 80x80 for fast preview render.\nUncheck it when you are done with preparing the settings and want to render it in full resolution");
		SetupControl(periodBox, L"How many frames for the self-similar center child to zoom in to the same size as the parent if you are zooming in.\nOr for the parent to zoom out to the same size as the child if you are zooming out.\nThis will equal the number of generated frames of the animation if the center child is the same color.\nOr it will be a third of the number of generated frames of the animation if the center child is a different color.");
		SetupControl(periodMultiplierBox, L"Multiplies the frame count, slowing down the rotaion and hue shifts.");
		SetupControl(zoomButton, L"Toggle in which direction you want the fractal zoom . ZoomIn, or <- ZoomOut");
		SetupControl(defaultZoom, L"Type the initial zoom of the first image (in number of skipped frames).");
		SetupControl(spinSelect, L"Choose in which direction you want the zoom animation to spin, or to not spin.");
		SetupControl(spinSpeedBox, L"Type the extra speed on the spinning from the values possible for looping.");
		SetupControl(defaultAngle, L"Type the initial angle of the first image (in degrees).");
		SetupControl(hueSelect, L"Choose the color scheme of the fractal.\nAlso you can make the color hues slowly cycle as the fractal zoom.");
		SetupControl(hueSpeedBox, L"Type the extra speed on the hue shifting from the values possible for looping.\nOnly possible if you have chosen color cycling on the left.");
		SetupControl(defaultHue, L"Type the initial hue angle of the first image (in degrees).");
		SetupControl(ambBox, L"The strength of the ambient grey color in the empty spaces far away between the generated fractal dots.");
		SetupControl(noiseBox, L"The strength of the random noise in the empty spaces far away between the generated fractal dots.");
		SetupControl(saturateBox, L"Enhance the color saturation of the fractal dots.\nUseful when the fractal is too gray, like the Sierpinski Triangle (Triflake).");
		SetupControl(detailBox, L"Level of Detail (The lower the finer).");
		SetupControl(bloomBox, L"Bloom: 0 will be maximally crisp, but possibly dark with think fractals. Higher values wil blur/bloom out the fractal dots.");
		SetupControl(blurBox, L"Motion blur: Lowest no blur and fast generation, highest 10 smeared frames 10 times slower generation.");
		SetupControl(parallelBox, L"Enable parallelism (and then tune with the Max Threads slider).\nSelect the type of parallelism with the followinf checkBox to the right.");
		SetupControl(parallelTypeBox, L"Select which parallelism to be used if the left checkBox is enabled.\nOf Animation = Batching animation frames, recommended for Animations with perfect pixels.\nOf Depth/Of Recursion = parallel single image generation, recommmended for fast single images, 1 in a million pixels might be slightly wrong");
		SetupControl(threadsBox, L"The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
		SetupControl(abortBox, L"How many millisecond of pause after the last settings change until the generator restarts?");
		SetupControl(delayBox, L"A delay between frames in 1/100 of seconds for the preview and exported GIF file.\nThe framerate will be roughly 100/delay");
		SetupControl(prevButton, L"Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
		SetupControl(nextButton, L"Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
		SetupControl(animateButton, L"Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
		SetupControl(encodeButton, L"Only Image - only generates one image\nAnimation RAM - generated an animation without GIF encoding, faster but can't save GIF afterwards\nEncode GIF - encodes GIF while generating an animation - can save a GIF afterwards");
		SetupControl(helpButton, L"Show README.txt.");
		SetupControl(pngButton, L"Save the currently displayed frame into a PNG file.\nStop the animation and select the frame you wish to export with the buttons above.");
		SetupControl(gifButton, L"Save the full animation into a GIF file.");

		// Read the REDME.txt for the help button
		if (File::Exists("README.txt"))
			helpLabel->Text = File::ReadAllText("README.txt");

		// Update Input fields to default values
		generator = new FractalGenerator();
		for (Fractal** i = generator->GetFractals(); *i != nullptr; ++i)
			fractalSelect->Items->Add(gcnew String(((*i)->name).c_str()));
		generator->selectFractal = -1;
		fractalSelect->SelectedIndex = 0;
		// Update Input fields to default values - modifySettings is true from constructor so that it doesn't abort and restant the generator over and over
		abortBox_TextChanged(DN);
		periodBox_TextChanged(DN);
		periodMultiplierBox_TextChanged(DN);
		parallelTypeBox_SelectedIndexChanged(DN);
		delayBox_TextChanged(DN);
		defaultZoom_TextChanged(DN);
		spinSpeedBox_TextChanged(DN);
		hueSpeedBox_TextChanged(DN);
		defaultHue_TextChanged(DN);
		ambBox_TextChanged(DN);
		noiseBox_TextChanged(DN);
		bloomBox_TextChanged(DN);
		blurBox_TextChanged(DN);
		saturateBox_TextChanged(DN);
		parallelTypeBox->SelectedIndex = 0;
		hueSelect->SelectedIndex = 0;
		SetupFractal();
		threadsBox->Text = (maxTasks = Math::Max(0, Environment::ProcessorCount - 2)).ToString();
		modifySettings = false;
		helpPanel->Visible = false;
		// Start the generator
		TryResize();
		ResizeAll();
		aTask = gTask = nullptr;
		generator->StartGenerate();
	}
	System::Void GeneratorForm::timer_Tick(System::Object^ sender, System::EventArgs^ e) {
		// Window Size Update
		WindowSizeRefresh();
		if (queueReset > 0) {
			if (!(IsTaskNotRunning(gTask) && IsTaskNotRunning(aTask)))
				return;
			if (queueAbort)
				aTask = Task::Run(gcnew Action(this, &GeneratorForm::Abort), (cancel = gcnew CancellationTokenSource())->Token);
			if ((queueReset -= (short)timer->Interval) > 0)
				return;
			SetupFractal();
			ResizeAll();
			queueReset = 0;
			generator->StartGenerate();
		}
		const auto gTaskNotRunning = IsTaskNotRunning(gTask);
		// Fetch the state of generated bitmaps
		const auto b = generator->GetBitmapsFinished(), bt = generator->GetFrames();
		if (bt <= 0)
			return;
		// Only Allow GIF Export when generation is finished
		gifButton->Enabled = generator->IsGifReady() && gTaskNotRunning;
		if (b > 0) {
			while (bitmapFinished < b && generator->GetBitmapState(bitmapFinished) == 4) {
				bitmaps[bitmapFinished]->UnlockBits(bitmapData[bitmapFinished]);
				generator->UnlockBitmapState(bitmapFinished);
				++bitmapFinished;
			}
			// Fetch bitmap, make sure the index is is range
			Bitmap^ bitmap = bitmaps[currentBitmapIndex = currentBitmapIndex % b];
			if (bitmap != nullptr) {
				// Update the display with it if necessary
				if (currentBitmap != bitmap) {
					currentBitmap = bitmap;
					screenPanel->Invalidate();
				}
				// Animate the frame index
				if (animated)
					currentBitmapIndex = (currentBitmapIndex + 1) % b;
			}
		}
		// Info text refresh
		if (b < bt) {
			statusLabel->Text = "Generating: ";
			infoLabel->Text = b.ToString();
		} else {
			statusLabel->Text = "Finished: ";
			infoLabel->Text = currentBitmapIndex.ToString();
		}
		infoLabel->Text += " / " + bt.ToString();
		gifButton->Text = gTaskNotRunning ? "Save GIF" : "Saving GIF...";
	}
	System::Void GeneratorForm::GeneratorForm_FormClosing(System::Object^ sender, System::Windows::Forms::FormClosingEventArgs^ e) {
		if (!IsTaskNotRunning(gTask)) {
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
		if (cancel != nullptr)
			cancel->Cancel();
		if (!IsTaskNotRunning(gTask))
			gTask->Wait();
		if (!IsTaskNotRunning(aTask))
			aTask->Wait();
		Abort();
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
		// 
		// Unlock unfinished bitmaps:
		for (int i = bitmapFinished; i < bitmaps->Length; ++i)
			if (generator->GetBitmapState(i) < 5 && bitmaps[i] != nullptr) {
				try {
					bitmaps[i]->UnlockBits(bitmapData[i]);
				} catch (Exception^) {}
			}
		bitmapFinished = currentBitmapIndex = 0;
	}
	System::Void GeneratorForm::SetupControl(Control^ control, System::String^ tip) {
		// Add tooltip and set the next tabIndex
		toolTips->SetToolTip(control, tip);
		control->TabIndex = ++controlTabIndex;
		//MyControls->Add(control);
	}
#pragma endregion

#pragma region Size
	System::Void GeneratorForm::ResizeAll() {
		generator->selectWidth = width;
		generator->selectHeight = height;
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
			// (Abort should be called before this or else it will crash)
			// generator->StartGenerate(); should be called after
		gifButton->Enabled = false;
		currentBitmapIndex = bitmapFinished = 0;
		generator->ResetGenerator();
		SizeAdapt();

		// Prepare Bitmaps, locked BitmapData, and the memory pointer for the unmanaged code
		const auto bt = generator->GetFrames();
		bitmaps = gcnew array<Bitmap^>(bt);
		bitmapData = gcnew array<System::Drawing::Imaging::BitmapData^>(bt);
		for (auto i = 0; i < bt; ++i) {
			bitmaps[i] = gcnew Bitmap(width, height);
			bitmapData[i] = bitmaps[i]->LockBits(
				System::Drawing::Rectangle(0, 0, width, height),
				ImageLockMode::WriteOnly,
				PixelFormat::Format24bppRgb);
			generator->SetPixelsPointer(i, (uint8_t*)(void*)bitmapData[i]->Scan0);
		}
	}
	bool GeneratorForm::TryResize() {
		previewMode = !previewBox->Checked;
		width = 8;
		height = 8;
		if (!int16_t::TryParse(resX->Text, width) || width <= 8)
			width = 8;
		if (!int16_t::TryParse(resY->Text, height) || height <= 0)
			height = 8;
		previewBox->Text = "Resolution: " + width.ToString() + "x" + height.ToString();
		if (previewMode)
			width = height = 80;
		return generator->selectWidth != width || generator->selectHeight != height;
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
			Math::Max(640, bh + Math::Max(460, height + 8))
		);
	}
	System::Void GeneratorForm::ResizeScreen() {
		int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
		const auto screenHeight = Math::Max((int)height, Math::Min(Height - bh - 8, (Width - bw - 314) * (int)height / (int)width));
		screenPanel->SetBounds(305, 4, screenHeight * width / height, screenHeight);
		helpPanel->SetBounds(305, 4, Width - bw - 314, Height - bh - 8);
		screenPanel->Invalidate();
	}
#pragma endregion

#pragma region Input
	System::Void GeneratorForm::SetupFractal() {
		generator->SetupFractal();
		parallel_Changed(DN);
		detailBox_TextChanged(DN);
		generator->SetupColor();
		generator->SetupAngle();
		generator->SetupCutFunction();
	}
	System::Void GeneratorForm::QueueReset() {
		if (modifySettings)
			return;
		if (queueReset <= 0) {
			gifButton->Enabled = false;
			currentBitmapIndex = bitmapFinished = 0;
			//if (gTask == nullptr && aTask == nullptr)
			if (IsTaskNotRunning(gTask) && IsTaskNotRunning(aTask))
				aTask = Task::Run(gcnew Action(this, &GeneratorForm::Abort), (cancel = gcnew CancellationTokenSource())->Token);
			else queueAbort = true;
		}
		queueReset = abortDelay;
	}
	System::Void GeneratorForm::fractalBox_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectFractal(Math::Max(0, fractalSelect->SelectedIndex)))
			return;
		// Fractal is different - load it, change the setting and restart generation
		// Fill the fractal's adjuistable definition combos
		// Fill the fractal's adjuistable cutfunction seed combos, and restart generation
		if (!modifySettings) {
			modifySettings = true;
			FillSelects();
			FillCutParams();
			modifySettings = false;
			QueueReset();
		} else {
			FillSelects();
			FillCutParams();
		}
	}
#define LoadCombo(S,A,V) S->Items->Clear();auto V = generator->GetFractal()->A;S->Enabled = V != nullptr;if (S->Enabled) {while (V != nullptr && V->first != "")S->Items->Add(gcnew String(((V++)->first).c_str()));S->Enabled = S->Items->Count > 0;if (S->Enabled)S->SelectedIndex = 0;};
	System::Void GeneratorForm::FillSelects() {
		// Fill angle childred definitnions combobox
		LoadCombo(angleSelect, childAngle, a)
		// Fill color children definitnions combobox
		LoadCombo(colorSelect, childColor, c)
		// Fill cutfunction definitnions combobox
		LoadCombo(cutSelect, cutFunction, f)
	}
	System::Void GeneratorForm::angleSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectAngle(Math::Max(0, angleSelect->SelectedIndex)))
			return;
		// Angle children definition is different - change the setting and restart generation
		QueueReset();
	}
	System::Void GeneratorForm::colorSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectColor(Math::Max(0, colorSelect->SelectedIndex)))
			return;
		// Color children definition is different - change the setting and restart generation
		QueueReset();
	}
	System::Void GeneratorForm::cutSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectCutFunction(Math::Max(0, cutSelect->SelectedIndex)))
			return;
		// Cutfunction is different - change the setting and restart generation
		FillCutParams();
		QueueReset();
	}
	System::Void GeneratorForm::FillCutParams() { CutParamBoxEnabled(generator->GetCutFunction()); }

#define DIFF_PARAM(NEW, GEN) if (generator->GEN == NEW) return;
#define APPLY_PARAM(NEW, GEN) generator->GEN = NEW; QueueReset();
#define APPLY_DIFF_PARAM(NEW, GEN) DIFF_PARAM(NEW, GEN) APPLY_PARAM(NEW, GEN)
#define CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX) TYPE NEW; if(!TYPE::TryParse(BOX->Text, NEW)) NEW = MIN; \
	BOX->Text = (NEW = Clamp(NEW, static_cast<TYPE>(MIN), static_cast<TYPE>(MAX))).ToString();
#define APPLY_CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX, GEN) CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX) APPLY_DIFF_PARAM(NEW, GEN)
#define APPLY_DCLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX, TYPE2, NEW2, MUL, GEN) CLAMP_PARAM(TYPE, BOX, NEW, MIN, MAX)\
	const TYPE2 NEW2 = static_cast<TYPE2>(NEW * MUL); APPLY_DIFF_PARAM(NEW2, GEN)
#define APPLY_MOD_PARAM(TYPE, BOX, NEW, MIN, MAX, GEN)TYPE NEW; if(TYPE::TryParse(BOX->Text, NEW)) NEW = MIN; \
	while (NEW < MIN)NEW += (MAX-MIN); while (NEW >= MAX)NEW -= (MAX-MIN); APPLY_DIFF_PARAM(NEW, GEN)

	System::Void GeneratorForm::cutparamBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_CLAMP_PARAM(uint16_t, cutparamBox, newCutparam, 0, cutparamMaximum, cutparam)
	}
	System::Void GeneratorForm::ResolutionChanged(System::Object^ sender, System::EventArgs^ e) {
		if (TryResize())
			QueueReset();
	}
	System::Void GeneratorForm::periodBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_CLAMP_PARAM(uint16_t, periodBox, newPeriod, 1, 1000, period)
	}
	System::Void GeneratorForm::periodMultiplierBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_CLAMP_PARAM(uint16_t, periodMultiplierBox, newPeriod, 1, 10, periodMultiplier)
	}
	System::Void GeneratorForm::zoomButton_Click(System::Object^ sender, System::EventArgs^ e) {
		// zoom is different - change the setting and restart generation
		generator->selectZoom = -generator->selectZoom;
		zoomButton->Text = "Zoom: " + ((generator->selectZoom > 0) ? "->" : "<-");
		QueueReset();
	}
	System::Void GeneratorForm::defaultZoom_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		int finalPeriod = generator->selectPeriod * generator->GetFinalPeriod();
		APPLY_MOD_PARAM(int16_t, defaultZoom, newZoom, 0, finalPeriod, defaultZoom)
	}
	System::Void GeneratorForm::spinSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		const auto newSpin = Math::Max(0, Math::Min(4, spinSelect->SelectedIndex)) - 2;
		APPLY_DIFF_PARAM(newSpin, defaultSpin)
	}
	System::Void GeneratorForm::spinSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_CLAMP_PARAM(uint16_t, spinSpeedBox, newSpeed, 0, 255, extraSpin)
	}
	System::Void GeneratorForm::defaultAngle_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_MOD_PARAM(int16_t, defaultAngle, newAngle, 0, 360, defaultAngle)
	}
	System::Void GeneratorForm::hueSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		uint8_t colorChoice = (uint8_t)((hueSelect->SelectedIndex) % 6);
		int8_t newHueCycle = ((int8_t)(colorChoice) / 2 + 1) % 3 - 1;
		if (generator->SelectColorPalette(colorChoice % 2) && newHueCycle == generator->hueCycle)
			return;
		APPLY_PARAM(newHueCycle, hueCycle)
	}
	System::Void GeneratorForm::hueSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		CLAMP_PARAM(uint16_t, hueSpeedBox, newSpeed, 0, 255)
			DIFF_PARAM(newSpeed, extraHue)
			// hue speed is different - change the setting and if it's actually huecycling restart generation
			if (generator->hueCycle != 0) {
				APPLY_PARAM(newSpeed, extraHue)
			} else generator->selectExtraHue = newSpeed;
	}
	System::Void GeneratorForm::defaultHue_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_MOD_PARAM(uint16_t, defaultHue, newHue, 0, 360, defaultHue)
	}
	System::Void GeneratorForm::ambBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_DCLAMP_PARAM(uint16_t, ambBox, newAmb, 0, 30, uint8_t, newnewAmb, 4, amb)
	}
	System::Void GeneratorForm::noiseBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_DCLAMP_PARAM(uint16_t, noiseBox, newNoise, 0, 30, float, newNoiseFloat, .1f, noise)
	}
	System::Void GeneratorForm::saturateBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_DCLAMP_PARAM(uint16_t, saturateBox, newSaturate, 0, 10, float, newSaturateFloat, .1f, saturate)
	}
	System::Void GeneratorForm::detailBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		CLAMP_PARAM(uint16_t, detailBox, newDetail, 0, 10)
			const auto newDetailFloat = newDetail * .1f * generator->GetFractal()->minSize;
		APPLY_DIFF_PARAM(newDetailFloat, detail)
			generator->SetMaxIterations();
	}
	System::Void GeneratorForm::bloomBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		APPLY_DCLAMP_PARAM(uint16_t, bloomBox, newBloom, 0, 40, float, newSaturateFloat, .25f, bloom)
	}
	System::Void GeneratorForm::blurBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		CLAMP_PARAM(uint16_t, blurBox, newBlur, 0, 40)
			++newBlur;
		APPLY_DIFF_PARAM(newBlur, selectBlur)
	}
	System::Void GeneratorForm::parallel_Changed(System::Object^ sender, System::EventArgs^ e) {
		CLAMP_PARAM(uint16_t, threadsBox, newThreads, 2, maxTasks)
			threadsLabel->Text = "Maximum threads (0-" + maxTasks + "):";
		generator->selectMaxTasks = (short)(parallelBox->Checked && newThreads > 0 ? newThreads : -1);
		generator->selectMaxGenerationTasks = generator->selectMaxTasks - 1;
		generator->SelectThreadingDepth();
	}
	System::Void GeneratorForm::parallelTypeBox_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		generator->selectParallelType = parallelTypeBox->SelectedIndex;
	}
	System::Void GeneratorForm::abortBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		CLAMP_PARAM(uint16_t, abortBox, newAbort, 1, 10000)
			abortDelay = newAbort;
	}
	System::Void GeneratorForm::delayBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		CLAMP_PARAM(uint16_t, delayBox, newDelay, 1, 100)
			DIFF_PARAM(newDelay, delay)
			// Delay is diffenret, change it, and restart the generation if ou were encoding a gif
			generator->selectDelay = newDelay;
		auto fpsrate = 100 / generator->selectDelay;
		timer->Interval = generator->selectDelay * 10;
		delayLabel->Text = "Abort / FPS: " + fpsrate.ToString();
		if (generator->selectEncode == 2)
			QueueReset();//ResetGenerate();
	}
	System::Void GeneratorForm::prevButton_Click(System::Object^ sender, System::EventArgs^ e) {
		animated = false;
		const int b = generator->GetBitmapsFinished();
		currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + b - 1) % b;
	}
	System::Void GeneratorForm::nextButton_Click(System::Object^ sender, System::EventArgs^ e) {
		animated = false;
		const int b = generator->GetBitmapsFinished();
		currentBitmapIndex = b == 0 ? -1 : (currentBitmapIndex + 1) % b;
	}
	System::Void GeneratorForm::animateButton_Click(System::Object^ sender, System::EventArgs^ e) {
		animateButton->Text = (animated = !animated) ? "Playing" : "Paused";
	}
	System::Void GeneratorForm::encodeButton_Click(System::Object^ sender, System::EventArgs^ e) {
		switch (generator->selectEncode = (generator->selectEncode + 1) % 3) {
		case 0:
			// Only generates one image
			encodeButton->Text = "Only Image";
			break;
		case 1:
			// Generates an animation for you to see faster, but without encoding a Gif to export
			encodeButton->Text = "RAM Animation";
			break;
		case 2:
			// Full generation including GIF encoding
			encodeButton->Text = "Encode GIF";
			if (!generator->IsGifReady())
				QueueReset();
			break;
		}
	}
	System::Void GeneratorForm::helpButton_Click(System::Object^ sender, System::EventArgs^ e) {
		helpPanel->Visible = screenPanel->Visible;
		screenPanel->Visible = !screenPanel->Visible;
	}
	System::Void GeneratorForm::pngButton_Click(System::Object^ sender, System::EventArgs^ e) {
		savePng->ShowDialog();
	}
	System::Void GeneratorForm::gifButton_Click(System::Object^ sender, System::EventArgs^ e) {
		saveGif->ShowDialog();
	}
	System::Void GeneratorForm::screenPanel_Paint(System::Object^ sender, System::Windows::Forms::PaintEventArgs^ e) {
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
#pragma endregion

#pragma region Output
	System::Void GeneratorForm::savePng_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e) {
		Stream^ myStream;
		if ((myStream = savePng->OpenFile()) != nullptr) {
			currentBitmap->Save(myStream, System::Drawing::Imaging::ImageFormat::Png);
			myStream->Close();
		}
	}
	System::Void GeneratorForm::saveGif_FileOk(System::Object^ sender, System::ComponentModel::CancelEventArgs^ e) {
		gifButton->Enabled = false;
		if (!IsTaskNotRunning(gTask)) {
			cancel->Cancel();
			return;
		}
		gifPath = ((SaveFileDialog^)sender)->FileName;
		// Gif Export Task
		gTask = Task::Run(gcnew Action(this, &GeneratorForm::ExportGif), (cancel = gcnew CancellationTokenSource())->Token);
	}
	System::Void GeneratorForm::ExportGif() {
		//while (!cancel->Token.IsCancellationRequested && generator->SaveGif(gifPath))
		//	Thread::Sleep(1000);
		generator->SaveGif();
		while (!cancel->Token.IsCancellationRequested) {
			try {
				File::Move(gcnew String(generator->GetTempGif().c_str()), gifPath);
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
	// Using Marshal
	/*std::string GeneratorForm::ConvertToStdString(System::String^ managedString) {
		using namespace System::Runtime::InteropServices;
		const auto chars = (const char*)(Marshal::StringToHGlobalAnsi(managedString)).ToPointer();
		const auto nativeString(chars);
		Marshal::FreeHGlobal(System::IntPtr((void*)chars));
		return nativeString;
	}*/
#pragma endregion

}