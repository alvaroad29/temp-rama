using System;

namespace backend.Models.Dtos;

public class UserDataDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public string? ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    // Propiedades para baneo
    public bool IsBanned { get; set; }
    public DateTimeOffset? BannedUntil { get; set; }
    
    // Propiedades adicionales para ordenamiento
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
}
