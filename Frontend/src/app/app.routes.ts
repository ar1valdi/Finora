import { Routes } from '@angular/router';
import { UsersComponent } from './pages/users/users.component';
import { HomeComponent } from './pages/home/home.component';
import { AccountComponent } from './pages/account/account.component';
import { RegisterComponent } from './pages/register/register.component';
import { UpdateUserFormComponent } from './pages/users/update-user-form/update-user-form.component';
import { AtmComponent } from './pages/atm/atm.component';
import { TransferComponent } from './pages/transfer/transfer.component';

export const routes: Routes = [
    { path: 'users', component: UsersComponent },
    { path: 'login', component: RegisterComponent },
    { path: 'account', component: AccountComponent },
    { path: 'users/update/:id', component: UpdateUserFormComponent },
    { path: '', component: HomeComponent },
    { path: 'atm', component: AtmComponent },
    { path: 'transfer', component: TransferComponent },
];
