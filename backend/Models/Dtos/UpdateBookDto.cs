using System;
using System.ComponentModel.DataAnnotations;


namespace backend.Models.Dtos;


public class UpdateBookDto
{

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> GenreIds { get; set; } = new List<int>();

    [Required]
    public State State { get; set; }

    [Url]
    public string? ImageUrl { get; set; }
}