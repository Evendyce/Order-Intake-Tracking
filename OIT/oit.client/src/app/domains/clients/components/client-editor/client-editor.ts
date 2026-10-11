import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  SimpleChanges,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Client } from '../../models/client';

@Component({
  selector: 'oit-client-editor',
  imports: [FormsModule],
  templateUrl: './client-editor.html',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class ClientEditor {
  @Input() client: Client | null = null;
  @Input() busy = false;
  @Output() saveClient = new EventEmitter<{ name: string; email: string; isActive: boolean }>();
  @Output() cancelEdit = new EventEmitter<void>();
  name = '';
  email = '';
  isActive = true;
  ngOnChanges(changes: SimpleChanges) {
    if (changes['client']) {
      this.name = this.client?.name ?? '';
      this.email = this.client?.email ?? '';
      this.isActive = this.client?.isActive ?? true;
    }
  }
}
