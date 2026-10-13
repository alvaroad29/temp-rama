using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Author
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El campo Nombre es requerido")]
    public string Name { get; set; } = string.Empty;
    // [StringLength(1000, ErrorMessage = "La biografía no puede superar los 1000 caracteres")]
    // public string Biography { get; set; } = string.Empty;
    // public DateTime DateOfBirth { get; set; }
}
