namespace StudentManagement.Core.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "General";
        public int? TargetUserId { get; set; }
        public string? TargetRole { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? TargetUserName { get; set; }
    }

    public class CreateNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "General";
        public int? TargetUserId { get; set; }
        public string? TargetRole { get; set; }
    }

    public class UpdateNotificationRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public int? TargetUserId { get; set; }
        public string? TargetRole { get; set; }
        public bool? IsRead { get; set; }
    }
} 