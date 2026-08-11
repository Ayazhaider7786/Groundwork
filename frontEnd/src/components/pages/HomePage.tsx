import { Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { Button } from '../ui/Button';

const CAPABILITIES = [
  {
    title: 'Uptime monitoring',
    body: 'We call your sites and APIs every 1–5 minutes. Down, slow, or returning the wrong status code — you hear about it from us, not from your customers.',
  },
  {
    title: 'Heartbeat monitoring',
    body: 'Your background jobs ping us when they finish. If a ping does not arrive on schedule, the job failed silently and we tell you within minutes.',
  },
  {
    title: 'Public status page',
    body: 'A branded page showing real-time service health. Share it, embed it, and stop answering "is it down?" one email at a time.',
  },
];

const STEPS = [
  { step: '1', title: 'Add a monitor', body: 'Paste a URL for uptime. Create a check and copy the ping URL for heartbeat.' },
  { step: '2', title: 'Configure alerts', body: 'Pick who hears about it: email, Slack, Teams, SMS, WhatsApp or a webhook.' },
  { step: '3', title: 'Relax', body: 'The dashboard shows everything at a glance, and we interrupt you only when something breaks.' },
];

export const HomePage = () => {
  const { user, isAuthenticated } = useAuth();

  return (
    <>
      <section className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-24">
        <p className="text-sm font-medium text-brand">Uptime and heartbeat monitoring</p>

        <h1 className="mt-3 max-w-2xl text-4xl font-semibold tracking-tight text-ink sm:text-5xl">
          Is my stuff running right now?
        </h1>

        <p className="mt-5 max-w-2xl text-lg text-ink-soft">
          {isAuthenticated
            ? `Welcome back, ${user?.fullName}. Everything you are watching is one dashboard away.`
            : 'One tool that watches your websites, your APIs and the background jobs nobody else is looking at. Free to start, $9 a month when you outgrow it.'}
        </p>

        {isAuthenticated ? null : (
          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/signup">
              <Button variant="primary">Create a free account</Button>
            </Link>
            <Link to="/about">
              <Button variant="secondary">Why we built it</Button>
            </Link>
          </div>
        )}
      </section>

      <section className="border-y border-line bg-surface">
        <div className="mx-auto grid max-w-6xl gap-6 px-4 py-14 sm:px-6 md:grid-cols-3">
          {CAPABILITIES.map((capability) => (
            <article key={capability.title} className="rounded-xl border border-line bg-surface-raised p-6">
              <h2 className="text-lg font-semibold text-ink">{capability.title}</h2>
              <p className="mt-2 text-sm leading-relaxed text-ink-soft">{capability.body}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 py-16 sm:px-6">
        <h2 className="text-2xl font-semibold text-ink">How it works</h2>

        <div className="mt-8 grid gap-6 md:grid-cols-3">
          {STEPS.map((item) => (
            <article key={item.step} className="flex gap-4">
              <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-brand-soft text-sm font-semibold text-brand">
                {item.step}
              </span>
              <div>
                <h3 className="font-medium text-ink">{item.title}</h3>
                <p className="mt-1 text-sm leading-relaxed text-ink-soft">{item.body}</p>
              </div>
            </article>
          ))}
        </div>

        <div className="mt-10 rounded-xl border border-line bg-surface p-6">
          <p className="text-sm font-medium text-ink">One line in your background job is the whole integration:</p>
          <pre className="mt-3 overflow-x-auto rounded-lg bg-surface-sunken p-4 text-sm text-ink-soft">
            <code>await httpClient.GetAsync("https://api.deadoralive.dev/ping/abc-123-def");</code>
          </pre>
        </div>
      </section>
    </>
  );
};
