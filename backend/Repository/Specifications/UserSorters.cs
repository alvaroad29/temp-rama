using backend.Models;

public static class UserSorters
{
    public static IQueryable<User> ApplySorting(
        this IQueryable<User> query,
        string? sortBy,
        bool sortDescending)
    {
        return (sortBy?.ToLower(), sortDescending) switch
        {
            ("username", false) => query.OrderBy(u => u.UserName),
            ("username", true) => query.OrderByDescending(u => u.UserName),
            ("banned", false) => query.OrderBy(u => u.LockoutEnabled).ThenBy(u => u.LockoutEnd),
            ("banned", true) => query.OrderByDescending(u => u.LockoutEnabled).ThenByDescending(u => u.LockoutEnd),
            _ => query.OrderBy(u => u.UserName) // Default
        };
    }
}