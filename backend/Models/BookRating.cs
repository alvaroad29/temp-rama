using System.ComponentModel.DataAnnotations;


namespace backend.Models;

public class BookRating
{
    public int Id { get; set; }
    
    [Range(1, 5)]
    public int RatingValue  { get; set; }
    public DateTime RatingDate { get; set; } = DateTime.Now;
    
    // Claves foráneas
    public int BookId { get; set; }
    public string UserId { get; set; }
    
    // Navegación
    public Book Book { get; set; }
    public User User { get; set; }
}
