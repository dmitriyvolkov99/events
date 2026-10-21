import clsx from 'clsx';

export const fieldClass = (hasError: boolean) =>
  clsx(
    'block w-full rounded-lg border-0 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm',
    'ring-1 ring-inset placeholder:text-slate-400 focus:ring-2 focus:outline-none',
    hasError ? 'ring-red-400 focus:ring-red-500' : 'ring-slate-300 focus:ring-brand-500'
  );