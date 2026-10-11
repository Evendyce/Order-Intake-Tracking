import { DOCUMENT } from '@angular/common';
import { inject, Injectable, signal } from '@angular/core';

export type Theme = 'dark' | 'light';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  readonly theme = signal<Theme>(
    this.document.documentElement.dataset['theme'] === 'light' ? 'light' : 'dark',
  );

  constructor() {
    this.apply(this.theme());
  }

  toggle(): void {
    const next = this.theme() === 'dark' ? 'light' : 'dark';
    this.theme.set(next);
    this.apply(next);
    // Theme preferences should never prevent navigation when storage is blocked.
    try {
      this.document.defaultView?.localStorage.setItem('vg-theme', next);
    } catch {
      // The selected theme remains available for the current session.
    }
  }

  private apply(theme: Theme): void {
    this.document.documentElement.dataset['theme'] = theme;
    this.document.documentElement.style.colorScheme = theme;
  }
}
