using Booking.Domain.Entities;
using Common.Application.Repositories;

namespace Booking.Application.Repositories;
public interface ITicketRepository : IBaseRepository<Ticket>
{
}
