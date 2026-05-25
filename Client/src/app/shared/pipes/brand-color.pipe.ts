import { Pipe, PipeTransform } from '@angular/core';

const COLORS: Record<string, string> = {
  A: '#1976d2', B: '#388e3c', C: '#f57c00', D: '#7b1fa2',
  E: '#00796b', F: '#c2185b', G: '#455a64', H: '#5d4037',
  I: '#1565c0', J: '#2e7d32', K: '#e65100', L: '#6a1b9a',
  M: '#00695c', N: '#ad1457', O: '#37474f', P: '#4e342e',
  Q: '#0d47a1', R: '#1b5e20', S: '#bf360c', T: '#4a148c',
  U: '#004d40', V: '#880e4f', W: '#263238', X: '#3e2723',
  Y: '#01579b', Z: '#33691e',
};

@Pipe({ name: 'brandColor', standalone: true })
export class BrandColorPipe implements PipeTransform {
  transform(brand?: string): string {
    if (!brand || brand.length === 0) return '#1976d2';
    const first = brand.charAt(0).toUpperCase();
    return COLORS[first] || '#1976d2';
  }
}
