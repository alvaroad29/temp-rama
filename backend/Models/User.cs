using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace backend.Models;

public class User: IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La URL de imagen debe tener {1} caracteres o menos")]
    public string? ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navegación
    public ICollection<Book> Books { get; set; } = new List<Book>();
    public ICollection<BookRating> BookRatings { get; set; } = new List<BookRating>(); 

    // Listas creadas por el usuario
    public ICollection<BookList> CreatedBookLists { get; set; } = new List<BookList>();
    
    // Listas que el usuario sigue
    public ICollection<BookListFollower> FollowedBookLists { get; set; } = new List<BookListFollower>();
}
