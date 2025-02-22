using Common.Application.Dtos;

namespace Common.Application.Interfaces;
public interface IPaginationService
{
    Task<PaginationResult<T>> CreatePaginatedResultAsync<T>(
        IQueryable<T> query,
        PaginationParams paginationParams);
}
