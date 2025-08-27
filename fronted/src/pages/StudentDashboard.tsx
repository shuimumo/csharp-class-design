import React, { useState, useEffect, useCallback } from 'react';
import { Card, Row, Col, Statistic, List, Avatar, Button, message } from 'antd';
import {
  BookOutlined,
  FileTextOutlined,
  TeamOutlined,
  TrophyOutlined
} from '@ant-design/icons';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

interface Course {
  id: number;
  courseCode: string;
  courseName: string;
  description?: string;
  credits: number;
  teacherName?: string;
  schedule?: string;
  location?: string;
}

interface Assignment {
  id: number;
  title: string;
  description?: string;
  dueDate: string;
  courseName: string;
}

interface Grade {
  id: number;
  courseName: string;
  score: number;
  grade?: string;
  assignmentTitle?: string;
}

const StudentDashboard: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [courses, setCourses] = useState<Course[]>([]);
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [grades, setGrades] = useState<Grade[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchDashboardData = useCallback(async () => {
    try {
      let assignmentsRes;
      if (user?.role === 'Student') {
        assignmentsRes = await api.get('/assignments/my-student-assignments');
      } else {
        assignmentsRes = await api.get('/assignments/my-assignments');
      }
      let gradesRes;
      if (user?.role === 'Student') {
        gradesRes = await api.get('/students/my-grades');
      } else {
        gradesRes = await api.get('/grades/my-grades');
      }
      const [coursesRes] = await Promise.all([
        api.get('/courses/my-courses'),
      ]);
      setCourses(coursesRes.data);
      setAssignments(assignmentsRes.data);
      setGrades(gradesRes.data);
    } catch (error) {
      message.error('获取数据失败');
    } finally {
      setLoading(false);
    }
  }, [user?.role]);

  useEffect(() => {
    fetchDashboardData();
  }, [fetchDashboardData]);



  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">欢迎回来，{user?.username}！</h1>
      </div>

          <Row gutter={[16, 16]}>
            <Col span={6}>
              <Card className="statistic-card">
                <Statistic
                  title="我的课程"
                  value={courses.length}
                  prefix={<BookOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card className="statistic-card">
                <Statistic
                  title="待完成作业"
                  value={assignments.filter(a => new Date(a.dueDate) > new Date()).length}
                  prefix={<FileTextOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card className="statistic-card">
                <Statistic
                  title="平均成绩"
                  value={grades.length > 0 ? (grades.reduce((sum, g) => sum + g.score, 0) / grades.length).toFixed(1) : 0}
                  prefix={<TrophyOutlined />}
                  suffix="分"
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card className="statistic-card">
                <Statistic
                  title="总学分"
                  value={courses.reduce((sum, c) => sum + c.credits, 0)}
                  prefix={<TeamOutlined />}
                />
              </Card>
            </Col>
          </Row>

          <Row gutter={[16, 16]} style={{ marginTop: '24px' }}>
            <Col span={12}>
              <Card title="我的课程" className="dashboard-card">
                <List
                  loading={loading}
                  dataSource={courses.slice(0, 5)}
                  renderItem={(course) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={<Avatar icon={<BookOutlined />} />}
                        title={course.courseName}
                        description={`${course.courseCode} | ${course.teacherName || '未分配教师'} | ${course.credits}学分`}
                      />
                    </List.Item>
                  )}
                />
                {courses.length > 5 && (
                  <Button type="link" onClick={() => navigate('/student-courses')}>
                    查看全部课程
                  </Button>
                )}
              </Card>
            </Col>
            
            <Col span={12}>
              <Card title="最近作业" className="dashboard-card">
                <List
                  loading={loading}
                  dataSource={assignments.slice(0, 5)}
                  renderItem={(assignment) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={<Avatar icon={<FileTextOutlined />} />}
                        title={assignment.title}
                        description={`${assignment.courseName} | 截止日期: ${new Date(assignment.dueDate).toLocaleDateString()}`}
                      />
                    </List.Item>
                  )}
                />
                {assignments.length > 5 && (
                  <Button type="link" onClick={() => navigate('/student-assignments')}>
                    查看全部作业
                  </Button>
                )}
              </Card>
            </Col>
          </Row>

          <Row gutter={[16, 16]} style={{ marginTop: '24px' }}>
            <Col span={24}>
              <Card title="最近成绩" className="dashboard-card">
                <List
                  loading={loading}
                  dataSource={grades.slice(0, 10)}
                  renderItem={(grade) => (
                    <List.Item>
                      <List.Item.Meta
                        avatar={<Avatar icon={<TrophyOutlined />} style={{ backgroundColor: grade.score >= 90 ? '#52c41a' : grade.score >= 80 ? '#1890ff' : grade.score >= 60 ? '#faad14' : '#f5222d' }} />}
                        title={`${grade.courseName}${grade.assignmentTitle ? ` - ${grade.assignmentTitle}` : ''}`}
                        description={`成绩: ${grade.score}分 ${grade.grade ? `(${grade.grade})` : ''}`}
                      />
                    </List.Item>
                  )}
                />
                {grades.length > 10 && (
                  <Button type="link" onClick={() => navigate('/student-grades')}>
                    查看全部成绩
                  </Button>
                )}
              </Card>
            </Col>
          </Row>
    </div>
  );
};

export default StudentDashboard; 