using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Passenger.Infrastructure
{
    public partial class UserManagementContext : DbContext, IDataContext
    {
        public UserManagementContext(DbContextOptions<UserManagementContext> options) : base(options) { }
        DbSet<T> IDataContext.Set<T>() => Set<T>();

        public virtual DbSet<Passenger.Domain.Entities.Passenger> Passengers { get; set; }
    }
}
