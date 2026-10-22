import type { ComponentType } from 'react';
import { createBrowserRouter } from 'react-router-dom';
import LoadingComponent from './layout/LoadingComponent';
import RouteError from '../features/errors/RouteError';

const page = (importer: () => Promise<{ default: ComponentType }>) => async () => {
  const { default: Component } = await importer();
  return { Component };
};

export const router = createBrowserRouter([
  {
    lazy: page(() => import('./layout/App')),
    ErrorBoundary: RouteError,
    hydrateFallbackElement: <LoadingComponent content="Loading app" />,
    children: [
      { path: '/', lazy: page(() => import('../features/home/HomePage')) },
      {
        lazy: page(() => import('./layout/AppLayout')),
        children: [
          {
            ErrorBoundary: RouteError,
            children: [
              {
                lazy: page(() => import('./layout/PrivateRoute')),
                children: [
                  { path: 'activities', lazy: page(() => import('../features/activities/dashboard/ActivityDashboard')) },
                  { path: 'activities/:id', lazy: page(() => import('../features/activities/details/ActivityDetails')) },
                  { path: 'createActivity', lazy: page(() => import('../features/activities/form/ActivityForm')) },
                  { path: 'manage/:id', lazy: page(() => import('../features/activities/form/ActivityForm')) },
                  { path: 'profiles/:username', lazy: page(() => import('../features/profiles/ProfilePage')) },
                ],
              },
              { path: 'server-error', lazy: page(() => import('../features/errors/ServerError')) },
              { path: '*', lazy: page(() => import('../features/errors/NotFound')) },
            ],
          },
        ],
      },
    ],
  },
]);