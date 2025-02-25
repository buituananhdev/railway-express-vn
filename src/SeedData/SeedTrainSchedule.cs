using Admin.Domain.Entities;
using Admin.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrainScheduleCalculator
{
    private const double MAX_SPEED = 320.0; // km/h
    private const int STOP_TIME_SECONDS = 90; // 1 minute 30 seconds
    private readonly DateTime FIRST_DEPARTURE_TIME = new DateTime(2024, 12, 28, 5, 0, 0);
    private readonly DateTime RETURN_DEPARTURE_TIME = new DateTime(2024, 12, 28, 10, 19, 0);
    public readonly AdminContext _context;
    public TrainScheduleCalculator(AdminContext context)
    {
        _context = context;
    }

    public async Task<List<TrainSchedule>> CalculateSchedule(int fromStationId, int toStationId)
    {
        var result = new List<TrainSchedule>();

        var _stations = await _context.Stations.ToListAsync();
        var trains = await _context.Trains.ToListAsync();
        var fromStation = _stations.Find(s => s.StationOrder == fromStationId);
        var toStation = _stations.Find(s => s.StationOrder == toStationId);

        if (fromStation == null || toStation == null)
            throw new ArgumentException("ID ga không hợp lệ");

        // Xác định hành trình (chiều đi hay chiều về)
        bool isReturnTrip = IsReturnTrip(fromStation, toStation);

        // Kiểm tra tính hợp lệ của hành trình
        ValidateJourney(fromStation, toStation, isReturnTrip);

        for (int i = 0; i < trains.Count; i++)
        {
            var groupIndex = i / 3; // 0 for first group, 1 for second group, 2 for third group
            var baseTime = isReturnTrip ? RETURN_DEPARTURE_TIME.AddHours(groupIndex) : FIRST_DEPARTURE_TIME.AddHours(groupIndex);

            // Tính thời gian đến ga đi
            var timeToFromStation = CalculateTimeToStation(0, fromStation.KilometricPoint);
            var arrivalTimeAtFromStation = baseTime.Add(timeToFromStation);

            var departureTimeFromStation = arrivalTimeAtFromStation.AddSeconds(STOP_TIME_SECONDS);


            // Tính thời gian đến ga đích
            var travelTime = CalculateTimeToStation(fromStation.KilometricPoint, toStation.KilometricPoint);
            var arrivalTimeAtToStation = departureTimeFromStation.Add(travelTime);

            var trainSchedule = new TrainSchedule
            {
                TrainId = trains[i].Id,
                Train = trains[i],
                DepartureStationId = fromStation.Id,
                DepartureStation = fromStation,
                ArrivalStationId = toStation.Id,
                ArrivalStation = toStation,
                DepartureTime = departureTimeFromStation,
                ArrivalTime = arrivalTimeAtToStation
            };


            result.Add(trainSchedule);
        }

        return result;
    }

    private TimeSpan CalculateTimeToStation(double fromDistance, double toDistance)
    {
        var distance = Math.Abs(toDistance - fromDistance);
        return TimeSpan.FromHours(distance / MAX_SPEED);
    }

    private bool IsReturnTrip(Station fromStation, Station toStation)
    {
        // Nếu ga đi có số thứ tự lớn hơn ga đến, đó là chuyến về
        return fromStation.StationOrder > toStation.StationOrder;
    }

    private void ValidateJourney(Station fromStation, Station toStation, bool isReturnTrip)
    {
        // Kiểm tra thứ tự ga trong hành trình
        if (fromStation.StationOrder == toStation.StationOrder)
            throw new ArgumentException("Ga đi và ga đến không được trùng nhau");

        // Kiểm tra hành trình hợp lệ
        if (isReturnTrip)
        {
            if (fromStation.Id <= toStation.Id)
                throw new ArgumentException("Thứ tự ga không hợp lệ cho chuyến về");
        }
        else
        {
            if (fromStation.Id >= toStation.Id)
                throw new ArgumentException("Thứ tự ga không hợp lệ cho chuyến đi");
        }
    }

    public string FormatSchedule(List<TrainSchedule> schedules)
    {
        var result = "";
        var direction = schedules.First().IsReturnTrip ? "về" : "đi";
        result += $"Lịch trình chuyến {direction}:\n";

        foreach (var schedule in schedules)
        {
            result += $"Tàu {schedule.TrainId}: {schedule.DepartureTime:HH:mm:ss} → {schedule.ArrivalTime:HH:mm:ss}\n";
        }
        return result;
    }
    private bool IsFirstOrLastStation(Station station, bool isReturnTrip)
    {
        if (isReturnTrip)
        {
            // Trong chuyến về, ga 23 là ga đầu và ga 1 là ga cuối
            return station.StationOrder == 23 || station.StationOrder == 1;
        }
        else
        {
            // Trong chuyến đi, ga 1 là ga đầu và ga 23 là ga cuối
            return station.StationOrder == 1 || station.StationOrder == 23;
        }
    }

    public async Task InsertAllSchedulesToDatabase()
    {
        var schedules = new List<TrainSchedule>();
        var stations = await _context.Stations.OrderBy(s => s.StationOrder).ToListAsync();
        var trains = await _context.Trains.ToListAsync();

        // Vòng đi (từ ga thấp đến ga cao)
        Console.WriteLine("Đang tạo lịch trình chiều đi...");
        for (int fromOrder = 1; fromOrder <= 23; fromOrder++)
        {
            for (int toOrder = fromOrder + 1; toOrder <= 23; toOrder++)
            {
                var fromStation = stations.First(s => s.StationOrder == fromOrder);
                var toStation = stations.First(s => s.StationOrder == toOrder);

                for (int i = 0; i < trains.Count; i++)
                {
                    var groupIndex = i / 3;
                    var baseTime = FIRST_DEPARTURE_TIME.AddHours(groupIndex);

                    DateTime departureTimeFromStation;
                    if (IsFirstOrLastStation(fromStation, true))
                    {
                        departureTimeFromStation = baseTime;
                    }
                    else
                    {
                        // Corrected starting point for return trips
                        var timeToFromStation = CalculateTimeToStation(1541, fromStation.KilometricPoint);
                        var arrivalTimeAtFromStation = baseTime.Add(timeToFromStation);
                        departureTimeFromStation = arrivalTimeAtFromStation.AddSeconds(STOP_TIME_SECONDS);
                    }

                    var travelTime = CalculateTimeToStation(fromStation.KilometricPoint, toStation.KilometricPoint);
                    var arrivalTimeAtToStation = departureTimeFromStation.Add(travelTime);

                    var schedule = new TrainSchedule
                    {
                        TrainId = trains[i].Id,
                        Train = trains[i],
                        DepartureStationId = fromStation.Id,
                        DepartureStation = fromStation,
                        ArrivalStationId = toStation.Id,
                        ArrivalStation = toStation,
                        Distance = Math.Abs(toStation.KilometricPoint - fromStation.KilometricPoint),
                        DepartureTime = departureTimeFromStation,
                        ArrivalTime = arrivalTimeAtToStation,
                        IsReturnTrip = false
                    };

                    schedules.Add(schedule);
                }
            }
        }

        // Vòng về (từ ga cao đến ga thấp)
        Console.WriteLine("Đang tạo lịch trình chiều về...");
        for (int fromOrder = 23; fromOrder >= 1; fromOrder--)
        {
            for (int toOrder = fromOrder - 1; toOrder >= 1; toOrder--)
            {
                var fromStation = stations.First(s => s.StationOrder == fromOrder);
                var toStation = stations.First(s => s.StationOrder == toOrder);

                for (int i = 0; i < trains.Count; i++)
                {
                    var groupIndex = i / 3;
                    var baseTime = RETURN_DEPARTURE_TIME.AddHours(groupIndex);

                    DateTime departureTimeFromStation;
                    if (IsFirstOrLastStation(fromStation, true))
                    {
                        // Ga đầu không có thời gian chờ
                        departureTimeFromStation = baseTime;
                    }
                    else
                    {
                        var timeToFromStation = CalculateTimeToStation(0, fromStation.KilometricPoint);
                        var arrivalTimeAtFromStation = baseTime.Add(timeToFromStation);
                        departureTimeFromStation = arrivalTimeAtFromStation.AddSeconds(STOP_TIME_SECONDS);
                    }

                    var travelTime = CalculateTimeToStation(fromStation.KilometricPoint, toStation.KilometricPoint);
                    var arrivalTimeAtToStation = departureTimeFromStation.Add(travelTime);

                    var schedule = new TrainSchedule
                    {
                        TrainId = trains[i].Id,
                        Train = trains[i],
                        DepartureStationId = fromStation.Id,
                        DepartureStation = fromStation,
                        ArrivalStationId = toStation.Id,
                        ArrivalStation = toStation,
                        Distance = Math.Abs(toStation.KilometricPoint - fromStation.KilometricPoint),
                        DepartureTime = departureTimeFromStation,
                        ArrivalTime = arrivalTimeAtToStation,
                        IsReturnTrip = true
                    };

                    schedules.Add(schedule);
                }
            }
        }

        try
        {
            Console.WriteLine($"Bắt đầu lưu {schedules.Count} lịch trình vào database...");
            await _context.TrainSchedules.AddRangeAsync(schedules);
            await _context.SaveChangesAsync();
            Console.WriteLine("Đã lưu thành công tất cả lịch trình!");

            // In thống kê
            var outboundCount = schedules.Count(s => !s.IsReturnTrip);
            var returnCount = schedules.Count(s => s.IsReturnTrip);
            Console.WriteLine("\n=== THỐNG KÊ ===");
            Console.WriteLine($"Số lịch trình chiều đi: {outboundCount}");
            Console.WriteLine($"Số lịch trình chiều về: {returnCount}");
            Console.WriteLine($"Tổng số lịch trình: {schedules.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi lưu vào database: {ex.Message}");
            throw;
        }
    }
}
