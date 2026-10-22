import { observer } from 'mobx-react-lite';
import Calendar from 'react-calendar';
import { CalendarDays, Filter } from 'lucide-react';
import { cn } from '../../../app/ui/cn';
import { useStore } from '../../../app/stores/store';
import Card from '../../../app/ui/Card';
import type { ActivityFilter } from '../../../app/stores/activityStore';

const filters: { key: ActivityFilter; label: string }[] = [
  { key: 'all', label: 'All Events' },
  { key: 'isGoing', label: "I'm going" },
  { key: 'isHost', label: "I'm hosting" },
];

const ActivityFilters = () => {
  const { activityStore: { filter, startDate, setFilter, setStartDate } } = useStore();

  const calendar = (
    <Calendar
      onChange={(value) => {
        if (value instanceof Date) setStartDate(value);
      }}
      value={startDate ?? new Date()}
    />
  );

  return (
    <div className="space-y-4">
      <Card className="p-0">
        <div className="hidden items-center gap-2 border-b border-slate-100 dark:border-zinc-800 px-5 py-3 text-sm font-semibold text-slate-900 dark:text-zinc-100 lg:flex">
          <Filter className="size-4 text-brand-600" />
          Filters
        </div>
        <ul className="flex gap-2 overflow-x-auto p-2 lg:flex-col lg:gap-0 lg:overflow-visible">
          {filters.map(({ key, label }) => (
            <li key={key} className="shrink-0 lg:shrink">
              <button
                onClick={() => setFilter(key)}
                className={cn(
                  'w-full rounded-lg px-3 py-2 text-left text-sm font-medium whitespace-nowrap transition-colors',
                  filter === key ? 'bg-brand-50 dark:bg-brand-500/10 text-brand-700 dark:text-brand-300' : 'text-slate-600 dark:text-zinc-400 hover:bg-slate-50 dark:hover:bg-zinc-800'
                )}
              >
                {label}
              </button>
            </li>
          ))}
        </ul>
      </Card>

      {/* телефон: календарь под спойлером */}
      <details className="rounded-2xl  bg-white dark:bg-zinc-900 shadow-sm ring-1 ring-slate-200 dark:ring-zinc-800 lg:hidden">
        <summary className="flex cursor-pointer items-center gap-2 px-5 py-3 text-sm font-medium text-slate-700 dark:text-zinc-300">
          <CalendarDays className="size-4 text-brand-600" />
          Pick a date
        </summary>
        <div className="p-2">{calendar}</div>
      </details>

      {/* десктоп: календарь всегда виден */}
      <Card className="hidden p-2 lg:block">{calendar}</Card>
    </div>
  );
};

export default observer(ActivityFilters);