using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
// using Microsoft.AspNetCore.Identity.Client;
namespace backend.Mapping;

public class ChapterProfile: Profile
{
    public ChapterProfile()
    {
        CreateMap<Chapter, ChapterReadDto>();
        CreateMap<Chapter, ChapterDto>();
        CreateMap<Chapter, ChapterAdminDto>();
        CreateMap<CreateChapterDto, Chapter>();
        CreateMap<UpdateChapterDto, Chapter>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}