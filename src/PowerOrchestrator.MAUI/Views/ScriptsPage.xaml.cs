#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

/// <summary>
/// Scripts page for managing PowerShell scripts
/// </summary>
public partial class ScriptsPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptsPage"/> class
    /// </summary>
    /// <param name="viewModel">The scripts view model</param>
    public ScriptsPage(ScriptsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif