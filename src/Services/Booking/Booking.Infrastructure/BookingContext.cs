using Booking.Domain.Entities;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Booking.Infrastructure;
public partial class BookingContext : DbContext, IDataContext
{
    public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }
    DbSet<T> IDataContext.Set<T>() => Set<T>();

    public virtual DbSet<Ticket> Tickets { get; set; } = null!;
    public virtual DbSet<PassengerInfo> PassengerInfos { get; set; } = null!;

    // OnModelCreating method to configure entity relationships and constraints
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PassengerInfo entity
        modelBuilder.Entity<PassengerInfo>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.PassengerDetails)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Ticket entity
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18,2)");

            entity.PrimitiveCollection(e => e.SeatIds)
                .ElementType(guid => guid.HasConversion(new GuidToStringConverter()));
        });
    }
}
