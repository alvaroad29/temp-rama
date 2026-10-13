namespace backend.Models;

public class BookSubmission
{
    public int Id { get; set; }
    
    // Datos del usuario que envía la solicitud
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    // Datos del libro propuesto
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string Synopsis { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty; // JSON array de genre IDs
    
    // Datos del volumen
    public string VolumeTitle { get; set; } = string.Empty;
    public int VolumeOrder { get; set; } = 1;
    
    // Datos del capítulo
    public string ChapterTitle { get; set; } = string.Empty;
    public int ChapterOrder { get; set; } = 1;
    public string ChapterContent { get; set; } = string.Empty; // Markdown
    
    // Estado y revisión
    public BookSubmissionStatus Status { get; set; } = BookSubmissionStatus.Pending;
    public string? AdminFeedback { get; set; }
    public string? ReviewedByAdminId { get; set; }
    public User? ReviewedByAdmin { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// Enum para el estado
public enum BookSubmissionStatus
{
    Pending = 1,    // Esperando revisión
    Approved = 2,   // Aprobado
    Rejected = 3,   // Rechazado
    NeedsChanges = 4 // Requiere cambios
}