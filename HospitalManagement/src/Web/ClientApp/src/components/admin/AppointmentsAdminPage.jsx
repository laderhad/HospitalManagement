import { useCallback, useEffect, useState } from 'react';
import { Ban, CalendarPlus, CheckCircle2, RefreshCw, Save } from 'lucide-react';
import {
  AppointmentsClient,
  CreateAppointmentCommand,
  DoctorsClient,
  PatientsClient,
  RescheduleAppointmentCommand
} from '../../web-api-client';
import {
  ApiNotice,
  dateLabel,
  dateTimeInputValue,
  EmptyState,
  LoadingState,
  PageHeader,
  StatusBadge
} from './AdminCommon';

const appointmentsClient = new AppointmentsClient();
const doctorsClient = new DoctorsClient();
const patientsClient = new PatientsClient();

const emptyAppointment = {
  patientId: '',
  doctorId: '',
  appointmentDate: ''
};

export function AppointmentsAdminPage() {
  const [appointments, setAppointments] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [patients, setPatients] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [isScheduling, setIsScheduling] = useState(false);
  const [detail, setDetail] = useState(null);
  const [createForm, setCreateForm] = useState(emptyAppointment);
  const [rescheduleDate, setRescheduleDate] = useState('');
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState('');

  const loadAppointments = useCallback(async nextSelectedId => {
    setLoading(true);
    setError(null);

    try {
      const [appointmentRecords, doctorRecords, patientRecords] = await Promise.all([
        appointmentsClient.getAppointments(),
        doctorsClient.getDoctors(),
        patientsClient.getPatients()
      ]);

      setAppointments(appointmentRecords);
      setDoctors(doctorRecords);
      setPatients(patientRecords);
      setCreateForm(current => ({
        ...current,
        doctorId: current.doctorId || doctorRecords.find(doctor => doctor.isActive)?.id || doctorRecords[0]?.id || '',
        patientId: current.patientId || patientRecords[0]?.id || ''
      }));

      setSelectedId(currentId => {
        const activeSelection = nextSelectedId || currentId || appointmentRecords[0]?.id || '';
        return appointmentRecords.some(record => record.id === activeSelection) ? activeSelection : appointmentRecords[0]?.id || '';
      });
    } catch (requestError) {
      setError(requestError);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAppointments();
  }, [loadAppointments]);

  useEffect(() => {
    if (!selectedId) {
      setDetail(null);
      return;
    }

    let mounted = true;
    setDetailLoading(true);
    setError(null);

    appointmentsClient.getAppointmentById(selectedId)
      .then(record => {
        if (!mounted) return;
        setDetail(record);
        setRescheduleDate(dateTimeInputValue(record.appointmentDate));
      })
      .catch(requestError => mounted && setError(requestError))
      .finally(() => mounted && setDetailLoading(false));

    return () => {
      mounted = false;
    };
  }, [selectedId]);

  const updateCreateForm = event => {
    const { name, value } = event.target;
    setCreateForm(current => ({ ...current, [name]: value }));
  };

  const createAppointment = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      const id = await appointmentsClient.createAppointment(
        new CreateAppointmentCommand({
          patientId: createForm.patientId,
          doctorId: createForm.doctorId,
          appointmentDate: new Date(createForm.appointmentDate)
        })
      );
      setCreateForm(current => ({ ...current, appointmentDate: '' }));
      setIsScheduling(false);
      setSuccess('Appointment scheduled.');
      await loadAppointments(id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const rescheduleAppointment = async event => {
    event.preventDefault();
    if (!detail) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await appointmentsClient.rescheduleAppointment(
        detail.id,
        new RescheduleAppointmentCommand({
          id: detail.id,
          appointmentDate: new Date(rescheduleDate)
        })
      );
      setSuccess('Appointment rescheduled.');
      await loadAppointments(detail.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const cancelAppointment = async () => {
    if (!detail) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await appointmentsClient.cancelAppointment(detail.id);
      setSuccess('Appointment cancelled.');
      await loadAppointments(detail.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const completeAppointment = async () => {
    if (!detail) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await appointmentsClient.completeAppointment(detail.id);
      setSuccess('Appointment completed.');
      await loadAppointments(detail.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="admin-page">
      <PageHeader
        title="Appointments"
        description="Schedule administrative appointments and manage their status."
        actions={(
          <>
            <button type="button" className="compact-action" onClick={() => setIsScheduling(true)}>
              <CalendarPlus size={16} strokeWidth={2} />
              New appointment
            </button>
            <button type="button" className="secondary compact-action" onClick={() => loadAppointments()} disabled={loading}>
              <RefreshCw size={16} strokeWidth={2} />
              Refresh
            </button>
          </>
        )}
      />
      <ApiNotice error={error} success={success} />

      <div className="management-grid appointments-grid">
        <section className="record-pane">
          <div className="pane-title">
            <CalendarPlus size={18} strokeWidth={2} />
            <h2>Appointment queue</h2>
          </div>
          <div className="directory-summary">
            <span>{appointments.length} appointments</span>
            <small>Scheduled visits can be rescheduled, completed, or cancelled.</small>
          </div>

          {loading ? <LoadingState /> : appointments.length ? (
            <ul className="record-list">
              {appointments.map(appointment => (
                <li key={appointment.id}>
                  <button
                    type="button"
                    className={appointment.id === selectedId ? 'selected' : undefined}
                    onClick={() => {
                      setIsScheduling(false);
                      setSelectedId(appointment.id);
                    }}
                  >
                    <span>
                      <strong>{appointment.patientFullName}</strong>
                      <small>{appointment.doctorFullName} | {dateLabel(appointment.appointmentDate)}</small>
                    </span>
                    <StatusBadge status={appointment.status} />
                  </button>
                </li>
              ))}
            </ul>
          ) : (
            <EmptyState title="No appointments yet" body="Use New appointment to schedule the first visit." />
          )}
        </section>

        <section className="detail-pane">
          {isScheduling ? (
            <>
              <div className="detail-heading">
                <div>
                  <h2>New appointment</h2>
                  <p>Match an existing patient with an available doctor.</p>
                </div>
              </div>
              <form className="detail-form" onSubmit={createAppointment}>
                <div className="field-grid">
                  <label>Patient
                    <select required name="patientId" value={createForm.patientId} onChange={updateCreateForm} autoFocus>
                      <option value="" disabled>Select patient</option>
                      {patients.map(patient => <option key={patient.id} value={patient.id}>{patient.firstName} {patient.lastName}</option>)}
                    </select>
                  </label>
                  <label>Doctor
                    <select required name="doctorId" value={createForm.doctorId} onChange={updateCreateForm}>
                      <option value="" disabled>Select doctor</option>
                      {doctors.filter(doctor => doctor.isActive).map(doctor => (
                        <option key={doctor.id} value={doctor.id}>{doctor.firstName} {doctor.lastName} | {doctor.departmentName}</option>
                      ))}
                    </select>
                  </label>
                  <label className="span-two">Appointment time<input required type="datetime-local" name="appointmentDate" value={createForm.appointmentDate} onChange={updateCreateForm} /></label>
                </div>
                <div className="form-actions">
                  <button disabled={saving || !patients.length || !doctors.some(doctor => doctor.isActive)}>
                    <CalendarPlus size={16} strokeWidth={2} />
                    Schedule appointment
                  </button>
                  <button type="button" className="secondary" onClick={() => setIsScheduling(false)}>
                    Cancel
                  </button>
                </div>
              </form>
            </>
          ) : detailLoading ? <LoadingState label="Loading appointment detail" /> : detail ? (
            <>
              <div className="detail-heading">
                <div>
                  <h2>{detail.patientFullName}</h2>
                  <p>{detail.doctorFullName} | {detail.departmentName}</p>
                </div>
                <StatusBadge status={detail.status} />
              </div>

              <div className="fact-strip">
                <div>
                  <span>Time</span>
                  <strong>{dateLabel(detail.appointmentDate)}</strong>
                </div>
                <div>
                  <span>Patient</span>
                  <strong>{detail.patientFullName}</strong>
                </div>
                <div>
                  <span>Doctor</span>
                  <strong>{detail.doctorFullName}</strong>
                </div>
              </div>

              <form className="detail-form" onSubmit={rescheduleAppointment}>
                <label>
                  Reschedule
                  <input
                    required
                    type="datetime-local"
                    value={rescheduleDate}
                    onChange={event => setRescheduleDate(event.target.value)}
                    disabled={detail.status !== 0}
                  />
                </label>
                <div className="form-actions">
                  <button disabled={saving || detail.status !== 0}>
                    <Save size={16} strokeWidth={2} />
                    Save time
                  </button>
                  <button type="button" className="secondary" onClick={completeAppointment} disabled={saving || detail.status !== 0}>
                    <CheckCircle2 size={16} strokeWidth={2} />
                    Complete
                  </button>
                  <button type="button" className="contrast" onClick={cancelAppointment} disabled={saving || detail.status !== 0}>
                    <Ban size={16} strokeWidth={2} />
                    Cancel
                  </button>
                </div>
              </form>
            </>
          ) : (
            <EmptyState title="Select an appointment" body="Timing and status controls appear here." />
          )}
        </section>
      </div>
    </section>
  );
}
