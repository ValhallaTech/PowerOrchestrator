#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

public partial class RolesPage : ContentPage
{
    public RolesPage(RolesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif