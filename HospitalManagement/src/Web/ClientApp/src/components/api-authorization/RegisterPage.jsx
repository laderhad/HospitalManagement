import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from './AuthContext';
import { Button, FieldGrid, Input, Select, Textarea } from '../ui';

const initialForm = {
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  gender: '0',
  contactNumber: '',
  address: ''
};

const passwordRules = [
  { test: value => value.length >= 6, message: 'at least 6 characters' },
  { test: value => /[a-z]/.test(value), message: 'one lowercase letter' },
  { test: value => /[A-Z]/.test(value), message: 'one uppercase letter' },
  { test: value => /\d/.test(value), message: 'one number' },
  { test: value => /[^A-Za-z0-9]/.test(value), message: 'one symbol' }
];

const genderOptions = [
  ['0', 'Unknown'],
  ['1', 'Female'],
  ['2', 'Male'],
  ['3', 'Other']
];

function validateEmail(value) {
  return /^[^\s@]+@[^\s@]+$/.test(value.trim());
}

function missingPasswordRules(value) {
  return passwordRules.filter(rule => !rule.test(value)).map(rule => rule.message);
}

function getFormValues(formElement) {
  const data = new FormData(formElement);

  return {
    email: data.get('email')?.toString() ?? '',
    password: data.get('password')?.toString() ?? '',
    firstName: data.get('firstName')?.toString() ?? '',
    lastName: data.get('lastName')?.toString() ?? '',
    dateOfBirth: data.get('dateOfBirth')?.toString() ?? '',
    gender: data.get('gender')?.toString() ?? '0',
    contactNumber: data.get('contactNumber')?.toString() ?? '',
    address: data.get('address')?.toString() ?? ''
  };
}

function isRegistrationFormValid(values) {
  const requiredFieldsValid = [
    values.firstName,
    values.lastName,
    values.dateOfBirth,
    values.contactNumber,
    values.address
  ].every(value => value.trim());

  return validateEmail(values.email)
    && missingPasswordRules(values.password).length === 0
    && requiredFieldsValid;
}

function formatRegistrationError(error) {
  const response = parseResponse(error?.response);
  const validationErrors = response?.errors
    ? Object.values(response.errors).flat().filter(Boolean)
    : [];

  if (validationErrors.length) return validationErrors.join(' ');
  if (response?.detail) return response.detail;
  if (response?.title) return response.title;
  return 'Registration failed. Please check the form and try again.';
}

function parseResponse(response) {
  if (!response || typeof response !== 'string') return null;
  try {
    return JSON.parse(response);
  } catch {
    return null;
  }
}

export function RegisterPage() {
  const [form, setForm] = useState(initialForm);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');
  const { register } = useAuth();
  const navigate = useNavigate();

  const emailValid = validateEmail(form.email);
  const passwordMissing = missingPasswordRules(form.password);
  const passwordValid = passwordMissing.length === 0;
  const formValid = isRegistrationFormValid(form);

  const updateForm = event => {
    const { name, value } = event.target;
    setError('');
    setForm(current => ({ ...current, [name]: value }));
  };

  const handleSubmit = async event => {
    event.preventDefault();
    setSubmitted(true);
    setError('');

    const submittedForm = getFormValues(event.currentTarget);
    setForm(submittedForm);

    if (!isRegistrationFormValid(submittedForm)) return;

    try {
      await register({
        email: submittedForm.email.trim(),
        password: submittedForm.password,
        firstName: submittedForm.firstName.trim(),
        lastName: submittedForm.lastName.trim(),
        dateOfBirth: new Date(`${submittedForm.dateOfBirth}T00:00:00Z`),
        gender: Number(submittedForm.gender),
        contactNumber: submittedForm.contactNumber.trim(),
        address: submittedForm.address.trim()
      });
      navigate('/login', { state: { message: 'Patient account created. You can log in now.' } });
    } catch (registrationError) {
      setError(formatRegistrationError(registrationError));
    }
  };

  return (
    <article className="auth-card auth-card-wide">
      <header className="auth-header">
        <h2>Register patient</h2>
        <p>Create a patient account and profile in one step.</p>
      </header>
      {error && <p className="error">{error}</p>}
      <form className="detail-form" onSubmit={handleSubmit}>
        <FieldGrid>
          <Input
            required
            label="Email"
            type="email"
            id="email"
            name="email"
            autoComplete="username"
            value={form.email}
            onChange={updateForm}
            aria-invalid={submitted && !emailValid ? true : undefined}
          />
          <Input
            required
            label="Password"
            type="password"
            id="password"
            name="password"
            autoComplete="new-password"
            value={form.password}
            onChange={updateForm}
            aria-invalid={submitted && !passwordValid ? true : undefined}
            aria-describedby="password-helper"
          />
          <small id="password-helper" className="span-two">
            {passwordMissing.length ? `Password needs ${passwordMissing.join(', ')}.` : 'Use at least 6 characters with uppercase, lowercase, number, and symbol.'}
          </small>
          <Input required label="First name" id="firstName" name="firstName" value={form.firstName} onChange={updateForm} />
          <Input required label="Last name" id="lastName" name="lastName" value={form.lastName} onChange={updateForm} />
          <Input required label="Date of birth" type="date" id="dateOfBirth" name="dateOfBirth" value={form.dateOfBirth} onChange={updateForm} />
          <Select label="Gender" id="gender" name="gender" value={form.gender} onChange={updateForm}>
            {genderOptions.map(([value, label]) => <option key={value} value={value}>{label}</option>)}
          </Select>
          <Input required label="Contact number" id="contactNumber" name="contactNumber" className="span-two" value={form.contactNumber} onChange={updateForm} />
          <Textarea required label="Address" rows={3} id="address" name="address" className="span-two" value={form.address} onChange={updateForm} />
        </FieldGrid>
        <Button type="submit">Register patient</Button>
        <p className="auth-switch">Already have an account? <Link to="/login">Log in</Link></p>
      </form>
    </article>
  );
}
