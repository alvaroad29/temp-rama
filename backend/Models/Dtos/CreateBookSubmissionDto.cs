using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class CreateBookSubmissionDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? CoverUrl { get; set; }

    [Required]
    [StringLength(4000)]
    public string Synopsis { get; set; } = string.Empty;

    [Required]
    public List<int> GenreIds { get; set; } = new();

    [Required]
    [StringLength(100)]
    public string VolumeTitle { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int VolumeOrder { get; set; } = 1;

    [Required]
    [StringLength(100)]
    public string ChapterTitle { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ChapterOrder { get; set; } = 1;

    [Required]
    [StringLength(20000, ErrorMessage = "El contenido del capítulo no puede exceder los 20,000 caracteres")]
    public string ChapterContent { get; set; } = string.Empty;
}