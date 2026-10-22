import { format } from 'date-fns';
import { observer } from 'mobx-react-lite';
import { Calendar, Info, MapPin } from 'lucide-react';
import type { ReactNode } from 'react';
import { Activity } from '../../../app/models/activity';
import Card from '../../../app/ui/Card';

interface Props {
  activity: Activity;
}

function Row({ icon, children }: { icon: ReactNode; children: ReactNode }) {
  return (
    <div className="flex items-start gap-4 px-6 py-4">
      <span className="mt-0.5 text-brand-600">{icon}</span>
      <div className="text-sm text-slate-700 dark:text-zinc-300">{children}</div>
    </div>
  );
}

export default observer(function ActivityDetailedInfo({ activity }: Props) {
  return (
    <Card className="divide-y divide-slate-100 dark:divide-zinc-800 p-0">
      <Row icon={<Info className="size-5" />}>
        <p className="whitespace-pre-wrap">{activity.description}</p>
      </Row>
      <Row icon={<Calendar className="size-5" />}>{format(activity.date, 'dd MMM yyyy h:mm aa')}</Row>
      <Row icon={<MapPin className="size-5" />}>
        {activity.venue}, {activity.city}
      </Row>
    </Card>
  );
});