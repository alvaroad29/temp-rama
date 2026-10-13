using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
namespace backend.Repository.IRepository;

public interface IChapterRepository
{
    Task<Chapter?> GetChapterByIdAsync(int chapterId);
    Task IncrementViewCountAsync(Chapter chapter);
    Task AddToReadingHistoryAsync(ReadingHistory history);
    Task<bool> HasUserViewedChapterAsync(int chapterId, string userId);
    Task<Chapter> CreateChapterAsync(Chapter chapter);

    Task<Chapter?> GetPreviousChapterAsync(int bookId, int currentVolumeId, int currentOrder);
    Task<Chapter?> GetNextChapterAsync(int bookId, int currentVolumeId, int currentOrder);
    Task<int> GetNextOrderForVolumeAsync(int volumeId);
    Task UpdateChapterAsync(int chapterId, UpdateChapterDto updateChapterDto);
    Task<bool> ToggleChapterActiveAsync(int chapterId, bool isActive);
    // Task<bool> SoftDeleteChapterAsync(int chapterId);
}