import { Component, computed, inject, signal } from '@angular/core';
import { LucideAngularModule, SquarePen, Trash2 } from 'lucide-angular';
import { AuthService } from '../../../auth/services/auth.service';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'profile-page',
  imports: [LucideAngularModule, RouterLink, DatePipe],
  templateUrl: './profile-page.component.html',
})
export class ProfilePageComponent {
  authService = inject(AuthService);
  user = this.authService.user;

  memberSince = new Date(2022, 0, 20); // Jan 20, 2022

  // Iconos
  readonly editIcon = SquarePen;
  readonly trashIcon = Trash2;

  // Estados
  imagePreview = signal<string | null>(null);
  selectedFile = signal<File | null>(null);
  isLoading = signal(false);
  successMessage = signal<string | null>(null);
  errorMessages = signal<string[]>([]);

  // Obtener imagen actual o placeholder
  currentImage = computed(() => {
    return this.imagePreview() || this.user()?.imageUrl || './assets/image-placeholder.png';
  });

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];


      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        this.setErrors(['Solo se permiten imágenes JPG o PNG']);
        return;
      }

      if (file.size > 1 * 1024 * 1024) {
        this.setErrors(['La imagen no puede pesar más de 1MB']);
        return;
      }

      // Limpiar errores
      this.clearErrors();
      this.selectedFile.set(file);

      // Crear preview
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview.set(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  uploadImage() {
    const file = this.selectedFile();
    if (!file) return;

    this.isLoading.set(true);
    this.clearErrors();

    this.authService.uploadProfilePicture(file).subscribe({
      next: (response) => {
        this.isLoading.set(false);

        if (response.success) {
          this.successMessage.set(response.message || 'Foto de perfil actualizada correctamente');
          this.imagePreview.set(null);
          this.selectedFile.set(null);


          this.clearFileInput();

          // Limpiar mensajes después de 3 segundos
          setTimeout(() => {
            this.successMessage.set(null);
          }, 3000);
        } else {
          this.setErrors(response.errors || ['Error al subir la imagen']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.setErrors(['Error inesperado al subir la imagen']);
      }
    });
  }

  removeImage() {
    this.isLoading.set(true);
    this.clearErrors();

    this.authService.deleteProfilePicture().subscribe({
      next: (response) => {
        this.isLoading.set(false);

        if (response.success) {
          this.successMessage.set(response.message || 'Foto de perfil eliminada correctamente');
          this.imagePreview.set(null);
          this.selectedFile.set(null);

          setTimeout(() => {
            this.successMessage.set(null);
          }, 3000);
        } else {
          this.setErrors(response.errors || ['Error al eliminar la imagen']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.setErrors(['Error inesperado al eliminar la imagen']);
      }
    });
  }

  cancelUpload() {
    this.imagePreview.set(null);
    this.selectedFile.set(null);
    this.clearErrors();
    this.clearFileInput();
  }

  private setErrors(errors: string[]) {
    this.errorMessages.set(errors);
  }

  private clearErrors() {
    this.errorMessages.set([]);
  }

  private clearFileInput() {
    const fileInput = document.getElementById('profileImage') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

}
