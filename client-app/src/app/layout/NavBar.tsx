import { observer } from 'mobx-react-lite';
import { Link, NavLink } from 'react-router-dom';
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/react';
import { ChevronDown, LogOut, Plus, User } from 'lucide-react';
import { cn } from '../ui/cn';
import { useStore } from '../stores/store';
import Avatar from '../ui/Avatar';
import ThemeToggle from '../theme/ThemeToggle';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'rounded-lg px-3 py-2 text-sm font-medium transition-colors',
    isActive ? 'bg-white/15 text-white' : 'text-slate-300 hover:bg-white/10 hover:text-white'
  );

const NavBar = () => {
  const { userStore: { user, logout } } = useStore();

  return (
    <header className="sticky top-0 z-40 bg-slate-900 shadow-md dark:bg-zinc-900 dark:shadow-none dark:ring-1 dark:ring-zinc-800">
      <div className="mx-auto flex h-16 max-w-6xl items-center gap-1 px-4 sm:gap-2">
        <NavLink to="/" end className="mr-2 flex items-center gap-2 text-lg font-bold tracking-tight text-white sm:mr-4">
          <img src="/assets/logo.png" alt="" className="size-7" />
          Events
        </NavLink>

        {user && (
          <nav className="flex items-center gap-1">
            <NavLink to="/activities" className={navLinkClass}>
              All events
            </NavLink>
          </nav>
        )}

        <div className="ml-auto flex items-center gap-2 sm:gap-3">
          <ThemeToggle />

          {user ? (
            <>
              <Link
                to="/createActivity"
                aria-label="Create Event"
                className="inline-flex items-center gap-1.5 rounded-lg bg-brand-600 px-3.5 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-500"
              >
                <Plus className="size-4" />
                <span className="hidden sm:inline">Create Event</span>
              </Link>

              <Menu>
                <MenuButton className="flex items-center gap-2 rounded-full py-1 pr-2 pl-1 text-sm text-slate-200 hover:bg-white/10 focus:outline-none data-focus:ring-2 data-focus:ring-brand-500">
                  <Avatar src={user.image} alt={user.username} className="size-8" />
                  <span className="hidden font-medium sm:inline">{user.username}</span>
                  <ChevronDown className="size-4 text-slate-400" />
                </MenuButton>

                <MenuItems
                  anchor="bottom end"
                  className="z-50 w-48 rounded-xl bg-white p-1 shadow-lg ring-1 ring-slate-200 [--anchor-gap:8px] focus:outline-none dark:bg-zinc-900 dark:ring-zinc-700"
                >
                  <MenuItem>
                    <Link
                      to={`/profiles/${user.username}`}
                      className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-700 data-focus:bg-slate-100 dark:text-zinc-300 dark:data-focus:bg-zinc-800"
                    >
                      <User className="size-4" /> My Profile
                    </Link>
                  </MenuItem>
                  <MenuItem>
                    <button
                      onClick={logout}
                      className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-sm text-slate-700 data-focus:bg-slate-100 dark:text-zinc-300 dark:data-focus:bg-zinc-800"
                    >
                      <LogOut className="size-4" /> Logout
                    </button>
                  </MenuItem>
                </MenuItems>
              </Menu>
            </>
          ) : (
            <Link
              to="/"
              className="rounded-lg bg-brand-600 px-3.5 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-500"
            >
              Log in
            </Link>
          )}
        </div>
      </div>
    </header>
  );
};

export default observer(NavBar);