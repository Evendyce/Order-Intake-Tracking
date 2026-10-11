import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Order, OrderStatus } from '../../models/order';

@Component({
  selector: 'oit-order-status',
  templateUrl: './order-status.html',
  styleUrl: './order-status.css',
})
export class OrderStatusComponent {
  @Input({ required: true }) order!: Order;
  @Input() busy = false;
  @Output() changeStatus = new EventEmitter<OrderStatus>();
  steps: OrderStatus[] = ['Pending', 'Confirmed', 'Fulfilled'];
}
