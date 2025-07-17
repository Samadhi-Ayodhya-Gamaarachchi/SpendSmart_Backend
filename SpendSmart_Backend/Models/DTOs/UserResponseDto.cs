namespace SpendSmart_Backend.Models.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }

        // Computed properties for display
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string LastLoginDisplay => LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
        public string StatusBadgeColor => Status switch
        {
            "Active" => "success",
            "Inactive" => "warning",
            "Suspended" => "error",
            _ => "default"
        };
    }
}
