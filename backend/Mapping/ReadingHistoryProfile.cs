using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
// using Microsoft.AspNetCore.Identity.Client;
namespace backend.Mapping;

public class ReadingHistoryProfile: Profile
{
    public ReadingHistoryProfile()
    {
        CreateMap<ReadingHistory, ReadingHistoryDto>()
        .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.Chapter.Id))
        .ForMember(dest => dest.ChapterTitle, opt => opt.MapFrom(src => src.Chapter.Title))
        .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Chapter.Volume.Book.Id))
        .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Chapter.Volume.Book.Title))
        .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Chapter.Volume.Book.ImageUrl));
    }
}