using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Chapter
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    public int Order { get; set; } 
    public string Content { get; set; } = string.Empty; // Contenido del capítulo, texto, url o referencia
    public int ViewCount { get; set; } = 0;
    // En tu modelo (Chapter) y en ChapterDto
    public DateTime PublishDate { get; set; } = DateTime.Now;
    public DateTime? UpdateDate { get; set; }
    public bool IsActive { get; set; } = true; // por si necesito borrado logico


   // Relación con Volumen (N capítulos → 1 volumen)
    public int VolumeId { get; set; }
    public Volume Volume { get; set; }
}
