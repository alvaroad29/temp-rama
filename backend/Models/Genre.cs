using System.ComponentModel.DataAnnotations;
using backend.Models;

public class Genre
{
    [Key]
    public int Id { get; set; }
    //[StringLength(45, ErrorMessage = "El campo Nombre debe tener {1} caracteres o menos")]
    [Required(ErrorMessage = "El campo Nombre es requerido")] // regla de validacion de la webapi
    public string Name { get; set; } = string.Empty; // regla de la clase
    public bool IsActive { get; set; } = true;
    public ICollection<Book> Books { get; set; } = new List<Book>();

    public ICollection<BookList> Lists { get; set; } = new List<BookList>();
}