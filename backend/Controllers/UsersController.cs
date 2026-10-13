using System.Security.Claims;
using AutoMapper;
using backend.Models.Dtos;
using backend.Models.Dtos.UserQueries;
using backend.Repository;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper, IImageService imageService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _imageService = imageService;            
        }


        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] UserQueryParameters parameters)
        {
            // Validación de parámetros
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 10;

            var pagedResponse = await _userRepository.GetUsers(parameters);
            
            return Ok(pagedResponse);
        }

        [Authorize]
        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userRepository.GetUser(id);
            if (user == null)
            {
                return NotFound($"El usuario con el id {id} no existe");
            }
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost(Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            //Validaciones
            if (createUserDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _userRepository.IsUsernameUnique(createUserDto.Username))
            {
                ModelState.AddModelError(nameof(createUserDto.Username), "El nombre de usuario ya está en uso");
                return BadRequest(ModelState);
            }

            if (!await _userRepository.IsEmailUnique(createUserDto.Email))
            {
                ModelState.AddModelError(nameof(createUserDto.Email), "El email ya está registrado");
                return BadRequest(ModelState);
            }
            var result = await _userRepository.Register(createUserDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario");
            }
            return CreatedAtRoute("GetUser", new { id = result.Id }, result);

        }

        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {
            //Validaciones
            if (userLoginDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var result = await _userRepository.Login(userLoginDto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Capturar el error de cuenta suspendida
                if (ex.Message.Contains("suspendida") || ex.Message.Contains("baneada"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new { message = "Tu cuenta ha sido suspendida. Contacta al administrador." });
                }
                
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [Authorize]
        [HttpGet("check-status")]
        public async Task<IActionResult> CheckAuthStatus()
        {
            // Obtener usuario actual del contexto
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userResponse = await _userRepository.CheckStatus(userId!);

            if (userResponse == null)
                return Unauthorized();

            return Ok(userResponse);            
        }

        [Authorize]
        [HttpPut("{id}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                // Validaciones básicas
                if (updateUserDto == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar que al menos un campo viene para actualizar
                if (string.IsNullOrEmpty(updateUserDto.Username) && string.IsNullOrEmpty(updateUserDto.NewPassword))
                {
                    ModelState.AddModelError("", "Debe proporcionar al menos un campo para actualizar");
                    return BadRequest(ModelState);
                }

                // Obtener el usuario actual del token
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validar lógica de contraseñas
                var isAdmin = User.IsInRole("Admin");
                if (!string.IsNullOrEmpty(updateUserDto.NewPassword))
                {
                    // Si hay nueva password, debe haber confirmación
                    if (string.IsNullOrEmpty(updateUserDto.ConfirmPassword))
                    {
                        ModelState.AddModelError(nameof(updateUserDto.ConfirmPassword), "La confirmación de contraseña es requerida");
                        return BadRequest(ModelState);
                    }

                    // Para usuarios no admin, debe proporcionar la password actual
                    if (currentUserId == id && !isAdmin && string.IsNullOrEmpty(updateUserDto.CurrentPassword))
                    {
                        ModelState.AddModelError(nameof(updateUserDto.CurrentPassword), "La contraseña actual es requerida");
                        return BadRequest(ModelState);
                    }
                }

                // Verificar que el usuario existe
                var existingUser = await _userRepository.GetUser(id);
                if (existingUser == null)
                {
                    return NotFound($"El usuario con el id {id} no existe");
                }

                // Verificar permisos: solo el propio usuario o un admin pueden editar
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid("No tenes permisos para editar este usuario");
                }

                // Validar username único (solo si se está cambiando)
                if (!string.IsNullOrEmpty(updateUserDto.Username) &&
                    updateUserDto.Username != existingUser.UserName &&
                    !await _userRepository.IsUsernameUnique(updateUserDto.Username))
                {
                    ModelState.AddModelError(nameof(updateUserDto.Username), "El nombre de usuario ya está en uso");
                    return BadRequest(ModelState);
                }

                // Actualizar el usuario
                var result = await _userRepository.UpdateUser(id, updateUserDto, isAdmin);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejar errores específicos
                if (ex.Message.Contains("La contraseña actual es incorrecta"))
                {
                    return BadRequest(new { message = "La contraseña actual es incorrecta" });
                }
                else if (ex.Message.Contains("Error al cambiar la contraseña"))
                {
                    return BadRequest(new { message = "Error al cambiar la contraseña" });
                }
                else if (ex.Message.Contains("Usuario no encontrado"))
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                else if (ex.Message.Contains("nueva contraseña debe ser diferente"))
                {
                    return BadRequest(new { message = "La nueva contraseña debe ser diferente a la actual" });
                }

                // Loggear error inesperado

                return StatusCode(500, "Error interno del servidor");
            }

        }
        
        [HttpPost("profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file, [FromQuery] string? id = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { Error = "No se envió ningún archivo" });

                if (file.Length > 1 * 1024 * 1024) // 1 MB máximo
                    return BadRequest(new { Error = "La imagen no puede superar 1 MB" });

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });

                string imageUrl;
                string message;

                // Determinar qué usuario actualizar
                string targetUserId = id ?? currentUserId; // Si no viene id, usar el usuario actual
                
                // solo puede modificar su propio perfil (a menos que sea admin)
                var isAdmin = User.IsInRole("Admin");
                if (!isAdmin && targetUserId != currentUserId)
                    return Forbid();

                var user = await _userRepository.GetUser(targetUserId);
                if (user == null)
                    return NotFound(new { Error = "Usuario no encontrado" });

                // Borrar la imagen anterior de Cloudinary si existe
                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    await DeleteImageFromCloudinary(user.ImageUrl);
                }

                imageUrl = await _imageService.UploadImageAsync(
                    file,
                    "user-profiles",
                    maxWidth: 250,
                    maxHeight: 250
                );



                var updateResult = await _userRepository.updateUserProfile(targetUserId, imageUrl);
                if (!updateResult)
                {
                    // Si falla la actualización, borrar la imagen de Cloudinary
                    try
                    {
                        await _imageService.DeleteImageAsync(await _imageService.ExtractPublicIdFromUrl(imageUrl));
                    }
                    catch (Exception deleteEx)
                    {
                        
                    }
                    
                    return StatusCode(500, new { Error = "Error al actualizar el perfil del usuario" });
                }

                return Ok(new
                {
                    Url = imageUrl,
                    Message = "Foto de perfil actualizada correctamente",
                    UserId = targetUserId 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpDelete("{id}/profile")]
        public async Task<IActionResult> DeleteProfilePicture(string id)
        {
            try
            {
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });

                var existingUser = await _userRepository.GetUser(id);
                if (existingUser == null)
                    return NotFound(new { message = $"No se encontró el usuario con ID {id}" });

                if (userId != id)
                    return Forbid();

                // Eliminar de Cloudinary si existe imagen
                
                if (!string.IsNullOrEmpty(existingUser.ImageUrl))
                {
                    
                    try
                    {
                        await DeleteImageFromCloudinary(existingUser.ImageUrl);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                // Actualizar la lista (poner ImageUrl como null)
                existingUser.ImageUrl = null;
                var updateUser = _mapper.Map<UpdateUserDto>(existingUser);

                
                
                await _userRepository.updateUserProfile(id, updateUser.ImageUrl);

                return Ok(new { message = "Imagen de perfil eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
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

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/ban")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BanUser(string userId)
        {
            try
            {
                // Verificar que el usuario existe
                var user = await _userRepository.GetUser(userId);
                if (user == null)
                {
                    return NotFound($"El usuario con el id {userId} no existe");
                }

                // Prevenir que un admin se baneé a sí mismo
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == userId)
                {
                    return BadRequest("No puedes banearte a ti mismo");
                }

                var result = await _userRepository.BanUser(userId);
                if (!result)
                {
                    return BadRequest("Error al banear al usuario");
                }

                return Ok(new UserActionResponseDto 
                { 
                    Success = true, 
                    Message = "Usuario baneado correctamente" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/unban")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            try
            {
                // Verificar que el usuario existe
                var user = await _userRepository.GetUser(userId);
                if (user == null)
                {
                    return NotFound($"El usuario con el id {userId} no existe");
                }

                var result = await _userRepository.UnbanUser(userId);
                if (!result)
                {
                    return BadRequest("Error al desbanear al usuario");
                }

                return Ok(new UserActionResponseDto 
                { 
                    Success = true, 
                    Message = "Usuario desbaneado correctamente" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                // Verificar que el usuario existe
                var user = await _userRepository.GetUser(userId);
                if (user == null)
                {
                    return NotFound($"El usuario con el id {userId} no existe");
                }

                // Prevenir que un admin se elimine a sí mismo
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == userId)
                {
                    return BadRequest("No puedes eliminarte a ti mismo");
                }

                var result = await _userRepository.DeleteUser(userId);
                if (!result)
                {
                    return BadRequest("Error al eliminar al usuario");
                }

                return Ok(new UserActionResponseDto 
                { 
                    Success = true, 
                    Message = "Usuario eliminado correctamente" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                if (assignRoleDto == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validar rol
                var validRoles = new[] { "Admin", "Author", "User" };
                if (!validRoles.Contains(assignRoleDto.RoleName))
                {
                    return BadRequest($"Rol inválido. Roles válidos: {string.Join(", ", validRoles)}");
                }

                // Verificar que el usuario existe
                var user = await _userRepository.GetUser(userId);
                if (user == null)
                {
                    return NotFound($"El usuario con el id {userId} no existe");
                }

                var result = await _userRepository.AssignRole(userId, assignRoleDto.RoleName);
                if (!result)
                {
                    return BadRequest($"Error al asignar el rol {assignRoleDto.RoleName} al usuario");
                }

                return Ok(new UserActionResponseDto 
                { 
                    Success = true, 
                    Message = $"Rol {assignRoleDto.RoleName} asignado correctamente" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}/roles/{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            try
            {
                // Validar rol
                var validRoles = new[] { "Admin", "Author", "User" };
                if (!validRoles.Contains(roleName))
                {
                    return BadRequest($"Rol inválido. Roles válidos: {string.Join(", ", validRoles)}");
                }

                // Verificar que el usuario existe
                var user = await _userRepository.GetUser(userId);
                if (user == null)
                {
                    return NotFound($"El usuario con el id {userId} no existe");
                }

                // Prevenir remover el rol "User" si es el único que tiene
                var userRoles = user.Roles;
                if (userRoles.Count == 1 && userRoles.Contains(roleName))
                {
                    return BadRequest("No se puede remover el único rol del usuario");
                }

                var result = await _userRepository.RemoveRole(userId, roleName);
                if (!result)
                {
                    return BadRequest($"Error al remover el rol {roleName} del usuario");
                }

                return Ok(new UserActionResponseDto
                {
                    Success = true,
                    Message = $"Rol {roleName} removido correctamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
        

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminStatisticsDto>> GetAdminStatistics()
        {
            try
            {
                var statistics = await _userRepository.GetAdminStatistics();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpGet("author/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthorStatisticsDto>> GetAuthorStatistics()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var statistics = await _userRepository.GetAuthorStatistics(userId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        // para que Admin vea estadísticas de un autor específico
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/author/{authorId}/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthorStatisticsDto>> GetAuthorStatisticsByAdmin(string authorId)
        {
            try
            {
                var user = await _userRepository.GetUser(authorId);
                if (user == null)
                {
                    return NotFound(new { message = $"Autor con id {authorId} no encontrado" });
                }

                var statistics = await _userRepository.GetAuthorStatistics(authorId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

    }
}
