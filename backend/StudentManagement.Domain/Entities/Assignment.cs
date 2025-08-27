// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 作业实体类 - 表示系统中的作业信息
    /// 这个类会被Entity Framework Core映射到数据库的Assignments表
    /// </summary>
    public class Assignment
    {
        /// <summary>
        /// 作业的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 作业所属课程的ID（外键）
        /// 通过这个字段可以知道作业属于哪门课程
        /// </summary>
        public int CourseId { get; set; }
        
        /// <summary>
        /// 作业标题
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(100)] 表示标题最大长度不能超过100个字符
        /// = string.Empty 给属性一个默认值，避免null引用异常
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 作业描述
        /// [StringLength(500)] 表示描述最大长度不能超过500个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// 作业截止日期
        /// DateTime类型用于存储日期和时间信息
        /// </summary>
        public DateTime DueDate { get; set; }
        
        /// <summary>
        /// 作业满分
        /// decimal类型用于存储精确的小数值，适合存储分数
        /// = 100 设置默认满分为100分
        /// </summary>
        public decimal MaxScore { get; set; } = 100;
        
        /// <summary>
        /// 作业权重
        /// 用于计算总成绩时，这个作业占的比重
        /// 1.00m 表示默认权重为1.0，m后缀表示decimal类型
        /// </summary>
        public decimal Weight { get; set; } = 1.00m;
        
        /// <summary>
        /// 作业创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录作业是什么时候创建的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 作业最后更新时间
        /// 每次修改作业信息时，这个字段会自动更新
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 导航属性 - 表示这个作业属于哪门课程
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// </summary>
        public virtual Course Course { get; set; } = null!;
        
        /// <summary>
        /// 导航属性 - 表示这个作业的所有提交记录
        /// ICollection<T> 表示一个集合，可以包含多个作业提交
        /// 一个作业可以有多个学生提交
        /// new List<AssignmentSubmission>() 初始化一个空列表
        /// </summary>
        public virtual ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
        
        /// <summary>
        /// 导航属性 - 表示这个作业的所有成绩记录
        /// 一个作业可以产生多个成绩（每个学生一个成绩）
        /// </summary>
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
} 