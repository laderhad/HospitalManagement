export function formatApiError(error) {
  if (error?.status === 401) return 'Your session is not authorized for this request. Log in again and retry.';
  if (error?.status === 403) return 'This account does not have access for this area.';

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
