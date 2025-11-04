import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useAuth } from '../AuthContext';
import NavigationHeader from '../components/NavigationHeader';
import { fetchNotifications, markAsRead } from '../api';
import type { Notification } from '../types';
import './NotificationsPage.css';

const NotificationsPage: React.FC = () => {
  const { user } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [selectedNotification, setSelectedNotification] = useState<Notification | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const pageSize = 10;
  const scrollRef = useRef<HTMLDivElement>(null);

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);
    
    if (diffInHours < 1) return 'Just now';
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
        setNotifications(prev => [...prev, ...newNotifications]);
      } else {
        setNotifications(newNotifications);
        setPage(currentPage);
        // Reset selection if not appending
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

  useEffect(() => {
    loadNotifications(1, false);
  }, [loadNotifications]);

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
      
      setNotifications(prev =>
        prev.map(notification =>
          notification.id === id 
            ? { ...notification, isRead: true }
            : notification
        )
      );

      if (selectedNotification?.id === id) {
        setSelectedNotification(prev => prev ? { ...prev, isRead: true } : null);
      }

      // Refresh first page to ensure consistency
      if (page === 1) {
        loadNotifications(1, false);
      }
    } catch (error) {
      console.error('Error marking as read:', error);
    }
  };

  const handleSelectNotification = (notification: Notification) => {
    setSelectedNotification(notification);
    
    // Mark as read if not already read
    if (!notification.isRead && user?.token) {
      handleMarkAsRead(notification.id);
    }
  };

  const unreadCount = notifications.filter(n => !n.isRead).length;
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
              <span className="unread-badge">{unreadCount}</span>
            </div>
            {totalPages > 1 && (
              <div className="pagination-info">
                Page {page} of {totalPages}
              </div>
            )}
          </div>

          <div className="notifications-list" ref={scrollRef}>
            {notifications.map((notification) => {
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
            })}

            {loadingMore && (
              <div className="loading-more">
                <div className="loading-spinner-small"></div>
                Loading more...
              </div>
            )}

            {hasMore && !loadingMore && (
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
                No more notifications
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
              {notifications.length === 0 && (
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
