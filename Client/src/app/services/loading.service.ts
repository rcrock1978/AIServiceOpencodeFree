import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private requestCount = signal(0);
  readonly isLoading = signal(false);

  show() {
    this.requestCount.update(c => c + 1);
    this.isLoading.set(true);
  }

  hide() {
    this.requestCount.update(c => Math.max(0, c - 1));
    if (this.requestCount() === 0) this.isLoading.set(false);
  }
}
