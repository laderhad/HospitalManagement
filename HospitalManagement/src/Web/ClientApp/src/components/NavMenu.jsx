import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from './api-authorization/AuthContext';
import { ThemeToggle } from './ThemeToggle';

function AuthLinks() {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async (e) => {
    e.preventDefault();
    await logout();
    navigate('/login');
  };

  if (isAuthenticated) {
    return <li><a href="#" onClick={handleLogout}>Log out</a></li>;
  }
  return (
    <>
      <li><Link to="/login">Log in</Link></li>
      <li><Link to="/register">Register</Link></li>
    </>
  );
}

export function TopNavigation() {
  return (
    <header className="top-navigation">
      <nav className="top-navigation-inner" aria-label="Hospital management">
        <ul className="brand-links">
          <li>
            <Link to="/">
              <strong>HospitalManagement</strong>
              <span>Administration</span>
            </Link>
          </li>
        </ul>
        <ul className="workspace-links">
          <li><AdminNavLink to="/" label="Dashboard" end /></li>
          <li><AdminNavLink to="/departments" label="Departments" /></li>
          <li><AdminNavLink to="/doctors" label="Doctors" /></li>
          <li><AdminNavLink to="/patients" label="Patients" /></li>
          <li><AdminNavLink to="/appointments" label="Appointments" /></li>
        </ul>
        <ul className="session-links">
          <AuthLinks />
          <li aria-hidden="true" className="nav-separator"></li>
          <li><ThemeToggle /></li>
        </ul>
      </nav>
    </header>
  );
}

export const NavMenu = TopNavigation;

function AdminNavLink({ label, ...props }) {
  return (
    <NavLink {...props} className={({ isActive }) => isActive ? 'active' : undefined}>
      <span>{label}</span>
    </NavLink>
  );
}
