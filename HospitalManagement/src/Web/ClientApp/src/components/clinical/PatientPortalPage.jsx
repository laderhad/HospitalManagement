import { useCallback, useEffect, useState } from 'react';
import { Ban, CalendarPlus, FileText, RefreshCw, Save, TestTube2, UserRound } from 'lucide-react';
import {
  AppointmentsClient,
  CreateMyAppointmentCommand,
  DoctorsClient,
  ExaminationsClient,
  LabRequestsClient,
  LabResultsClient,
  PrescriptionsClient,
  RescheduleAppointmentCommand,
  PatientsClient,
  UpdateMyPatientProfileCommand
} from '../../web-api-client';
import {
  ApiNotice,
  dateInputValue,
  dateLabel,
  dateTimeInputValue,
  EmptyState,
  genderLabels,
  LoadingState,
  PageHeader,
  StatusBadge
} from '../admin/AdminCommon';

const appointmentsClient = new AppointmentsClient();
const doctorsClient = new DoctorsClient();
const examinationsClient = new ExaminationsClient();
const labRequestsClient = new LabRequestsClient();
const labResultsClient = new LabResultsClient();
const patientsClient = new PatientsClient();
const prescriptionsClient = new PrescriptionsClient();

const emptyAppointment = {
  doctorId: '',
  appointmentDate: ''
};

