import { Injectable } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'somo-theme';
  theme: Theme = 'light';

  constructor() {
    const saved = localStorage.getItem(this.storageKey) as Theme | null;
    const preferred = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    this.apply(saved ?? preferred);
  }

  get isDark(): boolean {
    return this.theme === 'dark';
  }

  toggle(): void {
    this.apply(this.isDark ? 'light' : 'dark');
  }

  private apply(theme: Theme): void {
    this.theme = theme;
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem(this.storageKey, theme);
  }
}
