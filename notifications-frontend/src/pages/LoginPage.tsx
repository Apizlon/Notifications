import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { register, login } from '../api';
import { useAuth } from '../AuthContext';
import type { User } from '../types';
import './LoginPage.css';

const LoginPage: React.FC = () => {
  const [isRegister, setIsRegister] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { setUser } = useAuth();
  const navigate = useNavigate();

  // Redirect if already logged in
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      navigate('/main', { replace: true });
    }
  }, [navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!username.trim() || !password.trim()) {
      setError('Please fill in all fields');
      return;
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    setError(null);
    setLoading(true);

    try {
      let user: User | null = null;

      if (isRegister) {
        // Register
        await register({ username: username.trim(), password });
        setError('Registration successful! Please log in.');
        setIsRegister(false);
        setLoading(false);
        return;
      } else {
        // Login
        const response = await login({ username: username.trim(), password });
        user = {
          id: response.data.user.id,
          username: response.data.user.username,
          token: response.data.token
        };

        // Save to localStorage
        localStorage.setItem('token', user.token);
        localStorage.setItem('user', JSON.stringify({
          id: user.id,
          username: user.username
        }));

        setUser(user);
        navigate('/main', { replace: true });
      }
    } catch (err: any) {
      console.error('Auth error:', err);
      setError(err.response?.data?.message || (isRegister ? 'Registration failed' : 'Invalid credentials'));
      setLoading(false);
    }
  };

  const toggleMode = () => {
    setIsRegister(!isRegister);
    setError(null);
    setUsername('');
    setPassword('');
  };

  return (
    <div className="login-container">
      <form onSubmit={handleSubmit} className="login-form">
        <h2>{isRegister ? 'Register' : 'Login'}</h2>
        
        <div className="form-group">
          <input
            type="text"
            value={username}
            onChange={(e) => {
              setUsername(e.target.value);
              if (error) setError(null);
            }}
            placeholder="Username"
            className="input-field"
            disabled={loading}
            required
          />
        </div>

        <div className="form-group">
          <input
            type="password"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value);
              if (error) setError(null);
            }}
            placeholder="Password"
            className="input-field"
            disabled={loading}
            required
          />
        </div>

        {error && <div className="error-message">{error}</div>}

        <button 
          type="submit" 
          disabled={loading || !username.trim() || !password.trim()}
          className="submit-button"
        >
          {loading ? 'Loading...' : (isRegister ? 'Register' : 'Login')}
        </button>

        <button
          type="button"
          onClick={toggleMode}
          disabled={loading}
          className="toggle-button"
        >
          {isRegister ? 'Already have an account? Login' : "Don't have an account? Register"}
        </button>
      </form>
    </div>
  );
};

export default LoginPage;
