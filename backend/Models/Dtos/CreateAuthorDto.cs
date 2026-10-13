using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class CreateAuthorDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
    [MinLength(3, ErrorMessage = "El nombre no puede tener menos de 3 caracteres")]
    public string Name { get; set; } = string.Empty;
    [MaxLength(1000, ErrorMessage = "La biografía no puede superar los 1000 caracteres")]
    public string Biography { get; set; } = string.Empty;

}
