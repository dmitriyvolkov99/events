import clsx from 'clsx';
import type { ReactNode } from 'react';

type Tone = 'red' | 'orange' | 'green' | 'slate';

const tones: Record<Tone, string> = {
  red: 'bg-red-50 text-red-700 ring-red-200',
  orange: 'bg-orange-50 text-orange-700 ring-orange-200',
  green: 'bg-green-50 text-green-700 ring-green-200',
  slate: 'bg-slate-100 text-slate-700 ring-slate-200 dark:bg-zinc-800 dark:text-zinc-300 dark:ring-zinc-700',
};

export default function Badge({ tone = 'slate', children }: { tone?: Tone; children: ReactNode }) {
  return (
    <span className={clsx('inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset', tones[tone])}>
      {children}
    </span>
  );
}