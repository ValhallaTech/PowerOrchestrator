#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif