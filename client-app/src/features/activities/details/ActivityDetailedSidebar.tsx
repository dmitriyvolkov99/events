import { Link } from 'react-router-dom';
import { observer } from 'mobx-react-lite';
import { Activity } from '../../../app/models/activity';
import Card from '../../../app/ui/Card';
import Avatar from '../../../app/ui/Avatar';
import Badge from '../../../app/ui/Badge';

interface Props {
  activity: Activity;
}

export default observer(function ActivityDetailedSidebar({ activity: { attendees, host } }: Props) {
  if (!attendees) return null;

  return (
    <Card className="overflow-hidden p-0">
      <div className="bg-brand-600 px-5 py-3 text-center text-sm font-semibold text-white">
        {attendees.length} {attendees.length === 1 ? 'Person' : 'People'} going
      </div>
      <ul className="divide-y divide-slate-100 dark:divide-zinc-800">
        {attendees.map((attendee) => (
          <li key={attendee.username} className="flex items-center gap-3 px-5 py-3">
            <Avatar src={attendee.image} alt={attendee.dispayName ?? attendee.username} className="size-12" />
            <div className="min-w-0 flex-1">
              <Link
                to={`/profiles/${attendee.username}`}
                className="block truncate font-medium text-slate-900 dark:text-zinc-100 hover:text-brand-600"
              >
                {attendee.dispayName}
              </Link>
              {attendee.following && <span className="text-xs font-medium text-orange-500">Following</span>}
            </div>
            {attendee.username === host?.username && <Badge tone="orange">Host</Badge>}
          </li>
        ))}
      </ul>
    </Card>
  );
});