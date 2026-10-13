using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SeedController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [AllowAnonymous]
    [HttpGet(Name = "Seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Seed()
    {
        try
        {
           // ⭐ PRIMERO eliminar BookRatings (dependencia de Books)
            _context.BookRatings.RemoveRange(_context.BookRatings);
            await _context.SaveChangesAsync();

            // ⭐ LUEGO eliminar ReadingHistories (dependencia de Chapters)
            _context.ReadingHistories.RemoveRange(_context.ReadingHistories);
            await _context.SaveChangesAsync();

            // ⭐ LUEGO eliminar Chapters (dependencia de Volumes)
            _context.Chapters.RemoveRange(_context.Chapters);
            await _context.SaveChangesAsync();

            // ⭐ LUEGO eliminar Volumes (dependencia de Books)
            _context.Volumes.RemoveRange(_context.Volumes);
            await _context.SaveChangesAsync();

            // ⭐ FINALMENTE eliminar Books y Genres
            _context.Books.RemoveRange(_context.Books);
            _context.Genres.RemoveRange(_context.Genres);
            await _context.SaveChangesAsync();

            // Sembrar todo
            await SeedData.Initialize(_context, _userManager, _roleManager);

            return Ok("Base de datos poblada exitosamente con usuarios, roles, libros, capítulos y historial.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al sembrar datos: {ex.Message}");
        }
    }
}