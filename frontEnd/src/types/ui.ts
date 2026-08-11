import type { ButtonHTMLAttributes, InputHTMLAttributes, ReactNode } from 'react';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  isLoading?: boolean;
}

export interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  name: string;
  error?: string;
}

export interface TextAreaFieldProps {
  label: string;
  name: string;
  value: string;
  rows?: number;
  error?: string;
  onChange: (value: string) => void;
}

export type AlertTone = 'danger' | 'success' | 'warning';

export interface AlertProps {
  tone: AlertTone;
  children: ReactNode;
}
