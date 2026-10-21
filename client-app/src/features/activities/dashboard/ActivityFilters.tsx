import { observer } from 'mobx-react-lite';
import Calendar from 'react-calendar';
import { Filter } from 'lucide-react';
import clsx from 'clsx';
import { useStore } from '../../../app/stores/store';
import Card from '../../../app/ui/Card';

const filters = [
  { key: 'all', label: 'All Events' },
  { key: 'isGoing', label: "I'm going" },
  { key: 'isHost', label: "I'm hosting" },
];

const ActivityFilters = () => {
  const { activityStore: { predicate, setPredicate } } = useStore();

  return (
    <div className="space-y-4">
      <Card className="p-0">
        <div className="flex items-center gap-2 border-b border-slate-100 px-5 py-3 text-sm font-semibold text-slate-900">
          <Filter className="size-4 text-brand-600" />
          Filters
        </div>
        <ul className="p-2">
          {filters.map(({ key, label }) => (
            <li key={key}>
              <button
                onClick={() => setPredicate(key, 'true')}
                className={clsx(
                  'w-full rounded-lg px-3 py-2 text-left text-sm font-medium transition-colors',
                  predicate.has(key)
                    ? 'bg-brand-50 text-brand-700'
                    : 'text-slate-600 hover:bg-slate-50'
                )}
              >
                {label}
              </button>
            </li>
          ))}
        </ul>
      </Card>

      <Card className="p-2">
        <Calendar
          onChange={(date) => setPredicate('startDate', date as Date)}
          value={predicate.get('startDate') || new Date()}
        />
      </Card>
    </div>
  );
};

export default observer(ActivityFilters);