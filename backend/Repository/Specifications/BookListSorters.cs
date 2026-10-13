using backend.Models;

public static class BookListSorters
{
    public static IQueryable<BookList> ApplySorting(
        this IQueryable<BookList> query, 
        string? sortBy, 
        bool sortDescending)
    {
        return (sortBy?.ToLower(), sortDescending) switch
        {
            ("title", false) => query.OrderBy(bl => bl.Title),
            ("title", true) => query.OrderByDescending(bl => bl.Title),
            ("followercount", false) => query.OrderBy(bl => bl.FollowersCount),
            ("followercount", true) => query.OrderByDescending(bl => bl.FollowersCount),
            ("creationdate", false) => query.OrderBy(bl => bl.CreationDate),
            ("creationdate", true) => query.OrderByDescending(bl => bl.CreationDate),
            ("updatedate", false) => query.OrderBy(bl => bl.UpdateDate),
            ("updatedate", true) => query.OrderByDescending(bl => bl.UpdateDate),
            ("bookcount", false) => query.OrderBy(bl => bl.BookCount),
            ("bookcount", true) => query.OrderByDescending(bl => bl.BookCount),
            _ => query.OrderBy(bl => bl.Title) // Default
        };
    }
}