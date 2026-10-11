import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  SimpleChanges,
} from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { FieldFeedback } from '../../../../shared/components/field-feedback/field-feedback';
import { FieldRules } from '../../../../shared/forms/field-rules';
import { Client } from '../../models/client';

@Component({
  selector: 'oit-client-editor',
  imports: [FormsModule, FieldFeedback, FieldRules],
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
  save(form: NgForm) {
    form.control.markAllAsTouched();
    if (form.valid && !this.busy)
      this.saveClient.emit({ name: this.name, email: this.email, isActive: this.isActive });
  }
  ngOnChanges(changes: SimpleChanges) {
    if (changes['client']) {
      this.name = this.client?.name ?? '';
      this.email = this.client?.email ?? '';
      this.isActive = this.client?.isActive ?? true;
    }
  }
}
