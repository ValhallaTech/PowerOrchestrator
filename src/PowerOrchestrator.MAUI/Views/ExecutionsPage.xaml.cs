#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

public partial class ExecutionsPage : ContentPage
{
    public ExecutionsPage(ExecutionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif