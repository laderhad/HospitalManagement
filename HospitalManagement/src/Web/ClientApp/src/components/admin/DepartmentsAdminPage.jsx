import { useCallback, useEffect, useState } from 'react';
import { Building2, Plus, RefreshCw, Save } from 'lucide-react';
import {
  CreateDepartmentCommand,
  DepartmentsClient,
  SetDepartmentActiveStateCommand,
  UpdateDepartmentCommand
} from '../../web-api-client';
import { ActiveBadge, ApiNotice, EmptyState, LoadingState, PageHeader } from './AdminCommon';

const departmentsClient = new DepartmentsClient();

export function DepartmentsAdminPage() {
  const [departments, setDepartments] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [createName, setCreateName] = useState('');
  const [editName, setEditName] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState('');

  const selectedDepartment = departments.find(department => department.id === selectedId);

  const loadDepartments = useCallback(async (nextSelectedId) => {
    setLoading(true);
    setError(null);

    try {
      const records = await departmentsClient.getDepartments();
      setDepartments(records);
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
    loadDepartments();
  }, [loadDepartments]);

  useEffect(() => {
    setEditName(selectedDepartment?.name ?? '');
  }, [selectedDepartment]);

  const createDepartment = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      const id = await departmentsClient.createDepartment(new CreateDepartmentCommand({ name: createName.trim() }));
      setCreateName('');
      setIsCreating(false);
      setSuccess('Department created.');
      await loadDepartments(id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const updateDepartment = async event => {
    event.preventDefault();
    if (!selectedDepartment) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await departmentsClient.updateDepartment(
        selectedDepartment.id,
        new UpdateDepartmentCommand({ id: selectedDepartment.id, name: editName.trim() })
      );
      setSuccess('Department updated.');
      await loadDepartments(selectedDepartment.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const setDepartmentActiveState = async () => {
    if (!selectedDepartment) return;

    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await departmentsClient.setDepartmentActiveState(
        selectedDepartment.id,
        new SetDepartmentActiveStateCommand({
          id: selectedDepartment.id,
          isActive: !selectedDepartment.isActive
        })
      );
      setSuccess(selectedDepartment.isActive ? 'Department deactivated.' : 'Department activated.');
      await loadDepartments(selectedDepartment.id);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="admin-page">
      <PageHeader
        title="Departments"
        description="Maintain the clinical units doctors and appointments depend on."
        actions={(
          <>
            <button type="button" className="compact-action" onClick={() => setIsCreating(true)}>
              <Plus size={16} strokeWidth={2} />
              New department
            </button>
            <button type="button" className="secondary compact-action" onClick={() => loadDepartments()} disabled={loading}>
              <RefreshCw size={16} strokeWidth={2} />
              Refresh
            </button>
          </>
        )}
      />
      <ApiNotice error={error} success={success} />

      <div className="management-grid">
        <section className="record-pane">
          <div className="pane-title">
            <Building2 size={18} strokeWidth={2} />
            <h2>Department registry</h2>
          </div>
          <div className="directory-summary">
            <span>{departments.length} departments</span>
            <small>Doctor profiles are assigned to these clinical units.</small>
          </div>

          {loading ? <LoadingState /> : departments.length ? (
            <ul className="record-list">
              {departments.map(department => (
                <li key={department.id}>
                  <button
                    type="button"
                    className={department.id === selectedId ? 'selected' : undefined}
                    onClick={() => {
                      setIsCreating(false);
                      setSelectedId(department.id);
                    }}
                  >
                    <span>
                      <strong>{department.name}</strong>
                      <small>{department.isActive ? 'Available for active workflows' : 'Not available for new work'}</small>
                    </span>
                    <ActiveBadge active={department.isActive} />
                  </button>
                </li>
              ))}
            </ul>
          ) : (
            <EmptyState title="No departments yet" body="Create the first department before adding doctors." />
          )}
        </section>

        <section className="detail-pane">
          {isCreating ? (
            <>
              <div className="detail-heading">
                <div>
                  <h2>New department</h2>
                  <p>Create the unit before assigning doctors to it.</p>
                </div>
              </div>
              <form className="detail-form" onSubmit={createDepartment}>
                <label>
                  Department name
                  <input
                    required
                    maxLength={100}
                    value={createName}
                    onChange={event => setCreateName(event.target.value)}
                    placeholder="Cardiology"
                    autoFocus
                  />
                </label>
                <div className="form-actions">
                  <button disabled={saving || !createName.trim()}>
                    <Plus size={16} strokeWidth={2} />
                    Create department
                  </button>
                  <button type="button" className="secondary" onClick={() => setIsCreating(false)}>
                    Cancel
                  </button>
                </div>
              </form>
            </>
          ) : selectedDepartment ? (
            <>
              <div className="detail-heading">
                <div>
                  <h2>{selectedDepartment.name}</h2>
                  <p>Update the name or pause this department for new work.</p>
                </div>
                <ActiveBadge active={selectedDepartment.isActive} />
              </div>

              <form className="detail-form" onSubmit={updateDepartment}>
                <label>
                  Department name
                  <input
                    required
                    maxLength={100}
                    value={editName}
                    onChange={event => setEditName(event.target.value)}
                  />
                </label>
                <div className="form-actions">
                  <button disabled={saving || !editName.trim()}>
                    <Save size={16} strokeWidth={2} />
                    Save name
                  </button>
                  <button type="button" className="secondary" onClick={setDepartmentActiveState} disabled={saving}>
                    {selectedDepartment.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                </div>
              </form>
            </>
          ) : (
            <EmptyState title="Select a department" body="Department status and name edits appear here." />
          )}
        </section>
      </div>
    </section>
  );
}
