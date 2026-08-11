import { Link } from 'react-router-dom';
import { Button } from '../ui/Button';

export const NotFoundPage = () => (
  <div className="mx-auto max-w-md px-4 py-24 text-center sm:px-6">
    <p className="text-sm font-medium text-brand">404</p>

    <h1 className="mt-2 text-3xl font-semibold tracking-tight text-ink">This page is dead</h1>

    <p className="mt-3 text-ink-soft">The rest of the site, we are happy to report, is alive.</p>

    <div className="mt-8 flex justify-center">
      <Link to="/">
        <Button variant="primary">Back to home</Button>
      </Link>
    </div>
  </div>
);
