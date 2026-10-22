import { format } from 'date-fns';
import { observer } from 'mobx-react-lite';
import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Tab, TabGroup, TabList } from '@headlessui/react';
import { Calendar } from 'lucide-react';
import { UserActivity } from '../../app/models/profile';
import { useStore } from '../../app/stores/store';
import Panel from '../../app/ui/Panel';

const panes = [
  { menuItem: 'Future Events', key: 'future' },
  { menuItem: 'Past Events', key: 'past' },
  { menuItem: 'Hosting Events', key: 'hosting' },
];

const ProfileActivities = () => {
  const { profileStore } = useStore();
  const { loadUserActivities, profile, loadingActivities, userActivities } = profileStore;

  useEffect(() => {
    loadUserActivities(profile!.username);
  }, [loadUserActivities, profile]);

  return (
    <Panel loading={loadingActivities}>
      <h3 className="mb-4 flex items-center gap-2 text-lg font-semibold text-slate-900 dark:text-zinc-100">
        <Calendar className="size-5 text-brand-600" /> Activities
      </h3>

      <TabGroup onChange={(index) => loadUserActivities(profile!.username, panes[index].key)}>
        <TabList className="mb-5 flex gap-6 border-b border-slate-200 dark:border-zinc-700">
          {panes.map((pane) => (
            <Tab
              key={pane.key}
              className="-mb-px border-b-2 border-transparent pb-3 text-sm font-medium text-slate-500 dark:text-zinc-400 focus:outline-none data-focus:text-brand-700 dark:data-focus:text-brand-300 data-hover:text-slate-700 dark:data-hover:text-zinc-300 data-selected:border-brand-600 data-selected:text-brand-700 dark:data-selected:text-brand-300"
            >
              {pane.menuItem}
            </Tab>
          ))}
        </TabList>
      </TabGroup>

      {userActivities.length === 0 && !loadingActivities ? (
        <p className="text-sm text-slate-500 dark:text-zinc-400">Nothing here yet.</p>
      ) : (
        <ul className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {userActivities.map((activity: UserActivity) => (
            <li key={activity.id}>
              <Link
                to={`/activities/${activity.id}`}
                className="block overflow-hidden rounded-xl ring-1 ring-slate-200 dark:ring-zinc-800 transition-shadow hover:shadow-md"
              >
                <img
                  src={`/assets/categoryImages/${activity.category}.jpg`}
                  alt={activity.category}
                  className="h-28 w-full object-cover"
                />
                <div className="p-3 text-center">
                  <p className="truncate font-semibold text-slate-900 dark:text-zinc-100">{activity.title}</p>
                  <p className="mt-1 text-xs text-slate-500 dark:text-zinc-400">{format(new Date(activity.date), 'do LLL')}</p>
                  <p className="text-xs text-slate-500 dark:text-zinc-400">{format(new Date(activity.date), 'h:mm a')}</p>
                </div>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </Panel>
  );
};

export default observer(ProfileActivities);