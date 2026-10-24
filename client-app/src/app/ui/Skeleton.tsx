import { cn } from './cn';

export default function Skeleton({ className }: { className?: string }) {
  return (
    <div
      aria-hidden="true"
      className={cn('animate-pulse rounded-md bg-slate-200 motion-reduce:animate-none dark:bg-zinc-700', className)}
    />
  );
}