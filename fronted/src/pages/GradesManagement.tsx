import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Modal, Form, Input, InputNumber, message, Popconfirm, Select } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const { Option } = Select;
const { TextArea } = Input;

interface Grade {
  id: number;
  studentName: string;
  courseName: string;
  assignmentTitle?: string;
  score: number;
  grade?: string;
  studentId?: number;
  courseId?: number;
  assignmentId?: number;
}

interface DropdownOption {
  value: number;
  label: string;
}

const GradesManagement: React.FC = () => {
  const { user } = useAuth();
  const [grades, setGrades] = useState<Grade[]>([]);
  const [students, setStudents] = useState<DropdownOption[]>([]);
  const [courses, setCourses] = useState<DropdownOption[]>([]);
  const [assignments, setAssignments] = useState<DropdownOption[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingGrade, setEditingGrade] = useState<Grade | null>(null);
  const [form] = Form.useForm();

  const fetchGrades = async () => {
    setLoading(true);
    try {
      console.log('获取成绩数据，用户角色:', user?.role);
      
      // 根据用户角色调用不同的接口
      const endpoint = user?.role === 'Teacher' ? '/grades/my-grades' : '/grades';
      const res = await api.get(endpoint);
      
      console.log('成绩数据:', res.data);
      setGrades(res.data);
    } catch (err: any) {
      console.error('获取成绩列表失败:', err);
      message.error('获取成绩列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const fetchDropdownData = async () => {
    try {
      // 获取学生下拉列表
      const studentsRes = await api.get('/grades/students-dropdown');
      setStudents(studentsRes.data);

      // 获取课程下拉列表
      const coursesRes = await api.get('/grades/courses-dropdown');
      setCourses(coursesRes.data);

      // 获取作业下拉列表
      const assignmentsRes = await api.get('/grades/assignments-dropdown');
      setAssignments(assignmentsRes.data);
    } catch (err: any) {
      console.error('获取下拉列表数据失败:', err);
      message.error('获取下拉列表数据失败: ' + (err.response?.data?.message || err.message));
    }
  };

  useEffect(() => {
    fetchGrades();
    if (user?.role === 'Teacher') {
      fetchDropdownData();
    }
  }, [user?.role]);

  const handleAdd = () => {
    setEditingGrade(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (grade: Grade) => {
    setEditingGrade(grade);
    form.setFieldsValue(grade);
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await api.delete(`/grades/${id}`);
      message.success('删除成功');
      fetchGrades();
    } catch (err: any) {
      console.error('删除成绩失败:', err);
      message.error('删除失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();
      if (editingGrade) {
        await api.put(`/grades/${editingGrade.id}`, values);
        message.success('编辑成功');
      } else {
        await api.post('/grades', values);
        message.success('新增成功');
      }
      setModalVisible(false);
      fetchGrades();
    } catch (err: any) {
      console.error('保存成绩失败:', err);
      message.error('保存失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleCourseChange = async (courseId: number) => {
    try {
      // 当课程改变时，重新获取该课程的作业列表
      const assignmentsRes = await api.get(`/grades/assignments-dropdown?courseId=${courseId}`);
      setAssignments(assignmentsRes.data);
      
      // 清空作业选择
      form.setFieldsValue({ assignmentId: undefined });
    } catch (err: any) {
      console.error('获取作业列表失败:', err);
    }
  };

  const handleScoreChange = (score: number | null) => {
    if (score !== null) {
      const gradeLetter = getGradeLetter(score);
      form.setFieldsValue({ grade: gradeLetter });
    } else {
      form.setFieldsValue({ grade: '' });
    }
  };

  const getGradeLetter = (score: number) => {
    if (score >= 90) return 'A';
    if (score >= 80) return 'B';
    if (score >= 70) return 'C';
    if (score >= 60) return 'D';
    return 'F';
  };

  const columns = [
    { title: '学生', dataIndex: 'studentName', key: 'studentName' },
    { title: '课程', dataIndex: 'courseName', key: 'courseName' },
    { title: '作业', dataIndex: 'assignmentTitle', key: 'assignmentTitle' },
    { title: '分数', dataIndex: 'score', key: 'score' },
    { title: '等级', dataIndex: 'grade', key: 'grade' },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Grade) => (
        <>
          <Button icon={<EditOutlined />} size="small" onClick={() => handleEdit(record)} style={{ marginRight: 8 }}>编辑</Button>
          <Popconfirm title="确定删除该成绩吗？" onConfirm={() => handleDelete(record.id)}>
            <Button icon={<DeleteOutlined />} size="small" danger>删除</Button>
          </Popconfirm>
        </>
      ),
    },
  ];

  return (
    <Card title="成绩管理" extra={<Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>新增成绩</Button>}>
      <Table
        dataSource={grades}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10 }}
      />
      <Modal
        title={editingGrade ? '编辑成绩' : '新增成绩'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => setModalVisible(false)}
        destroyOnClose
        width={600}
      >
        <Form form={form} layout="vertical">
          <Form.Item 
            name="studentId" 
            label="学生" 
            rules={[{ required: true, message: '请选择学生' }]}
          >
            <Select
              placeholder="请选择学生"
              showSearch
              filterOption={(input, option) =>
                (option?.label as string)?.toLowerCase().indexOf(input.toLowerCase()) >= 0
              }
            >
              {students.map(student => (
                <Option key={student.value} value={student.value}>
                  {student.label}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item 
            name="courseId" 
            label="课程" 
            rules={[{ required: true, message: '请选择课程' }]}
          >
            <Select
              placeholder="请选择课程"
              onChange={handleCourseChange}
              showSearch
              filterOption={(input, option) =>
                (option?.label as string)?.toLowerCase().indexOf(input.toLowerCase()) >= 0
              }
            >
              {courses.map(course => (
                <Option key={course.value} value={course.value}>
                  {course.label}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item 
            name="assignmentId" 
            label="作业"
          >
            <Select
              placeholder="请选择作业（可选）"
              allowClear
              showSearch
              filterOption={(input, option) =>
                (option?.label as string)?.toLowerCase().indexOf(input.toLowerCase()) >= 0
              }
            >
              {assignments.map(assignment => (
                <Option key={assignment.value} value={assignment.value}>
                  {assignment.label}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item 
            name="score" 
            label="分数" 
            rules={[{ required: true, message: '请输入分数' }]}
          >
            <InputNumber 
              min={0} 
              max={100} 
              style={{ width: '100%' }} 
              placeholder="请输入分数（0-100）"
              onChange={handleScoreChange}
            />
          </Form.Item>

          <Form.Item 
            name="grade" 
            label="等级"
          >
            <Input placeholder="系统将根据分数自动生成等级" readOnly />
          </Form.Item>

          <Form.Item 
            name="comments" 
            label="评语"
          >
            <TextArea rows={3} placeholder="请输入评语（可选）" />
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default GradesManagement; 