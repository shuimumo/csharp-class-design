using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Type { get; set; } = "General";
        
        public int? TargetUserId { get; set; }
        
        [StringLength(20)]
        public string? TargetRole { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User? TargetUser { get; set; }
    }
} 