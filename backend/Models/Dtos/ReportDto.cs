namespace backend.Models.Dtos;
public class ReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreationDate { get; set; }
     public DateTime? ReviewedAt { get; set; } 
    public ReportStatus Status { get; set; }
    public ReportType ReportType { get; set; }
    public string? AdminNotes { get; set; }

    // Información del libro
    public int BookId { get; set; }
    public string BookTitle { get; set; } = null!;
    public string? BookImageUrl { get; set; }

    // Información del usuario que reporta
    public string ReporterId { get; set; } = null!;
    public string ReporterName { get; set; } = null!;
    public string? ReporterEmail { get; set; }

    // Información del admin asignado
    public string? ReviewedByAdminId { get; set; }
    public string? ReviewedByAdminName  { get; set; }
}