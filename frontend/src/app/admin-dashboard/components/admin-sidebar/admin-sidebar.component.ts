import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminSidebarItemComponent } from './menu-item/menu-item.component';
import { MenuItem } from '../../interfaces/menu-item.interface';



@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [CommonModule, AdminSidebarItemComponent],
  templateUrl: './admin-sidebar.component.html',
})
export class AdminSidebarComponent {
  isOpen = input.required<boolean>();
  width = input.required<string>();
  adminMenuItems = input.required<MenuItem[]>();
  booksMenuItems = input.required<MenuItem[]>();
  annoucementMenuItems = input.required<MenuItem[]>();
  closeSidebar = output<void>();
}
