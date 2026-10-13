namespace backend.Models.Dtos;
public class BookListDetailDto : BookListDto
{
    public PagedResponse<BookListItemDto> Books { get; set; }
}