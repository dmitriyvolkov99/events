import { observer } from 'mobx-react-lite';
import { useStore } from '../../app/stores/store';
import Card from '../../app/ui/Card';

const ServerError = () => {
  const { commonStore } = useStore();

  return (
    <div className="mx-auto max-w-3xl">
      <h1 className="text-3xl font-bold text-slate-900 dark:text-zinc-100">Server Error</h1>
      <p className="mt-2 text-sm font-semibold text-red-600">{commonStore.error?.message}</p>

      {commonStore.error?.details && (
        <Card className="mt-6">
          <h4 className="text-xs font-semibold tracking-wider text-brand-600 uppercase">Stack Trace</h4>
          <pre className="mt-3 overflow-x-auto text-xs text-slate-700 dark:text-zinc-300">{commonStore.error.details}</pre>
        </Card>
      )}
    </div>
  );
};

export default observer(ServerError);