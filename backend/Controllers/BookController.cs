using System.Security.Claims;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using backend.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IGenreRepository _genreRepository;
        // private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;


        public BookController(IBookRepository bookRepository, IGenreRepository genreRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _genreRepository = genreRepository;
            // _authorRepository = authorRepository;
            _mapper = mapper;

        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBooks([FromQuery] BookQueryParameters parameters)
        {
            var isAdmin = User.IsInRole("Admin");
            // Validación adicional si es necesaria
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 10;

            var pagedResponse = await _bookRepository.GetBooks(parameters, isAdmin);
            var booksDto = _mapper.Map<List<BookDto>>(pagedResponse.Data);

            var response = new PagedResponse<BookDto>(
                booksDto,
                pagedResponse.PageNumber,
                pagedResponse.PageSize,
                pagedResponse.TotalRecords);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("recent-updates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecentBooksWithChapters()
        {
            try
            {
                var books = await _bookRepository.GetRecentBooksWithChaptersAsync(12);
                var booksDto = _mapper.Map<List<RecentBookWithChaptersDto>>(books);
                return Ok(booksDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetBook")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookRepository.GetBook(id);
            if (book == null)
            {
                return NotFound($"El libro con el id {id} no existe");
            }
            var bookDto = _mapper.Map<BookDto>(book);
            return Ok(bookDto);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}/detailed", Name = "GetBookDetailed")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookDetailed(int id)
        {
            var book = await _bookRepository.GetBookWithVolumesAndChapters(id);
            if (book == null)
            {
                return NotFound($"El libro con el id {id} no existe");
            }

            var bookDto = _mapper.Map<BookDetailsDto>(book);
            return Ok(bookDto);
        }


        [AllowAnonymous]
        [HttpGet("popular")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetPopularBooks(
            [FromQuery] string period = "daily")
        {
            var today = DateTime.UtcNow.Date;
            DateTime startDate;

            startDate = period.ToLower() switch
            {
                "daily" => today,
                "weekly" => today.AddDays(-7),
                "monthly" => today.AddDays(-30),
                _ => today // default: daily
            };


            var popularBooks = await _bookRepository.GetPopularBooksAsync(startDate, 10);

            var dtos = _mapper.Map<List<BookDto>>(popularBooks);
            return Ok(dtos);
        }

        [HttpGet("popular/all")]
        [AllowAnonymous]
        public async Task<ActionResult<PopularBooksResponseDto>> GetPopularBooksAll()
        {
            var daily = await _bookRepository.GetPopularBooksAsync(DateTime.UtcNow.AddDays(-1), 10);
            var weekly = await _bookRepository.GetPopularBooksAsync(DateTime.UtcNow.AddDays(-7), 10);
            var monthly = await _bookRepository.GetPopularBooksAsync(DateTime.UtcNow.AddDays(-30), 10);

            return Ok(new PopularBooksResponseDto
            {
                Daily = _mapper.Map<List<BookDto>>(daily),
                Weekly = _mapper.Map<List<BookDto>>(weekly),
                Monthly = _mapper.Map<List<BookDto>>(monthly)
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto createBookDto)
        {
            if (createBookDto == null)
            {
                return BadRequest(ModelState);
            }

            bool bookExists = await _bookRepository.BookExist(createBookDto.Title);
            if (bookExists)
            {
                ModelState.AddModelError("Custom Error", "Ya existe un libro con este título");
                return BadRequest(ModelState);
            }

            //Validacion genero
            var genres = new List<Genre>();
            foreach (var genreId in createBookDto.GenreIds)
            {
                var genre = await _genreRepository.GetGenre(genreId);
                if (genre == null)
                {
                    ModelState.AddModelError("Custom Error", $"El género del libro con id {genreId} no existe");
                    return BadRequest(ModelState);
                }
                genres.Add(genre);
            }

            // 1. Obtener el ID del autor desde el token
            var authorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var book = _mapper.Map<Book>(createBookDto);
            book.AuthorId = authorId;
            book.State = State.Activo;

            // Asignar los géneros 
            book.Genres = genres;

            var created = await _bookRepository.CreateBook(book);
            if (!created)
            {
                ModelState.AddModelError("Custom Error", "Algo salió mal al guardar el registro");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            var createdBookDto = await _bookRepository.GetBook(book.Id);
            var bookDto = _mapper.Map<BookDto>(createdBookDto);
            return CreatedAtRoute("GetBook", new { id = book.Id }, bookDto);
        }


        [HttpPost("{bookId}/rate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RateBook(int bookId, [FromBody] RatingRequestDto ratingRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var bookRating = await _bookRepository.RateBookAsync(bookId, userId, ratingRequest.Rating);
                var responseDto = _mapper.Map<BookRatingResponseDto>(bookRating);

                return Ok(responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{bookId}/user-rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserRating(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var rating = await _bookRepository.GetUserRatingAsync(bookId, userId);
            return Ok(new UserRatingDto { Rating = rating });
        }

        [HttpDelete("{bookId}/rating")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRating(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var success = await _bookRepository.DeleteRatingAsync(bookId, userId);
                if (!success)
                    return NotFound("No se encontró una calificación para eliminar");

                return Ok(new { message = "Calificación eliminada exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{bookId}/ratings")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookRatings(int bookId, [FromQuery] RatingQueryParameters parameters)
        {
            // Validar que el libro existe
            var bookExists = await _bookRepository.BookExistsAsync(bookId);
            if (!bookExists)
                return NotFound($"El libro con ID {bookId} no existe");

            var pagedRatings = await _bookRepository.GetBookRatingsAsync(bookId, parameters);
            var ratingsDto = _mapper.Map<ICollection<BookRatingDto>>(pagedRatings.Data);
            
            var response = new PagedResponse<BookRatingDto>(
                ratingsDto,
                pagedRatings.PageNumber,
                pagedRatings.PageSize,
                pagedRatings.TotalRecords
            );

            return Ok(response);
        }

        [HttpGet("user/ratings")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserRatings([FromQuery] RatingQueryParameters parameters)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var pagedRatings = await _bookRepository.GetUserRatingsAsync(userId, parameters);
            var ratingsDto = _mapper.Map<ICollection<UserBookRatingDto>>(pagedRatings.Data);

            var response = new PagedResponse<UserBookRatingDto>(
                ratingsDto,
                pagedRatings.PageNumber,
                pagedRatings.PageSize,
                pagedRatings.TotalRecords
            );

            return Ok(response);
        }
        
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookDto updateBookDto)
        {
            if (updateBookDto == null)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el libro existe
            var existingBook = await _bookRepository.GetBook(id);
            if (existingBook == null)
            {
                return NotFound($"El libro con el id {id} no existe");
            }

            // Solo el autor o un admin pueden editar el libro
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && existingBook.AuthorId != currentUserId)
            {
                return Forbid();
            }

            // Validar géneros
            var genres = new List<Genre>();
            foreach (var genreId in updateBookDto.GenreIds)
            {
                var genre = await _genreRepository.GetGenre(genreId);
                if (genre == null)
                {
                    ModelState.AddModelError("Custom Error", $"El género del libro con id {genreId} no existe");
                    return BadRequest(ModelState);
                }
                genres.Add(genre);
            }

            // Actualizar el libro
            var updated = await _bookRepository.UpdateBookAsync(id, updateBookDto, genres);
            if (!updated)
            {
                ModelState.AddModelError("Custom Error", "Algo salió mal al actualizar el libro");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            // Obtener el libro actualizado
            var updatedBook = await _bookRepository.GetBook(id);
            var bookDto = _mapper.Map<BookDto>(updatedBook);
            
            return Ok(bookDto);
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateBook(int id, [FromBody] DeactivateBookDto deactivateDto)
        {
            if (deactivateDto == null)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el libro existe
            var existingBook = await _bookRepository.GetBook(id);
            if (existingBook == null)
            {
                return NotFound($"El libro con el id {id} no existe");
            }

            // Desactivar/activar el libro
            var updated = await _bookRepository.SetBookActiveStatusAsync(id, deactivateDto.IsActive);
            if (!updated)
            {
                ModelState.AddModelError("Custom Error", "Algo salió mal al actualizar el estado del libro");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return Ok(new { 
                message = deactivateDto.IsActive ? "Libro activado correctamente" : "Libro desactivado correctamente",
                isActive = deactivateDto.IsActive 
            });
        }


        // [HttpPut("{bookId:int}", Name = "UpdateBook")]
        // [ProducesResponseType(StatusCodes.Status403Forbidden)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // public IActionResult UpdateBook(int bookId, [FromBody] UpdateBookDto updateBookDto)
        // {
        //     if (updateBookDto == null)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     if (!await _bookRepository.BookExist(bookId))
        //     {
        //         ModelState.AddModelError("Custom Error", $"El libro con el id {bookId} no existe");
        //         return NotFound(ModelState);
        //     }

        //     // if (_bookRepository.GetBooks().Any(b => b.Titulo.ToLower() == updateBookDto.Titulo.ToLower()))
        //     // {
        //     //     ModelState.AddModelError("Custom Error", "El libro ya existe");
        //     //     return BadRequest(ModelState);
        //     // }

        //     //Validacion genero
        //     if (!_genreRepository.GenreExists(updateBookDto.IdGenero))
        //     {
        //         ModelState.AddModelError("Custom Error", $"El género del libro con id {updateBookDto.IdGenero} no existe");
        //         return BadRequest(ModelState);
        //     }


        //     // Mapear el DTO a la entidad Book
        //     var book = _mapper.Map<Book>(updateBookDto);
        //     book.IdBook = bookId;
        //     if (!_bookRepository.UpdateBook(book))
        //     {
        //         ModelState.AddModelError("Custom Error", $"Algo salió mal al actualizar el registro {book.Titulo}");
        //         return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
        //     }

        //     return NoContent();
        // }

        // [HttpDelete("{bookId:int}", Name = "DeleteBook")]
        // [ProducesResponseType(StatusCodes.Status403Forbidden)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // public IActionResult DeleteBook(int bookId)
        // {
        //     if (bookId == 0)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     var book = _bookRepository.GetBook(bookId);
        //     if (book == null)
        //     {
        //         return NotFound($"El libro con el id {bookId} no existe");
        //     }
        //     if (!_bookRepository.DeleteBook(book))
        //     {
        //         ModelState.AddModelError("Custom Error", $"Algo salió mal al eliminar el libro {book.Titulo}");
        //         return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
        //     }

        //     return NoContent();
        // }

    }

}
