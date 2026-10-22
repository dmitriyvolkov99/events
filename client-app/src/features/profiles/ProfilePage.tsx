import { observer } from 'mobx-react-lite';
import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import LoadingComponent from '../../app/layout/LoadingComponent';
import { useStore } from '../../app/stores/store';
import ProfileContent from './ProfileContent';
import ProfileHeader from './ProfileHeader';

const ProfilePage = () => {
  const { username } = useParams<{ username: string }>();
  const { profileStore } = useStore();
  const { loadProfile, loadingProfile, profile, setActiveTab } = profileStore;

  useEffect(() => {
    loadProfile(username!);
    return () => setActiveTab(0);
  }, [loadProfile, username, setActiveTab]);

  if (loadingProfile) return <LoadingComponent content="Loading profile" />;

  return (
    <div className="space-y-6">
      {profile && (
        <>
          <title>{`${profile.dispayName} - Events`}</title>
          <ProfileHeader profile={profile} />
          <ProfileContent profile={profile} />
        </>
      )}
    </div>
  );
};

export default observer(ProfilePage);