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
      <TabList className="mb-4 flex gap-1 overflow-x-auto rounded-xl bg-slate-100 p-1">
        {tabs.map((tab) => (
          <Tab
            key={tab}
            className="rounded-lg px-4 py-2 text-sm font-medium whitespace-nowrap text-slate-600 focus:outline-none data-focus:outline-2 data-focus:outline-brand-500 data-hover:text-slate-900 data-selected:bg-white data-selected:text-brand-700 data-selected:shadow-sm"
          >
            {tab}
          </Tab>
        ))}
      </TabList>

      <TabPanels>
        <TabPanel>
          <Card>About content</Card>
        </TabPanel>
        <TabPanel>
          <ProfilePhotos profile={profile} />
        </TabPanel>
        <TabPanel>
          <ProfileActivities />
        </TabPanel>
        <TabPanel>
          <ProfileFollowings />
        </TabPanel>
        <TabPanel>
          <ProfileFollowings />
        </TabPanel>
      </TabPanels>
    </TabGroup>
  );
};

export default observer(ProfileContent);