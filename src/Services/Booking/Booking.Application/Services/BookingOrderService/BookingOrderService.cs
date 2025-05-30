using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Booking.Domain.Specifications.BookingOrder;
using Common.Application.Interfaces;
using Common.Application.Repositories;
using Common.Application.Services;

namespace Booking.Application.Services;
public class BookingOrderService : BaseService<BookingOrder, AddBookingOrderDto, AddBookingOrderDto, BookingOrderDto>, IBookingOrderService
{
    private readonly IBookingUnitOfWork _unitOfWork;
    public BookingOrderService(IBookingOrderRepository repository,
        IBookingUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService)
        : base(repository, unitOfWork, mapper, paginationService)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingOrderDto> GetBookingOrderByTicketIdAsync(Guid ticketId)
    {
        var specification = new TicketIdSpecification(ticketId);
        return await _unitOfWork.BookingOrderRepository.FirstOrDefaultAsync<BookingOrderDto>(spec: specification);
    }
}
