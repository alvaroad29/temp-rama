using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Models.Dtos;
using backend.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository;

public class ChapterRepository : IChapterRepository
{
    ApplicationDbContext _db;

    public ChapterRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Chapter?> GetChapterByIdAsync(int chapterId)
    {
        return await _db.Chapters
        .Include(c => c.Volume) 
        .FirstOrDefaultAsync(c => c.Id == chapterId);
    }

    public async Task IncrementViewCountAsync(Chapter chapter)
    {
        // Incrementar ViewCount del capítulo
        chapter.ViewCount++;
        _db.Chapters.Update(chapter);

        // Incrementar ViewCount del libro
        var book = await _db.Volumes
            .Where(v => v.Id == chapter.VolumeId)
            .Select(v => v.Book)
            .FirstOrDefaultAsync();
        
        if (book != null)
        {
            book.ViewCount++;
            _db.Books.Update(book);
        }

        await _db.SaveChangesAsync();
    }

    public async Task AddToReadingHistoryAsync(ReadingHistory history)
    {
        _db.ReadingHistories.Add(history);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> HasUserViewedChapterAsync(int chapterId, string userId)
    {
        return await _db.ReadingHistories
            .AnyAsync(r => r.ChapterId == chapterId && r.UserId == userId);
    }

    public async Task<Chapter> CreateChapterAsync(Chapter chapter)
    {
         var bookId = await _db.Volumes
        .Where(v => v.Id == chapter.VolumeId)
        .Select(v => v.BookId)
        .FirstOrDefaultAsync();

        await _db.Chapters.AddAsync(chapter);
        await _db.SaveChangesAsync();

        var book = await _db.Books.FindAsync(bookId);
        if (book != null)
        {
            // Actualizar LastChapterPublishDate SIEMPRE (porque se publicó)
            book.LastChapterPublishDate = chapter.PublishDate;

            // Solo incrementar ChapterCount si el capítulo está ACTIVO
            if (chapter.IsActive)
            {
                book.ChapterCount = (book.ChapterCount ?? 0) + 1;
            }
            else
            {
                // Asegurar que ChapterCount no sea nulo (inicializar en 0 si es necesario)
                book.ChapterCount ??= 0;
            }

            await _db.SaveChangesAsync();
        }

        return chapter;
    }


    public async Task<Chapter?> GetNextChapterAsync(int bookId, int currentVolumeId, int currentOrder)
    {
        // Buscar en el volumen actual (capítulos posteriores)
        var nextInSameVolume = await _db.Chapters
            .Where(c => c.Volume.BookId == bookId 
                    && c.VolumeId == currentVolumeId 
                    && c.Order > currentOrder)
            .OrderBy(c => c.Order)
            .FirstOrDefaultAsync();

        if (nextInSameVolume != null)
            return nextInSameVolume;

        // Si no hay en el mismo volumen, buscar el primer capítulo del próximo volumen
        var nextVolume = await _db.Volumes
            .Where(v => v.BookId == bookId && v.Order > (
                _db.Volumes.Where(v2 => v2.Id == currentVolumeId).Select(v2 => v2.Order).FirstOrDefault()
            ))
            .OrderBy(v => v.Order)
            .FirstOrDefaultAsync();

        if (nextVolume == null)
            return null;

        return await _db.Chapters
            .Where(c => c.VolumeId == nextVolume.Id)
            .OrderBy(c => c.Order)
            .FirstOrDefaultAsync();
    }

    public async Task<Chapter?> GetPreviousChapterAsync(int bookId, int currentVolumeId, int currentOrder)
    {
        // Buscar en el volumen actual (capítulos anteriores)
        var prevInSameVolume = await _db.Chapters
            .Where(c => c.Volume.BookId == bookId 
                    && c.VolumeId == currentVolumeId 
                    && c.Order < currentOrder)
            .OrderByDescending(c => c.Order)
            .FirstOrDefaultAsync();

        if (prevInSameVolume != null)
            return prevInSameVolume;

        // Si no hay en el mismo volumen, buscar el último capítulo del volumen anterior
        var prevVolume = await _db.Volumes
            .Where(v => v.BookId == bookId && v.Order < (
                _db.Volumes.Where(v2 => v2.Id == currentVolumeId).Select(v2 => v2.Order).FirstOrDefault()
            ))
            .OrderByDescending(v => v.Order)
            .FirstOrDefaultAsync();

        if (prevVolume == null)
            return null;

        return await _db.Chapters
            .Where(c => c.VolumeId == prevVolume.Id)
            .OrderByDescending(c => c.Order)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetNextOrderForVolumeAsync(int volumeId)
    {
        var lastOrder = await _db.Chapters
            .Where(c => c.VolumeId == volumeId)
            .OrderByDescending(c => c.Order)
            .Select(c => c.Order)
            .FirstOrDefaultAsync();
        return lastOrder + 1;
    }

     public async Task UpdateChapterAsync(int chapterId, UpdateChapterDto updateChapterDto)
    {
        var chapter = await _db.Chapters.FindAsync(chapterId);
        if (chapter == null) 
            throw new KeyNotFoundException("Chapter not found");

        if (!string.IsNullOrEmpty(updateChapterDto.Title))
            chapter.Title = updateChapterDto.Title;
        
        if (updateChapterDto.Order.HasValue)
            chapter.Order = updateChapterDto.Order.Value;
        
        if (!string.IsNullOrEmpty(updateChapterDto.Content))
            chapter.Content = updateChapterDto.Content;
        
        if (updateChapterDto.IsActive.HasValue)
            chapter.IsActive = updateChapterDto.IsActive.Value;

        chapter.UpdateDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ToggleChapterActiveAsync(int chapterId, bool isActive)
    {
        var chapter = await _db.Chapters
        .Include(c => c.Volume) // Necesario para acceder a Volume.BookId
        .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null) return false;

        // Si el estado no cambia, no hacer nada
        if (chapter.IsActive == isActive)
        {
            await _db.SaveChangesAsync();
            return true;
        }

        var bookId = chapter.Volume.BookId;
        var wasActive = chapter.IsActive;
        chapter.IsActive = isActive;
        chapter.UpdateDate = DateTime.UtcNow;

        // Solo actualizar ChapterCount si hay un cambio real en activación
        if (wasActive != isActive)
        {
            var book = await _db.Books.FindAsync(bookId);
            if (book != null)
            {
                if (isActive)
                {
                    book.ChapterCount++; // Se activó → sumar
                }
                else
                {
                    var currentCount = book.ChapterCount ?? 0;
                    book.ChapterCount = Math.Max(0, currentCount - 1);
                }
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }
    
}
