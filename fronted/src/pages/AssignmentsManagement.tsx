import React, { useEffect, useState, useCallback } from 'react';
import { Card, Table, Button, Modal, Form, Input, DatePicker, message, Popconfirm, Select } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import dayjs from 'dayjs';

interface Assignment {
  id: number;
  title: string;
  description?: string;
  dueDate: string;
  courseName?: string;
  courseId?: number;
  maxScore?: number;
  weight?: number;
}

interface Course {
  value: number;
  label: string;
}

const AssignmentsManagement: React.FC = () => {
  const { user } = useAuth();
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingAssignment, setEditingAssignment] = useState<Assignment | null>(null);
  const [form] = Form.useForm();

  const fetchAssignments = useCallback(async () => {
    setLoading(true);
    try {
      console.log('获取作业数据，用户角色:', user?.role);
      
      // 根据用户角色调用不同的接口
      const endpoint = user?.role === 'Teacher' ? '/assignments/my-assignments' : '/assignments';
      const res = await api.get(endpoint);
      
      console.log('作业数据:', res.data);
      setAssignments(res.data);
    } catch (err: any) {
      console.error('获取作业列表失败:', err);
      message.error('获取作业列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  }, [user?.role]);

  const fetchCourses = async () => {
    try {
      const res = await api.get('/courses');
      const courseOptions = res.data.map((course: any) => ({
        value: course.id,
        label: `${course.courseCode} - ${course.courseName}`
      }));
      setCourses(courseOptions);
    } catch (err: any) {
      console.error('获取课程列表失败:', err);
    }
  };

  useEffect(() => {
    fetchAssignments();
    if (user?.role === 'Teacher') {
      fetchCourses();
    }
  }, [user?.role, fetchAssignments]);

  const handleAdd = () => {
    setEditingAssignment(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (assignment: Assignment) => {
    setEditingAssignment(assignment);
    form.setFieldsValue({ 
      ...assignment, 
      dueDate: assignment.dueDate ? dayjs(assignment.dueDate) : null 
    });
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await api.delete(`/assignments/${id}`);
      message.success('删除成功');
      fetchAssignments();
    } catch (err: any) {
      console.error('删除作业失败:', err);
      message.error('删除失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();
      
      // 格式化日期
      if (values.dueDate) {
        values.dueDate = values.dueDate.format('YYYY-MM-DDTHH:mm:ss');
      }
      
      if (editingAssignment) {
        await api.put(`/assignments/${editingAssignment.id}`, values);
        message.success('编辑成功');
      } else {
        await api.post('/assignments', values);
        message.success('新增成功');
      }
      setModalVisible(false);
      fetchAssignments();
    } catch (err: any) {
      console.error('保存作业失败:', err);
      message.error('保存失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const columns = [
    { title: '标题', dataIndex: 'title', key: 'title' },
    { title: '描述', dataIndex: 'description', key: 'description' },
    { title: '截止日期', dataIndex: 'dueDate', key: 'dueDate', render: (date: string) => date ? new Date(date).toLocaleDateString() : '' },
    { title: '课程', dataIndex: 'courseName', key: 'courseName' },
    { title: '满分', dataIndex: 'maxScore', key: 'maxScore' },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Assignment) => (
        <>
          <Button icon={<EditOutlined />} size="small" onClick={() => handleEdit(record)} style={{ marginRight: 8 }}>编辑</Button>
          <Popconfirm title="确定删除该作业吗？" onConfirm={() => handleDelete(record.id)}>
            <Button icon={<DeleteOutlined />} size="small" danger>删除</Button>
          </Popconfirm>
        </>
      ),
    },
  ];

  return (
    <Card title="作业管理" extra={<Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>新增作业</Button>}>
      <Table
        dataSource={assignments}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10 }}
      />
      <Modal
        title={editingAssignment ? '编辑作业' : '新增作业'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => setModalVisible(false)}
        destroyOnClose
      >
        <Form form={form} layout="vertical">
          <Form.Item name="title" label="标题" rules={[{ required: true, message: '请输入标题' }]}> <Input /> </Form.Item>
          <Form.Item name="description" label="描述"> <Input.TextArea rows={2} /> </Form.Item>
          <Form.Item name="dueDate" label="截止日期" rules={[{ required: true, message: '请选择截止日期' }]}> <DatePicker style={{ width: '100%' }} showTime /> </Form.Item>
          <Form.Item name="courseId" label="课程" rules={[{ required: true, message: '请选择课程' }]}> 
            <Select options={courses} placeholder="请选择课程" /> 
          </Form.Item>
          <Form.Item name="maxScore" label="满分" rules={[{ required: true, message: '请输入满分' }]}> <Input type="number" /> </Form.Item>
          <Form.Item name="weight" label="权重"> <Input type="number" step="0.1" /> </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default AssignmentsManagement; 