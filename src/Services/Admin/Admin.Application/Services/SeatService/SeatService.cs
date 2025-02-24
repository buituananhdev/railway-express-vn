using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Specifications;
using AutoMapper;
using Common.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Admin.Application.Services;
public class SeatService : ISeatService
{
    private readonly BookingGrpcService.BookingGrpcServiceClient _bookingGrpcServiceClient;
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IMapper _mapper;

    public SeatService(IAdminUnitOfWork adminUnitOfWork, IMapper mapper, BookingGrpcService.BookingGrpcServiceClient bookingGrpcServiceClient)
    {
        _adminUnitOfWork = adminUnitOfWork;
        _mapper = mapper;
        _bookingGrpcServiceClient = bookingGrpcServiceClient;
    }
    public async Task<List<SeatDto>> GetSeatsByTrainCarAndScheduleAsync(
        Guid trainCarId,
        Guid trainScheduleId,
        DateTime journeyDate)
    {
        try
        {
            var specification = new SeatTrainCarIdSpecification(trainCarId);
            var seats = await _adminUnitOfWork.SeatRepository.ToListAsync(spec: specification);
            var seatDtos = _mapper.Map<List<SeatDto>>(seats);

            if (!seatDtos.Any())
            {
                return seatDtos;
            }

            var request = new BatchCheckSeatStatusRequest
            {
                ScheduleId = trainScheduleId.ToString(),
                JourneyDate = Timestamp.FromDateTime(journeyDate.ToUniversalTime()),
                SeatIds = { seatDtos.Select(s => s.Id.ToString()) }
            };

            var result = await _bookingGrpcServiceClient.BatchCheckSeatStatusAsync(request);

            foreach (var seatDto in seatDtos)
            {
                var isBooked = result.SeatStatuses.GetValueOrDefault(seatDto.Id.ToString());
                seatDto.Status = isBooked
                    ? Domain.Enums.SeatStatusEnum.Booked
                    : Domain.Enums.SeatStatusEnum.Available;
            }

            return seatDtos;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
