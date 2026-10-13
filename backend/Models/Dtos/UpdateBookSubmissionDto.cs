using System.ComponentModel.DataAnnotations;
namespace backend.Models.Dtos;

public class UpdateBookSubmissionDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Synopsis { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> GenreIds { get; set; } = new List<int>();

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