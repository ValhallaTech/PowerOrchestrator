#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

/// <summary>
/// Repositories page for managing Git repositories
/// </summary>
public partial class RepositoriesPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoriesPage"/> class
    /// </summary>
    /// <param name="viewModel">The repositories view model</param>
    public RepositoriesPage(RepositoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif