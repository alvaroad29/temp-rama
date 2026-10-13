using backend.Models;
using Microsoft.EntityFrameworkCore;

public static class BookListFilters
{
    public static IQueryable<BookList> ApplySearchFilter(
    this IQueryable<BookList> query,
    string? searchText)
{
    if (string.IsNullOrEmpty(searchText))
        return query;

    var pattern = $"%{searchText}%";

    return query.Where(bl =>
        EF.Functions.ILike(bl.Title, pattern) ||
        (bl.Description != null && EF.Functions.ILike(bl.Description, pattern)));
}

    public static IQueryable<BookList> ApplyGenreFilter(
        this IQueryable<BookList> query, 
        List<int>? genreIds
    )
    {
        if (genreIds == null || !genreIds.Any()) 
            return query;

        // Filtrar listas que contengan TODOS los géneros seleccionados
        foreach (var genreId in genreIds)
        {
            query = query.Where(bl => bl.Genres.Any(g => g.Id == genreId));
        }

        return query;
    }
}

