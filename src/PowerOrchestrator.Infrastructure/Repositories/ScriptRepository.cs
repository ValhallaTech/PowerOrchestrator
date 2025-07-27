using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// Script repository implementation
/// </summary>
public class ScriptRepository : Repository<Script>, IScriptRepository
{
    /// <summary>
    /// Initializes a new instance of the ScriptRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public ScriptRepository(PowerOrchestratorDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Script>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Name.Contains(name))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Script>> GetActiveScriptsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Script>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        var tagList = tags.ToList();
        return await _dbSet
            .Where(s => tagList.Any(tag => s.Tags.Contains(tag)))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Script?> GetWithExecutionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Executions.OrderByDescending(e => e.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}