using System;
namespace backend.Models.Dtos;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? UpdateDate { get; set; } = null;
    public decimal? Score { get; set; }
    public int TotalRatings { get; set; }
    public string? Description { get; set; }
    public int? ChapterCount{ get; set; }
    public int ViewCount { get; set; }
    public State State { get; set; }
    public string ImageUrl { get; set; }
    public bool IsActive { get; set; }
    
    //Relacion con Genero
    public List<GenreDto> Genres { get; set; } = new List<GenreDto>();

    // Datos del autor
    public AuthorDto Author { get; set; }  
        

}