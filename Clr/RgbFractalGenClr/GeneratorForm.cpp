// Starts the generator with special testing settings
//#define CUSTOMDEBUGTEST

#include "GeneratorForm.h"
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

namespace RgbFractalGenClr {

	using namespace System::IO;

#pragma region Designer
	GeneratorForm::GeneratorForm(void) {
		InitializeComponent();
		this->screenPanel = gcnew DoubleBufferedPanel();
		this->screenPanel->Location = System::Drawing::Point(239, 13);
		this->screenPanel->Name = L"screenPanel";
		this->screenPanel->Size = System::Drawing::Size(80, 80);
		this->screenPanel->TabIndex = 25;
		this->screenPanel->Click += gcnew System::EventHandler(this, &GeneratorForm::screenPanel_Click);
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
		this->ambBar = (gcnew System::Windows::Forms::TrackBar());
		this->noiseBar = (gcnew System::Windows::Forms::TrackBar());
		this->saturateBar = (gcnew System::Windows::Forms::TrackBar());
		this->detailBar = (gcnew System::Windows::Forms::TrackBar());
		this->threadsBar = (gcnew System::Windows::Forms::TrackBar());
		this->parallelBox = (gcnew System::Windows::Forms::CheckBox());
		this->parallelTypeBox = (gcnew System::Windows::Forms::CheckBox());
		this->pngButton = (gcnew System::Windows::Forms::Button());
		this->gifButton = (gcnew System::Windows::Forms::Button());
		this->blurBar = (gcnew System::Windows::Forms::TrackBar());
		this->timer = (gcnew System::Windows::Forms::Timer(this->components));
		this->savePng = (gcnew System::Windows::Forms::SaveFileDialog());
		this->saveGif = (gcnew System::Windows::Forms::SaveFileDialog());
		this->fractalLabel = (gcnew System::Windows::Forms::Label());
		this->delayLabel = (gcnew System::Windows::Forms::Label());
		this->ambLabel = (gcnew System::Windows::Forms::Label());
		this->noiseLabel = (gcnew System::Windows::Forms::Label());
		this->saturateLabel = (gcnew System::Windows::Forms::Label());
		this->detailLabel = (gcnew System::Windows::Forms::Label());
		this->threadsLabel = (gcnew System::Windows::Forms::Label());
		this->statusLabel = (gcnew System::Windows::Forms::Label());
		this->infoLabel = (gcnew System::Windows::Forms::Label());
		this->blurLabel = (gcnew System::Windows::Forms::Label());
		this->defaultZoom = (gcnew System::Windows::Forms::TextBox());
		this->defaultAngle = (gcnew System::Windows::Forms::TextBox());
		this->encodeButton = (gcnew System::Windows::Forms::Button());
		this->cutparamBar = (gcnew System::Windows::Forms::TrackBar());
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
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->ambBar))->BeginInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->noiseBar))->BeginInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->saturateBar))->BeginInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->detailBar))->BeginInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->threadsBar))->BeginInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->blurBar))->BeginInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->cutparamBar))->BeginInit();
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
		this->resX->Location = System::Drawing::Point(17, 182);
		this->resX->Name = L"resX";
		this->resX->Size = System::Drawing::Size(46, 23);
		this->resX->TabIndex = 4;
		this->resX->Text = L"1920";
		this->resX->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::resX_TextChanged);
		// 
		// resY
		// 
		this->resY->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
														static_cast<System::Byte>(238)));
		this->resY->Location = System::Drawing::Point(71, 182);
		this->resY->Name = L"resY";
		this->resY->Size = System::Drawing::Size(46, 23);
		this->resY->TabIndex = 5;
		this->resY->Text = L"1080";
		this->resY->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::resY_TextChanged);
		// 
		// previewBox
		// 
		this->previewBox->AutoSize = true;
		this->previewBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															  static_cast<System::Byte>(238)));
		this->previewBox->Location = System::Drawing::Point(125, 186);
		this->previewBox->Name = L"previewBox";
		this->previewBox->Size = System::Drawing::Size(101, 19);
		this->previewBox->TabIndex = 6;
		this->previewBox->Text = L"Preview Mode";
		this->previewBox->UseVisualStyleBackColor = true;
		this->previewBox->CheckedChanged += gcnew System::EventHandler(this, &GeneratorForm::previewBox_CheckedChanged);
		// 
		// periodBox
		// 
		this->periodBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															 static_cast<System::Byte>(238)));
		this->periodBox->Location = System::Drawing::Point(17, 211);
		this->periodBox->Name = L"periodBox";
		this->periodBox->Size = System::Drawing::Size(86, 23);
		this->periodBox->TabIndex = 7;
		this->periodBox->Text = L"120";
		this->periodBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::periodBox_TextChanged);
		// 
		// delayBox
		// 
		this->delayBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->delayBox->Location = System::Drawing::Point(18, 716);
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
		this->zoomButton->Location = System::Drawing::Point(17, 240);
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
		this->prevButton->Location = System::Drawing::Point(17, 753);
		this->prevButton->Name = L"prevButton";
		this->prevButton->Size = System::Drawing::Size(66, 27);
		this->prevButton->TabIndex = 24;
		this->prevButton->Text = L"<- Frame";
		this->prevButton->UseVisualStyleBackColor = true;
		this->prevButton->Click += gcnew System::EventHandler(this, &GeneratorForm::prevButton_Click);
		// 
		// nextButton
		// 
		this->nextButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->nextButton->Location = System::Drawing::Point(114, 753);
		this->nextButton->Name = L"nextButton";
		this->nextButton->Size = System::Drawing::Size(66, 27);
		this->nextButton->TabIndex = 25;
		this->nextButton->Text = L"Frame ->";
		this->nextButton->UseVisualStyleBackColor = true;
		this->nextButton->Click += gcnew System::EventHandler(this, &GeneratorForm::nextButton_Click);
		// 
		// animateButton
		// 
		this->animateButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->animateButton->Location = System::Drawing::Point(191, 753);
		this->animateButton->Name = L"animateButton";
		this->animateButton->Size = System::Drawing::Size(105, 27);
		this->animateButton->TabIndex = 26;
		this->animateButton->Text = L"Playing";
		this->animateButton->UseVisualStyleBackColor = true;
		this->animateButton->Click += gcnew System::EventHandler(this, &GeneratorForm::animateButton_Click);
		// 
		// ambBar
		// 
		this->ambBar->LargeChange = 2;
		this->ambBar->Location = System::Drawing::Point(129, 371);
		this->ambBar->Maximum = 30;
		this->ambBar->Name = L"ambBar";
		this->ambBar->Size = System::Drawing::Size(167, 45);
		this->ambBar->TabIndex = 15;
		this->ambBar->Value = 20;
		this->ambBar->Scroll += gcnew System::EventHandler(this, &GeneratorForm::ambBar_Scroll);
		// 
		// noiseBar
		// 
		this->noiseBar->LargeChange = 2;
		this->noiseBar->Location = System::Drawing::Point(129, 430);
		this->noiseBar->Maximum = 30;
		this->noiseBar->Name = L"noiseBar";
		this->noiseBar->Size = System::Drawing::Size(167, 45);
		this->noiseBar->TabIndex = 16;
		this->noiseBar->Value = 20;
		this->noiseBar->Scroll += gcnew System::EventHandler(this, &GeneratorForm::noiseBar_Scroll);
		// 
		// saturateBar
		// 
		this->saturateBar->LargeChange = 2;
		this->saturateBar->Location = System::Drawing::Point(129, 489);
		this->saturateBar->Name = L"saturateBar";
		this->saturateBar->Size = System::Drawing::Size(167, 45);
		this->saturateBar->TabIndex = 17;
		this->saturateBar->Value = 5;
		this->saturateBar->Scroll += gcnew System::EventHandler(this, &GeneratorForm::saturateBar_Scroll);
		// 
		// detailBar
		// 
		this->detailBar->LargeChange = 2;
		this->detailBar->Location = System::Drawing::Point(129, 548);
		this->detailBar->Maximum = 20;
		this->detailBar->Minimum = 1;
		this->detailBar->Name = L"detailBar";
		this->detailBar->Size = System::Drawing::Size(167, 45);
		this->detailBar->TabIndex = 18;
		this->detailBar->Value = 10;
		this->detailBar->Scroll += gcnew System::EventHandler(this, &GeneratorForm::detailBar_Scroll);
		// 
		// threadsBar
		// 
		this->threadsBar->LargeChange = 10;
		this->threadsBar->Location = System::Drawing::Point(125, 675);
		this->threadsBar->Maximum = 128;
		this->threadsBar->Name = L"threadsBar";
		this->threadsBar->Size = System::Drawing::Size(171, 45);
		this->threadsBar->TabIndex = 22;
		this->threadsBar->Scroll += gcnew System::EventHandler(this, &GeneratorForm::threadsBar_Scroll);
		// 
		// parallelBox
		// 
		this->parallelBox->AutoSize = true;
		this->parallelBox->Checked = true;
		this->parallelBox->CheckState = System::Windows::Forms::CheckState::Checked;
		this->parallelBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->parallelBox->Location = System::Drawing::Point(18, 650);
		this->parallelBox->Name = L"parallelBox";
		this->parallelBox->Size = System::Drawing::Size(125, 19);
		this->parallelBox->TabIndex = 20;
		this->parallelBox->Text = L"Parallel Generation";
		this->parallelBox->UseVisualStyleBackColor = true;
		this->parallelBox->CheckedChanged += gcnew System::EventHandler(this, &GeneratorForm::parallelBox_CheckedChanged);
		// 
		// parallelTypeBox
		// 
		this->parallelTypeBox->AutoSize = true;
		this->parallelTypeBox->Checked = true;
		this->parallelTypeBox->CheckState = System::Windows::Forms::CheckState::Checked;
		this->parallelTypeBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->parallelTypeBox->Location = System::Drawing::Point(149, 650);
		this->parallelTypeBox->Name = L"parallelTypeBox";
		this->parallelTypeBox->Size = System::Drawing::Size(89, 19);
		this->parallelTypeBox->TabIndex = 21;
		this->parallelTypeBox->Text = L"...Of Images";
		this->parallelTypeBox->UseVisualStyleBackColor = true;
		this->parallelTypeBox->CheckedChanged += gcnew System::EventHandler(this, &GeneratorForm::parallelTypeBox_CheckedChanged);
		// 
		// pngButton
		// 
		this->pngButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->pngButton->Location = System::Drawing::Point(114, 819);
		this->pngButton->Name = L"pngButton";
		this->pngButton->Size = System::Drawing::Size(66, 27);
		this->pngButton->TabIndex = 28;
		this->pngButton->Text = L"Save PNG";
		this->pngButton->UseVisualStyleBackColor = true;
		this->pngButton->Click += gcnew System::EventHandler(this, &GeneratorForm::pngButton_Click);
		// 
		// gifButton
		// 
		this->gifButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->gifButton->Location = System::Drawing::Point(191, 819);
		this->gifButton->Name = L"gifButton";
		this->gifButton->Size = System::Drawing::Size(105, 27);
		this->gifButton->TabIndex = 29;
		this->gifButton->Text = L"Save GIF";
		this->gifButton->UseVisualStyleBackColor = true;
		this->gifButton->Click += gcnew System::EventHandler(this, &GeneratorForm::gifButton_Click);
		// 
		// blurBar
		// 
		this->blurBar->LargeChange = 2;
		this->blurBar->Location = System::Drawing::Point(129, 599);
		this->blurBar->Minimum = 1;
		this->blurBar->Name = L"blurBar";
		this->blurBar->Size = System::Drawing::Size(167, 45);
		this->blurBar->TabIndex = 19;
		this->blurBar->Value = 1;
		this->blurBar->Scroll += gcnew System::EventHandler(this, &GeneratorForm::blurBar_Scroll);
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
		this->delayLabel->Location = System::Drawing::Point(91, 719);
		this->delayLabel->Name = L"delayLabel";
		this->delayLabel->Size = System::Drawing::Size(39, 15);
		this->delayLabel->TabIndex = 0;
		this->delayLabel->Text = L"Delay:";
		// 
		// ambLabel
		// 
		this->ambLabel->AutoSize = true;
		this->ambLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															static_cast<System::Byte>(238)));
		this->ambLabel->Location = System::Drawing::Point(17, 371);
		this->ambLabel->Name = L"ambLabel";
		this->ambLabel->Size = System::Drawing::Size(82, 15);
		this->ambLabel->TabIndex = 0;
		this->ambLabel->Text = L"Void Ambient:";
		// 
		// noiseLabel
		// 
		this->noiseLabel->AutoSize = true;
		this->noiseLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->noiseLabel->Location = System::Drawing::Point(17, 430);
		this->noiseLabel->Name = L"noiseLabel";
		this->noiseLabel->Size = System::Drawing::Size(66, 15);
		this->noiseLabel->TabIndex = 0;
		this->noiseLabel->Text = L"Void Noise:";
		// 
		// saturateLabel
		// 
		this->saturateLabel->AutoSize = true;
		this->saturateLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->saturateLabel->Location = System::Drawing::Point(17, 489);
		this->saturateLabel->Name = L"saturateLabel";
		this->saturateLabel->Size = System::Drawing::Size(86, 15);
		this->saturateLabel->TabIndex = 0;
		this->saturateLabel->Text = L"Super Saturate:";
		// 
		// detailLabel
		// 
		this->detailLabel->AutoSize = true;
		this->detailLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->detailLabel->Location = System::Drawing::Point(17, 548);
		this->detailLabel->Name = L"detailLabel";
		this->detailLabel->Size = System::Drawing::Size(40, 15);
		this->detailLabel->TabIndex = 0;
		this->detailLabel->Text = L"Detail:";
		// 
		// threadsLabel
		// 
		this->threadsLabel->AutoSize = true;
		this->threadsLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->threadsLabel->Location = System::Drawing::Point(13, 675);
		this->threadsLabel->Name = L"threadsLabel";
		this->threadsLabel->Size = System::Drawing::Size(77, 15);
		this->threadsLabel->TabIndex = 0;
		this->threadsLabel->Text = L"Max Threads:";
		// 
		// statusLabel
		// 
		this->statusLabel->AutoSize = true;
		this->statusLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->statusLabel->Location = System::Drawing::Point(11, 783);
		this->statusLabel->Name = L"statusLabel";
		this->statusLabel->Size = System::Drawing::Size(64, 15);
		this->statusLabel->TabIndex = 0;
		this->statusLabel->Text = L"Initializing:";
		// 
		// infoLabel
		// 
		this->infoLabel->AutoSize = true;
		this->infoLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->infoLabel->Location = System::Drawing::Point(114, 783);
		this->infoLabel->Name = L"infoLabel";
		this->infoLabel->Size = System::Drawing::Size(28, 15);
		this->infoLabel->TabIndex = 0;
		this->infoLabel->Text = L"info";
		// 
		// blurLabel
		// 
		this->blurLabel->AutoSize = true;
		this->blurLabel->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->blurLabel->Location = System::Drawing::Point(17, 599);
		this->blurLabel->Name = L"blurLabel";
		this->blurLabel->Size = System::Drawing::Size(73, 15);
		this->blurLabel->TabIndex = 0;
		this->blurLabel->Text = L"Motion Blur:";
		// 
		// defaultZoom
		// 
		this->defaultZoom->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
															   static_cast<System::Byte>(238)));
		this->defaultZoom->Location = System::Drawing::Point(109, 243);
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
		this->defaultAngle->Location = System::Drawing::Point(175, 293);
		this->defaultAngle->Name = L"defaultAngle";
		this->defaultAngle->Size = System::Drawing::Size(121, 23);
		this->defaultAngle->TabIndex = 13;
		this->defaultAngle->Text = L"0";
		this->defaultAngle->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::defaultAngle_TextChanged);
		// 
		// encodeButton
		// 
		this->encodeButton->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9));
		this->encodeButton->Location = System::Drawing::Point(191, 786);
		this->encodeButton->Name = L"encodeButton";
		this->encodeButton->Size = System::Drawing::Size(105, 27);
		this->encodeButton->TabIndex = 27;
		this->encodeButton->Text = L"Encode GIF";
		this->encodeButton->UseVisualStyleBackColor = true;
		this->encodeButton->Click += gcnew System::EventHandler(this, &GeneratorForm::encodeButton_Click);
		// 
		// cutparamBar
		// 
		this->cutparamBar->LargeChange = 1;
		this->cutparamBar->Location = System::Drawing::Point(20, 130);
		this->cutparamBar->Maximum = 1;
		this->cutparamBar->Name = L"cutparamBar";
		this->cutparamBar->Size = System::Drawing::Size(276, 45);
		this->cutparamBar->TabIndex = 3;
		this->cutparamBar->Scroll += gcnew System::EventHandler(this, &GeneratorForm::cutparamBar_Scroll);
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
		this->defaultHue->Location = System::Drawing::Point(175, 342);
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
		this->periodMultiplierBox->Location = System::Drawing::Point(109, 211);
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
		this->periodLabel->Location = System::Drawing::Point(174, 214);
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
		this->helpPanel->Size = System::Drawing::Size(763, 763);
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
		this->helpButton->Location = System::Drawing::Point(16, 819);
		this->helpButton->Name = L"helpButton";
		this->helpButton->Size = System::Drawing::Size(66, 27);
		this->helpButton->TabIndex = 35;
		this->helpButton->Text = L"Help";
		this->helpButton->UseVisualStyleBackColor = true;
		this->helpButton->Click += gcnew System::EventHandler(this, &GeneratorForm::helpButton_Click);
		// 
		// angleLabel
		// 
		this->angleLabel->AutoSize = true;
		this->angleLabel->Location = System::Drawing::Point(15, 47);
		this->angleLabel->Name = L"angleLabel";
		this->angleLabel->Size = System::Drawing::Size(42, 13);
		this->angleLabel->TabIndex = 36;
		this->angleLabel->Text = L"Angles:";
		// 
		// colorLabel
		// 
		this->colorLabel->AutoSize = true;
		this->colorLabel->Location = System::Drawing::Point(15, 76);
		this->colorLabel->Name = L"colorLabel";
		this->colorLabel->Size = System::Drawing::Size(36, 13);
		this->colorLabel->TabIndex = 37;
		this->colorLabel->Text = L"Colors";
		// 
		// cutLabel
		// 
		this->cutLabel->AutoSize = true;
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
		this->zoomLabel->Location = System::Drawing::Point(175, 246);
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
		this->spinSelect->Location = System::Drawing::Point(18, 293);
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
		this->hueSelect->Location = System::Drawing::Point(18, 342);
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
		this->spinLabel->Location = System::Drawing::Point(21, 274);
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
		this->label1->Location = System::Drawing::Point(17, 324);
		this->label1->Name = L"label1";
		this->label1->Size = System::Drawing::Size(246, 15);
		this->label1->TabIndex = 45;
		this->label1->Text = L"Hue Select           HueSpeed           Default Hue";
		// 
		// spinSpeedBox
		// 
		this->spinSpeedBox->Font = (gcnew System::Drawing::Font(L"Segoe UI", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
																static_cast<System::Byte>(238)));
		this->spinSpeedBox->Location = System::Drawing::Point(109, 293);
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
		this->hueSpeedBox->Location = System::Drawing::Point(109, 342);
		this->hueSpeedBox->Name = L"hueSpeedBox";
		this->hueSpeedBox->Size = System::Drawing::Size(60, 23);
		this->hueSpeedBox->TabIndex = 47;
		this->hueSpeedBox->Text = L"0";
		this->hueSpeedBox->TextChanged += gcnew System::EventHandler(this, &GeneratorForm::hueSpeedBox_TextChanged);
		// 
		// GeneratorForm
		// 
		this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
		this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
		this->ClientSize = System::Drawing::Size(1079, 876);
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
		this->Controls->Add(this->cutparamBar);
		this->Controls->Add(this->encodeButton);
		this->Controls->Add(this->defaultAngle);
		this->Controls->Add(this->defaultZoom);
		this->Controls->Add(this->blurLabel);
		this->Controls->Add(this->blurBar);
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
		this->Controls->Add(this->ambLabel);
		this->Controls->Add(this->ambBar);
		this->Controls->Add(this->noiseLabel);
		this->Controls->Add(this->noiseBar);
		this->Controls->Add(this->saturateLabel);
		this->Controls->Add(this->saturateBar);
		this->Controls->Add(this->detailLabel);
		this->Controls->Add(this->detailBar);
		this->Controls->Add(this->threadsLabel);
		this->Controls->Add(this->threadsBar);
		this->Controls->Add(this->parallelBox);
		this->Controls->Add(this->parallelTypeBox);
		this->Controls->Add(this->statusLabel);
		this->Controls->Add(this->infoLabel);
		this->Name = L"GeneratorForm";
		this->Text = L"RGB Fractal Zoom Generator Clr v1.73";
		this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &GeneratorForm::GeneratorForm_FormClosing);
		this->Load += gcnew System::EventHandler(this, &GeneratorForm::GeneratorForm_Load);
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->ambBar))->EndInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->noiseBar))->EndInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->saturateBar))->EndInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->detailBar))->EndInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->threadsBar))->EndInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->blurBar))->EndInit();
		(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->cutparamBar))->EndInit();
		this->helpPanel->ResumeLayout(false);
		this->helpPanel->PerformLayout();
		this->ResumeLayout(false);
		this->PerformLayout();

	}
