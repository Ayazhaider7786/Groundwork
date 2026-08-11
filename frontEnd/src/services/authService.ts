import type { AuthResponse, LoginRequest, RegisterRequest, User } from '../types/auth';
import { apiClient } from './apiClient';
import { tokenStorage } from './tokenStorage';

const persist = (response: AuthResponse): AuthResponse => {
  tokenStorage.write({
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    accessTokenExpiresAt: response.accessTokenExpiresAt,
  });

  return response;
};

export const authService = {
  async register(request: RegisterRequest): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/register', request);
    return persist(data);
  },

  async login(request: LoginRequest): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', request);
    return persist(data);
  },

  async logout(): Promise<void> {
    const session = tokenStorage.read();

    try {
      if (session) {
        await apiClient.post('/auth/logout', { refreshToken: session.refreshToken });
      }
    } catch {
      // Best effort. The token may already have been rotated or revoked server
      // side, which answers 404 — not something worth showing a user who asked
      // to sign out. Clearing the local session is the outcome that matters.
    } finally {
      tokenStorage.clear();
    }
  },

  async getCurrentUser(): Promise<User> {
    const { data } = await apiClient.get<User>('/auth/me');
    return data;
  },

  hasStoredSession(): boolean {
    return tokenStorage.read() !== null;
  },
};
