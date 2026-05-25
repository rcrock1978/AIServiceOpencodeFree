import { Component, input, output } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CartItem } from '../../../core/models/cart.model';

@Component({
  selector: 'app-cart-item',
  standalone: true,
  imports: [CurrencyPipe, MatIconModule, MatButtonModule],
  template: `
    <div class="cart-item">
      <div class="item-image" [class]="'bg-' + (item().product?.type?.toLowerCase() || 'primary')">
        <mat-icon>shopping_bag</mat-icon>
      </div>
      <div class="item-details">
        <h3 class="item-name">{{ item().product?.name || 'Product' }}</h3>
        @if (item().product; as prod) {
          <p class="item-meta">
            {{ prod.brand }} &middot; {{ prod.type }}
          </p>
        }
        <p class="item-price">{{ item().product?.price | currency }}</p>
      </div>
      <div class="item-quantity">
        <button mat-icon-button (click)="onDecrement()" [disabled]="item().quantity <= 1">
          <mat-icon>remove</mat-icon>
        </button>
        <span class="qty-value">{{ item().quantity }}</span>
        <button mat-icon-button (click)="onIncrement()">
          <mat-icon>add</mat-icon>
        </button>
      </div>
      <div class="item-total">
        <p>{{ (item().product?.price ?? 0) * item().quantity | currency }}</p>
      </div>
      <button mat-icon-button class="remove-btn" (click)="onRemove()" aria-label="Remove item">
        <mat-icon color="warn">delete</mat-icon>
      </button>
    </div>
  `,
  styles: [`
    .cart-item {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px 0;
    }
    .item-image {
      width: 80px;
      height: 80px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .item-image mat-icon { font-size: 32px; width: 32px; height: 32px; color: #fff; }
    .bg-primary { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
    .bg-electronics { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); }
    .bg-clothing { background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); }
    .bg-books { background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%); }
    .bg-home { background: linear-gradient(135deg, #fa709a 0%, #fee140 100%); }
    .item-details { flex: 1; min-width: 0; }
    .item-name { font-size: 1rem; font-weight: 600; margin: 0 0 4px; }
    .item-meta { font-size: 0.85rem; color: #666; margin: 0 0 4px; }
    .item-price { font-size: 0.9rem; font-weight: 500; color: #3f51b5; margin: 0; }
    .item-quantity {
      display: flex;
      align-items: center;
      gap: 4px;
    }
    .qty-value {
      min-width: 24px;
      text-align: center;
      font-weight: 600;
      font-size: 1rem;
    }
    .item-total {
      min-width: 80px;
      text-align: right;
    }
    .item-total p { font-size: 1rem; font-weight: 600; margin: 0; color: #1a1a2e; }
    .remove-btn { flex-shrink: 0; }
  `]
})
export class CartItemComponent {
  readonly item = input.required<CartItem>();
  readonly quantityChange = output<number>();
  readonly remove = output<void>();

  onDecrement(): void {
    this.quantityChange.emit(this.item().quantity - 1);
  }

  onIncrement(): void {
    this.quantityChange.emit(this.item().quantity + 1);
  }

  onRemove(): void {
    this.remove.emit();
  }
}
