import { Component, OnInit, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { CartService } from '../../../services/cart.service';
import { CartItemComponent } from './cart-item.component';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    CurrencyPipe, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule, MatDividerModule, MatSnackBarModule,
    CartItemComponent
  ],
  template: `
    <div class="cart-page">
      <div class="container">
        <h1 class="page-title">Shopping Cart @if (itemCount() > 0) {<span class="count-badge">{{ itemCount() }}</span>}</h1>

        @if (itemCount() === 0) {
          <div class="empty-cart">
            <mat-icon class="empty-icon">shopping_cart</mat-icon>
            <h2>Your cart is empty</h2>
            <p>Looks like you haven't added anything yet.</p>
            <button mat-flat-button color="primary" routerLink="/store">Continue Shopping</button>
          </div>
        } @else {
          <div class="cart-layout">
            <div class="cart-items">
              <mat-card>
                <mat-card-content>
                  @if (cart(); as c) {
                    @for (item of c.items; track item.id; let last = $last) {
                      <app-cart-item
                        [item]="item"
                        (quantityChange)="updateQuantity(item, $event)"
                        (remove)="removeItem(item)"
                      />
                      @if (!last) { <mat-divider></mat-divider> }
                    }
                  }
                </mat-card-content>
              </mat-card>
            </div>

            <div class="order-summary">
              <mat-card>
                <mat-card-header>
                  <mat-card-title>Order Summary</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="summary-row">
                    <span>Subtotal ({{ itemCount() }} items)</span>
                    <span class="summary-value">{{ totalPrice() | currency }}</span>
                  </div>
                  <mat-divider class="summary-divider"></mat-divider>
                  <div class="summary-row total">
                    <span>Total</span>
                    <span class="summary-value total-value">{{ totalPrice() | currency }}</span>
                  </div>
                  <button
                    mat-flat-button
                    color="primary"
                    class="checkout-btn"
                    [disabled]="itemCount() === 0"
                    (click)="checkout()"
                  >
                    Proceed to Checkout
                  </button>
                  <a mat-button routerLink="/store" class="continue-link">Continue Shopping</a>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .cart-page { padding: 32px 0; min-height: calc(100vh - 64px); }
    .page-title {
      font-size: 1.75rem;
      font-weight: 700;
      margin-bottom: 24px;
      display: flex;
      align-items: center;
      gap: 12px;
    }
    .count-badge {
      font-size: 0.85rem;
      background: #3f51b5;
      color: #fff;
      border-radius: 12px;
      padding: 2px 10px;
      font-weight: 600;
    }
    .empty-cart {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 80px 0;
      text-align: center;
    }
    .empty-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #bbb;
      margin-bottom: 16px;
    }
    .empty-cart h2 { font-size: 1.5rem; margin-bottom: 8px; }
    .empty-cart p { color: #666; margin-bottom: 24px; }
    .cart-layout { display: grid; grid-template-columns: 1fr 360px; gap: 24px; align-items: start; }
    .cart-items mat-card { padding: 0 16px; }
    .order-summary { position: sticky; top: 88px; }
    .order-summary mat-card-header { padding-bottom: 0; }
    .summary-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 8px 0;
      font-size: 0.95rem;
    }
    .summary-value { font-weight: 600; }
    .summary-divider { margin: 8px 0; }
    .total { padding-top: 12px; }
    .total-value { font-size: 1.2rem; color: #3f51b5; }
    .checkout-btn {
      width: 100%;
      margin-top: 20px;
      padding: 8px 0;
      font-size: 1rem;
    }
    .continue-link {
      display: block;
      text-align: center;
      margin-top: 12px;
      width: 100%;
    }
    @media (max-width: 768px) {
      .cart-layout { grid-template-columns: 1fr; }
    }
  `]
})
export class CartComponent implements OnInit {
  private cartService = inject(CartService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  readonly cart = this.cartService.cart;
  readonly itemCount = this.cartService.itemCount;
  readonly totalPrice = this.cartService.totalPrice;

  ngOnInit(): void {
    this.cartService.getCart().subscribe({
      error: () => this.snackBar.open('Failed to load cart', 'Close', { duration: 3000 })
    });
  }

  updateQuantity(item: any, newQty: number): void {
    if (newQty < 1) return;
    this.cartService.addItem(item.productId, newQty).subscribe({
      error: () => this.snackBar.open('Failed to update quantity', 'Close', { duration: 3000 })
    });
  }

  removeItem(item: any): void {
    this.cartService.removeItem(item.id).subscribe({
      error: () => this.snackBar.open('Failed to remove item', 'Close', { duration: 3000 })
    });
  }

  checkout(): void {
    this.router.navigate(['/store/checkout']);
  }
}
