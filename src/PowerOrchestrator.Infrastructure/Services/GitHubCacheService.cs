using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

namespace PowerOrchestrator.Infrastructure.Services;

/// <summary>
/// Caching service for GitHub data
/// </summary>
public interface IGitHubCacheService
{
    /// <summary>
    /// Caches repository information
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="repository">Repository to cache</param>
    /// <param name="expiry">Cache expiry time</param>
    Task SetRepositoryAsync(string key, GitHubRepository repository, TimeSpan? expiry = null);

    /// <summary>
    /// Gets cached repository information
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Cached repository or null</returns>
    Task<GitHubRepository?> GetRepositoryAsync(string key);

    /// <summary>
    /// Caches repository files
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="files">Files to cache</param>
    /// <param name="expiry">Cache expiry time</param>
    Task SetFilesAsync(string key, IEnumerable<GitHubFile> files, TimeSpan? expiry = null);

    /// <summary>
    /// Gets cached repository files
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Cached files or null</returns>
    Task<IEnumerable<GitHubFile>?> GetFilesAsync(string key);

    /// <summary>
    /// Caches script metadata
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="metadata">Metadata to cache</param>
    /// <param name="expiry">Cache expiry time</param>
    Task SetScriptMetadataAsync(string key, ScriptMetadata metadata, TimeSpan? expiry = null);

    /// <summary>
    /// Gets cached script metadata
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Cached metadata or null</returns>
    Task<ScriptMetadata?> GetScriptMetadataAsync(string key);

    /// <summary>
    /// Removes cached data
    /// </summary>
    /// <param name="key">Cache key</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes cached data by pattern
    /// </summary>
    /// <param name="pattern">Key pattern</param>
    Task RemoveByPatternAsync(string pattern);
}

/// <summary>
/// Redis-based caching service for GitHub data
/// </summary>
public class GitHubCacheService : IGitHubCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<GitHubCacheService> _logger;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Initializes a new instance of the GitHubCacheService
    /// </summary>
    /// <param name="cache">Distributed cache instance</param>
    /// <param name="logger">Logger instance</param>
    public GitHubCacheService(IDistributedCache cache, ILogger<GitHubCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SetRepositoryAsync(string key, GitHubRepository repository, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonConvert.SerializeObject(repository);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry
            };

            await _cache.SetStringAsync(key, json, options);
            _logger.LogDebug("Cached repository data for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache repository data for key: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<GitHubRepository?> GetRepositoryAsync(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogDebug("Cache miss for repository key: {Key}", key);
                return null;
            }

            var repository = JsonConvert.DeserializeObject<GitHubRepository>(json);
            _logger.LogDebug("Cache hit for repository key: {Key}", key);
            return repository;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached repository data for key: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetFilesAsync(string key, IEnumerable<GitHubFile> files, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonConvert.SerializeObject(files);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry
            };

            await _cache.SetStringAsync(key, json, options);
            _logger.LogDebug("Cached files data for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache files data for key: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubFile>?> GetFilesAsync(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogDebug("Cache miss for files key: {Key}", key);
                return null;
            }

            var files = JsonConvert.DeserializeObject<IEnumerable<GitHubFile>>(json);
            _logger.LogDebug("Cache hit for files key: {Key}", key);
            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached files data for key: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetScriptMetadataAsync(string key, ScriptMetadata metadata, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonConvert.SerializeObject(metadata);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromHours(1) // Metadata can be cached longer
            };

            await _cache.SetStringAsync(key, json, options);
            _logger.LogDebug("Cached script metadata for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache script metadata for key: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<ScriptMetadata?> GetScriptMetadataAsync(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogDebug("Cache miss for metadata key: {Key}", key);
                return null;
            }

            var metadata = JsonConvert.DeserializeObject<ScriptMetadata>(json);
            _logger.LogDebug("Cache hit for metadata key: {Key}", key);
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached script metadata for key: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Removed cache entry for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache entry for key: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // Note: This is a simplified implementation. 
            // For full Redis pattern support, you would need to use StackExchange.Redis directly
            _logger.LogWarning("RemoveByPatternAsync not fully implemented for IDistributedCache. Pattern: {Pattern}", pattern);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache entries by pattern: {Pattern}", pattern);
        }
    }

    /// <summary>
    /// Generates a cache key for repository data
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <returns>Cache key</returns>
    public static string GetRepositoryKey(string owner, string name) => $"github:repo:{owner}:{name}";

    /// <summary>
    /// Generates a cache key for repository files
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="branch">Branch name</param>
    /// <returns>Cache key</returns>
    public static string GetFilesKey(string owner, string name, string branch) => $"github:files:{owner}:{name}:{branch}";

    /// <summary>
    /// Generates a cache key for script metadata
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <param name="path">File path</param>
    /// <param name="sha">File SHA</param>
    /// <returns>Cache key</returns>
    public static string GetMetadataKey(string owner, string name, string path, string sha) => $"github:metadata:{owner}:{name}:{path}:{sha}";
}