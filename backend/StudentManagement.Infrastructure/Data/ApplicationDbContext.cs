// 引入Entity Framework Core的命名空间，用于数据库操作
using Microsoft.EntityFrameworkCore;
// 引入我们项目中的实体类
using StudentManagement.Domain.Entities;

// 定义命名空间，用于组织和管理相关的类
namespace StudentManagement.Infrastructure.Data
{
    /// <summary>
    /// 应用程序数据库上下文类
    /// 继承自DbContext，这是Entity Framework Core的核心类
    /// 负责管理数据库连接、实体映射和数据库操作
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// 构造函数，接收数据库配置选项
        /// DbContextOptions<ApplicationDbContext> 包含数据库连接字符串等信息
        /// 这些选项通常在Program.cs中通过依赖注入配置
        /// </summary>
        /// <param name="options">数据库配置选项</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 用户表的数据集合
        /// DbSet<T> 表示数据库中的一个表，T是实体类型
        /// Users 是数据库中的表名（EF Core会自动将类名复数化）
        /// </summary>
        public DbSet<User> Users { get; set; }
        
        /// <summary>
        /// 学生表的数据集合
        /// 存储所有学生的信息
        /// </summary>
        public DbSet<Student> Students { get; set; }
        
        /// <summary>
        /// 教师表的数据集合
        /// 存储所有教师的信息
        /// </summary>
        public DbSet<Teacher> Teachers { get; set; }
        
        /// <summary>
        /// 课程表的数据集合
        /// 存储所有课程的信息
        /// </summary>
        public DbSet<Course> Courses { get; set; }
        
        /// <summary>
        /// 选课记录表的数据集合
        /// 存储学生选课的信息
        /// </summary>
        public DbSet<Enrollment> Enrollments { get; set; }
        
        /// <summary>
        /// 作业表的数据集合
        /// 存储所有作业的信息
        /// </summary>
        public DbSet<Assignment> Assignments { get; set; }
        
        /// <summary>
        /// 作业提交表的数据集合
        /// 存储学生提交作业的信息
        /// </summary>
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        
        /// <summary>
        /// 成绩表的数据集合
        /// 存储学生的成绩信息
        /// </summary>
        public DbSet<Grade> Grades { get; set; }
        
