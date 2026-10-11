import { Component, Input } from '@angular/core';

@Component({ selector: 'oit-feedback', templateUrl: './feedback.html' })
export class Feedback {
  @Input() message = '';
}
