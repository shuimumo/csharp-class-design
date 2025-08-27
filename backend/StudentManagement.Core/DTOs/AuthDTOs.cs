// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Core.DTOs
{
    /// <summary>
    /// 登录请求数据传输对象
    /// 用于接收前端发送的用户登录信息
    /// 这个类定义了用户登录时需要提供的数据结构
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 用户名
        /// [Required] 表示这个字段是必填的，不能为空
        /// 用户登录时必须提供用户名
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户密码
        /// [Required] 表示这个字段是必填的，不能为空
        /// 用户登录时必须提供密码
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录响应数据传输对象
    /// 用于向前端返回用户登录成功后的信息
    /// 包含JWT令牌和用户基本信息
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT身份验证令牌
        /// 前端需要保存这个令牌，后续的API请求都要带上它
        /// 用于证明用户已经登录的身份
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户名
        /// 返回给前端显示当前登录的用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户角色
        /// 可以是：Student（学生）、Teacher（教师）、Admin（管理员）
        /// 前端根据角色显示不同的界面和功能
        /// </summary>
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户ID
        /// 用户的唯一标识符，前端可能需要用它来获取更多用户信息
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 令牌过期时间
        /// 告诉前端这个令牌什么时候会失效
        /// 前端可以提前刷新令牌或提示用户重新登录
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// 注册请求数据传输对象
    /// 用于接收前端发送的用户注册信息
    /// 包含用户基本信息和角色特定的信息
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// 用户名
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(50)] 表示用户名最大长度不能超过50个字符
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户密码
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(255)] 表示密码最大长度不能超过255个字符
        /// 注意：实际存储时应该对密码进行哈希加密
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户邮箱地址
        /// [Required] 表示这个字段是必填的，不能为空
        /// [EmailAddress] 验证输入的是否为有效的邮箱格式
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户角色
        /// [Required] 表示这个字段是必填的，不能为空
        /// 决定用户是学生还是教师，影响后续的注册流程
        /// </summary>
        [Required]
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生特定字段 - 学号
        /// string? 表示这个字段可以为null（可选字段）
        /// 只有当Role为"Student"时，这个字段才有意义
        /// </summary>
        public string? StudentNumber { get; set; }
        
        /// <summary>
        /// 学生特定字段 - 名字
        /// 学生的名字，用于显示和记录
        /// </summary>
        public string? FirstName { get; set; }
        
        /// <summary>
        /// 学生特定字段 - 姓氏
        /// 学生的姓氏，用于显示和记录
        /// </summary>
        public string? LastName { get; set; }
        
        /// <summary>
        /// 学生特定字段 - 专业
        /// 学生所学的专业，如：计算机科学、数学等
        /// </summary>
        public string? Major { get; set; }
        
        /// <summary>
        /// 学生特定字段 - 班级
        /// 学生所在的班级，如：2023级1班
        /// </summary>
        public string? Class { get; set; }
        
        /// <summary>
        /// 教师特定字段 - 工号
        /// 只有当Role为"Teacher"时，这个字段才有意义
        /// 教师的唯一工作编号
        /// </summary>
        public string? TeacherNumber { get; set; }
        
        /// <summary>
        /// 教师特定字段 - 所属部门
        /// 教师工作的部门，如：计算机系、数学系等
        /// </summary>
        public string? Department { get; set; }
        
        /// <summary>
        /// 教师特定字段 - 职称
        /// 教师的职称，如：教授、副教授、讲师等
        /// </summary>
        public string? Title { get; set; }
    }
} 