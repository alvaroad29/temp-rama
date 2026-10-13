using System;
using backend.Models;
using backend.Models.Dtos;  

namespace backend.Repository.IRepository;

public interface IVolumeRepository
{
    // Task<VolumeDto> GetVolumeWithChapters(int volumeId);
    Task<IEnumerable<VolumeSimpleDto>> GetVolumesSimpleByBook(int bookId);
    Task<IEnumerable<ChapterDto>> GetChaptersByVolume(int volumeId);
    Task<IEnumerable<VolumeDto>> GetVolumesByBook(int bookId);
    Task<Volume?> GetVolumeWithBook(int volumeId);
    Task<Volume> CreateVolume(VolumeCreateDto volumeDto);
    Task UpdateVolume(int volumeId, VolumeUpdateDto volumeDto);
    Task<bool> DeleteVolume(int volumeId);
    Task<bool> VolumeExists(int volumeId);
    Task<bool> ToggleVolumeActive(int volumeId, bool isActive);

}