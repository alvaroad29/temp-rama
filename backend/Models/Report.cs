using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public enum ReportStatus
{
    Pendiente = 1,
    EnRevision = 2,
    Resuelto = 3
}

public enum ReportType
{
    Error = 1,
    ContenidoInapropiado = 2,
    DerechosAutor = 3,
    Otro = 4
}

public class Report
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    // Relación con el libro reportado
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Relación con el usuario que reporta
    public string ReporterId { get; set; } = null!;
    public User Reporter { get; set; } = null!;

    // Estado y tipo del reporte
    public ReportStatus Status { get; set; } = ReportStatus.Pendiente;
    public ReportType ReportType { get; set; } = ReportType.Otro;

    // Administrador que gestiona el reporte (opcional)
    public string? ReviewedByAdminId { get; set; }
    public User? ReviewedByAdmin { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public string? AdminNotes { get; set; }
    
    // fechas

    public DateTime CreationDate { get; set; } = DateTime.Now;

}