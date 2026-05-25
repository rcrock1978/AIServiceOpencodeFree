import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProductService } from '../../../services/product.service';
import { CartService } from '../../../services/cart.service';
import { Product } from '../../../core/models/product.model';
import { BrandColorPipe } from '../../../shared/pipes/brand-color.pipe';
import { ProductCardComponent } from './product-card.component';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CurrencyPipe, BrandColorPipe,
    MatCardModule, MatButtonModule, MatIconModule, MatChipsModule,
    MatDividerModule, MatProgressSpinnerModule, MatSnackBarModule, MatTooltipModule,
    ProductCardComponent,
  ],
  template: `
    <div class="detail-container">
      <button mat-button (click)="goBack()" class="back-btn">
        <mat-icon>arrow_back</mat-icon> Back to Store
      </button>

      @if (product(); as p) {
        <div class="detail-layout">
          <div class="image-section">
            <div class="product-image" [style.background]="p.brand | brandColor">
              <span class="brand-initial">{{ p.brand.charAt(0) }}</span>
            </div>
          </div>

          <div class="info-section">
            <div class="product-brand">{{ p.brand }}</div>
            <h1 class="product-name">{{ p.name }}</h1>

            <div class="badges">
              <mat-chip-set>
                <mat-chip class="brand-chip">{{ p.brand }}</mat-chip>
                <mat-chip class="type-chip">{{ p.type }}</mat-chip>
              </mat-chip-set>
            </div>

            <div class="product-rating">
              @for (star of [1,2,3,4,5]; track star) {
                <mat-icon class="star {{ star <= p.rating ? 'filled' : 'empty' }}">
                  {{ star <= p.rating ? 'star' : 'star_border' }}
                </mat-icon>
              }
              <span class="rating-value">{{ p.rating }} / 5</span>
            </div>

            <div class="product-price">{{ p.price | currency }}</div>

            <mat-divider />

            <p class="product-description">{{ p.description }}</p>

            <div class="stock-status">
              <mat-chip [class.in-stock]="p.stock > 0" [class.out-of-stock]="p.stock === 0">
                {{ p.stock > 0 ? 'In Stock' : 'Out of Stock' }}
              </mat-chip>
              @if (p.stock > 0) {
                <span class="stock-count">{{ p.stock }} available</span>
              }
            </div>

            <div class="add-to-cart-section">
              <div class="quantity-selector">
                <button mat-icon-button (click)="decrementQuantity()" [disabled]="quantity() <= 1" matTooltip="Decrease">
                  <mat-icon>remove</mat-icon>
                </button>
                <span class="quantity-value">{{ quantity() }}</span>
                <button mat-icon-button (click)="incrementQuantity()" [disabled]="quantity() >= p.stock" matTooltip="Increase">
                  <mat-icon>add</mat-icon>
                </button>
              </div>

              <button mat-raised-button color="primary" class="add-to-cart-btn"
                      (click)="addToCart()"
                      [disabled]="p.stock === 0">
                <mat-icon>shopping_cart</mat-icon>
                Add to Cart — {{ p.price * quantity() | currency }}
              </button>
            </div>
          </div>
        </div>
      } @else if (loading()) {
        <div class="loading-container">
          <mat-spinner diameter="48" />
        </div>
      }
      @if (similarProducts().length > 0) {
        <div class="similar-section">
          <h2>Similar Products</h2>
          <div class="similar-grid">
            @for (sp of similarProducts(); track sp.id) {
              <app-product-card [product]="sp" (addToCart)="addSimilarToCart($event)" />
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .detail-container {
      max-width: 1100px;
      margin: 0 auto;
      padding: 24px;
    }
    .back-btn { margin-bottom: 24px; }
    .loading-container {
      display: flex;
      justify-content: center;
      padding: 64px 0;
    }
    .detail-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 48px;
      margin-bottom: 32px;
    }
    .product-image {
      height: 400px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 16px;
    }
    .brand-initial {
      font-size: 80px;
      font-weight: 700;
      color: rgba(255,255,255,0.8);
      text-transform: uppercase;
    }
    .product-brand {
      font-size: 14px;
      text-transform: uppercase;
      letter-spacing: 1.5px;
      color: #666;
      margin-bottom: 8px;
    }
    .product-name {
      font-size: 28px;
      font-weight: 600;
      margin: 0 0 12px;
    }
    .badges { margin-bottom: 16px; }
    .brand-chip { background: #e3f2fd; color: #1565c0; }
    .type-chip { background: #f3e5f5; color: #7b1fa2; }
    .product-rating {
      display: flex;
      align-items: center;
      gap: 4px;
      margin-bottom: 16px;
    }
    .star { font-size: 22px; width: 22px; height: 22px; }
    .star.filled { color: #ffc107; }
    .star.empty { color: #e0e0e0; }
    .rating-value {
      font-size: 15px;
      color: #666;
      margin-left: 8px;
    }
    .product-price {
      font-size: 32px;
      font-weight: 700;
      color: #1976d2;
      margin-bottom: 16px;
    }
    .product-description {
      font-size: 15px;
      line-height: 1.7;
      color: #444;
      margin: 16px 0;
    }
    .stock-status {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 24px;
    }
    .stock-count { font-size: 14px; color: #666; }
    mat-chip.in-stock { background: #e8f5e9; color: #2e7d32; }
    mat-chip.out-of-stock { background: #ffebee; color: #c62828; }
    .add-to-cart-section {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    .quantity-selector {
      display: flex;
      align-items: center;
      gap: 8px;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      padding: 4px;
    }
    .quantity-value {
      font-size: 18px;
      font-weight: 600;
      min-width: 32px;
      text-align: center;
    }
    .add-to-cart-btn {
      flex: 1;
      padding: 8px 24px;
      font-size: 16px;
    }
    .section-divider { margin: 32px 0; }
    .similar-section h2 {
      font-size: 22px;
      font-weight: 600;
      margin-bottom: 20px;
    }
    .similar-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
      gap: 20px;
    }
    @media (max-width: 768px) {
      .detail-layout { grid-template-columns: 1fr; gap: 24px; }
      .product-image { height: 280px; }
    }
  `],
})
export class ProductDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly snackBar = inject(MatSnackBar);

  readonly product = signal<Product | null>(null);
  readonly loading = signal(true);
  readonly quantity = signal(1);
  readonly similarProducts = signal<Product[]>([]);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProduct(id);
    }
  }

  private loadProduct(id: string): void {
    this.loading.set(true);
    this.productService.getProduct(id).subscribe({
      next: (p) => {
        this.product.set(p);
        this.loading.set(false);
        this.loadSimilarProducts(p);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/store']);
      },
    });
  }

  private loadSimilarProducts(product: Product): void {
    this.productService.getProducts({
      search: product.type,
      pageSize: 4,
    }).subscribe({
      next: (res) => {
        this.similarProducts.set(
          res.products.filter(p => p.id !== product.id).slice(0, 4)
        );
      },
    });
  }

  goBack(): void {
    window.history.length > 1
      ? window.history.back()
      : this.router.navigate(['/store']);
  }

  incrementQuantity(): void {
    this.quantity.update(q => q + 1);
  }

  decrementQuantity(): void {
    this.quantity.update(q => Math.max(1, q - 1));
  }

  addToCart(): void {
    const p = this.product();
    if (!p) return;
    this.cartService.addItem(p.id, this.quantity()).subscribe({
      next: () => {
        this.snackBar.open(`${p.name} added to cart`, 'Dismiss', { duration: 3000 });
      },
    });
  }

  addSimilarToCart(product: Product): void {
    this.cartService.addItem(product.id, 1).subscribe({
      next: () => {
        this.snackBar.open(`${product.name} added to cart`, 'Dismiss', { duration: 3000 });
      },
    });
  }
}
