import { observer } from 'mobx-react-lite';
import { Link, NavLink } from 'react-router-dom';
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/react';
import { ChevronDown, LogOut, Plus, User } from 'lucide-react';
import clsx from 'clsx';
import { useStore } from '../stores/store';
import Avatar from '../ui/Avatar';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  clsx(
    'rounded-lg px-3 py-2 text-sm font-medium transition-colors',
    isActive ? 'bg-white/15 text-white' : 'text-slate-300 hover:bg-white/10 hover:text-white'
  );

const NavBar = () => {
  const { userStore: { user, logout } } = useStore();

  return (
    <header className="sticky top-0 z-40 bg-slate-900 shadow-md">
      <div className="mx-auto flex h-16 max-w-6xl items-center gap-2 px-4">
        <NavLink to="/" end className="mr-4 text-lg font-bold tracking-tight text-white">
          Events
        </NavLink>

        <nav className="flex items-center gap-1">
          <NavLink to="/activities" className={navLinkClass}>Events</NavLink>
        </nav>

        <div className="ml-auto flex items-center gap-3">
          <Link
            to="/createActivity"
            className="inline-flex items-center gap-1.5 rounded-lg bg-brand-600 px-3.5 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-500"
          >
            <Plus className="size-4" />
            Create Event
          </Link>

          <Menu>
            <MenuButton className="flex items-center gap-2 rounded-full py-1 pr-2 pl-1 text-sm text-slate-200 hover:bg-white/10 focus:outline-none data-focus:ring-2 data-focus:ring-brand-500">
              <Avatar src={user?.image} alt={user?.username ?? 'User'} className="size-8" />
              <span className="font-medium">{user?.username}</span>
              <ChevronDown className="size-4 text-slate-400" />
            </MenuButton>

            <MenuItems
              anchor="bottom end"
              className="z-50 w-48 rounded-xl bg-white p-1 shadow-lg ring-1 ring-slate-200 [--anchor-gap:8px] focus:outline-none"
            >
              <MenuItem>
                <Link
                  to={`/profiles/${user?.username}`}
                  className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-700 data-focus:bg-slate-100"
                >
                  <User className="size-4" /> My Profile
                </Link>
              </MenuItem>
              <MenuItem>
                <button
                  onClick={logout}
                  className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-sm text-slate-700 data-focus:bg-slate-100"
                >
                  <LogOut className="size-4" /> Logout
                </button>
              </MenuItem>
            </MenuItems>
          </Menu>
        </div>
      </div>
    </header>
  );
};

export default observer(NavBar);