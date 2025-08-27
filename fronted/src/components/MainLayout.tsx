import React from 'react';
import { Layout, Menu, Avatar, Button } from 'antd';
import {
  UserOutlined,
  LogoutOutlined,
  DashboardOutlined,
  BookOutlined,
  TeamOutlined,
  FileTextOutlined,
  TrophyOutlined,
  BellOutlined
} from '@ant-design/icons';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate, useLocation } from 'react-router-dom';

const { Header, Sider, Content } = Layout;

interface MainLayoutProps {
  children: React.ReactNode;
}

const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const getMenuItems = () => {
    if (user?.role === 'Teacher') {
      return [
        {
          key: 'teacher-dashboard',
          icon: <DashboardOutlined />,
          label: '仪表板',
        },
        {
          key: 'courses',
          icon: <BookOutlined />,
          label: '课程管理',
        },
        {
          key: 'students',
          icon: <TeamOutlined />,
          label: '学生管理',
        },
        {
          key: 'assignments',
          icon: <FileTextOutlined />,
          label: '作业管理',
        },
        {
          key: 'grades',
          icon: <TrophyOutlined />,
          label: '成绩管理',
        },
        {
          key: 'notifications',
          icon: <BellOutlined />,
          label: '通知',
        },
      ];
    } else if (user?.role === 'Student') {
      return [
        {
          key: 'student-dashboard',
          icon: <DashboardOutlined />,
          label: '仪表板',
        },
        {
          key: 'student-courses',
          icon: <BookOutlined />,
          label: '我的课程',
        },
        {
          key: 'student-assignments',
          icon: <FileTextOutlined />,
          label: '作业',
        },
        {
          key: 'student-grades',
          icon: <TrophyOutlined />,
          label: '成绩',
        },
        {
          key: 'student-notifications',
          icon: <BellOutlined />,
          label: '通知',
        },
      ];
    }
    return [];
  };

  const handleMenuClick = ({ key }: { key: string }) => {
    navigate(`/${key}`);
  };

  const getSelectedKey = () => {
    const path = location.pathname;
    if (path === '/teacher-dashboard' || path === '/') return 'teacher-dashboard';
    if (path === '/student-dashboard') return 'student-dashboard';
    return path.substring(1); // 移除开头的 '/'
  };

  const getSidebarTitle = () => {
    return user?.role === 'Teacher' ? '教师仪表盘' : '学生仪表盘';
  };

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider width={200} theme="light">
        <div className="sidebar-title">
          {getSidebarTitle()}
        </div>
        <Menu
          mode="inline"
          selectedKeys={[getSelectedKey()]}
          style={{ height: '100%', borderRight: 0 }}
          items={getMenuItems()}
          onClick={handleMenuClick}
        />
      </Sider>
      
      <Layout>
        <Header style={{ 
          background: '#fff', 
          padding: '0 24px', 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'center',
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
          borderBottom: '1px solid #f0f0f0'
        }}>
          <h2 style={{ margin: 0, color: '#1890ff' }}>
            {user?.role === 'Teacher' ? '教师仪表板' : '学生仪表板'}
          </h2>
          <div className="header-actions">
            <span className="user-info">
              <Avatar icon={<UserOutlined />} />
              <span>{user?.username}</span>
            </span>
            <Button 
              type="text" 
              icon={<LogoutOutlined />} 
              onClick={handleLogout}
            >
              退出
            </Button>
          </div>
        </Header>
        
        <Content style={{ 
          margin: '24px', 
          padding: '24px', 
          background: '#f0f2f5',
          borderRadius: '8px',
          minHeight: 'calc(100vh - 112px)'
        }}>
          {children}
        </Content>
      </Layout>
    </Layout>
  );
};

export default MainLayout; 