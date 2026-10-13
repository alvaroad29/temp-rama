using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
namespace backend.Repository.IRepository;

public interface IReadingHistoryRepository
{
    // Repositories/IReadingHistoryRepository.cs
   Task<PagedResponse<ReadingHistory>> GetHistoryByUserIdAsync(string userId, HistoryQueryParameters parameters);
}