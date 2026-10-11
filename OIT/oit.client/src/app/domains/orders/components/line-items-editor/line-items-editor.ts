import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OrderLine } from '../../models/order';

@Component({
  selector: 'oit-line-items-editor',
  imports: [FormsModule],
  templateUrl: './line-items-editor.html',
})
export class LineItemsEditor {
  @Input({ required: true }) items!: OrderLine[];
  @Input() disabled = false;
  @Output() itemsChange = new EventEmitter<OrderLine[]>();
  add() {
    this.itemsChange.emit([...this.items, { sku: '', name: '', quantity: 1, unitPrice: 0 }]);
  }
  remove(index: number) {
    this.itemsChange.emit(this.items.filter((_, i) => i !== index));
  }
  valid() {
    return (
      this.items.length > 0 &&
      this.items.length <= 100 &&
      this.items.every(
        (i) =>
          i.sku.trim() &&
          i.name.trim() &&
          Number.isInteger(i.quantity) &&
          i.quantity >= 1 &&
          i.quantity <= 100000 &&
          Number.isFinite(i.unitPrice) &&
          i.unitPrice >= 0 &&
          i.unitPrice <= 1000000 &&
          Math.abs(i.unitPrice * 100 - Math.round(i.unitPrice * 100)) < 0.000001,
      )
    );
  }
}
