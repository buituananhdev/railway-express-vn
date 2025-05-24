using Common.Application.Repositories;

namespace Booking.Application.Repositories;
public interface IBookingUnitOfWork : IUnitOfWork
{
    ITicketRepository TicketRepository { get; }
    IPassengerInfoRepository PassengerInfoRepository { get; }
    ITicketSeatRepository TicketSeatRepository { get; }
}
