namespace backend.Models;

public class ReadingHistory
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public Chapter Chapter { get; set; }
    public string?  UserId { get; set; }
    public User? User { get; set; }
    public DateTime ViewDate { get; set; } = DateTime.UtcNow;
}