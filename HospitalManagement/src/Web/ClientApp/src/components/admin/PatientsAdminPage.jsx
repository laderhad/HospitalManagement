import { useCallback, useEffect, useState } from 'react';
import { RefreshCw, Save, UsersRound } from 'lucide-react';
import { PatientsClient, UpdatePatientCommand } from '../../web-api-client';
import {
  ApiNotice,
  Button,
  dateInputValue,
  EmptyState,
  FormSection,
  genderLabels,
  Input,
  LoadingState,
  PageHeader,
  Select,
  Textarea,
  shortDateLabel
} from './AdminCommon';

const patientsClient = new PatientsClient();

export function PatientsAdminPage() {
  const [patients, setPatients] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [detail, setDetail] = useState(null);
  const [editForm, setEditForm] = useState(null);
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState('');

  const loadPatients = useCallback(async nextSelectedId => {
    setLoading(true);
    setError(null);

    try {
      const records = await patientsClient.getPatients();
      setPatients(records);
      setSelectedId(currentId => {
        const activeSelection = nextSelectedId || currentId || records[0]?.id || '';
        return records.some(record => record.id === activeSelection) ? activeSelection : records[0]?.id || '';
      });
    } catch (requestError) {
      setError(requestError);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPatients();
  }, [loadPatients]);

  useEffect(() => {
    if (!selectedId) {
      setDetail(null);
      setEditForm(null);
      return;
    }

    let mounted = true;
    setDetailLoading(true);
    setError(null);

    patientsClient.getPatientById(selectedId)
      .then(record => {
        if (!mounted) return;
        setDetail(record);
        setEditForm({
          firstName: record.firstName,
          lastName: record.lastName,
          dateOfBirth: dateInputValue(record.dateOfBirth),
          gender: String(record.gender),
          contactNumber: record.contactNumber,
          address: record.address
        });
      })
      .catch(requestError => mounted && setError(requestError))
      .finally(() => mounted && setDetailLoading(false));

    return () => {
      mounted = false;
    };
  }, [selectedId]);

  const updateEditForm = event => {
    const { name, value } = event.target;
    setEditForm(current => ({ ...current, [name]: value }));
  };

  const savePatient = async event => {
    event.preventDefault();
    if (!detail || !editForm) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await patientsClient.updatePatient(
        detail.id,
        new UpdatePatientCommand({
          id: detail.id,
          firstName: editForm.firstName,
          lastName: editForm.lastName,
          dateOfBirth: new Date(`${editForm.dateOfBirth}T00:00:00Z`),
          gender: Number(editForm.gender),
          contactNumber: editForm.contactNumber,
          address: editForm.address
        })
      );
      setSuccess('Patient profile updated.');
      await loadPatients(detail.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="admin-page">
      <PageHeader
        title="Patients"
        description="Review and update patient profiles already linked to registered users."
        actions={(
          <Button type="button" variant="secondary" className="compact-action" onClick={() => loadPatients()} disabled={loading}>
            <RefreshCw size={16} strokeWidth={2} />
            Refresh
          </Button>
        )}
      />
      <ApiNotice error={error} success={success} />

      <div className="management-grid patients-grid">
        <PatientRegistry
          loading={loading}
          patients={patients}
          selectedId={selectedId}
          onSelect={setSelectedId}
        />

        <section className="detail-pane">
          {detailLoading ? <LoadingState label="Loading patient detail" /> : detail && editForm ? (
            <PatientDetailForm
              detail={detail}
              editForm={editForm}
              saving={saving}
              onChange={updateEditForm}
              onSubmit={savePatient}
            />
          ) : (
            <EmptyState title="Select a patient" body="Patient detail and edit controls appear here." />
          )}
        </section>
      </div>
    </section>
  );
}

export function PatientRegistry({ loading, patients, selectedId, onSelect }) {
  return (
    <section className="record-pane patient-registry" aria-label="Patient registry">
      <header className="registry-header">
        <div className="pane-title">
          <UsersRound size={18} strokeWidth={2} />
          <h2>Patient registry</h2>
        </div>
        <p>Profiles are updated after the patient account exists.</p>
      </header>
      <div className="directory-summary">
        <span>{patients.length} patient profiles</span>
        <small>Registered records</small>
      </div>

      {loading ? <LoadingState /> : patients.length ? (
        <div className="registry-table" role="table" aria-label="Patients">
          <div className="registry-columns" role="row">
            <span role="columnheader">Patient</span>
            <span role="columnheader">Demographics</span>
          </div>
          <ul className="record-list">
            {patients.map(patient => (
              <li key={patient.id}>
                <button
                  type="button"
                  className={patient.id === selectedId ? 'selected' : undefined}
                  aria-current={patient.id === selectedId ? 'true' : undefined}
                  onClick={() => onSelect(patient.id)}
                >
                  <span>
                    <strong>{patient.firstName} {patient.lastName}</strong>
                    <small>Profile record</small>
                  </span>
                  <span className="registry-demographics">
                    <strong>{genderLabels[patient.gender]}</strong>
                    <small>{shortDateLabel(patient.dateOfBirth)}</small>
                  </span>
                </button>
              </li>
            ))}
          </ul>
        </div>
      ) : (
        <EmptyState title="No patient profiles" body="Patient creation remains in the account-linking flow for now." />
      )}
    </section>
  );
}

export function PatientDetailForm({ detail, editForm, saving, onChange, onSubmit }) {
  return (
    <>
      <div className="detail-heading medical-record-heading">
        <div>
          <span className="detail-kicker">Patient profile</span>
          <h2>{detail.firstName} {detail.lastName}</h2>
          <p>{genderLabels[detail.gender]} | Born {shortDateLabel(detail.dateOfBirth)}</p>
        </div>
      </div>

      <form className="detail-form patient-detail-form" onSubmit={onSubmit}>
        <FormSection title="Basic Information">
          <div className="field-grid">
            <Input required maxLength={100} label="First name" name="firstName" value={editForm.firstName} onChange={onChange} />
            <Input required maxLength={100} label="Last name" name="lastName" value={editForm.lastName} onChange={onChange} />
            <Input required type="date" label="Date of birth" name="dateOfBirth" value={editForm.dateOfBirth} onChange={onChange} />
            <Select label="Gender" name="gender" value={editForm.gender} onChange={onChange}>
              {Object.entries(genderLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
            </Select>
          </div>
        </FormSection>

        <FormSection title="Contact Information">
          <div className="field-grid">
            <Input required maxLength={20} label="Contact number" name="contactNumber" value={editForm.contactNumber} onChange={onChange} />
          </div>
        </FormSection>

        <FormSection title="Address">
          <Textarea required maxLength={500} rows={4} label="Address" name="address" value={editForm.address} onChange={onChange} />
        </FormSection>

        <div className="form-actions record-actions">
          <Button disabled={saving}>
            <Save size={16} strokeWidth={2} />
            Save profile
          </Button>
        </div>
      </form>
    </>
  );
}
