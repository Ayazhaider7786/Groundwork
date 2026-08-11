import { AxiosError } from 'axios';
import type { ApiErrorResponse } from '../types/api';

/**
 * Turns any caught error into one message fit to show a user.
 *
 * @param error    the caught error
 * @param fallback shown for network failures and 5xx, where the server's own
 *                 message is either absent or not safe to surface
 */
export const extractApiError = (error: unknown, fallback: string): string => {
  if (!(error instanceof AxiosError) || !error.response) {
    return fallback;
  }

  const data = error.response.data as ApiErrorResponse | undefined;

  if (!data) {
    return fallback;
  }

  // Identity returns a flat list; several errors can arrive at once.
  if (Array.isArray(data.errors) && data.errors.length > 0) {
    return data.errors.join(' ');
  }

  // FluentValidation returns a field-keyed map.
  if (data.errors && typeof data.errors === 'object') {
    const messages = Object.values(data.errors as Record<string, string[]>).flat();

    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  if (data.detail) {
    return data.detail;
  }

  if (data.title) {
    return data.title;
  }

  return fallback;
};
