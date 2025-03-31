namespace Common.Application.Dtos;
public class PaginationResult<T>
{
    public List<T> Data { get; set; }
    public PaginationMetadata MetaData  { get; set; }
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
