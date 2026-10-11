import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  ViewChild,
} from '@angular/core';

@Component({ selector: 'oit-confirm-dialog', templateUrl: './confirm-dialog.html' })
export class ConfirmDialog implements AfterViewInit {
  @Input() title = 'Confirm action';
  @Input() message = '';
  @Input() action = 'Confirm';
  @Output() confirmed = new EventEmitter<void>();
  @Output() dismissed = new EventEmitter<void>();
  @ViewChild('dialog') dialog!: ElementRef<HTMLDialogElement>;
  ngAfterViewInit() {
    this.dialog.nativeElement.showModal();
  }
}
