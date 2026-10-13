using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Announcement
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contenido es obligatorio")]
    [MaxLength(5000, ErrorMessage = "El contenido no puede exceder los 5000 caracteres")]
    public string Content { get; set; } = string.Empty;
 
    // Relación con el usuario que creó el anuncio
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(CreatedByUserId))]
    public User CreatedBy { get; set; } = null!;

    // Campos de auditoría
    public DateTime CreatedAt { get; set; } =  DateTime.Now;

}
