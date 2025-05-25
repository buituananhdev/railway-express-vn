using System.Linq.Expressions;
using Admin.Application.Dtos;
using Admin.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;

namespace Admin.Application.Services;
public interface ITrainCarService
{
    Task<TrainCarDto> CreateAsync(AddTrainCarDto createDto);
    Task<PaginationResult<TrainCarDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<TrainCar>? specification = null,
        List<Expression<Func<TrainCar, object>>>? includes = null);
    Task<TrainCarDto> UpdateAsync(Guid id, AddTrainCarDto updateDto);
    Task DeleteAsync(Guid id);
    Task<TrainCarDto> GetByIdAsync(Guid id);
    Task<List<TrainCarDto>> GetTrainCarsAndPriceAsync(Guid trainId, Guid scheduleId, DateTime journeyDate);
    Task<List<TrainCarDto>> GetTrainCarsByTrainIdAsync(Guid trainId);
}
