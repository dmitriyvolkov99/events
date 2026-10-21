import { observer } from 'mobx-react-lite';
import type { SyntheticEvent } from 'react';
import clsx from 'clsx';
import { Profile } from '../../app/models/profile';
import { useStore } from '../../app/stores/store';
import Spinner from '../../app/ui/Spinner';

interface Props {
  profile: Profile;
}

const FollowButton = ({ profile }: Props) => {
  const { profileStore, userStore } = useStore();
  const { updateFollowing, loading } = profileStore;

  if (userStore.user?.username === profile.username) return null;

  function handleFollow(e: SyntheticEvent, username: string) {
    e.preventDefault();
    updateFollowing(username, !profile.following);
  }

  return (
    <button
      type="button"
      disabled={loading}
      onClick={(e) => handleFollow(e, profile.username)}
      className={clsx(
        'group inline-flex w-full items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-medium text-white transition-colors disabled:opacity-60',
        profile.following ? 'bg-brand-600 hover:bg-red-600' : 'bg-green-600 hover:bg-green-700'
      )}
    >
      {loading && <Spinner className="size-4" />}
      {profile.following ? (
        <>
          <span className="group-hover:hidden">Following</span>
          <span className="hidden group-hover:inline">Unfollow</span>
        </>
      ) : (
        'Follow'
      )}
    </button>
  );
};

export default observer(FollowButton);