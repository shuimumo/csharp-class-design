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
// 引入异步编程相关的命名空间
using System.Threading.Tasks;
// 引入集合相关的命名空间
using System.Collections.Generic;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.API.Controllers
{
    /// <summary>
    /// 选课管理控制器 - 处理所有与选课相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/enrollments
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class EnrollmentsController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public EnrollmentsController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取选课列表 - GET请求
        /// 路径：GET /api/enrollments
        /// [Authorize(Roles = "Admin")] 表示只有管理员角色才能访问
        /// 返回所有选课信息，按选课日期降序排列
        /// </summary>
        /// <returns>返回选课列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        [Authorize(Roles = "Admin")]  // 只允许管理员角色访问
        public async Task<ActionResult<IEnumerable<object>>> GetEnrollments()  // 异步方法，返回选课列表
        {
            // 查询所有选课信息，同时加载学生和课程信息，按选课日期降序排列
            var enrollments = await _context.Enrollments  // 异步查询Enrollments表
                .Include(e => e.Student)  // 同时加载关联的学生信息
                .Include(e => e.Course)  // 同时加载关联的课程信息
                .OrderByDescending(e => e.EnrollmentDate)  // 按选课日期降序排列
                .Select(e => new  // 将查询结果投影为匿名对象
                {
                    e.Id,  // 选课ID
                    StudentId = e.StudentId,  // 学生ID
                    StudentName = e.Student.FirstName + " " + e.Student.LastName,  // 学生姓名（组合名字和姓氏）
                    CourseId = e.CourseId,  // 课程ID
                    CourseName = e.Course.CourseName,  // 课程名称
                    e.Status,  // 选课状态
                    e.EnrollmentDate,  // 选课日期
                    e.Grade  // 成绩
                })
                .ToListAsync();  // 异步执行查询并转换为列表
            return Ok(enrollments);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个选课信息 - GET请求
        /// 路径：GET /api/enrollments/{id}
        /// 根据选课ID获取特定选课的详细信息
        /// 权限控制：管理员和教师可以查看任何选课，学生只能查看自己的选课
        /// </summary>
        /// <param name="id">选课ID</param>
        /// <returns>返回选课信息，如果没找到则返回404，如果权限不足则返回403</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        public async Task<ActionResult<object>> GetEnrollment(int id)  // 异步方法，接收选课ID参数
        {
            // 根据ID查找选课，同时加载学生和课程信息
            var enrollment = await _context.Enrollments  // 异步查询Enrollments表
                .Include(e => e.Student)  // 同时加载关联的学生信息
                .Include(e => e.Course)  // 同时加载关联的课程信息
                .FirstOrDefaultAsync(e => e.Id == id);  // 查找指定ID的选课

            if (enrollment == null) return NotFound();  // 如果选课不存在，返回404错误

            // 检查权限：管理员和教师可以查看任何选课，学生只能查看自己的选课
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");  // 从JWT令牌中获取当前用户ID
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 从JWT令牌中获取当前用户角色

            // 权限检查：管理员和教师可以查看任何选课，学生只能查看自己的选课
            if (currentUserRole != "Admin" &&   // 如果不是管理员
                currentUserRole != "Teacher" &&   // 且不是教师
                enrollment.Student.UserId != currentUserId)  // 且不是选课的学生本人
            {
                return Forbid();  // 返回禁止访问错误
            }

            return Ok(new  // 返回成功响应
            {
                enrollment.Id,  // 选课ID
                StudentId = enrollment.StudentId,  // 学生ID
                StudentName = enrollment.Student.FirstName + " " + enrollment.Student.LastName,  // 学生姓名（组合名字和姓氏）
                CourseId = enrollment.CourseId,  // 课程ID
                CourseName = enrollment.Course.CourseName,  // 课程名称
                enrollment.Status,  // 选课状态
                enrollment.EnrollmentDate,  // 选课日期
                enrollment.Grade  // 成绩
            });
        }

        /// <summary>
        /// 创建新选课 - POST请求
        /// 路径：POST /api/enrollments
        /// [Authorize(Roles = "Student")] 表示只有学生角色才能选课
        /// </summary>
        /// <param name="request">创建选课请求对象，包含课程ID</param>
        /// <returns>返回创建的选课信息，如果验证失败则返回相应错误</returns>
        [HttpPost]  // 标识这个方法处理POST请求
        [Authorize(Roles = "Student")]  // 只允许学生角色访问
        public async Task<ActionResult<object>> CreateEnrollment([FromBody] CreateEnrollmentRequest request)  // 异步方法，从请求体获取参数
        {
            // 从JWT令牌中获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 根据用户ID查找学生
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);  // 根据用户ID查找学生
            if (student == null)  // 检查学生是否找到
            {
                return NotFound(new { message = "Student not found" });  // 返回未找到错误
            }

            // 检查课程是否存在
            var course = await _context.Courses.FindAsync(request.CourseId);  // 根据课程ID查找课程
            if (course == null)  // 检查课程是否找到
            {
                return NotFound(new { message = "Course not found" });  // 返回未找到错误
            }

            // 检查是否已经选过这门课
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == request.CourseId);  // 检查是否已存在该选课
            
            if (existingEnrollment != null)  // 如果已存在，返回BadRequest
            {
                return BadRequest(new { message = "已经选过这门课程" });
            }

            // 检查课程是否已满
            var enrolledCount = await _context.Enrollments
                .CountAsync(e => e.CourseId == request.CourseId && e.Status == "Enrolled");  // 检查已选该课程的学生数量
            
            if (course.MaxStudents > 0 && enrolledCount >= course.MaxStudents)  // 如果课程有最大学生限制且已满
            {
                return BadRequest(new { message = "课程已满" });
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = request.CourseId,
                Status = "Enrolled",
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.Id }, new
            {
                enrollment.Id,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                enrollment.Status,
                enrollment.EnrollmentDate
            });
        }

        /// <summary>
        /// 更新选课信息 - PUT请求
        /// 路径：PUT /api/enrollments/{id}
        /// [Authorize(Roles = "Admin,Teacher")] 表示管理员或教师可以更新选课
        /// </summary>
        /// <param name="id">选课ID</param>
        /// <param name="request">更新选课请求对象，包含状态和成绩</param>
        /// <returns>返回204 No Content，如果选课不存在则返回404</returns>
        [HttpPut("{id}")]  // 标识这个方法处理PUT请求，路径包含id参数
        [Authorize(Roles = "Admin,Teacher")]  // 只允许管理员或教师访问
        public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] UpdateEnrollmentRequest request)  // 异步方法，接收选课ID和更新请求
        {
            var enrollment = await _context.Enrollments.FindAsync(id);  // 根据ID查找选课
            if (enrollment == null) return NotFound();  // 如果选课不存在，返回404错误

            if (request.Status != null) enrollment.Status = request.Status;  // 更新状态
            if (request.Grade.HasValue) enrollment.Grade = request.Grade.Value;  // 更新成绩

            await _context.SaveChangesAsync();  // 保存更改到数据库
            return NoContent();  // 返回204 No Content
        }

        /// <summary>
        /// 删除选课 - DELETE请求
        /// 路径：DELETE /api/enrollments/{id}
        /// [Authorize(Roles = "Student")] 表示只有学生角色才能删除选课
        /// </summary>
        /// <param name="id">选课ID</param>
        /// <returns>返回204 No Content，如果选课不存在或权限不足则返回404或403</returns>
        [HttpDelete("{id}")]  // 标识这个方法处理DELETE请求，路径包含id参数
        [Authorize(Roles = "Student")]  // 只允许学生角色访问
        public async Task<IActionResult> DeleteEnrollment(int id)  // 异步方法，接收选课ID
        {
            // 从JWT令牌中获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 根据用户ID查找学生
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);  // 根据用户ID查找学生
            if (student == null)  // 检查学生是否找到
            {
                return NotFound(new { message = "Student not found" });  // 返回未找到错误
            }

            // 根据选课ID和学生ID查找选课
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == id && e.StudentId == student.Id);  // 查找指定ID和学生ID的选课
            
            if (enrollment == null) return NotFound();  // 如果选课不存在，返回404错误

            _context.Enrollments.Remove(enrollment);  // 删除选课
            await _context.SaveChangesAsync();  // 保存更改到数据库
            return NoContent();  // 返回204 No Content
        }

        /// <summary>
        /// 获取当前用户选课列表 - GET请求
        /// 路径：GET /api/enrollments/my-enrollments
        /// [Authorize(Roles = "Student")] 表示只有学生角色才能访问
        /// 返回当前用户选课的详细信息，按选课日期降序排列
        /// </summary>
        /// <returns>返回当前用户选课列表，如果未找到则返回空列表</returns>
        [HttpGet("my-enrollments")]  // 标识这个方法处理GET请求，路径包含"my-enrollments"
        [Authorize(Roles = "Student")]  // 只允许学生角色访问
        public async Task<ActionResult<IEnumerable<object>>> GetMyEnrollments()  // 异步方法，返回当前用户选课列表
        {
            // 从JWT令牌中获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 根据用户ID查找学生
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);  // 根据用户ID查找学生
            if (student == null)  // 检查学生是否找到
            {
                return NotFound(new { message = "Student not found" });  // 返回未找到错误
            }

            // 查询当前学生的所有选课，同时加载课程和教师信息，按选课日期降序排列
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)  // 加载课程信息
                .Include(e => e.Course.Teacher)  // 加载教师信息
                .Where(e => e.StudentId == student.Id)  // 筛选当前学生
                .OrderByDescending(e => e.EnrollmentDate)  // 按选课日期降序排列
                .Select(e => new  // 将查询结果投影为匿名对象
                {
                    e.Id,  // 选课ID
                    CourseId = e.CourseId,  // 课程ID
                    CourseName = e.Course.CourseName,  // 课程名称
                    CourseCode = e.Course.CourseCode,  // 课程代码
                    TeacherName = e.Course.Teacher != null ? e.Course.Teacher.FirstName + " " + e.Course.Teacher.LastName : null,  // 教师姓名（组合名字和姓氏）
                    e.Status,  // 选课状态
                    e.EnrollmentDate,  // 选课日期
                    e.Grade,  // 成绩
                    Credits = e.Course.Credits  // 学分
                })
                .ToListAsync();  // 异步执行查询并转换为列表

            return Ok(enrollments);  // 返回成功响应
        }

        /// <summary>
        /// 获取课程选课列表 - GET请求
        /// 路径：GET /api/enrollments/course/{courseId}
        /// [Authorize(Roles = "Teacher,Admin")] 表示教师或管理员可以访问
        /// 返回指定课程的所有选课信息，按学生姓氏和名字排序
        /// </summary>
        /// <param name="courseId">课程ID</param>
        /// <returns>返回课程选课列表，如果未找到则返回空列表</returns>
        [HttpGet("course/{courseId}")]  // 标识这个方法处理GET请求，路径包含"course"和课程ID
        [Authorize(Roles = "Teacher,Admin")]  // 只允许教师或管理员访问
        public async Task<ActionResult<IEnumerable<object>>> GetCourseEnrollments(int courseId)  // 异步方法，接收课程ID参数
        {
            // 查询指定课程的所有选课，同时加载学生和用户信息，按学生姓氏和名字排序
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)  // 加载学生信息
                .Include(e => e.Student.User)  // 加载用户信息
                .Where(e => e.CourseId == courseId)  // 筛选指定课程
                .OrderBy(e => e.Student.LastName)  // 按学生姓氏排序
                .ThenBy(e => e.Student.FirstName)  // 按学生名字排序
                .Select(e => new  // 将查询结果投影为匿名对象
                {
                    e.Id,  // 选课ID
                    StudentId = e.StudentId,  // 学生ID
                    StudentName = e.Student.FirstName + " " + e.Student.LastName,  // 学生姓名（组合名字和姓氏）
                    StudentNumber = e.Student.StudentNumber,  // 学生学号
                    Email = e.Student.User.Email,  // 学生邮箱
                    e.Status,  // 选课状态
                    e.EnrollmentDate,  // 选课日期
                    e.Grade  // 成绩
                })
                .ToListAsync();  // 异步执行查询并转换为列表

            return Ok(enrollments);  // 返回成功响应
        }
    }

    /// <summary>
    /// 创建选课请求对象
    /// </summary>
    public class CreateEnrollmentRequest
    {
        /// <summary>
        /// 课程ID
        /// </summary>
        public int CourseId { get; set; }
    }

    /// <summary>
    /// 更新选课请求对象
    /// </summary>
    public class UpdateEnrollmentRequest
    {
        /// <summary>
        /// 选课状态
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// 成绩
        /// </summary>
        public decimal? Grade { get; set; }
    }
} 