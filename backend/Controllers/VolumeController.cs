using System.Security.Claims;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using backend.Repository;
using backend.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    // [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]

    public class VolumeController : ControllerBase
    {
        private readonly IVolumeRepository _volumeRepository;
        private readonly IMapper _mapper;
        private readonly IBookRepository _bookRepository;
        private readonly UserManager<User> _userManager;

        public VolumeController(
            IVolumeRepository volumeRepository, 
            IMapper mapper,
            IBookRepository bookRepository,
            UserManager<User> userManager)
        {
            _volumeRepository = volumeRepository;
            _mapper = mapper;
            _bookRepository = bookRepository;
            _userManager = userManager;
        }

        // GET: api/Volume/book/1/volumes-simple
        [HttpGet("book/{bookId}/volumes-simple")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVolumesSimpleByBook(int bookId)
        {
            var volumes = await _volumeRepository.GetVolumesSimpleByBook(bookId);
            return Ok(volumes);
        }

        // GET: api/Volume/5/chapters
        [HttpGet("{volumeId}/chapters")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChaptersByVolume(int volumeId)
        {
            var chapters = await _volumeRepository.GetChaptersByVolume(volumeId);
            return Ok(chapters);
        }

        // GET: api/Volume/book/1/volumes
        [HttpGet("book/{bookId}/volumes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVolumesByBook(int bookId)
        {
            var volumes = await _volumeRepository.GetVolumesByBook(bookId);
            return Ok(volumes);
        }

        // POST: api/Volume/book/1/volumes
       [HttpPost("book/{bookId}/volumes")]
        [Authorize]
        public async Task<IActionResult> CreateVolume(int bookId, [FromBody] VolumeCreateDto volumeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar permisos: solo admin o dueño del libro
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            var book = await _bookRepository.GetBook(bookId);
            if (book == null)
                return NotFound($"Libro con ID {bookId} no encontrado");

            if (!isAdmin && book.AuthorId != currentUserId)
                return Forbid();

            volumeDto.BookId = bookId;
            var volume = await _volumeRepository.CreateVolume(volumeDto);
            
            // Mapear a DTO para evitar ciclos
            var volumeResponseDto = _mapper.Map<VolumeResponseDto>(volume);
            
            return CreatedAtAction(nameof(GetVolumesByBook), new { bookId }, volumeResponseDto);
        }

        // PUT: api/Volume/5
        [HttpPut("{volumeId}")]
        [Authorize]
        public async Task<IActionResult> UpdateVolume(int volumeId, [FromBody] VolumeUpdateDto volumeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verificar permisos: solo admin o dueño del libro
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                var volume = await _volumeRepository.GetVolumeWithBook(volumeId);
                if (volume == null)
                    return NotFound($"Volumen con ID {volumeId} no encontrado");

                if (!isAdmin && volume.Book.AuthorId != currentUserId)
                    return Forbid();

                await _volumeRepository.UpdateVolume(volumeId, volumeDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

       [HttpPatch("{volumeId}/toggle-active")]
[Authorize]
public async Task<IActionResult> ToggleVolumeActive(int volumeId, [FromBody] ToggleVolumeActiveDto toggleDto)
{
    try
    {
        // Verificar permisos: solo admin o dueño del libro
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        var volume = await _volumeRepository.GetVolumeWithBook(volumeId);
        if (volume == null)
            return NotFound($"Volumen con ID {volumeId} no encontrado");

        if (!isAdmin && volume.Book.AuthorId != currentUserId)
            return Forbid();

        var result = await _volumeRepository.ToggleVolumeActive(volumeId, toggleDto.IsActive);
        
        if (!result)
            return NotFound($"Volumen con ID {volumeId} no encontrado");

        return Ok(new { 
            message = toggleDto.IsActive ? "Volumen activado correctamente" : "Volumen desactivado correctamente",
            isActive = toggleDto.IsActive 
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error interno del servidor: {ex.Message}");
    }
}


    }

}
