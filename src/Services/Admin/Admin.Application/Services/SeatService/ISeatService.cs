using System.Linq.Expressions;
using Admin.Application.Dtos;
using Admin.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;

namespace Admin.Application.Services;
public interface ISeatService
{
    Task<SeatDto> CreateAsync(AddSeatDto createDto);
    Task<PaginationResult<SeatDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<Seat>? specification = null,
        List<Expression<Func<Seat, object>>>? includes = null);
    Task<SeatDto> UpdateAsync(Guid id, AddSeatDto updateDto);
    Task DeleteAsync(Guid id);
    Task<SeatDto> GetByIdAsync(Guid id);
    Task<List<SeatDto>> GetSeatsByTrainCarAndScheduleAsync(Guid trainCarId, Guid trainScheduleId, DateTime journeyDate);
    Task LockSeatsAsync(LockSeatDto lockSeatDto);
    Task<bool> IsSeatLocked(Guid seatId, Guid scheduleId, DateTime journeyDate);
    Task<int> GetAvailableSeatsAsync(Guid trainCarId, Guid scheduleId, DateTime journeyDate);
    Task<SeatFullInformationDto> GetSeatWithTrainInformationAsync(Guid seatId);
}
