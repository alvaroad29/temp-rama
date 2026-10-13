using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
// using Microsoft.AspNetCore.Identity.Client;
namespace backend.Mapping;

public class ReportProfile: Profile
{
    public ReportProfile()
    {  
        CreateMap<CreateReportDto, Report>();
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
            .ForMember(dest => dest.BookImageUrl, opt => opt.MapFrom(src => src.Book.ImageUrl))
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter.DisplayName))
            .ForMember(dest => dest.ReporterEmail, opt => opt.MapFrom(src => src.Reporter.Email))
            .ForMember(dest => dest.ReviewedByAdminName , 
                opt => opt.MapFrom(src => src.ReviewedByAdmin != null ? src.ReviewedByAdmin.UserName : null));
        
        CreateMap<UpdateReportDto, Report>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    
    }
}