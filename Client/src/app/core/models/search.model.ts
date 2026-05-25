import { Product } from './product.model';

export interface SearchRequest {
  query: string;
}

export interface VectorSearchResponse {
  results: Product[];
  query: string;
}
