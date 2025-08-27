import React, { useEffect, useState } from 'react';
import { Card, Table, Statistic, Row, Col, Progress, Tag, Space, Descriptions, Modal, Button, message } from 'antd';
import type { Key } from 'antd/es/table/interface';
import { TrophyOutlined, EyeOutlined, BarChartOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';

interface Grade {
  id: number;
  courseName: string;
  assignmentTitle?: string;
  score: number;
  grade?: string;
  maxScore?: number;
  weight?: number;
  feedback?: string;
  gradedAt?: string;
  courseId?: number;
}

interface CourseGrade {
  courseId: number;
  courseName: string;
  averageScore: number;
  totalAssignments: number;
  completedAssignments: number;
  grades: Grade[];
}

const StudentGrades: React.FC = () => {
  const { user } = useAuth();
  const [grades, setGrades] = useState<Grade[]>([]);
  const [courseGrades, setCourseGrades] = useState<CourseGrade[]>([]);
  const [loading, setLoading] = useState(false);
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [selectedGrade, setSelectedGrade] = useState<Grade | null>(null);

  const fetchGrades = async () => {
    setLoading(true);
    try {
      console.log('获取学生成绩数据');
      const res = await api.get('/students/my-grades');
      console.log('学生成绩数据:', res.data);
      setGrades(res.data);
      
      // 按课程分组计算平均分
      const courseMap = new Map<number, CourseGrade>();
      
      res.data.forEach((grade: Grade) => {
        if (!courseMap.has(grade.courseId!)) {
          courseMap.set(grade.courseId!, {
            courseId: grade.courseId!,
            courseName: grade.courseName,
            averageScore: 0,
            totalAssignments: 0,
            completedAssignments: 0,
            grades: []
          });
        }
        
        const courseGrade = courseMap.get(grade.courseId!)!;
        courseGrade.grades.push(grade);
        courseGrade.totalAssignments++;
        courseGrade.completedAssignments++;
      });
      
      // 计算每门课程的平均分
      courseMap.forEach(course => {
        const totalScore = course.grades.reduce((sum, grade) => sum + grade.score, 0);
        course.averageScore = course.grades.length > 0 ? totalScore / course.grades.length : 0;
      });
      
      setCourseGrades(Array.from(courseMap.values()));
    } catch (err: any) {
      console.error('获取成绩列表失败:', err);
      message.error('获取成绩列表失败: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchGrades();
  }, []);

  const handleViewDetail = (grade: Grade) => {
    setSelectedGrade(grade);
    setDetailModalVisible(true);
  };

  const getGradeColor = (score: number) => {
    if (score >= 90) return '#52c41a';
    if (score >= 80) return '#1890ff';
    if (score >= 70) return '#faad14';
    if (score >= 60) return '#fa8c16';
    return '#f5222d';
  };

  const getGradeTag = (score: number) => {
    if (score >= 90) return <Tag color="green">优秀</Tag>;
    if (score >= 80) return <Tag color="blue">良好</Tag>;
    if (score >= 70) return <Tag color="orange">中等</Tag>;
    if (score >= 60) return <Tag color="gold">及格</Tag>;
    return <Tag color="red">不及格</Tag>;
  };

  const getOverallAverage = () => {
    if (grades.length === 0) return 0;
    const totalScore = grades.reduce((sum, grade) => sum + grade.score, 0);
    return totalScore / grades.length;
  };

  const columns = [
    { 
      title: '课程', 
      dataIndex: 'courseName', 
      key: 'courseName'
    },
    { 
      title: '作业/考试', 
      dataIndex: 'assignmentTitle', 
      key: 'assignmentTitle',
      render: (text: string) => text || '课程总评'
    },
    { 
      title: '得分', 
      dataIndex: 'score', 
      key: 'score',
      render: (score: number, record: Grade) => (
        <Space>
          <span style={{ color: getGradeColor(score), fontWeight: 'bold' }}>
            {score}分
          </span>
          {record.maxScore && <span style={{ color: '#999' }}>/ {record.maxScore}分</span>}
        </Space>
      ),
      sorter: (a: Grade, b: Grade) => a.score - b.score
    },
    { 
      title: '等级', 
      key: 'grade',
      render: (_: any, record: Grade) => getGradeTag(record.score)
    },
    { 
      title: '权重', 
      dataIndex: 'weight', 
      key: 'weight',
      render: (weight: number) => weight ? `${weight}%` : '-'
    },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Grade) => (
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

  return (
    <div style={{ padding: '24px' }}>
      {/* 统计卡片 */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="总平均分"
              value={getOverallAverage()}
              precision={1}
              prefix={<TrophyOutlined />}
              suffix="分"
              valueStyle={{ color: getGradeColor(getOverallAverage()) }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="已评成绩"
              value={grades.length}
              prefix={<BarChartOutlined />}
              suffix="项"
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="优秀成绩"
              value={grades.filter(g => g.score >= 90).length}
              prefix={<TrophyOutlined />}
              suffix="项"
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="不及格"
              value={grades.filter(g => g.score < 60).length}
              prefix={<TrophyOutlined />}
              suffix="项"
              valueStyle={{ color: '#f5222d' }}
            />
          </Card>
        </Col>
      </Row>

      {/* 课程成绩概览 */}
      <Card title="课程成绩概览" style={{ marginBottom: '24px' }}>
        <Row gutter={[16, 16]}>
          {courseGrades.map(course => (
            <Col span={8} key={course.courseId}>
              <Card size="small">
                <div style={{ textAlign: 'center' }}>
                  <h4>{course.courseName}</h4>
                  <Progress
                    type="circle"
                    percent={Math.round(course.averageScore)}
                    format={percent => `${percent}分`}
                    strokeColor={getGradeColor(course.averageScore)}
                  />
                  <p style={{ marginTop: '8px', color: '#666' }}>
                    已完成 {course.completedAssignments}/{course.totalAssignments} 项
                  </p>
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      </Card>

      {/* 成绩详情表格 */}
      <Card title="成绩详情">
        <Table
          dataSource={grades}
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

      {/* 成绩详情模态框 */}
      <Modal
        title="成绩详情"
        open={detailModalVisible}
        onCancel={() => setDetailModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setDetailModalVisible(false)}>
            关闭
          </Button>
        ]}
        width={600}
      >
        {selectedGrade && (
          <Descriptions bordered column={1}>
            <Descriptions.Item label="课程">{selectedGrade.courseName}</Descriptions.Item>
            <Descriptions.Item label="作业/考试">{selectedGrade.assignmentTitle || '课程总评'}</Descriptions.Item>
            <Descriptions.Item label="得分">
              <span style={{ color: getGradeColor(selectedGrade.score), fontWeight: 'bold', fontSize: '16px' }}>
                {selectedGrade.score}分
              </span>
              {selectedGrade.maxScore && <span style={{ color: '#999' }}> / {selectedGrade.maxScore}分</span>}
            </Descriptions.Item>
            <Descriptions.Item label="等级">{getGradeTag(selectedGrade.score)}</Descriptions.Item>
            <Descriptions.Item label="权重">{selectedGrade.weight ? `${selectedGrade.weight}%` : '-'}</Descriptions.Item>
            {selectedGrade.feedback && (
              <Descriptions.Item label="教师反馈">
                <div style={{ whiteSpace: 'pre-wrap' }}>{selectedGrade.feedback}</div>
              </Descriptions.Item>
            )}
            <Descriptions.Item label="评阅时间">
              {selectedGrade.gradedAt ? new Date(selectedGrade.gradedAt).toLocaleString() : '-'}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>
    </div>
  );
};

export default StudentGrades; 