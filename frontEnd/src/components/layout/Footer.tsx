import { Link } from 'react-router-dom';

export const Footer = () => (
  <footer className="border-t border-line bg-surface">
    <div className="mx-auto flex max-w-6xl flex-col gap-3 px-4 py-6 text-sm text-muted sm:flex-row sm:items-center sm:justify-between sm:px-6">
      <p>DeadOrAlive — uptime and heartbeat monitoring.</p>

      <div className="flex gap-4">
        <Link to="/about" className="transition-colors hover:text-ink">
          About us
        </Link>
        <Link to="/contact" className="transition-colors hover:text-ink">
          Contact us
        </Link>
      </div>
    </div>
  </footer>
);
