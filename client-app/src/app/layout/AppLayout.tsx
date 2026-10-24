import { Outlet } from 'react-router-dom';
import NavBar from './NavBar';

export default function AppLayout() {
  return (
    <>
      <a
        href="#main"
        onClick={(e) => {
          e.preventDefault();
          document.getElementById('main')?.focus();
        }}
        className="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-[70] focus:rounded-lg focus:bg-white focus:px-4 focus:py-2 focus:text-sm focus:font-medium focus:text-slate-900 dark:focus:bg-zinc-900 dark:focus:text-zinc-100"
      >
        Skip to content
      </a>
      <NavBar />
      <main id="main" tabIndex={-1} className="mx-auto max-w-6xl px-4 py-8 focus:outline-none">
        <Outlet />
      </main>
    </>
  );
}