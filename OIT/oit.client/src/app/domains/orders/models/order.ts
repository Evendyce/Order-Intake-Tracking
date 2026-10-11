export type OrderStatus = 'Pending' | 'Confirmed' | 'Fulfilled' | 'Cancelled';
export interface OrderLine {
  sku: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal?: number;
}
export interface Order {
  id: string;
  externalReference: string;
  clientId: string;
  clientName: string;
  clientEmail: string;
  createdAt: string;
  updatedAt: string;
  status: OrderStatus;
  currency: string;
  notes: string;
  subtotal: number;
  total: number;
  version: string;
  canEdit: boolean;
  allowedStatuses: OrderStatus[];
  items: OrderLine[];
}
export interface OrderInput {
  externalReference: string;
  clientId?: string;
  newClient?: { name: string; email: string };
  currency: string;
  notes: string;
  items: OrderLine[];
  version?: string;
}
export interface Submission {
  isDuplicate: boolean;
  overview: Order;
  differences: string[];
}
