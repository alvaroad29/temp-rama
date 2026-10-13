import { computed, Injectable, signal } from '@angular/core';

export interface ReadingSettings {
  font: string;
  textSize: number;
  lineHeight: number;
  highContrast: boolean;
  readingMode: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ReadingSettingsService {
  private readonly STORAGE_KEY = 'reading-settings';

  // Configuración por defecto
  private defaultSettings: ReadingSettings = {
    font: 'Open Sans, sans-serif',
    textSize: 16,
    lineHeight: 24,
    highContrast: false,
    readingMode: false
  };

  // Signals individuales para cada configuración
  private fontSignal = signal<string>(this.defaultSettings.font);
  private textSizeSignal = signal<number>(this.defaultSettings.textSize);
  private lineHeightSignal = signal<number>(this.defaultSettings.lineHeight);
  private highContrastSignal = signal<boolean>(this.defaultSettings.highContrast);
  private readingModeSignal = signal<boolean>(this.defaultSettings.readingMode);

  // Signal computada para la configuración completa
  currentSettings = computed<ReadingSettings>(() => ({
    font: this.fontSignal(),
    textSize: this.textSizeSignal(),
    lineHeight: this.lineHeightSignal(),
    highContrast: this.highContrastSignal(),
    readingMode: this.readingModeSignal()
  }));

  constructor() {
    // Cargar configuraciones al inicializar el servicio
    this.loadStoredSettings();
    this.initFullscreenListener();
  }

  // Cargar configuraciones guardadas
  private loadStoredSettings(): void {
    if (typeof window === 'undefined' || !localStorage) {
      return;
    }

    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        const settings: ReadingSettings = { ...this.defaultSettings, ...JSON.parse(stored) };
        this.setSettings(settings);
      }
    } catch (error) {
      console.error('Error loading reading settings:', error);
    }
  }

  // Guardar configuraciones
  private saveToStorage(): void {
    try {
      if (typeof window !== 'undefined' && localStorage) {
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.currentSettings()));
      }
    } catch (error) {
      console.error('Error saving reading settings:', error);
    }
  }

  // Métodos públicos para actualizar configuraciones individuales
  setFont(font: string): void {
    this.fontSignal.set(font);
    this.saveToStorage();
  }

  setTextSize(size: number): void {
    this.textSizeSignal.set(size);
    this.saveToStorage();
  }

  setLineHeight(height: number): void {
    this.lineHeightSignal.set(height);
    this.saveToStorage();
  }

  setHighContrast(contrast: boolean): void {
    this.highContrastSignal.set(contrast);
    this.saveToStorage();
  }

  setReadingMode(enabled: boolean): void {
    this.readingModeSignal.set(enabled);
    this.saveToStorage();

    if (enabled) {
      this.enterFullscreen();
    } else {
      this.exitFullscreen();
    }
  }


  // Método para toggle del modo lectura
  toggleReadingMode(): void {
    const newValue = !this.readingModeSignal();
    this.setReadingMode(newValue);
  }

  // Manejo de pantalla completa
  private enterFullscreen(): void {
    const docEl = document.documentElement as any;

    if (docEl.requestFullscreen) {
      docEl.requestFullscreen();
    } else if (docEl.webkitRequestFullscreen) { /* Safari */
      docEl.webkitRequestFullscreen();
    } else if (docEl.msRequestFullscreen) { /* IE11 */
      docEl.msRequestFullscreen();
    }
  }

  private exitFullscreen(): void {
    const doc = document as any;

    if (doc.exitFullscreen) {
      doc.exitFullscreen();
    } else if (doc.webkitExitFullscreen) { /* Safari */
      doc.webkitExitFullscreen();
    } else if (doc.msExitFullscreen) { /* IE11 */
      doc.msExitFullscreen();
    }
  }

  // Escuchar cambios de pantalla completa
  initFullscreenListener(): void {
    document.addEventListener('fullscreenchange', this.handleFullscreenChange.bind(this));
    document.addEventListener('webkitfullscreenchange', this.handleFullscreenChange.bind(this));
    document.addEventListener('msfullscreenchange', this.handleFullscreenChange.bind(this));
  }

  private handleFullscreenChange(): void {
    const isFullscreen = !!(
      document.fullscreenElement ||
      (document as any).webkitFullscreenElement ||
      (document as any).msFullscreenElement
    );

    // Sincronizar el signal con el estado real de pantalla completa
    if (this.readingModeSignal() !== isFullscreen) {
      this.readingModeSignal.set(isFullscreen);
      this.saveToStorage();
    }
  }

  // Actualizar todas las configuraciones a la vez
  setSettings(settings: ReadingSettings): void {
    this.fontSignal.set(settings.font);
    this.textSizeSignal.set(settings.textSize);
    this.lineHeightSignal.set(settings.lineHeight);
    this.highContrastSignal.set(settings.highContrast);
    this.readingModeSignal.set(settings.readingMode || false);
    this.saveToStorage();
  }

  // Restablecer a valores por defecto
  resetSettings(): void {
    this.setSettings(this.defaultSettings);
  }

  // Getters individuales (opcionales, pero útiles)
  get font() { return this.fontSignal.asReadonly(); }
  get textSize() { return this.textSizeSignal.asReadonly(); }
  get lineHeight() { return this.lineHeightSignal.asReadonly(); }
  get highContrast() { return this.highContrastSignal.asReadonly(); }
  get readingMode() { return this.readingModeSignal.asReadonly(); }
}
