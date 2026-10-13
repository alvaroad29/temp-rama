import { Component, inject, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ThemeService } from '../../services/Theme.service';
import { LucideAngularModule, Moon, Sun } from 'lucide-angular';

@Component({
  selector: 'toggle-theme',
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './toggle-theme.component.html',
})
export class ToggleThemeComponent {
  themeService = inject(ThemeService);

  // toggle-theme
  isLigth = model(this.themeService.getCurrentTheme()() === 'light');

   // Iconos
  readonly sun = Sun;
  readonly moon = Moon;
}
