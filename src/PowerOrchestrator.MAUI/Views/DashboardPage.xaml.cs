#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

/// <summary>
/// Dashboard page showing system overview and quick actions
/// </summary>
public partial class DashboardPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardPage"/> class
    /// </summary>
    /// <param name="viewModel">The dashboard view model</param>
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif