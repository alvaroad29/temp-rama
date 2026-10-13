import { LucideAngularModule, Calendar } from 'lucide-angular';
// annoucement-page.component.ts
import { Component, signal, ElementRef, AfterViewInit, ViewChildren, QueryList, computed, inject } from '@angular/core';
import { Announcement } from '../../interfaces/announcement-interface';
import { RelativeTimePipe } from '../../../shared/pipes/relative-time.pipe';
import { CommonModule } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { AnnoucementsService } from '../../../shared/services/annoucements.service';

@Component({
  selector: 'app-annoucement-page',
  imports: [RelativeTimePipe, CommonModule, LucideAngularModule],
  templateUrl: './annoucement-page.component.html',
  styles: [`
    .collapsible-content {
      transition: max-height 0.3s ease, opacity 0.3s ease;
      overflow: hidden;
    }

    .line-clamp-4 {
      display: -webkit-box;
      -webkit-line-clamp: 4;
      -webkit-box-orient: vertical;
      overflow: hidden;
      line-height: 1.6;
    }

    .content-measurement {
      visibility: hidden;
      position: absolute;
      top: -9999px;
      left: -9999px;
      width: 100%;
      line-height: 1.6;
    }
  `]
})
  export class AnnoucementPageComponent implements AfterViewInit {
    @ViewChildren('contentParagraph') contentParagraphs!: QueryList<ElementRef>;


  private annoucementService = inject(AnnoucementsService);

  annoucementResource = rxResource({
    loader: () => {
        return this.annoucementService.getAnnoucements();
      },
    });

  announcements = computed(() => {
    const resource = this.annoucementResource.value();
    return resource || [];
  });

  // Signal para controlar qué anuncios están expandidos
  expandedAnnouncements = signal<Set<number>>(new Set());

  // Signal para controlar qué anuncios necesitan truncado (detectado automáticamente)
  truncatableAnnouncements = signal<Set<number>>(new Set());

  ngAfterViewInit() {
    this.contentParagraphs.changes.subscribe(() => {
      setTimeout(() => {
        this.detectTruncatableContent();
      }, 0);
    });

    // Detectar inicialmente
    setTimeout(() => {
      this.detectTruncatableContent();
    }, 100);
  }

  private detectTruncatableContent() {
    const truncatable = new Set<number>();

    this.contentParagraphs.forEach((paragraph, index) => {
      const element = paragraph.nativeElement;
      const announcement = this.announcements()[index];

      // Crear un elemento temporal para medir la altura real del contenido
      const measurementElement = this.createMeasurementElement(element, announcement.content);
      document.body.appendChild(measurementElement);

      // Obtener la altura del contenido completo
      const fullHeight = measurementElement.scrollHeight;

      // Calcular la altura máxima para 4 líneas
      const lineHeight = parseFloat(getComputedStyle(measurementElement).lineHeight);
      const maxLines = 4;
      const maxHeight = lineHeight * maxLines;

      // Si el contenido completo es más alto que 4 líneas, necesita truncado
      if (fullHeight > maxHeight + 5) { // +5 px de margen para evitar falsos positivos
        truncatable.add(announcement.id);
        console.log(`Anuncio ${announcement.id} necesita truncado:`, {
          fullHeight,
          maxHeight,
          content: announcement.content.substring(0, 50) + '...'
        });
      }

      // Limpiar el elemento temporal
      document.body.removeChild(measurementElement);
    });

    this.truncatableAnnouncements.set(truncatable);
  }

  private createMeasurementElement(originalElement: HTMLElement, content: string): HTMLElement {
    const measurementElement = document.createElement('p');

    // Copiar los estilos relevantes del elemento original
    const originalStyles = getComputedStyle(originalElement);
    measurementElement.style.font = originalStyles.font;
    measurementElement.style.fontSize = originalStyles.fontSize;
    measurementElement.style.fontFamily = originalStyles.fontFamily;
    measurementElement.style.fontWeight = originalStyles.fontWeight;
    measurementElement.style.lineHeight = originalStyles.lineHeight || '1.6';
    measurementElement.style.letterSpacing = originalStyles.letterSpacing;
    measurementElement.style.wordSpacing = originalStyles.wordSpacing;

    // Obtener el ancho del contenedor padre
    const containerWidth = originalElement.parentElement?.clientWidth || originalElement.clientWidth;
    measurementElement.style.width = `${containerWidth}px`;

    // Estilos para medición oculta
    measurementElement.style.visibility = 'hidden';
    measurementElement.style.position = 'absolute';
    measurementElement.style.top = '-9999px';
    measurementElement.style.left = '-9999px';
    measurementElement.style.margin = '0';
    measurementElement.style.padding = '0';
    measurementElement.style.border = 'none';
    measurementElement.style.whiteSpace = 'normal';
    measurementElement.style.wordWrap = 'break-word';

    // Agregar el contenido
    measurementElement.textContent = content;

    return measurementElement;
  }

  // Función para alternar la visibilidad del contenido
  toggleAnnouncement(id: number): void {
    const expanded = new Set(this.expandedAnnouncements());
    if (expanded.has(id)) {
      expanded.delete(id);
    } else {
      expanded.add(id);
    }
    this.expandedAnnouncements.set(expanded);
  }

  // Función para verificar si un anuncio está expandido
  isExpanded(id: number): boolean {
    return this.expandedAnnouncements().has(id);
  }

  // Función que determina si mostrar el botón basándose en la detección automática
  shouldTruncate(id: number): boolean {
    return this.truncatableAnnouncements().has(id);
  }


  readonly calendar = Calendar;
}
