import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {ApiService} from '../../services/api.service';
import {Item} from '../../models/item';
import {Loan, LoanStatus} from '../../models/loan';
import {AuthService} from '../../services/auth.service';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatCardModule} from '@angular/material/card';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import {MatNativeDateModule} from '@angular/material/core';

@Component({
  selector: 'app-loan-form',
  templateUrl: './loan-form.component.html',
  styleUrls: ['./loan-form.component.css'],
  standalone: true,
  imports: [
    RouterLink,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
})
export class LoanFormComponent implements OnInit {
  item: Item | null = null;
  loading = true;
  error = '';
  loanForm: FormGroup;
  submitting = false;

  blockedUntil: Date | undefined = undefined;
  isUserBlocked = false;

  currentUserId: number | undefined;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService
  ) {
    this.loanForm = this.formBuilder.group({
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.authService.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUserId = user.id;

        if (user.isBlocked) {
          this.error = `Du är blockerad från att låna: ${user.blockReason}`;
          this.blockedUntil = user.blockedUntil ? new Date(user.blockedUntil) : undefined;
          this.isUserBlocked = true;
        }
      } else {
        this.error = 'Du måste vara inloggad för att låna föremål';
        this.router.navigate(['/login'], {
          queryParams: {returnUrl: this.router.url}
        });
      }
    });

    this.loadItem();
  }

  loadItem(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (isNaN(id)) {
      this.error = 'Ogiltigt ID för föremål';
      this.loading = false;
      return;
    }

    this.apiService.getItem(id).subscribe({
      next: (data) => {
        this.item = data;
        this.loading = false;

        const today = new Date();
        const nextWeek = new Date();
        nextWeek.setDate(today.getDate() + 7);

        this.loanForm.patchValue({
          startDate: this.formatDate(today),
          endDate: this.formatDate(nextWeek)
        });
      },
      error: (err) => {
        this.error = 'Ett fel uppstod när föremålet skulle hämtas.';
        this.loading = false;
        console.error('Error fetching item:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.loanForm.invalid || !this.item || !this.currentUserId) {
      if (!this.currentUserId) {
        this.error = 'Du måste vara inloggad för att låna föremål';
      }
      return;
    }

    this.submitting = true;

    const loanData: Loan = {
      itemId: this.item.id!,
      borrowerId: this.currentUserId,
      startDate: new Date(this.loanForm.value.startDate),
      endDate: new Date(this.loanForm.value.endDate),
      status: LoanStatus.Requested,
      notes: this.loanForm.value.notes
    };

    this.apiService.createLoan(loanData).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/items'], {
          queryParams: {loanRequested: true}
        });
      },
      error: (err) => {
        this.submitting = false;
        this.error = 'Ett fel uppstod när låneförfrågan skulle skapas.';
        console.error('Error creating loan:', err);
      }
    });
  }

  private formatDate(date: Date): string {
    const d = new Date(date);
    let month = '' + (d.getMonth() + 1);
    let day = '' + d.getDate();
    const year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [year, month, day].join('-');
  }
}
