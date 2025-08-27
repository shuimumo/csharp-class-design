// 引入授权相关的命名空间，用于控制API访问权限
using Microsoft.AspNetCore.Authorization;
// 引入ASP.NET Core MVC的命名空间，用于创建Web API控制器
using Microsoft.AspNetCore.Mvc;
// 引入Entity Framework Core的命名空间，用于数据库操作
using Microsoft.EntityFrameworkCore;
// 引入我们项目中的DTO类，用于数据传输
using StudentManagement.Core.DTOs;
// 引入我们项目中的实体类，用于数据库操作
using StudentManagement.Domain.Entities;
// 引入我们项目中的数据库上下文，用于访问数据库
using StudentManagement.Infrastructure.Data;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.API.Controllers
{
    /// <summary>
    /// 课程管理控制器 - 处理所有与课程相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/courses
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class CoursesController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public CoursesController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取课程列表 - GET请求
        /// 路径：GET /api/courses
        /// 返回所有课程信息，包括教师信息和已注册学生数量
        /// </summary>
        /// <returns>返回课程列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()  // 异步方法，返回课程列表
        {
            // 查询所有课程，同时加载教师信息和注册信息
            var courses = await _context.Courses  // 异步查询Courses表
                .Include(c => c.Teacher)  // 同时加载关联的教师信息
                .Include(c => c.Enrollments)  // 同时加载关联的注册信息
                .Select(c => new CourseDto  // 将查询结果投影为CourseDto对象
                {
                    Id = c.Id,  // 课程ID
                    CourseCode = c.CourseCode,  // 课程代码
                    CourseName = c.CourseName,  // 课程名称
                    Description = c.Description,  // 课程描述
                    Credits = c.Credits,  // 学分
                    TeacherId = c.TeacherId,  // 教师ID
                    TeacherName = c.Teacher != null ? ((c.Teacher.FirstName ?? "") + " " + (c.Teacher.LastName ?? "")).Trim() : null,  // 教师姓名（组合名字和姓氏）
                    MaxStudents = c.MaxStudents,  // 最大学生数
                    StartDate = c.StartDate,  // 开始日期
                    EndDate = c.EndDate,  // 结束日期
                    Schedule = c.Schedule,  // 课程安排
                    Location = c.Location,  // 上课地点
                    Status = c.Status,  // 课程状态
                    EnrolledStudents = c.Enrollments.Count(e => e.Status == "Enrolled")  // 已注册学生数量（只统计状态为"Enrolled"的）
                })
                .ToListAsync();  // 异步执行查询并转换为列表

            // 返回200成功状态码和课程列表
            return Ok(courses);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个课程信息 - GET请求
        /// 路径：GET /api/courses/{id}
        /// 根据课程ID获取特定课程的详细信息
        /// </summary>
        /// <param name="id">课程ID</param>
        /// <returns>返回课程信息，如果没找到则返回404</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        public async Task<ActionResult<CourseDto>> GetCourse(int id)  // 异步方法，接收课程ID参数
        {
            // 根据ID查找课程，同时加载教师信息和注册信息
            var course = await _context.Courses  // 异步查询Courses表
                .Include(c => c.Teacher)  // 同时加载关联的教师信息
                .Include(c => c.Enrollments)  // 同时加载关联的注册信息
                .FirstOrDefaultAsync(c => c.Id == id);  // 查找指定ID的课程

            // 如果课程不存在，返回404错误
            if (course == null)  // 检查课程是否找到
            {
                return NotFound();  // 返回未找到错误
            }

            // 创建课程DTO对象，将实体数据转换为传输对象
            var courseDto = new CourseDto  // 创建课程DTO对象
            {
                Id = course.Id,  // 课程ID
                CourseCode = course.CourseCode,  // 课程代码
                CourseName = course.CourseName,  // 课程名称
                Description = course.Description,  // 课程描述
                Credits = course.Credits,  // 学分
                TeacherId = course.TeacherId,  // 教师ID
                TeacherName = course.Teacher != null ? ((course.Teacher.FirstName ?? "") + " " + (course.Teacher.LastName ?? "")).Trim() : null,  // 教师姓名（组合名字和姓氏）
                MaxStudents = course.MaxStudents,  // 最大学生数
                StartDate = course.StartDate,  // 开始日期
                EndDate = course.EndDate,  // 结束日期
                Schedule = course.Schedule,  // 课程安排
                Location = course.Location,  // 上课地点
                Status = course.Status,  // 课程状态
                EnrolledStudents = course.Enrollments.Count(e => e.Status == "Enrolled")  // 已注册学生数量（只统计状态为"Enrolled"的）
            };

            // 返回200成功状态码和课程信息
            return Ok(courseDto);  // 返回成功响应
        }

        /// <summary>
        /// 创建新课程 - POST请求
        /// 路径：POST /api/courses
        /// [Authorize(Roles = "Teacher,Admin")] 表示只有教师和管理员角色才能创建课程
        /// </summary>
        /// <param name="request">创建课程请求对象，包含课程信息</param>
        /// <returns>返回创建的课程信息，如果课程代码已存在则返回400错误</returns>
        [HttpPost]  // 标识这个方法处理POST请求
        [Authorize(Roles = "Teacher,Admin")]  // 只允许教师和管理员角色访问
        public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseRequest request)  // 异步方法，创建课程
        {
            // 检查课程代码是否已经存在
            if (await _context.Courses.AnyAsync(c => c.CourseCode == request.CourseCode))  // 检查课程代码是否已存在
            {
                return BadRequest(new { message = "Course code already exists" });  // 返回错误请求
            }

            // 获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 如果是教师创建课程，自动设置TeacherId
            int? teacherId = request.TeacherId;  // 初始化教师ID
            if (User.IsInRole("Teacher"))  // 检查当前用户是否是教师角色
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);  // 根据用户ID查找教师
                if (teacher == null)  // 检查教师是否找到
                {
                    return NotFound(new { message = "Teacher not found" });  // 返回未找到错误
                }
                teacherId = teacher.Id;  // 设置教师ID为当前教师的ID
            }

            // 创建新课程对象
            var course = new Course  // 创建新的课程对象
            {
                CourseCode = request.CourseCode,  // 课程代码
                CourseName = request.CourseName,  // 课程名称
                Description = request.Description,  // 课程描述
                Credits = request.Credits,  // 学分
                TeacherId = teacherId,  // 教师ID
                MaxStudents = request.MaxStudents,  // 最大学生数
                StartDate = request.StartDate,  // 开始日期
                EndDate = request.EndDate,  // 结束日期
                Schedule = request.Schedule,  // 课程安排
                Location = request.Location,  // 上课地点
                Status = "Active"  // 设置课程状态为活跃
            };

            // 将课程添加到数据库上下文
            _context.Courses.Add(course);  // 将课程添加到数据库上下文的Courses集合中
            // 保存更改到数据库
            await _context.SaveChangesAsync();  // 异步保存更改到数据库

            // 创建课程DTO对象，用于返回响应
            var courseDto = new CourseDto  // 创建课程DTO对象
            {
                Id = course.Id,  // 课程ID
                CourseCode = course.CourseCode,  // 课程代码
                CourseName = course.CourseName,  // 课程名称
                Description = course.Description,  // 课程描述
                Credits = course.Credits,  // 学分
                TeacherId = course.TeacherId,  // 教师ID
                MaxStudents = course.MaxStudents,  // 最大学生数
                StartDate = course.StartDate,  // 开始日期
                EndDate = course.EndDate,  // 结束日期
                Schedule = course.Schedule,  // 课程安排
                Location = course.Location,  // 上课地点
                Status = course.Status,  // 课程状态
                EnrolledStudents = 0  // 新创建的课程还没有学生注册
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, courseDto);
        }

        // PUT: api/Courses/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> UpdateCourse(int id, UpdateCourseRequest request)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // 检查权限：教师只能编辑自己的课程
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }

            if (User.IsInRole("Teacher"))
            {
                if (course.Teacher?.UserId != currentUserId)
                {
                    return Forbid();
                }
            }

            // Update course properties
            if (request.CourseName != null) course.CourseName = request.CourseName;
            if (request.Description != null) course.Description = request.Description;
            if (request.Credits.HasValue) course.Credits = request.Credits.Value;
            if (request.TeacherId.HasValue) course.TeacherId = request.TeacherId.Value;
            if (request.MaxStudents.HasValue) course.MaxStudents = request.MaxStudents.Value;
            if (request.StartDate.HasValue) course.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) course.EndDate = request.EndDate.Value;
            if (request.Schedule != null) course.Schedule = request.Schedule;
            if (request.Location != null) course.Location = request.Location;
            if (request.Status != null) course.Status = request.Status;

            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.Assignments)
                .Include(c => c.Grades)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // 检查是否有学生已选课
            if (course.Enrollments.Any(e => e.Status == "Enrolled"))
            {
                return BadRequest(new { message = "无法删除已有学生选课的课程" });
            }

            // 删除相关的作业、成绩等
            _context.Assignments.RemoveRange(course.Assignments);
            _context.Grades.RemoveRange(course.Grades);
            _context.Enrollments.RemoveRange(course.Enrollments);
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Courses/my-courses
        [HttpGet("my-courses")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetMyCourses()
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }

            IQueryable<Course> coursesQuery;

            if (currentUserRole == "Student")
            {
                // Get courses where student is enrolled
                coursesQuery = _context.Courses
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .Where(c => c.Enrollments.Any(e => e.Student.UserId == currentUserId && e.Status == "Enrolled"));
            }
            else if (currentUserRole == "Teacher")
            {
                // Get courses taught by the teacher
                coursesQuery = _context.Courses
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .Where(c => c.Teacher != null && c.Teacher.UserId == currentUserId);
            }
            else
            {
                // Admin can see all courses
                coursesQuery = _context.Courses
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments);
            }

            var courses = await coursesQuery
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Credits = c.Credits,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher != null ? ((c.Teacher.FirstName ?? "") + " " + (c.Teacher.LastName ?? "")).Trim() : null,
                    MaxStudents = c.MaxStudents,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Schedule = c.Schedule,
                    Location = c.Location,
                    Status = c.Status,
                    EnrolledStudents = c.Enrollments.Count(e => e.Status == "Enrolled")
                })
                .ToListAsync();

            return Ok(courses);
        }
    }
} 