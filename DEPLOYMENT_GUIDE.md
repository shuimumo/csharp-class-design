# 学生管理系统部署与使用指南

## 项目概述

这是一个基于.NET 8和React的学生管理系统，采用前后端分离架构，支持学生、教师和管理员三种角色，提供课程管理、作业管理、成绩管理、通知管理等功能。

### 技术栈

**后端：**
- .NET 8 Web API
- Entity Framework Core
- SQL Server
- JWT认证
- CORS支持

**前端：**
- React 18
- TypeScript
- Ant Design
- Axios
- React Router

## 系统要求

### 开发环境
- Windows 10/11
- .NET 8 SDK
- Node.js 16+
- SQL Server 2019+
- Visual Studio 2022 或 VS Code

### 生产环境
- Windows Server 2019+
- .NET 8 Runtime
- SQL Server 2019+
- IIS 或 Nginx

## 部署步骤

### 1. 数据库准备

#### 1.1 安装SQL Server
1. 下载并安装SQL Server 2019或更高版本
2. 安装SQL Server Management Studio (SSMS)
3. 启动SQL Server服务

#### 1.2 创建数据库
```sql
-- 连接到SQL Server
-- 创建数据库
CREATE DATABASE StudentManagementDB;
GO

-- 使用数据库
USE StudentManagementDB;
GO
```

#### 1.3 运行数据库初始化脚本
```bash
# 在项目根目录执行
sqlcmd -S localhost -d StudentManagementDB -i database/init.sql
```

### 2. 后端部署

#### 2.1 配置数据库连接
编辑 `backend/StudentManagement.API/appsettings.json`：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentManagementDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

#### 2.2 安装依赖
```bash
cd backend/StudentManagement.API
dotnet restore
```

#### 2.3 运行数据库迁移
```bash
dotnet ef database update
```

#### 2.4 启动后端服务
```bash
dotnet run
```

后端服务将在 `https://localhost:5001` 启动。

### 3. 前端部署

#### 3.1 安装依赖
```bash
cd frontend
npm install
```

#### 3.2 配置API地址
编辑 `frontend/src/services/api.ts`，确保API地址正确：
```typescript
const API_BASE_URL = 'https://localhost:5001/api';
```

#### 3.3 启动前端服务
```bash
npm start
```

前端服务将在 `http://localhost:3000` 启动。

### 4. 生产环境部署

#### 4.1 后端发布
```bash
cd backend/StudentManagement.API
dotnet publish -c Release -o ./publish
```

#### 4.2 前端构建
```bash
cd frontend
npm run build
```

#### 4.3 IIS部署
1. 在IIS中创建网站
2. 将后端publish文件夹内容复制到网站目录
3. 配置应用程序池为.NET Core
4. 将前端build文件夹内容复制到静态文件目录

## 系统使用指南

### 1. 用户角色与权限

#### 1.1 管理员 (Admin)
- 用户管理：创建、编辑、删除学生和教师账户
- 课程管理：创建、编辑、删除课程
- 系统监控：查看系统日志和统计信息

#### 1.2 教师 (Teacher)
- 课程管理：管理自己教授的课程
- 作业管理：发布、编辑、删除作业
- 成绩管理：录入、编辑学生成绩
- 通知管理：发布课程通知
- 学生管理：查看选课学生信息

#### 1.3 学生 (Student)
- 课程查看：查看已选课程
- 作业查看：查看课程作业
- 作业提交：提交作业
- 成绩查看：查看个人成绩
- 通知查看：查看课程通知

### 2. 功能模块详解

#### 2.1 用户认证
- 支持用户名/密码登录
- JWT Token认证
- 角色权限控制
- 会话管理

#### 2.2 课程管理
- 课程创建与编辑
- 课程分配教师
- 学生选课管理
- 课程状态管理

#### 2.3 作业管理
- 作业发布
- 作业提交
- 作业评分
- 截止日期管理

#### 2.4 成绩管理
- 成绩录入
- 成绩统计
- 等级自动生成
- 成绩查询

#### 2.5 通知管理
- 通知发布
- 通知推送
- 通知分类
- 通知状态管理

### 3. 操作流程

#### 3.1 管理员操作流程
1. 登录系统
2. 创建教师账户
3. 创建学生账户
4. 创建课程
5. 分配教师到课程
6. 管理用户权限

#### 3.2 教师操作流程
1. 登录系统
2. 查看分配的课程
3. 发布课程作业
4. 发布课程通知
5. 录入学生成绩
6. 查看学生提交情况

#### 3.3 学生操作流程
1. 登录系统
2. 查看课程信息
3. 查看作业列表
4. 提交作业
5. 查看成绩
6. 查看通知

### 4. 常见问题解决

#### 4.1 数据库连接问题
**问题：** 无法连接到数据库
**解决：**
1. 检查SQL Server服务是否启动
2. 验证连接字符串是否正确
3. 确认数据库是否存在
4. 检查防火墙设置

#### 4.2 认证问题
**问题：** 登录失败或Token无效
**解决：**
1. 检查用户名密码是否正确
2. 确认用户账户是否激活
3. 检查JWT配置是否正确
4. 清除浏览器缓存

#### 4.3 权限问题
**问题：** 无法访问某些功能
**解决：**
1. 确认用户角色是否正确
2. 检查权限配置
3. 联系管理员分配权限

#### 4.4 前端显示问题
**问题：** 页面显示异常或功能不可用
**解决：**
1. 检查浏览器控制台错误
2. 确认API地址配置正确
3. 检查网络连接
4. 清除浏览器缓存

### 5. 系统维护

#### 5.1 数据备份
```sql
-- 备份数据库
BACKUP DATABASE StudentManagementDB 
TO DISK = 'C:\Backup\StudentManagementDB.bak'
WITH FORMAT, INIT, NAME = 'StudentManagementDB-Full Database Backup';
```

#### 5.2 日志管理
- 应用程序日志位于 `backend/StudentManagement.API/logs/`
- 数据库日志可通过SQL Server Management Studio查看
- 前端错误日志在浏览器控制台

#### 5.3 性能优化
- 定期清理临时文件
- 优化数据库查询
- 配置适当的缓存策略
- 监控系统资源使用

### 6. 安全建议

#### 6.1 密码安全
- 使用强密码策略
- 定期更换密码
- 启用密码复杂度要求

#### 6.2 网络安全
- 配置HTTPS
- 设置防火墙规则
- 限制访问IP
- 启用CORS保护

#### 6.3 数据安全
- 定期备份数据
- 加密敏感信息
- 限制数据库访问权限
- 审计用户操作
