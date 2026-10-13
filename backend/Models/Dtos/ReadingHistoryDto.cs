using System;
namespace backend.Models.Dtos;

public class ReadingHistoryDto
{
    public int ChapterId { get; set; }
    public string ChapterTitle { get; set; }
    public int BookId { get; set; }        // ID del libro
    public string BookTitle { get; set; }
    public string ImageUrl { get; set; }   // Portada del libro

    public DateTime ViewDate { get; set; }
}