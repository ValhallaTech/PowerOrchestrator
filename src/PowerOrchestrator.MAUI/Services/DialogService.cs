using Microsoft.Extensions.Logging;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Dialog service implementation for showing dialogs and alerts
/// </summary>
public class DialogService : IDialogService
{
    private readonly ILogger<DialogService> _logger;
    private bool _isLoadingVisible;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public DialogService(ILogger<DialogService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        try
        {
            _logger.LogInformation("Showing alert: {Title} - {Message}", title, message);
            
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing alert");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        try
        {
            _logger.LogInformation("Showing confirmation: {Title} - {Message}", title, message);
            
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing confirmation");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task ShowLoadingAsync(string message = "Loading...")
    {
        try
        {
            if (_isLoadingVisible) return;
            
            _logger.LogInformation("Showing loading dialog: {Message}", message);
            _isLoadingVisible = true;
            
            // TODO: Implement a proper loading dialog using UraniumUI
            // For now, we'll use a simple approach
            await Task.Delay(50); // Simulate showing loading
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing loading dialog");
        }
    }

    /// <inheritdoc/>
    public async Task HideLoadingAsync()
    {
        try
        {
            if (!_isLoadingVisible) return;
            
            _logger.LogInformation("Hiding loading dialog");
            _isLoadingVisible = false;
            
            // TODO: Implement hiding the loading dialog
            await Task.Delay(50); // Simulate hiding loading
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding loading dialog");
        }
    }
}