#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

public partial class AuditPage : ContentPage
{
    public AuditPage(AuditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif