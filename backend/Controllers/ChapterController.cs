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
// using UglyToad.PdfPig;
// using DocumentFormat.OpenXml.Packaging;
using System.IO;

namespace backend.Controllers
{
    // [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]

    public class ChapterController : ControllerBase
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IVolumeRepository _volumeRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public ChapterController(
            IChapterRepository chapterRepository,
         IVolumeRepository volumeRepository,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _chapterRepository = chapterRepository;
            _volumeRepository = volumeRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterReadDto>> GetChapter(int id)
        {
            // 1. Obtener el capítulo
            var chapter = await _chapterRepository.GetChapterByIdAsync(id);
            if (chapter == null)
            {
                return NotFound($"Capítulo con ID {id} no encontrado.");
            }

            var bookId = chapter.Volume.BookId;

            // 2. Incrementar ViewCount (siempre)
            await _chapterRepository.IncrementViewCountAsync(chapter);

            // 3. Si el usuario está logueado, registrar en ReadingHistory
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                // Evitar duplicados: verificar si ya vio este capítulo
                var hasViewed = await _chapterRepository.HasUserViewedChapterAsync(id, currentUser.Id);
                if (!hasViewed)
                {
                    var history = new ReadingHistory
                    {
                        ChapterId = id,
                        UserId = currentUser.Id,
                        ViewDate = DateTime.UtcNow
                    };
                    await _chapterRepository.AddToReadingHistoryAsync(history);
                }
            } else {
                 var history = new ReadingHistory
                {
                    ChapterId = id,
                    UserId = null,
                    ViewDate = DateTime.UtcNow
                };
                await _chapterRepository.AddToReadingHistoryAsync(history);
            }

            // 4. Calcular IDs de anterior y siguiente
            var previous = await _chapterRepository.GetPreviousChapterAsync(bookId, chapter.VolumeId, chapter.Order);
            var next = await _chapterRepository.GetNextChapterAsync(bookId, chapter.VolumeId, chapter.Order);

            // 5. Mapear a DTO y devolver
            var dto = _mapper.Map<ChapterReadDto>(chapter);
            dto.PreviousChapterId = previous?.Id;
            dto.NextChapterId = next?.Id;
            return Ok(dto);
        }

       // GET: api/Chapter/5/admin (mas simple, para editar)
        [HttpGet("{id}/admin")]
        [Authorize]
        public async Task<ActionResult<ChapterAdminDto>> GetChapterForAdmin(int id)
        {
            var chapter = await _chapterRepository.GetChapterByIdAsync(id);
            if (chapter == null)
            {
                return NotFound($"Capítulo con ID {id} no encontrado.");
            }

            // Verificar permisos: solo admin o dueño del libro
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            var volume = await _volumeRepository.GetVolumeWithBook(chapter.VolumeId);
            if (volume == null)
                return NotFound($"Volumen no encontrado");

            if (!isAdmin && volume.Book.AuthorId != currentUserId)
                return Forbid();

            var dto = _mapper.Map<ChapterAdminDto>(chapter);
            return Ok(dto);
        }

        // POST: api/Chapter
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDto createChapterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validar que el volumen existe
                var volume = await _volumeRepository.GetVolumeWithBook(createChapterDto.VolumeId);
                if (volume == null)
                    return NotFound(new { message = "Volumen no encontrado" });

                // Verificar permisos: solo admin o dueño del libro
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && volume.Book.AuthorId != currentUserId)
                    return Forbid();

                // Calcular automáticamente el siguiente Order disponible
                int nextOrder = await _chapterRepository.GetNextOrderForVolumeAsync(createChapterDto.VolumeId);

                var chapter = new Chapter
                {
                    Title = createChapterDto.Title,
                    Content = createChapterDto.Content,
                    VolumeId = createChapterDto.VolumeId,
                    Order = nextOrder,
                    PublishDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    ViewCount = 0,
                    IsActive = createChapterDto.IsActive 
                };

                var createdChapter = await _chapterRepository.CreateChapterAsync(chapter);
                var chapterDto = _mapper.Map<ChapterAdminDto>(createdChapter);

                return Ok(chapterDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // PUT: api/Chapter/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateChapter(int id, [FromBody] UpdateChapterDto updateChapterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var chapter = await _chapterRepository.GetChapterByIdAsync(id);
                if (chapter == null)
                    return NotFound($"Capítulo con ID {id} no encontrado");

                // Verificar permisos: solo admin o dueño del libro
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                var volume = await _volumeRepository.GetVolumeWithBook(chapter.VolumeId);
                if (volume == null)
                    return NotFound($"Volumen no encontrado");

                if (!isAdmin && volume.Book.AuthorId != currentUserId)
                    return Forbid();

                await _chapterRepository.UpdateChapterAsync(id, updateChapterDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // PATCH: api/Chapter/5/toggle-active
        [HttpPatch("{id}/toggle-active")]
        [Authorize]
        public async Task<IActionResult> ToggleChapterActive(int id, [FromBody] ToggleChapterActiveDto toggleDto)
        {
            try
            {
                var chapter = await _chapterRepository.GetChapterByIdAsync(id);
                if (chapter == null)
                    return NotFound($"Capítulo con ID {id} no encontrado");

                // Verificar permisos: solo admin o dueño del libro
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
                var volume = await _volumeRepository.GetVolumeWithBook(chapter.VolumeId);
                if (volume == null)
                    return NotFound($"Volumen no encontrado");

                if (!isAdmin && volume.Book.AuthorId != currentUserId)
                    return Forbid();

                var result = await _chapterRepository.ToggleChapterActiveAsync(id, toggleDto.IsActive);
                
                if (!result)
                    return NotFound($"Capítulo con ID {id} no encontrado");

                return Ok(new { 
                    message = toggleDto.IsActive ? "Capítulo activado correctamente" : "Capítulo desactivado correctamente",
                    isActive = toggleDto.IsActive 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        
        // [HttpDelete("{id}")]
        // [Authorize]
        // public async Task<IActionResult> DeleteChapter(int id)
        // {
        //     try
        //     {
        //         var chapter = await _chapterRepository.GetChapterByIdAsync(id);
        //         if (chapter == null)
        //             return NotFound($"Capítulo con ID {id} no encontrado");

        //         // Verificar permisos: solo admin o dueño del libro
        //         var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //         var isAdmin = User.IsInRole("Admin");
                
        //         var volume = await _volumeRepository.GetVolumeWithBook(chapter.VolumeId);
        //         if (volume == null)
        //             return NotFound($"Volumen no encontrado");

        //         if (!isAdmin && volume.Book.AuthorId != currentUserId)
        //             return Forbid();

        //         var result = await _chapterRepository.SoftDeleteChapterAsync(id);
        //         return result ? NoContent() : NotFound();
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { message = "Error interno del servidor" });
        //     }
        // }

        

    }
}
