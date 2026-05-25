import { Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ChatMessage } from '../models/chat-message.model';
import { ProductCardComponent } from './product-card.component';

@Component({
  selector: 'app-chat-message',
  standalone: true,
  imports: [DatePipe, MatIconModule, ProductCardComponent],
  template: `
    <div class="message" [class.user]="message().role === 'user'" [class.assistant]="message().role === 'assistant'">
      <div class="avatar">
        @if (message().role === 'user') {
          <mat-icon>person</mat-icon>
        } @else {
          <mat-icon>smart_toy</mat-icon>
        }
      </div>
      <div class="bubble">
        <div class="content">{{ message().content }}</div>
        @if (message().products?.length) {
          <div class="products">
            @for (product of message().products; track product.id) {
              <app-product-card [product]="product" />
            }
          </div>
        }
        <div class="timestamp">{{ message().timestamp | date:'short' }}</div>
      </div>
    </div>
  `,
  styles: [`
    .message {
      display: flex;
      gap: 8px;
      margin-bottom: 12px;
      max-width: 100%;

      &.user {
        flex-direction: row-reverse;

        .bubble {
          background: #7c4dff;
          color: #fff;
          border-radius: 16px 4px 16px 16px;
        }

        .timestamp { text-align: right; }
      }

      &.assistant {
        .bubble {
          background: #f0f0f0;
          color: #333;
          border-radius: 4px 16px 16px 16px;
        }
      }
    }

    .avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: #e0e0e0;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;

      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
        line-height: 18px;
      }
    }

    .bubble {
      padding: 10px 14px;
      max-width: calc(100% - 48px);
      box-shadow: 0 1px 2px rgba(0,0,0,0.1);
    }

    .content {
      white-space: pre-wrap;
      word-break: break-word;
      line-height: 1.5;
    }

    .products {
      margin-top: 8px;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .timestamp {
      font-size: 11px;
      opacity: 0.6;
      margin-top: 4px;
    }
  `]
})
export class ChatMessageComponent {
  readonly message = input.required<ChatMessage>();
}
