using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using backend.Repository.IRepository;

namespace backend.Repository;

public class VolumeRepository : IVolumeRepository
{
    ApplicationDbContext _db;

    public VolumeRepository(ApplicationDbContext db)
    {
        _db = db;
    }


     // Nuevo: Obtener volúmenes simples (sin capítulos) de un libro
    public async Task<IEnumerable<VolumeSimpleDto>> GetVolumesSimpleByBook(int bookId)
    {
        return await _db.Volumes
            .Where(v => v.BookId == bookId)
            .OrderBy(v => v.Order)
            .Select(v => new VolumeSimpleDto
            {
                Id = v.Id,
                Title = v.Title,
                Order = v.Order,
                ChapterCount = v.Chapters.Count(c => c.IsActive),
                IsActive = v.IsActive
            })
            .ToListAsync();
    }

    // Nuevo: Obtener capítulos de un volumen específico
    public async Task<IEnumerable<ChapterDto>> GetChaptersByVolume(int volumeId)
    {
        return await _db.Chapters
            .Where(c => c.VolumeId == volumeId)
            .OrderBy(c => c.Order)
            .Select(c => new ChapterDto
            {
                Id = c.Id,
                Title = c.Title,
                ViewCount = c.ViewCount,
                PublishDate = c.PublishDate,
                Order = c.Order,
                IsActive = c.IsActive
            })
            .ToListAsync();
    }

    // Existente: Obtener volúmenes con capítulos activos de un libro
    public async Task<IEnumerable<VolumeDto>> GetVolumesByBook(int bookId)
    {
        return await _db.Volumes
            .Where(v => v.BookId == bookId && v.IsActive)
            .OrderBy(v => v.Order)
            .Select(v => new VolumeDto
            {
                Id = v.Id,
                Title = v.Title,
                Order = v.Order,
                Chapters = v.Chapters
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Order)
                    .Select(c => new ChapterDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        ViewCount = c.ViewCount,
                        PublishDate = c.PublishDate,
                        Order = c.Order,
                        IsActive = c.IsActive
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    // Nuevo: Obtener volumen con información del libro para verificar permisos
    public async Task<Volume?> GetVolumeWithBook(int volumeId)
    {
        return await _db.Volumes
            .Include(v => v.Book)
            .FirstOrDefaultAsync(v => v.Id == volumeId);
    }

    // Crear volumen
    public async Task<Volume> CreateVolume(VolumeCreateDto volumeDto)
    {
        var volume = new Volume
        {
            Title = volumeDto.Title,
            Order = volumeDto.Order,
            BookId = volumeDto.BookId,
            IsActive = true
        };

        _db.Volumes.Add(volume);
        await _db.SaveChangesAsync();
        return volume;
    }

    // Actualizar volumen
    public async Task UpdateVolume(int volumeId, VolumeUpdateDto volumeDto)
    {
        var volume = await _db.Volumes.FindAsync(volumeId);
        if (volume == null) throw new KeyNotFoundException("Volume not found");

        if (volumeDto.Title != null)
            volume.Title = volumeDto.Title;
        
        if (volumeDto.Order.HasValue)
            volume.Order = volumeDto.Order.Value;
        
        if (volumeDto.IsActive.HasValue)
            volume.IsActive = volumeDto.IsActive.Value;

        await _db.SaveChangesAsync();
    }

    // Eliminar volumen (soft delete)
    public async Task<bool> DeleteVolume(int volumeId)
    {
        var volume = await _db.Volumes.FindAsync(volumeId);
        if (volume == null) return false;

        volume.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VolumeExists(int volumeId)
    {
        return await _db.Volumes
            .AnyAsync(v => v.Id == volumeId && v.IsActive);
    }

    public async Task<bool> ToggleVolumeActive(int volumeId, bool isActive)
{
    var volume = await _db.Volumes.FindAsync(volumeId);
    if (volume == null) return false;

    volume.IsActive = isActive;
    await _db.SaveChangesAsync();
    return true;
}
}
