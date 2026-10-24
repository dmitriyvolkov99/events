import { observer } from 'mobx-react-lite';
import { Dialog, DialogBackdrop, DialogPanel } from '@headlessui/react';
import { useStore } from '../../stores/store';

const ModalContainer = () => {
  const { modalStore } = useStore();

  return (
    <Dialog open={modalStore.modal.open} onClose={modalStore.closeModal} className="relative z-50">
      <DialogBackdrop
        transition
        className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm transition duration-150 ease-out data-closed:opacity-0"
      />
      <div className="fixed inset-0 flex items-center justify-center overflow-y-auto p-4">
        <DialogPanel
          transition
          className="w-full max-w-sm rounded-2xl bg-white p-6 shadow-xl transition duration-150 ease-out data-closed:scale-95 data-closed:opacity-0 dark:bg-zinc-900"
        >
          {modalStore.modal.body}
        </DialogPanel>
      </div>
    </Dialog>
  );
};

export default observer(ModalContainer);