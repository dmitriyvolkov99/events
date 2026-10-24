import { observer } from 'mobx-react-lite';
import { useEffect } from 'react';
import { useParams } from 'react-router';
import { useStore } from '../../../app/stores/store';
import ActivityDetailedChat from './ActivityDetailedChat';
import ActivityDetailedHeader from './ActivityDetailedHeader';
import ActivityDetailedInfo from './ActivityDetailedInfo';
import ActivityDetailedSidebar from './ActivityDetailedSidebar';
import ActivityDetailsSkeleton from './ActivityDetailsSkeleton';

const ActivityDetails = () => {
  const { activityStore } = useStore();
  const { selectedActivity: activity, loadActivity, loadingInitial, clearSelectedActivity } = activityStore;

  const { id } = useParams<{ id: string }>();

  useEffect(() => {
    if (id) loadActivity(id);
    return () => clearSelectedActivity();
  }, [id, loadActivity, clearSelectedActivity]);

  if (loadingInitial || !activity) return <ActivityDetailsSkeleton />;

  return (
    <div className="grid gap-8 lg:grid-cols-3">
      <title>{`${activity.title} - Events`}</title>
      <div className="space-y-6 lg:col-span-2">
        <ActivityDetailedHeader activity={activity} />
        <ActivityDetailedInfo activity={activity} />
        <ActivityDetailedChat activityId={activity.id} />
      </div>
      <aside className="lg:sticky lg:top-24 lg:self-start">
        <ActivityDetailedSidebar activity={activity} />
      </aside>
    </div>
  );
};

export default observer(ActivityDetails);