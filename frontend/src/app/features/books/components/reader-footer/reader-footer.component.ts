import { Component, HostListener, inject, input, output } from '@angular/core';
import { ArrowLeft, ChevronLeft, ChevronRight, LucideAngularModule, Maximize2, Minimize2, Settings } from 'lucide-angular';
import { ReaderSettingsMenuComponent } from './setting-panel/setting-panel.component';
import { ReadingSettings } from '../../interfaces/reading-setting.interface';
import { RouterLink } from '@angular/router';
import { ReadingSettingsService } from '../../services/reading-settings.service';

@Component({
  selector: 'reader-footer',
  imports: [LucideAngularModule, ReaderSettingsMenuComponent, RouterLink],
  templateUrl: './reader-footer.component.html',
})
export class ReaderFooterComponent {
  private readingSettingsService = inject(ReadingSettingsService);

  bookId = input<number|null>();
  previousChapter = input<number|null>();
  nextChapter = input<number|null>();

  showFooter = true;
  private lastScrollTop = 0;
  isSettingsOpen = false;

  readingMode = this.readingSettingsService.readingMode;


  @HostListener('window:scroll', [])
  onWindowScroll() {
    const st = window.pageYOffset || document.documentElement.scrollTop;
    this.showFooter = st < this.lastScrollTop;
    this.lastScrollTop = st <= 0 ? 0 : st;
  }

  // Iconos
  readonly setting = Settings;
  readonly left = ChevronLeft;
  readonly right = ChevronRight;
  readonly arrowLeft = ArrowLeft;
   readonly maximize = Maximize2;
  readonly minimize = Minimize2;

  // Métodos del menú de configuración
  toggleSettings() {
    this.isSettingsOpen = !this.isSettingsOpen;
  }

  closeSettings() {
    this.isSettingsOpen = false;
  }

  toggleReadingMode() {
    this.readingSettingsService.toggleReadingMode();
  }
}
