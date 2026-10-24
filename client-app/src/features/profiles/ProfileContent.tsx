import { observer } from 'mobx-react-lite';
import { Tab, TabGroup, TabList, TabPanel, TabPanels } from '@headlessui/react';
import { Profile } from '../../app/models/profile';
import { useStore } from '../../app/stores/store';
import Card from '../../app/ui/Card';
import ProfileActivities from './ProfileActivities';
import ProfileFollowings from './ProfileFollowings';
import ProfilePhotos from './ProfilePhotos';

interface Props {
  profile: Profile;
}

const tabs = ['About', 'Photos', 'Events', 'Followers', 'Following'];

const ProfileContent = ({ profile }: Props) => {
  const { profileStore } = useStore();

  return (
    <TabGroup key={profile.username} onChange={(index) => profileStore.setActiveTab(index)}>
      <TabList className="mb-4 flex gap-1 overflow-x-auto rounded-xl bg-slate-100 dark:bg-zinc-800 p-1">
        {tabs.map((tab) => (
          <Tab
            key={tab}
            className="rounded-lg px-4 py-2 text-sm font-medium whitespace-nowrap text-slate-600 dark:text-zinc-400 focus:outline-none data-focus:outline-2 data-focus:outline-brand-500 data-hover:text-slate-900 dark:data-hover:text-zinc-100 data-selected:bg-white dark:data-selected:bg-zinc-700 data-selected:text-brand-700 dark:data-selected:text-brand-300 data-selected:shadow-sm"
          >
            {tab}
          </Tab>
        ))}
      </TabList>

      <TabPanels>
        <TabPanel className="animate-fade-in motion-reduce:animate-none">
          <Card>About content</Card>
        </TabPanel>
        <TabPanel className="animate-fade-in motion-reduce:animate-none">
          <ProfilePhotos profile={profile} />
        </TabPanel>
        <TabPanel className="animate-fade-in motion-reduce:animate-none">
          <ProfileActivities />
        </TabPanel>
        <TabPanel className="animate-fade-in motion-reduce:animate-none">
          <ProfileFollowings />
        </TabPanel>
        <TabPanel className="animate-fade-in motion-reduce:animate-none">
          <ProfileFollowings />
        </TabPanel>
      </TabPanels>
    </TabGroup>
  );
};

export default observer(ProfileContent);