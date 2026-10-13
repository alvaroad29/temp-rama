using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class ToggleVolumeActiveDto
{
    [Required]
    public bool IsActive { get; set; }
}