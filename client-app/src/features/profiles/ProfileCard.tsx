import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { Users } from 'lucide-react';
import { Profile } from '../../app/models/profile';
import FollowButton from './FollowButton';
import SafeImage from '../../app/ui/SafeImage';

interface Props {
  profile: Profile;
}

const ProfileCard = ({ profile }: Props) => {
  return (
    <div className="overflow-hidden rounded-xl  bg-white dark:bg-zinc-900">
      <Link to={`/profiles/${profile.username}`} className="block">
      <SafeImage
        src={profile.image}
        fallback="/assets/user.png"
        alt={profile.dispayName}
        className="aspect-square w-full object-cover"
      />
        <p className="px-4 pt-3 pb-2 font-semibold text-slate-900 dark:text-zinc-100">{profile.dispayName}</p>
      </Link>
      <div className="flex items-center gap-1.5 border-t border-slate-100 dark:border-zinc-800 px-4 py-2.5 text-sm text-slate-600 dark:text-zinc-400">
        <Users className="size-4 text-slate-400" />
        {profile.followersCount} followers
      </div>
<div className="p-3 pt-0"><FollowButton profile={profile} /></div>
    </div>
  );
};

export default observer(ProfileCard);