export interface User {
  id: string;
  username: string;
  token: string;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: number; // 0=info, 1=warning, 2=success, 3=error
  createdAt: string;
  isRead: boolean;
}

export interface LoginResponse {
  user: {
    id: string;
    username: string;
  };
  token: string;
}

export interface PaginatedNotifications {
  notifications: Notification[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface SendNotificationData {
  title: string;
  message: string;
  type: number;
}
