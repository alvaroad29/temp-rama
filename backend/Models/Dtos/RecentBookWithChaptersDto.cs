using System;
namespace backend.Models.Dtos;

public class RecentBookWithChaptersDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public decimal? Score { get; set; }
    public int TotalRatings { get; set; }
    public State State { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<ChapterDto> RecentChapters { get; set; } = new List<ChapterDto>();
}