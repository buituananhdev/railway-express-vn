using Common.Application.Dtos;
using Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Common.Application.Pagination;
public class PaginationService : IPaginationService
{
    public async Task<PaginationResult<T>> CreatePaginatedResultAsync<T>(
        IQueryable<T> query,
        PaginationParams paginationParams)
    {
        var pageNumber = paginationParams.PageNumber > 0 ? paginationParams.PageNumber : 1;
        var pageSize = paginationParams.PageSize > 0 ? paginationParams.PageSize : 10;

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (!string.IsNullOrEmpty(paginationParams.SortBy))
        {
            //query = query.OrderBy(paginationParams.SortBy);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<T>
        {
            Data = items,
            CurrentPage = pageNumber,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
