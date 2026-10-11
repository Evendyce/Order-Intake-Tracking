import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme-service';

describe('VoidGlass preference', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    localStorage.removeItem('vg-theme');
    delete document.documentElement.dataset['theme'];
  });

  it('continues the palette applied before bootstrap and persists a toggle', () => {
    document.documentElement.dataset['theme'] = 'light';
    const service = TestBed.inject(ThemeService);
    expect(service.theme()).toBe('light');
    service.toggle();
    expect(document.documentElement.dataset['theme']).toBe('dark');
    expect(document.documentElement.style.colorScheme).toBe('dark');
    expect(localStorage.getItem('vg-theme')).toBe('dark');
  });

  it('still switches palettes when preference storage is unavailable', () => {
    document.documentElement.dataset['theme'] = 'dark';
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new DOMException('Storage unavailable', 'SecurityError');
    });
    const service = TestBed.inject(ThemeService);
    expect(() => service.toggle()).not.toThrow();
    expect(service.theme()).toBe('light');
    expect(document.documentElement.dataset['theme']).toBe('light');
  });
});
