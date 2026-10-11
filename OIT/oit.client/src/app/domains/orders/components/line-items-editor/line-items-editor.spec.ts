import { TestBed } from '@angular/core/testing';
import { LineItemsEditor } from './line-items-editor';

describe('Line item validation feedback', () => {
  it('reveals standalone field errors on order submission and clears them after correction', async () => {
    const fixture = TestBed.createComponent(LineItemsEditor);
    fixture.componentRef.setInput('items', [
      { sku: '', name: '', quantity: 1.5, unitPrice: 1.234 },
    ]);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.nativeElement.textContent).not.toContain('Quantity must be a whole number.');
    fixture.componentRef.setInput('submitted', true);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('SKU is required.');
    expect(fixture.nativeElement.textContent).toContain('Quantity must be a whole number.');
    expect(fixture.nativeElement.textContent).toContain(
      'Unit price must have at most two decimal places.',
    );
    const quantity: HTMLInputElement = fixture.nativeElement.querySelector(
      '[oitFieldRules="quantity"]',
    );
    expect(quantity.getAttribute('aria-invalid')).toBe('true');
    expect(
      fixture.nativeElement.querySelector(`#${quantity.getAttribute('aria-describedby')}`)
        .textContent,
    ).toContain('whole number');
    quantity.value = '3';
    quantity.dispatchEvent(new Event('input', { bubbles: true }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('Quantity must be a whole number.');
    expect(quantity.getAttribute('aria-invalid')).toBe('false');
    fixture.destroy();
  });
});
