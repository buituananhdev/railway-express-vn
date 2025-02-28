using Booking.Application.Services;
using Common.Protos;
using Grpc.Core;

namespace Booking.Infrastructure.GrpcServices;
public class BookingService : Common.Protos.BookingGrpcService.BookingGrpcServiceBase
{
    private readonly ITicketService _ticketService;

    public BookingService(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public override async Task<BatchCheckSeatStatusResponse> BatchCheckSeatStatus(
        BatchCheckSeatStatusRequest request,
        ServerCallContext context)
    {
        var scheduleId = Guid.Parse(request.ScheduleId);
        var journeyDate = request.JourneyDate.ToDateTime();

        var seatIds = request.SeatIds.Select(Guid.Parse).ToList();
        var results = await _ticketService.AreSeatsBookedForScheduleAsync(seatIds, scheduleId, journeyDate);

        return new BatchCheckSeatStatusResponse
        {
            SeatStatuses = { results.ToDictionary(r => r.Key.ToString(), r => r.Value) }
        };
    }
}
