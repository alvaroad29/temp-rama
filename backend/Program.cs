using System.Text;
using backend.Constants;
using backend.Middleware;
using backend.Models;
using backend.Repositories;
using backend.Repository;
using backend.Repository.IRepository;
using backend.Services;
using backend.Services.Interfaces;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dbConnectionString));

builder.Services.AddSingleton<Cloudinary>(sp =>
{
  var config = sp.GetRequiredService<IConfiguration>();
  var account = new Account(
      config["Cloudinary:CloudName"],
      config["Cloudinary:ApiKey"],
      config["Cloudinary:ApiSecret"]
  );
  return new Cloudinary(account);
});

builder.Services.AddScoped<IImageService, CloudinaryImageService>();

// Configurar límites de tamaño para uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5 Mb
});



builder.Services.AddScoped<IGenreRepository, GenreRepository>();
// builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVolumeRepository, VolumeRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IReadingHistoryRepository, ReadingHistoryRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IBookListRepository, BookListRepository>();
builder.Services.AddScoped<IBookSubmissionRepository, BookSubmissionRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

builder.Services.AddIdentity<User, IdentityRole>(options => 
{
    // Configuración de requisitos de contraseña
    options.Password.RequireDigit = true;          // Requiere números
    options.Password.RequireLowercase = true;      // Requiere minúsculas
    options.Password.RequireUppercase = true;      // Requiere mayúsculas
    options.Password.RequireNonAlphanumeric = false; // Requiere caracteres especiales
    options.Password.RequiredLength = 6;           // Longitud mínima
    options.Password.RequiredUniqueChars = 3;      // Caracteres únicos requeridos
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if(string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("La clave secreta no está configurada en ApiSettings:SecretKey");
}
builder.Services.AddAuthentication(options =>
{ 
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
  options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                    "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                    "Ejemplo: \"12345abcdef\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });
  }    
);
const string FrontendOrigin = "https://booklist-frontend-snfs.onrender.com";

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCors",
        policy =>
        {
            policy.WithOrigins(
                    FrontendOrigin,
                    "http://booklist-frontend-snfs.onrender.com",  // HTTP también
                    "https://two024-grupo5.onrender.com",          // Backend mismo
                    "http://localhost:4200",                       // Desarrollo
                    "http://localhost:8080"                        // Docker local
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()  // ⭐⭐ CRÍTICO para JWT/Cookies
                .WithExposedHeaders("Authorization")  // ⭐ Para leer el header
                .SetPreflightMaxAge(TimeSpan.FromHours(1));
        });
});

var app = builder.Build();

if (Environment.GetEnvironmentVariable("DOCKER_ENV") == "true")
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        if (Environment.GetEnvironmentVariable("SEED_DATA") == "true")
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await SeedData.Initialize(context, userManager, roleManager);
            logger.LogInformation("Seed de datos completado");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al migrar o cargar datos iniciales");
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ProductionCors");

app.UseAuthentication();

app.UseMiddleware<LockoutMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
