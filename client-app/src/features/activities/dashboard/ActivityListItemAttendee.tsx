import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import clsx from 'clsx';
import { Profile } from '../../../app/models/profile';
import ProfileCard from '../../profiles/ProfileCard';
import Avatar from '../../../app/ui/Avatar';

interface Props {
  attendees: Profile[];
}

const ActivityListItemAttendee = ({ attendees }: Props) => {
  return (
    <ul className="flex flex-wrap items-center gap-2">
      {attendees.map((attendee) => (
        <li key={attendee.username} className="group relative">
          <Link to={`/profiles/${attendee.username}`} className="block rounded-full">
            <Avatar
              src={attendee.image}
              alt={attendee.dispayName ?? attendee.username}
              className={clsx('size-9', attendee.following && 'ring-2 ring-red-500')}
            />
          </Link>

          <div className="invisible absolute top-full left-0 z-30 pt-2 opacity-0 transition-opacity group-focus-within:visible group-focus-within:opacity-100 group-hover:visible group-hover:opacity-100">
            <div className="w-56 rounded-xl  bg-white dark:bg-zinc-900 shadow-lg ring-1 ring-slate-200 dark:ring-zinc-800">
              <ProfileCard profile={attendee} />
            </div>
          </div>
        </li>
      ))}
    </ul>
  );
};

export default observer(ActivityListItemAttendee);