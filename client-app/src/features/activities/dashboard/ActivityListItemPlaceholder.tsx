import Card from '../../../app/ui/Card';

const ActivityListItemPlaceholder = () => (
  <Card className="animate-pulse p-0" aria-hidden="true">
    <div className="flex gap-4 p-5">
      <div className="size-14 rounded-full bg-slate-200" />
      <div className="flex-1 space-y-2.5 pt-1">
        <div className="h-4 w-1/2 rounded bg-slate-200" />
        <div className="h-3 w-1/3 rounded bg-slate-200" />
      </div>
    </div>
    <div className="border-t border-slate-100 px-5 py-3">
      <div className="h-3 w-2/3 rounded bg-slate-200" />
    </div>
    <div className="flex items-center justify-between border-t border-slate-100 px-5 py-4">
      <div className="h-3 w-1/2 rounded bg-slate-200" />
      <div className="h-8 w-16 rounded-lg bg-slate-200" />
    </div>
  </Card>
);

export default ActivityListItemPlaceholder;