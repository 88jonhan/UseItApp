import {Component, OnInit} from '@angular/core';
import {ApiService} from '../../services/api.service';
import {Item} from '../../models/item';
import {RouterLink} from '@angular/router';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatCardModule} from '@angular/material/card';
import {MatChipsModule} from '@angular/material/chips';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';

@Component({
  selector: 'app-item-list',
  templateUrl: './item-list.component.html',
  styleUrls: ['./item-list.component.css'],
  standalone: true,
  imports: [
    RouterLink,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule

  ],
})
export class ItemListComponent implements OnInit {
  items: Item[] = [];
  loading = true;
  error = '';
  searchTerm = '';

  constructor(private apiService: ApiService) {
  }

  ngOnInit(): void {
    this.loadItems();
  }

  loadItems(): void {
    this.loading = true;
    this.apiService.getItems().subscribe({
      next: (data) => {
        console.log('Data received from API:', data); // <-- check this
        this.items = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Ett fel uppstod när föremålen skulle hämtas.';
        this.loading = false;
        console.error('Error fetching items:', err);
      }
    });
  }

  get filteredItems(): Item[] {
    if (!this.searchTerm.trim()) {
      return this.items;
    }

    const term = this.searchTerm.toLowerCase();
    return this.items.filter(item =>
      item.name.toLowerCase().includes(term) ||
      item.description.toLowerCase().includes(term) ||
      item.category.toLowerCase().includes(term)
    );
  }
}
