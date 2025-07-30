using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.Application.Interfaces.Services;
using System.Text;
using Newtonsoft.Json;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for handling GitHub webhook events
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IRepositorySyncService _syncService;
    private readonly ILogger<WebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of the WebhookController
    /// </summary>
    /// <param name="webhookService">Webhook service</param>
    /// <param name="syncService">Repository synchronization service</param>
    /// <param name="logger">Logger instance</param>
    public WebhookController(
        IWebhookService webhookService,
        IRepositorySyncService syncService,
        ILogger<WebhookController> logger)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes GitHub webhook events
    /// </summary>
    /// <returns>Result of webhook processing</returns>
    [HttpPost("github")]
    public async Task<IActionResult> ProcessGitHubWebhook()
    {
        try
        {
            // Validate required headers
            if (!Request.Headers.ContainsKey("X-GitHub-Event"))
            {
                _logger.LogWarning("Webhook request missing X-GitHub-Event header");
                return BadRequest(new { error = "Missing X-GitHub-Event header" });
            }

            var eventType = Request.Headers["X-GitHub-Event"].ToString();
            var signature = Request.Headers["X-Hub-Signature-256"].ToString();
            var delivery = Request.Headers["X-GitHub-Delivery"].ToString();

            // Read the payload
            string payload;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                payload = await reader.ReadToEndAsync();
            }

            // Validate payload is valid JSON
            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogWarning("Webhook request has empty payload");
                return BadRequest(new { error = "Empty payload" });
            }

            try
            {
                JsonConvert.DeserializeObject(payload);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Webhook request has malformed JSON payload");
                return BadRequest(new { error = "Malformed JSON payload" });
            }

            // Validate signature if provided
            if (!string.IsNullOrEmpty(signature))
            {
                var isValidSignature = await _webhookService.ValidateWebhookSignatureAsync(payload, signature);
                if (!isValidSignature)
                {
                    _logger.LogWarning("Webhook request has invalid signature");
                    return Unauthorized(new { error = "Invalid signature" });
                }
            }

            // Validate timestamp to prevent replay attacks
            var timestamp = Request.Headers["X-GitHub-Delivery-Timestamp"].ToString();
            if (!string.IsNullOrEmpty(timestamp))
            {
                var isValidTimestamp = _webhookService.ValidateEventTimestamp(timestamp);
                if (!isValidTimestamp)
                {
                    _logger.LogWarning("Webhook request has invalid or expired timestamp");
                    return Unauthorized(new { error = "Invalid or expired timestamp" });
                }
            }

            // Process the webhook event
            var result = await _webhookService.ProcessWebhookEventAsync(eventType, payload);

            if (result.Success)
            {
                _logger.LogInformation("Successfully processed webhook event {EventType} for delivery {Delivery}", 
                    eventType, delivery);
                return Ok(new { 
                    message = "Webhook processed successfully",
                    eventType,
                    delivery,
                    processedAt = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogWarning("Failed to process webhook event {EventType}: {Message}", 
                    eventType, result.Message);
                return BadRequest(new { 
                    error = result.Message,
                    eventType,
                    delivery
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(500, new { error = "Internal server error processing webhook" });
        }
    }

    /// <summary>
    /// Health check endpoint for webhook service
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public IActionResult GetWebhookHealth()
    {
        return Ok(new { 
            status = "healthy",
            service = "webhook",
            timestamp = DateTime.UtcNow
        });
    }
}