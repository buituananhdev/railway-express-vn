using Booking.Application.Dtos;
using Booking.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace Booking.Application.Services;
public interface IBookingOrderService
{
    Task<BookingOrderDto> CreateAsync(AddBookingOrderDto createDto);
    Task<PaginationResult<BookingOrderDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<BookingOrder>? specification = null,
        List<Expression<Func<BookingOrder, object>>>? includes = null);
    Task<BookingOrderDto> UpdateAsync(Guid id, AddBookingOrderDto updateDto);
    Task DeleteAsync(Guid id);
    Task<BookingOrderDto> GetByIdAsync(Guid id);
    Task<BookingOrderDto> GetBookingOrderByTicketIdAsync(Guid ticketId);
}
