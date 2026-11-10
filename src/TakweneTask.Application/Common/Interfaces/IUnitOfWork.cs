using TakweneTask.Application.Common.Models;

namespace TakweneTask.Application.Common.Interfaces;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    IArtistRepository Artists { get; }
    ITrackRepository Tracks { get; }
    IDspRepository Dsps { get; }
    IRepository<T> Repository<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<T>> PaginateAsync<T>(
        IQueryable<T> query,
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default) where T : class;

    Task<PagedResult<T>> PaginateAsync<T>(
        IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default) where T : class;

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);

    Task<TResult> ExecuteStrategyAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    void ClearChangeTracker();
    bool HasChanges();
    void Detach<TEntity>(TEntity entity) where TEntity : class;
}
