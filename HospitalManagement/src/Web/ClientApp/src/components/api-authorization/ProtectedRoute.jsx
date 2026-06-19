import { Navigate, useLocation } from 'react-router-dom';
import { defaultRouteForUser, hasAnyRole, useAuth } from './AuthContext';

export function ProtectedRoute({ children, roles = [] }) {
  const { isAuthenticated, isLoading, user } = useAuth();
  const location = useLocation();

  if (isLoading) return null;
  if (!isAuthenticated) return <Navigate to="/login" state={{ returnUrl: location.pathname }} replace />;
  if (!hasAnyRole(user, roles)) return <Navigate to={defaultRouteForUser(user)} replace />;
  return children;
}
