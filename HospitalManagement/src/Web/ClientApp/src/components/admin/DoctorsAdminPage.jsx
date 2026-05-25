import { useCallback, useEffect, useState } from 'react';
import { Plus, RefreshCw, Save, Stethoscope } from 'lucide-react';
import {
  CreateDoctorCommand,
  DepartmentsClient,
  DoctorsClient,
  SetDoctorActiveStateCommand,
  UpdateDoctorCommand
} from '../../web-api-client';
import { ActiveBadge, ApiNotice, EmptyState, LoadingState, PageHeader } from './AdminCommon';

const departmentsClient = new DepartmentsClient();
const doctorsClient = new DoctorsClient();

const emptyDoctor = {
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  departmentId: '',
  contactNumber: '',
  specialty: '',
  title: ''
};

export function DoctorsAdminPage() {
  const [doctors, setDoctors] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [detail, setDetail] = useState(null);
  const [createForm, setCreateForm] = useState(emptyDoctor);
  const [editForm, setEditForm] = useState(null);
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState('');

  const loadDoctors = useCallback(async nextSelectedId => {
    setLoading(true);
    setError(null);

    try {
      const [doctorRecords, departmentRecords] = await Promise.all([
        doctorsClient.getDoctors(),
        departmentsClient.getDepartments()
      ]);

      setDoctors(doctorRecords);
      setDepartments(departmentRecords);
      setCreateForm(current => ({
        ...current,
        departmentId: current.departmentId || departmentRecords.find(department => department.isActive)?.id || departmentRecords[0]?.id || ''
      }));

      setSelectedId(currentId => {
        const activeSelection = nextSelectedId || currentId || doctorRecords[0]?.id || '';
        return doctorRecords.some(record => record.id === activeSelection) ? activeSelection : doctorRecords[0]?.id || '';
      });
    } catch (requestError) {
      setError(requestError);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDoctors();
  }, [loadDoctors]);

  useEffect(() => {
    if (!selectedId) {
      setDetail(null);
      setEditForm(null);
      return;
    }

    let mounted = true;
    setDetailLoading(true);
    setError(null);

    doctorsClient.getDoctorById(selectedId)
      .then(record => {
        if (!mounted) return;
        setDetail(record);
        setEditForm({
          firstName: record.firstName,
          lastName: record.lastName,
          departmentId: record.departmentId,
          contactNumber: record.contactNumber,
          specialty: record.specialty ?? '',
          title: record.title ?? ''
        });
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

  const updateEditForm = event => {
    const { name, value } = event.target;
    setEditForm(current => ({ ...current, [name]: value }));
  };

  const createDoctor = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      const id = await doctorsClient.createDoctor(new CreateDoctorCommand(createForm));
      setCreateForm(current => ({
        ...emptyDoctor,
        departmentId: current.departmentId
      }));
      setIsCreating(false);
      setSuccess('Doctor account and profile created.');
      await loadDoctors(id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const saveDoctor = async event => {
    event.preventDefault();
    if (!detail || !editForm) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await doctorsClient.updateDoctor(detail.id, new UpdateDoctorCommand({ id: detail.id, ...editForm }));
      setSuccess('Doctor profile updated.');
      await loadDoctors(detail.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const setDoctorActiveState = async () => {
    if (!detail) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await doctorsClient.setDoctorActiveState(
        detail.id,
        new SetDoctorActiveStateCommand({ id: detail.id, isActive: !detail.isActive })
      );
      setSuccess(detail.isActive ? 'Doctor deactivated.' : 'Doctor activated.');
      await loadDoctors(detail.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="admin-page">
      <PageHeader
        title="Doctors"
        description="Create doctor accounts and keep their department assignments current."
        actions={(
          <>
            <button type="button" className="compact-action" onClick={() => setIsCreating(true)}>
              <Plus size={16} strokeWidth={2} />
              New doctor
            </button>
            <button type="button" className="secondary compact-action" onClick={() => loadDoctors()} disabled={loading}>
              <RefreshCw size={16} strokeWidth={2} />
              Refresh
            </button>
          </>
        )}
      />
      <ApiNotice error={error} success={success} />

      <div className="management-grid doctors-grid">
        <section className="record-pane">
          <div className="pane-title">
            <Stethoscope size={18} strokeWidth={2} />
            <h2>Doctor registry</h2>
          </div>
          <div className="directory-summary">
            <span>{doctors.length} doctors</span>
            <small>Select a doctor to update assignment and availability.</small>
          </div>

          {loading ? <LoadingState /> : doctors.length ? (
            <ul className="record-list">
              {doctors.map(doctor => (
                <li key={doctor.id}>
                  <button
                    type="button"
                    className={doctor.id === selectedId ? 'selected' : undefined}
                    onClick={() => {
                      setIsCreating(false);
                      setSelectedId(doctor.id);
                    }}
                  >
                    <span>
                      <strong>{doctor.firstName} {doctor.lastName}</strong>
                      <small>{doctor.departmentName}</small>
                    </span>
                    <ActiveBadge active={doctor.isActive} />
                  </button>
                </li>
              ))}
            </ul>
          ) : (
            <EmptyState title="No doctors yet" body="Create a department, then add a doctor account." />
          )}
        </section>

        <section className="detail-pane">
          {isCreating ? (
            <>
              <div className="detail-heading">
                <div>
                  <h2>New doctor</h2>
                  <p>Create the account and assign its clinical unit.</p>
                </div>
              </div>
              <form className="detail-form" onSubmit={createDoctor}>
                <div className="field-grid">
                  <label>Email<input required type="email" maxLength={256} name="email" value={createForm.email} onChange={updateCreateForm} autoFocus /></label>
                  <label>Password<input required minLength={8} type="password" name="password" value={createForm.password} onChange={updateCreateForm} /></label>
                  <label>First name<input required maxLength={100} name="firstName" value={createForm.firstName} onChange={updateCreateForm} /></label>
                  <label>Last name<input required maxLength={100} name="lastName" value={createForm.lastName} onChange={updateCreateForm} /></label>
                  <label>Department
                    <select required name="departmentId" value={createForm.departmentId} onChange={updateCreateForm}>
                      <option value="" disabled>Select department</option>
                      {departments.map(department => <option key={department.id} value={department.id}>{department.name}</option>)}
                    </select>
                  </label>
                  <label>Contact number<input required maxLength={20} name="contactNumber" value={createForm.contactNumber} onChange={updateCreateForm} /></label>
                  <label>Specialty<input maxLength={100} name="specialty" value={createForm.specialty} onChange={updateCreateForm} /></label>
                  <label>Title<input maxLength={100} name="title" value={createForm.title} onChange={updateCreateForm} /></label>
                </div>
                <div className="form-actions">
                  <button disabled={saving || !departments.length}>
                    <Plus size={16} strokeWidth={2} />
                    Create doctor
                  </button>
                  <button type="button" className="secondary" onClick={() => setIsCreating(false)}>
                    Cancel
                  </button>
                </div>
              </form>
            </>
          ) : detailLoading ? <LoadingState label="Loading doctor detail" /> : detail && editForm ? (
            <>
              <div className="detail-heading">
                <div>
                  <h2>{detail.firstName} {detail.lastName}</h2>
                  <p>{detail.departmentName}</p>
                </div>
                <ActiveBadge active={detail.isActive} />
              </div>

              <form className="detail-form" onSubmit={saveDoctor}>
                <div className="field-grid">
                  <label>First name<input required maxLength={100} name="firstName" value={editForm.firstName} onChange={updateEditForm} /></label>
                  <label>Last name<input required maxLength={100} name="lastName" value={editForm.lastName} onChange={updateEditForm} /></label>
                  <label>Department
                    <select required name="departmentId" value={editForm.departmentId} onChange={updateEditForm}>
                      {departments.map(department => <option key={department.id} value={department.id}>{department.name}</option>)}
                    </select>
                  </label>
                  <label>Contact number<input required maxLength={20} name="contactNumber" value={editForm.contactNumber} onChange={updateEditForm} /></label>
                  <label>Specialty<input maxLength={100} name="specialty" value={editForm.specialty} onChange={updateEditForm} /></label>
                  <label>Title<input maxLength={100} name="title" value={editForm.title} onChange={updateEditForm} /></label>
                </div>
                <div className="form-actions">
                  <button disabled={saving}>
                    <Save size={16} strokeWidth={2} />
                    Save profile
                  </button>
                  <button type="button" className="secondary" onClick={setDoctorActiveState} disabled={saving}>
                    {detail.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                </div>
              </form>
            </>
          ) : (
            <EmptyState title="Select a doctor" body="Profile, assignment, and active-state controls appear here." />
          )}
        </section>
      </div>
    </section>
  );
}