export function PatientPortalPage() {
  const [profile, setProfile] = useState(null);
  const [profileForm, setProfileForm] = useState(null);
  const [doctors, setDoctors] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [examinations, setExaminations] = useState([]);
  const [prescriptions, setPrescriptions] = useState([]);
  const [labRequests, setLabRequests] = useState([]);
  const [labResults, setLabResults] = useState([]);
  const [appointmentForm, setAppointmentForm] = useState(emptyAppointment);
  const [selectedId, setSelectedId] = useState('');
  const [rescheduleDate, setRescheduleDate] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState('');

  const selectedAppointment = appointments.find(appointment => appointment.id === selectedId);

  const loadPortal = useCallback(async nextSelectedId => {
    setLoading(true);
    setError(null);

    try {
      const [
        profileRecord,
        doctorRecords,
        appointmentRecords,
        examinationRecords,
        prescriptionRecords,
        labRequestRecords,
        labResultRecords
      ] = await Promise.all([
        patientsClient.getMyPatientProfile(),
        doctorsClient.getAvailableDoctors(),
        appointmentsClient.getMyPatientAppointments(),
        examinationsClient.getMyPatientExaminations(),
        prescriptionsClient.getMyPatientPrescriptions(),
        labRequestsClient.getMyPatientLabRequests(),
        labResultsClient.getMyPatientLabResults()
      ]);

      setProfile(profileRecord);
      setProfileForm({
        firstName: profileRecord.firstName,
        lastName: profileRecord.lastName,
        dateOfBirth: dateInputValue(profileRecord.dateOfBirth),
        gender: String(profileRecord.gender),
        contactNumber: profileRecord.contactNumber,
        address: profileRecord.address
      });
      setDoctors(doctorRecords);
      setAppointments(appointmentRecords);
      setExaminations(examinationRecords);
      setPrescriptions(prescriptionRecords);
      setLabRequests(labRequestRecords);
      setLabResults(labResultRecords);
      setAppointmentForm(current => ({
        ...current,
        doctorId: current.doctorId || doctorRecords[0]?.id || ''
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
    loadPortal();
  }, [loadPortal]);

  useEffect(() => {
    if (selectedAppointment) {
      setRescheduleDate(dateTimeInputValue(selectedAppointment.appointmentDate));
    }
  }, [selectedAppointment]);

  const updateProfileForm = event => {
    const { name, value } = event.target;
    setProfileForm(current => ({ ...current, [name]: value }));
  };

  const updateAppointmentForm = event => {
    const { name, value } = event.target;
    setAppointmentForm(current => ({ ...current, [name]: value }));
  };

  const saveProfile = async event => {
    event.preventDefault();
    if (!profileForm) return;
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await patientsClient.updateMyPatientProfile(new UpdateMyPatientProfileCommand({
        firstName: profileForm.firstName,
        lastName: profileForm.lastName,
        dateOfBirth: new Date(`${profileForm.dateOfBirth}T00:00:00Z`),
        gender: Number(profileForm.gender),
        contactNumber: profileForm.contactNumber,
        address: profileForm.address
      }));
      setSuccess('Profile updated.');
      await loadPortal(selectedId);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const createAppointment = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      const id = await appointmentsClient.createMyAppointment(new CreateMyAppointmentCommand({
        doctorId: appointmentForm.doctorId,
        appointmentDate: new Date(appointmentForm.appointmentDate)
      }));
      setAppointmentForm(current => ({ ...current, appointmentDate: '' }));
      setSuccess('Appointment requested.');
      await loadPortal(id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const rescheduleAppointment = async event => {
    event.preventDefault();
    if (!selectedAppointment) return;
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await appointmentsClient.rescheduleAppointment(
        selectedAppointment.id,
        new RescheduleAppointmentCommand({
          id: selectedAppointment.id,
          appointmentDate: new Date(rescheduleDate)
        })
      );
      setSuccess('Appointment rescheduled.');
      await loadPortal(selectedAppointment.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const cancelAppointment = async () => {
    if (!selectedAppointment) return;
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await appointmentsClient.cancelAppointment(selectedAppointment.id);
      setSuccess('Appointment cancelled.');
      await loadPortal(selectedAppointment.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="admin-page">
      <PageHeader
        title="Patient portal"
        description="Review your profile, appointments, and clinical records."
        actions={(
          <button type="button" className="secondary compact-action" onClick={() => loadPortal()} disabled={loading}>
            <RefreshCw size={16} strokeWidth={2} />
            Refresh
          </button>
        )}
      />
      <ApiNotice error={error} success={success} />

      {loading ? <LoadingState label="Loading patient portal" /> : (
        <div className="clinical-stack">
          <div className="management-grid patient-portal-grid">
            <section className="record-pane">
              <div className="pane-title">
                <UserRound size={18} strokeWidth={2} />
                <h2>My profile</h2>
              </div>
              {profile && <p className="muted-copy">{profile.firstName} {profile.lastName} | {genderLabels[profile.gender]}</p>}

              {profileForm ? (
                <form className="detail-form" onSubmit={saveProfile}>
                  <div className="field-grid">
                    <label>First name<input required name="firstName" value={profileForm.firstName} onChange={updateProfileForm} /></label>
                    <label>Last name<input required name="lastName" value={profileForm.lastName} onChange={updateProfileForm} /></label>
                    <label>Date of birth<input required type="date" name="dateOfBirth" value={profileForm.dateOfBirth} onChange={updateProfileForm} /></label>
                    <label>Gender
                      <select name="gender" value={profileForm.gender} onChange={updateProfileForm}>
                        {Object.entries(genderLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                      </select>
                    </label>
                    <label className="span-two">Contact number<input required name="contactNumber" value={profileForm.contactNumber} onChange={updateProfileForm} /></label>
                    <label className="span-two">Address<textarea required rows={3} name="address" value={profileForm.address} onChange={updateProfileForm} /></label>
                  </div>
                  <div className="form-actions">
                    <button disabled={saving}>
                      <Save size={16} strokeWidth={2} />
                      Save profile
                    </button>
                  </div>
                </form>
              ) : (
                <EmptyState title="Profile unavailable" body="Your account needs a patient profile." />
              )}
            </section>

            <section className="detail-pane">
              <div className="detail-heading">
                <div>
                  <h2>Appointments</h2>
                  <p>Book a visit with an active doctor and manage scheduled appointments.</p>
                </div>
              </div>

              <form className="detail-form" onSubmit={createAppointment}>
                <div className="field-grid">
                  <label>Doctor
                    <select required name="doctorId" value={appointmentForm.doctorId} onChange={updateAppointmentForm}>
                      <option value="" disabled>Select doctor</option>
                      {doctors.map(doctor => (
                        <option key={doctor.id} value={doctor.id}>{doctor.firstName} {doctor.lastName} | {doctor.departmentName}</option>
                      ))}
                    </select>
                  </label>
                  <label>Appointment time<input required type="datetime-local" name="appointmentDate" value={appointmentForm.appointmentDate} onChange={updateAppointmentForm} /></label>
                </div>
                <div className="form-actions">
                  <button disabled={saving || !doctors.length}>
                    <CalendarPlus size={16} strokeWidth={2} />
                    Book appointment
                  </button>
                </div>
              </form>

              <div className="patient-appointment-grid">
                <section>
                  <h3>My appointments</h3>
                  {appointments.length ? (
                    <ul className="record-list compact-record-list">
                      {appointments.map(appointment => (
                        <li key={appointment.id}>
                          <button
                            type="button"
                            className={appointment.id === selectedId ? 'selected' : undefined}
                            onClick={() => setSelectedId(appointment.id)}
                          >
                            <span>
                              <strong>{appointment.doctorFullName}</strong>
                              <small>{dateLabel(appointment.appointmentDate)}</small>
                            </span>
                            <StatusBadge status={appointment.status} />
                          </button>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <EmptyState title="No appointments" body="Book your first appointment above." />
                  )}
                </section>

                <section>
                  <h3>Selected appointment</h3>
                  {selectedAppointment ? (
                    <form className="detail-form" onSubmit={rescheduleAppointment}>
                      <label>Appointment time
                        <input
                          required
                          type="datetime-local"
                          value={rescheduleDate}
                          onChange={event => setRescheduleDate(event.target.value)}
                          disabled={selectedAppointment.status !== 0}
                        />
                      </label>
                      <div className="form-actions">
                        <button disabled={saving || selectedAppointment.status !== 0}>
                          <Save size={16} strokeWidth={2} />
                          Save time
                        </button>
                        <button type="button" className="contrast" onClick={cancelAppointment} disabled={saving || selectedAppointment.status !== 0}>
                          <Ban size={16} strokeWidth={2} />
                          Cancel
                        </button>
                      </div>
                    </form>
                  ) : (
                    <EmptyState title="Select an appointment" body="Reschedule and cancellation controls appear here." />
                  )}
                </section>
              </div>
            </section>
          </div>

          <section className="clinical-list-section">
            <div className="pane-title">
              <FileText size={18} strokeWidth={2} />
              <h2>My clinical records</h2>
            </div>
            <div className="clinical-summary-grid">
              <SummaryBlock title="Examinations" records={examinations} label={record => `${dateLabel(record.appointmentDate)} | ${record.diagnosis}`} />
              <SummaryBlock title="Prescriptions" records={prescriptions} label={record => `${record.medicationName} | ${record.dosage} | ${record.frequency}`} />
              <SummaryBlock title="Lab requests" records={labRequests} label={record => `${dateLabel(record.requestDate)} | ${record.itemCount} tests`} />
              <SummaryBlock title="Lab results" records={labResults} icon={TestTube2} label={record => `${record.testName}: ${record.resultValue} ${record.units}`} />
            </div>
          </section>
        </div>
      )}
    </section>
  );
}

function SummaryBlock({ title, records, label, icon: Icon }) {
  return (
    <div className="summary-block">
      <h4>{Icon && <Icon size={15} strokeWidth={2} />}{title}</h4>
      {records.length ? (
        <ul className="summary-list">
          {records.slice(0, 5).map(record => <li key={record.id}>{label(record)}</li>)}
        </ul>
      ) : (
        <span>No records</span>
      )}
    </div>
  );
}
