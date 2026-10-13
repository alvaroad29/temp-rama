using System;
using backend.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository;

public class GenreRepository : IGenreRepository
{
    private readonly ApplicationDbContext _db;
    public GenreRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<bool> GenreExists(int id)
    {
        return await _db.Genres.AnyAsync(g => g.Id == id);
    }
    public async Task<bool> GenreExists(string name)
    {
        return await _db.Genres
            .AnyAsync(g => g.Name.ToLower().Trim() == name.ToLower().Trim());
    }
    public async Task<bool> CreateGenre(Genre genre)
    {
        genre.IsActive = true; 
        await _db.Genres.AddAsync(genre);
        return await Save();
    }

    // public async Task<bool> DeleteGenre(Genre genre)
    // {
    //     genre.IsActive = false;
    //     _db.Genres.Update(genre);
    //     return await Save();
    // }

    public async Task<Genre?> GetGenre(int id)
    {
        return await _db.Genres
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);
    }

    public async Task<Genre?> GetGenreForAdmin(int id)
    {
        return await _db.Genres
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Genre>> GetByIds(List<int> ids)
    {
        return await _db.Genres
            .Where(g => ids.Contains(g.Id))
            // .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<Genre>> GetGenres()
    {
        return await _db.Genres
            .OrderBy(g => g.Name)
            .Where(g => g.IsActive)
            .AsNoTracking() //?
            .ToListAsync();
    }

    public async Task<bool> Save()
    {
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateGenre(Genre genre)
    {
        _db.Genres.Update(genre);
        return await Save();
    }

     public async Task<ICollection<Genre>> GetAllGenresForAdmin()
    {
        return await _db.Genres
            .OrderBy(g => g.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ToggleGenreActive(int id)
    {
        var genre = await _db.Genres.FindAsync(id);
        if (genre == null) return false;

        genre.IsActive = !genre.IsActive;
        _db.Genres.Update(genre);
        return await Save();
    }
}
