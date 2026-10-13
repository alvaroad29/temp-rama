import { Component, input, output } from '@angular/core';
import { MenuItem } from '../../../admin-dashboard/interfaces/menu-item.interface';
import { CommonModule } from '@angular/common';
import { MenuItemComponent } from './menu-item/menu-item.component';

@Component({
  selector: 'app-author-sidebar',
  imports: [CommonModule, MenuItemComponent],
  templateUrl: './author-sidebar.component.html',
})
export class AuthorSidebarComponent {
  isOpen = input.required<boolean>();
  width = input.required<string>();
  authorMenuItems = input.required<MenuItem[]>();
  closeSidebar = output<void>();
}
