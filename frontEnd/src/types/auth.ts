/*
  Role values are string literals matching the backend's JSON, which serializes
  role names as strings. A real TS `enum` is not usable here — tsconfig has
  erasableSyntaxOnly on, and enums emit a runtime object.
*/
export const Role = {
  Admin: 'Admin',
} as const;

export type Role = (typeof Role)[keyof typeof Role];

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  roles: Role[];
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  user: User;
}

/** What is persisted between page loads. The user is re-fetched, never stored. */
export interface StoredSession {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
}

export interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  /** True while a stored session is being verified on first load. */
  isRestoringSession: boolean;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
}
