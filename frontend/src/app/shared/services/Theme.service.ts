import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark';
@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly currentTheme = signal<Theme>('dark');

  constructor() {
    // Cargar tema guardado
    const savedTheme = this.getThemeInLocalStorage();
    this.setTheme(savedTheme);
  }

  getCurrentTheme() {
    return this.currentTheme.asReadonly();
  }

  private setTheme(theme: Theme) {
    this.currentTheme.set(theme);
    document.documentElement.setAttribute('data-theme', theme);
    this.setThemeInLocalStorage(theme);
  }

  toggleTheme() {
    if (this.currentTheme() === 'dark') {
      this.setTheme('light');
    } else {
      this.setTheme('dark');
    }
  }

  private setThemeInLocalStorage(theme: Theme) {
    localStorage.setItem('theme', theme);
  }

  private getThemeInLocalStorage() {
    return localStorage.getItem('theme') as Theme ?? 'dark';
  }

}
