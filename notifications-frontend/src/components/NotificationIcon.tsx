import React from 'react';

interface NotificationIconProps {
  type: 'info' | 'warning' | 'success' | 'error' | 'default';
  className?: string;
}

export default function NotificationIcon({ type, className = '' }: NotificationIconProps) {
  const iconMap = {
    info: 'ℹ️',
    warning: '⚠️',
    success: '✅',
    error: '❌',
    default: '🔔'
  };

  const iconClass = `notification-icon-${type} ${className}`;

  return (
    <span className={iconClass} aria-hidden="true">
      {iconMap[type] || iconMap.default}
    </span>
  );
}
