using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;

namespace backend.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, UserDataDto>().ReverseMap();
        CreateMap<UserDataDto, UpdateUserDto>().ReverseMap();
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UserLoginDto>().ReverseMap();
        CreateMap<User, UserLoginResponseDto>().ReverseMap();

        // Mapeo de User a AuthorDto
        CreateMap<User, AuthorDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName)); 
    }

}
