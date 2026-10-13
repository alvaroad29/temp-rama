using Microsoft.AspNetCore.Identity;
using backend.Models;
using System.Security.Claims;

namespace backend.Middleware
{
    public class LockoutMiddleware
    {
        private readonly RequestDelegate _next;

        public LockoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
        {
            // Solo validar si el usuario está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);
                    
                    if (user != null)
                    {
                        // Verificar si el usuario está bloqueado
                        var isLockedOut = await userManager.IsLockedOutAsync(user);
                        
                        if (isLockedOut)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new 
                            { 
                                message = "Tu cuenta ha sido suspendida. Contacta al administrador." 
                            });
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}