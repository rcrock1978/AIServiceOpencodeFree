import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ChatResponse } from '../core/models/chat-message.model';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  readonly currentConversationId = signal<string | undefined>(undefined);
  readonly conversationId = this.currentConversationId.asReadonly();

  ask(message: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>('/api/chat/ask', { message }).pipe(
      tap(res => {
        if (res.conversationId) this.currentConversationId.set(res.conversationId);
      })
    );
  }

  setConversationId(id: string): void {
    this.currentConversationId.set(id);
  }

  askWithContext(message: string, conversationId: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>('/api/chat/ask/context', { message, conversationId }).pipe(
      tap(res => {
        if (res.conversationId) this.currentConversationId.set(res.conversationId);
      })
    );
  }
}
