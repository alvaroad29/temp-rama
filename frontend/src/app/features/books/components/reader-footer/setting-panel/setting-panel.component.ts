import { Component, inject, input, Input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ReadingSettings } from './../../../interfaces/reading-setting.interface';
import { ReadingSettingsService } from '../../../services/reading-settings.service';

@Component({
  selector: 'reader-settings-menu',
  standalone: true,
  imports: [CommonModule],
  templateUrl: `./setting-panel.component.html`
})
export class ReaderSettingsMenuComponent {
  private readingSettingsService = inject(ReadingSettingsService);

  isOpen = input(false);
  closeEvent = output();

  // Usar signals del servicio
  selectedFont = this.readingSettingsService.font;
  textSize = this.readingSettingsService.textSize;
  lineHeight = this.readingSettingsService.lineHeight;
  highContrast = this.readingSettingsService.highContrast;


  // Configuraciones
  fonts = [
    { label: 'OpenSans', value: 'Open Sans, sans-serif' },
    { label: 'Source serif', value: 'Source Serif Pro, serif' },
    { label: 'Mono', value: 'Monaco, Consolas, monospace' },
    { label: 'Merriweather', value: 'Merriweather, serif' },
  ];

  selectFont(font: any) {
    this.readingSettingsService.setFont(font.value);
  }

  increaseTextSize() {
    if (this.textSize() < 24) {
      this.readingSettingsService.setTextSize(this.textSize() + 1);
    }
  }

  decreaseTextSize() {
    if (this.textSize() > 12) {
      this.readingSettingsService.setTextSize(this.textSize() - 1);
    }
  }

  increaseLineHeight() {
    if (this.lineHeight() < 36) {
      this.readingSettingsService.setLineHeight(this.lineHeight() + 1);
    }
  }

  decreaseLineHeight() {
    if (this.lineHeight() > 16) {
      this.readingSettingsService.setLineHeight(this.lineHeight() - 1);
    }
  }

  toggleContrast(high: boolean) {
    this.readingSettingsService.setHighContrast(high);
  }

  resetSettings() {
    this.readingSettingsService.resetSettings();
  }

  closeMenu() {
    this.closeEvent.emit();
  }
}
