using System.ComponentModel.DataAnnotations;
namespace backend.Models.Dtos.BookQueries;
public class BookListBooksQueryParameters
{
    // Paginación
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    // Ordenamiento específico para libros en listas
    public string? SortBy { get; set; } // "title", "score", "addeddate", "order"
    public bool SortDescending { get; set; } = false;
}