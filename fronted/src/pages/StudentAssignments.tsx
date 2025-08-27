import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Modal, Form, Input, message, Tag, Space, Descriptions } from 'antd';
import { EyeOutlined, UploadOutlined, CheckCircleOutlined, ClockCircleOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';

interface Assignment {
  id: number;
  title: string;
  description?: string;
  dueDate: string;
  courseName?: string;
  courseId?: number;
  maxScore?: number;
  weight?: number;
  createdAt?: string;
  isSubmitted?: boolean;
  submissionId?: number;
}

interface AssignmentSubmission {
  id: number;
  assignmentId: number;
  content: string;
  submittedAt: string;
  score?: number;
  feedback?: string;
}

const StudentAssignments: React.FC = () => {
  const { user } = useAuth();
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(false);
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [submitModalVisible, setSubmitModalVisible] = useState(false);
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | null>(null);
  const [submission, setSubmission] = useState<AssignmentSubmission | null>(null);
  const [form] = Form.useForm();

  const fetchAssignments = async () => {
    setLoading(true);
    try {
      console.log('获取学生作业数据');
      const res = await api.get('/assignments/my-student-assignments');
      console.log('学生作业数据:', res.data);
      
      // 获取每个作业的提交状态
      const assignmentsWithSubmissionStatus = await Promise.all(
        res.data.map(async (assignment: Assignment) => {
          try {
            const submissionRes = await api.get(`/assignment-submissions/my-submission/${assignment.id}`);
            return {
              ...assignment,
              isSubmitted: true,
              submissionId: submissionRes.data.id
            };
          } catch (error) {
            return {
              ...assignment,
              isSubmitted: false
            };
          }
        })
      );
      
      setAssignments(assignmentsWithSubmissionStatus);
    } catch (err: any) {
      console.error('获取作业列表失败:', err);
      message.error('获取作业列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAssignments();
  }, []);

  const handleViewDetail = async (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    
    // 如果已提交，获取提交详情
    if (assignment.isSubmitted && assignment.submissionId) {
      try {
        const submissionRes = await api.get(`/assignment-submissions/${assignment.submissionId}`);
        setSubmission(submissionRes.data);
      } catch (error) {
        console.error('获取提交详情失败:', error);
      }
    } else {
      setSubmission(null);
    }
    
    setDetailModalVisible(true);
  };

  const handleSubmit = (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    form.resetFields();
    setSubmitModalVisible(true);
  };

  const handleSubmitAssignment = async () => {
    try {
      const values = await form.validateFields();
      await api.post('/assignment-submissions', {
        assignmentId: selectedAssignment?.id,
        content: values.content
      });
      message.success('作业提交成功');
      setSubmitModalVisible(false);
      fetchAssignments();
    } catch (err: any) {
      console.error('提交作业失败:', err);
      message.error('提交失败: ' + (err.response?.data?.message || err.message));
    }
  };

  const getStatusTag = (assignment: Assignment) => {
    const now = new Date();
    const dueDate = new Date(assignment.dueDate);
    
    if (assignment.isSubmitted) {
      return <Tag color="green" icon={<CheckCircleOutlined />}>已提交</Tag>;
    } else if (now > dueDate) {
      return <Tag color="red">已逾期</Tag>;
    } else {
      return <Tag color="orange" icon={<ClockCircleOutlined />}>待提交</Tag>;
    }
  };

  const columns = [
    { 
      title: '标题', 
      dataIndex: 'title', 
      key: 'title',
      render: (text: string, record: Assignment) => (
        <Button type="link" onClick={() => handleViewDetail(record)}>
          {text}
        </Button>
      )
    },
    { title: '课程', dataIndex: 'courseName', key: 'courseName' },
    { 
      title: '截止日期', 
      dataIndex: 'dueDate', 
      key: 'dueDate', 
      render: (date: string) => date ? new Date(date).toLocaleDateString() : '',
      sorter: (a: Assignment, b: Assignment) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime()
    },
    { title: '满分', dataIndex: 'maxScore', key: 'maxScore' },
    { 
      title: '状态', 
      key: 'status',
      render: (_: any, record: Assignment) => getStatusTag(record)
    },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Assignment) => (
        <Space>
          <Button 
            icon={<EyeOutlined />} 
            size="small" 
            onClick={() => handleViewDetail(record)}
          >
            查看详情
          </Button>
          {!record.isSubmitted && new Date(record.dueDate) > new Date() && (
            <Button 
              type="primary"
              icon={<UploadOutlined />} 
              size="small" 
              onClick={() => handleSubmit(record)}
            >
              提交作业
            </Button>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Card title="我的作业" style={{ marginBottom: '24px' }}>
        <Table
          dataSource={assignments}
          columns={columns}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 10 }}
        />
      </Card>

      {/* 作业详情模态框 */}
      <Modal
        title="作业详情"
        open={detailModalVisible}
        onCancel={() => setDetailModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setDetailModalVisible(false)}>
            关闭
          </Button>
        ]}
        width={800}
      >
        {selectedAssignment && (
          <div>
            <Descriptions bordered column={1}>
              <Descriptions.Item label="作业标题">{selectedAssignment.title}</Descriptions.Item>
              <Descriptions.Item label="课程">{selectedAssignment.courseName}</Descriptions.Item>
              <Descriptions.Item label="描述">{selectedAssignment.description || '无'}</Descriptions.Item>
              <Descriptions.Item label="截止日期">
                {new Date(selectedAssignment.dueDate).toLocaleString()}
              </Descriptions.Item>
              <Descriptions.Item label="满分">{selectedAssignment.maxScore}分</Descriptions.Item>
              <Descriptions.Item label="权重">{selectedAssignment.weight || 1}</Descriptions.Item>
              <Descriptions.Item label="状态">{getStatusTag(selectedAssignment)}</Descriptions.Item>
            </Descriptions>

            {submission && (
              <div style={{ marginTop: '24px' }}>
                <h3>我的提交</h3>
                <Descriptions bordered column={1}>
                  <Descriptions.Item label="提交内容">{submission.content}</Descriptions.Item>
                  <Descriptions.Item label="提交时间">
                    {new Date(submission.submittedAt).toLocaleString()}
                  </Descriptions.Item>
                  {submission.score !== undefined && (
                    <Descriptions.Item label="得分">{submission.score}分</Descriptions.Item>
                  )}
                  {submission.feedback && (
                    <Descriptions.Item label="教师反馈">{submission.feedback}</Descriptions.Item>
                  )}
                </Descriptions>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* 提交作业模态框 */}
      <Modal
        title="提交作业"
        open={submitModalVisible}
        onOk={handleSubmitAssignment}
        onCancel={() => setSubmitModalVisible(false)}
        destroyOnClose
      >
        <Form form={form} layout="vertical">
          <Form.Item 
            name="content" 
            label="作业内容" 
            rules={[{ required: true, message: '请输入作业内容' }]}
          >
            <Input.TextArea rows={6} placeholder="请在此输入您的作业内容..." />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default StudentAssignments; 