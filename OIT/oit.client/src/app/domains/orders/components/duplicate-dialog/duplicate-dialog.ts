import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  ViewChild,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Submission } from '../../models/order';

@Component({
  selector: 'oit-duplicate-dialog',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './duplicate-dialog.html',
})
export class DuplicateDialog implements AfterViewInit {
  @Input({ required: true }) result!: Submission;
  @Output() dismissed = new EventEmitter<void>();
  @ViewChild('dialog') dialog!: ElementRef<HTMLDialogElement>;
  // Native modal semantics provide focus containment and Escape dismissal without a custom overlay.
  ngAfterViewInit() {
    this.dialog.nativeElement.showModal();
  }
}
