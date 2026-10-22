import { observer } from 'mobx-react-lite';
import { useEffect, useState } from 'react';
import { PagingParams } from '../../../app/models/paginations';
import { useStore } from '../../../app/stores/store';
import { useInfiniteScroll } from '../../../app/common/hooks/useInfiniteScroll';
import Spinner from '../../../app/ui/Spinner';
import ActivityFilters from './ActivityFilters';
import ActivityList from './ActivityList';
import ActivityListItemPlaceholder from './ActivityListItemPlaceholder';

const ActivityDashboard = () => {
  const { activityStore } = useStore();
  const { loadActivities, activityRegistry, setPagingParams, pagination } = activityStore;

  const [loadingNext, setLoadingNext] = useState(false);

  function handleNext() {
    setLoadingNext(true);
    setPagingParams(new PagingParams(pagination!.currentPage + 1));
    loadActivities().then(() => setLoadingNext(false));
  }

  useEffect(() => {
    if (activityRegistry.size <= 1) loadActivities();
  }, [activityRegistry.size, activityStore, loadActivities]);

  const hasMore = !loadingNext && !!pagination && pagination.currentPage < pagination.totalPages;
  const sentinelRef = useInfiniteScroll(handleNext, hasMore);

  return (
    <div className="grid gap-8 lg:grid-cols-3">
      <section className="lg:col-span-2">
        {activityStore.loadingInitial && !loadingNext ? (
          <div className="space-y-4">
            <ActivityListItemPlaceholder />
            <ActivityListItemPlaceholder />
          </div>
        ) : (
          <>
            <ActivityList />
            <div ref={sentinelRef} className="h-px" />
          </>
        )}

        {loadingNext && (
          <div className="flex justify-center py-6 text-brand-600">
            <Spinner className="size-6" />
          </div>
        )}
      </section>

      <aside className="order-first lg:order-none lg:sticky lg:top-24 lg:self-start">
        <ActivityFilters />
      </aside>
    </div>
  );
};

export default observer(ActivityDashboard);