using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public enum State
{
    Activo = 1,
    Completado = 2,
    Pausado = 3
}

public class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? UpdateDate { get; set; }

    [Column(TypeName = "decimal(2,1)")]
    public decimal? Score { get; set; } = 0;
    public int TotalRatings { get; set; } = 0;
    public int? ChapterCount { get; set; }
    public int ViewCount { get; set; } 
    public bool IsActive { get; set; } = true;
    public State State { get; set; }
    public string? ImageUrl { get; set; }

    //Relacion con Genero
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public DateTime? LastChapterPublishDate { get; set; }

    // Relación con Author
    public string AuthorId { get; set; }
    public User Author { get; set; }


    // Relación con Volúmenes (1 libro → N volúmenes)
    public ICollection<Volume> Volumes { get; set; } = new List<Volume>();
    
    // Relación con las calificaciones (1 libro -> N calificaciones)
    public ICollection<BookRating> Ratings { get; set; } = new List<BookRating>();

}
