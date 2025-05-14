import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Item } from '../models/item';
import { Loan, LoanStatus } from '../models/loan';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:7001/api';

  constructor(private http: HttpClient) { }

  // Items
  getItems(): Observable<Item[]> {
    return this.http.get<Item[]>(`${this.apiUrl}/items`);
  }

  getItem(id: number): Observable<Item> {
    return this.http.get<Item>(`${this.apiUrl}/items/${id}`);
  }

  createItem(item: Omit<Item, 'ownerId' | 'id'>): Observable<Item> {
    return this.http.post<Item>(`${this.apiUrl}/items`, item);
  }

  updateItem(id: number, item: Item): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/items/${id}`, item);
  }

  deleteItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/items/${id}`);
  }

  // User Items
  getUserItems(): Observable<Item[]> {
    return this.http.get<Item[]>(`${this.apiUrl}/items/user`);
  }

  // Loans
  getLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(`${this.apiUrl}/loans`);
  }

  getUserLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(`${this.apiUrl}/loans/user`);
  }

  getLoan(id: number): Observable<Loan> {
    return this.http.get<Loan>(`${this.apiUrl}/loans/${id}`);
  }

  createLoan(loan: Loan): Observable<Loan> {
    return this.http.post<Loan>(`${this.apiUrl}/loans`, loan);
  }

  updateLoanStatus(id: number, status: LoanStatus): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/loans/${id}/status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  getOwnerLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(`${this.apiUrl}/loans/owner`);
  }

  initiateReturn(loanId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/loans/${loanId}/initiate-return`, {});
  }

  confirmReturn(loanId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/loans/${loanId}/confirm-return`, {});
  }

  approveRequest(loanId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/loans/${loanId}/approve`, {});
  }

  rejectRequest(loanId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/loans/${loanId}/reject`, {});
  }

  activateLoan(loanId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/loans/${loanId}/activate`, {});
  }
}
