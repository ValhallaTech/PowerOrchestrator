using Microsoft.Extensions.Logging;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// Service for managing offline capabilities and data caching
/// </summary>
public interface IOfflineService
{
    /// <summary>
    /// Gets a value indicating whether the application is currently offline
    /// </summary>
    bool IsOffline { get; }

    /// <summary>
    /// Gets cached data for the specified key
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>The cached data or default if not found</returns>
    Task<T?> GetCachedDataAsync<T>(string key);

    /// <summary>
    /// Caches data with the specified key
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="data">The data to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <returns>A task representing the operation</returns>
    Task SetCachedDataAsync<T>(string key, T data, TimeSpan? expiration = null);

    /// <summary>
    /// Queues an operation for execution when back online
    /// </summary>
    /// <param name="operation">The operation to queue</param>
    /// <returns>A task representing the operation</returns>
    Task QueueOfflineOperationAsync(OfflineOperation operation);

    /// <summary>
    /// Processes all queued offline operations
    /// </summary>
    /// <returns>A task representing the operation</returns>
    Task ProcessOfflineOperationsAsync();

    /// <summary>
    /// Occurs when the connectivity status changes
    /// </summary>
    event EventHandler<bool> ConnectivityChanged;
}

/// <summary>
/// Represents an operation to be executed when back online
/// </summary>
public class OfflineOperation
{
    /// <summary>
    /// Gets or sets the operation identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the operation type
    /// </summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation data
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets the operation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Implementation of the offline service
/// </summary>
public class OfflineService : IOfflineService
{
    private readonly ILogger<OfflineService> _logger;
    private readonly ISettingsService _settingsService;
    private readonly Dictionary<string, CachedItem> _cache = new();
    private readonly List<OfflineOperation> _offlineQueue = new();
    private bool _isOffline = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflineService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="settingsService">The settings service</param>
    public OfflineService(
        ILogger<OfflineService> logger,
        ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
        
        // Initialize connectivity monitoring
        InitializeConnectivityMonitoring();
    }

    /// <inheritdoc/>
    public bool IsOffline => _isOffline;

    /// <inheritdoc/>
    public event EventHandler<bool>? ConnectivityChanged;

    /// <inheritdoc/>
    public async Task<T?> GetCachedDataAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (!item.IsExpired)
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    await Task.CompletedTask;
                    return (T?)item.Data;
                }
                else
                {
                    _cache.Remove(key);
                    _logger.LogDebug("Cache expired for key: {Key}", key);
                }
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            await Task.CompletedTask;
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached data for key: {Key}", key);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task SetCachedDataAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        try
        {
            var expirationTime = expiration.HasValue 
                ? DateTime.UtcNow.Add(expiration.Value)
                : DateTime.UtcNow.AddHours(1); // Default 1 hour expiration

            _cache[key] = new CachedItem
            {
                Data = data,
                ExpirationTime = expirationTime
            };

            _logger.LogDebug("Cached data for key: {Key}, expires at: {Expiration}", key, expirationTime);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching data for key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task QueueOfflineOperationAsync(OfflineOperation operation)
    {
        try
        {
            _offlineQueue.Add(operation);
            _logger.LogInformation("Queued offline operation: {OperationType} with ID: {Id}", 
                operation.OperationType, operation.Id);
                
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing offline operation: {OperationType}", operation.OperationType);
        }
    }

    /// <inheritdoc/>
    public async Task ProcessOfflineOperationsAsync()
    {
        if (_isOffline || !_offlineQueue.Any())
            return;

        try
        {
            _logger.LogInformation("Processing {Count} offline operations", _offlineQueue.Count);

            var operations = _offlineQueue.ToList();
            _offlineQueue.Clear();

            foreach (var operation in operations)
            {
                try
                {
                    // In a real implementation, this would route to appropriate handlers
                    _logger.LogInformation("Processing offline operation: {OperationType}", operation.OperationType);
                    
                    // Simulate processing
                    await Task.Delay(100);
                    
                    _logger.LogDebug("Successfully processed offline operation: {Id}", operation.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing offline operation: {Id}", operation.Id);
                    
                    // Retry logic
                    if (operation.RetryCount < operation.MaxRetries)
                    {
                        operation.RetryCount++;
                        _offlineQueue.Add(operation);
                        _logger.LogInformation("Requeued operation {Id} for retry {RetryCount}/{MaxRetries}", 
                            operation.Id, operation.RetryCount, operation.MaxRetries);
                    }
                    else
                    {
                        _logger.LogWarning("Operation {Id} exceeded max retries and will be discarded", operation.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing offline operations");
        }
    }

    /// <summary>
    /// Initializes connectivity monitoring
    /// </summary>
    private void InitializeConnectivityMonitoring()
    {
        try
        {
#if NET8_0
            // Console mode - simulate online status
            _isOffline = false;
            _logger.LogInformation("Offline service initialized in console mode (always online)");
#else
            // In MAUI mode, you would use Microsoft.Maui.Networking.Connectivity
            // Connectivity.ConnectivityChanged += OnConnectivityChanged;
            _isOffline = false; // Default to online
            _logger.LogInformation("Offline service initialized");
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing connectivity monitoring");
            _isOffline = true; // Assume offline if we can't determine status
        }
    }

    /// <summary>
    /// Handles connectivity changes
    /// </summary>
    /// <param name="isConnected">Whether the device is connected</param>
    private async void OnConnectivityChanged(bool isConnected)
    {
        try
        {
            var wasOffline = _isOffline;
            _isOffline = !isConnected;

            _logger.LogInformation("Connectivity changed: {Status}", isConnected ? "Online" : "Offline");

            ConnectivityChanged?.Invoke(this, isConnected);

            // Process queued operations when coming back online
            if (wasOffline && isConnected)
            {
                await ProcessOfflineOperationsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling connectivity change");
        }
    }

    /// <summary>
    /// Represents a cached item
    /// </summary>
    private class CachedItem
    {
        public object? Data { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }
}