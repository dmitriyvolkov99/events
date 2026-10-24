import type { ReactNode } from 'react';

interface Props {
  id: string;
  label?: string;
  hint?: string;
  error?: string;
  children: ReactNode;
}

export default function FormField({ id, label, hint, error, children }: Props) {
  return (
    <div className="mb-4">
      {label && (
        <label htmlFor={id} className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-zinc-300">
          {label}
        </label>
      )}
      {children}
      {hint && !error && (
        <p id={`${id}-hint`} className="mt-1.5 text-xs text-slate-500 dark:text-zinc-400">
          {hint}
        </p>
      )}
      {error && (
        <p id={`${id}-error`} className="mt-1.5 text-sm text-red-600 dark:text-red-400">
          {error}
        </p>
      )}
    </div>
  );
}