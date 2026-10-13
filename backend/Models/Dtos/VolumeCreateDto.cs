using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class VolumeCreateDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    
    [Required]
     [Range(1, int.MaxValue)]
    public int Order { get; set; }
    
    [Required]
    public int BookId { get; set; }
}