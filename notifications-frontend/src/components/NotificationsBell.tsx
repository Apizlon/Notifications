import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../AuthContext';
import { fetchLastThreeNotifications, markAsRead } from '../api';
import type { Notification } from '../types';
import './NotificationsBell.css';

interface NotificationsBellProps {
  unreadCount: number;
}

const NotificationsBell: React.FC<NotificationsBellProps> = ({ unreadCount }) => {
  const { user, signalRConnection } = useAuth();
  const navigate = useNavigate();
  const [showDropdown, setShowDropdown] = useState(false);
  const [recentNotifications, setRecentNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown on outside click
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowDropdown(false);
      }
    };

    if (showDropdown) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => document.removeEventListener('mousedown', handleClickOutside);
    }
  }, [showDropdown]);

  // Load recent notifications when dropdown opens
  const loadRecentNotifications = async () => {
    if (!user?.token || loading) return;
    
    setLoading(true);
    try {
      const response = await fetchLastThreeNotifications(user.token);
      setRecentNotifications(response.data || []);
    } catch (error) {
      console.error('Error loading recent notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleToggle = async () => {
    if (!showDropdown) {
      setShowDropdown(true);
      await loadRecentNotifications();
    } else {
      setShowDropdown(false);
    }
  };

  const handleViewAll = () => {
    setShowDropdown(false);
    navigate('/notifications');
  };

  const handleMarkAsRead = async (id: string) => {
    if (!user?.token) return;
    
    try {
      await markAsRead(user.token, id);
      setRecentNotifications(prev =>
        prev.map(notif =>
          notif.id === id ? { ...notif, isRead: true } : notif
        )
      );
    } catch (error) {
      console.error('Error marking as read:', error);
    }
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMinutes = (now.getTime() - date.getTime()) / (1000 * 60);
    
    if (diffInMinutes < 1) return 'Now';
    if (diffInMinutes < 60) return `${Math.floor(diffInMinutes)}m ago`;
    if (diffInMinutes < 1440) return `${Math.floor(diffInMinutes / 60)}h ago`;
    return date.toLocaleDateString('ru-RU', { 
      day: 'numeric', 
      month: 'short' 
    });
  };

  const truncateText = (text: string, maxLength: number = 60) => {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength).trim() + '...';
  };

  const getIcon = (type: number) => {
    switch (type) {
      case 0: return 'ℹ️';
      case 1: return '⚠️';
      case 2: return '✅';
      case 3: return '❌';
      default: return '🔔';
    }
  };

  if (!user) return null;

  return (
    <div className="notifications-bell" ref={dropdownRef}>
      <button 
        onClick={handleToggle}
        className={`bell-button ${showDropdown ? 'active' : ''}`}
        aria-label={`Notifications (${unreadCount})`}
        title={`You have ${unreadCount} unread notifications`}
      >
        🔔
        {unreadCount > 0 && (
          <span className="notification-badge">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {showDropdown && (
        <div className="dropdown-menu">
          <div className="dropdown-header">
            <h3>Notifications</h3>
            <button onClick={handleViewAll} className="view-all-btn">
              View all
            </button>
          </div>

          <div className="dropdown-content">
            {loading ? (
              <div className="loading-item">Loading...</div>
            ) : recentNotifications.length > 0 ? (
              recentNotifications.map((notification) => {
                const isUnread = !notification.isRead;
                const truncatedMessage = truncateText(notification.message);
                
                return (
                  <div 
                    key={notification.id} 
                    className={`notification-item ${isUnread ? 'unread' : ''}`}
                  >
                    <div className="notification-icon">
                      {getIcon(notification.type)}
                    </div>
                    
                    <div className="notification-details">
                      <div className="notification-title">
                        <strong>{notification.title}</strong>
                      </div>
                      <div className="notification-message">
                        {truncatedMessage}
                      </div>
                      <div className="notification-meta">
                        <span className="notification-time">
                          {formatDate(notification.createdAt)}
                        </span>
                        {isUnread && (
                          <button 
                            className="mark-read-small"
                            onClick={() => handleMarkAsRead(notification.id)}
                            title="Mark as read"
                          >
                            ✓
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })
            ) : (
              <div className="no-notifications">
                No recent notifications
              </div>
            )}
          </div>

          {recentNotifications.length > 0 && (
            <div className="dropdown-footer">
              <button onClick={handleViewAll} className="view-all-footer">
                See all notifications ({unreadCount} unread)
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default NotificationsBell;
