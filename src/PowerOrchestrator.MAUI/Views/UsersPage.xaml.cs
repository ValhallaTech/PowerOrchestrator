#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

public partial class UsersPage : ContentPage
{
    public UsersPage(UsersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif