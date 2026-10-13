using AutoMapper;
using backend.Models;
using backend.Models.Dtos;

namespace backend.Mapping;

public class BookListProfile : Profile
{
    public BookListProfile()
    {
        CreateMap<CreateBookListDto, BookList>();
        CreateMap<UpdateBookListDto, BookList>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<BookList, BookListDto>()
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator.UserName))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres));
            

        CreateMap<BookList, BookListDetailDto>()
           .IncludeBase<BookList, BookListDto>()
           .ForMember(dest => dest.Books, opt => opt.Ignore());

        CreateMap<Genre, GenreDto>();

        CreateMap<BookList, BookListWithMembershipDto>()
            .ForMember(dest => dest.ContainsBook, opt => opt.MapFrom(src => 
                src.BookListBooks.Any(blb => blb.Book.IsActive)));
    }
}