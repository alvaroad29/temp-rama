namespace backend.Models.Dtos;

public class AuthorStatisticsDto
{
    public int TotalBooks { get; set; }
    public int ActiveBooks { get; set; }
    public int CompletedBooks { get; set; }
    public int PausedBooks { get; set; }
    public int TotalChapters { get; set; }
    public int TotalVolumes { get; set; }
    
    // Métricas de engagement
    public int TotalViews { get; set; }
    public int ViewsThisWeek { get; set; }
    public int ViewsThisMonth { get; set; }
    
    // Calificaciones
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    
    // Actividad
    public DateTime? LastChapterPublished { get; set; }
    public DateTime? LastBookUpdate { get; set; }
    
    // Libro más popular
    public PopularBookDto? MostPopularBook { get; set; }
    public PopularBookDto? BestRatedBook { get; set; }
    
    // Tendencias
    public List<MonthlyViewsDto> ViewsTrend { get; set; } = new();
    public List<BookPerformanceDto> BooksPerformance { get; set; } = new();
}

public class PopularBookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int ViewCount { get; set; }
    public decimal? Score { get; set; }
    public int TotalRatings { get; set; }
}

public class MonthlyViewsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Views { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));
}

public class BookPerformanceDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public decimal? Score { get; set; }
    public int ChapterCount { get; set; }
    public State State { get; set; }
}