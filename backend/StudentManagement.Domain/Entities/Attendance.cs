using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Domain.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        
        public int CourseId { get; set; }
        
        public DateTime Date { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Present";
        
        [StringLength(200)]
        public string? Notes { get; set; }
        
        public int? RecordedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
        public virtual Teacher? RecordedByTeacher { get; set; }
    }
} 