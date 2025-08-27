@echo off
chcp 65001 >nul
echo ========================================
echo   学生管理系统一键部署脚本
echo ========================================
echo.

REM 检查 SQL Server 是否运行
echo 1. 检查 SQL Server 服务状态...
sc query MSSQLSERVER | find "RUNNING" >nul
if %errorlevel% neq 0 (
    echo   错误: SQL Server 服务未运行！
    echo   请启动 SQL Server 服务后再运行此脚本
    pause
    exit /b 1
)
echo   ✓ SQL Server 服务正常运行

REM 创建数据库
echo.
echo 2. 创建数据库 StudentManagementDB...
sqlcmd -S localhost -Q "IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'StudentManagementDB') CREATE DATABASE StudentManagementDB" >nul 2>&1
if %errorlevel% neq 0 (
    echo   错误: 创建数据库失败！
    pause
    exit /b 1
)
echo   ✓ 数据库创建成功

REM 执行初始化脚本
echo.
echo 3. 执行数据库初始化脚本...
if exist "database\init.sql" (
    sqlcmd -S localhost -d StudentManagementDB -i "database\init.sql" >nul 2>&1
    if %errorlevel% neq 0 (
        echo   警告: 初始化脚本执行可能有问题，继续执行修复...
    ) else (
        echo   ✓ 数据库初始化完成
    )
) else (
    echo   错误: 找不到 database\init.sql 文件！
    pause
    exit /b 1
)

REM 执行修复脚本
echo.
echo 4. 执行数据库修复脚本...
if exist "database\fix-relationships.sql" (
    sqlcmd -S localhost -d StudentManagementDB -i "database\fix-relationships.sql"
    if %errorlevel% neq 0 (
        echo   警告: 修复脚本执行可能有问题
    ) else (
        echo   ✓ 数据库修复完成
    )
) else (
    echo   警告: 找不到修复脚本，跳过修复步骤
)

REM 构建后端项目
echo.
echo 5. 构建后端项目...
cd backend
dotnet build --configuration Release >nul 2>&1
if %errorlevel% neq 0 (
    echo   错误: 后端项目构建失败！
    cd ..
    pause
    exit /b 1
)
echo   ✓ 后端项目构建成功
cd ..

REM 安装前端依赖
echo.
echo 6. 安装前端依赖...
cd frontend
if exist "node_modules" (
    echo   ✓ node_modules 已存在，跳过安装
) else (
    npm install >nul 2>&1
    if %errorlevel% neq 0 (
        echo   错误: 前端依赖安装失败！
        cd ..
        pause
        exit /b 1
    )
    echo   ✓ 前端依赖安装成功
)
cd ..

REM 启动应用程序
echo.
echo 7. 启动应用程序...
echo   启动后端 API 服务...
start cmd /k "cd backend\StudentManagement.API && dotnet run --configuration Release"
timeout /t 3 /nobreak >nul

echo   启动前端开发服务器...
start cmd /k "cd frontend && npm start"

echo.
echo ========================================
echo   部署完成！
echo ========================================
echo.
echo 应用程序已启动：
echo   - 后端 API: http://localhost:5000 或 https://localhost:7000
echo   - 前端界面: http://localhost:3000
echo.
echo 按任意键关闭此窗口...
pause >nul
