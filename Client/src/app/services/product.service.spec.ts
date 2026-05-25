import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch products with params', () => {
    const mockProducts = {
      products: [
        { id: '1', name: 'Test Product', price: 29.99, brand: 'Test', type: 'shirt', rating: 4.5, stock: 10 },
      ],
      totalCount: 1,
    };

    service.getProducts({ brand: 'Test', page: 1, pageSize: 10 }).subscribe(res => {
      expect(res.products.length).toBe(1);
      expect(res.totalCount).toBe(1);
    });

    const req = httpMock.expectOne(r => r.url === '/catalog/products' && r.params.has('brand'));
    expect(req.request.method).toBe('GET');
    req.flush(mockProducts);
  });

  it('should fetch single product', () => {
    const mockProduct = { id: '1', name: 'Product', price: 9.99, brand: 'A', type: 'hat', rating: 3, stock: 5 };

    service.getProduct('1').subscribe(p => {
      expect(p.name).toBe('Product');
    });

    const req = httpMock.expectOne('/catalog/products/1');
    expect(req.request.method).toBe('GET');
    req.flush(mockProduct);
  });

  it('should fetch brands', () => {
    service.getBrands().subscribe(b => {
      expect(b.length).toBe(2);
    });

    const req = httpMock.expectOne('/catalog/brands');
    req.flush(['Nike', 'Adidas']);
  });

  it('should fetch types', () => {
    service.getTypes().subscribe(t => {
      expect(t).toContain('shirt');
    });

    const req = httpMock.expectOne('/catalog/types');
    req.flush(['shirt', 'shoes']);
  });
});
