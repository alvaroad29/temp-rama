using backend.Models;
using backend.Models.Dtos.BookQueries;
using Microsoft.EntityFrameworkCore; // Necesario para EF.Functions
public static class BookFilters
{
     public static IQueryable<Book> ApplySearchFilter(
    this IQueryable<Book> query,
    string? searchText,
    SearchInField? searchIn)
{
    if (string.IsNullOrEmpty(searchText))
        return query;

    var pattern = $"%{searchText}%";

    return searchIn switch
    {
        SearchInField.Title => query.Where(b =>
            EF.Functions.ILike(b.Title, pattern)),

        SearchInField.Author => query.Where(b =>
            EF.Functions.ILike(b.Author.DisplayName, pattern)),

        SearchInField.Description => query.Where(b =>
            b.Description != null &&
            EF.Functions.ILike(b.Description, pattern)),

        _ => query.Where(b =>
            EF.Functions.ILike(b.Title, pattern) ||
            (b.Description != null && EF.Functions.ILike(b.Description, pattern)) ||
            EF.Functions.ILike(b.Author.DisplayName, pattern))
    };
}

    public static IQueryable<Book> ApplyStateFilter(
        this IQueryable<Book> query, 
        State? state)
    {
        return state.HasValue 
            ? query.Where(b => b.State == state.Value) 
            : query;
    }

    public static IQueryable<Book> ApplyGenreFilter(
        this IQueryable<Book> query, 
        List<int>? genreIds)
    {
        if (genreIds == null || !genreIds.Any()) 
            return query;

        foreach (var id in genreIds)
        {
            query = query.Where(b => b.Genres.Any(g => g.Id == id));
        }
        return query;
    }
}