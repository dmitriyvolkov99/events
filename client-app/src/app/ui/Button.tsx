import { cn } from './cn';
import type { ButtonHTMLAttributes } from 'react';
import Spinner from './Spinner';

type Variant = 'primary' | 'secondary' | 'ghost' | 'danger';

const variants: Record<Variant, string> = {
  primary: 'bg-brand-600 text-white hover:bg-brand-700',
  secondary: 'bg-white dark:bg-zinc-900 text-slate-700 dark:text-zinc-300 ring-1 ring-slate-300 dark:ring-zinc-700 hover:bg-slate-50 dark:hover:bg-zinc-800',
  ghost: 'text-slate-600 dark:text-zinc-400 hover:bg-slate-100 dark:hover:bg-zinc-800',
  danger: 'bg-red-600 text-white hover:bg-red-700',
};

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  loading?: boolean;
}

export default function Button({ variant = 'primary', loading, className, children, disabled, ...rest }: Props) {
  return (
    <button
      {...rest}
      disabled={disabled || loading}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-medium transition-colors',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500',
        'disabled:cursor-not-allowed disabled:opacity-50',
        variants[variant],
        className
      )}
    >
      {loading && <Spinner className="size-4" />}
      {children}
    </button>
  );
}