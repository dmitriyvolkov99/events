import { observer } from 'mobx-react-lite';
import { Profile } from '../../app/models/profile';
import Card from '../../app/ui/Card';
import Avatar from '../../app/ui/Avatar';
import FollowButton from './FollowButton';

interface Props {
  profile: Profile;
}

const Stat = ({ label, value }: { label: string; value: number }) => (
  <div className="text-center">
    <div className="text-2xl font-bold text-slate-900">{value}</div>
    <div className="text-xs tracking-wide text-slate-500 uppercase">{label}</div>
  </div>
);

const ProfileHeader = ({ profile }: Props) => {
  return (
    <Card className="grid gap-6 md:grid-cols-3 md:items-center">
      <div className="flex items-center gap-5 md:col-span-2">
        <Avatar src={profile.image} alt={profile.dispayName} className="size-20 sm:size-24" />
        <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-900">{profile.dispayName}</h1>
      </div>

      <div className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <Stat label="Followers" value={profile.followersCount} />
          <Stat label="Following" value={profile.followingCount} />
        </div>
        <div className="border-t border-slate-100 pt-4">
          <FollowButton profile={profile} />
        </div>
      </div>
    </Card>
  );
};

export default observer(ProfileHeader);