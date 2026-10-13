using System;

namespace backend.Models.Dtos;

public class UserRegisterDto
{
    public string? Id { get; set; }
    public string DisplayName { get; set; } 
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } 
    public required string Email { get; set; } 
    public string? Role { get; set; } 

}
