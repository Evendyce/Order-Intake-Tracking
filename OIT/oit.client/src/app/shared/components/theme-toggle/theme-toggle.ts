import { Component, inject } from '@angular/core';
import { ThemeService } from '../../theme/theme-service';

@Component({
  selector: 'oit-theme-toggle',
  templateUrl: './theme-toggle.html',
  styleUrl: './theme-toggle.css',
})
export class ThemeToggle {
  readonly theme = inject(ThemeService);
}
