namespace backend.Models.Dtos;

public class AdminStatisticsDto
{
    public UserStats Users { get; set; } = new();
    public BookStats Books { get; set; } = new();
    public ReportStats Reports { get; set; } = new();
    public SubmissionStats Submissions { get; set; } = new();
    public List<MonthlyRegistrationDto> UserRegistrationTrend { get; set; } = new();
    public List<DailyActivityDto> RecentActivity { get; set; } = new();
}

public class UserStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int BannedUsers { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
}

public class BookStats
{
    public int TotalBooks { get; set; }
    public int ActiveBooks { get; set; }
    public int InactiveBooks { get; set; }
    public int CompletedBooks { get; set; }
    public int PausedBooks { get; set; }
    public int TotalChapters { get; set; }
}

public class ReportStats
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int InReviewReports { get; set; }
    public int ResolvedReports { get; set; }
}

public class SubmissionStats
{
    public int TotalSubmissions { get; set; }
    public int PendingSubmissions { get; set; }
    public int ApprovedSubmissions { get; set; }
    public int RejectedSubmissions { get; set; }
}

public class MonthlyRegistrationDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));
}

public class DailyActivityDto
{
    public DateTime Date { get; set; }
    public int NewUsers { get; set; }
    public int NewBooks { get; set; }
    public int NewReports { get; set; }
}