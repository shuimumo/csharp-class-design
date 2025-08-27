import React, { useEffect, useState } from 'react';
import { Card, List, Avatar, Button, Modal, message, Tag, Space, Descriptions, Badge } from 'antd';
import { BellOutlined, EyeOutlined, CheckOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuth } from '../contexts/AuthContext';

interface Notification {
  id: number;
  title: string;
  content: string;
  type?: string;
  createdAt?: string;
  targetRole?: string;
  isRead?: boolean;
  targetUserName?: string;
}

const StudentNotifications: React.FC = () => {
  const { user } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(false);
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [selectedNotification, setSelectedNotification] = useState<Notification | null>(null);

  const fetchNotifications = async () => {
    setLoading(true);
    try {
      console.log('获取学生通知数据');
      const res = await api.get('/notifications/my-notifications');
      console.log('学生通知数据:', res.data);
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
  }, []);

  const handleViewDetail = async (notification: Notification) => {
    setSelectedNotification(notification);
    setDetailModalVisible(true);
    
    // 如果未读，标记为已读
    if (!notification.isRead) {
      try {
        await api.put(`/notifications/${notification.id}/mark-read`);
        // 更新本地状态
        setNotifications(prev => 
          prev.map(n => 
            n.id === notification.id ? { ...n, isRead: true } : n
          )
        );
      } catch (error) {
        console.error('标记已读失败:', error);
      }
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      // 获取所有未读通知
      const unreadNotifications = notifications.filter(n => !n.isRead);
      
      // 批量标记为已读
      await Promise.all(
        unreadNotifications.map(n => 
          api.put(`/notifications/${n.id}/mark-read`)
        )
      );
      
      // 更新本地状态
      setNotifications(prev => 
        prev.map(n => ({ ...n, isRead: true }))
      );
      
      message.success('已全部标记为已读');
    } catch (error) {
      console.error('批量标记已读失败:', error);
      message.error('操作失败');
    }
  };

  const getNotificationIcon = (type?: string) => {
    switch (type) {
      case 'assignment':
        return <Avatar icon={<BellOutlined />} style={{ backgroundColor: '#1890ff' }} />;
      case 'grade':
        return <Avatar icon={<BellOutlined />} style={{ backgroundColor: '#52c41a' }} />;
      case 'course':
        return <Avatar icon={<BellOutlined />} style={{ backgroundColor: '#faad14' }} />;
      default:
        return <Avatar icon={<BellOutlined />} style={{ backgroundColor: '#722ed1' }} />;
    }
  };

  const getNotificationTypeTag = (type?: string) => {
    switch (type) {
      case 'assignment':
        return <Tag color="blue">作业</Tag>;
      case 'grade':
        return <Tag color="green">成绩</Tag>;
      case 'course':
        return <Tag color="orange">课程</Tag>;
      default:
        return <Tag color="purple">通知</Tag>;
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      <Card 
        title={
          <Space>
            <span>我的通知</span>
            <Badge count={notifications.filter(n => !n.isRead).length} />
          </Space>
        } 
        extra={
          <Button 
            type="primary" 
            icon={<CheckOutlined />} 
            onClick={handleMarkAllAsRead}
            disabled={!notifications.some(n => !n.isRead)}
          >
            全部标记为已读
          </Button>
        }
        style={{ marginBottom: '24px' }}
      >
        <List
          loading={loading}
          dataSource={notifications}
          renderItem={(notification) => (
            <List.Item
              actions={[
                <Button 
                  key="view" 
                  type="link" 
                  icon={<EyeOutlined />} 
                  onClick={() => handleViewDetail(notification)}
                >
                  查看详情
                </Button>
              ]}
            >
              <List.Item.Meta
                avatar={getNotificationIcon(notification.type)}
                title={
                  <Space>
                    <span style={{ 
                      fontWeight: notification.isRead ? 'normal' : 'bold',
                      color: notification.isRead ? '#666' : '#000'
                    }}>
                      {notification.title}
                    </span>
                    {!notification.isRead && <Badge status="processing" />}
                    {getNotificationTypeTag(notification.type)}
                  </Space>
                }
                description={
                  <div>
                    <div style={{ 
                      color: notification.isRead ? '#999' : '#666',
                      marginBottom: '4px'
                    }}>
                      {notification.content.length > 100 
                        ? `${notification.content.substring(0, 100)}...` 
                        : notification.content
                      }
                    </div>
                    <div style={{ fontSize: '12px', color: '#999' }}>
                      {notification.createdAt && new Date(notification.createdAt).toLocaleString()}
                    </div>
                  </div>
                }
              />
            </List.Item>
          )}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `第 ${range[0]}-${range[1]} 条，共 ${total} 条`
          }}
        />
      </Card>

      {/* 通知详情模态框 */}
      <Modal
        title="通知详情"
        open={detailModalVisible}
        onCancel={() => setDetailModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setDetailModalVisible(false)}>
            关闭
          </Button>
        ]}
        width={600}
      >
        {selectedNotification && (
          <Descriptions bordered column={1}>
            <Descriptions.Item label="通知标题">{selectedNotification.title}</Descriptions.Item>
            <Descriptions.Item label="通知类型">{getNotificationTypeTag(selectedNotification.type)}</Descriptions.Item>
            <Descriptions.Item label="通知内容">
              <div style={{ whiteSpace: 'pre-wrap' }}>{selectedNotification.content}</div>
            </Descriptions.Item>
            <Descriptions.Item label="发布时间">
              {selectedNotification.createdAt && new Date(selectedNotification.createdAt).toLocaleString()}
            </Descriptions.Item>
            <Descriptions.Item label="阅读状态">
              {selectedNotification.isRead ? 
                <Tag color="green">已读</Tag> : 
                <Tag color="red">未读</Tag>
              }
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>
    </div>
  );
};

export default StudentNotifications; 