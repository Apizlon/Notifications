import React, { createContext, useContext, useState, useEffect, useRef } from 'react';
import type { User, Notification } from './types';
import { 
  getSignalRConnection, 
  startSignalR, 
  stopSignalR, 
  invokeGetUnreadCount 
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

          // Setup event handlers
          connection.on('receiveUnreadCount', (count: number) => {
            setUnreadCount(count);
          });

          connection.on('receiveNotification', (notification: Notification) => {
            setUnreadCount(prev => prev + 1);
          });

          connection.on('notificationRead', (notificationId: string) => {
            setUnreadCount(prev => Math.max(0, prev - 1));
          });

          // Get initial unread count
          await invokeGetUnreadCount(connection);
        } catch (error) {
          console.error('SignalR setup error:', error);
        }
      };

      setupConnection();
    }

    // Cleanup on logout
    return () => {
      if (signalRConnection) {
        stopSignalR();
        setSignalRConnection(null);
      }
    };
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

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
