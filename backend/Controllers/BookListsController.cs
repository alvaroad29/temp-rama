using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using backend.Repository.IRepository;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookListsController : ControllerBase
    {
        private readonly IBookListRepository _bookListRepository;
        private readonly IImageService _imageService;
        private readonly IGenreRepository _genreRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public BookListsController(
            IBookListRepository bookListRepository,
            IMapper mapper,
            UserManager<User> userManager,
            IGenreRepository genreRepository,
            IImageService imageService
        )
        {
            _bookListRepository = bookListRepository;
            _mapper = mapper;
            _userManager = userManager;
            _genreRepository = genreRepository;
            _imageService = imageService;
        }

        /// <summary>
        /// Obtiene todas las listas de libros con paginación, filtros y ordenamiento
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResponse<BookListDto>>> GetAllBookLists(
            [FromQuery] BookListQueryParameters parameters)
        {
            try
            {
                // Validación de parámetros
                if (parameters.PageNumber < 1) parameters.PageNumber = 1;
                if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 10;

                var includePrivate = User.IsInRole("Admin");
                var pagedResponse = await _bookListRepository.GetAllAsync(parameters, includePrivate);

                var bookListDtos = _mapper.Map<List<BookListDto>>(pagedResponse.Data);

                var response = new PagedResponse<BookListDto>(
                    bookListDtos,
                    pagedResponse.PageNumber,
                    pagedResponse.PageSize,
                    pagedResponse.TotalRecords);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al obtener las listas de libros",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene una lista de libros por ID, 
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BookListDto>> GetBookList(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                // Obtener la lista para verificar si el usuario es el dueño
                var bookList = await _bookListRepository.GetByIdAsync(id, true); // true para incluir privadas

                if (bookList == null)
                {
                    return NotFound(new { message = $"No se encontró la lista de libros con ID {id}" });
                }

                // Verificar si puede ver la lista privada
                var canViewPrivate = isAdmin || bookList.CreatorId == userId;

                if (!bookList.IsPublic && !canViewPrivate)
                {
                    return NotFound(new { message = $"No se encontró la lista de libros con ID {id}" });
                }

                var bookListDto = _mapper.Map<BookListDto>(bookList);
                return Ok(bookListDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al obtener la lista de libros", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una lista de libros por ID incluyendo los libros que conteine, 
        /// </summary>

        [HttpGet("{id}/detailed")]
        [AllowAnonymous]
        public async Task<ActionResult<BookListDetailDto>> GetBookListDetailed(
             int id,
             [FromQuery] BookListBooksQueryParameters parameters)
        {
            try
            {
                // Validación de parámetros
                if (parameters.PageNumber < 1) parameters.PageNumber = 1;
                if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 20;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var bookList = await _bookListRepository.GetByIdAsync(id, true);
                if (bookList == null)
                    return NotFound(new { message = $"No se encontró la lista de libros con ID {id}" });

                var canViewPrivate = isAdmin || bookList.CreatorId == userId;
                if (!bookList.IsPublic && !canViewPrivate)
                    return NotFound(new { message = $"No se encontró la lista de libros con ID {id}" });

                var pagedBooks = await _bookListRepository.GetBooksByListIdAsync(id, parameters);

                var bookListDto = _mapper.Map<BookListDetailDto>(bookList);
                bookListDto.Books = pagedBooks;

                if (!string.IsNullOrEmpty(userId))
                {
                    bookListDto.IsFollowing = await _bookListRepository.IsFollowingBookListAsync(id, userId);
                }

                return Ok(bookListDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }


        /// <summary>
        /// Obtiene las listas de libros de un usuario específico
        /// </summary>
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookListDto>>> GetBookListsByUser(string userId)
        {
            // sin paginacion por que solo puede haber un maximo de 20 listas
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { message = "El ID de usuario es requerido" });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var includePrivate = isAdmin || currentUserId == userId;

                var bookLists = await _bookListRepository.GetByUserIdAsync(userId, includePrivate);
                var bookListDtos = _mapper.Map<IEnumerable<BookListDto>>(bookLists);

                return Ok(bookListDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al obtener las listas de libros del usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las listas de un usuario indicando si contienen un libro específico
        /// </summary>
         [HttpGet("user/{userId}/book/{bookId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookListWithMembershipDto>>> GetUserBookListsWithBookMembership(string userId, int bookId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || bookId <= 0)
                {
                    return BadRequest(new { message = "El ID de usuario y libro son requeridos" });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                var includePrivate = isAdmin || currentUserId == userId;

                var bookLists = await _bookListRepository.GetBookListsWithMembershipAsync(userId, bookId, includePrivate);

                return Ok(bookLists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al obtener las listas del usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva lista de libros
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookListDto>> CreateBookList([FromBody] CreateBookListDto createDto)
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

                const int MAX_LISTS_PER_USER = 20; // limite

                var canCreateMore = await _bookListRepository.CanUserCreateMoreListsAsync(userId, MAX_LISTS_PER_USER);
                if (!canCreateMore)
                {
                    return BadRequest(new
                    {
                        message = $"Has alcanzado el límite máximo de {MAX_LISTS_PER_USER} listas por usuario"
                    });
                }

                List<Genre> selectedGenres = new List<Genre>();
                if (createDto.GenreIds.Any())
                {

                    selectedGenres = await _genreRepository.GetByIds(createDto.GenreIds);
                    if (selectedGenres.Count != createDto.GenreIds.Count)
                    {
                        return BadRequest(new { message = "Uno o más géneros especificados no existen" });
                    }
                }

                var bookList = _mapper.Map<BookList>(createDto);
                bookList.CreatorId = userId;
                bookList.CreationDate = DateTime.Now;
                bookList.UpdateDate = DateTime.Now;
                bookList.IsActive = true;
                bookList.Genres = selectedGenres;

                var createdBookList = await _bookListRepository.CreateAsync(bookList);
                var bookListDto = _mapper.Map<BookListDto>(createdBookList);

                return CreatedAtAction(nameof(GetBookList), new { id = bookListDto.Id }, bookListDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completo: {ex}");
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al crear la lista de libros",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Actualiza una lista de libros existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBookList(int id, [FromBody] UpdateBookListDto updateDto)
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

                var isAdmin = User.IsInRole("Admin");

                var existingBookList = await _bookListRepository.GetByIdAsync(id, true);
                if (existingBookList == null)
                {
                    return NotFound(new { message = $"No se encontró la lista de libros con ID {id}" });
                }

                if (!isAdmin && existingBookList.CreatorId != userId)
                    return Forbid();

                _mapper.Map(updateDto, existingBookList);
                existingBookList.UpdateDate = DateTime.Now;

                List<Genre> selectedGenres = new List<Genre>();
                if (updateDto.GenreIds != null)
                {
                    existingBookList.Genres.Clear();

                    selectedGenres = await _genreRepository.GetByIds(updateDto.GenreIds);
                    if (selectedGenres.Count != updateDto.GenreIds.Count)
                    {
                        return BadRequest(new { message = "Uno o más géneros especificados no existen" });
                    }
                }
                existingBookList.Genres = selectedGenres;

                await _bookListRepository.UpdateAsync(existingBookList);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al actualizar la lista de libros", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una lista de libros (borrado lógico)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookList(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                var existingBookList = await _bookListRepository.GetByIdAsync(id, true);
                if (existingBookList == null)
                {
                    return NotFound(new { message = $"No se encontró la lista de libros con ID {id}" });
                }

                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && existingBookList.CreatorId != userId)
                    return Forbid();


                // eliminar portada de lista
                if (!string.IsNullOrEmpty(existingBookList.ImageUrl))
                {
                    try
                    {
                        var publicId = await _imageService.ExtractPublicIdFromUrl(existingBookList.ImageUrl);
                        await _imageService.DeleteImageAsync(publicId);
                    }
                    catch (Exception ex)
                    {
                    }
                }


                var result = await _bookListRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"No se encontró la lista de libros con ID {id}" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al eliminar la lista de libros", error = ex.Message });
            }
        }

        /// <summary>
        /// Agrega un libro a una lista
        /// </summary>
        [HttpPost("{bookListId}/books/{bookId}")]
        public async Task<IActionResult> AddBookToList(int bookListId, int bookId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                var isAdmin = User.IsInRole("Admin");

                // solo owner o admin pueden agregar libros
                if (!isAdmin && !await _bookListRepository.IsOwnerAsync(bookListId, userId))
                    return Forbid();

                const int MAX_BOOKS = 100;
                var bookList = await _bookListRepository.GetByIdAsync(bookListId, true);
                if (bookList != null && bookList.BookCount >= MAX_BOOKS)
                {
                    return BadRequest(new
                    {
                        message = $"La lista ha alcanzado el límite máximo de {MAX_BOOKS} libros"
                    });
                }

                var result = await _bookListRepository.AddBookToListAsync(bookListId, bookId);
                if (!result)
                {
                    return BadRequest(new { message = "El libro ya existe en la lista" });
                }

                return Ok(new { message = "Libro agregado a la lista exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al agregar el libro a la lista", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un libro de una lista
        /// </summary>
        [HttpDelete("{bookListId}/books/{bookId}")]
        public async Task<IActionResult> RemoveBookFromList(int bookListId, int bookId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                var isAdmin = User.IsInRole("Admin");

                // Solo owner o admin pueden remover libros
                if (!isAdmin && !await _bookListRepository.IsOwnerAsync(bookListId, userId))
                    return Forbid();

                var result = await _bookListRepository.RemoveBookFromListAsync(bookListId, bookId);
                if (!result)
                {
                    return NotFound(new { message = "El libro no existe en la lista" });
                }

                return Ok(new { message = "Libro eliminado de la lista exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al eliminar el libro de la lista", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información de límites de listas para el usuario actual
        /// </summary>
        [HttpGet("limits")]
        public async Task<ActionResult> GetUserListLimits()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });

                const int MAX_LISTS_PER_USER = 20;
                var currentCount = await _bookListRepository.GetUserBookListCountAsync(userId);
                var canCreateMore = currentCount < MAX_LISTS_PER_USER;

                return Ok(new
                {
                    currentCount = currentCount,
                    maxLimit = MAX_LISTS_PER_USER,
                    remaining = MAX_LISTS_PER_USER - currentCount,
                    canCreateMore = canCreateMore
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener información de límites",
                    error = ex.Message
                });
            }
        }
        
        [HttpPost("cover")]
        public async Task<IActionResult> UploadListCover(IFormFile file, [FromQuery] int? listId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { Error = "No se envió ningún archivo" });

                if (file.Length > 2 * 1024 * 1024) // 2MB máximo
                    return BadRequest(new { Error = "La imagen no puede superar 2MB" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });

                string imageUrl;
                string message;

                // CASO 1: listId tiene valor → Actualizar portada de lista existente
                if (listId.HasValue && listId.Value > 0)
                {
                    var list = await _bookListRepository.GetByIdAsync(listId.Value);
                    
                    if (list == null) 
                        return NotFound(new { Error = "Lista no encontrada" });
                            
                    if (list.CreatorId != userId)
                        return Forbid();

                    // Borrar la imagen anterior de Cloudinary si existe
                    if (!string.IsNullOrEmpty(list.ImageUrl))
                    {
                        await DeleteImageFromCloudinary(list.ImageUrl);
                    }

                    imageUrl = await _imageService.UploadImageAsync(
                        file, 
                        "list-covers",
                        maxWidth: 600,
                        maxHeight: 400
                    );

                    // Actualizar la lista existente
                    list.ImageUrl = imageUrl;
                    list.UpdateDate = DateTime.Now;
                    await _bookListRepository.UpdateAsync(list);

                    message = "Portada actualizada correctamente";
                }
                // CASO 2: listId es null o 0 → Subir imagen para nueva lista
                else
                {
                    imageUrl = await _imageService.UploadImageAsync(
                        file, 
                        "list-covers",
                        maxWidth: 600,
                        maxHeight: 400
                    );

                    message = "Portada subida correctamente. Ahora puedes crear la lista.";
                }

                return Ok(new { 
                    Url = imageUrl,
                    Message = message,
                    ListId = listId // Devuelve el listId para referencia
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpDelete("{id}/cover")]
        public async Task<IActionResult> DeleteListCover(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });

                var existingList = await _bookListRepository.GetByIdAsync(id);
                if (existingList == null)
                    return NotFound(new { message = $"No se encontró la lista con ID {id}" });
                    
                if (existingList.CreatorId != userId)
                    return Forbid();

                // Eliminar de Cloudinary si existe imagen
                if (!string.IsNullOrEmpty(existingList.ImageUrl))
                {
                    try
                    {
                        var publicId = await _imageService.ExtractPublicIdFromUrl(existingList.ImageUrl);
                        await _imageService.DeleteImageAsync(publicId);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }

                // Actualizar la lista (poner ImageUrl como null)
                existingList.ImageUrl = null;
                existingList.UpdateDate = DateTime.Now;
                await _bookListRepository.UpdateAsync(existingList);

                return Ok(new { message = "Portada eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error interno del servidor al eliminar la portada", 
                    error = ex.Message 
                });
            }
        }

        private async Task DeleteImageFromCloudinary(string imageUrl)
        {
            try
            {
                var publicId = await _imageService.ExtractPublicIdFromUrl(imageUrl);
                await _imageService.DeleteImageAsync(publicId);
            }
            catch (Exception ex)
            {

            }
        }
        
        /// <summary>
        /// Seguir una lista de libros
        /// </summary>
        [HttpPost("follow")]
        public async Task<ActionResult> FollowBookList([FromBody] FollowBookListRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                var success = await _bookListRepository.FollowBookListAsync(request.BookListId, userId);
                
                if (!success)
                {
                    return BadRequest(new { message = "No se pudo seguir la lista. Puede que no exista, no sea pública o ya la estés siguiendo." });
                }

                return Ok(new { message = "Lista seguida correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al seguir la lista", error = ex.Message });
            }
        }

        /// <summary>
        /// Dejar de seguir una lista de libros
        /// </summary>
        [HttpPost("unfollow")]
        public async Task<ActionResult> UnfollowBookList([FromBody] FollowBookListRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                var success = await _bookListRepository.UnfollowBookListAsync(request.BookListId, userId);
                
                if (!success)
                {
                    return BadRequest(new { message = "No se pudo dejar de seguir la lista. Puede que no exista o no la estés siguiendo." });
                }

                return Ok(new { message = "Lista dejada de seguir correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al dejar de seguir la lista", error = ex.Message });
            }
        }

        /// <summary>
        /// Verificar si el usuario actual sigue una lista
        /// </summary>
        [HttpGet("{id}/is-following")]
        public async Task<ActionResult<bool>> IsFollowingBookList(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                var isFollowing = await _bookListRepository.IsFollowingBookListAsync(id, userId);
                
                return Ok(isFollowing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al verificar seguimiento", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener listas seguidas por el usuario actual
        /// </summary>
        [HttpGet("followed")]
        public async Task<ActionResult<PagedResponse<BookListDto>>> GetFollowedBookLists(
            [FromQuery] BookListQueryParameters parameters)
        {
            try
            {
                // Validación de parámetros
                if (parameters.PageNumber < 1) parameters.PageNumber = 1;
                if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 10;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var pagedResponse = await _bookListRepository.GetFollowedBookListsAsync(userId, parameters);

                var bookListDtos = _mapper.Map<List<BookListDto>>(pagedResponse.Data);

                var response = new PagedResponse<BookListDto>(
                    bookListDtos,
                    pagedResponse.PageNumber,
                    pagedResponse.PageSize,
                    pagedResponse.TotalRecords);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al obtener las listas seguidas",
                    error = ex.Message
                });
            }
        }
        

    }
}