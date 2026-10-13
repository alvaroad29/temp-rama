// DTO para las calificaciones que hizo un usuario (incluye info del libro)

namespace backend.Models.Dtos;
public class UserBookRatingDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; }
    public string BookImageUrl { get; set; }
    public int RatingValue { get; set; }
    public DateTime RatingDate { get; set; }
}