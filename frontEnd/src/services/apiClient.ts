import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type { AuthResponse } from '../types/auth';
import { tokenStorage } from './tokenStorage';

/** Unset in development, where Vite proxies /api to the backend. */
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use((config) => {
  const session = tokenStorage.read();

  if (session) {
    config.headers.Authorization = `Bearer ${session.accessToken}`;
  }

  return config;
});

// One refresh at a time: a page that fires several requests at once must not
// spend every stored refresh token, since redeeming one revokes it.
let refreshInFlight: Promise<string | null> | null = null;

const refreshSession = async (): Promise<string | null> => {
  const session = tokenStorage.read();

  if (!session) {
    return null;
  }

  try {
    // Bare axios, not apiClient — this call must not re-enter the interceptor.
    const { data } = await axios.post<AuthResponse>(`${API_BASE_URL}/auth/refresh`, {
      refreshToken: session.refreshToken,
    });

    tokenStorage.write({
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      accessTokenExpiresAt: data.accessTokenExpiresAt,
    });

    return data.accessToken;
  } catch {
    tokenStorage.clear();
    return null;
  }
};

type RetriableRequest = InternalAxiosRequestConfig & { hasRetried?: boolean };

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const request = error.config as RetriableRequest | undefined;

    if (error.response?.status !== 401 || !request || request.hasRetried) {
      return Promise.reject(error);
    }

    request.hasRetried = true;

    if (!refreshInFlight) {
      refreshInFlight = refreshSession().finally(() => {
        refreshInFlight = null;
      });
    }

    const accessToken = await refreshInFlight;

    if (!accessToken) {
      return Promise.reject(error);
    }

    request.headers.Authorization = `Bearer ${accessToken}`;
    return apiClient(request);
  },
);
