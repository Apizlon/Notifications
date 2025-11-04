import React, { useState } from 'react';
import { useAuth } from '../AuthContext';
import NavigationHeader from '../components/NavigationHeader';
import { sendTestNotification } from '../api';
import './MainPage.css';

const MainPage: React.FC = () => {
  const { user, unreadCount, logout } = useAuth();
  const [showTestForm, setShowTestForm] = useState(false);
  const [formData, setFormData] = useState({
    title: '',
    message: '',
    type: 0
  });
  const [sending, setSending] = useState(false);
  const [error, setError] = useState('');

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'type' ? Number(value) : value
    }));
    if (error) setError('');
  };

  const handleSendTest = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user?.token || !user.id) return;

    setSending(true);
    setError('');

    try {
      await sendTestNotification(user.token, user.id, formData);
      alert('Test notification sent successfully!');
      setFormData({ title: '', message: '', type: 0 });
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send notification');
    } finally {
      setSending(false);
    }
  };

  const handleProfileClick = () => {
    // Placeholder for profile functionality
    alert('Profile page coming soon!');
  };

  if (!user) {
    return null;
  }

  return (
    <div className="main-page">
      <NavigationHeader 
        title="Main" 
        showProfile={true}
        showNotifications={true}
        unreadCount={unreadCount}
        onProfileClick={handleProfileClick}
        onLogout={logout}
      />
      
      <main className="main-content">
        <section className="welcome-section">
          <h1>Welcome, {user.username}!</h1>
          <p className="welcome-text">
            Dashboard with {unreadCount} unread notifications. 
            Use the bell icon to view recent notifications.
          </p>
          
          <div className="action-buttons">
            <button 
              onClick={() => window.location.href = '/notifications'}
              className="primary-button"
            >
              View Notifications ({unreadCount})
            </button>
            <button 
              onClick={() => setShowTestForm(!showTestForm)}
              className="secondary-button"
            >
              {showTestForm ? 'Hide Test Form' : 'Send Test Notification'}
            </button>
          </div>
        </section>

        {showTestForm && (
          <section className="test-form-section">
            <h2>Send Test Notification</h2>
            <form onSubmit={handleSendTest} className="test-form">
              <div className="form-group">
                <label htmlFor="title">Title *</label>
                <input
                  id="title"
                  name="title"
                  type="text"
                  value={formData.title}
                  onChange={handleInputChange}
                  placeholder="Notification title"
                  required
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label htmlFor="message">Message *</label>
                <textarea
                  id="message"
                  name="message"
                  value={formData.message}
                  onChange={handleInputChange}
                  placeholder="Notification message"
                  rows={3}
                  required
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label htmlFor="type">Type</label>
                <select
                  id="type"
                  name="type"
                  value={formData.type}
                  onChange={handleInputChange}
                  className="form-input"
                >
                  <option value={0}>Info</option>
                  <option value={1}>Warning</option>
                  <option value={2}>Success</option>
                  <option value={3}>Error</option>
                </select>
              </div>

              {error && <div className="error-message">{error}</div>}

              <button 
                type="submit"
                disabled={sending || !formData.title || !formData.message}
                className="send-button"
              >
                {sending ? 'Sending...' : 'Send Test Notification'}
              </button>
            </form>
          </section>
        )}

        <section className="stats-section">
          <h2>Quick Stats</h2>
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-icon">🔔</div>
              <h3>Notifications</h3>
              <p className="stat-number">{unreadCount}</p>
              <p>Unread notifications</p>
            </div>
            <div className="stat-card">
              <div className="stat-icon">👤</div>
              <h3>Profile</h3>
              <p>Manage your account</p>
            </div>
            <div className="stat-card">
              <div className="stat-icon">⚙️</div>
              <h3>Settings</h3>
              <p>App preferences</p>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
};

export default MainPage;
