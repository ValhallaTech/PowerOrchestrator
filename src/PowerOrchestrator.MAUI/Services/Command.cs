using System.Windows.Input;

namespace PowerOrchestrator.MAUI.Services;

#if NET8_0
/// <summary>
/// Simple command implementation for console mode
/// </summary>
public class Command : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class
    /// </summary>
    /// <param name="execute">The execution logic</param>
    /// <param name="canExecute">The execution status logic</param>
    public Command(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        _execute();
    }

    /// <summary>
    /// Raises the CanExecuteChanged event
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Generic command implementation for console mode
/// </summary>
/// <typeparam name="T">The parameter type</typeparam>
public class Command<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="Command{T}"/> class
    /// </summary>
    /// <param name="execute">The execution logic</param>
    /// <param name="canExecute">The execution status logic</param>
    public Command(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public bool CanExecute(object? parameter)
    {
        if (parameter is T typedParam)
            return _canExecute?.Invoke(typedParam) ?? true;
        return false;
    }

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        if (parameter is T typedParam)
            _execute(typedParam);
    }

    /// <summary>
    /// Raises the CanExecuteChanged event
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
#endif