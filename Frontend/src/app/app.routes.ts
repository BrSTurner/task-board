import { Routes } from '@angular/router';

import { authGuard, guestGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  // The /tasks guard sends signed-out visitors to /login.
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  {
    path: 'login',
    title: 'Sign in · TaskBoard',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/pages/login/login').then((m) => m.LoginPage),
  },
  {
    path: 'register',
    title: 'Create account · TaskBoard',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/pages/register/register').then((m) => m.RegisterPage),
  },
  {
    path: 'tasks',
    title: 'My tasks · TaskBoard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/pages/task-board/task-board').then((m) => m.TaskBoardPage),
  },
  { path: '**', redirectTo: 'tasks' },
];
