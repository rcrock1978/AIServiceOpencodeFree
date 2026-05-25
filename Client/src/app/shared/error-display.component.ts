import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-error-display',
  standalone: true,
  imports: [MatIconModule],
  template: `
    @if (message) {
      <div class="error-alert">
        <mat-icon color="warn">error</mat-icon>
        <span>{{ message }}</span>
      </div>
    }
  `,
  styles: `
    .error-alert {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      background: #fff3f3;
      border: 1px solid #f44336;
      border-radius: 4px;
      color: #d32f2f;
      margin: 8px 0;
    }
  `
})
export class ErrorDisplayComponent {
  @Input() message = '';
}
