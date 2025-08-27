import React, { useEffect, useState, useCallback } from 'react';
import { Card, Table, Button, Modal, Form, Input, InputNumber, message, Popconfirm, Select, DatePicker } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import dayjs from 'dayjs';

interface Course {
  id: number;
  courseCode: string;
  courseName: string;
  description?: string;
  credits: number;
  teacherId?: number;
  teacherName?: string;
  maxStudents?: number;
  schedule?: string;
  location?: string;
  status?: string;
  enrolledStudents?: number;
  startDate?: string;
  endDate?: string;
}

const CourseManagement: React.FC = () => {
  const { user } = useAuth();
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingCourse, setEditingCourse] = useState<Course | null>(null);
  const [form] = Form.useForm();

  const fetchCourses = useCallback(async () => {
    setLoading(true);
    try {
      console.log('获取课程数据，用户角色:', user?.role);
      
      // 根据用户角色调用不同的接口
      const endpoint = user?.role === 'Teacher' ? '/courses/my-courses' : '/courses';
      const res = await api.get(endpoint);
      
      console.log('课程数据:', res.data);
      setCourses(res.data);
    } catch (err: any) {
      console.error('获取课程列表失败:', err);
      message.error('获取课程列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  }, [user?.role]);

  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  const handleAdd = () => {
    setEditingCourse(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (course: Course) => {
    setEditingCourse(course);
    form.setFieldsValue({
      ...course,
      startDate: course.startDate ? dayjs(course.startDate) : null,
      endDate: course.endDate ? dayjs(course.endDate) : null
    });
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await api.delete(`/courses/${id}`);
      message.success('删除成功');
      fetchCourses();
    } catch (err: any) {
      console.error('删除课程失败:', err);
      message.error('删除失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();
      
      // 格式化日期
      if (values.startDate) {
        values.startDate = values.startDate.format('YYYY-MM-DD');
      }
      if (values.endDate) {
        values.endDate = values.endDate.format('YYYY-MM-DD');
      }
      
      if (editingCourse) {
        await api.put(`/courses/${editingCourse.id}`, values);
        message.success('编辑成功');
      } else {
        await api.post('/courses', values);
        message.success('新增成功');
      }
      setModalVisible(false);
      fetchCourses();
    } catch (err: any) {
      console.error('保存课程失败:', err);
      message.error('保存失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const columns = [
    { title: '课程代码', dataIndex: 'courseCode', key: 'courseCode' },
    { title: '课程名称', dataIndex: 'courseName', key: 'courseName' },
    { title: '学分', dataIndex: 'credits', key: 'credits' },
    { title: '教师', dataIndex: 'teacherName', key: 'teacherName' },
    { title: '已选人数', dataIndex: 'enrolledStudents', key: 'enrolledStudents' },
    { title: '最大人数', dataIndex: 'maxStudents', key: 'maxStudents' },
    { title: '上课时间', dataIndex: 'schedule', key: 'schedule' },
    { title: '地点', dataIndex: 'location', key: 'location' },
    { title: '状态', dataIndex: 'status', key: 'status' },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Course) => (
        <>
          <Button icon={<EditOutlined />} size="small" onClick={() => handleEdit(record)} style={{ marginRight: 8 }}>编辑</Button>
          {user?.role === 'Admin' && (
            <Popconfirm title="确定删除该课程吗？" onConfirm={() => handleDelete(record.id)}>
              <Button icon={<DeleteOutlined />} size="small" danger>删除</Button>
            </Popconfirm>
          )}
        </>
      ),
    },
  ];

  return (
    <Card title="课程管理" extra={(user?.role === 'Admin' || user?.role === 'Teacher') && <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>新增课程</Button>}>
      <Table
        dataSource={courses}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10 }}
      />
      <Modal
        title={editingCourse ? '编辑课程' : '新增课程'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => setModalVisible(false)}
        destroyOnClose
        width={600}
      >
        <Form form={form} layout="vertical">
          <Form.Item name="courseCode" label="课程代码" rules={[{ required: true, message: '请输入课程代码' }]}> <Input /> </Form.Item>
          <Form.Item name="courseName" label="课程名称" rules={[{ required: true, message: '请输入课程名称' }]}> <Input /> </Form.Item>
          <Form.Item name="description" label="课程描述"> <Input.TextArea rows={3} /> </Form.Item>
          <Form.Item name="credits" label="学分" rules={[{ required: true, message: '请输入学分' }]}> <InputNumber min={0} style={{ width: '100%' }} /> </Form.Item>
          <Form.Item name="maxStudents" label="最大人数"> <InputNumber min={1} style={{ width: '100%' }} /> </Form.Item>
          <Form.Item name="schedule" label="上课时间"> <Input /> </Form.Item>
          <Form.Item name="location" label="地点"> <Input /> </Form.Item>
          <Form.Item name="startDate" label="开始日期"> <DatePicker style={{ width: '100%' }} /> </Form.Item>
          <Form.Item name="endDate" label="结束日期"> <DatePicker style={{ width: '100%' }} /> </Form.Item>
          <Form.Item name="status" label="状态"> 
            <Select>
              <Select.Option value="Active">活跃</Select.Option>
              <Select.Option value="Inactive">非活跃</Select.Option>
              <Select.Option value="Completed">已完成</Select.Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default CourseManagement; 