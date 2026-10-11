import { TestBed } from '@angular/core/testing';
import { ClientEditor } from './client-editor';

describe('Client form validation feedback', () => {
  it('explains empty fields on submit and permits saving after correction', async () => {
    const fixture = TestBed.createComponent(ClientEditor);
    const saved = vi.fn();
    fixture.componentInstance.saveClient.subscribe(saved);
    fixture.detectChanges();
    await fixture.whenStable();
    const form: HTMLFormElement = fixture.nativeElement.querySelector('form');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
    expect(saved).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Name is required.');
    expect(fixture.nativeElement.textContent).toContain('Email is required.');
    expect(fixture.nativeElement.querySelector('[name="name"]').getAttribute('aria-invalid')).toBe(
      'true',
    );

    for (const [name, value] of [
      ['name', 'Client'],
      ['email', 'client@example.com'],
    ]) {
      const input: HTMLInputElement = fixture.nativeElement.querySelector(`[name="${name}"]`);
      input.value = value;
      input.dispatchEvent(new Event('input', { bubbles: true }));
    }
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('is required.');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    expect(saved).toHaveBeenCalledExactlyOnceWith({
      name: 'Client',
      email: 'client@example.com',
      isActive: true,
    });
    fixture.destroy();
  });

  it('rejects whitespace names and explains malformed email after blur', async () => {
    const fixture = TestBed.createComponent(ClientEditor);
    fixture.detectChanges();
    await fixture.whenStable();
    for (const [name, value] of [
      ['name', '   '],
      ['email', 'invalid'],
    ]) {
      const input: HTMLInputElement = fixture.nativeElement.querySelector(`[name="${name}"]`);
      input.value = value;
      input.dispatchEvent(new Event('input', { bubbles: true }));
      input.dispatchEvent(new Event('blur'));
    }
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Name is required.');
    expect(fixture.nativeElement.textContent).toContain('Enter a valid email address.');
    fixture.destroy();
  });
});
