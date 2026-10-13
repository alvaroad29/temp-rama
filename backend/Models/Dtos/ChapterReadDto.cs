namespace backend.Models.Dtos;
public class ChapterReadDto // se usa cuando vas a leer
{
    public string Title { get; set; }
    public int Order { get; set; }
    public string Content { get; set; }
    public int? PreviousChapterId { get; set; }
    public int? NextChapterId { get; set; }
}