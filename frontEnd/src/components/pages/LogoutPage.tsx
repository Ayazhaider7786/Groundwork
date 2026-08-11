import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

export const LogoutPage = () => {
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const signOut = async () => {
      await logout();
      navigate('/', { replace: true });
    };

    void signOut();
  }, [logout, navigate]);

  return (
    <div className="mx-auto max-w-md px-4 py-24 text-center sm:px-6">
      <p className="text-ink-soft">Signing you out…</p>
    </div>
  );
};
