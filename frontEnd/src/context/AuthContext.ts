import { createContext } from 'react';
import type { AuthContextValue } from '../types/auth';

// Context object only — kept free of JSX so the provider file stays a pure
// component module and fast refresh works cleanly.
export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
