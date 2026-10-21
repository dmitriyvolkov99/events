import { makeAutoObservable, observable } from 'mobx';
import type { ReactNode } from 'react';

interface Modal {
  open: boolean;
  body: ReactNode;
}

export default class ModalStore {
  modal: Modal = { open: false, body: null };

  constructor() {
    makeAutoObservable(this, { modal: observable.ref });
  }

  openModal = (content: ReactNode) => {
    this.modal = { open: true, body: content };
  };

  closeModal = () => {
    this.modal = { open: false, body: null };
  };
}