import { Routes } from '@angular/router';

export const storeRoutes: Routes = [
  { path: '', loadComponent: () => import('./browse/product-list.component').then(m => m.ProductListComponent) },
  { path: 'cart', loadComponent: () => import('./cart/cart.component').then(m => m.CartComponent) },
  { path: 'checkout', loadComponent: () => import('./checkout/checkout.component').then(m => m.CheckoutComponent) },
  { path: 'orders', loadComponent: () => import('./orders/order-history.component').then(m => m.OrderHistoryComponent) },
  { path: ':id', loadComponent: () => import('./browse/product-detail.component').then(m => m.ProductDetailComponent) },
];
