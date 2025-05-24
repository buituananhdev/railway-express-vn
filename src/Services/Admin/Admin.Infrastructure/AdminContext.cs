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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRelationships(modelBuilder);

        ConfigureIndexes(modelBuilder);
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
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

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // === SEAT INDEXES ===
        modelBuilder.Entity<Seat>()
            .HasIndex(s => s.TrainCarId)
            .HasDatabaseName("IX_Seats_TrainCarId");

        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.TrainCarId, s.SeatNumber })
            .HasDatabaseName("IX_Seats_TrainCarId_SeatNumber")
            .IsUnique();

        // === STATION INDEXES ===
        modelBuilder.Entity<Station>()
            .HasIndex(s => s.StationName)
            .HasDatabaseName("IX_Stations_StationName");

        modelBuilder.Entity<Station>()
            .HasIndex(s => s.CityName)
            .HasDatabaseName("IX_Stations_CityName");

        modelBuilder.Entity<Station>()
            .HasIndex(s => s.StationOrder)
            .HasDatabaseName("IX_Stations_StationOrder");

        modelBuilder.Entity<Station>()
            .HasIndex(s => new { s.CityName, s.StationName })
            .HasDatabaseName("IX_Stations_CityName_StationName");

        // === TRAIN INDEXES ===
        modelBuilder.Entity<Train>()
            .HasIndex(t => t.TrainName)
            .HasDatabaseName("IX_Trains_TrainName");

        modelBuilder.Entity<Train>()
            .HasIndex(t => t.Track)
            .HasDatabaseName("IX_Trains_Track");

        // === TRAIN CAR INDEXES ===
        modelBuilder.Entity<TrainCar>()
            .HasIndex(tc => tc.TrainId)
            .HasDatabaseName("IX_TrainCars_TrainId");

        modelBuilder.Entity<TrainCar>()
            .HasIndex(tc => new { tc.TrainId, tc.CarNumber })
            .HasDatabaseName("IX_TrainCars_TrainId_CarNumber");

        modelBuilder.Entity<TrainCar>()
            .HasIndex(tc => tc.SeatType)
            .HasDatabaseName("IX_TrainCars_SeatType");

        // === TRAIN SCHEDULE INDEXES ===
        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => ts.TrainId)
            .HasDatabaseName("IX_TrainSchedules_TrainId");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => ts.DepartureStationId)
            .HasDatabaseName("IX_TrainSchedules_DepartureStationId");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => ts.ArrivalStationId)
            .HasDatabaseName("IX_TrainSchedules_ArrivalStationId");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => new { ts.DepartureStationId, ts.ArrivalStationId })
            .HasDatabaseName("IX_TrainSchedules_DepartureStationId_ArrivalStationId");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => ts.DepartureTime)
            .HasDatabaseName("IX_TrainSchedules_DepartureTime");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => ts.ArrivalTime)
            .HasDatabaseName("IX_TrainSchedules_ArrivalTime");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => ts.IsReturnTrip)
            .HasDatabaseName("IX_TrainSchedules_IsReturnTrip");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => new { ts.DepartureStationId, ts.DepartureTime, ts.IsReturnTrip })
            .HasDatabaseName("IX_TrainSchedules_DepartureStationId_DepartureTime_IsReturnTrip");

        modelBuilder.Entity<TrainSchedule>()
            .HasIndex(ts => new { ts.ArrivalStationId, ts.ArrivalTime })
            .HasDatabaseName("IX_TrainSchedules_ArrivalStationId_ArrivalTime");

        // === TRAIN STATUS INDEXES ===
        modelBuilder.Entity<TrainStatus>()
            .HasIndex(ts => ts.TrainId)
            .HasDatabaseName("IX_TrainStatuses_TrainId");

        modelBuilder.Entity<TrainStatus>()
            .HasIndex(ts => ts.StationId)
            .HasDatabaseName("IX_TrainStatuses_StationId");

        modelBuilder.Entity<TrainStatus>()
            .HasIndex(ts => ts.Status)
            .HasDatabaseName("IX_TrainStatuses_Status");

        modelBuilder.Entity<TrainStatus>()
            .HasIndex(ts => new { ts.StationId, ts.Status })
            .HasDatabaseName("IX_TrainStatuses_StationId_Status");

        modelBuilder.Entity<TrainStatus>()
            .HasIndex(ts => new { ts.TrainId, ts.StationId })
            .HasDatabaseName("IX_TrainStatuses_TrainId_StationId");
    }
}
