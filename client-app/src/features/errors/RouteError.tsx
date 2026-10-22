import { isRouteErrorResponse, Link, useRouteError } from 'react-router-dom';
import Card from '../../app/ui/Card';

export default function RouteError() {
  const error = useRouteError();

  const message = isRouteErrorResponse(error)
    ? `${error.status} ${error.statusText}`
    : error instanceof Error
      ? error.message
      : 'Unknown error';

  return (
    <Card className="mx-auto mt-16 max-w-xl text-center">
      <title>Error · Events</title>
      <h1 className="text-xl font-semibold text-slate-900 dark:text-zinc-100">Something went wrong</h1>
      <p className="mt-2 text-sm text-slate-600 dark:text-zinc-400">{message}</p>
      <Link
        to="/"
        className="mt-6 inline-block rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-700"
      >
        Back to home
      </Link>
    </Card>
  );
}