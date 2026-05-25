import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CurrencyPipe } from '@angular/common';
import { BrandColorPipe } from '../../../shared/pipes/brand-color.pipe';
import { Product } from '../../../core/models/product.model';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [
    RouterLink, CurrencyPipe, BrandColorPipe,
    MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatTooltipModule,
  ],
  template: `
    <mat-card class="product-card" [routerLink]="['/store', product().id]">
      <div class="image-placeholder" [style.background]="product().brand | brandColor">
        <span class="brand-initial">{{ product().brand.charAt(0) }}</span>
      </div>

      <mat-card-content>
        <div class="product-brand">{{ product().brand }}</div>
        <div class="product-name">{{ product().name }}</div>

        <div class="product-rating">
          @for (star of [1,2,3,4,5]; track star) {
            <mat-icon class="star {{ star <= product().rating ? 'filled' : 'empty' }}">
              {{ star <= product().rating ? 'star' : 'star_border' }}
            </mat-icon>
          }
          <span class="rating-value">{{ product().rating }}</span>
        </div>

        <div class="product-price">{{ product().price | currency }}</div>

        <mat-chip-set>
          <mat-chip [class.in-stock]="product().stock > 0" [class.out-of-stock]="product().stock === 0">
            {{ product().stock > 0 ? 'In Stock' : 'Out of Stock' }}
          </mat-chip>
        </mat-chip-set>
      </mat-card-content>

      <mat-card-actions>
        <button mat-raised-button color="primary" class="add-to-cart-btn"
                (click)="onAddToCart($event)"
                [disabled]="product().stock === 0"
                matTooltip="Add to cart">
          <mat-icon>shopping_cart</mat-icon>
          Add to Cart
        </button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .product-card {
      display: flex;
      flex-direction: column;
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
      border-radius: 12px;
      height: 100%;
    }
    .product-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0,0,0,0.12);
    }
    .image-placeholder {
      height: 180px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 12px 12px 0 0;
    }
    .brand-initial {
      font-size: 48px;
      font-weight: 700;
      color: rgba(255,255,255,0.8);
      text-transform: uppercase;
    }
    .product-brand {
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 1px;
      color: #666;
      margin-top: 12px;
    }
    .product-name {
      font-size: 16px;
      font-weight: 500;
      margin: 4px 0 8px;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .product-rating {
      display: flex;
      align-items: center;
      gap: 2px;
      margin-bottom: 8px;
    }
    .star { font-size: 18px; width: 18px; height: 18px; }
    .star.filled { color: #ffc107; }
    .star.empty { color: #e0e0e0; }
    .rating-value {
      font-size: 13px;
      color: #666;
      margin-left: 4px;
    }
    .product-price {
      font-size: 22px;
      font-weight: 700;
      color: #1976d2;
      margin-bottom: 8px;
    }
    mat-chip.in-stock { background: #e8f5e9; color: #2e7d32; }
    mat-chip.out-of-stock { background: #ffebee; color: #c62828; }
    .add-to-cart-btn {
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
    }
    mat-card-actions { padding: 8px 16px 16px; margin: 0; }
  `],
})
export class ProductCardComponent {
  readonly product = input.required<Product>();
  readonly addToCart = output<Product>();

  onAddToCart(event: MouseEvent): void {
    event.stopPropagation();
    this.addToCart.emit(this.product());
  }
}
