namespace backend.Models.Dtos;
public class ChapterViewHistoryDto
{
    public DateTime ViewDate { get; set; }
    public int ChapterId { get; set; }
    public string ChapterTitle { get; set; }
}