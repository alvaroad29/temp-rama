import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormUtils } from '../../../../auth/utils/form-utils';
import { AnnoucementsService, ApiResponse } from '../../../../shared/services/annoucements.service';

@Component({
  selector: 'app-edit-announcement-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './edit-announcement-page.component.html',
})
export class EditAnnouncementPageComponent implements OnInit {

  private annoucementService = inject(AnnoucementsService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  formUtils = FormUtils;

  announcementForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    content: ['', [Validators.required, Validators.maxLength(5000)]],
    updateDate: [false] // Campo booleano para actualizar fecha
  });

  successMessage = signal<string | null>(null);
  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  isLoading = signal(false);
  isLoadingAnnouncement = signal(true);
  announcementId!: number;

  ngOnInit() {
    this.loadAnnouncementData();
  }

  private loadAnnouncementData() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.setErrors(['ID de anuncio no válido']);
      this.isLoadingAnnouncement.set(false);
      return;
    }

    this.announcementId = +id;
    this.isLoadingAnnouncement.set(true);

    this.annoucementService.getAnnoucementById(this.announcementId).subscribe({
      next: (announcement) => {
        this.announcementForm.patchValue({
          title: announcement.title,
          content: announcement.content
          // updateDate se mantiene en false por defecto
        });
        this.isLoadingAnnouncement.set(false);
      },
      error: (error) => {
        this.setErrors(['Error al cargar el anuncio']);
        this.isLoadingAnnouncement.set(false);
        console.error('Error loading announcement:', error);
      }
    });
  }

  onSubmit() {
    if (this.announcementForm.invalid) {
      this.announcementForm.markAllAsTouched();
      return;
    }

    this.clearErrors();
    this.isLoading.set(true);

    const { title, content, updateDate } = this.announcementForm.value;


    this.annoucementService.editAnnoucement(this.announcementId, title, content, updateDate)
      .subscribe({
        next: (response: ApiResponse) => {
          this.isLoading.set(false);

          if (response.success) {
            this.successMessage.set('Anuncio actualizado correctamente');

            // Redirigir después de 2 segundos
            setTimeout(() => {
              this.router.navigate(['/admin/announcements']);
            }, 2000);
          } else {
            this.setErrors(response.errors || ['Error desconocido']);
          }
        },
        error: (error) => {
          this.isLoading.set(false);
          this.setErrors(['Ha ocurrido un error inesperado']);
          console.error('Error no manejado:', error);
        }
      });
  }

  private setErrors(errors: string[]) {
    this.hasError.set(true);
    this.errorMessages.set(errors);
  }

  private clearErrors() {
    this.hasError.set(false);
    this.errorMessages.set([]);
  }
}
