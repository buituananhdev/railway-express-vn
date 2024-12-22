using Admin.Domain.Entities;
using Admin.Domain.Enums;
using Admin.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class SeedData
{
    public static void Seed(AdminContext context)
    {
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                var stationsList = new[]
                {
                    new Station { Id = Guid.NewGuid(), StationName = "Hà Nội", Location = "Hà Nội", Coordinates = "105.8542,21.0285", StationOrder = 1 },
                    new Station { Id = Guid.NewGuid(), StationName = "Phủ Lý, Hà Nam", Location = "Hà Nam", Coordinates = "105.9122,20.5411", StationOrder = 2 },
                    new Station { Id = Guid.NewGuid(), StationName = "Nam Định", Location = "Nam Định", Coordinates = "106.1681,20.4241", StationOrder = 3 },
                    new Station { Id = Guid.NewGuid(), StationName = "Ninh Bình", Location = "Ninh Bình", Coordinates = "105.9757,20.2534", StationOrder = 4 },
                    new Station { Id = Guid.NewGuid(), StationName = "Thanh Hóa", Location = "Thanh Hóa", Coordinates = "105.7764,19.8075", StationOrder = 5 },
                    new Station { Id = Guid.NewGuid(), StationName = "Vinh, Nghệ An", Location = "Nghệ An", Coordinates = "105.6813,18.6796", StationOrder = 6 },
                    new Station { Id = Guid.NewGuid(), StationName = "Hà Tĩnh", Location = "Hà Tĩnh", Coordinates = "105.9057,18.3428", StationOrder = 7 },
                    new Station { Id = Guid.NewGuid(), StationName = "Vũng Áng, Hà Tĩnh", Location = "Hà Tĩnh", Coordinates = "106.3981,18.1033", StationOrder = 8 },
                    new Station { Id = Guid.NewGuid(), StationName = "Đồng Hới, Quảng Bình", Location = "Quảng Bình", Coordinates = "106.6233,17.4688", StationOrder = 9 },
                    new Station { Id = Guid.NewGuid(), StationName = "Đông Hà, Quảng Trị", Location = "Quảng Trị", Coordinates = "107.1012,16.8100", StationOrder = 10 },
                    new Station { Id = Guid.NewGuid(), StationName = "Huế, Thừa Thiên Huế", Location = "Thừa Thiên Huế", Coordinates = "107.5909,16.4637", StationOrder = 11 },
                    new Station { Id = Guid.NewGuid(), StationName = "Đà Nẵng", Location = "Đà Nẵng", Coordinates = "108.2022,16.0544", StationOrder = 12 },
                    new Station { Id = Guid.NewGuid(), StationName = "Tam Kỳ, Quảng Nam", Location = "Quảng Nam", Coordinates = "108.4740,15.5736", StationOrder = 13 },
                    new Station { Id = Guid.NewGuid(), StationName = "Quảng Ngãi", Location = "Quảng Ngãi", Coordinates = "108.8040,15.1205", StationOrder = 14 },
                    new Station { Id = Guid.NewGuid(), StationName = "Bồng Sơn, Bình Định", Location = "Bình Định", Coordinates = "109.0032,14.4456", StationOrder = 15 },
                    new Station { Id = Guid.NewGuid(), StationName = "Diêu Trì, Bình Định", Location = "Bình Định", Coordinates = "109.2207,13.8583", StationOrder = 16 },
                    new Station { Id = Guid.NewGuid(), StationName = "Tuy Hòa, Phú Yên", Location = "Phú Yên", Coordinates = "109.3209,13.0954", StationOrder = 17 },
                    new Station { Id = Guid.NewGuid(), StationName = "Tháp Chàm, Ninh Thuận", Location = "Ninh Thuận", Coordinates = "108.9848,11.5680", StationOrder = 18 },
                    new Station { Id = Guid.NewGuid(), StationName = "Phan Rí, Bình Thuận", Location = "Bình Thuận", Coordinates = "108.5970,11.1920", StationOrder = 19 },
                    new Station { Id = Guid.NewGuid(), StationName = "Mương Mán, Bình Thuận", Location = "Bình Thuận", Coordinates = "107.8922,10.9050", StationOrder = 20 },
                    new Station { Id = Guid.NewGuid(), StationName = "Long Thành, Đồng Nai", Location = "Đồng Nai", Coordinates = "106.9294,10.7858", StationOrder = 21 },
                    new Station { Id = Guid.NewGuid(), StationName = "Thủ Thiêm, TP Hồ Chí Minh", Location = "TP Hồ Chí Minh", Coordinates = "106.7064,10.7638", StationOrder = 22 }
                };

                context.Stations.AddRange(stationsList);
                context.SaveChanges();

                var trains = new List<Train>
                {
                    new Train { TrainName = "VNSE-01", Track = Track.Track1 },
                    new Train { TrainName = "VNSE-02", Track = Track.Track2 },
                    new Train { TrainName = "VNSE-03", Track = Track.Track1 },
                    new Train { TrainName = "VNSE-04", Track = Track.Track2 },
                    new Train { TrainName = "VNSE-05", Track = Track.Track1 },
                    new Train { TrainName = "VNSE-06", Track = Track.Track2 }
                };

                context.Trains.AddRange(trains);
                context.SaveChanges();

                var trainCars = new List<TrainCar>();

                for (int i = 0; i < trains.Count; i++)
                {
                    for (int j = 1; j <= 10; j++)
                    {
                        var seatType = (j == 1) ? SeatType.Business : SeatType.Standard;
                        var carNumber = $"C{j}";

                        var trainCar = new TrainCar
                        {
                            TrainId = trains[i].Id,
                            CarNumber = carNumber,
                            SeatType = seatType,
                            TotalSeats = 50
                        };

                        trainCars.Add(trainCar);
                    }
                }

                context.TrainCars.AddRange(trainCars);
                context.SaveChanges();

                var seats = new List<Seat>();
                var traincars = context.TrainCars.ToList();

                for (int k = 0; k < traincars.Count; k++)
                {
                    seats.Add(new Seat
                    {
                        TrainCarId = traincars[k].Id,
                        SeatNumber = $"{traincars[k].CarNumber}-{k + 1:D2}"
                    });
                }

                context.Seats.AddRange(seats);
                context.SaveChanges();

                var trainStatuses = new List<Admin.Domain.Entities.TrainStatus>();
                var haNoiStation = context.Stations.FirstOrDefault(x => x.Location == "Hà Nội");

                foreach (var train in trains)
                {
                    trainStatuses.Add(new Admin.Domain.Entities.TrainStatus
                    {
                        TrainId = train.Id,
                        StationId = haNoiStation.Id,
                        Status = Admin.Domain.Enums.TrainStatus.AtStation,
                        Remarks = "Train is active."
                    });
                }

                context.TrainStatuses.AddRange(trainStatuses);
                context.SaveChanges();

                var trainSchedules = new List<TrainSchedule>();

                double averageDistance = 67;
                double speed = 320;
                double travelTimeMinutes = (averageDistance / speed) * 60;
                double stopTime = 3;
                double startDelayAtStation1 = 60;
                double waitTimeAtStation23 = 0;
                var stations = context.Stations.ToList();

                foreach (var train in trains)
                {
                    DateTime departureTime = DateTime.Now;
                    DateTime currentTime = departureTime;

                    for (int i = 0; i < stations.Count; i++)
                    {
                        var departureStation = stations[i];
                        for (int j = i + 1; j < stations.Count; j++)
                        {
                            var arrivalStation = stations[j];
                            currentTime = currentTime.AddMinutes(travelTimeMinutes);

                            if (i == 0)
                            {
                                currentTime = currentTime.AddMinutes(startDelayAtStation1);
                            }
                            else if (j == stations.Count - 1)
                            {
                                currentTime = currentTime.AddMinutes(waitTimeAtStation23);
                            }
                            else
                            {
                                currentTime = currentTime.AddMinutes(stopTime);
                            }

                            trainSchedules.Add(new TrainSchedule
                            {
                                TrainId = train.Id,
                                DepartureStationId = departureStation.Id,
                                ArrivalStationId = arrivalStation.Id,
                                DepartureTime = currentTime.AddMinutes(-travelTimeMinutes),
                                ArrivalTime = currentTime
                            });
                        }
                    }
                }

                context.TrainSchedules.AddRange(trainSchedules);
                context.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while seeding data: {ex.Message}");
                transaction.Rollback();
            }
        }
    }

    public static void Main()
    {
        var connectionstring = "Server=localhost;Port=3306;Database=RailwayExpresVN_DEV1;Uid=root;Pwd=123456Aa;";
        var options = new DbContextOptionsBuilder<AdminContext>()
            .UseMySql(connectionstring, ServerVersion.AutoDetect(connectionstring))
            .Options;
        var context = new AdminContext(options);
        Seed(context);
    }
}
