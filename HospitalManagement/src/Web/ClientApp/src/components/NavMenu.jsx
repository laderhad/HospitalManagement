import { Link, NavLink, useNavigate } from 'react-router-dom';
import { ROLES, useAuth } from './api-authorization/AuthContext';
import { ThemeToggle } from './ThemeToggle';

function AuthLinks() {
  const { isAuthenticated, user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async (e) => {
    e.preventDefault();
    await logout();
    navigate('/login');
  };

  if (isAuthenticated) {
    return (
      <>
        {user?.email && <li><span className="session-email">{user.email}</span></li>}
        <li><a href="#" onClick={handleLogout}>Log out</a></li>
      </>
    );
  }
  return (
    <>
      <li><Link to="/login">Log in</Link></li>
      <li><Link to="/register">Register</Link></li>
    </>
  );
}

export function TopNavigation() {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];
  const isAdmin = roles.includes(ROLES.administrator);
  const isDoctor = roles.includes(ROLES.doctor);
  const isPatient = roles.includes(ROLES.patient);

  return (
    <header className="top-navigation">
      <nav className="top-navigation-inner" aria-label="Hospital management">
        <ul className="brand-links">
          <li>
            <Link to="/">
              <strong>HospitalManagement</strong>
              <span>Clinical operations</span>
            </Link>
          </li>
        </ul>
        <ul className="workspace-links">
          {isAuthenticated && isAdmin && (
            <>
              <li><AdminNavLink to="/admin" label="Dashboard" end /></li>
              <li><AdminNavLink to="/departments" label="Departments" /></li>
              <li><AdminNavLink to="/doctors" label="Doctors" /></li>
              <li><AdminNavLink to="/patients" label="Patients" /></li>
              <li><AdminNavLink to="/appointments" label="Appointments" /></li>
              <li><AdminNavLink to="/lab-results" label="Lab results" /></li>
            </>
          )}
          {isAuthenticated && isDoctor && <li><AdminNavLink to="/doctor" label="Doctor workspace" /></li>}
          {isAuthenticated && isPatient && <li><AdminNavLink to="/patient" label="Patient portal" /></li>}
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
