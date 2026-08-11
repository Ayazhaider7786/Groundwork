import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { seedDataService } from '../../services/seedDataService';
import type { SeedUser } from '../../types/seedData';
import { extractApiError } from '../../utils/apiErrors';
import { Alert } from '../ui/Alert';
import { Button } from '../ui/Button';
import { TextField } from '../ui/TextField';

export const LoginPage = () => {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [isSeedPanelOpen, setIsSeedPanelOpen] = useState(false);
  const [seedUsers, setSeedUsers] = useState<SeedUser[] | null>(null);
  const [seedError, setSeedError] = useState('');
  const [isLoadingSeedUsers, setIsLoadingSeedUsers] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      await login({ email, password });
      navigate('/', { replace: true });
    } catch (caught) {
      setError(extractApiError(caught, 'Could not sign you in. Please try again.'));
    } finally {
      setIsSubmitting(false);
    }
  };

  const toggleSeedPanel = async () => {
    if (isSeedPanelOpen) {
      setIsSeedPanelOpen(false);
      return;
    }

    setIsSeedPanelOpen(true);

    // Fetch once, then keep the list — reopening the panel should not re-hit the API.
    if (seedUsers !== null) {
      return;
    }

    setSeedError('');
    setIsLoadingSeedUsers(true);

    try {
      setSeedUsers(await seedDataService.getSeedUsers());
    } catch (caught) {
      setSeedError(extractApiError(caught, 'Could not load seed accounts. Is the API running?'));
    } finally {
      setIsLoadingSeedUsers(false);
    }
  };

  const applySeedUser = (seedUser: SeedUser) => {
    setEmail(seedUser.email);
    setPassword(seedUser.password);
    setError('');
    setIsSeedPanelOpen(false);
  };

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="mx-auto max-w-md px-4 py-16 sm:px-6">
      <h1 className="text-3xl font-semibold tracking-tight text-ink">Log in</h1>

      <p className="mt-2 text-ink-soft">Welcome back. Let us see what is still standing.</p>

      <form onSubmit={handleSubmit} noValidate className="mt-8 flex flex-col gap-5">
        {error ? <Alert tone="danger">{error}</Alert> : null}

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
          autoComplete="current-password"
          onChange={(event) => setPassword(event.target.value)}
        />

        <Button type="submit" isLoading={isSubmitting}>
          Log in
        </Button>
      </form>

      {/*
        Development affordance only. The endpoint 404s outside Development, and this
        block is compiled out of a production build entirely so the bundle never
        even advertises it.
      */}
      {import.meta.env.DEV ? (
        <div className="mt-8 rounded-xl border border-dashed border-line p-4">
          <div className="flex items-center justify-between gap-3">
            <div>
              <p className="text-sm font-medium text-ink">Seed data</p>
              <p className="text-xs text-muted">Development only — fills the form for you.</p>
            </div>

            <Button variant="secondary" type="button" onClick={toggleSeedPanel} isLoading={isLoadingSeedUsers}>
              {isSeedPanelOpen ? 'Hide' : 'Seed data'}
            </Button>
          </div>

          {isSeedPanelOpen ? (
            <div className="mt-4">
              {seedError ? <Alert tone="danger">{seedError}</Alert> : null}

              {!seedError && seedUsers !== null && seedUsers.length === 0 ? (
                <Alert tone="warning">
                  No seed accounts are configured. Add one under the SeedData:Users section and set its
                  password with user-secrets.
                </Alert>
              ) : null}

              {seedUsers !== null && seedUsers.length > 0 ? (
                <ul className="flex flex-col gap-2">
                  {seedUsers.map((seedUser) => (
                    <li key={seedUser.email}>
                      <button
                        type="button"
                        onClick={() => applySeedUser(seedUser)}
                        className="flex w-full flex-col gap-0.5 rounded-lg border border-line bg-surface px-3 py-2.5 text-left transition-colors hover:border-brand hover:bg-brand-soft"
                      >
                        <span className="flex items-center justify-between gap-2">
                          <span className="text-sm font-medium text-ink">{seedUser.fullName}</span>
                          <span className="rounded-full bg-brand-soft px-2 py-0.5 text-xs font-medium text-brand">
                            {seedUser.role}
                          </span>
                        </span>
                        <span className="text-xs text-muted">{seedUser.email}</span>
                      </button>
                    </li>
                  ))}
                </ul>
              ) : null}
            </div>
          ) : null}
        </div>
      ) : null}

      <p className="mt-6 text-sm text-ink-soft">
        No account yet?{' '}
        <Link to="/signup" className="font-medium text-brand hover:underline">
          Sign up
        </Link>
      </p>
    </div>
  );
};
