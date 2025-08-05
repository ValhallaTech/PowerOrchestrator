using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using Serilog;
using System.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for alert configurations using Dapper
/// </summary>
public class AlertConfigurationRepository : IAlertConfigurationRepository
{
    private readonly string _connectionString;
    private readonly ILogger _logger = Log.ForContext<AlertConfigurationRepository>();

    /// <summary>
    /// Initializes a new instance of the AlertConfigurationRepository class
    /// </summary>
    /// <param name="configuration">Configuration to get connection string</param>
    public AlertConfigurationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentException("DefaultConnection connection string not found");
    }

    /// <summary>
    /// Creates a new alert configuration
    /// </summary>
    /// <param name="alertConfig">Alert configuration to create</param>
    public async Task CreateAsync(AlertConfiguration alertConfig)
    {
        const string sql = @"
            INSERT INTO alert_configurations (id, name, description, metric_name, condition, threshold_value, 
                                            severity, is_enabled, notification_channels, created_at, modified_at, created_by)
            VALUES (@Id, @Name, @Description, @MetricName, @Condition, @ThresholdValue, 
                    @Severity, @IsEnabled, @NotificationChannels::jsonb, @CreatedAt, @ModifiedAt, @CreatedBy)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            alertConfig.Id,
            alertConfig.Name,
            alertConfig.Description,
            alertConfig.MetricName,
            alertConfig.Condition,
            alertConfig.ThresholdValue,
            alertConfig.Severity,
            alertConfig.IsEnabled,
            NotificationChannels = Newtonsoft.Json.JsonConvert.SerializeObject(alertConfig.NotificationChannels),
            alertConfig.CreatedAt,
            alertConfig.ModifiedAt,
            alertConfig.CreatedBy
        });

        _logger.Information("Created alert configuration {AlertId} for metric {MetricName}", 
            alertConfig.Id, alertConfig.MetricName);
    }

    /// <summary>
    /// Updates an existing alert configuration
    /// </summary>
    /// <param name="alertConfig">Alert configuration to update</param>
    public async Task UpdateAsync(AlertConfiguration alertConfig)
    {
        const string sql = @"
            UPDATE alert_configurations 
            SET name = @Name, description = @Description, metric_name = @MetricName, 
                condition = @Condition, threshold_value = @ThresholdValue, severity = @Severity,
                is_enabled = @IsEnabled, notification_channels = @NotificationChannels::jsonb,
                modified_at = @ModifiedAt
            WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            alertConfig.Id,
            alertConfig.Name,
            alertConfig.Description,
            alertConfig.MetricName,
            alertConfig.Condition,
            alertConfig.ThresholdValue,
            alertConfig.Severity,
            alertConfig.IsEnabled,
            NotificationChannels = Newtonsoft.Json.JsonConvert.SerializeObject(alertConfig.NotificationChannels),
            alertConfig.ModifiedAt
        });

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Alert configuration with ID {alertConfig.Id} not found");
        }

        _logger.Information("Updated alert configuration {AlertId}", alertConfig.Id);
    }

    /// <summary>
    /// Gets an alert configuration by ID
    /// </summary>
    /// <param name="id">Alert configuration ID</param>
    /// <returns>Alert configuration or null if not found</returns>
    public async Task<AlertConfiguration?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, name, description, metric_name, condition, threshold_value, 
                   severity, is_enabled, notification_channels, created_at, modified_at, created_by
            FROM alert_configurations 
            WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync(sql, new { Id = id });
        
        if (result == null) return null;

        return new AlertConfiguration
        {
            Id = result.id,
            Name = result.name,
            Description = result.description,
            MetricName = result.metric_name,
            Condition = result.condition,
            ThresholdValue = result.threshold_value,
            Severity = result.severity,
            IsEnabled = result.is_enabled,
            NotificationChannels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.notification_channels) ?? new List<string>(),
            CreatedAt = result.created_at,
            ModifiedAt = result.modified_at,
            CreatedBy = result.created_by
        };
    }

    /// <summary>
    /// Gets all alert configurations
    /// </summary>
    /// <returns>List of alert configurations</returns>
    public async Task<List<AlertConfiguration>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, name, description, metric_name, condition, threshold_value, 
                   severity, is_enabled, notification_channels, created_at, modified_at, created_by
            FROM alert_configurations 
            ORDER BY name";

        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync(sql);

        return results.Select(result => new AlertConfiguration
        {
            Id = result.id,
            Name = result.name,
            Description = result.description,
            MetricName = result.metric_name,
            Condition = result.condition,
            ThresholdValue = result.threshold_value,
            Severity = result.severity,
            IsEnabled = result.is_enabled,
            NotificationChannels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.notification_channels) ?? new List<string>(),
            CreatedAt = result.created_at,
            ModifiedAt = result.modified_at,
            CreatedBy = result.created_by
        }).ToList();
    }

    /// <summary>
    /// Gets enabled alert configurations
    /// </summary>
    /// <returns>List of enabled alert configurations</returns>
    public async Task<List<AlertConfiguration>> GetEnabledAlertsAsync()
    {
        const string sql = @"
            SELECT id, name, description, metric_name, condition, threshold_value, 
                   severity, is_enabled, notification_channels, created_at, modified_at, created_by
            FROM alert_configurations 
            WHERE is_enabled = true
            ORDER BY name";

        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync(sql);

        return results.Select(result => new AlertConfiguration
        {
            Id = result.id,
            Name = result.name,
            Description = result.description,
            MetricName = result.metric_name,
            Condition = result.condition,
            ThresholdValue = result.threshold_value,
            Severity = result.severity,
            IsEnabled = result.is_enabled,
            NotificationChannels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.notification_channels) ?? new List<string>(),
            CreatedAt = result.created_at,
            ModifiedAt = result.modified_at,
            CreatedBy = result.created_by
        }).ToList();
    }

    /// <summary>
    /// Deletes an alert configuration
    /// </summary>
    /// <param name="id">Alert configuration ID</param>
    public async Task DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM alert_configurations WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        if (rowsAffected > 0)
        {
            _logger.Information("Deleted alert configuration {AlertId}", id);
        }
    }
}