#pragma endregion

#pragma region Core
	System::Void GeneratorForm::GeneratorForm_Load(System::Object^ sender, System::EventArgs^ e) {
		// Read the REDME.txt for the help button
		if (File::Exists("README.txt"))
			helpLabel->Text = File::ReadAllText("README.txt");
		helpPanel->Visible = false;

		// Setupd interactable controls (tooltips + tabIndex)
		SetupControl(fractalSelect, L"Select the type of fractal to generate");
		SetupControl(angleSelect, L"Select the children angles definition.");
		SetupControl(colorSelect, L"Select the children colors definition.");
		SetupControl(cutSelect, L"Select the cutfunction definition.");
		SetupControl(cutparamBar, L"Select the cutfunction seed.");
		SetupControl(cutparamBox, L"Type the cutfunction seed.");
		SetupControl(resX, L"Type the X resolution of the render (width)");
		SetupControl(resY, L"Type the Y resolution of the render (height)");
		SetupControl(previewBox, L"If checked, the resolution will be only 80x80 for fast preview render.\nUncheck it when you are done with preparing the settings and want to render it in full resolution");
		SetupControl(periodBox, L"How many frames for the self-similar center child to zoom in to the same size as the parent if you are zooming in.\nOr for the parent to zoom out to the same size as the child if you are zooming out.\nThis will equal the number of generated frames of the animation if the center child is the same color.\nOr it will be a third of the number of generated frames of the animation if the center child is a different color.");
		SetupControl(periodMultiplierBox, L"Multiplies the frame count, slowing down the rotaion and hue shifts.");
		SetupControl(zoomButton, L"Toggle in which direction you want the fractal zoom -> ZoomIn, or <- ZoomOut");
		SetupControl(defaultZoom, L"Type the initial zoom of the first image (in number of skipped frames).");
		SetupControl(spinSelect, L"Choose in which direction you want the zoom animation to spin, or to not spin.");
		SetupControl(spinSpeedBox, L"Type the extra speed on the spinning from the values possible for looping.");
		SetupControl(defaultAngle, L"Type the initial angle of the first image (in degrees).");
		SetupControl(hueSelect, L"Choose the color scheme of the fractal.\nAlso you can make the color hues slowly cycle as the fractal zoom.");
		SetupControl(hueSpeedBox, L"Type the extra speed on the hue shifting from the values possible for looping.\nOnly possible if you have chosen color cycling on the left.");
		SetupControl(defaultHue, L"Type the initial hue angle of the first image (in degrees).");
		SetupControl(defaultAngle, L"Type the initial angle of the first image (in degrees).");
		SetupControl(defaultHue, L"Type the initial hue angle of the first image (in degrees).");
		SetupControl(ambBar, L"The strength of the ambient grey color in the empty spaces far away between the generated fractal dots.");
		SetupControl(noiseBar, L"The strength of the random noise in the empty spaces far away between the generated fractal dots.");
		SetupControl(saturateBar, L"Enhance the color saturation of the fractal dots.\nUseful when the fractal is too gray, like the Sierpinski Triangle (Triflake).");
		SetupControl(detailBar, L"Level of Detail (The lower the finer).");
		SetupControl(blurBar, L"Motion blur: Lowest no blur and fast generation, highest 10 smeared frames 10 times slower generation.");
		SetupControl(parallelBox, L"Enable parallelism (and then tune with the Max Threads slider).\nSelect the type of parallelism with the followinf checkBox to the right.");
		SetupControl(parallelTypeBox, L"Type of parallelism to be used if the left checkBox is enabled.\n...Of Images = parallel single image generation, recommmended for fast single images, 1 in a million pixels might be slightly wrong\n...Of Animation Frames = batching animation frames, recommended for Animations with perfect pixels.");
		SetupControl(threadsBar, L"The maximum allowed number of parallel CPU threads for either generation or drawing.\nAt least one of the parallel check boxes below must be checked for this to apply.\nTurn it down from the maximum if your system is already busy elsewhere, or if you want some spare CPU threads for other stuff.\nThe generation should run the fastest if you tune this to the exact number of free available CPU threads.\nThe maximum on this slider is the number of all CPU threads, but not only the free ones.");
		SetupControl(delayBox, L"A delay between frames in 1/100 of seconds for the preview and exported GIF file.\nThe framerate will be roughly 100/delay");
		SetupControl(prevButton, L"Stop the animation and move to the previous frame.\nUseful for selecting the exact frame you want to export to PNG file.");
		SetupControl(nextButton, L"Stop the animation and move to the next frame.\nUseful for selecting the exact frame you want to export to PNG file.");
		SetupControl(animateButton, L"Toggle preview animation.\nWill seamlessly loop when the fractal is finished generating.\nClicking on the image does the same thing.");
		SetupControl(encodeButton, L"Only Image - only generates one image\nAnimation RAM - generated an animation without GIF encoding, faster but can't save GIF afterwards\nEncode GIF - encodes GIF while generating an animation - can save a GIF afterwards");
		SetupControl(pngButton, L"Save the currently displayed frame into a PNG file.\nStop the animation and select the frame you wish to export with the buttons above.");
		SetupControl(gifButton, L"Save the full animation into a GIF file.");

		// Update Input fields to default values
		generator = gcnew FractalGenerator();
		for (Fractal ** i = generator->GetFractals(); *i != nullptr; ++i)
			fractalSelect->Items->Add(gcnew String(((*i)->name).c_str()));
		SelectMaxThreads();
		fractalSelect->SelectedIndex = 0;
		auto maxThreads = Environment::ProcessorCount - 2;
		threadsBar->Maximum = maxThreads;
		threadsBar->Value = maxThreads;
		SelectMaxThreads();
		periodBox_TextChanged(nullptr, nullptr);
		periodMultiplierBox_TextChanged(nullptr, nullptr);
		SelectParallelType();
		delayBox_TextChanged(nullptr, nullptr);
		defaultZoom_TextChanged(nullptr, nullptr);
		spinSpeedBox_TextChanged(nullptr, nullptr);
		hueSpeedBox_TextChanged(nullptr, nullptr);
		defaultHue_TextChanged(nullptr, nullptr);
		hueSelect->SelectedIndex = 0;
		ambBar_Scroll(nullptr, nullptr);
		noiseBar_Scroll(nullptr, nullptr);
		blurBar_Scroll(nullptr, nullptr);
		saturateBar_Scroll(nullptr, nullptr);

		// Setup bitmap and start generation
		modifySettings = false;
		ResizeAll();

		generator->StartGenerate();
	}
	System::Void GeneratorForm::timer_Tick(System::Object^ sender, System::EventArgs^ e) {
		const auto gTaskNotRunning = gTask == nullptr || gTask->IsCanceled || gTask->IsCompleted || gTask->IsFaulted;
		// Fetch the state of generated bitmaps
		const auto b = generator->GetBitmapsFinished(), bt = generator->GetBitmapsTotal();
		if (bt <= 0)
			return;
		// Only Allow GIF Export when generation is finished
		gifButton->Enabled = generator->IsGifReady() && gTaskNotRunning;
		if (b > 0) {
			// Fetch bitmap, make sure the index is is range
			currentBitmapIndex = currentBitmapIndex % b;
			Bitmap^ bitmap = generator->GetBitmap(currentBitmapIndex);
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
		statusLabel->Text = b < bt ? "Generating: " : "Finished: ";
		infoLabel->Text = (b < bt ? b.ToString() : currentBitmapIndex.ToString()) + " / " + bt.ToString();
		gifButton->Text = gTaskNotRunning ? "Save GIF" : "Saving GIF...";
		// Window Size Update
		WindowSizeRefresh();
	}
	System::Void GeneratorForm::GeneratorForm_FormClosing(System::Object^ sender, System::Windows::Forms::FormClosingEventArgs^ e) {
		Abort();
	}
	System::Void GeneratorForm::Abort() {
		if (modifySettings)
			return;
		// Cancel gTask
		if (cancel != nullptr)
			cancel->Cancel();
		if (gTask != nullptr && !(gTask->IsCanceled || gTask->IsCompleted || gTask->IsFaulted))
			gTask->Wait();
		// Cancel FractalGenerator threads
		generator->RequestCancel();
	}
	System::Void GeneratorForm::ResetGenerator() {
		if (modifySettings)
			return;
		// Resets the generator
		// (Abort should be called before this or else it will crash)
		// generator->StartGenerate(); should be called after
		gifButton->Enabled = false;
		currentBitmapIndex = 0;
		generator->ResetGenerator();
		
	}
	System::Void GeneratorForm::ResetGenerate() {
		if (modifySettings)
			return;
		// Just restart the generator without resizing
		// (Abort should be called before this or else it will crash)
		ResetGenerator();
		generator->StartGenerate();
	}
	System::Void GeneratorForm::ResizeGenerate() {
		if (modifySettings)
			return;
		// Resize and restard generator (Abort should be called before this or else it will crash)
		ResizeAll();
		generator->StartGenerate();
	}
	System::Void GeneratorForm::AbortGenerate() {
		if (modifySettings)
			return;
		// Just Abort and regenerate with nothing inbetween
		Abort();
		ResizeGenerate();
	}
	System::Void GeneratorForm::SetupControl(Control^ control, System::String^ tip) {
		// Add tooltip and set the next tabIndex
		toolTips->SetToolTip(control, tip);
		control->TabIndex = ++controlTabIndex;
	}
#pragma endregion

#pragma region Size
	System::Void GeneratorForm::ResizeAll() {
		TryResize();
		// Update the size of the window and display
		SetMinimumSize();
		SetClientSizeCore(
			generator->width + 314,
			Math::Max(generator->height + 8, 320)
		);
		ResizeScreen();
		WindowSizeRefresh();
#ifdef CUSTOMDEBUGTEST
		generator->DebugStart(); animated = false;
#endif
		ResetGenerator();
		SizeAdapt();
	}
	bool GeneratorForm::TryResize() {
		previewMode = !previewBox->Checked;
		uint16_t w = 8, h = 8;
		if (!uint16_t::TryParse(resX->Text, w) || w <= 8)
			w = 8;
		if (!uint16_t::TryParse(resY->Text, h) || h <= 0)
			h = 8;
		previewBox->Text = "Resolution: " + w.ToString() + "x" + h.ToString();
		if (previewMode)
			w = h = 80;
		if (generator->width == w && generator->height == h)
			return false;
		// resoltion is changed - request the fractal to resize the buffer and restart generation
		generator->width = w;
		generator->height = h;
		return true;
	}
	System::Void GeneratorForm::WindowSizeRefresh() {
		if (fx == Width && fy == Height)
			return;
		// User has manually resized the window - strech the display
		ResizeScreen();
		SetMinimumSize();
		int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
		SetClientSizeCore(
			Math::Max(Width - bw, 314 + Math::Max(screenPanel->Width, (int)generator->width)),
			Math::Max(Height - bh, 8 + Math::Max(screenPanel->Height, (int)generator->height))
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
			Math::Max(1100, bw + generator->width + 284),
			Math::Max(900, bh + Math::Max(460, generator->height + 8))
		);
	}
	System::Void GeneratorForm::ResizeScreen() {
		int bw = 16, bh = 39; // Have to do this because for some ClientSize was returning bullshit values all of a sudden
		const auto screenHeight = Math::Max((int)generator->height, Math::Min(Height - bh - 8, (Width - bw - 314) * (int)generator->height / (int)generator->width));
		screenPanel->SetBounds(305, 4, screenHeight * generator->width / generator->height, screenHeight);
		screenPanel->Invalidate();
	}
#pragma endregion

#pragma region Input
	System::Void GeneratorForm::fractalBox_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectFractal(Math::Max(0, fractalSelect->SelectedIndex)))
			return;
		// Fractal is different - load it, change the setting and restart generation
		Abort();
		generator->SetupFractal();
		// Fill the fractal's adjuistable definition combos
		if (!modifySettings) {
			modifySettings = true;
			FillSelects();
			detailBar_Scroll(nullptr, nullptr);
			modifySettings = false;
		} else FillSelects();
		// Fill the fractal's adjuistable cutfunction seed combos, and restart generation
		FillCutParams();
		ResetGenerate();
	}
