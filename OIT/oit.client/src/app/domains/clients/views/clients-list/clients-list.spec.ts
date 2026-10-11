import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ClientsList } from './clients-list';

describe('Client list error recovery', () => {
  let requests: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ClientsList],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    requests = TestBed.inject(HttpTestingController);
  });

  afterEach(() => requests.verify());

  it('keeps the loaded clients visible after a duplicate-email save fails', () => {
    const fixture = TestBed.createComponent(ClientsList);
    requests
      .expectOne('/api/clients')
      .flush([
        {
          id: 'client-1',
          name: 'Existing client',
          email: 'existing@example.com',
          isActive: true,
          version: 'v1',
        },
      ]);
    fixture.componentInstance.edit(null);
    fixture.componentInstance.save({
      name: 'Duplicate',
      email: 'existing@example.com',
      isActive: true,
    });
    requests
      .expectOne({ method: 'POST', url: '/api/clients' })
      .flush(
        { detail: 'A client with this email already exists.' },
        { status: 409, statusText: 'Conflict' },
      );
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'already exists',
    );
    expect(fixture.nativeElement.querySelector('tbody').textContent).toContain('Existing client');
    expect(fixture.nativeElement.textContent).toContain('Add client');
    expect(fixture.nativeElement.querySelector('oit-client-editor')).not.toBeNull();
    fixture.destroy();
  });

  it('shows a loading failure without presenting it as an empty client list, and can reload', () => {
    const fixture = TestBed.createComponent(ClientsList);
    requests.expectOne('/api/clients').flush('', { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'Cannot reach the API',
    );
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
    expect(fixture.nativeElement.textContent).not.toContain('No clients yet');

    fixture.componentInstance.load();
    requests.expectOne('/api/clients').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('No clients yet');
    fixture.destroy();
  });
});
