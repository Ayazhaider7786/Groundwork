const DIFFERENTIATORS = [
  {
    title: 'Both halves, one price',
    body: 'Uptime tools ignore your cron jobs. Cron tools ignore your website. The few that do both start above $20 a month. We do both from $0.',
  },
  {
    title: 'Simplicity as a feature',
    body: 'No agents to install, no YAML to write, no setup wizard. Paste a URL, or copy a ping URL. Onboarding should take under two minutes.',
  },
  {
    title: '.NET first',
    body: 'A dedicated NuGet package that plugs into IHostedService, Hangfire and Quartz.NET, instead of thirty minutes reading generic HTTP docs.',
  },
  {
    title: 'Alerts where you actually are',
    body: 'WhatsApp and SMS alongside Slack and Teams, because Slack adoption is not universal outside a handful of markets.',
  },
];

export const AboutUsPage = () => (
  <div className="mx-auto max-w-3xl px-4 py-16 sm:px-6">
    <h1 className="text-3xl font-semibold tracking-tight text-ink sm:text-4xl">About us</h1>

    <p className="mt-5 text-lg text-ink-soft">
      DeadOrAlive exists because most small teams find out their software is broken from an angry
      customer rather than from their own systems.
    </p>

    <section className="mt-12">
      <h2 className="text-xl font-semibold text-ink">The two blind spots</h2>

      <p className="mt-3 leading-relaxed text-ink-soft">
        The first is downtime. An SSL certificate expires at 2am on a Saturday, the API starts
        rejecting requests, the mobile app stops working — and nobody notices until Monday, by which
        point the business has lost 48 hours.
      </p>

      <p className="mt-3 leading-relaxed text-ink-soft">
        The second is quieter and usually more expensive. A nightly job syncing inventory fails on a
        Tuesday because of a database timeout. It fails silently. By Friday the store is selling
        products that are not in the warehouse, and the root cause is three days old.
      </p>
    </section>

    <section className="mt-12">
      <h2 className="text-xl font-semibold text-ink">Why the existing tools do not reach these teams</h2>

      <p className="mt-3 leading-relaxed text-ink-soft">
        Enterprise observability platforms are priced per host and per gigabyte. Cloud-native options
        assume you host on that cloud. Self-hosted stacks need a DevOps engineer to keep running. Each
        answer is reasonable for somebody — just not for a four-person team on a VPS, who end up
        monitoring nothing at all.
      </p>

      <p className="mt-3 leading-relaxed text-ink-soft">
        Teams in that position are not choosing between us and a large observability suite. They are
        choosing between us and doing nothing.
      </p>
    </section>

    <section className="mt-12">
      <h2 className="text-xl font-semibold text-ink">What makes us different</h2>

      <dl className="mt-6 grid gap-6 sm:grid-cols-2">
        {DIFFERENTIATORS.map((item) => (
          <div key={item.title} className="rounded-xl border border-line bg-surface p-5">
            <dt className="font-medium text-ink">{item.title}</dt>
            <dd className="mt-2 text-sm leading-relaxed text-ink-soft">{item.body}</dd>
          </div>
        ))}
      </dl>
    </section>
  </div>
);
