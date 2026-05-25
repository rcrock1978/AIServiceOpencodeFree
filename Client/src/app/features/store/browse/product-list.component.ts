import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, Subscription, debounceTime, distinctUntilChanged } from 'rxjs';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { ProductService } from '../../../services/product.service';
import { CartService } from '../../../services/cart.service';
import { Product } from '../../../core/models/product.model';
import { ProductCardComponent } from './product-card.component';
import { ProductFiltersComponent } from './product-filters.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    MatPaginatorModule, MatProgressSpinnerModule, MatSnackBarModule, MatIconModule,
    ProductCardComponent, ProductFiltersComponent,
  ],
  template: `
    <div class="store-container">
      <app-product-filters
        [brands]="brands()"
        [types]="types()"
        [selectedBrand]="selectedBrand()"
        [selectedType]="selectedType()"
        [sortBy]="sortBy()"
        [searchQuery]="searchQuery()"
        (brandChange)="onBrandChange($event)"
        (typeChange)="onTypeChange($event)"
        (sortChange)="onSortChange($event)"
        (searchChange)="onSearch($event)"
      />

      <div class="result-info">
        <span>{{ totalCount() }} product{{ totalCount() !== 1 ? 's' : '' }} found</span>
      </div>

      @if (loading()) {
        <div class="loading-container">
          <mat-spinner diameter="48" />
        </div>
      } @else if (products().length === 0) {
        <div class="empty-state">
          <mat-icon class="empty-icon">search_off</mat-icon>
          <p>No products found. Try adjusting your search or filters.</p>
        </div>
      } @else {
        <div class="product-grid">
          @for (product of products(); track product.id) {
            <app-product-card
              [product]="product"
              (addToCart)="addToCart($event)"
            />
          }
        </div>

        <mat-paginator
          [length]="totalCount()"
          [pageSize]="pageSize()"
          [pageIndex]="page() - 1"
          [pageSizeOptions]="[12, 24, 48]"
          (page)="onPageChange($event)"
          showFirstLastButtons
        />
      }
    </div>
  `,
  styles: [`
    .store-container {
      max-width: 1280px;
      margin: 0 auto;
      padding: 24px;
    }
    .result-info {
      font-size: 14px;
      color: #666;
      margin-bottom: 16px;
    }
    .loading-container {
      display: flex;
      justify-content: center;
      padding: 64px 0;
    }
    .empty-state {
      text-align: center;
      padding: 64px 24px;
      color: #666;
    }
    .empty-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      color: #bbb;
    }
    .product-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 24px;
      margin-bottom: 32px;
    }
    mat-paginator { margin-top: 16px; }
  `],
})
export class ProductListComponent implements OnInit, OnDestroy {
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  readonly products = signal<Product[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly searchQuery = signal('');
  readonly selectedBrand = signal('');
  readonly selectedType = signal('');
  readonly sortBy = signal('name');
  readonly page = signal(1);
  readonly pageSize = signal(12);
  readonly brands = signal<string[]>([]);
  readonly types = signal<string[]>([]);

  private readonly searchSubject = new Subject<string>();
  private searchSub?: Subscription;
  private readonly componentSubs: Subscription[] = [];

  ngOnInit(): void {
    this.loadBrands();
    this.loadTypes();
    this.loadProducts();

    this.searchSub = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
    ).subscribe(query => {
      this.searchQuery.set(query);
      this.page.set(1);
      this.loadProducts();
    });
    this.componentSubs.push(this.searchSub);
  }

  ngOnDestroy(): void {
    this.componentSubs.forEach(s => s.unsubscribe());
  }

  loadProducts(): void {
    this.loading.set(true);
    const params = {
      search: this.searchQuery() || undefined as string | undefined,
      page: this.page(),
      pageSize: this.pageSize(),
      brand: this.selectedBrand() || undefined as string | undefined,
      type: this.selectedType() || undefined as string | undefined,
      sortBy: this.sortBy(),
    };

    this.productService.getProducts(params).subscribe({
      next: (res) => {
        this.products.set(res.products);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  loadBrands(): void {
    this.productService.getBrands().subscribe({
      next: (b) => this.brands.set(b),
    });
  }

  loadTypes(): void {
    this.productService.getTypes().subscribe({
      next: (t) => this.types.set(t),
    });
  }

  onSearch(query: string): void {
    this.searchSubject.next(query);

    if (query.split(' ').length >= 3) {
      this.searchAI(query);
    }
  }

  onBrandChange(brand: string): void {
    this.selectedBrand.set(brand);
    this.page.set(1);
    this.loadProducts();
  }

  onTypeChange(type: string): void {
    this.selectedType.set(type);
    this.page.set(1);
    this.loadProducts();
  }

  onSortChange(sortBy: string): void {
    this.sortBy.set(sortBy);
    this.loadProducts();
  }

  onPageChange(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.loadProducts();
  }

  addToCart(product: Product): void {
    this.cartService.addItem(product.id, 1).subscribe({
      next: () => {
        this.snackBar.open(`${product.name} added to cart`, 'Dismiss', { duration: 3000 });
      },
    });
  }

  searchAI(query: string): void {
    this.productService.searchHybrid(query).subscribe({
      next: (res) => {
        if (res.results.length > 0) {
          this.products.set(res.results);
          this.totalCount.set(res.results.length);
        }
      },
    });
  }
}
