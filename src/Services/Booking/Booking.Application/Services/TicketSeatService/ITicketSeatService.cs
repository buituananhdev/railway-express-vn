using System.Linq.Expressions;
using Booking.Application.Dtos;
using Booking.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;

namespace Booking.Application.Services;
public interface ITicketSeatService
{
    Task<TicketSeatDto> CreateAsync(AddTicketSeatDto createDto);
    Task<PaginationResult<TicketSeatDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<TicketSeat>? specification = null,
        List<Expression<Func<TicketSeat, object>>>? includes = null);
    Task<TicketSeatDto> UpdateAsync(Guid id, AddTicketSeatDto updateDto);
    Task DeleteAsync(Guid id);
    Task<TicketSeatDto> GetByIdAsync(Guid id);
}
