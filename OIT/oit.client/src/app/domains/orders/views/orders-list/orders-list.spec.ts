import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { OrdersList } from './orders-list';
import { readRetryInterceptor } from '../../../../shared/http/read-retry-interceptor';

describe('Orders page startup', () => {
  let requests: HttpTestingController;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({
      imports: [OrdersList],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([readRetryInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    requests = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    requests.verify();
    vi.useRealTimers();
  });

  it('keeps loading until the client options recover, without flashing an error or empty state', () => {
    const fixture = TestBed.createComponent(OrdersList);
    requests.expectOne('/api/orders').flush([]);
    requests.expectOne('/api/clients').flush('', { status: 502, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="status"]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
    expect(fixture.nativeElement.textContent).not.toContain('No orders match');

    vi.advanceTimersByTime(1000);
    requests.expectOne('/api/clients').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="status"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('No orders match');
    fixture.destroy();
  });

  it('shows a real outage after retries without claiming that there are no matching orders', () => {
    const fixture = TestBed.createComponent(OrdersList);
    requests.expectOne('/api/clients').flush([]);
    for (let attempt = 0; attempt <= 8; attempt++) {
      requests.expectOne('/api/orders').flush('', { status: 503, statusText: 'Unavailable' });
      if (attempt < 8) vi.advanceTimersByTime(1000);
    }
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="status"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'Cannot reach the API',
    );
    expect(fixture.nativeElement.textContent).not.toContain('No orders match');
    fixture.destroy();
  });
});
