import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChatService } from './chat.service';
import { ChatResponse } from '../core/models/chat-message.model';

describe('ChatService', () => {
  let service: ChatService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(ChatService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should send ask request and set conversationId', () => {
    const mockResponse: ChatResponse = {
      reply: 'Here are some products',
      products: [{ id: '1', name: 'Shoe', price: 49.99, description: '', brand: 'Nike', type: 'shoes', imageUrl: '', rating: 4, stock: 5, createdAt: '', updatedAt: '' }],
      conversationId: 'conv-1',
    };

    service.ask('show me shoes').subscribe(res => {
      expect(res.reply).toBe('Here are some products');
      expect(service.currentConversationId()).toBe('conv-1');
    });

    const req = httpMock.expectOne('/api/chat/ask');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ message: 'show me shoes' });
    req.flush(mockResponse);
  });

  it('should send askWithContext request', () => {
    service.setConversationId('existing-conv');

    const mockResponse: ChatResponse = {
      reply: 'Following up',
      conversationId: 'existing-conv',
    };

    service.askWithContext('tell me more', 'existing-conv').subscribe(res => {
      expect(res.reply).toBe('Following up');
    });

    const req = httpMock.expectOne('/api/chat/ask/context');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ message: 'tell me more', conversationId: 'existing-conv' });
    req.flush(mockResponse);
  });
});
