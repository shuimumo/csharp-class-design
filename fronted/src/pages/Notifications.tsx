import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Modal, Form, Input, message, Popconfirm } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';

interface Notification {
  id: number;
  title: string;
  content: string;
  type?: string;
  createdAt?: string;
  targetRole?: string;
}

const Notifications: React.FC = () => {
  const { user } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingNotification, setEditingNotification] = useState<Notification | null>(null);
  const [form] = Form.useForm();

  const fetchNotifications = async () => {
    setLoading(true);
    try {
      console.log('获取通知数据，用户角色:', user?.role);
      
      // 根据用户角色调用不同的接口
      let endpoint = '/notifications';
      if (user?.role === 'Student' || user?.role === 'Teacher') {
        endpoint = '/notifications/my-notifications';
      }
      const res = await api.get(endpoint);
      
      console.log('通知数据:', res.data);
      setNotifications(res.data);
    } catch (err: any) {
      console.error('获取通知列表失败:', err);
      message.error('获取通知列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchNotifications();
  }, [user?.role]);

  const handleAdd = () => {
    setEditingNotification(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (notification: Notification) => {
    setEditingNotification(notification);
    form.setFieldsValue(notification);
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await api.delete(`/notifications/${id}`);
      message.success('删除成功');
      fetchNotifications();
    } catch (err: any) {
      console.error('删除通知失败:', err);
      message.error('删除失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();
      if (editingNotification) {
        await api.put(`/notifications/${editingNotification.id}`, values);
        message.success('编辑成功');
      } else {
        await api.post('/notifications', values);
        message.success('新增成功');
      }
      setModalVisible(false);
      fetchNotifications();
    } catch (err: any) {
      console.error('保存通知失败:', err);
      message.error('保存失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const columns = [
    { title: '标题', dataIndex: 'title', key: 'title' },
    { title: '内容', dataIndex: 'content', key: 'content' },
    { title: '类型', dataIndex: 'type', key: 'type' },
    { title: '目标角色', dataIndex: 'targetRole', key: 'targetRole' },
    { title: '创建时间', dataIndex: 'createdAt', key: 'createdAt', render: (date: string) => date ? new Date(date).toLocaleString() : '' },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Notification) => (
        <>
          {(user?.role === 'Admin' || user?.role === 'Teacher') && (
            <Button icon={<EditOutlined />} size="small" onClick={() => handleEdit(record)} style={{ marginRight: 8 }}>编辑</Button>
          )}
          {user?.role === 'Admin' && (
            <Popconfirm title="确定删除该通知吗？" onConfirm={() => handleDelete(record.id)}>
              <Button icon={<DeleteOutlined />} size="small" danger>删除</Button>
            </Popconfirm>
          )}
        </>
      ),
    },
  ];

  return (
    <Card title="通知中心" extra={(user?.role === 'Admin' || user?.role === 'Teacher') && <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>新增通知</Button>}>
      <Table
        dataSource={notifications}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10 }}
      />
      <Modal
        title={editingNotification ? '编辑通知' : '新增通知'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => setModalVisible(false)}
        destroyOnClose
      >
        <Form form={form} layout="vertical">
          <Form.Item name="title" label="标题" rules={[{ required: true, message: '请输入标题' }]}> <Input /> </Form.Item>
          <Form.Item name="content" label="内容" rules={[{ required: true, message: '请输入内容' }]}> <Input.TextArea rows={2} /> </Form.Item>
          <Form.Item name="type" label="类型"> <Input /> </Form.Item>
          <Form.Item name="targetRole" label="目标角色"> <Input /> </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default Notifications; 