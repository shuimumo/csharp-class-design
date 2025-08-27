// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 选课记录实体类 - 表示学生选课的信息
    /// 这个类会被Entity Framework Core映射到数据库的Enrollments表
    /// 选课记录是连接学生和课程的桥梁，记录学生选修了哪些课程
    /// </summary>
    public class Enrollment
    {
        /// <summary>
        /// 选课记录的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 学生ID（外键）
        /// 通过这个字段可以知道是哪个学生选课
        /// 与Student表形成多对一关系
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// 课程ID（外键）
        /// 通过这个字段可以知道学生选修了哪门课程
        /// 与Course表形成多对一关系
        /// </summary>
        public int CourseId { get; set; }
        
        /// <summary>
        /// 选课日期
        /// DateTime类型用于存储日期和时间信息
        /// = DateTime.UtcNow 设置默认值为当前时间
        /// 用于记录学生是什么时候选课的
        /// </summary>
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 选课状态
        /// [StringLength(20)] 表示状态最大长度不能超过20个字符
        /// = "Enrolled" 设置默认状态为已选课
        /// 常见的状态值：Enrolled（已选课）、Dropped（已退课）、Completed（已完成）等
        /// 用于跟踪学生的选课状态变化
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Enrolled";
        
        /// <summary>
        /// 课程成绩
        /// decimal? 表示这个字段可以为null（可选字段）
        /// 用于存储学生在这门课程中的最终成绩
        /// 如果为null，表示课程还没有结束或成绩还没有录入
        /// </summary>
        public decimal? Grade { get; set; }
        
        /// <summary>
        /// 选课记录创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录选课记录是什么时候创建的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 选课记录最后更新时间
        /// 每次修改选课记录时，这个字段会自动更新
        /// 用于跟踪选课记录的变更历史
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 导航属性 - 表示选课的学生
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个多对一的关系：多个选课记录对应一个学生
        /// </summary>
        public virtual Student Student { get; set; } = null!;
        
        /// <summary>
        /// 导航属性 - 表示被选修的课程
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// null! 表示这个属性不会为null（EF Core会确保这一点）
        /// 这是一个多对一的关系：多个选课记录对应一门课程
        /// </summary>
        public virtual Course Course { get; set; } = null!;
    }
} 