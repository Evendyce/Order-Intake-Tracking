import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Client } from '../models/client';

@Injectable({ providedIn: 'root' })
export class ClientsApi {
  private http = inject(HttpClient);
  list() {
    return this.http.get<Client[]>('/api/clients');
  }
  create(input: { name: string; email: string }) {
    return this.http.post<Client>('/api/clients', input);
  }
  update(client: Client) {
    return this.http.put<Client>(`/api/clients/${client.id}`, client);
  }
  remove(client: Client) {
    // DELETE has no request body here; the query version still protects against stale removals.
    return this.http.delete<void>(`/api/clients/${client.id}`, {
      params: { version: client.version },
    });
  }
}
