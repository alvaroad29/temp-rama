using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Volume
{
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    public int Order { get; set; } // para ordenarlos
    public bool IsActive { get; set; } = true; // por si necesito borrado logico
    // Relación con Libro (N volúmenes → 1 libro)
    public int BookId { get; set; }
    public Book Book { get; set; }

    // Relación con Capítulos (1 volumen → N capítulos)
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}
