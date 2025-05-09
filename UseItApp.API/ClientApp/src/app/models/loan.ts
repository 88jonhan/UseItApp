import { User } from './user';
import { Item } from './item';

export enum LoanStatus {
  Requested,
  Approved,
  Rejected,
  Active,
  ReturnInitiated,
  Returned,
  Overdue,
}

export interface Loan {
  id?: number;
  startDate: Date;
  endDate: Date;
  actualReturnDate?: Date;
  status: LoanStatus;
  notes?: string;
  createdAt?: Date;
  itemId: number;
  borrowerId: number;
  item?: Item;
  borrower?: User;
}
