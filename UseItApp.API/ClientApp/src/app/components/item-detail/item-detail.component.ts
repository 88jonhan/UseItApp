import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import { ApiService } from '../../services/api.service';
import { Item } from '../../models/item';
import {AuthService} from '../../services/auth.service';
import {MatChipsModule} from '@angular/material/chips';
import {MatCardModule} from '@angular/material/card';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatIconModule} from '@angular/material/icon';
import {MatButtonModule} from '@angular/material/button';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-item-detail',
  templateUrl: './item-detail.component.html',
  styleUrls: ['./item-detail.component.css'],
  standalone: true,
  imports: [
    RouterLink,
    MatChipsModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule,
    NgClass
  ],
})
export class ItemDetailComponent implements OnInit {
  item: Item | null = null;
  loading = true;
  error = '';
  currentUserId: number | undefined = undefined;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    // Först hämta aktuell användare
    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        if (user) {
          this.currentUserId = user.id;
          console.log('Current user loaded:', this.currentUserId);
        } else {
          console.log('No user found');
        }
        // Efter att ha laddat användaren, ladda föremålet
        this.loadItem();
      },
      error: (err) => {
        console.error('Error loading current user:', err);
        // Även om det blir fel, försök ladda föremålet
        this.loadItem();
      }
    });
  }

  loadCurrentUser(): void {
    this.authService.getCurrentUser().subscribe(user => {
      this.currentUserId = user?.id ?? undefined;
    });
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
      },
      error: (err) => {
        this.error = 'Ett fel uppstod när föremålet skulle hämtas.';
        this.loading = false;
        console.error('Error fetching item:', err);
      }
    });
  }

  requestLoan(): void {
    if (this.item) {
      this.router.navigate(['/loan', this.item.id]);
    }
  }

  get isOwner(): boolean {
    return this.item?.ownerId === this.currentUserId;
  }

  deleteItem(): void {
    console.log(this.item)
    if (!this.item || !confirm('Är du säker på att du vill ta bort detta föremål?')) {
      return;
    }

    this.apiService.deleteItem(this.item.id!).subscribe({
      next: () => {
        this.router.navigate(['/profile']);
      },
      error: (err) => {
        console.error('Error deleting item:', err);
        alert('Ett fel uppstod när föremålet skulle tas bort');
      }
    });
  }
}
