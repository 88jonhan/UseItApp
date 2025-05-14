import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { Item } from '../../models/item';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatSelectModule} from '@angular/material/select';
import {MatCardModule} from '@angular/material/card';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatCheckboxModule} from '@angular/material/checkbox';

@Component({
  selector: 'app-edit-item',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
    MatFormFieldModule,
    MatSelectModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule
  ],
  templateUrl: './edit-item.component.html',
  styleUrls: ['./edit-item.component.css']
})
export class EditItemComponent implements OnInit {
  itemForm: FormGroup;
  itemId: number = 0;
  loading = true;
  submitting = false;
  error = '';

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService
  ) {
    this.itemForm = this.formBuilder.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      category: ['', Validators.required],
      imageUrl: [''],
      isAvailable: [true]
    });
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (isNaN(id)) {
      this.error = 'Ogiltigt ID';
      this.loading = false;
      return;
    }

    this.itemId = id;
    this.loadItem(id);
  }

  loadItem(id: number): void {
    this.apiService.getItem(id).subscribe({
      next: (item) => {
        this.itemForm.patchValue({
          name: item.name,
          description: item.description,
          category: item.category,
          imageUrl: item.imageUrl,
          isAvailable: item.isAvailable
        });
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Kunde inte hämta föremålet';
        this.loading = false;
        console.error('Error fetching item:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.itemForm.invalid) {
      return;
    }

    this.submitting = true;

    const updatedItem: Item = {
      id: this.itemId,
      name: this.itemForm.value.name,
      description: this.itemForm.value.description,
      category: this.itemForm.value.category,
      imageUrl: this.itemForm.value.imageUrl || undefined,
      isAvailable: this.itemForm.value.isAvailable,
      ownerId: 0 // Detta kommer att ignoreras på servern eftersom den behåller den befintliga ägaren
    };

    this.apiService.updateItem(this.itemId, updatedItem).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/items', this.itemId]);
      },
      error: (err) => {
        this.error = 'Ett fel uppstod när föremålet skulle uppdateras';
        this.submitting = false;
        console.error('Error updating item:', err);
      }
    });
  }
}
