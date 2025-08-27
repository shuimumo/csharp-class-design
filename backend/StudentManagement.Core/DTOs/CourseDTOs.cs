namespace StudentManagement.Core.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int MaxStudents { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Schedule { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = string.Empty;
        public int EnrolledStudents { get; set; }
    }

    public class CreateCourseRequest
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int? TeacherId { get; set; }
        public int MaxStudents { get; set; } = 50;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Schedule { get; set; }
        public string? Location { get; set; }
    }

    public class UpdateCourseRequest
    {
        public string? CourseName { get; set; }
        public string? Description { get; set; }
        public int? Credits { get; set; }
        public int? TeacherId { get; set; }
        public int? MaxStudents { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Schedule { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
    }
} 