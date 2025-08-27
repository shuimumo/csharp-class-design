// 引入授权相关的命名空间，用于控制API访问权限
using Microsoft.AspNetCore.Authorization;
// 引入ASP.NET Core MVC的命名空间，用于创建Web API控制器
using Microsoft.AspNetCore.Mvc;
// 引入Entity Framework Core的命名空间，用于数据库操作
using Microsoft.EntityFrameworkCore;
// 引入我们项目中的数据库上下文，用于访问数据库
using StudentManagement.Infrastructure.Data;
// 引入LINQ查询相关的命名空间
using System.Linq;
// 引入异步编程相关的命名空间
using System.Threading.Tasks;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.API.Controllers
{
    /// <summary>
    /// 仪表板控制器 - 处理所有与仪表板相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/dashboard
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class DashboardController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public DashboardController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取仪表板概览信息 - GET请求
        /// 路径：GET /api/dashboard/overview
        /// 根据用户角色返回不同的统计信息：
        /// - 管理员：全局统计信息
        /// - 教师：只统计自己相关的信息
        /// - 学生：只统计自己相关的信息
        /// </summary>
        /// <returns>返回根据用户角色定制的统计信息，如果用户信息无效则返回401错误</returns>
        [HttpGet("overview")]  // 标识这个方法处理GET请求，路径是overview
        public async Task<ActionResult<object>> GetOverview()  // 异步方法，返回概览信息
        {
            // 从JWT令牌中获取当前用户ID和角色
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 从JWT令牌中获取当前用户ID
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 从JWT令牌中获取当前用户角色

            // 检查用户ID是否有效
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))  // 检查用户ID是否有效
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 根据用户角色返回不同的统计信息
            if (currentUserRole == "Admin")  // 如果是管理员角色
            {
                // 管理员：全局统计
                var courseCount = await _context.Courses.CountAsync();  // 统计课程总数
                var studentCount = await _context.Students.CountAsync();  // 统计学生总数
                var teacherCount = await _context.Teachers.CountAsync();  // 统计教师总数
                var assignmentCount = await _context.Assignments.CountAsync();  // 统计作业总数
                var gradeCount = await _context.Grades.CountAsync();  // 统计成绩总数
                var avgScore = await _context.Grades.AnyAsync() ? await _context.Grades.AverageAsync(g => g.Score) : 0;  // 计算平均分数（如果有成绩的话）
                var gradeDist = await _context.Grades  // 统计成绩分布
                    .GroupBy(g => g.Score >= 90 ? "90+" : g.Score >= 80 ? "80-89" : g.Score >= 70 ? "70-79" : g.Score >= 60 ? "60-69" : "<60")  // 按分数段分组
                    .Select(g => new { Range = g.Key, Count = g.Count() })  // 选择分数段和数量
                    .ToListAsync();  // 异步执行查询并转换为列表
                return Ok(new  // 返回成功响应
                {
                    courseCount,  // 课程数量
                    studentCount,  // 学生数量
                    teacherCount,  // 教师数量
                    assignmentCount,  // 作业数量
                    gradeCount,  // 成绩数量
                    avgScore,  // 平均分数
                    gradeDist  // 成绩分布
                });
            }
            else if (currentUserRole == "Teacher")  // 如果是教师角色
            {
                // 老师：只统计自己相关
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);  // 根据用户ID查找教师
                if (teacher == null) return NotFound(new { message = "Teacher not found" });  // 如果教师不存在，返回404错误
                var courseCount = await _context.Courses.CountAsync(c => c.TeacherId == teacher.Id);  // 统计该教师的课程数量
                var studentCount = await _context.Courses  // 统计该教师的学生数量
                    .Where(c => c.TeacherId == teacher.Id)  // 筛选该教师的课程
                    .SelectMany(c => c.Enrollments)  // 选择所有注册信息
                    .Select(e => e.StudentId)  // 选择学生ID
                    .Distinct()  // 去重
                    .CountAsync();  // 统计数量
                var assignmentCount = await _context.Assignments.CountAsync(a => a.Course.TeacherId == teacher.Id);  // 统计该教师的作业数量
                var gradeCount = await _context.Grades.CountAsync(g => g.Course.TeacherId == teacher.Id);  // 统计该教师的成绩数量
                var avgScore = await _context.Grades.Where(g => g.Course.TeacherId == teacher.Id).AnyAsync() ? await _context.Grades.Where(g => g.Course.TeacherId == teacher.Id).AverageAsync(g => g.Score) : 0;  // 计算该教师的平均分数
                var gradeDist = await _context.Grades.Where(g => g.Course.TeacherId == teacher.Id)  // 统计该教师的成绩分布
                    .GroupBy(g => g.Score >= 90 ? "90+" : g.Score >= 80 ? "80-89" : g.Score >= 70 ? "70-79" : g.Score >= 60 ? "60-69" : "<60")  // 按分数段分组
                    .Select(g => new { Range = g.Key, Count = g.Count() })  // 选择分数段和数量
                    .ToListAsync();  // 异步执行查询并转换为列表
                return Ok(new  // 返回成功响应
                {
                    courseCount,  // 课程数量
                    studentCount,  // 学生数量
                    assignmentCount,  // 作业数量
                    gradeCount,  // 成绩数量
                    avgScore,  // 平均分数
                    gradeDist  // 成绩分布
                });
            }
            else if (currentUserRole == "Student")  // 如果是学生角色
            {
                // 学生：只统计自己相关
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);  // 根据用户ID查找学生
                if (student == null) return NotFound(new { message = "Student not found" });  // 如果学生不存在，返回404错误
                var courseCount = await _context.Enrollments.CountAsync(e => e.StudentId == student.Id && e.Status == "Enrolled");  // 统计该学生的已注册课程数量
                var assignmentCount = await _context.Assignments.CountAsync(a => a.Course.Enrollments.Any(e => e.StudentId == student.Id && e.Status == "Enrolled"));  // 统计该学生的作业数量
                var gradeCount = await _context.Grades.CountAsync(g => g.StudentId == student.Id);  // 统计该学生的成绩数量
                var avgScore = await _context.Grades.Where(g => g.StudentId == student.Id).AnyAsync() ? await _context.Grades.Where(g => g.StudentId == student.Id).AverageAsync(g => g.Score) : 0;  // 计算该学生的平均分数
                var gradeDist = await _context.Grades.Where(g => g.StudentId == student.Id)  // 统计该学生的成绩分布
                    .GroupBy(g => g.Score >= 90 ? "90+" : g.Score >= 80 ? "80-89" : g.Score >= 70 ? "70-79" : g.Score >= 60 ? "60-69" : "<60")  // 按分数段分组
                    .Select(g => new { Range = g.Key, Count = g.Count() })  // 选择分数段和数量
                    .ToListAsync();  // 异步执行查询并转换为列表
                return Ok(new  // 返回成功响应
                {
                    courseCount,  // 课程数量
                    assignmentCount,  // 作业数量
                    gradeCount,  // 成绩数量
                    avgScore,  // 平均分数
                    gradeDist  // 成绩分布
                });
            }
            else  // 如果是其他角色
            {
                return Forbid();  // 返回禁止访问错误
            }
        }
    }
} 