import { AlertTriangle, LockKeyhole } from 'lucide-react';
import { appointmentStatuses } from './constants';
import { formatApiError } from './formatters';

export function classNames(...values) {
  return values.filter(Boolean).join(' ');
}

export function PageHeader({ title, description, actions }) {
  return (
    <header className="page-header">
      <div className="page-title-group">
        <h1>{title}</h1>
        {description && <p>{description}</p>}
      </div>
      {actions && <div className="page-actions">{actions}</div>}
    </header>
  );
}

export function Button({ variant = 'primary', size = 'md', className = '', children, ...props }) {
  return (
    <button
      className={classNames('ui-button', `ui-button-${variant}`, `ui-button-${size}`, className)}
      {...props}
    >
      {children}
    </button>
  );
}

export function IconButton({ label, className = '', children, ...props }) {
  return (
    <button className={classNames('icon-button', className)} aria-label={label} title={label} {...props}>
      {children}
    </button>
  );
}

export function Input({ label, className = '', ...props }) {
  return (
    <label className={classNames('form-field', className)}>
      <span>{label}</span>
      <input {...props} />
    </label>
  );
}

export function Select({ label, className = '', children, ...props }) {
  return (
    <label className={classNames('form-field', className)}>
      <span>{label}</span>
      <select {...props}>{children}</select>
    </label>
  );
}

export function Textarea({ label, className = '', ...props }) {
  return (
    <label className={classNames('form-field', className)}>
      <span>{label}</span>
      <textarea {...props} />
    </label>
  );
}

export function FieldGrid({ children, className = '' }) {
  return <div className={classNames('field-grid', className)}>{children}</div>;
}

export function FormActions({ children, className = '' }) {
  return <div className={classNames('form-actions', className)}>{children}</div>;
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

export function PaneTitle({ icon: Icon, title, children, className = '' }) {
  return (
    <div className={classNames('pane-title', className)}>
      {Icon && <Icon size={18} strokeWidth={2} />}
      <h2>{title}</h2>
      {children}
    </div>
  );
}

export function DetailHeader({ kicker, title, description, aside }) {
  return (
    <div className="detail-heading">
      <div>
        {kicker && <span className="detail-kicker">{kicker}</span>}
        <h2>{title}</h2>
        {description && <p>{description}</p>}
      </div>
      {aside && <div className="detail-aside">{aside}</div>}
    </div>
  );
}

export function DirectorySummary({ count, label, detail }) {
  return (
    <div className="directory-summary">
      <span>{count} {label}</span>
      {detail && <small>{detail}</small>}
    </div>
  );
}

export function Panel({ title, icon, children, className = '', actions }) {
  return (
    <section className={classNames('ui-panel', className)}>
      {(title || actions) && (
        <header className="ui-panel-header">
          {title && <PaneTitle icon={icon} title={title} />}
          {actions && <div className="panel-actions">{actions}</div>}
        </header>
      )}
      {children}
    </section>
  );
}

export function ApiNotice({ error, success }) {
  if (!error && !success) return null;

  const forbidden = error?.status === 403;
  const Icon = forbidden ? LockKeyhole : AlertTriangle;

  return (
    <aside className={classNames('api-notice', error ? 'error' : 'success')} role={error ? 'alert' : 'status'}>
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
  return <span className={classNames('status-badge', `status-${label.toLowerCase()}`)}>{label}</span>;
}

export function ActiveBadge({ active }) {
  return <span className={classNames('status-badge', active ? 'status-active' : 'status-inactive')}>{active ? 'Active' : 'Inactive'}</span>;
}

export function SummaryBlock({ title, records, label, icon: Icon, limit = 5 }) {
  return (
    <div className="summary-block">
      <h4>{Icon && <Icon size={15} strokeWidth={2} />}{title}</h4>
      {records.length ? (
        <ul className="summary-list">
          {records.slice(0, limit).map(record => <li key={record.id}>{label(record)}</li>)}
        </ul>
      ) : (
        <span>No records</span>
      )}
    </div>
  );
}
