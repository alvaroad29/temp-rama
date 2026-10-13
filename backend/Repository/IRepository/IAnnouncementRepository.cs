using backend.Models;

namespace backend.Repository.IRepository;

public interface IAnnouncementRepository
{
    Task<IEnumerable<Announcement>> GetAllAsync();
    Task<Announcement?> GetByIdAsync(int id);
    Task<Announcement> CreateAsync(Announcement announcement);
    Task<Announcement> UpdateAsync(Announcement announcement);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<Announcement?> GetLatestAsync();
}
