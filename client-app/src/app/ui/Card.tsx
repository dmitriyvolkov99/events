import clsx from 'clsx';
import type { HTMLAttributes } from 'react';

export default function Card({ className, ...rest }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      {...rest}
      className={clsx('rounded-2xl bg-white p-5 shadow-sm ring-1 ring-slate-200', className)}
    />
  );
}