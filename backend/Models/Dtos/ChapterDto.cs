using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class ChapterDto // para mostrar la lista de capitulos
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishDate { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
}
