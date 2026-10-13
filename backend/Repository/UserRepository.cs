using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.UserQueries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Repository;

public class UserRepository : IUserRepository
{
    ApplicationDbContext _db;
    private string? secretKey;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    private readonly ITokenService _tokenService;
    public UserRepository(
        ApplicationDbContext db,
        IConfiguration configuration,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper,
        ITokenService tokenService
    )
    {
        _db = db;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    public async Task<UserDataDto?> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDataDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            ImageUrl = user.ImageUrl,
            Roles = roles.ToList()
        };

    }

public async Task<PagedResponse<UserDataDto>> GetUsers(UserQueryParameters parameters)
{
    var query = _userManager.Users
        .AsNoTracking();

    // Aplicar filtros
    query = query
        .ApplySearchFilter(parameters.SearchText, parameters.SearchIn)
        .ApplyBannedFilter(parameters.IsBanned);

    // Aplicar ordenamiento
    query = query.ApplySorting(parameters.SortBy, parameters.SortDescending);

    // Obtener total antes de paginación
    var totalRecords = await query.CountAsync();

    // Aplicar paginación
    var users = await query
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .ToListAsync();

    // Convertir a UserDataDto
    var usersDto = new List<UserDataDto>();
    foreach (var user in users)
    {
        var roles = await _userManager.GetRolesAsync(user);
        usersDto.Add(new UserDataDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            ImageUrl = user.ImageUrl,
            Roles = roles.ToList(),
            IsBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
            BannedUntil = user.LockoutEnd,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled
        });
    }

    return new PagedResponse<UserDataDto>(usersDto, parameters.PageNumber, parameters.PageSize, totalRecords);
}

