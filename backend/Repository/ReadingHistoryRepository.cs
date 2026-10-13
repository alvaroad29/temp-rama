using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using backend.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class ReadingHistoryRepository : IReadingHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ReadingHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

   public async Task<PagedResponse<ReadingHistory>> GetHistoryByUserIdAsync(string userId, HistoryQueryParameters parameters)
{
    if (parameters.PageNumber < 1) parameters.PageNumber = 1;
    if (parameters.PageSize < 1) parameters.PageSize = 10;
    if (parameters.PageSize > 100) parameters.PageSize = 100;

    // ✅ Cambia a IQueryable<ReadingHistory> (no IOrderedQueryable)
    var query = _context.ReadingHistories
        .Where(r => r.UserId == userId)
        .Include(r => r.Chapter)
            .ThenInclude(c => c.Volume)
                .ThenInclude(v => v.Book)
        .AsQueryable(); // ← Esto asegura que sea IQueryable

    // Filtros
    if (parameters.FromDate.HasValue)
        query = query.Where(r => r.ViewDate >= parameters.FromDate.Value);

    if (parameters.ToDate.HasValue)
        query = query.Where(r => r.ViewDate <= parameters.ToDate.Value);

    if (!string.IsNullOrEmpty(parameters.BookTitle))
        query = query.Where(r => r.Chapter.Volume.Book.Title.Contains(parameters.BookTitle));

    // ✅ Aplica OrderBy al final
    var orderedQuery = query.OrderByDescending(r => r.ViewDate);

    var totalRecords = await orderedQuery.CountAsync();

    var history = await orderedQuery
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .ToListAsync();

    return new PagedResponse<ReadingHistory>(history, parameters.PageNumber, parameters.PageSize, totalRecords);
}
    }
}