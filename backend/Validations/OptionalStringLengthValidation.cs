using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
public class OptionalStringLengthAttribute : ValidationAttribute
{
    public int MaximumLength { get; set; }
    public int MinimumLength { get; set; }

    public OptionalStringLengthAttribute(int maximumLength)
    {
        MaximumLength = maximumLength;
        MinimumLength = 0;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var stringValue = value as string;

        // Si es null o vacío, es válido (opcional)
        if (string.IsNullOrEmpty(stringValue))
            return ValidationResult.Success;

        // Si no está vacío, validar longitud
        if (stringValue.Length < MinimumLength || stringValue.Length > MaximumLength)
            return new ValidationResult($"El campo debe tener entre {MinimumLength} y {MaximumLength} caracteres.");            

        return ValidationResult.Success;
    }
}