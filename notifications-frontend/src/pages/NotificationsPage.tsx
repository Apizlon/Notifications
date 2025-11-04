import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useAuth } from '../AuthContext';
import NavigationHeader from '../components/NavigationHeader';
import { fetchNotifications, markAsRead } from '../api';
import type { Notification } from '../types';
import './NotificationsPage.css';

const NotificationsPage: React.FC = () => {
  const { user, unreadCount, signalRConnection } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [selectedNotification, setSelectedNotification] = useState<Notification | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const pageSize = 10;
  const scrollRef = useRef<HTMLDivElement>(null);

  // Локальный счётчик непрочитанных на текущей странице
  const unreadOnPage = notifications.filter(n => !n.isRead).length;

  const loadNotifications = useCallback(async (currentPage = 1, append = false) => {
    if (!user?.token) {
      setLoading(false);
      return;
    }

    if (!append) {
      setLoading(true);
    } else {
      setLoadingMore(true);
    }

    try {
      const response = await fetchNotifications(user.token, currentPage, pageSize);
      const { notifications: newNotifications, totalCount: newTotal } = response.data;

      setTotalCount(newTotal);

      if (append) {
        // При добавлении новых страниц не смешиваем с непрочитанными
        setNotifications(prev => [...prev, ...newNotifications]);
      } else {
        setNotifications(newNotifications);
        setPage(currentPage);
        
        // Сбрасываем выделение если его нет в новом списке
        if (selectedNotification && !newNotifications.find(n => n.id === selectedNotification.id)) {
          setSelectedNotification(null);
        }
      }

      setHasMore(newNotifications.length === pageSize && (currentPage * pageSize) < newTotal);
    } catch (error) {
      console.error('Error loading notifications:', error);
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, [user?.token, selectedNotification, pageSize]);

  // Перезагружаем первую страницу при изменении общего количества непрочитанных
  useEffect(() => {
    if (page === 1 && totalCount !== 0 && unreadCount !== unreadOnPage) {
      // Если общее количество отличается от локального, обновляем список
      loadNotifications(1, false);
    }
  }, [unreadCount, totalCount, unreadOnPage, page]);

  // SignalR обработчики для новых уведомлений
  useEffect(() => {
    if (!signalRConnection) return;

    const handleNewNotification = () => {
      console.log('New notification received, reloading page 1');
      // Всегда перезагружаем первую страницу при новом уведомлении
      if (page === 1) {
        loadNotifications(1, false);
      }
    };

    const handleUnreadUpdated = () => {
      console.log('Unread count updated, checking if reload needed');
      // Перезагружаем если общее количество изменилось
      if (page === 1) {
        loadNotifications(1, false);
      }
    };

    signalRConnection.on('receiveNotification', handleNewNotification);
    signalRConnection.on('receiveUnreadCount', handleUnreadUpdated);
    signalRConnection.on('unreadCountUpdated', handleUnreadUpdated);
    signalRConnection.on('notificationRead', handleUnreadUpdated);

    return () => {
      signalRConnection.off('receiveNotification', handleNewNotification);
      signalRConnection.off('receiveUnreadCount', handleUnreadUpdated);
      signalRConnection.off('unreadCountUpdated', handleUnreadUpdated);
      signalRConnection.off('notificationRead', handleUnreadUpdated);
    };
  }, [signalRConnection, page, loadNotifications]);

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMinutes = (now.getTime() - date.getTime()) / (1000 * 60);
    const diffInHours = diffInMinutes / 60;
    
    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${Math.floor(diffInMinutes)}m ago`;
    if (diffInHours < 24) return `${Math.floor(diffInHours)}h ago`;
    return date.toLocaleDateString('ru-RU', { 
      day: 'numeric', 
      month: 'short', 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  };

  const getNotificationIcon = (type: number) => {
    switch (type) {
      case 0: return 'ℹ️';
      case 1: return '⚠️';
      case 2: return '✅';
      case 3: return '❌';
      default: return '🔔';
    }
  };

  // Загружаем уведомления при монтировании
  useEffect(() => {
    loadNotifications(1, false);
  }, [loadNotifications]);

  // Скролл для подгрузки
  useEffect(() => {
    const handleScroll = () => {
      if (scrollRef.current && !loadingMore && hasMore) {
        const { scrollTop, scrollHeight, clientHeight } = scrollRef.current;
        if (scrollHeight - scrollTop - clientHeight < 100) {
          loadNotifications(page + 1, true);
        }
      }
    };

    const element = scrollRef.current;
    if (element) {
      element.addEventListener('scroll', handleScroll);
      return () => element.removeEventListener('scroll', handleScroll);
    }
  }, [page, hasMore, loadingMore, loadNotifications]);

  const handleMarkAsRead = async (id: string) => {
    if (!user?.token) return;

    try {
      await markAsRead(user.token, id);
      
      // Обновляем локальное состояние
      setNotifications(prev =>
        prev.map(n =>
          n.id === id ? { ...n, isRead: true } : n
        )
      );

      if (selectedNotification?.id === id) {
        setSelectedNotification(prev => prev ? { ...prev, isRead: true } : null);
      }

      // Перезагружаем первую страницу для синхронизации
      if (page === 1) {
        loadNotifications(1, false);
      }
    } catch (error) {
      console.error('Error marking as read:', error);
      // При ошибке перезагружаем для восстановления состояния
      if (page === 1) {
        loadNotifications(1, false);
      }
    }
  };

  const handleSelectNotification = (notification: Notification) => {
    setSelectedNotification(notification);
    
    // Mark as read if not already read
    if (!notification.isRead && user?.token) {
      handleMarkAsRead(notification.id);
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  if (loading && notifications.length === 0) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <div>Loading notifications...</div>
      </div>
    );
  }

  return (
    <div className="notifications-page">
      <NavigationHeader 
        title={`Notifications (${unreadCount} unread)`}
        showBackButton={true}
        backTo="/main"
      />
      
      <div className="page-container">
        <div className="sidebar">
          <div className="sidebar-header">
            <div className="header-content">
              <h2>All Notifications</h2>
              <span className="unread-badge">{unreadOnPage}</span>
            </div>
            {totalPages > 1 && (
              <div className="pagination-info">
                Page {page} of {totalPages} • {unreadCount} total unread
              </div>
            )}
          </div>

          <div className="notifications-list" ref={scrollRef}>
            {notifications.length === 0 && !loading ? (
              <div className="no-notifications">
                <div className="empty-icon">🔔</div>
                <p>No notifications yet</p>
              </div>
            ) : (
              notifications.map((notification) => {
                const isSelected = selectedNotification?.id === notification.id;
                const isUnread = !notification.isRead;
                
                return (
                  <div
                    key={notification.id}
                    onClick={() => handleSelectNotification(notification)}
                    className={`notification-item ${isSelected ? 'selected' : ''} ${isUnread ? 'unread' : ''}`}
                  >
                    <div className="notification-icon">
                      <span>{getNotificationIcon(notification.type)}</span>
                    </div>
                    
                    <div className="notification-content">
                      <div className="notification-title">
                        {notification.title}
                      </div>
                      <div className="notification-message">
                        {notification.message}
                      </div>
                    </div>
                    
                    <div className="notification-meta">
                      <div className={`status-dot ${isUnread ? 'unread' : 'read'}`}></div>
                      <div className="notification-time">
                        {formatDate(notification.createdAt)}
                      </div>
                      {isUnread && (
                        <button 
                          className="mark-read-btn"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleMarkAsRead(notification.id);
                          }}
                          title="Mark as read"
                        >
                          ✓
                        </button>
                      )}
                    </div>
                  </div>
                );
              })
            )}

            {loadingMore && (
              <div className="loading-more">
                <div className="loading-spinner-small"></div>
                Loading more...
              </div>
            )}

            {hasMore && !loadingMore && notifications.length > 0 && (
              <div className="load-more-section">
                <button 
                  onClick={() => loadNotifications(page + 1, true)}
                  className="load-more-button"
                >
                  Load more notifications
                </button>
              </div>
            )}

            {!hasMore && notifications.length > 0 && (
              <div className="no-more-notifications">
                No more notifications to load
              </div>
            )}
          </div>
        </div>

        <div className="content-area">
          {selectedNotification ? (
            <div className="notification-detail">
              <div className="detail-header">
                <span className="detail-icon">
                  {getNotificationIcon(selectedNotification.type)}
                </span>
                <div className="detail-meta">
                  <h1 className="detail-title">{selectedNotification.title}</h1>
                  <div className="detail-info">
                    <span className="detail-time">
                      {formatDate(selectedNotification.createdAt)}
                    </span>
                    {!selectedNotification.isRead && (
                      <button 
                        className="mark-read-detail"
                        onClick={() => handleMarkAsRead(selectedNotification.id)}
                      >
                        Mark as read
                      </button>
                    )}
                  </div>
                </div>
              </div>
              
              <div className="detail-content">
                <p>{selectedNotification.message}</p>
              </div>
            </div>
          ) : (
            <div className="empty-state">
              <div className="empty-icon">🔔</div>
              <h3>Select a notification</h3>
              <p>Click on any notification from the list to view details</p>
              {notifications.length === 0 && !loading && (
                <p>No notifications yet</p>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default NotificationsPage;
