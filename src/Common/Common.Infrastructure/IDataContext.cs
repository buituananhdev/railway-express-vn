using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Common.Infrastructure;
public interface IDataContext : IDisposable
{
    DbSet<T> Set<T>() where T : class;
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
