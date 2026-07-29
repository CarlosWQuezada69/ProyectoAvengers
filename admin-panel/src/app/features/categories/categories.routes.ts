import { Routes } from '@angular/router';

export const categoryRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./category-list/category-list').then(m => m.CategoryListComponent),
  },
];
