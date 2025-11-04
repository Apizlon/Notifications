import axios from 'axios';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';
import type { PaginatedNotifications, SendNotificationData } from './types';

const USER_API = 'http://localhost:5001/api';
const NOTIFICATION_API = 'http://localhost:5002/api';

let signalRConnection: HubConnection | null = null;

export const getSignalRConnection = (token: string) => {
  if (signalRConnection?.state && signalRConnection.state !== 0) {
    return signalRConnection;
  }

  signalRConnection = new HubConnectionBuilder()
    .withUrl(`${NOTIFICATION_API.replace('/api', '')}/notificationHub`, {
      accessTokenFactory: () => token
    })
    .configureLogging(LogLevel.Information)
    .build();

  return signalRConnection;
};

export const startSignalR = async (token: string) => {
  if (!signalRConnection) {
    signalRConnection = getSignalRConnection(token);
  }

  if (signalRConnection.state !== 0) {
    await signalRConnection.start();
    console.log('SignalR connected');
  }

  return signalRConnection;
};

export const stopSignalR = async () => {
  if (signalRConnection && signalRConnection.state === 1) {
    await signalRConnection.stop();
    console.log('SignalR disconnected');
  }
  signalRConnection = null;
};

// Auth API
export const register = (data: { username: string; password: string }) =>
  axios.post(`${USER_API}/auth/register`, data);

export const login = (data: { username: string; password: string }) =>
  axios.post<{ user: { id: string; username: string }; token: string }>(
    `${USER_API}/auth/login`,
    data
  );

// Notifications API
export const fetchLastThreeNotifications = (token: string) =>
  axios.get<Notification[]>(`${NOTIFICATION_API}/notification/last-three`, {
    headers: { Authorization: `Bearer ${token}` }
  });

export const fetchNotifications = (
  token: string,
  page = 1,
  pageSize = 10
) =>
  axios.get<PaginatedNotifications>(`${NOTIFICATION_API}/notification`, {
    params: { page, pageSize },
    headers: { Authorization: `Bearer ${token}` }
  });

export const markAsRead = (token: string, id: string) =>
  axios.put(`${NOTIFICATION_API}/notification/${id}/read`, {}, {
    headers: { Authorization: `Bearer ${token}` }
  });

export const sendTestNotification = (
  token: string,
  userId: string,
  data: SendNotificationData
) =>
  axios.post(
    `${USER_API}/notification/send`,
    {
      userIds: [userId],
      title: data.title,
      message: data.message,
      type: data.type
    },
    { headers: { Authorization: `Bearer ${token}` } }
  );

// SignalR methods
export const invokeGetUnreadCount = (connection: HubConnection) =>
  connection.invoke('getUnreadCount');

export const invokeMarkAsRead = (connection: HubConnection, id: string) =>
  connection.invoke('markAsRead', id);
