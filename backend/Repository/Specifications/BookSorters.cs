using backend.Models;

public static class BookSorters
{
    public static IQueryable<Book> ApplySorting(
        this IQueryable<Book> query, 
        string? sortBy, 
        bool sortDescending)
    {
        return (sortBy?.ToLower(), sortDescending) switch
        {
            ("title", false) => query.OrderBy(b => b.Title),
            ("title", true) => query.OrderByDescending(b => b.Title),
            ("creationdate", false) => query.OrderBy(b => b.CreationDate),
            ("creationdate", true) => query.OrderByDescending(b => b.CreationDate),
            ("updatedate", false) => query.OrderBy(b => b.UpdateDate),
            ("updatedate", true) => query.OrderByDescending(b => b.UpdateDate),
            ("score", false) => query.OrderBy(b => b.Score),
            ("score", true) => query.OrderByDescending(b => b.Score),
            ("viewcount", false) => query.OrderBy(b => b.ViewCount),
            ("viewcount", true) => query.OrderByDescending(b => b.ViewCount),
            ("recentchapters", false) => query.OrderByDescending(b => b.LastChapterPublishDate),
            ("recentchapters", true) => query.OrderBy(b => b.LastChapterPublishDate),
            _ => query.OrderBy(b => b.Title) // Default
        };
    }
}