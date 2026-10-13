using backend.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   public abstract class BaseController : ControllerBase
    {
        // Método para obtener errores del ModelState
        protected List<string> GetModelStateErrors()
        {
            return ModelState
                // 1. Filtrar solo las entradas que tienen errores
                .Where(ms => ms.Value?.Errors.Count > 0)

                // 2. Aplanar la estructura (de Dictionary a lista plana de errores)
                .SelectMany(ms => ms.Value!.Errors)
                
                // 3. Extraer solo el mensaje de error de cada ValidationError
                .Select(e => e.ErrorMessage)
                
                // 4. Convertir a List<string>
                .ToList();
        }

        // Métodos helper para respuestas estandarizadas
        protected IActionResult BadRequestResponse(string message, List<string>? errors = null)
        {
            var response = ApiResponse<object>.ErrorResponse(message, 400, errors);
            return BadRequest(response);
        }

        protected IActionResult NotFoundResponse(string message)
        {
            var response = ApiResponse<object>.ErrorResponse(message, 404);
            return NotFound(response);
        }

        protected IActionResult ForbiddenResponse(string message)
        {
            var response = ApiResponse<object>.ErrorResponse(message, 403);
            return StatusCode(403, response);
        }

        protected IActionResult SuccessResponse<T>(T data, string? message = null)
        {
            var response = ApiResponse<T>.SuccessResponse(data, message);
            return Ok(response);
        }

        protected IActionResult ModelStateErrorResponse()
        {
            var errors = GetModelStateErrors();
            return BadRequestResponse("Datos de entrada inválidos", errors);
        }

        protected IActionResult InternalServerErrorResponse(string message)
        {
            var response = ApiResponse<object>.ErrorResponse(message, 500);
            return StatusCode(500, response);
    }
    }
}
