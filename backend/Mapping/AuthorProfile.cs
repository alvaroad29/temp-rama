using System;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;

namespace backend.Mapping;


public class AuthorProfile: Profile
{
    public AuthorProfile()
    {
        CreateMap<Author, AuthorDto>().ReverseMap();
        CreateMap<Author, CreateAuthorDto>().ReverseMap();
    }
}