        /// <summary>
        /// 通知表的数据集合
        /// 存储系统通知信息
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }
        
        /// <summary>
        /// 考勤表的数据集合
        /// 存储学生的考勤记录
        /// </summary>
        public DbSet<Attendance> Attendances { get; set; }

        /// <summary>
        /// 配置数据库模型的方法
        /// 这个方法在EF Core创建数据库时被调用
        /// 用于配置实体之间的关系、索引、约束等
        /// </summary>
        /// <param name="modelBuilder">模型构建器，用于配置实体</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 调用基类的OnModelCreating方法
            base.OnModelCreating(modelBuilder);

            // 配置User实体的数据库映射
            modelBuilder.Entity<User>(entity =>
            {
                // 为Username字段创建唯一索引，确保用户名不重复
                entity.HasIndex(e => e.Username).IsUnique();
                // 为Email字段创建唯一索引，确保邮箱不重复
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // 配置Student实体的数据库映射
            modelBuilder.Entity<Student>(entity =>
            {
                // 为StudentNumber字段创建唯一索引，确保学号不重复
                entity.HasIndex(e => e.StudentNumber).IsUnique();
                // 配置Student和User之间的一对一关系
                entity.HasOne(e => e.User)           // Student有一个User
                    .WithOne(e => e.Student)         // User有一个Student
                    .HasForeignKey<Student>(e => e.UserId)  // 外键是UserId
                    .OnDelete(DeleteBehavior.Cascade);      // 级联删除：删除User时自动删除Student
            });

            // 配置Teacher实体的数据库映射
            modelBuilder.Entity<Teacher>(entity =>
            {
                // 为TeacherNumber字段创建唯一索引，确保工号不重复
                entity.HasIndex(e => e.TeacherNumber).IsUnique();
                // 配置Teacher和User之间的一对一关系
                entity.HasOne(e => e.User)           // Teacher有一个User
                    .WithOne(e => e.Teacher)         // User有一个Teacher
                    .HasForeignKey<Teacher>(e => e.UserId)  // 外键是UserId
                    .OnDelete(DeleteBehavior.Cascade);      // 级联删除：删除User时自动删除Teacher
            });

            // 配置Course实体的数据库映射
            modelBuilder.Entity<Course>(entity =>
            {
                // 为CourseCode字段创建唯一索引，确保课程代码不重复
                entity.HasIndex(e => e.CourseCode).IsUnique();
                // 配置Course和Teacher之间的一对多关系
                entity.HasOne(e => e.Teacher)        // Course有一个Teacher
                    .WithMany(e => e.Courses)        // Teacher有多个Course
                    .HasForeignKey(e => e.TeacherId) // 外键是TeacherId
                    .OnDelete(DeleteBehavior.SetNull); // 设置为null：删除Teacher时Course的TeacherId设为null
            });

            // 配置Enrollment实体的数据库映射
            modelBuilder.Entity<Enrollment>(entity =>
            {
                // 为StudentId和CourseId的组合创建唯一索引，确保一个学生不能重复选同一门课
                entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
                // 配置Enrollment和Student之间的一对多关系
                entity.HasOne(e => e.Student)        // Enrollment有一个Student
                    .WithMany(e => e.Enrollments)    // Student有多个Enrollment
                    .HasForeignKey(e => e.StudentId) // 外键是StudentId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Student时自动删除相关选课记录
                // 配置Enrollment和Course之间的一对多关系
                entity.HasOne(e => e.Course)         // Enrollment有一个Course
                    .WithMany(e => e.Enrollments)    // Course有多个Enrollment
                    .HasForeignKey(e => e.CourseId)  // 外键是CourseId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Course时自动删除相关选课记录
            });

            // 配置Assignment实体的数据库映射
            modelBuilder.Entity<Assignment>(entity =>
            {
                // 配置Assignment和Course之间的一对多关系
                entity.HasOne(e => e.Course)         // Assignment有一个Course
                    .WithMany(e => e.Assignments)    // Course有多个Assignment
                    .HasForeignKey(e => e.CourseId)  // 外键是CourseId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Course时自动删除相关作业
            });

            // 配置AssignmentSubmission实体的数据库映射
            modelBuilder.Entity<AssignmentSubmission>(entity =>
            {
                // 配置AssignmentSubmission和Assignment之间的一对多关系
                entity.HasOne(e => e.Assignment)     // AssignmentSubmission有一个Assignment
                    .WithMany(e => e.Submissions)    // Assignment有多个AssignmentSubmission
                    .HasForeignKey(e => e.AssignmentId) // 外键是AssignmentId
                    .OnDelete(DeleteBehavior.Cascade);  // 级联删除：删除Assignment时自动删除相关提交
                // 配置AssignmentSubmission和Student之间的一对多关系
                entity.HasOne(e => e.Student)        // AssignmentSubmission有一个Student
                    .WithMany(e => e.AssignmentSubmissions) // Student有多个AssignmentSubmission
                    .HasForeignKey(e => e.StudentId) // 外键是StudentId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Student时自动删除相关提交
            });

            // 配置Grade实体的数据库映射
            modelBuilder.Entity<Grade>(entity =>
            {
                // 配置Grade和Student之间的一对多关系
                entity.HasOne(e => e.Student)        // Grade有一个Student
                    .WithMany(e => e.Grades)         // Student有多个Grade
                    .HasForeignKey(e => e.StudentId) // 外键是StudentId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Student时自动删除相关成绩
                // 配置Grade和Course之间的一对多关系
                entity.HasOne(e => e.Course)         // Grade有一个Course
                    .WithMany(e => e.Grades)         // Course有多个Grade
                    .HasForeignKey(e => e.CourseId)  // 外键是CourseId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Course时自动删除相关成绩
                // 配置Grade和Assignment之间的一对多关系
                entity.HasOne(e => e.Assignment)     // Grade有一个Assignment
                    .WithMany(e => e.Grades)         // Assignment有多个Grade
                    .HasForeignKey(e => e.AssignmentId) // 外键是AssignmentId
                    .OnDelete(DeleteBehavior.SetNull);  // 设置为null：删除Assignment时Grade的AssignmentId设为null
                // 配置Grade和Teacher之间的一对多关系（谁评的成绩）
                entity.HasOne(e => e.GradedByTeacher) // Grade有一个GradedByTeacher
                    .WithMany(e => e.Grades)          // Teacher有多个Grade
                    .HasForeignKey(e => e.GradedBy)   // 外键是GradedBy
                    .OnDelete(DeleteBehavior.SetNull); // 设置为null：删除Teacher时Grade的GradedBy设为null
                
                // 映射 GradeLetter 属性到数据库的 Grade 列
                // 这样数据库列名是Grade，但C#属性名是GradeLetter
                entity.Property(e => e.GradeLetter)
                    .HasColumnName("Grade");
            });

            // 配置Notification实体的数据库映射
            modelBuilder.Entity<Notification>(entity =>
            {
                // 配置Notification和User之间的一对多关系
                entity.HasOne(e => e.TargetUser)     // Notification有一个TargetUser
                    .WithMany(e => e.Notifications)  // User有多个Notification
                    .HasForeignKey(e => e.TargetUserId) // 外键是TargetUserId
                    .OnDelete(DeleteBehavior.SetNull);  // 设置为null：删除User时Notification的TargetUserId设为null
            });

            // 配置Attendance实体的数据库映射
            modelBuilder.Entity<Attendance>(entity =>
            {
                // 配置Attendance和Student之间的一对多关系
                entity.HasOne(e => e.Student)        // Attendance有一个Student
                    .WithMany(e => e.Attendances)    // Student有多个Attendance
                    .HasForeignKey(e => e.StudentId) // 外键是StudentId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Student时自动删除相关考勤记录
                // 配置Attendance和Course之间的一对多关系
                entity.HasOne(e => e.Course)         // Attendance有一个Course
                    .WithMany(e => e.Attendances)    // Course有多个Attendance
                    .HasForeignKey(e => e.CourseId)  // 外键是CourseId
                    .OnDelete(DeleteBehavior.Cascade); // 级联删除：删除Course时自动删除相关考勤记录
                // 配置Attendance和Teacher之间的一对多关系（谁记录的考勤）
                entity.HasOne(e => e.RecordedByTeacher) // Attendance有一个RecordedByTeacher
                    .WithMany(e => e.Attendances)       // Teacher有多个Attendance
                    .HasForeignKey(e => e.RecordedBy)   // 外键是RecordedBy
                    .OnDelete(DeleteBehavior.SetNull);  // 设置为null：删除Teacher时Attendance的RecordedBy设为null
            });
        }
    }
} 