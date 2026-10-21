import clsx from 'clsx';
import Spinner from '../ui/Spinner';

interface Props {
  inverted?: boolean;
  content?: string;
}

const LoadingComponent = ({ inverted = true, content = 'Loading...' }: Props) => {
  return (
    <div
      className={clsx(
        'fixed inset-0 z-50 flex flex-col items-center justify-center gap-3 backdrop-blur-sm',
        inverted ? 'bg-white/80 text-slate-700' : 'bg-slate-900/70 text-white'
      )}
    >
      <Spinner className="size-8" />
      {content && <p className="text-sm font-medium">{content}</p>}
    </div>
  );
};

export default LoadingComponent;