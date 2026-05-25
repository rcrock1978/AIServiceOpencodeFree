import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-product-filters',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatIconModule],
  template: `
    <div class="filters-row">
      <mat-form-field appearance="outline" class="search-field">
        <mat-icon matPrefix>search</mat-icon>
        <input matInput [ngModel]="searchQuery()" (ngModelChange)="onSearchChange($event)"
               placeholder="Search products..." />
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Brand</mat-label>
        <mat-select [value]="selectedBrand()" (valueChange)="onBrandChange($event)">
          <mat-option value="">All Brands</mat-option>
          @for (brand of brands(); track brand) {
            <mat-option [value]="brand">{{ brand }}</mat-option>
          }
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Type</mat-label>
        <mat-select [value]="selectedType()" (valueChange)="onTypeChange($event)">
          <mat-option value="">All Types</mat-option>
          @for (type of types(); track type) {
            <mat-option [value]="type">{{ type }}</mat-option>
          }
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Sort By</mat-label>
        <mat-select [value]="sortBy()" (valueChange)="onSortChange($event)">
          <mat-option value="name">Name</mat-option>
          <mat-option value="price">Price</mat-option>
          <mat-option value="rating">Rating</mat-option>
        </mat-select>
      </mat-form-field>
    </div>
  `,
  styles: [`
    .filters-row {
      display: flex;
      gap: 16px;
      align-items: flex-start;
      flex-wrap: wrap;
      margin-bottom: 24px;
    }
    .search-field { flex: 1; min-width: 240px; }
    .filter-field { min-width: 160px; }
  `],
})
export class ProductFiltersComponent {
  readonly brands = input<string[]>([]);
  readonly types = input<string[]>([]);
  readonly selectedBrand = input<string>('');
  readonly selectedType = input<string>('');
  readonly sortBy = input<string>('name');
  readonly searchQuery = input<string>('');

  readonly brandChange = output<string>();
  readonly typeChange = output<string>();
  readonly sortChange = output<string>();
  readonly searchChange = output<string>();

  onBrandChange(value: string): void { this.brandChange.emit(value); }
  onTypeChange(value: string): void { this.typeChange.emit(value); }
  onSortChange(value: string): void { this.sortChange.emit(value); }
  onSearchChange(value: string): void { this.searchChange.emit(value); }
}
