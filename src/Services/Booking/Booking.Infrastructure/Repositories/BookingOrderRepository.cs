using AutoMapper;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Common.Infrastructure;
using Common.Infrastructure.Repositories;

namespace Booking.Infrastructure.Repositories;
public class BookingOrderRepository : BaseRepository<BookingOrder>, IBookingOrderRepository
{
    public BookingOrderRepository(IDataContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

