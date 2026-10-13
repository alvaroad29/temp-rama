namespace backend.Models.Dtos.BookQueries;
public class ReportQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public ReportStatus? Status { get; set; }
    public ReportType? ReportType { get; set; }
    public int? BookId { get; set; }
    public string? ReporterId { get; set; }
    public string? SortBy { get; set; } = "CreationDate";
    public bool SortDescending { get; set; } = true;
}