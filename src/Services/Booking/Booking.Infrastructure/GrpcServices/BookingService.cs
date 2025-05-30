using AutoMapper;
using Booking.Application.Services;
using Booking.Domain.Specifications.Ticket;
using Common.Protos;
using Grpc.Core;

namespace Booking.Infrastructure.GrpcServices;
public class BookingService : Common.Protos.BookingGrpcService.BookingGrpcServiceBase
{
    private readonly ITicketService _ticketService;
    private readonly IMapper _mapper;
    public BookingService(ITicketService ticketService, IMapper mapper)
    {
        _ticketService = ticketService;
        _mapper = mapper;
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

    public override async Task<GetTicketPriceResponse> GetTicketPrice(GetTicketPriceRequest request, ServerCallContext context)
    {
        var price = await _ticketService.GetTicketPricesByBookingOrderAsync(Guid.Parse(request.BookingOrderId));
        return new GetTicketPriceResponse
        {
            Price = price
        };
    }

    public override async Task<GetTicketInformationResponses> GetTicketInformation(
        GetTicketInformationRequest request,
        ServerCallContext context)
    {
        var tickets = await _ticketService.GetTicketWithPassengerInfoAsync(Guid.Parse(request.BookingOrderId));
        var result = _mapper.Map<List<TicketInformation>>(tickets);
        return new GetTicketInformationResponses
        {
            Tickets = { result }
        };
    }
}
