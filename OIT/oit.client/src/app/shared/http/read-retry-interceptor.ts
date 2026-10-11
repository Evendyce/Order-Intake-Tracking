import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { retry, throwError, timer } from 'rxjs';

export const readRetryInterceptor: HttpInterceptorFn = (request, next) => {
  if (request.method !== 'GET' || !request.url.startsWith('/api/')) return next(request);

  // The UI can start before the API finishes migrations. Retry unavailable reads for
  // eight times, one second apart; replaying writes could cause unintended duplicate changes.
  return next(request).pipe(
    retry({
      count: 8,
      delay: (error: unknown) =>
        error instanceof HttpErrorResponse && [0, 502, 503, 504].includes(error.status)
          ? timer(1000)
          : throwError(() => error),
    }),
  );
};
