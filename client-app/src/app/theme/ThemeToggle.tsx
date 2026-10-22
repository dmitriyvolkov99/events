import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/react';
import { Check, Monitor, Moon, Sun, type LucideIcon } from 'lucide-react';
import { useTheme, type Theme } from './useTheme';

const options: { value: Theme; label: string; Icon: LucideIcon }[] = [
  { value: 'light', label: 'Light', Icon: Sun },
  { value: 'dark', label: 'Dark', Icon: Moon },
  { value: 'system', label: 'System', Icon: Monitor },
];

export default function ThemeToggle() {
  const { theme, setTheme } = useTheme();

  return (
    <Menu>
      <MenuButton
        aria-label="Change theme"
        className="rounded-full p-2 text-slate-300 hover:bg-white/10 hover:text-white focus:outline-none data-focus:ring-2 data-focus:ring-brand-500"
      >
        {theme === 'dark' ? (
          <Moon className="size-5" />
        ) : theme === 'light' ? (
          <Sun className="size-5" />
        ) : (
          <Monitor className="size-5" />
        )}
      </MenuButton>

      <MenuItems
        anchor="bottom end"
        className="z-50 w-40 rounded-xl  bg-white dark:bg-zinc-900 p-1 shadow-lg ring-1 ring-slate-200 dark:ring-zinc-800 [--anchor-gap:8px] focus:outline-none"
      >
        {options.map(({ value, label, Icon }) => (
          <MenuItem key={value}>
            <button
              onClick={() => setTheme(value)}
              className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-sm text-slate-700 dark:text-zinc-300 dark:hover:bg-zinc-800 data-focus:bg-slate-100 dark:data-focus:bg-zinc-800"
            >
              <Icon className="size-4" />
              {label}
              {theme === value && <Check className="ml-auto size-4 text-brand-600" />}
            </button>
          </MenuItem>
        ))}
      </MenuItems>
    </Menu>
  );
}