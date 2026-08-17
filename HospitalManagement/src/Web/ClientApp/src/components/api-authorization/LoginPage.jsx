import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from './AuthContext';
import { Button, Input } from '../ui';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await login(email, password);
      const returnUrl = location.state?.returnUrl || '/';
      navigate(returnUrl, { replace: true });
    } catch (loginError) {
      setError(loginError?.status === 401
        ? 'Invalid email or password.'
        : 'Login service is not reachable. Start the backend with docker compose up --build from the HospitalManagement folder, then try again.');
    }
  };

  const handleChange = (setter) => (e) => {
    setError('');
    setter(e.target.value);
  };

  return (
    <article className="auth-card">
      <header className="auth-header">
        <h2>Log in</h2>
        <p>Continue to your hospital workspace.</p>
      </header>
      {location.state?.message && <p className="success">{location.state.message}</p>}
      <form className="detail-form" onSubmit={handleSubmit}>
        <Input label="Email" type="email" id="email" autoComplete="username"
          value={email} onChange={handleChange(setEmail)}
          aria-invalid={error || undefined}
          aria-describedby={error ? 'login-error' : undefined} />
        <Input label="Password" type="password" id="password" autoComplete="current-password"
          value={password} onChange={handleChange(setPassword)}
          aria-invalid={error || undefined}
          aria-describedby={error ? 'login-error' : undefined} />
        {error && <small id="login-error">{error}</small>}
        <Button type="submit">Log in</Button>
        <p className="auth-switch">Don't have an account? <Link to="/register">Register</Link></p>
      </form>
    </article>
  );
}
