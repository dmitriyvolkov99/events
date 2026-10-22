import type { ReactNode } from 'react';

interface Props {
  id: string;
  label?: string;
  error?: string;
  children: ReactNode;
}

export default function FormField({ id, label, error, children }: Props) {
  return (
    <div className="mb-4">
      {label && (
        <label htmlFor={id} className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-zinc-300">
          {label}
        </label>
      )}
      {children}
      {error && <p className="mt-1.5 text-sm text-red-600">{error}</p>}
    </div>
  );
}