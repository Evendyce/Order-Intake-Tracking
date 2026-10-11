import { DOCUMENT } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ThemeToggle } from './shared/components/theme-toggle/theme-toggle';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, ThemeToggle],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly document = inject(DOCUMENT);

  skipToContent(event: Event): void {
    // Focus the routed content without navigating relative to the document's base URL.
    event.preventDefault();
    this.document.getElementById('main-content')?.focus();
  }
}
