import {Routes} from '@angular/router';
import {HomeComponent} from './components/home/home.component';
import {ItemListComponent} from './components/item-list/item-list.component';
import {ItemDetailComponent} from './components/item-detail/item-detail.component';
import {LoanFormComponent} from './components/loan-form/loan-form.component';
import {ProfileComponent} from './components/profile/profile.component';
import {LoginComponent} from './components/login/login.component';
import {RegisterComponent} from './components/register/register.component';
import {authGuard} from './guards/auth.guard';
import {EditItemComponent} from './components/edit-item-component/edit-item-component';

export const routes: Routes = [
  {path: '', component: HomeComponent},
  {path: 'items', component: ItemListComponent},
  {path: 'items/:id', component: ItemDetailComponent},
  {path: 'items/:id/edit', component: EditItemComponent, canActivate: [authGuard]},
  {path: 'loan/:id', component: LoanFormComponent, canActivate: [authGuard]},
  {path: 'profile', component: ProfileComponent, canActivate: [authGuard]},
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegisterComponent},
  {path: '**', redirectTo: ''}
];
