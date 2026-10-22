import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from 'react-router-dom';
import './app/layout/styles.css';
import 'react-calendar/dist/Calendar.css';
import 'react-datepicker/dist/react-datepicker.css';
import '@fontsource-variable/inter';
import { store, StoreContext } from './app/stores/store';
import { router } from './app/routes';

const container = document.getElementById('root');
if (container) {
  createRoot(container).render(
    <StrictMode>
      <StoreContext.Provider value={store}>
        <RouterProvider router={router} />
      </StoreContext.Provider>
    </StrictMode>
  );
}