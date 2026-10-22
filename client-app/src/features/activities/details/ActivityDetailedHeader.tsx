import { format } from 'date-fns';
import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { Activity } from '../../../app/models/activity';
import { useStore } from '../../../app/stores/store';
import Card from '../../../app/ui/Card';
import Button from '../../../app/ui/Button';
import Badge from '../../../app/ui/Badge';

interface Props {
  activity: Activity;
}

export default observer(function ActivityDetailedHeader({ activity }: Props) {
  const { activityStore: { updateAttendance, loading, cancelActivityToggle } } = useStore();

  return (
    <Card className="overflow-hidden p-0">
      <div className="relative">
        <img
          src={`/assets/categoryImages/${activity.category}.jpg`}
          alt={activity.category}
          className="h-48 sm:h-64 w-full object-cover"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-slate-900/85 via-slate-900/30 to-transparent" />

        {activity.isCancelled && (
          <div className="absolute top-4 left-4">
            <Badge tone="red">Cancelled</Badge>
          </div>
        )}

        <div className="absolute inset-x-0 bottom-0 p-6 text-white">
          <h1 className="text-2xl font-bold tracking-tight sm:text-3xl">{activity.title}</h1>
          <p className="mt-1 text-sm text-slate-200">{format(activity.date!, 'dd MMM yyyy')}</p>
          <p className="text-sm text-slate-200">
            Hosted by{' '}
            <Link to={`/profiles/${activity.host?.username}`} className="font-semibold text-white underline-offset-2 hover:underline">
              {activity.host?.dispayName}
            </Link>
          </p>
        </div>
      </div>

      <div className="flex items-center justify-between gap-3 border-t border-slate-100 px-6 py-4">
        {activity.isHost ? (
          <>
            <Button
              variant={activity.isCancelled ? 'primary' : 'danger'}
              onClick={cancelActivityToggle}
              loading={loading}
            >
              {activity.isCancelled ? 'Re-activate' : 'Cancel Activity'}
            </Button>

            {activity.isCancelled ? (
              <span aria-disabled="true" className="cursor-not-allowed rounded-lg bg-slate-100 px-4 py-2 text-sm font-medium text-slate-400">
                Manage Event
              </span>
            ) : (
              <Link
                to={`/manage/${activity.id}`}
                className="rounded-lg bg-orange-500 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-orange-600"
              >
                Manage Event
              </Link>
            )}
          </>
        ) : activity.isGoing ? (
          <Button variant="secondary" loading={loading} onClick={updateAttendance}>
            Cancel attendance
          </Button>
        ) : (
          <Button loading={loading} disabled={activity.isCancelled} onClick={updateAttendance}>
            Join Activity
          </Button>
        )}
      </div>
    </Card>
  );
});