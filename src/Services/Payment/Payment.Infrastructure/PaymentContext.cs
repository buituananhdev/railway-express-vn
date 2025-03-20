using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;

namespace Payment.Infrastructure;
public partial class PaymentContext : DbContext, IDataContext
{
    public PaymentContext(DbContextOptions<PaymentContext> options) : base(options) { }
    DbSet<T> IDataContext.Set<T>() => Set<T>();

    public virtual DbSet<PaymentRecord> PaymentRecordr { get; set; } = null!;

    // OnModelCreating method to configure entity relationships and constraints
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentRecord>()
            .ToTable("PaymentRecords");
    }
}
