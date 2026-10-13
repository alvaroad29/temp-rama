using System;
using System.ComponentModel.DataAnnotations;


namespace backend.Models.Dtos;

public class DeactivateBookDto
{
    [Required]
    public bool IsActive { get; set; }
}
