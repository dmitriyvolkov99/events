import { useNavigation } from 'react-router-dom';

export default function NavigationProgress() {
  const { state } = useNavigation();
  if (state === 'idle') return null;

  return (
    <div
      role="progressbar"
      aria-label="Loading page"
      className="pointer-events-none fixed inset-x-0 top-0 z-[60] h-0.5 overflow-hidden"
    >
      <div className="nav-progress h-full bg-brand-500" />
    </div>
  );
}