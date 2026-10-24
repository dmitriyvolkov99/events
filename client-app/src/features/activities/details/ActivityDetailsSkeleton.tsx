import Card from '../../../app/ui/Card';
import Skeleton from '../../../app/ui/Skeleton';

export default function ActivityDetailsSkeleton() {
  return (
    <div className="grid gap-8 lg:grid-cols-3" role="status" aria-label="Loading event">
      <div className="space-y-6 lg:col-span-2">
        <Card className="overflow-hidden p-0">
          <Skeleton className="h-48 rounded-none sm:h-64" />
          <div className="border-t border-slate-100 px-6 py-4 dark:border-zinc-800">
            <Skeleton className="h-9 w-36 rounded-lg" />
          </div>
        </Card>

        <Card className="divide-y divide-slate-100 p-0 dark:divide-zinc-800">
          {['w-full', 'w-1/2', 'w-2/3'].map((width) => (
            <div key={width} className="flex items-center gap-4 px-6 py-4">
              <Skeleton className="size-5 shrink-0" />
              <Skeleton className={`h-4 ${width}`} />
            </div>
          ))}
        </Card>

        <Card className="space-y-4">
          <Skeleton className="h-5 w-40" />
          <Skeleton className="h-16 w-full" />
        </Card>
      </div>

      <Card className="space-y-4">
        <Skeleton className="h-5 w-32" />
        {[0, 1, 2].map((i) => (
          <div key={i} className="flex items-center gap-3">
            <Skeleton className="size-12 shrink-0 rounded-full" />
            <Skeleton className="h-4 w-32" />
          </div>
        ))}
      </Card>
    </div>
  );
}