using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;
public class UpdateChapterDto
{
    [StringLength(100)]
    public string? Title { get; set; }

    [Range(1, int.MaxValue)]
    public int? Order { get; set; }

    [Required]
    [StringLength(20000, ErrorMessage = "El contenido del capítulo no puede exceder los 20,000 caracteres")]
    public string Content { get; set; } = string.Empty;

    public bool? IsActive { get; set; }
}