// Mantener el método antiguo para compatibilidad
public async Task<ICollection<UserDataDto>> GetUsers()
{
    var users = await _userManager.Users
        .OrderBy(u => u.UserName)
        .ToListAsync();

    var usersDto = new List<UserDataDto>();
    foreach (var user in users)
    {
        var roles = await _userManager.GetRolesAsync(user);
        usersDto.Add(new UserDataDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            ImageUrl = user.ImageUrl,
            Roles = roles.ToList(),
            IsBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
            BannedUntil = user.LockoutEnd
        });
    }

    return usersDto;
}

    public async Task<bool> IsUsernameUnique(string username)
    {
        return await _userManager.FindByNameAsync(username) == null;
    }
    public async Task<bool> IsEmailUnique(string email)
    {
        return await _userManager.FindByEmailAsync(email) == null;
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Email))
        {
            return null;
        }
        var user = await _userManager.FindByEmailAsync(userLoginDto.Email);

        if (user == null)
        {
            return null;
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new Exception("Tu cuenta ha sido suspendida");
        }

        if (userLoginDto.Password == null)
        {
            return null;
        }

        var result = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
        if (!result)
        {
            return null;
        }



        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);
        var userDto = _mapper.Map<UserDataDto>(user);
        userDto.Roles = roles.ToList();


        return new UserLoginResponseDto()
        {
            Token = token,
            User = userDto,
            Message = "Login exitoso"
        };
    }

    public async Task<UserDataDto> Register(CreateUserDto createUserDto)
    {

        if (string.IsNullOrEmpty(createUserDto.DisplayName)) // si no viene es igual al username
        {
            createUserDto.DisplayName = createUserDto.Username;
        }

        var user = new User()
        {
            UserName = createUserDto.Username,
            NormalizedUserName = createUserDto.Username.ToLower(),
            DisplayName = createUserDto.DisplayName,
            Email = createUserDto.Email,
            CreatedAt = DateTime.UtcNow 
        };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var userRole = "User";
        var roleExits = await _roleManager.RoleExistsAsync(userRole);

        if (!roleExits)
        {
            var identityRole = new IdentityRole(userRole);
            await _roleManager.CreateAsync(identityRole);
        }

        await _userManager.AddToRoleAsync(user, userRole);

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDataDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserLoginResponseDto> CheckStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var roles = await _userManager.GetRolesAsync(user!);
        var token = _tokenService.GenerateToken(user!, roles);

        var userDto = _mapper.Map<UserDataDto>(user);
        userDto.Roles = roles.ToList();
        return new UserLoginResponseDto()
        {
            Token = token,
            User = userDto,
            Message = "Status Ok"
        };
    }

    public async Task<UserLoginResponseDto?> UpdateUser(string userId, UpdateUserDto updateUserDto, bool isAdmin = false)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Usuario no encontrado");

            bool usernameChanged = false;
            var originalUsername = user.UserName;

            // Actualizar username si se proporcionó
            if (!string.IsNullOrEmpty(updateUserDto.Username) &&
                updateUserDto.Username != user.UserName)
            {
                user.UserName = updateUserDto.Username;
                usernameChanged = true;

                // Guardar cambios de username inmediatamente
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    throw new Exception("Error al actualizar el username");
            }

            // Actualizar password si se proporcionó
            if (!string.IsNullOrEmpty(updateUserDto.NewPassword))
            {
                var isSamePassword = await _userManager.CheckPasswordAsync(user, updateUserDto.NewPassword);
                if (isSamePassword)
                {
                    throw new Exception("La nueva contraseña debe ser diferente a la actual");
                }

                // Verificar contraseña actual para usuarios no admin
                if (!isAdmin && !string.IsNullOrEmpty(updateUserDto.CurrentPassword))
                {
                    var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, updateUserDto.CurrentPassword);
                    if (!isCurrentPasswordValid)
                    {
                        // Revertir cambios de username si falla
                        if (usernameChanged)
                        {
                            user.UserName = originalUsername;
                            await _userManager.UpdateAsync(user);
                        }
                        throw new Exception("La contraseña actual es incorrecta"); // ← Específico
                    }
                }

                // Cambiar la contraseña
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, updateUserDto.NewPassword);

                if (!result.Succeeded)
                {
                    // Revertir cambios de username si falla
                    if (usernameChanged)
                    {
                        user.UserName = originalUsername;
                        await _userManager.UpdateAsync(user);
                    }
                    throw new Exception("Error al cambiar la contraseña");
                }
            }

            // Obtener usuario actualizado
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserDataDto>(user);
            userDto.Roles = roles.ToList();

            string? tokenJwt = null;
            if (usernameChanged)
            {
                tokenJwt = _tokenService.GenerateToken(user, roles);
            }

            return new UserLoginResponseDto
            {
                Token = tokenJwt,
                User = userDto,
                Message = "Usuario actualizado"
            };
        }
        catch (Exception ex)
        {
            throw; // Relanzar con mensaje específico
        }
    }

    public async Task<bool> updateUserProfile(string userId, string imageUrl)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.ImageUrl = imageUrl;
            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {            
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> BanUser(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Agregar claim de baneo o usar LockoutEnabled
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue; // Baneo permanente
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UnbanUser(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            user.LockoutEnd = null; // Remover baneo
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteUser(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> AssignRole(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Verificar si el rol existe
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
                return false;

            // Verificar si ya tiene el rol
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains(roleName))
                return true; // Ya tiene el rol, no es un error

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> RemoveRole(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Verificar si tiene el rol
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains(roleName))
                return true; // No tiene el rol, no es un error

            // Prevenir remover el último rol
            if (userRoles.Count == 1)
                return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Repository/UserRepository.cs

// public async Task<AdminStatisticsDto> GetAdminStatistics()
// {
//     var now = DateTime.UtcNow;
//     var oneWeekAgo = now.AddDays(-7);
//     var oneMonthAgo = now.AddMonths(-1);
//     var sixMonthsAgo = now.AddMonths(-6);

//     // Estadísticas de usuarios
//     var totalUsers = await _userManager.Users.CountAsync();
//     var bannedUsers = await _userManager.Users
//         .CountAsync(u => u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow);
//     var newUsersThisWeek = await _userManager.Users
//         .CountAsync(u => u.CreatedAt >= oneWeekAgo);
//     var newUsersThisMonth = await _userManager.Users
//         .CountAsync(u => u.CreatedAt >= oneMonthAgo);

//     // Estadísticas de libros
//     var totalBooks = await _db.Books.CountAsync();
//     var activeBooks = await _db.Books.CountAsync(b => b.IsActive);
//     var inactiveBooks = await _db.Books.CountAsync(b => !b.IsActive);
//     var completedBooks = await _db.Books.CountAsync(b => b.State == State.Completado);
//     var pausedBooks = await _db.Books.CountAsync(b => b.State == State.Pausado);
//     var totalChapters = await _db.Chapters.CountAsync(c => c.IsActive);

//     // Estadísticas de reportes
//     var totalReports = await _db.Reports.CountAsync();
//     var pendingReports = await _db.Reports.CountAsync(r => r.Status == ReportStatus.Pendiente);
//     var inReviewReports = await _db.Reports.CountAsync(r => r.Status == ReportStatus.EnRevision);
//     var resolvedReports = await _db.Reports.CountAsync(r => r.Status == ReportStatus.Resuelto);

//     // Estadísticas de solicitudes
//     var totalSubmissions = await _db.BookSubmissions.CountAsync();
//     var pendingSubmissions = await _db.BookSubmissions.CountAsync(s => s.Status == BookSubmissionStatus.Pending);
//     var approvedSubmissions = await _db.BookSubmissions.CountAsync(s => s.Status == BookSubmissionStatus.Approved);
//     var rejectedSubmissions = await _db.BookSubmissions.CountAsync(s => s.Status == BookSubmissionStatus.Rejected);

//     // Tendencia de registros (últimos 6 meses)
//     var userRegistrationTrend = await _userManager.Users
//         .Where(u => u.CreatedAt >= sixMonthsAgo)
//         .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
//         .Select(g => new MonthlyRegistrationDto
//         {
//             Year = g.Key.Year,
//             Month = g.Key.Month,
//             Count = g.Count()
//         })
//         .OrderBy(x => x.Year)
//         .ThenBy(x => x.Month)
//         .ToListAsync();

//     // Actividad reciente (últimos 7 días)
//     var recentActivity = await _userManager.Users
//         .Where(u => u.CreatedAt >= oneWeekAgo)
//         .GroupBy(u => u.CreatedAt.Date)
//         .Select(g => new
//         {
//             Date = g.Key,
//             NewUsers = g.Count()
//         })
//         .Join(
//             _db.Books.Where(b => b.CreationDate >= oneWeekAgo)
//                 .GroupBy(b => b.CreationDate.Date)
//                 .Select(g => new { Date = g.Key, NewBooks = g.Count() }),
//             u => u.Date,
//             b => b.Date,
//             (u, b) => new { u.Date, u.NewUsers, b.NewBooks }
//         )
//         .Join(
//             _db.Reports.Where(r => r.CreationDate >= oneWeekAgo)
//                 .GroupBy(r => r.CreationDate.Date)
//                 .Select(g => new { Date = g.Key, NewReports = g.Count() }),
//             ub => ub.Date,
//             r => r.Date,
//             (ub, r) => new DailyActivityDto
//             {
//                 Date = ub.Date,
//                 NewUsers = ub.NewUsers,
//                 NewBooks = ub.NewBooks,
//                 NewReports = r.NewReports
//             }
//         )
//         .OrderBy(x => x.Date)
//         .ToListAsync();

//     return new AdminStatisticsDto
//     {
//         Users = new UserStats
//         {
//             TotalUsers = totalUsers,
//             ActiveUsers = totalUsers - bannedUsers,
//             BannedUsers = bannedUsers,
//             NewUsersThisWeek = newUsersThisWeek,
//             NewUsersThisMonth = newUsersThisMonth
//         },
//         Books = new BookStats
//         {
//             TotalBooks = totalBooks,
//             ActiveBooks = activeBooks,
//             InactiveBooks = inactiveBooks,
//             CompletedBooks = completedBooks,
//             PausedBooks = pausedBooks,
//             TotalChapters = totalChapters
//         },
//         Reports = new ReportStats
//         {
//             TotalReports = totalReports,
//             PendingReports = pendingReports,
//             InReviewReports = inReviewReports,
//             ResolvedReports = resolvedReports
//         },
//         Submissions = new SubmissionStats
//         {
//             TotalSubmissions = totalSubmissions,
//             PendingSubmissions = pendingSubmissions,
//             ApprovedSubmissions = approvedSubmissions,
//             RejectedSubmissions = rejectedSubmissions
//         },
//         UserRegistrationTrend = userRegistrationTrend,
//         RecentActivity = recentActivity
//     };
// }

public async Task<AdminStatisticsDto> GetAdminStatistics()
{
    var now = DateTime.UtcNow;
    var oneWeekAgo = now.AddDays(-7);
    var oneMonthAgo = now.AddMonths(-1);
    var sixMonthsAgo = now.AddMonths(-6);

    // === 1. ESTADÍSTICAS DE USUARIOS EN UNA SOLA CONSULTA ===
  var userStats = await _userManager.Users
    .GroupBy(u => 1)
    .Select(g => new
    {
        TotalUsers = g.Count(),
        BannedUsers = g.Count(u => u.LockoutEnabled && u.LockoutEnd > DateTimeOffset.UtcNow),
        NewUsersThisWeek = g.Count(u => u.CreatedAt >= oneWeekAgo),
        NewUsersThisMonth = g.Count(u => u.CreatedAt >= oneMonthAgo)
    })
    .FirstOrDefaultAsync() ?? new { TotalUsers = 0, BannedUsers = 0, NewUsersThisWeek = 0, NewUsersThisMonth = 0 };

    // === 2. ESTADÍSTICAS DE LIBROS Y CAPÍTULOS ===
    var bookStats = await _db.Books
        .GroupBy(b => 1)
        .Select(g => new
        {
            TotalBooks = g.Count(),
            ActiveBooks = g.Count(b => b.IsActive),
            InactiveBooks = g.Count(b => !b.IsActive),
            CompletedBooks = g.Count(b => b.State == State.Completado),
            PausedBooks = g.Count(b => b.State == State.Pausado)
        })
        .FirstOrDefaultAsync() ?? new { TotalBooks = 0, ActiveBooks = 0, InactiveBooks = 0, CompletedBooks = 0, PausedBooks = 0 };

    var totalChapters = await _db.Chapters
        .CountAsync(c => c.IsActive); // Este sí se mantiene separado (no está en Books)

    // === 3. ESTADÍSTICAS DE REPORTES ===
    var reportStats = await _db.Reports
        .GroupBy(r => 1)
        .Select(g => new
        {
            TotalReports = g.Count(),
            PendingReports = g.Count(r => r.Status == ReportStatus.Pendiente),
            InReviewReports = g.Count(r => r.Status == ReportStatus.EnRevision),
            ResolvedReports = g.Count(r => r.Status == ReportStatus.Resuelto)
        })
        .FirstOrDefaultAsync() ?? new { TotalReports = 0, PendingReports = 0, InReviewReports = 0, ResolvedReports = 0 };

    // === 4. ESTADÍSTICAS DE SOLICITUDES (BOOK SUBMISSIONS) ===
    var submissionStats = await _db.BookSubmissions
        .GroupBy(s => 1)
        .Select(g => new
        {
            TotalSubmissions = g.Count(),
            PendingSubmissions = g.Count(s => s.Status == BookSubmissionStatus.Pending),
            ApprovedSubmissions = g.Count(s => s.Status == BookSubmissionStatus.Approved),
            RejectedSubmissions = g.Count(s => s.Status == BookSubmissionStatus.Rejected)
        })
        .FirstOrDefaultAsync() ?? new { TotalSubmissions = 0, PendingSubmissions = 0, ApprovedSubmissions = 0, RejectedSubmissions = 0 };

    // === 5. TENDENCIA DE REGISTROS (últimos 6 meses) ===
    var userRegistrationTrend = await _userManager.Users
        .Where(u => u.CreatedAt >= sixMonthsAgo)
        .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
        .Select(g => new MonthlyRegistrationDto
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            Count = g.Count()
        })
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ToListAsync();

   // === 6. ACTIVIDAD RECIENTE (últimos 7 días) - CORREGIDO ===
var startDate = now.Date.AddDays(-6); // Últimos 7 días incluyendo hoy
var dateRange = Enumerable.Range(0, 7)
    .Select(i => startDate.AddDays(i))
    .ToList(); // Lista de DateTime (solo fecha, sin hora)

// Pre-cargar datos por fecha (DateTime.Date devuelve DateTime a las 00:00:00)
var userActivity = await _userManager.Users
    .Where(u => u.CreatedAt >= oneWeekAgo)
    .GroupBy(u => u.CreatedAt.Date) // Esto es DateTime
    .ToDictionaryAsync(g => g.Key, g => g.Count());

var bookActivity = await _db.Books
    .Where(b => b.CreationDate >= oneWeekAgo)
    .GroupBy(b => b.CreationDate.Date)
    .ToDictionaryAsync(g => g.Key, g => g.Count());

var reportActivity = await _db.Reports
    .Where(r => r.CreationDate >= oneWeekAgo)
    .GroupBy(r => r.CreationDate.Date)
    .ToDictionaryAsync(g => g.Key, g => g.Count());

// Construir resultado final
var recentActivity = dateRange.Select(date =>
{
    // Aseguramos que `date` es una fecha sin hora (00:00:00)
    var dateKey = date.Date; 
    return new DailyActivityDto
    {
        Date = dateKey, // Asignamos DateTime
        NewUsers = userActivity.GetValueOrDefault(dateKey, 0),
        NewBooks = bookActivity.GetValueOrDefault(dateKey, 0),
        NewReports = reportActivity.GetValueOrDefault(dateKey, 0)
    };
}).ToList();

    return new AdminStatisticsDto
    {
        Users = new UserStats
        {
            TotalUsers = userStats.TotalUsers,
            ActiveUsers = userStats.TotalUsers - userStats.BannedUsers,
            BannedUsers = userStats.BannedUsers,
            NewUsersThisWeek = userStats.NewUsersThisWeek,
            NewUsersThisMonth = userStats.NewUsersThisMonth
        },
        Books = new BookStats
        {
            TotalBooks = bookStats.TotalBooks,
            ActiveBooks = bookStats.ActiveBooks,
            InactiveBooks = bookStats.InactiveBooks,
            CompletedBooks = bookStats.CompletedBooks,
            PausedBooks = bookStats.PausedBooks,
            TotalChapters = totalChapters
        },
        Reports = new ReportStats
        {
            TotalReports = reportStats.TotalReports,
            PendingReports = reportStats.PendingReports,
            InReviewReports = reportStats.InReviewReports,
            ResolvedReports = reportStats.ResolvedReports
        },
        Submissions = new SubmissionStats
        {
            TotalSubmissions = submissionStats.TotalSubmissions,
            PendingSubmissions = submissionStats.PendingSubmissions,
            ApprovedSubmissions = submissionStats.ApprovedSubmissions,
            RejectedSubmissions = submissionStats.RejectedSubmissions
        },
        UserRegistrationTrend = userRegistrationTrend,
        RecentActivity = recentActivity
    };
}

// public async Task<AuthorStatisticsDto> GetAuthorStatistics(string authorId)
// {
//     var now = DateTime.UtcNow;
//     var oneWeekAgo = now.AddDays(-7);
//     var oneMonthAgo = now.AddMonths(-1);
//     var sixMonthsAgo = now.AddMonths(-6);

//     // Obtener libros del autor
//     var authorBooks = await _db.Books
//         .Where(b => b.AuthorId == authorId)
//         .Include(b => b.Ratings)
//         .Include(b => b.Volumes)
//             .ThenInclude(v => v.Chapters)
//         .ToListAsync();

//     var bookIds = authorBooks.Select(b => b.Id).ToList();

//     // Estadísticas básicas
//     var totalBooks = authorBooks.Count;
//     var activeBooks = authorBooks.Count(b => b.IsActive && b.State == State.Activo);
//     var completedBooks = authorBooks.Count(b => b.State == State.Completado);
//     var pausedBooks = authorBooks.Count(b => b.State == State.Pausado);

//     var totalChapters = authorBooks.Sum(b => b.Volumes.Sum(v => v.Chapters.Count(c => c.IsActive)));
//     var totalVolumes = authorBooks.Sum(b => b.Volumes.Count(v => v.IsActive));

//     // Vistas
//     var totalViews = authorBooks.Sum(b => b.ViewCount);
    
//     // Vistas de la última semana (aproximado usando ReadingHistory)
//     var viewsThisWeek = await _db.ReadingHistories
//         .Where(rh => rh.Chapter.Volume.BookId != null && bookIds.Contains(rh.Chapter.Volume.BookId) 
//             && rh.ViewDate >= oneWeekAgo)
//         .CountAsync();

//     var viewsThisMonth = await _db.ReadingHistories
//         .Where(rh => rh.Chapter.Volume.BookId != null && bookIds.Contains(rh.Chapter.Volume.BookId) 
//             && rh.ViewDate >= oneMonthAgo)
//         .CountAsync();

//     // Calificaciones
//     var allRatings = authorBooks.SelectMany(b => b.Ratings).ToList();
//     var totalRatings = allRatings.Count;
//     var averageRating = totalRatings > 0 
//         ? (decimal)allRatings.Average(r => r.RatingValue) 
//         : 0;

//     // Último capítulo publicado
//     var lastChapterPublished = authorBooks
//         .SelectMany(b => b.Volumes.SelectMany(v => v.Chapters))
//         .Where(c => c.IsActive)
//         .OrderByDescending(c => c.PublishDate)
//         .Select(c => c.PublishDate)
//         .FirstOrDefault();

//     // Última actualización de libro
//     var lastBookUpdate = authorBooks
//         .Where(b => b.UpdateDate.HasValue)
//         .OrderByDescending(b => b.UpdateDate)
//         .Select(b => b.UpdateDate)
//         .FirstOrDefault();

//     // Libro más popular (por vistas)
//     var mostPopularBook = authorBooks
//         .OrderByDescending(b => b.ViewCount)
//         .Select(b => new PopularBookDto
//         {
//             Id = b.Id,
//             Title = b.Title,
//             ImageUrl = b.ImageUrl,
//             ViewCount = b.ViewCount,
//             Score = b.Score,
//             TotalRatings = b.TotalRatings
//         })
//         .FirstOrDefault();

//     // Libro mejor calificado
//     var bestRatedBook = authorBooks
//         .Where(b => b.Score.HasValue && b.TotalRatings >= 5) // Al menos 5 calificaciones
//         .OrderByDescending(b => b.Score)
//         .ThenByDescending(b => b.TotalRatings)
//         .Select(b => new PopularBookDto
//         {
//             Id = b.Id,
//             Title = b.Title,
//             ImageUrl = b.ImageUrl,
//             ViewCount = b.ViewCount,
//             Score = b.Score,
//             TotalRatings = b.TotalRatings
//         })
//         .FirstOrDefault();

//     // Tendencia de vistas (últimos 6 meses) - usando ReadingHistory
//     var viewsTrend = await _db.ReadingHistories
//         .Where(rh => rh.Chapter.Volume.BookId != null && bookIds.Contains(rh.Chapter.Volume.BookId) 
//             && rh.ViewDate >= sixMonthsAgo)
//         .GroupBy(rh => new { rh.ViewDate.Year, rh.ViewDate.Month })
//         .Select(g => new MonthlyViewsDto
//         {
//             Year = g.Key.Year,
//             Month = g.Key.Month,
//             Views = g.Count()
//         })
//         .OrderBy(x => x.Year)
//         .ThenBy(x => x.Month)
//         .ToListAsync();

//     // Performance de cada libro
//     var booksPerformance = authorBooks
//         .OrderByDescending(b => b.ViewCount)
//         .Take(10) 
//         .Select(b => new BookPerformanceDto
//         {
//             BookId = b.Id,
//             Title = b.Title,
//             ViewCount = b.ViewCount,
//             Score = b.Score,
//             ChapterCount = b.Volumes.Sum(v => v.Chapters.Count(c => c.IsActive)),
//             State = b.State
//         })
//         .ToList();

//     return new AuthorStatisticsDto
//     {
//         TotalBooks = totalBooks,
//         ActiveBooks = activeBooks,
//         CompletedBooks = completedBooks,
//         PausedBooks = pausedBooks,
//         TotalChapters = totalChapters,
//         TotalVolumes = totalVolumes,
//         TotalViews = totalViews,
//         ViewsThisWeek = viewsThisWeek,
//         ViewsThisMonth = viewsThisMonth,
//         AverageRating = averageRating,
//         TotalRatings = totalRatings,
//         LastChapterPublished = lastChapterPublished == default ? null : lastChapterPublished,
//         LastBookUpdate = lastBookUpdate,
//         MostPopularBook = mostPopularBook,
//         BestRatedBook = bestRatedBook,
//         ViewsTrend = viewsTrend,
//         BooksPerformance = booksPerformance
//     };
// }

public async Task<AuthorStatisticsDto> GetAuthorStatistics(string authorId)
{
    var now = DateTime.UtcNow;
    var oneWeekAgo = now.AddDays(-7);
    var oneMonthAgo = now.AddMonths(-1);
    var sixMonthsAgo = now.AddMonths(-6);

    // === 1. Estadísticas principales en una sola consulta ===
    var stats = await _db.Books
        .Where(b => b.AuthorId == authorId)
        .GroupBy(b => 1) // Agrupación ficticia para hacer agregados
        .Select(g => new
        {
            TotalBooks = g.Count(),
            ActiveBooks = g.Count(b => b.IsActive && b.State == State.Activo),
            CompletedBooks = g.Count(b => b.State == State.Completado),
            PausedBooks = g.Count(b => b.State == State.Pausado),
            TotalViews = g.Sum(b => b.ViewCount),
            LastBookUpdate = g.Max(b => b.UpdateDate),
            LastChapterPublished = g
                .SelectMany(b => b.Volumes)
                .SelectMany(v => v.Chapters)
                .Where(c => c.IsActive)
                .Max(c => (DateTime?)c.PublishDate)
        })
        .FirstOrDefaultAsync();

    if (stats == null)
    {
        // Si no hay libros, retornar vacío
        return new AuthorStatisticsDto();
    }

    // === 2. Calificaciones promedio (en una consulta) ===
    var ratingStats = await _db.BookRatings
        .Where(r => r.Book.AuthorId == authorId)
        .GroupBy(r => 1)
        .Select(g => new
        {
            TotalRatings = g.Count(),
            AverageRating = g.Any() ? (decimal)g.Average(r => r.RatingValue) : 0
        })
        .FirstOrDefaultAsync();

    // === 3. Conteos de vistas desde ReadingHistory (optimizado con JOIN) ===
    var viewCounts = await _db.ReadingHistories
        .Where(rh => rh.Chapter.Volume.Book.AuthorId == authorId)
        .GroupBy(rh => 1)
        .Select(g => new
        {
            ViewsThisWeek = g.Count(rh => rh.ViewDate >= oneWeekAgo),
            ViewsThisMonth = g.Count(rh => rh.ViewDate >= oneMonthAgo)
        })
        .FirstOrDefaultAsync();

    // === 4. Tendencia de vistas (últimos 6 meses) ===
    var viewsTrend = await _db.ReadingHistories
        .Where(rh => rh.Chapter.Volume.Book.AuthorId == authorId && rh.ViewDate >= sixMonthsAgo)
        .GroupBy(rh => new { rh.ViewDate.Year, rh.ViewDate.Month })
        .Select(g => new MonthlyViewsDto
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            Views = g.Count()
        })
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ToListAsync();

    // === 5. Libro más popular (solo los campos necesarios) ===
    var mostPopularBook = await _db.Books
        .Where(b => b.AuthorId == authorId)
        .OrderByDescending(b => b.ViewCount)
        .Select(b => new PopularBookDto
        {
            Id = b.Id,
            Title = b.Title,
            ImageUrl = b.ImageUrl,
            ViewCount = b.ViewCount,
            Score = b.Score,
            TotalRatings = b.TotalRatings
        })
        .FirstOrDefaultAsync();

    // === 6. Libro mejor calificado (con mínimo de 5 ratings) ===
    var bestRatedBook = await _db.Books
        .Where(b => b.AuthorId == authorId && b.Score.HasValue && b.TotalRatings >= 5)
        .OrderByDescending(b => b.Score)
        .ThenByDescending(b => b.TotalRatings)
        .Select(b => new PopularBookDto
        {
            Id = b.Id,
            Title = b.Title,
            ImageUrl = b.ImageUrl,
            ViewCount = b.ViewCount,
            Score = b.Score,
            TotalRatings = b.TotalRatings
        })
        .FirstOrDefaultAsync();

    // === 7. Performance de libros (top 10) ===
    var booksPerformance = await _db.Books
        .Where(b => b.AuthorId == authorId)
        .OrderByDescending(b => b.ViewCount)
        .Take(10)
        .Select(b => new BookPerformanceDto
        {
            BookId = b.Id,
            Title = b.Title,
            ViewCount = b.ViewCount,
            Score = b.Score,
            ChapterCount = b.Volumes.Sum(v => v.Chapters.Count(c => c.IsActive)),
            State = b.State
        })
        .ToListAsync();

    // === Total de volúmenes y capítulos (agregado eficiente) ===
    var volumeChapterStats = await _db.Books
        .Where(b => b.AuthorId == authorId)
        .SelectMany(b => b.Volumes)
        .Where(v => v.IsActive)
        .GroupBy(v => 1)
        .Select(g => new
        {
            TotalVolumes = g.Count(),
            TotalChapters = g.Sum(v => v.Chapters.Count(c => c.IsActive))
        })
        .FirstOrDefaultAsync();

    return new AuthorStatisticsDto
    {
        TotalBooks = stats.TotalBooks,
        ActiveBooks = stats.ActiveBooks,
        CompletedBooks = stats.CompletedBooks,
        PausedBooks = stats.PausedBooks,
        TotalChapters = volumeChapterStats?.TotalChapters ?? 0,
        TotalVolumes = volumeChapterStats?.TotalVolumes ?? 0,
        TotalViews = stats.TotalViews,
        ViewsThisWeek = viewCounts?.ViewsThisWeek ?? 0,
        ViewsThisMonth = viewCounts?.ViewsThisMonth ?? 0,
        AverageRating = ratingStats?.AverageRating ?? 0,
        TotalRatings = ratingStats?.TotalRatings ?? 0,
        LastChapterPublished = stats.LastChapterPublished,
        LastBookUpdate = stats.LastBookUpdate,
        MostPopularBook = mostPopularBook,
        BestRatedBook = bestRatedBook,
        ViewsTrend = viewsTrend,
        BooksPerformance = booksPerformance
    };
}
}
