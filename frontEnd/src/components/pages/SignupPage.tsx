import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { extractApiError } from '../../utils/apiErrors';
import { Alert } from '../ui/Alert';
import { Button } from '../ui/Button';
import { TextField } from '../ui/TextField';

export const SignupPage = () => {
  const { register, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      await register({ fullName, email, password });
      navigate('/', { replace: true });
    } catch (caught) {
      setError(extractApiError(caught, 'Could not create your account. Please try again.'));
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="mx-auto max-w-md px-4 py-16 sm:px-6">
      <h1 className="text-3xl font-semibold tracking-tight text-ink">Sign up</h1>

      <p className="mt-2 text-ink-soft">Five monitors, one-minute checks, no card. Start watching things.</p>

      <form onSubmit={handleSubmit} noValidate className="mt-8 flex flex-col gap-5">
        {error ? <Alert tone="danger">{error}</Alert> : null}

        <TextField
          label="Full name"
          name="fullName"
          value={fullName}
          required
          autoComplete="name"
          onChange={(event) => setFullName(event.target.value)}
        />

        <TextField
          label="Email address"
          name="email"
          type="email"
          value={email}
          required
          autoComplete="email"
          onChange={(event) => setEmail(event.target.value)}
        />

        <TextField
          label="Password"
          name="password"
          type="password"
          value={password}
          required
          autoComplete="new-password"
          onChange={(event) => setPassword(event.target.value)}
        />

        <p className="-mt-2 text-xs text-muted">
          At least 8 characters, with an uppercase letter, a lowercase letter and a digit.
        </p>

        <Button type="submit" isLoading={isSubmitting}>
          Create account
        </Button>
      </form>

      <p className="mt-6 text-sm text-ink-soft">
        Already have an account?{' '}
        <Link to="/login" className="font-medium text-brand hover:underline">
          Log in
        </Link>
      </p>
    </div>
  );
};
