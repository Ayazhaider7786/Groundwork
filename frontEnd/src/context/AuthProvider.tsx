import { useCallback, useEffect, useMemo, useState, type PropsWithChildren } from 'react';
import { authService } from '../services/authService';
import type { LoginRequest, RegisterRequest, User } from '../types/auth';
import { AuthContext } from './AuthContext';

export const AuthProvider = ({ children }: PropsWithChildren) => {
  const [user, setUser] = useState<User | null>(null);
  const [isRestoringSession, setIsRestoringSession] = useState(authService.hasStoredSession());

  // A stored token proves nothing on its own — it may be expired or revoked, so
  // the session is confirmed by asking the API who we are.
  useEffect(() => {
    if (!authService.hasStoredSession()) {
      return;
    }

    let isCancelled = false;

    const restoreSession = async () => {
      try {
        const currentUser = await authService.getCurrentUser();

        if (!isCancelled) {
          setUser(currentUser);
        }
      } catch {
        if (!isCancelled) {
          setUser(null);
        }
      } finally {
        if (!isCancelled) {
          setIsRestoringSession(false);
        }
      }
    };

    void restoreSession();

    return () => {
      isCancelled = true;
    };
  }, []);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await authService.login(request);
    setUser(response.user);
  }, []);

  const register = useCallback(async (request: RegisterRequest) => {
    const response = await authService.register(request);
    setUser(response.user);
  }, []);

  const logout = useCallback(async () => {
    await authService.logout();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: user !== null,
      isRestoringSession,
      login,
      register,
      logout,
    }),
    [user, isRestoringSession, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
