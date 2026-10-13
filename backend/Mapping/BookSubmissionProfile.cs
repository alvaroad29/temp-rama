using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;

namespace backend.Mapping;

public class BookSubmissionProfile : Profile
{
    public BookSubmissionProfile()
    {
       CreateMap<CreateBookSubmissionDto, BookSubmission>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => 
                string.Join(",", src.GenreIds)))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => BookSubmissionStatus.Pending));

        // Entity -> Response DTO
        CreateMap<BookSubmission, BookSubmissionResponseDto>()
            .ForMember(dest => dest.GenreIds, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.Genres)
                    ? src.Genres.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToList()
                    : new List<int>()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.ReviewedByAdminName, opt => opt.MapFrom(src =>
                src.ReviewedByAdmin != null ? src.ReviewedByAdmin.UserName : null));
                
        CreateMap<UpdateBookSubmissionDto, BookSubmission>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => 
                string.Join(",", src.GenreIds)))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => BookSubmissionStatus.Pending))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}