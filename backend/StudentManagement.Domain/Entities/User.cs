// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 用户实体类 - 表示系统中的所有用户（学生、教师、管理员）
    /// 这是一个基础用户类，学生和教师都继承自这个类
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 用户名
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(50)] 表示用户名最大长度不能超过50个字符
        /// = string.Empty 给属性一个默认值，避免null引用异常
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户密码
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(255)] 表示密码最大长度不能超过255个字符
        /// 注意：实际存储时应该是对密码进行哈希加密后的值
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户邮箱地址
        /// [Required] 表示这个字段是必填的，不能为空
        /// [EmailAddress] 验证输入的是否为有效的邮箱格式
        /// [StringLength(100)] 表示邮箱最大长度不能超过100个字符
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户角色
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(20)] 表示角色名称最大长度不能超过20个字符
        /// 常见的角色值：Student（学生）、Teacher（教师）、Admin（管理员）
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录用户是什么时候注册的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 用户最后更新时间
        /// 每次修改用户信息时，这个字段会自动更新
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 用户是否处于活跃状态
        /// bool类型表示真或假
        /// = true 设置默认值为true，表示新用户默认是活跃的
        /// 可以用于软删除，而不是真正从数据库删除用户
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// 导航属性 - 如果这个用户是学生，则指向学生信息
        /// Student? 表示这个属性可以为null（不是所有用户都是学生）
        /// 这是一个一对一的关系：一个用户对应一个学生信息
        /// </summary>
        public virtual Student? Student { get; set; }
        
        /// <summary>
        /// 导航属性 - 如果这个用户是教师，则指向教师信息
        /// Teacher? 表示这个属性可以为null（不是所有用户都是教师）
        /// 这是一个一对一的关系：一个用户对应一个教师信息
        /// </summary>
        public virtual Teacher? Teacher { get; set; }
        
        /// <summary>
        /// 导航属性 - 这个用户收到的所有通知
        /// ICollection<Notification> 表示一个集合，可以包含多个通知
        /// 一个用户可以收到多个通知
        /// new List<Notification>() 初始化一个空列表
        /// </summary>
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
} 