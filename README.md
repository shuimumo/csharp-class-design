# 学生管理系统 (Student Management System)

一个基于.NET 8和React的现代化学生管理系统，采用前后端分离架构，支持多角色权限管理，提供完整的教学管理功能。

## 🚀 功能特性

### 核心功能
- **用户认证与授权** - JWT Token认证，支持多角色权限控制
- **课程管理** - 课程创建、编辑、分配教师、学生选课
- **作业管理** - 作业发布、提交、评分、截止日期管理
- **成绩管理** - 成绩录入、统计、等级生成、查询
- **通知管理** - 通知发布、推送、分类管理
- **仪表盘** - 数据可视化，实时统计信息展示

### 用户角色
- **管理员** - 系统管理、用户管理、课程管理、权限分配
- **教师** - 课程管理、作业发布、成绩录入、通知发布
- **学生** - 课程查看、作业提交、成绩查询、通知接收

## 🛠️ 技术栈

### 后端技术
- **框架**: .NET 8 Web API
- **ORM**: Entity Framework Core 8.0
- **数据库**: SQL Server 2019+
- **认证**: JWT Bearer Token
- **文档**: Swagger/OpenAPI
- **架构**: 分层架构 (API, Core, Domain, Infrastructure)

### 前端技术
- **框架**: React 18 + TypeScript
- **UI组件**: Ant Design 5.x
- **路由**: React Router v6
- **HTTP客户端**: Axios
- **构建工具**: Create React App

## 📦 项目结构

```
student-management-system/
├── backend/                    # 后端项目
│   ├── StudentManagement.API/  # Web API 项目
│   ├── StudentManagement.Core/ # 核心业务逻辑
│   ├── StudentManagement.Domain/ # 领域模型
│   └── StudentManagement.Infrastructure/ # 基础设施
├── frontend/                   # 前端项目
│   ├── src/
│   │   ├── components/         # 公共组件
│   │   ├── pages/             # 页面组件
│   │   ├── services/          # API服务
│   │   └── contexts/          # React Context
├── database/                   # 数据库脚本
│   ├── init.sql               # 数据库初始化脚本
│   └── fix-relationships.sql  # 关系修复脚本
└── docs/                      # 文档
    └── DEPLOYMENT_GUIDE.md    # 部署指南
```

## 🚀 快速开始

### 环境要求
- .NET 8 SDK
- Node.js 16+
- SQL Server 2019+
- Git

### 1. 克隆项目
```bash
git clone https://github.com/shuimumo/csharp-class-design.git
cd csharp-class-design
```

### 2. 数据库设置
```sql
-- 创建数据库
CREATE DATABASE StudentManagementDB;
GO

-- 运行初始化脚本
sqlcmd -S localhost -d StudentManagementDB -i database/init.sql
```

### 3. 后端配置
```bash
cd backend/StudentManagement.API

# 安装依赖
dotnet restore

# 配置数据库连接字符串
# 编辑 appsettings.json 中的 ConnectionStrings

# 启动后端服务
dotnet run
```
后端服务将在 `https://localhost:5001` 启动，Swagger文档在 `https://localhost:5001/swagger`

### 4. 前端配置
```bash
cd frontend

# 安装依赖
npm install

# 配置API地址
# 编辑 src/services/api.ts 中的 API_BASE_URL

# 启动前端服务
npm start
```
前端服务将在 `http://localhost:3000` 启动

## 📋 默认账户

### 管理员账户
- **用户名**: admin
- **密码**: admin123
- **权限**: 系统管理、用户管理、课程管理

### 教师账户
- **用户名**: teacher1
- **密码**: teacher123
- **权限**: 课程管理、作业发布、成绩录入

### 学生账户
- **用户名**: student1
- **密码**: student123
- **权限**: 课程查看、作业提交、成绩查询

## 📊 API文档

系统提供完整的Swagger API文档，启动后端服务后访问：
- **Swagger UI**: `https://localhost:5001/swagger`
- **OpenAPI规范**: `https://localhost:5001/swagger/v1/swagger.json`

### 主要API端点
- `GET /api/courses` - 获取课程列表
- `POST /api/assignments` - 创建作业
- `GET /api/grades/{studentId}` - 获取学生成绩
- `POST /api/auth/login` - 用户登录
- `GET /api/notifications` - 获取通知列表

### 日志查看
- 后端日志: `backend/StudentManagement.API/logs/`
- 前端错误: 浏览器开发者工具控制台
- 数据库日志: SQL Server Management Studio

