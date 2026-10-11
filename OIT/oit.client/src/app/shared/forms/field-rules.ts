import { Directive, forwardRef, Input } from '@angular/core';
import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator } from '@angular/forms';

@Directive({
  selector: '[oitFieldRules]',
  providers: [{ provide: NG_VALIDATORS, useExisting: forwardRef(() => FieldRules), multi: true }],
})
export class FieldRules implements Validator {
  @Input() oitFieldRules: 'text' | 'quantity' | 'price' = 'text';

  validate(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    // Native required/min/max validators cover empty values and bounds. These rules
    // add the business constraints they do not cover, without silently changing input.
    if (value === null || value === undefined || value === '') return null;
    if (this.oitFieldRules === 'text') return String(value).trim() ? null : { required: true };
    if (this.oitFieldRules === 'quantity')
      return Number.isInteger(value) ? null : { wholeQuantity: true };
    // Ignore binary floating-point noise when checking two-decimal prices; do not round the user's value.
    return Number.isFinite(value) && Math.abs(value * 100 - Math.round(value * 100)) < 0.000001
      ? null
      : { pricePrecision: true };
  }
}
