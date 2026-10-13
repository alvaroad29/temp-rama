using backend.Models;
using backend.Models.Dtos;

namespace backend.Repository.IRepository;

public interface IBookSubmissionRepository
{
    Task<BookSubmission?> GetByIdAsync(int id);
    Task<BookSubmission?> GetByIdWithUserAsync(int id);
    Task<IEnumerable<BookSubmission>> GetByUserIdAsync(string userId);
    Task<IEnumerable<BookSubmission>> GetPendingSubmissionsAsync();
    Task<IEnumerable<BookSubmission>> GetAllWithUsersAsync();
    Task<BookSubmission> CreateAsync(BookSubmission submission);
    Task<bool> UpdateStatusAsync(int id, BookSubmissionStatus status, string? adminFeedback, string reviewedByAdminId);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
     Task<bool> UserHasSubmissionWithCoverAsync(string userId, string coverUrl);
    // En IBookSubmissionRepository.cs
    Task<BookSubmission?> UpdateAsync(int id, UpdateBookSubmissionDto updateDto);
    Task<Dictionary<BookSubmissionStatus, int>> GetSubmissionStatsAsync();

}