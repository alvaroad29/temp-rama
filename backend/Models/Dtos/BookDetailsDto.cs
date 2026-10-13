namespace backend.Models.Dtos;
public class BookDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public decimal? Score { get; set; }
    public int TotalRatings { get; set; }
    public string? Description { get; set; }
    public int? ChapterCount { get; set; }
    public int ViewCount { get; set; }
    public State State { get; set; }
    public string ImageUrl { get; set; }
    
    public List<GenreDto> Genres { get; set; } = new List<GenreDto>();
    public AuthorDto Author { get; set; }
    
    public List<VolumeDto> Volumes { get; set; } = new List<VolumeDto>();
}

