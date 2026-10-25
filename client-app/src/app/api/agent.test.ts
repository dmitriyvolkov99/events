import axios, { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('react-toastify', () => ({ toast: { error: vi.fn() } }));
vi.mock('../routes', () => ({ router: { navigate: vi.fn() } }));

import { toast } from 'react-toastify';
import { store } from '../stores/store';
import { PaginatedResult } from '../models/paginations';
import { router } from '../routes';
import agent from './agent';

const adapter = vi.fn();

function respondWith(status: number, data: unknown = {}, headers: Record<string, string> = {}) {
  adapter.mockImplementation(async (config: InternalAxiosRequestConfig) => {
    const response = { data, status, statusText: String(status), headers, config } as AxiosResponse;
    if (status < 200 || status >= 300) {
      throw new AxiosError(
        `Request failed with status code ${status}`,
        AxiosError.ERR_BAD_REQUEST,
        config,
        null,
        response
      );
    }
    return response;
  });
}

describe('agent interceptors', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubEnv('DEV', false);
    localStorage.clear();
    store.commonStore.setToken(null);
    axios.defaults.adapter = adapter;
  });

  afterEach(() => vi.unstubAllEnvs());

  it('sends the bearer token when logged in', async () => {
    store.commonStore.setToken('jwt-token');
    respondWith(200, {});

    await agent.Account.current();

    expect(adapter.mock.calls[0][0].headers.get('Authorization')).toBe('Bearer jwt-token');
  });

  it('sends no Authorization header without a token', async () => {
    respondWith(200, {});

    await agent.Account.current();

    expect(adapter.mock.calls[0][0].headers.get('Authorization')).toBeFalsy();
  });

  it('wraps paged responses using the Pagination header', async () => {
    const pagination = { currentPage: 1, itemsPerPage: 10, totalItems: 1, totalPages: 1 };
    respondWith(200, [{ id: '1' }], { pagination: JSON.stringify(pagination) });

    const result = await agent.Activities.list(new URLSearchParams());

    expect(result).toBeInstanceOf(PaginatedResult);
    expect(result.data).toEqual([{ id: '1' }]);
    expect(result.pagination.totalItems).toBe(1);
  });

  it('rejects with a flat list of messages on 400 validation errors', async () => {
    respondWith(400, { errors: { Email: ['Email is taken'], Password: ['Too short', 'Needs a digit'] } });

    await expect(agent.Account.register({ email: 'a@b.c', password: 'x' })).rejects.toEqual([
      'Email is taken',
      'Too short',
      'Needs a digit',
    ]);
  });

  it('shows a toast on a plain-text 400', async () => {
    respondWith(400, 'Bad request text');

    await expect(agent.Activities.details('1')).rejects.toMatchObject({ response: { status: 400 } });

    expect(toast.error).toHaveBeenCalledWith('Bad request text');
  });

  it('redirects to not-found when a GET fails with an id error', async () => {
    respondWith(400, { errors: { id: ['Invalid id'] } });

    await expect(agent.Activities.details('bad')).rejects.toEqual(['Invalid id']);

    expect(router.navigate).toHaveBeenCalledWith('/not-found');
  });

  it('shows a toast on 401', async () => {
    respondWith(401);

    await expect(agent.Account.current()).rejects.toMatchObject({ response: { status: 401 } });

    expect(toast.error).toHaveBeenCalledTimes(1);
  });

  it('redirects to not-found on 404', async () => {
    respondWith(404);

    await expect(agent.Activities.details('1')).rejects.toMatchObject({ response: { status: 404 } });

    expect(router.navigate).toHaveBeenCalledWith('/not-found');
  });

  it('stores the error and redirects to server-error on 500', async () => {
    respondWith(500, { statusCode: 500, message: 'Boom', details: 'stack trace' });

    await expect(agent.Activities.list(new URLSearchParams())).rejects.toMatchObject({
      response: { status: 500 },
    });

    expect(store.commonStore.error?.message).toBe('Boom');
    expect(router.navigate).toHaveBeenCalledWith('/server-error');
  });

  it('passes network errors through without toasts or redirects', async () => {
    adapter.mockRejectedValue(new AxiosError('Network Error', AxiosError.ERR_NETWORK));

    await expect(agent.Account.current()).rejects.toMatchObject({ message: 'Network Error' });

    expect(toast.error).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('shows a toast on 429', async () => {
    respondWith(429);

    await expect(agent.Account.current()).rejects.toMatchObject({ response: { status: 429 } });

    expect(toast.error).toHaveBeenCalledTimes(1);
  });

  it('does not show a toast when the login request returns 401', async () => {
    respondWith(401);

    await expect(agent.Account.login({ email: 'a@b.c', password: 'x' })).rejects.toMatchObject({
      response: { status: 401 },
    });

    expect(toast.error).not.toHaveBeenCalled();
  });
});