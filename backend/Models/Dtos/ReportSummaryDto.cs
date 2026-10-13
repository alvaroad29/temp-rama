namespace backend.Models.Dtos;
public class ReportSummaryDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int InReviewReports { get; set; }
    public int ResolvedReports { get; set; }
}