import { observer } from 'mobx-react-lite';
import { Users } from 'lucide-react';
import { useStore } from '../../app/stores/store';
import Panel from '../../app/ui/Panel';
import ProfileCard from './ProfileCard';

const ProfileFollowings = () => {
  const { profileStore } = useStore();
  const { profile, followings, loadingFollowings, activeTab } = profileStore;

  return (
    <Panel loading={loadingFollowings}>
      <h3 className="mb-5 flex items-center gap-2 text-lg font-semibold text-slate-900">
        <Users className="size-5 text-brand-600" />
        {activeTab === 3
          ? `People following ${profile?.dispayName}`
          : `People ${profile?.dispayName} is following`}
      </h3>

      {followings.length === 0 && !loadingFollowings ? (
        <p className="text-sm text-slate-500">Nobody here yet.</p>
      ) : (
        <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {followings.map((p) => (
            <li key={p.username} className="rounded-xl shadow-sm ring-1 ring-slate-200">
              <ProfileCard profile={p} />
            </li>
          ))}
        </ul>
      )}
    </Panel>
  );
};

export default observer(ProfileFollowings);