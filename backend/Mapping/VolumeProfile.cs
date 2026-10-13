using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
// using Microsoft.AspNetCore.Identity.Client;
namespace backend.Mapping;

public class VolumeProfile: Profile
{
    public VolumeProfile()
    {
           // Mapeo de Volume a VolumeDto
        CreateMap<Volume, VolumeDto>()
            .ForMember(dest => dest.Chapters, opt => opt.MapFrom(src => 
                src.Chapters.Where(c => c.IsActive).OrderBy(c => c.Order)));

        // Mapeo de Volume a VolumeSimpleDto
        CreateMap<Volume, VolumeSimpleDto>()
            .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => 
                src.Chapters.Count(c => c.IsActive)));

        CreateMap<Volume, VolumeResponseDto>();

        // Mapeo de Chapter a ChapterDto
        CreateMap<Chapter, ChapterDto>();

        // Mapeo para crear volumen
        CreateMap<VolumeCreateDto, Volume>();

        // Mapeo para actualizar volumen
        CreateMap<VolumeUpdateDto, Volume>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}