using System;
namespace backend.Models.Dtos;

public class CreateBookDto
{

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } // en realidad seria una imagen

    // Lista de IDs de géneros asociados
    public List<int> GenreIds { get; set; } = new();

}