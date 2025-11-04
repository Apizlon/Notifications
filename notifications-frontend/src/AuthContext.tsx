import React, { createContext, useContext, useState, useEffect, useRef } from 'react';
import type { User, Notification } from './types';
import { 
  getSignalRConnection, 
  startSignalR, 
  stopSignalR 
} from './api';

interface AuthContextType {
  user: User | null;
  setUser: (user: User | null) => void;
  unreadCount: number;
  loading: boolean;
  signalRConnection: any | null;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [signalRConnection, setSignalRConnection] = useState<any | null>(null);
  const connectionRef = useRef<any>(null);

  // Restore user from localStorage
  useEffect(() => {
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');
    
    if (token && userData) {
      try {
        const parsedUser = JSON.parse(userData);
        const restoredUser: User = {
          id: parsedUser.id,
          username: parsedUser.username,
          token
        };
        setUser(restoredUser);
        // НЕ запрашиваем unreadCount при восстановлении - ждём события от SignalR
      } catch (error) {
        console.error('Error restoring user:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    setLoading(false);
  }, []);

  // Setup SignalR when user logs in
  useEffect(() => {
    if (user?.token) {
      const setupConnection = async () => {
        try {
          const connection = getSignalRConnection(user.token);
          connectionRef.current = connection;
          await startSignalR(user.token);
          setSignalRConnection(connection);

          // Setup event handlers - регистрируем каждый раз, SignalR сам управляет дубликатами
          connection.on('receiveUnreadCount', (count: number) => {
            console.log('Received unread count via SignalR:', count);
            setUnreadCount(count);
          });

          connection.on('receiveNotification', (notification: Notification) => {
            console.log('New notification received via SignalR:', notification);
            setUnreadCount(prev => prev + 1);
          });

          connection.on('notificationRead', (notificationId: string) => {
            console.log('Notification marked as read via SignalR:', notificationId);
            setUnreadCount(prev => Math.max(0, prev - 1));
          });

          connection.on('unreadCountUpdated', (count: number) => {
            console.log('Unread count updated via SignalR:', count);
            setUnreadCount(count);
          });

          // НЕ вызываем invokeGetUnreadCount - сервер сам отправляет через события
          console.log('SignalR connected, waiting for unread count from server...');

        } catch (error) {
          console.error('SignalR setup error:', error);
          setSignalRConnection(null);
        }
      };

      setupConnection();

      // Cleanup
      return () => {
        if (signalRConnection && connectionRef.current) {
          // SignalR автоматически удаляет обработчики при stop()
          connectionRef.current.stop().catch(console.error);
        }
      };
    } else {
      // Cleanup when user logs out
      if (signalRConnection) {
        stopSignalR();
        setSignalRConnection(null);
        setUnreadCount(0);
      }
    }
  }, [user?.token]);

  const logout = () => {
    if (signalRConnection) {
      stopSignalR();
    }
    
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
    setUnreadCount(0);
    setSignalRConnection(null);
    window.location.href = '/login';
  };

  const value: AuthContextType = {
    user,
    setUser,
    unreadCount,
    loading,
    signalRConnection,
    logout
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <div>Loading...</div>
      </div>
    );
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
