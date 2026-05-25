import { Component, output, signal, computed, inject, OnDestroy } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { VoiceService } from '../../services/voice.service';

@Component({
  selector: 'app-voice-input',
  standalone: true,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule],
  template: `
    <div class="voice-input">
      @if (isRecording()) {
        <div class="recording-indicator">
          <div class="pulse-dot"></div>
          <span class="elapsed">{{ elapsedTime }}s</span>
          <div class="audio-visualization">
            @for (bar of visualizationBars; track $index) {
              <div class="bar" [style.height.px]="bar"></div>
            }
          </div>
        </div>
      }
      <button
        mat-icon-button
        [color]="isRecording() ? 'warn' : 'primary'"
        [matTooltip]="isRecording() ? 'Stop recording' : 'Start voice input'"
        [class.recording]="isRecording()"
        (click)="toggleRecording()"
      >
        <mat-icon>{{ isRecording() ? 'stop' : 'mic' }}</mat-icon>
      </button>
    </div>
  `,
  styles: [`
    .voice-input {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .recording-indicator {
      display: flex;
      align-items: center;
      gap: 6px;
      background: #fff1f0;
      border-radius: 20px;
      padding: 4px 12px;
    }

    .pulse-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #f44336;
      animation: pulse 1s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; transform: scale(1); }
      50% { opacity: 0.5; transform: scale(1.3); }
    }

    .elapsed {
      font-size: 12px;
      font-weight: 500;
      color: #f44336;
      min-width: 24px;
    }

    .audio-visualization {
      display: flex;
      align-items: center;
      gap: 2px;
      height: 24px;
    }

    .bar {
      width: 3px;
      border-radius: 2px;
      background: #f44336;
      transition: height 0.1s;
    }

    button.recording {
      animation: glow 1s ease-in-out infinite;
    }

    @keyframes glow {
      0%, 100% { box-shadow: 0 0 0 0 rgba(244,67,54,0.4); }
      50% { box-shadow: 0 0 0 6px rgba(244,67,54,0); }
    }
  `]
})
export class VoiceInputComponent implements OnDestroy {
  readonly transcript = output<string>();

  private readonly voiceService = inject(VoiceService);

  readonly isRecording = signal(false);
  readonly isSupported = computed(() => typeof navigator !== 'undefined' && 'MediaRecorder' in window);
  readonly audioLevel = signal(0);

  visualizationBars: number[] = [8, 12, 16, 10, 14, 18, 12, 8];

  private mediaRecorder: MediaRecorder | null = null;
  private audioChunks: Blob[] = [];
  private audioStream: MediaStream | null = null;
  private animationFrameId: number | null = null;
  private analyserNode: AnalyserNode | null = null;
  private startTime = 0;
  private elapsedInterval: ReturnType<typeof setInterval> | null = null;
  elapsedTime = 0;

  toggleRecording(): void {
    if (this.isRecording()) {
      this.stopRecording();
    } else {
      this.startRecording();
    }
  }

  private startRecording(): void {
    if (!this.isSupported()) return;

    navigator.mediaDevices.getUserMedia({ audio: true })
      .then(stream => {
        this.audioStream = stream;
        this.audioChunks = [];
        this.startTime = Date.now();
        this.elapsedTime = 0;

        const mimeType = MediaRecorder.isTypeSupported('audio/webm') ? 'audio/webm' : 'audio/mp4';
        this.mediaRecorder = new MediaRecorder(stream, { mimeType });

        this.mediaRecorder.ondataavailable = (event) => {
          if (event.data.size > 0) {
            this.audioChunks.push(event.data);
          }
        };

        this.mediaRecorder.onstop = () => {
          this.processRecording();
        };

        this.mediaRecorder.start(250);
        this.isRecording.set(true);

        this.elapsedInterval = setInterval(() => {
          this.elapsedTime = Math.floor((Date.now() - this.startTime) / 1000);
        }, 1000);

        this.setupAudioAnalysis(stream);
      })
      .catch(() => {});
  }

  private stopRecording(): void {
    this.mediaRecorder?.stop();
    this.isRecording.set(false);

    if (this.elapsedInterval) {
      clearInterval(this.elapsedInterval);
      this.elapsedInterval = null;
    }

    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }

    this.analyserNode = null;

    this.audioStream?.getTracks().forEach(t => t.stop());
    this.audioStream = null;
  }

  private setupAudioAnalysis(stream: MediaStream): void {
    try {
      const audioContext = new AudioContext();
      const source = audioContext.createMediaStreamSource(stream);
      const analyser = audioContext.createAnalyser();
      analyser.fftSize = 32;
      source.connect(analyser);
      this.analyserNode = analyser;
      this.updateAudioVisualization();
    } catch {
      // Audio analysis not supported
    }
  }

  private updateAudioVisualization(): void {
    if (!this.analyserNode || !this.isRecording()) return;

    const dataArray = new Uint8Array(this.analyserNode.frequencyBinCount);
    this.analyserNode.getByteFrequencyData(dataArray);

    const avg = dataArray.reduce((a, b) => a + b, 0) / dataArray.length;
    this.audioLevel.set(avg);

    this.visualizationBars = Array.from({ length: 8 }, (_, i) => {
      const idx = Math.floor((i / 8) * dataArray.length);
      return Math.max(4, Math.min(24, dataArray[idx] / 12));
    });

    this.animationFrameId = requestAnimationFrame(() => this.updateAudioVisualization());
  }

  private processRecording(): void {
    const blob = new Blob(this.audioChunks, { type: 'audio/webm' });
    this.audioChunks = [];

    this.voiceService.transcribe(blob).subscribe({
      next: (res) => {
        if (res.text) {
          this.transcript.emit(res.text);
        }
      }
    });
  }

  ngOnDestroy(): void {
    if (this.elapsedInterval) {
      clearInterval(this.elapsedInterval);
    }
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
    this.audioStream?.getTracks().forEach(t => t.stop());
  }
}
