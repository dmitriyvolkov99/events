import { beforeEach, describe, expect, it } from 'vitest';
import CommonStore from './commonStore';

describe('CommonStore', () => {
  beforeEach(() => localStorage.clear());

  it('reads the token from localStorage', () => {
    localStorage.setItem('jwt', 'abc');
    expect(new CommonStore().token).toBe('abc');
  });

  it('persists token changes and removes the token on logout', () => {
    const store = new CommonStore();

    store.setToken('xyz');
    expect(localStorage.getItem('jwt')).toBe('xyz');

    store.setToken(null);
    expect(localStorage.getItem('jwt')).toBeNull();
  });
});