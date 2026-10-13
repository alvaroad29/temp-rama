using System.ComponentModel.DataAnnotations;
namespace backend.Models.Dtos;
public class UpdateBookListDto
{
    [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres")]
    public string? Title { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsPublic { get; set; }
    public List<int>? GenreIds { get; set; }
}