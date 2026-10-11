import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { OrdersApi } from '../../services/orders-api';
import { ClientsApi } from '../../../clients/services/clients-api';
import { Client } from '../../../clients/models/client';
import { Order } from '../../models/order';
import { Feedback } from '../../../../shared/components/feedback/feedback';
import { apiError } from '../../../../shared/errors/api-error';

@Component({
  imports: [CurrencyPipe, DatePipe, FormsModule, RouterLink, Feedback],
  templateUrl: './orders-list.html',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class OrdersList {
  private api = inject(OrdersApi);
  private clientsApi = inject(ClientsApi);
  orders = signal<Order[]>([]);
  clients = signal<Client[]>([]);
  error = signal('');
  loading = signal(false);
  filters = { reference: '', clientId: '', status: '' };
  constructor() {
    this.clientsApi
      .list()
      .subscribe({ next: (c) => this.clients.set(c), error: (e) => this.error.set(apiError(e)) });
    this.load();
  }
  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.list(this.filters).subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (e) => {
        this.error.set(apiError(e));
        this.loading.set(false);
      },
    });
  }
  reset() {
    this.filters = { reference: '', clientId: '', status: '' };
    this.load();
  }
}
