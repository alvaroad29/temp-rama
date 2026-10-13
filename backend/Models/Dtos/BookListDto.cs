using System.ComponentModel.DataAnnotations;
namespace backend.Models.Dtos;

public class BookListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public int BookCount { get; set; }
    public int FollowersCount { get; set; }
    public bool IsPublic { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public List<GenreDto> Genres { get; set; } = new List<GenreDto>();
    public bool IsFollowing { get; set; }
}
