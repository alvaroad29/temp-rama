using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? DisplayName { get; set; } = string.Empty;
    public string? Username { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Role { get; set; } 

}
