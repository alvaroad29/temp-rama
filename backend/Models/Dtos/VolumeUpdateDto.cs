using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class VolumeUpdateDto
{
    [StringLength(100)]
    public string? Title { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? Order { get; set; }    
    public bool? IsActive { get; set; }
}