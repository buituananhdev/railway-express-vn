using System.Collections.Generic;
using Admin.Application.Dtos;
using Admin.Application.Services;
using AutoMapper;
using Common.Protos;
using Grpc.Core;

namespace Admin.Infrastructure.GrpcServices;
public class AdminService : Common.Protos.AdminGrpcService.AdminGrpcServiceBase
{
    private readonly IMapper _mapper;
    private readonly ISeatService _seatService;
    private readonly IStationService _stationService;
    private readonly ITrainScheduleService _trainScheduleService;
    public AdminService(IMapper mapper, ISeatService seatService, IStationService stationService, ITrainScheduleService trainScheduleService)
    {
        _mapper = mapper;
        _seatService = seatService;
        _stationService = stationService;
        _trainScheduleService = trainScheduleService;
    }
    public override async Task<GetSeatInformationResponse> GetSeatInformation(
        GetSeatInformationRequest request,
        ServerCallContext context)
    {
        var seatId = Guid.Parse(request.SeatId);
        var results = await _seatService.GetSeatWithTrainInformationAsync(seatId);

        return _mapper.Map<GetSeatInformationResponse>(results);
    }

    public override async Task<GetStationInformationResponse> GetStationInformation(GetStationInformationRequest request, ServerCallContext context)
    {
        var results = await _stationService.GetStationByNameAsync(request.StationName);
        return new GetStationInformationResponse
        {
            StationId = results.Id.ToString()
        };
    }

    public override async Task<GetTrainScheduleResponse> GetTrainSchedule(
    GetTrainScheduleRequest request,
    ServerCallContext context)
    {
        var dto = new GetTrainSchedulesDto
        {
            DepartureStationId = Guid.Parse(request.DepartureStationId),
            ArrivalStationId = Guid.Parse(request.ArrivalStationId),
            DepartureDate = request.DepartureDate.ToDateTime()
        };

        var schedules = await _trainScheduleService.GetTrainSchedulesAsync(dto)
                                                   .ConfigureAwait(false);

        if (schedules.Count == 0)
            throw new RpcException(new Status(StatusCode.NotFound,
                               "Không tìm thấy lịch tàu phù hợp"));

        var target = request.DepartureTime.ToTimeSpan();
        var bestMatch = schedules.MinBy(s =>
            Math.Abs((s.DepartureTime.TimeOfDay - target).TotalMinutes));

        return new GetTrainScheduleResponse
        {
            TrainScheduleId = bestMatch.Id.ToString(),
            TrainId = bestMatch.Train.Id.ToString(),
            BasePrice = (double)bestMatch.FromPrice
        };
    }

    public override async Task<GetRandomeAvailableSeatResponse> GetRandomeAvailableSeat(GetRandomeAvailableSeatRequest request, ServerCallContext context)
    {
        var trainCarId = Guid.Parse(request.TrainId);
        var scheduleId = Guid.Parse(request.ScheduleId);
        var journeyDate = request.JourneyDate.ToDateTime();
        var availableSeats = await _seatService.GetRandomeAvailableSeatAsync(trainCarId, scheduleId, journeyDate, request.Quantity);
        var response = new GetRandomeAvailableSeatResponse();
        response.SeatIds.AddRange(availableSeats.ConvertAll(seatId => seatId.ToString()));
        return response;
    }

    public override async Task<GetTrainScheduleInformationResponse> GetTrainScheduleInformation(
        GetTrainScheduleInformationRequest request,
        ServerCallContext context)
    {
        var scheduleId = Guid.Parse(request.ScheduleId);
        var results = await _trainScheduleService.GetTrainScheduleInformationAsync(scheduleId);
        return _mapper.Map<GetTrainScheduleInformationResponse>(results);
    }
}
