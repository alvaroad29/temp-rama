using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class CreateAnnouncementDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contenido es obligatorio")]
    [MaxLength(5000, ErrorMessage = "El contenido no puede exceder los 5000 caracteres")]
    public string Content { get; set; } = string.Empty;
}
