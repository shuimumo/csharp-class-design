import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Modal, Form, Input, Tabs, message } from 'antd';
import api from '../services/api';

const { TabPane } = Tabs;

interface UserAccount {
  id: number;
  username: string;
  email: string;
  role: string;
}

const AccountManagement: React.FC = () => {
  const [activeTab, setActiveTab] = useState('teacher');
  const [accounts, setAccounts] = useState<UserAccount[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserAccount | null>(null);
  const [form] = Form.useForm();

  const fetchAccounts = async (role: string) => {
    setLoading(true);
    try {
      const res = await api.get(role === 'teacher' ? '/teachers/users' : '/students/users');
      setAccounts(res.data);
    } catch (err: any) {
      message.error('获取账号列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAccounts(activeTab);
  }, [activeTab]);

  const handleResetPassword = (user: UserAccount) => {
    setSelectedUser(user);
    form.resetFields();
    setModalVisible(true);
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();
      await api.post('/auth/reset-password', { userId: selectedUser?.id, password: values.password });
      message.success('密码重置成功');
      setModalVisible(false);
    } catch (err: any) {
      message.error('重置失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const columns = [
    { title: '用户名', dataIndex: 'username', key: 'username' },
    { title: '邮箱', dataIndex: 'email', key: 'email' },
    { title: '操作', key: 'action', render: (_: any, record: UserAccount) => (
      <Button onClick={() => handleResetPassword(record)} type="link">重置密码</Button>
    ) },
  ];

  return (
    <Card title="账号管理">
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="教师账号" key="teacher" />
        <TabPane tab="学生账号" key="student" />
      </Tabs>
      <Table
        dataSource={accounts}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10 }}
      />
      <Modal
        title={`重置密码：${selectedUser?.username}`}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => setModalVisible(false)}
        destroyOnClose
      >
        <Form form={form} layout="vertical">
          <Form.Item name="password" label="新密码" rules={[{ required: true, message: '请输入新密码' }, { min: 6, message: '密码至少6位' }]}> <Input.Password /> </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default AccountManagement; 