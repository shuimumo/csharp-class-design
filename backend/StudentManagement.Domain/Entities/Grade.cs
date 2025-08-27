// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 成绩实体类 - 表示学生的成绩信息
    /// 这个类会被Entity Framework Core映射到数据库的Grades表
    /// 成绩记录学生在课程或作业中的表现，是教学评价的重要依据
    /// </summary>
    public class Grade
    {
        /// <summary>
        /// 成绩记录的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 学生ID（外键）
        /// 通过这个字段可以知道是哪个学生的成绩
        /// 与Student表形成多对一关系
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// 课程ID（外键）
        /// 通过这个字段可以知道成绩属于哪门课程
        /// 与Course表形成多对一关系
        /// </summary>
        public int CourseId { get; set; }
        
        /// <summary>
        /// 作业ID（外键，可选）
        /// int? 表示这个字段可以为null（可选字段）
        /// 如果成绩是针对特定作业的，这个字段会指向具体的作业
        /// 如果为null，表示这是课程的总成绩
        /// </summary>
        public int? AssignmentId { get; set; }
        
        /// <summary>
        /// 成绩分数
        /// decimal类型用于存储精确的小数值，适合存储分数
        /// 用于存储学生的具体得分，如：85.5分
        /// </summary>
        public decimal Score { get; set; }
        
        /// <summary>
        /// 成绩等级
        /// [StringLength(2)] 表示等级最大长度不能超过2个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 常见的等级值：A（优秀）、B（良好）、C（中等）、D（及格）、F（不及格）
        /// 用于快速了解学生的成绩水平
        /// </summary>
        [StringLength(2)]
        public string? GradeLetter { get; set; }
        
        /// <summary>
        /// 成绩评语
        /// [StringLength(500)] 表示评语内容最大长度不能超过500个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 用于存储教师对成绩的详细评语和建议
        /// 帮助学生了解自己的学习情况和改进方向
        /// </summary>
        [StringLength(500)]
        public string? Comments { get; set; }
        
        /// <summary>
        /// 评分教师ID（外键，可选）
        /// int? 表示这个字段可以为null（可选字段）
        /// 通过这个字段可以知道是哪个教师评的分
        /// 如果为null，表示成绩还没有被评分
        /// </summary>
        public int? GradedBy { get; set; }
        
        /// <summary>
        /// 评分时间
        /// DateTime类型用于存储日期和时间信息
        /// = DateTime.UtcNow 设置默认值为当前时间
        /// 用于记录教师是什么时候评分的
        /// </summary>
        public DateTime GradedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 成绩记录创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录成绩记录是什么时候创建的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 成绩记录最后更新时间
        /// 每次修改成绩记录时，这个字段会自动更新
        /// 用于跟踪成绩记录的变更历史
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 导航属性 - 表示成绩所属的学生
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个多对一的关系：多个成绩记录对应一个学生
        /// </summary>
        public virtual Student Student { get; set; } = null!;
        
        /// <summary>
        /// 导航属性 - 表示成绩所属的课程
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个多对一的关系：多个成绩记录对应一门课程
        /// </summary>
        public virtual Course Course { get; set; } = null!;
        
        /// <summary>
        /// 导航属性 - 表示成绩所属的作业（可选）
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// Assignment? 表示这个属性可以为null（成绩可能不是针对特定作业的）
        /// 这是一个多对一的关系：多个成绩记录对应一个作业
        /// </summary>
        public virtual Assignment? Assignment { get; set; }
        
        /// <summary>
        /// 导航属性 - 表示评分的教师（可选）
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// Teacher? 表示这个属性可以为null（成绩可能还没有被评分）
        /// 这是一个多对一的关系：多个成绩记录对应一个教师
        /// </summary>
        public virtual Teacher? GradedByTeacher { get; set; }
    }
} 