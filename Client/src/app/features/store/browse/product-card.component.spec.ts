import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ProductCardComponent } from './product-card.component';
import { Product } from '../../../core/models/product.model';

describe('ProductCardComponent', () => {
  let component: ProductCardComponent;
  let fixture: ComponentFixture<ProductCardComponent>;

  const mockProduct: Product = {
    id: '1',
    name: 'Running Shoes',
    description: 'Comfortable running shoes',
    price: 89.99,
    brand: 'Nike',
    type: 'shoes',
    imageUrl: '',
    rating: 4.5,
    stock: 10,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductCardComponent],
      providers: [provideRouter([]), provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductCardComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('product', mockProduct);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display product info', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Running Shoes');
    expect(compiled.textContent).toContain('Nike');
    expect(compiled.textContent).toContain('$89.99');
    expect(compiled.textContent).toContain('In Stock');
  });

  it('should show Out of Stock when stock is 0', () => {
    const outOfStock = { ...mockProduct, stock: 0 };
    fixture.componentRef.setInput('product', outOfStock);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Out of Stock');
  });

  it('should emit addToCart on button click', () => {
    let emittedProduct: Product | undefined;
    component.addToCart.subscribe(p => emittedProduct = p);

    const btn = fixture.nativeElement.querySelector('.add-to-cart-btn') as HTMLButtonElement;
    btn.click();

    expect(emittedProduct).toBeDefined();
    expect(emittedProduct!.name).toBe('Running Shoes');
  });
});
