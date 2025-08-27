// 引入授权相关的命名空间，用于控制API访问权限
using Microsoft.AspNetCore.Authorization;
// 引入ASP.NET Core MVC的命名空间，用于创建Web API控制器
using Microsoft.AspNetCore.Mvc;
// 引入Entity Framework Core的命名空间，用于数据库操作
using Microsoft.EntityFrameworkCore;
// 引入我们项目中的实体类，用于数据库操作
using StudentManagement.Domain.Entities;
// 引入我们项目中的数据库上下文，用于访问数据库
using StudentManagement.Infrastructure.Data;
// 引入我们项目中的DTO类，用于数据传输
using StudentManagement.Core.DTOs;
// 引入异步编程相关的命名空间
using System.Threading.Tasks;
// 引入集合相关的命名空间
using System.Collections.Generic;
// 引入LINQ查询相关的命名空间
using System.Linq;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.API.Controllers
{
    /// <summary>
    /// 成绩管理控制器 - 处理所有与成绩相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/grades
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class GradesController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
    {
        /// <summary>
        /// 数据库上下文，用于访问数据库
        /// readonly 表示这个字段只能在构造函数中赋值，之后不能修改
        /// 通过依赖注入获取
        /// </summary>
        private readonly ApplicationDbContext _context;  // 私有只读字段，存储数据库上下文

        /// <summary>
        /// 构造函数，接收依赖注入的数据库上下文
        /// ApplicationDbContext 会由.NET的依赖注入容器自动提供
        /// </summary>
        /// <param name="context">数据库上下文</param>
        public GradesController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取成绩列表 - GET请求
        /// 路径：GET /api/grades
        /// 返回所有成绩信息，包括学生、课程、作业和评分教师信息
        /// </summary>
        /// <returns>返回成绩列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetGrades()  // 异步方法，返回成绩列表
        {
            // 查询所有成绩，同时加载相关的学生、课程、作业和评分教师信息
            var grades = await _context.Grades  // 异步查询Grades表
                .Include(g => g.Student)  // 同时加载关联的学生信息
                .Include(g => g.Course)  // 同时加载关联的课程信息
                .Include(g => g.Assignment)  // 同时加载关联的作业信息
                .Include(g => g.GradedByTeacher)  // 同时加载关联的评分教师信息
                .Select(g => new GradeDto  // 将查询结果投影为GradeDto对象
                {
                    Id = g.Id,  // 成绩ID
                    StudentId = g.StudentId,  // 学生ID
                    StudentName = g.Student.FirstName + " " + g.Student.LastName,  // 学生姓名（组合名字和姓氏）
                    CourseId = g.CourseId,  // 课程ID
                    CourseName = g.Course.CourseName,  // 课程名称
                    AssignmentId = g.AssignmentId,  // 作业ID
                    AssignmentTitle = g.Assignment != null ? g.Assignment.Title : null,  // 作业标题（如果作业存在）
                    Score = g.Score,  // 分数
                    GradeLetter = g.GradeLetter,  // 等级字母
                    Comments = g.Comments,  // 评语
                    GradedBy = g.GradedBy,  // 评分教师ID
                    GradedByTeacherName = g.GradedByTeacher != null ? g.GradedByTeacher.FirstName + " " + g.GradedByTeacher.LastName : null,  // 评分教师姓名（组合名字和姓氏）
                    GradedAt = g.GradedAt  // 评分时间
                })
                .ToListAsync();  // 异步执行查询并转换为列表
            return Ok(grades);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个成绩信息 - GET请求
        /// 路径：GET /api/grades/{id}
        /// 根据成绩ID获取特定成绩的详细信息
        /// </summary>
        /// <param name="id">成绩ID</param>
        /// <returns>返回成绩信息，如果没找到则返回404</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        public async Task<ActionResult<GradeDto>> GetGrade(int id)  // 异步方法，接收成绩ID参数
        {
            // 根据ID查找成绩，同时加载相关的学生、课程、作业和评分教师信息
            var g = await _context.Grades  // 异步查询Grades表
                .Include(x => x.Student)  // 同时加载关联的学生信息
                .Include(x => x.Course)  // 同时加载关联的课程信息
                .Include(x => x.Assignment)  // 同时加载关联的作业信息
                .Include(x => x.GradedByTeacher)  // 同时加载关联的评分教师信息
                .FirstOrDefaultAsync(x => x.Id == id);  // 查找指定ID的成绩
            if (g == null) return NotFound();  // 如果成绩不存在，返回404错误
            return Ok(new GradeDto  // 返回成功响应
            {
                Id = g.Id,  // 成绩ID
                StudentId = g.StudentId,  // 学生ID
                StudentName = g.Student.FirstName + " " + g.Student.LastName,  // 学生姓名（组合名字和姓氏）
                CourseId = g.CourseId,  // 课程ID
                CourseName = g.Course.CourseName,  // 课程名称
                AssignmentId = g.AssignmentId,  // 作业ID
                AssignmentTitle = g.Assignment != null ? g.Assignment.Title : null,  // 作业标题（如果作业存在）
                Score = g.Score,  // 分数
                GradeLetter = g.GradeLetter,  // 等级字母
                Comments = g.Comments,  // 评语
                GradedBy = g.GradedBy,  // 评分教师ID
                GradedByTeacherName = g.GradedByTeacher != null ? g.GradedByTeacher.FirstName + " " + g.GradedByTeacher.LastName : null,  // 评分教师姓名（组合名字和姓氏）
                GradedAt = g.GradedAt  // 评分时间
            });
        }

        /// <summary>
        /// 创建新成绩 - POST请求
        /// 路径：POST /api/grades
        /// [Authorize(Roles = "Admin,Teacher")] 表示只有管理员和教师角色才能创建成绩
        /// </summary>
        /// <param name="request">创建成绩请求对象，包含成绩信息</param>
        /// <returns>返回创建的成绩信息，如果用户信息无效或教师不存在则返回相应错误</returns>
        [HttpPost]  // 标识这个方法处理POST请求
        [Authorize(Roles = "Admin,Teacher")]  // 只允许管理员和教师角色访问
        public async Task<ActionResult> CreateGrade([FromBody] CreateGradeRequest request)  // 异步方法，从请求体获取参数
        {
            // 获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 获取当前教师信息
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);  // 根据用户ID查找教师
            if (teacher == null)  // 检查教师是否找到
            {
                return NotFound(new { message = "Teacher not found" });  // 返回未找到错误
            }

            // 自动生成等级
            string gradeLetter = GetGradeLetter(request.Score);
            
            var grade = new Grade
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                AssignmentId = request.AssignmentId,
                Score = request.Score,
                GradeLetter = request.GradeLetter ?? gradeLetter,
                Comments = request.Comments,
                GradedBy = teacher.Id,
                GradedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
            return Ok(grade);
        }

        // PUT: api/grades/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult> UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
        {
            var g = await _context.Grades.FindAsync(id);
            if (g == null) return NotFound();
            if (request.Score.HasValue) 
            {
                g.Score = request.Score.Value;
                // 如果分数改变，自动更新等级
                g.GradeLetter = request.GradeLetter ?? GetGradeLetter(request.Score.Value);
            }
            if (request.GradeLetter != null) g.GradeLetter = request.GradeLetter;
            if (request.Comments != null) g.Comments = request.Comments;
            g.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/grades/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult> DeleteGrade(int id)
        {
            var g = await _context.Grades.FindAsync(id);
            if (g == null) return NotFound();
            _context.Grades.Remove(g);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/grades/my-grades
        [HttpGet("my-grades")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetMyGrades()
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }
            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Course)
                .Include(g => g.Assignment)
                .Include(g => g.GradedByTeacher)
                .Where(g => g.Course.TeacherId == teacher.Id)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = g.Student.FirstName + " " + g.Student.LastName,
                    CourseId = g.CourseId,
                    CourseName = g.Course.CourseName,
                    AssignmentId = g.AssignmentId,
                    AssignmentTitle = g.Assignment != null ? g.Assignment.Title : null,
                    Score = g.Score,
                    GradeLetter = g.GradeLetter,
                    Comments = g.Comments,
                    GradedBy = g.GradedBy,
                    GradedByTeacherName = g.GradedByTeacher != null ? g.GradedByTeacher.FirstName + " " + g.GradedByTeacher.LastName : null,
                    GradedAt = g.GradedAt
                })
                .ToListAsync();
            return Ok(grades);
        }

        // GET: api/grades/students-dropdown
        [HttpGet("students-dropdown")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentsDropdown()
        {
            try
            {
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    return Unauthorized(new { message = "Invalid user information" });
                }
                
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);
                if (teacher == null)
                {
                    return NotFound(new { message = "Teacher not found" });
                }

                            // 增强防御性，避免空引用
            var students = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.Course != null && e.Course.TeacherId == teacher.Id && e.Status == "Enrolled" && e.Student != null)
                .Select(e => new
                {
                    value = e.Student.Id,
                    label = e.Student.StudentNumber + " - " + e.Student.FirstName + " " + e.Student.LastName
                })
                .Distinct()
                .OrderBy(s => s.label)
                .ToListAsync();

                // 添加日志以便调试
                Console.WriteLine($"Found {students.Count} students for teacher {teacher.Id}");

                return Ok(students);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentsDropdown: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/grades/courses-dropdown
        [HttpGet("courses-dropdown")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<object>>> GetCoursesDropdown()
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // 获取该教师的所有课程
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacher.Id)
                .Select(c => new
                {
                    value = c.Id,
                    label = c.CourseCode + " - " + c.CourseName
                })
                .OrderBy(c => c.label)
                .ToListAsync();

            return Ok(courses);
        }

        // GET: api/grades/assignments-dropdown
        [HttpGet("assignments-dropdown")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<object>>> GetAssignmentsDropdown([FromQuery] int? courseId = null)
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var query = _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course.TeacherId == teacher.Id);

            if (courseId.HasValue)
            {
                query = query.Where(a => a.CourseId == courseId.Value);
            }

            var assignments = await query
                .Select(a => new
                {
                    value = a.Id,
                    label = a.Title + " (" + a.Course.CourseName + ")"
                })
                .OrderBy(a => a.label)
                .ToListAsync();

            return Ok(assignments);
        }

        // 辅助方法：根据分数生成等级
        private string GetGradeLetter(decimal score)
        {
            if (score >= 90) return "A";
            if (score >= 80) return "B";
            if (score >= 70) return "C";
            if (score >= 60) return "D";
            return "F";
        }
    }
} 