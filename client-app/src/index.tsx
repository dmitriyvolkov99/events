import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import './app/layout/styles.css';
import 'react-calendar/dist/Calendar.css';
import 'react-datepicker/dist/react-datepicker.css';
import '@fontsource-variable/inter';
import App from './app/layout/App';
import ScrollToTop from './app/layout/ScrollToTop';
import { store, StoreContext } from './app/stores/store';

const container = document.getElementById('root');
if (container) {
  createRoot(container).render(
    <StrictMode>
      <StoreContext.Provider value={store}>
        <BrowserRouter>
          <ScrollToTop />
          <App />
        </BrowserRouter>
      </StoreContext.Provider>
    </StrictMode>
  );
}