#if !NET8_0
using PowerOrchestrator.MAUI.ViewModels;

namespace PowerOrchestrator.MAUI.Views;

/// <summary>
/// Login page for user authentication
/// </summary>
public partial class LoginPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPage"/> class
    /// </summary>
    /// <param name="viewModel">The login view model</param>
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Setup keyboard navigation and accessibility
        SetupKeyboardNavigation();
        SetupAccessibilityFeatures();
    }

    /// <summary>
    /// Sets up keyboard navigation for the login form
    /// </summary>
    private void SetupKeyboardNavigation()
    {
        try
        {
            // Tab order for keyboard navigation
            SemanticProperties.SetTabIndex(this.FindByName("UsernameField"), 1);
            SemanticProperties.SetTabIndex(this.FindByName("PasswordField"), 2);
            SemanticProperties.SetTabIndex(this.FindByName("RememberMeCheckbox"), 3);
            SemanticProperties.SetTabIndex(this.FindByName("LoginButton"), 4);

            // Handle Enter key on password field to submit form
            if (this.FindByName("PasswordField") is Microsoft.Maui.Controls.Entry passwordEntry)
            {
                passwordEntry.Completed += OnPasswordCompleted;
            }
        }
        catch (Exception)
        {
            // Graceful fallback if elements not found
        }
    }

    /// <summary>
    /// Sets up accessibility features for screen readers
    /// </summary>
    private void SetupAccessibilityFeatures()
    {
        try
        {
            // Set semantic headings for better navigation
            if (this.FindByName("WelcomeLabel") is Label welcomeLabel)
            {
                SemanticProperties.SetHeadingLevel(welcomeLabel, SemanticHeadingLevel.Level1);
            }

            // Group related elements for better screen reader navigation
            var loginForm = this.FindByName("LoginFormCard");
            if (loginForm != null)
            {
                SemanticProperties.SetDescription(loginForm, "Login form containing username, password, and sign-in controls");
            }
        }
        catch (Exception)
        {
            // Graceful fallback if elements not found
        }
    }

    /// <summary>
    /// Handles password field completion (Enter key press)
    /// </summary>
    /// <param name="sender">The password entry</param>
    /// <param name="e">Event arguments</param>
    private void OnPasswordCompleted(object? sender, EventArgs e)
    {
        try
        {
            // Execute login command when Enter is pressed on password field
            if (BindingContext is LoginViewModel viewModel && viewModel.LoginCommand.CanExecute(null))
            {
                viewModel.LoginCommand.Execute(null);
            }
        }
        catch (Exception)
        {
            // Graceful error handling
        }
    }

    /// <summary>
    /// Called when the page appears
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            // Focus the username field when page appears for better accessibility
            if (this.FindByName("UsernameField") is Microsoft.Maui.Controls.Entry usernameEntry)
            {
                usernameEntry.Focus();
            }
        }
        catch (Exception)
        {
            // Graceful fallback
        }
    }
}
#endif