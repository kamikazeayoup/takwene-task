using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.Common.Models;
using TakweneTask.Infrastructure.Persistence.Repositories;

namespace TakweneTask.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction? _currentTransaction;

    private IArtistRepository? _artists;
    private ITrackRepository? _tracks;
    private IDspRepository? _dsps;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IArtistRepository Artists => _artists ??= new ArtistRepository(_dbContext, this);
    public ITrackRepository Tracks => _tracks ??= new TrackRepository(_dbContext, this);
    public IDspRepository Dsps => _dsps ??= new DspRepository(_dbContext);

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new Repository<T>(_dbContext);
            _repositories[type] = repo;
        }

        return (IRepository<T>)repo;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<T>> PaginateAsync<T>(
        IQueryable<T> query,
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(paginationParams);

        var pageNumber = paginationParams.PageNumber < 1 ? 1 : paginationParams.PageNumber;
        var pageSize = paginationParams.PageSize < 1 ? 10 : (paginationParams.PageSize > 100 ? 100 : paginationParams.PageSize);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public Task<PagedResult<T>> PaginateAsync<T>(
        IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default) where T : class
    {
        return PaginateAsync(query, new PaginationParams(pageNumber, pageSize), cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<TResult> ExecuteStrategyAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(operation);
    }

    public void ClearChangeTracker()
    {
        _dbContext.ChangeTracker.Clear();
    }

    public bool HasChanges()
    {
        return _dbContext.ChangeTracker.HasChanges();
    }

    public void Detach<TEntity>(TEntity entity) where TEntity : class
    {
        _dbContext.Entry(entity).State = EntityState.Detached;
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
