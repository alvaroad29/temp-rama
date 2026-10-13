namespace backend.Models.Dtos;
public class UpdateReportDto
{
    public ReportStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}