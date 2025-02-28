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
                    new Station { Id = Guid.NewGuid(), StationName = "Ngọc Hồi", CityName = "Hà Nội", Location = "Xã Liên Ninh và Ngọc Hồi, huyện Thanh Trì", KilometricPoint = 0, Coordinates = "105.8542,21.0285", StationOrder = 1 },
                    new Station { Id = Guid.NewGuid(), StationName = "Phủ Lý", CityName = "Hà Nam", Location = "Xã Liêm Tuyền và Liêm Tiết, TP Phủ Lý", KilometricPoint = 44, Coordinates = "105.9217,20.9274", StationOrder = 2 },
                    new Station { Id = Guid.NewGuid(), StationName = "Nam Định", CityName = "Nam Định", Location = "Xã Mỹ Hưng, huyện Mỹ Lộc", KilometricPoint = 68, Coordinates = "106.1610,20.4388", StationOrder = 3 },
                    new Station { Id = Guid.NewGuid(), StationName = "Ninh Bình", CityName = "Ninh Bình", Location = "Xã Khánh Thượng, huyện Yên Mô", KilometricPoint = 103, Coordinates = "105.9742,20.2501", StationOrder = 4 },
                    new Station { Id = Guid.NewGuid(), StationName = "Thanh Hóa", CityName = "Thanh Hóa", Location = "Xã Đông Tân và Đông Lĩnh, TP Thanh Hóa", KilometricPoint = 150, Coordinates = "105.7723,19.8075", StationOrder = 5 },
                    new Station { Id = Guid.NewGuid(), StationName = "Vinh", CityName = "Nghệ An", Location = "Xã Hưng Tây, huyện Hưng Nguyên", KilometricPoint = 281, Coordinates = "105.6757,18.6701", StationOrder = 6 },
                    new Station { Id = Guid.NewGuid(), StationName = "Hà Tĩnh", CityName = "Hà Tĩnh", Location = "Xã Thạch Đài, huyện Thạch Hà", KilometricPoint = 332, Coordinates = "105.9056,18.3428", StationOrder = 7 },
                    new Station { Id = Guid.NewGuid(), StationName = "Vũng Áng", CityName = "Hà Tĩnh", Location = "Xã Kỳ Hoa, TX Kỳ Anh", KilometricPoint = 390, Coordinates = "106.2673,18.0914", StationOrder = 8 },
                    new Station { Id = Guid.NewGuid(), StationName = "Đồng Hới", CityName = "Quảng Bình", Location = "Xã Nghĩa Ninh, TP Đồng Hới", KilometricPoint = 467, Coordinates = "106.6232,17.4784", StationOrder = 9 },
                    new Station { Id = Guid.NewGuid(), StationName = "Đông Hà", CityName = "Quảng Trị", Location = "Phường Đông Lương, TP Đông Hà", KilometricPoint = 556, Coordinates = "107.0973,16.8162", StationOrder = 10 },
                    new Station { Id = Guid.NewGuid(), StationName = "Huế", CityName = "TT Huế", Location = "Xã Phú Mỹ, huyện Phú Vang", KilometricPoint = 621, Coordinates = "107.5828,16.4637", StationOrder = 11 },
                    new Station { Id = Guid.NewGuid(), StationName = "Đà Nẵng", CityName = "Đà Nẵng", Location = "Xã Hòa Sơn, huyện Hòa Vang", KilometricPoint = 701, Coordinates = "108.2203,16.0758", StationOrder = 12 },
                    new Station { Id = Guid.NewGuid(), StationName = "Tam Kỳ", CityName = "Quảng Nam", Location = "Phường Trường Xuân, TP Tam Kỳ", KilometricPoint = 769, Coordinates = "108.4831,15.5736", StationOrder = 13 },
                    new Station { Id = Guid.NewGuid(), StationName = "Quảng Ngãi", CityName = "Quảng Ngãi", Location = "Phường Quảng Phú, TP Quảng Ngãi và xã Nghĩa Kỳ, Tư Nghĩa", KilometricPoint = 830, Coordinates = "108.8093,15.1160", StationOrder = 14 },
                    new Station { Id = Guid.NewGuid(), StationName = "Bồng Sơn", CityName = "Bình Định", Location = "Xã Hoài Tân và TT Bồng Sơn, huyện Hoài Nhơn", KilometricPoint = 910, Coordinates = "109.0342,14.4474", StationOrder = 15 },
                    new Station { Id = Guid.NewGuid(), StationName = "Diêu Trì", CityName = "Bình Định", Location = "Xã Phước An, huyện Tuy Phước", KilometricPoint = 987, Coordinates = "109.1556,13.8294", StationOrder = 16 },
                    new Station { Id = Guid.NewGuid(), StationName = "Tuy Hòa", CityName = "Phú Yên", Location = "Xã Hòa Thành, thị xã Đông Hòa", KilometricPoint = 1080, Coordinates = "109.2970,13.0954", StationOrder = 17 },
                    new Station { Id = Guid.NewGuid(), StationName = "Diên Khánh", CityName = "Khánh Hòa", Location = "Xã Diên Thành, huyện Diên Khánh", KilometricPoint = 1180, Coordinates = "109.1080,12.2597", StationOrder = 18 },
                    new Station { Id = Guid.NewGuid(), StationName = "Tháp Chàm", CityName = "Ninh Thuận", Location = "Phường Phước Mỹ, TP Phan Rang", KilometricPoint = 1258, Coordinates = "108.9606,11.5833", StationOrder = 19 },
                    new Station { Id = Guid.NewGuid(), StationName = "Phan Rí", CityName = "Bình Thuận", Location = "Xã Phan Hòa, huyện Bắc Bình", KilometricPoint = 1324, Coordinates = "108.4873,11.0538", StationOrder = 20 },
                    new Station { Id = Guid.NewGuid(), StationName = "Mương Mán", CityName = "Bình Thuận", Location = "Xã Mương Mán, huyện Hàm Thuận Nam", KilometricPoint = 1394, Coordinates = "108.1681,10.9257", StationOrder = 21 },
                    new Station { Id = Guid.NewGuid(), StationName = "Long Thành", CityName = "Đồng Nai", Location = "Xã Bình Sơn, huyện Long Thành", KilometricPoint = 1506, Coordinates = "107.0047,10.7542", StationOrder = 22 },
                    new Station { Id = Guid.NewGuid(), StationName = "Thủ Thiêm", CityName = "TP.HCM", Location = "Phường An Phú, TP Thủ Đức", KilometricPoint = 1541, Coordinates = "106.7472,10.7914", StationOrder = 23 }
                };

                context.Stations.AddRange(stationsList);
                context.SaveChanges();

                var trains = new List<Train>
                {
                    new Train { TrainName = "VNSE-01", Track = Track.Track1 },
                    new Train { TrainName = "VNSE-02", Track = Track.Track2 },
                    new Train { TrainName = "VNSE-03", Track = Track.Track3 },
                    new Train { TrainName = "VNSE-04", Track = Track.Track1 },
                    new Train { TrainName = "VNSE-05", Track = Track.Track2 },
                    new Train { TrainName = "VNSE-06", Track = Track.Track3 },
                    new Train { TrainName = "VNSE-07", Track = Track.Track1 },
                    new Train { TrainName = "VNSE-08", Track = Track.Track2 },
                    new Train { TrainName = "VNSE-09", Track = Track.Track3 }

                };

                context.Trains.AddRange(trains);
                context.SaveChanges();

                var trainCars = new List<TrainCar>();

                for (int i = 0; i < trains.Count; i++)
                {
                    for (int j = 1; j <= 10; j++)
                    {
                        var seatType = (j == 1) ? SeatType.Business : SeatType.Standard;
                        var carNumber = $"{j}";

                        var trainCar = new TrainCar
                        {
                            TrainId = trains[i].Id,
                            Description = (j == 1) ? "Business Car" : "Standard Car",
                            CarNumber = carNumber,
                            SeatType = seatType,
                            TotalSeats = 52
                        };

                        trainCars.Add(trainCar);
                    }
                }

                context.TrainCars.AddRange(trainCars);
                context.SaveChanges();

                var seats = new List<Seat>();
                var traincars = context.TrainCars.ToList();

                foreach (var traincar in traincars)
                {
                    for (int seatNum = 1; seatNum <= traincar.TotalSeats; seatNum++)
                    {
                        seats.Add(new Seat
                        {
                            TrainCarId = traincar.Id,
                            SeatNumber = seatNum.ToString()
                        });
                    }
                }

                context.Seats.AddRange(seats);
                context.SaveChanges();

                var trainStatuses = new List<Admin.Domain.Entities.TrainStatus>();
                var firstStation = context.Stations.FirstOrDefault(x => x.StationOrder == 1);

                foreach (var train in trains)
                {
                    trainStatuses.Add(new Admin.Domain.Entities.TrainStatus
                    {
                        TrainId = train.Id,
                        StationId = firstStation.Id,
                        Status = Admin.Domain.Enums.TrainStatus.AtStation,
                        Remarks = "Train is active."
                    });
                }

                context.TrainStatuses.AddRange(trainStatuses);
                context.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while seeding data: {ex}");
                transaction.Rollback();
            }
        }
    }

    public static async Task Main()
    {
        var connectionstring = "Server=localhost;Port=3306;Database=RailwayExpresVN_DEV1;Uid=root;Pwd=123456Aa;";
        var options = new DbContextOptionsBuilder<AdminContext>()
            .UseMySql(connectionstring, ServerVersion.AutoDetect(connectionstring))
            .Options;

        using (var context = new AdminContext(options))
        {
            Seed(context);
            var calculator = new TrainScheduleCalculator(context);
            try
            {
                // Clear existing schedules (optional)
                var existingSchedules = await context.TrainSchedules.ToListAsync();
                if (existingSchedules.Any())
                {
                    Console.WriteLine("Xóa lịch trình cũ...");
                    context.TrainSchedules.RemoveRange(existingSchedules);
                    await context.SaveChangesAsync();
                }

                // Insert all schedules
                await calculator.InsertAllSchedulesToDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
