using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;
public class UpdateBookSubmissionStatusDto
{
    [Required]
    public BookSubmissionStatus Status { get; set; }

    [StringLength(1000)]
    public string? AdminFeedback { get; set; }
}