import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { App } from './app/app';
import { routes } from './app/app.routes';
import { readRetryInterceptor } from './app/shared/http/read-retry-interceptor';

bootstrapApplication(App, {
  providers: [provideHttpClient(withInterceptors([readRetryInterceptor])), provideRouter(routes)],
}).catch((error) => console.error(error));
