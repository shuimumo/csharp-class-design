import React, { useEffect, useState, useCallback } from 'react';
import { Card, Table, Button, Modal, Form, Input, message, Popconfirm } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';

interface Student {
  id: number;
  studentNumber: string;
  firstName: string;
  lastName: string;
  major?: string;
  class?: string;
  email: string;
}

const StudentManagement: React.FC = () => {
  const { user } = useAuth();
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingStudent, setEditingStudent] = useState<Student | null>(null);
  const [form] = Form.useForm();

  const fetchStudents = useCallback(async () => {
    setLoading(true);
    try {
      console.log('获取学生数据，用户角色:', user?.role);
      
      // 根据用户角色调用不同的接口
      const endpoint = user?.role === 'Teacher' ? '/students/my-students' : '/students';
      const res = await api.get(endpoint);
      
      console.log('学生数据:', res.data);
      setStudents(res.data);
    } catch (err: any) {
      console.error('获取学生列表失败:', err);
      message.error('获取学生列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  }, [user?.role]);

  useEffect(() => {
    fetchStudents();
  }, [user?.role, fetchStudents]);

  const handleAdd = () => {
    setEditingStudent(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (student: Student) => {
    setEditingStudent(student);
    form.setFieldsValue(student);
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await api.delete(`/students/${id}`);
      message.success('删除成功');
      fetchStudents();
    } catch (err: any) {
      console.error('删除学生失败:', err);
      message.error('删除失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();
      if (editingStudent) {
        await api.put(`/students/${editingStudent.id}`, values);
        message.success('编辑成功');
      } else {
        await api.post('/students', values);
        message.success('新增成功');
      }
      setModalVisible(false);
      fetchStudents();
    } catch (err: any) {
      console.error('保存学生失败:', err);
      message.error('保存失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const columns = [
    { title: '学号', dataIndex: 'studentNumber', key: 'studentNumber' },
    { title: '姓名', dataIndex: 'firstName', key: 'firstName', render: (_: any, record: Student) => `${record.firstName} ${record.lastName}` },
    { title: '专业', dataIndex: 'major', key: 'major' },
    { title: '班级', dataIndex: 'class', key: 'class' },
    { title: '邮箱', dataIndex: 'email', key: 'email' },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Student) => (
        <>
          <Button icon={<EditOutlined />} size="small" onClick={() => handleEdit(record)} style={{ marginRight: 8 }}>编辑</Button>
          {user?.role === 'Admin' && (
            <Popconfirm title="确定删除该学生吗？" onConfirm={() => handleDelete(record.id)}>
              <Button icon={<DeleteOutlined />} size="small" danger>删除</Button>
            </Popconfirm>
          )}
        </>
      ),
    },
  ];

  return (
    <Card title="学生管理" extra={user?.role === 'Admin' && <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>新增学生</Button>}>
      <Table
        dataSource={students}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10 }}
      />
      <Modal
        title={editingStudent ? '编辑学生' : '新增学生'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => setModalVisible(false)}
        destroyOnClose
      >
        <Form form={form} layout="vertical">
          <Form.Item name="studentNumber" label="学号" rules={[{ required: true, message: '请输入学号' }]}> <Input /> </Form.Item>
          <Form.Item name="firstName" label="名" rules={[{ required: true, message: '请输入名' }]}> <Input /> </Form.Item>
          <Form.Item name="lastName" label="姓" rules={[{ required: true, message: '请输入姓' }]}> <Input /> </Form.Item>
          <Form.Item name="major" label="专业"> <Input /> </Form.Item>
          <Form.Item name="class" label="班级"> <Input /> </Form.Item>
          <Form.Item name="email" label="邮箱" rules={[{ required: true, message: '请输入邮箱' }, { type: 'email', message: '请输入有效邮箱' }]}> <Input /> </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default StudentManagement; 