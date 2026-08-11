import type { AlertProps, AlertTone } from '../../types/ui';

const TONE_CLASSES: Record<AlertTone, string> = {
  danger: 'bg-danger-soft text-danger',
  success: 'bg-success-soft text-success',
  warning: 'bg-warning-soft text-warning',
};

export const Alert = ({ tone, children }: AlertProps) => (
  <div role="alert" className={`rounded-lg px-4 py-3 text-sm ${TONE_CLASSES[tone]}`}>
    {children}
  </div>
);
