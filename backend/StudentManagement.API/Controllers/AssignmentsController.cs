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
    /// 作业管理控制器 - 处理所有与作业相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/assignments
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class AssignmentsController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public AssignmentsController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取作业列表 - GET请求
        /// 路径：GET /api/assignments
        /// 返回所有作业信息，按截止日期降序排列
        /// </summary>
        /// <returns>返回作业列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        public async Task<ActionResult<IEnumerable<object>>> GetAssignments()  // 异步方法，返回作业列表
        {
            // 查询所有作业，同时加载课程信息，按截止日期降序排列
            var assignments = await _context.Assignments  // 异步查询Assignments表
                .Include(a => a.Course)  // 同时加载关联的课程信息
                .OrderByDescending(a => a.DueDate)  // 按截止日期降序排列
                .Select(a => new {  // 将查询结果投影为匿名对象
                    a.Id,  // 作业ID
                    a.CourseId,  // 课程ID
                    CourseName = a.Course.CourseName,  // 课程名称
                    a.Title,  // 作业标题
                    a.Description,  // 作业描述
                    a.DueDate,  // 截止日期
                    a.MaxScore,  // 最高分数
                    a.Weight,  // 权重
                    a.CreatedAt,  // 创建时间
                    a.UpdatedAt  // 更新时间
                })
                .ToListAsync();  // 异步执行查询并转换为列表
            return Ok(assignments);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个作业信息 - GET请求
        /// 路径：GET /api/assignments/{id}
        /// 根据作业ID获取特定作业的详细信息
        /// </summary>
        /// <param name="id">作业ID</param>
        /// <returns>返回作业信息，如果没找到则返回404</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        public async Task<ActionResult<object>> GetAssignment(int id)  // 异步方法，接收作业ID参数
        {
            // 根据ID查找作业，同时加载课程信息
            var a = await _context.Assignments.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == id);  // 异步查询Assignments表并加载课程信息
            if (a == null) return NotFound();  // 如果作业不存在，返回404错误
            return Ok(new {  // 返回成功响应
                a.Id,  // 作业ID
                a.CourseId,  // 课程ID
                CourseName = a.Course.CourseName,  // 课程名称
                a.Title,  // 作业标题
                a.Description,  // 作业描述
                a.DueDate,  // 截止日期
                a.MaxScore,  // 最高分数
                a.Weight,  // 权重
                a.CreatedAt,  // 创建时间
                a.UpdatedAt  // 更新时间
            });
        }

        /// <summary>
        /// 创建新作业 - POST请求
        /// 路径：POST /api/assignments
        /// [Authorize(Roles = "Admin,Teacher")] 表示只有管理员和教师角色才能创建作业
        /// </summary>
        /// <param name="assignment">作业对象，包含作业信息</param>
        /// <returns>返回创建结果，如果验证失败或权限不足则返回相应错误</returns>
        [HttpPost]  // 标识这个方法处理POST请求
        [Authorize(Roles = "Admin,Teacher")]  // 只允许管理员和教师角色访问
        public async Task<ActionResult> CreateAssignment([FromBody] Assignment assignment)  // 异步方法，从请求体获取参数
        {
            // 检查模型状态是否有效
            if (!ModelState.IsValid)  // 检查模型验证是否通过
            {
                return BadRequest(ModelState);  // 返回错误请求
            }

            // 验证课程是否存在
            var course = await _context.Courses.FindAsync(assignment.CourseId);  // 根据课程ID查找课程
            if (course == null)  // 检查课程是否找到
            {
                return BadRequest(new { message = "课程不存在" });  // 返回错误请求
            }

            // 验证教师是否有权限为该课程创建作业
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);  // 根据用户ID查找教师
            if (teacher == null)  // 检查教师是否找到
            {
                return NotFound(new { message = "Teacher not found" });  // 返回未找到错误
            }
            if (course.TeacherId != teacher.Id)  // 检查教师是否有权限为该课程创建作业
            {
                return StatusCode(403, new { message = "您没有权限为该课程创建作业" });  // 返回禁止访问错误
            }

            // 设置作业的创建时间和更新时间
            assignment.CreatedAt = DateTime.UtcNow;  // 设置创建时间为当前UTC时间
            assignment.UpdatedAt = DateTime.UtcNow;  // 设置更新时间为当前UTC时间
            _context.Assignments.Add(assignment);  // 将作业添加到数据库上下文
            await _context.SaveChangesAsync();  // 异步保存更改到数据库
            return Ok(assignment);  // 返回成功响应
        }

        /// <summary>
        /// 更新作业 - PUT请求
        /// 路径：PUT /api/assignments/{id}
        /// [Authorize(Roles = "Admin,Teacher")] 表示只有管理员和教师角色才能更新作业
        /// </summary>
        /// <param name="id">作业ID</param>
        /// <param name="assignment">包含更新信息的作业对象</param>
        /// <returns>返回更新结果，如果验证失败或权限不足则返回相应错误</returns>
        [HttpPut("{id}")]  // 标识这个方法处理PUT请求，路径包含id参数
        [Authorize(Roles = "Admin,Teacher")]  // 只允许管理员和教师角色访问
        public async Task<ActionResult> UpdateAssignment(int id, [FromBody] Assignment assignment)  // 异步方法，接收id和更新后的作业对象
        {
            // 检查模型状态是否有效
            if (!ModelState.IsValid)  // 检查模型验证是否通过
            {
                return BadRequest(ModelState);  // 返回错误请求
            }

            // 查找要更新的作业，同时加载课程信息
            var existingAssignment = await _context.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);  // 异步查找并加载课程信息
            if (existingAssignment == null) return NotFound();  // 如果作业不存在，返回404错误

            // 验证课程是否存在
            var course = await _context.Courses.FindAsync(assignment.CourseId);  // 根据课程ID查找课程
            if (course == null)  // 检查课程是否找到
            {
                return BadRequest(new { message = "课程不存在" });  // 返回错误请求
            }

            // 验证教师是否有权限修改该作业
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);  // 根据用户ID查找教师
            if (teacher == null)  // 检查教师是否找到
            {
                return NotFound(new { message = "Teacher not found" });  // 返回未找到错误
            }
            if (course.TeacherId != teacher.Id)  // 检查教师是否有权限修改该作业
            {
                return StatusCode(403, new { message = "您没有权限修改该作业" });  // 返回禁止访问错误
            }

            // 更新作业的属性
            existingAssignment.Title = assignment.Title;  // 更新标题
            existingAssignment.Description = assignment.Description;  // 更新描述
            existingAssignment.DueDate = assignment.DueDate;  // 更新截止日期
            existingAssignment.MaxScore = assignment.MaxScore;  // 更新最高分数
            existingAssignment.Weight = assignment.Weight;  // 更新权重
            existingAssignment.UpdatedAt = DateTime.UtcNow;  // 更新更新时间
            existingAssignment.CourseId = assignment.CourseId;  // 更新课程ID
            await _context.SaveChangesAsync();  // 异步保存更改到数据库
            return NoContent();  // 返回204 No Content，表示成功更新
        }

        /// <summary>
        /// 删除作业 - DELETE请求
        /// 路径：DELETE /api/assignments/{id}
        /// [Authorize(Roles = "Admin,Teacher")] 表示只有管理员和教师角色才能删除作业
        /// </summary>
        /// <param name="id">作业ID</param>
        /// <returns>返回删除结果，如果权限不足则返回相应错误</returns>
        [HttpDelete("{id}")]  // 标识这个方法处理DELETE请求，路径包含id参数
        [Authorize(Roles = "Admin,Teacher")]  // 只允许管理员和教师角色访问
        public async Task<ActionResult> DeleteAssignment(int id)  // 异步方法，接收作业ID参数
        {
            // 查找要删除的作业，同时加载课程信息
            var assignment = await _context.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);  // 异步查找并加载课程信息
            if (assignment == null) return NotFound();  // 如果作业不存在，返回404错误

            // 验证教师是否有权限删除该作业
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);  // 根据用户ID查找教师
            if (teacher == null)  // 检查教师是否找到
            {
                return NotFound(new { message = "Teacher not found" });  // 返回未找到错误
            }
            if (assignment.Course.TeacherId != teacher.Id)  // 检查教师是否有权限删除该作业
            {
                return StatusCode(403, new { message = "您没有权限删除该作业" });  // 返回禁止访问错误
            }

            _context.Assignments.Remove(assignment);  // 从数据库上下文中移除作业
            await _context.SaveChangesAsync();  // 异步保存更改到数据库
            return NoContent();  // 返回204 No Content，表示成功删除
        }

        /// <summary>
        /// 获取当前教师创建的作业列表 - GET请求
        /// 路径：GET /api/assignments/my-assignments
        /// [Authorize(Roles = "Teacher")] 表示只有教师角色才能访问
        /// </summary>
        /// <returns>返回当前教师创建的作业列表</returns>
        [HttpGet("my-assignments")]  // 标识这个方法处理GET请求，路径包含"my-assignments"
        [Authorize(Roles = "Teacher")]  // 只允许教师角色访问
        public async Task<ActionResult<IEnumerable<object>>> GetMyAssignments()  // 异步方法，返回当前教师创建的作业列表
        {
            // 从JWT令牌中获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }
            // 根据用户ID查找教师
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }
            // 查询该教师创建的所有作业，按截止日期降序排列
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course.TeacherId == teacher.Id)
                .OrderByDescending(a => a.DueDate)
                .Select(a => new {
                    a.Id,
                    a.CourseId,
                    CourseName = a.Course.CourseName,
                    a.Title,
                    a.Description,
                    a.DueDate,
                    a.MaxScore,
                    a.Weight,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .ToListAsync();
            return Ok(assignments);
        }

        /// <summary>
        /// 获取当前学生选课下的作业列表 - GET请求
        /// 路径：GET /api/assignments/my-student-assignments
        /// [Authorize(Roles = "Student")] 表示只有学生角色才能访问
        /// </summary>
        /// <returns>返回当前学生选课下的作业列表</returns>
        [HttpGet("my-student-assignments")]  // 标识这个方法处理GET请求，路径包含"my-student-assignments"
        [Authorize(Roles = "Student")]  // 只允许学生角色访问
        public async Task<ActionResult<IEnumerable<object>>> GetMyStudentAssignments()  // 异步方法，返回当前学生选课下的作业列表
        {
            // 从JWT令牌中获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });
            }
            // 根据用户ID查找学生
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }
            // 查询该学生选课下的所有作业
            var courseIds = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == "Enrolled")
                .Select(e => e.CourseId)
                .ToListAsync();
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => courseIds.Contains(a.CourseId))
                .OrderByDescending(a => a.DueDate)
                .Select(a => new {
                    a.Id,
                    a.CourseId,
                    CourseName = a.Course.CourseName,
                    a.Title,
                    a.Description,
                    a.DueDate,
                    a.MaxScore,
                    a.Weight,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .ToListAsync();
            return Ok(assignments);
        }
    }
} 