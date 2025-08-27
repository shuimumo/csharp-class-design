// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 作业提交实体类 - 表示学生提交作业的信息
    /// 这个类会被Entity Framework Core映射到数据库的AssignmentSubmissions表
    /// 作业提交记录学生完成作业的情况，包括提交内容、评分、反馈等
    /// </summary>
    public class AssignmentSubmission
    {
        /// <summary>
        /// 作业提交的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 作业ID（外键）
        /// 通过这个字段可以知道学生提交的是哪个作业
        /// 与Assignment表形成多对一关系
        /// </summary>
        public int AssignmentId { get; set; }
        
        /// <summary>
        /// 学生ID（外键）
        /// 通过这个字段可以知道是哪个学生提交的作业
        /// 与Student表形成多对一关系
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// 作业提交日期
        /// DateTime类型用于存储日期和时间信息
        /// = DateTime.UtcNow 设置默认值为当前时间
        /// 用于记录学生是什么时候提交作业的
        /// </summary>
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 作业提交内容
        /// string? 表示这个字段可以为null（可选字段）
        /// 用于存储学生提交的作业文本内容
        /// 如果作业是文件形式，这个字段可能为空
        /// </summary>
        public string? Content { get; set; }
        
        /// <summary>
        /// 作业文件路径
        /// [StringLength(500)] 表示文件路径最大长度不能超过500个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 用于存储学生上传的作业文件的存储路径
        /// 如果作业是纯文本，这个字段可能为空
        /// </summary>
        [StringLength(500)]
        public string? FilePath { get; set; }
        
        /// <summary>
        /// 作业得分
        /// decimal? 表示这个字段可以为null（可选字段）
        /// 用于存储教师对作业的评分
        /// 如果为null，表示作业还没有被评分
        /// </summary>
        public decimal? Score { get; set; }
        
        /// <summary>
        /// 教师反馈
        /// [StringLength(500)] 表示反馈内容最大长度不能超过500个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 用于存储教师对作业的详细反馈和评价
        /// 帮助学生了解作业的优缺点和改进方向
        /// </summary>
        [StringLength(500)]
        public string? Feedback { get; set; }
        
        /// <summary>
        /// 学生备注
        /// [StringLength(500)] 表示备注内容最大长度不能超过500个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 用于存储学生在提交作业时的备注说明
        /// 如：特殊说明、问题描述等
        /// </summary>
        [StringLength(500)]
        public string? Comments { get; set; }
        
        /// <summary>
        /// 作业提交状态
        /// [StringLength(20)] 表示状态最大长度不能超过20个字符
        /// = "Submitted" 设置默认状态为已提交
        /// 常见的状态值：Submitted（已提交）、Graded（已评分）、Late（逾期提交）等
        /// 用于跟踪作业的处理进度
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Submitted";
        
        /// <summary>
        /// 作业提交记录创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录作业提交记录是什么时候创建的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 作业提交记录最后更新时间
        /// 每次修改作业提交记录时，这个字段会自动更新
        /// 用于跟踪作业提交记录的变更历史
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 导航属性 - 表示被提交的作业
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个多对一的关系：多个作业提交对应一个作业
        /// </summary>
        public virtual Assignment Assignment { get; set; } = null!;
        
        /// <summary>
        /// 导航属性 - 表示提交作业的学生
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个多对一的关系：多个作业提交对应一个学生
        /// </summary>
        public virtual Student Student { get; set; } = null!;
    }
} 