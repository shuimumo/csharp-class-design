import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import Login from './pages/Login';
import StudentDashboard from './pages/StudentDashboard';
import TeacherDashboard from './pages/TeacherDashboard';
import StudentManagement from './pages/StudentManagement';
import CourseManagement from './pages/CourseManagement';
import AdminDashboard from './pages/AdminDashboard';
import AssignmentsManagement from './pages/AssignmentsManagement';
import GradesManagement from './pages/GradesManagement';
import Notifications from './pages/Notifications';
import AccountManagement from './pages/AccountManagement';
import StudentAssignments from './pages/StudentAssignments';
import StudentNotifications from './pages/StudentNotifications';
import StudentGrades from './pages/StudentGrades';
import StudentCourses from './pages/StudentCourses';

import MainLayout from './components/MainLayout';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import './index.css';

const PrivateRoute: React.FC<{ children: React.ReactNode; allowedRoles?: string[] }> = ({ 
  children, 
  allowedRoles 
}) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && user && !allowedRoles.includes(user.role)) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

const AppContent: React.FC = () => {
  const { user, isAuthenticated } = useAuth();
  console.log('AppContent user:', user, 'isAuthenticated:', isAuthenticated, 'pathname:', window.location.pathname);

  if (!isAuthenticated) {
    return (
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    );
  }

  return (
    <Routes>
      <Route path="/" element={<Navigate to={`/${user?.role === 'Student' ? 'student-dashboard' : user?.role === 'Teacher' ? 'teacher-dashboard' : 'admin-dashboard'}`} replace />} />
      
      {/* 管理员页面 - 不使用 MainLayout */}
      <Route path="/admin-dashboard" element={
        <PrivateRoute allowedRoles={['Admin']}>
          <AdminDashboard />
        </PrivateRoute>
      } />
      <Route path="/accounts" element={
        <PrivateRoute allowedRoles={['Admin']}>
          <AccountManagement />
        </PrivateRoute>
      } />
      
      {/* 教师页面 - 使用 MainLayout */}
      <Route path="/teacher-dashboard" element={
        <PrivateRoute allowedRoles={['Teacher']}>
          <MainLayout>
            <TeacherDashboard />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/students" element={
        <PrivateRoute allowedRoles={['Teacher', 'Admin']}>
          <MainLayout>
            <StudentManagement />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/courses" element={
        <PrivateRoute allowedRoles={['Teacher', 'Admin']}>
          <MainLayout>
            <CourseManagement />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/assignments" element={
        <PrivateRoute allowedRoles={['Teacher', 'Admin']}>
          <MainLayout>
            <AssignmentsManagement />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/grades" element={
        <PrivateRoute allowedRoles={['Teacher', 'Admin']}>
          <MainLayout>
            <GradesManagement />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/notifications" element={
        <PrivateRoute allowedRoles={['Teacher', 'Admin']}>
          <MainLayout>
            <Notifications />
          </MainLayout>
        </PrivateRoute>
      } />
      
      {/* 学生页面 - 使用 MainLayout */}
      <Route path="/student-dashboard" element={
        <PrivateRoute allowedRoles={['Student']}>
          <MainLayout>
            <StudentDashboard />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/student-courses" element={
        <PrivateRoute allowedRoles={['Student']}>
          <MainLayout>
            <StudentCourses />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/student-assignments" element={
        <PrivateRoute allowedRoles={['Student']}>
          <MainLayout>
            <StudentAssignments />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/student-grades" element={
        <PrivateRoute allowedRoles={['Student']}>
          <MainLayout>
            <StudentGrades />
          </MainLayout>
        </PrivateRoute>
      } />
      <Route path="/student-notifications" element={
        <PrivateRoute allowedRoles={['Student']}>
          <MainLayout>
            <StudentNotifications />
          </MainLayout>
        </PrivateRoute>
      } />
      
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

const App: React.FC = () => {
  return (
    <ConfigProvider locale={zhCN}>
      <AuthProvider>
        <Router>
          <AppContent />
        </Router>
      </AuthProvider>
    </ConfigProvider>
  );
};

export default App; 