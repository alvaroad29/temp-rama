using System;

namespace backend.Models.Dtos;
public class BookSubmissionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string Synopsis { get; set; } = string.Empty;
    public List<int> GenreIds { get; set; } = new();
    public string VolumeTitle { get; set; } = string.Empty;
    public int VolumeOrder { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
    public int ChapterOrder { get; set; }
    public string ChapterContent { get; set; } = string.Empty;
    public BookSubmissionStatus Status { get; set; }
    public string? AdminFeedback { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Propiedades solo para admin (opcionales)
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ReviewedByAdminName { get; set; }
    public DateTime? ReviewedAt { get; set; }
}