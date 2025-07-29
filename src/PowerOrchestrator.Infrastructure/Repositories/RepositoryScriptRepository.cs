using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Data;

namespace PowerOrchestrator.Infrastructure.Repositories;

/// <summary>
/// Repository script repository implementation
/// </summary>
public class RepositoryScriptRepository : Repository<RepositoryScript>, IRepositoryScriptRepository
{
    /// <summary>
    /// Initializes a new instance of the RepositoryScriptRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public RepositoryScriptRepository(PowerOrchestratorDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RepositoryScript>> GetByRepositoryIdAsync(Guid repositoryId)
    {
        return await _dbSet
            .Include(rs => rs.Repository)
            .Include(rs => rs.Script)
            .Where(rs => rs.RepositoryId == repositoryId)
            .OrderBy(rs => rs.FilePath)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RepositoryScript>> GetByRepositoryAndBranchAsync(Guid repositoryId, string branch)
    {
        return await _dbSet
            .Include(rs => rs.Repository)
            .Include(rs => rs.Script)
            .Where(rs => rs.RepositoryId == repositoryId && rs.Branch == branch)
            .OrderBy(rs => rs.FilePath)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<RepositoryScript?> GetByPathAndBranchAsync(Guid repositoryId, string filePath, string branch)
    {
        return await _dbSet
            .Include(rs => rs.Repository)
            .Include(rs => rs.Script)
            .FirstOrDefaultAsync(rs => rs.RepositoryId == repositoryId && 
                                      rs.FilePath == filePath && 
                                      rs.Branch == branch);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RepositoryScript>> GetByShaAsync(string sha)
    {
        return await _dbSet
            .Include(rs => rs.Repository)
            .Include(rs => rs.Script)
            .Where(rs => rs.Sha == sha)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RepositoryScript>> GetModifiedAfterAsync(Guid repositoryId, DateTime modifiedAfter)
    {
        return await _dbSet
            .Include(rs => rs.Repository)
            .Include(rs => rs.Script)
            .Where(rs => rs.RepositoryId == repositoryId && rs.LastModified > modifiedAfter)
            .OrderBy(rs => rs.LastModified)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> DeleteByRepositoryIdAsync(Guid repositoryId)
    {
        var scripts = await _dbSet
            .Where(rs => rs.RepositoryId == repositoryId)
            .ToListAsync();

        _dbSet.RemoveRange(scripts);
        await _context.SaveChangesAsync();
        return scripts.Count;
    }

    /// <inheritdoc />
    public async Task<int> DeleteByRepositoryAndBranchAsync(Guid repositoryId, string branch)
    {
        var scripts = await _dbSet
            .Where(rs => rs.RepositoryId == repositoryId && rs.Branch == branch)
            .ToListAsync();

        _dbSet.RemoveRange(scripts);
        await _context.SaveChangesAsync();
        return scripts.Count;
    }

    /// <inheritdoc />
    public async Task UpdateShaAndModifiedAsync(Guid id, string sha, DateTime lastModified)
    {
        var repositoryScript = await _dbSet.FindAsync(id);
        if (repositoryScript != null)
        {
            repositoryScript.Sha = sha;
            repositoryScript.LastModified = lastModified;
            repositoryScript.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}