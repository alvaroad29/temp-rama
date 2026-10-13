namespace backend.Models.Dtos;
public class VolumeSimpleDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Order { get; set; }
    public int ChapterCount { get; set; }
    public bool IsActive { get; set; } 
}