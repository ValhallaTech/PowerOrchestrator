using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerOrchestrator.Infrastructure.Configuration;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// GitHub API rate limiting service
/// </summary>
public interface IGitHubRateLimitService
{
    /// <summary>
    /// Waits for API rate limit if necessary
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task WaitForRateLimitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an API call
    /// </summary>
    void RecordApiCall();

    /// <summary>
    /// Gets current rate limit status
    /// </summary>
    /// <returns>Rate limit information</returns>
    RateLimitStatus GetRateLimitStatus();

    /// <summary>
    /// Checks if we're approaching rate limit
    /// </summary>
    /// <returns>True if approaching limit</returns>
    bool IsApproachingRateLimit();
}

/// <summary>
/// Rate limit status information
/// </summary>
public class RateLimitStatus
{
    /// <summary>
    /// Gets or sets the remaining API calls
    /// </summary>
    public int Remaining { get; set; }

    /// <summary>
    /// Gets or sets the total API limit
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets when the rate limit resets
    /// </summary>
    public DateTime ResetTime { get; set; }

    /// <summary>
    /// Gets the percentage of API calls remaining
    /// </summary>
    public double RemainingPercentage => Limit > 0 ? (double)Remaining / Limit * 100 : 0;
}

/// <summary>
/// GitHub API rate limiting service implementation
/// </summary>
public class GitHubRateLimitService : IGitHubRateLimitService
{
    private readonly ILogger<GitHubRateLimitService> _logger;
    private readonly GitHubOptions _options;
    private readonly object _lock = new();
    
    private int _remainingCalls = 5000; // GitHub's default limit
    private int _totalLimit = 5000;
    private DateTime _resetTime = DateTime.UtcNow.AddHours(1);
    private readonly Queue<DateTime> _recentCalls = new();

    /// <summary>
    /// Initializes a new instance of the GitHubRateLimitService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">GitHub configuration options</param>
    public GitHubRateLimitService(ILogger<GitHubRateLimitService> logger, IOptions<GitHubOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task WaitForRateLimitAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            CleanupOldCalls();
            
            // Check if we've exceeded our safety threshold (80% of limit)
            var safetyThreshold = (int)(_totalLimit * 0.8);
            if (_remainingCalls <= (_totalLimit - safetyThreshold))
            {
                var waitTime = CalculateWaitTime();
                if (waitTime > TimeSpan.Zero)
                {
                    _logger.LogWarning("Rate limit approaching. Waiting {WaitTime} before next API call", waitTime);
                }
            }
        }

        // Check if we need to wait for rate limit reset
        var currentTime = DateTime.UtcNow;
        if (currentTime < _resetTime && _remainingCalls <= 10) // Emergency threshold
        {
            var waitTime = _resetTime.Subtract(currentTime);
            _logger.LogWarning("Rate limit exceeded. Waiting {WaitTime} for reset", waitTime);
            
            try
            {
                await Task.Delay(waitTime, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Rate limit wait cancelled");
                throw;
            }
        }
    }

    /// <inheritdoc />
    public void RecordApiCall()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            _recentCalls.Enqueue(now);
            _remainingCalls = Math.Max(0, _remainingCalls - 1);
            
            CleanupOldCalls();
            
            _logger.LogDebug("API call recorded. Remaining: {Remaining}/{Total}", _remainingCalls, _totalLimit);
        }
    }

    /// <inheritdoc />
    public RateLimitStatus GetRateLimitStatus()
    {
        lock (_lock)
        {
            CleanupOldCalls();
            
            return new RateLimitStatus
            {
                Remaining = _remainingCalls,
                Limit = _totalLimit,
                ResetTime = _resetTime
            };
        }
    }

    /// <inheritdoc />
    public bool IsApproachingRateLimit()
    {
        lock (_lock)
        {
            CleanupOldCalls();
            
            // Consider it approaching if we have less than 20% remaining
            var threshold = (int)(_totalLimit * 0.2);
            return _remainingCalls <= threshold;
        }
    }

    /// <summary>
    /// Updates rate limit information from GitHub API response headers
    /// </summary>
    /// <param name="remaining">Remaining API calls</param>
    /// <param name="limit">Total API limit</param>
    /// <param name="resetTime">Reset time</param>
    public void UpdateRateLimitInfo(int remaining, int limit, DateTime resetTime)
    {
        lock (_lock)
        {
            _remainingCalls = remaining;
            _totalLimit = limit;
            _resetTime = resetTime;
            
            _logger.LogDebug("Rate limit info updated: {Remaining}/{Total}, resets at {ResetTime}", 
                remaining, limit, resetTime);
        }
    }

    private void CleanupOldCalls()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        while (_recentCalls.Count > 0 && _recentCalls.Peek() < oneHourAgo)
        {
            _recentCalls.Dequeue();
        }
    }

    private TimeSpan CalculateWaitTime()
    {
        if (_recentCalls.Count < 10) // If we haven't made many calls, no need to wait
            return TimeSpan.Zero;

        // Calculate average time between recent calls
        var recentCallsArray = _recentCalls.ToArray();
        if (recentCallsArray.Length < 2)
            return TimeSpan.Zero;

        var totalSpan = recentCallsArray[^1] - recentCallsArray[0];
        var averageInterval = totalSpan.TotalMilliseconds / (recentCallsArray.Length - 1);

        // If we're making calls too quickly, wait a bit
        if (averageInterval < 1000) // Less than 1 second between calls
        {
            return TimeSpan.FromMilliseconds(1000 - averageInterval);
        }

        return TimeSpan.Zero;
    }
}