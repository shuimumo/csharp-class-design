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
    /// 作业提交管理控制器 - 处理所有与作业提交相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/assignment-submissions
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class AssignmentSubmissionsController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public AssignmentSubmissionsController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取作业提交列表 - GET请求
        /// 路径：GET /api/assignment-submissions
        /// [Authorize(Roles = "Teacher,Admin")] 表示只有教师和管理员角色才能访问
        /// 支持按作业ID和学生ID筛选
        /// </summary>
        /// <param name="assignmentId">作业ID筛选条件（可选）</param>
        /// <param name="studentId">学生ID筛选条件（可选）</param>
        /// <returns>返回作业提交列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        [Authorize(Roles = "Teacher,Admin")]  // 只允许教师和管理员角色访问
        public async Task<ActionResult<IEnumerable<object>>> GetSubmissions([FromQuery] int? assignmentId = null, [FromQuery] int? studentId = null)  // 异步方法，从查询字符串获取参数
        {
            // 创建查询对象，包含学生、作业和课程信息
            var query = _context.AssignmentSubmissions  // 创建AssignmentSubmissions表的查询对象
                .Include(s => s.Student)  // 同时加载关联的学生信息
                .Include(s => s.Assignment)  // 同时加载关联的作业信息
                .Include(s => s.Assignment.Course)  // 同时加载关联的课程信息
                .AsQueryable();  // 转换为可查询对象

            // 如果提供了作业ID筛选条件
            if (assignmentId.HasValue)  // 检查作业ID是否有效
            {
                query = query.Where(s => s.AssignmentId == assignmentId.Value);  // 添加作业ID筛选条件
            }

            // 如果提供了学生ID筛选条件
            if (studentId.HasValue)  // 检查学生ID是否有效
            {
                query = query.Where(s => s.StudentId == studentId.Value);  // 添加学生ID筛选条件
            }

            // 执行查询并将结果转换为匿名对象
            var submissions = await query  // 异步执行查询
                .Select(s => new  // 将查询结果投影为匿名对象
                {
                    s.Id,  // 提交ID
                    StudentId = s.StudentId,  // 学生ID
                    StudentName = s.Student.FirstName + " " + s.Student.LastName,  // 学生姓名（组合名字和姓氏）
                    StudentNumber = s.Student.StudentNumber,  // 学生学号
                    AssignmentId = s.AssignmentId,  // 作业ID
                    AssignmentTitle = s.Assignment.Title,  // 作业标题
                    CourseName = s.Assignment.Course.CourseName,  // 课程名称
                    s.Content,  // 提交内容
                    s.SubmissionDate,  // 提交日期
                    s.Score,  // 分数
                    s.Comments,  // 评语
                    s.Status  // 状态
                })
                .OrderByDescending(s => s.SubmissionDate)  // 按提交日期降序排列
                .ToListAsync();  // 异步执行查询并转换为列表

            return Ok(submissions);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个作业提交信息 - GET请求
        /// 路径：GET /api/assignment-submissions/{id}
        /// 根据提交ID获取特定作业提交的详细信息
        /// 权限控制：教师和管理员可以查看任何提交，学生只能查看自己的提交
        /// </summary>
        /// <param name="id">提交ID</param>
        /// <returns>返回作业提交信息，如果没找到则返回404，如果权限不足则返回403</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        public async Task<ActionResult<object>> GetSubmission(int id)  // 异步方法，接收提交ID参数
        {
            // 根据ID查找作业提交，同时加载学生、作业和课程信息
            var submission = await _context.AssignmentSubmissions  // 异步查询AssignmentSubmissions表
                .Include(s => s.Student)  // 同时加载关联的学生信息
                .Include(s => s.Assignment)  // 同时加载关联的作业信息
                .Include(s => s.Assignment.Course)  // 同时加载关联的课程信息
                .FirstOrDefaultAsync(s => s.Id == id);  // 查找指定ID的提交

            if (submission == null)  // 检查提交是否找到
            {
                return NotFound();  // 返回未找到错误
            }

            // 检查权限：教师和管理员可以查看任何提交，学生只能查看自己的提交
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");  // 从JWT令牌中获取当前用户ID
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 从JWT令牌中获取当前用户角色

            // 权限检查：教师和管理员可以查看任何提交，学生只能查看自己的提交
            if (currentUserRole != "Teacher" && currentUserRole != "Admin" && submission.Student.UserId != currentUserId)  // 检查权限
            {
                return Forbid();  // 返回禁止访问错误
            }

            return Ok(new  // 返回成功响应
            {
                submission.Id,  // 提交ID
                StudentId = submission.StudentId,  // 学生ID
                StudentName = submission.Student.FirstName + " " + submission.Student.LastName,  // 学生姓名（组合名字和姓氏）
                StudentNumber = submission.Student.StudentNumber,  // 学生学号
                AssignmentId = submission.AssignmentId,  // 作业ID
                AssignmentTitle = submission.Assignment.Title,  // 作业标题
                CourseName = submission.Assignment.Course.CourseName,  // 课程名称
                submission.Content,  // 提交内容
                submission.SubmissionDate,  // 提交日期
                submission.Score,  // 分数
                submission.Comments,  // 评语
                submission.Status  // 状态
            });
        }

        // POST: api/assignment-submissions
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<object>> CreateSubmission([FromBody] CreateSubmissionRequest request)
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

            // 检查作业是否存在
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == request.AssignmentId);

            if (assignment == null)
            {
                return NotFound(new { message = "Assignment not found" });
            }

            // 检查学生是否已选该课程
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == assignment.CourseId && e.Status == "Enrolled");

            if (enrollment == null)
            {
                return BadRequest(new { message = "您未选择该课程" });
            }

            // 检查是否已提交
            var existingSubmission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.StudentId == student.Id && s.AssignmentId == request.AssignmentId);

            if (existingSubmission != null)
            {
                return BadRequest(new { message = "您已经提交过该作业" });
            }

            // 检查是否超过截止日期
            if (assignment.DueDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "作业已超过截止日期" });
            }

            var submission = new AssignmentSubmission
            {
                StudentId = student.Id,
                AssignmentId = request.AssignmentId,
                Content = request.Content,
                SubmissionDate = DateTime.UtcNow,
                Status = "Submitted"
            };

            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, new
            {
                submission.Id,
                StudentId = submission.StudentId,
                AssignmentId = submission.AssignmentId,
                submission.Content,
                submission.SubmissionDate,
                submission.Status
            });
        }

        // PUT: api/assignment-submissions/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateSubmission(int id, [FromBody] UpdateSubmissionRequest request)
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

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id && s.StudentId == student.Id);

            if (submission == null)
            {
                return NotFound();
            }

            // 检查是否超过截止日期
            if (submission.Assignment.DueDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "作业已超过截止日期，无法修改" });
            }

            if (request.Content != null)
            {
                submission.Content = request.Content;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/assignment-submissions/5/grade
        [HttpPut("{id}/grade")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GradeSubmission(int id, [FromBody] GradeSubmissionRequest request)
        {
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
            {
                return NotFound();
            }

            // 检查权限：只有该课程的教师可以评分
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }

            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (currentUserRole == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);
                if (teacher == null || submission.Assignment.Course.TeacherId != teacher.Id)
                {
                    return Forbid();
                }
            }

            if (request.Score.HasValue)
            {
                submission.Score = request.Score.Value;
            }

            if (request.Comments != null)
            {
                submission.Comments = request.Comments;
            }

            submission.Status = "Graded";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/assignment-submissions/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteSubmission(int id)
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

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id && s.StudentId == student.Id);

            if (submission == null)
            {
                return NotFound();
            }

            // 检查是否超过截止日期
            if (submission.Assignment.DueDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "作业已超过截止日期，无法删除" });
            }

            _context.AssignmentSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/assignment-submissions/my-submissions
        [HttpGet("my-submissions")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<object>>> GetMySubmissions()
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

            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Include(s => s.Assignment.Course)
                .Where(s => s.StudentId == student.Id)
                .Select(s => new
                {
                    s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    CourseName = s.Assignment.Course.CourseName,
                    s.Content,
                    s.SubmissionDate,
                    s.Score,
                    s.Comments,
                    s.Status,
                    DueDate = s.Assignment.DueDate,
                    MaxScore = s.Assignment.MaxScore
                })
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();

            return Ok(submissions);
        }

        // GET: api/assignment-submissions/assignment/5
        [HttpGet("assignment/{assignmentId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetAssignmentSubmissions(int assignmentId)
        {
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Where(s => s.AssignmentId == assignmentId)
                .Select(s => new
                {
                    s.Id,
                    StudentId = s.StudentId,
                    StudentName = s.Student.FirstName + " " + s.Student.LastName,
                    StudentNumber = s.Student.StudentNumber,
                    s.Content,
                    s.SubmissionDate,
                    s.Score,
                    s.Comments,
                    s.Status,
                    DueDate = s.Assignment.DueDate,
                    MaxScore = s.Assignment.MaxScore,
                    IsLate = s.SubmissionDate > s.Assignment.DueDate
                })
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            return Ok(submissions);
        }

        // GET: api/assignment-submissions/my-submission/{assignmentId}
        [HttpGet("my-submission/{assignmentId}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<object>> GetMySubmission(int assignmentId)
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

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Include(s => s.Assignment.Course)
                .FirstOrDefaultAsync(s => s.StudentId == student.Id && s.AssignmentId == assignmentId);

            if (submission == null)
            {
                return NotFound(new { message = "Submission not found" });
            }

            return Ok(new
            {
                submission.Id,
                StudentId = submission.StudentId,
                AssignmentId = submission.AssignmentId,
                AssignmentTitle = submission.Assignment.Title,
                CourseName = submission.Assignment.Course.CourseName,
                submission.Content,
                submission.SubmissionDate,
                submission.Score,
                submission.Comments,
                submission.Status
            });
        }
    }

    public class CreateSubmissionRequest
    {
        public int AssignmentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateSubmissionRequest
    {
        public string? Content { get; set; }
    }

    public class GradeSubmissionRequest
    {
        public decimal? Score { get; set; }
        public string? Comments { get; set; }
    }
} 