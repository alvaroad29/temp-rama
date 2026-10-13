namespace backend.Models;
public class BookListFollower
{
    public int BookListId { get; set; }
    public BookList BookList { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    public DateTime FollowDate { get; set; } = DateTime.Now;
}