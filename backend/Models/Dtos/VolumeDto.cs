using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class VolumeDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Order { get; set; }
    public List<ChapterDto> Chapters { get; set; }
}