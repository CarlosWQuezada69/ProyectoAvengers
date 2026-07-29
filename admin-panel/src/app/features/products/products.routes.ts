import { Routes } from '@angular/router';

export const productRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./product-list/product-list').then(m => m.ProductListComponent),
  },
  {
    path: 'new',
    loadComponent: () => import('./product-form/product-form').then(m => m.ProductFormComponent),
  },
  {
    path: ':id',
    loadComponent: () => import('./product-form/product-form').then(m => m.ProductFormComponent),
  },
];
