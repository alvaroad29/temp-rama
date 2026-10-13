namespace backend.Models.Dtos;
public class VolumeResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
    public int BookId { get; set; }
}