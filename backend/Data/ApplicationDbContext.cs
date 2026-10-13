using backend;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // para identity

     // === ÍNDICES para Book ===
         modelBuilder.Entity<Book>()
        .HasIndex(b => new { b.IsActive, b.State, b.CreationDate })
        .HasDatabaseName("IX_Book_IsActive_State_CreationDate")
        .IsDescending(false, false, true); // Solo CreationDate DESC

        modelBuilder.Entity<Book>()
        .HasIndex(b => new { b.IsActive, b.LastChapterPublishDate })
        .HasDatabaseName("IX_Book_IsActive_LastChapterPublishDate")
        .IsDescending(false, true);

         modelBuilder.Entity<Book>()
        .HasIndex(b => new { b.IsActive, b.Score })
        .HasDatabaseName("IX_Book_IsActive_Score")
        .IsDescending(false, true);

        modelBuilder.Entity<Book>()
            .HasIndex(b => new { b.IsActive, b.ViewCount })
            .HasDatabaseName("IX_Book_IsActive_ViewCount")
            .IsDescending(false, true);

        modelBuilder.Entity<Book>()
            .HasIndex(b => new { b.IsActive, b.AuthorId, b.CreationDate })
            .HasDatabaseName("IX_Book_IsActive_AuthorId_CreationDate")
            .IsDescending(false, false, true);

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.Title)
            .HasDatabaseName("IX_Book_Title");

        // === ÍNDICES para BookList ===
     modelBuilder.Entity<BookList>()
        .HasIndex(bl => new { bl.IsActive, bl.IsPublic, bl.CreationDate })
        .HasDatabaseName("IX_BookList_IsActive_IsPublic_CreationDate")
        .IsDescending(false, false, true);

    modelBuilder.Entity<BookList>()
        .HasIndex(bl => new { bl.IsActive, bl.IsPublic, bl.CreatorId })
        .HasDatabaseName("IX_BookList_IsActive_IsPublic_CreatorId");

        // cambio nombre de las tablas
        modelBuilder.Entity<User>().ToTable("Users"); 
        modelBuilder.Entity<IdentityRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

        modelBuilder.Entity<ReadingHistory>()
            .HasOne(c => c.Chapter)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReadingHistory>()
            .HasOne(c => c.User)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        // CONFIGURACIÓN DE BOOKRATING
        
modelBuilder.Entity<BookRating>(entity =>
{
    // Índice único: un usuario no puede calificar dos veces el mismo libro
    entity.HasIndex(br => new { br.BookId, br.UserId })
          .IsUnique();

    // Índice para búsquedas rápidas por libro
    entity.HasIndex(br => br.BookId);

    // Índice para búsquedas por usuario
    entity.HasIndex(br => br.UserId);

    // RELACIONES CON DELETE BEHAVIOR SEGURO
    entity.HasOne(br => br.Book)
          .WithMany(b => b.Ratings)
          .HasForeignKey(br => br.BookId)
          .OnDelete(DeleteBehavior.ClientCascade);

    entity.HasOne(br => br.User)
          .WithMany(u => u.BookRatings)
          .HasForeignKey(br => br.UserId)
          .OnDelete(DeleteBehavior.ClientCascade); 
});

// 👇 ESTO VA FUERA DEL BLOQUE DE BookRating 👇

// Configurar clave compuesta para BookListBook
modelBuilder.Entity<BookListBook>()
    .HasKey(blb => new { blb.BookListId, blb.BookId });

modelBuilder.Entity<BookListBook>()
    .HasOne(blb => blb.BookList)
    .WithMany(bl => bl.BookListBooks)
    .HasForeignKey(blb => blb.BookListId)
    .OnDelete(DeleteBehavior.ClientCascade);

modelBuilder.Entity<BookListBook>()
    .HasOne(blb => blb.Book)
    .WithMany() // ← Si Book no tiene una colección de BookListBook, está bien
    .HasForeignKey(blb => blb.BookId)
    .OnDelete(DeleteBehavior.ClientCascade);

// Configurar clave compuesta para BookListFollower
modelBuilder.Entity<BookListFollower>()
    .HasKey(blf => new { blf.BookListId, blf.UserId });

modelBuilder.Entity<BookListFollower>()
    .HasOne(blf => blf.BookList)
    .WithMany(bl => bl.Followers)
    .HasForeignKey(blf => blf.BookListId)
    .OnDelete(DeleteBehavior.ClientCascade);

modelBuilder.Entity<BookListFollower>()
    .HasOne(blf => blf.User)
    .WithMany(u => u.FollowedBookLists)
    .HasForeignKey(blf => blf.UserId)
    .OnDelete(DeleteBehavior.ClientCascade);

        // Opcional: índice en Book.Score si haces queries por puntuación
        // modelBuilder.Entity<Book>()
        //     .HasIndex(b => b.Score);

        modelBuilder.Entity<Report>(entity =>
    {
        // Relación con Book - NO ACTION para evitar ciclos
        entity.HasOne(r => r.Book)
              .WithMany() // Report no está incluido en la navegación de Book
              .HasForeignKey(r => r.BookId)
              .OnDelete(DeleteBehavior.NoAction);

        // Relación con Reporter (User) - NO ACTION
        entity.HasOne(r => r.Reporter)
              .WithMany() // Report no está incluido en la navegación de User
              .HasForeignKey(r => r.ReporterId)
              .OnDelete(DeleteBehavior.NoAction);

        // Relación con AdminAssignee (User) - NO ACTION
        entity.HasOne(r => r.ReviewedByAdmin)
              .WithMany() // Report no está incluido en la navegación de User
              .HasForeignKey(r => r.ReviewedByAdminId)
              .OnDelete(DeleteBehavior.NoAction);

        // Índices para mejor performance
        entity.HasIndex(r => r.BookId);
        entity.HasIndex(r => r.ReporterId);
        entity.HasIndex(r => r.Status);
        entity.HasIndex(r => r.CreationDate);
    });
    }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Volume> Volumes { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<ReadingHistory> ReadingHistories { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<BookRating> BookRatings { get; set; }
    public DbSet<BookList> BookLists { get; set; }
    public DbSet<BookListBook> BookListBooks { get; set; }
    public DbSet<BookListFollower> BookListFollowers { get; set; }
    public DbSet<BookSubmission> BookSubmissions { get; set; }
    public DbSet<Report> Reports { get; set; }
}