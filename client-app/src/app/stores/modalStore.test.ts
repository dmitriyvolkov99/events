import { describe, expect, it } from 'vitest';
import ModalStore from './modalStore';

describe('ModalStore', () => {
  it('starts closed', () => {
    const store = new ModalStore();
    expect(store.modal.open).toBe(false);
    expect(store.modal.body).toBeNull();
  });

  it('opens with content and closes', () => {
    const store = new ModalStore();
    store.openModal('content');
    expect(store.modal).toEqual({ open: true, body: 'content' });

    store.closeModal();
    expect(store.modal).toEqual({ open: false, body: null });
  });
});