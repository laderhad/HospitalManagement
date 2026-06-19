import { useCallback, useEffect, useState } from 'react';
import { RefreshCw, Save, TestTube2 } from 'lucide-react';
import { CreateLabResultCommand, LabRequestsClient, LabResultsClient } from '../../web-api-client';
import { ApiNotice, dateLabel, EmptyState, LoadingState, PageHeader } from './AdminCommon';

const labRequestsClient = new LabRequestsClient();
const labResultsClient = new LabResultsClient();

const emptyResult = {
  labRequestItemId: '',
  resultValue: '',
  units: '',
  referenceRange: '',
  notes: ''
};

export function AdminLabResultsPage() {
  const [requests, setRequests] = useState([]);
  const [results, setResults] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [detail, setDetail] = useState(null);
  const [form, setForm] = useState(emptyResult);
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState('');

  const loadRequests = useCallback(async nextSelectedId => {
    setLoading(true);
    setError(null);

    try {
      const [requestRecords, resultRecords] = await Promise.all([
        labRequestsClient.getLabRequests(),
        labResultsClient.getLabResults()
      ]);

      setRequests(requestRecords);
      setResults(resultRecords);
      setSelectedId(currentId => {
        const activeSelection = nextSelectedId || currentId || requestRecords[0]?.id || '';
        return requestRecords.some(record => record.id === activeSelection) ? activeSelection : requestRecords[0]?.id || '';
      });
    } catch (requestError) {
      setError(requestError);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadRequests();
  }, [loadRequests]);

  useEffect(() => {
    if (!selectedId) {
      setDetail(null);
      setForm(emptyResult);
      return;
    }

    let mounted = true;
    setDetailLoading(true);
    setError(null);

    labRequestsClient.getLabRequestById(selectedId)
      .then(record => {
        if (!mounted) return;
        setDetail(record);
        setForm(current => {
          const itemIds = record.items?.map(item => item.id) ?? [];
          return {
            ...current,
            labRequestItemId: itemIds.includes(current.labRequestItemId) ? current.labRequestItemId : record.items?.[0]?.id || ''
          };
        });
      })
      .catch(requestError => mounted && setError(requestError))
      .finally(() => mounted && setDetailLoading(false));

    return () => {
      mounted = false;
    };
  }, [selectedId]);

  const updateForm = event => {
    const { name, value } = event.target;
    setForm(current => ({ ...current, [name]: value }));
  };

  const createResult = async event => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess('');

    try {
      await labResultsClient.createLabResult(new CreateLabResultCommand({
        labRequestItemId: form.labRequestItemId,
        resultValue: form.resultValue,
        units: form.units,
        referenceRange: form.referenceRange,
        notes: form.notes || undefined
      }));
      setForm(current => ({ ...emptyResult, labRequestItemId: current.labRequestItemId }));
      setSuccess('Lab result saved.');
      await loadRequests(selectedId);
    } catch (requestError) {
      setError(requestError);
    } finally {
      setSaving(false);
    }
  };

  const selectedResults = results.filter(result => result.labRequestId === selectedId);

  return (
    <section className="admin-page">
      <PageHeader
        title="Lab results"
        description="Enter results for lab request items created from clinical examinations."
        actions={(
          <button type="button" className="secondary compact-action" onClick={() => loadRequests()} disabled={loading}>
            <RefreshCw size={16} strokeWidth={2} />
            Refresh
          </button>
        )}
      />
      <ApiNotice error={error} success={success} />

      <div className="management-grid lab-results-grid">
        <section className="record-pane">
          <div className="pane-title">
            <TestTube2 size={18} strokeWidth={2} />
            <h2>Lab request queue</h2>
          </div>
          <div className="directory-summary">
            <span>{requests.length} requests</span>
            <small>Open requests and result counts</small>
          </div>

          {loading ? <LoadingState /> : requests.length ? (
            <ul className="record-list">
              {requests.map(request => (
                <li key={request.id}>
                  <button
                    type="button"
                    className={request.id === selectedId ? 'selected' : undefined}
                    onClick={() => setSelectedId(request.id)}
                  >
                    <span>
                      <strong>{request.patientFullName}</strong>
                      <small>{request.doctorFullName} | {dateLabel(request.requestDate)}</small>
                    </span>
                    <span className="compact-count">{request.resultCount}/{request.itemCount}</span>
                  </button>
                </li>
              ))}
            </ul>
          ) : (
            <EmptyState title="No lab requests" body="Doctors create lab requests from examinations." />
          )}
        </section>

        <section className="detail-pane">
          {detailLoading ? <LoadingState label="Loading lab request" /> : detail ? (
            <>
              <div className="detail-heading">
                <div>
                  <h2>{detail.patientFullName}</h2>
                  <p>{detail.doctorFullName} | {dateLabel(detail.requestDate)}</p>
                </div>
              </div>

              <form className="detail-form" onSubmit={createResult}>
                <div className="field-grid">
                  <label className="span-two">Test item
                    <select required name="labRequestItemId" value={form.labRequestItemId} onChange={updateForm}>
                      <option value="" disabled>Select test item</option>
                      {detail.items?.map(item => (
                        <option key={item.id} value={item.id}>{item.testName} ({item.resultCount} results)</option>
                      ))}
                    </select>
                  </label>
                  <label>Result value<input required name="resultValue" value={form.resultValue} onChange={updateForm} /></label>
                  <label>Units<input required name="units" value={form.units} onChange={updateForm} /></label>
                  <label className="span-two">Reference range<input required name="referenceRange" value={form.referenceRange} onChange={updateForm} /></label>
                  <label className="span-two">Notes<textarea rows={3} name="notes" value={form.notes} onChange={updateForm} /></label>
                </div>
                <div className="form-actions">
                  <button disabled={saving || !detail.items?.length}>
                    <Save size={16} strokeWidth={2} />
                    Save result
                  </button>
                </div>
              </form>

              <section className="clinical-list-section">
                <h3>Recorded results</h3>
                {selectedResults.length ? (
                  <ul className="summary-list">
                    {selectedResults.map(result => (
                      <li key={result.id}>
                        <strong>{result.testName}</strong>
                        <span>{result.resultValue} {result.units} | {result.referenceRange}</span>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <EmptyState title="No results recorded" body="Save the first result for this request." />
                )}
              </section>
            </>
          ) : (
            <EmptyState title="Select a lab request" body="Test items and result entry controls appear here." />
          )}
        </section>
      </div>
    </section>
  );
}
