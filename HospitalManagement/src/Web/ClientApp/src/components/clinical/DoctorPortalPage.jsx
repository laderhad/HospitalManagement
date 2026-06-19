import { useCallback, useEffect, useMemo, useState } from 'react';
import { Ban, CheckCircle2, ClipboardPlus, FileText, RefreshCw, Save, TestTube2 } from 'lucide-react';
import {
  AppointmentsClient,
  CreateExaminationCommand,
  CreateLabRequestCommand,
  CreatePrescriptionCommand,
  ExaminationsClient,
  LabRequestsClient,
  LabResultsClient,
  PrescriptionsClient,
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
} from '../admin/AdminCommon';

const appointmentsClient = new AppointmentsClient();
const examinationsClient = new ExaminationsClient();
const prescriptionsClient = new PrescriptionsClient();
const labRequestsClient = new LabRequestsClient();
const labResultsClient = new LabResultsClient();

const emptyExamination = {
  appointmentId: '',
  diagnosis: '',
  treatment: ''
};

const emptyPrescription = {
  examinationId: '',
  medicationName: '',
  dosage: '',
  frequency: '',
  durationDays: '7',
  notes: ''
};

const emptyLabRequest = {
  examinationId: '',
  testNames: ''
};

export function DoctorPortalPage() {
  const [appointments, setAppointments] = useState([]);
  const [examinations, setExaminations] = useState([]);
  const [prescriptions, setPrescriptions] = useState([]);
  const [labRequests, setLabRequests] = useState([]);
  const [labResults, setLabResults] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [rescheduleDate, setRescheduleDate] = useState('');
  const [examinationForm, setExaminationForm] = useState(emptyExamination);
  const [prescriptionForm, setPrescriptionForm] = useState(emptyPrescription);
  const [labRequestForm, setLabRequestForm] = useState(emptyLabRequest);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState('');

  const selectedAppointment = appointments.find(appointment => appointment.id === selectedId);

  const loadWorkspace = useCallback(async nextSelectedId => {
    setLoading(true);
    setError(null);

    try {
      const [
        appointmentRecords,
        examinationRecords,
        prescriptionRecords,
        labRequestRecords,
        labResultRecords
      ] = await Promise.all([
        appointmentsClient.getMyDoctorAppointments(),
        examinationsClient.getMyDoctorExaminations(),
        prescriptionsClient.getMyDoctorPrescriptions(),
        labRequestsClient.getMyDoctorLabRequests(),
        labResultsClient.getMyDoctorLabResults()
      ]);

      setAppointments(appointmentRecords);
      setExaminations(examinationRecords);
      setPrescriptions(prescriptionRecords);
      setLabRequests(labRequestRecords);
      setLabResults(labResultRecords);
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
    loadWorkspace();
  }, [loadWorkspace]);

  useEffect(() => {
    if (!selectedAppointment) return;
    setRescheduleDate(dateTimeInputValue(selectedAppointment.appointmentDate));
    setExaminationForm(current => ({
      ...current,
      appointmentId: current.appointmentId || selectedAppointment.id
    }));
  }, [selectedAppointment]);

  useEffect(() => {
    const firstExaminationId = examinations[0]?.id || '';
    const examinationIds = examinations.map(examination => examination.id);
    setPrescriptionForm(current => ({
      ...current,
      examinationId: examinationIds.includes(current.examinationId) ? current.examinationId : firstExaminationId
    }));
    setLabRequestForm(current => ({
      ...current,
      examinationId: examinationIds.includes(current.examinationId) ? current.examinationId : firstExaminationId
    }));
  }, [examinations]);

  const examinationLookup = useMemo(() => new Map(examinations.map(exam => [exam.appointmentId, exam])), [examinations]);

  const updateExaminationForm = event => {
    const { name, value } = event.target;
    setExaminationForm(current => ({ ...current, [name]: value }));
  };

  const updatePrescriptionForm = event => {
    const { name, value } = event.target;
    setPrescriptionForm(current => ({ ...current, [name]: value }));
  };

  const updateLabRequestForm = event => {
    const { name, value } = event.target;
    setLabRequestForm(current => ({ ...current, [name]: value }));
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
      await loadWorkspace(selectedAppointment.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const cancelAppointment = async () => updateAppointmentStatus('cancel');
  const completeAppointment = async () => updateAppointmentStatus('complete');

  const updateAppointmentStatus = async action => {
    if (!selectedAppointment) return;
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      if (action === 'complete') {
        await appointmentsClient.completeAppointment(selectedAppointment.id);
        setSuccess('Appointment completed.');
      } else {
        await appointmentsClient.cancelAppointment(selectedAppointment.id);
        setSuccess('Appointment cancelled.');
      }
      await loadWorkspace(selectedAppointment.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const createExamination = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await examinationsClient.createExamination(new CreateExaminationCommand(examinationForm));
      setExaminationForm(current => ({ ...emptyExamination, appointmentId: current.appointmentId }));
      setSuccess('Examination created.');
      await loadWorkspace(examinationForm.appointmentId);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const createPrescription = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await prescriptionsClient.createPrescription(new CreatePrescriptionCommand({
        ...prescriptionForm,
        durationDays: Number(prescriptionForm.durationDays)
      }));
      setPrescriptionForm(current => ({ ...emptyPrescription, examinationId: current.examinationId }));
      setSuccess('Prescription added.');
      await loadWorkspace(selectedId);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const createLabRequest = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await labRequestsClient.createLabRequest(new CreateLabRequestCommand({
        examinationId: labRequestForm.examinationId,
        testNames: labRequestForm.testNames.split('\n').map(name => name.trim()).filter(Boolean)
      }));
      setLabRequestForm(current => ({ ...emptyLabRequest, examinationId: current.examinationId }));
      setSuccess('Lab request created.');
      await loadWorkspace(selectedId);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="admin-page">
      <PageHeader
        title="Doctor workspace"
        description="Manage your appointments and create examination outputs for your patients."
        actions={(
          <button type="button" className="secondary compact-action" onClick={() => loadWorkspace()} disabled={loading}>
            <RefreshCw size={16} strokeWidth={2} />
            Refresh
          </button>
        )}
      />
      <ApiNotice error={error} success={success} />

      <div className="management-grid clinical-grid">
        <section className="record-pane">
          <div className="pane-title">
            <ClipboardPlus size={18} strokeWidth={2} />
            <h2>My appointments</h2>
          </div>
          <div className="directory-summary">
            <span>{appointments.length} appointments</span>
            <small>{examinations.length} examinations</small>
          </div>

          {loading ? <LoadingState /> : appointments.length ? (
            <ul className="record-list">
              {appointments.map(appointment => {
                const hasExamination = examinationLookup.has(appointment.id);
                return (
                  <li key={appointment.id}>
                    <button
                      type="button"
                      className={appointment.id === selectedId ? 'selected' : undefined}
                      onClick={() => {
                        setSelectedId(appointment.id);
                        setExaminationForm(current => ({ ...current, appointmentId: appointment.id }));
                      }}
                    >
                      <span>
                        <strong>{appointment.patientFullName}</strong>
                        <small>{dateLabel(appointment.appointmentDate)}{hasExamination ? ' | Exam ready' : ''}</small>
                      </span>
                      <StatusBadge status={appointment.status} />
                    </button>
                  </li>
                );
              })}
            </ul>
          ) : (
            <EmptyState title="No appointments" body="Scheduled patient visits appear here." />
          )}
        </section>

        <section className="detail-pane">
          {loading ? <LoadingState label="Loading doctor workspace" /> : (
            <div className="clinical-stack">
              {selectedAppointment ? (
                <section className="clinical-panel">
                  <div className="detail-heading">
                    <div>
                      <h2>{selectedAppointment.patientFullName}</h2>
                      <p>{dateLabel(selectedAppointment.appointmentDate)}</p>
                    </div>
                    <StatusBadge status={selectedAppointment.status} />
                  </div>
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
                      <button type="button" className="secondary" onClick={completeAppointment} disabled={saving || selectedAppointment.status !== 0}>
                        <CheckCircle2 size={16} strokeWidth={2} />
                        Complete
                      </button>
                      <button type="button" className="contrast" onClick={cancelAppointment} disabled={saving || selectedAppointment.status !== 0}>
                        <Ban size={16} strokeWidth={2} />
                        Cancel
                      </button>
                    </div>
                  </form>
                </section>
              ) : (
                <EmptyState title="Select an appointment" body="Appointment controls and clinical actions appear here." />
              )}

              <section className="clinical-panel">
                <PanelTitle icon={ClipboardPlus} title="Create examination" />
                <form className="detail-form" onSubmit={createExamination}>
                  <div className="field-grid">
                    <label className="span-two">Appointment
                      <select required name="appointmentId" value={examinationForm.appointmentId} onChange={updateExaminationForm}>
                        <option value="" disabled>Select appointment</option>
                        {appointments.filter(appointment => appointment.status !== 2).map(appointment => (
                          <option key={appointment.id} value={appointment.id}>{appointment.patientFullName} | {dateLabel(appointment.appointmentDate)}</option>
                        ))}
                      </select>
                    </label>
                    <label className="span-two">Diagnosis<textarea required rows={3} name="diagnosis" value={examinationForm.diagnosis} onChange={updateExaminationForm} /></label>
                    <label className="span-two">Treatment<textarea required rows={3} name="treatment" value={examinationForm.treatment} onChange={updateExaminationForm} /></label>
                  </div>
                  <div className="form-actions">
                    <button disabled={saving || !appointments.length}>
                      <ClipboardPlus size={16} strokeWidth={2} />
                      Create examination
                    </button>
                  </div>
                </form>
              </section>

              <section className="clinical-panel">
                <PanelTitle icon={FileText} title="Add prescription" />
                <form className="detail-form" onSubmit={createPrescription}>
                  <ClinicalExaminationSelect value={prescriptionForm.examinationId} examinations={examinations} onChange={updatePrescriptionForm} />
                  <div className="field-grid">
                    <label>Medication<input required name="medicationName" value={prescriptionForm.medicationName} onChange={updatePrescriptionForm} /></label>
                    <label>Dosage<input required name="dosage" value={prescriptionForm.dosage} onChange={updatePrescriptionForm} /></label>
                    <label>Frequency<input required name="frequency" value={prescriptionForm.frequency} onChange={updatePrescriptionForm} /></label>
                    <label>Duration days<input required min="1" type="number" name="durationDays" value={prescriptionForm.durationDays} onChange={updatePrescriptionForm} /></label>
                    <label className="span-two">Notes<textarea rows={3} name="notes" value={prescriptionForm.notes} onChange={updatePrescriptionForm} /></label>
                  </div>
                  <div className="form-actions">
                    <button disabled={saving || !examinations.length}>
                      <FileText size={16} strokeWidth={2} />
                      Add prescription
                    </button>
                  </div>
                </form>
              </section>

              <section className="clinical-panel">
                <PanelTitle icon={TestTube2} title="Create lab request" />
                <form className="detail-form" onSubmit={createLabRequest}>
                  <ClinicalExaminationSelect value={labRequestForm.examinationId} examinations={examinations} onChange={updateLabRequestForm} />
                  <label>Test names<textarea required rows={4} name="testNames" value={labRequestForm.testNames} onChange={updateLabRequestForm} placeholder="Complete blood count&#10;Glucose" /></label>
                  <div className="form-actions">
                    <button disabled={saving || !examinations.length}>
                      <TestTube2 size={16} strokeWidth={2} />
                      Create lab request
                    </button>
                  </div>
                </form>
              </section>

              <section className="clinical-list-section">
                <h3>Recent clinical output</h3>
                <div className="clinical-summary-grid">
                  <SummaryBlock title="Prescriptions" records={prescriptions} label={record => `${record.patientFullName} | ${record.medicationName}`} />
                  <SummaryBlock title="Lab requests" records={labRequests} label={record => `${record.patientFullName} | ${record.itemCount} tests`} />
                  <SummaryBlock title="Lab results" records={labResults} label={record => `${record.patientFullName} | ${record.testName}: ${record.resultValue}`} />
                </div>
              </section>
            </div>
          )}
        </section>
      </div>
    </section>
  );
}

function PanelTitle({ icon: Icon, title }) {
  return (
    <div className="pane-title clinical-panel-title">
      <Icon size={18} strokeWidth={2} />
      <h2>{title}</h2>
    </div>
  );
}

function ClinicalExaminationSelect({ value, examinations, onChange }) {
  return (
    <label>Examination
      <select required name="examinationId" value={value} onChange={onChange}>
        <option value="" disabled>Select examination</option>
        {examinations.map(examination => (
          <option key={examination.id} value={examination.id}>{examination.patientFullName} | {dateLabel(examination.appointmentDate)}</option>
        ))}
      </select>
    </label>
  );
}

function SummaryBlock({ title, records, label }) {
  return (
    <div className="summary-block">
      <h4>{title}</h4>
      {records.length ? (
        <ul className="summary-list">
          {records.slice(0, 4).map(record => <li key={record.id}>{label(record)}</li>)}
        </ul>
      ) : (
        <span>No records</span>
      )}
    </div>
  );
}
