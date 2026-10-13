using System;

namespace backend.Models.Dtos;

public class BookRatingResponseDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string UserId { get; set; }
    public int RatingValue { get; set; }
    public DateTime RatingDate { get; set; }
    public string Username { get; set; }
}
