// Controllers/BookSubmissionsController.cs
using AutoMapper;
using backend.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookSubmissionsController : ControllerBase
{
    private readonly IBookSubmissionRepository _submissionRepository;
    private readonly IMapper _mapper;
    private readonly IImageService _imageService;


    public BookSubmissionsController(
        IBookSubmissionRepository submissionRepository,
        IMapper mapper,
        IImageService imageService
    )
    {
        _submissionRepository = submissionRepository;
        _mapper = mapper;
        _imageService = imageService;
    }

    // POST: api/booksubmissions
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookSubmissionResponseDto>> CreateSubmission(CreateBookSubmissionDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var submission = _mapper.Map<BookSubmission>(createDto);
            submission.UserId = userId;

            var createdSubmission = await _submissionRepository.CreateAsync(submission);
            var result = _mapper.Map<BookSubmissionResponseDto>(createdSubmission);

            // No mostrar info de usuer y admin en dto
            result.UserId = null;
            result.UserName = null;
            result.ReviewedByAdminName = null;

            return CreatedAtAction(nameof(GetSubmission), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<BookSubmissionResponseDto>> UpdateSubmission(int id, UpdateBookSubmissionDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            // Obtener la submission con el usuario
            var submission = await _submissionRepository.GetByIdWithUserAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = "Solicitud no encontrada" });
            }

            // Verificar que el usuario es el dueño
            if (submission.UserId != userId)
            {
                return Forbid();
            }

            // Verificar que la submission está en estado NeedsChanges
            if (submission.Status != BookSubmissionStatus.NeedsChanges)
            {
                return BadRequest(new { message = "Solo se pueden editar solicitudes que requieren cambios" });
            }

            // Actualizar la submission
            var updatedSubmission = await _submissionRepository.UpdateAsync(id, updateDto);
            if (updatedSubmission == null)
            {
                return NotFound(new { message = "Error al actualizar la solicitud" });
            }

            var result = _mapper.Map<BookSubmissionResponseDto>(updatedSubmission);

            // Ocultar información sensible para el usuario
            result.UserId = null;
            result.UserName = null;
            result.ReviewedByAdminName = null;

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // GET: api/booksubmissions/my-submissions
    [HttpGet("my-submissions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BookSubmissionResponseDto>>> GetMySubmissions()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var submissions = await _submissionRepository.GetByUserIdAsync(userId);
            var result = _mapper.Map<IEnumerable<BookSubmissionResponseDto>>(submissions);

            // Para usuario normal, ocultar información de admin
            foreach (var item in result)
            {
                item.UserId = null;
                item.UserName = null;
                item.ReviewedByAdminName = null;
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // GET: api/booksubmissions/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<BookSubmissionResponseDto>> GetSubmission(int id)
    {
        try
        {
            var submission = await _submissionRepository.GetByIdWithUserAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = "Solicitud no encontrada" });
            }

            // Solo el dueño o un admin puede ver la submission
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && submission.UserId != userId)
            {
                return Forbid();
            }

            var result = _mapper.Map<BookSubmissionResponseDto>(submission);

            // Si no es admin, ocultar información sensible
            if (!isAdmin)
            {
                result.UserId = null;
                result.UserName = null;
                result.ReviewedByAdminName = null;
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // GET: api/booksubmissions/admin/pending
    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BookSubmissionResponseDto>>> GetPendingSubmissions()
    {
        try
        {
            var submissions = await _submissionRepository.GetPendingSubmissionsAsync();
            var result = _mapper.Map<IEnumerable<BookSubmissionResponseDto>>(submissions);

            // Para admin, mostrar toda la información
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // GET: api/booksubmissions/admin/all
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BookSubmissionResponseDto>>> GetAllSubmissions()
    {
        try
        {
            var submissions = await _submissionRepository.GetAllWithUsersAsync();
            var result = _mapper.Map<IEnumerable<BookSubmissionResponseDto>>(submissions);

            // Para admin, mostrar toda la información
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // PUT: api/booksubmissions/admin/{id}/status
    [HttpPut("admin/{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSubmissionStatus(int id, UpdateBookSubmissionStatusDto updateDto)
    {
        try
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new { message = "Admin no autenticado" });
            }

            var success = await _submissionRepository.UpdateStatusAsync(
                id, updateDto.Status, updateDto.AdminFeedback, adminId);

            if (!success)
            {
                return NotFound(new { message = "Solicitud no encontrada" });
            }

            return Ok(new { message = "Estado actualizado correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // DELETE: api/booksubmissions/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteSubmission(int id)
    {
        try
        {
            var submission = await _submissionRepository.GetByIdAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = "Solicitud no encontrada" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && submission.UserId != userId)
            {
                return Forbid();
            }

            // Solo permitir eliminar si está pendiente o el usuario es admin
            if (!isAdmin && submission.Status != BookSubmissionStatus.Pending)
            {
                return BadRequest(new { message = "Solo se pueden eliminar solicitudes pendientes" });
            }

            var success = await _submissionRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Solicitud no encontrada" });
            }

            return Ok(new { message = "Solicitud eliminada correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }


    [HttpPost("cover")]
    [Authorize]
    public async Task<IActionResult> UploadSubmissionCover(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Error = "No se envió ningún archivo" });

            if (file.Length > 2 * 1024 * 1024) // 2 MB máximo (más que perfiles)
                return BadRequest(new { Error = "La imagen no puede superar 2 MB" });

            // Validar que sea una imagen
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { Error = "Formato de imagen no válido. Use JPG, PNG o WebP" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });

            // Subir imagen a Cloudinary en carpeta específica para submissions
            string imageUrl = await _imageService.UploadImageAsync(
                file,
                "book-submissions", // Carpeta específica para submissions
                maxWidth: 800,      // Tamaño adecuado para portadas
                maxHeight: 1200
            );

            return Ok(new
            {
                Url = imageUrl,
                Message = "Portada subida correctamente",
                FileName = file.FileName
            });
        }
        catch (Exception ex)
        {

            return StatusCode(500, new { Error = "Error interno del servidor al subir la imagen" });
        }
    }

   [HttpDelete("cover")]
[Authorize]
public async Task<IActionResult> DeleteSubmissionCover([FromQuery] string imageUrl)
{
    try
    {
        if (string.IsNullOrEmpty(imageUrl))
            return BadRequest(new { Error = "URL de imagen no proporcionada" });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Usuario no autenticado" });

        // Verificar que la imagen pertenece a una submission del usuario
        var userHasSubmissionWithThisCover = await _submissionRepository.UserHasSubmissionWithCoverAsync(userId, imageUrl);
        if (!userHasSubmissionWithThisCover)
        {
            return BadRequest(new { Error = "No tienes permisos para eliminar esta imagen o la imagen no existe" });
        }

        // Eliminar de Cloudinary
        try
        {
            var publicId = await _imageService.ExtractPublicIdFromUrl(imageUrl);
            await _imageService.DeleteImageAsync(publicId);
        }
        catch (Exception ex)
        {
            // _logger.LogWarning(ex, "No se pudo eliminar la imagen de Cloudinary, pero continuamos");
            // Continuamos aunque falle la eliminación en Cloudinary
        }

        return Ok(new { message = "Portada eliminada correctamente" });
    }
    catch (Exception ex)
    {
        
        return StatusCode(500, new { Error = "Error interno del servidor al eliminar la imagen" });
    }
}

// GET: api/booksubmissions/admin/stats
[HttpGet("admin/stats")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<Dictionary<BookSubmissionStatus, int>>> GetSubmissionStats()
{
    try
    {
        var stats = await _submissionRepository.GetSubmissionStatsAsync();
        return Ok(stats);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
}
