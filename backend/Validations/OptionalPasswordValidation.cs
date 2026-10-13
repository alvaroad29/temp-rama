using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
public class OptionalPasswordValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var password = value as string;
        
        // Si está vacío o es null, es válido (opcional)
        if (string.IsNullOrEmpty(password))
            return ValidationResult.Success;

        // Si no está vacío, aplicar todas las validaciones
        if (!Regex.IsMatch(password, @"[a-z]"))
            return new ValidationResult("La contraseña debe contener al menos una letra minúscula.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return new ValidationResult("La contraseña debe contener al menos una letra mayúscula.");

        if (!Regex.IsMatch(password, @"[0-9]"))
            return new ValidationResult("La contraseña debe contener al menos un número.");

        if (Regex.IsMatch(password, @"\s")) 
            return new ValidationResult("La contraseña no puede contener espacios.");

        return ValidationResult.Success;
    }
}