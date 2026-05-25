import { Product } from './product.model';

export interface ChatMessage {
  id: string;
  conversationId?: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  products?: Product[];
  timestamp?: Date;
  createdAt: string;
}

export interface ChatResponse {
  reply: string;
  products?: Product[];
  conversationId?: string;
}

export interface AskRequest {
  message: string;
}

export interface AskWithContextRequest {
  message: string;
  conversationId: string;
}
