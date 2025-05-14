import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {CommonModule, DatePipe} from '@angular/common';
import {ApiService} from '../../services/api.service';
import {AuthService} from '../../services/auth.service';
import {Item} from '../../models/item';
import {Loan, LoanStatus} from '../../models/loan';
import {RouterLink} from '@angular/router';
import {MatCardModule} from '@angular/material/card';
import {MatButtonModule} from '@angular/material/button';
import {MatChipsModule} from '@angular/material/chips';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatOptionModule} from '@angular/material/core';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    DatePipe,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatOptionModule,
    MatInputModule,
    MatSelectModule
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  user: any;
  myItems: Item[] = [];
  myLoans: Loan[] = [];
  myLendingItems: Loan[] = [];
  loadingItems = true;
  loadingLoans = true;
  loadingLending = true;
  errorItems = '';
  errorLoans = '';
  errorLending = '';

  newItemForm: FormGroup;
  showNewItemForm = false;
  submitting = false;

  constructor(
    private formBuilder: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService
  ) {
    this.newItemForm = this.formBuilder.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      category: ['', Validators.required],
      imageUrl: ['']
    });
  }

  ngOnInit(): void {
    this.authService.getCurrentUser().subscribe(user => {
      this.user = user;
    });

    this.loadItems();
    this.loadLoans();
    this.loadLendingItems();
  }

  loadItems(): void {
    this.loadingItems = true;
    this.apiService.getUserItems().subscribe({
      next: (items) => {
        this.myItems = items;
        this.loadingItems = false;
      },
      error: (err) => {
        this.errorItems = 'Failed to load your items';
        this.loadingItems = false;
        console.error('Error loading items:', err);
      }
    });
  }

  loadLoans(): void {
    this.loadingLoans = true;
    this.apiService.getUserLoans().subscribe({
      next: (loans) => {
        this.myLoans = loans;
        this.loadingLoans = false;
        console.log(loans)
      },
      error: (err) => {
        this.errorLoans = 'Failed to load your loans';
        this.loadingLoans = false;
        console.error('Error loading loans:', err);
      }
    });
  }

  loadLendingItems(): void {
    this.loadingLending = true;
    this.apiService.getOwnerLoans().subscribe({
      next: (loans) => {
        this.myLendingItems = loans;
        this.loadingLending = false;
      },
      error: (err) => {
        this.errorLending = 'Failed to load your lending items';
        this.loadingLending = false;
        console.error('Error loading lending items:', err);
      }
    });
  }

  toggleNewItemForm(): void {
    this.showNewItemForm = !this.showNewItemForm;
    if (!this.showNewItemForm) {
      this.newItemForm.reset();
    }
  }

  onSubmitNewItem(): void {
    if (this.newItemForm.invalid) {
      return;
    }

    this.submitting = true;

    const newItem = {
      name: this.newItemForm.value.name,
      description: this.newItemForm.value.description,
      category: this.newItemForm.value.category,
      imageUrl: this.newItemForm.value.imageUrl || undefined,
      isAvailable: true
    };

    this.apiService.createItem(newItem).subscribe({
      next: (item) => {
        this.myItems.push(item);
        this.submitting = false;
        this.toggleNewItemForm();
      },
      error: (err) => {
        console.error('Error creating item:', err);
        this.submitting = false;
      }
    });
  }

  deleteItem(id: number): void {
    if (confirm('Är du säker på att du vill ta bort detta föremål?')) {
      this.apiService.deleteItem(id).subscribe({
        next: () => {
          this.myItems = this.myItems.filter(item => item.id !== id);
        },
        error: (err) => {
          console.error('Error deleting item:', err);
        }
      });
    }
  }

  initiateReturn(id: number): void {
    this.apiService.initiateReturn(id).subscribe({
      next: () => {
        const loan = this.myLoans.find(l => l.id === id);
        if (loan) {
          loan.status = LoanStatus.ReturnInitiated;
        }
      },
      error: (err) => {
        console.error('Error initiating return:', err);
      }
    });
  }

  confirmReturn(id: number): void {
    this.apiService.confirmReturn(id).subscribe({
      next: () => {
        const loan = this.myLendingItems.find(l => l.id === id);
        if (loan) {
          loan.status = LoanStatus.Returned;

          if (loan.item && loan.item.id) {
            const item = this.myItems.find(i => i.id === loan.item!.id);
            if (item) {
              item.isAvailable = true;
            }
          }
        }
      },
      error: (err) => {
        console.error('Error confirming return:', err);
      }
    });
  }

  getLoanStatusDisplayName(status: LoanStatus): string {
    switch (status) {
      case LoanStatus.Requested:
        return 'Förfrågan';
      case LoanStatus.Approved:
        return 'Godkänd';
      case LoanStatus.Active:
        return 'Aktiv';
      case LoanStatus.ReturnInitiated:
        return 'Återlämning';
      case LoanStatus.Rejected:
        return 'Avböjd';
      case LoanStatus.Returned:
        return 'Återlämnad';
      case LoanStatus.Overdue:
        return 'Försenad';
      default:
        return status;
    }
  }

  getLoanStatusClass(status: any): string {
    if (typeof status === 'string') {
      return status.toLowerCase();
    } else if (status !== null && status !== undefined) {
      return String(status).toLowerCase();
    }
    return '';
  }

  approveRequest(id: number): void {
    this.apiService.approveRequest(id).subscribe({
      next: () => {
        const loan = this.myLendingItems.find(l => l.id === id);
        if (loan) {
          loan.status = LoanStatus.Approved;
        }
      },
      error: (err) => {
        console.error('Error approving loan request:', err);
      }
    });
  }

  rejectRequest(id: number): void {
    this.apiService.rejectRequest(id).subscribe({
      next: () => {
        const loan = this.myLendingItems.find(l => l.id === id);
        if (loan) {
          loan.status = LoanStatus.Rejected;

          if (loan.item && loan.item.id) {
            const item = this.myItems.find(i => i.id === loan.item!.id);
            if (item) {
              item.isAvailable = true;
            }
          }
        }
      },
      error: (err) => {
        console.error('Error rejecting loan request:', err);
      }
    });
  }

  activateLoan(id: number): void {
    this.apiService.activateLoan(id).subscribe({
      next: () => {
        const loan = this.myLendingItems.find(l => l.id === id);
        if (loan) {
          loan.status = LoanStatus.Active;
        }
      },
      error: (err) => {
        console.error('Error activating loan:', err);
      }
    });
  }

  protected readonly LoanStatus = LoanStatus;
}
