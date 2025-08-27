import React, { useState } from 'react';
import { Form, Input, Button, Card, message, Tabs } from 'antd';
import { UserOutlined, LockOutlined, MailOutlined } from '@ant-design/icons';
import { useAuth } from '../contexts/AuthContext';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';

const { TabPane } = Tabs;

interface LoginForm {
  username: string;
  password: string;
}

interface RegisterForm {
  username: string;
  password: string;
  email: string;
  role: string;
  studentNumber?: string;
  firstName?: string;
  lastName?: string;
  major?: string;
  class?: string;
  teacherNumber?: string;
  department?: string;
  title?: string;
}

const Login: React.FC = () => {
  const [loginLoading, setLoginLoading] = useState(false);
  const [registerLoading, setRegisterLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const [loginRole, setLoginRole] = useState<'student' | 'staff'>('student');
  const [registerForm] = Form.useForm();
  const [registerRole, setRegisterRole] = useState<'student' | 'teacher'>('student');

  const onLoginFinish = async (values: LoginForm) => {
    setLoginLoading(true);
    try {
      const response = await api.post('/auth/login', values);
      console.log('Login response:', response); // Debug log
      const { token, username, role, userId } = response.data;
      console.log('Parsed data:', { token, username, role, userId }); // Debug log
      
      login(token, {
        id: userId,
        username,
        email: '',
        role
      });
      
      message.success('登录成功！');
      navigate('/'); // 登录成功后跳转主页
    } catch (error: any) {
      console.error('登录失败', error, error.response?.data);
      message.error(error.response?.data?.message || '登录失败，请检查用户名和密码');
      form.setFieldsValue({ password: '' });
      const passwordInput = document.querySelector('input[type="password"]') as HTMLInputElement;
      if (passwordInput) passwordInput.focus();
    } finally {
      setLoginLoading(false);
    }
  };

  const onRegisterFinish = async (values: RegisterForm) => {
    setRegisterLoading(true);
    try {
      const payload = { ...values, role: registerRole === 'student' ? 'Student' : 'Teacher' };
      const response = await api.post('/auth/register', payload);
      const { token, username, role, userId } = response.data;
      login(token, {
        id: userId,
        username,
        email: values.email,
        role
      });
      message.success('注册成功！即将跳转主页');
      setTimeout(() => navigate('/'), 1000);
    } catch (error: any) {
      message.error(error.response?.data?.message || '注册失败，请检查输入信息');
      registerForm.setFieldsValue({ password: '' });
      const passwordInput = document.querySelector('input[type="password"]') as HTMLInputElement;
      if (passwordInput) passwordInput.focus();
    } finally {
      setRegisterLoading(false);
    }
  };

  return (
    <div className="login-container" style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'linear-gradient(135deg, #e0e7ff 0%, #f0fdfa 100%)',
    }}>
      <Card className="login-form" style={{
        width: 400,
        boxShadow: '0 8px 32px 0 rgba(31, 38, 135, 0.15)',
        borderRadius: 18,
        padding: '32px 24px',
        background: 'rgba(255,255,255,0.98)',
        border: 'none',
      }}>
        <h1 className="login-title" style={{
          fontSize: 32,
          fontWeight: 700,
          textAlign: 'center',
          marginBottom: 8,
          color: '#2d3a4b',
          letterSpacing: 2
        }}>学生管理系统</h1>
        <div style={{ textAlign: 'center', color: '#888', marginBottom: 24, fontSize: 16 }}>
          统一身份认证平台
        </div>
        <Tabs activeKey={loginRole} onChange={key => setLoginRole(key as 'student' | 'staff')} centered style={{ marginBottom: 24 }}>
          <TabPane tab={<span style={{ fontSize: 16 }}>{'学生登录'}</span>} key="student" />
          <TabPane tab={<span style={{ fontSize: 16 }}>{'教师/管理员登录'}</span>} key="staff" />
        </Tabs>
        <Tabs defaultActiveKey="login" centered style={{ marginBottom: 0 }}>
          <TabPane tab={<span style={{ fontSize: 16 }}>登录</span>} key="login">
            <Form
              name="login"
              form={form}
              onFinish={onLoginFinish}
              autoComplete="off"
              layout="vertical"
              style={{ marginTop: 8 }}
            >
              <Form.Item
                name="username"
                style={{ marginBottom: 18 }}
                rules={[{ required: true, message: '请输入用户名！' }]}
              >
                <Input 
                  prefix={<UserOutlined />} 
                  placeholder="用户名" 
                  size="large"
                  style={{ borderRadius: 8 }}
                />
              </Form.Item>

              <Form.Item
                name="password"
                style={{ marginBottom: 18 }}
                rules={[{ required: true, message: '请输入密码！' }]}
              >
                <Input.Password
                  prefix={<LockOutlined />}
                  placeholder="密码"
                  size="large"
                  style={{ borderRadius: 8 }}
                />
              </Form.Item>

              <Form.Item style={{ marginBottom: 8 }}>
                <Button 
                  type="primary" 
                  htmlType="submit" 
                  loading={loginLoading}
                  size="large"
                  block
                  style={{
                    borderRadius: 8,
                    background: 'linear-gradient(90deg, #6366f1 0%, #38bdf8 100%)',
                    border: 'none',
                    fontWeight: 600,
                    letterSpacing: 1
                  }}
                >
                  登录
                </Button>
              </Form.Item>
              <div style={{ textAlign: 'center', color: '#888', fontSize: 15, marginBottom: 0 }}>
                当前身份：{loginRole === 'student' ? '学生' : '教师/管理员'}
              </div>
            </Form>
          </TabPane>

          <TabPane tab="注册" key="register">
            <Form
              name="register"
              form={registerForm}
              onFinish={onRegisterFinish}
              autoComplete="off"
              layout="vertical"
            >
              <Form.Item
                name="username"
                rules={[{ required: true, message: '请输入用户名！' }]}
              >
                <Input 
                  prefix={<UserOutlined />} 
                  placeholder="用户名" 
                  size="large"
                />
              </Form.Item>

              <Form.Item
                name="email"
                rules={[
                  { required: true, message: '请输入邮箱！' },
                  { type: 'email', message: '请输入有效的邮箱地址！' }
                ]}
              >
                <Input 
                  prefix={<MailOutlined />} 
                  placeholder="邮箱" 
                  size="large"
                />
              </Form.Item>

              <Form.Item
                name="password"
                rules={[
                  { required: true, message: '请输入密码！' },
                  { min: 6, message: '密码至少6位！' }
                ]}
              >
                <Input.Password
                  prefix={<LockOutlined />}
                  placeholder="密码"
                  size="large"
                />
              </Form.Item>

              <Form.Item
                name="role"
                style={{ display: 'none' }}
                initialValue={registerRole === 'student' ? 'Student' : 'Teacher'}
              >
                <Input type="hidden" />
              </Form.Item>
              <Tabs activeKey={registerRole} onChange={key => setRegisterRole(key as 'student' | 'teacher')} size="small" style={{ marginBottom: 16 }}>
                <TabPane tab="学生" key="student">
                  <Form.Item
                    name="studentNumber"
                    rules={[{ required: true, message: '请输入学号！' }]}
                  >
                    <Input placeholder="学号" />
                  </Form.Item>
                  <Form.Item
                    name="firstName"
                    rules={[{ required: true, message: '请输入姓名！' }]}
                  >
                    <Input placeholder="姓名" />
                  </Form.Item>
                  <Form.Item name="major">
                    <Input placeholder="专业" />
                  </Form.Item>
                  <Form.Item name="class">
                    <Input placeholder="班级" />
                  </Form.Item>
                </TabPane>
                <TabPane tab="教师" key="teacher">
                  <Form.Item
                    name="teacherNumber"
                    rules={[{ required: true, message: '请输入工号！' }]}
                  >
                    <Input placeholder="工号" />
                  </Form.Item>
                  <Form.Item
                    name="firstName"
                    rules={[{ required: true, message: '请输入姓名！' }]}
                  >
                    <Input placeholder="姓名" />
                  </Form.Item>
                  <Form.Item name="department">
                    <Input placeholder="院系" />
                  </Form.Item>
                  <Form.Item name="title">
                    <Input placeholder="职称" />
                  </Form.Item>
                </TabPane>
              </Tabs>
              <Form.Item>
                <Button 
                  type="primary" 
                  htmlType="submit" 
                  loading={registerLoading}
                  size="large"
                  block
                >
                  注册
                </Button>
              </Form.Item>
            </Form>
          </TabPane>
        </Tabs>
      </Card>
    </div>
  );
};

export default Login;
