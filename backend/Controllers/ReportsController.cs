using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using backend.Models;
using backend.Repository.IRepository;
using backend.Repository;
using backend.Models.Dtos;
using System.ComponentModel.DataAnnotations;
using backend.Models.Dtos.BookQueries;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _reportRepository;
    private readonly IMapper _mapper;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;

    public ReportsController(
        IReportRepository reportRepository,
        IMapper mapper,
        IBookRepository bookRepository,
        IUserRepository userRepository
    )
    {
        _reportRepository = reportRepository;
        _mapper = mapper;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Crea un nuevo reporte
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportDto>> CreateReport([FromBody] CreateReportDto createReportDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            // Verificar que el libro existe
            var book = await _bookRepository.GetBook(createReportDto.BookId);
            if (book == null)
                return NotFound("Libro no encontrado");

            var report = _mapper.Map<Report>(createReportDto);
            report.ReporterId = userId;
            report.CreationDate = DateTime.Now;
            report.Status = ReportStatus.Pendiente;

            var createdReport = await _reportRepository.CreateAsync(report);
            var reportDto = _mapper.Map<ReportDto>(createdReport);

            return Ok(reportDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene todos los reportes paginados (solo administradores)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<ReportDto>>> GetAllReports([FromQuery] ReportQueryParameters parameters)
    {
        try
        {
            // Validación de parámetros
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 10;

            var pagedResponse = await _reportRepository.GetAllAsync(parameters);
            var reportsDto = _mapper.Map<List<ReportDto>>(pagedResponse.Data);

            var response = new PagedResponse<ReportDto>(
                reportsDto,
                pagedResponse.PageNumber,
                pagedResponse.PageSize,
                pagedResponse.TotalRecords);

            return Ok(response);
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un reporte específico por ID (solo administradores)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportDto>> GetReport(int id)
    {
        try
        {
            var report = await _reportRepository.GetByIdAsync(id);
            if (report == null)
                return NotFound("Reporte no encontrado");

            var reportDto = _mapper.Map<ReportDto>(report);
            return Ok(reportDto);
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene reportes por estado (solo administradores)
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ReportDto>>> GetReportsByStatus(ReportStatus status)
    {
        try
        {
            var reports = await _reportRepository.GetByStatusAsync(status);
            var reportDtos = _mapper.Map<List<ReportDto>>(reports);
            return Ok(reportDtos);
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene reportes por libro (solo administradores)
    /// </summary>
    [HttpGet("book/{bookId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ReportDto>>> GetReportsByBook(int bookId)
    {
        try
        {
            var reports = await _reportRepository.GetByBookIdAsync(bookId);
            var reportDtos = _mapper.Map<List<ReportDto>>(reports);
            return Ok(reportDtos);
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene los reportes del usuario actual paginados
    /// </summary>
    [HttpGet("my-reports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<ReportDto>>> GetMyReports([FromQuery] ReportQueryParameters parameters)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            // Validación de parámetros
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 10;

            // Forzar que solo obtenga los reportes del usuario actual
            parameters.ReporterId = userId;

            var pagedResponse = await _reportRepository.GetAllAsync(parameters);
            var reportsDto = _mapper.Map<List<ReportDto>>(pagedResponse.Data);

            var response = new PagedResponse<ReportDto>(
                reportsDto,
                pagedResponse.PageNumber,
                pagedResponse.PageSize,
                pagedResponse.TotalRecords);

            return Ok(response);
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un reporte (solo admins)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportDto>> UpdateReport(int id, [FromBody] UpdateReportDto updateReportDto)
    {
        try
        {
            var report = await _reportRepository.GetByIdAsync(id);
            if (report == null)
                return NotFound("Reporte no encontrado");

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(adminId))
            {
                var adminUser = await _userRepository.GetUser(adminId);
                if (adminUser == null)
                    return BadRequest("Usuario administrador no encontrado");
            }

            // Actualizar propiedades
            report.Status = updateReportDto.Status;
            report.AdminNotes = updateReportDto.AdminNotes ?? report.AdminNotes;
            report.ReviewedByAdminId = adminId;
            report.ReviewedAt = DateTime.Now;

            var updatedReport = await _reportRepository.UpdateAsync(report);
            var reportDto = _mapper.Map<ReportDto>(updatedReport);



            return Ok(reportDto);
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Error interno del servidor");
        }
    }



    /// <summary>
    /// Obtiene el resumen de reportes (solo administradores)
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary()
    {
        try
        {
            var summary = await _reportRepository.GetSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {

            return StatusCode(500, "Error interno del servidor");
        }
    }


}



  // /// <summary>
    // /// Asigna un reporte a un administrador (solo administradores)
    // /// </summary>
    // [HttpPatch("{id}/assign")]
    // [Authorize(Roles = "Admin")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // public async Task<ActionResult<ReportDto>> AssignReport(int id, [FromBody] AssignReportDto assignDto)
    // {
    //     try
    //     {
    //         var report = await _reportRepository.GetByIdAsync(id);
    //         if (report == null)
    //             return NotFound("Reporte no encontrado");

    //         // Verificar que el usuario asignado existe y es admin
    //         var adminUser = await _userRepository.GetUser(assignDto.AdminAssigneeId);
    //         if (adminUser == null)
    //             return BadRequest("Usuario administrador no encontrado");

    //         report.AdminAssigneeId = assignDto.AdminAssigneeId;
    //         report.Status = ReportStatus.EnRevision;
    //         report.UpdateDate = DateTime.Now;

    //         var updatedReport = await _reportRepository.UpdateAsync(report);
    //         var reportDto = _mapper.Map<ReportDto>(updatedReport);

            

    //         return Ok(reportDto);
    //     }
    //     catch (Exception ex)
    //     {
            
    //         return StatusCode(500, "Error interno del servidor");
    //     }
    // }

    /// <summary>
    /// Cambia el estado de un reporte (solo administradores)
    /// </summary>
    // [HttpPatch("{id}/status")]
    // [Authorize(Roles = "Admin")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // public async Task<ActionResult<ReportDto>> ChangeStatus(int id, [FromBody] ChangeStatusDto statusDto)
    // {
    //     try
    //     {
    //         var report = await _reportRepository.GetByIdAsync(id);
    //         if (report == null)
    //             return NotFound("Reporte no encontrado");

    //         report.Status = statusDto.Status;
    //         report.UpdateDate = DateTime.Now;

    //         var updatedReport = await _reportRepository.UpdateAsync(report);
    //         var reportDto = _mapper.Map<ReportDto>(updatedReport);

            

    //         return Ok(reportDto);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, "Error interno del servidor");
    //     }
    // }
    
        // /// <summary>
    // /// Elimina un reporte (solo administradores)
    // /// </summary>
    // [HttpDelete("{id}")]
    // [Authorize(Roles = "Admin")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // public async Task<IActionResult> DeleteReport(int id)
    // {
    //     try
    //     {
    //         var result = await _reportRepository.DeleteAsync(id);
    //         if (!result)
    //             return NotFound("Reporte no encontrado");

            

    //         return NoContent();
    //     }
    //     catch (Exception ex)
    //     {
            
    //         return StatusCode(500, "Error interno del servidor");
    //     }
    // }

// DTOs adicionales para los nuevos endpoints
// public class AssignReportDto
// {
//     [Required]
//     public string AdminAssigneeId { get; set; } = null!;
// }

// public class ChangeStatusDto
// {
//     [Required]
//     public ReportStatus Status { get; set; }
// }