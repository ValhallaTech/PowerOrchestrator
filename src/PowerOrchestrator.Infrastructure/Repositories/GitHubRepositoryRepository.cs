using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// GitHub repository implementation
/// </summary>
public class GitHubRepositoryRepository : Repository<GitHubRepository>, IGitHubRepositoryRepository
{
    /// <summary>
    /// Initializes a new instance of the GitHubRepositoryRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public GitHubRepositoryRepository(PowerOrchestratorDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<GitHubRepository?> GetByOwnerAndNameAsync(string owner, string name)
    {
        return await _dbSet
            .Include(r => r.Scripts)
            .Include(r => r.SyncHistory)
            .FirstOrDefaultAsync(r => r.Owner == owner && r.Name == name);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubRepository>> GetByStatusAsync(RepositoryStatus status)
    {
        return await _dbSet
            .Where(r => r.Status == status)
            .OrderBy(r => r.FullName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubRepository>> GetRepositoriesNeedingSyncAsync(DateTime olderThan)
    {
        return await _dbSet
            .Where(r => r.Status == RepositoryStatus.Active && 
                       (r.LastSyncAt == null || r.LastSyncAt < olderThan))
            .OrderBy(r => r.LastSyncAt ?? DateTime.MinValue)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GitHubRepository>> GetByOwnerAsync(string owner)
    {
        return await _dbSet
            .Where(r => r.Owner == owner)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task UpdateLastSyncTimeAsync(Guid repositoryId, DateTime syncTime)
    {
        var repository = await _dbSet.FindAsync(repositoryId);
        if (repository != null)
        {
            repository.LastSyncAt = syncTime;
            repository.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task UpdateStatusAsync(Guid repositoryId, RepositoryStatus status)
    {
        var repository = await _dbSet.FindAsync(repositoryId);
        if (repository != null)
        {
            repository.Status = status;
            repository.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}