import { HttpErrorResponse } from '@angular/common/http';

// Normalize transport, model-validation, and Problem Details failures into shared UI feedback.
export function apiError(error: HttpErrorResponse): string {
  if ([0, 502, 503, 504].includes(error.status))
    return 'Cannot reach the API. Check that the server is running, then retry.';
  const body = error.error;
  if (body?.errors) return Object.values(body.errors).flat().join(' ');
  return body?.detail ?? 'The request could not be completed. Please retry.';
}
