import Card from '../../app/ui/Card';
import Skeleton from '../../app/ui/Skeleton';

export default function ProfilePageSkeleton() {
  return (
    <div className="space-y-6" role="status" aria-label="Loading profile">
      <Card className="grid gap-6 md:grid-cols-3 md:items-center">
        <div className="flex items-center gap-5 md:col-span-2">
          <Skeleton className="size-20 rounded-full sm:size-24" />
          <Skeleton className="h-8 w-48" />
        </div>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Skeleton className="h-12" />
            <Skeleton className="h-12" />
          </div>
          <Skeleton className="h-9 w-full rounded-lg" />
        </div>
      </Card>

      <Skeleton className="h-11 w-full rounded-xl" />

      <Card className="space-y-4">
        <Skeleton className="h-5 w-32" />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} className="aspect-square rounded-xl" />
          ))}
        </div>
      </Card>
    </div>
  );
}