// user-list-page.component.ts
import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';


import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { UserService } from '../../../../features/user/services/user.service';
import { PaginationService } from '../../../../shared/components/pagination/pagination.service';
import { UserQueryParams } from '../../../../features/user/interfaces/user-query-params';
import { User } from '../../../../auth/interfaces/user.interface';

@Component({
  selector: 'app-user-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './user-list-page.component.html'
})
export class UserListPageComponent {
  private userService = inject(UserService);
  private router = inject(Router);
  paginationService = inject(PaginationService);

  // Signals para búsqueda y filtros
  searchText = signal('');
  searchIn = signal<'all' | 'username' | 'id'>('all');
  isBanned = signal<boolean | undefined>(undefined);
  sortBy = signal('username');
  sortDescending = signal(false);

  // Resource para cargar usuarios
  usersResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      searchText: this.searchText(),
      searchIn: this.searchIn(),
      isBanned: this.isBanned(),
      sortBy: this.sortBy(),
      sortDescending: this.sortDescending()
    }),
    loader: ({ request }) => {
      const params: UserQueryParams = {
        pageNumber: request.page,
        pageSize: 10, // Puedes hacer esto configurable
        searchText: request.searchText,
        searchIn: request.searchIn,
        isBanned: request.isBanned,
        sortBy: request.sortBy,
        sortDescending: request.sortDescending
      };

      return this.userService.getUsers(params);
    }
  });

  // Computed signals para datos derivados
  users = computed(() => this.usersResource.value()?.data ?? []);
  totalRecords = computed(() => this.usersResource.value()?.totalRecords ?? 0);
  totalPages = computed(() => Math.ceil(this.totalRecords() / 10)); // pageSize = 10

  // Métodos de búsqueda y filtros
  onSearch() {
    this.usersResource.reload(); // Recargar los datos
  }

  onFilterChange() {
    
    this.usersResource.reload(); // Recargar los datos
  }

  // Métodos para acciones de usuario
  banUser(user: User) {
    if (confirm(`¿Estás seguro de que quieres banear a ${user.userName}?`)) {
      this.userService.banUser(user.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.usersResource.reload(); // Recargar la lista
          }
        },
        error: (err) => {
          alert('Error al banear usuario');
          console.error('Error banning user:', err);
        }
      });
    }
  }

  unbanUser(user: User) {
    if (confirm(`¿Estás seguro de que quieres desbanear a ${user.userName}?`)) {
      this.userService.unbanUser(user.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.usersResource.reload(); // Recargar la lista
          }
        },
        error: (err) => {
          alert('Error al desbanear usuario');
          console.error('Error unbanning user:', err);
        }
      });
    }
  }

  assignAdmin(user: User) {
    if (confirm(`¿Asignar rol Admin a ${user.userName}?`)) {
      this.userService.assignRole(user.id, 'Admin').subscribe({
        next: (response) => {
          if (response.success) {
            this.usersResource.reload(); // Recargar la lista
          }
        },
        error: (err) => {
          alert('Error al asignar rol');
          console.error('Error assigning role:', err);
        }
      });
    }
  }

  removeAdmin(user: User) {
    if (confirm(`¿Remover rol Admin de ${user.userName}?`)) {
      this.userService.removeRole(user.id, 'Admin').subscribe({
        next: (response) => {
          if (response.success) {
            this.usersResource.reload(); // Recargar la lista
          }
        },
        error: (err) => {
          alert('Error al remover rol');
          console.error('Error removing role:', err);
        }
      });
    }
  }

  viewUserDetails(user: User) {
    this.router.navigate(['/admin/users', user.id]);
  }

  getRoleBadgeClass(role: string): string {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'bg-purple-100 text-purple-800';
      case 'author':
        return 'bg-blue-100 text-blue-800';
      case 'user':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  
onBannedChange(value: string) {
  if (value === '') {
    this.isBanned.set(undefined);
  } else if (value === 'true') {
    this.isBanned.set(true);
  } else {
    this.isBanned.set(false);
  }
  this.onFilterChange();
}
}
