import { ChangeDetectionStrategy, Component, inject, signal, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { OrderInput, Submission } from '../../models/order';
import { OrdersApi } from '../../services/orders-api';
import { ClientsApi } from '../../../clients/services/clients-api';
import { Client } from '../../../clients/models/client';
import { LineItemsEditor } from '../../components/line-items-editor/line-items-editor';
import { DuplicateDialog } from '../../components/duplicate-dialog/duplicate-dialog';
import { Feedback } from '../../../../shared/components/feedback/feedback';
import { apiError } from '../../../../shared/errors/api-error';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  imports: [FormsModule, RouterLink, LineItemsEditor, DuplicateDialog, Feedback],
  templateUrl: './order-form.html',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class OrderForm {
  private api = inject(OrdersApi);
  private clientsApi = inject(ClientsApi);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  @ViewChild(LineItemsEditor) lines!: LineItemsEditor;
  id = this.route.snapshot.paramMap.get('id');
  clients = signal<Client[]>([]);
  error = signal('');
  busy = signal(false);
  loading = signal(!!this.id);
  duplicate = signal<Submission | null>(null);
  canEdit = signal(true);
  newClientMode = false;
  newClient = { name: '', email: '' };
  model: OrderInput = {
    externalReference: '',
    currency: 'USD',
    notes: '',
    clientId: '',
    items: [{ sku: '', name: '', quantity: 1, unitPrice: 0 }],
  };
  constructor() {
    this.clientsApi
      .list()
      .subscribe({ next: (c) => this.clients.set(c), error: (e) => this.error.set(apiError(e)) });
    if (this.id)
      this.api.get(this.id).subscribe({
        next: (o) => {
          this.model = {
            externalReference: o.externalReference,
            currency: o.currency,
            notes: o.notes,
            clientId: o.clientId,
            version: o.version,
            items: o.items.map((i) => ({
              sku: i.sku,
              name: i.name,
              quantity: i.quantity,
              unitPrice: i.unitPrice,
            })),
          };
          this.canEdit.set(o.canEdit);
          this.loading.set(false);
        },
        error: (e) => {
          this.error.set(apiError(e));
          this.loading.set(false);
          this.canEdit.set(false);
        },
      });
  }
  save(form: NgForm) {
    this.error.set('');
    if (form.invalid || !this.lines.valid()) {
      form.control.markAllAsTouched();
      this.error.set(
        'Check required fields, whole quantities, and prices with at most two decimals.',
      );
      return;
    }
    const input = {
      ...this.model,
      clientId: this.newClientMode ? undefined : this.model.clientId,
      newClient: this.newClientMode ? this.newClient : undefined,
    };
    this.busy.set(true);
    if (this.id)
      this.api
        .update(this.id, input)
        .subscribe({
          next: (o) => this.router.navigate(['/orders', o.id]),
          error: (e) => this.failed(e),
        });
    else
      this.api.create(input).subscribe({
        next: (result) => {
          this.busy.set(false);
          if (result.isDuplicate) this.duplicate.set(result);
          else this.router.navigate(['/orders', result.overview.id]);
        },
        error: (e) => this.failed(e),
      });
  }
  private failed(error: HttpErrorResponse) {
    this.busy.set(false);
    // Duplicate conflicts carry a resource overview; ordinary conflicts remain error messages.
    if (error.status === 409 && error.error?.isDuplicate) this.duplicate.set(error.error);
    else this.error.set(apiError(error));
  }
}
