import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CartService } from './cart.service';
import { Cart, Order, CheckoutResponse, CartResponse, OrderHistoryResponse } from '../core/models/cart.model';

describe('CartService', () => {
  let service: CartService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(CartService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch cart and update signal', () => {
    const mockResponse: CartResponse = {
      cart: {
        id: 'cart-1',
        userId: 'user-1',
        items: [
          { id: 'item-1', cartId: 'cart-1', productId: 'p-1', quantity: 2, product: { id: 'p-1', name: 'Shoe', price: 49.99, description: '', brand: 'Nike', type: 'shoes', imageUrl: '', rating: 4, stock: 5, createdAt: '', updatedAt: '' }, createdAt: '' },
        ],
        createdAt: '',
        updatedAt: '',
      },
    };

    service.getCart().subscribe(response => {
      expect(response.cart.items.length).toBe(1);
      service.setCart(response.cart);
    });

    const req = httpMock.expectOne('/api/cart');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should compute itemCount and totalPrice', () => {
    const cart: Cart = {
      id: 'cart-1',
      userId: 'user-1',
      items: [
        { id: 'i1', cartId: 'cart-1', productId: 'p1', quantity: 2, product: { id: 'p1', name: 'A', price: 10, description: '', brand: '', type: '', imageUrl: '', rating: 3, stock: 1, createdAt: '', updatedAt: '' }, createdAt: '' },
        { id: 'i2', cartId: 'cart-1', productId: 'p2', quantity: 1, product: { id: 'p2', name: 'B', price: 20, description: '', brand: '', type: '', imageUrl: '', rating: 4, stock: 2, createdAt: '', updatedAt: '' }, createdAt: '' },
      ],
      createdAt: '',
      updatedAt: '',
    };

    service.setCart(cart);

    expect(service.itemCount()).toBe(3);
    expect(service.totalPrice()).toBe(40);
  });

  it('should add item', () => {
    const mockResponse: CartResponse = {
      cart: { id: 'cart-1', userId: 'user-1', items: [], createdAt: '', updatedAt: '' },
    };
    service.addItem('p-1', 1).subscribe(response => {
      expect(response.cart.id).toBe('cart-1');
    });

    const req = httpMock.expectOne('/api/cart/add');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ productId: 'p-1', quantity: 1 });
    req.flush(mockResponse);
  });

  it('should checkout', () => {
    const mockResponse: CheckoutResponse = { orderId: 'order-1', status: 'confirmed', totalAmount: 49.99 };

    service.checkout('123 Main St').subscribe(res => {
      expect(res.orderId).toBe('order-1');
    });

    const req = httpMock.expectOne('/api/cart/checkout');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ shippingAddress: '123 Main St' });
    req.flush(mockResponse);
  });

  it('should fetch orders', () => {
    const mockResponse: OrderHistoryResponse = {
      orders: [
        { id: 'order-1', userId: 'user-1', totalAmount: 49.99, status: 'confirmed', shippingAddress: '123 St', items: [], createdAt: '', updatedAt: '' },
      ],
    };

    service.getOrders().subscribe(response => {
      expect(response.orders.length).toBe(1);
    });

    const req = httpMock.expectOne('/api/orders');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});
