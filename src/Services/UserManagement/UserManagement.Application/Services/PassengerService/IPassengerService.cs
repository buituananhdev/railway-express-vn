using System.Linq.Expressions;
using Common.Application.Dtos;
using Common.Domain.Specifications;
using UserManagement.Application.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Services;

public interface IPassengerService
{
    Task<PassengerDto> CreateAsync(AddPassengerDto passengerDto);
    Task<PassengerDto> GetByIdAsync(Guid id);
    Task<PassengerDto> GetByEmailAsync(string email);
    Task<PaginationResult<PassengerDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<Passenger>? specification = null,
        List<Expression<Func<Passenger, object>>>? includes = null);
    Task DeleteAsync(Guid id);
    Task<PassengerDto> UpdateAsync(Guid id, UpdatePassengerDto updatePassengerDto);
}
