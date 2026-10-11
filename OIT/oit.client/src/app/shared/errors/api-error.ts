import { HttpErrorResponse } from '@angular/common/http';

export function apiError(error: HttpErrorResponse): string {
  if (error.status === 0)
    return 'Cannot reach the API. Check that the server is running, then retry.';
  const body = error.error;
  if (body?.errors) return Object.values(body.errors).flat().join(' ');
  return body?.detail ?? 'The request could not be completed. Please retry.';
}
