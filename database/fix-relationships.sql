-- 修复学生管理系统数据库关系脚本
-- 这个脚本用于修复 init.sql 和 Entity Framework 模型之间的不一致

USE StudentManagementDB;
GO

-- 1. 确保所有表名和列名与 Entity Framework 期望的一致
-- Entity Framework 使用复数表名，init.sql 已经正确

-- 2. 检查并修复 Grade 表的列映射
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Grades' AND COLUMN_NAME = 'Grade')
BEGIN
    -- 如果 Grade 列存在且类型不是 NVARCHAR(2)，需要修改
    IF (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Grades' AND COLUMN_NAME = 'Grade') != 'nvarchar'
    BEGIN
        ALTER TABLE Grades ALTER COLUMN Grade NVARCHAR(2);
    END
END
ELSE
BEGIN
    -- 如果 Grade 列不存在，但 Entity Framework 期望它存在（映射到 GradeLetter）
    ALTER TABLE Grades ADD Grade NVARCHAR(2);
END
GO

-- 3. 确保所有外键约束正确
-- 检查 Students 表的外键
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME LIKE 'FK_Students_Users%')
BEGIN
    ALTER TABLE Students 
    ADD CONSTRAINT FK_Students_Users_UserId 
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE;
END
GO

-- 检查 Teachers 表的外键
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME LIKE 'FK_Teachers_Users%')
BEGIN
    ALTER TABLE Teachers 
    ADD CONSTRAINT FK_Teachers_Users_UserId 
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE;
END
GO

-- 检查 Courses 表的外键
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME LIKE 'FK_Courses_Teachers%')
BEGIN
    ALTER TABLE Courses 
    ADD CONSTRAINT FK_Courses_Teachers_TeacherId 
    FOREIGN KEY (TeacherId) REFERENCES Teachers(Id) ON DELETE SET NULL;
END
GO

-- 4. 确保所有索引正确
-- 检查 Users 表的索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
END
GO

-- 检查 Students 表的索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_StudentNumber' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE UNIQUE INDEX IX_Students_StudentNumber ON Students(StudentNumber);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_UserId' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE UNIQUE INDEX IX_Students_UserId ON Students(UserId);
END
GO

-- 5. 添加缺失的约束（如果需要）
-- 检查 Enrollments 表的唯一约束
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME LIKE 'UQ_Enrollments_StudentId_CourseId%')
BEGIN
    ALTER TABLE Enrollments 
    ADD CONSTRAINT UQ_Enrollments_StudentId_CourseId 
    UNIQUE (StudentId, CourseId);
END
GO

PRINT '数据库关系修复完成！';
PRINT '请确保 Entity Framework 模型与数据库架构一致。';
PRINT '建议使用 Entity Framework Migrations 来管理数据库架构变更。';
