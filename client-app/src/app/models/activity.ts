import { Profile } from './profile';

export class Activity {
  id!: string;
  title = '';
  date: Date | null = null;
  description = '';
  category = '';
  city = '';
  venue = '';
  hostUsername = '';
  isCancelled = false;
  isGoing = false;
  isHost = false;
  host?: Profile;
  attendees: Profile[] = [];

  constructor(init?: ActivityFormValues) {
    Object.assign(this, init);
  }
}

export class ActivityFormValues {
  id?: string = undefined;
  title = '';
  category = '';
  description = '';
  date: Date | null = null;
  city = '';
  venue = '';

  constructor(activity?: ActivityFormValues) {
    if (activity) {
      this.id = activity.id;
      this.title = activity.title;
      this.category = activity.category;
      this.description = activity.description;
      this.date = activity.date;
      this.city = activity.city;
      this.venue = activity.venue;
    }
  }
}