import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { useStore } from '../../app/stores/store';
import LoginForm from '../users/LoginForm';
import RegisterForm from '../users/RegisterForm';

const outlineBtn =
  'rounded-xl border border-white/40 px-7 py-3 text-lg font-medium text-white transition-colors hover:bg-white/10';

const HomePage = () => {
  const { userStore, modalStore } = useStore();

  return (
    <section className="flex min-h-screen items-center bg-gradient-to-br from-brand-700 via-slate-900 to-slate-950 text-center text-white">
      <div className="mx-auto max-w-2xl px-4">
        <img src="/assets/logo.png" alt="logo" className="mx-auto mb-4 size-24 sm:size-28" />
        <h1 className="text-5xl font-bold tracking-tight sm:text-6xl">Events</h1>

        {userStore.isLoggedIn ? (
          <>
            <p className="mt-6 text-2xl text-slate-200">Welcome to Events</p>
            <div className="mt-8">
              <Link to="/activities" className={outlineBtn}>
                Go to events
              </Link>
            </div>
          </>
        ) : (
          <div className="mt-10 flex flex-wrap justify-center gap-4">
            <button onClick={() => modalStore.openModal(<LoginForm />)} className={outlineBtn}>
              Login
            </button>
            <button onClick={() => modalStore.openModal(<RegisterForm />)} className={outlineBtn}>
              Register
            </button>
          </div>
        )}
      </div>
    </section>
  );
};

export default observer(HomePage);