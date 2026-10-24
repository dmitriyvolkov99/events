import { cn } from '../../ui/cn';

export const fieldClass = (hasError: boolean) =>
  cn(
    'block w-full rounded-lg border-0 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm dark:bg-zinc-900 dark:text-zinc-100',
    'ring-1 ring-inset placeholder:text-slate-400 focus:ring-2 focus:outline-none dark:placeholder:text-zinc-500',
    hasError ? 'ring-red-400 focus:ring-red-500' : 'ring-slate-300 focus:ring-brand-500 dark:ring-zinc-700'
  );