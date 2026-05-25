import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cart, Order, AddToCartRequest, CheckoutRequest, CheckoutResponse } from '../core/models/cart.model';

@Injectable({ providedIn: 'root' })
export class CartService {
  private http = inject(HttpClient);
  private cartSignal = signal<Cart | null>(null);

  readonly cart = this.cartSignal.asReadonly();
  readonly itemCount = computed(() =>
    this.cartSignal()?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0
  );
  readonly totalPrice = computed(() =>
    this.cartSignal()?.items.reduce((sum, item) => sum + (item.product?.price ?? 0) * item.quantity, 0) ?? 0
  );

  getCart(): Observable<Cart> {
    return this.http.get<Cart>('/api/cart');
  }

  setCart(cart: Cart): void {
    this.cartSignal.set(cart);
  }

  addItem(productId: string, quantity: number): Observable<Cart> {
    const body: AddToCartRequest = { productId, quantity };
    return this.http.post<Cart>('/api/cart/add', body);
  }

  removeItem(productId: string): Observable<void> {
    return this.http.delete<void>('/api/cart/remove', { params: { productId } });
  }

  checkout(shippingAddress: string): Observable<CheckoutResponse> {
    const body: CheckoutRequest = { shippingAddress };
    return this.http.post<CheckoutResponse>('/api/cart/checkout', body);
  }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>('/api/orders');
  }
}
