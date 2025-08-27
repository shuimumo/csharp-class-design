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
// 引入加密算法的命名空间，用于密码哈希
using System.Security.Cryptography;
// 引入文本编码的命名空间，用于字符串和字节数组的转换
using System.Text;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.API.Controllers
{
    /// <summary>
    /// 教师管理控制器 - 处理所有与教师相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/teachers
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class TeachersController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public TeachersController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取教师列表 - GET请求
        /// 路径：GET /api/teachers
        /// [Authorize(Roles = "Admin")] 表示只有管理员角色才能访问
        /// 返回所有教师信息，包括用户信息
        /// </summary>
        /// <returns>返回教师列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        [Authorize(Roles = "Admin")]  // 只允许管理员角色访问
        public async Task<ActionResult<IEnumerable<TeacherDto>>> GetTeachers()  // 异步方法，返回教师列表
        {
            // 查询所有教师，同时加载用户信息
            var teachers = await _context.Teachers  // 异步查询Teachers表
                .Include(t => t.User)  // 同时加载关联的用户信息
                .Select(t => new TeacherDto  // 将查询结果投影为TeacherDto对象
                {
                    Id = t.Id,  // 教师ID
                    TeacherNumber = t.TeacherNumber,  // 工号
                    FirstName = t.FirstName ?? string.Empty,  // 名字，如果为null则设为空字符串
                    LastName = t.LastName ?? string.Empty,  // 姓氏，如果为null则设为空字符串
                    DateOfBirth = t.DateOfBirth,  // 出生日期
                    Gender = t.Gender ?? string.Empty,  // 性别，如果为null则设为空字符串
                    Phone = t.Phone ?? string.Empty,  // 电话号码，如果为null则设为空字符串
                    Address = t.Address ?? string.Empty,  // 地址，如果为null则设为空字符串
                    Department = t.Department ?? string.Empty,  // 部门，如果为null则设为空字符串
                    Title = t.Title ?? string.Empty,  // 职称，如果为null则设为空字符串
                    HireDate = t.HireDate,  // 入职日期
                    Email = t.User.Email ?? string.Empty,  // 邮箱（从用户表获取），如果为null则设为空字符串
                    Username = t.User.Username ?? string.Empty  // 用户名（从用户表获取），如果为null则设为空字符串
                })
                .ToListAsync();  // 异步执行查询并转换为列表

            // 返回200成功状态码和教师列表
            return Ok(teachers);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个教师信息 - GET请求
        /// 路径：GET /api/teachers/{id}
        /// [Authorize(Roles = "Admin")] 表示只有管理员角色才能访问
        /// 根据教师ID获取特定教师的详细信息
        /// </summary>
        /// <param name="id">教师ID</param>
        /// <returns>返回教师信息，如果没找到则返回404</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        [Authorize(Roles = "Admin")]  // 只允许管理员角色访问
        public async Task<ActionResult<TeacherDto>> GetTeacher(int id)  // 异步方法，接收教师ID参数
        {
            // 根据ID查找教师，同时加载用户信息
            var teacher = await _context.Teachers  // 异步查询Teachers表
                .Include(t => t.User)  // 同时加载关联的用户信息
                .FirstOrDefaultAsync(t => t.Id == id);  // 查找指定ID的教师

            // 如果教师不存在，返回404错误
            if (teacher == null)  // 检查教师是否找到
            {
                return NotFound();  // 返回未找到错误
            }

            // 创建教师DTO对象，将实体数据转换为传输对象
            var teacherDto = new TeacherDto  // 创建教师DTO对象
            {
                Id = teacher.Id,  // 教师ID
                TeacherNumber = teacher.TeacherNumber,  // 工号
                FirstName = teacher.FirstName ?? string.Empty,  // 名字，如果为null则设为空字符串
                LastName = teacher.LastName ?? string.Empty,  // 姓氏，如果为null则设为空字符串
                DateOfBirth = teacher.DateOfBirth,  // 出生日期
                Gender = teacher.Gender ?? string.Empty,  // 性别，如果为null则设为空字符串
                Phone = teacher.Phone ?? string.Empty,  // 电话号码，如果为null则设为空字符串
                Address = teacher.Address ?? string.Empty,  // 地址，如果为null则设为空字符串
                Department = teacher.Department ?? string.Empty,  // 部门，如果为null则设为空字符串
                Title = teacher.Title ?? string.Empty,  // 职称，如果为null则设为空字符串
                HireDate = teacher.HireDate,  // 入职日期
                Email = teacher.User.Email ?? string.Empty,  // 邮箱（从用户表获取），如果为null则设为空字符串
                Username = teacher.User.Username ?? string.Empty  // 用户名（从用户表获取），如果为null则设为空字符串
            };

            // 返回200成功状态码和教师信息
            return Ok(teacherDto);  // 返回成功响应
        }

        /// <summary>
        /// 创建新教师 - POST请求
        /// 路径：POST /api/teachers
        /// [Authorize(Roles = "Admin")] 表示只有管理员角色才能创建教师
        /// </summary>
        /// <param name="request">创建教师请求对象，包含教师信息和用户信息</param>
        /// <returns>返回创建的教师信息，如果工号、用户名或邮箱已存在则返回400错误</returns>
        [HttpPost]  // 标识这个方法处理POST请求
        [Authorize(Roles = "Admin")]  // 只允许管理员角色访问
        public async Task<ActionResult<TeacherDto>> CreateTeacher(CreateTeacherRequest request)  // 异步方法，创建教师
        {
            // 检查工号是否已经存在
            if (await _context.Teachers.AnyAsync(t => t.TeacherNumber == request.TeacherNumber))  // 检查工号是否已存在
            {
                return BadRequest(new { message = "Teacher number already exists" });  // 返回错误请求
            }

            // 检查用户名是否已经存在
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))  // 检查用户名是否已存在
            {
                return BadRequest(new { message = "用户名已存在" });  // 返回错误请求
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "邮箱已存在" });
            }

            // Create user first with proper password hashing
            var user = new User
            {
                Username = request.Username,
                Password = HashPassword(request.Password),
                Email = request.Email,
                Role = "Teacher",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create teacher
            var teacher = new Teacher
            {
                UserId = user.Id,
                TeacherNumber = request.TeacherNumber,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Phone = request.Phone ?? string.Empty,
                Address = request.Address ?? string.Empty,
                Department = request.Department ?? string.Empty,
                Title = request.Title ?? string.Empty
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            var teacherDto = new TeacherDto
            {
                Id = teacher.Id,
                TeacherNumber = teacher.TeacherNumber,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                DateOfBirth = teacher.DateOfBirth,
                Gender = teacher.Gender,
                Phone = teacher.Phone,
                Address = teacher.Address,
                Department = teacher.Department,
                Title = teacher.Title,
                HireDate = teacher.HireDate,
                Email = user.Email,
                Username = user.Username
            };

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, teacherDto);
        }

        // PUT: api/Teachers/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher(int id, UpdateTeacherRequest request)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            if (request.FirstName != null) teacher.FirstName = request.FirstName;
            if (request.LastName != null) teacher.LastName = request.LastName;
            if (request.DateOfBirth.HasValue) teacher.DateOfBirth = request.DateOfBirth;
            if (request.Gender != null) teacher.Gender = request.Gender;
            if (request.Phone != null) teacher.Phone = request.Phone;
            if (request.Address != null) teacher.Address = request.Address;
            if (request.Department != null) teacher.Department = request.Department;
            if (request.Title != null) teacher.Title = request.Title;

            if (request.Email != null) teacher.User.Email = request.Email;
            if (request.Username != null) teacher.User.Username = request.Username;

            teacher.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Teachers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Check if teacher has active courses
            if (teacher.Courses.Any(c => c.Status == "Active"))
            {
                return BadRequest(new { message = "无法删除有活跃课程的教师" });
            }

            // Soft delete by setting IsActive to false
            teacher.User.IsActive = false;
            teacher.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Teachers/my-students
        [HttpGet("my-students")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetMyStudents()
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

            var students = await _context.Courses
                .Where(c => c.TeacherId == teacher.Id)
                .SelectMany(c => c.Enrollments)
                .Select(e => e.Student)
                .Distinct()
                .Include(s => s.User)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    StudentNumber = s.StudentNumber,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    DateOfBirth = s.DateOfBirth,
                    Gender = s.Gender,
                    Phone = s.Phone,
                    Address = s.Address,
                    EnrollmentDate = s.EnrollmentDate,
                    Major = s.Major,
                    Class = s.Class,
                    Email = s.User.Email,
                    Username = s.User.Username
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetTeacherUsers()
        {
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            var users = teachers.Select(t => new {
                id = t.User.Id,
                username = t.User.Username,
                email = t.User.Email,
                role = t.User.Role
            });
            return Ok(users);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 