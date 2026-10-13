using System.ComponentModel.DataAnnotations;
namespace backend.Models.Dtos.BookQueries;
public class BookListQueryParameters
{
    // Paginación
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    // Filtros
    public string? SearchText { get; set; }
    public List<int>? GenreIds { get; set; }

    // Ordenamiento
    public string? SortBy { get; set; } // "title", "followercount", "creationdate", "updatedate"
    public bool SortDescending { get; set; } = false;
}