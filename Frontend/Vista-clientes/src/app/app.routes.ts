import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home-page').then((m) => m.HomePageComponent)
  },
  {
    path: 'productos',
    loadComponent: () => import('./pages/catalog/catalog-page').then((m) => m.CatalogPageComponent)
  },
  {
    path: 'productos/:slug',
    loadComponent: () =>
      import('./pages/product-detail/product-detail-page').then((m) => m.ProductDetailPageComponent)
  },
  {
    path: 'acerca',
    loadComponent: () => import('./pages/about/about-page').then((m) => m.AboutPageComponent)
  },
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found-page').then((m) => m.NotFoundPageComponent)
  }
];