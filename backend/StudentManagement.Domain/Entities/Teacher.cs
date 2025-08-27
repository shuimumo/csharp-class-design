// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 教师实体类 - 表示系统中的教师信息
    /// 这个类会被Entity Framework Core映射到数据库的Teachers表
    /// 教师是系统中的重要角色，负责教授课程、评分和记录考勤
    /// </summary>
    public class Teacher
    {
        /// <summary>
        /// 教师的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 关联的用户ID（外键）
        /// 通过这个字段可以知道这个教师档案属于哪个用户账户
        /// 与User表形成一对一关系
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 教师工号
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(20)] 表示工号最大长度不能超过20个字符
        /// 工号是教师的唯一工作编号，通常由学校分配
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TeacherNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// 教师名字
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(50)] 表示名字最大长度不能超过50个字符
        /// 用于显示和记录教师的基本信息
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// 教师姓氏
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(50)] 表示姓氏最大长度不能超过50个字符
        /// 与名字一起构成教师的完整姓名
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// 教师出生日期
        /// DateTime? 表示这个字段可以为null（可选字段）
        /// 用于记录教师的年龄信息，可能用于统计或显示
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
        
        /// <summary>
        /// 教师性别
        /// [StringLength(10)] 表示性别最大长度不能超过10个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 常见的值：Male（男）、Female（女）、Other（其他）
        /// </summary>
        [StringLength(10)]
        public string? Gender { get; set; }
        
        /// <summary>
        /// 教师电话号码
        /// [StringLength(20)] 表示电话号码最大长度不能超过20个字符
        /// 用于联系教师，可能用于紧急情况或重要通知
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }
        
        /// <summary>
        /// 教师地址
        /// [StringLength(200)] 表示地址最大长度不能超过200个字符
        /// 用于记录教师的联系地址，可能用于邮寄或统计
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; }
        
        /// <summary>
        /// 教师邮箱地址
        /// [EmailAddress] 验证输入的是否为有效的邮箱格式
        /// [StringLength(100)] 表示邮箱最大长度不能超过100个字符
        /// 用于系统通知、密码重置等功能的联系邮箱
        /// </summary>
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        
        /// <summary>
        /// 教师所属部门
        /// [StringLength(100)] 表示部门名称最大长度不能超过100个字符
        /// 如：计算机系、数学系、物理系等
        /// 用于组织架构管理和课程分配
        /// </summary>
        [StringLength(100)]
        public string? Department { get; set; }
        
        /// <summary>
        /// 教师职称
        /// [StringLength(50)] 表示职称最大长度不能超过50个字符
        /// 如：教授、副教授、讲师、助教等
        /// 用于显示教师资历和权限管理
        /// </summary>
        [StringLength(50)]
        public string? Title { get; set; }
        
        /// <summary>
        /// 教师入职日期
        /// DateTime类型用于存储日期和时间信息
        /// = DateTime.UtcNow 设置默认值为当前时间
        /// 用于计算教师工作年限和统计
        /// </summary>
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 教师信息创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录教师档案是什么时候创建的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 教师信息最后更新时间
        /// 每次修改教师信息时，这个字段会自动更新
        /// 用于跟踪教师信息的变更历史
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 导航属性 - 表示这个教师档案属于哪个用户账户
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个一对一的关系：一个教师对应一个用户账户
        /// </summary>
        public virtual User User { get; set; } = null!;
        
        /// <summary>
        /// 导航属性 - 表示这个教师教授的所有课程
        /// ICollection<Course> 表示一个集合，可以包含多个课程
        /// 一个教师可以教授多门课程
        /// new List<Course>() 初始化一个空列表
        /// 这是一个一对多的关系：一个教师对应多个课程
        /// </summary>
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        
        /// <summary>
        /// 导航属性 - 表示这个教师评分的所有成绩
        /// 一个教师可以给多个学生的作业或考试评分
        /// 用于跟踪教师的评分工作和统计
        /// </summary>
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        
        /// <summary>
        /// 导航属性 - 表示这个教师记录的所有考勤
        /// 一个教师可以记录多个学生的考勤情况
        /// 用于跟踪教师的考勤记录工作和统计
        /// </summary>
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
} 