import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Flag, Heart, History, List, LucideAngularModule, User } from 'lucide-angular';

@Component({
  selector: 'user-top-menu',
  imports: [LucideAngularModule, RouterLink, RouterLinkActive],
  templateUrl: './user-top-menu.component.html',
})
export class UserTopMenuComponent {

  // Iconos
    readonly user = User;
    readonly list = List;
    readonly history = History;
    readonly flag = Flag;
    readonly heart = Heart;
}
