import { Component, OnInit } from '@angular/core';
import { UsersService } from '../../services/users.service';
import { User } from '../../models/users/user.model';
import { DatePipe } from '@angular/common';
import { Paginated } from '../../models/wrappers/paginated';
import { Router } from '@angular/router';
import { AddUserFormComponent } from './add-user-form/add-user-form.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DatePipe, AddUserFormComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit{
  users: User[] = [];
  isAddingUser = false;

  constructor(private readonly usersService: UsersService, private readonly router: Router) {}

  ngOnInit() {
    this.reloadUsers();
  }

  reloadUsers() {
    this.usersService.getAllUsers(1, 100).then((paginatedUsers: Paginated<User>) => {
      console.log('Users', paginatedUsers);
      this.users = paginatedUsers.data;
    }, (error) => {
      console.log('Error', error);
    });
  }

  updateUser(id: string) {
    this.router.navigate(['/users/update', id]);
  }

  deleteUser(id: string) {
    if (confirm('Are you sure you want to delete this user?')) {
      this.usersService.deleteUser(id).then(() => {
        this.users = this.users.filter((user) => user.id !== id);
        this.reloadUsers();
      });
    }
  }

  toggleAddUserForm() {
    this.isAddingUser = !this.isAddingUser;
  }

  afterAddUser() {
    this.isAddingUser = false;
    this.reloadUsers();
  }
}
