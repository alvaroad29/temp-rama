using System;
using AutoMapper;
using backend.Models.Dtos;

namespace backend.Mapping;

public class GenreProfile: Profile
{
    public GenreProfile()
    {
        CreateMap<Genre, GenreDto>().ReverseMap();
        CreateMap<Genre, CreateGenreDto>().ReverseMap();
    }
}
