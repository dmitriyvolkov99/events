import { Link } from 'react-router-dom';
import { Clock, MapPin } from 'lucide-react';
import { format } from 'date-fns';
import { Activity } from '../../../app/models/activity';
import ActivityListItemAttendee from './ActivityListItemAttendee';
import Card from '../../../app/ui/Card';
import Avatar from '../../../app/ui/Avatar';
import Badge from '../../../app/ui/Badge';

interface Props {
  activity: Activity;
}

const ActivityListItem = ({ activity }: Props) => {
  return (
<Card className="p-0">
  {activity.isCancelled && (
    <div className="rounded-t-2xl bg-red-600 px-5 py-1.5 text-center text-sm font-medium text-white">
      Cancelled
    </div>
  )}

      <div className="flex gap-4 p-5">
        <Avatar src={activity.host?.image} alt={activity.host?.dispayName ?? 'Host'} className="size-14" />
        <div className="min-w-0 flex-1">
          <Link
            to={`/activities/${activity.id}`}
            className="text-lg font-semibold text-slate-900 dark:text-zinc-100 hover:text-brand-600"
          >
            {activity.title}
          </Link>
          <p className="mt-0.5 text-sm text-slate-500 dark:text-zinc-400">
            Hosted by{' '}
            <Link to={`/profiles/${activity.hostUsername}`} className="font-medium text-slate-700 dark:text-zinc-300 hover:text-brand-600">
              {activity.host?.dispayName}
            </Link>
          </p>
          <div className="mt-2 flex flex-wrap gap-2">
            {activity.isHost && <Badge tone="orange">You are hosting this event</Badge>}
            {activity.isGoing && !activity.isHost && <Badge tone="green">You are going to this event</Badge>}
          </div>
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-x-6 gap-y-1 border-t border-slate-100 dark:border-zinc-800 px-5 py-3 text-sm text-slate-600 dark:text-zinc-400">
        <span className="inline-flex items-center gap-1.5">
          <Clock className="size-4 text-slate-400" />
          {format(activity.date!, 'dd MMM yyyy h:mm aa')}
        </span>
        <span className="inline-flex items-center gap-1.5">
          <MapPin className="size-4 text-slate-400" />
          {activity.venue}
        </span>
      </div>

      <div className="border-t border-slate-100 dark:border-zinc-800 bg-slate-50 dark:bg-zinc-800/50 px-5 py-3">
        <ActivityListItemAttendee attendees={activity.attendees!} />
      </div>

      <div className="flex items-center justify-between gap-4 border-t border-slate-100 dark:border-zinc-800 px-5 py-4">
        <p className="line-clamp-2 text-sm text-slate-600 dark:text-zinc-400">{activity.description}</p>
        <Link
          to={`/activities/${activity.id}`}
          className="shrink-0 rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-700"
        >
          View
        </Link>
      </div>
    </Card>
  );
};

export default ActivityListItem;