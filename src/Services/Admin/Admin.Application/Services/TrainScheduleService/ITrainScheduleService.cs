using System.Linq.Expressions;
using Admin.Application.Dtos;
using Admin.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;

namespace Admin.Application.Services;
public interface ITrainScheduleService
{
    Task<TrainScheduleDto> CreateAsync(AddTrainScheduleDto createDto);
    Task<PaginationResult<TrainScheduleDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<TrainSchedule>? specification = null,
        List<Expression<Func<TrainSchedule, object>>>? includes = null);
    Task<TrainScheduleDto> UpdateAsync(Guid id, AddTrainScheduleDto updateDto);
    Task DeleteAsync(Guid id);
    Task<TrainScheduleDto> GetByIdAsync(Guid id);
    Task<List<TrainScheduleDto>> GetTrainSchedulesAsync(GetTrainSchedulesDto getTrainSchedulesDto);
    Task<TrainScheduleDto> GetTrainScheduleInformationAsync(Guid scheduleId);
    Task<TrainScheduleDto?> GetTrainScheduleClosestTimeAsync(GetTrainSchedulesDto request, TimeSpan target);
}
