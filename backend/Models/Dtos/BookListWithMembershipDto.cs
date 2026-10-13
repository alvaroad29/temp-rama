namespace backend.Models.Dtos;

public class BookListWithMembershipDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int BookCount { get; set; }
    public bool ContainsBook { get; set; }
}