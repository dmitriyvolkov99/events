import { User } from './user';

export class Profile {
  username: string;
  displayName: string;
  image?: string;
  bio?: string;
  followersCount = 0;
  followingCount = 0;
  following = false;
  photos?: Photo[];

  constructor(user: User) {
    this.username = user.username;
    this.displayName = user.displayName;
    this.image = user.image;
  }
}

export interface Photo {
  id: string;
  url: string;
  isMain: boolean;
}

export interface UserActivity {
  id: string;
  title: string;
  category: string;
  date: Date;
}