import { makeAutoObservable, reaction, runInAction } from "mobx";
import agent from "../api/agent";
import { Photo, Profile, UserActivity } from "../models/profile";
import { store } from "./store";

export default class ProfileStore{
    profile: Profile | null = null;
    loadingProfile = false;
    uploading = false;
    loading = false;
    followings: Profile[] = [];
    loadingFollowings = false;
    activeTab = 0;
    userActivities: UserActivity[] = [];
    loadingActivities = false;

    constructor(){
        makeAutoObservable(this);

        reaction(
            () => this.activeTab,
            activeTab => {
                if(activeTab === 3 || activeTab === 4){
                    const predicate = activeTab === 3 ? 'followers' : 'following';
                    this.loadFollowings(predicate);
                }else{
                    this.followings = [];
                }
            }
        )
    }

    setActiveTab = (activeTab: number) => {
        this.activeTab = activeTab;
    }

    get isCurrentUser(){
        if(store.userStore.user && this.profile){
            return store.userStore.user.username === this.profile.username;
        }
        return false;
    }

    loadProfile = async(username: string) => {
        this.loadingProfile = true;

        try{
            const profile = await agent.Profiles.get(username);
            runInAction(() => {
                this.profile = profile;
                this.loadingProfile = false;
            })
        }catch(error){
            console.log(error);
            runInAction(() => this.loadingProfile = false)
        }
    }
    uploadPhoto = async (file:Blob) => {
        this.uploading = true;

        try{
            const response = await agent.Profiles.uploadPhoto(file);
            const photo = response.data;
            runInAction(() => {
                if(this.profile ){
                    this.profile.photos?.push(photo);
                    if(photo.isMain && store.userStore.user){
                        store.userStore.setImage(photo.url);
                        this.profile.image = photo.url;
                    }
                }
                this.uploading = false;
            })
        }catch{
            runInAction(() => this.uploading = false)
        }
    }

setMainPhoto = async (photo: Photo) => {
  this.loading = true;
  try {
    await agent.Profiles.setMainPhoto(photo.id);
    runInAction(() => {
      store.userStore.setImage(photo.url);
      if (this.profile?.photos) {
        const current = this.profile.photos.find((p) => p.isMain);
        if (current) current.isMain = false;
        const next = this.profile.photos.find((p) => p.id === photo.id);
        if (next) next.isMain = true;
        this.profile.image = photo.url;
      }
    });
  } catch (error) {
    console.log(error);
  } finally {
    runInAction(() => (this.loading = false));
  }
};

deletePhoto = async (photo: Photo) => {
  this.loading = true;
  try {
    await agent.Profiles.deletePhoto(photo.id);
    runInAction(() => {
      if (this.profile) {
        this.profile.photos = this.profile.photos?.filter((p) => p.id !== photo.id);
      }
    });
  } catch (error) {
    console.log(error);
  } finally {
    runInAction(() => (this.loading = false));
  }
};

updateFollowing = async (username: string, following: boolean) => {
  this.loading = true;
  try {
    await agent.Profiles.updateFollowing(username);
    store.activityStore.updateAttendeeFollowing(username);
    runInAction(() => {
      const currentUsername = store.userStore.user?.username;
      const delta = following ? 1 : -1;

      if (this.profile) {
        if (this.profile.username === username && username !== currentUsername) {
          this.profile.followersCount += delta;
          this.profile.following = !this.profile.following;
        }
        if (this.profile.username === currentUsername) {
          this.profile.followingCount += delta;
        }
      }

      this.followings.forEach((profile) => {
        if (profile.username === username) {
          profile.followersCount += profile.following ? -1 : 1;
          profile.following = !profile.following;
        }
      });
    });
  } catch (error) {
    console.log(error);
  } finally {
    runInAction(() => (this.loading = false));
  }
};

    loadFollowings = async(predicate: string) => {
        this.loadingFollowings = true;
        try{
            const followings = await agent.Profiles.listFollowings(this.profile!.username, predicate);
            runInAction(() => {
                this.followings = followings;
                this.loadingFollowings = false;
            })
        }catch{
            runInAction(() => this.loadingFollowings = false)
        }
    }

loadUserActivities = async (username: string, predicate = 'future') => {
  this.loadingActivities = true;
  try {
    const activities = await agent.Profiles.listActivities(username, predicate);
    runInAction(() => {
      this.userActivities = activities;
      this.loadingActivities = false;
    });
  } catch (error) {
    console.log(error);
    runInAction(() => (this.loadingActivities = false));
  }
};
}