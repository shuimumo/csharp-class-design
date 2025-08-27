namespace StudentManagement.Core.DTOs
{
    public class GradeDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int? AssignmentId { get; set; }
        public string? AssignmentTitle { get; set; }
        public decimal Score { get; set; }
        public string? GradeLetter { get; set; }
        public string? Comments { get; set; }
        public int? GradedBy { get; set; }
        public string? GradedByTeacherName { get; set; }
        public DateTime GradedAt { get; set; }
    }

    public class CreateGradeRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int? AssignmentId { get; set; }
        public decimal Score { get; set; }
        public string? GradeLetter { get; set; }
        public string? Comments { get; set; }
    }

    public class UpdateGradeRequest
    {
        public decimal? Score { get; set; }
        public string? GradeLetter { get; set; }
        public string? Comments { get; set; }
    }
} 