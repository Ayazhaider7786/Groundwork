import { Link, NavLink } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { Button } from '../ui/Button';

const NAV_LINKS = [
  { to: '/', label: 'Home', end: true },
  { to: '/about', label: 'About us', end: false },
  { to: '/contact', label: 'Contact us', end: false },
];

const linkClasses = (isActive: boolean) =>
  `rounded-md px-3 py-2 text-sm font-medium transition-colors ${
    isActive ? 'bg-brand-soft text-brand' : 'text-ink-soft hover:bg-surface-raised hover:text-ink'
  }`;

export const Navbar = () => {
  const { user, isAuthenticated, isRestoringSession } = useAuth();

  return (
    <header className="border-b border-line bg-surface">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-x-6 gap-y-3 px-4 py-3 sm:px-6">
        <Link to="/" className="flex items-center gap-2 text-lg font-semibold text-ink">
          <span className="size-2.5 rounded-full bg-success" aria-hidden="true" />
          DeadOrAlive
        </Link>

        <nav className="order-3 flex w-full items-center gap-1 sm:order-none sm:w-auto">
          {NAV_LINKS.map((link) => (
            <NavLink key={link.to} to={link.to} end={link.end} className={({ isActive }) => linkClasses(isActive)}>
              {link.label}
            </NavLink>
          ))}
        </nav>

        <div className="ml-auto flex items-center gap-2">
          {isRestoringSession ? (
            <span className="text-sm text-muted">Checking session…</span>
          ) : isAuthenticated ? (
            <>
              <span className="hidden text-sm text-ink-soft sm:inline">{user?.fullName}</span>
              <Link to="/logout">
                <Button variant="secondary">Log out</Button>
              </Link>
            </>
          ) : (
            <>
              <Link to="/login">
                <Button variant="ghost">Log in</Button>
              </Link>
              <Link to="/signup">
                <Button variant="primary">Sign up</Button>
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
};
