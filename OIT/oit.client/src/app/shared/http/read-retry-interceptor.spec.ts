import { TestBed } from '@angular/core/testing';
import {
  HttpClient,
  HttpErrorResponse,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { readRetryInterceptor } from './read-retry-interceptor';

describe('API read availability retries', () => {
  let http: HttpClient;
  let requests: HttpTestingController;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([readRetryInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    requests = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    requests.verify();
    vi.useRealTimers();
  });

  it.each([0, 502, 503, 504])('recovers from temporary unavailability (%s)', (status) => {
    const received = vi.fn();
    const failed = vi.fn();
    http.get('/api/orders').subscribe({ next: received, error: failed });
    const first = requests.expectOne('/api/orders');
    if (status === 0) first.error(new ProgressEvent('error'));
    else first.flush('', { status, statusText: 'Unavailable' });

    expect(failed).not.toHaveBeenCalled();
    vi.advanceTimersByTime(999);
    requests.expectNone('/api/orders');
    vi.advanceTimersByTime(1);
    requests.expectOne('/api/orders').flush([{ externalReference: 'DEMO-001' }]);
    expect(received).toHaveBeenCalledWith([{ externalReference: 'DEMO-001' }]);
    expect(failed).not.toHaveBeenCalled();
  });

  it('stops after eight retries and returns the availability error', () => {
    const failed = vi.fn();
    http.get('/api/orders').subscribe({ error: failed });
    for (let attempt = 0; attempt <= 8; attempt++) {
      requests.expectOne('/api/orders').flush('', { status: 503, statusText: 'Unavailable' });
      if (attempt < 8) {
        expect(failed).not.toHaveBeenCalled();
        vi.advanceTimersByTime(1000);
      }
    }
    expect(failed).toHaveBeenCalledOnce();
    expect((failed.mock.calls[0][0] as HttpErrorResponse).status).toBe(503);
    vi.advanceTimersByTime(10000);
    requests.expectNone('/api/orders');
  });

  it.each([400, 401, 403, 404, 409, 422, 500])(
    'reports other errors immediately (%s)',
    (status) => {
      const failed = vi.fn();
      http.get('/api/orders').subscribe({ error: failed });
      requests.expectOne('/api/orders').flush('', { status, statusText: 'Failed' });
      expect(failed).toHaveBeenCalledOnce();
      vi.advanceTimersByTime(10000);
      requests.expectNone('/api/orders');
    },
  );

  it.each(['POST', 'PUT', 'PATCH', 'DELETE'])('never replays a %s write', (method) => {
    const failed = vi.fn();
    http.request(method, '/api/orders', { body: {} }).subscribe({ error: failed });
    requests.expectOne('/api/orders').flush('', { status: 503, statusText: 'Unavailable' });
    expect(failed).toHaveBeenCalledOnce();
    vi.advanceTimersByTime(10000);
    requests.expectNone('/api/orders');
  });

  it('cancels pending retries when the read is unsubscribed', () => {
    const subscription = http.get('/api/orders').subscribe();
    requests.expectOne('/api/orders').flush('', { status: 502, statusText: 'Unavailable' });
    subscription.unsubscribe();
    vi.advanceTimersByTime(10000);
    requests.expectNone('/api/orders');
  });

  it('does not retry requests outside the API', () => {
    const failed = vi.fn();
    http.get('/assets/example.json').subscribe({ error: failed });
    requests
      .expectOne('/assets/example.json')
      .flush('', { status: 503, statusText: 'Unavailable' });
    expect(failed).toHaveBeenCalledOnce();
    vi.advanceTimersByTime(10000);
    requests.expectNone('/assets/example.json');
  });
});
