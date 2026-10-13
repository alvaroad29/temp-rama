using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class UpdateUserDto
{
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, guion bajo (_) y guion medio (-)")]
    [OptionalStringLength(200, MinimumLength = 5)]
    public string? Username { get; set; }

    [OptionalStringLength(100, MinimumLength = 6)]
    [OptionalPasswordValidation]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmPassword { get; set; }

    [OptionalStringLength(100, MinimumLength = 6)]
    // [OptionalPasswordValidation]
    public string? CurrentPassword { get; set; }

    public string? ImageUrl { get; set; }

}