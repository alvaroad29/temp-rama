using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos.UserQueries;

// Models/Dtos/UserQueries/UserQueryParameters.cs
using System.ComponentModel.DataAnnotations;

public class UserQueryParameters
{
    // Paginación
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    // Filtros
    public string? SearchText { get; set; }
    public SearchInUserField? SearchIn { get; set; } = SearchInUserField.All;
    public bool? IsBanned { get; set; } // Filtro por baneo

    // Ordenamiento
    public string? SortBy { get; set; } = "username"; // "username", "email", "displayname"
    public bool SortDescending { get; set; } = false;
}

public enum SearchInUserField
{
    All,
    Username,
    Id
}