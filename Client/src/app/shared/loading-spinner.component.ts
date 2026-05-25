import { Component, inject } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { LoadingService } from '../services/loading.service';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [MatProgressBarModule],
  template: `
    @if (loadingService.isLoading()) {
      <div class="loading-bar">
        <mat-progress-bar mode="indeterminate" color="accent"></mat-progress-bar>
      </div>
    }
  `,
  styles: `
    .loading-bar {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 9999;
    }
  `
})
export class LoadingSpinnerComponent {
  readonly loadingService = inject(LoadingService);
}
