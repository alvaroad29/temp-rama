import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuItem } from '../../../interfaces/menu-item.interface';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-admin-sidebar-item',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './menu-item.component.html',
})
export class AdminSidebarItemComponent {
  item = input.required<MenuItem>();

  getBadgeClasses(color: string): string {
    const colorClasses = {
      red: 'bg-red-100 text-red-800',
      green: 'bg-green-100 text-green-800',
      blue: 'bg-blue-100 text-blue-800'
    };
    return colorClasses[color as keyof typeof colorClasses] || colorClasses.red;
  }
}
