import { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { observer } from 'mobx-react-lite';
import { ToastContainer } from 'react-toastify';
import { useStore } from '../stores/store';
import LoadingComponent from './LoadingComponent';
import ModalContainer from '../common/modals/ModalContainer';
import ScrollToTop from './ScrollToTop';
import NavigationProgress from './NavigationProgress';

function App() {
  const { commonStore, userStore } = useStore();

  useEffect(() => {
    if (commonStore.token) {
      userStore.getUser().finally(() => commonStore.setAppLoaded());
    } else {
      commonStore.setAppLoaded();
    }
  }, [commonStore, userStore]);

  if (!commonStore.appLoaded) return <LoadingComponent content="Loading app" />;

  return (
    <>
      <NavigationProgress />
      <ScrollToTop />
      <ToastContainer position="bottom-right" hideProgressBar />
      <ModalContainer />
      <Outlet />
    </>
  );
}

export default observer(App);