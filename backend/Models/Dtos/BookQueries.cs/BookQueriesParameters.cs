using System.ComponentModel.DataAnnotations;
namespace backend.Models.Dtos.BookQueries;
public class BookQueryParameters
{
    // Paginación
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    // Filtros
    public string? SearchText { get; set; }
    public SearchInField? SearchIn { get; set; } // Campo para buscar
    public State? State { get; set; }
    public List<int>? GenreIds { get; set; } // IDs de géneros a filtrar

    // Ordenamiento
    public string? SortBy { get; set; } // "title", "score", "description", 'LastChapterPublishDate'
    public bool SortDescending { get; set; } = false; // "asc", "desc"
}

public enum SearchInField
{
    All,
    Title,
    Author,
    Description
}