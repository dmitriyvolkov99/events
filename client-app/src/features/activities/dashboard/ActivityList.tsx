import { observer } from 'mobx-react-lite';
import { Fragment } from 'react';
import { useStore } from '../../../app/stores/store';
import ActivityListItem from './ActivityListItem';

const ActivityList = () => {
  const { activityStore } = useStore();
  const { groupedActivities } = activityStore;

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