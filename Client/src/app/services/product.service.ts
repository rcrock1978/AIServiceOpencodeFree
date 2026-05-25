import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../core/models/product.model';
import { VectorSearchResponse } from '../core/models/search.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);

  getProducts(params?: {
    brand?: string;
    type?: string;
    search?: string;
    sortBy?: string;
    page?: number;
    pageSize?: number;
  }): Observable<{ products: Product[]; totalCount: number }> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          httpParams = httpParams.set(key, String(value));
        }
      });
    }
    return this.http.get<{ products: Product[]; totalCount: number }>('/catalog/products', { params: httpParams });
  }

  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`/catalog/products/${id}`);
  }

  getBrands(): Observable<string[]> {
    return this.http.get<string[]>('/catalog/brands');
  }

  getTypes(): Observable<string[]> {
    return this.http.get<string[]>('/catalog/types');
  }

  searchSemantic(query: string): Observable<VectorSearchResponse> {
    return this.http.post<VectorSearchResponse>('/api/search/vector', { query });
  }

  searchHybrid(query: string): Observable<VectorSearchResponse> {
    return this.http.post<VectorSearchResponse>('/api/search/hybrid', { query });
  }
}
