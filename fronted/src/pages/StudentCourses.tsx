import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Modal, Descriptions, Tag, Space, Avatar, List, Row, Col, Statistic, message } from 'antd';
import { BookOutlined, UserOutlined, CalendarOutlined, EnvironmentOutlined, EyeOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';

interface Course {
  id: number;
  courseCode: string;
  courseName: string;
  description?: string;
  credits: number;
  teacherName?: string;
  schedule?: string;
  location?: string;
  maxStudents?: number;
  enrolledStudents?: number;
  status?: string;
}

const StudentCourses: React.FC = () => {
  const { user } = useAuth();
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(false);
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState<Course | null>(null);

  const fetchCourses = async () => {
    setLoading(true);
    try {
      console.log('获取学生课程数据');
      const res = await api.get('/courses/my-courses');
      console.log('学生课程数据:', res.data);
      setCourses(res.data);
    } catch (err: any) {
      console.error('获取课程列表失败:', err);
      message.error('获取课程列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCourses();
  }, []);

  const handleViewDetail = (course: Course) => {
    setSelectedCourse(course);
    setDetailModalVisible(true);
  };

  const getStatusTag = (status?: string) => {
    switch (status) {
      case 'Enrolled':
        return <Tag color="green">已选课</Tag>;
      case 'Completed':
        return <Tag color="blue">已完成</Tag>;
      case 'InProgress':
        return <Tag color="orange">进行中</Tag>;
      default:
        return <Tag color="default">未知</Tag>;
    }
  };

  const columns = [
    { 
      title: '课程代码', 
      dataIndex: 'courseCode', 
      key: 'courseCode',
      render: (text: string, record: Course) => (
        <Button type="link" onClick={() => handleViewDetail(record)}>
          {text}
        </Button>
      )
    },
    { title: '课程名称', dataIndex: 'courseName', key: 'courseName' },
    { title: '学分', dataIndex: 'credits', key: 'credits' },
    { title: '授课教师', dataIndex: 'teacherName', key: 'teacherName' },
    { 
      title: '上课时间', 
      dataIndex: 'schedule', 
      key: 'schedule',
      render: (schedule: string) => schedule || '未设置'
    },
    { 
      title: '上课地点', 
      dataIndex: 'location', 
      key: 'location',
      render: (location: string) => location || '未设置'
    },
    { 
      title: '状态', 
      key: 'status',
      render: (_: any, record: Course) => getStatusTag(record.status)
    },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Course) => (
        <Button 
          icon={<EyeOutlined />} 
          size="small" 
          onClick={() => handleViewDetail(record)}
        >
          查看详情
        </Button>
      ),
    },
  ];

  const getTotalCredits = () => {
    return courses.reduce((sum, course) => sum + course.credits, 0);
  };

  return (
    <div style={{ padding: '24px' }}>
      {/* 统计卡片 */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="已选课程"
              value={courses.length}
              prefix={<BookOutlined />}
              suffix="门"
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="总学分"
              value={getTotalCredits()}
              prefix={<BookOutlined />}
              suffix="学分"
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="进行中课程"
              value={courses.filter(c => c.status === 'InProgress').length}
              prefix={<BookOutlined />}
              suffix="门"
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="已完成课程"
              value={courses.filter(c => c.status === 'Completed').length}
              prefix={<BookOutlined />}
              suffix="门"
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
      </Row>

      {/* 课程列表 */}
      <Card title="我的课程">
        <Table
          dataSource={courses}
          columns={columns}
          rowKey="id"
          loading={loading}
          pagination={{ 
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `第 ${range[0]}-${range[1]} 条，共 ${total} 条`
          }}
        />
      </Card>

      {/* 课程详情模态框 */}
      <Modal
        title="课程详情"
        open={detailModalVisible}
        onCancel={() => setDetailModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setDetailModalVisible(false)}>
            关闭
          </Button>
        ]}
        width={700}
      >
        {selectedCourse && (
          <div>
            <Descriptions bordered column={1}>
              <Descriptions.Item label="课程代码">{selectedCourse.courseCode}</Descriptions.Item>
              <Descriptions.Item label="课程名称">{selectedCourse.courseName}</Descriptions.Item>
              <Descriptions.Item label="课程描述">
                {selectedCourse.description || '暂无描述'}
              </Descriptions.Item>
              <Descriptions.Item label="学分">{selectedCourse.credits}学分</Descriptions.Item>
              <Descriptions.Item label="授课教师">{selectedCourse.teacherName || '未分配'}</Descriptions.Item>
              <Descriptions.Item label="上课时间">
                {selectedCourse.schedule || '未设置'}
              </Descriptions.Item>
              <Descriptions.Item label="上课地点">
                {selectedCourse.location || '未设置'}
              </Descriptions.Item>
              <Descriptions.Item label="选课状态">{getStatusTag(selectedCourse.status)}</Descriptions.Item>
              {selectedCourse.maxStudents && (
                <Descriptions.Item label="课程容量">
                  {selectedCourse.enrolledStudents || 0}/{selectedCourse.maxStudents}人
                </Descriptions.Item>
              )}
            </Descriptions>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default StudentCourses; 