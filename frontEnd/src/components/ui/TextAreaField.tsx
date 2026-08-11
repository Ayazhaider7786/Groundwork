import type { TextAreaFieldProps } from '../../types/ui';

export const TextAreaField = ({ label, name, value, rows = 5, error, onChange }: TextAreaFieldProps) => (
  <div className="flex flex-col gap-1.5">
    <label htmlFor={name} className="text-sm font-medium text-ink-soft">
      {label}
    </label>

    <textarea
      id={name}
      name={name}
      rows={rows}
      value={value}
      aria-invalid={error !== undefined}
      onChange={(event) => onChange(event.target.value)}
      className={`resize-y rounded-lg border bg-surface px-3 py-2.5 text-sm text-ink outline-none transition-colors placeholder:text-muted focus:border-brand ${
        error ? 'border-danger' : 'border-line'
      }`}
    />

    {error ? <span className="text-xs text-danger">{error}</span> : null}
  </div>
);
