namespace backend.Models.Dtos;
using System.ComponentModel.DataAnnotations;
public class AssignRoleDto
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}