using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;

namespace backend.Mapping;


public class AnnouncementProfile : Profile
{
    public AnnouncementProfile()
    {
        CreateMap<Announcement, AnnouncementDto>()
            .ForMember(dest => dest.CreatedByUserName, 
                        opt => opt.MapFrom(src => src.CreatedBy.DisplayName));

        CreateMap<CreateAnnouncementDto, Announcement>();
        // .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => 
        //     src. ?? DateTime.UtcNow));

        CreateMap<UpdateAnnouncementDto, Announcement>();
    }
}