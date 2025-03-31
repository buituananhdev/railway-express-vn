using System.Linq.Expressions;
using Common.Application.Dtos;
using Common.Domain.Specifications;
using UserManagement.Application.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Services;

public interface IPassengerService
{
    Task AddPassengerAsync(AddPassengerDto passengerDto);
    Task<PassengerDto> GetPassengerByIDAsync(Guid id);
    Task<PassengerDto> GetPassengerByEmailAsync(string email);
    Task<PaginationResult<PaginatePassengerDto>> GetPassengerListAsync(
        PaginationParams paginationParams,
        Specification<Passenger>? specification = null,
        List<Expression<Func<Passenger, object>>>? includes = null);
    Task DeletePassengerAsync(Guid id);
    Task UpdatePassengerAsync(Guid id, UpdatePassengerDto updatePassengerDto);
}
