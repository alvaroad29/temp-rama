using System.Text.Json;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using backend.Repository.IRepository;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace backend.Repository;
public class BookSubmissionRepository : IBookSubmissionRepository
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IImageService _imageService;

    public BookSubmissionRepository(ApplicationDbContext db, IImageService imageService,  UserManager<User> userManager)
    {
        _db = db;
        _imageService = imageService;
        _userManager = userManager;
    }

    public async Task<BookSubmission?> GetByIdAsync(int id)
    {
        return await _db.BookSubmissions
            .FirstOrDefaultAsync(bs => bs.Id == id);
    }

    public async Task<BookSubmission?> GetByIdWithUserAsync(int id)
    {
        return await _db.BookSubmissions
            .Include(bs => bs.User)
            .Include(bs => bs.ReviewedByAdmin)
            .FirstOrDefaultAsync(bs => bs.Id == id);
    }

    public async Task<IEnumerable<BookSubmission>> GetByUserIdAsync(string userId)
    {
        return await _db.BookSubmissions
            .Where(bs => bs.UserId == userId)
            .OrderByDescending(bs => bs.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BookSubmission>> GetPendingSubmissionsAsync()
    {
        return await _db.BookSubmissions
            .Where(bs => bs.Status == BookSubmissionStatus.Pending)
            .Include(bs => bs.User)
            .OrderBy(bs => bs.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BookSubmission>> GetAllWithUsersAsync()
    {
        return await _db.BookSubmissions
            .Include(bs => bs.User)
            .Include(bs => bs.ReviewedByAdmin)
            .OrderByDescending(bs => bs.CreatedAt)
            .ToListAsync();
    }

    public async Task<BookSubmission> CreateAsync(BookSubmission submission)
    {
        _db.BookSubmissions.Add(submission);
        await _db.SaveChangesAsync();
        return submission;
    }

    public async Task<BookSubmission?> UpdateAsync(int id, UpdateBookSubmissionDto updateDto)
{
    using var transaction = await _db.Database.BeginTransactionAsync();
    
    try
    {
        var submission = await _db.BookSubmissions
            .Include(bs => bs.User)
            .FirstOrDefaultAsync(bs => bs.Id == id);

        if (submission == null)
        {
            return null;
        }

        // Actualizar campos básicos
        submission.Title = updateDto.Title;
        submission.Synopsis = updateDto.Synopsis ?? string.Empty;
        submission.Genres = string.Join(",", updateDto.GenreIds);
        submission.VolumeTitle = updateDto.VolumeTitle;
        submission.VolumeOrder = updateDto.VolumeOrder;
        submission.ChapterTitle = updateDto.ChapterTitle;
        submission.ChapterOrder = updateDto.ChapterOrder;
        submission.ChapterContent = updateDto.ChapterContent;

        // Cambiar estado a Pending nuevamente
        submission.Status = BookSubmissionStatus.Pending;
        
        // Limpiar feedback anterior y datos de revisión
        submission.AdminFeedback = null;
        submission.ReviewedByAdminId = null;
        submission.ReviewedAt = null;
        
        // Actualizar timestamp
        submission.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return submission;
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}

 
    public async Task<bool> UpdateStatusAsync(int id, BookSubmissionStatus status, string? adminFeedback, string reviewedByAdminId)
    {

        var submission = await _db.BookSubmissions.FindAsync(id);
        if (submission == null) 
        {
            
            return false;
        }


        // Guardar el estado anterior para verificar si cambió a Approved
        var previousStatus = submission.Status;

        submission.Status = status;
        submission.AdminFeedback = adminFeedback;
        submission.ReviewedByAdminId = reviewedByAdminId;
        submission.ReviewedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;

         
        await _db.SaveChangesAsync();
        

        // Si el estado cambió a Approved, crear el libro automáticamente
        if (status == BookSubmissionStatus.Approved && previousStatus != BookSubmissionStatus.Approved)
        {
            
            await CreateBookFromSubmissionAsync(submission);
            await AssignAuthorRoleIfFirstApprovalAsync(submission.UserId);
        }
         
        return true;
    }

    private async Task AssignAuthorRoleIfFirstApprovalAsync(string userId)
    {
        try
        {
            // Verificar si es el primer libro aprobado del usuario
            var approvedBooksCount = await _db.BookSubmissions
                .CountAsync(bs => bs.UserId == userId && bs.Status == BookSubmissionStatus.Approved);

            // Si es exactamente 1 (este es el primero), asignar rol Author
            if (approvedBooksCount == 1)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Verificar si ya tiene el rol Author
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (!userRoles.Contains("Author"))
                    {
                        // Asignar rol Author
                        var result = await _userManager.AddToRoleAsync(user, "Author");
                        if (!result.Succeeded)
                        {
                            // Log del error pero no fallar el proceso
                            Console.WriteLine($"Error asignando rol Author: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log del error pero no revertir la aprobación
            Console.WriteLine($"Error asignando rol Author: {ex.Message}");
        }
    }

    private async Task CreateBookFromSubmissionAsync(BookSubmission submission)
    {
        try
        {
            // 1. Crear el libro
            var book = new Book
            {
                Title = submission.Title,
                Description = submission.Synopsis,
                ImageUrl = submission.CoverUrl,
                AuthorId = submission.UserId,
                State = State.Activo,
                IsActive = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                ViewCount = 0,
                TotalRatings = 0,
                Score = 0,
                ChapterCount = 1
            };

            // 2. Asignar géneros al libro
            if (!string.IsNullOrEmpty(submission.Genres))
            {
                var genreIds = submission.Genres.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(int.Parse)
                                            .ToList();
                
                var genres = await _db.Genres
                    .Where(g => genreIds.Contains(g.Id))
                    .ToListAsync();
                
                book.Genres = genres;
            }

            // 3. Guardar el libro
            _db.Books.Add(book);
            await _db.SaveChangesAsync(); // Guardar para obtener el ID

            // 4. Crear el volumen
            var volume = new Volume
            {
                Title = submission.VolumeTitle,
                Order = submission.VolumeOrder,
                BookId = book.Id,
                IsActive = true
            };

            _db.Volumes.Add(volume);
            await _db.SaveChangesAsync(); // Guardar para obtener el ID

            // 5. Crear el capítulo
            var chapter = new Chapter
            {
                Title = submission.ChapterTitle,
                Content = submission.ChapterContent,
                VolumeId = volume.Id,
                Order = submission.ChapterOrder,
                PublishDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                ViewCount = 0,
                IsActive = true
            };

            _db.Chapters.Add(chapter);
            await _db.SaveChangesAsync();

            // 6. Actualizar el contador de capítulos del libro
            book.ChapterCount = 1;
            book.LastChapterPublishDate = chapter.PublishDate;
            await _db.SaveChangesAsync();


        }
        catch (Exception ex)
        {
            // Log del error pero no revertir la aprobación
            // El admin ya aprobó la submission, el libro se puede crear manualmente después
            Console.WriteLine($"Error creando libro desde submission: {ex.Message}");
            // Puedes agregar logging aquí
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var submission = await _db.BookSubmissions.FindAsync(id);
        if (submission == null) return false;

        _db.BookSubmissions.Remove(submission);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _db.BookSubmissions.AnyAsync(bs => bs.Id == id);
    }

    public async Task<bool> UserHasSubmissionWithCoverAsync(string userId, string coverUrl)
    {
        return await _db.BookSubmissions
            .AnyAsync(bs => bs.UserId == userId && bs.CoverUrl == coverUrl);
    }

    public async Task<Dictionary<BookSubmissionStatus, int>> GetSubmissionStatsAsync()
    {
        return await _db.BookSubmissions
            .GroupBy(bs => bs.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }


}