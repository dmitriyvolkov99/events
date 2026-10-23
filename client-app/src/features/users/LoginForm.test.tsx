import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('../../app/api/agent', () => ({
  default: {
    Account: { login: vi.fn(), register: vi.fn(), current: vi.fn() },
    Activities: {},
    Profiles: {},
  },
}));
vi.mock('../../app/routes', () => ({ router: { navigate: vi.fn() } }));

import agent from '../../app/api/agent';
import { router } from '../../app/routes';
import { store } from '../../app/stores/store';
import LoginForm from './LoginForm';

async function fillAndSubmit(password: string) {
  await userEvent.type(screen.getByPlaceholderText('Email'), 'bob@test.com');
  await userEvent.type(screen.getByPlaceholderText('Password'), password);
  await userEvent.click(screen.getByRole('button', { name: 'Log in' }));
}

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    store.userStore.user = null;
    store.commonStore.setToken(null);
  });

  afterEach(() => cleanup());

  it('shows an error and stays on the page when credentials are wrong', async () => {
    vi.mocked(agent.Account.login).mockRejectedValue(new Error('401'));
    render(<LoginForm />);

    await fillAndSubmit('wrong');

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid email or password');
    expect(store.userStore.isLoggedIn).toBe(false);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('logs the user in and goes to activities', async () => {
    vi.mocked(agent.Account.login).mockResolvedValue({ username: 'bob', dispayName: 'Bob', token: 'jwt' });
    render(<LoginForm />);

    await fillAndSubmit('Pa$$w0rd');

    await waitFor(() => expect(store.userStore.isLoggedIn).toBe(true));
    expect(agent.Account.login).toHaveBeenCalledWith(
      expect.objectContaining({ email: 'bob@test.com', password: 'Pa$$w0rd' })
    );
    expect(router.navigate).toHaveBeenCalledWith('/activities');
  });
});