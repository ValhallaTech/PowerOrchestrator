using Microsoft.EntityFrameworkCore.Storage;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Infrastructure.Data;
using PowerOrchestrator.Infrastructure.Repositories;

namespace PowerOrchestrator.Infrastructure;

/// <summary>
/// Unit of Work implementation for managing database transactions and repository coordination
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PowerOrchestratorDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Lazy loading of repositories
    private IScriptRepository? _scripts;
    private IExecutionRepository? _executions;
    private IAuditLogRepository? _auditLogs;
    private IHealthCheckRepository? _healthChecks;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class
    /// </summary>
    /// <param name="context">The database context</param>
    public UnitOfWork(PowerOrchestratorDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public IScriptRepository Scripts => _scripts ??= new ScriptRepository(_context);

    /// <inheritdoc />
    public IExecutionRepository Executions => _executions ??= new ExecutionRepository(_context);

    /// <inheritdoc />
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    /// <inheritdoc />
    public IHealthCheckRepository HealthChecks => _healthChecks ??= new HealthCheckRepository(_context);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to rollback.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the unit of work
    /// </summary>
    /// <param name="disposing">Whether disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context?.Dispose();
            _disposed = true;
        }
    }
}