import { observer } from 'mobx-react-lite';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useStore } from '../stores/store';

function PrivateRoute() {
  const { userStore: { isLoggedIn } } = useStore();
  const location = useLocation();

  return isLoggedIn ? <Outlet /> : <Navigate to="/" state={{ from: location }} replace />;
}

export default observer(PrivateRoute);