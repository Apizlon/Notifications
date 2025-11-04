import React from 'react';
import { useNavigate } from 'react-router-dom';
import NotificationsBell from './NotificationsBell';
import './NavigationHeader.css';

interface NavigationHeaderProps {
  title: string;
  showBackButton?: boolean;
  backTo?: string;
  showProfile?: boolean;
  showNotifications?: boolean;
  unreadCount?: number;
  onProfileClick?: () => void;
  onLogout?: () => void;
}

const NavigationHeader: React.FC<NavigationHeaderProps> = ({
  title,
  showBackButton = false,
  backTo = '/',
  showProfile = false,
  showNotifications = true,
  unreadCount = 0,
  onProfileClick,
  onLogout
}) => {
  const navigate = useNavigate();

  const handleBack = () => {
    if (backTo) {
      navigate(backTo);
    } else {
      navigate(-1);
    }
  };

  const handleProfile = (e: React.MouseEvent) => {
    e.stopPropagation();
    onProfileClick?.();
  };

  const handleLogout = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (window.confirm('Are you sure you want to logout?')) {
      onLogout?.();
    }
  };

  return (
    <header className="navigation-header">
      <div className="header-left">
        {showBackButton && (
          <button onClick={handleBack} className="back-button" aria-label="Go back">
            <span className="back-icon">←</span>
            <span className="back-text">Back</span>
          </button>
        )}
        <h1 className="header-title">{title}</h1>
      </div>

      <div className="header-right">
        {showProfile && (
          <button 
            onClick={handleProfile}
            className="profile-button"
            aria-label="Profile"
          >
            👤
          </button>
        )}

        {showNotifications && <NotificationsBell unreadCount={unreadCount} />}

        {onLogout && (
          <button 
            onClick={handleLogout}
            className="logout-button"
            aria-label="Logout"
          >
            Logout
          </button>
        )}
      </div>
    </header>
  );
};

export default NavigationHeader;
