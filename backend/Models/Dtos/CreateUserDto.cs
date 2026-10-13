using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class CreateUserDto
{
  [Required(ErrorMessage = "El nombre de usuario es requerido")]
  [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, guion bajo (_) y guion medio (-)")]
  [StringLength(20, MinimumLength = 5, ErrorMessage = "El nombre de usuario debe tener entre 5 y 20 caracteres")]
  public string Username { get; set; } = string.Empty; // Nombre de usuario único

  public string? DisplayName { get; set; } = string.Empty;
  [Required(ErrorMessage = "La contraseña es requerida")]
  [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
  [CustomPasswordValidation]
  public string Password { get; set; } = string.Empty;
  [Required(ErrorMessage = "El email es requerido")]
  [EmailAddress(ErrorMessage = "Formato de email inválido")]
  [StringLength(254, ErrorMessage = "El email no puede exceder los 254 caracteres")]
  public string Email { get; set; } = string.Empty;
  // [Required(ErrorMessage = "El campo role es requerido")]
  // public string Role { get; set; } = string.Empty;
}
