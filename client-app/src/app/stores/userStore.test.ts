import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('../api/agent', () => ({
  default: {
    Account: { login: vi.fn(), register: vi.fn(), current: vi.fn() },
    Activities: { list: vi.fn() },
    Profiles: {},
  },
}));
vi.mock('../routes', () => ({ router: { navigate: vi.fn() } }));

import agent from '../api/agent';
import { router } from '../routes';
import { store } from './store';

const user = { username: 'bob', dispayName: 'Bob', token: 'jwt-token' };
const creds = { email: 'bob@test.com', password: 'Pa$$w0rd' };

describe('UserStore', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    store.userStore.user = null;
    store.commonStore.setToken(null);
    store.modalStore.closeModal();
  });

  it('logs in: saves token and user, closes the modal, goes to activities', async () => {
    vi.mocked(agent.Account.login).mockResolvedValue(user);
    store.modalStore.openModal('login form');

    await store.userStore.login(creds);

    expect(store.userStore.user).toEqual(user);
    expect(store.userStore.isLoggedIn).toBe(true);
    expect(store.commonStore.token).toBe('jwt-token');
    expect(store.modalStore.modal.open).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith('/activities');
  });

  it('keeps the user logged out when login fails', async () => {
    vi.mocked(agent.Account.login).mockRejectedValue(new Error('401'));

    await expect(store.userStore.login(creds)).rejects.toThrow('401');

    expect(store.userStore.isLoggedIn).toBe(false);
    expect(store.commonStore.token).toBeNull();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('logs out: clears token and user, goes home', async () => {
    vi.mocked(agent.Account.login).mockResolvedValue(user);
    await store.userStore.login(creds);
    vi.mocked(router.navigate).mockClear();

    store.userStore.logout();

    expect(store.userStore.user).toBeNull();
    expect(store.commonStore.token).toBeNull();
    expect(localStorage.getItem('jwt')).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith('/');
  });

  it('loads the current user', async () => {
    vi.mocked(agent.Account.current).mockResolvedValue(user);

    await store.userStore.getUser();

    expect(store.userStore.user).toEqual(user);
  });

  it('stays logged out when the current user request fails', async () => {
    const log = vi.spyOn(console, 'log').mockImplementation(() => {});
    vi.mocked(agent.Account.current).mockRejectedValue(new Error('401'));

    await store.userStore.getUser();

    expect(store.userStore.user).toBeNull();
    log.mockRestore();
  });
});