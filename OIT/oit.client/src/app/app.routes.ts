import { Routes } from '@angular/router';

// Views are loaded by domain so the root shell stays independent of business screens.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  {
    path: 'orders',
    loadComponent: () =>
      import('./domains/orders/views/orders-list/orders-list').then((m) => m.OrdersList),
  },
  {
    path: 'orders/new',
    loadComponent: () =>
      import('./domains/orders/views/order-form/order-form').then((m) => m.OrderForm),
  },
  {
    path: 'orders/:id/edit',
    loadComponent: () =>
      import('./domains/orders/views/order-form/order-form').then((m) => m.OrderForm),
  },
  {
    path: 'orders/:id',
    loadComponent: () =>
      import('./domains/orders/views/order-detail/order-detail').then((m) => m.OrderDetail),
  },
  {
    path: 'clients',
    loadComponent: () =>
      import('./domains/clients/views/clients-list/clients-list').then((m) => m.ClientsList),
  },
  { path: '**', redirectTo: 'orders' },
];
