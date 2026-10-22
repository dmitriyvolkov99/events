import { cn } from './cn';
import type { HTMLAttributes } from 'react';

export default function Card({ className, ...rest }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      {...rest}
      className={cn('rounded-2xl  bg-white dark:bg-zinc-900 p-5 shadow-sm ring-1 ring-slate-200 dark:ring-zinc-800', className)}
    />
  );
}