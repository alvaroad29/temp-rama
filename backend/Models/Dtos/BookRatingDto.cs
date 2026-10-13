//  dto para mostrar todas calificaciones de un libro

namespace backend.Models.Dtos;
public class BookRatingDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public int RatingValue { get; set; }
    public DateTime RatingDate { get; set; }
}