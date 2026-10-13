import { Component, Input, ElementRef, ViewChild, AfterViewInit, input } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ChevronUp, LucideAngularModule } from 'lucide-angular';
import { BookDetails } from '../../interfaces/book-details-interface';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time.pipe';

interface Content {
  id: string;
  title: string;
  date: string;
  views: number;
}

interface Section {
  bookId?: string;
  id: string;
  title: string;
  content: Content[];
}

@Component({
  selector: 'book-details-content',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule,  RelativeTimePipe, TitleCasePipe],
  templateUrl: './book-details-content.component.html'
})
export class ChapterAccordionComponent implements AfterViewInit {
  book = input<BookDetails>();



  expandedSections: Set<number> = new Set();
  private contentHeights: Map<number, number> = new Map();

  ngAfterViewInit(): void {
    // Calculamos las alturas después de que la vista se haya inicializado
    this.calculateContentHeights();
  }

  toggleSection(id: number): void {
    if (this.expandedSections.has(id)) {
      this.expandedSections.delete(id);
    } else {
      this.expandedSections.add(id);
      // Recalculamos la altura cuando se expande
      setTimeout(() => this.calculateContentHeight(id), 0);
    }
  }

  isExpanded(id: number): boolean {
    return this.expandedSections.has(id);
  }

  getContentHeight(id: number): number {
    // Si tenemos la altura calculada, la usamos
    if (this.contentHeights.has(id)) {
      return this.contentHeights.get(id)!;
    }

    // Si no, calculamos una altura estimada más generosa
    const section = this.book()?.volumes.find(s => s.id === id);
    if (!section) return 0;

    // Estimación más adaptativa basada en el viewport
    const isMobile = window.innerWidth < 768;
    const itemHeight = isMobile ? 120 : 80; // Más altura en móvil
    const padding = 32; // padding del contenedor

    return (section.chapters.length * itemHeight) + padding;
  }

  private calculateContentHeights(): void {
     this.book()?.volumes.forEach(section => {
      this.calculateContentHeight(section.id);
    });
  }

  private calculateContentHeight(id: number): void {
    const contentElement = document.getElementById(`content-${id}`);
    if (contentElement) {
      // Temporalmente mostramos el contenido para medir su altura real
      const originalMaxHeight = contentElement.style.maxHeight;
      contentElement.style.maxHeight = 'none';
      contentElement.style.visibility = 'hidden';
      contentElement.style.position = 'absolute';

      const height = contentElement.scrollHeight;

      // Restauramos los estilos originales
      contentElement.style.maxHeight = originalMaxHeight;
      contentElement.style.visibility = '';
      contentElement.style.position = '';

      this.contentHeights.set(id, height);
    }
  }


  // iconos
  readonly chevron = ChevronUp;
}
