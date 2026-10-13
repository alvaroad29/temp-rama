import { Component, computed, inject, OnInit, signal, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ReaderFooterComponent } from "../../../features/books/components/reader-footer/reader-footer.component";
import { ReadingSettings } from '../../../features/books/interfaces/reading-setting.interface';
import { rxResource } from '@angular/core/rxjs-interop';
import { BooksService } from '../../../features/books/services/books.service';
import { MarkdownModule, MarkdownService } from 'ngx-markdown';
import { ReadingSettingsService } from '../../../features/books/services/reading-settings.service';
import { marked } from 'marked';

@Component({
  selector: 'app-reader-page',
  imports: [CommonModule, RouterModule, ReaderFooterComponent, MarkdownModule],
  templateUrl: './reader-page.component.html',
  styleUrls: ['./reader-page.component.css'],
})
export class ReaderPageComponent implements OnInit {
  activatedRoute = inject(ActivatedRoute);
  private booksService = inject(BooksService);
  private readingSettingsService = inject(ReadingSettingsService);

  private markdownService = inject(MarkdownService);

  bookId = signal<number>(0);
  chapterId = signal<number>(0);

  currentUrl = computed(() => {
    const book = this.bookId();
    const chapter = this.chapterId();
    return `/library/${book}/${chapter}`;
  });

  // Usar signals del servicio directamente
  font = this.readingSettingsService.font;
  textSize = this.readingSettingsService.textSize;
  lineHeight = this.readingSettingsService.lineHeight;
  highContrast = this.readingSettingsService.highContrast;
  readingMode = this.readingSettingsService.readingMode;

  // Computada para las clases de contraste
  containerClasses = computed(() =>
    this.highContrast() ? 'bg-base-200' : 'bg-base-100'
  );

  chapterResource = rxResource({
    request: () => ({id: this.chapterId()}),
    loader: ({request}) => {
        return this.booksService.getChapter(request.id);
      },
  });

  constructor() {
     this.readingSettingsService.initFullscreenListener();
    this.activatedRoute.paramMap.subscribe(params => {
      const bookId = Number(params.get('bookId'));
      const chapterId = Number(params.get('chapterId'));

      this.bookId.set(bookId);
      this.chapterId.set(chapterId);
    });
  }

  ngOnInit() {
    this.configureMarkdownLinks();
  }

  private host = inject(ElementRef);

  // Convierte {#nombre} en un span con clase (la clase sobrevive al sanitizer)
  content = computed(() =>
    (this.chapterResource.value()?.content ?? '')
      .replace(/\{#([\w-]+)\}/g, '<span class="md-anchor md-anchor-$1"></span>')
  );

  onMarkdownReady() {
  this.host.nativeElement
    .querySelectorAll('span.md-anchor')
    .forEach((el: HTMLElement) => {
      const cls = [...el.classList].find(c => c.startsWith('md-anchor-'));
      if (cls) el.id = 'md-' + cls.slice('md-anchor-'.length);
    });
}


  private configureMarkdownLinks() {
    this.markdownService.renderer.link = (token: any) => {
      const href: string = token.href ?? '';
      const title = token.title ? ` title="${token.title}"` : '';
      // Renderiza el texto del enlace (soporta negrita, cursiva, etc.)
      const text = marked.parseInline(token.text ?? href) as string;

      // Enlaces externos: nueva pestaña
      if (href.startsWith('http')) {
        return `<a href="${href}"${title} target="_blank" rel="noopener noreferrer">${text}</a>`;
      }

      // Internos (#nota1) y cualquier otro: enlace normal
      return `<a href="${href}"${title}>${text}</a>`;
    };
  }

  onContentClick(event: MouseEvent) {
    const anchor = (event.target as HTMLElement).closest('a');
    const href = anchor?.getAttribute('href');
    if (!href || !href.startsWith('#') || href.length < 2) return;

    event.preventDefault();
    const id = decodeURIComponent(href.slice(1));
    document.getElementById('md-' + id)
      ?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }

  ngAfterViewInit() {
    window.scrollTo(0, 0);
  }
}
