using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using PowerOrchestrator.MAUI.Services;

namespace PowerOrchestrator.MAUI.ViewModels;

/// <summary>
/// Base view model class providing common functionality
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the logger instance
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the navigation service
    /// </summary>
    protected INavigationService NavigationService { get; }

    /// <summary>
    /// Gets the dialog service
    /// </summary>
    protected IDialogService DialogService { get; }

    /// <summary>
    /// Gets the API service
    /// </summary>
    protected IApiService ApiService { get; }

    private bool _isBusy;
    private string _title = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseViewModel"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="navigationService">The navigation service</param>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="apiService">The API service</param>
    protected BaseViewModel(
        ILogger logger,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiService apiService)
    {
        Logger = logger;
        NavigationService = navigationService;
        DialogService = dialogService;
        ApiService = apiService;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view model is busy
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Gets or sets the title of the view
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Occurs when a property value changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets a property value and raises PropertyChanged if the value has changed
    /// </summary>
    /// <typeparam name="T">The property type</typeparam>
    /// <param name="backingStore">The backing field</param>
    /// <param name="value">The new value</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>True if the property was changed</returns>
    protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises the PropertyChanged event
    /// </summary>
    /// <param name="propertyName">The property name</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Executes an async command with error handling and busy state management
    /// </summary>
    /// <param name="operation">The operation to execute</param>
    /// <param name="loadingMessage">Optional loading message</param>
    /// <returns>A task representing the operation</returns>
    protected async Task ExecuteAsync(Func<Task> operation, string? loadingMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            
            if (!string.IsNullOrEmpty(loadingMessage))
            {
                await DialogService.ShowLoadingAsync(loadingMessage);
            }

            await operation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing operation in {ViewModelType}", GetType().Name);
            await DialogService.ShowAlertAsync("Error", "An error occurred. Please try again.");
        }
        finally
        {
            IsBusy = false;
            
            if (!string.IsNullOrEmpty(loadingMessage))
            {
                await DialogService.HideLoadingAsync();
            }
        }
    }

    /// <summary>
    /// Called when the view model is initialized
    /// </summary>
    /// <returns>A task representing the initialization</returns>
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the view appears
    /// </summary>
    /// <returns>A task representing the operation</returns>
    public virtual Task OnAppearingAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the view disappears
    /// </summary>
    /// <returns>A task representing the operation</returns>
    public virtual Task OnDisappearingAsync()
    {
        return Task.CompletedTask;
    }
}