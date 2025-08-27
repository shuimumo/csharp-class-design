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

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.API.Controllers
{
    /// <summary>
    /// 通知管理控制器 - 处理所有与通知相关的API请求
    /// [ApiController] 表示这是一个Web API控制器
    /// [Route("api/[controller]")] 定义API路由，[controller]会自动替换为控制器名称（去掉Controller后缀）
    /// 所以这个控制器的路由是：/api/notifications
    /// [Authorize] 表示这个控制器的所有方法都需要身份验证
    /// </summary>
    [ApiController]  // 标识这是一个API控制器，自动处理模型验证和错误响应
    [Route("api/[controller]")]  // 定义路由模板，[controller]会被替换为控制器名称（去掉Controller后缀）
    [Authorize]  // 要求所有方法都需要身份验证
    public class NotificationsController : ControllerBase  // 继承ControllerBase，提供基本的控制器功能
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
        public NotificationsController(ApplicationDbContext context)  // 构造函数，接收依赖注入的参数
        {
            // 将注入的数据库上下文保存到私有字段中
            _context = context;  // 保存数据库上下文
        }

        /// <summary>
        /// 获取通知列表 - GET请求
        /// 路径：GET /api/notifications
        /// [Authorize(Roles = "Admin")] 表示只有管理员角色才能访问
        /// 返回所有通知信息，按创建时间降序排列
        /// </summary>
        /// <returns>返回通知列表，如果没找到则返回空列表</returns>
        [HttpGet]  // 标识这个方法处理GET请求
        [Authorize(Roles = "Admin")]  // 只允许管理员角色访问
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()  // 异步方法，返回通知列表
        {
            // 查询所有通知，同时加载目标用户信息，按创建时间降序排列
            var notifications = await _context.Notifications  // 异步查询Notifications表
                .Include(n => n.TargetUser)  // 同时加载关联的目标用户信息
                .OrderByDescending(n => n.CreatedAt)  // 按创建时间降序排列
                .Select(n => new NotificationDto  // 将查询结果投影为NotificationDto对象
                {
                    Id = n.Id,  // 通知ID
                    Title = n.Title,  // 通知标题
                    Content = n.Content,  // 通知内容
                    Type = n.Type,  // 通知类型
                    TargetUserId = n.TargetUserId,  // 目标用户ID
                    TargetRole = n.TargetRole,  // 目标角色
                    IsRead = n.IsRead,  // 是否已读
                    CreatedAt = n.CreatedAt,  // 创建时间
                    TargetUserName = n.TargetUser != null ? n.TargetUser.Username : null  // 目标用户名（如果目标用户存在）
                })
                .ToListAsync();  // 异步执行查询并转换为列表
            return Ok(notifications);  // 返回成功响应
        }

        /// <summary>
        /// 获取单个通知信息 - GET请求
        /// 路径：GET /api/notifications/{id}
        /// 根据通知ID获取特定通知的详细信息
        /// 权限控制：只有管理员、目标用户或目标角色可以查看
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>返回通知信息，如果没找到则返回404，如果权限不足则返回403</returns>
        [HttpGet("{id}")]  // 标识这个方法处理GET请求，路径包含id参数
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)  // 异步方法，接收通知ID参数
        {
            // 根据ID查找通知，同时加载目标用户信息
            var notification = await _context.Notifications  // 异步查询Notifications表
                .Include(n => n.TargetUser)  // 同时加载关联的目标用户信息
                .FirstOrDefaultAsync(n => n.Id == id);  // 查找指定ID的通知
                
            if (notification == null) return NotFound();  // 如果通知不存在，返回404错误

            // 检查权限：只有管理员、目标用户或目标角色可以查看
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");  // 从JWT令牌中获取当前用户ID
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 从JWT令牌中获取当前用户角色

            // 权限检查：管理员可以查看所有通知，目标用户可以查看自己的通知，目标角色可以查看发给该角色的通知
            if (currentUserRole != "Admin" &&   // 如果不是管理员
                notification.TargetUserId != currentUserId &&   // 且不是目标用户
                notification.TargetRole != currentUserRole)  // 且不是目标角色
            {
                return Forbid();  // 返回禁止访问错误
            }

            // 创建通知DTO对象，将实体数据转换为传输对象
            var notificationDto = new NotificationDto  // 创建通知DTO对象
            {
                Id = notification.Id,  // 通知ID
                Title = notification.Title,  // 通知标题
                Content = notification.Content,  // 通知内容
                Type = notification.Type,  // 通知类型
                TargetUserId = notification.TargetUserId,  // 目标用户ID
                TargetRole = notification.TargetRole,  // 目标角色
                IsRead = notification.IsRead,  // 是否已读
                CreatedAt = notification.CreatedAt,  // 创建时间
                TargetUserName = notification.TargetUser != null ? notification.TargetUser.Username : null  // 目标用户名（如果目标用户存在）
            };

            return Ok(notificationDto);  // 返回成功响应
        }

        /// <summary>
        /// 创建新通知 - POST请求
        /// 路径：POST /api/notifications
        /// [Authorize(Roles = "Admin,Teacher")] 表示只有管理员和教师角色才能创建通知
        /// </summary>
        /// <param name="request">创建通知请求对象，包含通知信息</param>
        /// <returns>返回创建的通知信息</returns>
        [HttpPost]  // 标识这个方法处理POST请求
        [Authorize(Roles = "Admin,Teacher")]  // 只允许管理员和教师角色访问
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationRequest request)  // 异步方法，从请求体获取参数
        {
            // 创建新通知对象
            var notification = new Notification  // 创建新的通知对象
            {
                Title = request.Title,  // 通知标题
                Content = request.Content,  // 通知内容
                Type = request.Type,  // 通知类型
                TargetUserId = request.TargetUserId,  // 目标用户ID
                TargetRole = request.TargetRole,  // 目标角色
                IsRead = false,  // 设置为未读状态
                CreatedAt = DateTime.UtcNow  // 设置创建时间为当前UTC时间
            };

            _context.Notifications.Add(notification);  // 将新通知添加到数据库上下文
            await _context.SaveChangesAsync();  // 异步保存更改到数据库

            // 创建通知DTO对象，将实体数据转换为传输对象
            var notificationDto = new NotificationDto  // 创建通知DTO对象
            {
                Id = notification.Id,  // 通知ID
                Title = notification.Title,  // 通知标题
                Content = notification.Content,  // 通知内容
                Type = notification.Type,  // 通知类型
                TargetUserId = notification.TargetUserId,  // 目标用户ID
                TargetRole = notification.TargetRole,  // 目标角色
                IsRead = notification.IsRead,  // 是否已读
                CreatedAt = notification.CreatedAt  // 创建时间
            };

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notificationDto);  // 返回创建成功响应
        }

        /// <summary>
        /// 更新通知信息 - PUT请求
        /// 路径：PUT /api/notifications/{id}
        /// [Authorize(Roles = "Admin,Teacher")] 表示只有管理员和教师角色才能更新通知
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <param name="request">更新通知请求对象，包含要更新的信息</param>
        /// <returns>返回204 No Content，表示更新成功</returns>
        [HttpPut("{id}")]  // 标识这个方法处理PUT请求，路径包含id参数
        [Authorize(Roles = "Admin,Teacher")]  // 只允许管理员和教师角色访问
        public async Task<IActionResult> UpdateNotification(int id, [FromBody] UpdateNotificationRequest request)  // 异步方法，接收通知ID和请求体
        {
            // 根据ID查找通知
            var notification = await _context.Notifications.FindAsync(id);  // 异步查找指定ID的通知
            if (notification == null) return NotFound();  // 如果通知不存在，返回404错误

            // 根据请求更新通知属性
            if (request.Title != null) notification.Title = request.Title;  // 更新标题
            if (request.Content != null) notification.Content = request.Content;  // 更新内容
            if (request.Type != null) notification.Type = request.Type;  // 更新类型
            if (request.TargetUserId.HasValue) notification.TargetUserId = request.TargetUserId;  // 更新目标用户ID
            if (request.TargetRole != null) notification.TargetRole = request.TargetRole;  // 更新目标角色
            if (request.IsRead.HasValue) notification.IsRead = request.IsRead.Value;  // 更新是否已读

            await _context.SaveChangesAsync();  // 异步保存更改到数据库
            return NoContent();  // 返回204 No Content
        }

        /// <summary>
        /// 删除通知 - DELETE请求
        /// 路径：DELETE /api/notifications/{id}
        /// [Authorize(Roles = "Admin")] 表示只有管理员角色才能删除通知
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>返回204 No Content，表示删除成功</returns>
        [HttpDelete("{id}")]  // 标识这个方法处理DELETE请求，路径包含id参数
        [Authorize(Roles = "Admin")]  // 只允许管理员角色访问
        public async Task<IActionResult> DeleteNotification(int id)  // 异步方法，接收通知ID参数
        {
            // 根据ID查找通知
            var notification = await _context.Notifications.FindAsync(id);  // 异步查找指定ID的通知
            if (notification == null) return NotFound();  // 如果通知不存在，返回404错误

            _context.Notifications.Remove(notification);  // 从数据库上下文中移除通知
            await _context.SaveChangesAsync();  // 异步保存更改到数据库
            return NoContent();  // 返回204 No Content
        }

        /// <summary>
        /// 获取我的通知列表 - GET请求
        /// 路径：GET /api/notifications/my-notifications
        /// 获取当前登录用户收到的所有通知
        /// 权限控制：只有登录用户才能访问
        /// </summary>
        /// <returns>返回当前用户收到的通知列表</returns>
        [HttpGet("my-notifications")]  // 标识这个方法处理GET请求，路径包含"my-notifications"
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications()  // 异步方法，返回当前用户收到的通知列表
        {
            // 从JWT令牌中获取当前用户ID和角色
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 获取用户ID声明
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 获取用户角色声明

            // 验证用户ID是否有效
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 查询当前用户收到的所有通知，包括目标用户和目标角色的通知，按创建时间降序排列
            var notifications = await _context.Notifications  // 异步查询Notifications表
                .Include(n => n.TargetUser)  // 同时加载关联的目标用户信息
                .Where(n => (n.TargetUserId == currentUserId) || (n.TargetRole == currentUserRole))  // 筛选条件：目标用户是当前用户或目标角色是当前用户角色
                .OrderByDescending(n => n.CreatedAt)  // 按创建时间降序排列
                .Select(n => new NotificationDto  // 将查询结果投影为NotificationDto对象
                {
                    Id = n.Id,  // 通知ID
                    Title = n.Title,  // 通知标题
                    Content = n.Content,  // 通知内容
                    Type = n.Type,  // 通知类型
                    TargetUserId = n.TargetUserId,  // 目标用户ID
                    TargetRole = n.TargetRole,  // 目标角色
                    IsRead = n.IsRead,  // 是否已读
                    CreatedAt = n.CreatedAt,  // 创建时间
                    TargetUserName = n.TargetUser != null ? n.TargetUser.Username : null  // 目标用户名（如果目标用户存在）
                })
                .ToListAsync();  // 异步执行查询并转换为列表

            return Ok(notifications);  // 返回成功响应
        }

        /// <summary>
        /// 标记通知为已读 - PUT请求
        /// 路径：PUT /api/notifications/{id}/mark-read
        /// 权限控制：只有目标用户或目标角色可以标记为已读
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>返回204 No Content，表示标记成功</returns>
        [HttpPut("{id}/mark-read")]  // 标识这个方法处理PUT请求，路径包含id参数，并包含"mark-read"
        public async Task<IActionResult> MarkAsRead(int id)  // 异步方法，接收通知ID参数
        {
            // 从JWT令牌中获取当前用户ID和角色
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 获取用户ID声明
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 获取用户角色声明

            // 验证用户ID是否有效
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 根据ID查找通知
            var notification = await _context.Notifications.FindAsync(id);  // 异步查找指定ID的通知
            if (notification == null) return NotFound();  // 如果通知不存在，返回404错误

            // 检查权限：只有目标用户或目标角色可以标记为已读
            if (notification.TargetUserId != currentUserId && notification.TargetRole != currentUserRole)
            {
                return Forbid();  // 返回禁止访问错误
            }

            notification.IsRead = true;  // 将通知标记为已读
            await _context.SaveChangesAsync();  // 异步保存更改到数据库
            return NoContent();  // 返回204 No Content
        }

        /// <summary>
        /// 获取未读通知数量 - GET请求
        /// 路径：GET /api/notifications/unread-count
        /// 获取当前登录用户未读通知的数量
        /// 权限控制：只有登录用户才能访问
        /// </summary>
        /// <returns>返回未读通知数量</returns>
        [HttpGet("unread-count")]  // 标识这个方法处理GET请求，路径包含"unread-count"
        public async Task<ActionResult<object>> GetUnreadCount()  // 异步方法，返回未读通知数量
        {
            // 从JWT令牌中获取当前用户ID和角色
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 获取用户ID声明
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;  // 获取用户角色声明

            // 验证用户ID是否有效
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 查询当前用户未读通知的数量，包括目标用户和目标角色的通知
            var unreadCount = await _context.Notifications  // 异步查询Notifications表
                .CountAsync(n => !n.IsRead && ((n.TargetUserId == currentUserId) || (n.TargetRole == currentUserRole)));  // 筛选条件：未读且目标用户是当前用户或目标角色是当前用户角色

            return Ok(new { unreadCount });  // 返回成功响应
        }

        /// <summary>
        /// 获取教师发布的通知列表 - GET请求
        /// 路径：GET /api/notifications/teacher-published
        /// [Authorize(Roles = "Teacher")] 表示只有教师角色才能访问
        /// 获取教师发布的所有通知（包括目标角色和特定用户的通知）
        /// </summary>
        /// <returns>返回教师发布的通知列表</returns>
        [HttpGet("teacher-published")]  // 标识这个方法处理GET请求，路径包含"teacher-published"
        [Authorize(Roles = "Teacher")]  // 只允许教师角色访问
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetTeacherPublishedNotifications()  // 异步方法，返回教师发布的通知列表
        {
            // 从JWT令牌中获取当前用户ID
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;  // 获取用户ID声明
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user information" });  // 返回未授权错误
            }

            // 获取教师发布的所有通知（包括目标角色和特定用户的通知）
            var notifications = await _context.Notifications  // 异步查询Notifications表
                .Include(n => n.TargetUser)  // 同时加载关联的目标用户信息
                .Where(n => n.TargetRole == "Student" || n.TargetRole == "Teacher" || n.TargetUserId == currentUserId)  // 筛选条件：目标角色是学生或教师，或目标用户是当前用户
                .OrderByDescending(n => n.CreatedAt)  // 按创建时间降序排列
                .Select(n => new NotificationDto  // 将查询结果投影为NotificationDto对象
                {
                    Id = n.Id,  // 通知ID
                    Title = n.Title,  // 通知标题
                    Content = n.Content,  // 通知内容
                    Type = n.Type,  // 通知类型
                    TargetUserId = n.TargetUserId,  // 目标用户ID
                    TargetRole = n.TargetRole,  // 目标角色
                    IsRead = n.IsRead,  // 是否已读
                    CreatedAt = n.CreatedAt,  // 创建时间
                    TargetUserName = n.TargetUser != null ? n.TargetUser.Username : null  // 目标用户名（如果目标用户存在）
                })
                .ToListAsync();  // 异步执行查询并转换为列表

            return Ok(notifications);  // 返回成功响应
        }
    }
} 