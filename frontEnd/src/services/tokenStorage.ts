import type { StoredSession } from '../types/auth';

const SESSION_KEY = 'deadoralive.session';

/**
 * Owns the persisted session. Kept apart from authService so apiClient can read
 * the token without importing the service that calls apiClient.
 */
export const tokenStorage = {
  read(): StoredSession | null {
    const raw = window.localStorage.getItem(SESSION_KEY);

    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as StoredSession;
    } catch {
      // A corrupt entry is worse than none — drop it rather than crash on boot.
      window.localStorage.removeItem(SESSION_KEY);
      return null;
    }
  },

  write(session: StoredSession): void {
    window.localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  },

  clear(): void {
    window.localStorage.removeItem(SESSION_KEY);
  },
};
