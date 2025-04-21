using System.Linq.Expressions;
using Admin.Application.Dtos;
using Admin.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;

namespace Admin.Application.Services;
public interface ITrainService
{
    Task<TrainDto> CreateAsync(AddTrainDto createDto);
    Task<PaginationResult<TrainDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<Train>? specification = null,
        List<Expression<Func<Train, object>>>? includes = null);
    Task<TrainDto> UpdateAsync(Guid id, AddTrainDto updateDto);
    Task DeleteAsync(Guid id);
    Task<TrainDto> GetByIdAsync(Guid id);
    Task<List<TrainDto>> GetTrainsAsync();
    Task<TrainCarDto> CreateTrainCarAsync(AddTrainCarDto trainCarDto);
    Task<TrainCarDto> UpdateTrainCarAsync(Guid id, AddTrainCarDto trainCarDto);

    Task DeleteTrainCarAsync(Guid id);
}
