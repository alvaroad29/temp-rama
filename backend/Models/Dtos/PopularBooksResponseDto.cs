namespace backend.Models.Dtos;

public class PopularBooksResponseDto
{
    public List<BookDto> Daily { get; set; } = new();
    public List<BookDto> Weekly { get; set; } = new();
    public List<BookDto> Monthly { get; set; } = new();
}