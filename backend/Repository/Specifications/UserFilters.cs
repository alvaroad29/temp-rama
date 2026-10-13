// Repository/UserFilters.cs
using backend.Models;
using backend.Models.Dtos.UserQueries;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository;

public static class UserFilters
{
   public static IQueryable<User> ApplySearchFilter(
    this IQueryable<User> query,
    string? searchText,
    SearchInUserField? searchIn)
{
    if (string.IsNullOrEmpty(searchText))
        return query;

    var pattern = $"%{searchText}%";

    return searchIn switch
    {
        SearchInUserField.Username => query.Where(u =>
            EF.Functions.ILike(u.UserName!, pattern)),

        SearchInUserField.Id => query.Where(u =>
            u.Id.Contains(searchText)),

        _ => query.Where(u =>
            EF.Functions.ILike(u.UserName!, pattern) ||
            u.Id.Contains(searchText))
    };
}

    public static IQueryable<User> ApplyBannedFilter(
        this IQueryable<User> query,
        bool? isBanned)
    {
        if (!isBanned.HasValue)
            return query;

        if (isBanned.Value)
        {
            // Usuario baneado: LockoutEnabled=true y LockoutEnd en futuro
            return query.Where(u => 
                u.LockoutEnabled && 
                u.LockoutEnd.HasValue && 
                u.LockoutEnd > DateTimeOffset.UtcNow);
        }
        else
        {
            // Usuario no baneado: LockoutEnabled=false o LockoutEnd en pasado/nulo
            return query.Where(u => 
                !u.LockoutEnabled || 
                !u.LockoutEnd.HasValue || 
                u.LockoutEnd <= DateTimeOffset.UtcNow);
        }
    }
}