using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class UpdateAnnouncementDto
{
    [OptionalStringLength(200, MinimumLength = 1)]
    public string? Title { get; set; } = string.Empty;

    [OptionalStringLength(500, MinimumLength = 1)]
    public string? Content { get; set; } = string.Empty;

    public bool UpdateDate { get; set; }
}
