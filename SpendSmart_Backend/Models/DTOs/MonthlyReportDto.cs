namespace SpendSmart_Backend.Models.DTOs
{
    public class MonthlyReportDto
    {
        public DateTime ReportMonth { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int Year { get; set; }
        public UserStatsDto UserStats { get; set; } = new();
        public ActivityStatsDto ActivityStats { get; set; } = new();
        public GrowthStatsDto GrowthStats { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int NewRegistrations { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public double UserGrowthPercentage { get; set; }
    }

    public class ActivityStatsDto
    {
        public int TotalLogins { get; set; }
        public int AverageLoginsPerDay { get; set; }
        public DateTime PeakActivityDate { get; set; }
        public int PeakActivityLogins { get; set; }
        public double ActivityGrowthPercentage { get; set; }
    }

    public class GrowthStatsDto
    {
        public double UserGrowthVsPrevious { get; set; }
        public double ActivityGrowthVsPrevious { get; set; }
        public string TrendDescription { get; set; } = string.Empty;
    }

    public class ReportStatusDto
    {
        public bool HasReport { get; set; }
        public DateTime? LastGenerated { get; set; }
        public string Status { get; set; } = "Ready"; // Ready, Generating, Error
        public string? ErrorMessage { get; set; }
    }
}