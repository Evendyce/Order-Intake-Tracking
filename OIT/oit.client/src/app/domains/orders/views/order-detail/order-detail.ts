import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Order, OrderStatus } from '../../models/order';
import { OrdersApi } from '../../services/orders-api';
import { OrderStatusComponent } from '../../components/order-status/order-status';
import { Feedback } from '../../../../shared/components/feedback/feedback';
import { apiError } from '../../../../shared/errors/api-error';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';

@Component({
  imports: [RouterLink, CurrencyPipe, DatePipe, OrderStatusComponent, Feedback, ConfirmDialog],
  templateUrl: './order-detail.html',
})
export class OrderDetail {
  private api = inject(OrdersApi);
  private route = inject(ActivatedRoute);
  order = signal<Order | null>(null);
  error = signal('');
  busy = signal(false);
  confirmingCancellation = signal(false);
  constructor() {
    this.load();
  }
  load() {
    this.api.get(this.route.snapshot.paramMap.get('id')!).subscribe({
      next: (o) => {
        this.order.set(o);
        this.error.set('');
      },
      error: (e) => this.error.set(apiError(e)),
    });
  }
  status(status: OrderStatus) {
    if (status === 'Cancelled') {
      this.confirmingCancellation.set(true);
      return;
    }
    this.saveStatus(status);
  }
  cancel() {
    this.confirmingCancellation.set(false);
    this.saveStatus('Cancelled');
  }
  private saveStatus(status: OrderStatus) {
    this.busy.set(true);
    this.error.set('');
    this.api.status(this.order()!, status).subscribe({
      next: (o) => {
        this.order.set(o);
        this.busy.set(false);
      },
      error: (e) => {
        this.error.set(apiError(e));
        this.busy.set(false);
      },
    });
  }
}
