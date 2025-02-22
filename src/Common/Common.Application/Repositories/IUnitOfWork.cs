using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Repositories;
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync();
    Task CommitAsync();
    void BeginTransaction();
    void Rollback();
}
