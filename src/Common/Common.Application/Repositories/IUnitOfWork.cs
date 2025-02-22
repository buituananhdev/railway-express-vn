namespace Common.Application.Repositories;
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync();
    Task CommitAsync();
    void BeginTransaction();
    void Rollback();
}
