using Dapper;
using Microsoft.Extensions.Configuration;
using Serilog;
using Npgsql;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Data;
using System.Data;
using static PowerOrchestrator.Application.Interfaces.Repositories.ISyncHistoryRepository;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// High-performance repository using Dapper for bulk operations
/// </summary>
public interface IBulkOperationsRepository
{
    /// <summary>
    /// Bulk insert repository scripts
    /// </summary>
    /// <param name="scripts">Scripts to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> BulkInsertRepositoryScriptsAsync(IEnumerable<RepositoryScript> scripts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update repository scripts
    /// </summary>
    /// <param name="scripts">Scripts to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> BulkUpdateRepositoryScriptsAsync(IEnumerable<RepositoryScript> scripts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets repository scripts by SHA values for change detection
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="filePaths">File paths to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of file path to SHA mapping</returns>
    Task<Dictionary<string, string>> GetScriptShasByPathsAsync(Guid repositoryId, IEnumerable<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes repository scripts by file paths
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="filePaths">File paths to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> DeleteScriptsByPathsAsync(Guid repositoryId, IEnumerable<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sync statistics for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync statistics</returns>
    Task<SyncStatistics> GetSyncStatisticsAsync(Guid repositoryId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dapper-based bulk operations repository implementation
/// </summary>
public class BulkOperationsRepository : IBulkOperationsRepository
{
    private readonly PowerOrchestratorDbContext _context;
    private readonly ILogger _logger = Log.ForContext<BulkOperationsRepository>();
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the BulkOperationsRepository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="configuration">Configuration to get connection string</param>
    public BulkOperationsRepository(PowerOrchestratorDbContext context, IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentException("DefaultConnection connection string not found");
    }

    /// <inheritdoc />
    public async Task<int> BulkInsertRepositoryScriptsAsync(IEnumerable<RepositoryScript> scripts, CancellationToken cancellationToken = default)
    {
        var scriptList = scripts.ToList();
        if (!scriptList.Any())
            return 0;

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                INSERT INTO powerorchestrator.repository_scripts 
                (id, repository_id, script_id, file_path, branch, sha, metadata, security_analysis, last_modified, created_at, updated_at)
                VALUES 
                (@Id, @RepositoryId, @ScriptId, @FilePath, @Branch, @Sha, @Metadata::jsonb, @SecurityAnalysis::jsonb, @LastModified, @CreatedAt, @UpdatedAt)";

            var parameters = scriptList.Select(script => new
            {
                Id = script.Id == Guid.Empty ? Guid.NewGuid() : script.Id,
                RepositoryId = script.RepositoryId,
                ScriptId = script.ScriptId,
                FilePath = script.FilePath,
                Branch = script.Branch,
                Sha = script.Sha,
                Metadata = script.Metadata,
                SecurityAnalysis = script.SecurityAnalysis,
                LastModified = script.LastModified,
                CreatedAt = script.CreatedAt == default ? DateTime.UtcNow : script.CreatedAt,
                UpdatedAt = script.UpdatedAt == default ? DateTime.UtcNow : script.UpdatedAt
            });

            var result = await connection.ExecuteAsync(sql, parameters);
            _logger.Information("Bulk inserted {Count} repository scripts", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to bulk insert {Count} repository scripts", scriptList.Count);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> BulkUpdateRepositoryScriptsAsync(IEnumerable<RepositoryScript> scripts, CancellationToken cancellationToken = default)
    {
        var scriptList = scripts.ToList();
        if (!scriptList.Any())
            return 0;

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                UPDATE powerorchestrator.repository_scripts 
                SET sha = @Sha, 
                    metadata = @Metadata::jsonb, 
                    security_analysis = @SecurityAnalysis::jsonb, 
                    last_modified = @LastModified, 
                    updated_at = @UpdatedAt
                WHERE repository_id = @RepositoryId AND file_path = @FilePath AND branch = @Branch";

            var parameters = scriptList.Select(script => new
            {
                RepositoryId = script.RepositoryId,
                FilePath = script.FilePath,
                Branch = script.Branch,
                Sha = script.Sha,
                Metadata = script.Metadata,
                SecurityAnalysis = script.SecurityAnalysis,
                LastModified = script.LastModified,
                UpdatedAt = DateTime.UtcNow
            });

            var result = await connection.ExecuteAsync(sql, parameters);
            _logger.Information("Bulk updated {Count} repository scripts", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to bulk update {Count} repository scripts", scriptList.Count);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> GetScriptShasByPathsAsync(Guid repositoryId, IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
    {
        var pathList = filePaths.ToList();
        if (!pathList.Any())
            return new Dictionary<string, string>();

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                SELECT file_path, sha 
                FROM powerorchestrator.repository_scripts 
                WHERE repository_id = @RepositoryId AND file_path = ANY(@FilePaths)";

            var result = await connection.QueryAsync<(string FilePath, string Sha)>(sql, new
            {
                RepositoryId = repositoryId,
                FilePaths = pathList.ToArray()
            });

            return result.ToDictionary(x => x.FilePath, x => x.Sha);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get script SHAs for {Count} paths", pathList.Count);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> DeleteScriptsByPathsAsync(Guid repositoryId, IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
    {
        var pathList = filePaths.ToList();
        if (!pathList.Any())
            return 0;

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                DELETE FROM powerorchestrator.repository_scripts 
                WHERE repository_id = @RepositoryId AND file_path = ANY(@FilePaths)";

            var result = await connection.ExecuteAsync(sql, new
            {
                RepositoryId = repositoryId,
                FilePaths = pathList.ToArray()
            });

            _logger.Information("Deleted {Count} repository scripts by paths", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete scripts by {Count} paths", pathList.Count);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SyncStatistics> GetSyncStatisticsAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                SELECT 
                    COUNT(*) as total_syncs,
                    COUNT(CASE WHEN status = 'Completed' THEN 1 END) as successful_syncs,
                    COUNT(CASE WHEN status = 'Failed' THEN 1 END) as failed_syncs,
                    COALESCE(AVG(CASE WHEN status = 'Completed' THEN duration_ms END), 0) as avg_duration_ms,
                    COALESCE(SUM(scripts_processed), 0) as total_scripts_processed
                FROM powerorchestrator.sync_history 
                WHERE repository_id = @RepositoryId";

            var result = await connection.QuerySingleOrDefaultAsync(sql, new { RepositoryId = repositoryId });

            return new SyncStatistics
            {
                RepositoryId = repositoryId,
                TotalSyncs = result?.total_syncs ?? 0,
                SuccessfulSyncs = result?.successful_syncs ?? 0,
                FailedSyncs = result?.failed_syncs ?? 0,
                AverageDuration = TimeSpan.FromMilliseconds(result?.avg_duration_ms ?? 0),
                TotalScriptsProcessed = result?.total_scripts_processed ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get sync statistics for repository {RepositoryId}", repositoryId);
            throw;
        }
    }
}