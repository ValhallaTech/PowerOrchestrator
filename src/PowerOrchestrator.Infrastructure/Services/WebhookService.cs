using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Infrastructure.Configuration;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// GitHub webhook service implementation with security validation
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly GitHubOptions _options;
    private readonly GitHubClient _client;
    private readonly IRepositorySyncService _syncService;

    /// <summary>
    /// Initializes a new instance of the WebhookService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">GitHub configuration options</param>
    /// <param name="syncService">Repository synchronization service</param>
    public WebhookService(
        ILogger<WebhookService> logger,
        IOptions<GitHubOptions> options,
        IRepositorySyncService syncService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));

        // Configure GitHub client
        _client = new GitHubClient(new ProductHeaderValue(_options.ApplicationName))
        {
            Credentials = new Credentials(_options.AccessToken)
        };

        // Set Enterprise URL if provided
        if (!string.IsNullOrEmpty(_options.EnterpriseBaseUrl))
        {
            _client = new GitHubClient(new ProductHeaderValue(_options.ApplicationName), new Uri(_options.EnterpriseBaseUrl))
            {
                Credentials = new Credentials(_options.AccessToken)
            };
        }
    }

    /// <inheritdoc />
    public async Task<Webhook> CreateWebhookAsync(string repositoryFullName)
    {
        try
        {
            _logger.LogInformation("Creating webhook for repository {Repository}", repositoryFullName);

            var parts = repositoryFullName.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Repository full name must be in format 'owner/name'", nameof(repositoryFullName));
            }

            var owner = parts[0];
            var name = parts[1];

            var hookConfig = new Dictionary<string, string>
            {
                { "url", $"{_options.WebhookEndpointBaseUrl}/api/webhooks/github" },
                { "content_type", "json" },
                { "secret", _options.WebhookSecret },
                { "insecure_ssl", _options.ValidateWebhookSsl ? "0" : "1" }
            };

            var hook = new NewRepositoryHook("web", hookConfig)
            {
                Events = new[] { "push", "pull_request", "create", "delete", "repository" },
                Active = true
            };

            var createdHook = await _client.Repository.Hooks.Create(owner, name, hook);

            _logger.LogInformation("Successfully created webhook {WebhookId} for repository {Repository}", 
                createdHook.Id, repositoryFullName);

            return new Webhook
            {
                Id = createdHook.Id,
                Url = createdHook.Config["url"]?.ToString() ?? string.Empty,
                Events = createdHook.Events,
                Active = createdHook.Active,
                CreatedAt = createdHook.CreatedAt.DateTime,
                UpdatedAt = createdHook.UpdatedAt.DateTime,
                Config = createdHook.Config.ToDictionary(kv => kv.Key, kv => (object)kv.Value)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create webhook for repository {Repository}", repositoryFullName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteWebhookAsync(string repositoryFullName)
    {
        try
        {
            _logger.LogInformation("Deleting webhook for repository {Repository}", repositoryFullName);

            var parts = repositoryFullName.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Repository full name must be in format 'owner/name'", nameof(repositoryFullName));
            }

            var owner = parts[0];
            var name = parts[1];

            // Find existing webhook
            var hooks = await _client.Repository.Hooks.GetAll(owner, name);
            var targetUrl = $"{_options.WebhookEndpointBaseUrl}/api/webhooks/github";
            var webhook = hooks.FirstOrDefault(h => h.Config.ContainsKey("url") && h.Config["url"]?.ToString() == targetUrl);

            if (webhook == null)
            {
                _logger.LogWarning("No webhook found for repository {Repository} with URL {Url}", repositoryFullName, targetUrl);
                return false;
            }

            await _client.Repository.Hooks.Delete(owner, name, webhook.Id);

            _logger.LogInformation("Successfully deleted webhook {WebhookId} for repository {Repository}", 
                webhook.Id, repositoryFullName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete webhook for repository {Repository}", repositoryFullName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Webhook>> GetWebhooksAsync(string repositoryFullName)
    {
        try
        {
            _logger.LogDebug("Fetching webhooks for repository {Repository}", repositoryFullName);

            var parts = repositoryFullName.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Repository full name must be in format 'owner/name'", nameof(repositoryFullName));
            }

            var owner = parts[0];
            var name = parts[1];

            var hooks = await _client.Repository.Hooks.GetAll(owner, name);

            return hooks.Select(h => new Webhook
            {
                Id = h.Id,
                Url = h.Config.ContainsKey("url") ? h.Config["url"] : string.Empty,
                Events = h.Events,
                Active = h.Active,
                CreatedAt = h.CreatedAt.DateTime,
                UpdatedAt = h.UpdatedAt.DateTime,
                Config = h.Config.ToDictionary(kv => kv.Key, kv => (object)kv.Value)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch webhooks for repository {Repository}", repositoryFullName);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<bool> ValidateWebhookSignatureAsync(string payload, string signature)
    {
        try
        {
            if (string.IsNullOrEmpty(signature) || !signature.StartsWith("sha256="))
            {
                _logger.LogWarning("Invalid webhook signature format");
                return Task.FromResult(false);
            }

            if (string.IsNullOrEmpty(_options.WebhookSecret))
            {
                _logger.LogError("Webhook secret is not configured");
                return Task.FromResult(false);
            }

            var expectedSignature = signature.Substring(7); // Remove "sha256=" prefix
            var keyBytes = Encoding.UTF8.GetBytes(_options.WebhookSecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(payloadBytes);
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            var isValid = CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(expectedSignature),
                Convert.FromHexString(computedSignature));

            if (!isValid)
            {
                _logger.LogWarning("Webhook signature validation failed");
            }

            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook signature");
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public async Task<WebhookProcessingResult> ProcessWebhookEventAsync(string eventType, string payload)
    {
        var result = new WebhookProcessingResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing webhook event: {EventType}", eventType);

            var parsedPayload = JsonConvert.DeserializeObject<dynamic>(payload);
            
            // Extract repository information
            if (parsedPayload?.repository == null)
            {
                result.Success = false;
                result.Message = "No repository information found in webhook payload";
                return result;
            }

            var repositoryFullName = parsedPayload.repository.full_name?.ToString();
            if (string.IsNullOrEmpty(repositoryFullName))
            {
                result.Success = false;
                result.Message = "Repository full name not found in webhook payload";
                return result;
            }

            // Create webhook event object
            var webhookEvent = new WebhookEvent
            {
                EventType = eventType,
                RepositoryFullName = repositoryFullName!,
                RawPayload = payload
            };

            // Extract additional event-specific information
            switch (eventType.ToLowerInvariant())
            {
                case "push":
                    ExtractPushEventData(parsedPayload, webhookEvent);
                    break;
                case "pull_request":
                    ExtractPullRequestEventData(parsedPayload, webhookEvent);
                    break;
                case "create":
                case "delete":
                    ExtractBranchEventData(parsedPayload, webhookEvent);
                    break;
            }

            // Only process sync-triggering events
            var syncTriggeringEvents = new[] { "push", "pull_request", "create", "delete" };
            if (syncTriggeringEvents.Contains(eventType.ToLowerInvariant()))
            {
                // Process the event through sync service
                var syncResult = await _syncService.HandleWebhookEventAsync(webhookEvent);

                result.Success = syncResult.Status == Domain.ValueObjects.SyncStatus.Completed;
                result.Message = result.Success ? "Webhook event processed successfully" : syncResult.ErrorMessage ?? "Sync failed";
                result.Data["sync_result"] = syncResult;
            }
            else
            {
                // Event received but not processed for sync
                result.Success = true;
                result.Message = $"Webhook event '{eventType}' received but does not trigger synchronization";
                result.Data["event_type"] = eventType;
            }

            _logger.LogInformation("Webhook event {EventType} for {Repository} processed: {Status}", 
                (object)eventType, (object)(repositoryFullName ?? "unknown"), result.Success ? "Success" : "Failed");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "Error processing webhook event";
            result.ErrorDetails = ex.Message;
            _logger.LogError(ex, "Failed to process webhook event: {EventType}", eventType);
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    /// <inheritdoc />
    public bool ValidateEventTimestamp(string timestamp, int tolerance = 300)
    {
        try
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                return false;
            }

            if (!long.TryParse(timestamp, out var unixTimestamp))
            {
                return false;
            }

            var eventTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            var now = DateTimeOffset.UtcNow;
            var timeDifference = Math.Abs((now - eventTime).TotalSeconds);

            return timeDifference <= tolerance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating event timestamp: {Timestamp}", timestamp);
            return false;
        }
    }

    /// <summary>
    /// Gets webhook delivery logs for a repository
    /// </summary>
    public Task<IEnumerable<WebhookDelivery>> GetWebhookDeliveriesAsync(string repositoryFullName, int limit = 50)
    {
        try
        {
            _logger.LogDebug("Fetching webhook deliveries for repository {Repository}", repositoryFullName);

            // Note: Webhook deliveries API may not be available in all Octokit versions
            // For now, return empty collection and log a warning
            _logger.LogWarning("Webhook deliveries API is not implemented in this version");
            return Task.FromResult(Enumerable.Empty<WebhookDelivery>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch webhook deliveries for repository {Repository}", repositoryFullName);
            throw;
        }
    }

    private static void ExtractPushEventData(dynamic root, WebhookEvent webhookEvent)
    {
        try
        {
            if (root.@ref != null)
            {
                var refValue = root.@ref.ToString();
                if (refValue?.StartsWith("refs/heads/") == true)
                {
                    webhookEvent.Branch = refValue.Substring(11); // Remove "refs/heads/"
                }
            }

            if (root.head_commit?.id != null)
            {
                webhookEvent.CommitSha = root.head_commit.id.ToString();
            }

            if (root.commits != null)
            {
                var modifiedFiles = new List<string>();
                foreach (var commit in root.commits)
                {
                    if (commit.added != null)
                    {
                        foreach (var file in commit.added)
                        {
                            var fileName = file?.ToString();
                            if (!string.IsNullOrEmpty(fileName))
                                modifiedFiles.Add(fileName);
                        }
                    }
                    if (commit.modified != null)
                    {
                        foreach (var file in commit.modified)
                        {
                            var fileName = file?.ToString();
                            if (!string.IsNullOrEmpty(fileName))
                                modifiedFiles.Add(fileName);
                        }
                    }
                    if (commit.removed != null)
                    {
                        foreach (var file in commit.removed)
                        {
                            var fileName = file?.ToString();
                            if (!string.IsNullOrEmpty(fileName))
                                modifiedFiles.Add(fileName);
                        }
                    }
                }
                webhookEvent.ModifiedFiles = modifiedFiles.Distinct();
            }
        }
        catch
        {
            // Ignore errors in data extraction
        }
    }

    private static void ExtractPullRequestEventData(dynamic root, WebhookEvent webhookEvent)
    {
        try
        {
            if (root.pull_request?.head?.@ref != null)
            {
                webhookEvent.Branch = root.pull_request.head.@ref.ToString();
            }

            if (root.pull_request?.head?.sha != null)
            {
                webhookEvent.CommitSha = root.pull_request.head.sha.ToString();
            }
        }
        catch
        {
            // Ignore errors in data extraction
        }
    }

    private static void ExtractBranchEventData(dynamic root, WebhookEvent webhookEvent)
    {
        try
        {
            if (root.@ref != null)
            {
                webhookEvent.Branch = root.@ref.ToString();
            }

            if (root.ref_type != null)
            {
                var refType = root.ref_type.ToString();
                if (refType != "branch")
                {
                    // This is a tag event, not a branch event
                    webhookEvent.Branch = null;
                }
            }
        }
        catch
        {
            // Ignore errors in data extraction
        }
    }
}