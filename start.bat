@echo off
chcp 65001 >nul

echo 学生管理系统启动脚本
echo ========================

echo.
echo 1. 启动后端API服务...
cd backend\StudentManagement.API
start "Student Management API" cmd /k "dotnet run"
cd ..\..

echo.
echo 2. 等待API服务启动...
timeout /t 10 /nobreak

echo.
echo 3. 启动前端开发服务器...
cd frontend
start "Student Management Frontend" cmd /k "npm start"
cd ..

echo.
echo 系统启动完成！
echo 后端API: http://localhost:5000
echo 前端应用: http://localhost:3000
echo Swagger文档: http://localhost:5000/swagger
echo.
echo 按任意键退出...
pause > nul 