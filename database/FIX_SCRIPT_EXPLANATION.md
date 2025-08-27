# 数据库修复脚本详细说明

## 📋 脚本名称：fix-relationships.sql

## 🎯 主要目的
这个脚本用于**自动检测和修复**手动创建的数据库架构 (`init.sql`) 与 Entity Framework 代码模型之间的兼容性问题。

## 🔧 具体修复内容

### 1. Grade 表列类型修复
- **问题**: EF Core 期望 `Grades` 表的 `Grade` 列为 `NVARCHAR(2)` 类型
- **修复**: 检查并确保列类型正确，如果不正确则自动修改

### 2. 外键约束验证
- **检查的表**: Students, Teachers, Courses
- **确保**: 所有外键关系正确建立，删除行为配置正确
- **包括**: ON DELETE CASCADE 和 ON DELETE SET NULL 约束

### 3. 索引完整性检查
- **验证的索引**:
  - `IX_Users_Username` (用户名唯一索引)
  - `IX_Users_Email` (邮箱唯一索引) 
  - `IX_Students_StudentNumber` (学号唯一索引)
  - `IX_Students_UserId` (学生用户ID索引)

### 4. 唯一约束保障
- **重点**: 确保 `Enrollments` 表有 `(StudentId, CourseId)` 的唯一约束
- **作用**: 防止同一个学生重复选择同一门课程

## 🚀 使用场景

### 部署时使用
在运行 `init.sql` 创建数据库后，运行此脚本：
```bat
sqlcmd -S localhost -d StudentManagementDB -i database\fix-relationships.sql
```

### 数据库迁移后
当数据库结构发生变化时，运行此脚本确保兼容性

### 故障排查时
当出现数据库相关错误时，运行此脚本修复可能的结构问题

## ✅ 安全性保障

- **非破坏性**: 脚本只添加缺失的约束和索引，不会删除现有数据
- **条件执行**: 所有操作都有 `IF NOT EXISTS` 检查，避免重复创建
- **错误处理**: 脚本包含错误检查，确保操作安全

## 📊 预期输出

运行成功后会出现：
```
数据库关系修复完成！
请确保 Entity Framework 模型与数据库架构一致。
建议使用 Entity Framework Migrations 来管理数据库架构变更。
```

## 🛠️ 技术细节

### 使用的系统视图：
- `INFORMATION_SCHEMA.COLUMNS` - 检查列信息
- `INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS` - 检查外键约束
- `sys.indexes` - 检查索引信息
- `INFORMATION_SCHEMA.TABLE_CONSTRAINTS` - 检查表约束

### 兼容性：
- 支持 SQL Server 2012 及以上版本
- 与 Entity Framework Core 8.0 完全兼容

## 🔍 故障排除

如果脚本运行失败，检查：
1. 数据库连接是否正常
2. 数据库名称是否为 `StudentManagementDB`
3. SQL Server 实例是否运行在 localhost

这个脚本是确保手动 SQL 初始化与 EF Core 代码模型兼容的关键工具。
