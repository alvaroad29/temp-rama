
using System.Data.Common;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using backend.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _db;
    public BookRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResponse<Book>> GetBooks(BookQueryParameters parameters, bool includeInactive )
    {
        var query = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genres)
            .AsNoTracking();

         if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        // Aplicar filtros (extensión desde BookFilters)
        query = query
            .ApplySearchFilter(parameters.SearchText, parameters.SearchIn)
            .ApplyStateFilter(parameters.State)
            .ApplyGenreFilter(parameters.GenreIds);

        // Aplicar ordenamiento (extensión desde BookSorters)
        query = query.ApplySorting(parameters.SortBy, parameters.SortDescending);

        // Paginación (se mantiene en el repositorio)
        var totalRecords = await query.CountAsync();
        var books = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResponse<Book>(books, parameters.PageNumber, parameters.PageSize, totalRecords);
    }

    public async Task<List<Book>> GetRecentBooksWithChaptersAsync(int count = 12)
    {
        return await _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genres)
            .Include(b => b.Volumes)
                .ThenInclude(v => v.Chapters
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.PublishDate)
                    .Take(3))
            .Where(b => b.IsActive && b.Volumes.Any(v => v.Chapters.Any(c => c.IsActive)))
            .OrderByDescending(b => b.LastChapterPublishDate ?? b.CreationDate)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
        }

    public async Task<IEnumerable<Book>> GetPopularBooksAsync(DateTime startDate, int count)
    {
        return await _db.ReadingHistories
            .Where(r => r.ViewDate >= startDate)
            .Include(r => r.Chapter)
                .ThenInclude(c => c.Volume)
                    .ThenInclude(v => v.Book)
                        .ThenInclude(b => b.Author)
            .Include(r => r.Chapter.Volume.Book)
                .ThenInclude(b => b.Genres)
            .GroupBy(r => r.Chapter.Volume.Book)
            .OrderByDescending(g => g.Count()) // Ordenar por cantidad de vistas
            .Take(count)
            .Select(g => g.Key)
            .Where(b => b.IsActive)  
            .ToListAsync();
    }

    public async Task<bool> CreateBook(Book book)
    {
        if (book == null)
        {
            return false;
        }
        book.CreationDate = DateTime.Now;
        book.UpdateDate = DateTime.Now;
        await _db.Books.AddAsync(book);
        return await Save();
    }

    // public bool DeleteBook(Book book)
    // {
    //     if (book == null)
    //     {
    //         return false;
    //     }
    //     _db.Books.Remove(book);
    //     return Save();
    // }

    public async Task<Book?> GetBook(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _db.Books
            .Include(c => c.Author)
            .Include(c => c.Genres)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Book?> GetBookWithVolumesAndChapters(int bookId)
    {
        return await _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genres.Where(g => g.IsActive))
            .Include(b => b.Volumes)
                .ThenInclude(v => v.Chapters.Where(c => c.IsActive))
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive);
    }

    // public ICollection<Book> GetBooksByAuthor(int idAuthor)
    // {
    //     if (idAuthor <= 0)
    //     {
    //         return new List<Book>();
    //     }
    //     return _db.Books.Include(c => c.Genre).Include(c => c.Author).Where(c => c.IdAutor == idAuthor).OrderBy(c => c.Titulo).ToList();
    // }

    // public ICollection<Book> GetBooksByGenre(int idGenre)
    // {
    //     if (idGenre <= 0)
    //     {
    //         return new List<Book>();
    //     }
    //     return _db.Books.Include(c => c.Genre).Include(c => c.Author).Where(c => c.IdGenero == idGenre).OrderBy(c => c.Titulo).ToList();
    // }


    // public bool BookExist(int id)
    // {
    //     if (id <= 0)
    //     {
    //         return false;
    //     }
    //     return _db.Books.Any(c => c.IdBook == id);
    // }

    public async Task<bool> BookExist(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        return await _db.Books.AnyAsync(c => c.Title.ToLower().Trim() == name.ToLower().Trim());
    }

    // public bool UpdateBook(Book book)
    // {
    //     if (book == null)
    //     {
    //         return false;
    //     }
    //     book.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
    //     _db.Books.Update(book);
    //     return Save();
    // }


    public async Task<bool> Save()
    {
        return await _db.SaveChangesAsync() >= 0;
    }

    public async Task<BookRating> RateBookAsync(int bookId, string userId, int rating)
    {
        var book = await _db.Books
            .Include(b => b.Ratings)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book == null)
            throw new ArgumentException("Libro no encontrado");

        var existingRating = await _db.BookRatings
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);

        BookRating ratingEntity;
        int? oldRating = null;

        if (existingRating != null)
        {
            // Actualizar rating existente
            oldRating = existingRating.RatingValue;
            existingRating.RatingValue = rating;
            existingRating.RatingDate = DateTime.Now;
            ratingEntity = existingRating;
        }
        else
        {
            // Crear nuevo rating
            ratingEntity = new BookRating
            {
                BookId = bookId,
                UserId = userId,
                RatingValue = rating,
                RatingDate = DateTime.Now
            };
            _db.BookRatings.Add(ratingEntity);
        }

        // Actualizar el promedio del libro (síncrono, no necesita async)
        UpdateBookRating(book, rating, oldRating);

        await _db.SaveChangesAsync();

        // Cargar datos relacionados para la respuesta
        await _db.Entry(ratingEntity)
            .Reference(r => r.User)
            .LoadAsync();

        return ratingEntity;
    }

    private void UpdateBookRating(Book book, int newRating, int? oldRating = null)
    {
        if (oldRating.HasValue)
        {
            // Remover el rating antiguo
            var totalScore = (book.Score ?? 0) * book.TotalRatings;
            totalScore -= oldRating.Value;
            book.TotalRatings--;
            book.Score = book.TotalRatings > 0 ? totalScore / book.TotalRatings : 0;
        }

        // Agregar el nuevo rating
        var currentTotalScore = (book.Score ?? 0) * book.TotalRatings;
        book.TotalRatings++;
        book.Score = (currentTotalScore + newRating) / book.TotalRatings;
        book.UpdateDate = DateTime.Now;
    }

    public async Task<int?> GetUserRatingAsync(int bookId, string userId)
    {
        return await _db.BookRatings
            .Where(r => r.BookId == bookId && r.UserId == userId)
            .Select(r => (int?)r.RatingValue)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteRatingAsync(int bookId, string userId)
    {
        var book = await _db.Books
            .Include(b => b.Ratings)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book == null)
            throw new ArgumentException("Libro no encontrado");

        var rating = await _db.BookRatings
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);

        if (rating == null)
            return false;

        // Eliminar la calificación
        _db.BookRatings.Remove(rating);

        // Actualizar el promedio del libro
        if (book.TotalRatings > 1)
        {
            var totalScore = (book.Score ?? 0) * book.TotalRatings;
            totalScore -= rating.RatingValue;
            book.TotalRatings--;
            book.Score = totalScore / book.TotalRatings;
        }
        else
        {
            // Era la única calificación
            book.Score = 0;
            book.TotalRatings = 0;
        }

        book.UpdateDate = DateTime.Now;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> BookExistsAsync(int bookId)
    {
        return await _db.Books.AnyAsync(b => b.Id == bookId);
    }

    public async Task<PagedResponse<BookRating>> GetBookRatingsAsync(int bookId, RatingQueryParameters parameters)
    {
        var query = _db.BookRatings
            .Include(r => r.User)
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.RatingDate);

        var totalRecords = await query.CountAsync();
        
        var ratings = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResponse<BookRating>(
            ratings,
            parameters.PageNumber,
            parameters.PageSize,
            totalRecords
        );
    }

    public async Task<PagedResponse<BookRating>> GetUserRatingsAsync(string userId, RatingQueryParameters parameters)
    {
        var query = _db.BookRatings
            .Include(r => r.Book)
            .ThenInclude(b => b.Author)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RatingDate);

        var totalRecords = await query.CountAsync();

        var ratings = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResponse<BookRating>(
            ratings,
            parameters.PageNumber,
            parameters.PageSize,
            totalRecords
        );
    }
    
    public async Task<bool> UpdateBookAsync(int bookId, UpdateBookDto updateDto, List<Genre> genres)
{
    using var transaction = await _db.Database.BeginTransactionAsync();
    
    try
    {
        var book = await _db.Books
            .Include(b => b.Genres)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book == null)
            return false;

        // Actualizar propiedades básicas
        book.Title = updateDto.Title;
        book.Description = updateDto.Description;
        book.State = updateDto.State;
        book.ImageUrl = updateDto.ImageUrl;
        book.UpdateDate = DateTime.Now;

        // Actualizar géneros
        book.Genres.Clear();
        foreach (var genre in genres)
        {
            book.Genres.Add(genre);
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return true;
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}

public async Task<bool> SetBookActiveStatusAsync(int bookId, bool isActive)
{
    var book = await _db.Books.FindAsync(bookId);
    if (book == null)
        return false;

    book.IsActive = isActive;
    book.UpdateDate = DateTime.Now;
    
    return await Save();
}


}
