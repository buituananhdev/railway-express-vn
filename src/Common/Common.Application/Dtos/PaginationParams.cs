namespace Common.Application.Dtos;
public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    public string? SortBy { get; set; }
    public bool? IsSortAscending { get; set; } = true;
    public bool? IsActive { get; set; } = true;
}
