import type { ReactNode } from 'react';
import Card from './Card';
import Spinner from './Spinner';

interface Props {
  loading?: boolean;
  children: ReactNode;
}

export default function Panel({ loading, children }: Props) {
  return (
    <Card className="relative">
      {loading && (
        <div className="absolute inset-0 z-10 flex items-center justify-center rounded-2xl bg-white/70">
          <Spinner className="size-8 text-brand-600" />
        </div>
      )}
      {children}
    </Card>
  );
}