import { Component, signal, inject, AfterViewChecked, ViewChild, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ChatMessage } from '../models/chat-message.model';
import { Product } from '../models/product.model';
import { ChatService } from '../../services/chat.service';
import { VoiceService } from '../../services/voice.service';
import { ChatMessageComponent } from './chat-message.component';
import { VoiceInputComponent } from './voice-input.component';

const STORAGE_KEY = 'chatbot_messages';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatProgressBarModule,
    MatTooltipModule,
    ChatMessageComponent,
    VoiceInputComponent,
  ],
  template: `
    <!-- FAB button -->
    @if (!isOpen()) {
      <button
        class="chat-fab"
        mat-fab
        color="primary"
        matTooltip="Open AI Shopping Assistant"
        (click)="toggleChat()"
      >
        <mat-icon>chat</mat-icon>
      </button>
    }

    <!-- Chat panel -->
    @if (isOpen()) {
      <div class="chat-panel">
        <div class="chat-header">
          <mat-icon>smart_toy</mat-icon>
          <span class="title">AI Shopping Assistant</span>
          <button mat-icon-button matTooltip="Close" (click)="toggleChat()">
            <mat-icon>close</mat-icon>
          </button>
        </div>

        <div class="chat-messages" #messagesContainer>
          @for (msg of messages(); track msg.id) {
            <app-chat-message [message]="msg" />
          }

          @if (isLoading()) {
            <div class="loading-indicator">
              <mat-progress-bar mode="indeterminate" />
              <span>Thinking...</span>
            </div>
          }
        </div>

        <div class="chat-input">
          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <input
              matInput
              [(ngModel)]="inputMessage"
              placeholder="Ask about products..."
              (keyup.enter)="sendMessage()"
              [disabled]="isLoading()"
            />
          </mat-form-field>

          <app-voice-input (transcript)="handleVoiceInput($event)" />

          <button
            mat-icon-button
            color="primary"
            matTooltip="Send"
            [disabled]="!inputMessage().trim() || isLoading()"
            (click)="sendMessage()"
          >
            <mat-icon>send</mat-icon>
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .chat-fab {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 1000;
    }

    .chat-panel {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 1000;
      width: 380px;
      height: 600px;
      max-height: calc(100vh - 48px);
      background: #fff;
      border-radius: 16px;
      box-shadow: 0 8px 32px rgba(0,0,0,0.2);
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .chat-header {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 16px;
      background: #7c4dff;
      color: #fff;

      .title {
        flex: 1;
        font-weight: 500;
        font-size: 15px;
      }
    }

    .chat-messages {
      flex: 1;
      overflow-y: auto;
      padding: 12px;
      display: flex;
      flex-direction: column;
    }

    .loading-indicator {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 4px;
      padding: 12px;
      font-size: 12px;
      color: #666;
    }

    .chat-input {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      border-top: 1px solid #e0e0e0;

      mat-form-field {
        flex: 1;

        ::ng-deep .mat-mdc-form-field-subscript-wrapper {
          display: none;
        }
      }
    }

    @media (max-width: 480px) {
      .chat-panel {
        width: 100%;
        height: 100%;
        max-height: 100vh;
        bottom: 0;
        right: 0;
        border-radius: 0;
      }
    }
  `]
})
export class ChatbotComponent implements AfterViewChecked, OnInit, OnDestroy {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef<HTMLElement>;

  readonly isOpen = signal(false);
  readonly messages = signal<ChatMessage[]>([]);
  readonly inputMessage = signal('');
  readonly isLoading = signal(false);

  private readonly chatService = inject(ChatService);
  private readonly voiceService = inject(VoiceService);

  private synthSub: Subscription | null = null;

  ngOnInit(): void {
    this.restoreMessages();
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.synthSub?.unsubscribe();
  }

  toggleChat(): void {
    this.isOpen.update(v => !v);
    if (this.isOpen()) {
      this.loadConversation();
    }
  }

  sendMessage(): void {
    const text = this.inputMessage().trim();
    if (!text || this.isLoading()) return;

    this.inputMessage.set('');
    this.addMessage({ role: 'user', content: text });
    this.isLoading.set(true);

    const convId = this.chatService.conversationId();
    const request = convId
      ? this.chatService.askWithContext(text, convId)
      : this.chatService.ask(text);

    request.subscribe({
      next: (res) => {
        if (res.conversationId) {
          this.chatService.setConversationId(res.conversationId);
        }
        this.addMessage({
          role: 'assistant',
          content: res.reply,
          products: res.products,
        });
        this.isLoading.set(false);
        this.persistMessages();

        if (res.reply) {
          this.synthSub = this.voiceService.synthesize(res.reply).subscribe({
            next: (blob) => {
              const url = URL.createObjectURL(blob);
              const audio = new Audio(url);
              audio.onended = () => URL.revokeObjectURL(url);
              audio.play();
            }
          });
        }
      },
      error: () => {
        this.addMessage({
          role: 'assistant',
          content: 'Sorry, I ran into an error. Please try again.',
        });
        this.isLoading.set(false);
      }
    });
  }

  handleVoiceInput(transcript: string): void {
    this.inputMessage.set(transcript);
    this.sendMessage();
  }

  private addMessage(msg: { role: 'user' | 'assistant'; content: string; products?: Product[] }): void {
    const id = crypto.randomUUID();
    const timestamp = new Date();
    this.messages.update(m => [...m, {
      id, role: msg.role, content: msg.content,
      products: msg.products, timestamp, createdAt: timestamp.toISOString()
    }]);
  }

  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }

  private loadConversation(): void {
    // Conversation is tracked via conversationId signal on ChatService
  }

  private persistMessages(): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.messages()));
    } catch {
      // localStorage may be full or unavailable
    }
  }

  private restoreMessages(): void {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const parsed: ChatMessage[] = JSON.parse(stored);
        this.messages.set(parsed.map(m => ({ ...m, timestamp: m.timestamp ? new Date(m.timestamp as any) : undefined })));
      }
    } catch {
      localStorage.removeItem(STORAGE_KEY);
    }
  }
}
