import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './AuthContext';
import LoginPage from './pages/LoginPage';
import MainPage from './pages/MainPage';
import NotificationsPage from './pages/NotificationsPage';
import './App.css';

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { user, loading } = useAuth();
  
  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <div>Loading...</div>
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

const AuthRedirect = ({ children }: { children: React.ReactNode }) => {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <div>Loading...</div>
      </div>
    );
  }

  if (user) {
    return <Navigate to="/main" replace />;
  }

  return <>{children}</>;
};

const AppContent = () => (
  <Router>
    <div className="App">
      <Routes>
        <Route path="/login" element={<AuthRedirect><LoginPage /></AuthRedirect>} />
        <Route 
          path="/main" 
          element={
            <ProtectedRoute>
              <MainPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/notifications" 
          element={
            <ProtectedRoute>
              <NotificationsPage />
            </ProtectedRoute>
          } 
        />
        <Route path="/" element={<Navigate to="/main" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </div>
  </Router>
);

const App = () => (
  <AuthProvider>
    <AppContent />
  </AuthProvider>
);

export default App;
