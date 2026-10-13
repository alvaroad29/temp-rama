using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.MicrosoftExtensions;

namespace backend.Models.Dtos;

public class CreateReportDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(5000)]
    public string Description { get; set; } = null!;

    public ReportType ReportType { get; set; } = ReportType.Otro;
}