using AutoMapper;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Common.Infrastructure;
using Common.Infrastructure.Repositories;

namespace Booking.Infrastructure.Repositories;
public class TicketSeatRepository : BaseRepository<TicketSeat>, ITicketSeatRepository
{
    public TicketSeatRepository(IDataContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
