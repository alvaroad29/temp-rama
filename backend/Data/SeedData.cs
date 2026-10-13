// SeedData/SeedData.cs
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class SeedData
{
    /// <summary>
    /// Crea los roles si no existen.
    /// </summary>
    public static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
    {
        var roleNames = new[] { "Admin", "Author", "User" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    throw new InvalidOperationException($"Error creando rol {roleName}: {string.Join(", ", result.Errors)}");
            }
        }
    }

    /// <summary>
    /// Crea un usuario si no existe, y le asigna un rol.
    /// </summary>
    private static async Task<User> CreateOrFindUser(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        string email,
        string displayName,
        string password,
        string roleName)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null) return existingUser;

        var user = new User
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Error creando usuario {email}: {string.Join(", ", result.Errors)}");

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Error asignando rol {roleName} a {email}: {string.Join(", ", roleResult.Errors)}");
        }

        return user;
    }

    /// <summary>
    /// Puebla la base de datos con géneros, libros, volúmenes, capítulos y usuarios.
    /// </summary>
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Evitar re-seed si ya hay datos
        if (context.Books.Any()) return;

        var random = Random.Shared;

        // ===== CREAR ROLES =====
        await CreateRoles(roleManager);

        // ===== CREAR USUARIOS =====
        var adminUser = await CreateOrFindUser(userManager, roleManager, "admin@example.com", "Admin", "Admin123!", "Admin");
        var authorUser = await CreateOrFindUser(userManager, roleManager, "autor1@example.com", "Autor Principal", "Autor123!", "Author");
        var normalUser = await CreateOrFindUser(userManager, roleManager, "user1@example.com", "Usuario Normal", "User123!", "User");

        var users = new[] { adminUser, authorUser, normalUser };

        // ===== GÉNEROS =====
        var genres = new List<Genre>
        {
            new Genre { Name = "Fantasía" },
            new Genre { Name = "Ciencia Ficción" },
            new Genre { Name = "Romance" },
            new Genre { Name = "Aventura" },
            new Genre { Name = "Misterio" },
            new Genre { Name = "Horror" },
            new Genre { Name = "Drama" },
            new Genre { Name = "Comedia" },
            new Genre { Name = "Acción" },
            new Genre { Name = "Slice of Life" }
        };

        if (!context.Genres.Any())
        {
            context.Genres.AddRange(genres);
            await context.SaveChangesAsync();
        }

        // Refrescar géneros desde BD (para asegurar IDs)
        genres = await context.Genres.ToListAsync();

        // ===== LIBROS =====
        var titles = new[]
        {
            "El Viaje Eterno", "Sombras del Pasado", "Luz de Esperanza", "Crónicas de Acero", "Marea de Fuego",
            "Reflejos de la Luna", "Código Génesis", "Pasiones Prohibidas", "Guardianes del Viento", "El Fin del Tiempo"
        };

        var descriptions = new[]
        {
            "Una historia llena de magia y misterio.",
            "Un viaje a través del tiempo y el espacio.",
            "Romance que desafía los límites de la razón.",
            "Un mundo distópico donde nada es lo que parece.",
            "Una comedia ligera con un toque de drama.",
            "El destino de una civilización está en juego.",
            "Descubre los secretos ocultos bajo la superficie.",
            "Una historia de redención y segundas oportunidades.",
            "Un thriller que mantendrá tu corazón al límite.",
            "Aventuras épicas en un mundo fantástico."
        };

        var imageUrls = new[]
        {
            "https://picsum.photos/seed/book1/400/600",
            "https://picsum.photos/seed/book2/400/600",
            "https://picsum.photos/seed/book3/400/600"
        };

        var allStates = Enum.GetValues<State>().Cast<State>().ToArray();

        var books = new List<Book>();
        for (int i = 0; i < 20; i++)
        {
            var randomGenres = genres.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();

            // Generar score entre 3.0 y 5.0 (más realista para libros)
            decimal score = Math.Round((decimal)(random.NextDouble() * 2 + 3), 1);
            
            // Algunos libros con scores más bajos (1.0-3.0) para variedad
            if (random.Next(100) < 20) // 20% de probabilidad
            {
                score = Math.Round((decimal)(random.NextDouble() * 2 + 1), 1);
            }

            var book = new Book
            {
                Title = titles[random.Next(titles.Length)],
                Description = descriptions[random.Next(descriptions.Length)],
                CreationDate = DateTime.Now.AddMonths(-random.Next(1, 13)),
                UpdateDate = DateTime.Now.AddMonths(-random.Next(1, 30)),
                Score = score,
                TotalRatings = random.Next(0, 500),
                ChapterCount = random.Next(1, 31),
                ViewCount = random.Next(0, 1000) switch
                {
                    < 900 => random.Next(0, 100),
                    < 990 => random.Next(100, 5000),
                    _ => random.Next(5000, 10000)
                },
                IsActive = random.NextDouble() > 0.1,
                State = allStates[random.Next(allStates.Length)],
                ImageUrl = imageUrls[random.Next(imageUrls.Length)],
                AuthorId = authorUser.Id,
                Genres = randomGenres
            };
            books.Add(book);
        }

        context.Books.AddRange(books);
        await context.SaveChangesAsync();

       var readingHistories = new List<ReadingHistory>();

