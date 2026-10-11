import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgModel } from '@angular/forms';

@Component({
  selector: 'oit-field-feedback',
  templateUrl: './field-feedback.html',
  // NgModel keeps the same reference as its validation state changes.
  changeDetection: ChangeDetectionStrategy.Default,
})
export class FieldFeedback {
  @Input({ required: true }) control!: NgModel;
  @Input({ required: true }) field!: string;
  @Input({ required: true }) errorId!: string;
  @Input() submitted = false;

  get message(): string {
    // Wait for interaction or submission so an untouched form does not start in error.
    if (!this.control.invalid || (!this.control.touched && !this.submitted)) return '';
    const errors = this.control.errors ?? {};
    if (errors['required']) return `${this.field} is required.`;
    if (errors['email']) return 'Enter a valid email address.';
    if (errors['min']) return `${this.field} must be at least ${errors['min'].min}.`;
    if (errors['max']) return `${this.field} must be no more than ${errors['max'].max}.`;
    if (errors['wholeQuantity']) return 'Quantity must be a whole number.';
    if (errors['pricePrecision']) return 'Unit price must have at most two decimal places.';
    if (errors['maxlength']) return `${this.field} is too long.`;
    return `Check ${this.field.toLowerCase()}.`;
  }
}
