import type { ReactNode } from 'react';

export default function Alert({ children }: { children: ReactNode }) {
  return (
    <div role="alert" className="mb-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700 ring-1 ring-red-200 ring-inset">
      {children}
    </div>
  );
}