using System;

namespace backend.Repository.IRepository;

public interface IGenreRepository
{
    Task<ICollection<Genre>> GetGenres();
    Task<ICollection<Genre>> GetAllGenresForAdmin();
    Task<Genre?> GetGenre(int id);
     Task<Genre?> GetGenreForAdmin(int id);
    Task<List<Genre>> GetByIds(List<int> ids);
    Task<bool> GenreExists(int id);
    Task<bool> GenreExists(string name);
    Task<bool> CreateGenre(Genre genre);
    Task<bool> UpdateGenre(Genre genre);
    Task<bool> ToggleGenreActive(int id);
    Task<bool> Save();
    
}
