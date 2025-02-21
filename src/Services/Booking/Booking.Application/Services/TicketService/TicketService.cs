using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;

namespace Booking.Application.Services;
public class TicketService : ITicketService
{
    private readonly IBookingUnitOfWork _bookingUnitOfWork;
    private readonly IMapper _mapper;

    public TicketService(IBookingUnitOfWork bookingUnitOfWork, IMapper mapper)
    {
        _bookingUnitOfWork = bookingUnitOfWork;
        _mapper = mapper;
    }
    public async Task AddTicketAsync(AddTicketDto addTicketDto)
    {
        try
        {
            var passenger = _mapper.Map<Ticket>(addTicketDto);
            await _bookingUnitOfWork.TicketRepository.AddAsync(passenger);
            await _bookingUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
