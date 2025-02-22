using Admin.Domain.Entities;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure;
public class AdminContext : DbContext, IDataContext
{
    public AdminContext(DbContextOptions<AdminContext> options) : base(options) { }

    public DbSet<Train> Trains { get; set; }
    public DbSet<TrainCar> TrainCars { get; set; }
    public DbSet<TrainSchedule> TrainSchedules { get; set; }
    public DbSet<TrainStatus> TrainStatuses { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<Seat> Seats { get; set; }

    DbSet<T> IDataContext.Set<T>() => Set<T>();

   // OnModelCreating method to configure relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships, indices, etc.

        modelBuilder.Entity<Train>()
            .HasMany(t => t.TrainCars)
            .WithOne(tc => tc.Train)
            .HasForeignKey(tc => tc.TrainId);

        modelBuilder.Entity<TrainCar>()
            .HasMany(tc => tc.Seats)
            .WithOne(s => s.TrainCar)
            .HasForeignKey(s => s.TrainCarId);

        modelBuilder.Entity<TrainSchedule>()
            .HasOne(ts => ts.DepartureStation)
            .WithMany(s => s.DepartureTrainSchedules)
            .HasForeignKey(ts => ts.DepartureStationId);

        modelBuilder.Entity<TrainSchedule>()
            .HasOne(ts => ts.ArrivalStation)
            .WithMany(s => s.ArrivalTrainSchedules)
            .HasForeignKey(ts => ts.ArrivalStationId);

        modelBuilder.Entity<TrainStatus>()
            .HasOne(ts => ts.Train)
            .WithMany()
            .HasForeignKey(ts => ts.TrainId);

        modelBuilder.Entity<TrainStatus>()
            .HasOne(ts => ts.Station)
            .WithMany()
            .HasForeignKey(ts => ts.StationId);
    }
}
