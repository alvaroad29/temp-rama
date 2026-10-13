namespace backend.Models.Dtos;
public class ChapterAdminDto // Para administración con contenido
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Order { get; set; }
    public string Content { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public bool IsActive { get; set; }
    public int VolumeId { get; set; }
}