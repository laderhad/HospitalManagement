import { createContext, useCallback, useContext, useState, useEffect } from 'react';
import { UsersClient, LoginRequest, RegisterPatientRequest } from '../../web-api-client';

const AuthContext = createContext(null);

const client = new UsersClient();

export const ROLES = {
  administrator: 'Administrator',
  doctor: 'Doctor',
  patient: 'Patient'
};

export function hasAnyRole(user, roles = []) {
  if (!roles.length) return true;
  return roles.some(role => user?.roles?.includes(role));
}

export function defaultRouteForUser(user) {
  if (user?.roles?.includes(ROLES.administrator)) return '/admin';
  if (user?.roles?.includes(ROLES.doctor)) return '/doctor';
  if (user?.roles?.includes(ROLES.patient)) return '/patient';
  return '/login';
}

export function AuthProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState(null);

  const loadCurrentUser = useCallback(() =>
    client.getCurrentUser()
      .then(currentUser => {
        setUser(currentUser);
        setIsAuthenticated(true);
        return currentUser;
      }), []);

  useEffect(() => {
    loadCurrentUser()
      .catch(() => {
        setUser(null);
        setIsAuthenticated(false);
      })
      .finally(() => setIsLoading(false));
  }, [loadCurrentUser]);

  const login = (email, password) =>
    client.login(true, undefined, new LoginRequest({ email, password }))
      .then(loadCurrentUser);

  const register = patientRegistration =>
    client.registerPatient(new RegisterPatientRequest(patientRegistration));

  const logout = () =>
    client.logout({})
      .then(() => {
        setUser(null);
        setIsAuthenticated(false);
      });

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
