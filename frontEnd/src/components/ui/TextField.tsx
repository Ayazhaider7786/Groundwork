import type { TextFieldProps } from '../../types/ui';

export const TextField = ({ label, name, error, className = '', ...rest }: TextFieldProps) => (
  <div className="flex flex-col gap-1.5">
    <label htmlFor={name} className="text-sm font-medium text-ink-soft">
      {label}
    </label>

    <input
      id={name}
      name={name}
      aria-invalid={error !== undefined}
      {...rest}
      className={`rounded-lg border bg-surface px-3 py-2.5 text-sm text-ink outline-none transition-colors placeholder:text-muted focus:border-brand ${
        error ? 'border-danger' : 'border-line'
      } ${className}`}
    />

    {error ? <span className="text-xs text-danger">{error}</span> : null}
  </div>
);
