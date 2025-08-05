namespace PowerOrchestrator.Infrastructure.Configuration;

/// <summary>
/// Configuration options for alerting system
/// </summary>
public class AlertingOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Alerting";

    /// <summary>
    /// Gets or sets whether alerting is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the processing interval in seconds
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum processing time in seconds
    /// </summary>
    public int MaxProcessingTimeSeconds { get; set; } = 15;

    /// <summary>
    /// Gets or sets notification channel configurations
    /// </summary>
    public NotificationChannelsOptions NotificationChannels { get; set; } = new();
}

/// <summary>
/// Configuration options for notification channels
/// </summary>
public class NotificationChannelsOptions
{
    /// <summary>
    /// Gets or sets email notification options
    /// </summary>
    public EmailNotificationOptions Email { get; set; } = new();

    /// <summary>
    /// Gets or sets webhook notification options
    /// </summary>
    public WebhookNotificationOptions Webhook { get; set; } = new();
}

/// <summary>
/// Configuration options for email notifications
/// </summary>
public class EmailNotificationOptions
{
    /// <summary>
    /// Gets or sets whether email notifications are enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the SMTP server
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the from email address
    /// </summary>
    public string From { get; set; } = string.Empty;
}

/// <summary>
/// Configuration options for webhook notifications
/// </summary>
public class WebhookNotificationOptions
{
    /// <summary>
    /// Gets or sets whether webhook notifications are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the webhook endpoints
    /// </summary>
    public List<string> Endpoints { get; set; } = new();
}