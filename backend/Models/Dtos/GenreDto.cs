using System;

namespace backend.Models.Dtos;

public class GenreDto
{
    public int Id { get; set; }
    //[StringLength(45, ErrorMessage = "El campo Nombre debe tener {1} caracteres o menos")]
    public string Name { get; set; } = string.Empty; // regla de la clase
    public bool IsActive { get; set; }
}
