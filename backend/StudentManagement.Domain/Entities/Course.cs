// 引入数据验证特性命名空间，用于添加数据验证规则
using System.ComponentModel.DataAnnotations;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Domain.Entities
{
    /// <summary>
    /// 课程实体类 - 表示系统中的课程信息
    /// 这个类会被Entity Framework Core映射到数据库的Courses表
    /// 课程是教学活动的核心，包含课程基本信息、教师、学生等
    /// </summary>
    public class Course
    {
        /// <summary>
        /// 课程的唯一标识符（主键）
        /// 数据库会自动生成这个值，通常是从1开始递增的整数
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 课程代码
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(20)] 表示课程代码最大长度不能超过20个字符
        /// 课程代码是课程的唯一标识符，如：CS101、MATH201等
        /// 用于课程管理和学生选课
        /// </summary>
        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 课程名称
        /// [Required] 表示这个字段是必填的，不能为空
        /// [StringLength(100)] 表示课程名称最大长度不能超过100个字符
        /// 如：计算机科学导论、高等数学、数据结构等
        /// 用于显示和记录课程信息
        /// </summary>
        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = string.Empty;
        
        /// <summary>
        /// 课程描述
        /// [StringLength(500)] 表示描述最大长度不能超过500个字符
        /// string? 表示这个字段可以为null（可选字段）
        /// 用于详细描述课程内容、教学目标、先修要求等
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// 课程学分
        /// int类型用于存储整数值
        /// 表示这门课程的价值，用于计算学生的总学分和GPA
        /// 通常1学分对应16-18学时的课程
        /// </summary>
        public int Credits { get; set; }
        
        /// <summary>
        /// 授课教师ID（外键）
        /// int? 表示这个字段可以为null（可选字段）
        /// 通过这个字段可以知道这门课程由哪位教师教授
        /// 如果为null，表示课程还没有分配教师
        /// </summary>
        public int? TeacherId { get; set; }
        
        /// <summary>
        /// 课程最大学生数量
        /// int类型用于存储整数值
        /// = 50 设置默认最大学生数为50人
        /// 用于控制选课人数，确保教学质量
        /// </summary>
        public int MaxStudents { get; set; } = 50;
        
        /// <summary>
        /// 课程开始日期
        /// DateTime? 表示这个字段可以为null（可选字段）
        /// 用于记录课程什么时候开始，可能用于课程安排和统计
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// 课程结束日期
        /// DateTime? 表示这个字段可以为null（可选字段）
        /// 用于记录课程什么时候结束，可能用于课程安排和统计
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// 课程时间安排
        /// [StringLength(100)] 表示时间安排最大长度不能超过100个字符
        /// 如：周一 8:00-9:40、周二 14:00-15:40等
        /// 用于学生了解课程的具体上课时间
        /// </summary>
        [StringLength(100)]
        public string? Schedule { get; set; }
        
        /// <summary>
        /// 课程上课地点
        /// [StringLength(100)] 表示地点最大长度不能超过100个字符
        /// 如：教学楼A101、实验楼B203等
        /// 用于学生了解课程的具体上课地点
        /// </summary>
        [StringLength(100)]
        public string? Location { get; set; }
        
        /// <summary>
        /// 课程状态
        /// [StringLength(20)] 表示状态最大长度不能超过20个字符
        /// = "Active" 设置默认状态为活跃
        /// 常见的状态值：Active（活跃）、Inactive（非活跃）、Completed（已完成）等
        /// 用于控制课程是否可选、是否显示等
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Active";
        
        /// <summary>
        /// 课程创建时间
        /// DateTime.UtcNow 获取当前的UTC时间（世界协调时间）
        /// 用于记录课程是什么时候创建的
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 课程最后更新时间
        /// 每次修改课程信息时，这个字段会自动更新
        /// 用于跟踪课程信息的变更历史
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 导航属性 - 表示这门课程的授课教师
        /// virtual 关键字允许Entity Framework进行延迟加载
        /// Teacher? 表示这个属性可以为null（课程可能还没有分配教师）
        /// 这是一个多对一的关系：多门课程对应一个教师
        /// </summary>
        public virtual Teacher? Teacher { get; set; }
        
        /// <summary>
        /// 导航属性 - 表示这门课程的所有选课记录
        /// ICollection<Enrollment> 表示一个集合，可以包含多个选课记录
        /// 一门课程可以有多个学生选修
        /// new List<Enrollment>() 初始化一个空列表
        /// 这是一个一对多的关系：一门课程对应多个选课记录
        /// </summary>
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        
        /// <summary>
        /// 导航属性 - 表示这门课程的所有作业
        /// 一门课程可以布置多个作业
        /// 用于教师管理作业和学生查看作业要求
        /// </summary>
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        
        /// <summary>
        /// 导航属性 - 表示这门课程的所有成绩
        /// 一门课程可以产生多个学生的成绩
        /// 用于计算课程的平均分、及格率等统计信息
        /// </summary>
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        
        /// <summary>
        /// 导航属性 - 表示这门课程的所有考勤记录
        /// 一门课程可以有多条考勤记录（每次上课）
        /// 用于统计课程的出勤率和考勤情况
        /// </summary>
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
} 