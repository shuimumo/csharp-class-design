// 引入ASP.NET Core MVC的命名空间，用于创建Web API控制器
using Microsoft.AspNetCore.Mvc;
// 引入Entity Framework Core的命名空间，用于数据库操作
using Microsoft.EntityFrameworkCore;
// 引入JWT令牌相关的命名空间，用于生成和验证JWT令牌
using Microsoft.IdentityModel.Tokens;
// 引入我们项目中的DTO类，用于数据传输
using StudentManagement.Core.DTOs;
// 引入我们项目中的实体类，用于数据库操作
using StudentManagement.Domain.Entities;
// 引入我们项目中的数据库上下文，用于访问数据库
using StudentManagement.Infrastructure.Data;
// 引入JWT令牌处理器的命名空间，用于创建JWT令牌
using System.IdentityModel.Tokens.Jwt;
// 引入声明（Claims）的命名空间，用于在JWT中存储用户信息
using System.Security.Claims;
// 引入加密算法的命名空间，用于密码哈希
using System.Security.Cryptography;
// 引入文本编码的命名空间，用于字符串和字节数组的转换
using System.Text;
// 引入授权相关的命名空间，用于控制API访问权限
using Microsoft.AspNetCore.Authorization;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.API.Controllers
{
    /// <summary>
    /// 身份验证控制器
    /// 处理用户登录、注册、密码重置等身份验证相关的功能
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/auth
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    public class AuthController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
    {
        /// <summary>
        /// 数据库上下文，用于访问数据库
        /// readonly 表示这个字段只能在构造函数中赋值，之后不能修改
        /// 通过依赖注入获取
        /// </summary>
        private readonly ApplicationDbContext _context;  // 私有只读字段，存储数据库上下文
        
        /// <summary>
        /// 配置对象，用于读取配置文件中的设置
        /// 比如JWT密钥、数据库连接字符串等
        /// 通过依赖注入获取
        /// </summary>
        private readonly IConfiguration _configuration;  // 私有只读字段，存储配置对象

        /// <summary>
        /// 构造函数，接收依赖注入的服务
        /// ApplicationDbContext 和 IConfiguration 会由.NET的依赖注入容器自动提供
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="configuration">配置对象</param>
        public AuthController(ApplicationDbContext context, IConfiguration configuration)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的服务保存到私有字段中
            _context = context;  // 保存数据库上下文
            _configuration = configuration;  // 保存配置对象
        }

        /// <summary>
        /// 用户登录接口
        /// [HttpPost("login")] 表示这个方法处理POST请求，路径是 /api/auth/login
        /// [AllowAnonymous] 表示这个接口不需要身份验证，任何人都可以访问
        /// 返回类型是 ActionResult<LoginResponse>，表示返回登录响应信息
        /// </summary>
        /// <param name="request">登录请求对象，包含用户名和密码</param>
        /// <returns>登录成功返回用户信息和JWT令牌，失败返回错误信息</returns>
        [HttpPost("login")]  // 标识这个方法处理POST请求，路径是login
        [AllowAnonymous]  // 允许匿名访问，不需要身份验证
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)  // 异步方法，返回登录响应
        {
            // 在控制台输出登录尝试的信息（用于调试）
            Console.WriteLine($"Login attempt: username={request.Username}, password={request.Password}");  // 输出登录尝试信息
            
            // 从数据库中查找用户
            // FirstOrDefaultAsync 异步查找第一个匹配的用户，如果没找到返回null
            // u.Username == request.Username 检查用户名是否匹配
            // && u.IsActive 检查用户是否处于活跃状态
            var user = await _context.Users  // 异步查询Users表
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);  // 查找匹配的用户名且状态为活跃的用户

            // 如果用户不存在或处于非活跃状态
            if (user == null)  // 检查用户是否找到
            {
                Console.WriteLine("User not found or inactive");  // 输出用户未找到的信息
                // 返回401未授权状态码，表示用户名或密码错误
                // 为了安全考虑，不明确说明是用户名错误还是密码错误
                return Unauthorized(new { message = "用户名或密码错误" });  // 返回未授权错误
            }

            // 对输入的密码进行哈希处理
            var inputHash = HashPassword(request.Password);  // 对输入的密码进行哈希处理
            Console.WriteLine($"Input hash: {inputHash}");  // 输出输入密码的哈希值
            Console.WriteLine($"DB hash: {user.Password}");  // 输出数据库中存储的密码哈希值
            
            // 验证密码是否正确
            if (!VerifyPassword(request.Password, user.Password))  // 验证密码是否匹配
            {
                Console.WriteLine("Password mismatch");  // 输出密码不匹配的信息
                // 密码不匹配，返回401未授权状态码
                return Unauthorized(new { message = "用户名或密码错误" });  // 返回未授权错误
            }

            // 生成JWT令牌
            var token = GenerateJwtToken(user);  // 生成JWT令牌

            // 返回登录成功的响应
            // Ok() 表示返回200成功状态码
            return Ok(new LoginResponse  // 返回成功响应
            {
                Token = token,                    // JWT令牌
                Username = user.Username,          // 用户名
                Role = user.Role,                  // 用户角色
                UserId = user.Id,                  // 用户ID
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)  // 令牌过期时间（60分钟后）
            });
        }

        /// <summary>
        /// 用户注册接口
        /// [HttpPost("register")] 表示这个方法处理POST请求，路径是 /api/auth/register
        /// [AllowAnonymous] 表示这个接口不需要身份验证，任何人都可以访问
        /// </summary>
        /// <param name="request">注册请求对象，包含用户信息和角色特定信息</param>
        /// <returns>注册成功返回用户信息和JWT令牌，失败返回错误信息</returns>
        [HttpPost("register")]  // 标识这个方法处理POST请求，路径是register
        [AllowAnonymous]  // 允许匿名访问，不需要身份验证
        public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request)  // 异步方法，返回登录响应
        {
            // 检查用户名是否已经存在
            // AnyAsync 异步检查是否存在满足条件的记录
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))  // 检查用户名是否已存在
            {
                // 返回400错误状态码，表示请求有问题
                return BadRequest(new { message = "用户名已存在" });  // 返回错误请求
            }

            // 检查邮箱是否已经存在
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))  // 检查邮箱是否已存在
            {
                return BadRequest(new { message = "邮箱已存在" });  // 返回错误请求
            }

            // 创建新用户
            var user = new User  // 创建新的用户对象
            {
                Username = request.Username,                    // 用户名
                Password = HashPassword(request.Password),      // 对密码进行哈希处理
                Email = request.Email,                          // 邮箱
                Role = request.Role,                            // 用户角色
                IsActive = true                                 // 设置为活跃状态
            };

            // 将用户添加到数据库上下文
            _context.Users.Add(user);  // 将用户添加到数据库上下文的Users集合中
            // 保存更改到数据库
            await _context.SaveChangesAsync();  // 异步保存更改到数据库

            // 根据用户角色创建相应的学生或教师档案
            if (request.Role == "Student")  // 如果用户角色是学生
            {
                // 创建学生档案
                var student = new Student  // 创建新的学生对象
                {
                    UserId = user.Id,                          // 关联到用户ID
                    StudentNumber = request.StudentNumber ?? "", // 学号，如果为null则设为空字符串
                    FirstName = request.FirstName ?? "",         // 名字
                    LastName = request.LastName ?? "",           // 姓氏
                    Major = request.Major,                      // 专业
                    Class = request.Class                       // 班级
                };
                _context.Students.Add(student);  // 将学生添加到数据库上下文的Students集合中
            }
            else if (request.Role == "Teacher")  // 如果用户角色是教师
            {
                // 创建教师档案
                var teacher = new Teacher  // 创建新的教师对象
                {
                    UserId = user.Id,                          // 关联到用户ID
                    TeacherNumber = request.TeacherNumber ?? "", // 工号
                    FirstName = request.FirstName ?? "",         // 名字
                    LastName = request.LastName ?? "",           // 姓氏
                    Department = request.Department,             // 部门
                    Title = request.Title                       // 职称
                };
                _context.Teachers.Add(teacher);  // 将教师添加到数据库上下文的Teachers集合中
            }

            // 保存学生或教师档案到数据库
            await _context.SaveChangesAsync();  // 异步保存更改到数据库

            // 生成JWT令牌
            var token = GenerateJwtToken(user);  // 生成JWT令牌

            // 返回注册成功的响应
            return Ok(new LoginResponse  // 返回成功响应
            {
                Token = token,  // JWT令牌
                Username = user.Username,  // 用户名
                Role = user.Role,  // 用户角色
                UserId = user.Id,  // 用户ID
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)  // 令牌过期时间（60分钟后）
            });
        }

        /// <summary>
        /// 重置密码接口
        /// [HttpPost("reset-password")] 表示这个方法处理POST请求，路径是 /api/auth/reset-password
        /// [Authorize(Roles = "Admin")] 表示只有管理员角色才能访问这个接口
        /// </summary>
        /// <param name="request">重置密码请求对象，包含用户ID和新密码</param>
        /// <returns>重置成功返回成功消息，失败返回错误信息</returns>
        [HttpPost("reset-password")]  // 标识这个方法处理POST请求，路径是reset-password
        [Authorize(Roles = "Admin")]  // 只允许管理员角色访问
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)  // 异步方法，从请求体获取参数
        {
            // 根据用户ID查找用户
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);  // 根据用户ID查找用户
            if (user == null)  // 检查用户是否找到
            {
                // 用户不存在，返回404错误
                return NotFound(new { message = "用户不存在" });  // 返回未找到错误
            }
            
            // 更新用户密码
            user.Password = HashPassword(request.Password);  // 更新用户密码为新的哈希密码
            user.UpdatedAt = DateTime.UtcNow;  // 更新修改时间为当前UTC时间
            
            // 保存更改到数据库
            await _context.SaveChangesAsync();  // 异步保存更改到数据库
            
            // 返回成功消息
            return Ok(new { message = "密码重置成功" });  // 返回成功消息
        }

        /// <summary>
        /// 生成JWT令牌的私有方法
        /// 根据用户信息创建包含用户身份信息的JWT令牌
        /// </summary>
        /// <param name="user">用户对象，包含用户的基本信息</param>
        /// <returns>JWT令牌字符串</returns>
        private string GenerateJwtToken(User user)  // 私有方法，生成JWT令牌
        {
            // 从配置文件中读取JWT密钥，如果为空则使用默认值
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSecretKeyHere"));  // 创建对称安全密钥
            
            // 创建签名凭据，使用HMAC-SHA256算法
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);  // 创建签名凭据

            // 创建声明数组，包含用户的基本信息
            var claims = new[]  // 创建声明数组
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),    // 用户ID声明
                new Claim(ClaimTypes.Name, user.Username),                   // 用户名声明
                new Claim(ClaimTypes.Email, user.Email),                     // 邮箱声明
                new Claim(ClaimTypes.Role, user.Role)                        // 用户角色声明
            };

            // 创建JWT令牌
            var token = new JwtSecurityToken(  // 创建JWT安全令牌
                issuer: _configuration["Jwt:Issuer"],                        // 发行者
                audience: _configuration["Jwt:Audience"],                    // 受众
                claims: claims,                                              // 声明信息
                expires: DateTime.UtcNow.AddMinutes(60),                     // 过期时间（60分钟后）
                signingCredentials: credentials                              // 签名凭据
            );

            // 将JWT令牌对象转换为字符串并返回
            return new JwtSecurityTokenHandler().WriteToken(token);  // 将令牌对象转换为字符串
        }

        /// <summary>
        /// 对密码进行哈希处理的私有方法
        /// 使用SHA256算法对密码进行单向加密，确保密码安全
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <returns>哈希后的密码字符串</returns>
        private string HashPassword(string password)  // 私有方法，对密码进行哈希处理
        {
            // 使用using语句确保资源被正确释放
            using (var sha256 = SHA256.Create())  // 创建SHA256哈希算法实例
            {
                // 将密码字符串转换为字节数组，然后进行哈希处理
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));  // 计算密码的哈希值
                // 将哈希后的字节数组转换为Base64字符串
                return Convert.ToBase64String(hashedBytes);  // 返回Base64编码的哈希字符串
            }
        }

        /// <summary>
        /// 验证密码的私有方法
        /// 比较输入的密码和数据库中存储的哈希密码是否匹配
        /// </summary>
        /// <param name="password">用户输入的原始密码</param>
        /// <param name="hashedPassword">数据库中存储的哈希密码</param>
        /// <returns>如果密码匹配返回true，否则返回false</returns>
        private bool VerifyPassword(string password, string hashedPassword)  // 私有方法，验证密码
        {
            // 对输入的密码进行哈希处理
            var hashedInput = HashPassword(password);  // 对输入密码进行哈希处理
            // 比较两个哈希值是否相等
            return hashedInput == hashedPassword;  // 返回比较结果
        }
    }

    /// <summary>
    /// 重置密码请求的数据传输对象
    /// 用于接收管理员重置用户密码的请求
    /// </summary>
    public class ResetPasswordRequest  // 重置密码请求类
    {
        /// <summary>
        /// 要重置密码的用户ID
        /// </summary>
        public int UserId { get; set; }  // 用户ID属性
        
        /// <summary>
        /// 新的密码
        /// </summary>
        public string Password { get; set; } = string.Empty;  // 新密码属性，默认为空字符串
    }
} 