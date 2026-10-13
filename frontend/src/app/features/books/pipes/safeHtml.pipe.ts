import { Pipe, type PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
  name: 'SafeHtml',
})
export class SafeHtmlPipe implements PipeTransform {

  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string, baseUrl: string = ''): SafeHtml {
    if (!value) return '';

    // Transformar enlaces internos para que sean absolutos
    const transformedHtml = value.replace(
      /href="#([^"]*)"/g,
      `href="${baseUrl}#$1"`
    );

    return this.sanitizer.bypassSecurityTrustHtml(transformedHtml);
  }
}
