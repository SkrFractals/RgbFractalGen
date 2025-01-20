#pragma once
public ref class DoubleBufferedPanel : public System::Windows::Forms::Panel
{
public:
    // Constructor to enable double buffering
    DoubleBufferedPanel() {
        this->SetStyle(System::Windows::Forms::ControlStyles::DoubleBuffer |
            System::Windows::Forms::ControlStyles::UserPaint |
            System::Windows::Forms::ControlStyles::AllPaintingInWmPaint, true);
        this->UpdateStyles();
    }
};