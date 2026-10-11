import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Order, OrderInput, OrderStatus, Submission } from '../models/order';

@Injectable({ providedIn: 'root' })
// Keep HTTP contracts here and interaction state in views; relative URLs also work through the dev proxy.
export class OrdersApi {
  private http = inject(HttpClient);
  list(filters: { reference: string; clientId: string; status: string }) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) if (value) params = params.set(key, value);
    return this.http.get<Order[]>('/api/orders', { params });
  }
  get(id: string) {
    return this.http.get<Order>(`/api/orders/${id}`);
  }
  create(input: OrderInput) {
    return this.http.post<Submission>('/api/orders', input);
  }
  update(id: string, input: OrderInput) {
    return this.http.put<Order>(`/api/orders/${id}`, input);
  }
  status(order: Order, status: OrderStatus) {
    // Submit the last loaded version, allowing the server to detect stale status changes.
    return this.http.patch<Order>(`/api/orders/${order.id}/status`, {
      status,
      version: order.version,
    });
  }
}
