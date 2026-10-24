import { useState } from 'react';
import { CalendarDays } from 'lucide-react';
import { cn } from './cn';

interface Props {
  category: string;
  className?: string;
  priority?: boolean;
}

export default function CategoryImage({ category, className, priority }: Props) {
  const [failedCategory, setFailedCategory] = useState<string | null>(null);

  if (failedCategory === category) {
    return (
      <div
        role="img"
        aria-label={category}
        className={cn(
          'flex items-center justify-center bg-gradient-to-br from-brand-500 to-brand-700 text-white/80',
          className
        )}
      >
        <CalendarDays className="size-10" />
      </div>
    );
  }

  return (
    <img
      src={`/assets/categoryImages/${category}.jpg`}
      alt={category}
      loading={priority ? 'eager' : 'lazy'}
      decoding="async"
      onError={() => setFailedCategory(category)}
      className={cn('object-cover', className)}
    />
  );
}