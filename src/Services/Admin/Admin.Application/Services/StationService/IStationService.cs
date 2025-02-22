using System.Linq.Expressions;
using Admin.Application.Dtos;
using Admin.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;

namespace Admin.Application.Services;
public interface IStationService
{
    Task<StationDto> CreateAsync(AddStationDto createDto);
    Task<PaginationResult<StationDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<Station>? specification = null,
        List<Expression<Func<Station, object>>>? includes = null);
    Task<StationDto> UpdateAsync(Guid id, AddStationDto updateDto);
    Task DeleteAsync(Guid id);
    Task<StationDto> GetByIdAsync(Guid id);
    Task<List<StationDto>> GetStations();
}
