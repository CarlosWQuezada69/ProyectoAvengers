import { Routes } from '@angular/router';

export const sliderRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./slider-list/slider-list').then(m => m.SliderListComponent),
  },
];
