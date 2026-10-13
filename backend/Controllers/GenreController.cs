using AutoMapper;
using backend.Models.Dtos;
using backend.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]

    public class GenreController : ControllerBase
    {
        private readonly IGenreRepository _genreRepository;
        private readonly IMapper _mapper;

        public GenreController(IGenreRepository genreRepository, IMapper mapper)
        {
            _genreRepository = genreRepository;
            _mapper = mapper;
        }

       [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _genreRepository.GetGenres();
            var genresDto = _mapper.Map<List<GenreDto>>(genres);
            return Ok(genresDto);
        }

        [HttpGet("admin/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllGenresForAdmin()
        {
            var genres = await _genreRepository.GetAllGenresForAdmin();
            var genresDto = _mapper.Map<List<GenreDto>>(genres);
            return Ok(genresDto);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetGenre")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGenre(int id)
        {
            var genre = await _genreRepository.GetGenreForAdmin(id);

            if (genre == null)
            {
                return NotFound($"El género con el id {id} no existe o está inactivo");
            }
            
            var genreDto = _mapper.Map<GenreDto>(genre);
            return Ok(genreDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateGenre([FromBody] CreateGenreDto createGenreDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (await _genreRepository.GenreExists(createGenreDto.Name))
            {
                ModelState.AddModelError("", "El género ya existe");
                return BadRequest(ModelState);
            }
            
            var genre = _mapper.Map<Genre>(createGenreDto);
            
            var created = await _genreRepository.CreateGenre(genre);
            if (!created)
            {
                ModelState.AddModelError("", $"Algo salió mal al guardar el género {genre.Name}");
                return StatusCode(500, ModelState);
            }

            var genreResponse = _mapper.Map<GenreDto>(genre);
            return CreatedAtRoute("GetGenre", new { id = genreResponse.Id }, genreResponse);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] CreateGenreDto updateGenreDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingGenre = await _genreRepository.GetGenreForAdmin(id);
            if (existingGenre == null)
            {
                return NotFound($"El género con el id {id} no existe");
            }

            // Verificar si el nombre ya existe (excluyendo el género actual)
            if (await _genreRepository.GenreExists(updateGenreDto.Name) && 
                existingGenre.Name.ToLower() != updateGenreDto.Name.ToLower())
            {
                ModelState.AddModelError("", "Ya existe un género activo con ese nombre");
                return BadRequest(ModelState);
            }

            existingGenre.Name = updateGenreDto.Name;
            
            var updated = await _genreRepository.UpdateGenre(existingGenre);
            if (!updated)
            {
                ModelState.AddModelError("", $"Algo salió mal al actualizar el género {existingGenre.Name}");
                return StatusCode(500, ModelState);
            }
            
            return NoContent();
        }

        // Endpoint unificado para activar/desactivar
        [HttpPatch("{id:int}/toggle-active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleGenreActive(int id)
        {
            var genre = await _genreRepository.GetGenreForAdmin(id);
            if (genre == null)
            {
                return NotFound($"El género con el id {id} no existe");
            }

            var oldStatus = genre.IsActive;
            var toggled = await _genreRepository.ToggleGenreActive(id);
            
            if (!toggled)
            {
                ModelState.AddModelError("", $"Algo salió mal al cambiar el estado del género {genre.Name}");
                return StatusCode(500, ModelState);
            }

            // Obtener el género actualizado para saber el nuevo estado
            var updatedGenre = await _genreRepository.GetGenreForAdmin(id);
            var newStatus = updatedGenre?.IsActive ?? false;

            return Ok(new { 
                message = $"Género {(newStatus ? "activado" : "desactivado")} correctamente",
                id = id,
                name = genre.Name,
                previousStatus = oldStatus,
                currentStatus = newStatus
            });
        }
    }
}

