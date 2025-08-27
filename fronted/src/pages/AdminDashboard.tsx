import React from 'react';
import { Card, Row, Col, Button } from 'antd';
import { TeamOutlined, BarChartOutlined, LogoutOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const AdminDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <Card title={<div style={{display:'flex',justifyContent:'space-between',alignItems:'center'}}>
      <span>管理员仪表盘</span>
      <Button icon={<LogoutOutlined />} onClick={handleLogout} type="primary" danger size="small">退出登录</Button>
    </div>}>
      <Row gutter={[24, 24]}>
        <Col span={8}>
          <Card hoverable onClick={() => navigate('/students')}>
            <TeamOutlined style={{ fontSize: 32, color: '#1890ff' }} />
            <div style={{ marginTop: 12 }}>学生管理</div>
          </Card>
        </Col>
        <Col span={8}>
          <Card hoverable onClick={() => navigate('/courses')}>
            <TeamOutlined style={{ fontSize: 32, color: '#52c41a' }} />
            <div style={{ marginTop: 12 }}>课程管理</div>
          </Card>
        </Col>
        <Col span={8}>
          <Card hoverable onClick={() => navigate('/grades')}>
            <BarChartOutlined style={{ fontSize: 32, color: '#faad14' }} />
            <div style={{ marginTop: 12 }}>成绩管理</div>
          </Card>
        </Col>
        <Col span={8}>
          <Card hoverable onClick={() => navigate('/notifications')}>
            <BarChartOutlined style={{ fontSize: 32, color: '#722ed1' }} />
            <div style={{ marginTop: 12 }}>通知中心</div>
          </Card>
        </Col>
        <Col span={8}>
          <Card hoverable onClick={() => navigate('/accounts')}>
            <TeamOutlined style={{ fontSize: 32, color: '#eb2f96' }} />
            <div style={{ marginTop: 12 }}>账号管理</div>
          </Card>
        </Col>
        {/* 可扩展更多管理员功能入口 */}
      </Row>
    </Card>
  );
};

export default AdminDashboard; 