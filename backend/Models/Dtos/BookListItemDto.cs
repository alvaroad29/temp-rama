namespace backend.Models.Dtos;
public class BookListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public decimal? Score { get; set; }
    public DateTime AddedDate { get; set; }
    public int Order { get; set; }
    public State State { get; set; }
}