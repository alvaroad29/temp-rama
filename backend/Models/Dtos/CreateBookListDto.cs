using System.ComponentModel.DataAnnotations;
namespace backend.Models.Dtos;

public class CreateBookListDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsPublic { get; set; } = true;

    public List<int> GenreIds { get; set; } = new List<int>();
}