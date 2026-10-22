import { observer } from 'mobx-react-lite';
import { Fragment } from 'react';
import { useStore } from '../../../app/stores/store';
import ActivityListItem from './ActivityListItem';
import Card from '../../../app/ui/Card';
import { CalendarX } from 'lucide-react';

const ActivityList = () => {
  const { activityStore } = useStore();
  const { groupedActivities } = activityStore;

  if (groupedActivities.length === 0) {
    return (
      <Card className="py-12 text-center">
        <CalendarX className="mx-auto size-10 text-slate-400 dark:text-zinc-500" />
        <p className="mt-3 text-sm text-slate-600 dark:text-zinc-400">No events found for these filters.</p>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {groupedActivities.map(([group, activities]) => (
        <Fragment key={group}>
          <h3 className="pt-4 text-xs font-semibold tracking-wider text-brand-600 uppercase first:pt-0">
            {group}
          </h3>
          {activities.map((activity) => (
            <ActivityListItem key={activity.id} activity={activity} />
          ))}
        </Fragment>
      ))}
    </div>
  );
};

export default observer(ActivityList);