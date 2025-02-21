using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure
{
    public partial class UserManagementContext : DbContext, IDataContext
    {
        public UserManagementContext(DbContextOptions<UserManagementContext> options) : base(options) { }
        DbSet<T> IDataContext.Set<T>() => Set<T>();

        public virtual DbSet<Passenger> Passengers { get; set; }
    }
}
