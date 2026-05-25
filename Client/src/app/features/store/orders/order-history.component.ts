import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Order } from '../../../core/models/cart.model';
import { CurrencyPipe, DatePipe, SlicePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { CartService } from '../../../services/cart.service';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [
    CurrencyPipe, DatePipe, SlicePipe, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule, MatDividerModule,
    MatChipsModule, MatExpansionModule, MatSnackBarModule
  ],
  template: `
    <div class="orders-page">
      <div class="container">
        <h1 class="page-title">My Orders</h1>

        @if (orders().length === 0) {
          <div class="empty-orders">
            <mat-icon class="empty-icon">receipt_long</mat-icon>
            <h2>No orders yet</h2>
            <p>Your order history will appear here.</p>
            <button mat-flat-button color="primary" routerLink="/store">Start Shopping</button>
          </div>
        } @else {
          <div class="orders-list">
            @for (order of orders(); track order.id) {
              <mat-card class="order-card">
                <mat-card-content>
                  <div class="order-header">
                    <div class="order-info">
                      <span class="order-id">Order #{{ order.id | slice:0:8 }}...</span>
                      <span class="order-date">{{ order.createdAt | date:'mediumDate' }}</span>
                    </div>
                    <div class="order-status">
                      <mat-chip [class]="getStatusColor(order.status)">
                        <mat-icon class="status-icon">{{ getStatusIcon(order.status) }}</mat-icon>
                        {{ order.status }}
                      </mat-chip>
                    </div>
                  </div>

                  <div class="order-meta">
                    <span>{{ order.items.length }} item(s)</span>
                    <span class="order-total">{{ order.totalAmount | currency }}</span>
                  </div>

                  <mat-divider class="order-divider"></mat-divider>

                  <mat-accordion>
                    <mat-expansion-panel hideToggle>
                      <mat-expansion-panel-header>
                        <mat-panel-title>View Items</mat-panel-title>
                      </mat-expansion-panel-header>
                      @for (item of order.items; track item.id) {
                        <div class="order-item">
                          <span class="item-name">{{ item.product?.name || 'Product' }} <strong>&times;{{ item.quantity }}</strong></span>
                          <span class="item-price">{{ item.unitPrice | currency }}</span>
                          <span class="item-line-total">{{ item.unitPrice * item.quantity | currency }}</span>
                        </div>
                      }
                    </mat-expansion-panel>
                  </mat-accordion>
                </mat-card-content>
              </mat-card>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .orders-page { padding: 32px 0; min-height: calc(100vh - 64px); }
    .page-title { font-size: 1.75rem; font-weight: 700; margin-bottom: 24px; }
    .empty-orders {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 80px 0;
      text-align: center;
    }
    .empty-icon { font-size: 64px; width: 64px; height: 64px; color: #bbb; margin-bottom: 16px; }
    .empty-orders h2 { font-size: 1.5rem; margin-bottom: 8px; }
    .empty-orders p { color: #666; margin-bottom: 24px; }
    .orders-list { display: flex; flex-direction: column; gap: 16px; max-width: 800px; margin: 0 auto; }
    .order-card { border-radius: 12px; }
    .order-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
    }
    .order-info { display: flex; flex-direction: column; gap: 4px; }
    .order-id { font-weight: 600; font-size: 0.95rem; }
    .order-date { font-size: 0.85rem; color: #666; }
    .status-icon { font-size: 16px; width: 16px; height: 16px; margin-right: 4px; }
    .order-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.95rem;
      color: #555;
      margin-bottom: 4px;
    }
    .order-total { font-weight: 700; font-size: 1.1rem; color: #1a1a2e; }
    .order-divider { margin: 12px 0; }
    .order-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 8px 0;
      font-size: 0.9rem;
    }
    .item-name { flex: 1; }
    .item-price { color: #666; margin: 0 16px; min-width: 60px; text-align: right; }
    .item-line-total { font-weight: 600; min-width: 70px; text-align: right; }

    :host ::ng-deep .mat-mdc-chip.mat-accent { background: #fff3e0; color: #e65100; }
    :host ::ng-deep .pending { background: #fff3e0 !important; color: #e65100 !important; }
    :host ::ng-deep .confirmed { background: #e3f2fd !important; color: #1565c0 !important; }
    :host ::ng-deep .shipped { background: #fff3e0 !important; color: #ef6c00 !important; }
    :host ::ng-deep .delivered { background: #e8f5e9 !important; color: #2e7d32 !important; }
    :host ::ng-deep .cancelled { background: #fce4ec !important; color: #c62828 !important; }
  `]
})
export class OrderHistoryComponent implements OnInit {
  private cartService = inject(CartService);
  private snackBar = inject(MatSnackBar);

  readonly orders = signal<Order[]>([]);

  ngOnInit(): void {
    this.cartService.getOrders().subscribe({
      next: (response) => this.orders.set(response.orders ?? []),
      error: () => this.snackBar.open('Failed to load orders', 'Close', { duration: 3000 })
    });
  }

  getStatusColor(status: string): string {
    const map: Record<string, string> = {
      pending: 'pending',
      confirmed: 'confirmed',
      shipped: 'shipped',
      delivered: 'delivered',
      cancelled: 'cancelled',
    };
    return map[status.toLowerCase()] || '';
  }

  getStatusIcon(status: string): string {
    const map: Record<string, string> = {
      pending: 'schedule',
      confirmed: 'check_circle',
      shipped: 'local_shipping',
      delivered: 'verified',
      cancelled: 'cancel',
    };
    return map[status.toLowerCase()] || 'help';
  }
}
