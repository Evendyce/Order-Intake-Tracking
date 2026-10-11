import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Client } from '../../models/client';
import { ClientsApi } from '../../services/clients-api';
import { ClientEditor } from '../../components/client-editor/client-editor';
import { Feedback } from '../../../../shared/components/feedback/feedback';
import { apiError } from '../../../../shared/errors/api-error';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';

@Component({ imports: [ClientEditor, Feedback, ConfirmDialog], templateUrl: './clients-list.html' })
export class ClientsList {
  private api = inject(ClientsApi);
  private destroyRef = inject(DestroyRef);
  clients = signal<Client[]>([]);
  selected = signal<Client | null>(null);
  editing = signal(false);
  error = signal('');
  busy = signal(false);
  loading = signal(true);
  loadFailed = signal(false);
  pendingRemoval = signal<Client | null>(null);
  constructor() {
    this.load();
  }
  load() {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.error.set('');
    this.api
      .list()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (c) => {
          this.clients.set(c);
          this.loading.set(false);
        },
        error: (e) => {
          this.loadFailed.set(true);
          this.error.set(apiError(e));
          this.loading.set(false);
        },
      });
  }
  edit(client: Client | null) {
    this.selected.set(client);
    this.editing.set(true);
    this.error.set('');
  }
  save(input: { name: string; email: string; isActive: boolean }) {
    this.busy.set(true);
    this.error.set('');
    const client = this.selected();
    const request = client ? this.api.update({ ...client, ...input }) : this.api.create(input);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.editing.set(false);
        this.load();
      },
      error: (e) => {
        // A failed write leaves the loaded list usable for review and recovery.
        this.error.set(apiError(e));
        this.busy.set(false);
      },
    });
  }
  remove(client: Client) {
    this.pendingRemoval.set(client);
  }
  confirmRemoval() {
    const client = this.pendingRemoval()!;
    this.pendingRemoval.set(null);
    this.busy.set(true);
    this.error.set('');
    this.api.remove(client).subscribe({
      next: () => {
        this.busy.set(false);
        this.load();
      },
      error: (e) => {
        this.error.set(apiError(e));
        this.busy.set(false);
      },
    });
  }
}
