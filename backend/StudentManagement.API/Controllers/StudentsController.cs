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
    /// 学生管理控制器 - 处理所有与学生相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/students
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class StudentsController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public StudentsController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取学生列表 - GET请求
        /// 路径：GET /api/students
        /// [Authorize(Roles = "Teacher,Admin")] 表示只有教师和管理员角色才能访问
        /// 支持搜索、按专业筛选、按班级筛选等功能
        /// </summary>
        /// <param name="search">搜索关键词，可以搜索姓名、学号、用户名、邮箱</param>
        /// <param name="major">专业筛选条件</param>
        /// <param name="className">班级筛选条件</param>
        /// <returns>返回学生列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        [Authorize(Roles = "Teacher,Admin")]  // 只允许教师和管理员角色访问
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents([FromQuery] string? search = null, [FromQuery] string? major = null, [FromQuery] string? className = null)  // 异步方法，从查询字符串获取参数
        {
            // 创建查询对象，包含学生信息和关联的用户信息
            // Include(s => s.User) 表示同时加载用户信息，避免N+1查询问题
            var query = _context.Students.Include(s => s.User).AsQueryable();  // 创建包含用户信息的查询对象

            // 搜索功能 - 如果提供了搜索关键词
            if (!string.IsNullOrEmpty(search))  // 检查搜索关键词是否不为空
            {
                // 在多个字段中搜索：名字、姓氏、学号、用户名、邮箱
                // Contains 方法用于模糊匹配，类似于SQL的LIKE操作
                query = query.Where(s =>   // 添加搜索条件
                    s.FirstName.Contains(search) ||   // 在名字中搜索
                    s.LastName.Contains(search) ||    // 在姓氏中搜索
                    s.StudentNumber.Contains(search) ||  // 在学号中搜索
                    s.User.Username.Contains(search) ||  // 在用户名中搜索
                    s.User.Email.Contains(search)  // 在邮箱中搜索
                );
            }

            // 按专业筛选 - 如果提供了专业筛选条件
            if (!string.IsNullOrEmpty(major))  // 检查专业筛选条件是否不为空
            {
                // 精确匹配专业名称
                query = query.Where(s => s.Major == major);  // 添加专业筛选条件
            }

            // 按班级筛选 - 如果提供了班级筛选条件
            if (!string.IsNullOrEmpty(className))  // 检查班级筛选条件是否不为空
            {
                // 精确匹配班级名称
                query = query.Where(s => s.Class == className);  // 添加班级筛选条件
            }

            // 执行查询并将结果转换为DTO对象
            var students = await query  // 异步执行查询
                .Select(s => new StudentDto  // 将查询结果投影为StudentDto对象
                {
                    Id = s.Id,                           // 学生ID
                    StudentNumber = s.StudentNumber,      // 学号
                    FirstName = s.FirstName,              // 名字
                    LastName = s.LastName,                // 姓氏
                    DateOfBirth = s.DateOfBirth,         // 出生日期
                    Gender = s.Gender,                    // 性别
                    Phone = s.Phone,                      // 电话号码
                    Address = s.Address,                  // 地址
                    EnrollmentDate = s.EnrollmentDate,    // 入学日期
                    Major = s.Major,                      // 专业
                    Class = s.Class,                      // 班级
                    Email = s.User.Email,                 // 邮箱（从用户表获取）
                    Username = s.User.Username            // 用户名（从用户表获取）
                })
                .ToListAsync();  // 异步执行查询并转换为列表

            // 返回200成功状态码和学生列表
            return Ok(students);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个学生信息 - GET请求
        /// 路径：GET /api/students/{id}
        /// 需要身份验证，但权限控制更复杂：
        /// - 教师和管理员可以查看任何学生
        /// - 学生只能查看自己的信息
        /// </summary>
        /// <param name="id">学生ID</param>
        /// <returns>返回学生信息，如果没找到则返回404，如果权限不足则返回403</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        public async Task<ActionResult<StudentDto>> GetStudent(int id)  // 异步方法，接收学生ID参数
        {
            // 根据ID查找学生，同时加载用户信息
            var student = await _context.Students  // 异步查询Students表
                .Include(s => s.User)  // 同时加载关联的用户信息
                .FirstOrDefaultAsync(s => s.Id == id);  // 查找指定ID的学生

            // 如果学生不存在，返回404错误
            if (student == null)  // 检查学生是否找到
            {
                return NotFound();  // 返回未找到错误
            }

            // 检查当前用户是否有权限查看这个学生的信息
            // 从JWT令牌中获取当前用户ID
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");  // 从JWT令牌中获取当前用户ID
            // 从JWT令牌中获取当前用户角色
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 从JWT令牌中获取当前用户角色

            // 权限检查：
            // 1. 教师和管理员可以查看任何学生
            // 2. 学生只能查看自己的信息
            if (currentUserRole != "Teacher" && currentUserRole != "Admin" && student.UserId != currentUserId)  // 检查权限
            {
                // 权限不足，返回403禁止访问
                return Forbid();  // 返回禁止访问错误
            }

            // 创建学生DTO对象，将实体数据转换为传输对象
            var studentDto = new StudentDto  // 创建学生DTO对象
            {
                Id = student.Id,  // 学生ID
                StudentNumber = student.StudentNumber,  // 学号
                FirstName = student.FirstName,  // 名字
                LastName = student.LastName,  // 姓氏
                DateOfBirth = student.DateOfBirth,  // 出生日期
                Gender = student.Gender,  // 性别
                Phone = student.Phone,  // 电话号码
                Address = student.Address,  // 地址
                EnrollmentDate = student.EnrollmentDate,  // 入学日期
                Major = student.Major,  // 专业
                Class = student.Class,  // 班级
                Email = student.User.Email,  // 邮箱（从用户表获取）
                Username = student.User.Username  // 用户名（从用户表获取）
            };

            return Ok(studentDto);  // 返回成功响应
        }

        // POST: api/Students
        [HttpPost]  // 标识这个方法处理POST请求
        [Authorize(Roles = "Teacher,Admin")]  // 只允许教师和管理员角色访问
        public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentRequest request)  // 异步方法，创建学生
        {
            // Check if student number already exists
            if (await _context.Students.AnyAsync(s => s.StudentNumber == request.StudentNumber))  // 检查学号是否已存在
            {
                return BadRequest(new { message = "Student number already exists" });  // 返回错误请求
            }

            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))  // 检查用户名是否已存在
            {
                return BadRequest(new { message = "用户名已存在" });  // 返回错误请求
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))  // 检查邮箱是否已存在
            {
                return BadRequest(new { message = "邮箱已存在" });  // 返回错误请求
            }

            // Create user first with proper password hashing
            var user = new User
            {
                Username = request.Username,
                Password = HashPassword(request.Password),
                Email = request.Email,
                Role = "Student",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create student
            var student = new Student
            {
                UserId = user.Id,
                StudentNumber = request.StudentNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Phone = request.Phone,
                Address = request.Address,
                Major = request.Major,
                Class = request.Class
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var studentDto = new StudentDto
            {
                Id = student.Id,
                StudentNumber = student.StudentNumber,
                FirstName = student.FirstName,
                LastName = student.LastName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Phone = student.Phone,
                Address = student.Address,
                EnrollmentDate = student.EnrollmentDate,
                Major = student.Major,
                Class = student.Class,
                Email = user.Email,
                Username = user.Username
            };

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, studentDto);
        }

        // POST: api/Students/batch
        [HttpPost("batch")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateStudentsBatch([FromBody] List<CreateStudentRequest> requests)
        {
            var results = new List<object>();
            var errors = new List<string>();

            foreach (var request in requests)
            {
                try
                {
                    // Check if student number already exists
                    if (await _context.Students.AnyAsync(s => s.StudentNumber == request.StudentNumber))
                    {
                        errors.Add($"学号 {request.StudentNumber} 已存在");
                        continue;
                    }

                    // Check if username or email already exists
                    if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                    {
                        errors.Add($"用户名 {request.Username} 已存在");
                        continue;
                    }

                    if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    {
                        errors.Add($"邮箱 {request.Email} 已存在");
                        continue;
                    }

                    // Create user
                    var user = new User
                    {
                        Username = request.Username,
                        Password = HashPassword(request.Password),
                        Email = request.Email,
                        Role = "Student",
                        IsActive = true
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create student
                    var student = new Student
                    {
                        UserId = user.Id,
                        StudentNumber = request.StudentNumber,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        DateOfBirth = request.DateOfBirth,
                        Gender = request.Gender,
                        Phone = request.Phone,
                        Address = request.Address,
                        Major = request.Major,
                        Class = request.Class
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    results.Add(new { 
                        success = true, 
                        studentNumber = request.StudentNumber, 
                        message = "创建成功" 
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"学号 {request.StudentNumber}: {ex.Message}");
                }
            }

            return Ok(new { 
                success = errors.Count == 0,
                created = results.Count,
                errors = errors,
                results = results
            });
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, UpdateStudentRequest request)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Check if the current user is authorized to update this student
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole != "Teacher" && currentUserRole != "Admin" && student.UserId != currentUserId)
            {
                return Forbid();
            }

            // Update student properties
            if (request.FirstName != null) student.FirstName = request.FirstName;
            if (request.LastName != null) student.LastName = request.LastName;
            if (request.DateOfBirth.HasValue) student.DateOfBirth = request.DateOfBirth;
            if (request.Gender != null) student.Gender = request.Gender;
            if (request.Phone != null) student.Phone = request.Phone;
            if (request.Address != null) student.Address = request.Address;
            if (request.Major != null) student.Major = request.Major;
            if (request.Class != null) student.Class = request.Class;

            // Update user email if provided
            if (request.Email != null) student.User.Email = request.Email;

            student.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Check if student has active enrollments
            if (student.Enrollments.Any(e => e.Status == "Enrolled"))
            {
                return BadRequest(new { message = "无法删除有活跃选课记录的学生" });
            }

            // Soft delete by setting IsActive to false
            student.User.IsActive = false;
            student.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Students/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetStudentStatistics()
        {
            var totalStudents = await _context.Students.CountAsync();
            var activeStudents = await _context.Students.CountAsync(s => s.User.IsActive);
            var maleStudents = await _context.Students.CountAsync(s => s.Gender == "Male");
            var femaleStudents = await _context.Students.CountAsync(s => s.Gender == "Female");

            var majorStats = await _context.Students
                .Where(s => !string.IsNullOrEmpty(s.Major))
                .GroupBy(s => s.Major)
                .Select(g => new { Major = g.Key, Count = g.Count() })
                .ToListAsync();

            var classStats = await _context.Students
                .Where(s => !string.IsNullOrEmpty(s.Class))
                .GroupBy(s => s.Class)
                .Select(g => new { Class = g.Key, Count = g.Count() })
                .ToListAsync();

            var enrollmentYearStats = await _context.Students
                .GroupBy(s => s.EnrollmentDate.Year)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Year)
                .ToListAsync();

            return Ok(new
            {
                totalStudents,
                activeStudents,
                maleStudents,
                femaleStudents,
                majorStats,
                classStats,
                enrollmentYearStats
            });
        }

        // GET: api/Students/majors
        [HttpGet("majors")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult<IEnumerable<string>>> GetMajors()
        {
            var majors = await _context.Students
                .Where(s => !string.IsNullOrEmpty(s.Major))
                .Select(s => s.Major)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            return Ok(majors);
        }

        // GET: api/Students/classes
        [HttpGet("classes")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult<IEnumerable<string>>> GetClasses()
        {
            var classes = await _context.Students
                .Where(s => !string.IsNullOrEmpty(s.Class))
                .Select(s => s.Class)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(classes);
        }

        // GET: api/Students/5/courses
        [HttpGet("{id}/courses")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentCourses(int id)
        {
            var student = await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Check if the current user is authorized to view this student's courses
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole != "Teacher" && currentUserRole != "Admin" && student.UserId != currentUserId)
            {
                return Forbid();
            }

            var courses = student.Enrollments
                .Select(e => new
                {
                    e.Id,
                    CourseId = e.CourseId,
                    CourseName = e.Course.CourseName,
                    CourseCode = e.Course.CourseCode,
                    TeacherName = e.Course.Teacher != null ? e.Course.Teacher.FirstName + " " + e.Course.Teacher.LastName : null,
                    e.Status,
                    e.EnrollmentDate,
                    e.Grade,
                    Credits = e.Course.Credits
                })
                .OrderByDescending(c => c.EnrollmentDate)
                .ToList();

            return Ok(courses);
        }

        // GET: api/Students/my-grades
        [HttpGet("my-grades")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetMyGrades()
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            var grades = await _context.Grades
                .Include(g => g.Course)
                .Include(g => g.Assignment)
                .Where(g => g.StudentId == student.Id)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = student.FirstName + " " + student.LastName,
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

        // GET: api/Students/my-assignments
        [HttpGet("my-assignments")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyAssignments()
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions)
                .Where(a => a.Course.Enrollments.Any(e => e.StudentId == student.Id && e.Status == "Enrolled"))
                .Select(a => new
                {
                    Id = a.Id,
                    CourseId = a.CourseId,
                    CourseName = a.Course.CourseName,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    MaxScore = a.MaxScore,
                    Weight = a.Weight,
                    SubmissionStatus = a.Submissions.Any(s => s.StudentId == student.Id) ? "已提交" : "未提交",
                    SubmissionDate = a.Submissions.Where(s => s.StudentId == student.Id).Select(s => (DateTime?)s.SubmissionDate).FirstOrDefault(),
                    Score = a.Submissions.Where(s => s.StudentId == student.Id).Select(s => s.Score).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(assignments);
        }

        // GET: api/Students/my-students
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
        public async Task<ActionResult<IEnumerable<object>>> GetStudentUsers()
        {
            var students = await _context.Students.Include(s => s.User).ToListAsync();
            var users = students.Select(s => new {
                id = s.User.Id,
                username = s.User.Username,
                email = s.User.Email,
                role = s.User.Role
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