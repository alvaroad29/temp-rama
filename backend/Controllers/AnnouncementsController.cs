using System.Security.Claims;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementRepository _repository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public AnnouncementsController(
            IAnnouncementRepository repository,
            IMapper mapper,
            UserManager<User> userManager
        )
        {
            _repository = repository;
            _mapper = mapper;
            _userManager = userManager;
        
        }

        /// <summary>
        /// Obtiene todos los anuncios
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Permitir que cualquier usuario vea los anuncios
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAllAnnouncements()
        {
            try
            {
                var announcements = await _repository.GetAllAsync();
                var announcementDtos = _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
                
                
                return Ok(announcementDtos);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error interno del servidor al obtener los anuncios" });
            }
        }

        /// <summary>
        /// Obtiene un anuncio por ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous] // Permitir que cualquier usuario vea un anuncio específico
        public async Task<ActionResult<AnnouncementDto>> GetAnnouncement(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "El ID debe ser un número positivo" });
                }

                var announcement = await _repository.GetByIdAsync(id);
                
                if (announcement == null)
                {
                    
                    return NotFound(new { message = $"No se encontró el anuncio con ID {id}" });
                }

                var announcementDto = _mapper.Map<AnnouncementDto>(announcement);
                return Ok(announcementDto);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error interno del servidor al obtener el anuncio" });
            }
        }

        /// <summary>
        /// Crea un nuevo anuncio
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AnnouncementDto>> CreateAnnouncement([FromBody] CreateAnnouncementDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                var announcement = _mapper.Map<Announcement>(createDto);
                announcement.CreatedByUserId = userId;
                announcement.CreatedAt =  DateTime.Now;

                var createdAnnouncement = await _repository.CreateAsync(announcement);
                var announcementDto = _mapper.Map<AnnouncementDto>(createdAnnouncement);

                return CreatedAtAction(
                    nameof(GetAnnouncement),
                    new { id = createdAnnouncement.Id },
                    announcementDto);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error interno del servidor al crear el anuncio" });
            }
        }

        /// <summary>
        /// Actualiza un anuncio existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AnnouncementDto>> UpdateAnnouncement(int id, [FromBody] UpdateAnnouncementDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingAnnouncement = await _repository.GetByIdAsync(id);
                if (existingAnnouncement == null)
                {
                    return NotFound(new { message = $"No se encontró el anuncio con ID {id}" });
                }

                 if (!string.IsNullOrEmpty(updateDto.Title))
                 existingAnnouncement.Title = updateDto.Title;
            
                if (!string.IsNullOrEmpty(updateDto.Content))
                    existingAnnouncement.Content = updateDto.Content;
                    
                if (updateDto.UpdateDate)
                {
                    existingAnnouncement.CreatedAt = DateTime.Now;
                }
                
                
                var updatedAnnouncement = await _repository.UpdateAsync(existingAnnouncement);
                var announcementDto = _mapper.Map<AnnouncementDto>(updatedAnnouncement);

                
                return Ok(announcementDto);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error interno del servidor al actualizar el anuncio" });
            }
        }

        /// <summary>
        /// Elimina un anuncio
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAnnouncement(int id)
        {
            try
            {
                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound(new { message = $"No se encontró el anuncio con ID {id}" });
                }

                var deleted = await _repository.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, new { message = "Error al eliminar el anuncio" });
                }


                return NoContent();
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error interno del servidor al eliminar el anuncio" });
            }
        }

        /// <summary>
        /// Obtiene el último anuncio (más reciente por fecha)
        /// </summary>
        [HttpGet("latest")]
        [AllowAnonymous] // Permitir que cualquier usuario vea el último anuncio
        public async Task<ActionResult<AnnouncementDto>> GetLatestAnnouncement()
        {
            try
            {
                var announcement = await _repository.GetLatestAsync();
                
                if (announcement == null)
                {
                    return NotFound(new { message = "No hay anuncios disponibles" });
                }

                var announcementDto = _mapper.Map<AnnouncementDto>(announcement);
                return Ok(announcementDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al obtener el último anuncio" });
            }
        }
    }
}
