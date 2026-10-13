using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.MicrosoftExtensions;

namespace backend.Models.Dtos;

public class CreateGenreDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede tener mas de 50 caracteres")]
    [MinLength(3, ErrorMessage = "El nombre no puede tener menos de 3 caracteres")]
    public string Name { get; set; } = string.Empty; // regla de la clase

}
