import React, { useState, useEffect, useCallback } from 'react';
import { Card, Row, Col, Statistic, List, Avatar, Button, message } from 'antd';
import {
  UserOutlined,
  BookOutlined,
  FileTextOutlined,
  TeamOutlined,
  TrophyOutlined,
  PlusOutlined
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
  enrolledStudents: number;
  maxStudents: number;
  schedule?: string;
  location?: string;
}

interface Student {
  id: number;
  studentNumber: string;
  firstName: string;
  lastName: string;
  major?: string;
  class?: string;
  email: string;
}

interface Assignment {
  id: number;
  title: string;
  courseName: string;
  dueDate: string;
  submittedCount: number;
  totalStudents: number;
}

const TeacherDashboard: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [courses, setCourses] = useState<Course[]>([]);
  const [students, setStudents] = useState<Student[]>([]);
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(true);

  console.log('TeacherDashboard 渲染', user);

  const fetchDashboardData = useCallback(async () => {
    try {
      console.log('开始获取教师仪表盘数据');
      const [coursesRes, studentsRes, assignmentsRes] = await Promise.all([
        api.get('/courses/my-courses'),
        api.get('/students/my-students'),
        api.get('/assignments/my-assignments')
      ]);
      console.log('课程数据:', coursesRes.data);
      console.log('学生数据:', studentsRes.data);
      console.log('作业数据:', assignmentsRes.data);
      setCourses(coursesRes.data);
      setStudents(studentsRes.data);
      setAssignments(assignmentsRes.data);
    } catch (error: any) {
      console.error('获取教师仪表盘数据失败:', error);
      message.error('获取数据失败: ' + (error.response?.data?.message || error.message));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboardData();
  }, [fetchDashboardData]);



  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">欢迎回来，{user?.username}老师！</h1>
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
              title="学生总数"
              value={students.length}
              prefix={<TeamOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card className="statistic-card">
            <Statistic
              title="待批改作业"
              value={assignments.filter(a => a.submittedCount > 0).length}
              prefix={<FileTextOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card className="statistic-card">
            <Statistic
              title="平均出勤率"
              value={85.6}
              prefix={<TrophyOutlined />}
              suffix="%"
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: '24px' }}>
        <Col span={12}>
          <Card 
            title="我的课程" 
            className="dashboard-card"
            extra={
              <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/courses')}>
                添加课程
              </Button>
            }
          >
            <List
              loading={loading}
              dataSource={courses.slice(0, 5)}
              renderItem={(course) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<Avatar icon={<BookOutlined />} />}
                    title={course.courseName}
                    description={`${course.courseCode} | ${course.enrolledStudents}/${course.maxStudents}人 | ${course.credits}学分`}
                  />
                  <div>
                    <Button type="link" onClick={() => navigate(`/courses/${course.id}`)}>
                      查看详情
                    </Button>
                  </div>
                </List.Item>
              )}
            />
            {courses.length > 5 && (
              <Button type="link" onClick={() => navigate('/courses')}>
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
                    description={`${assignment.courseName} | 截止日期: ${new Date(assignment.dueDate).toLocaleDateString()} | 已提交: ${assignment.submittedCount}/${assignment.totalStudents}`}
                  />
                  <div>
                    <Button type="link" onClick={() => navigate(`/assignments/${assignment.id}`)}>
                      批改
                    </Button>
                  </div>
                </List.Item>
              )}
            />
            {assignments.length > 5 && (
              <Button type="link" onClick={() => navigate('/assignments')}>
                查看全部作业
              </Button>
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: '24px' }}>
        <Col span={24}>
          <Card title="学生列表" className="dashboard-card">
            <List
              loading={loading}
              dataSource={students.slice(0, 10)}
              renderItem={(student) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<Avatar icon={<UserOutlined />} />}
                    title={`${student.firstName} ${student.lastName}`}
                    description={`学号: ${student.studentNumber} | ${student.major || ''} ${student.class || ''} | ${student.email}`}
                  />
                  <div className="action-buttons">
                    <Button type="link" onClick={() => navigate(`/students/${student.id}`)}>
                      查看详情
                    </Button>
                    <Button type="link" onClick={() => navigate(`/grades/student/${student.id}`)}>
                      查看成绩
                    </Button>
                  </div>
                </List.Item>
              )}
            />
            {students.length > 10 && (
              <Button type="link" onClick={() => navigate('/students')}>
                查看全部学生
              </Button>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default TeacherDashboard; 