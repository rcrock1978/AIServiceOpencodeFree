import { Component, input, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatRippleModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Product } from '../models/product.model';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [MatRippleModule, MatIconModule, MatButtonModule],
  template: `
    <div class="product-card" matRipple>
      <div class="image-placeholder" [style.background]="'#' + product().id.slice(0, 6)">
        <span class="initial">{{ product().name[0] }}</span>
      </div>
      <div class="info">
        <div class="name">{{ product().name }}</div>
        <div class="brand">{{ product().brand }}</div>
        <div class="price">\${{ product().price.toFixed(2) }}</div>
      </div>
      <div class="actions">
        <button mat-icon-button matTooltip="View product" (click)="viewProduct()">
          <mat-icon>visibility</mat-icon>
        </button>
        <button mat-icon-button matTooltip="Add to cart" (click)="addToCart()">
          <mat-icon>add_shopping_cart</mat-icon>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .product-card {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px;
      border-radius: 8px;
      background: #fff;
      border: 1px solid #e0e0e0;
      cursor: pointer;
      transition: box-shadow 0.2s;

      &:hover {
        box-shadow: 0 2px 8px rgba(0,0,0,0.12);
      }
    }

    .image-placeholder {
      width: 48px;
      height: 48px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .initial {
      color: #fff;
      font-size: 20px;
      font-weight: 600;
    }

    .info {
      flex: 1;
      min-width: 0;
    }

    .name {
      font-size: 13px;
      font-weight: 500;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .brand {
      font-size: 11px;
      color: #666;
    }

    .price {
      font-size: 13px;
      font-weight: 600;
      color: #7c4dff;
    }

    .actions {
      display: flex;
      gap: 4px;
    }

    ::ng-deep .mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      line-height: 18px;
    }
  `]
})
export class ProductCardComponent {
  readonly product = input.required<Product>();

  private readonly router = inject(Router);
  private readonly cartService = inject(CartService);

  viewProduct(): void {
    this.router.navigate(['/store', this.product().id]);
  }

  addToCart(): void {
    this.cartService.addItem(this.product().id, 1).subscribe();
  }
}