/// <summary>
/// Repository implementation for alert instances using Dapper
/// </summary>
public class AlertInstanceRepository : IAlertInstanceRepository
{
    private readonly string _connectionString;
    private readonly ILogger _logger = Log.ForContext<AlertInstanceRepository>();

    /// <summary>
    /// Initializes a new instance of the AlertInstanceRepository class
    /// </summary>
    /// <param name="configuration">Configuration to get connection string</param>
    public AlertInstanceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentException("DefaultConnection connection string not found");
    }

    /// <summary>
    /// Creates a new alert instance
    /// </summary>
    /// <param name="alertInstance">Alert instance to create</param>
    public async Task CreateAsync(AlertInstance alertInstance)
    {
        const string sql = @"
            INSERT INTO alert_instances (id, alert_configuration_id, state, actual_value, threshold_value, 
                                       triggered_at, acknowledged_at, acknowledged_by, resolved_at, resolved_by, 
                                       context, notification_status)
            VALUES (@Id, @AlertConfigurationId, @State, @ActualValue, @ThresholdValue, 
                    @TriggeredAt, @AcknowledgedAt, @AcknowledgedBy, @ResolvedAt, @ResolvedBy, 
                    @Context, @NotificationStatus::jsonb)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            alertInstance.Id,
            alertInstance.AlertConfigurationId,
            alertInstance.State,
            alertInstance.ActualValue,
            alertInstance.ThresholdValue,
            alertInstance.TriggeredAt,
            alertInstance.AcknowledgedAt,
            alertInstance.AcknowledgedBy,
            alertInstance.ResolvedAt,
            alertInstance.ResolvedBy,
            alertInstance.Context,
            NotificationStatus = Newtonsoft.Json.JsonConvert.SerializeObject(alertInstance.NotificationStatus)
        });

        _logger.Information("Created alert instance {AlertInstanceId} for configuration {AlertConfigId}", 
            alertInstance.Id, alertInstance.AlertConfigurationId);
    }

    /// <summary>
    /// Updates an existing alert instance
    /// </summary>
    /// <param name="alertInstance">Alert instance to update</param>
    public async Task UpdateAsync(AlertInstance alertInstance)
    {
        const string sql = @"
            UPDATE alert_instances 
            SET state = @State, actual_value = @ActualValue, threshold_value = @ThresholdValue,
                acknowledged_at = @AcknowledgedAt, acknowledged_by = @AcknowledgedBy, 
                resolved_at = @ResolvedAt, resolved_by = @ResolvedBy, context = @Context,
                notification_status = @NotificationStatus::jsonb
            WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            alertInstance.Id,
            alertInstance.State,
            alertInstance.ActualValue,
            alertInstance.ThresholdValue,
            alertInstance.AcknowledgedAt,
            alertInstance.AcknowledgedBy,
            alertInstance.ResolvedAt,
            alertInstance.ResolvedBy,
            alertInstance.Context,
            NotificationStatus = Newtonsoft.Json.JsonConvert.SerializeObject(alertInstance.NotificationStatus)
        });

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Alert instance with ID {alertInstance.Id} not found");
        }

        _logger.Information("Updated alert instance {AlertInstanceId}", alertInstance.Id);
    }

    /// <summary>
    /// Gets an alert instance by ID
    /// </summary>
    /// <param name="id">Alert instance ID</param>
    /// <returns>Alert instance or null if not found</returns>
    public async Task<AlertInstance?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT ai.id, ai.alert_configuration_id, ai.state, ai.actual_value, ai.threshold_value, 
                   ai.triggered_at, ai.acknowledged_at, ai.acknowledged_by, ai.resolved_at, ai.resolved_by, 
                   ai.context, ai.notification_status,
                   ac.id as config_id, ac.name, ac.description, ac.metric_name, ac.condition, 
                   ac.threshold_value as config_threshold, ac.severity, ac.is_enabled, 
                   ac.notification_channels, ac.created_at, ac.modified_at, ac.created_by
            FROM alert_instances ai
            LEFT JOIN alert_configurations ac ON ai.alert_configuration_id = ac.id
            WHERE ai.id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync(sql, new { Id = id });
        
        if (result == null) return null;

        var alertInstance = new AlertInstance
        {
            Id = result.id,
            AlertConfigurationId = result.alert_configuration_id,
            State = result.state,
            ActualValue = result.actual_value,
            ThresholdValue = result.threshold_value,
            TriggeredAt = result.triggered_at,
            AcknowledgedAt = result.acknowledged_at,
            AcknowledgedBy = result.acknowledged_by,
            ResolvedAt = result.resolved_at,
            ResolvedBy = result.resolved_by,
            Context = result.context,
            NotificationStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result.notification_status) ?? new Dictionary<string, string>()
        };

        if (result.config_id != null)
        {
            alertInstance.AlertConfiguration = new AlertConfiguration
            {
                Id = result.config_id,
                Name = result.name,
                Description = result.description,
                MetricName = result.metric_name,
                Condition = result.condition,
                ThresholdValue = result.config_threshold,
                Severity = result.severity,
                IsEnabled = result.is_enabled,
                NotificationChannels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.notification_channels) ?? new List<string>(),
                CreatedAt = result.created_at,
                ModifiedAt = result.modified_at,
                CreatedBy = result.created_by
            };
        }

        return alertInstance;
    }

    /// <summary>
    /// Gets active alert instances
    /// </summary>
    /// <returns>List of active alert instances</returns>
    public async Task<List<AlertInstance>> GetActiveAlertsAsync()
    {
        const string sql = @"
            SELECT ai.id, ai.alert_configuration_id, ai.state, ai.actual_value, ai.threshold_value, 
                   ai.triggered_at, ai.acknowledged_at, ai.acknowledged_by, ai.resolved_at, ai.resolved_by, 
                   ai.context, ai.notification_status,
                   ac.id as config_id, ac.name, ac.description, ac.metric_name, ac.condition, 
                   ac.threshold_value as config_threshold, ac.severity, ac.is_enabled, 
                   ac.notification_channels, ac.created_at, ac.modified_at, ac.created_by
            FROM alert_instances ai
            LEFT JOIN alert_configurations ac ON ai.alert_configuration_id = ac.id
            WHERE ai.state IN ('Triggered', 'Acknowledged')
            ORDER BY ai.triggered_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync(sql);

        return results.Select(result => 
        {
            var alertInstance = new AlertInstance
            {
                Id = result.id,
                AlertConfigurationId = result.alert_configuration_id,
                State = result.state,
                ActualValue = result.actual_value,
                ThresholdValue = result.threshold_value,
                TriggeredAt = result.triggered_at,
                AcknowledgedAt = result.acknowledged_at,
                AcknowledgedBy = result.acknowledged_by,
                ResolvedAt = result.resolved_at,
                ResolvedBy = result.resolved_by,
                Context = result.context,
                NotificationStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result.notification_status) ?? new Dictionary<string, string>()
            };

            if (result.config_id != null)
            {
                alertInstance.AlertConfiguration = new AlertConfiguration
                {
                    Id = result.config_id,
                    Name = result.name,
                    Description = result.description,
                    MetricName = result.metric_name,
                    Condition = result.condition,
                    ThresholdValue = result.config_threshold,
                    Severity = result.severity,
                    IsEnabled = result.is_enabled,
                    NotificationChannels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.notification_channels) ?? new List<string>(),
                    CreatedAt = result.created_at,
                    ModifiedAt = result.modified_at,
                    CreatedBy = result.created_by
                };
            }

            return alertInstance;
        }).ToList();
    }

    /// <summary>
    /// Gets active alert for a specific configuration
    /// </summary>
    /// <param name="configId">Alert configuration ID</param>
    /// <returns>Active alert instance or null if not found</returns>
    public async Task<AlertInstance?> GetActiveAlertForConfigurationAsync(Guid configId)
    {
        const string sql = @"
            SELECT id, alert_configuration_id, state, actual_value, threshold_value, 
                   triggered_at, acknowledged_at, acknowledged_by, resolved_at, resolved_by, 
                   context, notification_status
            FROM alert_instances 
            WHERE alert_configuration_id = @ConfigId 
              AND state IN ('Triggered', 'Acknowledged')
            ORDER BY triggered_at DESC
            LIMIT 1";

        using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync(sql, new { ConfigId = configId });
        
        if (result == null) return null;

        return new AlertInstance
        {
            Id = result.id,
            AlertConfigurationId = result.alert_configuration_id,
            State = result.state,
            ActualValue = result.actual_value,
            ThresholdValue = result.threshold_value,
            TriggeredAt = result.triggered_at,
            AcknowledgedAt = result.acknowledged_at,
            AcknowledgedBy = result.acknowledged_by,
            ResolvedAt = result.resolved_at,
            ResolvedBy = result.resolved_by,
            Context = result.context,
            NotificationStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result.notification_status) ?? new Dictionary<string, string>()
        };
    }

    /// <summary>
    /// Gets alert instances for a time period
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <returns>List of alert instances</returns>
    public async Task<List<AlertInstance>> GetAlertInstancesByPeriodAsync(DateTime from, DateTime to)
    {
        const string sql = @"
            SELECT ai.id, ai.alert_configuration_id, ai.state, ai.actual_value, ai.threshold_value, 
                   ai.triggered_at, ai.acknowledged_at, ai.acknowledged_by, ai.resolved_at, ai.resolved_by, 
                   ai.context, ai.notification_status,
                   ac.id as config_id, ac.name, ac.description, ac.metric_name, ac.condition, 
                   ac.threshold_value as config_threshold, ac.severity, ac.is_enabled, 
                   ac.notification_channels, ac.created_at, ac.modified_at, ac.created_by
            FROM alert_instances ai
            LEFT JOIN alert_configurations ac ON ai.alert_configuration_id = ac.id
            WHERE ai.triggered_at >= @From AND ai.triggered_at <= @To
            ORDER BY ai.triggered_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync(sql, new { From = from, To = to });

        return results.Select(result => 
        {
            var alertInstance = new AlertInstance
            {
                Id = result.id,
                AlertConfigurationId = result.alert_configuration_id,
                State = result.state,
                ActualValue = result.actual_value,
                ThresholdValue = result.threshold_value,
                TriggeredAt = result.triggered_at,
                AcknowledgedAt = result.acknowledged_at,
                AcknowledgedBy = result.acknowledged_by,
                ResolvedAt = result.resolved_at,
                ResolvedBy = result.resolved_by,
                Context = result.context,
                NotificationStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result.notification_status) ?? new Dictionary<string, string>()
            };

            if (result.config_id != null)
            {
                alertInstance.AlertConfiguration = new AlertConfiguration
                {
                    Id = result.config_id,
                    Name = result.name,
                    Description = result.description,
                    MetricName = result.metric_name,
                    Condition = result.condition,
                    ThresholdValue = result.config_threshold,
                    Severity = result.severity,
                    IsEnabled = result.is_enabled,
                    NotificationChannels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.notification_channels) ?? new List<string>(),
                    CreatedAt = result.created_at,
                    ModifiedAt = result.modified_at,
                    CreatedBy = result.created_by
                };
            }

            return alertInstance;
        }).ToList();
    }
}