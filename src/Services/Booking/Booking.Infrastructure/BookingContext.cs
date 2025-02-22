using Booking.Domain.Entities;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure;
public partial class BookingContext : DbContext, IDataContext
{
    public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }
    DbSet<T> IDataContext.Set<T>() => Set<T>();

    public virtual DbSet<Ticket> Tickets { get; set; }
    public virtual DbSet<PassengerInfo> PassengerInfos { get; set; }

    // OnModelCreating method to configure relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PassengerInfo>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            entity.Property(e => e.IdentityNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.Email)
                .HasMaxLength(100);

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.PassengerDetails)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Remarks)
                .HasMaxLength(500);
        });
    }
}