#define LoadCombo(S,A,V) S->Items->Clear();auto V = generator->GetFractal()->A;S->Enabled = V != nullptr;if (S->Enabled) {while (V != nullptr && V->first != "")S->Items->Add(gcnew String(((V++)->first).c_str()));S->Enabled = S->Items->Count > 0;if (S->Enabled)S->SelectedIndex = 0;};
	System::Void GeneratorForm::FillSelects() {	
		// Fill angle childred definitnions combobox
		LoadCombo(angleSelect, childAngle, a);
		// Fill color children definitnions combobox
		LoadCombo(colorSelect, childColor, c);
		// Fill cutfunction definitnions combobox
		LoadCombo(cutSelect, cutFunction, f);
	}
	System::Void GeneratorForm::angleSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectAngle(Math::Max(0, angleSelect->SelectedIndex)))
			return;
		// Angle children definition is different - change the setting and restart generation
		AbortGenerate();
	}
	System::Void GeneratorForm::colorSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectColor(Math::Max(0, colorSelect->SelectedIndex)))
			return;
		// Color children definition is different - change the setting and restart generation
		Abort();
		generator->SelectColor();
		ResetGenerate();
	}
	System::Void GeneratorForm::cutSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		if (generator->SelectCutFunction(Math::Max(0, cutSelect->SelectedIndex)))
			return;
		// Cutfunction is different - change the setting and restart generation
		Abort();
		FillCutParams();
		ResetGenerate();
	}
	System::Void GeneratorForm::FillCutParams() {
		const auto cf = generator->GetCutFunction();
		// query the number of seedss from the CutFunction
		const auto cm = cf == nullptr || (*cf)(0, -1) <= 0 ? 0 : ((*cf)(0, 1 - (1 << 16)) + 1) / (*cf)(0, -1);
		// set the maximum of the trackBar for the seed to that value
		if (modifySettings) {
			cutparamBar->Maximum = cm;
			cutparamBox->Text = "0";
			cutparamBar->Value = 0;
		} else {
			modifySettings = true;
			cutparamBar->Maximum = cm;
			cutparamBox->Text = "0";
			cutparamBar->Value = 0;
			modifySettings = false;
		}
		cutparamBar->Enabled = cutparamBar->Maximum > 0;
	}
	System::Void GeneratorForm::cutparamBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		uint16_t newcutparam;
		if (!uint16_t::TryParse(cutparamBox->Text, newcutparam) || newcutparam < 0 || newcutparam > cutparamBar->Maximum)
			newcutparam = 0;
		if (newcutparam == generator->cutparam)
			return;
		// Cutfunction seed is different - change the setting and restart generation
		Abort();
		// update the value in the trackBar for the seed
		if (modifySettings) {
			cutparamBar->Value = newcutparam;
		} else {
			modifySettings = true;
			cutparamBar->Value = newcutparam;
			modifySettings = false;
		}
		generator->cutparam = newcutparam;
		ResetGenerate();
	}
	System::Void GeneratorForm::cutparamBar_Scroll(System::Object^ sender, System::EventArgs^ e) {
		uint16_t v = cutparamBar->Value;
		if (v == generator->cutparam)
			return;
		// Cutfunction seed is different - change the setting and restart generation
		Abort();
		// update the value in the text box for the seed
		if (modifySettings) {
			cutparamBox->Text = v.ToString();
		} else {
			modifySettings = true;
			cutparamBox->Text = v.ToString();
			modifySettings = false;
		}
		generator->cutparam = v;
		ResetGenerate();
	}
	System::Void GeneratorForm::resX_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		if (TryResize())
			AbortGenerate();
	}
	System::Void GeneratorForm::resY_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		if (TryResize())
			AbortGenerate();
	}
	System::Void GeneratorForm::previewBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e) {
		if (TryResize())
			AbortGenerate();
	}
	System::Void GeneratorForm::periodBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		uint16_t newPeriod = 1;
		if (!uint16_t::TryParse(periodBox->Text, newPeriod) || newPeriod <= 0)
			newPeriod = 120;
		if (generator->period == newPeriod)
			return;
		// period is different - change the setting and restart generation
		Abort();
		generator->period = newPeriod;
		ResetGenerate();
	}
	System::Void GeneratorForm::periodMultiplierBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		uint16_t newPeriod = 1;
		if (!uint16_t::TryParse(periodMultiplierBox->Text, newPeriod) || newPeriod  <= 1)
			newPeriod = 1;
		if (generator->periodMultiplier == newPeriod)
			return;
		// period is different - change the setting and restart generation
		Abort();
		generator->periodMultiplier = newPeriod;
		ResetGenerate();
	}
	System::Void GeneratorForm::zoomButton_Click(System::Object^ sender, System::EventArgs^ e) {
		// zoom is different - change the setting and restart generation
		Abort();
		generator->zoom = -generator->zoom;
		zoomButton->Text = "Zoom: " + ((generator->zoom > 0) ? "->" : "<-");
		ResetGenerate();
	}
	System::Void GeneratorForm::defaultZoom_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		int16_t newZoom = 0;
		if (!int16_t::TryParse(defaultZoom->Text, newZoom))
			newZoom = 0;
		int finalPeriod = generator->period * generator->GetFinalPeriod();
		while (newZoom < 0)
			newZoom += finalPeriod;
		while (newZoom >= finalPeriod)
			newZoom -= finalPeriod;
		if (generator->defaultZoom == newZoom)
			return;
		// default zoom is different - change the setting and restart generation
		Abort();
		generator->defaultZoom = newZoom;
		ResetGenerate();
	}
	System::Void GeneratorForm::spinSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		const auto newSpin = Math::Max(0, Math::Min(4, spinSelect->SelectedIndex)) - 2;
		if (generator->defaultSpin == newSpin)
			return;
		// spin type is different - change the setting and restart generation
		Abort();
		generator->defaultSpin = newSpin;
		ResetGenerate();
	}
	System::Void GeneratorForm::spinSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		uint8_t newSpeed = 0;
		if (!uint8_t::TryParse(spinSpeedBox->Text, newSpeed) || newSpeed < 0)
			newSpeed = 0;
		if (generator->extraSpin == newSpeed)
			return;
		// spin speed is different - change the setting and restart generation
		Abort();
		generator->extraSpin = newSpeed;
		ResetGenerate();
	}
	System::Void GeneratorForm::defaultAngle_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		int16_t newAngle = 0;
		if (!int16_t::TryParse(defaultAngle->Text, newAngle))
			newAngle = 0;
		while (newAngle < 0)
			newAngle += 360;
		while (newAngle >= 360)
			newAngle -= 360;
		if (generator->defaultAngle == newAngle)
			return;
		// angle is different - change the setting and restart generation
		Abort();
		generator->defaultAngle = newAngle;
		ResetGenerate();
	}
	System::Void GeneratorForm::hueSelect_SelectedIndexChanged(System::Object^ sender, System::EventArgs^ e) {
		uint8_t colorChoice = (uint8_t)((hueSelect->SelectedIndex) % 6);
		int8_t newHueCycle = ((int8_t)(colorChoice) / 2 + 1) % 3 - 1;
		if (generator->SelectColorPalette(colorChoice % 2) && newHueCycle == generator->hueCycle)
			return;
		Abort();
		// hue is different - change the setting and restart generation
		generator->hueCycle = newHueCycle;
		generator->SelectColor();
		ResetGenerate();
	}
	System::Void GeneratorForm::hueSpeedBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		uint8_t newSpeed = 0;
		if (!uint8_t::TryParse(hueSpeedBox->Text, newSpeed) || newSpeed < 0)
			newSpeed = 0;
		if (generator->extraHue == newSpeed)
			return;
		// hue speed is different - change the setting and if it's actually huecycling restart generation
		if (generator->hueCycle != 0) {
			Abort();
			generator->extraHue = newSpeed;
			ResetGenerate();
		} else
			generator->extraHue = newSpeed;
	}
	System::Void GeneratorForm::defaultHue_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		uint16_t newHue = 0;
		if (!uint16_t::TryParse(defaultHue->Text, newHue))
			newHue = 0;
		while (newHue < 0)
			newHue += 360;
		while (newHue >= 360)
			newHue -= 360;
		if (generator->defaultHue == newHue)
			return;
		Abort();
		// Hue is different - change the setting and restart generation
		generator->defaultHue = newHue;
		ResetGenerate();
	}
	System::Void GeneratorForm::ambBar_Scroll(System::Object^ sender, System::EventArgs^ e) {
		const auto newAmb = (uint16_t)(ambBar->Value * 4);
		if (generator->amb == newAmb)
			return;
		Abort();
		// Ambient is different - change the setting and restart generation
		generator->amb = newAmb;
		ResetGenerate();
	}
	System::Void GeneratorForm::noiseBar_Scroll(System::Object^ sender, System::EventArgs^ e) {
		const auto newNoise = noiseBar->Value * .1f;
		if (generator->noise == newNoise)
			return;
		Abort();
		// Noise is different - change the setting and restart generation
		generator->noise = newNoise;
		ResetGenerate();
	}
	System::Void GeneratorForm::saturateBar_Scroll(System::Object^ sender, System::EventArgs^ e) {
		const auto newSat = saturateBar->Value * .1f;
		if (generator->saturate == newSat)
			return;
		Abort();
		// Saturation is different - change the setting and restart generation
		generator->saturate = newSat;
		ResetGenerate();
	}
	System::Void GeneratorForm::detailBar_Scroll(System::Object^ sender, System::EventArgs^ e) {
		const auto newDetail = detailBar->Value * .1f;
		if (generator->SelectDetail(newDetail))
			return;
		// Detail is different - change the setting and restart generation
		AbortGenerate();
	}
	System::Void GeneratorForm::blurBar_Scroll(System::Object^ sender, System::EventArgs^ e) {
		if (generator->selectBlur == blurBar->Value)
			return;
		Abort();
		// Blur is different - change the setting and restart generation
		generator->selectBlur = blurBar->Value;
		ResetGenerate();
	}
	System::Void GeneratorForm::parallelBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e) {
		SelectMaxThreads();
		generator->SelectThreadingDepth();
	}
	System::Void GeneratorForm::SelectMaxThreads() {
		generator->maxTasks = (int16_t)(parallelBox->Checked ? threadsBar->Value : -1);
		generator->maxGenerationTasks = generator->maxTasks - 1;
	}
	System::Void GeneratorForm::parallelTypeBox_CheckedChanged(System::Object^ sender, System::EventArgs^ e) {
		SelectParallelType();
	}
	System::Void GeneratorForm::SelectParallelType() {
		parallelTypeBox->Text = (generator->parallelType = parallelTypeBox->Checked) ? "...of Images" : "...of Animation Frames";
	}
	System::Void GeneratorForm::threadsBar_Scroll(System::Object^ sender, System::EventArgs^ e) {
		SelectMaxThreads();
		generator->SelectThreadingDepth();
	}
	System::Void GeneratorForm::delayBox_TextChanged(System::Object^ sender, System::EventArgs^ e) {
		uint16_t newDelay = 1;
		if (!uint16_t::TryParse(delayBox->Text, newDelay) || newDelay <= 0)
			newDelay = 5;
		if (generator->delay == newDelay)
			return;
		if (generator->encode == 2)
			Abort();
		// Delay is diffenret, change it, and restart the generation if ou were encoding a gif
		generator->delay = newDelay;
		int fpsrate = 100 / generator->delay;
		timer->Interval = generator->delay * 10;
		delayLabel->Text = "Delay: " + (generator->delay * 10).ToString() + "ms, Framerate: " + fpsrate.ToString();
		if (generator->encode == 2)
			ResetGenerate();
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
		switch (generator->encode = (generator->encode + 1) % 3) {
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
				AbortGenerate();
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
	System::Void GeneratorForm::screenPanel_Click(System::Object^ sender, System::EventArgs^ e) {
		animated = !animated;
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
		gifPath = ((SaveFileDialog^)sender)->FileName;
		// Gif Export Task
		gTask = Task::Run(gcnew Action(this, &GeneratorForm::ExportGif), (cancel = gcnew CancellationTokenSource())->Token);
	}
	System::Void GeneratorForm::ExportGif() {
		int attempt = 0;
		while (++attempt < 10 && !cancel->Token.IsCancellationRequested && generator->SaveGif(gifPath))
			Thread::Sleep(1000);

		/*while (!cancel->Token.IsCancellationRequested) {
			try {
				File::Move("gif.tmp", gifPath);
			}
			catch (IOException^ ex) {
				Console::WriteLine("An error occurred: {0}", ex->Message);
				System::Threading::Thread::Sleep(1000);
				continue;
			}
			catch (UnauthorizedAccessException^ ex) {
				Console::WriteLine("Access denied: {0}", ex->Message);
				System::Threading::Thread::Sleep(1000);
				continue;
			}
			catch (Exception^ ex) {
				Console::WriteLine("Unexpected error: {0}", ex->Message);
				System::Threading::Thread::Sleep(1000);
				continue;
			}
			break;
		}*/
	}
	/*std::string GeneratorForm::ConvertToStdString(System::String^ managedString) {
		using namespace System::Runtime::InteropServices;
		const auto chars = (const char*)(Marshal::StringToHGlobalAnsi(managedString)).ToPointer();
		const auto nativeString(chars);
		Marshal::FreeHGlobal(System::IntPtr((void*)chars));
		return nativeString;
	}*/
#pragma endregion

}