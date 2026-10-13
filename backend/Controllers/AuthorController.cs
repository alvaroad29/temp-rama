using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public AuthorController(IAuthorRepository authorRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAuthors()
        {
            var authors = _authorRepository.GetAuthors();
            var authorsDto = new List<AuthorDto>();
            foreach (var author in authors)
            {
                authorsDto.Add(_mapper.Map<AuthorDto>(author));
            }
            return Ok(authorsDto);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetAuthor")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAuthor(int id)
        {
            var author = _authorRepository.GetAuthor(id);

            if (author == null)
            {
                return NotFound($"El autor con el id {id} no existe ");
            }
            var authorDto = _mapper.Map<AuthorDto>(author);
            return Ok(authorDto);
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
        {
            if (createAuthorDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_authorRepository.AuthorExists(createAuthorDto.Name))
            {
                ModelState.AddModelError("", "El autor ya existe");
                return BadRequest(ModelState);
            }
            var author = _mapper.Map<Author>(createAuthorDto);
            if (!_authorRepository.CreateAuthor(author))
            {
                ModelState.AddModelError("Custom error", "Algo salió mal al crear el autor");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return CreatedAtRoute("GetAuthor", new { id = author.Id }, author);
        }

        [HttpPatch("{id:int}", Name = "UpdateAuthor")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateAuthor(int id, [FromBody] CreateAuthorDto updateAuthorDto)
        {
            
            if (!_authorRepository.AuthorExists(id))
            {
                return NotFound($"El autor con el id {id} no existe");
            }
            if (updateAuthorDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_authorRepository.AuthorExists(updateAuthorDto.Name) && _authorRepository.GetAuthor(id).Name != updateAuthorDto.Name)
            {
                ModelState.AddModelError("Custom Error", "El autor ya existe");
                return BadRequest(ModelState);
            }
            var author = _mapper.Map<Author>(updateAuthorDto);
            author.Id = id;
            if (!_authorRepository.UpdateAuthor(author))
            {
                ModelState.AddModelError("Custom Error", $"Algo salió mal al actualizar el autor {author.Name}");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return NoContent();
        }
        [HttpDelete("{id:int}", Name = "DeleteAuthor")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteAuthor(int id)
        {
            if (!_authorRepository.AuthorExists(id))
            {
                return NotFound($"El autor con el id {id} no existe");
            }
            var author = _authorRepository.GetAuthor(id);
            if (author == null)
            {
                return NotFound($"El autor con el id {id} no existe");
            }

            if (!_authorRepository.DeleteAuthor(author))
            {
                ModelState.AddModelError("Custom Error", $"Algo salió mal al borrar el autor {author.Name}");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return NoContent();
        }

    }
}
