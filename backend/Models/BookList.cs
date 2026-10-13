using System.ComponentModel.DataAnnotations;


namespace backend.Models;

public class BookList
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(100, ErrorMessage = "El título debe tener {1} caracteres o menos")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción debe tener {1} caracteres o menos")]
    public string? Description { get; set; }

    [StringLength(255, ErrorMessage = "La URL de imagen debe tener {1} caracteres o menos")]
    public string? ImageUrl { get; set; } = string.Empty;

    public DateTime CreationDate { get; set; } = DateTime.Now;

    public DateTime? UpdateDate { get; set; }

    [Range(0, 100)]
    public int BookCount { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int FollowersCount { get; set; } = 0;

    public bool IsPublic { get; set; } = true;
    public bool IsActive { get; set; } = true;

    // Relación con el creador (Usuario)
    [Required]
    public string CreatorId { get; set; } = string.Empty;
    public User Creator { get; set; } = null!;

    // Relación con géneros (N:N)
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();

    // Relación con libros a través de la tabla intermedia
    public ICollection<BookListBook> BookListBooks { get; set; } = new List<BookListBook>();

    // Relación con seguidores 
    public ICollection<BookListFollower> Followers { get; set; } = new List<BookListFollower>();
}