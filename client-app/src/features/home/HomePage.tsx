import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { CalendarPlus, MessageCircle, Users } from 'lucide-react';
import { useStore } from '../../app/stores/store';
import LoginForm from '../users/LoginForm';
import RegisterForm from '../users/RegisterForm';

const features = [
  { icon: CalendarPlus, title: 'Create events', text: 'Plan a meetup, pick the date and place, and invite people.' },
  { icon: Users, title: 'Join and follow', text: 'Join events, follow other people and see who is going.' },
  { icon: MessageCircle, title: 'Chat live', text: 'Discuss every event in a real-time chat.' },
];

const solidBtn =
  'rounded-xl bg-white px-7 py-3 text-lg font-medium text-slate-900 transition-colors hover:bg-slate-100';
const outlineBtn =
  'rounded-xl border border-white/40 px-7 py-3 text-lg font-medium text-white transition-colors hover:bg-white/10';

const HomePage = () => {
  const { userStore, modalStore } = useStore();

  return (
    <section className="flex min-h-screen items-center bg-gradient-to-br from-brand-700 via-slate-900 to-slate-950 py-16 text-center text-white">
      <div className="mx-auto max-w-4xl px-4">
        <img src="/assets/logo.png" alt="logo" className="mx-auto mb-4 size-24 sm:size-28" />
        <h1 className="text-5xl font-bold tracking-tight sm:text-6xl">Events</h1>
        <p className="mx-auto mt-4 max-w-xl text-lg text-slate-300">
          Find things to do, host your own events and talk to the people who come.
        </p>

        <div className="mt-10 flex flex-wrap justify-center gap-4">
          {userStore.isLoggedIn ? (
            <Link to="/activities" className={solidBtn}>
              Go to events
            </Link>
          ) : (
            <>
              <button onClick={() => modalStore.openModal(<LoginForm />)} className={solidBtn}>
                Login
              </button>
              <button onClick={() => modalStore.openModal(<RegisterForm />)} className={outlineBtn}>
                Register
              </button>
            </>
          )}
        </div>

        <ul className="mt-16 grid gap-4 text-left sm:grid-cols-3">
          {features.map(({ icon: Icon, title, text }) => (
            <li key={title} className="rounded-2xl bg-white/5 p-5 ring-1 ring-white/10">
              <Icon className="size-6 text-brand-300" />
              <h2 className="mt-3 font-semibold">{title}</h2>
              <p className="mt-1 text-sm text-slate-300">{text}</p>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
};

export default observer(HomePage);