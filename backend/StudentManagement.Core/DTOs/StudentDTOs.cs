// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Core.DTOs
{
    /// <summary>
    /// 学生数据传输对象 - 用于向前端返回学生的基本信息
    /// 这个类包含了学生的主要信息，用于列表显示和基本信息展示
    /// 不包含敏感信息如密码，确保数据传输的安全性
    /// </summary>
    public class StudentDto
    {
        /// <summary>
        /// 学生ID，用于唯一标识学生
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 学生学号，学校的唯一标识符
        /// </summary>
        public string StudentNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生名字
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生姓氏
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生出生日期，可选字段
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
        
        /// <summary>
        /// 学生性别，可选字段
        /// </summary>
        public string? Gender { get; set; }
        
        /// <summary>
        /// 学生电话号码，可选字段
        /// </summary>
        public string? Phone { get; set; }
        
        /// <summary>
        /// 学生地址，可选字段
        /// </summary>
        public string? Address { get; set; }
        
        /// <summary>
        /// 学生入学日期
        /// </summary>
        public DateTime EnrollmentDate { get; set; }
        
        /// <summary>
        /// 学生专业，可选字段
        /// </summary>
        public string? Major { get; set; }
        
        /// <summary>
        /// 学生班级，可选字段
        /// </summary>
        public string? Class { get; set; }
        
        /// <summary>
        /// 学生邮箱地址
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生用户名，用于登录
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生账户是否处于活跃状态
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    public class StudentDetailDto : StudentDto
    {
        public int EnrolledCoursesCount { get; set; }
        public decimal AverageGrade { get; set; }
        public int CompletedAssignmentsCount { get; set; }
        public int TotalAssignmentsCount { get; set; }
        public List<StudentCourseDto> Courses { get; set; } = new List<StudentCourseDto>();
    }

    public class StudentCourseDto
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public decimal? Grade { get; set; }
        public int Credits { get; set; }
    }

    public class CreateStudentRequest
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Major { get; set; }
        public string? Class { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateStudentRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Major { get; set; }
        public string? Class { get; set; }
        public string? Email { get; set; }
    }

    public class StudentStatisticsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int MaleStudents { get; set; }
        public int FemaleStudents { get; set; }
        public List<MajorStatDto> MajorStats { get; set; } = new List<MajorStatDto>();
        public List<ClassStatDto> ClassStats { get; set; } = new List<ClassStatDto>();
        public List<EnrollmentYearStatDto> EnrollmentYearStats { get; set; } = new List<EnrollmentYearStatDto>();
    }

    public class MajorStatDto
    {
        public string Major { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ClassStatDto
    {
        public string Class { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class EnrollmentYearStatDto
    {
        public int Year { get; set; }
        public int Count { get; set; }
    }

    public class StudentSearchRequest
    {
        public string? Search { get; set; }
        public string? Major { get; set; }
        public string? Class { get; set; }
        public string? Gender { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }
} 