using Booking.Domain.Entities;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure;
public partial class BookingContext : DbContext, IDataContext
{
    public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }
    DbSet<T> IDataContext.Set<T>() => Set<T>();

    public virtual DbSet<Ticket> Tickets { get; set; } = null!;
    public virtual DbSet<PassengerInfo> PassengerInfos { get; set; } = null!;
    public virtual DbSet<TicketSeat> TicketSeats { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ticket configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.Property(e => e.TicketNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.BookingDate)
                .IsRequired();

            entity.Property(e => e.JourneyDate)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(e => e.Remarks)
                .HasMaxLength(500);

            // Indexes
            entity.HasIndex(e => e.TicketNumber)
                .IsUnique()
                .HasDatabaseName("IX_Tickets_TicketNumber");

            entity.HasIndex(e => e.TrainScheduleId)
                .HasDatabaseName("IX_Tickets_TrainScheduleId");

            entity.HasIndex(e => e.JourneyDate)
                .HasDatabaseName("IX_Tickets_JourneyDate");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Tickets_Status");

            // Relationships
            entity.HasMany(t => t.PassengerDetails)
                .WithOne(p => p.Ticket)
                .HasForeignKey(p => p.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.TicketSeats)
                .WithOne(ts => ts.Ticket)
                .HasForeignKey(ts => ts.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PassengerInfo configuration
        modelBuilder.Entity<PassengerInfo>(entity =>
        {
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .HasMaxLength(255);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.IdentityNumber)
                .HasMaxLength(50);

            entity.Property(e => e.AgeGroup)
                .HasConversion<string>()
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.TicketId)
                .HasDatabaseName("IX_PassengerInfos_TicketId");

            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_PassengerInfos_Email");

            entity.HasIndex(e => e.IdentityNumber)
                .HasDatabaseName("IX_PassengerInfos_IdentityNumber");

            // Relationship with TicketSeat (One-to-One)
            entity.HasOne(p => p.TicketSeat)
                .WithOne(ts => ts.PassengerInfo)
                .HasForeignKey<TicketSeat>(ts => ts.PassengerInfoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TicketSeat configuration
        modelBuilder.Entity<TicketSeat>(entity =>
        {
            // Indexes
            entity.HasIndex(e => e.TicketId)
                .HasDatabaseName("IX_TicketSeats_TicketId");

            entity.HasIndex(e => e.SeatId)
                .HasDatabaseName("IX_TicketSeats_SeatId");

            entity.HasIndex(e => e.PassengerInfoId)
                .HasDatabaseName("IX_TicketSeats_PassengerInfoId");

            // Composite index for unique seat per ticket
            entity.HasIndex(e => new { e.TicketId, e.SeatId })
                .IsUnique()
                .HasDatabaseName("IX_TicketSeats_TicketId_SeatId");
        });
    }
}
