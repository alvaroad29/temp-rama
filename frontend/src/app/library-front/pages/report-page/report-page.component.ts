import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormUtils } from '../../../auth/utils/form-utils';
import { BookDetails } from '../../../features/books/interfaces/book-details-interface';
import { BooksService } from '../../../features/books/services/books.service';
import { ReportsService, ReportType, CreateReportDto } from '../../../features/books/services/reports.service';

@Component({
  selector: 'app-report-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './report-page.component.html',
})
export class ReportPageComponent {
   private reportsService = inject(ReportsService);
  private booksService = inject(BooksService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  router = inject(Router);

  formUtils = FormUtils;

  reportForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(5000)]],
    reportType: [ReportType.Otro, [Validators.required]]
  });

  bookId = signal<number | null>(null);
  book = signal<BookDetails | null>(null);
  reportTypes = this.reportsService.getReportTypes();

  successMessage = signal<string | null>(null);
  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  isLoading = signal(false);
  isBookLoading = signal(true);

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const bookId = params['bookId'];
      if (bookId) {
        this.bookId.set(+bookId);
        this.loadBookDetails(+bookId);
      }
    });
  }

  private loadBookDetails(bookId: number): void {
    this.isBookLoading.set(true);
    this.booksService.getBook(bookId).subscribe({
      next: (book) => {
        this.book.set(book);
        this.isBookLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading book:', error);
        this.setErrors(['No se pudo cargar la información del libro']);
        this.isBookLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.reportForm.invalid) {
      this.reportForm.markAllAsTouched();
      return;
    }

    if (!this.bookId()) {
      this.setErrors(['No se ha especificado un libro para reportar']);
      return;
    }

    this.clearErrors();
    this.isLoading.set(true);

    const formValue = this.reportForm.value;
    const reportData: CreateReportDto = {
      bookId: this.bookId()!,
      title: formValue.title,
      description: formValue.description,
      reportType: Number(formValue.reportType) // Asegurar que sea número
    };

    console.log('Enviando reporte:', reportData); // Para debug

    this.reportsService.createReport(reportData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.successMessage.set('Reporte creado correctamente. Te contactaremos si necesitamos más información.');

        this.reportForm.reset({ reportType: ReportType.Otro });

        // Redirigir después de 3 segundos
        setTimeout(() => {
          if (this.bookId()) {
            this.router.navigate(['/library', this.bookId()]);
          } else {
            this.router.navigate(['/library']);
          }
        }, 3000);
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('Error completo:', error);

        // Manejar diferentes tipos de errores
        if (error.error?.errors) {
          // Error de validación del backend
          const validationErrors = this.flattenValidationErrors(error.error.errors);
          this.setErrors(validationErrors);
        } else if (error.error?.title) {
          // Error con mensaje específico
          this.setErrors([error.error.title]);
        } else if (error.status === 400) {
          this.setErrors(['Datos inválidos. Por favor verifica la información.']);
        } else if (error.status === 401) {
          this.setErrors(['Debes iniciar sesión para reportar un problema.']);
        } else if (error.status === 404) {
          this.setErrors(['El libro no fue encontrado.']);
        } else {
          this.setErrors(['Ha ocurrido un error al crear el reporte']);
        }
      }
    });
  }

  // Método para aplanar los errores de validación del backend
  private flattenValidationErrors(errors: any): string[] {
    const flattened: string[] = [];

    for (const key in errors) {
      if (Array.isArray(errors[key])) {
        flattened.push(...errors[key]);
      } else {
        flattened.push(errors[key]);
      }
    }

    return flattened;
  }

  private setErrors(errors: string[] | string): void {
    this.hasError.set(true);
    // Asegurarnos de que siempre sea un array
    if (Array.isArray(errors)) {
      this.errorMessages.set(errors);
    } else {
      this.errorMessages.set([errors]);
    }
  }

  private clearErrors(): void {
    this.hasError.set(false);
    this.errorMessages.set([]);
  }

  get characterCount(): number {
    return this.reportForm.get('description')?.value?.length || 0;
  }

  get maxCharacters(): number {
    return 5000;
  }
}
