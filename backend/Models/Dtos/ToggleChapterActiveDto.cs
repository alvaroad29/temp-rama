using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;
public class ToggleChapterActiveDto
{
    [Required]
    public bool IsActive { get; set; }
}