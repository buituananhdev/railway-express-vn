using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure;

public partial class UserManagementContext : DbContext, IDataContext
{
    public UserManagementContext(DbContextOptions<UserManagementContext> options) : base(options) { }
    DbSet<T> IDataContext.Set<T>() => Set<T>();

    public virtual DbSet<Passenger> Passengers { get; set; }
    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    // OnModelCreating method to configure relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.UserAccount)
            .WithMany()
            .HasForeignKey(p => p.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
