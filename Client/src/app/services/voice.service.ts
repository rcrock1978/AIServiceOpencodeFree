import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SttResponse {
  text: string;
  confidence?: number;
}

@Injectable({ providedIn: 'root' })
export class VoiceService {
  private http = inject(HttpClient);

  transcribe(audioBlob: Blob): Observable<SttResponse> {
    const formData = new FormData();
    formData.append('audio', audioBlob, 'recording.webm');
    return this.http.post<SttResponse>('/api/voice/stt', formData);
  }

  synthesize(text: string): Observable<Blob> {
    return this.http.post('/api/voice/tts', { text }, { responseType: 'blob' }) as Observable<Blob>;
  }
}
