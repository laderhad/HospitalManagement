import { LoginPage } from "./components/api-authorization/LoginPage";
import { RegisterPage } from "./components/api-authorization/RegisterPage";
import { ProtectedRoute } from "./components/api-authorization/ProtectedRoute";
import { AdminDashboard } from "./components/admin/AdminDashboard";
import { AppointmentsAdminPage } from "./components/admin/AppointmentsAdminPage";
import { DepartmentsAdminPage } from "./components/admin/DepartmentsAdminPage";
import { DoctorsAdminPage } from "./components/admin/DoctorsAdminPage";
import { PatientsAdminPage } from "./components/admin/PatientsAdminPage";

const AppRoutes = [
  {
    index: true,
    element: <ProtectedRoute><AdminDashboard /></ProtectedRoute>
  },
  {
    path: '/admin',
    element: <ProtectedRoute><AdminDashboard /></ProtectedRoute>
  },
  {
    path: '/departments',
    element: <ProtectedRoute><DepartmentsAdminPage /></ProtectedRoute>
  },
  {
    path: '/doctors',
    element: <ProtectedRoute><DoctorsAdminPage /></ProtectedRoute>
  },
  {
    path: '/patients',
    element: <ProtectedRoute><PatientsAdminPage /></ProtectedRoute>
  },
  {
    path: '/appointments',
    element: <ProtectedRoute><AppointmentsAdminPage /></ProtectedRoute>
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
