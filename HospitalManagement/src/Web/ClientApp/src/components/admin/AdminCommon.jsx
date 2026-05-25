import { AlertTriangle, LockKeyhole } from 'lucide-react';

export const appointmentStatuses = {
  0: 'Scheduled',
  1: 'Completed',
  2: 'Cancelled'
};

export const genderLabels = {
  0: 'Unknown',
  1: 'Female',
  2: 'Male',
  3: 'Other'
};

export function PageHeader({ title, description, actions }) {
  return (
    <header className="page-header">
      <div>
        <h1>{title}</h1>
        {description && <p>{description}</p>}
      </div>
      {actions && <div className="page-actions">{actions}</div>}
    </header>
  );
}

export function Button({ variant = 'primary', className = '', children, ...props }) {
  const classes = ['ui-button', `ui-button-${variant}`, className].filter(Boolean).join(' ');
  return <button className={classes} {...props}>{children}</button>;
}

export function Input({ label, className = '', ...props }) {
  return (
    <label className={['form-field', className].filter(Boolean).join(' ')}>
      <span>{label}</span>
      <input {...props} />
    </label>
  );
}

export function Select({ label, className = '', children, ...props }) {
  return (
    <label className={['form-field', className].filter(Boolean).join(' ')}>
      <span>{label}</span>
      <select {...props}>{children}</select>
    </label>
  );
}

export function Textarea({ label, className = '', ...props }) {
  return (
    <label className={['form-field', className].filter(Boolean).join(' ')}>
      <span>{label}</span>
      <textarea {...props} />
    </label>
  );
}

export function FormSection({ title, description, children }) {
  return (
    <section className="form-section">
      <header>
        <h3>{title}</h3>
        {description && <p>{description}</p>}
      </header>
      <div>{children}</div>
    </section>
  );
}

export function ApiNotice({ error, success }) {
  if (!error && !success) return null;

  const forbidden = error?.status === 403;
  const Icon = forbidden ? LockKeyhole : AlertTriangle;

  return (
    <aside className={`api-notice${error ? ' error' : ' success'}`} role={error ? 'alert' : 'status'}>
      {error && <Icon size={18} strokeWidth={2} />}
      <span>{error ? formatApiError(error) : success}</span>
    </aside>
  );
}

export function EmptyState({ title, body }) {
  return (
    <div className="empty-state">
      <strong>{title}</strong>
      {body && <span>{body}</span>}
    </div>
  );
}

export function LoadingState({ label = 'Loading records' }) {
  return <p className="loading-state" aria-busy="true">{label}</p>;
}

export function StatusBadge({ status }) {
  const label = appointmentStatuses[status] ?? 'Unknown';
  return <span className={`status-badge status-${label.toLowerCase()}`}>{label}</span>;
}

export function ActiveBadge({ active }) {
  return <span className={`status-badge ${active ? 'status-active' : 'status-inactive'}`}>{active ? 'Active' : 'Inactive'}</span>;
}

export function formatApiError(error) {
  if (error?.status === 401) return 'Your session is not authorized for this request. Log in again and retry.';
  if (error?.status === 403) return 'This account does not have administrator access for this area.';

  const response = parseResponse(error?.response);
  const validationErrors = response?.errors
    ? Object.values(response.errors).flat().filter(Boolean)
    : [];

  if (validationErrors.length) return validationErrors.join(' ');
  if (response?.detail) return response.detail;
  if (response?.title) return response.title;
  return error?.message || 'The request could not be completed.';
}

export function dateLabel(value) {
  if (!value) return 'Not set';
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(new Date(value));
}

export function shortDateLabel(value) {
  if (!value) return 'Not set';
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(value));
}

export function dateInputValue(value) {
  if (!value) return '';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '' : date.toISOString().slice(0, 10);
}

export function dateTimeInputValue(value) {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function parseResponse(response) {
  if (!response || typeof response !== 'string') return null;
  try {
    return JSON.parse(response);
  } catch {
    return null;
  }
}
