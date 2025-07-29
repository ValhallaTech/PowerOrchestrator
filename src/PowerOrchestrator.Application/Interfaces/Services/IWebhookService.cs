namespace PowerOrchestrator.Application.Interfaces.Services;

/// <summary>
/// Service interface for GitHub webhook management
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Creates a webhook for a repository
    /// </summary>
    /// <param name="repositoryFullName">Repository full name (owner/name)</param>
    /// <returns>Created webhook information</returns>
    Task<Webhook> CreateWebhookAsync(string repositoryFullName);

    /// <summary>
    /// Deletes a webhook for a repository
    /// </summary>
    /// <param name="repositoryFullName">Repository full name (owner/name)</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteWebhookAsync(string repositoryFullName);

    /// <summary>
    /// Lists all webhooks for a repository
    /// </summary>
    /// <param name="repositoryFullName">Repository full name (owner/name)</param>
    /// <returns>Collection of webhooks</returns>
    Task<IEnumerable<Webhook>> GetWebhooksAsync(string repositoryFullName);

    /// <summary>
    /// Validates a webhook signature
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="signature">Webhook signature</param>
    /// <returns>True if signature is valid</returns>
    Task<bool> ValidateWebhookSignatureAsync(string payload, string signature);

    /// <summary>
    /// Processes a webhook event
    /// </summary>
    /// <param name="eventType">Type of webhook event</param>
    /// <param name="payload">Webhook payload</param>
    /// <returns>Processing result</returns>
    Task<WebhookProcessingResult> ProcessWebhookEventAsync(string eventType, string payload);

    /// <summary>
    /// Validates webhook event timestamp to prevent replay attacks
    /// </summary>
    /// <param name="timestamp">Event timestamp</param>
    /// <param name="tolerance">Tolerance in seconds (default 5 minutes)</param>
    /// <returns>True if timestamp is valid</returns>
    bool ValidateEventTimestamp(string timestamp, int tolerance = 300);

    /// <summary>
    /// Gets webhook delivery logs for a repository
    /// </summary>
    /// <param name="repositoryFullName">Repository full name</param>
    /// <param name="limit">Maximum number of logs to return</param>
    /// <returns>Collection of webhook delivery logs</returns>
    Task<IEnumerable<WebhookDelivery>> GetWebhookDeliveriesAsync(string repositoryFullName, int limit = 50);
}

/// <summary>
/// Represents a GitHub webhook
/// </summary>
public class Webhook
{
    /// <summary>
    /// Gets or sets the webhook ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the webhook URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the webhook events
    /// </summary>
    public IEnumerable<string> Events { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets whether the webhook is active
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets when the webhook was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the webhook was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the webhook configuration
    /// </summary>
    public Dictionary<string, object> Config { get; set; } = new();
}

/// <summary>
/// Represents the result of webhook processing
/// </summary>
public class WebhookProcessingResult
{
    /// <summary>
    /// Gets or sets whether processing was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the processing message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when processing was completed
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the processing duration
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets any error details
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Gets or sets additional processing data
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Represents a webhook delivery log entry
/// </summary>
public class WebhookDelivery
{
    /// <summary>
    /// Gets or sets the delivery ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the webhook ID
    /// </summary>
    public long WebhookId { get; set; }

    /// <summary>
    /// Gets or sets the event type
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delivery status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets when the delivery was attempted
    /// </summary>
    public DateTime DeliveredAt { get; set; }

    /// <summary>
    /// Gets or sets the delivery duration
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the response body
    /// </summary>
    public string? Response { get; set; }

    /// <summary>
    /// Gets or sets the request headers
    /// </summary>
    public Dictionary<string, string> RequestHeaders { get; set; } = new();

    /// <summary>
    /// Gets or sets the response headers
    /// </summary>
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();
}