import { Link } from 'react-router-dom';
import { Search } from 'lucide-react';
import Card from '../../app/ui/Card';

const NotFound = () => {
  return (
    <Card className="mx-auto flex max-w-xl flex-col items-center gap-5 py-12 text-center">
      <Search className="size-12 text-slate-400" />
      <p className="text-lg font-medium text-slate-700">
        Oops - we've looked everywhere and could not find this.
      </p>
      <Link
        to="/activities"
        className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-700"
      >
        Return to activities page
      </Link>
    </Card>
  );
};

export default NotFound;