// ===== VOLÚMENES Y CAPÍTULOS (solo se añaden al contexto, no se guardan aún) =====
var volumeTitles = new[]
{
    "Volumen I", "Volumen II", "Volumen III", "Edición Especial", "Colección Completa",
    "Primera Parte", "Segunda Parte", "Arco Final", "Saga Inicial", "Compilación Definitiva"
};

var chapterTitles = new[]
{
    "El Principio", "Encuentro Inesperado", "La Decisión", "Punto de No Retorno", "Revelaciones",
    "Batalla Final", "Epílogo", "Nuevos Comienzos", "Secretos Ocultos", "El Camino Elegido",
    "Cicatrices del Pasado", "Destino Cruzado", "Luz en la Oscuridad", "El Precio de la Victoria"
};

var chapterContents = new[]
{
    "<p>Este capítulo introduce a los personajes principales...</p>",
    "<p>Un giro inesperado cambia el curso de la historia...</p>",
    "<p>Los protagonistas enfrentan su mayor desafío...</p>",
    "<p>El clímax de la historia lleva a un enfrentamiento épico...</p>",
    "<p>Los eventos posteriores al conflicto principal se desarrollan...</p>"
};

foreach (var book in books)
{
    var volumeCount = random.Next(1, 5);
    var currentChapterOrder = 1;

    for (int v = 0; v < volumeCount; v++)
    {
        var volume = new Volume
        {
            Title = $"{volumeTitles[random.Next(volumeTitles.Length)]} - {book.Title}",
            Order = v + 1,
            IsActive = random.NextDouble() > 0.1,
            BookId = book.Id
        };

        context.Volumes.Add(volume);
    }
} 


// PRIMERO guardar los volúmenes para obtener sus IDs reales
await context.SaveChangesAsync();

// AHORA crear los capítulos con los VolumeId correctos
var allVolumes = await context.Volumes.ToListAsync();
var volumesByBookId = allVolumes.GroupBy(v => v.BookId).ToDictionary(g => g.Key, g => g.ToList());

foreach (var book in books)
{
    if (volumesByBookId.TryGetValue(book.Id, out var bookVolumes))
    {
        var currentChapterOrder = 1;
        
        foreach (var volume in bookVolumes)
        {
            var chapterCount = random.Next(3, 8);
            var chapters = new List<Chapter>();

            for (int c = 0; c < chapterCount; c++)
            {
                var chapter = new Chapter
                {
                    Title = $"{chapterTitles[random.Next(chapterTitles.Length)]} - {volume.Title}",
                    Order = currentChapterOrder++,
                    Content = chapterContents[random.Next(chapterContents.Length)],
                    ViewCount = random.Next(0, 1000),
                    PublishDate = book.CreationDate.AddDays(random.Next(0, 30)),
                    UpdateDate = random.NextDouble() > 0.7 ? book.UpdateDate?.AddDays(random.Next(0, 10)) : null,
                    IsActive = random.NextDouble() > 0.1,
                    VolumeId = volume.Id // ✅ Ahora el VolumeId es válido
                };
                chapters.Add(chapter);
            }

            context.Chapters.AddRange(chapters);
        }
    }
}

// Guardar los capítulos
await context.SaveChangesAsync();

// === AHORA PODEMOS CREAR HISTORIAL DE LECTURA CON ChapterId VÁLIDOS ===
var allChapters = await context.Chapters.ToListAsync(); // Todos los capítulos con Id real

foreach (var chapter in allChapters)
{
    // 10% de probabilidad de que el usuario normal haya leído este capítulo
    if (random.NextDouble() < 0.1)
    {
        readingHistories.Add(new ReadingHistory
        {
            ChapterId = chapter.Id, // ✅ Ahora el Id es real
            UserId = normalUser.Id,
            ViewDate = DateTime.UtcNow.AddDays(-random.Next(0, 30))
        });
    }
}

// === GUARDAR EL HISTORIAL ===
if (readingHistories.Any())
{
    context.ReadingHistories.AddRange(readingHistories);
    await context.SaveChangesAsync();
}

      
        

        
    }
}