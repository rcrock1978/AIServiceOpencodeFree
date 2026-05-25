import { Component, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { CartService } from '../../../services/cart.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CurrencyPipe, FormsModule,
    MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule,
    MatDividerModule, MatSnackBarModule
  ],
  template: `
    <div class="checkout-page">
      <div class="container">
        <h1 class="page-title">Checkout</h1>

        <div class="checkout-layout">
          <div class="order-summary-col">
            <mat-card>
              <mat-card-header>
                <mat-card-title>Order Summary</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                @if (cart(); as c) {
                  @for (item of c.items; track item.id) {
                    <div class="order-item">
                      <span class="item-info">{{ item.product?.name || 'Product' }} <strong>&times;{{ item.quantity }}</strong></span>
                      <span class="item-total">{{ (item.product?.price ?? 0) * item.quantity | currency }}</span>
                    </div>
                  }
                }
                <mat-divider class="summary-divider"></mat-divider>
                <div class="subtotal-row">
                  <span>Subtotal ({{ itemCount() }} items)</span>
                  <span class="subtotal-value">{{ totalPrice() | currency }}</span>
                </div>
              </mat-card-content>
            </mat-card>
          </div>

          <div class="shipping-col">
            <mat-card>
              <mat-card-header>
                <mat-card-title>Shipping Address</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <form #checkoutForm="ngForm" (ngSubmit)="onSubmit()">
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Full Name</mat-label>
                    <input matInput [(ngModel)]="fullName" name="fullName" required />
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Address Line 1</mat-label>
                    <input matInput [(ngModel)]="addressLine1" name="addressLine1" required />
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Address Line 2 (optional)</mat-label>
                    <input matInput [(ngModel)]="addressLine2" name="addressLine2" />
                  </mat-form-field>

                  <div class="row-fields">
                    <mat-form-field appearance="outline">
                      <mat-label>City</mat-label>
                      <input matInput [(ngModel)]="city" name="city" required />
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>State</mat-label>
                      <input matInput [(ngModel)]="state" name="state" required />
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>ZIP Code</mat-label>
                      <input matInput [(ngModel)]="zipCode" name="zipCode" required />
                    </mat-form-field>
                  </div>

                  <button
                    mat-flat-button
                    color="primary"
                    class="place-order-btn"
                    type="submit"
                    [disabled]="submitted()"
                  >
                    @if (!submitted()) {
                      <span>Place Order</span>
                    } @else {
                      <span>Placing Order...</span>
                    }
                  </button>
                </form>
              </mat-card-content>
            </mat-card>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .checkout-page { padding: 32px 0; min-height: calc(100vh - 64px); }
    .page-title { font-size: 1.75rem; font-weight: 700; margin-bottom: 24px; }
    .checkout-layout { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; align-items: start; }
    .order-item {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      font-size: 0.95rem;
    }
    .item-info { color: #333; }
    .item-total { font-weight: 600; }
    .summary-divider { margin: 12px 0; }
    .subtotal-row {
      display: flex;
      justify-content: space-between;
      font-size: 1rem;
      font-weight: 600;
      padding-top: 4px;
    }
    .subtotal-value { color: #3f51b5; }
    .full-width { width: 100%; margin-bottom: 12px; }
    .row-fields { display: flex; gap: 12px; }
    .row-fields mat-form-field { flex: 1; }
    .place-order-btn {
      width: 100%;
      margin-top: 8px;
      padding: 8px 0;
      font-size: 1rem;
    }
    @media (max-width: 768px) {
      .checkout-layout { grid-template-columns: 1fr; }
      .row-fields { flex-direction: column; gap: 0; }
    }
  `]
})
export class CheckoutComponent {
  private cartService = inject(CartService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  readonly cart = this.cartService.cart;
  readonly itemCount = this.cartService.itemCount;
  readonly totalPrice = this.cartService.totalPrice;

  fullName = '';
  addressLine1 = '';
  addressLine2 = '';
  city = '';
  state = '';
  zipCode = '';

  submitted = signal(false);

  onSubmit(): void {
    if (!this.fullName || !this.addressLine1 || !this.city || !this.state || !this.zipCode) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }
    if (this.itemCount() === 0) {
      this.snackBar.open('Your cart is empty', 'Close', { duration: 3000 });
      return;
    }
    this.submitted.set(true);
    const address = [this.fullName, this.addressLine1, this.addressLine2, this.city, this.state, this.zipCode]
      .filter(Boolean)
      .join(', ');
    this.cartService.checkout(address).subscribe({
      next: () => {
        this.snackBar.open('Order placed successfully!', 'Close', { duration: 5000 });
        this.router.navigate(['/store/orders']);
      },
      error: () => {
        this.submitted.set(false);
        this.snackBar.open('Failed to place order', 'Close', { duration: 3000 });
      }
    });
  }
}
