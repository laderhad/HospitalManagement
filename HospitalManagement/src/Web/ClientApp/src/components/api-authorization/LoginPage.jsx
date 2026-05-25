import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from './AuthContext';

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
    <article>
      <h2>Log in</h2>
      <form onSubmit={handleSubmit}>
        <label htmlFor="email">Email</label>
        <input type="email" id="email" autoComplete="username"
          value={email} onChange={handleChange(setEmail)}
          aria-invalid={error || undefined}
          aria-describedby={error ? 'login-error' : undefined} />
        <label htmlFor="password">Password</label>
        <input type="password" id="password" autoComplete="current-password"
          value={password} onChange={handleChange(setPassword)}
          aria-invalid={error || undefined}
          aria-describedby={error ? 'login-error' : undefined} />
        {error && <small id="login-error">{error}</small>}
        <button type="submit">Log in</button>
        <p style={{ marginTop: '1rem' }}>Don't have an account? <Link to="/register">Register</Link></p>
      </form>
    </article>
  );
}
