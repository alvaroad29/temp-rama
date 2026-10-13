using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
// using Microsoft.Identity.Client;
namespace backend.Mapping;

public class BookProfile: Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookDto>()
        .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres))
        .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()))
        .ReverseMap();


        CreateMap<Book, CreateBookDto>().ReverseMap();
        // CreateMap<Book, UpdateBookDto>().ReverseMap();

        CreateMap<Book, BookDetailsDto>()
        .ForMember(dest => dest.Volumes, opt => opt.MapFrom(src =>
            src.Volumes
            .Where(v => v.IsActive)
            .OrderBy(v => v.Order)));

        CreateMap<BookRating, BookRatingResponseDto>()
           .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName));
            
        CreateMap<BookRating, BookRatingDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName));

        // Mapeo para UserBookRatingDto (calificaciones de un usuario)
        CreateMap<BookRating, UserBookRatingDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
            .ForMember(dest => dest.BookImageUrl, opt => opt.MapFrom(src => src.Book.ImageUrl));

        // En tu AutoMapper Profile
        CreateMap<Book, RecentBookWithChaptersDto>()
            .ForMember(dest => dest.RecentChapters, opt => opt.MapFrom(src => 
                src.Volumes
                    .SelectMany(v => v.Chapters)
                    .OrderByDescending(c => c.PublishDate)
                    .Take(3)
                    .Select(c => new ChapterDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        ViewCount = c.ViewCount,
                        PublishDate = c.PublishDate,
                        Order = c.Order
                    })
                    .ToList()));
    }
}