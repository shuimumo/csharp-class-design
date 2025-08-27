// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 学生实体类 - 表示系统中的学生信息
    /// 这个类会被Entity Framework Core映射到数据库的Students表
    /// 学生是系统中的核心角色，可以选课、提交作业、查看成绩等
    /// </summary>
    public class Student
    {
        /// <summary>
        /// 学生的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 关联的用户ID（外键）
        /// 通过这个字段可以知道这个学生档案属于哪个用户账户
        /// 与User表形成一对一关系
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 学生学号
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(20)] 表示学号最大长度不能超过20个字符
        /// 学号是学生的唯一标识符，通常由学校分配，用于学籍管理
        /// </summary>
        [Required]
        [StringLength(20)]
        public string StudentNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生名字
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(50)] 表示名字最大长度不能超过50个字符
        /// 用于显示和记录学生的基本信息
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生姓氏
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(50)] 表示姓氏最大长度不能超过50个字符
        /// 与名字一起构成学生的完整姓名
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// 学生出生日期
        /// DateTime? 表示这个字段可以为null（可选字段）
        /// 用于记录学生的年龄信息，可能用于统计或显示
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
        
        /// <summary>
        /// 学生性别
        /// [StringLength(10)] 表示性别最大长度不能超过10个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 常见的值：Male（男）、Female（女）、Other（其他）
        /// </summary>
        [StringLength(10)]
        public string? Gender { get; set; }
        
        /// <summary>
        /// 学生电话号码
        /// [StringLength(20)] 表示电话号码最大长度不能超过20个字符
        /// 用于联系学生，可能用于紧急情况或重要通知
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }
        
        /// <summary>
        /// 学生地址
        /// [StringLength(200)] 表示地址最大长度不能超过200个字符
        /// 用于记录学生的联系地址，可能用于邮寄或统计
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; }
        
        /// <summary>
        /// 学生入学日期
        /// DateTime类型用于存储日期和时间信息
        /// = DateTime.UtcNow 设置默认值为当前时间
        /// 用于计算学生在校时间和统计
        /// </summary>
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 学生专业
        /// [StringLength(100)] 表示专业名称最大长度不能超过100个字符
        /// 如：计算机科学与技术、软件工程、数据科学等
        /// 用于课程分配、统计分析和学籍管理
        /// </summary>
        [StringLength(100)]
        public string? Major { get; set; }
        
        /// <summary>
        /// 学生班级
        /// [StringLength(50)] 表示班级名称最大长度不能超过50个字符
        /// 如：2023级1班、计算机2班等
        /// 用于组织管理、课程安排和统计
        /// </summary>
        [StringLength(50)]
        public string? Class { get; set; }
        
        /// <summary>
        /// 学生信息创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录学生档案是什么时候创建的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 学生信息最后更新时间
        /// 每次修改学生信息时，这个字段会自动更新
        /// 用于跟踪学生信息的变更历史
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 导航属性 - 表示这个学生档案属于哪个用户账户
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个一对一的关系：一个学生对应一个用户账户
        /// </summary>
        public virtual User User { get; set; } = null!;
        
        /// <summary>
        /// 导航属性 - 表示这个学生的所有选课记录
        /// ICollection<Enrollment> 表示一个集合，可以包含多个选课记录
        /// 一个学生可以选修多门课程
        /// new List<Enrollment>() 初始化一个空列表
        /// 这是一个一对多的关系：一个学生对应多个选课记录
        /// </summary>
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        
        /// <summary>
        /// 导航属性 - 表示这个学生提交的所有作业
        /// 一个学生可以提交多个作业
        /// 用于跟踪学生的作业完成情况和统计
        /// </summary>
        public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
        
        /// <summary>
        /// 导航属性 - 表示这个学生的所有成绩
        /// 一个学生可以有多门课程的成绩
        /// 用于计算学生的平均分、GPA等统计信息
        /// </summary>
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        
        /// <summary>
        /// 导航属性 - 表示这个学生的所有考勤记录
        /// 一个学生可以有多条考勤记录（每门课程每次上课）
        /// 用于统计学生的出勤率和考勤情况
        /// </summary>
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
} 