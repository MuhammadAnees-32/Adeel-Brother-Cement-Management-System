import { useState, type FormEvent } from 'react';
import { Navigate } from 'react-router-dom';
import { api } from '@/api/client';
import { useAuth, type AppScreen } from './AuthContext';

export function LoginPage() {
  const { user, isLoading, login } = useAuth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isLoading) return <div className="page-loading">Checking session...</div>;
  if (user) return <Navigate to="/" replace />;

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const response = await api.login({ username, password });
      sessionStorage.setItem('abc_auth', JSON.stringify({
        token: response.token,
        username: response.username,
        role: response.role,
        allowedScreens: response.allowedScreens,
      }));
      const me = await api.getMe();
      login({
        username: me.username,
        role: me.role as 'Admin' | 'Salesman',
        allowedScreens: me.allowedScreens as AppScreen[],
        token: response.token,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={handleSubmit}>
        <div className="login-brand">
          <h1>Adeel & Brother</h1>
          <p>Cement & Sirya Agency</p>
        </div>
        <h2>Sign In</h2>
        {error && <div className="form-error">{error}</div>}
        <label>
          Username
          <input
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            autoComplete="username"
            required
          />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
            required
          />
        </label>
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Signing in...' : 'Sign In'}
        </button>
        <p className="login-hint">Default admin: admin / Admin@123</p>
      </form>
    </div>
  );
}
