import { Navigate } from "react-router-dom";
import { LoginPage } from "./components/api-authorization/LoginPage";
import { RegisterPage } from "./components/api-authorization/RegisterPage";
import { ProtectedRoute } from "./components/api-authorization/ProtectedRoute";
import { ROLES, defaultRouteForUser, useAuth } from "./components/api-authorization/AuthContext";
import { AdminDashboard } from "./components/admin/AdminDashboard";
import { AdminLabResultsPage } from "./components/admin/AdminLabResultsPage";
import { AppointmentsAdminPage } from "./components/admin/AppointmentsAdminPage";
import { DepartmentsAdminPage } from "./components/admin/DepartmentsAdminPage";
import { DoctorsAdminPage } from "./components/admin/DoctorsAdminPage";
import { PatientsAdminPage } from "./components/admin/PatientsAdminPage";
import { DoctorPortalPage } from "./components/clinical/DoctorPortalPage";
import { PatientPortalPage } from "./components/clinical/PatientPortalPage";

function RoleLanding() {
  const { user } = useAuth();
  return <Navigate to={defaultRouteForUser(user)} replace />;
}

const AppRoutes = [
  {
    index: true,
    element: <ProtectedRoute><RoleLanding /></ProtectedRoute>
  },
  {
    path: '/admin',
    element: <ProtectedRoute roles={[ROLES.administrator]}><AdminDashboard /></ProtectedRoute>
  },
  {
    path: '/departments',
    element: <ProtectedRoute roles={[ROLES.administrator]}><DepartmentsAdminPage /></ProtectedRoute>
  },
  {
    path: '/doctors',
    element: <ProtectedRoute roles={[ROLES.administrator]}><DoctorsAdminPage /></ProtectedRoute>
  },
  {
    path: '/patients',
    element: <ProtectedRoute roles={[ROLES.administrator]}><PatientsAdminPage /></ProtectedRoute>
  },
  {
    path: '/appointments',
    element: <ProtectedRoute roles={[ROLES.administrator]}><AppointmentsAdminPage /></ProtectedRoute>
  },
  {
    path: '/lab-results',
    element: <ProtectedRoute roles={[ROLES.administrator]}><AdminLabResultsPage /></ProtectedRoute>
  },
  {
    path: '/doctor',
    element: <ProtectedRoute roles={[ROLES.doctor]}><DoctorPortalPage /></ProtectedRoute>
  },
  {
    path: '/patient',
    element: <ProtectedRoute roles={[ROLES.patient]}><PatientPortalPage /></ProtectedRoute>
  },
  {
    path: '/login',
    element: <LoginPage />
  },
  {
    path: '/register',
    element: <RegisterPage />
  }
];

export default AppRoutes;
