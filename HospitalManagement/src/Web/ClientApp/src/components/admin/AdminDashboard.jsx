import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Building2, CalendarDays, Stethoscope, TestTube2, UsersRound } from 'lucide-react';
import { AppointmentsClient, DepartmentsClient, DoctorsClient, LabRequestsClient, PatientsClient } from '../../web-api-client';
import { ApiNotice, LoadingState, PageHeader } from './AdminCommon';

const dashboardResources = [
  {
    key: 'departments',
    label: 'Departments',
    path: '/departments',
    icon: Building2,
    load: () => new DepartmentsClient().getDepartments()
  },
  {
    key: 'doctors',
    label: 'Doctors',
    path: '/doctors',
    icon: Stethoscope,
    load: () => new DoctorsClient().getDoctors()
  },
  {
    key: 'patients',
    label: 'Patients',
    path: '/patients',
    icon: UsersRound,
    load: () => new PatientsClient().getPatients()
  },
  {
    key: 'appointments',
    label: 'Appointments',
    path: '/appointments',
    icon: CalendarDays,
    load: () => new AppointmentsClient().getAppointments()
  },
  {
    key: 'labRequests',
    label: 'Lab requests',
    path: '/lab-results',
    icon: TestTube2,
    load: () => new LabRequestsClient().getLabRequests()
  }
];

export function AdminDashboard() {
  const [counts, setCounts] = useState({});
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;

    Promise.allSettled(dashboardResources.map(resource => resource.load()))
      .then(results => {
        if (!mounted) return;

        const nextCounts = {};
        let firstError = null;

        results.forEach((result, index) => {
          if (result.status === 'fulfilled') {
            nextCounts[dashboardResources[index].key] = result.value?.length ?? 0;
          } else {
            firstError ??= result.reason;
          }
        });

        setCounts(nextCounts);
        setError(firstError);
      })
      .finally(() => mounted && setLoading(false));

    return () => {
      mounted = false;
    };
  }, []);

  return (
    <section className="admin-page">
      <PageHeader
        title="Administration"
        description="Coordinate clinical capacity, patient records, and appointments from one workspace."
      />

      <ApiNotice error={error} />

      {loading ? <LoadingState label="Loading admin overview" /> : (
        <div className="metric-grid">
          {dashboardResources.map(({ key, label, path, icon: Icon }) => (
            <Link key={key} to={path} className="metric-tile">
              <Icon size={20} strokeWidth={2} />
              <span>{label}</span>
              <strong>{counts[key] ?? '--'}</strong>
            </Link>
          ))}
        </div>
      )}

      <div className="workflow-band">
        <div>
          <h2>Admin workflow</h2>
          <p>Start with departments, add doctors to those units, keep patient profiles current, then schedule appointments.</p>
        </div>
        <div className="quick-links">
          {dashboardResources.map(({ key, label, path }) => (
            <Link key={key} to={path}>{label}</Link>
          ))}
        </div>
      </div>
    </section>
  );
}
