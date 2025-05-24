using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Common.Application.Interfaces;
using Common.Application.Services;

namespace Booking.Application.Services;
public class TicketSeatService : BaseService<TicketSeat, AddTicketSeatDto, AddTicketSeatDto, TicketSeatDto>, ITicketSeatService
{
    public TicketSeatService(
        ITicketSeatRepository repository,
        IBookingUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService) : base(repository, unitOfWork, mapper, paginationService)
    {
    }
}
