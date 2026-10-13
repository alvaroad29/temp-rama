
using backend.Models;
using backend.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _db;

        public AnnouncementRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await _db.Announcements
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            return await _db.Announcements
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Announcement> CreateAsync(Announcement announcement)
        {
            _db.Announcements.Add(announcement);
            await _db.SaveChangesAsync();
            
            // Cargar el usuario relacionado después de guardar
            await _db.Entry(announcement)
                .Reference(a => a.CreatedBy)
                .LoadAsync();
                
            return announcement;
        }

        public async Task<Announcement> UpdateAsync(Announcement announcement)
        {
            _db.Announcements.Update(announcement);
            await _db.SaveChangesAsync();
            
            // Cargar el usuario relacionado después de actualizar
            await _db.Entry(announcement)
                .Reference(a => a.CreatedBy)
                .LoadAsync();
                
            return announcement;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var announcement = await _db.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            _db.Announcements.Remove(announcement);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Announcements.AnyAsync(a => a.Id == id);
        }

        public async Task<Announcement?> GetLatestAsync()
        {
        return await _db.Announcements
            .Include(a => a.CreatedBy)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();
        }
    }
}