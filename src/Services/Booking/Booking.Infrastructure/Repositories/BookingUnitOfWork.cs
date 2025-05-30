using Booking.Application.Repositories;
using Common.Infrastructure;
using Common.Infrastructure.Repositories;

namespace Booking.Infrastructure.Repositories;
public class BookingUnitOfWork : UnitOfWork, IBookingUnitOfWork
{
    public ITicketRepository TicketRepository { get; private set; }
    public IPassengerInfoRepository PassengerInfoRepository { get; private set; }
    public ITicketSeatRepository TicketSeatRepository { get; private set; }
    public IBookingOrderRepository BookingOrderRepository { get; private set; }
    public BookingUnitOfWork(
        IDataContext context,
        ITicketRepository ticketRepository,
        IPassengerInfoRepository passengerInfoRepository,
        ITicketSeatRepository ticketSeatRepository) : base(context)
    {
        TicketRepository = ticketRepository;
        PassengerInfoRepository = passengerInfoRepository;
        TicketSeatRepository = ticketSeatRepository;
    }
}
